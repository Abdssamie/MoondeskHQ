using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AquaPP.Core.Interfaces;
using AquaPP.Core.Models.IoT;
using AquaPP.Data;
using Microsoft.EntityFrameworkCore;

namespace AquaPP.Infrastructure.Data.Repositories;

public class SensorRepository : ISensorRepository
{
    private readonly ApplicationDbContext _context;

    public SensorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Sensor>> GetAllAsync()
    {
        return await _context.Sensors
            .Include(s => s.Asset)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sensor>> GetAllWithAssetsAsync()
    {
        return await _context.Sensors
            .Include(s => s.Asset)
            .Include(s => s.Readings)
            .ToListAsync();
    }

    public async Task<Sensor?> GetByIdAsync(int id)
    {
        return await _context.Sensors
            .Include(s => s.Asset)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IEnumerable<Sensor>> GetByAssetIdAsync(int assetId)
    {
        return await _context.Sensors
            .Where(s => s.AssetId == assetId)
            .ToListAsync();
    }

    public async Task<Sensor> AddAsync(Sensor sensor)
    {
        _context.Sensors.Add(sensor);
        await _context.SaveChangesAsync();
        return sensor;
    }

    public async Task UpdateAsync(Sensor sensor)
    {
        _context.Sensors.Update(sensor);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var sensor = await _context.Sensors.FindAsync(id);
        if (sensor != null)
        {
            _context.Sensors.Remove(sensor);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Sensor>> GetActiveSensorsAsync()
    {
        return await _context.Sensors
            .Where(s => s.IsActive)
            .Include(s => s.Asset)
            .ToListAsync();
    }

    public async Task<IEnumerable<Sensor>> GetSensorsByTypeAsync(SensorType type)
    {
        return await _context.Sensors
            .Where(s => s.Type == type)
            .Include(s => s.Asset)
            .ToListAsync();
    }
}
