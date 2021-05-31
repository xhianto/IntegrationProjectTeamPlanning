using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

/// <summary>
/// Model Class to store name and emailaddress of an event, stored in an CalendarEvent instance. . 
/// Attributes can be send to MS Graph API using the annotated Json properties.
/// </summary>
namespace Office365Service.Models
{

    public class CalendarEventEmailAddress
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("address")]
        public string Address { get; set; }
    }
}
