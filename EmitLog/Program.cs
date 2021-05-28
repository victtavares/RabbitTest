using System;
using System.Text;
using RabbitMQ.Client;

namespace EmitLog {
    class Program {
        static void Main(string[] args) {
            const string exchangeName = "logs";
            var factory = new ConnectionFactory() { HostName = "localhost" };
            
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel()) {
                channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout);

                while (true) {
                    Console.WriteLine($"Digite o texto (0 para sair):");
                    
                    var message = Console.ReadLine() ?? "";
                    var body = Encoding.UTF8.GetBytes(message);
                    
                    channel.BasicPublish(exchangeName,"",null, body );
                    Console.WriteLine($" [x] Sent {message}");
                    
                    if (message == "0") {
                        break;
                    }
                }
            }
        }
    }
}