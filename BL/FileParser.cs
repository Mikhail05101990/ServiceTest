using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace FileSystem.Parsers;

public class XmlParser
{
    static readonly string statusPropsKey = "RapidControlStatus";
    static readonly string moduleInfoKey = "DeviceStatus";
    List<FileInfo> oldFiles;
    string _targetDirectory;

    public XmlParser(string targetDirectory)
    {
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
        oldFiles = newFiles;

        if(newFiles.Count < 1)
            Console.WriteLine("No new lines.");

        for(int i = 0; i < newFiles.Count; i++)
            Task.Factory.StartNew(HandleFile, newFiles[i]);
    }

    private void HandleFile(object? obj)
    {
        string path = obj == null ? throw new Exception("Empty path passed to process.") : (string)obj;

        XmlDocument doc = new XmlDocument();
        doc.Load(path);
        XmlNodeList? list;

        if(doc.DocumentElement != null)
        {
            list = doc.DocumentElement.SelectNodes($"/{moduleInfoKey}");

            if(list != null)
            {
                foreach (XmlNode item in list)
                {
                    XmlProps dict = XMLNodeToDictionary(item);
                }
            }
            else
                throw new Exception("Invalid xml data");
        }
        else
            throw new Exception("Invalid xml");
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

    private XmlProps XMLNodeToDictionary(XmlNode node)
    {
        Dictionary<string, string> general = new();
        Dictionary<string, string> status = new();
        
        if (node.HasChildNodes)
        {
            foreach(XmlNode child in node.ChildNodes)
            {
                string name = child.Name;
                string value = child.InnerText;
                general.Add(name, value);
            }
        }

        string stProps = general[statusPropsKey];
        XmlDocument doc = new XmlDocument();
        doc.Load(stProps);

        return new XmlProps()
        {
            GeneralProps = general,
            StatusProps = status
        };
    }

    class FileInfo
    {
        public string Path {get;set;} = "";
        public DateTime Modified {get;set;}
    }

    class XmlProps
    {
        public Dictionary<string, string> GeneralProps { get; set; } = new();
        public Dictionary<string, string> StatusProps { get; set; } = new();
    }
}