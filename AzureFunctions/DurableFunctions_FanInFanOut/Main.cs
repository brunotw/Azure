using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Collections.Generic;
using Microsoft.PowerPlatform.Dataverse.Client;
using DurableFunctions_FanOutFanIn.Model;
using DurableFunctions_FanOutFanIn.Helpers;

namespace DurableFunctions_FanInFanOut
{
    public static class Main
    {
        [FunctionName(FNames.Trigger)]
        public static async Task<HttpResponseMessage> HttpStart([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
                                                              [DurableClient] IDurableOrchestrationClient starter, ILogger log)
        {

            log.LogInformation("Trigger started");
            string instanceId = await starter.StartNewAsync("Orchestrator");
            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName(FNames.Orchestrator)]
        public static async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            try
            {
                log.LogWarning("Orchestrator started");

                var applicant = new Lead { Topic = Helper.GetInvalidTopic(), FirstName = "Invalid", LastName = "Topic" };
                var retryOptions = new RetryOptions(TimeSpan.FromSeconds(5), 2) { Handle = HandleError };
                var result = await context.CallActivityAsync<ActivityResponse>(FNames.CreateLead, applicant);

                log.LogInformation("First Assync task completed. Starting Fan Out Fan In tasks.");

                List<Task<ActivityResponse>> tasks = new List<Task<ActivityResponse>>
                {
                    context.CallActivityAsync<ActivityResponse>(FNames.CreateLead, new Lead { Topic = "Topic", FirstName = "bruno", LastName = "Willian" }),
                    context.CallActivityAsync<ActivityResponse>(FNames.CreateLead, new Lead { Topic = "Topic", FirstName = "Lilian", LastName = "Satiko" }),
                    context.CallActivityWithRetryAsync<ActivityResponse>(FNames.CreateLead, retryOptions, applicant)
                };

                await Task.WhenAll(tasks);

                log.LogInformation("All tasks completed");

                foreach (var task in tasks)
                {
                    var r = task.Result;
                    log.LogWarning("Success: " + r.Success);
                }

                log.LogWarning("Orchestrator completed");
            }
            catch (Exception ex)
            {
                log.LogError("Orchestrator error: " + ex.Message);
            }
        }

        [FunctionName(FNames.CreateLead)]
        public static ActivityResponse CreateLead([ActivityTrigger] Lead lead, ILogger log)
        {
            try
            {
                log.LogWarning($"Creating applicant {lead.FirstName} {lead.LastName}");

                string connectionString = Helper.GetConnectionString();
                ServiceClient serviceClient = new ServiceClient(connectionString);

                Microsoft.Xrm.Sdk.Entity leadDataverse = new Microsoft.Xrm.Sdk.Entity("lead");
                leadDataverse["subject"] = lead.Topic;
                leadDataverse["firstname"] = lead.FirstName;
                leadDataverse["lastname"] = lead.LastName;

                Guid leadId = serviceClient.Create(leadDataverse);

                ActivityResponse response = new ActivityResponse
                {
                    Result = leadId.ToString(),
                    Success = true
                };

                log.LogWarning($"Applicant  {lead.FirstName} {lead.LastName} has been created.");

                return response;
            }
            catch (Exception ex)
            {
                log.LogWarning($"Failed while trying to create lead  [{lead.FirstName} {lead.LastName}]. Error Details: {ex.Message}");

                throw new CustomException
                {
                    ErrorMessage = ex.Message,
                    ShouldRetry = true,
                    Lead = lead
                };
            }
        }

        public static bool HandleError(Exception ex)
        {
            if (ex.InnerException is CustomException)
            {
                CustomException customException = ex.InnerException as CustomException;
                return customException.ShouldRetry;
            }

            return false;
        }

    }
}
