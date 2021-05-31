using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Office365Service.Models
{
    /// <summary>
    /// Model Class to store name and emailaddress of an attendee, stored in an CalendarAttendee instance. 
    /// Attributesan be send to MS Graph API according as Json properties.
    /// </summary>
    public class CalendarAttendeeEmail
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("address")]
        public string Address { get; set; }
    }
}
