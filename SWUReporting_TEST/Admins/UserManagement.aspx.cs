using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Data;
using System.Configuration;
using ReportBuilder;

namespace SWUReporting_TEST
{
    public partial class UserManagement : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void Btn_Submit(object sender, EventArgs e)
        {            
            string Admin = string.Empty;
            if (rbYes.Checked)
            {
                Admin = "1";
            }
            else if (rbNo.Checked)
            {
                Admin = "0";
            }
            try
            {
                DB db = new DB();
                db.Connect();
                String add_user = "Insert into dbo.Users (Fname,Lname,Trigram,Admin) values(@Fname, @Lname, @Trigram, @Admin)";
                SqlCommand cmd = new SqlCommand(add_user, db.dbConn);
                cmd.Parameters.AddWithValue("@Fname", fname_text.Text);
                cmd.Parameters.AddWithValue("@Lname", lname_text.Text);
                cmd.Parameters.AddWithValue("@Trigram", Trig_text.Text);
                cmd.Parameters.AddWithValue("@Admin", Admin);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                Response.Write("User Added Sucessfully");

            }
            catch (Exception ex)
            {
                
            }
            
        }

    }
}