using Microsoft.AspNetCore.Mvc;
using Moondesk.Domain.Interfaces.Repositories;
using Moondesk.Domain.Models.IoT;
using Swashbuckle.AspNetCore.Annotations;

namespace Moondesk.API.Controllers;

[SwaggerTag("Manage IoT assets (equipment) within an organization")]
public class AssetsController : BaseApiController
{
    private readonly IAssetRepository _assetRepository;

    public AssetsController(IAssetRepository assetRepository)
    {
        _assetRepository = assetRepository;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "List all assets", Description = "Get all assets for the current organization")]
    [SwaggerResponse(200, "Success", typeof(IEnumerable<Asset>))]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<IActionResult> GetAll()
    {
        if (!HasOrganization()) return Unauthorized();
        
        var assets = await _assetRepository.GetAllAsync();
        return Ok(assets);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get asset by ID", Description = "Get a specific asset with its sensors")]
    [SwaggerResponse(200, "Success", typeof(Asset))]
    [SwaggerResponse(404, "Asset not found")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<IActionResult> GetById(int id)
    {
        if (!HasOrganization()) return Unauthorized();
        
        var asset = await _assetRepository.GetByIdAsync(id);
        if (asset == null) return NotFound();
        
        return Ok(asset);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Create new asset", Description = "Register a new asset in the organization")]
    [SwaggerResponse(201, "Asset created", typeof(Asset))]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<IActionResult> Create([FromBody] Asset asset)
    {
        if (!HasOrganization()) return Unauthorized();
        
        asset.OrganizationId = OrganizationId!;
        var created = await _assetRepository.AddAsync(asset);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Update asset", Description = "Update an existing asset")]
    [SwaggerResponse(204, "Asset updated")]
    [SwaggerResponse(404, "Asset not found")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<IActionResult> Update(int id, [FromBody] Asset asset)
    {
        if (!HasOrganization()) return Unauthorized();
        
        var existing = await _assetRepository.GetByIdAsync(id);
        if (existing == null) return NotFound();
        
        asset.Id = id;
        asset.OrganizationId = OrganizationId!;
        await _assetRepository.UpdateAsync(asset);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete asset", Description = "Remove an asset and all its sensors")]
    [SwaggerResponse(204, "Asset deleted")]
    [SwaggerResponse(404, "Asset not found")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!HasOrganization()) return Unauthorized();
        
        var existing = await _assetRepository.GetByIdAsync(id);
        if (existing == null) return NotFound();
        
        await _assetRepository.DeleteAsync(id);
        return NoContent();
    }
}
