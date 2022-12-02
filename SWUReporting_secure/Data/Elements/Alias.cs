using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SWUReporting
{

    public class Alias
    {
        #region Properties
        int id;
        public int ID { get { return id; } }
        public string Name { get; set; }
        public int status { get; set; }
        public int geoID { get; set; }
        public string geoVal { get; set; }
        public int hasCustomTargets { get; set; }
        public DateTime? DashboardDate { get; set; }
        public List<CustomTargets> targets { get; set; }
        public DateTime? DashboardCommunicatedDate { get; set; }
        public int Contract3DX { get; set; }
        public int ContractDW { get; set; }
        public int EDUOnly { get; set; }
        private DB db { get; set; }
        #endregion

        #region Constructors

        public Alias(DB dbIn) { db = dbIn;}
        public Alias(int aliasID, DB dbIn)
        {
            db = dbIn;
            GetByID(aliasID);
        }

        public Alias(string aliasName, DB dbIn)
        {
            db = dbIn;
            GetByName(aliasName);
        }

        #endregion
        public void Insert()
        {
            if (ID > 0)
            {
                //it exists, so update
                string query = @"update VARAlias
                                set VAR_Alias = @name,
                                geo_id = @geoID, status = @status, custom_targets = @customTargets, DashboardDate = @dashboardDate, CommunicationDate = @communicationDate
                                , [3dx] = @3dx, DW = @DW, EDU = @edu
                                WHERE ID = @id;";
                SqlCommand cmd = new SqlCommand(query, db.dbConn);
                //parameters            
                cmd.Parameters.AddWithValue("@name", Name);
                cmd.Parameters.AddWithValue("@geoID", geoID);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@customTargets", hasCustomTargets);
                cmd.Parameters.AddWithValue("@id", ID);
                cmd.Parameters.AddWithValue("@dashboardDate", DashboardDate ?? Convert.DBNull);
                cmd.Parameters.AddWithValue("@communicationDate", DashboardCommunicatedDate ?? Convert.DBNull);
                cmd.Parameters.AddWithValue("@3dx", Contract3DX);
                cmd.Parameters.AddWithValue("@DW", ContractDW);
                cmd.Parameters.AddWithValue("@edu", EDUOnly);
                dynamic r = null;
                r = cmd.ExecuteScalar();
            }
            else
            {
                //doesn't yet exist
                string query = @"DECLARE @id INT;
                                insert into VARAlias(VAR_Alias, geo_id, status, custom_targets, DashboardDate, CommunicationDate) 
                               values(@name, @geoID, @status, @customTargets, @dashboardDate, @communicationDate)
                               SET @id = scope_identity();
                               SELECT @id as OutputID;";
                SqlCommand cmd = new SqlCommand(query, db.dbConn);
                //parameters            
                cmd.Parameters.AddWithValue("@name", Name);
                cmd.Parameters.AddWithValue("@geoID", geoID);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@customTargets", hasCustomTargets);
                cmd.Parameters.AddWithValue("@dashboardDate", DashboardDate ?? Convert.DBNull);
                cmd.Parameters.AddWithValue("@communicationDate", DashboardCommunicatedDate ?? Convert.DBNull);
                dynamic r = null;
                r = cmd.ExecuteScalar();
                id = Convert.ToInt32(r);
            }

            if (targets != null && targets.Count > 0)
            {
                //insert or update the targets
                foreach (var target in targets)
                {
                    target.Insert();
                }
            }
        }

        public bool GetByName(string aliasName)
        {
            SqlCommand cmd;
            string query = @"SELECT VARAlias.ID, VAR_Alias, geo_id, status, custom_targets, GEO, DashboardDate, [3dx], DW, EDU
                            from VARAlias
                            full outer join GEOs on GEOs.ID = geo_id                            
                            where VARAlias.VAR_Alias = @varName;";
            cmd = new SqlCommand(query, db.dbConn);
            cmd.Parameters.AddWithValue("@varName", aliasName);
            SqlDataReader r = cmd.ExecuteReader();
            if (r.HasRows)
            {
                while (r.Read())
                {
                    id = r.GetInt32(0);
                    Name = r.GetString(1);
                    geoID = r.GetInt32(2);
                    status = r.GetInt32(3);
                    hasCustomTargets = r.GetInt32(4);
                    if (!r.IsDBNull(5))
                        geoVal = r.GetString(5);

                    if (!r.IsDBNull(6))
                        DashboardDate = (DateTime)r["DashboardDate"];
                    if (!r.IsDBNull(7))
                        Contract3DX = r.GetInt32(7); //3DX
                    if (!r.IsDBNull(8))
                        ContractDW = r.GetInt32(8);  //DW
                    if (!r.IsDBNull(9))
                        ContractDW = r.GetInt32(9);  //EDU
                }
                r.Close();
                return true;
            }
            r.Close();
            return false;
        }

        public bool GetByID(int aliasID)
        {
            SqlCommand cmd;
            string query = @"SELECT VARAlias.ID, VAR_Alias, geo_id, status, custom_targets, GEO, DashboardDate, [3dx], DW, EDU
                            from VARAlias
                            full outer join GEOs on GEOs.ID = geo_id                            
                            where VARAlias.ID = @varID;";
            cmd = new SqlCommand(query, db.dbConn);
            cmd.Parameters.AddWithValue("@varID", aliasID);
            SqlDataReader r = cmd.ExecuteReader();
            if (r.HasRows)
            {
                while (r.Read())
                {
                    id = r.GetInt32(0);
                    Name = r.GetString(1);
                    geoID = r.GetInt32(2);
                    status = r.GetInt32(3);
                    hasCustomTargets = r.GetInt32(4);
                    if (!r.IsDBNull(5))
                        geoVal = r.GetString(5);

                    if (!r.IsDBNull(6))
                        DashboardDate = (DateTime)r["DashboardDate"];

                    if (!r.IsDBNull(7))
                        Contract3DX = r.GetInt32(7);  //3dx
                    if (!r.IsDBNull(8))
                        ContractDW = r.GetInt32(8);  //DW
                    if (!r.IsDBNull(9))
                        ContractDW = r.GetInt32(9);  //EDU
                }
            }
            r.Close();
            if (hasCustomTargets == 1)
            {
                //get all the custom targets                
                query = @"select ID, Target, course_alias_id, var_alias_id 
                        FROM CustomTargets
                        WHERE var_alias_id = @varID;";
                cmd = new SqlCommand(query, db.dbConn);
                cmd.Parameters.AddWithValue("@varID", id);
                r = cmd.ExecuteReader();
                if (r.HasRows)
                {
                    while (r.Read())
                    {
                        CustomTargets t = new CustomTargets(db);
                        t.ID = r.GetInt32(0);
                        t.Target = (float)(double)r[1];
                        t.courseAliasID = r.GetInt32(2);
                        t.varAliasID = r.GetInt32(3);
                        if (targets == null)
                        {
                            targets = new List<CustomTargets>();
                        }
                        targets.Add(t);
                    }
                }
                r.Close();
            }
            return true;
        }
    }
}