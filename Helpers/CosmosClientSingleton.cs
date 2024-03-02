using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Cosmos;
using System.Net.Sockets;

namespace TrailsAPI.Helpers
{
    public class CosmosClientSingleton
    {
        private static CosmosClient? _instance;
        private static readonly object _lock = new object();

        public static CosmosClient GetInstance(IConfiguration configuration)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    string? endpoint = null;
                    string? primaryKey = null;
                    using (var tcpClient = new TcpClient())
                    {
                        SecretClientOptions options = new SecretClientOptions();
                        string keyVaultUrl = configuration["KeyVaultUrl"];
                        var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

                        endpoint = AzureHelper.GetSecretFromKeyVault(secretClient, "cosmos-endpoint");
                        primaryKey = AzureHelper.GetSecretFromKeyVault(secretClient, "cosmos-primarykey");
                        var cosmosClientOptions = new CosmosClientOptions()
                        {
                            SerializerOptions = new CosmosSerializationOptions()
                            {
                                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                            },
                        };

                        _instance = new CosmosClient(endpoint, primaryKey, cosmosClientOptions);
                    }
                }
            }
            return _instance;
        }
    }
}
