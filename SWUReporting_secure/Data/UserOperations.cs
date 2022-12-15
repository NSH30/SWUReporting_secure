using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data;
using System.Configuration;
using SWUReporting;
using System.Linq;

namespace ReportBuilder
{
    public class UserOperations
    {
        public static DB db;

        public DataTable GetUsersSearchres(string filter)
        {
            string query;
            query = @"select [Users].[Fname], [lname], [Trigram], [Admin] from Users
                        where ([Users].[Fname] like @filter
                        or lname like @filter
                        or Trigram like @filter";
            SqlCommand cmd = new SqlCommand(query, db.dbConn);
            cmd.Parameters.AddWithValue("@filter", filter);
            return db.TableQuery(cmd, query);
        }

        
        
    }
}