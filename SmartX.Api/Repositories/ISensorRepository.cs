using SmartX.Api.Models;

namespace SmartX.Api.Repositories
{
    public interface ISensorRepository
    {
        Task<IReadOnlyList<Sensor>> GetAllAsync();

        Task<Sensor?> GetByIdAsync(Guid id);

        Task<Sensor?> GetByDeviceIdentifierAsync(string deviceIdentifier);

        Task AddAsync(Sensor sensor);

        Task<bool> UpdateAsync(Sensor sensor);

        Task<bool> DeleteAsync(Guid id);
    }
}