using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Office365Service.Models
{
/// <summary>
/// Model Class to send the attendees of an event. 
/// Attributes can be send to RabbitMQ using the annotated XML properties.
/// </summary>
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
        [XmlElement("creatorId")]
        public Guid CreatorId { get; set; }
        [XmlElement("attendeeId")]
        public Guid AttendeeId { get; set; }
        [XmlElement("eventId")]
        public Guid EventId { get; set; }
    }
}
