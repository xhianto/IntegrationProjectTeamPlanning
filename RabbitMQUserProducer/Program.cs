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
    class Program
    {
        private static string email = "nestorw@ipwt3.onmicrosoft.com";
        private static string uuid = "";
        private static Services OfficeService = new Services();
        private static Token BearerToken = OfficeService.RefreshAccesToken();

        static void Main(string[] args)
        {
            uuid = OfficeService.GetUUIDFromEmail(email);
            //List<CalendarEvent> events = new List<CalendarEvent>();
            Console.WriteLine(BearerToken.Access_token);
            User user = new User();

            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();

            restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
            restRequest.AddHeader("Prefer", "outlook.timezone=\"Romance Standard Time\"");
            restRequest.AddHeader("Prefer", "outlook.body-content-type=\"text\"");

            restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{uuid}");
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


            Uri rabbitMQUri = new Uri(Constant.RabbitMQConnectionUrl);

            var factory = new ConnectionFactory
            {
                Uri = rabbitMQUri
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            
            string xmlString = OfficeService.ConvertUserToRabbitMQUser(user);
            Console.WriteLine(xmlString);
            var xml = Encoding.UTF8.GetBytes(xmlString);
            channel.BasicPublish(Constant.RabbitMQUserExchangeName, "", null, xml);
        }
    }
}
