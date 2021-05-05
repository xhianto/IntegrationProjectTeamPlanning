using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Office365Service.Models
{
    public class CalendarEvent
    {
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("start")]
        public TimeZone Start { get; set; }
        [JsonProperty("end")]
        public TimeZone End { get; set; }
        [JsonProperty("location")]
        public Location Location { get; set; }
        [JsonProperty("organizer")]
        public Organizer Organizer { get; set; }
        [JsonProperty("bodyPreview")]
        public string BodyPreview { get; set; }
    }
}
