using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileSystem.Parsers;

public class XmlParser
{
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
        string path;
        
        if(obj == null)
            throw new Exception("Empty path passed to process.");
        else
            path = (string)obj;

        using (FileStream fs = File.OpenRead(path))
        {
            string content;
            int iteration = 0;
            byte[] b = new byte[4096];
            ASCIIEncoding temp = new ASCIIEncoding();
            int readLen;
            
            while ((readLen = fs.Read(b,0,b.Length)) > 0 )
            {
                iteration++;

                if(iteration > 1)
                    throw new Exception($"{path} is too big.");

                content = temp.GetString(b,0,readLen);
            }
        }
    }

    private List<FileInfo> GetNewFiles()
    {
        List<FileInfo> result = new();
        string[] files = Directory.GetFiles(_targetDirectory, "*.xml");

        for(int i = 0; i < files.Length; i++)
        {
            FileInfo? obj = oldFiles.FirstOrDefault(obj => obj.Path.Equals(files[i]));
            DateTime modification = File.GetLastWriteTime(files[i]);

            if(obj !=null)
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

    class FileInfo
    {
        public string Path {get;set;} = "";
        public DateTime Modified {get;set;}
    }
}