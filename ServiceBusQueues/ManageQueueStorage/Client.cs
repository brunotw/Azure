using System;
using System.Collections.Generic;
using System.Text;

namespace ManageQueueStorage
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
    }
}
