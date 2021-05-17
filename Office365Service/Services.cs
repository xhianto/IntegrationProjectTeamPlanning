using Newtonsoft.Json;
using Office365Service.Models;
using RestSharp;
using System;
using System.IO;
using System.Text;
using System.Xml;
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
        public static void SetRestRequestHeader(RestRequest restRequest)
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
            rabbitMQEvent.Header.Source = XMLSource.PLANNING;
            rabbitMQEvent.UUID = new Guid(uuid);
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


        /// <summary>
        /// Method posting an incoming new event into the MS Graph API.
        /// </summary>
        /// <param name="rabbitMQEvent">New event sent by the RabbitMQ message broker</param>
        public void EventCreate(RabbitMQEvent rabbitMQEvent)
        {
              
            /* --- prepare the restclient --- */
            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();
            

            /* --- create an MS Graph CalendarEvent and fill the properties with the corresponding values from the received RabbitMQEvent --- */
            CalendarEvent calendarEvent = new CalendarEvent();
            calendarEvent.Subject = rabbitMQEvent.Title;
            //calendarEvent.Start = new CalendarEventTimeZone();

            //if (rabbitMQEvent.Header.Source.ToLower() != "canvas")
            calendarEvent.Start.DateTime = DateTime.Parse(rabbitMQEvent.Start.ToString());
            //else
            //    calendarEvent.Start.DateTime = DateTime.ParseExact(rabbitMQEvent.Start.ToString(), "d/MM/yyyy H:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            calendarEvent.Start.Zone = "Romance Standard Time";
            //calendarEvent.End = new CalendarEventTimeZone();
            //if (rabbitMQEvent.Header.Source.ToLower() != "canvas")
            calendarEvent.End.DateTime = DateTime.Parse(rabbitMQEvent.End.ToString());
            //else
            //    calendarEvent.End.DateTime = DateTime.ParseExact(rabbitMQEvent.End.ToString(), "d/MM/yyyy H:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            calendarEvent.End.Zone = "Romance Standard Time";
            //calendarEvent.BodyPreview = rabbitMQEvent.Description;
            //calendarEvent.Body = new CalendarEventBody();
            calendarEvent.Body.ContentType = "text";
            calendarEvent.Body.Content = rabbitMQEvent.Description;
            //calendarEvent.Organizer = new CalendarEventOrganizer();
            //calendarEvent.Organizer.EmailAddress = new CalendarEventEmailAddress();
            calendarEvent.Organizer.EmailAddress.Address = rabbitMQEvent.OrganiserId.ToString();
            //calendarEvent.Location = new CalendarEventLocation();
            //calendarEvent.Location.Address = new CalendarEventLocationAddress();
            //calendarEvent.Location.DisplayName = rabbitMQEvent.Location;
            string[] location = rabbitMQEvent.Location.Split('%');
            calendarEvent.Location.Address.Street = location[0] + " " + location[1] + " " + location[2];
            calendarEvent.Location.Address.City = location[3];
            calendarEvent.Location.Address.PostalCode = location[4];


            /* --- Retrieve a valid accestoken for creating the event in the MS Graph API --- */
            BearerToken = RefreshAccesToken();

            /* --- Serialize the event into json and attach it to the rest request --- */
            var json = JsonConvert.SerializeObject(calendarEvent);
            restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
            restRequest.AddJsonBody(json);

            /* --- test --- */
            Console.WriteLine(json);
            //restRequest.AddHeader("Prefer", "outlook.timezone=\"Romance Standard Time\"");
            //restRequest.AddHeader("Prefer", "outlook.body-content-type=\"text\"");


            /* --- execute the rest request to post the new event in the MS Graph API --- */
            restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{rabbitMQEvent.OrganiserId}/events");
            var response = restClient.Post(restRequest);

            Console.WriteLine(response.StatusCode);
        }


        /// <summary>
        /// Method posting an incoming change of an event into the MS Graph API.
        /// </summary>
        /// <param name="rabbitMQEvent">Updated event sent by the RabbitMQ message broker</param>
        public void EventUpdate(RabbitMQEvent rabbitMQEvent)
        {
            Console.WriteLine("Update van Event nog niet klaar!");
        }


        /// <summary>
        /// Method posting an incoming delete of an event into the MS Graph API.
        /// </summary>
        /// <param name="rabbitMQEvent">Deleted event sent by the RabbitMQ message broker</param>
        public void EventDelete(RabbitMQEvent rabbitMQEvent)
        {
            Console.WriteLine("Delete van Event nog niet klaar!");
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

        /// <summary>
        /// Method posting an incoming new user into the MS Graph API.
        /// </summary>
        /// <param name="rabbitMQUser">New user sent by the RabbitMQ message broker</param>
        public void UserCreate(RabbitMQUser rabbitMQUser)
        {
            //Mock data om nieuw user aan te maken
            rabbitMQUser = new RabbitMQUser();
            rabbitMQUser.FirstName = "Steve";
            rabbitMQUser.LastName = "Herman";
            rabbitMQUser.Role = "Clown";

            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();

            User user = new User();
            //required
            user.AccountEnabled = true;
            user.DisplayName = rabbitMQUser.FirstName + " " + rabbitMQUser.LastName;
            user.MailNickname = rabbitMQUser.FirstName.Replace(' ', '.') + "." + rabbitMQUser.LastName;
            user.UserPrincipalName = user.MailNickname + "@ipwt3.onmicrosoft.com";
            user.PasswordProfile = new UserPasswordProfile();
            user.PasswordProfile.ForceChangePasswordNextSignIn = false;
            user.PasswordProfile.Password = Constant.StandardPassword;

            user.GivenName = rabbitMQUser.FirstName;
            user.SurName = rabbitMQUser.LastName;
            user.JobTitle = rabbitMQUser.Role;

            BearerToken = RefreshAccesToken();

            var json = JsonConvert.SerializeObject(user);
            restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
            restRequest.AddJsonBody(json);
            Console.WriteLine(json);

            restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users");
            var response = restClient.Post(restRequest);

            Console.WriteLine(response.StatusCode);
        }


        /// <summary>
        /// Method posting an incoming change of a user into the MS Graph API.
        /// </summary>
        /// <param name="rabbitMQUser">Updated user sent by the RabbitMQ message broker</param>
        public void UserUpdate(RabbitMQUser rabbitMQUser)
        {
            Console.WriteLine("Update van User nog niet klaar!");
        }


        /// <summary>
        /// Method posting an incoming delete of a user into the MS Graph API.
        /// </summary>
        /// <param name="rabbitMQUser">Deleted user sent by the RabbitMQ message broker</param>
        public void UserDelete(RabbitMQUser rabbitMQUser)
        {
            Console.WriteLine("Delete van User nog niet klaar!");
            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();
            BearerToken = RefreshAccesToken();

            restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
            Console.WriteLine(rabbitMQUser.UUID);
            restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{rabbitMQUser.UUID}");
            var response = restClient.Delete(restRequest);

            Console.WriteLine(response.StatusCode);
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

            return ConvertObjectToXML(heartBeat);
        }

    }
}  /* ---  --- */
