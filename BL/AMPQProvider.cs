using System;
using System.Text;
using RabbitMQ.Client;

namespace CloudProviders.AMPQ;

public class AMPQProvider
{
    static readonly string url = "amqps://xsycrzfm:VWTUP14de_hoxi_TJkoZWCOaq7Qi56og@beaver.rmq.cloudamqp.com/xsycrzfm";
    static readonly string queueName = "QueueTest";
    
    public void SendMessage(string message)
    {
        bool durable = true;
        bool exclusive = false;
        bool autoDelete = false;

        var factory = new ConnectionFactory
        {
            Uri = new Uri(url)
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queueName, durable, exclusive, autoDelete, null);
        var data = Encoding.UTF8.GetBytes(message);
        var exchangeName = "";
        var routingKey = queueName;
        channel.BasicPublish(exchangeName, routingKey, null, data);
    }
}