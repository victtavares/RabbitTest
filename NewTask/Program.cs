 using System;
 using System.Text;
 using RabbitMQ.Client;

 namespace NewTask {
    class Program {
        static void Main(string[] args) {
            var taskQueueName = "task_queue";
            
            var factory = new ConnectionFactory() { HostName = "localhost" };
            
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel()) {
                channel.QueueDeclare(taskQueueName, true, false, false, null);
                
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                
                while (true) {
                    Console.WriteLine($"Digite o texto (0 para sair):");
                    
                    var message = Console.ReadLine() ?? "";
                    var body = Encoding.UTF8.GetBytes(message);
                
       
                    channel.BasicPublish("", routingKey:taskQueueName,properties, body );
                    
                    Console.WriteLine($" [x] Sent {message}");
                    
                    if (message == "0") {
                        break;
                    }
                }
            }
        }
    }
}