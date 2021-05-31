using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

/// <summary>
/// Model Class to send the header of the Heartbeat of this system. 
/// Attributes can be send to RabbitMQ using the annotated XML properties.
/// </summary>
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
