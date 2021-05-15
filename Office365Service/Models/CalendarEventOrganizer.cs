using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Office365Service.Models
{
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
