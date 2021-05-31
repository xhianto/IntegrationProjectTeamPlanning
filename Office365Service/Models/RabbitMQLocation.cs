using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Office365Service.Models
{
/// <summary>
/// Model Class to send the location of an event. 
/// Attributes can be send to RabbitMQ using the annotated XML properties.
/// </summary>
    //mogelijk paar toevoegingen net als bij events
    public class RabbitMQLocation
    {
        public string StreetName { get; set; }
        public string Number { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
    }
}
