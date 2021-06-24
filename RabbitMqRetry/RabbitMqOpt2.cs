#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqRetry {
    public class RabbitMqOpt2 {
        private const int MaxRetries = 3;
        private const int RetryDelay = 5000;
        
        private const string AarinUserExchange = "aarin.users";
        private const string MailmanUserCreatedQueue = "mailman.users.created";
        private const string AarinUserRetryOneExchange = "aarin.users.retryOne";
        private const string AarinUserWaitQueue = "aarin.users.wait_queue";
        private const string AarinUserRetryTwoExchange = "aarin.users.retryTwo";
        private readonly IModel _channel;

        public RabbitMqOpt2(IModel channel) {
            _channel = channel;
            //Setup rabbit MQ topology
            _channel.ExchangeDeclare(AarinUserExchange, ExchangeType.Direct);
            _channel.QueueDeclare(MailmanUserCreatedQueue, arguments: new Dictionary<string, object> {
                { "x-dead-letter-exchange", AarinUserRetryOneExchange },
                { "x-dead-letter-routing-key", MailmanUserCreatedQueue }
            });
            
            _channel.ExchangeDeclare(AarinUserRetryOneExchange, ExchangeType.Direct);
            _channel.QueueDeclare(AarinUserWaitQueue, arguments: new Dictionary<string, object> {
                { "x-dead-letter-exchange", AarinUserRetryTwoExchange },
                { "x-message-ttl", RetryDelay }
            });
            
            _channel.ExchangeDeclare(AarinUserRetryTwoExchange, ExchangeType.Direct);
            
            _channel.QueueBind(MailmanUserCreatedQueue, AarinUserExchange, "created");
            _channel.QueueBind(MailmanUserCreatedQueue, AarinUserRetryTwoExchange, MailmanUserCreatedQueue);
            _channel.QueueBind(AarinUserWaitQueue, AarinUserRetryOneExchange, MailmanUserCreatedQueue);

            
            var consumer = new EventingBasicConsumer(_channel);
            
            _channel.BasicConsume(MailmanUserCreatedQueue,false, consumer);

            consumer.Received += (_, ea) => {
                var retryCount = ea.BasicProperties.GetDeathRetryCount();

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" Receive message: {message} | retry count: {retryCount}", message);

                if (retryCount < MaxRetries) {
                    Console.WriteLine($"{DateTime.Now} | rejecting (retry via DLX)");
                    _channel.BasicReject(ea.DeliveryTag, false);
                } else {
                    Console.WriteLine($"{DateTime.Now} | max retries reached - Acking");
                    _channel.BasicAck(ea.DeliveryTag, false);
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

