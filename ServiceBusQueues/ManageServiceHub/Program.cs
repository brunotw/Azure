﻿using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Messaging.EventHubs.Producer;

namespace ManageServiceHub
{
    class Program
    {
        private const string eventHubConnectionString = "Endpoint=sb://eventhubdefaultnamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=fw5jxoqFwDSOiwsm/Pk0mNBLLd1cwRX85wCPh5NweIE=";
        private const string eventHubName = "eventhub1";
        private const string blobStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=queuestoragebw;AccountKey=arixgNYblRu6/N0RpNxmBmmmmpA4XU3NFw4OIVPw1vQh1aYTqELGGFZTV8sTq/tEwl63DsUtC/cDYaqkDGu1Rg==;EndpointSuffix=core.windows.net";
        private const string blobContainerName = "container1";

        static void Main()
        {
            Sender().Wait();
            //Receiver().Wait();
        }

        private async static Task Sender()
        {
            // Create a producer client that you can use to send events to an event hub
            await using (var producerClient = new EventHubProducerClient(eventHubConnectionString, eventHubName))
            {
                // Create a batch of events 
                using EventDataBatch eventBatch = await producerClient.CreateBatchAsync();

                // Add events to the batch. An event is a represented by a collection of bytes and metadata. 
                eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(Client.GetSampleClientSerialized(20))));
                eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(Client.GetSampleClientSerialized(30))));
                eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(Client.GetSampleClientSerialized(40))));

                // Use the producer client to send the batch of events to the event hub
                await producerClient.SendAsync(eventBatch);
                Console.WriteLine("A batch of 3 events has been published.");
            }
        }

        private async static Task Receiver()
        {
            // Read from the default consumer group: $Default
            string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;

            // Create a blob container client that the event processor will use 
            BlobContainerClient storageClient = new BlobContainerClient(blobStorageConnectionString, blobContainerName);

            // Create an event processor client to process events in the event hub
            EventProcessorClient processor = new EventProcessorClient(storageClient, consumerGroup, eventHubConnectionString, eventHubName);

            // Register handlers for processing events and handling errors
            processor.ProcessEventAsync += ProcessEventHandler;
            processor.ProcessErrorAsync += ProcessErrorHandler;

            // Start the processing
            await processor.StartProcessingAsync();

            // Wait for 10 seconds for the events to be processed
            await Task.Delay(TimeSpan.FromSeconds(10));

            // Stop the processing
            await processor.StopProcessingAsync();
        }

        static async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            // Write the body of the event to the console window
            Console.WriteLine("\tReceived event: {0}", Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray()));

            // Update checkpoint in the blob storage so that the app receives only new events the next time it's run
            await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
        }

        static Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
        {
            // Write details about the error to the console window
            Console.WriteLine($"\tPartition '{ eventArgs.PartitionId}': an unhandled exception was encountered. This was not expected to happen.");
            Console.WriteLine(eventArgs.Exception.Message);
            return Task.CompletedTask;
        }
    }
}