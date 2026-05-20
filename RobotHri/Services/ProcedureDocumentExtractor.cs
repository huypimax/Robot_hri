using System.Diagnostics;
using System.Text;
using NPOI.HWPF;
using NPOI.HWPF.Extractor;
using NPOI.XWPF.UserModel;

namespace RobotHri.Services;

/// <summary>
/// Loads procedure Word templates from MauiAsset (Resources/Raw) and extracts
/// sections headed by "Thành phần hồ sơ" and "Cách thức thực hiện".
/// </summary>
public class ProcedureDocumentExtractor : IProcedureDocumentExtractor
{
    public async Task<ProcedureDocumentSections> ExtractAsync(
        string? mauiAssetRelativePath,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(mauiAssetRelativePath))
            return default;

        await using var stream = await TryOpenAssetAsync(mauiAssetRelativePath, cancellationToken).ConfigureAwait(false);
        if (stream is null)
            return default;

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
        ms.Position = 0;

        // Many government templates use a ".doc" extension but the file is actually OOXML (ZIP).
        // HWPF throws OfficeXmlFileException for those; detect by ZIP signature and use XWPF.
        var isOpenXmlZip = IsOfficeOpenXmlZip(ms);
        ms.Position = 0;

        if (isOpenXmlZip || mauiAssetRelativePath.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
        {
            var structured = ExtractSectionsFromDocx(ms, languageCode);
            if (LooksStructuredEnough(structured))
                return ApplyPostExtractCleanup(structured, languageCode);

            ms.Position = 0;
            var fullText = ReadDocxText(ms);
            if (string.IsNullOrWhiteSpace(fullText))
                return default;

            return languageCode.StartsWith("vi", StringComparison.OrdinalIgnoreCase)
                ? ApplyPostExtractCleanup(ExtractVietnamese(fullText), languageCode)
                : ApplyPostExtractCleanup(ExtractEnglishPreferringVinText(fullText), languageCode);
        }

        if (mauiAssetRelativePath.EndsWith(".doc", StringComparison.OrdinalIgnoreCase))
        {
            var fullText = ReadDocText(ms);
            if (string.IsNullOrWhiteSpace(fullText))
                return default;

            return languageCode.StartsWith("vi", StringComparison.OrdinalIgnoreCase)
                ? ApplyPostExtractCleanup(ExtractVietnamese(fullText), languageCode)
                : ApplyPostExtractCleanup(ExtractEnglishPreferringVinText(fullText), languageCode);
        }

        return default;
    }

    private static ProcedureDocumentSections ApplyPostExtractCleanup(ProcedureDocumentSections s, string languageCode) =>
        new(
            PostFormatSection(StripEmbeddedTrinhTuLeadingLines(s.Dossier), languageCode),
            PostFormatSection(s.Implementation, languageCode));

    /// <summary>
    /// Walks OOXML body in order so <see cref="XWPFTable"/> cells are included (paragraph-only extraction drops tables).
    /// Produces compact bullet summaries for dossier / implementation tables.
    /// </summary>
    private static ProcedureDocumentSections ExtractSectionsFromDocx(MemoryStream ms, string languageCode)
    {
        var doc = new XWPFDocument(ms);
        try
        {
            var dossier = new StringBuilder();
            var impl = new StringBuilder();
            var mode = DocxCollectMode.None;
            var hadDossierTableSinceHeading = false;

            foreach (var el in doc.BodyElements)
            {
                if (el is XWPFParagraph para)
                {
                    var line = NormalizeSingleLine(para.Text);
                    if (string.IsNullOrEmpty(line))
                        continue;

                    if (MatchesDossierHeading(line, languageCode))
                    {
                        if (mode == DocxCollectMode.Dossier)
                        {
                            // Some templates repeat "Thành phần hồ sơ" after misplaced procedural text — restart here.
                            dossier.Clear();
                            hadDossierTableSinceHeading = false;
                            if (!IsBareDossierHeadingLine(line, languageCode))
                                AppendLine(dossier, line);
                            continue;
                        }

                        if (mode == DocxCollectMode.Implementation)
                        {
                            mode = DocxCollectMode.Dossier;
                            hadDossierTableSinceHeading = false;
                            if (!IsBareDossierHeadingLine(line, languageCode))
                                AppendLine(dossier, line);
                            continue;
                        }

                        mode = DocxCollectMode.Dossier;
                        hadDossierTableSinceHeading = false;
                        if (!IsBareDossierHeadingLine(line, languageCode))
                            AppendLine(dossier, line);
                        continue;
                    }

                    if (MatchesImplementationHeading(line, languageCode))
                    {
                        mode = DocxCollectMode.Implementation;
                        if (!IsBareImplementationHeadingLine(line, languageCode))
                            AppendLine(impl, line);
                        continue;
                    }

                    if (mode == DocxCollectMode.Dossier && EndsDossierBlock(line, languageCode))
                    {
                        if (StartsLineWithHeading(line, "Trình tự thực hiện") &&
                            !hadDossierTableSinceHeading &&
                            !LooksLikeRealDossierContent(dossier))
                        {
                            dossier.Clear();
                        }

                        mode = DocxCollectMode.SkippingAfterDossier;
                        continue;
                    }

                    if (mode == DocxCollectMode.Implementation && EndsImplementationBlock(line, languageCode))
                    {
                        mode = DocxCollectMode.None;
                        continue;
                    }

                    switch (mode)
                    {
                        case DocxCollectMode.Dossier:
                            if (IsEmbeddedTrinhTuBulletParagraph(line, hadDossierTableSinceHeading))
                                break;
                            AppendLine(dossier, line);
                            break;
                        case DocxCollectMode.Implementation:
                            AppendLine(impl, line);
                            break;
                    }
                }
                else if (el is XWPFTable table)
                {
                    switch (mode)
                    {
                        case DocxCollectMode.Dossier:
                            hadDossierTableSinceHeading = true;
                            AppendTableSummary(dossier, table);
                            break;
                        case DocxCollectMode.Implementation:
                            AppendTableSummary(impl, table);
                            break;
                    }
                }
            }

            return new ProcedureDocumentSections(dossier.ToString(), impl.ToString());
        }
        finally
        {
            doc.Close();
        }
    }

    private static bool IsBareDossierHeadingLine(string line, string languageCode)
    {
        var t = line.TrimStart('*', ' ', '\t').Trim().TrimEnd(':', '：', '.', '。');
        if (t.StartsWith("Thành phần hồ sơ", StringComparison.OrdinalIgnoreCase) && t.Length <= 40)
            return true;
        if (languageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase) &&
            t.StartsWith("Dossier components", StringComparison.OrdinalIgnoreCase) && t.Length <= 48)
            return true;
        return false;
    }

    private static bool IsBareImplementationHeadingLine(string line, string languageCode)
    {
        var t = line.TrimStart('*', ' ', '\t').Trim().TrimEnd(':', '：', '.', '。');
        if (t.StartsWith("Cách thức thực hiện", StringComparison.OrdinalIgnoreCase) && t.Length <= 44)
            return true;
        if (t.StartsWith("How to proceed", StringComparison.OrdinalIgnoreCase) && t.Length <= 40)
            return true;
        if (languageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase) &&
            t.StartsWith("Implementation procedures", StringComparison.OrdinalIgnoreCase) && t.Length <= 52)
            return true;
        return false;
    }

    /// <summary>
    /// National-portal exports often place "Trình tự" steps as paragraphs starting with '+' directly under
    /// "Thành phần hồ sơ" with no section title. Skip those until the real dossier intro or table.
    /// </summary>
    private static bool IsEmbeddedTrinhTuBulletParagraph(string line, bool dossierHasTableYet)
    {
        if (dossierHasTableYet || string.IsNullOrWhiteSpace(line))
            return false;

        var t = line.TrimStart();
        if (!t.StartsWith('+'))
            return false;

        if (t.Contains("xuất trình các giấy tờ", StringComparison.OrdinalIgnoreCase) ||
            t.Contains("phải nộp các giấy tờ", StringComparison.OrdinalIgnoreCase) ||
            t.Contains("Giấy tờ phải nộp", StringComparison.OrdinalIgnoreCase) ||
            t.Contains("Giấy tờ phải xuất trình", StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    /// <summary>Remove leading '+' procedural block from plain-text dossier (HWPF / paragraph-only fallback).</summary>
    private static string StripEmbeddedTrinhTuLeadingLines(string dossier)
    {
        if (string.IsNullOrWhiteSpace(dossier))
            return string.Empty;

        var lines = dossier.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n', StringSplitOptions.None);
        var i = 0;
        while (i < lines.Length)
        {
            var line = lines[i].TrimEnd();
            if (string.IsNullOrWhiteSpace(line))
            {
                i++;
                continue;
            }

            if (IsEmbeddedTrinhTuBulletParagraph(line, dossierHasTableYet: false))
            {
                i++;
                continue;
            }

            break;
        }

        if (i >= lines.Length)
            return string.Empty;

        return string.Join("\n", lines.Skip(i)).Trim();
    }

    /// <summary>Detects real dossier copy (vs misplaced narrative under a wrong heading).</summary>
    private static bool LooksLikeRealDossierContent(StringBuilder dossier)
    {
        if (dossier.Length < 12)
            return false;
        var s = dossier.ToString();
        if (s.Contains("Giấy tờ", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("Tên giấy tờ", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("xuất trình", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("Bao gồm", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("bản chính", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("Bản sao", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("Một trong các giấy", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("request form", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("supporting document", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("application form", StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    private static bool LooksStructuredEnough(ProcedureDocumentSections s)
    {
        var d = s.Dossier.Trim();
        var i = s.Implementation.Trim();
        if (d.Contains('•', StringComparison.Ordinal) || i.Contains('•', StringComparison.Ordinal))
            return true;
        if (d.Length >= 48 || i.Length >= 48)
            return true;
        return false;
    }

    private enum DocxCollectMode
    {
        None,
        Dossier,
        SkippingAfterDossier,
        Implementation,
    }

    private static bool MatchesDossierHeading(string line, string languageCode)
    {
        if (languageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase))
        {
            if (line.Contains("Dossier components", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return line.Contains("Thành phần hồ sơ", StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesImplementationHeading(string line, string languageCode)
    {
        // Source TTHC files stay Vietnamese even when the app UI is English.
        if (line.Contains("Cách thức thực hiện", StringComparison.OrdinalIgnoreCase))
            return true;
        if (line.Contains("How to proceed", StringComparison.OrdinalIgnoreCase))
            return true;
        if (languageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase) &&
            line.Contains("Implementation procedures", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    /// <summary>Stop dossier before metadata / other major blocks (not part of hồ sơ).</summary>
    private static bool EndsDossierBlock(string line, string languageCode)
    {
        if (StartsLineWithHeading(line, "Trình tự thực hiện"))
            return true;
        if (StartsLineWithHeading(line, "Cách thức thực hiện"))
            return true;
        if (StartsLineWithHeading(line, "Đối tượng thực hiện"))
            return true;
        if (languageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase) &&
            StartsLineWithHeading(line, "How to proceed"))
            return true;
        if (languageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase) &&
            StartsLineWithHeading(line, "Implementation procedures"))
            return true;
        if (languageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase) &&
            StartsLineWithHeading(line, "Target audience"))
            return true;
        return false;
    }

    private static bool EndsImplementationBlock(string line, string languageCode)
    {
        if (StartsLineWithHeading(line, "Đối tượng thực hiện"))
            return true;
        if (StartsLineWithHeading(line, "Cơ quan thực hiện"))
            return true;
        if (languageCode.StartsWith("en", StringComparison.OrdinalIgnoreCase) &&
            (StartsLineWithHeading(line, "Target audience") || StartsLineWithHeading(line, "Implementing agency")))
            return true;
        return false;
    }

    private static bool StartsLineWithHeading(string line, string heading)
    {
        var t = line.TrimStart('*', ' ', '\t');
        return t.StartsWith(heading, StringComparison.OrdinalIgnoreCase);
    }

    private static void AppendLine(StringBuilder sb, string line)
    {
        if (sb.Length > 0)
            sb.AppendLine();
        sb.AppendLine(line.TrimEnd());
    }

    private static void AppendTableSummary(StringBuilder sb, XWPFTable table)
    {
        var rows = table.Rows;
        if (rows is null || rows.Count == 0)
            return;

        var matrix = new List<List<string>>();
        foreach (var row in rows)
        {
            var cells = row.GetTableCells();
            var line = new List<string>();
            foreach (var cell in cells)
                line.Add(NormalizeCellText(cell));
            matrix.Add(line);
        }

        while (matrix.Count > 0 && matrix[0].All(string.IsNullOrWhiteSpace))
            matrix.RemoveAt(0);
        if (matrix.Count == 0)
            return;

        var header = matrix[0].Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        var hasHeader = header.Count >= 2 && matrix.Count > 1 && RowLooksLikeHeaderRow(matrix[0]);
        var dataRows = hasHeader ? matrix.Skip(1) : matrix;

        if (sb.Length > 0)
            sb.AppendLine();

        foreach (var dataRow in dataRows)
        {
            var cells = dataRow.Select(c => c.Trim()).Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
            if (cells.Count == 0)
                continue;

            if (hasHeader && header.Count > 0)
            {
                sb.AppendLine("• " + FormatLabeledTableRow(header, cells));
                continue;
            }

            sb.AppendLine("• " + string.Join(" — ", cells));
        }

        sb.AppendLine();
    }

    private static bool RowLooksLikeHeaderRow(IReadOnlyList<string> cells)
    {
        var nonEmpty = cells.Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
        if (nonEmpty.Count < 2)
            return false;
        // Header cells are usually short; data rows often have one long cell.
        return nonEmpty.All(c => c.Length <= 72);
    }

    private static string FormatLabeledTableRow(IReadOnlyList<string> headers, IReadOnlyList<string> cells)
    {
        var parts = new List<string>();
        var n = Math.Min(headers.Count, cells.Count);
        for (var i = 0; i < n; i++)
        {
            var h = headers[i].Trim().TrimEnd(':', ' ');
            var v = cells[i].Trim();
            if (string.IsNullOrWhiteSpace(v))
                continue;
            parts.Add($"{h}: {v}");
        }

        if (parts.Count == 0)
            return string.Join(" — ", cells);
        return string.Join("\n  ", parts);
    }

    private static string NormalizeCellText(XWPFTableCell cell)
    {
        var sb = new StringBuilder();
        foreach (var p in cell.Paragraphs)
        {
            var t = NormalizeSingleLine(p.Text);
            if (string.IsNullOrWhiteSpace(t))
                continue;
            if (sb.Length > 0)
                sb.Append(' ');
            sb.Append(t);
        }

        return sb.ToString().Trim();
    }

    private static string NormalizeSingleLine(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return string.Empty;
        var parts = s.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", parts).Trim();
    }

    private static string PostFormatSection(string s, string languageCode)
    {
        if (string.IsNullOrWhiteSpace(s))
            return string.Empty;

        var lines = s.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n', StringSplitOptions.None)
            .Select(l => l.TrimEnd())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Where(l => !IsBareDossierHeadingLine(l, languageCode) && !IsBareImplementationHeadingLine(l, languageCode))
            .ToList();

        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            // Light readability: break very long merged lines after ';' when followed by space.
            var chunk = line;
            if (chunk.Length > 120 && chunk.Contains("; ", StringComparison.Ordinal))
                chunk = chunk.Replace("; ", ";\n", StringComparison.Ordinal);

            if (sb.Length > 0)
                sb.AppendLine();
            sb.Append(chunk);
        }

        return sb.ToString().Trim();
    }

    private static async Task<Stream?> TryOpenAssetAsync(string path, CancellationToken ct)
    {
        Exception? last = null;
        foreach (var c in EnumerateAssetPathCandidates(path))
        {
            try
            {
                return await FileSystem.OpenAppPackageFileAsync(c).WaitAsync(ct).ConfigureAwait(false);
            }
            catch (FileNotFoundException) { last = null; }
            catch (Exception ex) { last = ex; }
        }

        if (last is not null)
            Debug.WriteLine($"[ProcedureDocumentExtractor] OpenAppPackageFileAsync failed for '{path}': {last.Message}");
        else
            Debug.WriteLine($"[ProcedureDocumentExtractor] Asset not found (tried all candidates): '{path}'");
        return null;
    }

    /// <summary>
    /// MauiAsset <c>LogicalName</c> is usually <c>Files\name.doc</c> while code may use <c>Files/name.doc</c>;
    /// some setups only expose the file name. Try several spellings.
    /// </summary>
    private static IEnumerable<string> EnumerateAssetPathCandidates(string path)
    {
        var trimmed = path.Trim();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        void Add(string? s)
        {
            if (string.IsNullOrEmpty(s))
                return;
            var a = s.Replace('\\', '/');
            if (seen.Add(a))
            { /* primary: forward slashes */ }
            var b = s.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            if (!string.Equals(a, b, StringComparison.Ordinal))
                seen.Add(b);
        }

        Add(trimmed);
        var slash = trimmed.Replace('\\', '/');
        var fileName = Path.GetFileName(slash);
        if (!string.IsNullOrEmpty(fileName))
        {
            Add(fileName);
            Add($"Files/{fileName}");
            Add($"files/{fileName}");
        }

        return seen;
    }

    /// <summary>Office Open XML (.docx) packages start with PK (ZIP).</summary>
    private static bool IsOfficeOpenXmlZip(Stream stream)
    {
        var pos = stream.Position;
        try
        {
            Span<byte> sig = stackalloc byte[2];
            return stream.Read(sig) == 2 && sig[0] == 0x50 && sig[1] == 0x4B;
        }
        finally
        {
            stream.Position = pos;
        }
    }

    private static string ReadDocxText(MemoryStream ms)
    {
        var doc = new XWPFDocument(ms);
        try
        {
            // doc.Paragraphs omits table rows and breaks heading-based splitting — mirror BodyElements order.
            return ReadDocxBodyPlainText(doc);
        }
        finally
        {
            doc.Close();
        }
    }

    /// <summary>Flatten body paragraphs and table rows in document order (same as structured extraction).</summary>
    private static string ReadDocxBodyPlainText(XWPFDocument doc)
    {
        var sb = new StringBuilder();
        foreach (var el in doc.BodyElements)
        {
            if (el is XWPFParagraph para)
            {
                var t = NormalizeSingleLine(para.Text);
                if (string.IsNullOrEmpty(t))
                    continue;
                if (sb.Length > 0)
                    sb.AppendLine();
                sb.Append(t);
            }
            else if (el is XWPFTable table)
            {
                foreach (var row in table.Rows)
                {
                    var cells = row.GetTableCells()
                        .Select(NormalizeCellText)
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .ToList();
                    if (cells.Count == 0)
                        continue;
                    if (sb.Length > 0)
                        sb.AppendLine();
                    sb.Append(string.Join(" | ", cells));
                }
            }
        }

        return sb.ToString();
    }

    private static string ReadDocText(MemoryStream ms)
    {
        var hwpf = new HWPFDocument(ms);
        try
        {
            // WordExtractor walks paragraphs/body more reliably than Range.Text for many templates.
            var extracted = new WordExtractor(hwpf).Text;
            if (!string.IsNullOrWhiteSpace(extracted))
                return extracted;
            return hwpf.GetRange().Text ?? string.Empty;
        }
        finally
        {
            hwpf.Close();
        }
    }

    private static ProcedureDocumentSections ExtractVietnamese(string text)
    {
        const string dossierLabel = "Thành phần hồ sơ";
        const string implLabel = "Cách thức thực hiện";
        return SplitByTwoSections(text, dossierLabel, implLabel);
    }

    /// <summary>
    /// Many bundled forms are Vietnamese-only. Try English headings; if missing, parse with Vietnamese labels.
    /// </summary>
    private static ProcedureDocumentSections ExtractEnglishPreferringVinText(string text)
    {
        var en = SplitByTwoSections(text, "Dossier components", "Implementation procedures");
        if (!string.IsNullOrWhiteSpace(en.Dossier) || !string.IsNullOrWhiteSpace(en.Implementation))
            return en;

        en = SplitByTwoSections(text, "Dossier", "Implementation");
        if (!string.IsNullOrWhiteSpace(en.Dossier) || !string.IsNullOrWhiteSpace(en.Implementation))
            return en;

        return ExtractVietnamese(text);
    }

    private static ProcedureDocumentSections SplitByTwoSections(string text, string labelA, string labelB)
    {
        var norm = NormalizeWhitespace(text);
        if (string.IsNullOrEmpty(norm))
            return default;

        var iA = IndexOfHeader(norm, labelA);
        var iB = IndexOfHeader(norm, labelB);

        if (iA < 0 && iB < 0)
            return default;

        if (iA >= 0 && iB < 0)
        {
            var body = TakeAfterHeader(norm, iA, labelA);
            return new ProcedureDocumentSections(body.Trim(), string.Empty);
        }

        if (iB >= 0 && iA < 0)
        {
            var body = TakeAfterHeader(norm, iB, labelB);
            return new ProcedureDocumentSections(string.Empty, body.Trim());
        }

        var endA = MatchHeaderEnd(norm, iA, labelA);
        var endB = MatchHeaderEnd(norm, iB, labelB);

        // Both found: assign by document order (dossier often appears before implementation).
        if (iA < iB)
        {
            var dossier = norm.Substring(endA, iB - endA).Trim();
            var impl = norm[endB..].Trim();
            return new ProcedureDocumentSections(dossier, TrimToNextMajorSection(impl));
        }

        // Implementation heading may appear before dossier in the export; slice is between headings only.
        var implFirst = norm.Substring(endB, iA - endB).Trim();

        var dossierRest = norm[endA..].Trim();
        return new ProcedureDocumentSections(TrimToNextMajorSection(dossierRest), TrimToNextMajorSection(implFirst));
    }

    /// <summary>
    /// Finds <paramref name="marker"/> where it starts a block (beginning of text or after a newline),
    /// so phrases like "yêu cầu" inside sentences are not treated as the next form section.
    /// </summary>
    private static int IndexOfSectionHeader(string text, string marker)
    {
        var searchFrom = 0;
        while (searchFrom < text.Length)
        {
            var i = text.IndexOf(marker, searchFrom, StringComparison.OrdinalIgnoreCase);
            if (i < 0)
                return -1;
            if (i == 0 || text[i - 1] == '\n')
                return i;
            searchFrom = i + 1;
        }

        return -1;
    }

    private static int IndexOfHeader(string haystack, string label)
    {
        var idx = haystack.IndexOf(label, StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
            return idx;

        // Templates sometimes type headings without full Vietnamese diacritics.
        var foldedLabel = RemoveDiacritics(label);
        if (string.IsNullOrEmpty(foldedLabel))
            return -1;

        var max = haystack.Length;
        for (var oi = 0; oi < max; oi++)
        {
            var remaining = max - oi;
            if (remaining < foldedLabel.Length)
                break;

            var take = Math.Min(remaining, Math.Max(label.Length + 48, foldedLabel.Length + 24));
            var window = haystack.Substring(oi, take);
            var foldedWindow = RemoveDiacritics(window);
            if (foldedWindow.StartsWith(foldedLabel, StringComparison.OrdinalIgnoreCase))
                return oi;
        }

        return -1;
    }

    private static string TakeAfterHeader(string haystack, int headerStart, string label) =>
        haystack[MatchHeaderEnd(haystack, headerStart, label)..];

    /// <summary>Index immediately after the header text (skips trailing ':' / whitespace).</summary>
    private static int MatchHeaderEnd(string haystack, int start, string label)
    {
        if (start < 0 || start >= haystack.Length)
            return start;

        if (haystack.AsSpan(start).StartsWith(label.AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            var e = start + label.Length;
            while (e < haystack.Length && (haystack[e] == ':' || char.IsWhiteSpace(haystack[e])))
                e++;
            return e;
        }

        var target = RemoveDiacritics(label).Trim().TrimEnd(':', ' ');
        if (string.IsNullOrEmpty(target))
            return start + label.Length;

        var maxEnd = Math.Min(haystack.Length, start + Math.Max(label.Length + 48, target.Length + 32));
        for (var end = start + 1; end <= maxEnd; end++)
        {
            var slice = haystack.Substring(start, end - start);
            var fs = RemoveDiacritics(slice).Trim().TrimEnd(':', ' ', '\t', '\r', '\n');
            if (!fs.Equals(target, StringComparison.OrdinalIgnoreCase))
                continue;

            while (end < haystack.Length && (haystack[end] == ':' || char.IsWhiteSpace(haystack[end])))
                end++;
            return end;
        }

        return start + label.Length;
    }

    private static string TrimToNextMajorSection(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        ReadOnlySpan<string> cutMarkers =
        [
            "Đối tượng thực hiện",
            "Doi tuong thuc hien",
            "Cơ quan thực hiện",
            "Co quan thuc hien",
            "Kết quả thực hiện",
            "Ket qua thuc hien",
            "Phí, lệ phí",
            "Phi, le phi",
            "Yêu cầu, điều kiện",
            "Yeu cau, dieu kien",
            "Căn cứ pháp lý",
            "Can cu phap ly",
        ];

        var earliest = int.MaxValue;
        foreach (var m in cutMarkers)
        {
            var idx = IndexOfSectionHeader(text, m);
            if (idx >= 0 && idx < earliest)
                earliest = idx;
        }

        if (earliest < int.MaxValue)
            return text[..earliest].Trim();

        return text.Trim();
    }

    private static string NormalizeWhitespace(string s) =>
        string.Join("\n", s
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n', StringSplitOptions.None)
            .Select(line => line.TrimEnd()))
            .Trim();

    private static string RemoveDiacritics(string s)
    {
        var normalized = s.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
