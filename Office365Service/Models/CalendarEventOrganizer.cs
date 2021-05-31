using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Office365Service.Models
{
/// <summary>
/// Model Class to store name and emailaddress of the organiser of an event, stored in an CalendarEvent instance. . 
/// Attributes can be send to MS Graph API using the annotated Json properties.
/// </summary>
    public class CalendarEventOrganizer
    {
        public CalendarEventOrganizer()
        {
            EmailAddress = new CalendarEventEmailAddress();
        }
        [JsonProperty("emailAddress")]
        public CalendarEventEmailAddress EmailAddress { get; set; }
    }
}
