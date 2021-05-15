using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Office365Service.Models
{
    public class CalendarEvent
    {
        public CalendarEvent()
        {
            Start = new CalendarEventTimeZone();
            End = new CalendarEventTimeZone();
            Body = new CalendarEventBody();
            Location = new CalendarEventLocation();
            Organizer = new CalendarEventOrganizer();
        }
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
