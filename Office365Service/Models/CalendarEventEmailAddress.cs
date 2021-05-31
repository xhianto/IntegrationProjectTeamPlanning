using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Office365Service.Models
{
    /// <summary>
    /// Model Class to store  . 
    /// </summary>
    public class CalendarEventEmailAddress
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("address")]
        public string Address { get; set; }
    }
}
