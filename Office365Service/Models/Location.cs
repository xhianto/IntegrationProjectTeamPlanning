using System;
using System.Collections.Generic;
using System.Text;

namespace Office365Service.Models
{
    public class Location
    {
        public string DisplayName { get; set; }
        public string LocationUri { get; set; }
        public string LocationType { get; set; }
        public string UniqueId { get; set; }
        public string UniqueIdType { get; set; }
        public Address Address { get; set; }
        public Coordinates Coordinates { get; set; }
    }
}
