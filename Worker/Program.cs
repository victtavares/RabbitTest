using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Worker {
    class Program {
        static void Main(string[] args) {
            var taskQueueName = "task_queue";
            
            var factory = new ConnectionFactory() {HostName = "localhost"};
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel()) {
                channel.QueueDeclare(taskQueueName, true, false, false, null);
                channel.BasicQos(0,1, false);

                var consumer = new EventingBasicConsumer(channel);
                
                consumer.Received += (sender, ea) => {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($" [x] Received {message} at {DateTime.Now}", message);
                    
                    int dots = message.Split('.').Length - 1;
                    Thread.Sleep(dots * 1000);
                    
                    Console.WriteLine(" [x] Done");

                    var model = ((EventingBasicConsumer) sender)!.Model;
                    
                    model.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };

                channel.BasicConsume(taskQueueName, false, consumer);
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}