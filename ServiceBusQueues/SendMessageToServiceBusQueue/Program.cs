using System;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace ServiceBusQueues
{
    /*
    1. Add reference Microsoft.Azure.ServiceBus
    2. Get connection string from Shared Access Policies in Service Bus namespace
    */
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Console started");
                string queueName = "salesmessages";

                string connectionString = "Endpoint=sb://salesteamappbw.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=AgNEnKF7hPBqea58QisyNrgLvlOthO9XG1aTcsletXE=";
                QueueClient queueClient = new QueueClient(connectionString, queueName);

                Client client = new Client
                {
                    Name = "Bruno Willian",
                    Age = 29
                };

                string message = JsonConvert.SerializeObject(client);
                var encodedMessage = new Message(Encoding.UTF8.GetBytes(message));
                queueClient.SendAsync(encodedMessage);

                Console.Write("Message successfully sent");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine("Console finished");
                Console.ReadKey();
            }
        }

        public void MessageHandler()
        {
            Console.WriteLine("method called");
        }
    }

    public class Client
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
