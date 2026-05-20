namespace RobotHri.Services;

public interface IProcedureDocumentExtractor
{
    /// <summary>
    /// Reads a bundled Word file and extracts "Thành phần hồ sơ" / "Cách thức thực hiện"
    /// (and English markers when present). <paramref name="languageCode"/> is "vi" or "en".
    /// </summary>
    Task<ProcedureDocumentSections> ExtractAsync(
        string? mauiAssetRelativePath,
        string languageCode,
        CancellationToken cancellationToken = default);
}

public readonly record struct ProcedureDocumentSections(string Dossier, string Implementation);
