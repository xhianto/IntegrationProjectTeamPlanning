using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Office365Service.Models
{
    [Serializable, XmlRoot(ElementName = "heartbeat")]
    public class RabbitMQHeartBeat
    {
        [XmlElement("header")]
        public RabbitMQHeartBeatHeader Header { get; set; }
        [XmlElement("timeStamp")]
        public DateTime TimeStamp { get; set; }
    }
}
