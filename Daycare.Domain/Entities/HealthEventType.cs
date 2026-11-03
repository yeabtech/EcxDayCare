namespace Daycare.Domain.Entities;

/// <summary>
/// Types of health events that can be logged for a child.
/// </summary>
public enum HealthEventType
{
    Diaper = 1,
    Toilet = 2,
    Medication = 3,
    Temperature = 4,
    Symptoms = 5
}
