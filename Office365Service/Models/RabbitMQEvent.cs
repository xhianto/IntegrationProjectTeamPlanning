using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Office365Service.Models
{
    [Serializable, XmlRoot(ElementName = "event")]
    public class RabbitMQEvent
    {
        public Header header { get; set; }
        public string uuid { get; set; }
        public string entityVersion { get; set; }
        public string title { get; set; }
        public string organiserId { get; set; }
        public string description { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string location { get; set; }

    }
}
