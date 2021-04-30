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
        static void Main(string[] args)
        {
            List<OfficeCalendar> events = new List<OfficeCalendar>();
            string email = "docent@ipwt3.onmicrosoft.com";
            Services OfficeService = new Services();
            Token BearerToken = new Token();
            BearerToken = OfficeService.RefreshAccesToken();
            Console.WriteLine(BearerToken.access_token);

            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();

            restRequest.AddHeader("Authorization", BearerToken.token_type + " " + BearerToken.access_token);
            restRequest.AddHeader("Prefer", "outlook.timezone=\"Romance Standard Time\"");
            restRequest.AddHeader("Prefer", "outlook.body-content-type=\"text\"");

            restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{email}/calendar/events");
            var response = restClient.Get(restRequest);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine(response.Content);
                Response jsonFromGraph = JsonConvert.DeserializeObject<Response>(response.Content);
                events = jsonFromGraph.value;
            }
            foreach (var e in events)
            {
                e.email = email;
                Console.WriteLine(e.email);
                Console.WriteLine(e.subject);
                Console.WriteLine(e.start.dateTime);
                Console.WriteLine(e.start.timeZone);
                Console.WriteLine(e.end.dateTime);
                Console.WriteLine(e.end.timeZone);
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
            var json = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            // enter exchange name as first parameter if it exists
            channel.BasicPublish("wt3.event-exchange", queueName, null, json);
        }
    }
}
