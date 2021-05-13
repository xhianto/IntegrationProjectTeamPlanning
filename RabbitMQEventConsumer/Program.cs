using System;
using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;
using Office365Service.Models;
using Office365Service;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.IO;
using RestSharp;

namespace RabbitMQConsumer
{
    class Program
    {
        private static Services OfficeService = new Services();
        static void Main(string[] args)
        {
            List<CalendarEvent> events = new List<CalendarEvent>();
            Uri rabbitMQUri = new Uri(Constant.RabbitMQConnectionUrl);
            string queueName = Constant.RabbitMQEventQueueName;

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

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, e) =>
            {
                var message = e.Body.ToArray();
                var xml = Encoding.UTF8.GetString(message);
                Console.WriteLine(xml);
                //events = JsonConvert.DeserializeObject<List<CalendarEvent>>(xml);
                //foreach (var ev in events)
                //{
                //    Console.WriteLine(ev.Email);
                //    Console.WriteLine(ev.Subject);
                //    Console.WriteLine(ev.Start.DateTime);
                //    Console.WriteLine(ev.Start.Zone);
                //    Console.WriteLine(ev.End.DateTime);
                //    Console.WriteLine(ev.End.Zone);
                //}
                //int hulp = xml.IndexOf("<", 1, xml.Length-1);
                //Console.WriteLine(hulp);
                XmlSerializer serializer = new XmlSerializer(typeof(RabbitMQEvent));
                //xml = xml.Substring(hulp);
                //if (xml.)
                //if (xml.IndexOf("?", 0, 1) == 0)
                //    xml = xml.Substring(1);
                Console.WriteLine(xml);
                //string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                //if (xml.StartsWith(_byteOrderMarkUtf8))
                //    xml = xml.Remove(0, _byteOrderMarkUtf8.Length);
                using (TextReader reader = new StringReader(xml))
                {
                    RabbitMQEvent result = (RabbitMQEvent)serializer.Deserialize(reader);
                    //result.uuid = "e768646c-eaf9-4f82-99ce-0a49736deef7";
                    Console.WriteLine(result.Header.Method);
                    Console.WriteLine(result.Header.Source);
                    Console.WriteLine(result.UUID);
                    Console.WriteLine(result.EntityVersion);
                    Console.WriteLine(result.Title);
                    Console.WriteLine(result.OrganiserId);
                    Console.WriteLine(result.Description);
                    Console.WriteLine(result.Start);
                    Console.WriteLine(result.End);
                    //Console.WriteLine(result.location);

                    if (result.Header.Method.ToLower() == "create" && result.Header.Source.ToLower() == "planning")
                    {
                        OfficeService.Post(result);
                    }
                }
            };

            channel.BasicConsume(queueName, true, consumer);
            Console.ReadLine();
        }




    }
}
