using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ReceiveLog {
    class Program {
        static void Main(string[] args) {
            const string exchangeName = "logs";
            var factory = new ConnectionFactory() { HostName = "localhost" };
            
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel()) {
                channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Fanout);
                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queueName, "logs", "");
                
                Console.WriteLine(" [*] Waiting for logs.");
                
                var consumer = new EventingBasicConsumer(channel);
                
                consumer.Received += (sender, ea) => {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($" [x] {message} at {DateTime.Now}", message);
                };
                
                channel.BasicConsume(queueName, true, consumer);
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}