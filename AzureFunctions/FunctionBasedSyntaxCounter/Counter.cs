using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FunctionBasedSyntaxCounter
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Counter
    {
        [JsonProperty("value")]
        private int Value { get; set; }

        public void Add(int amount)
        {
            this.Value += amount;
        }

        public Task Reset()
        {
            this.Value = 0;
            return Task.CompletedTask;
        }

        public Task<int> Get()
        {
            return Task.FromResult(this.Value);
        }

        public void Delete()
        {
            Entity.Current.DeleteState();
        }

        [FunctionName(nameof(Counter))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx) => ctx.DispatchAsync<Counter>();

        [FunctionName("DeleteCounter")]
        public static async Task<HttpResponseMessage> DeleteCounter(
                           [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "Counter/{entityKey}")] HttpRequestMessage req,
                           [DurableClient] IDurableEntityClient client, string entityKey)
        {
            var entityId = new EntityId("Counter", entityKey);
            await client.SignalEntityAsync(entityId, "Delete");
            return req.CreateResponse(HttpStatusCode.Accepted);
        }
    }
}

