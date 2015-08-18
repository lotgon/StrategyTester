using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using Ionic.Zip;

namespace DukasHistoryDownloader
{
    public class DownloadHelper
    {
        public static MemoryStream DownloadAndExtract(string urlRequest)
        {
            WebRequest req = WebRequest.Create(urlRequest);
            WebResponse response = req.GetResponse();
            Stream stream = response.GetResponseStream();

            byte[] buffer = new byte[1024];

            using (FileStream fs = new FileStream("temp.bin", FileMode.Create))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                do
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;
                    bw.Write(buffer, 0, bytesRead);
                } while (true);
            }
            MemoryStream mem = new MemoryStream();
            using (ZipFile zip = ZipFile.Read("temp.bin"))
            {
                foreach (ZipEntry ze in zip)
                {
                    ze.Extract(mem);
                }
            }
            mem.Position = 0;

            return mem;
        }
    }
}
