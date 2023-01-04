using System.Web.UI.WebControls;
using System.Drawing;
using System;
using System.Web.Configuration;

namespace SWUReporting
{
    public class SharedTools
    {
        public static void FormatGridViewRows(int columnID, string value, GridViewRowEventArgs e, Color color)
        {
            if (e.Row.Cells[columnID].Text == value)
            {
                e.Row.ForeColor = color;
            }
        }
        public static bool IsQ4()
        {
            //first check for the bypass setting            
            string t = WebConfigurationManager.AppSettings["calculateFTEVals"];
            if (t == "0") { return true; }  //this will bypass (faking Q4 behavior)
            DateTime dt = DateTime.Today;
            bool isQ4 = dt.Month > 9 ? true : false;
            return isQ4;
        }
    }
}