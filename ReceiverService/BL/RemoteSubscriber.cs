using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RemoteProviders;

public class RabbitMQSubscriber
{
    static readonly string url = "amqps://xsycrzfm:VWTUP14de_hoxi_TJkoZWCOaq7Qi56og@beaver.rmq.cloudamqp.com/xsycrzfm";
    static readonly string queueName = "QueueTest";
    private IConnection? _connection;
    private IModel? _channel;
    private ManualResetEvent _resetEvent = new ManualResetEvent(false);

    public void ConsumeQueue()
    {
        bool durable = true;
        bool exclusive = false;
        bool autoDelete = false;
        
        var factory = new ConnectionFactory
        {
            Uri = new Uri(url)
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queueName, durable, exclusive, autoDelete, null);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, deliveryEventArgs) =>
        {
            var body = deliveryEventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine("** Received message: {0}", message);
            _channel.BasicAck(deliveryEventArgs.DeliveryTag, false);
        };

        _ = _channel.BasicConsume(consumer, queueName);
        _resetEvent.WaitOne();
        _channel?.Close();
        _channel = null;
        _connection?.Close();
        _connection = null;
    }
}
