using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Routes.Models;
using System.Net;
using static System.Net.WebRequestMethods;

namespace Routes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoutesController : ControllerBase
    {
        private CosmosClient cosmosClient;

        // The database we will create
        private Database database;

        // The container we will create.
        private Container container;

        // The name of the database and container we will create
        private string databaseId = "HikingRoutes";
        private string containerId = "Routes";

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
                ItemResponse<HikingRoute> andersenFamilyResponse = await this.container.ReadItemAsync<HikingRoute>(newRoute.Id, new PartitionKey(newRoute.Id));
                Console.WriteLine("Item in database with id: {0} already exists\n", andersenFamilyResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Andersen family. Note we provide the value of the partition key for this item, which is "Andersen"
                ItemResponse<HikingRoute> andersenFamilyResponse = await this.container.CreateItemAsync<HikingRoute>(newRoute, new PartitionKey(newRoute.Id));

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
                ItemResponse<HikingRoute> wakefieldFamilyResponse = await this.container.ReadItemAsync<HikingRoute>(route2.Id, new PartitionKey(route2.Id));
                Console.WriteLine("Item in database with id: {0} already exists\n", wakefieldFamilyResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in the container representing the Wakefield family. Note we provide the value of the partition key for this item, which is "Wakefield"
                ItemResponse<HikingRoute> wakefieldFamilyResponse = await this.container.CreateItemAsync<HikingRoute>(route2, new PartitionKey(route2.Id));

                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", wakefieldFamilyResponse.Resource.Id, wakefieldFamilyResponse.RequestCharge);
            }
        }

        private async Task CreateContainerAsync()
        {
            // Create a new container
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/id");
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
        }

        private async Task CreateDatabaseAsync()
        {
            // Create a new database
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine("Created Database: {0}\n", this.database.Id);
        }

        [HttpGet]
        public async Task<ActionResult> GetRoutes([FromQuery]int count)
        {
            string kvUri = Environment.GetEnvironmentVariable("VaultUri");
            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
            string[] cosmosSecrets = client.GetSecret("ConnectionStrings").Value.Value.Split(';');
            Dictionary<string, string> secrets = new Dictionary<string, string>();
            foreach (string s in cosmosSecrets.Where(s => s != ""))
            {
                string key = s.Substring(0, s.IndexOf('='));
                string value = s.Substring(s.IndexOf('=')+1);
                secrets.Add(key, value);
            }

            this.cosmosClient = new CosmosClient(secrets["AccountEndpoint"], secrets["AccountKey"], new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });
             // The database we will create
  
            await this.CreateDatabaseAsync();
            await this.CreateContainerAsync();
            await this. AddItemsToContainerAsync();
            var sqlQueryText = "SELECT * FROM c WHERE c.PartitionKey = 'Andersen'";

            Console.WriteLine("Running query: {0}\n", sqlQueryText);

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<HikingRoute> queryResultSetIterator = this.container.GetItemQueryIterator<HikingRoute>(queryDefinition);

            List<HikingRoute> families = new List<HikingRoute>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<HikingRoute> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (HikingRoute family in currentResultSet)
                {
                    families.Add(family);
                    Console.WriteLine("\tRead {0}\n", family);
                }
            }
            
            return Ok();
        }

        [HttpPut]
        public ActionResult UpdateRoute(string nombre)
        {
            if(nombre != null)
            {
                string[] Routes = { "Pollo", "Paella" };
                if (Routes.Contains(nombre))
                    return Ok(nombre);
            }

            return BadRequest();


        }

        [HttpPost]
        public ActionResult CreateNewRoute([FromBody] Models.HikingRoute newRoute)
        {
            if (newRoute != null)
                return Created("", newRoute);

            return BadRequest();

        }

        [HttpDelete]
        public ActionResult DeleteRoutes()
        {
            bool badThingHappened = false;

            if (badThingHappened)
                return BadRequest();

            return NoContent();
        }
    }
}
