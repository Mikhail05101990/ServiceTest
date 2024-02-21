namespace ServiceTest.Dtos;

public class XmlProps
{
    public Dictionary<string, string> Props { get; set; } = new();
    public XmlProps? InnerProps { get; set; } = null;
}