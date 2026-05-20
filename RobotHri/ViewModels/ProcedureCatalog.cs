using RobotHri.Languages;

namespace RobotHri.ViewModels;

public static class ProcedureCatalog
{
    public static IReadOnlyList<ProcedureItem> BuildItems()
    {
        return new List<ProcedureItem>
        {
            new ProcedureItem(
                "DestinationPoint1",
                StringIds.PROCEDURE_NAME_1.GetString(),
                StringIds.PROCEDURE_POINT_1_DOCS.GetString(),
                StringIds.PROCEDURE_POINT_1_COUNTER.GetString(),
                StringIds.PROCEDURE_POINT_1_NOTE.GetString(),
                "Files/chungthubansao.doc",
                1),
            new ProcedureItem(
                "DestinationPoint2",
                StringIds.PROCEDURE_NAME_2.GetString(),
                StringIds.PROCEDURE_POINT_2_DOCS.GetString(),
                StringIds.PROCEDURE_POINT_2_COUNTER.GetString(),
                StringIds.PROCEDURE_POINT_2_NOTE.GetString(),
                "Files/chungthugiayto.doc",
                2),
            new ProcedureItem(
                "DestinationPoint3",
                StringIds.PROCEDURE_NAME_3.GetString(),
                StringIds.PROCEDURE_POINT_3_DOCS.GetString(),
                StringIds.PROCEDURE_POINT_3_COUNTER.GetString(),
                StringIds.PROCEDURE_POINT_3_NOTE.GetString(),
                "Files/khieunaitocao.doc",
                3),
            new ProcedureItem(
                "DestinationPoint4",
                StringIds.PROCEDURE_NAME_4.GetString(),
                StringIds.PROCEDURE_POINT_4_DOCS.GetString(),
                StringIds.PROCEDURE_POINT_4_COUNTER.GetString(),
                StringIds.PROCEDURE_POINT_4_NOTE.GetString(),
                "Files/dangkykhaisinh.doc",
                4),
            new ProcedureItem(
                "DestinationPoint5",
                StringIds.PROCEDURE_NAME_5.GetString(),
                StringIds.PROCEDURE_POINT_5_DOCS.GetString(),
                StringIds.PROCEDURE_POINT_5_COUNTER.GetString(),
                StringIds.PROCEDURE_POINT_5_NOTE.GetString(),
                "Files/khieunaitocao.doc",
                5),
            new ProcedureItem(
                "DestinationPoint6",
                StringIds.PROCEDURE_NAME_6.GetString(),
                StringIds.PROCEDURE_POINT_6_DOCS.GetString(),
                StringIds.PROCEDURE_POINT_6_COUNTER.GetString(),
                StringIds.PROCEDURE_POINT_6_NOTE.GetString(),
                "Files/kinhdoanhkaraoke.doc",
                6),
            new ProcedureItem(
                "DestinationPoint7",
                StringIds.PROCEDURE_NAME_7.GetString(),
                StringIds.PROCEDURE_POINT_7_DOCS.GetString(),
                StringIds.PROCEDURE_POINT_7_COUNTER.GetString(),
                StringIds.PROCEDURE_POINT_7_NOTE.GetString(),
                "Files/dangkymoitruong.doc",
                7),
            new ProcedureItem(
                "DestinationPoint8",
                StringIds.PROCEDURE_NAME_8.GetString(),
                StringIds.PROCEDURE_POINT_8_DOCS.GetString(),
                StringIds.PROCEDURE_POINT_8_COUNTER.GetString(),
                StringIds.PROCEDURE_POINT_8_NOTE.GetString(),
                "Files/xaynha.doc",
                8),
            new ProcedureItem(
                "DestinationPoint9",
                StringIds.PROCEDURE_NAME_9.GetString(),
                StringIds.PROCEDURE_POINT_9_DOCS.GetString(),
                StringIds.PROCEDURE_POINT_9_COUNTER.GetString(),
                StringIds.PROCEDURE_POINT_9_NOTE.GetString(),
                "Files/capthebaohiem.doc",
                9)
        };
    }
}

public class ProcedureItem
{
    public string DestinationKey { get; }
    public string Name { get; }
    public string RequiredDocs { get; }
    public string CounterName { get; }
    public string Note { get; }
    /// <summary>MauiAsset logical path under Resources/Raw (e.g. Files/foo.doc).</summary>
    public string? RawAssetPath { get; }
    /// <summary>1..9 for optional per-language overrides.</summary>
    public int ProcedureIndex { get; }

    public ProcedureItem(
        string destinationKey,
        string name,
        string requiredDocs,
        string counterName,
        string note,
        string? rawAssetPath = null,
        int procedureIndex = 0)
    {
        DestinationKey = destinationKey;
        Name = name;
        RequiredDocs = requiredDocs;
        CounterName = counterName;
        Note = note;
        RawAssetPath = rawAssetPath;
        ProcedureIndex = procedureIndex;
    }
}
