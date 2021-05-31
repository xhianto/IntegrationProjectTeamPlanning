using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Office365Service.Models
{
    /// <summary>
    /// Model Class to store the body of an xml message representing an Event. 
    /// Attributes can be send to MS Graph API using the annotated Json properties.
    /// </summary>
    public class CalendarEventBody
    {
        [JsonProperty("contentType")]
        public string ContentType { get; set; }
        [JsonProperty("content")]
        public string Content { get; set; }

    }
}
