using System;

namespace DurableFunctions.Common
{
    public class CommonHelper
    {
        public static string GetConnectionString()
        {
            string url = GetEnvironmentVariable("crmproductionurl");
            string clientId = GetEnvironmentVariable("integrationuser_clientid");
            string clientSecret = GetEnvironmentVariable("integrationuser_clientsecret");

            string connectionString = $"AuthType=ClientSecret; url={url};ClientId={clientId};ClientSecret={clientSecret}";
            return connectionString;
        }

        public static string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
