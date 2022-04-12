using System;
using System.Collections.Generic;
using System.Text;

namespace DurableFunctions_ExternalEvents.Model
{
    public class RaiseEventResponse
    {
        public string instanceId { get; set; }
        public string approved { get; set; }
    }
}
