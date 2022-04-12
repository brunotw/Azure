using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace DurableEntitiesCounter
{
    public class CounterHandler
    {
        [FunctionName("GetCounter")]
        public static async Task<HttpResponseMessage> GetCounter([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Counter/{counterKey}")] HttpRequestMessage req,
                                                                 [DurableClient] IDurableOrchestrationClient client, string counterKey, ILogger log)
        {

            var instanceId = await client.StartNewAsync("GetCounter_Orchestrator", null, counterKey);
            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            //https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-instance-management?tabs=csharp
            //Wait for orchestration completion
            return await client.WaitForCompletionOrCreateCheckStatusResponseAsync(req, instanceId, System.TimeSpan.MaxValue);

        }

        [FunctionName("GetCounter_Orchestrator")]
        public static async Task<int> DatabaseGetOrchestratorAsync([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var counterName = context.GetInput<string>();
            var entityId = new EntityId(nameof(Counter), counterName);

            //Implementing proxy to use interface            
            var proxy = context.CreateEntityProxy<ICounter>(entityId);
            int currentValue = await proxy.Get();

            //If not using interface below should be applied.
            //int currentValue = await context.CallEntityAsync<int>(entityId, "Get");

            return currentValue;
        }
    }
}
