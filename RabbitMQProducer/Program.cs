using System;
using System.Collections.Generic;
using Office365Service;
using Office365Service.Models;
using RestSharp;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using System.IO;
using System.Xml;

namespace RabbitMQProducer
{
    class Program
    {
        private static string email = "docent@ipwt3.onmicrosoft.com";
        private static string uuid = "";
        private static Services OfficeService = new Services();
        private static Token BearerToken = OfficeService.RefreshAccesToken();

        static void Main(string[] args)
        {
            GetUUIDFromEmail(email);
            List<CalendarEvent> events = new List<CalendarEvent>();
            Console.WriteLine(BearerToken.Access_token);

            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();

            restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
            restRequest.AddHeader("Prefer", "outlook.timezone=\"Romance Standard Time\"");
            restRequest.AddHeader("Prefer", "outlook.body-content-type=\"text\"");

            restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{uuid}/calendar/events");
            var response = restClient.Get(restRequest);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine(response.Content);
                Response jsonFromGraph = JsonConvert.DeserializeObject<Response>(response.Content);
                events = jsonFromGraph.Value;
            }
            //test
            foreach (var e in events)
            {
                Console.WriteLine(email);
                Console.WriteLine(uuid);
                Console.WriteLine(e.Subject);
                Console.WriteLine(e.Start.DateTime);
                Console.WriteLine(e.Start.Zone);
                Console.WriteLine(e.End.DateTime);
                Console.WriteLine(e.End.Zone);
            }

            Uri rabbitMQUri = new Uri(Constant.RabbitMQConnectionUrl);
            var message = events;

            foreach (CalendarEvent e in events)
            {
                var factory = new ConnectionFactory
                {
                    Uri = rabbitMQUri
                };
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();
                //channel.QueueDeclare(
                //    "",
                //    durable: true,
                //    exclusive: false,
                //    autoDelete: false,
                //    arguments: null);
                //dit alleen even veranderen in een xml
                string test = ConvertToXml(e);
                var xml = Encoding.UTF8.GetBytes(test);
                Console.WriteLine(test);
                channel.BasicPublish(Constant.RabbitExchangeName,"", null, xml);   //to-canvas_event-queue
                //  channel.BasicPublish("wt3.event-exchange", "to-frontend_event-queue", null, json);
                //  channel.BasicPublish("wt3.event-exchange", "to-monitoring_event-queue", null, json);
            }
        }

        public static void GetUUIDFromEmail(string email)
        {
            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();

            restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
            restRequest.AddHeader("Prefer", "outlook.timezone=\"Romance Standard Time\"");
            restRequest.AddHeader("Prefer", "outlook.body-content-type=\"text\"");

            restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{email}");
            var response = restClient.Get(restRequest);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine(response.Content);
                User user = JsonConvert.DeserializeObject<User>(response.Content);
                uuid = user.Id;
            }
        }
        public static string ConvertToXml(CalendarEvent calendarEvent)
        {
            //convert to xml
            using (var sw = new StringWriter())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.Encoding = Encoding.UTF8;
                using (var writer = XmlWriter.Create(sw, settings))
                {
                    // Build Xml with xw.

                    writer.WriteStartDocument();
                    writer.WriteStartElement("Event");
                    writer.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, "event.xsd");
                    writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                    writer.WriteElementString("uuid", uuid);
                    writer.WriteElementString("entityVersion", "15");
                    writer.WriteElementString("title", calendarEvent.Subject);
                    //Naam of email van organiser
                    writer.WriteElementString("organiserId", calendarEvent.Organizer.EmailAddress.Address);
                    writer.WriteElementString("description", calendarEvent.BodyPreview);
                    writer.WriteElementString("start", calendarEvent.Start.DateTime.ToString());
                    writer.WriteElementString("end", calendarEvent.End.DateTime.ToString());
                    //    writer.WriteStartElement("Location");
                    ////probleempje met cijfer uit straatnaam halen
                    //    Console.WriteLine(calendarEvent.Location.Address.Street);
                    //if (calendarEvent.Location != null)
                    //{
                    //    if (calendarEvent.Location.Address.Street.Length > 0)
                    //    {
                    //        string[] hulp = calendarEvent.Location.Address.Street.Split(' ');
                    //        writer.WriteElementString("number", hulp[hulp.Length-1]);
                    //    }
                    //    writer.WriteElementString("streetName", calendarEvent.Location.Address.Street);
                    //    writer.WriteElementString("city", calendarEvent.Location.Address.City);
                    //    writer.WriteElementString("postalCode", calendarEvent.Location.Address.PostalCode);
                    //}
                    //else
                    //{
                    //    writer.WriteElementString("streetName", "");
                    //    writer.WriteElementString("number", "");
                    //    writer.WriteElementString("city", "");
                    //    writer.WriteElementString("postalCode", "");
                    //}
                    //////Adress bij ons is een object, wat wil je ontvangen
                    ////writer.WriteElementString("locationName", calendarEvent.Location.Address.Street);
                    //////writer.WriteElementString("locationAddress", Event.LocationAddress);
                    ////if (Event.LocationAddress.Contains('%'))// formaat: straatnaam % huisnr % postcode % stad
                    ////{
                    ////    string[] address = Event.LocationAddress.Split('%');
                    ////    if (address.Length == 4)
                    ////    {
                    ////        writer.WriteElementString("streetname", address[0]);
                    ////        writer.WriteElementString("number", address[1]);
                    ////        writer.WriteElementString("city", address[3]);
                    ////        writer.WriteElementString("postalcode", address[2]);
                    ////    }
                    ////}
                    //writer.WriteEndElement(); //EndElement Location
                    writer.WriteEndElement(); //EndElement Event
                    writer.WriteEndDocument();
                    writer.Flush();
                    writer.Close();

                }
                return sw.ToString();
            }
        }
    }
}
