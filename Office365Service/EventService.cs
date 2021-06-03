using Newtonsoft.Json;
using Office365Service.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Office365Service
{
    public class EventService
    {
        Services services = new Services();
        MasterDBServices masterDBService = new MasterDBServices();
        Token BearerToken = new Token();

        /// <summary>
        /// Method posting an incoming new event into the MS Graph API.
        /// </summary>
        /// <param name="rabbitMQEvent">New event sent by the RabbitMQ message broker</param>
        public void EventCreate(RabbitMQEvent rabbitMQEvent)
        {
            Master masterUserId = masterDBService.GetGraphIdFromMUUID(rabbitMQEvent.OrganiserId);
            if (masterUserId != null)
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
                calendarEvent.Organizer.EmailAddress.Address = services.GetEmailFromUUID(masterUserId.SourceEntityId);
                //calendarEvent.Location = new CalendarEventLocation();
                //calendarEvent.Location.Address = new CalendarEventLocationAddress();
                //calendarEvent.Location.DisplayName = rabbitMQEvent.Location;
                string[] location = rabbitMQEvent.Location.Split('%');
                calendarEvent.Location.Address.Street = location[0] + " " + location[1] + " " + location[2];
                calendarEvent.Location.Address.City = location[3];
                calendarEvent.Location.Address.PostalCode = location[4];


                /* --- Retrieve a valid accestoken for creating the event in the MS Graph API --- */
                BearerToken = services.RefreshAccesToken();

                /* --- Serialize the event into json and attach it to the rest request --- */
                var json = JsonConvert.SerializeObject(calendarEvent);
                restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
                restRequest.AddJsonBody(json);

                /* --- test --- */
                Console.WriteLine(json);
                //restRequest.AddHeader("Prefer", "outlook.timezone=\"Romance Standard Time\"");
                //restRequest.AddHeader("Prefer", "outlook.body-content-type=\"text\"");


                /* --- execute the rest request to post the new event in the MS Graph API --- */
                restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{masterUserId.SourceEntityId}/events");
                var response = restClient.Post(restRequest);

                Console.WriteLine(response.StatusCode);

                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    var responsJson = response.Content;
                    CalendarEvent responseEvent = JsonConvert.DeserializeObject<CalendarEvent>(responsJson);
                    Console.WriteLine(responseEvent.Id);
                    masterDBService.CreateEntity(rabbitMQEvent.UUID, responseEvent.Id, "Event");
                }
            }
        }


        /// <summary>
        /// Method posting an incoming change of an event into the MS Graph API.
        /// </summary>
        /// <param name="rabbitMQEvent">Updated event sent by the RabbitMQ message broker</param>
        public void EventUpdate(RabbitMQEvent rabbitMQEvent)
        {
            //Console.WriteLine("Update van Event nog niet klaar!");
            Master masterUserId = masterDBService.GetGraphIdFromMUUID(rabbitMQEvent.OrganiserId);
            Master masterEventId = masterDBService.GetGraphIdFromMUUID(rabbitMQEvent.UUID);

            if (masterEventId != null && masterUserId != null /*&& masterDBService.CheckSourceEntityVersionIsHigher(rabbitMQEvent.UUID, rabbitMQEvent.Header.Source)*/)
            {
                RestClient restClient = new RestClient();
                RestRequest restRequest = new RestRequest();

                CalendarEvent calendarEvent = new CalendarEvent();
                calendarEvent.Subject = rabbitMQEvent.Title;

                calendarEvent.Start.DateTime = DateTime.Parse(rabbitMQEvent.Start.ToString());
                calendarEvent.Start.Zone = "Romance Standard Time";
                calendarEvent.End.DateTime = DateTime.Parse(rabbitMQEvent.End.ToString());
                calendarEvent.End.Zone = "Romance Standard Time";
                calendarEvent.Body.ContentType = "text";
                calendarEvent.Body.Content = rabbitMQEvent.Description;
                calendarEvent.Organizer.EmailAddress.Address = rabbitMQEvent.OrganiserId.ToString();
                string[] location = rabbitMQEvent.Location.Split('%');
                calendarEvent.Location.Address.Street = location[0] + " " + location[1] + " " + location[2];
                calendarEvent.Location.Address.City = location[3];
                calendarEvent.Location.Address.PostalCode = location[4];

                /* --- Retrieve a valid accestoken for creating the event in the MS Graph API --- */
                BearerToken = services.RefreshAccesToken();

                /* --- Serialize the event into json and attach it to the rest request --- */
                var json = JsonConvert.SerializeObject(calendarEvent);

                restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
                restRequest.AddJsonBody(json);

                /* --- execute the rest request to post the new event in the MS Graph API --- */
                restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{masterUserId.SourceEntityId}/events/{masterEventId.SourceEntityId}");
                var response = restClient.Patch(restRequest);

                Console.WriteLine(response.StatusCode);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    masterDBService.ChangeEntityVersion(rabbitMQEvent.UUID);
                }
            }
        }


        /// <summary>
        /// Method posting an incoming delete of an event into the MS Graph API.
        /// </summary>
        /// <param name="rabbitMQEvent">Deleted event sent by the RabbitMQ message broker</param>
        public void EventDelete(RabbitMQEvent rabbitMQEvent)
        {
            //Console.WriteLine("Delete van Event nog niet klaar!");
            Master masterUserId = masterDBService.GetGraphIdFromMUUID(rabbitMQEvent.OrganiserId);
            Master masterEventId = masterDBService.GetGraphIdFromMUUID(rabbitMQEvent.UUID);

            if (masterEventId != null && masterUserId != null && masterDBService.CheckSourceEntityVersionIsHigher(rabbitMQEvent.UUID, rabbitMQEvent.Header.Source))
            {
                RestClient restClient = new RestClient();
                RestRequest restRequest = new RestRequest();
                BearerToken = services.RefreshAccesToken();

                restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
                Console.WriteLine(rabbitMQEvent.UUID);
                restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{masterUserId.SourceEntityId}/events/{masterEventId.SourceEntityId}");
                var response = restClient.Delete(restRequest);

                Console.WriteLine(response.StatusCode);
            }
        }
    }
}
