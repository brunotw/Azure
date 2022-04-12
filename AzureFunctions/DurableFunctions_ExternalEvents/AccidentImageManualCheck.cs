using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DurableFunctions_ExternalEvents.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DurableFunctions_ExternalEvents
{
    public static class AccidentImageManualCheck
    {
        [FunctionName(FunctionNames.AccidentImageManualCheckTrigger)]
        public static async Task<HttpResponseMessage> HttpStart([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
                                                                [DurableClient] IDurableOrchestrationClient starter, ILogger log)
        {

            log.LogWarning($"TRACK 1");
            string imageBase64 = req.Content.ReadAsStringAsync().Result;
            string instanceId = await starter.StartNewAsync(FunctionNames.AccidentImageManualCheckOrchestrator, null, imageBase64);
            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName(FunctionNames.RaiseApprovalEvent)]
        public static async Task<IActionResult> RaiseApprovalEvent([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
                                                              [DurableClient] IDurableOrchestrationClient client, ILogger log)
        {
            log.LogWarning($"Approval event has been called");

            var response = req.Content.ReadAsAsync<RaiseEventResponse>();

            string instanceId = response.Result.instanceId;
            string approved = response.Result.approved;
            await client.RaiseEventAsync(instanceId, EventNames.Approval, approved);

            return new AcceptedResult();
        }


        [FunctionName(FunctionNames.AccidentImageManualCheckOrchestrator)]
        public static async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            log.LogWarning($"TRACK 2");
            string base64string = context.GetInput<string>();
            log.LogWarning($"TRACK 2.1");
            RequestApproval requestApproval = new RequestApproval
            {
                AccidentImageBase64 = base64string,
                InstanceId = context.InstanceId
            };
            log.LogWarning($"TRACK 2.2");
            await context.CallActivityAsync<string>(FunctionNames.RequestApprovalByEmail, requestApproval);
            log.LogWarning($"TRACK 2.3");

            using (var timeoutCts = new CancellationTokenSource())
            {
                log.LogWarning($"TRACK 2.4");
                DateTime dueTime = context.CurrentUtcDateTime.AddMinutes(1);
                log.LogError(dueTime.ToString());
                log.LogWarning($"TRACK 2.5");
                Task durableTimeout = context.CreateTimer(dueTime, timeoutCts.Token);
                log.LogWarning($"TRACK 2.6");
                Task<string> approvalEvent = context.WaitForExternalEvent<string>(EventNames.Approval);
                log.LogWarning($"TRACK 2.7");

                if (approvalEvent == await Task.WhenAny(approvalEvent, durableTimeout))
                {
                    log.LogWarning($"Approved");
                    timeoutCts.Cancel();
                    //await context.CallActivityAsync("ProcessApproval", approvalEvent.Result);
                }
                else
                {
                    log.LogWarning($"Rejected");
                    //await context.CallActivityAsync("Escalate", null);
                }
            }

            log.LogWarning($"TRACK 2.8");
        }

        [FunctionName(FunctionNames.RequestApprovalByEmail)]
        public static void RequestApprovalByEmail([ActivityTrigger] RequestApproval requestApproval, ILogger log)
        {
            log.LogWarning($"TRACK 3");

            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Endpoints.SendApprovalEmail);
            request.Content = new StringContent(JsonSerializer.Serialize(requestApproval));
            request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.SendAsync(request);
        }
    }
}