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

        public string ConvertCalendarEventToRabbitMQEvent(CalendarEvent calendarEvent, string uuid)
        {
            
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
            rabbitMQEvent.Location = straat + "%" + calendarEvent.Location.Address.City + "%" + calendarEvent.Location.Address.PostalCode;
            return ConvertObjectToXML(rabbitMQEvent);
        }

        public string ConvertObjectToXML<T>(T obj)
        {
            string xml = "";
            try
            {
                MemoryStream mStream = new MemoryStream();
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.UTF8);
                writer.Formatting = System.Xml.Formatting.Indented;
                serializer.Serialize(writer, obj);
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
            calendarEvent.Start = new CalendarEventTimeZone();
            if (rabbitMQEvent.Header.Source.ToLower() != "canvas")
                calendarEvent.Start.DateTime = DateTime.Parse(rabbitMQEvent.Start.ToString());
            else
                calendarEvent.Start.DateTime = DateTime.ParseExact(rabbitMQEvent.Start.ToString(), "d/MM/yyyy H:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            calendarEvent.Start.Zone = "Romance Standard Time";
            calendarEvent.End = new CalendarEventTimeZone();
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
            calendarEvent.Location = new CalendarEventLocation();
            calendarEvent.Location.Address = new CalendarEventLocationAddress();
            //calendarEvent.Location.DisplayName = rabbitMQEvent.Location;
            string[] location = rabbitMQEvent.Location.Split('%');
            calendarEvent.Location.Address.Street = location[0] + " " + location[1] + " " + location[2];
            calendarEvent.Location.Address.City = location[3];
            calendarEvent.Location.Address.PostalCode = location[4];
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
        public string getHeartBeat()
        {
            RabbitMQHeartBeat heartBeat = new RabbitMQHeartBeat();
            heartBeat.Header = new RabbitMQHeartBeatHeader();
            heartBeat.Header.Status = "ONLINE";
            heartBeat.Header.Source = "PLANNING";
            heartBeat.TimeStamp = DateTime.Now;

            return ConvertObjectToXML(heartBeat);
        }
        public string ConvertUserToRabbitMQUser(User user)
        {
            RabbitMQUser rabbitMQUser = new RabbitMQUser();
            rabbitMQUser.Header = new RabbitMQHeader();

            rabbitMQUser.Header.Method = "CREATE";
            rabbitMQUser.Header.Source = "PLANNING";
            rabbitMQUser.UUID = new Guid(user.Id);
            rabbitMQUser.EntityVersion = 1;
            rabbitMQUser.LastName = user.SurName;
            rabbitMQUser.FirstName = user.GivenName;
            rabbitMQUser.EmailAddress = user.UserPrincipalName;
            rabbitMQUser.Role = "Student";
            return ConvertObjectToXML(rabbitMQUser);
        }
        public void UserPost(RabbitMQUser rabbitMQUser)
        {
            rabbitMQUser = new RabbitMQUser();
            rabbitMQUser.FirstName = "Karel";
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
    }
}
