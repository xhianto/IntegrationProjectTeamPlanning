using System;
using System.Timers;

namespace RabbitMQHeartBeatProducer
{
    class Program
    {
        static void Main(string[] args)
        {
            //    HeartBeat heartBeat = new HeartBeat();

            //    Timer timer = new Timer();
            //    timer.Elapsed += new ElapsedEventHandler(heartBeat.sendHeartBeat);
            //    timer.Interval = 1000;
            //    timer.Start();
            //    //while (true) { }
            //    Console.Read();
            var service = new Office365Service.Services();
            //service.test();
            service.RefreshAccesToken();
        }
    }
}
