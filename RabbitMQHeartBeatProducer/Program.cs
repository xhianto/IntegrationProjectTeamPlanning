using System;
using System.Timers;
using Office365Service.Models;
using Office365Service;

namespace RabbitMQHeartBeatProducer
{
    class Program
    {
        static void Main(string[] args)
        {
            //HeartBeat heartBeat = new HeartBeat();

            //Timer timer = new Timer();
            //timer.Elapsed += new ElapsedEventHandler(heartBeat.sendHeartBeat);
            //timer.Interval = 1000;
            //timer.Start();
            //while (true) { }



            //    Console.Read();
            //var service = new Office365Service.Services();
            ////service.test();
            //service.RefreshAccesToken();

            MasterDBServices service = new MasterDBServices();
            Master frontend = new Master();
            Master planning = new Master();
            Master planning2 = new Master();

            using(var context = new MasterDbContext())
            {
                frontend = context.Master.Find(188);
                planning = context.Master.Find(190);
                planning2 = context.Master.Find(192);
            }

            Guid front = new Guid(frontend.Uuid);
            Guid plan = new Guid(planning.Uuid);
            Guid plan2 = new Guid(planning2.Uuid);
            //Guid plan3 = new Guid("f51aa4ea-dfc3-eb11-b876-00155d110504");
            //byte[] array = plan3.ToByteArray();
            //foreach (byte b in array)
            //{
            //    Console.Write(b + ", ");
            //}
            //Console.WriteLine();
            //byte[] mysql = service.MySQLByteArray(plan3);
            //foreach (byte b in mysql)
            //{
            //    Console.Write(b + ", ");
            //}
            Console.WriteLine();
            Console.WriteLine("Canvas: " + front.ToString());
            Console.WriteLine("Frontend: " + plan.ToString());
            Console.WriteLine("Planning: " + plan2.ToString());
            //Console.WriteLine("uuid is           f51aa4ea-dfc3-eb11-b876-00155d110504");
            //Console.WriteLine("ToByteArray():    " + new Guid(array).ToString());
            //Console.WriteLine("MySQLByteArray(): " + new Guid(mysql).ToString());
        }
    }
}
