using Microsoft.AspNetCore.Mvc;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models.IoT;
using Swashbuckle.AspNetCore.Annotations;

namespace Moondesk.API.Controllers;

[SwaggerTag("Send commands to IoT devices")]
public class CommandsController : BaseApiController
{
    private readonly ICommandRepository _commandRepository;

    public CommandsController(ICommandRepository commandRepository)
    {
        _commandRepository = commandRepository;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Get pending commands", Description = "Get all pending commands for the organization")]
    [SwaggerResponse(200, "Success")]
    public async Task<IActionResult> GetPending()
    {
        if (!HasOrganization()) return Unauthorized();

        var commands = await _commandRepository.GetPendingCommandsAsync(OrganizationId!);
        return Ok(commands);
    }

    [HttpGet("by_sensor/{sensor_id}")]
    [SwaggerOperation(Summary = "Get commands by sensor")]
    [SwaggerResponse(200, "Success")]
    public async Task<IActionResult> GetBySensor(long sensor_id)
    {
        if (!HasOrganization()) return Unauthorized();

        var commands = await _commandRepository.GetBySensorIdAsync(sensor_id);
        return Ok(commands);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get command by ID")]
    [SwaggerResponse(200, "Success")]
    [SwaggerResponse(404, "Command not found")]
    public async Task<IActionResult> GetById(long id)
    {
        if (!HasOrganization()) return Unauthorized();

        var command = await _commandRepository.GetByIdAsync(id);
        if (command == null) return NotFound();

        return Ok(command);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create command", Description = "Send a new command to a device")]
    [SwaggerResponse(201, "Command created")]
    public async Task<IActionResult> Create([FromBody] Command command)
    {
        if (!HasOrganization()) return Unauthorized();

        command.OrganizationId = OrganizationId!;
        command.UserId = UserId!;

        var created = await _commandRepository.AddAsync(command);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
