using System;
using RabbitMQ.Client;
using System.Text;

namespace SimpleSender {
    class Program {
        static void Main(string[] args) {
            var factory = new ConnectionFactory() {HostName = "localhost"};
            using (var connection = factory.CreateConnection())
                
            using (var channel = connection.CreateModel()) {
                channel.QueueDeclare("hello", false, false, false, null);
                var message = $"Hello world! {DateTime.Now}";
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish("", "hello", null, body);
                Console.WriteLine(" [x] Sent {0}", message);
            }
        }
        
    }
}