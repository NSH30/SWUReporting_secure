using System;
using System.Data;

namespace ReportBuilder
{
    public class TableTools
    {
        public enum Type
        {
            tableSalesTransposed,
            tableTechTransposed
        }
        public static string[,] GetArrayFromTable(DataTable dt)
        {

            //List<string[,]> myTable = new List<string[,]>();
            int rowCount = 0;
            int columnCount = 0;

            string[,] myTableArr = new string[dt.Rows.Count + 1, dt.Columns.Count];
            //add headers first
            foreach (DataColumn dcH in dt.Columns)
            {
                myTableArr[rowCount, columnCount] = dcH.ToString();
                columnCount++;
            }
            rowCount++;
            //add subsequent row details
            foreach (DataRow dr in dt.Rows)
            {
                columnCount = 0;
                foreach (DataColumn dc in dt.Columns)
                {
                    myTableArr[rowCount, columnCount] = dr[dc.ToString()].ToString();
                    columnCount++;
                }
                rowCount++;
            }
            return myTableArr;

        }

        public static int getHeaderIndex(string[] vals, string value)
        {
            int i = Array.IndexOf(vals, value);
            return i;
        }
    }
}