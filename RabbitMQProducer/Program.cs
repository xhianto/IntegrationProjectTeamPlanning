using System;
using System.Collections.Generic;
using Office365Service;
using Office365Service.Models;
using RestSharp;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

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
            List<OfficeCalendar> events = new List<OfficeCalendar>();
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
                e.Email = email;
                Console.WriteLine(e.Email);
                Console.WriteLine(e.Subject);
                Console.WriteLine(e.Start.DateTime);
                Console.WriteLine(e.Start.TimeZone);
                Console.WriteLine(e.End.DateTime);
                Console.WriteLine(e.End.TimeZone);
            }

            Uri rabbitMQUri = new Uri(Constant.RabbitMQConnectionUrl);
            string queueName = Constant.RabbitMQQueueName;
            var message = events;

            var factory = new ConnectionFactory
            {
                Uri = rabbitMQUri
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(
                queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            //dit alleen even veranderen in een xml
            var json = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            channel.BasicPublish("", queueName, null, json);
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
    }
}
