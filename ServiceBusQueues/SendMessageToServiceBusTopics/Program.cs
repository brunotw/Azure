using Microsoft.Azure.ServiceBus;
using System;
using System.Text;

namespace SendMessageToServiceBusTopics
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Console started");
            string topicName= "salesperformancemessages";
            string connectionString = "Endpoint=sb://salesteamappbw.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=6BbN+0KHbz6n62KhlShCnXQ09PcHemTeY8LCtPY0j+E=";
            TopicClient topicClient = new TopicClient(connectionString, topicName);

            string message = "Cancel! I can't believe you use canned mushrooms!";
            var encodedMessage = new Message(Encoding.UTF8.GetBytes(message));
            topicClient.SendAsync(encodedMessage);

            Console.WriteLine("Message Sent");
            Console.ReadKey();
        }
    }
}
