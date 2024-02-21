using System.Text;
using System.Linq;
using DbProviders;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ReceiverService.Dtos;
using Newtonsoft.Json;
using DB.Entities;

namespace RemoteProviders;

public class RabbitMQSubscriber
{
    static readonly string url = "amqps://xsycrzfm:VWTUP14de_hoxi_TJkoZWCOaq7Qi56og@beaver.rmq.cloudamqp.com/xsycrzfm";
    static readonly string queueName = "QueueTest";
    private IConnection? _connection;
    private IModel? _channel;
    private ManualResetEvent _resetEvent = new ManualResetEvent(false);
    private readonly StatusContext _db;

    public RabbitMQSubscriber(StatusContext db)
    {
        _db = db;
    }
    
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
            ModulePacket? m = JsonConvert.DeserializeObject<ModulePacket>(message);

            if(m == null)
                throw new Exception("Failed to parse message");
            Console.WriteLine("ReceivedMessage");
            ProcessStatuses(m);
            ProcessModules(m);
            
            _channel.BasicAck(deliveryEventArgs.DeliveryTag, false);
        };

        _ = _channel.BasicConsume(consumer, queueName);
        _resetEvent.WaitOne();
        _channel?.Close();
        _channel = null;
        _connection?.Close();
        _connection = null;
    }

    private void ProcessStatuses(ModulePacket data)
    {
        for(int i = 0; i < data.DeviceStatuses.Length; i++)
        {
            string id = data.DeviceStatuses[i].Props["ModuleCategoryID"];
            string st = data.DeviceStatuses[i].InnerProps.Props["ModuleState"];
            var res = _db.ModuleStatistics.FirstOrDefault(m => m.ModuleCategoryID == id);
            
            if(res == null)
                _db.Add(new ModuleStatistics(){ ModuleCategoryID = id, ModuleState = st});
            else
                res.ModuleState = st;

            _db.SaveChanges();
        }
    }
    private void ProcessModules(ModulePacket data)
    {
        double onlineCount = 0;
        double generalCount = 0;
        
        for(int i = 0; i < data.DeviceStatuses.Length; i++)
        {
            if(data.DeviceStatuses[i].InnerProps.Props["ModuleState"].ToLower().StartsWith("online"))
                onlineCount++;
            
            generalCount++;
        }
        
        var res = _db.PacketStatistics.FirstOrDefault(p => p.PacketID == data.PackageID);
        int perc = Convert.ToInt32(onlineCount/generalCount*100);
        
        if(res == null)
                _db.Add(new PacketStatistics(){ PacketID = data.PackageID, AvailabilityPercent = perc});
            else
                res.AvailabilityPercent = perc;
        
        _db.SaveChanges();
    }
}
