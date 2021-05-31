using System;
using System.Collections.Generic;
using System.Text;

namespace Office365Service.Models
{
/// <summary>
/// Model Class to handle a response of the MS Graph API. 
/// </summary>
    public class Response
    {
        public List<CalendarEvent> Value { get; set; }  
    }
}
