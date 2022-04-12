using DurableFunctions.Common;
using DurableFunctions_Examples.Common;
using DurableFunctions_Examples.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using System;
using System.Threading.Tasks;

namespace DurableFunctions_Examples
{
    public class Activities
    {
        private static ServiceClient sClient = new ServiceClient(CommonHelper.GetConnectionString());

        [FunctionName(ANames.SayHello)]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName(ANames.CreateLead)]
        public static ActivityResponse CreateLead([ActivityTrigger] Lead lead, ILogger log)
        {
            try
            {
                log.LogWarning($"Creating lead {lead.FirstName} {lead.LastName}");

                //string connectionString = CommonHelper.GetConnectionString();
                //ServiceClient serviceClient = new ServiceClient(connectionString);

                Microsoft.Xrm.Sdk.Entity leadDataverse = new Microsoft.Xrm.Sdk.Entity("lead");
                leadDataverse["subject"] = lead.Topic;
                leadDataverse["firstname"] = lead.FirstName;
                leadDataverse["lastname"] = lead.LastName;

                if(sClient is Exception)
                {
                    sClient= new ServiceClient(CommonHelper.GetConnectionString());
                }

                Guid leadId = sClient.Create(leadDataverse);

                ActivityResponse response = new ActivityResponse
                {
                    Result = leadId.ToString(),
                    Success = true
                };

                log.LogWarning($"Lead  {lead.FirstName} {lead.LastName} has been created.");

                return response;
            }
            catch (Exception e)
            {
                throw e;
            }
          
        }


        [FunctionName(ANames.TupleIsEvenExample)]
        public static (bool IsEven, int Number) TupleIsEvenExample([ActivityTrigger] IDurableActivityContext context, ILogger log)
        {
            var (number, instanceId) = context.GetInput<(int number, string instanceId)>();
            log.LogWarning($"Checking if {number} is even. Instance Id: {instanceId}");
            return (number % 2 == 0, number);
        }
    }
}
