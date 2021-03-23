using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;

namespace CreateApplicant
{
    public static class CreateApplicant
    {
        [FunctionName("CreateApplicant")]
        public static void Run([ServiceBusTrigger("myqueue", Connection = "")]string myQueueItem, ILogger log)
        {
            string url = Environment.GetEnvironmentVariable("DataverseUri");
            string clientId = Environment.GetEnvironmentVariable("ClientId");
            string clientSecret = Environment.GetEnvironmentVariable("ClientSecret");

            string connection = $"Url={url};ClientId={clientId};Secret={clientSecret};AuthType=ClientSecret";

            log.LogInformation($"Url: {url}");
            log.LogInformation($"ClientId: {clientId}");
            log.LogInformation($"ClientSecret: {clientSecret}");
            log.LogInformation($"Connection: {connection}");
        }
    }
}
