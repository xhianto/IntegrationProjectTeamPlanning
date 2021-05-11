using System;
using System.Collections.Generic;
using System.Text;

namespace Office365Service.Models
{
    public class CalendarEventLocation
    {
        public string DisplayName { get; set; }
        public string LocationUri { get; set; }
        public string LocationType { get; set; }
        public string UniqueId { get; set; }
        public string UniqueIdType { get; set; }
        public CalendarEventLocationAddress Address { get; set; }
        public CalendarEventLocationCoordinates Coordinates { get; set; }
    }
}
