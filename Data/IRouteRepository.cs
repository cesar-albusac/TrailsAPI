using Routes.Models;

namespace Recipes.Data
{
    public interface IRouteRepository
    {
        Task<IEnumerable<HikingRoute>> GetAllRoutesAsync();

        Task<HikingRoute> GetRouteAsync(string id);

        Task<HikingRoute> AddRouteAsync(HikingRoute route);

        Task<HikingRoute> UpdateRouteAsync(HikingRoute route);

        Task DeleteRouteAsync(string id);

        Task DeleteAllRoutesAsync();

    }
}
