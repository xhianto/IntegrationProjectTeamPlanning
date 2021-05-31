using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

/// <summary>
/// Model Class to send the Heartbeat of this system. 
/// Attributes can be send to RabbitMQ using the annotated XML properties.
/// </summary>
namespace Office365Service.Models
{
    [Serializable, XmlRoot(ElementName = "heartbeat")]
    public class RabbitMQHeartBeat
    {
        public RabbitMQHeartBeat()
        {
            Header = new RabbitMQHeartBeatHeader();
        }
        [XmlElement("header")]
        public RabbitMQHeartBeatHeader Header { get; set; }
        [XmlElement("timeStamp")]
        public DateTime TimeStamp { get; set; }
    }
}
