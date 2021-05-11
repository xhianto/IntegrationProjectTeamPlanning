using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Office365Service.Models
{
    public class RabbitMQHeader
    {
        [XmlElement("method")]
        public string Method { get; set; }
        [XmlElement("source")]
        public string Source { get; set; }
    }
}
