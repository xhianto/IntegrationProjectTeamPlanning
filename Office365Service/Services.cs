using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Office365Service.Models;
using RestSharp;

namespace Office365Service
{
    public class Services
    {
        public Token BearerToken = new Token();
        public Token RefreshAccesToken()
        {
            if (!string.IsNullOrEmpty(Constant.Tenant_Id))
            {
                RestClient restClient = new RestClient();
                RestRequest restRequest = new RestRequest();

                restRequest.AddParameter("client_id", Constant.Client_Id);
                restRequest.AddParameter("scope", Constant.Scopes);
                restRequest.AddParameter("grant_type", "client_credentials");
                restRequest.AddParameter("client_secret", Constant.Client_Secret);

                restClient.BaseUrl = new Uri($"https://login.microsoftonline.com/{Constant.Tenant_Id}/oauth2/v2.0/token");
                var response = restClient.Post(restRequest);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    BearerToken = JsonConvert.DeserializeObject<Token>(response.Content);
                }
            }
            return BearerToken;
        }

        public string GetUUIDFromEmail(string email)
        {
            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();
            string useruuid = "";

            restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
            restRequest.AddHeader("Prefer", "outlook.timezone=\"Romance Standard Time\"");
            restRequest.AddHeader("Prefer", "outlook.body-content-type=\"text\"");

            restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{email}");
            var response = restClient.Get(restRequest);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine(response.Content);
                User user = JsonConvert.DeserializeObject<User>(response.Content);
                useruuid = user.Id;
            }
            return useruuid;
        }
        public string GetEmailFromUUID(string uuid)
        {
            string email = "";
            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();
            BearerToken = RefreshAccesToken();

            restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
            restRequest.AddHeader("Prefer", "outlook.timezone=\"Romance Standard Time\"");
            restRequest.AddHeader("Prefer", "outlook.body-content-type=\"text\"");

            restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{uuid}");
            var response = restClient.Get(restRequest);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //Console.WriteLine(response.Content);
                User user = JsonConvert.DeserializeObject<User>(response.Content);
                email = user.UserPrincipalName;
            }
            return email;
        }
        public string ConvertToXml(CalendarEvent calendarEvent, string uuid)
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
                    writer.WriteStartElement("event");
                    writer.WriteAttributeString("xsi", "noNamespaceSchemaLocation", null, "XMLvalidations\\event.xsd");
                    writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                    writer.WriteStartElement("header");
                    writer.WriteElementString("method", "CREATE"); //nog hardcoded voor create
                    writer.WriteElementString("source", "PLANNING");
                    writer.WriteEndElement();
                    writer.WriteElementString("uuid", uuid);
                    writer.WriteElementString("entityVersion", "15");
                    writer.WriteElementString("title", calendarEvent.Subject);
                    //Naam of email van organiser
                    writer.WriteElementString("organiserId", GetUUIDFromEmail(calendarEvent.Organizer.EmailAddress.Address));
                    writer.WriteElementString("description", "description");
                    writer.WriteElementString("start", "2021-01-01T01:01:00"); //calendarEvent.Start.DateTime.ToString());
                    writer.WriteElementString("end", "2021-01-01T02:01:00"); //calendarEvent.End.DateTime.ToString());
                    writer.WriteElementString("location", "straat 48 1000 stad");
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

        public string ConvertObjectToXML(CalendarEvent calendarEvent, string uuid)
        {
            string xml = "";
            RabbitMQEvent rabbitMQEvent = new RabbitMQEvent();
            rabbitMQEvent.Header = new RabbitMQHeader();
            rabbitMQEvent.Header.Method = "CREATE";
            rabbitMQEvent.Header.Source = "PLANNING";
            rabbitMQEvent.UUID = new Guid(uuid);
            rabbitMQEvent.EntityVersion = 1;
            rabbitMQEvent.Title = calendarEvent.Subject;
            rabbitMQEvent.OrganiserId = new Guid(uuid);
            rabbitMQEvent.Description = "Komt dit door?";
            rabbitMQEvent.Start = calendarEvent.Start.DateTime;
            rabbitMQEvent.End = calendarEvent.End.DateTime;
            rabbitMQEvent.Location = "straat 48 1000 stad";
            try
            {
                MemoryStream mStream = new MemoryStream();
                XmlSerializer serializer = new XmlSerializer(typeof(RabbitMQEvent));
                XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.UTF8);
                writer.Formatting = System.Xml.Formatting.Indented;
                serializer.Serialize(writer, rabbitMQEvent);
                xml = Encoding.UTF8.GetString(mStream.ToArray()).Substring(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return xml;
        }
        public void Post(RabbitMQEvent rabbitMQEvent)
        {
            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();

            CalendarEvent calendarEvent = new CalendarEvent();
            calendarEvent.Subject = rabbitMQEvent.Title;
            calendarEvent.Start = new Models.CalendarEventTimeZone();
            if (rabbitMQEvent.Header.Source.ToLower() != "canvas")
                calendarEvent.Start.DateTime = DateTime.Parse(rabbitMQEvent.Start.ToString());
            else
                calendarEvent.Start.DateTime = DateTime.ParseExact(rabbitMQEvent.Start.ToString(), "d/MM/yyyy H:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            calendarEvent.Start.Zone = "Romance Standard Time";
            calendarEvent.End = new Models.CalendarEventTimeZone();
            if (rabbitMQEvent.Header.Source.ToLower() != "canvas")
                calendarEvent.End.DateTime = DateTime.Parse(rabbitMQEvent.End.ToString());
            else
                calendarEvent.End.DateTime = DateTime.ParseExact(rabbitMQEvent.End.ToString(), "d/MM/yyyy H:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            calendarEvent.End.Zone = "Romance Standard Time";
            //calendarEvent.BodyPreview = rabbitMQEvent.Description;
            calendarEvent.Body = new CalendarEventBody();
            calendarEvent.Body.ContentType = "text";
            calendarEvent.Body.Content = rabbitMQEvent.Description;
            calendarEvent.Organizer = new CalendarEventOrganizer();
            calendarEvent.Organizer.EmailAddress = new CalendarEventEmailAddress();
            calendarEvent.Organizer.EmailAddress.Address = rabbitMQEvent.OrganiserId.ToString();
            //calendarEvent.Location.DisplayName = rabbitMQEvent.location;
            BearerToken = RefreshAccesToken();

            var json = JsonConvert.SerializeObject(calendarEvent);
            restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
            restRequest.AddJsonBody(json);
            Console.WriteLine(json);
            //restRequest.AddHeader("Prefer", "outlook.timezone=\"Romance Standard Time\"");
            //restRequest.AddHeader("Prefer", "outlook.body-content-type=\"text\"");

            restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{rabbitMQEvent.OrganiserId}/events");
            var response = restClient.Post(restRequest);

            Console.WriteLine(response.StatusCode);
        }
    }
}
