using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SWUReporting_TEST.Data
{
    public class FileDownloader
    {
        public static void ZipAndDownload(string[] files = null, string folderPath = null)
        {
            HttpResponse response = HttpContext.Current.Response;
            if (response.IsClientConnected)
            {
                response.Clear();
                response.BufferOutput = false;
                response.ContentType = "application/zip";
                response.AddHeader("content-disposition", "attachment; filename=SWU_reports.zip");

                using (ZipFile zipFile = new ZipFile())
                {
                    zipFile.CompressionLevel = Ionic.Zlib.CompressionLevel.BestSpeed;
                    if (!(folderPath == null))
                    {
                        zipFile.AddDirectory(folderPath);
                    }
                    else
                    {
                        zipFile.AddFiles(files, false, "");
                    }
                    
                    zipFile.Save(response.OutputStream);
                }
                response.Flush();
                //clean up files  
                if (!(folderPath == null))
                {
                    Directory.Delete(folderPath, true);
                }
                else
                {
                    foreach (var f in files)
                    {
                        try
                        {
                            File.Delete(f);
                        }
                        catch (Exception)
                        {
                            //do nothing
                        }

                    }
                }
                response.End();
                //HttpContext.Current.ApplicationInstance.CompleteRequest();
            }

            
        }

        public static void DownloadFile(string filePath)
        {
            HttpResponse response = HttpContext.Current.Response;
            byte[] Content = File.ReadAllBytes(filePath);
            response.ContentType = "application/octet-stream";
            response.AddHeader("content-disposition", "attachment; filename=" + Path.GetFileName(filePath));
            response.BufferOutput = true;
            response.OutputStream.Write(Content, 0, Content.Length);
            response.End();
        }
        public bool IsReusable { get { return false; } }
    }
}