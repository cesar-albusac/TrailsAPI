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
            var databaseName = configuration["CosmosDBSettings:DatabaseName"];
            var containerName = configuration["CosmosDBSettings:ContainerName"];
            this.container = this.cosmosClient.GetContainer(databaseName, containerName);
        }

        #region GET Methods

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

        public async Task<HikingRoute> GetRouteAsync(string id)
        {
            try
            {
                ItemResponse<HikingRoute> response = await this.container.ReadItemAsync<HikingRoute>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<IEnumerable<HikingRoute>> GetRoutesByDifficultyAsync(string difficulty)
        {
            var sqlQueryText = "SELECT * FROM routes r WHERE r.difficulty = @difficulty";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText).WithParameter("@difficulty", difficulty);

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

        #endregion

        #region POST Methods
        public async Task<HikingRoute> AddRouteAsync(HikingRoute route)
        {
            ItemResponse<HikingRoute> response = await this.container.CreateItemAsync<HikingRoute>(route, new PartitionKey(route.Id));
            return response.Resource;
        }

        #endregion

        #region PUT Methods

        public async Task<HikingRoute> UpdateRouteAsync(HikingRoute route)
        {
            ItemResponse<HikingRoute> response = await this.container.UpsertItemAsync<HikingRoute>(route, new PartitionKey(route.Id));
            return response.Resource;
        }

        #endregion

        #region DELETE Methods

        public async Task DeleteRouteAsync(string id)
        {
            await this.container.DeleteItemAsync<HikingRoute>(id, new PartitionKey(id));
        }
                          
        public async Task DeleteAllRoutesAsync()
        {
            var sqlQueryText = "SELECT * FROM routes";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            FeedIterator<HikingRoute> queryResultSetIterator = this.container.GetItemQueryIterator<HikingRoute>(queryDefinition);

            List<HikingRoute> routes = new List<HikingRoute>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<HikingRoute> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (HikingRoute route in currentResultSet)
                {
                    await this.container.DeleteItemAsync<HikingRoute>(route.Id, new PartitionKey(route.Id));
                }
            }   
        }

        #endregion

    }
}
