using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

/// <summary>
/// Model Class to store the address of the location of an event, stored in an CalendarEvent instance. 
/// Attributes can be send to MS Graph API using the annotated Json properties.
/// </summary>
namespace Office365Service.Models
{
    public class CalendarEventLocationAddress
    {
        [JsonProperty("street")]
        public string Street { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("countryOrRegion")]
        public string CountryOrRegion { get; set; }
        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }
    }
}
