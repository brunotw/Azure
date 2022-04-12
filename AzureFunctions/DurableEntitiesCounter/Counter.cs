using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading.Tasks;

namespace DurableEntitiesCounter
{
    public class Counter : ICounter
    {
        public int Value { get; set; }

        public Task<int> Get()
        {
            return Task.FromResult(++Value);
        }

        [FunctionName(nameof(Counter))]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
        {
            return context.DispatchAsync<Counter>();
        }
    }
}