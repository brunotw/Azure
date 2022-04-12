using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CreateApplicant
{
    public static class CreateApplicant
    {
        [FunctionName("CreateApplicant")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("1. [CreateApplicant] Azure function has been called.");

            string connectionString = GetConnectionString();
            log.LogInformation("2. Connection String: " + connectionString);
            ServiceClient serviceClient = new ServiceClient(connectionString);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Applicant applicant = JsonConvert.DeserializeObject<Applicant>(requestBody);

            for (int i = 0; i < 100; i++)
            {
                Entity applicantDataverse = new Entity("new_applicant");
                applicantDataverse["new_name"] = $"Test {i}";
                Guid applicantId = serviceClient.Create(applicantDataverse);
                log.LogInformation($"Applicant {i} of 100 with id {applicantId} created");
            }

            return new OkObjectResult("ok");

            //Entity applicantDataverse = new Entity("new_applicant");
            //applicantDataverse["new_name"] = applicant.name;
            //applicantDataverse["new_email"] = applicant.email;
            //Guid applicantId = serviceClient.Create(applicantDataverse);

            //string response = $"3. A new Applicant has been created with ID: { applicantId}";

            //log.LogInformation(response);
            //return new OkObjectResult(applicantId);
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
}

public class Applicant
{
    public string name { get; set; }
    public string email { get; set; }
}
