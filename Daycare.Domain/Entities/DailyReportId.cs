namespace Daycare.Domain.Entities;

/// <summary>
/// Strongly-typed identifier for a DailyReport aggregate entity.
/// </summary>
public sealed class DailyReportId : TypedId<DailyReportId>
{
    public DailyReportId() { }
    public DailyReportId(Guid value) : base(value) { }

    public static DailyReportId New() => new();
    public static DailyReportId Create() => new();
    public static DailyReportId Create(Guid value) => new(value);
    public static DailyReportId Create(string value) => new(Guid.Parse(value));
}
