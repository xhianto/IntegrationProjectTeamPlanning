using System;
using Office365Service;
using Office365Service.Models;
using System.Collections.Generic;
using RestSharp;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace RabbitMQUserProducer
{
    /// <summary>
    /// Method to publish MS Graph API users to the RabbitMQ Message Broker
    /// </summary>
    class Program
    {
        private static string email = "nestorw@ipwt3.onmicrosoft.com";
        private static string uuid = "";

        /* --- Instatiate the Services --- */
        private static Services OfficeService = new Services();

        /* --- Add a valid MS Graph API acces token to the Services --- */
        private static Token BearerToken = OfficeService.RefreshAccesToken();


        /// <summary>
        /// Main method for publishing MS Graph API users to the RabbitMQ Message Broker
        /// </summary>
        static void Main(string[] args)
        {
            /* --- Retrieve the MS Graph UUID corresponding to the given emailaddress --- */
            uuid = OfficeService.GetUUIDFromEmail(email);

            //List<CalendarEvent> events = new List<CalendarEvent>();
            Console.WriteLine(BearerToken.Access_token);


            User user = new User();

            /* --- prepare the restclient --- */
            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();
            OfficeService.SetRestRequestHeader(restRequest);
            //restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
            //restRequest.AddHeader("Prefer", "outlook.timezone=\"Romance Standard Time\"");
            //restRequest.AddHeader("Prefer", "outlook.body-content-type=\"text\"");
            restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{uuid}");


            /* --- Perform the GET rest request and place response in a User --- */
            var response = restClient.Get(restRequest);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine(response.Content);
                user = JsonConvert.DeserializeObject<User>(response.Content);
                //events = jsonFromGraph.Value;
            }


            //test
            //foreach (var e in events)
            //{
            //    Console.WriteLine(email);
            //    Console.WriteLine(uuid);
            //    Console.WriteLine(e.Subject);
            //    Console.WriteLine(e.Start.DateTime);
            //    Console.WriteLine(e.Start.Zone);
            //    Console.WriteLine(e.End.DateTime);
            //    Console.WriteLine(e.End.Zone);
            //}
            Console.WriteLine(user.Id);
            Console.WriteLine(user.GivenName);
            Console.WriteLine(user.SurName);
            Console.WriteLine(user.UserPrincipalName);


            /* --- prepare a RabbitMQ connection model the User --- */
            Uri rabbitMQUri = new Uri(Constant.RabbitMQConnectionUrl);
            var factory = new ConnectionFactory
            {
                Uri = rabbitMQUri
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            /* --- convert it to an XML string for processing by RabbitMQ message broker --- */
            string xmlString = OfficeService.ConvertUserToRabbitMQUser(user);
            Console.WriteLine(xmlString);
            var xml = Encoding.UTF8.GetBytes(xmlString);

            /* --- Publish the XML string to the RabbitMQ connection --- */
            channel.BasicPublish(Constant.RabbitMQUserExchangeName, "", null, xml);
        }
    }
}
