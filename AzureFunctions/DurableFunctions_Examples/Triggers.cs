using DurableFunctions_Examples.Common;
using DurableFunctions_Examples.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace DurableFunctions_Examples
{
    public class Triggers
    {
        [FunctionName(TNames.HttpStart)]
        public static async Task<HttpResponseMessage> HttpStart([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
                                                            [DurableClient] IDurableOrchestrationClient starter, ILogger log)
        {

            string instanceId = await starter.StartNewAsync (ONames.OrchestratorFunctionChaining, null);
            log.LogWarning($"Started orchestration with ID = '{instanceId}'.");
            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName(TNames.OrchestratorUsingRouter)]
        public static async Task<HttpResponseMessage> OrchestratorUsingRouter([HttpTrigger(AuthorizationLevel.Function, methods: "get", Route = "orchestrators/{orchestratorName}")] HttpRequestMessage req,
                                                          [DurableClient] IDurableClient starter, string orchestratorName, ILogger log)
        {
            log.LogInformation($"Trying to call an Orchestrator called {orchestratorName}");
            string instanceId = await starter.StartNewAsync(orchestratorName, null);
            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName(TNames.GetRequestContent)]
        public static async Task<HttpResponseMessage> GetRequestContent([HttpTrigger(AuthorizationLevel.Function, methods: "post")] HttpRequestMessage req,
                                                         [DurableClient] IDurableClient starter, ILogger log)
        {
            log.LogInformation("Client [GetRequestContent] started.");
            Client client = await req.Content.ReadAsAsync<Client>();
            log.LogInformation($"Client name is {client.Name}");
            return starter.CreateCheckStatusResponse(req, client.Name);
        }

        [FunctionName(TNames.GetQueryParameters)]
        public static HttpResponseMessage GetQueryParameters([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestMessage req,
                                                         [DurableClient] IDurableOrchestrationClient starter, ILogger log)
        {
            var queryDictionary = HttpUtility.ParseQueryString(req.RequestUri.Query);
            var name = queryDictionary["name"];

            if (string.IsNullOrEmpty(name) == false)
            {
                log.LogWarning($"Hello {name }");
            }
            else
            {
                log.LogWarning($"Expecting a query string parameter called [name].");
            }

            return starter.CreateCheckStatusResponse(req, name ?? "not provided");
        }
    }
}
