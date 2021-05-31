using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Office365Service.Models
{
    /// <summary>
    /// Model Class to store the name of the location of an event, stored in an CalendarEvent instance. 
    /// Attributes can be send to MS Graph API using the annotated Json properties.
    /// </summary>
    public class CalendarEventLocation
    {
        public CalendarEventLocation()
        {
            Address = new CalendarEventLocationAddress();
            Coordinates = new CalendarEventLocationCoordinates();
        }
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("locationUri")]
        public string LocationUri { get; set; }
        [JsonProperty("locationType")]
        public string LocationType { get; set; }
        [JsonProperty("uniqueId")]
        public string UniqueId { get; set; }
        [JsonProperty("uniqueIdType")]
        public string UniqueIdType { get; set; }
        [JsonProperty("address")]
        public CalendarEventLocationAddress Address { get; set; }
        [JsonProperty("coordinates")]
        public CalendarEventLocationCoordinates Coordinates { get; set; }
    }
}
