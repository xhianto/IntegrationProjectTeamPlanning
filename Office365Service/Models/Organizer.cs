using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Office365Service.Models
{
    public class Organizer
    {
        [JsonProperty("emailAddress")]
        public EmailAddress EmailAddress { get; set; }
    }
}
