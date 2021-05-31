using Office365Service;
using Office365Service.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;
using System.Xml.Serialization;

namespace RabbitMQFinalProduct
{
    class Program
    {
        private static Services OfficeService = new Services();
        static void Main(string[] args)
        {
            HeartBeat heartBeat = new HeartBeat();

            Timer timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(heartBeat.sendHeartBeat);
            timer.Interval = 1000;
            timer.Start();

            List<CalendarEvent> events = new List<CalendarEvent>();
            Uri rabbitMQUri = new Uri(Constant.RabbitMQConnectionUrl);
            string queueEventName = Constant.RabbitMQEventQueueName;
            string queueUserName = Constant.RabbitMQUserQueueName;
            string queueAttendanceName = Constant.RabbitMQAttendanceQueueName;

            var factory = new ConnectionFactory
            {
                Uri = rabbitMQUri
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            var eventConsumer = new EventingBasicConsumer(channel);
            var userConsumer = new EventingBasicConsumer(channel);
            var attendanceConsumer = new EventingBasicConsumer(channel);

            // User
            userConsumer.Received += (sender, e) =>
            {

                var message = e.Body.ToArray();
                var xml = Encoding.UTF8.GetString(message);
                Console.WriteLine(xml);
                if (OfficeService.XSDValidatie(xml, "user"))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(RabbitMQUser));
                    using (TextReader reader = new StringReader(xml))
                    {
                        RabbitMQUser result = (RabbitMQUser)serializer.Deserialize(reader);
                        Console.WriteLine(result.Header.Method);
                        Console.WriteLine(result.Header.Source);
                        Console.WriteLine(result.UUID);
                        Console.WriteLine(result.EntityVersion);
                        Console.WriteLine(result.LastName);
                        Console.WriteLine(result.FirstName);
                        Console.WriteLine(result.EmailAddress);
                        Console.WriteLine(result.Role);
                        if (result.Header.Source != XMLSource.PLANNING)
                        {
                            switch (result.Header.Method)
                            {
                                case XMLMethod.CREATE:
                                    OfficeService.UserCreate(result);
                                    break;
                                case XMLMethod.UPDATE:
                                    OfficeService.UserUpdate(result);
                                    break;
                                case XMLMethod.DELETE:
                                    OfficeService.UserDelete(result);
                                    break;
                            }
                        }
                    }
                }
            };

            //Event
            eventConsumer.Received += (sender, e) =>
            {
                var message = e.Body.ToArray();
                var xml = Encoding.UTF8.GetString(message);
                Console.WriteLine(xml);
                if (OfficeService.XSDValidatie(xml, "event"))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(RabbitMQEvent));
                    using (TextReader reader = new StringReader(xml))
                    {
                        RabbitMQEvent result = (RabbitMQEvent)serializer.Deserialize(reader);
                        Console.WriteLine(result.Header.Method);
                        Console.WriteLine(result.Header.Source);
                        Console.WriteLine(result.UUID);
                        Console.WriteLine(result.EntityVersion);
                        Console.WriteLine(result.Title);
                        Console.WriteLine(result.OrganiserId);
                        Console.WriteLine(result.Description);
                        Console.WriteLine(result.Start);
                        Console.WriteLine(result.End);
                        if (result.Header.Source != XMLSource.PLANNING)
                        {
                            switch (result.Header.Method)
                            {
                                case XMLMethod.CREATE:
                                    OfficeService.EventCreate(result);
                                    break;
                                case XMLMethod.UPDATE:
                                    OfficeService.EventUpdate(result);
                                    break;
                                case XMLMethod.DELETE:
                                    OfficeService.EventDelete(result);
                                    break;
                            }
                        }
                    }
                }
            };

            //Attendance
            attendanceConsumer.Received += (sender, e) =>
            {
                AttendanceService attendanceService = new AttendanceService();
                var message = e.Body.ToArray();
                var xml = Encoding.UTF8.GetString(message);
                Console.WriteLine(xml);
                if (OfficeService.XSDValidatie(xml, "attendance"))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(RabbitMQAttendance));
                    using (TextReader reader = new StringReader(xml))
                    {
                        RabbitMQAttendance result = (RabbitMQAttendance)serializer.Deserialize(reader);
                        Console.WriteLine(result.Header.Method);
                        Console.WriteLine(result.UUID);
                        Console.WriteLine(result.UserId);
                        Console.WriteLine(result.EventId);
                        switch (result.Header.Method)
                        {
                            case XMLMethod.CREATE:
                                attendanceService.AttendanceCreate(result);
                                break;

                            case XMLMethod.DELETE:
                                attendanceService.AttendanceDelete(result);
                                break;
                        }
                    }
                }
            };


            channel.BasicConsume(queueUserName, true, userConsumer);
            channel.BasicConsume(queueEventName, true, eventConsumer);
            channel.BasicConsume(queueAttendanceName, true, attendanceConsumer);
            Console.ReadLine();
        }
    }
}
