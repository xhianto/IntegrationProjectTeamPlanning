using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Office365Service.Models
{
    [Serializable, XmlRoot(ElementName = "attendance")]
    public class RabbitMQAttendance
    {
        public RabbitMQAttendance()
        {
            Header = new RabbitMQAttendanceHeader();
        }
        [XmlElement("header")]
        public RabbitMQAttendanceHeader Header { get; set; }
        [XmlElement("uuid")]
        public Guid UUID { get; set; }
        [XmlElement("userId")]
        public Guid UserId { get; set; }
        [XmlElement("eventId")]
        public Guid EventId { get; set; }
    }
}
