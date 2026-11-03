using System.Globalization;
using System.Net.Sockets;
using System.Security.Authentication;
using Daycare.Domain.Entities;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace ECX_Daycare.Services;

internal sealed class IncidentEmailNotificationService : IIncidentEmailNotificationService
{
    private readonly IConfiguration _configuration;

    public IncidentEmailNotificationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendIncidentNotificationAsync(
        Incident incident,
        string childName,
        IReadOnlyCollection<string> recipientEmails,
        CancellationToken cancellationToken)
    {
        var senderEmail = _configuration["INCIDENT_EMAIL_ADDRESS"]?.Trim();
        var appPassword = _configuration["INCIDENT_EMAIL_APP_PASSWORD"]?.Trim();
        if (string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(appPassword))
            throw new IncidentEmailConfigurationException(
                "Configure INCIDENT_EMAIL_ADDRESS and INCIDENT_EMAIL_APP_PASSWORD before sending incident emails.");

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(
                senderEmail,
                appPassword.Replace(" ", string.Empty, StringComparison.Ordinal),
                cancellationToken);

            foreach (var recipientEmail in recipientEmails)
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("ECX Daycare", senderEmail));
                message.To.Add(MailboxAddress.Parse(recipientEmail));
                message.Subject = $"Incident report available for {childName}";
                message.Body = new TextPart("plain")
                {
                    Text = $"""
                        An incident report involving {childName} was recorded on {incident.IncidentTime.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture)}.

                        Please sign in to the ECX Daycare Parent Portal and open Notifications to review the report details and care provided.

                        If you need immediate assistance, please contact the daycare directly.

                        This is an automated message. Please do not reply.
                        """
                };

                await client.SendAsync(message, cancellationToken);
            }

            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception exception) when (
            exception is MailKit.Security.AuthenticationException
            or System.Security.Authentication.AuthenticationException
            or MailKit.ServiceNotConnectedException
            or MailKit.ServiceNotAuthenticatedException
            or MailKit.Net.Smtp.SmtpCommandException
            or MailKit.Net.Smtp.SmtpProtocolException
            or SocketException
            or IOException)
        {
            throw new IncidentEmailDeliveryException(
                "The incident email could not be delivered through Gmail SMTP.",
                exception);
        }
    }
}

internal sealed class IncidentEmailConfigurationException : Exception
{
    public IncidentEmailConfigurationException(string message) : base(message) { }
}

internal sealed class IncidentEmailDeliveryException : Exception
{
    public IncidentEmailDeliveryException(string message, Exception innerException)
        : base(message, innerException) { }
}
