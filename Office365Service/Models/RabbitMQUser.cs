using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Office365Service.Models
{
    [Serializable, XmlRoot("user")]
    public class RabbitMQUser
    {
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
