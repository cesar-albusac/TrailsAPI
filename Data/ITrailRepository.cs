using Trails.Models;

namespace Trails.Data
{
    public interface ITrailRepository
    {
        Task<IEnumerable<Trail>> GetAllTrailsAsync();

        Task<Trail?> GetTrailAsync(string id);

        Task<Trail?> AddTrailAsync(Trail Trail);

        Task<Trail?> UpdateTrailAsync(Trail Trail);

        Task DeleteTrailAsync(string id);

        Task DeleteAllTrailsAsync();

    }
}
