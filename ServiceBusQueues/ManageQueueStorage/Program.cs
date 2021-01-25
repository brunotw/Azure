using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace ManageQueueStorage
{
    class Program
    {
        private const string connectionString = "DefaultEndpointsProtocol=https;AccountName=queuestoragebw;AccountKey=arixgNYblRu6/N0RpNxmBmmmmpA4XU3NFw4OIVPw1vQh1aYTqELGGFZTV8sTq/tEwl63DsUtC/cDYaqkDGu1Rg==;EndpointSuffix=core.windows.net";
        private const string queueName = "salesqueue";
        private static CloudStorageAccount storageAccount;
        private static CloudQueueClient client;
        private static CloudQueue queue;

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Main started");

                Init();
                SendMessageToQueue().Wait();
                Client client = StartReceiver().Result;

                Console.WriteLine("Main finished");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An exception has ocurred.");
                Console.WriteLine(ex.Message);
          
            }
            finally
            {
                Console.WriteLine("Process finished");
                Console.ReadKey();
            }
        }
        public static void Init()
        {
            storageAccount = CloudStorageAccount.Parse(connectionString);
            client = storageAccount.CreateCloudQueueClient();
            queue = client.GetQueueReference(queueName);
        }

        public async static Task SendMessageToQueue()
        {
            string message = JsonConvert.SerializeObject(Client.GetSampleClient());

            bool createdQueue = await queue.CreateIfNotExistsAsync();

            if (createdQueue)
            {
                Console.WriteLine("The queue of news articles was created.");
            }

            var queueMessage = new CloudQueueMessage(message); ;
            await queue.AddMessageAsync(queueMessage);

            Console.WriteLine("Message sent to queue: " + message);
        }

        public static async Task<Client> StartReceiver()
        {
            Client client = null;

            bool exists = await queue.ExistsAsync();
            if (exists)
            {
                CloudQueueMessage retrievedArticle = await queue.GetMessageAsync();
                if (retrievedArticle != null)
                {
                    string newsMessage = retrievedArticle.AsString;
                    await queue.DeleteMessageAsync(retrievedArticle);
                    client = JsonConvert.DeserializeObject<Client>(newsMessage);
                    Console.WriteLine("Message received: " + newsMessage);
                }
            }
            else
            {
                Console.WriteLine("<queue empty or not created>");
            }

            return client;
        }
    }
}
