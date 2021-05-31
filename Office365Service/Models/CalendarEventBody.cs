using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Office365Service.Models
{
    /// <summary>
    /// Model Class to store  . 
    /// </summary>
    public class CalendarEventBody
    {
        [JsonProperty("contentType")]
        public string ContentType { get; set; }
        [JsonProperty("content")]
        public string Content { get; set; }

    }
}
