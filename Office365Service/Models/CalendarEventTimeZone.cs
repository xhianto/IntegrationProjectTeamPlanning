using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Office365Service.Models
{
    public class CalendarEventTimeZone
    {
        [JsonProperty("dateTime")]
        public DateTime DateTime { get; set; }
        [JsonProperty("timeZone")]
        public string Zone { get; set; }
    }
}
