using SWUReporting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SWUReporting
{

    public class Learner
    {
        #region fields
        string country;
        #endregion
        #region Properties
        public int ID { get; set; }
        public string Name { get; set; }
        public string email { get; set; }
        public Company company { get; set; }
        public string Region { get; set; }
        public string Country {
            get
            {
                return country;
            }
            set
            {
                country = value;
                country_id = db.GetCountryID(country);
            }
        }
        public string GEO { get; set; }
        public string userState { get; set; }
        public string Profile { get; set; }
        public string Role { get; set; }

        public int region_id { get; set; }
        public int geo_id { get; set; }

        public int country_id { get; set; }
        public double fte_value { get; set; }
        private DB db { get; set; }
        #endregion

        #region Constructor
        public Learner(DB dbIn)
        {
            db = dbIn;
        }
        #endregion

        private void syncCountryAndGEO()
        {
            //learners with country and no GEO
            string query = string.Format("SELECT geo_id from Countries WHERE ID = {0};", country_id);
            geo_id = db.SingleValueQuery(query);
        }
        protected void GetCompanyByID(int companyID)
        {
            Company c = new Company(db);
            c.GetByID(companyID);
            company = c;
        }

        public void GetLearnerByEmail(string email, SqlConnection db)
        {
            SqlCommand cmd;
            string query = @"SELECT Learners.ID, Learners.Name, email, Role, Country, profile, userState, var_id, GEOs.GEO, country_id, fte_value, Learners.geo_id
                            from Learners 
                            join GEOs on GEOs.ID = Learners.geo_id
                            where Learners.[email] = @email;";
            cmd = new SqlCommand(query, db);
            cmd.Parameters.AddWithValue("@email", email);
            SqlDataReader r = cmd.ExecuteReader();
            int varID = -1;  //initialize with an invalid value
            if (r.HasRows)
            {
                while (r.Read())
                {
                    ID = r.GetInt32(0);
                    Name = r.GetString(1);
                    this.email = email;
                    if (!(r.GetValue(3) is DBNull))
                    {
                        Role = r.GetString(3);
                    }
                    Country = r.GetString(4);
                    Profile = r.GetString(5);
                    userState = r.GetString(6);
                    //company info collected by this method
                    varID = r.GetInt32(7);
                    GEO = r.GetString(8);
                    if (!r.IsDBNull(r.GetOrdinal("geo_id")))
                    {
                        geo_id = (int)r["geo_id"];
                    }
                    if (!r.IsDBNull(r.GetOrdinal("country_id")))
                    {
                        country_id = (int)r["country_id"];
                    }
                    fte_value = decimal.ToDouble((decimal)r["fte_value"]);
                }
            }
            r.Close();
            GetCompanyByID(varID);
        }

        public bool GetLearnerByID(int learnerID)
        {
            SqlCommand cmd;
            string query = @"SELECT Learners.ID, Learners.Name, email, Role, Country, profile, userState, var_id, GEOs.GEO, country_id, fte_value, Learners.geo_id
                            from Learners 
                            left outer join GEOs on GEOs.ID = Learners.geo_id
                            where Learners.[ID] = @learnerID;";
            cmd = new SqlCommand(query, db.dbConn);
            cmd.Parameters.AddWithValue("@learnerID", learnerID);
            SqlDataReader r = cmd.ExecuteReader();
            int varID = -1;  //initialize with an invalid value
            if (r.HasRows)
            {
                while (r.Read())
                {
                    ID = r.GetInt32(0);
                    Name = r.GetString(1);
                    email = r.GetString(2);
                    if (!(r.GetValue(3) is DBNull))
                    {
                        Role = r.GetString(3);
                    }
                    Country = r.GetString(4);
                    Profile = r.GetString(5);
                    userState = r.GetString(6);
                    //company info collected by this method
                    if (!r.IsDBNull(r.GetOrdinal("var_id")))
                    {
                        varID = (int)r["var_id"];
                    }
                    if (!r.IsDBNull(r.GetOrdinal("GEO")))
                    {
                        GEO = (string)r["GEO"];
                    }

                    if (!r.IsDBNull(r.GetOrdinal("geo_id")))
                    {
                        geo_id = (int)r["geo_id"];
                    }
                    if (!r.IsDBNull(r.GetOrdinal("country_id")))
                    {
                        country_id = (int)r["country_id"];
                    }
                    fte_value = decimal.ToDouble((decimal)r["fte_value"]);
                }
            }
            else
            {
                return false;
            }
            r.Close();
            GetCompanyByID(varID);
            return true;
        }

        public string Update()
        {
            SqlCommand cmd;
            string message = "";
            string query = @"UPDATE Learners
                             SET Name = @name,
                             email = @email,
                             Role = @role,
                             profile = @profile,
                             userState = @userstate,
                             Country = @country,
                             geo_id = @geo_id,    
                             var_id = @varID, country_id = @countryID,
                             fte_value = @fteVal                  
                             WHERE Learners.ID = @learnerID;";
            cmd = new SqlCommand(query, db.dbConn);
            cmd.Parameters.AddWithValue("@name", Name);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@profile", Profile);
            cmd.Parameters.AddWithValue("@userstate", userState);
            cmd.Parameters.AddWithValue("@country", Country);
            cmd.Parameters.AddWithValue("@role", Role);
            cmd.Parameters.AddWithValue("@learnerID", ID);
            cmd.Parameters.AddWithValue("@geo_id", geo_id);
            cmd.Parameters.AddWithValue("@GEO", GEO);
            cmd.Parameters.AddWithValue("@varID", company.ID);
            cmd.Parameters.AddWithValue("@countryID", country_id);
            cmd.Parameters.AddWithValue("@fteVal", Convert.ToDecimal(fte_value));
            try
            {
                var res = cmd.ExecuteNonQuery();
                if (res == 0)
                {
                    message = "Nothing was updated.";
                }
            }
            catch (Exception e)
            {
                //do something with query errors
                message = "ERROR: " + e.Message;
            }
            return message;
        }

        public bool Insert()
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
            if (region_id == 0 && geo_id > 0)
            {
                List<int> americas = new List<int> { 9, 10 };
                List<int> emear = new List<int> { 3, 4, 5, 6, 11 };
                List<int> asia = new List<int> { 1, 2, 7, 8, 12 };
                if (americas.Contains(geo_id)) { region_id = 1; }
                if (emear.Contains(geo_id)) { region_id = 2; }
                if (asia.Contains(geo_id)) { region_id = 3; }
            }
            //get country_id
            string cQuery = "SELECT ID FROM Countries WHERE Country = @Country;";
            cmd = new SqlCommand(cQuery, db.dbConn);
            cmd.Parameters.AddWithValue("@Country", Country);
            var ctry = cmd.ExecuteScalar();
            //validate return
            if (ctry != null)  //something about region
            {
                country_id = Convert.ToInt32(ctry);
            }

            //make sure the GEO is valid for the country
            syncCountryAndGEO();

            //query - check if exists, if not, insert, return ID, return AliasID and ParentNameID - if empty, user intervention needed            
            //updated 3/25/2021 to find existing learners by email address only (used to be where [Name] = @Name AND [email] = @email;)
            //this will not handle email changes and will create duplicate learners
            //but that is less likely than name changes
            //December 30, 2021, removed learner updates of userState and Role from Prime.  The reporting database becomes the master.
            string query = @"DECLARE @LearnerID as int, @Added as int = 0, @ftevalue as decimal(5,1) = 0 ;
                            set @LearnerID = (select ID from Learners where [email] = @email);
                            IF  @LearnerID IS NULL
                            BEGIN
                               set @Added = 1;                               
                               insert into Learners(Name, email, Country, userState, Role, Profile, var_id, region_id, geo_id, country_id, fte_value) 
                               values(@Name, @email, @Country, @userState, @Role, @Profile, @VAR_ID, @region_id, @geo_id, @country_id, @fteValue)
                               SET @LearnerID = scope_identity();
                               select @LearnerID as OutputID;
                            END
                            ELSE
                            /*BEGIN UPDATE Learners SET userState = @userState, Role = @Role WHERE ID = @LearnerID; END*/
                            select @LearnerID AS OutputID;
                            select @Added as AddedCount;";
            cmd = new SqlCommand(query, db.dbConn);
            //parameters                      
            cmd.Parameters.AddWithValue("@Name", Name);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@Country", Country);
            if (userState == null) { userState = "ACTIVE"; }
            cmd.Parameters.AddWithValue("@userState", userState);
            if (Profile == null) { Profile = "reseller"; }
            cmd.Parameters.AddWithValue("@Profile", Profile);
#if DEBUG
            if (company.ID < 1)
            {
                System.Diagnostics.Debug.Print("ERROR: No company ID for learner " + Name);
            }
#endif 
            cmd.Parameters.AddWithValue("@VAR_ID", company.ID);
            cmd.Parameters.AddWithValue("@region_id", region_id);
            cmd.Parameters.AddWithValue("@geo_id", geo_id);
            cmd.Parameters.AddWithValue("@country_id", country_id);

            //cmd.Parameters.AddWithValue("@fteValue", fte_value);  //removed 6/21/2021 - learner transcript import was clearing FTE values.
            if (Role == null)
            {
                cmd.Parameters.AddWithValue("@Role", DBNull.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue("@Role", Role);
            }
            int r = -1;
            int count = 0;
            SqlDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (Exception e)
            {
                //add a return value with exception message?
                Messaging.LogErrorToText(e);
                return false;
            }

            while (reader.Read())
            {
                r = (int)reader.GetValue(0);
            }

            reader.NextResult();

            while (reader.Read())
            {
                count = (int)reader.GetValue(0);
            }
            reader.Close();
            //collect response and set ID to new ID            
            ID = Convert.ToInt32(r);
            if (count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}