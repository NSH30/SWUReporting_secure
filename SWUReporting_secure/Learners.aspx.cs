using SWUReporting;
using System;

namespace SWUReporting
{
    public partial class Learners : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
         
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            elc2.ShowResults(tbSearch.Text, DB.dataSource.search, chkShowDeleted.Checked);
            elc2.Visible = true;
        }

        protected void btnShowDuplicates_Click(object sender, EventArgs e)
        {
            //show duplicates here
            elc2.showDuplicates();
        }

        protected void btnShowUsersMissingData_Click(object sender, EventArgs e)
        {
            elc2.showMissingData();
        }

    }
}
