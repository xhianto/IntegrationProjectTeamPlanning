using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Office365Service.Models
{
    public class RabbitMQAttendanceHeader
    {
        [XmlElement("method")]
        public XMLMethod Method { get; set; }
    }
}
