using System.Data;
using System;
using System.IO;
using System.Text;
using SWUReporting.Data;
using System.Collections.Generic;

namespace ReportBuilder
{

    public class ReportIO
    {

        #region Properties
        public static string savePath = "C:\\Upload\\";
        public static string sep = ",";
        public static string retChar = "\r\n";
        #endregion

        public static void DownloadSingleSheet(DataTable dt)
        {
            DataSet ds = new DataSet();
            ds.Tables.Add(dt);
            string[] sheets = new string[] { "Sheet1" };
            string filename;
            if (dt.TableName != "")
            {
                filename = dt.TableName;
            }
            else
            {
                filename = "SWUReport";
            }
            filename = filename + DateTime.Now.ToOADate().ToString();
            string filepath = OExcelNew.exportDocument(filename + ".xlsx", ds, sheets);
            FileDownloader.ZipAndDownload(new string[] { filepath });
        }

        internal static string WriteToFileEncoded(string csvString, string name)
        {
            string filePath = string.Empty;
            try
            {
                filePath = savePath + name;
                Encoding utf8WithoutBOM = new UTF8Encoding(false);

                TextWriter tw = new StreamWriter(filePath, false, utf8WithoutBOM);
                tw.Write(csvString);
                tw.Flush();
                tw.Close();
            }
            catch (Exception e)
            {
                return "";
            }
            return filePath;
        }

        internal static string CreateCSVFileFromDT(DataTable dt)
        {
            var csvString = getCSVFromTable(dt);
            string fileName = "CRE " + dt.TableName + ".csv";
            //string filePath = Path.GetTempPath() + fileName;
            string filePath; //= fileName;
            filePath = ReportIO.WriteToFileEncoded(csvString, fileName);  //pass just the file name, not path
            return filePath;
        }

        internal static string getCSVFromTable(DataTable t)
        {

            StringBuilder csvData = new StringBuilder();
            var colCount = t.Columns.Count;
            for (int i = 0; i < colCount; i++)
            {
                if (i < (colCount - 1))
                {
                    csvData.Append(t.Columns[i].ColumnName + sep);
                }
                else
                {  //no separator on the last column
                    csvData.Append(t.Columns[i].ColumnName);
                }

            }

            csvData.Append(retChar);
            for (int i = 0; i < t.Rows.Count; i++)
            {
                for (int k = 0; k < colCount; k++)
                {
                    if (k == 0)
                    {
                        csvData.Append(t.Rows[i].ItemArray[k].ToString() + sep);
                    }
                    else if (k < colCount - 1)
                    {
                        csvData.Append('\"' + t.Rows[i].ItemArray[k].ToString() + '\"' + sep);
                    }
                    else
                    {
                        csvData.Append('\"' + t.Rows[i].ItemArray[k].ToString() + '\"');
                    }
                }

                if (i < (t.Rows.Count - 1))
                {  //don't add a return character on the last line
                    csvData.Append(retChar);
                }
            }
            return csvData.ToString();
        }

        #region Unused or obsolete functions

        #endregion
    }
}