using System;
using System.Collections.Generic;
using System.Text;

namespace Office365Service.Models
{
    /// <summary>
    /// Class <c> officeCalendar </c> models an event in the MS Office Calendar.
    /// </summary>
    /// <param name="email">the e-mail address of the organisor of the event</param>
    /// <param name="subject">the title of the event</param>
    /// <param name="start">the time the event starts</param>
    /// <param name="end">the time the event ends</param>
    public class OfficeCalendar
    {
        public string email { get; set; }
        public string subject { get; set; }
        public TimeZone start { get; set; }
        public TimeZone end { get; set; }
    }
}
