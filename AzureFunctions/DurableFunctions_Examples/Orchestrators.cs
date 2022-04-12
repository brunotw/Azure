using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DurableFunctions_Examples.Common;
using DurableFunctions_Examples.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableFunctions_Examples
{
    public static class Orchestrators
    {
        [FunctionName(ONames.OrchestratorFunctionChaining)]
        public static async Task<List<string>> OrchestratorFunctionChaining([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            log.LogWarning($"{ONames.OrchestratorFunctionChaining} started.");
            
            var outputs = new List<string>();

            outputs.Add(await context.CallActivityAsync<string>(ANames.SayHello, "Bruno"));
            outputs.Add(await context.CallActivityAsync<string>(ANames.SayHello, "David"));
            outputs.Add(await context.CallActivityAsync<string>(ANames.SayHello, "Jonathan"));

            log.LogWarning($"{ONames.OrchestratorFunctionChaining} finalized.");

            return outputs;
        }

        [FunctionName(ONames.OrchestratorFanInFanOut)]
        public static async Task<List<ActivityResponse>> OrchestratorFanInFanOut([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            log.LogWarning("OrchestratorFanInFanOut started");

            List<Task<ActivityResponse>> tasks = new List<Task<ActivityResponse>>
                {
                    context.CallActivityAsync<ActivityResponse>(ANames.CreateLead, new Lead { Topic = "Topic 1", FirstName = "Bruno", LastName = "Willian" }),
                    context.CallActivityAsync<ActivityResponse>(ANames.CreateLead, new Lead { Topic = "Topic 2", FirstName = "Lilian", LastName = "Satiko" }),
                    context.CallActivityAsync<ActivityResponse>(ANames.CreateLead, new Lead { Topic = "Topic 3", FirstName = "Henrique", LastName = "Souza" })
                };

            await Task.WhenAll(tasks);

            log.LogWarning("All tasks completed");

            foreach (var task in tasks)
            {
                var r = task.Result;
                log.LogWarning("Success: " + r.Success);
            }

            log.LogWarning("OrchestratorFanInFanOut completed");

            List<ActivityResponse> result = tasks.Select(t => t.Result).ToList();

            return result;
        }


        [FunctionName(ONames.OrchestratorUsingTuple)]
        public static async Task<(bool, int)> OrchestratorUsingTuple([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var number = 2;
            var result = await context.CallActivityAsync<(bool isEven, int number)>(ANames.TupleIsEvenExample, (number, context.InstanceId));

            log.LogWarning($"Number {(result.isEven ? "is" : "is not")} even");

            return result;
        }

        [FunctionName("PerformanceTest")]
        public static async Task<TimeSpan> PerformanceTest([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var startDate = context.CurrentUtcDateTime;
            log.LogWarning("PerformanceTest started");

            List<Task<ActivityResponse>> tasks = new List<Task<ActivityResponse>>();

            for (int i = 0; i < 10; i++)
            {
                tasks.Add(context.CallActivityAsync<ActivityResponse>(ANames.CreateLead, new Lead { Topic = "Topic " + i, FirstName = "AA", LastName = "Willian" }));
            }

            await Task.WhenAll(tasks);
            var endDate = context.CurrentUtcDateTime;

            var resultDate = endDate - startDate;
            log.LogWarning($"All tasks completed in {resultDate}");
            log.LogWarning("PerformanceTest completed");

            List<ActivityResponse> result = tasks.Select(t => t.Result).ToList();

            return resultDate;
        }

    }
}
