using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Office365Service.Models
{
/// <summary>
/// Model Class to store the coordinates of the location of an event, stored in an CalendarEvent instance. 
/// Attributes can be send to MS Graph API using the annotated Json properties.
/// </summary>
    public class CalendarEventLocationCoordinates
    {
        [JsonProperty("latitude")]
        public float Latitude { get; set; }
        [JsonProperty("longitude")]
        public float Longitude { get; set; }
    }
}
