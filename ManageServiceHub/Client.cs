using Newtonsoft.Json;
using System;

namespace ManageServiceBus
{
    public class Client
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime CreatedOn { get; set; }

        public static Client GetSampleClient()
        {
            return new Client
            {
                Name = "Bruno Willian",
                Age = 29,
                CreatedOn = DateTime.Now
            };

        }

        public static string GetSampleClientSerialized(int age)
        {
            Client client = new Client
            {
                Name = "Bruno Willian",
                Age = age,
                CreatedOn = DateTime.Now
            };

            return JsonConvert.SerializeObject(client);

        }
    }
}
