using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace SWUReporting_TEST
{
    public class Messaging
    {
        public static void logresults(string message)
        {
            string Fpath = "C:\\Upload\\Importlog.txt";
            System.IO.StreamWriter strwtr;
            strwtr = new System.IO.StreamWriter(Fpath, true);
            strwtr.Write("********************" + " Import Log - " + DateTime.Now + "*********************");           
            strwtr.Write(message);
            strwtr.Write(Environment.NewLine);
            strwtr.Close();
        }
        public static void SendAlert(string message, Page pg)
        {
            ScriptManager.RegisterClientScriptBlock(pg, pg.GetType(), "alertMessage", "alert('" + message + "')", true);
        }

        public static void LogErrorToText(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("********************" + " Error Log - " + DateTime.Now + "*********************");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("Exception Type : " + ex.GetType().Name);
            sb.Append(Environment.NewLine);
            sb.Append("Error Message : " + ex.Message);
            sb.Append(Environment.NewLine);
            sb.Append("Error Source : " + ex.Source);
            sb.Append(Environment.NewLine);
            if (ex.StackTrace != null)
            {
                sb.Append("Error Trace : " + ex.StackTrace);
            }
            Exception innerEx = ex.InnerException;

            while (innerEx != null)
            {
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
                sb.Append("Exception Type : " + innerEx.GetType().Name);
                sb.Append(Environment.NewLine);
                sb.Append("Error Message : " + innerEx.Message);
                sb.Append(Environment.NewLine);
                sb.Append("Error Source : " + innerEx.Source);
                sb.Append(Environment.NewLine);
                if (ex.StackTrace != null)
                {
                    sb.Append("Error Trace : " + innerEx.StackTrace);
                }
                innerEx = innerEx.InnerException;
            }
            //string filePath = HttpContext.Current.Server.MapPath("ErrorLog.txt");
            string filePath = "C:\\Upload\\ErrorLog.txt";
            if (Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                if (!File.Exists(filePath))
                {
                    FileStream fs = File.Create(filePath);
                    fs.Close();
                }

                string oldText;
                using(StreamReader sr = new StreamReader(filePath))
                {
                    oldText = sr.ReadToEnd();
                }
                using (StreamWriter writer = new StreamWriter(filePath, false))
                {
                    writer.WriteLine(sb.ToString());
                    writer.WriteLine(oldText);
                    writer.Flush();
                }
            }
        }
    }
}