namespace Daycare.Domain.Entities;

/// <summary>
/// Metric assessing child sleep quality during a nap.
/// </summary>
public enum NapQuality
{
    Peaceful = 1,
    Restless = 2,
    Fussy = 3,
    Refused = 4
}
