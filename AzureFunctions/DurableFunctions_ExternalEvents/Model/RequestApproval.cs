using Newtonsoft.Json;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace DurableFunctions_ExternalEvents.Model
{
    public class RequestApproval
    {
        [JsonPropertyName("instanceId")]
        public string InstanceId { get; set; }

        [JsonPropertyName("accidentImageBase64")]
        public string AccidentImageBase64 { get; set; }
    }
}
