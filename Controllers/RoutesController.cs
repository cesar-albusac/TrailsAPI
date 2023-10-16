using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Recipes.Data;
using Routes.Models;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Xml;

namespace Routes.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class RoutesController : ControllerBase
    {
        private readonly IRouteRepository _routeRepository;
        public RoutesController(IRouteRepository routeRepository)
        {
            this._routeRepository = routeRepository;
        
        }

        //Get .gpx files from folder
        private void GetGPXfiles(string folderPath)
        {
            string[] filePaths = Directory.GetFiles(folderPath, "*.gpx");
            foreach (string filePath in filePaths)
            {
                ExtractDataFromGPX(filePath);
            }
        }

        // Extract data from gpx file like total distance, elevation, etc.
        private void ExtractDataFromGPX(string filePath)
        {
            // Create a new XmlDocument  
            XmlDocument doc = new XmlDocument();

            // Load data  
            doc.Load(filePath);

            // Create a namespace manager  
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);

            // Add the namespaces used in books.xml to the XmlNamespaceManager.  
            nsmgr.AddNamespace("x", "http://www.topografix.com/GPX/1/1");
            nsmgr.AddNamespace("gpxtpx", "http://www.garmin.com/xmlschemas/TrackPointExtension/v1");

            // Get the first book written by an author whose last name is Atwood.  
            XmlNodeList nodes = doc.SelectNodes("//x:gpx/x:trk/x:trkseg/x:trkpt", nsmgr);

            // Display all the book titles.  
            foreach (XmlNode node in nodes)
            {
                Console.WriteLine(node.Attributes["lat"].Value);
                Console.WriteLine(node.Attributes["lon"].Value);
                Console.WriteLine(node.SelectSingleNode("x:ele", nsmgr).InnerText);
                Console.WriteLine(node.SelectSingleNode("x:time", nsmgr).InnerText);
                Console.WriteLine(node.SelectSingleNode("gpxtpx:hr", nsmgr).InnerText);
            }
        }   

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
        private Container ContainerProductionClientKeyVault()
        {
            //get appsettings property values
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string databaseId = config["CosmosDBSettings:DatabaseName"];
            string containerId = config["CosmosDBSettings:RoutesCollection"];

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

        private Container ContainerClient()
        {
            GetGPXfiles(Directory.GetCurrentDirectory() + "\\RutasToInsert");
            //get appsettings property values
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string databaseId = config["CosmosDBSettings:DatabaseName"];
            string containerId = config["CosmosDBSettings:RoutesCollection"];
            string accountEndpoint = config["CosmosDBSettings:EndpointUri"];
            string accountKey = config["CosmosDBSettings:PrimaryKey"];

            var cosmosDbClient = new CosmosClient(accountEndpoint, accountKey, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });
            Container containerClient = cosmosDbClient.GetContainer(databaseId, containerId);
            return containerClient;
        }

        #region GET
        // Get all routes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HikingRoute>>> GetAllRoutes(/*[FromQuery] int count*/)
        {
            var routes = await _routeRepository.GetAllRoutesAsync();
            return Ok(routes);
        }

        // Get route by id
        [HttpGet("{id}")]
        public async Task<ActionResult> GetRouteById(int id)
        {
            if (id < 1)
                return BadRequest();

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

            if(routes.Count == 0)
            {
                return NotFound();
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
        // update all routes
        [HttpPut]
        public async Task<ActionResult> UpdateAllRoutes([FromBody] List<HikingRoute> routes)
        {
            if (routes == null)
            {
                return BadRequest();
            }

            foreach (HikingRoute route in routes)
            {
                ItemResponse<HikingRoute> response = await this.ContainerClient().ReadItemAsync<HikingRoute>(route.Id, new PartitionKey(route.Id));
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
                }
                else
                {
                    NotFound();
                }
            }

            return Ok();
        }

        // Update a route
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateRoute(string id, [FromBody] HikingRoute route)
        {
            if(route == null || route.Id == null)
            {
                return BadRequest();
            }

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
            else
            {
                NotFound();
            }

            return BadRequest();
        }

        #endregion

        #region Delete
        // Delete a route 
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRoute(int id)
        {
            if (id <= 0 )
            {
                return BadRequest();
            }

            ItemResponse<HikingRoute> response = await this.ContainerClient().DeleteItemAsync<HikingRoute>(id.ToString(), new PartitionKey(id));
            return NoContent();
        }

        // Delete all routes
        [HttpDelete]
        public async Task<ActionResult> DeleteAllRoutes()
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

            foreach(HikingRoute route in routes)
            {
                await this.ContainerClient().DeleteItemAsync<HikingRoute>(route.Id, new PartitionKey(route.Id));
            }

            return NoContent();
        }   

        #endregion  
    }
}
