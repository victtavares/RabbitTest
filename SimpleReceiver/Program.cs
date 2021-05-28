using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace SimpleReceiver {
    
    //Outro caminho: https://medium.com/@thiagoloureiro/rabbitmq-gerenciando-mensagens-no-c-1e81ae4bad0f
    class Program {
        static void Main(string[] args) {
            var factory = new ConnectionFactory() {HostName = "localhost"};
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel()) {
                channel.QueueDeclare("hello", false,false,false,null);
                
                var consumer = new EventingBasicConsumer(channel);
                
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($" [x] Received {message} at {DateTime.Now}", message);
                };
                
                channel.BasicConsume("hello", true, consumer);
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();

            }
        }
    }
}