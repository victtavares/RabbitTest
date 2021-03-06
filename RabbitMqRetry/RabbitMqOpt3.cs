using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqRetry {
    public class RabbitMqOpt3 {
        
        private const int MaxRetries = 3;
        private const int BaseRetryDelay = 3000;
        
        private const string AarinUserExchange = "aarin.users";
        private const string MailmanUserCreatedQueue = "mailman.users.created";
        private const string AarinUserRetryOneExchange = "aarin.users.retryOne";
        private const string AarinUserWaitQueue = "aarin.users.wait_queue";
        private const string AarinUserRetryTwoExchange = "aarin.users.retryTwo";
        private readonly IModel _channel;

        public RabbitMqOpt3(IModel channel) {
            _channel = channel;
            //Setup rabbit MQ topology
            _channel.ExchangeDeclare(AarinUserExchange, ExchangeType.Direct);
            _channel.QueueDeclare(MailmanUserCreatedQueue);
            
            _channel.ExchangeDeclare(AarinUserRetryOneExchange, ExchangeType.Direct);
            _channel.QueueDeclare(AarinUserWaitQueue, arguments: new Dictionary<string, object> {
                { "x-dead-letter-exchange", AarinUserRetryTwoExchange }
            });
            
            _channel.ExchangeDeclare(AarinUserRetryTwoExchange, ExchangeType.Direct);
            
            _channel.QueueBind(MailmanUserCreatedQueue, AarinUserExchange, "created");
            _channel.QueueBind(MailmanUserCreatedQueue, AarinUserRetryTwoExchange, MailmanUserCreatedQueue);
            _channel.QueueBind(AarinUserWaitQueue, AarinUserRetryOneExchange, MailmanUserCreatedQueue);

            
            var consumer = new EventingBasicConsumer(_channel);
            
           _channel.BasicConsume(MailmanUserCreatedQueue,false, consumer);
            //_channel.BasicConsume(AarinUserWaitQueue,false, consumer);

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
                    properties.Expiration = retryDelay.ToString();

                    if (properties.Headers == null) {
                        properties.Headers = new Dictionary<string, object>();
                    }
                    
                    properties.Headers.Add("x-retries", retryCount +1);

                    _channel.BasicPublish(AarinUserRetryOneExchange, MailmanUserCreatedQueue, properties, body);
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