using Microsoft.Azure.Cosmos;
using Routes.Models;

namespace Recipes.Data
{
    public class RouteRepository : IRouteRepository
    {
        private readonly CosmosClient cosmosClient;
        private readonly IConfiguration configuration;
        private readonly Container container;

        public RouteRepository(CosmosClient cosmosClient, IConfiguration configuration)
        {
            this.cosmosClient = cosmosClient;
            this.configuration = configuration;
            var databaseName = configuration["CosmosDb:DatabaseName"];
            var containerName = configuration["CosmosDb:ContainerName"];
            this.container = this.cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task<IEnumerable<HikingRoute>> GetAllRoutesAsync()
        {
            var sqlQueryText = "SELECT * FROM routes";
 
            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            FeedIterator<HikingRoute> queryResultSetIterator = this.container.GetItemQueryIterator<HikingRoute>(queryDefinition);

            List<HikingRoute> routes = new List<HikingRoute>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<HikingRoute> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (HikingRoute route in currentResultSet)
                {
                    routes.Add(route);
                }
            }

            return routes;
        }
    }
}
