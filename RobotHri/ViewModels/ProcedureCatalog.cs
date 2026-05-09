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
                StringIds.PROCEDURE_POINT_1_NOTE.GetString()),
            new ProcedureItem(
                "DestinationPoint2",
                StringIds.PROCEDURE_NAME_2.GetString(),
                StringIds.PROCEDURE_POINT_2_DOCS.GetString(),
                StringIds.PROCEDURE_POINT_2_COUNTER.GetString(),
                StringIds.PROCEDURE_POINT_2_NOTE.GetString()),
            new ProcedureItem(
                "DestinationPoint3",
                StringIds.PROCEDURE_NAME_3.GetString(),
                StringIds.PROCEDURE_POINT_3_DOCS.GetString(),
                StringIds.PROCEDURE_POINT_3_COUNTER.GetString(),
                StringIds.PROCEDURE_POINT_3_NOTE.GetString()),
            new ProcedureItem(
                "DestinationPoint4",
                StringIds.PROCEDURE_NAME_4.GetString(),
                StringIds.PROCEDURE_POINT_4_DOCS.GetString(),
                StringIds.PROCEDURE_POINT_4_COUNTER.GetString(),
                StringIds.PROCEDURE_POINT_4_NOTE.GetString()),
            new ProcedureItem(
                "DestinationPoint5",
                StringIds.PROCEDURE_NAME_5.GetString(),
                StringIds.PROCEDURE_POINT_5_DOCS.GetString(),
                StringIds.PROCEDURE_POINT_5_COUNTER.GetString(),
                StringIds.PROCEDURE_POINT_5_NOTE.GetString()),
            new ProcedureItem(
                "DestinationPoint6",
                StringIds.PROCEDURE_NAME_6.GetString(),
                StringIds.PROCEDURE_POINT_6_DOCS.GetString(),
                StringIds.PROCEDURE_POINT_6_COUNTER.GetString(),
                StringIds.PROCEDURE_POINT_6_NOTE.GetString()),
            new ProcedureItem(
                "DestinationPoint7",
                StringIds.PROCEDURE_NAME_7.GetString(),
                StringIds.PROCEDURE_POINT_7_DOCS.GetString(),
                StringIds.PROCEDURE_POINT_7_COUNTER.GetString(),
                StringIds.PROCEDURE_POINT_7_NOTE.GetString()),
            new ProcedureItem(
                "DestinationPoint8",
                StringIds.PROCEDURE_NAME_8.GetString(),
                StringIds.PROCEDURE_POINT_8_DOCS.GetString(),
                StringIds.PROCEDURE_POINT_8_COUNTER.GetString(),
                StringIds.PROCEDURE_POINT_8_NOTE.GetString()),
            new ProcedureItem(
                "DestinationPoint9",
                StringIds.PROCEDURE_NAME_9.GetString(),
                StringIds.PROCEDURE_POINT_9_DOCS.GetString(),
                StringIds.PROCEDURE_POINT_9_COUNTER.GetString(),
                StringIds.PROCEDURE_POINT_9_NOTE.GetString())
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

    public ProcedureItem(string destinationKey, string name, string requiredDocs, string counterName, string note)
    {
        DestinationKey = destinationKey;
        Name = name;
        RequiredDocs = requiredDocs;
        CounterName = counterName;
        Note = note;
    }
}
