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
        public CalendarEventTimeZone Start { get; set; }
        [JsonProperty("end")]
        public CalendarEventTimeZone End { get; set; }
        [JsonProperty("body")]
        public CalendarEventBody Body { get; set; }
        [JsonProperty("location")]
        public CalendarEventLocation Location { get; set; }
        [JsonProperty("organizer")]
        public CalendarEventOrganizer Organizer { get; set; }
        //[JsonProperty("bodyPreview")]
        //public string BodyPreview { get; set; }
    }
}
