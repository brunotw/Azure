using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DurableFunctions_Examples.Common
{
    public class Helper
    {
        public static async Task<string> ExtractNameParameter(HttpRequestMessage req)
        {
            ////Get parameter from query string (GET)
            //string name = req .Query["name"];

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            //return name;

            return string.Empty;
        }

        public static string BuildTriggerResponse(string name)
        {
            return string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";
        }

        internal static string GetConnectionString()
        {
            throw new NotImplementedException();
        }
    }
}
