using System;
using System.Collections.Generic;
using Office365Service;
using Office365Service.Models;
using RestSharp;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using System.IO;
using System.Xml;


namespace RabbitMQProducer
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
            List<CalendarEvent> events = new List<CalendarEvent>();
            Console.WriteLine(BearerToken.Access_token);


            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();

            Services.SetRestRequestHeader(restRequest);
            //restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
            //restRequest.AddHeader("Prefer", "outlook.timezone=\"Romance Standard Time\"");
            //restRequest.AddHeader("Prefer", "outlook.body-content-type=\"text\"");
            

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
                Console.WriteLine(email);
                Console.WriteLine(uuid);
                Console.WriteLine(e.Subject);
                Console.WriteLine(e.Start.DateTime);
                Console.WriteLine(e.Start.Zone);
                Console.WriteLine(e.End.DateTime);
                Console.WriteLine(e.End.Zone);
            }

            Uri rabbitMQUri = new Uri(Constant.RabbitMQConnectionUrl);
            var message = events;

            foreach (CalendarEvent e in events)
            {
                var factory = new ConnectionFactory
                {
                    Uri = rabbitMQUri
                };
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();
                //channel.QueueDeclare(
                //    "",
                //    durable: true,
                //    exclusive: false,
                //    autoDelete: false,
                //    arguments: null);
                //dit alleen even veranderen in een xml
                string xmlString = OfficeService.ConvertCalendarEventToRabbitMQEvent(e, uuid);
                Console.WriteLine(xmlString);
                var xml = Encoding.UTF8.GetBytes(xmlString);
                channel.BasicPublish(Constant.RabbitMQEventExchangeName, "", null, xml);   //to-canvas_event-queue
                //  channel.BasicPublish("wt3.event-exchange", "to-frontend_event-queue", null, json);
                //  channel.BasicPublish("wt3.event-exchange", "to-monitoring_event-queue", null, json);
            }
        }
    }
}

