using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AquaPP.Core.Interfaces;
using AquaPP.Core.Models.IoT;
using AquaPP.Data;
using Microsoft.EntityFrameworkCore;

namespace AquaPP.Infrastructure.Data.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly ApplicationDbContext _context;

    public AssetRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Asset>> GetAllAsync()
    {
        return await _context.Assets
            .Include(a => a.Sensors)
            .ToListAsync();
    }

    public async Task<Asset?> GetByIdAsync(int id)
    {
        return await _context.Assets.FindAsync(id);
    }

    public async Task<Asset?> GetByIdWithSensorsAsync(int id)
    {
        return await _context.Assets
            .Include(a => a.Sensors)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Asset> AddAsync(Asset asset)
    {
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
        return asset;
    }

    public async Task UpdateAsync(Asset asset)
    {
        _context.Assets.Update(asset);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset != null)
        {
            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Asset>> GetAssetsByStatusAsync(AssetStatus status)
    {
        return await _context.Assets
            .Where(a => a.Status == status)
            .Include(a => a.Sensors)
            .ToListAsync();
    }

    public async Task<IEnumerable<Asset>> GetAssetsByTypeAsync(string type)
    {
        return await _context.Assets
            .Where(a => a.Type == type)
            .Include(a => a.Sensors)
            .ToListAsync();
    }
}
