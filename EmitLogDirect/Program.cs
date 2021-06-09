using System;
using System.Linq;
using System.Text;
using RabbitMQ.Client;

namespace EmitLogDirect {
    class Program {
        static void Main(string[] args) {
            const string exchangeName = "direct_logs";
            var factory = new ConnectionFactory() { HostName = "localhost" };
            
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel()) {
                channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct);
                
                while (true) {
                    Console.WriteLine($"Digite o texto (0 para sair):");
                    
                    var content = Console.ReadLine()!.Split(" ");
                    var message = content.Length > 0 ? content[0]: "";
                    //  'info', 'warning', 'error'.
                    var severity = content.Length > 1 ? content[1] : "info";

                    var body = Encoding.UTF8.GetBytes(message);
                    
                    channel.BasicPublish(exchangeName,severity,null, body );
                    Console.WriteLine($" [x] Sent {message} : {severity}");
                    
                    if (message == "0") {
                        break;
                    }
                }
            }
        }
    }
}