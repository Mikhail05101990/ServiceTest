namespace ServiceTest.Dtos;

public class ModulePacket
{
    public string PackageID { get; set; } = "";
    public XmlProps[] DeviceStatuses{ get; set; } = new XmlProps[0];
}