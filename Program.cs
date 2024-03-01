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
    string? primaryKey = null;
    using (var tcpClient = new TcpClient())
    {
        SecretClientOptions options = new SecretClientOptions();
        string keyVaultUrl = configuration["KeyVaultUrl"];
        var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

        endpoint = GetSecretFromKeyVault(secretClient, "cosmos-endpoint");
        primaryKey = GetSecretFromKeyVault(secretClient, "cosmos-primarykey");
    }


    var cosmosClientOptions = new CosmosClientOptions()
    {
        SerializerOptions = new CosmosSerializationOptions()
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        },
    };

    var loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole();
    });

    var cosmosClient = new CosmosClient(endpoint, primaryKey, cosmosClientOptions);
    return cosmosClient;
});

string? GetSecretFromKeyVault(SecretClient secretClient, string secretName)
{
    try
    {
        KeyVaultSecret secret = secretClient.GetSecret(secretName);
        return secret.Value;
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        // Handle the exception
        return string.Empty;
    }
}

// Add the Log service to the builder

builder.Services.AddScoped<ITrailRepository, TrailRepository>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
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
