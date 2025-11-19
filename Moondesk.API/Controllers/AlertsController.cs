using Microsoft.AspNetCore.Mvc;
using Moondesk.Domain.Interfaces.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace Moondesk.API.Controllers;

[SwaggerTag("View and manage threshold violation alerts")]
public class AlertsController : BaseApiController
{
    private readonly IAlertRepository _alertRepository;

    public AlertsController(IAlertRepository alertRepository)
    {
        _alertRepository = alertRepository;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "List alerts", Description = "Get all alerts, optionally filtered by acknowledgment status")]
    [SwaggerResponse(200, "Success")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<IActionResult> GetAll([FromQuery] bool? acknowledged = null)
    {
        if (!HasOrganization()) return Unauthorized();
        
        var alerts = await _alertRepository.GetAlertsAsync();
        
        if (acknowledged.HasValue)
        {
            alerts = alerts.Where(a => a.Acknowledged == acknowledged.Value);
        }
        
        return Ok(alerts);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get alert by ID")]
    [SwaggerResponse(200, "Success")]
    [SwaggerResponse(404, "Alert not found")]
    public async Task<IActionResult> GetById(long id)
    {
        if (!HasOrganization()) return Unauthorized();
        
        var alert = await _alertRepository.GetAlertAsync(id);
        if (alert == null) return NotFound();
        
        return Ok(alert);
    }

    [HttpGet("by_sensor/{sensor_id}")]
    [SwaggerOperation(Summary = "Get alerts by sensor", Description = "Get all alerts for a specific sensor")]
    [SwaggerResponse(200, "Success")]
    public async Task<IActionResult> GetBySensor(long sensor_id)
    {
        if (!HasOrganization()) return Unauthorized();
        
        var alerts = await _alertRepository.GetAlertsBySensorAsync(sensor_id);
        return Ok(alerts);
    }

    [HttpPost("{id}/acknowledge")]
    [SwaggerOperation(Summary = "Acknowledge alert", Description = "Mark alert as reviewed by current user")]
    [SwaggerResponse(204, "Alert acknowledged")]
    [SwaggerResponse(404, "Alert not found")]
    public async Task<IActionResult> Acknowledge(long id)
    {
        if (!HasOrganization() || !IsAuthenticated()) return Unauthorized();
        
        var alert = await _alertRepository.GetAlertAsync(id);
        if (alert == null) return NotFound();
        
        alert.Acknowledged = true;
        alert.AcknowledgedAt = DateTimeOffset.UtcNow;
        alert.AcknowledgedBy = UserId;
        
        await _alertRepository.UpdateAlertAsync(id, alert);
        return NoContent();
    }
}
