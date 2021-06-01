using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using Office365Service;
using Office365Service.Models;
using RabbitMQ.Client;

namespace RabbitMQHeartBeatProducer
{
    public class HeartBeat
    {
        public void sendHeartBeat(object source, ElapsedEventArgs e)
        {
            Uri rabbitMQUri = new Uri(Constant.RabbitMQConnectionUrl);
            Services services = new Services();

            var factory = new ConnectionFactory
            {
                Uri = rabbitMQUri
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            
            string xmlString = services.getHeartBeat();
            Console.WriteLine(xmlString);
            Console.WriteLine("Dit is een extra lijn");
            var xml = Encoding.UTF8.GetBytes(xmlString);
            channel.BasicPublish("", Constant.RabbitMQHeartBeatName, null, xml);
        }
    }
}
