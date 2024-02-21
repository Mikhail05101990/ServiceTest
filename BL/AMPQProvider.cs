using System;
using System.Text;
using RabbitMQ.Client;
using ServiceTest.Dtos;
using Newtonsoft.Json;

namespace CloudProviders.AMPQ;

public class AMPQProvider
{
    static readonly string url = "amqps://xsycrzfm:VWTUP14de_hoxi_TJkoZWCOaq7Qi56og@beaver.rmq.cloudamqp.com/xsycrzfm";
    static readonly string queueName = "QueueTest";
    static readonly Random random = new Random();

    public void SendMessage(object info)
    {   
        ModulePacket packet = (ModulePacket)info;
        ModifyData(packet);
        string message = JsonConvert.SerializeObject(packet, Formatting.Indented);
        Console.WriteLine(message);
        var data = Encoding.UTF8.GetBytes(message);

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
        var exchangeName = "";
        var routingKey = queueName;
        channel.BasicPublish(exchangeName, routingKey, null, data);
    }

    private void ModifyData(ModulePacket data)
    {
        for(int i = 0; i < data.DeviceStatuses.Length; i++)
        {
            int val = random.Next(0, 3);
            XmlProps? innerProps = data.DeviceStatuses[i].InnerProps;
            
            if(innerProps == null)
                throw new Exception("Fail when read innerProps");
            
            string? nValue;

            if(innerProps.Props.TryGetValue("ModuleState", out nValue))
                innerProps.Props["ModuleState"] = ((States)val).ToString();
            else
                throw new Exception("ModuleState key not found");
        }
    }

    private enum States: int
    {
        Online, 
        Run, 
        NotReady, 
        Offline
    }
}