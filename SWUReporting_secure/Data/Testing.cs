using SWUReporting;
using SWUReporting.Data;
using System.IO;

namespace SWUReporting
{
    static class Testing
    {

        public static void EditCSVFTEFile(string filepath)
        {
            StreamReader sr = new StreamReader(filepath);
            string data = sr.ReadToEnd();
            sr.Close();
            //remove ; from emails
            data = data.Replace(";\t", "\t");

            //remove extra quotes from var names
            data = data.Replace(@"""", "");
            StreamWriter sw = new StreamWriter(@"C: \Users\IQ1\Desktop\test_update.txt");
            sw.Write(data);
            sw.Flush();
            sw.Close();
        }
        public static void ImportVT(DB db)
        {
            Importer i = new Importer(db);
            i.ImportVT(@"C:\Upload\var_employees.csv");
            
        }

        public static void CreateTransposedVTTable()
        {
            DB db = new DB("tmpVARDATA");
            db.Connect();
            db.CreateTransposedVTTempTable();
        }

        public static void PAP(DBReporting dbr, System.Web.UI.Page page)
        {
            bool status;
            string serverFile = dbr.CreatePAPUserReport(ReportType.TransposedPointsCSV, out status);
            //dbr.GetTransposedDataAllFTEs("goengineer");
            DBReporting.db.Disconnect();

            if (status)
            {
                string[] files = new string[] { serverFile };
                FileDownloader.ZipAndDownload(files);
            }
            else
            {
                //error message back to user that the report failed to create
                Messaging.SendAlert(serverFile, page);
            }
        }
    }
}