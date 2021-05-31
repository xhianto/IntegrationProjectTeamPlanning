using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

/// <summary>
/// Model Class to send the header of an event. 
/// Attributes can be send to RabbitMQ using the annotated XML properties.
/// </summary>
namespace Office365Service.Models
{
    public class RabbitMQHeader
    {
        [XmlElement("method")]
        public XMLMethod Method { get; set; }
        [XmlElement("source")]
        public XMLSource Source { get; set; }
    }
}
