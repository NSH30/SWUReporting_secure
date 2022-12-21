using ReportBuilder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SWUReporting_TEST
{
    public partial class EditCustomTargets : System.Web.UI.Page
    {
        protected static int companyID;
        protected static int courseID;
        protected static int fteTarget;
        protected static int fteValue;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Messaging.SendAlert("not postback", Page);
            }
            else
            {
                //Messaging.SendAlert("postback", Page);
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            DB db = new DB();
            db.Connect();        
            DataTable dt =  db.GetVARList(-1, string.Format("%{0}%", tbSearch.Text));
            db.Disconnect();
            gvAlias.DataSource = dt;
            gvAlias.DataBind();
            gvAlias.Visible = true;
        }

        protected void lnkTargets_Click(object sender, EventArgs e)  //clicked Edit/Add Custom Targets
        {
            //load the gridview and show it
            companyID = Convert.ToInt32((sender as LinkButton).CommandArgument);
            RefreshCustomTargets();
        }

        void RefreshCustomTargets()
        {
            DB db = new DB();
            db.Connect();
            DataTable dt = db.GetCustomTargets(varID: companyID);
            
            //get fte value for the company
            Alias a = new Alias(companyID, db);
            //DataTable ftes = db.GetFTEsByVAR(varFilter: a.Name);
            DataTable ftes = db.GetFTEVals6(varFilter: a.Name);
            db.Disconnect();
            //set Custom FTE Target = FTE Value * FTETarget column
            fteValue = Convert.ToInt32(ftes.Rows[0][2]);
            dt.Columns.Add("FTETarget", typeof(int), String.Format("[Custom Target] * {0}", fteValue));
            //dt["FTETarget"] = String.Format("Custom * {0}", ftes.Rows[0][2]);
            gvTargets.DataSource = dt;
            gvTargets.DataBind();
            gvTargets.Visible = true;
        }

        protected void lnkCustomTarget_Click(object sender, EventArgs e)
        {
            GetArgs(sender);
            //is there already a custom target???
            DB db = new DB();
            db.Connect();
            CustomTargets ct = new CustomTargets(db);
            
            ct.GetByID(companyID, courseID);
            db.Disconnect();
            if (ct.Target > 0)
            {
                //tbCustomTarget.Text = ct.Target.ToString();
                tbCustomFTETarget.Text = fteTarget.ToString();
            }
            else
            {
                //tbCustomTarget.Text = "";
                tbCustomFTETarget.Text = "";
            }
            //do we still have the companyID??
            //did this at the beginning of the procedure
            //string[] arg = new string[2];
            //arg = (sender as LinkButton).CommandArgument.ToString().Split(';');
            //companyID = Convert.ToInt32(arg[0]); //Convert.ToInt32((sender as LinkButton).CommandArgument);
            //courseID = Convert.ToInt32(arg[1]);

            //load a popup panel to edit the custom target value and save it
            //need the companyID and the CourseID here to use in the modal popup return
            mpe9.Show();
        }

        private void GetArgs(object sender)
        {
            //string[] arg = new string[];
            var arg = (sender as LinkButton).CommandArgument.ToString().Split(';');
            companyID = Convert.ToInt32(arg[0]); //Convert.ToInt32((sender as LinkButton).CommandArgument);
            courseID = Convert.ToInt32(arg[1]);
            if (arg[2] != "")
            {
                fteTarget = Convert.ToInt32(arg[2]);
            }

            
        }

        //save edits to the custom target
        protected void lnkEditCT_Click(object sender, EventArgs e)
        {
            //new custom target = fte target divided by total current FTEs
            var newVal = Convert.ToDouble( tbCustomFTETarget.Text) / fteValue;
            //update routine
            DB db = new DB();
            db.Connect();
            CustomTargets ct = new CustomTargets(db);
            
            ct.GetByID(companyID, courseID);
            ct.Target = (float)newVal;
            ct.varAliasID = companyID;
            ct.courseAliasID = courseID;
            
            ct.Insert();  //updates the custom target
            
            //need to set VARAlias.custom_targets = 1
            Alias a = new Alias(companyID, db);
            a.hasCustomTargets = 1;
            a.Insert();  //updates the Alias
            db.Disconnect();
            //refresh the gvTargets table
            RefreshCustomTargets();
        }

        protected void btnClose_Click(object sender, EventArgs e)
        {
            //nothing needed here??
        }
    }
}