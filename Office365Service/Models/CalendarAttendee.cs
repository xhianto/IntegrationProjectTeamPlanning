using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Office365Service.Models
{
    public class CalendarAttendees
    {
        public CalendarAttendees()
        {
            Attendees = new List<CalendarAttendee>();
        }
        [JsonProperty("attendees")]
        public List<CalendarAttendee> Attendees { get; set; }
    }
    public class CalendarAttendee
    {
        public CalendarAttendee()
        {
            EmailAddress = new CalendarAttendeeEmail();
        }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("emailAddress")]
        public CalendarAttendeeEmail EmailAddress { get; set; }
    }
}
