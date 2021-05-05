using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Office365Service.Models
{
    //mogelijk paar toevoegingen net als bij events
    public class RabbitMQLocation
    {
        public string StreetName { get; set; }
        public string Number { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
    }
}
