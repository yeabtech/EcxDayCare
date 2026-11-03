namespace Daycare.Domain.Entities;

/// <summary>
/// Classification of injuries recorded in an incident.
/// </summary>
public enum InjuryType
{
    Minor = 1,
    Moderate = 2,
    Severe = 3,
    HeadInjury = 4,
    AllergyReaction = 5,
    Other = 6
}
