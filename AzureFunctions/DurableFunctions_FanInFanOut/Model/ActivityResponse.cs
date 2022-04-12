using System;
using System.Collections.Generic;
using System.Text;

namespace DurableFunctions_FanOutFanIn.Model
{
    public class ActivityResponse
    {
        public bool Success { get; set; }
        public string Result { get; set; }
    }
}
