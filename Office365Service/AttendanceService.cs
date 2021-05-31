using System;
using System.Collections.Generic;
using System.Text;
using Office365Service.Models;
using RestSharp;
using Newtonsoft.Json;
using System.Linq;

/// <summary>
/// Service handling attendances to events in the MS Graph API
/// </summary>
namespace Office365Service
{
    public class AttendanceService
    {
        Services services = new Services();
        MasterDBServices masterDBServices = new MasterDBServices();
        Token BearerToken = new Token();

        public void AttendanceCreate(RabbitMQAttendance rabbitMQAttendance)
        {
            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();

            CalendarAttendees attendees = new CalendarAttendees();
            CalendarAttendee attendee = new CalendarAttendee();
            Master masterOrganiser = masterDBServices.GetGraphIdFromMUUID(rabbitMQAttendance.UUID);
            Master masterUserId = masterDBServices.GetGraphIdFromMUUID(rabbitMQAttendance.UserId);
            Master masterEventId = masterDBServices.GetGraphIdFromMUUID(rabbitMQAttendance.EventId);
            //graph attendees ophalen uit event in graph
            BearerToken = services.RefreshAccesToken();
            restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{masterOrganiser.SourceEntityId}/events/{masterEventId.SourceEntityId}");
            restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
            var response = restClient.Get(restRequest);

            attendees = JsonConvert.DeserializeObject<CalendarAttendees>(response.Content);
            //graph user ophalen en in attendee lijst attendees steken
            User user = services.GetUserFromUUID(masterUserId.SourceEntityId);
            attendee.EmailAddress.Name = user.DisplayName;
            attendee.EmailAddress.Address = user.UserPrincipalName;
            attendee.Type = "required";
            attendees.Attendees.Add(attendee);

            var json = JsonConvert.SerializeObject(attendees);
            
            restRequest.AddJsonBody(json);
            Console.WriteLine(json);

            response = restClient.Patch(restRequest);

            Console.WriteLine(response.StatusCode);

        }



        /// <summary>
        /// Method posting an incoming delete of a user into the MS Graph API.
        /// </summary>
        /// <param name="rabbitMQUser">Deleted user sent by the RabbitMQ message broker</param>
        public void AttendanceDelete(RabbitMQAttendance rabbitMQAttendance)
        {
            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();

            CalendarAttendees attendees = new CalendarAttendees();
            Master masterOrganiser = masterDBServices.GetGraphIdFromMUUID(rabbitMQAttendance.UUID);
            Master masterUserId = masterDBServices.GetGraphIdFromMUUID(rabbitMQAttendance.UserId);
            Master masterEventId = masterDBServices.GetGraphIdFromMUUID(rabbitMQAttendance.EventId);
            //graph attendees ophalen uit event in graph
            BearerToken = services.RefreshAccesToken();
            restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{masterOrganiser.SourceEntityId}/events/{masterEventId.SourceEntityId}");
            restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
            var response = restClient.Get(restRequest);

            attendees = JsonConvert.DeserializeObject<CalendarAttendees>(response.Content);
            string email = services.GetEmailFromUUID(masterUserId.SourceEntityId);
            attendees.Attendees = (from attendee in attendees.Attendees
                                   where attendee.EmailAddress.Address != email
                                   select attendee).ToList();
  
            var json = JsonConvert.SerializeObject(attendees);

            restRequest.AddJsonBody(json);
            Console.WriteLine(json);

            response = restClient.Patch(restRequest);

            Console.WriteLine(response.StatusCode);
        }
    }
}
