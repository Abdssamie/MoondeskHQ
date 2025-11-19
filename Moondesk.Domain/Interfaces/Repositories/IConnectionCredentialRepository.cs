using Moondesk.Domain.Enums;
using Moondesk.Domain.Models.Network;

namespace Moondesk.Domain.Interfaces.Repositories;

public interface IConnectionCredentialRepository
{
    Task<ConnectionCredential?> GetByIdAsync(long id);
    Task<ConnectionCredential?> GetByNameAsync(string name, string organizationId);
    Task<IEnumerable<ConnectionCredential>> GetByOrganizationIdAsync(string organizationId);
    Task<IEnumerable<ConnectionCredential>> GetByProtocolAsync(Protocol protocol, string organizationId);
    Task<IEnumerable<ConnectionCredential>> GetActiveAsyncByOrganizationIdAsync(string organizationId);
    Task<ConnectionCredential> CreateAsync(ConnectionCredential credential);
    Task<ConnectionCredential> UpdateAsync(ConnectionCredential credential);
    Task DeleteAsync(long id);
    Task<bool> ExistsAsync(long id);
    Task UpdateLastUsedAsync(long id);
}
