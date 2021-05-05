using System;
using System.Collections.Generic;
using System.Text;

namespace Office365Service.Models
{
    public class OfficeCalendar
    {
        public string Email { get; set; }
        public string Subject { get; set; }
        public TimeZone Start { get; set; }
        public TimeZone End { get; set; }
        public Location Location { get; set; }
    }
}
