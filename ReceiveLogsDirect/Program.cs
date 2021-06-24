using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ReceiveLogsDirect {
    class Program {
        static void Main(string[] args) {
            const string exchangeName = "direct_logs";
            var factory = new ConnectionFactory() { HostName = "localhost" };
            
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel()) {
                channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);
                var queueName = channel.QueueDeclare().QueueName;
                
                if(args.Length == 0) {
                    Console.Error.WriteLine("Usage: {0} [info] [warning] [error]", Environment.GetCommandLineArgs()[0]);
                    return;
                }

                foreach (var severity in args) { 
                    channel.QueueBind(queueName, exchangeName, severity);
                }
                
                Console.WriteLine(" [*] Waiting for logs.");
                
                var consumer = new EventingBasicConsumer(channel);
                
                consumer.Received += (sender, ea) => {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($" [x] {message}: {ea.RoutingKey} at {DateTime.Now}", message);
                };
                
                channel.BasicConsume(queueName, true, consumer);
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}