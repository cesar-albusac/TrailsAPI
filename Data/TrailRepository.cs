using Microsoft.Azure.Cosmos;
using Trails.Models;

namespace Trails.Data
{
    public class TrailRepository : ITrailRepository
    {
        private readonly CosmosClient cosmosClient;
        private readonly IConfiguration configuration;
        private readonly Container container;

        public TrailRepository(CosmosClient cosmosClient, IConfiguration configuration)
        {
            this.cosmosClient = cosmosClient;
            this.configuration = configuration;
            var databaseName = configuration["CosmosDBSettings:DatabaseName"];
            var containerName = configuration["CosmosDBSettings:ContainerName"];
            this.container = this.cosmosClient.GetContainer(databaseName, containerName);
        }

        #region GET Methods

        public async Task<IEnumerable<Trail>> GetAllTrailsAsync()
        {
            var sqlQueryText = "SELECT * FROM Trails";
 
            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            FeedIterator<Trail> queryResultSetIterator = this.container.GetItemQueryIterator<Trail>(queryDefinition);

            List<Trail> Trails = new List<Trail>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Trail> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Trail Trail in currentResultSet)
                {
                    Trails.Add(Trail);
                }
            }

            return Trails;
        }

        public async Task<Trail> GetTrailAsync(string id)
        {
            try
            {
                ItemResponse<Trail> response = await this.container.ReadItemAsync<Trail>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<IEnumerable<Trail>> GetTrailsByDifficultyAsync(string difficulty)
        {
            var sqlQueryText = "SELECT * FROM Trails r WHERE r.difficulty = @difficulty";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText).WithParameter("@difficulty", difficulty);

            FeedIterator<Trail> queryResultSetIterator = this.container.GetItemQueryIterator<Trail>(queryDefinition);

            List<Trail> Trails = new List<Trail>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Trail> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Trail Trail in currentResultSet)
                {
                    Trails.Add(Trail);
                }
            }

            return Trails;
        }

        #endregion

        #region POST Methods
        public async Task<Trail> AddTrailAsync(Trail Trail)
        {
            ItemResponse<Trail> response = await this.container.CreateItemAsync<Trail>(Trail, new PartitionKey(Trail.Id));
            return response.Resource;
        }

        #endregion

        #region PUT Methods

        public async Task<Trail> UpdateTrailAsync(Trail Trail)
        {
            ItemResponse<Trail> response = await this.container.UpsertItemAsync<Trail>(Trail, new PartitionKey(Trail.Id));
            return response.Resource;
        }

        #endregion

        #region DELETE Methods

        public async Task DeleteTrailAsync(string id)
        {
            await this.container.DeleteItemAsync<Trail>(id, new PartitionKey(id));
        }
                          
        public async Task DeleteAllTrailsAsync()
        {
            var sqlQueryText = "SELECT * FROM Trails";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            FeedIterator<Trail> queryResultSetIterator = this.container.GetItemQueryIterator<Trail>(queryDefinition);

            List<Trail> Trails = new List<Trail>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Trail> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Trail Trail in currentResultSet)
                {
                    await this.container.DeleteItemAsync<Trail>(Trail.Id, new PartitionKey(Trail.Id));
                }
            }   
        }

        #endregion

    }
}
