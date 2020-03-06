using System;
using System.IO;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VimeoDownloader;

namespace Downloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var filePath = "D:\\Pobrane - Chrome\\plikjson.txt";
            var directoryPath = "C:\\Users\\Mateusz02\\Desktop\\Output";
            var extension = ".mp4";
            if (IsAdministrator() == false)
            {
                Console.WriteLine("You need to run this application as administrator");
                throw new MemberAccessException("You need to run this application as administrator");
            }

            while (File.Exists(filePath) == false)
            {
                Console.WriteLine($"Type path to json.txt file");
                filePath = Console.ReadLine();
            }


            while (Directory.Exists(directoryPath) == false)
            {
                Console.WriteLine($"Type path to output directory");
                directoryPath = Console.ReadLine();
            }




            JObject json;
            using (StreamReader file = new StreamReader(filePath))
            {
                json = JObject.Parse(file.ReadToEnd());
            }
            foreach (JProperty property in json.Properties())
            {
                CustomFile obj = ProcessAddress(property.Name, property.Value.ToString(), extension, directoryPath);

                Directory.CreateDirectory(obj.FileDirectory);

                Video video = await Vimeo.Download(obj.videoId, VideoQuality.High);
                File.WriteAllBytes(obj.Filename, video.Data);
            }




        }
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static CustomFile ProcessAddress(string url, string video, string extension, string homeDir)
        {
            var slash2last = GetIndexOfSecondSlash(url);

            var directory = homeDir + (url.Substring(0, slash2last).Replace("/", "\\"));
            var filename = url.Substring(slash2last + 1, (url.Length - slash2last) - 2) + extension;
            var vid = video.Replace("https://vimeo.com/", "");
            vid = vid.Substring(0, GetIndexOfFirstSlash(vid));

            var customFile = new CustomFile();
            customFile.FileDirectory = directory;
            customFile.Filename = filename;
            customFile.videoId = vid;

            return customFile;
        }

        public static int GetIndexOfSecondSlash(string url)
        {
            bool first = false;
            for (int i = url.Length - 1; i >= 0; i--)
            {
                if (url[i] == '/')
                {
                    if (first)
                    {
                        return i;
                    }

                    first = true;
                }
            }

            return 0;
        }
        public static int GetIndexOfFirstSlash(string url)
        {
            for (int i = url.Length - 1; i >= 0; i--)
            {
                if (url[i] == '/')
                {
                    return i;
                }
            }

            return 0;
        }
    }

    public class CustomFile
    {
        public string FileDirectory { get; set; }
        public string Filename { get; set; }
        public string videoId { get; set; }

    }
}
