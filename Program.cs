using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Trails.Data;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSingleton((provider) =>
{
    var endpoint = configuration["CosmosDBSettings:EndpointUri"];
    var primaryKey = configuration["CosmosDBSettings:PrimaryKey"];
    var databaseName = configuration["CosmosDBSettings:DatabaseName"];

    var containerName = configuration["CosmosDBSettings:ContainerName"];
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
