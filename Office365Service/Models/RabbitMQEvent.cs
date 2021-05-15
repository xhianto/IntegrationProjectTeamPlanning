using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Office365Service.Models
{
    [Serializable, XmlRoot(ElementName = "event")]
    public class RabbitMQEvent
    {
        public RabbitMQEvent()
        {
            Header = new RabbitMQHeader();
        }

        [XmlElement("header")]
        public RabbitMQHeader Header { get; set; }
        [XmlElement("uuid")]
        public Guid UUID { get; set; }
        [XmlElement("entityVersion")]
        public int EntityVersion { get; set; }
        [XmlElement("title")]
        public string Title { get; set; }
        [XmlElement("organiserId")]
        public Guid OrganiserId { get; set; }
        [XmlElement("description")]
        public string Description { get; set; }
        [XmlElement("start")]
        public DateTime Start { get; set; }
        [XmlElement("end")]
        public DateTime End { get; set; }
        [XmlElement("location")]
        public string Location { get; set; }

    }
}
