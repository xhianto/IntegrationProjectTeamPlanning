using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Office365Service.Models
{
    public class RabbitMQHeartBeatHeader
    {
        [XmlElement("status")]
        public XMLStatus Status { get; set; }
        [XmlElement("source")]
        public XMLSource Source { get; set; }
    }
}
