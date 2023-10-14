using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Routes.Models;
using System.Collections.Generic;
using System.Net;
using static System.Net.WebRequestMethods;

namespace Routes.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class RoutesController : ControllerBase
    {
        // The name of the database and container we will create
        string databaseId = "HikingRoutes";
        string containerId = "Routes";

        private async Task AddItemsToContainerAsync()
        {
            // Create a family object for the Andersen family
            HikingRoute newRoute = new HikingRoute()
            {
                Name = "Test",
                Id = "1",
            };

            try
            {
                // Read the item to see if it exists.  
                ItemResponse<HikingRoute> andersenFamilyResponse = await this.ContainerClient().ReadItemAsync<HikingRoute>(newRoute.Id, new PartitionKey(newRoute.Id));
                Console.WriteLine("Item in database with id: {0} already exists\n", andersenFamilyResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
                ItemResponse<HikingRoute> andersenFamilyResponse = await this.ContainerClient().CreateItemAsync<HikingRoute>(newRoute, new PartitionKey(newRoute.Id));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", andersenFamilyResponse.Resource.Id, andersenFamilyResponse.RequestCharge);
            }

            HikingRoute route2 = new HikingRoute()
            {
                Name = "name2",
                Id = "2",
            };

            try
            {
                // Read the item to see if it exists
                ItemResponse<HikingRoute> wakefieldFamilyResponse = await this.ContainerClient().ReadItemAsync<HikingRoute>(route2.Id, new PartitionKey(route2.Id));
                Console.WriteLine("Item in database with id: {0} already exists\n", wakefieldFamilyResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Wakefield family. Note we provide the value of the partition key for this item, which is "Wakefield"
                ItemResponse<HikingRoute> wakefieldFamilyResponse = await this.ContainerClient().CreateItemAsync<HikingRoute>(route2, new PartitionKey(route2.Id));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", wakefieldFamilyResponse.Resource.Id, wakefieldFamilyResponse.RequestCharge);
            }
        }

        string kvUri = Environment.GetEnvironmentVariable("VaultUri");
        private Container ContainerClient()
        {
            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
            string[] cosmosSecrets = client.GetSecret("ConnectionStrings").Value.Value.Split(';');
            Dictionary<string, string> secrets = new Dictionary<string, string>();
            foreach (string s in cosmosSecrets.Where(s => s != ""))
            {
                string key = s.Substring(0, s.IndexOf('='));
                string value = s.Substring(s.IndexOf('=') + 1);
                secrets.Add(key, value);
            }

            var cosmosDbClient = new CosmosClient(secrets["AccountEndpoint"], secrets["AccountKey"], new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });
            Container containerClient = cosmosDbClient.GetContainer(databaseId, containerId);
            return containerClient;
        }

        #region GET
        // Get route by id
        [HttpGet("{id}")]
        public async Task<ActionResult> GetRouteById(string id)
        {
            var sqlQueryText = "SELECT * FROM routes r WHERE r.id = @id";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText).WithParameter("@id", id);
            FeedIterator<HikingRoute> queryResultSetIterator = this.ContainerClient().GetItemQueryIterator<HikingRoute>(queryDefinition);

            List<HikingRoute> routes = new List<HikingRoute>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<HikingRoute> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (HikingRoute route in currentResultSet)
                {
                    routes.Add(route);
                }
            }

            return Ok(routes);
        }   


        [HttpGet]
        public async Task<ActionResult> GetRoutes([FromQuery]int count)
        {
            var totalCount = "SELECT VALUE COUNT(1) FROM c";
            QueryDefinition totalCountQueryDefinition = new QueryDefinition(totalCount);
            var resultSetIterator = this.ContainerClient().GetItemQueryIterator<int>(totalCountQueryDefinition);

            var sqlQueryText = "SELECT * FROM routes";
            if(count != 0)
            {
                sqlQueryText += string.Format("OFFSET 0 LIMIT {0}", count);
            }

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            
            FeedIterator<HikingRoute> queryResultSetIterator = this.ContainerClient().GetItemQueryIterator<HikingRoute>(queryDefinition);

            List<HikingRoute> routes = new List<HikingRoute>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<HikingRoute> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (HikingRoute route in currentResultSet)
                {
                    routes.Add(route);
                }
            }
            
            return Ok(routes);
        }

        #endregion

        #region POST
        // Create a new route
        [HttpPost]
        public async Task<ActionResult> CreateNewRoute([FromBody] HikingRoute newRoute)
        {
            if (newRoute != null)
            {
                ItemResponse<HikingRoute> response = await this.ContainerClient().CreateItemAsync<HikingRoute>(newRoute, new PartitionKey(newRoute.Id));
                return Created("", response.Resource);
            }

            return BadRequest();
        }

        #endregion

        #region PUT

        // Update a route
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateRoute(string id, [FromBody] HikingRoute route)
        {
            ItemResponse<HikingRoute> response = await this.ContainerClient().ReadItemAsync<HikingRoute>(id, new PartitionKey(id));
            if (response != null)
            {
                response.Resource.Name = route.Name;
                response.Resource.Difficulty = route.Difficulty;
                response.Resource.EstimatedDuration = route.EstimatedDuration;
                response.Resource.DistanceInKilometers = route.DistanceInKilometers;
                response.Resource.StartingPoint = route.StartingPoint;
                response.Resource.EndingPoint = route.EndingPoint;
                response.Resource.Description = route.Description;
                response.Resource.ImageUrl = route.ImageUrl;
                response.Resource.GpxFileUrl = route.GpxFileUrl;
                response.Resource.Tags = route.Tags;


                response = await this.ContainerClient().ReplaceItemAsync<HikingRoute>(response.Resource, response.Resource.Id, new PartitionKey(response.Resource.Id));
                return Ok(response.Resource);
            }

            return BadRequest();
        }

        #endregion

        #region Delete
        // Delete a route 
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRoute(string id)
        {
            ItemResponse<HikingRoute> response = await this.ContainerClient().DeleteItemAsync<HikingRoute>(id, new PartitionKey(id));
            return NoContent();
        }

        // Delete all routes
        [HttpPost]
        [Route("delete")]
        public async Task<ActionResult> DeleteRoutes()
        {
            var sqlQueryText = "SELECT * FROM routes";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<HikingRoute> queryResultSetIterator = this.ContainerClient().GetItemQueryIterator<HikingRoute>(queryDefinition);

            List<HikingRoute> routes = new List<HikingRoute>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<HikingRoute> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (HikingRoute route in currentResultSet)
                {
                    routes.Add(route);
                }
            }

            foreach (HikingRoute route in routes)
            {
                await this.ContainerClient().DeleteItemAsync<HikingRoute>(route.Id, new PartitionKey(route.Id));
            }

            return NoContent();
        }

        #endregion  
    }
}
