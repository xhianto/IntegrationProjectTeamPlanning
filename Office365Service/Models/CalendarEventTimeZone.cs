using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Office365Service.Models
{
/// <summary>
/// Model Class to store the Timezone of an event, stored in an CalendarEvent instance. . 
/// Attributes can be send to MS Graph API using the annotated Json properties.
/// </summary>
    public class CalendarEventTimeZone
    {
        [JsonProperty("dateTime")]
        public DateTime DateTime { get; set; }
        [JsonProperty("timeZone")]
        public string Zone { get; set; }
    }
}
