using Azure.Security.KeyVault.Secrets;

namespace TrailsAPI.Helpers
{
    public class AzureHelper
    {
        public static string GetSecretFromKeyVault(SecretClient secretClient, string secretName)
        {
            KeyVaultSecret secret = secretClient.GetSecret(secretName);
            return secret.Value;
        }
    }
}
