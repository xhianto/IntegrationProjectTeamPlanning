using System;
using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Events;
using Office365Service.Models;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RabbitMQConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            List<OfficeCalendar> events = new List<OfficeCalendar>();
            Uri rabbitMQUri = new Uri(Constant.RabbitMQConnectionUrl);
            string queueName = Constant.RabbitMQQueueName;

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
                var json = Encoding.UTF8.GetString(message);
                Console.WriteLine(json);
                events = JsonConvert.DeserializeObject<List<OfficeCalendar>>(json);
                foreach (var ev in events)
                {
                    Console.WriteLine(ev.Email);
                    Console.WriteLine(ev.Subject);
                    Console.WriteLine(ev.Start.DateTime);
                    Console.WriteLine(ev.Start.TimeZone);
                    Console.WriteLine(ev.End.DateTime);
                    Console.WriteLine(ev.End.TimeZone);
                }
            };

            channel.BasicConsume(queueName, true, consumer);
            Console.ReadLine();
        }
    }
}
