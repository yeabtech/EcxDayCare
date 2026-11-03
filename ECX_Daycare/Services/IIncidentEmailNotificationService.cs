using Daycare.Domain.Entities;

namespace ECX_Daycare.Services;

/// <summary>Delivers incident notification emails to a child's linked parents.</summary>
public interface IIncidentEmailNotificationService
{
    /// <summary>Sends a privacy-preserving email for a parent-notified incident.</summary>
    /// <param name="incident">The incident report.</param>
    /// <param name="childName">The child's display name.</param>
    /// <param name="recipientEmails">The linked parents' email addresses.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SendIncidentNotificationAsync(
        Incident incident,
        string childName,
        IReadOnlyCollection<string> recipientEmails,
        CancellationToken cancellationToken);
}
