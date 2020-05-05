using System;
using System.IO;
using System.Net;

namespace StartWaterpump
{
    class Program
    {
        static void Main(string[] args)
        {
            bool close = true;
            string url = @"http://nas-server/wasserpumpe/on?time=5";

            foreach(string arg in args)
            {
                if (arg == "-k") close = false;
                else url = arg;
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                Console.WriteLine(reader.ReadToEnd());
            }

            if (close) Environment.Exit(0);
        }
    }
}
