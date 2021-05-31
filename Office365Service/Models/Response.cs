using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Model Class to handle a response of the MS Graph API. 
/// </summary>
namespace Office365Service.Models
{
    public class Response
    {
        public List<CalendarEvent> Value { get; set; }  
    }
}
