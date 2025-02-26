using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;

namespace TvOpenPlatform.KafkaClient.Models
{
    public struct MessageWrapper<T>
    {

        public MessageWrapper(Topic topic, Key key, T message, MessageHeaders headers)
        {
            this.Key = key;
            this.Message = message;
            this.Headers = headers;
            this.Topic = topic;
        }       

        public Topic Topic { get; }
        public Key Key { get; }
        public T Message { get; set; }
        public MessageHeaders Headers { get; set; }        
    }

    public struct Key
    {        
        public Key(string key)
        {
            _key = key;
        }

        private string _key;

        public static implicit operator string(Key messageKey) => messageKey._key;
    }

    public struct Topic
    {
        public Topic(string topic)
        {
            if (string.IsNullOrEmpty(topic))
            {
                throw new ArgumentException("Topic can not be null or empty");
            }

            _topic = topic;
        }

        private string _topic;

        public static implicit operator string(Topic messageTopic) => messageTopic._topic;
    }

    public class MessageHeaders
    {
        private IReadOnlyDictionary<string, byte[]> _headers = new Dictionary<string, byte[]>();

        internal static class EmptyReadOnlyDictionary<TKey, TValue>
        {
            public static readonly IReadOnlyDictionary<TKey, TValue> Empty
                = new Dictionary<TKey, TValue>();
        }

        public static MessageHeaders Empty = new MessageHeaders(EmptyReadOnlyDictionary<string, byte[]>.Empty);
     
        public MessageHeaders(IReadOnlyDictionary<string, byte[]> headers)
        {
            _headers = headers ?? EmptyReadOnlyDictionary<string, byte[]>.Empty;
        }

        public bool HasKey(string key)
        {
            return _headers.ContainsKey(key);
        }

        private IReadOnlyDictionary<string, byte[]> Headers
        {
            get
            {
                return _headers;
            }
        }
        
        public static implicit operator Headers(MessageHeaders messageHeaders)
        {
            var headers = new Headers();
            foreach(var pair in messageHeaders.Headers)
            {
                headers.Add(new Header(pair.Key, pair.Value));
            }

            return headers;
        }
    }

}
