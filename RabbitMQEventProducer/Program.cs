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
    /// <summary>
    /// Method to publish MS Graph API calendar events to the RabbitMQ Message Broker
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
        /// Main method for publishing MS Graph API calendar events to the RabbitMQ Message Broker
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            /* --- Retrieve the MS Graph UUID corresponding to the given emailaddress --- */
            uuid = OfficeService.GetUUIDFromEmail(email);


            List<CalendarEvent> events = new List<CalendarEvent>();
            Console.WriteLine(BearerToken.Access_token);

            /* --- prepare the restclient --- */
            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();
            OfficeService.SetRestRequestHeader(restRequest);
            //restRequest.AddHeader("Authorization", BearerToken.Token_type + " " + BearerToken.Access_token);
            //restRequest.AddHeader("Prefer", "outlook.timezone=\"Romance Standard Time\"");
            //restRequest.AddHeader("Prefer", "outlook.body-content-type=\"text\"");
            restClient.BaseUrl = new Uri($"https://graph.microsoft.com/v1.0/users/{uuid}/calendar/events");

            /* --- Perform the rest request and place response in list of Calendar Events --- */
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

            /* --- for every Calendar Event --- */
            foreach (CalendarEvent e in events)
            {
                /* --- prepare a RabbitMQ connection model for every Calendar Event --- */
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
                /* --- convert it to an XML string for processing by RabbitMQ message broker --- */
                string xmlString = OfficeService.ConvertCalendarEventToRabbitMQEvent(e, uuid);
                Console.WriteLine(xmlString); 
                var xml = Encoding.UTF8.GetBytes(xmlString);

                /* --- Publish the XML string to the RabbitMQ connection --- */
                channel.BasicPublish(Constant.RabbitMQEventExchangeName, "", null, xml);   //to-canvas_event-queue
                //  channel.BasicPublish("wt3.event-exchange", "to-frontend_event-queue", null, json);
                //  channel.BasicPublish("wt3.event-exchange", "to-monitoring_event-queue", null, json);
            }
        }
    }
}

