using System;
using System.Text;
using RabbitMQ.Client;

namespace EmitLogTopic {
    class Program {
        static void Main(string[] args) {
            const string exchangeName = "topic_logs";
            var factory = new ConnectionFactory() { HostName = "localhost" };
            
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel()) {
                channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Topic);
                
                while (true) {
                    Console.WriteLine($"Digite o texto (0 para sair):");
                    
                    var content = Console.ReadLine()!.Split(" ");
                    var message = content.Length > 0 ? content[0]: "";
                    //  'info', 'warning', 'error'.
                    // 'kern', 'cron', 'anonymous'
                    var routingKey = content.Length > 1 ? content[1] : "anonymous.info";

                    var body = Encoding.UTF8.GetBytes(message);
                    
                    channel.BasicPublish(exchangeName,routingKey,null, body );
                    Console.WriteLine($" [x] Sent {message} : {routingKey}");
                    
                    if (message == "0") {
                        break;
                    }
                }
            }
        }
    }
}