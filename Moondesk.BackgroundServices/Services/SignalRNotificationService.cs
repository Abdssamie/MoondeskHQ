using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moondesk.Domain.Interfaces.Services;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.BackgroundServices.Services;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<Moondesk.API.Hubs.SensorDataHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubContext<Moondesk.API.Hubs.SensorDataHub> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendAlertNotificationAsync(Alert alert, string organizationId)
    {
        try
        {
            await _hubContext.Clients
                .Group($"org_{organizationId}")
                .SendAsync("ReceiveAlert", new
                {
                    alert.Id,
                    alert.SensorId,
                    alert.Severity,
                    alert.Message,
                    alert.Timestamp,
                    alert.TriggerValue,
                    alert.ThresholdValue
                });

            _logger.LogInformation("Alert {AlertId} sent to organization {OrgId} via SignalR", 
                alert.Id, organizationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send alert via SignalR");
        }
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // TODO: Implement email sending (SendGrid, AWS SES, etc.)
        _logger.LogInformation("Email notification: {To} - {Subject}", to, subject);
        await Task.CompletedTask;
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        // TODO: Implement SMS sending (Twilio, AWS SNS, etc.)
        _logger.LogInformation("SMS notification: {Phone} - {Message}", phoneNumber, message);
        await Task.CompletedTask;
    }

    public async Task SendSensorReadingAsync(Reading reading, string organizationId)
    {
        try
        {
            await _hubContext.Clients
                .Group($"org_{organizationId}")
                .SendAsync("ReceiveReading", new
                {
                    reading.SensorId,
                    reading.Timestamp,
                    reading.Value,
                    reading.Quality
                });

            _logger.LogDebug("Reading sent to organization {OrgId} via SignalR", organizationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send reading via SignalR");
        }
    }
}
