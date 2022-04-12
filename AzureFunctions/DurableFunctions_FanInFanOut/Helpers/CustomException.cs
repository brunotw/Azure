using DurableFunctions_FanOutFanIn.Model;
using System;

namespace DurableFunctions_FanOutFanIn.Helpers
{
    public class CustomException : Exception
    {
        public string ErrorMessage { get; set; }
        public bool ShouldRetry { get; set; }
        public Lead Lead { get; set; }
    }
}
