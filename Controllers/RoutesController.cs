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
    [Route("/")]
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
      

        [HttpGet]
        public async Task<ActionResult> GetRoutes([FromQuery]int count)
        {
            var sqlQueryText = "SELECT * FROM routes";

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
