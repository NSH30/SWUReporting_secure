using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SWUReporting
{
    public partial class ViewActivities : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            tbEndSearchDate.Text = DateTime.Now.ToString();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            var startDate = tbSearchDate.Text;
            var searchString = tbSearchString.Text;
            string endSearchDate = tbEndSearchDate.Text;
            vac.ShowResults(startDate, searchString, endSearchDate,  ReportBuilder.DB.dataSource.search, cbCompleted.Checked);
            vac.Visible = true;
        }
    }
}