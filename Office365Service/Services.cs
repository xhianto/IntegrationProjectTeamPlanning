using Newtonsoft.Json;
using Office365Service.Models;
using RestSharp;
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Office365Service
{
    /// <summary>
    /// Services supporting the exchange of data between MS Graph API and RabbitMQ message broker.
    /// </summary>
    public class Services
    {

        /* --- GENERAL & HELPER SERVICES --- */

        public Token BearerToken = new Token();
        private MasterDBServices masterDBService = new MasterDBServices();

        /// <summary>
        /// Method for getting and staying authenticated on MS
        /// Will refresh 2 minutes before expiring
        /// </summary>
        /// <returns>a valid BearerToken</returns>
        public Token RefreshAccesToken()
        {
            if (BearerToken.Access_token == null || BearerToken.TimeStamp.AddSeconds(BearerToken.Expires_in - 120) < DateTime.Now) //refresh token 2 min voor expire tijd
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
                        BearerToken.TimeStamp = DateTime.Now;
                        Console.WriteLine(response.Content);
                    }
                }
            }
            return BearerToken;
        }

        /// <summary>
        /// Method for setting the header of rest requests
        /// Uses the BearerToken for authorization and sets the timezone to Roman Standard Time and the type to text
        /// </summary>
        /// <param name="restRequest">a rest request authorized to call the MS Graph API</param>
        public void SetRestRequestHeader(RestRequest restRequest)
        {
            restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
            restRequest.AddHeader("Prefer", "outlook.timezone=\"Romance Standard Time\"");
            restRequest.AddHeader("Prefer", "outlook.body-content-type=\"text\"");
        }

        /// <summary>
        /// Method to search the MSGraph API for the 'local' UUID (EntitySource_Id), given the emailaddress
        /// </summary>
        /// <param name="email">the emailaddress of the user being searched</param>
        /// <returns>the 'local' UUID of the user (in MS AD) being searched</returns>
        public string GetUUIDFromEmail(string email)
        {
            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();
            string useruuid = "";

            SetRestRequestHeader(restRequest);

            restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{email}");
            var response = restClient.Get(restRequest);

            // if the request is succesfully executed by the MS Graph API
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine(response.Content);
                // convert the JSON response from the MS Graph API to a User instance 
                User user = JsonConvert.DeserializeObject<User>(response.Content);

                useruuid = user.Id;
            }
            return useruuid;
        }

        /// <summary>
        /// Method to search the MSGraph API for the emailaddress, given the local UUID
        /// </summary>
        /// <param name="uuid">the 'local' UUID of the user (in MS AD) being searched</param>
        /// <returns>the emailaddress of the user being searched</returns>
        public string GetEmailFromUUID(string uuid)
        {
            string email = "";
            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();
            BearerToken = RefreshAccesToken();

            SetRestRequestHeader(restRequest);

            restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{uuid}");
            var response = restClient.Get(restRequest);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //Console.WriteLine(response.Content);

                // convert the JSON response from the MS Graph API to a User instance 
                User user = JsonConvert.DeserializeObject<User>(response.Content);

                email = user.UserPrincipalName;
            }
            return email;
        }

        public User GetUserFromUUID(string uuid)
        {
            User user = new User();
            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();
            BearerToken = RefreshAccesToken();

            SetRestRequestHeader(restRequest);

            restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{uuid}");
            var response = restClient.Get(restRequest);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //Console.WriteLine(response.Content);

                // convert the JSON response from the MS Graph API to a User instance 
                user = JsonConvert.DeserializeObject<User>(response.Content);
            }
            return user;
        }

        //public CalendarEvent GetEventFromUUID(string uuid)
        //{
        //    CalendarEvent calendarEvent = new CalendarEvent();
        //    RestClient restClient = new RestClient();
        //    RestRequest restRequest = new RestRequest();
        //    BearerToken = RefreshAccesToken();

        //    SetRestRequestHeader(restRequest);

        //    restClient.
        //}

        /// <summary>
        /// Method for converting any type of object to an xml string
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="obj">Generic object</param>
        /// <returns></returns>
        public string ConvertObjectToXML<T>(T obj)
        {
            string xml = "";
            try
            {
                MemoryStream mStream = new MemoryStream();
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                XmlTextWriter writer = new XmlTextWriter(mStream, null);
                writer.Formatting = System.Xml.Formatting.Indented;
                serializer.Serialize(writer, obj);
                xml = Encoding.UTF8.GetString(mStream.ToArray());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return xml;
        }



        /* --- EVENT RELATED SERVICES --- */


        /// <summary>
        /// Method to convert MS Graph Calendar data so the message broker RabbitMQ can handle them. 
        /// </summary>
        /// <param name="calendarEvent">a calendar event with attributes of MS Graph API</param>
        /// <param name="uuid">the UUID of the event in MS Graph API</param>
        /// <returns>event in xml format, ready to send to the RabbitMQ message queue</returns>
        public string ConvertCalendarEventToRabbitMQEvent(CalendarEvent calendarEvent, string uuid)
        {

            RabbitMQEvent rabbitMQEvent = new RabbitMQEvent();
            //rabbitMQEvent.Header = new RabbitMQHeader();
            rabbitMQEvent.Header.Method = XMLMethod.CREATE;
            //rabbitMQEvent.Header.Source = XMLSource.PLANNING;
            rabbitMQEvent.Header.Source = XMLSource.FRONTEND; //tester
            //rabbitMQEvent.UUID = new Guid(uuid);
            rabbitMQEvent.UUID = new Guid("f9daab25-bccd-11eb-b876-00155d110504"); //tester
            rabbitMQEvent.EntityVersion = 1;
            rabbitMQEvent.Title = calendarEvent.Subject;
            rabbitMQEvent.OrganiserId = new Guid(uuid);
            rabbitMQEvent.Description = "Komt dit door?";
            rabbitMQEvent.Start = calendarEvent.Start.DateTime;
            rabbitMQEvent.End = calendarEvent.End.DateTime;

            string straat = ConcatStreetNrAndBus(calendarEvent);
            rabbitMQEvent.Location = straat + "%" + calendarEvent.Location.Address.City + "%" + calendarEvent.Location.Address.PostalCode;
            return ConvertObjectToXML(rabbitMQEvent);
        }


        /// <summary>
        /// Method for concatting street, number and bus (if present) into one large '&'-seprated string (format: 'street&nr&bus&')
        /// </summary>
        /// <param name="calendarEvent">a calendar event with attributes of MS Graph API</param>
        /// <returns>one large '&'-seprated string ('street&nr&bus&')</returns>
        private static string ConcatStreetNrAndBus(CalendarEvent calendarEvent)
        {
            string straat = "";
            string[] hulp = calendarEvent.Location.Address.Street.Split(' ');
            if (hulp.Length == 2)
            {
                straat = hulp[0] + "%" + hulp[1] + "%";
            }
            else
            {
                if (int.TryParse(hulp[hulp.Length - 2], out _))
                {
                    string bus = hulp[hulp.Length - 1];
                    string huisnummer = hulp[hulp.Length - 2];
                    for (int i = 0; i < hulp.Length - 3; i++)
                    {
                        straat += hulp[i] + " ";
                    }
                    straat += hulp[hulp.Length - 3] + "%" + huisnummer + "%" + bus;
                }
                else
                {
                    string huisnummer = hulp[hulp.Length - 1];
                    for (int i = 0; i < hulp.Length - 2; i++)
                    {
                        straat += hulp[i] + " ";
                    }
                    straat += hulp[hulp.Length - 2] + "%" + huisnummer + "%";
                }
            }

            return straat;
        }




        /* --- USER RELATED SERVICES --- */

        /// <summary>
        /// Method to convert MS Graph User data so the message broker RabbitMQ can handle them. 
        /// </summary>
        /// <param name="user">a user with attributes of MS Graph API</param>
        /// <returns>user in xml format, ready to send to the RabbitMQ message queue</returns>
        public string ConvertUserToRabbitMQUser(User user)
        {
            /* ---  --- */
            RabbitMQUser rabbitMQUser = new RabbitMQUser();
            //rabbitMQUser.Header = new RabbitMQHeader();

            //rabbitMQUser.Header.Method = XMLMethod.DELETE;
            rabbitMQUser.Header.Method = XMLMethod.CREATE;
            rabbitMQUser.Header.Source = XMLSource.PLANNING;
            //rabbitMQUser.UUID = new Guid("42c5fb9e-a9db-47f9-af38-297cb7b88654");
            rabbitMQUser.UUID = new Guid(user.Id);
            rabbitMQUser.EntityVersion = 1;
            rabbitMQUser.LastName = user.SurName;
            rabbitMQUser.FirstName = user.GivenName;
            rabbitMQUser.EmailAddress = user.UserPrincipalName;
            rabbitMQUser.Role = "Student";
            return ConvertObjectToXML(rabbitMQUser);
        }

        


        /* --- MONITORING RELATED SERVICES --- */

        /// <summary>
        /// Method to generate a system heartbeat for monitoring purposes
        /// </summary>
        /// <returns>the system heartbeat in XML format</returns>
        public string getHeartBeat()
        {
            RabbitMQHeartBeat heartBeat = new RabbitMQHeartBeat();
            //heartBeat.Header = new RabbitMQHeartBeatHeader();
            heartBeat.Header.Status = XMLStatus.ONLINE;
            heartBeat.Header.Source = XMLSource.PLANNING;
            heartBeat.TimeStamp = DateTime.Now;
            Console.Write("Heartbeat made at: " + heartBeat.TimeStamp + ": ");
            return ConvertObjectToXML(heartBeat);
        }

        public void test()
        {
            var service = new MasterDBServices();
            //XMLSource source = XMLSource.FRONTEND;
            //Guid uuid = new Guid("f9daab25-bccd-11eb-b876-00155d110504");
            //string uuid = new Guid("a3ec8291-bcd1-11eb-b876-00155d110504");
            string uuid = service.GetMUUID().ToString();
            service.CreateEntity(new Guid(uuid), "Test", "EVENT");
            //masterDBService.GetGraphIdFromMUUID(uuid);
        }

        public bool XSDValidatie(string xml, string xsd) //geef xml string en welk xsd bestand je wilt gebruiken bvb "event.xsd"
        {
            XmlSchemaSet xmlSchema = new XmlSchemaSet();
            xmlSchema.Add("", Environment.CurrentDirectory + "/XMLvalidations/" + xsd + ".xsd");
            bool xmlValidation = true;

            XDocument doc = XDocument.Parse(xml);

            doc.Validate(xmlSchema, (sender, args) =>
            {
                Console.WriteLine("Error Message: " + args.Message);
                xmlValidation = false;
            });
            Console.WriteLine("XmlValidatie voor " + xsd + ": " + xmlValidation);
            return xmlValidation;
        }
    }
}  /* ---  --- */
