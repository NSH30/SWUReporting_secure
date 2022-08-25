using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Text;

namespace SWUReporting
{
    public partial class TransposedReport : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnDownload_Click(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                //set the sql parameters based on the user selections
                SWUTable.SelectParameters["VARFilter"].DefaultValue = txtVARFilter.Text;
                if (chkReseller.Checked & !chkEmployee.Checked)
                {
                    SWUTable.SelectParameters["ProfileFilter"].DefaultValue = "resell%";
                }
                else if (chkEmployee.Checked & !chkReseller.Checked)
                {
                    SWUTable.SelectParameters["ProfileFilter"].DefaultValue = "empl%";
                }
                else if (chkEmployee.Checked & chkReseller.Checked)
                {
                    SWUTable.SelectParameters["ProfileFilter"].DefaultValue = "%";
                }
                else
                {
                    //what happens if neither checkbox is checked??
                    //TODO: tell the user to select at least one.
                }
                
                var t = SWUTable;
                DataView dv = (DataView)SWUTable.Select(DataSourceSelectArguments.Empty);
                var ta = dv.Table;
                var output = tableToCSV(ta);
                //download the results
                Response.AddHeader("Content-disposition", "attachment; filename=Report.csv");
                Response.ContentType = "text/csv; charset=UTF-8";
                Response.Write(output);
                Response.End();
            }
            
        }

        private string tableToCSV(DataTable t)
        {
            StringBuilder csvData = new StringBuilder();
            var colCount = t.Columns.Count;
            for (int i = 0; i < colCount; i++)
            {
                csvData.Append(t.Columns[i].ColumnName + ',');
            }

            csvData.Append("\r\n");
            for (int i = 0; i < t.Rows.Count; i++)
            {
                for (int k = 0; k < colCount; k++)
                {
                    if (k == 0)
                    {
                        csvData.Append(t.Rows[i].ItemArray[k].ToString() + ',');  
                    }
                    else
                    {
                        csvData.Append('\"' + t.Rows[i].ItemArray[k].ToString() + '\"' + ',');  
                    }

                                 
                }
                csvData.Append("\r\n");
            }
            return csvData.ToString();
        }
    }
}