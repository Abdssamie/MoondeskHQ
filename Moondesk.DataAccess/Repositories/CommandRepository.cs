using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moondesk.DataAccess.Data;
using Moondesk.Domain.Enums;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models.IoT;

namespace Moondesk.DataAccess.Repositories;

public class CommandRepository : ICommandRepository
{
    private readonly MoondeskDbContext _context;
    private readonly ILogger<CommandRepository> _logger;

    public CommandRepository(MoondeskDbContext context, ILogger<CommandRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Command?> GetByIdAsync(long id)
    {
        try
        {
            return await _context.Commands
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving command with ID: {CommandId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Command>> GetBySensorIdAsync(long sensorId)
    {
        try
        {
            return await _context.Commands
                .AsNoTracking()
                .Where(c => c.SensorId == sensorId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving commands for sensor: {SensorId}", sensorId);
            throw;
        }
    }

    public async Task<IEnumerable<Command>> GetByStatusAsync(CommandStatus status)
    {
        try
        {
            return await _context.Commands
                .AsNoTracking()
                .Where(c => c.Status == status)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving commands by status: {Status}", status);
            throw;
        }
    }

    public async Task<IEnumerable<Command>> GetPendingCommandsAsync(string organizationId)
    {
        try
        {
            return await _context.Commands
                .AsNoTracking()
                .Where(c => c.OrganizationId == organizationId && c.Status == CommandStatus.Pending)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending commands for org: {OrganizationId}", organizationId);
            throw;
        }
    }

    public async Task<Command> AddAsync(Command command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        try
        {
            _context.Commands.Add(command);
            await _context.SaveChangesAsync();
            return command;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating command");
            throw;
        }
    }

    public async Task UpdateAsync(Command command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        try
        {
            var existing = await _context.Commands.FindAsync(command.Id);
            if (existing == null)
                return;

            _context.Entry(existing).CurrentValues.SetValues(command);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating command: {CommandId}", command?.Id);
            throw;
        }
    }

    public async Task DeleteAsync(long id)
    {
        try
        {
            var command = await _context.Commands.FindAsync(id);
            if (command == null)
                return;

            _context.Commands.Remove(command);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting command: {CommandId}", id);
            throw;
        }
    }
}
