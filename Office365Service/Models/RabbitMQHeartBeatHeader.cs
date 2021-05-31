using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Office365Service.Models
{
/// <summary>
/// Model Class to send the header of the Heartbeat of this system. 
/// Attributes can be send to RabbitMQ using the annotated XML properties.
/// </summary>
    public class RabbitMQHeartBeatHeader
    {
        [XmlElement("status")]
        public XMLStatus Status { get; set; }
        [XmlElement("source")]
        public XMLSource Source { get; set; }
    }
}
