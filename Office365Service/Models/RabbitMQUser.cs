using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Office365Service.Models
{
/// <summary>
/// Model Class to send the user of an event. 
/// Attributes can be send to RabbitMQ using the annotated XML properties.
/// </summary>
    [Serializable, XmlRoot("user")]
    public class RabbitMQUser
    {
        public RabbitMQUser()
        {
            Header = new RabbitMQHeader();
        }
        [XmlElement("header")]
        public RabbitMQHeader Header { get; set; }
        [XmlElement("uuid")]
        public Guid UUID { get; set; }
        [XmlElement("entityVersion")]
        public int EntityVersion { get; set; }
        [XmlElement("lastName")]
        public string LastName { get; set; }
        [XmlElement("firstName")]
        public string FirstName { get; set; }
        [XmlElement("emailAddress")]
        public string EmailAddress { get; set; }
        [XmlElement("role")]
        public string Role { get; set; }
    }
}
