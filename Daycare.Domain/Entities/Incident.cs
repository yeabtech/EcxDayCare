using Daycare.Domain.Results;

namespace Daycare.Domain.Entities;

/// <summary>
/// Represents an incident (injury, accident, or safety concern) involving a child.
/// Contains sensitive information with strict access control requirements.
/// </summary>
public sealed class Incident : BaseEntity<IncidentId>
{
    public ChildId ChildId { get; private set; } = null!;
    public DateTime IncidentTime { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public InjuryType InjuryType { get; private set; }
    public BodyPart AffectedBodyPart { get; private set; }
    public string? FirstAidGiven { get; private set; }
    public Guid StaffId { get; private set; }
    public bool ParentNotified { get; private set; }
    public string? ParentSignature { get; private set; }
    public string? FollowUp { get; private set; }
    public string? Notes { get; private set; }

    // Navigation property
    public Child? Child { get; private set; }

    private Incident() { }

    private Incident(
        IncidentId id,
        ChildId childId,
        DateTime incidentTime,
        string location,
        string description,
        InjuryType injuryType,
        BodyPart affectedBodyPart,
        string? firstAidGiven,
        Guid staffId,
        bool parentNotified,
        string? parentSignature,
        string? followUp,
        string? notes)
        : base(id)
    {
        ChildId = childId;
        IncidentTime = DateTime.SpecifyKind(incidentTime, DateTimeKind.Utc);
        Location = location;
        Description = description;
        InjuryType = injuryType;
        AffectedBodyPart = affectedBodyPart;
        FirstAidGiven = Trim(firstAidGiven);
        StaffId = staffId;
        ParentNotified = parentNotified;
        ParentSignature = Trim(parentSignature);
        FollowUp = Trim(followUp);
        Notes = Trim(notes);
    }

    // ─── Factory ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new Incident with domain validations.
    /// </summary>
    public static Result<Incident> Create(
        ChildId childId,
        DateTime incidentTime,
        string location,
        string description,
        InjuryType injuryType,
        BodyPart affectedBodyPart,
        string? firstAidGiven,
        Guid staffId,
        bool parentNotified = false,
        string? parentSignature = null,
        string? followUp = null,
        string? notes = null)
    {
        if (childId is null || childId.Value == Guid.Empty)
            return Result<Incident>.Failure("A valid Child ID is required.");

        if (staffId == Guid.Empty)
            return Result<Incident>.Failure("A valid Staff ID is required.");

        if (string.IsNullOrWhiteSpace(location))
            return Result<Incident>.Failure("Incident location is required.");

        if (string.IsNullOrWhiteSpace(description))
            return Result<Incident>.Failure("Incident description is required.");

        var utcTime = DateTime.SpecifyKind(incidentTime, DateTimeKind.Utc);

        if (utcTime > DateTime.UtcNow.AddMinutes(15))
            return Result<Incident>.Failure("Incident time cannot be in the future.");

        if (utcTime < DateTime.UtcNow.AddDays(-60))
            return Result<Incident>.Failure("Incident time cannot be older than 60 days.");

        var incident = new Incident(
            IncidentId.New(),
            childId,
            utcTime,
            location.Trim(),
            description.Trim(),
            injuryType,
            affectedBodyPart,
            firstAidGiven,
            staffId,
            parentNotified,
            parentSignature,
            followUp,
            notes);

        return Result<Incident>.Success(incident);
    }

    // ─── Behaviour ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Updates incident details.
    /// </summary>
    public Result Update(
        DateTime incidentTime,
        string location,
        string description,
        InjuryType injuryType,
        BodyPart affectedBodyPart,
        string? firstAidGiven,
        string? followUp,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(location))
            return Result.Failure("Incident location is required.");

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure("Incident description is required.");

        var utcTime = DateTime.SpecifyKind(incidentTime, DateTimeKind.Utc);

        if (utcTime > DateTime.UtcNow.AddMinutes(15))
            return Result.Failure("Incident time cannot be in the future.");

        if (utcTime < DateTime.UtcNow.AddDays(-60))
            return Result.Failure("Incident time cannot be older than 60 days.");

        IncidentTime = utcTime;
        Location = location.Trim();
        Description = description.Trim();
        InjuryType = injuryType;
        AffectedBodyPart = affectedBodyPart;
        FirstAidGiven = Trim(firstAidGiven);
        FollowUp = Trim(followUp);
        Notes = Trim(notes);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Records that parent has been notified of the incident.
    /// (In a real system, this might send a notification and record the timestamp and method)
    /// </summary>
    public Result NotifyParent()
    {
        if (ParentNotified)
            return Result.Failure("Parent has already been notified of this incident.");

        ParentNotified = true;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Records parent signature acknowledging the incident report.
    /// </summary>
    public Result RecordParentSignature(string signature)
    {
        if (string.IsNullOrWhiteSpace(signature))
            return Result.Failure("Parent signature cannot be empty.");

        if (!ParentNotified)
            return Result.Failure("Parent must be notified before recording signature.");

        ParentSignature = signature.Trim();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Records follow-up actions taken for the incident.
    /// </summary>
    public Result RecordFollowUp(string followUpDetails)
    {
        if (string.IsNullOrWhiteSpace(followUpDetails))
            return Result.Failure("Follow-up details cannot be empty.");

        FollowUp = followUpDetails.Trim();
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    // ─── Helpers ────────────────────────────────────────────────────────────────

    private static string? Trim(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
