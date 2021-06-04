using Microsoft.VisualStudio.TestTools.UnitTesting;
using Office365Service;
using Office365Service.Models;
using System;

namespace PlanningTest
{
    [TestClass]
    public class Test
    {
        Services services = new Services();
        XmlController xmlController = new XmlController();

        string userXML = "<?xml version='1.0' encoding='utf-8'?>\n" +
                         "<user xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>\n" +
                         "  <header>\n" +
                         "    <method>CREATE</method>\n" +
                         "    <source>CANVAS</source>\n" +
                         "  </header>\n" +
                         "  <uuid>bf2d8864-c07d-11eb-8529-0242ac130003</uuid>\n" +
                         "  <entityVersion>1</entityVersion>\n" +
                         "  <lastName>Doe</lastName>\n" +
                         "  <firstName>John</firstName>\n" +
                         "  <emailAddress>John.Doe@ipwt3.onmicrosoft.com</emailAddress>\n" +
                         "  <role>student</role>\n" +
                         "</user>";

        string eventXML = "<?xml version='1.0'?>\n" +
                          "<event xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>\n" +
                          "  <header>\n" +
                          "    <method>CREATE</method>\n" +
                          "    <source>CANVAS</source>\n" +
                          "  </header>\n" +
                          "  <uuid>dcf8fa36-c07d-11eb-8529-0242ac130003</uuid>\n" +
                          "  <entityVersion>1</entityVersion>\n" +
                          "  <title>etven </title>\n" +
                          "  <organiserId>bf2d8864-c07d-11eb-8529-0242ac130003</organiserId>\n" +
                          "  <description>Event</description>\n" +
                          "  <start>2021-06-18T08:00:00</start>\n" +
                          "  <end>2021-06-18T08:30:00</end>\n" +
                          "  <location>Rue de la Fontaine%4%%Brussels%1000</location>\n" +
                          "</event>";

        string attendanceXML = "<?xml version='1.0' encoding='UTF-8'?>\n" +
                               "<attendance xsi:noNamespaceSchemaLocation='attendance.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>\n" +
                               "    <header>\n" +
                               "        <method>CREATE</method>\n" +
                               "    </header>\n" +
                               "    <uuid>dcf8fa36-c07d-11eb-8529-0242ac130003</uuid>\n" +
                               "    <creatorId>bf2d8864-c07d-11eb-8529-0242ac130003</creatorId>\n" +
                               "    <attendeeId>c6365d02-c07d-11eb-8529-0242ac130003</attendeeId>\n" +
                               "    <eventId>dcf8fa36-c07d-11eb-8529-0242ac130003</eventId>\n" +
                               "</attendance>";

        string heartbeatXML = "<?xml version='1.0' encoding='UTF-8'?>\n" +
                              "<heartbeat xsi:noNamespaceSchemaLocation='schema.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>\n" +
                              "    <header>\n" +
                              "        <status>ONLINE</status>\n" +
                              "        <source>CANVAS</source>\n" +
                              "    </header>\n" +
                              "    <timeStamp>2021-05-30T21:00:00</timeStamp>\n" +
                              "</heartbeat>";

        [TestMethod]
        public void BearerTokenNotNull()
        {
            Assert.IsNotNull(services.RefreshAccesToken());
        }

        [TestMethod]
        public void CheckIfBearerToken()
        {
            Token token = services.RefreshAccesToken();
            Console.WriteLine(token);
            Assert.IsTrue(token.Token_type == "Bearer");
        }

        [TestMethod]
        public void ValideerUserXML()
        {
            Assert.IsTrue(xmlController.XSDValidatie(userXML, "User"));
        }

        [TestMethod]
        public void ValideerFalseUserXML()
        {
            Assert.IsFalse(xmlController.XSDValidatie("", "User"));
        }

        [TestMethod]
        public void ValideerEventXML()
        {
            Assert.IsTrue(xmlController.XSDValidatie(eventXML, "Event"));
        }

        [TestMethod]
        public void ValideerFalseEventXML()
        {
            Assert.IsFalse(xmlController.XSDValidatie("", "Event"));
        }

        [TestMethod]
        public void ValideerAttendanceXML()
        {
            Assert.IsTrue(xmlController.XSDValidatie(attendanceXML, "Attendance"));
        }

        [TestMethod]
        public void ValideerFalseAttendanceXML()
        {
            Assert.IsFalse(xmlController.XSDValidatie("", "Attendance"));
        }

        [TestMethod]
        public void ValideerHeartbeatXML()
        {
            Assert.IsTrue(xmlController.XSDValidatie(heartbeatXML, "Heartbeat"));
        }

        [TestMethod]
        public void ValideerFalseHeartbeatXML()
        {
            Assert.IsFalse(xmlController.XSDValidatie("", "Heartbeat"));
        }
        [TestMethod]
        public void ConvertToRabbitMQUser()
        {
            Assert.IsNotNull(xmlController.ConvertXMLtoObject<RabbitMQUser>(userXML));
        }
        [TestMethod]
        public void ConvertToFalseRabbitMQUser()
        {
            Assert.IsNotNull(xmlController.ConvertXMLtoObject<RabbitMQUser>(""));
        }
        [TestMethod]
        public void ConvertToRabbitMQEvent()
        {
            Assert.IsNotNull(xmlController.ConvertXMLtoObject<RabbitMQEvent>(eventXML));
        }
        [TestMethod]
        public void ConvertToFalseRabbitMQEvent()
        {
            Assert.IsNotNull(xmlController.ConvertXMLtoObject<RabbitMQEvent>(""));
        }
        [TestMethod]
        public void ConvertToRabbitMQAttendance()
        {
            Assert.IsNotNull(xmlController.ConvertXMLtoObject<RabbitMQAttendance>(attendanceXML));
        }
        [TestMethod]
        public void ConvertToFalseRabbitMQAttendance()
        {
            Assert.IsNotNull(xmlController.ConvertXMLtoObject<RabbitMQAttendance>(""));
        }
        [TestMethod]
        public void ConvertToRabbitMQHeartbeat()
        {
            Assert.IsNotNull(xmlController.ConvertXMLtoObject<RabbitMQHeartBeat>(heartbeatXML));
        }
        [TestMethod]
        public void ConvertToFalseRabbitMQHeartbeat()
        {
            Assert.IsNotNull(xmlController.ConvertXMLtoObject<RabbitMQHeartBeat>(""));
        }
        [TestMethod]
        public void ConvertUserToXML()
        {
            RabbitMQUser rabbitMQUser = new RabbitMQUser();
            rabbitMQUser.Header.Method = XMLMethod.CREATE;
            rabbitMQUser.Header.Source = XMLSource.PLANNING;
            rabbitMQUser.UUID = new Guid("bf2d8864-c07d-11eb-8529-0242ac130003");
            rabbitMQUser.EntityVersion = 1;
            rabbitMQUser.LastName = "Doe";
            rabbitMQUser.FirstName = "John";
            rabbitMQUser.EmailAddress = "John.Doe@ipwt3.onmicrosoft.com";
            rabbitMQUser.Role = "student";
            string xml = xmlController.ConvertObjectToXML(rabbitMQUser);
            Assert.IsTrue(xmlController.XSDValidatie(xml, "User"));
        }
        [TestMethod]
        public void ConvertEventToXML()
        {
            RabbitMQEvent rabbitMQEvent = new RabbitMQEvent();
            rabbitMQEvent.Header.Method = XMLMethod.CREATE;
            rabbitMQEvent.Header.Source = XMLSource.PLANNING;
            rabbitMQEvent.UUID = new Guid("bf2d8864-c07d-11eb-8529-0242ac130003");
            rabbitMQEvent.EntityVersion = 1;
            rabbitMQEvent.Title = "Titel";
            rabbitMQEvent.OrganiserId = new Guid("bf2d8864-c07d-11eb-8529-0242ac130003");
            rabbitMQEvent.Description = "Omschrijving";
            rabbitMQEvent.Start = DateTime.Now;
            rabbitMQEvent.End = rabbitMQEvent.Start.AddHours(3);
            rabbitMQEvent.Location = "Rue de la Fontaine%4%%Brussels%1000";
            string xml = xmlController.ConvertObjectToXML(rabbitMQEvent);
            Assert.IsTrue(xmlController.XSDValidatie(xml, "Event"));
        }
        [TestMethod]
        public void ConvertAttendanceToXML()
        {
            RabbitMQAttendance rabbitMQAttendance = new RabbitMQAttendance();
            rabbitMQAttendance.Header.Method = XMLMethod.CREATE;
            rabbitMQAttendance.UUID = new Guid("dcf8fa36-c07d-11eb-8529-0242ac130003");
            rabbitMQAttendance.CreatorId = new Guid("bf2d8864-c07d-11eb-8529-0242ac130003");
            rabbitMQAttendance.AttendeeId = new Guid("c6365d02-c07d-11eb-8529-0242ac130003");
            rabbitMQAttendance.EventId = new Guid("dcf8fa36-c07d-11eb-8529-0242ac130003");
            string xml = xmlController.ConvertObjectToXML(rabbitMQAttendance);
            Assert.IsTrue(xmlController.XSDValidatie(xml, "Attendance"));
        }
        [TestMethod]
        public void ConvertHeartbeatToXML()
        {
            RabbitMQHeartBeat rabbitMQHeartBeat = new RabbitMQHeartBeat();
            rabbitMQHeartBeat.Header.Source = XMLSource.PLANNING;
            rabbitMQHeartBeat.Header.Status = XMLStatus.ONLINE;
            rabbitMQHeartBeat.TimeStamp = DateTime.Now;
            string xml = xmlController.ConvertObjectToXML(rabbitMQHeartBeat);
            Assert.IsTrue(xmlController.XSDValidatie(xml, "Heartbeat"));
        }
    }
}
