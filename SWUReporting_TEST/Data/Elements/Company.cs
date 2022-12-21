using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ReportBuilder

{

    public class Company
    {
        #region Properties


        public int ID { get; set; }
        public string Name { get; set; }
        public int? AliasId { get; set; }  //not needed here?
        public int? ParentNameId { get; set; }  //not needed here?

        public string AliasVal { get; set; }

        public string ParentVal { get; set; }

        public int status { get; set; }
        private DB db { get; set; }
        #endregion

        #region Constructors
        public Company(DB dbIn)
        {
            db = dbIn;
        }


        public Company(string companyName, DB dbIn)
        {
            db = dbIn;
            Name = companyName;
            if (!Insert())
            {
                GetByID(ID);
            }
        }
        #endregion

        public bool GetByName(string companyName)
        {
            SqlCommand cmd;
            string query = @"SELECT VARs.ID, VARs.Name, VARs.var_parent_id, VARs.var_alias_id, VARAlias.status, 
                            VARAlias.VAR_Alias, VARParents.VAR_Parent 
                            from VARs
                            full outer join VARAlias on VARAlias.ID = VARs.var_alias_id
                            full outer join VARParents on VARParents.ID = VARs.var_parent_id
                            where VARs.Name = @varName;";     
            cmd = new SqlCommand(query, db.dbConn);
            cmd.Parameters.AddWithValue("@varName", companyName);
            SqlDataReader r = cmd.ExecuteReader();
            if (r.HasRows)
            {
                while (r.Read())
                {
                    ID = r.GetInt32(0);
                    Name = r.GetString(1);
                    if (!(r.GetValue(2) is DBNull) && r.GetInt32(2) > 0)
                    {
                        ParentNameId = r.GetInt32(2);
                        ParentVal = r.GetString(6);
                    }
                    if ((!(r.GetValue(3) is DBNull)) && (r.GetInt32(3) != -1))
                    {
                        AliasId = r.GetInt32(3);
                        AliasVal = r.GetString(5);
                    }
                    if (!(r.GetValue(4) is DBNull))
                    {
                        status = r.GetInt32(4);
                    }
                }
            }
            else
            {
                r.Close();
                return false;
            }
            r.Close();
            return true;
        }
        public bool GetByID(int companyID)
        {
            SqlCommand cmd;
            string query = @"SELECT VARs.ID, VARs.Name, VARs.var_parent_id, VARs.var_alias_id, VARAlias.status, 
                            VARAlias.VAR_Alias, VARParents.VAR_Parent 
                            from VARs
                            full outer join VARAlias on VARAlias.ID = VARs.var_alias_id
                            full outer join VARParents on VARParents.ID = VARs.var_parent_id
                            where VARs.ID = @varID;";
            cmd = new SqlCommand(query, db.dbConn);
            cmd.Parameters.AddWithValue("@varID", companyID);
            SqlDataReader r = cmd.ExecuteReader();
            if (r.HasRows)
            {
                while (r.Read())
                {
                    ID = r.GetInt32(0);
                    Name = r.GetString(1);
                    if ((!(r.GetValue(2) is DBNull)) && (r.GetInt32(2) != -1))
                    {
                        ParentNameId = r.GetInt32(2);
                        if (!(r.GetValue(6) is DBNull))  //if VAR has a parent ID, but parent has been deleted, this failed
                            ParentVal = r.GetString(6);
                    }
                    if ((!(r.GetValue(3) is DBNull)) && (r.GetInt32(3) != -1))
                    {
                        AliasId = r.GetInt32(3);
                        if(!(r.GetValue(5) is DBNull))  //there might be a var with an aliasID, but the alias has been deleted
                            AliasVal = r.GetString(5);
                    }
                    if (!(r.GetValue(4) is DBNull))
                    {
                        status = r.GetInt32(4);
                    }
                }
            }
            r.Close();
            return true;
        }

        public bool Insert()
        {
            //query - check if exists, if not, insert, return ID, return AliasID and ParentNameID - if empty, user intervention needed    
            //defaults to active status for a new VAR name        
            string query = @"DECLARE @CompanyID as int, @Added as int = 0;
                            set @CompanyID = (select ID from VARs where [Name] = @Name);
                            IF  @CompanyID IS NULL
                            BEGIN
                               set @Added = 1;
                               insert into VARs(Name, status) 
                               values(@Name, 1)
                               SET @CompanyID = scope_identity();
                            END
                            select @CompanyID AS OutputID;
                            select @Added AS AddedCount;";
            SqlCommand cmd = new SqlCommand(query, db.dbConn);
            //parameters            
            cmd.Parameters.AddWithValue("@Name", Name);
            //var r = cmd.ExecuteScalar();
            int r = -1;
            int count = 0;
            SqlDataReader reader = cmd.ExecuteReader();
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
        public string Update()
        {
            SqlCommand cmd;
            string message = "";
            string query = @"UPDATE VARs
                             SET var_alias_id = @alias_id, ";

            cmd = new SqlCommand(query, db.dbConn);
            cmd.Parameters.AddWithValue("@alias_id", AliasId);
            if (ParentNameId != null)
            {
                query = query + "var_parent_id = @parent_id,";
                cmd.Parameters.AddWithValue("@parent_id", ParentNameId);
            }
            query = query + "status = @status WHERE VARs.ID = @varID;";
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@varID", ID);
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
                message = e.Message;
            }
            return message;
        }

    }
}