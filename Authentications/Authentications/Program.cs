using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Authentications.Models;
using Newtonsoft.Json;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using System.Configuration;

namespace Authentications
{
    class Program
    {
        static void Main(string[] args)
        {
            //var execute = Task.Run(async () => await AuthenticationUsingWebAPI());
            //Task.WaitAll(execute);

            MicrosoftXrmToolingConnector();
        }


        public static void MicrosoftXrmToolingConnector()
        {
            //Reference. https://docs.microsoft.com/en-us/powerapps/developer/data-platform/xrm-tooling/use-connection-strings-xrm-tooling-connect
            //1. Supports OAuth
            //3. AuthType=Office365 doesn't support named account (login + password) if Mult-Factor Authentication is enabled

            string connectionString = ConfigurationManager.AppSettings["ClientSecret"];

            CrmServiceClient conn = new CrmServiceClient(connectionString);
            IOrganizationService orgService = conn.OrganizationWebProxyClient;

            //Get Id of current user
            WhoAmIResponse whoami = (WhoAmIResponse)orgService.Execute(new WhoAmIRequest());
            Guid userId = whoami.UserId;
        }

        private static async Task AuthenticationUsingWebAPI()
        {
            //Authenticated S2S using Web API
            //If you are integrating with CDS using the Web API you will need to handle the authentication process yourself by 
            //requesting an access token from Azure Active Directory.There are official libraries for a number of programming languages that can help you implement this.

            //Basically what you need to do is request an access token from the authority url for your tenant passing in the 
            //client id and secret together with the resource for which you are authenticating

            //The following C# code implements the GetAccessToken() method that authenticates with Active Directory and returns the access token

            string serviceUrl = "https://orge567ee2d.crm4.dynamics.com";
            string clientId = "409c6885-694f-4b09-a159-90fdfc4d57b2";
            string secret = "0~S~uLB1SENVpod-0~pVypFIK9wi7c0-Wt";
            string tenantId = "f9afad9b-70b2-4ce5-8aaa-d53af349c6d3";

            #region Get Token
            AuthenticationContext authContext = new AuthenticationContext("https://login.microsoftonline.com/" + tenantId);
            ClientCredential credential = new ClientCredential(clientId, secret);
            AuthenticationResult result = authContext.AcquireTokenAsync(serviceUrl, credential).Result;
            string token = result.AccessToken;
            #endregion

            #region Http Request
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = new TimeSpan(0, 2, 0);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Retrieve Top 1 Contact
                HttpResponseMessage response = await httpClient.GetAsync(serviceUrl + "/api/data/v9.1/contacts?$top=1");

                // Parse the response
                if (response.IsSuccessStatusCode)
                {
                    var contactsAsString = await response.Content.ReadAsStringAsync();
                    ContactResponse contactResponse = JsonConvert.DeserializeObject<ContactResponse>(contactsAsString);
                    Contact contact = contactResponse.Contacts.FirstOrDefault();
                }
            }
            #endregion
        }
    }
}
