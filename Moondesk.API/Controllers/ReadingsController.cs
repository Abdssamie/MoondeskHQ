using Microsoft.AspNetCore.Mvc;
using Moondesk.Domain.Interfaces.Repositories;
using Swashbuckle.AspNetCore.Annotations;

namespace Moondesk.API.Controllers;

[SwaggerTag("Query sensor telemetry data")]
public class ReadingsController : BaseApiController
{
    private readonly IReadingRepository _readingRepository;

    public ReadingsController(IReadingRepository readingRepository)
    {
        _readingRepository = readingRepository;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Get recent readings", Description = "Get recent readings for a sensor with optional limit")]
    [SwaggerResponse(200, "Success")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<IActionResult> GetReadings([FromQuery] long sensor_id, [FromQuery] int limit = 100)
    {
        if (!HasOrganization()) return Unauthorized();
        
        var readings = await _readingRepository.GetRecentReadingsAsync(OrganizationId!, sensor_id, limit);
        return Ok(readings);
    }

    [HttpGet("by_sensor/{sensor_id}")]
    [SwaggerOperation(Summary = "Get all readings by sensor", Description = "Get all readings for a specific sensor")]
    [SwaggerResponse(200, "Success")]
    public async Task<IActionResult> GetBySensor(long sensor_id)
    {
        if (!HasOrganization()) return Unauthorized();
        
        var readings = await _readingRepository.GetReadingsBySensorAsync(sensor_id);
        return Ok(readings);
    }
}
