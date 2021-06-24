using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqRetry {
    public class RabbitMqOpt4 {
        
        private const int MaxRetries = 3;
        private const int BaseRetryDelay = 3000;
        
        private const string AarinUserExchange = "aarin.users";
        private const string MailmanUserCreatedQueue = "mailman.users.created";
        private const string AarinUserRetryExchange = "aarin.users.retry";
        private readonly IModel _channel;

        public RabbitMqOpt4(IModel channel) {
            _channel = channel;
            //Setup rabbit MQ topology
            _channel.ExchangeDeclare(AarinUserExchange, ExchangeType.Direct);
            _channel.QueueDeclare(MailmanUserCreatedQueue);
            _channel.ExchangeDeclare(AarinUserRetryExchange, "x-delayed-message", arguments: new Dictionary<string, object> {
                { "x-delayed-type", "direct" }
            });
            
            _channel.QueueBind(MailmanUserCreatedQueue, AarinUserExchange, "created");
            _channel.QueueBind(MailmanUserCreatedQueue, AarinUserRetryExchange, MailmanUserCreatedQueue);


            var consumer = new EventingBasicConsumer(_channel);
            
           _channel.BasicConsume(MailmanUserCreatedQueue,false, consumer);

           consumer.Received += (sender, ea) => {
                Console.WriteLine("------- Received message-------");

                var retryCount = ea.BasicProperties.GetHeaderValue<long>("x-retries")!;

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" Receive message: {message} | retry count: {retryCount}", message);
                _channel.BasicAck(ea.DeliveryTag, false);
                
                if (retryCount < MaxRetries) {
                    var retryDelay = BaseRetryDelay * (retryCount + 1);
                    Console.WriteLine($"{DateTime.Now} - Publishing to retry exchange with {retryDelay / 1000}s delay");
                    
                    var properties = _channel.CreateBasicProperties();
                    properties.Persistent = true;

                    if (properties.Headers == null) {
                        properties.Headers = new Dictionary<string, object>();
                    }
                    
                    properties.Headers.Add("x-retries", retryCount +1);
                    properties.Headers.Add("x-delay", retryDelay);

                    _channel.BasicPublish(AarinUserRetryExchange, MailmanUserCreatedQueue, properties, body);
                } else {
                    Console.WriteLine($"{DateTime.Now} | max retries reached - throwing message");
                }
            };
        }
        
        public void PublishMessage(string message) {
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(AarinUserExchange, routingKey:"created",properties, body );
        }
    }
}