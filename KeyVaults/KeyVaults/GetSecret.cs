using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using System;

namespace KeyVaults
{
    #region Step by Step
    //1. Create App Registration.Add secret to it. Copy [Secret] and [Application (client) id]
    //2. Create Vault. Add secrets to it and then go to Access Polices
    //3. Add Access Policy
    //4. Add Secret Permisions (Get, Update, Create, etc)
    //4. Select Principal > Select the App Registration created on step 1
    #endregion

    class GetSecret
    {
        static void Main(string[] args)
        {
            //App Registration > Application (client) id
            string clientId = "c094c2c7-78d7-410a-b61f-f0037c5ffb01";

            //Key Vault > Overview > Vault URI
            string vaultUri = "https://integrationssecrets.vault.azure.net/";

            //Secret generated for App Registration
            string clientSecret = "d~H4puef_lLjx-6_toESqpPc0Havsq03r-";

            //Home > Tenant Properties
            string tenantId = "f9afad9b-70b2-4ce5-8aaa-d53af349c6d3";

            //Secret Name
            string secretName = "CRMProductionURL";

            var azureServiceTokenProvider = new AzureServiceTokenProvider($"RunAs=App;AppId={clientId};TenantId={tenantId};AppKey={clientSecret}");
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            SecretBundle secret = keyVaultClient.GetSecretAsync(vaultUri, secretName).GetAwaiter().GetResult();
            Console.WriteLine(secret.Value);
        }
    }
}
