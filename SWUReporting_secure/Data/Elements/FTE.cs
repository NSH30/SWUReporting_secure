using System;
using System.Data.SqlClient;

namespace ReportBuilder
{
    public class FTE
    {
        #region Properties
        public string contactID { get; set; }  //unique ID
        public string FTEStatus { get; set; }
        public double FTEValue { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string Account { get; set; }
        public string AccountStatus { get; set; }
        public string AccountType { get; set; }
        public string Organization { get; set; }
        public int parent_id { get; set; } //mapped from Organization
        public string GEO { get; set; }
        public int geo_id { get; set; } //mapped from Organization
        private DB db { get; set; }
        #endregion

        #region Constructor
        public FTE(DB dbIn)
        {
            db = dbIn;
        }
        #endregion
        public void Insert()
        {
            SqlCommand cmd;
            //get geo_id
            string gQuery = "SELECT [ID] FROM GEOs WHERE GEO = @GEO;";
            cmd = new SqlCommand(gQuery, db.dbConn);
            cmd.Parameters.AddWithValue("@GEO", GEO);
            var geo = cmd.ExecuteScalar();
            //validate return
            if (geo != null)  //something about geo
            {
                geo_id = Convert.ToInt32(geo);
            }

            //get parent_id
            string pQuery = "SELECT [ID] FROM VARParents WHERE VAR_Parent = @Org;";
            cmd = new SqlCommand(pQuery, db.dbConn);
            cmd.Parameters.AddWithValue("@Org", Organization);
            var p = cmd.ExecuteScalar();
            //validate return
            if (p != null)  //something about geo
            {
                parent_id = Convert.ToInt32(p);
            }
            //query - check if exists, if not, insert, return ID   
            string query = @"DECLARE @FTEID as nvarchar(20);
                            set @FTEID = (select [Contact Id] from FTE where [Contact Id] = @FTEID);
                            IF  @FTEID IS NULL
                            BEGIN
                               insert into FTE([Contact Id], [FTE Status], [FTE Value], [First Name], 
                                    [Last Name], [Account], [Account Status], [Account Type], [Organization],
                                    [parent_id], [geo_id]) 
                               values(@ContactId, @FTEStatus, @FTEValue, @FirstName, 
                                      @LastName, @Account, @AccountStatus, @AccountType, @Organization,
                                      @parent_id, @geo_id)
                               SET @FTEID = scope_identity();
                            END
                            select @FTEID AS OutputID;";
            cmd = new SqlCommand(query, db.dbConn);
            //parameters            
            cmd.Parameters.AddWithValue("@ContactId", contactID);
            cmd.Parameters.AddWithValue("@FTEStatus", FTEStatus);
            cmd.Parameters.AddWithValue("@FTEValue", FTEValue);
            cmd.Parameters.AddWithValue("@FirstName", firstName);
            cmd.Parameters.AddWithValue("@LastName", lastName);
            cmd.Parameters.AddWithValue("@Account", Account);
            cmd.Parameters.AddWithValue("@AccountStatus", AccountStatus);
            cmd.Parameters.AddWithValue("@AccountType", AccountType);
            cmd.Parameters.AddWithValue("@Organization", Organization);
            cmd.Parameters.AddWithValue("@parent_id", parent_id);
            cmd.Parameters.AddWithValue("@geo_id", geo_id);
            cmd.ExecuteScalar();
        }

        //deletes all FTE records from the DB - only perform before importing FTEs
        public void Clear()
        {
            SqlCommand cmd;
            string query = "DELETE FROM FTE WHERE 1=1;";
            cmd = new SqlCommand(query, db.dbConn);
            cmd.ExecuteNonQuery();
        }
    }
}