using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace DurableFunctions_ChainingPattern
{
    public static class DurableFunctions_ChainningPattern
    {
        [FunctionName("DurableFunctions_ChainningPattern")]
        public static async Task<HttpResponseMessage> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
        [DurableClient] IDurableOrchestrationClient starter, ILogger log)
        {
            string instanceId = await starter.StartNewAsync("Orchestrator", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("Orchestrator")]
        public static async Task<List<string>> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            List<string> outputs = new List<string>();

            outputs.Add(await context.CallActivityAsync<string>("CreateApplicant", new Applicant { Name = "Bruno Willian", Email = "email1@gmail.com" }));
            outputs.Add(await context.CallActivityAsync<string>("CreateApplicant", new Applicant { Name = "Lilian Satiko", Email = "email2@gmail.com" }));
            outputs.Add(await context.CallActivityAsync<string>("CreateApplicant", new Applicant { Name = "Henrique Palomo", Email = "email3@gmail.com" }));

            return outputs;
        }

        [FunctionName("CreateApplicant")]
        public static string CreateApplicant([ActivityTrigger] Applicant applicant, ILogger log)
        {
            log.LogInformation("1. [CreateApplicant] Azure function has been called.");

            string connectionString = GetConnectionString();
            log.LogInformation("2. Connection String: " + connectionString);
            ServiceClient serviceClient = new ServiceClient(connectionString);

            Microsoft.Xrm.Sdk.Entity applicantDataverse = new Microsoft.Xrm.Sdk.Entity("new_applicant");
            applicantDataverse["new_name"] = applicant.Name;
            applicantDataverse["new_email"] = applicant.Email;

            Guid applicantId = serviceClient.Create(applicantDataverse);

            log.LogInformation($"3. A new Applicant has been created with ID: { applicantId}");

            return applicantId.ToString();
        }

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

    public class Applicant
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

}