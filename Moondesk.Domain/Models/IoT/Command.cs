using System.ComponentModel.DataAnnotations;
using Moondesk.Domain.Enums;

namespace Moondesk.Domain.Models.IoT;

public class Command
{
    public long Id { get; set; }
    
    public required string UserId { get; set; }
    
    public long SensorId { get; set; }
    
    public required string OrganizationId { get; set; }
    
    [MaxLength(100)]
    public required string CommandType { get; set; }
    
    public string? Payload { get; set; }
    
    public CommandStatus Status { get; set; } = CommandStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExecutedAt { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public string? ErrorMessage { get; set; }
    
    public string? Response { get; set; }
    
    public int RetryCount { get; set; } = 0;
    
    public int MaxRetries { get; set; } = 3;
    
    public Dictionary<string, string> Metadata { get; set; } = new();
    
    // Navigation properties
    public Sensor Sensor { get; set; }= null!;

    public void MarkAsExecuting()
    {
        Status = CommandStatus.Executing;
        ExecutedAt = DateTime.UtcNow;
    }

    public void MarkAsCompleted(string? response = null)
    {
        Status = CommandStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Response = response;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = CommandStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }

    public bool CanRetry() => RetryCount < MaxRetries && Status == CommandStatus.Failed;

    public void IncrementRetry()
    {
        RetryCount++;
        Status = CommandStatus.Pending;
        ErrorMessage = null;
    }
}
