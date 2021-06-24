using System;
using System.Threading;
using RabbitMQ.Client;

namespace RabbitMqRetry {
    internal class Program {
        public static void Main(string[] args) {
            //https://engineering.nanit.com/rabbitmq-retries-the-full-story-ca4cc6c5b493
            
            var factory = new ConnectionFactory { HostName = "localhost" };
            
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel()) {
                var rabbitMq = new RabbitMqOpt2(channel);
                rabbitMq.PublishMessage("Hello!");
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }

            
        }
    }
}