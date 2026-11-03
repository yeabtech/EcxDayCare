namespace Daycare.Domain.Entities;

/// <summary>
/// Metric assessing child engagement in a scheduled activity.
/// </summary>
public enum ParticipationLevel
{
    Engaged = 1,
    Excited = 2,
    Observing = 3,
    Hesitant = 4,
    SoloPlay = 5
}
