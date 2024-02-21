using System.Xml;
using CloudProviders.AMPQ;
using ServiceTest.Dtos;

namespace FileSystem.Parsers;

public class XmlParser
{
    static readonly string moduleInfoKey = "DeviceStatus";
    static readonly string packageId = "PackageID";
    List<FileInfo> oldFiles;
    string _targetDirectory;
    ILogger _logger;
    AMPQProvider _rabbitProvider;

    public XmlParser(string targetDirectory, ILogger logger, AMPQProvider ampqProvider)
    {
        _logger = logger;
        _rabbitProvider = ampqProvider;

        if(Directory.Exists(targetDirectory))
        {
            _targetDirectory = targetDirectory;
            oldFiles = new();
        }
        else
            throw new DirectoryNotFoundException($"{targetDirectory} not found");
    }

    public void ProcessFiles()
    {
        List<FileInfo> newFiles = GetNewFiles();

        if(newFiles.Count > 0)
            oldFiles.AddRange(newFiles);

        List<Task> tasks = new List<Task>();

        for(int i = 0; i < newFiles.Count; i++)
        {
            string path = newFiles[i].Path;
            Task tsk = Task.Run(() => HandleFile(path));
            tasks.Add(tsk);
        }
            
        Task.WaitAll(tasks.ToArray());
    }

    private void HandleFile(object? obj)
    {
        string path = obj == null ? throw new Exception("Empty path passed to process.") : (string)obj;
        
        XmlDocument doc = new XmlDocument();
        doc.Load(path);

        _logger.LogInformation("Loaded xml file.");

        XmlElement? root = doc.DocumentElement;
        
        if(root == null)
            throw new Exception("Invaid xml document.");

        XmlNode? nextNode = root.FirstChild;

        if(nextNode == null)
            throw new Exception("No elements inside xml root.");

        Dictionary<string, string> generalProps = new();
        List<XmlProps> items = new List<XmlProps>();

        foreach(XmlNode child in root.ChildNodes)
        {
            if(child.Name.Equals(moduleInfoKey))
            {
                XmlProps props = XmlNodeToObject(child);
                items.Add(props);
            }
            else
                generalProps.Add(child.Name, child.InnerText);
        }

        _logger.LogInformation("xml processing finished.");

        ModulePacket data = new ModulePacket()
        {
            PackageID = generalProps[packageId],
            DeviceStatuses = items.ToArray()
        };

        _rabbitProvider.SendMessage(data);
    }

    private List<FileInfo> GetNewFiles()
    {
        List<FileInfo> result = new();
        string[] files = Directory.GetFiles(_targetDirectory, "*.xml");

        for(int i = 0; i < files.Length; i++)
        {
            FileInfo? obj = oldFiles.FirstOrDefault(obj => obj.Path.Equals(files[i]));
            DateTime modification = File.GetLastWriteTime(files[i]);

            if(obj != null)
            {
                if(obj.Modified != modification)
                {
                    result.Add(new FileInfo(){
                        Path = files[i],
                        Modified = modification
                    });
                }
            }
            else
                result.Add(new FileInfo(){
                    Path = files[i],
                    Modified = modification
                });  
        }
        
        return result;
    }

    private XmlProps XmlNodeToObject(XmlNode node)
    {
        XmlProps result = new XmlProps(){
            InnerProps = null,
            Props = new Dictionary<string, string>()
        };

        foreach(XmlNode child in node.ChildNodes)
            if(child.ChildNodes.Count > 1)
                result.InnerProps = XmlNodeToObject(child);
            else if(child.InnerText.Contains("encoding"))
            {
                result.InnerProps = new XmlProps();
                XmlNodeList nodes = GetNodesFromString(child.InnerText);
                    
                foreach(XmlNode n in nodes)
                    result.InnerProps.Props.Add(n.Name, n.InnerText);
            }
            else
               result.Props.Add(child.Name, child.InnerText);  

        return result;
    }

    private XmlNodeList GetNodesFromString(string text)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(text);
        XmlElement? root = doc.DocumentElement;
        
        if(root == null)
            throw new Exception("Invaid xml document.");
        
        XmlNodeList list = root.ChildNodes;

        return list;
    }

    class FileInfo
    {
        public string Path {get;set;} = "";
        public DateTime Modified {get;set;}
    }
}