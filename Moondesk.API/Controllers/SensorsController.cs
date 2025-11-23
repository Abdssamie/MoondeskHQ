using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models.IoT;
using Swashbuckle.AspNetCore.Annotations;

namespace Moondesk.API.Controllers;

[SwaggerTag("Manage sensors attached to assets")]
[Authorize(Policy = "OrgMember")]
public class SensorsController : BaseApiController
{
    private readonly ISensorRepository _sensorRepository;

    public SensorsController(ISensorRepository sensorRepository)
    {
        _sensorRepository = sensorRepository;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "List sensors", Description = "Get all sensors, optionally filtered by asset")]
    [SwaggerResponse(200, "Success", typeof(IEnumerable<Sensor>))]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<IActionResult> GetAll([FromQuery] int? asset_id = null)
    {
        if (!HasOrganization()) return Unauthorized();
        
        var sensors = asset_id.HasValue 
            ? await _sensorRepository.GetByAssetIdAsync(asset_id.Value)
            : await _sensorRepository.GetAllAsync();
            
        return Ok(sensors);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get sensor by ID")]
    [SwaggerResponse(200, "Success", typeof(Sensor))]
    [SwaggerResponse(404, "Sensor not found")]
    public async Task<IActionResult> GetById(int id)
    {
        if (!HasOrganization()) return Unauthorized();
        
        var sensor = await _sensorRepository.GetByIdAsync(id);
        if (sensor == null) return NotFound();
        
        return Ok(sensor);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create sensor", Description = "Add a new sensor to an asset")]
    [SwaggerResponse(201, "Sensor created", typeof(Sensor))]
    public async Task<IActionResult> Create([FromBody] Sensor sensor)
    {
        if (!HasOrganization()) return Unauthorized();
        
        sensor.OrganizationId = OrganizationId!;
        var created = await _sensorRepository.AddAsync(sensor);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Update sensor", Description = "Update sensor configuration including thresholds")]
    [SwaggerResponse(204, "Sensor updated")]
    [SwaggerResponse(404, "Sensor not found")]
    public async Task<IActionResult> Update(int id, [FromBody] Sensor sensor)
    {
        if (!HasOrganization()) return Unauthorized();
        
        var existing = await _sensorRepository.GetByIdAsync(id);
        if (existing == null) return NotFound();
        
        sensor.Id = id;
        sensor.OrganizationId = OrganizationId!;
        await _sensorRepository.UpdateAsync(sensor);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete sensor")]
    [SwaggerResponse(204, "Sensor deleted")]
    [SwaggerResponse(404, "Sensor not found")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!HasOrganization()) return Unauthorized();
        
        var existing = await _sensorRepository.GetByIdAsync(id);
        if (existing == null) return NotFound();
        
        await _sensorRepository.DeleteAsync(id);
        return NoContent();
    }
}
