using Microsoft.Azure.ServiceBus;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsumeMessagesFromServiceBusTopic
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Console started");
            string topicName = "salesperformancemessages";
            string serviceBusConnectionString = "Endpoint=sb://salesteamappbw.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=6BbN+0KHbz6n62KhlShCnXQ09PcHemTeY8LCtPY0j+E=";
            string subscriptionName = "Americas";

            SubscriptionClient subscriptionClient = new SubscriptionClient(serviceBusConnectionString, topicName, subscriptionName, ReceiveMode.PeekLock);

            var messageHandlerOptions = new MessageHandlerOptions(OnException);
            subscriptionClient.RegisterMessageHandler(OnMessage, messageHandlerOptions);
            
            Console.WriteLine("Listening, press any key");
            Console.ReadKey();
        }

        static Task OnMessage(Message m, CancellationToken ct)
        {
            var messageText = Encoding.UTF8.GetString(m.Body);
            Console.WriteLine("Got a message:");
            Console.WriteLine(messageText);
            Console.WriteLine($"Enqueued at {m.SystemProperties.EnqueuedTimeUtc}");
            return Task.CompletedTask;
        }

        static Task OnException(ExceptionReceivedEventArgs args)
        {
            Console.WriteLine("Got an exception:");
            Console.WriteLine(args.Exception.Message);
            Console.WriteLine(args.ExceptionReceivedContext.Action);
            Console.WriteLine(args.ExceptionReceivedContext.ClientId);
            Console.WriteLine(args.ExceptionReceivedContext.Endpoint);
            Console.WriteLine(args.ExceptionReceivedContext.EntityPath);
            return Task.CompletedTask;
        }
    }
}
