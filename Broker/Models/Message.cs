namespace Broker.Models
{
    public class Message
    {
        public string Topic { get; }

        public string Content { get; }

        public Message(string topic, string content)
        {
            Topic = topic;
            Content = content;
        }
    }
}