using System;
using Office365Service;
using Office365Service.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace RabbitMQUserConsumer
{
    class Program
    {
        private static Services OfficeService = new Services();
        //bearer token wordt niet refreshed, kan dus geen verbinding meer maken met api na 1 uur
        //private static Token BearerToken = OfficeService.RefreshAccesToken();
        static void Main(string[] args)
        {
            Uri rabbitMQUri = new Uri(Constant.RabbitMQConnectionUrl);
            string queueName = Constant.RabbitMQUserQueueName;

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
                XmlSerializer serializer = new XmlSerializer(typeof(RabbitMQUser));
                //xml = xml.Substring(hulp);
                //if (xml.)
                //if (xml.IndexOf("?", 0, 1) == 0)
                //    xml = xml.Substring(1);
                //Console.WriteLine(xml);
                //string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                //if (xml.StartsWith(_byteOrderMarkUtf8))
                //    xml = xml.Remove(0, _byteOrderMarkUtf8.Length);
                using (TextReader reader = new StringReader(xml))
                {
                    RabbitMQUser result = (RabbitMQUser)serializer.Deserialize(reader);
                    //result.uuid = "e768646c-eaf9-4f82-99ce-0a49736deef7";
                    Console.WriteLine(result.Header.Method);
                    Console.WriteLine(result.Header.Source);
                    Console.WriteLine(result.UUID);
                    Console.WriteLine(result.EntityVersion);
                    Console.WriteLine(result.LastName);
                    Console.WriteLine(result.FirstName);
                    Console.WriteLine(result.EmailAddress);
                    Console.WriteLine(result.Role);
                    //Console.WriteLine(result.location);

                    if (result.Header.Method.ToLower() == "create" && result.Header.Source.ToLower() == "planning")
                    {
                        OfficeService.UserPost(result);
                    }
                }
            };

            channel.BasicConsume(queueName, true, consumer);
            Console.ReadLine();
        }
    }
}
