using Newtonsoft.Json;

namespace Authentications.Models
{
    class ContactResponse
    {
        [JsonProperty(PropertyName = "value")]
        public Contact[] Contacts { get; set; }
    }
}
