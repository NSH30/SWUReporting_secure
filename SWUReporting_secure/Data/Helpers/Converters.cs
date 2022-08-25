using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ReportBuilder
{
    public class Converters
    {
        public static bool Divisible(int n, int divisor)
        {
            double res = n % divisor;
            if (res == 0)
                return true;
            else
                return false;
        }
        public static DateTime? getDate(string oaDate)
        {
            if (oaDate == "")
            {
                return null;
            }
            else
            {
                double oaDateD = Convert.ToDouble(oaDate);
                return DateTime.FromOADate(oaDateD);
            }
        }

        public static string convertDateYQ(DateTime d)
        {
            var month = d.ToString("MMMM");
            var q = String.Format("Q{0}", ((d.Month - 1) / 3) + 1);
            var year = d.ToString("yyyy");
            //return month.Substring(0, 3) + " " + year;
            return year + " " + q;
        }

        public static string convertDateYM(DateTime d)
        {
            var month = d.ToString("MMMM");
            var year = d.ToString("yyyy");
            return month.Substring(0, 3) + " " + year;
        }

        public static DataTable setDateDisplay(DataTable dt)
        {
            DataRow dr = null;
            //create a new column (string)
            var c = dt.Columns.Add("months", typeof(string));
            dt.Columns["months"].SetOrdinal(1);  //move next to GEO

            for (int r = 0; r < dt.Rows.Count; r++)
            {
                dr = dt.Rows[r];
                var val = Convert.ToDateTime(dr.ItemArray[2]); //original date is in column index 2 now                
                var items = dr.ItemArray;
                items[1] = convertDateYM(val);
                dr.ItemArray = items;

            }
            dt.Columns.Remove("Month");
            dt.Columns["months"].ColumnName = "Month";
            return dt;
        }

        public static int GetQuarterDifference(string d1)
        {
            DateTime d = DateTime.Now;
            var d2 = Converters.convertDateYQ(d);
            int y1 = Convert.ToInt32(d1.Substring(0, 4));
            int y2 = Convert.ToInt32(d2.Substring(0, 4));
            int q1 = Convert.ToInt32(d1.Substring(6, 1));
            int q2 = Convert.ToInt32(d2.Substring(6, 1));

            if (y1 == y2)  //same year
                return q2 - q1;
            //if ((y2 - y1) >= 1)  //subsequent year

            else
                return q2 + (4 - q1) + 4 * (y2 - y1 - 1);

        }
        public static string GetNextQuarterValue(string d1)
        {
            int y1 = Convert.ToInt32(d1.Substring(0, 4));
            int q1 = Convert.ToInt32(d1.Substring(6, 1));
            if (q1 == 4)
                return String.Format("{0} Q{1}", y1 + 1, 1);  //reset the quarter to 1 if it was Q4
            else
                return String.Format("{0} Q{1}", y1, q1 + 1);
        }

        public static bool IsNextQuarter(string d1, string d2)
        {
            int y1 = Convert.ToInt32(d1.Substring(0, 4));
            int y2 = Convert.ToInt32(d2.Substring(0, 4));
            int q1 = Convert.ToInt32(d1.Substring(6, 1));
            int q2 = Convert.ToInt32(d2.Substring(6, 1));
            if ((q2 - q1) == 1 && y1 == y2)
                return true;
            if ((q2 - q1) == -3 && (y2 - y1) == 1)
                return true;
            else
                return false;
        }

        public static Decimal? getDecimalFromString(string dVal)
        {
            Decimal d = 0;
            bool result = Decimal.TryParse(dVal, out d);
            if (result)
                return d;
            return 0;

            //if (dVal == "")
            //    return (Decimal)0;
            //else
            //    return Convert.ToDecimal(dVal);
        }

        public static DateTime? getDateFromString(string sDate)
        {
            if (sDate == "")
            {
                return null;
            }
            else
            {
                DateTime val;
                bool res = DateTime.TryParse(sDate, out val);
                if (res)
                {
                    return val;
                }
                else
                {
                    val = DateTime.FromOADate(Convert.ToDouble(sDate));
                    return val;
                }
            }
        }
    }
}