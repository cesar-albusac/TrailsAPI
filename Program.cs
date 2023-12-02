using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Cosmos;
using System.Net.Sockets;
using Trails.Data;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
    policy =>
    {
        policy.WithOrigins("https://trailsstorageaccount.blob.core.windows.net", "https://localhost:7145");
    });
});

builder.Services.AddSingleton((provider) =>
{
    string? endpoint = null;
    string primaryKey = null;
    using (var tcpClient = new TcpClient())
    {
        try // If Cosmos DB Emulator is running use the local container
        {
            tcpClient.Connect("localhost", 8081);
            var containerName = configuration["CosmosDBSettings:ContainerName"];
            endpoint = configuration["CosmosDBSettings:EndpointUri"];
            primaryKey = configuration["CosmosDBSettings:PrimaryKey"];
            var databaseName = configuration["CosmosDBSettings:DatabaseName"];
        }
        catch (Exception)
        {
            SecretClientOptions options = new SecretClientOptions();

            SecretClient client = new SecretClient(new Uri("https://hikingtrailskeyvault.vault.azure.net/"), new DefaultAzureCredential(), options);
            KeyVaultSecret endpointSecret = client.GetSecret("cosmos-endpoint");
            KeyVaultSecret primarykeySecret = client.GetSecret("cosmos-primarykey");

            endpoint = endpointSecret.Value;
            primaryKey = primarykeySecret.Value;
        }
    }


    var cosmosClientOptions = new CosmosClientOptions()
    {
        SerializerOptions = new CosmosSerializationOptions()
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    };

    var loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
    });

    var cosmosClient = new CosmosClient(endpoint, primaryKey, cosmosClientOptions);
    return cosmosClient;
});

// Add the Log service to the builder

builder.Services.AddSingleton<ITrailRepository, TrailRepository>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

    app.UseSwaggerUI();
}
app.UseCors();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
