using System.Web.UI.WebControls;
using System.Drawing;

namespace SWUReporting.Data
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
    }
}