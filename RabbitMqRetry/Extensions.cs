using System;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;

namespace RabbitMqRetry {
    static class Extensions {

        public static long GetDeathRetryCount(this IBasicProperties basicProperties) {
            var deathList =
                (basicProperties.Headers?.GetValueOrNull("x-death") as List<object>)?.FirstOrDefault() as Dictionary<string, object>;

            long retryCount = 0;
            if (deathList != null) {
                retryCount = (long) deathList.GetValueOrDefault("count", 0);
            }

            return retryCount;
        }
        
        public static T? GetHeaderValue<T>(this IBasicProperties basicProperties, string key) {
            var value = basicProperties.Headers?.GetValueOrNull(key);
            return value is T value1 ? value1 : default;
        }
        
        public static TValue? GetValueOrNull<TKey, TValue> 
        (this IDictionary<TKey, TValue> dictionary, 
            TKey key) {
            return dictionary.TryGetValue(key, out var value2) ? value2 : default;
        }
    
        public static TValue GetValueOrDefault<TKey, TValue> 
        (this IDictionary<TKey, TValue> dictionary, 
            TKey key,
            TValue defaultValue)  {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }
    }
}