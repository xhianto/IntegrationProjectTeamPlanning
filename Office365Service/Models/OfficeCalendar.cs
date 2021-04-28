using System;
using System.Collections.Generic;
using System.Text;

namespace Office365Service.Models
{
    public class OfficeCalendar
    {
        public string email { get; set; }
        public string subject { get; set; }
        public TimeZone start { get; set; }
        public TimeZone end { get; set; }
    }
}
