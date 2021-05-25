using System;
using System.Collections.Generic;
using System.Text;

namespace Office365Service.Models
{
    public class Constant
    {
        // Constants of Azure
        public const string Client_Id = "8e5616f2-f2a1-4ce6-987b-ada0b89c8bba";
        public const string Client_Secret = "A~CL32hEL-okZd4Norz-3ELr-_915Z~vxa";
        public const string Redirect_Url = "https://localhost:44338/oauth/admincallback";
        public const string Scopes = "https://graph.microsoft.com/.default";
        public const string Tenant_Id = "e0f82523-278e-4994-878e-5b879b05c241";

        // Constants of RabbitMQ
        public const string RabbitMQConnectionUrl = "amqp://guest:guest@10.3.17.66:5671";
        //public const string RabbitMQConnectionUrl = "amqp://guest:guest@10.3.17.61:5672";
        public const string RabbitMQEventExchangeName = "event-exchange";
        public const string RabbitMQEventQueueName = "to-planning_event-queue";
        public const string RabbitMQUserExchangeName = "user-exchange";
        public const string RabbitMQUserQueueName = "to-planning_user-queue";
        public const string RabbitMQHeartBeatName = "to-monitoring_heartbeat-queue";

        // User
        public const string StandardPassword = "Test123$"; //of iets dergelijks met user name etc

        //Monitoring Database
        public const string DBConnectionString = "server=10.3.17.63;user=muuid;password=muuid;database=masteruuid;";
    }
}
