using Routes.Models;

namespace Recipes.Data
{
    public interface IRouteRepository
    {
        Task<IEnumerable<HikingRoute>> GetAllRoutesAsync();
    }
}
