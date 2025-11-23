using Moondesk.Domain.Models.IoT;

namespace Moondesk.Domain.Interfaces.Services;

public interface INotificationService
{
    Task SendAlertNotificationAsync(Alert alert, string organizationId);
    Task SendEmailAsync(string to, string subject, string body);
    Task SendSmsAsync(string phoneNumber, string message);
}
