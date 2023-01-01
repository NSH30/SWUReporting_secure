using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data;
using System.Configuration;
using SWUReporting;
using System.Linq;

namespace SWUReporting
{
    public class DB
    {

        #region fields
        DataTable geos;
        DataTable courseNames;
        public SqlConnection dbConn;
        public ReportType reportType;
        #endregion

        #region Properties
        public DataTable Geos
        {
            get
            {
                if (geos == null)
                {
                    geos = GetGEOList();
                }
                return geos;
            }
        }
        public DataTable CourseNames
        {
            get
            {
                if (courseNames == null)
                {
                    courseNames = GetCourseAliasList();
                }
                return courseNames;
            }
        }

        private DataTable AlignmentData { get; set; }

        public string TempTableName { get; set; }
        public string TempVTTableName { get; set; }
        

        public int SalesAlignmentGoal
        {
            get
            {
                string query = @"select Sales from AlignmentTargets;";
                return SingleValueQuery(query);
            }
            set
            {
                string query = "UPDATE AlignmentTargets SET Sales = @value;";
                SimpleQuery(query);
            }
        }

        public int TechSalesAlignmentGoal
        {
            get
            {
                string query = @"select TechSales from AlignmentTargets;";
                return SingleValueQuery(query);
            }
            set
            {
                string query = String.Format("UPDATE AlignmentTargets SET TechSales = {0};", value);
                SimpleQuery(query);
            }
        }
        #endregion

        #region Enums

        public enum KPIRole
        {
            //none = 0,
            sales = 1,
            techsales = 2
        }
        public enum dataSource
        {
            search,
            import
        }

        public enum VARGrouping
        {
            byVARAlias,
            byVARParent
        }

        public enum ReportType
        {
            VAR, GEO
        }

        public enum rbTables
        {
            VARnames = 0,
            Learners = 1,
            Courses = 2,
            VARParents = 3,
            Activities = 4
        }
        #endregion

        #region connections
        public void Connect()
        {
            if (dbConn == null || dbConn.State != System.Data.ConnectionState.Open)
            {
                //string connString = ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
                string connString = ConfigurationManager.ConnectionStrings["SWUdb"].ConnectionString;
#if DEBUG
                connString = ConfigurationManager.ConnectionStrings["SWUdbDebug"].ConnectionString;
#endif
                dbConn = new SqlConnection(connString);
                dbConn.Open();
            }
        }
        public void Disconnect()
        {
            if (dbConn != null)
            {
                deleteTempTable(TempTableName);
                deleteTempTable(TempVTTableName);

                dbConn.Close();
                dbConn.Dispose();
            }
        }

        public void deleteTempTable(string tableName)
        {
            if (!String.IsNullOrEmpty(tableName))
            {
                //drop temp table if it exists
                string query = "IF OBJECT_ID (N'" + tableName + "', N'U') IS NOT NULL DROP TABLE " + tableName + ";";
                var res = SimpleQuery(query);
            }
        }
        #endregion

        #region Constructors
        public DB()
        {
            Connect();
        }

        /// <summary>
        /// Primary constructor
        /// </summary>
        /// <param name="tempTableName">Temp table name structure</param>
        public DB(string tempTableName)
        {
            Connect();
            TempTableName = tempTableName;
            TempVTTableName = TempTableName + "VT";
            
            //delete the temp table(s) in case it already exists
            deleteTempTable(TempTableName);
            deleteTempTable(TempVTTableName);

        }
        #endregion

        #region base SqlCommands
        public int SimpleQuery(string query) 
        {
            SqlCommand cmd = new SqlCommand(query, dbConn);
            cmd = new SqlCommand(query, dbConn);
            var rows = cmd.ExecuteNonQuery();
            return rows;
        }
        public int SimpleQuery(string query,SqlCommand cmd)
        {            
            var rows = cmd.ExecuteNonQuery();
            return rows;
        }

        public int SingleValueQuery(string query)
        {
            int count;
            SqlCommand cmd = new SqlCommand(query, dbConn);
            var r = cmd.ExecuteScalar();
            count = Convert.ToInt32(r);
            return count;
        }

        internal DataTable GetDraftSightTransposedTable(string varAlias = "%")
        {
            string courseName = "%draftsight%";
            string query = @"declare @courses nvarchar(max), @query1 nvarchar(max), @query2 nvarchar(max);
                                                        set @courses = STUFF(
                                (SELECT DISTINCT ','+QUOTENAME([CourseNameAlias])
                                FROM [CourseAlias] c WHERE c.CourseNameAlias LIKE ''+@courseName+'' FOR XML PATH('')), 1, 1, '');
                            set @query1 = 'select * from
                                (SELECT Learners.Name, Learners.email, Learners.Role, null [FTE Value], VARAlias.VAR_Alias as [VAR Alias], 
	                            VARs.Name as Company, Learners.Country, GEOs.GEO, null [TotalAchievement], CourseAlias.CourseNameAlias as [Course] --Courses.Name as [Course]
	                            , CASE WHEN MAX(Activities.status) = ''Unenrolled'' THEN null  WHEN MAX(completionDate) is null THEN ''In Progress'' ELSE convert(varchar,MAX(completionDate), 101) END as [Completion Date]
	                            --, Activities.completionDate as ''Completion Date'' 
                                FROM Activities join Learners on Learners.ID = Activities.learner_id
                                join Courses on Courses.ID = Activities.course_id '
                            set @query2 = 'join GEOs on GEOs.ID = Learners.geo_id
                                join VARs on VARs.ID = Learners.var_id
	                            join VARAlias on VARAlias.id = vars.var_alias_id
	                            join CourseAlias on CourseAlias.ID = Courses.alias_id                                
	                            where CourseAlias.CourseNameAlias LIKE '''+@courseName+''' 
                                AND Learners.userState = ''ACTIVE''
                                AND (Activities.status = ''Completed'' OR Activities.status = ''In Progress'' OR Activities.status = ''Unenrolled'')
                                AND VAR_Alias like '''+@varAlias+'''
	                            Group by Learners.ID, Learners.Name, Learners.email, Learners.[Role], VARAlias.VAR_Alias, VARs.Name, 
		                            Learners.Country, GEOs.GEO, CourseAlias.CourseNameAlias --Courses.Name
                                ) as SourceData PIVOT(MAX([Completion Date]) FOR [Course] IN('+@courses+')) as PT
                                order by GEO, Company, Name'
								--print @query1 + @query2;
	                            execute (@query1 + @query2);";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            cmd.Parameters.AddWithValue("@varAlias", varAlias);
            cmd.Parameters.AddWithValue("@courseName", courseName);
            return TableQuery(cmd, query);
        }

        internal void CreateTempTables()
        {
            try
            {
                CreateTransposedTempTable2(); //include everything
            }
            catch (Exception x)
            {

                throw;
            }
            CreateTransposedVTTempTable();  //technical cert table
            if (!SharedTools.IsQ4()) { SetVARParentFTEValues(); }  //freeze FTE values in Q4
            CreateAlignmentData(); //alignment data in a datatable
        }

        /// <summary>
        /// Parameters already added to cmd
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public DataTable TableQuery(SqlCommand cmd, string query = "")
        {
            SqlDataAdapter sqlDa = new SqlDataAdapter();
            if (query != "") { cmd.CommandText = query;}            
            cmd.Connection = dbConn;
            sqlDa.SelectCommand = cmd;
            DataTable dt = new DataTable();
            sqlDa.Fill(dt);
            return dt;
        }

        /// <summary>
        /// Create a temp table from the Alignment Report query
        /// </summary>
        internal void CreateAlignmentData()
        {
            AlignmentData = getAlignmentByVAR("%", "%");
            //re-format the datatable and add KPI points and color columns
            AlignmentData.Columns.Remove("GEO");
            AlignmentData.Columns["TotalAchievement"].ColumnName = "TotalPoints";
            AlignmentData.Columns["WeightedAchievement"].ColumnName = "Weighted Achievement";
            AlignmentData.Columns["Tech FTEs"].ColumnName = "TechFTE";
            AlignmentData.Columns["FTE Value"].ColumnName = "FTE";
            AlignmentData.Columns["TechTotalAchievement"].ColumnName = "TechTotalPoints";
            AlignmentData.Columns["Tech Weighted Achievement"].ColumnName = "TechWeightedAchievement";
            //AlignmentData.Columns["Tech Weighted Achievement"].ColumnName = "TechWeightedAchievement";
            AlignmentData.Columns.Add("Points");
            AlignmentData.Columns.Add("TechPoints");
            AlignmentData.Columns.Add("sBackground");
            AlignmentData.Columns.Add("tBackground");
            int salesGoal = SalesAlignmentGoal;
            int techGoal = TechSalesAlignmentGoal;

            foreach (DataRow r in AlignmentData.Rows)
            {
                //var data = r.ItemArray;
                r.BeginEdit();
                if(r.Field<decimal>("Weighted Achievement") > salesGoal)
                {
                    r["Points"] = 10;
                    r["sBackground"] = "green";
                }
                else
                {
                    r["Points"] = 0;
                    r["sBackground"] = "orange";
                }
                if (r.Field<decimal>("TechWeightedAchievement") > techGoal)
                {
                    r["TechPoints"] = 10;
                    r["tBackground"] = "green";
                }
                else
                {
                    r["TechPoints"] = 0;
                    r["tBackground"] = "orange";
                }
                r.EndEdit();                
            }
        }

        #endregion

        #region UI Queries
        internal DataTable GetCourseList()
        {
            string query = "select ID, Name from Courses ORDER BY Name";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            return TableQuery(cmd, query);

        }
        internal DataTable GetCourseListWithAlias(string escapedFilter = "", int ID = -1, bool includeCerts = true)
        {
            string query = @"select Courses.ID, Name, CourseNameAlias, DomainName, Courses.Type, CourseAlias.domain_cert from Courses 
                            left outer join CourseAlias on CourseAlias.ID = courses.alias_id
                            left outer join [Domains] on [Domains].ID = CourseAlias.domain_id
                            #WHERECLAUSE#
                            AND domain_cert <= @certs
                            ORDER BY Name";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            if (ID == -1)
            {
                query = query.Replace("#WHERECLAUSE#", "WHERE Name like @filter OR CourseAlias.CourseNameAlias like @filter OR DomainName like @filter");
                cmd.Parameters.AddWithValue("@filter", escapedFilter);
            }
            else
            {
                query = query.Replace("#WHERECLAUSE#", "WHERE Courses.ID = @ID ");
                cmd.Parameters.AddWithValue("@ID", ID);
            }
            int certVal = includeCerts ? 1 : 0;
            cmd.Parameters.AddWithValue("@certs", certVal);

            return TableQuery(cmd, query);
        }
        internal DataTable GetDomains()
        {
            string query = "select ID, DomainName from Domains ORDER BY DomainName";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            return TableQuery(cmd, query);
        }
        public DataTable GetParentList()
        {
            string query;
            query = @"Select ID, VAR_Parent from VARParents ORDER BY VAR_Parent";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            return TableQuery(cmd, query);
        }
        public DataTable GetCustomTargets(int varID)
        {
            string query;
            query = @"select CourseAlias.ID as [courseID], CourseNameAlias, CourseAlias.Target as [Default Target], ct.Target as [Custom Target], va.VAR_Alias, [ID] = @varID --, 0 as FTETarget
                    from CourseAlias
                    full outer JOIN (select * from CustomTargets where CustomTargets.var_alias_id = @varID) as ct on ct.course_alias_id = CourseAlias.ID
                    full outer JOIN (select * from VARAlias where VARAlias.ID = @varID) as va on va.ID = ct.var_alias_id
                    where CourseAlias.has_target > 0
                    order by domain_id, CourseNameAlias;";

            SqlCommand cmd = new SqlCommand(query, dbConn);

            cmd.Parameters.AddWithValue("@varID", varID);
            return TableQuery(cmd, query);
        }
        //passing -1 to the geoID argument returns all VAR Alias values
        public DataTable GetVARList(int geoID, string aliasFilter = "%", bool activeOnly = true)
        {
            string query;
            if (geoID == -1)
            {
                query = @"select ID, VAR_Alias, status, custom_targets, DashboardDate, CommunicationDate, [3dx], DW, EDU
                            from VARAlias
                            WHERE VAR_Alias like @aliasFilter AND VARAlias.status > @status                                                       
                            ORDER BY status DESC, VAR_Alias";
            }
            else
            {
                query = @"select ID, VAR_Alias, status, custom_targets, DashboardDate, CommunicationDate, [3dx], DW, EDU 
                        from VARAlias                             
                        WHERE (geo_id = @geoID
                        or geo_id Is Null) AND VAR_Alias like @aliasFilter AND VARAlias.status > @status
                        ORDER BY status DESC, VAR_Alias";
            }

            SqlCommand cmd = new SqlCommand(query, dbConn);
            if (geoID != -1)
            {
                cmd.Parameters.AddWithValue("@geoID", geoID);
            }
            if (activeOnly)
            {
                cmd.Parameters.AddWithValue("@status", 0);
            }
            else
            {
                cmd.Parameters.AddWithValue("@status", -1);
            }
            cmd.Parameters.AddWithValue("@aliasFilter", aliasFilter);
            return TableQuery(cmd, query);
        }

        public DataTable GetLearnerSearchRes(string filter, string geoFilter = "", bool showDeleted = false)
        {
            string query = @"select Learners.ID, [Learners].[Name], [email], [Role], [Country], [profile], [userState], [VAR_Alias], [GEOs].[GEO], Learners.fte_value from Learners 
                            left outer join GEOs on GEOs.ID = Learners.geo_id
                            left outer join VARs on VARs.ID = var_id
                            left outer join VARAlias on VARAlias.ID = VARs.var_alias_id
                           where (Learners.[Name] like @filter
                            or email like @filter
                            or Role like @filter
                            or profile like @filter
                            or GEOs.GEO like @geoFilter
                            or Learners.Country like @filter
                            or VARAlias.VAR_Alias like @filter)";
            if (!showDeleted)
            {
                query = query + " AND Learners.userState like 'ACTIVE'";
            }
            if (geoFilter == "")
            {
                geoFilter = filter;
            }
            query += " ORDER BY Learners.Name;";

            SqlCommand cmd = new SqlCommand(query, dbConn);
            cmd.Parameters.AddWithValue("@filter", filter);
            cmd.Parameters.AddWithValue("@geoFilter", geoFilter);
            return TableQuery(cmd, query);
        }

        internal DataTable GetLearnersByID(List<int> ids)
        {
            string query = @"select Name, email from Learners where ID in (@id1, @id2);";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            cmd.Parameters.AddWithValue("@id1", ids[0]);
            cmd.Parameters.AddWithValue("@id2", ids[1]);
            return TableQuery(cmd, query);
        }

        public bool GetIfUserExits(string email)
        {
            string query = @"select email from Learners where email = @email;";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            cmd.Parameters.AddWithValue("@email", email);
            if( SimpleQuery(query,cmd) > 0)
            {
                return true;
            }else
            {
                return false;
            }
  
        }
        public DataTable GetCompanySearchRes(string filter)
        {
            string query = @"select VARs.ID, VARs.Name, VARAlias.VAR_Alias, VARParents.VAR_Parent, VARs.status
                            from VARs
                            full outer join VARAlias on VARAlias.ID = VARs.var_alias_id
                            full outer join VARParents on VARParents.ID = VARs.var_parent_id
                           where VARs.[Name] like @filter
                            or VARAlias.VAR_Alias like @filter
                            or VARParents.VAR_Parent like @filter";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            cmd.Parameters.AddWithValue("@filter", filter);
            return TableQuery(cmd, query);
        }

        public DataTable GetCompaniesNotMapped()
        {
            string query = @"select VARs.ID, VARs.Name, VARAlias.VAR_Alias, VARParents.VAR_Parent, VARs.status
                            from VARs
                            full outer join VARAlias on VARAlias.ID = VARs.var_alias_id
                            full outer join VARParents on VARParents.ID = VARs.var_parent_id
                           where VARs.ID IS NOT NULL AND VARs.Status=1 AND ( VARs.var_alias_id IS NULL
                           or VARs.var_parent_id IS NULL)
						   order by VARs.Name;";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            return TableQuery(cmd, query);
        }

        public DataTable GetAddedLearners(string ids)
        {
            string query;
            query = @"select Learners.ID, [Learners].[Name], [email], [Role], [Country], [profile], [userState], [VAR_Alias], [GEOs].[GEO], fte_value from Learners 
                            left outer join GEOs on GEOs.ID = Learners.geo_id
                            left outer join VARs on VARs.ID = var_id
                            full outer join VARAlias on VARAlias.ID = VARs.var_alias_id ";
            if (ids.Split(',').Length - 1 > 2)
            {
                query = query + "where charindex(',' + CAST(Learners.ID as nvarchar(20)) + ',', @ids) > 0";
            }
            else
            {
                query = query + "where Learners.ID = @ids;";
                ids = ids.Split(',')[1];
            }
            SqlCommand cmd = new SqlCommand(query, dbConn);
            cmd.Parameters.AddWithValue("@ids", ids);
            return TableQuery(cmd, query);
        }

        public DataTable GetAddedCompanies(string ids)
        {
            string query;
            query = @"select VARs.ID, VARs.Name, VARs.status, VARAlias.VAR_Alias, VARParents.VAR_Parent
                      from VARs
                      full outer join VARAlias on VARAlias.ID = VARs.var_alias_id
                      full outer join VARParents on VARParents.ID = var_parent_id ";
            if (ids.Split(',').Length - 1 > 2)
            {
                query = query + "where charindex(',' + CAST(VARs.ID as nvarchar(20)) + ',', @ids) > 0";
            }
            else
            {
                query = query + "where VARs.ID = @ids;";
                ids = ids.Split(',')[1];
            }
            SqlCommand cmd = new SqlCommand(query, dbConn);
            cmd.Parameters.AddWithValue("@ids", ids);
            return TableQuery(cmd, query);
        }

        internal bool MergeLearners(List<int> ids)
        {
            //Should only ever be 2 ids
            if (ids.Count != 2)
            {
                return false;
            }
            //update the activities from one ID to the other
            int learnerToKeep = ids[0];
            int learnerToDelete = ids[1];
            string query = "";
            SqlCommand cmd;
            //remove duplicate courses
            query = @"select COUNT(*) as [cnt], MAX(enrollDate) as enrollDate, MAX(startDate) as startDate, 
                    MAX(completionDate) as completionDate, course_id from Activities
                    where learner_id in (@newLearnerID, @oldLearnerID) group by course_id having COUNT(*) > 1;";
            cmd = new SqlCommand(query, dbConn);
            cmd.Parameters.AddWithValue("@newLearnerID", learnerToKeep);
            cmd.Parameters.AddWithValue("@oldLearnerID", learnerToDelete);
            SqlDataReader r = cmd.ExecuteReader();
            List<int> courseIDs = new List<int>();
            List<DateTime> completionDates = new List<DateTime>();
            List<int> cStatus = new List<int>();
            if (r.HasRows)
            {
                while (r.Read())
                {
                    if (r["completionDate"] is DBNull)
                    {

                        if (r["startDate"] is DBNull)
                        {
                            completionDates.Add((DateTime)r["enrollDate"]);
                            cStatus.Add(0);
                        }
                        else
                        {
                            completionDates.Add((DateTime)r["startDate"]);
                            cStatus.Add(1);
                        }
                    }
                    else
                    {
                        completionDates.Add((DateTime)r["completionDate"]);
                        cStatus.Add(2);
                    }
                    courseIDs.Add((int)r["course_id"]);
                }
            }
            r.Close();
            //now have the courseIDs and completion dates of all duplicate courses
            for (int i = 0; i < courseIDs.Count; i++)
            {
                int courseID = courseIDs[i];
                DateTime completionDate = completionDates[i];
                int completed = cStatus[i];
                cmd = new SqlCommand(query, dbConn);
                string field = "";
                switch (completed)
                {
                    case 0:
                        field = "enrollDate";
                        break;
                    case 1:
                        field = "startDate";
                        break;
                    case 2:
                        field = "completionDate";
                        break;
                }
                cmd.CommandText = string.Format("DELETE from Activities where learner_id in (@newLearnerID, @oldLearnerID) and course_id = @courseID and ( {0} != @completionDate OR {0} IS NULL)", field);
                cmd.Parameters.AddWithValue("@courseID", courseID);
                cmd.Parameters.AddWithValue("@completionDate", completionDate);
                cmd.Parameters.AddWithValue("@newLearnerID", learnerToKeep);
                cmd.Parameters.AddWithValue("@oldLearnerID", learnerToDelete);
                var c = cmd.ExecuteNonQuery();
            }

            query = @"UPDATE Activities SET learner_id = @newLearnerID WHERE learner_id = @oldLearnerID;";
            cmd = new SqlCommand(query, dbConn);
            cmd.Parameters.AddWithValue("@newLearnerID", learnerToKeep);
            cmd.Parameters.AddWithValue("@oldLearnerID", learnerToDelete);
            cmd.ExecuteNonQuery();
            //then delete the learner with no activities
            query = @"DELETE from Learners WHERE ID = @oldLearnerID;";
            cmd = new SqlCommand(query, dbConn);
            cmd.Parameters.AddWithValue("@oldLearnerID", learnerToDelete);
            var res = cmd.ExecuteNonQuery();
            return true;
        }
        /// <summary>
        /// Get learner activities in a datatable by Learner ID
        /// </summary>
        /// <param name="ID">Learner ID</param>
        /// <returns></returns>
        public DataTable GetLearnerActivities(int ID)
        {
            string query = @"select Learners.Name, VARAlias.VAR_Alias , Learners.email,
                            Courses.Name as CourseName, CourseAlias.CourseNameAlias as CourseAlias, 
                            Courses.Type, Activities.[status], 
                            startDate, completionDate
                            from Activities
                            join Courses on Courses.ID = Activities.course_id
                            left outer join CourseAlias on CourseAlias.ID = Courses.alias_id
                            join Learners on Learners.ID = Activities.learner_id
							join VARs on VARs.ID = Learners.var_id
							left outer join VARAlias on VARAlias.ID = VARs.var_alias_id
                            where Activities.learner_id = @id";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            cmd.Parameters.AddWithValue("@id", ID);
            return TableQuery(cmd, query);
        }

        public DataTable GetUsersMissingData()
        {
            string query = @"select l.ID, l.Name, l.email, l.Role, l.Country, v.name as [VAR], g.GEO 
                            from learners l --l.id, l.name, l.email from learners l
                            left outer join vars v on v.id = l.var_id
                            left outer join geos g on g.id = l.geo_id
                            left outer join countries c on c.id = l.country_id
                            where (v.name = ''
                            or g.GEO = ''
                            or c.Country = '')
                            and userState = 'ACTIVE'";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            return TableQuery(cmd, query);
        }

        /// <summary>
        /// Get activities by search string and filters
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="searchString"></param>
        /// <param name="endDate"></param>
        /// <param name="includeDeleted"></param>
        /// <param name="geoFilter"></param>
        /// <param name="completed">True: filter only completed activities</param>
        /// <param name="salesOnly">True: filter on only sales roles, False: all roles</param>
        /// <returns></returns>
        public DataTable GetRecentActivities(DateTime startDate, string searchString, DateTime endDate,
            bool includeDeleted = false, string geoFilter = "%", bool completed = false, bool salesOnly = false)
        {
            string roleFilter = "%";
            if (salesOnly)
                roleFilter = "[si]%";  //all sales roles and inside sales

            string query = @"SET @endDate = DATEADD(day, 1, @endDate);  --add one day to include completions after 0:00
                            select Learners.Name, VARAlias.VAR_Alias, VARs.Name as [Company], Learners.email, Learners.profile, 
	                        Learners.role, Learners.Country, GEOs.GEO,
                            Courses.Name as CourseName, CourseAlias.CourseNameAlias as CourseAlias, 
                            Courses.Type, Activities.[status], 
                            enrollDate, startDate, completionDate, [DomainName]
                            from Activities
                            join Courses on Courses.ID = Activities.course_id
                            left outer join CourseAlias on CourseAlias.ID = Courses.alias_id
                            join Learners on Learners.ID = Activities.learner_id
	                        left outer join VARs on VARs.ID = Learners.var_id
                            left outer join GEOs on GEOs.ID = Learners.geo_id
                            left outer join Domains on Domains.ID = CourseAlias.domain_id
	                        left outer join VARAlias on VARAlias.ID = VARs.var_alias_id 
                            where (enrollDate > @startDate OR startDate > @startDate OR completionDate > @startDate) 
                            AND (completionDate >= @startDate OR completionDate is null) --additional clause to ignore previous completions with recent enrollments
                            AND Learners.Role LIKE @roleFilter
                            AND (enrollDate <= @endDate OR  startDate <= @endDate) AND (completionDate <= @endDate OR completionDate is null)  
	                        AND Learners.userState like @userState ";  //fixed @endDate WHERE clause to be AND Sep. 13, 2021, Updated again March 3, 2022.  Was not the right syntax
            if (searchString != "")
                query = query + " AND (Learners.Name like @filter OR VARs.Name like @filter OR Learners.Country like @filter OR Learners.email like @filter OR VARAlias.VAR_Alias like @filter OR Courses.Name like @filter OR CourseAlias.CourseNameAlias like @filter OR GEOs.GEO like @filter) ";

            if (geoFilter != "")
                query = query + " AND GEOs.GEO like @geoFilter";

            if (completed)
                query += " AND completionDate is not null";

            query += " ORDER BY completionDate, startDate, Learners.Name";

            SqlCommand cmd = new SqlCommand(query, dbConn);
            cmd.Parameters.AddWithValue("@startDate", startDate);
            cmd.Parameters.AddWithValue("@endDate", endDate);
            cmd.Parameters.AddWithValue("@roleFilter", roleFilter);
            if (geoFilter != "")
            {
                cmd.Parameters.AddWithValue("@geoFilter", geoFilter);
            }
            string stateVal = "ACTIVE";
            if (includeDeleted)
            {
                stateVal = "%";
            }

            if (searchString != "")
            {
                searchString = string.Format("%{0}%", searchString);
                cmd.Parameters.AddWithValue("@filter", searchString);
            }
            cmd.Parameters.AddWithValue("@userState", stateVal);
            return TableQuery(cmd, query);
        }
        public DataTable getCoursePoints()
        {
            DataTable table = new DataTable();
            using (var cmd = new SqlCommand("select * from CourseAlias order by domain_id, CourseNameAlias;", dbConn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.Text;
                da.Fill(table);
            }
            return table;
        }

        /// <summary>
        /// Get all learners who have the same name
        /// </summary>
        /// <returns>datatable of learners with duplicate names</returns>
        public DataTable getDuplicateLearners()
        {
            DataTable table = new DataTable();
            string query = @"select 
                count(Learners.Name) as [count]
                ,Learners.Name
                ,VARs.Name as [VAR], Learners.Country
                from Learners
                join VARs on Learners.var_id = VARs.ID
                left outer join VARAlias on VARAlias.id = vars.var_alias_id
                where learners.userState = 'ACTIVE'
                --where VARAlias.status = 1
                group by Learners.Name--, Learners.email
                , VARs.Name, Learners.Country
                having count(Learners.Name) > 1";
            using (var cmd = new SqlCommand(query, dbConn))
                return TableQuery(cmd, query);
        }

        internal DataTable GetCoursesFromAlias(Alias a)
        {
            DataTable table = new DataTable();
            string query = @"SELECT Courses.Name, Courses.Type from Courses WHERE Courses.alias_id = @alias;";
            using (var cmd = new SqlCommand(query, dbConn))
            {
                cmd.Parameters.AddWithValue("@alias", a.ID);
                return TableQuery(cmd, query);
            }
        }


        #endregion

        #region Report Queries

        /// <summary>
        /// Generate alignment report based on VARParents
        /// </summary>
        /// <param name="varFilter"></param>
        /// <param name="geoFilter"></param>
        /// <param name="roleFilter"></param>
        /// <returns></returns>
        internal DataTable getAlignmentByGEO4(string varFilter, string geoFilter)
        {
            string roleFilter = "[siv]%"; //only for sales calculations
            string query = @"--learner course completions into temp table
                            SELECT t1.lID, vp.GEOID, t1.CourseNameAlias, t1.alignment_points, t1.Name, t1.status, vp.VAR_Parent, vp.ID vID 
                            INTO #tempTable 
                            from (select DISTINCT l.id as lID, l.GEO, CourseNameAlias, l.Name, alignment_points, a.status, VAR_Parent, l.vpID as vID
                                from ActivitiesDetail a join AllActiveLearners l on l.id = a.learner_id
                                where VAR_Parent like @VARFilter AND l.role like @rolefilter
                                AND alignment_points > 0 AND kpi = 1  --sales courses only
                                AND GEO like @geofilter) as T1 full outer join ActiveVARParents vp on T1.vID = vp.ID

                            /* create temp table of all tech sales completions */
                            select DISTINCT l.id as lID, l.GEO, CourseNameAlias, l.Name, alignment_points, a.status, VAR_Parent, l.vpID as vID
	                            INTO #tempTechTable
                                from ActivitiesDetail a join AllActiveLearners l on l.id = a.learner_id
                                where VAR_Parent like @VARFilter 
	                            AND (l.Role like 'tech%sales' OR l.Role like 'tech%manag%')
                                AND alignment_points > 0 AND kpi = 2  --tech sales courses only
                                AND GEO like @geofilter

	                            /* calculate bonus points for old CSSP completions */
	                            declare @cFilter varchar(10) = '%css%', @cssp varchar(10) = '%CSSP%';
	                            select COUNT(one.Name) * 40 as bonus, tt.vID
	                            INTO #bonus from #tempTable tt
	                            JOIN (select COUNT(t.Name) as cnt, t.Name, t.vID --, t.CourseNameAlias
		                            FROM #tempTable t where t.CourseNameAlias like @cFilter --and t.Role = @roleFilter
		                            group by t.Name, t.vID having COUNT(t.Name) = 1 ) as one on one.Name = tt.Name
	                            where tt.CourseNameAlias like @cssp
	                            group by tt.vID;

	                            /* make FTE temp table */
                                select FTESales as fteVal, ID as vID into #ftes from VARParents 
	                            /* make tech FTE temp table */
	                            select FTETech as fteVal, ID as vID into #techFTEs from VARParents

	                            /* create the report output from the temp tables */
	                            select g.GEO, t.VAR_Parent as [VAR], f.fteVal as [FTE Value], ISNULL((SUM(t.alignment_points) + ISNULL(b.bonus, 0)), 0) as TotalAchievement
	                                , CONVERT(DECIMAL(6,1),ISNULL((SUM(t.alignment_points) + ISNULL(b.bonus,0))/NULLIF(f.fteVal,0),0)) as WeightedAchievement
	                                , ISNULL(tf.fteVal, 0) as [Tech FTEs], ISNULL(tt.alignment_points,0) as [TechTotalAchievement]
	                                , CONVERT(decimal(6,1),ISNULL(tt.alignment_points/NULLIF(tf.fteVal,0), 0)) as [Tech Weighted Achievement]
	                                from #tempTable t join GEOs g on t.GEOID = g.ID
	                                full outer join #ftes f on f.vID = t.vID
	                                left outer join #bonus b on b.vID = t.vID
	                                left outer join #techFTEs tf on tf.vID = t.vID
	                                /* need to sum the tech points before joining */
	                                left outer join (select SUM(ttt.alignment_points) as [alignment_points], ttt.vID from #tempTechTable ttt group by ttt.vID) as tt on tt.vID = t.vID
	                                where t.VAR_Parent is not null group by t.VAR_Parent, g.GEO, t.vID, f.fteVal, b.bonus, tf.fteVal, tt.alignment_points
	                                ORDER BY g.GEO, t.VAR_Parent";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            cmd.Parameters.AddWithValue("@varFilter", varFilter);
            cmd.Parameters.AddWithValue("@roleFilter", roleFilter);
            cmd.Parameters.AddWithValue("@geoFilter", geoFilter);
            cmd.Parameters.AddWithValue("@salesGoal", SalesAlignmentGoal);
            return TableQuery(cmd);

        }

        /// <summary>
        /// Generate alignment report based on VARAlias
        /// </summary>
        /// <param name="varFilter"></param>
        /// <param name="geoFilter"></param>
        /// <param name="roleFilter"></param>
        /// <returns></returns>
        internal DataTable getAlignmentByVAR(string varFilter, string geoFilter)
        {
            //changed to simpler queries from views
            //added DISTINCT filter in first completions query to avoid duplicates
            string roleFilter = "[siv]%"; //only for sales calculations
            string query = @"--learner course completions into temp table
                            SELECT DISTINCT t1.lID, vp.geo_id as GEOID, t1.CourseNameAlias, ISNULL(t1.alignment_points,0) as  alignment_points, t1.Name, t1.status, vp.VAR_Alias, vp.ID vID 
                            INTO #tempTable 
                            from (select a.learner_id as lID, l.GEO, CourseNameAlias, l.Name, alignment_points, VAR_Alias, l.vaID as vID, a.status
		                            from ActivitiesDetail a join AllActiveLearners l on l.id = a.learner_id 
		                            where l.VAR_Alias like @varFilter and l.Role like @rolefilter and l.GEO like @geofilter
		                            AND alignment_points > 0 AND kpi = 1  --sales courses only
		                            ) as T1 full outer join (select * from VARAlias where status = 1) as vp on T1.vID = vp.ID

                            /* create temp table of all tech sales completions */
                            select l.id as lID, GEO, CourseNameAlias, l.Name, alignment_points, VAR_Alias, l.vaID as vID, a.status
	                            INTO #tempTechTable 
	                            from ActivitiesDetail a join AllActiveLearners l on l.id = a.learner_id
	                            where VAR_Alias like @varFilter and GEO like @geofilter
	                            AND (l.Role like 'tech%sales' OR l.Role like 'tech%manag%')
	                            AND alignment_points > 0 AND kpi = 2  --tech sales courses only

	                            /* calculate bonus points for old CSSP completions */
	                            declare @cFilter varchar(10) = '%css%', @cssp varchar(10) = '%CSSP%';
	                            select COUNT(one.Name) * 40 as bonus, tt.vID
	                            INTO #bonus from #tempTable tt
	                            JOIN (select COUNT(t.Name) as cnt, t.Name, t.vID --, t.CourseNameAlias
		                            FROM #tempTable t where t.CourseNameAlias like @cFilter --and t.Role = @roleFilter
		                            group by t.Name, t.vID having COUNT(t.Name) = 1 ) as one on one.Name = tt.Name
	                            where tt.CourseNameAlias like @cssp
	                            group by tt.vID;

	                            /* make FTE temp table */
                                select ISNULL(sum(t1.ftesales),0) as fteVal, T1.VAR_Alias, T1.vaID as vID INTO #ftes
                                from (select distinct vp.ID as vpID, FTESales, FTETech, VAR_Alias, var_alias_id as vaID from VARParents vp
	                                join VARs v on v.var_parent_id = vp.id
	                                join VARAlias va on va.id = v.var_alias_id
	                                where va.status = 1) as T1
	                                group by t1.VAR_Alias, t1.vaid
	                            /* make tech FTE temp table */	
                                select ISNULL(sum(t1.FTETech),0) as fteVal, T1.VAR_Alias, T1.vaID as vID INTO #techFTEs
                                from (select distinct vp.ID as vpID, FTESales, FTETech, VAR_Alias, var_alias_id as vaID from VARParents vp
	                                join VARs v on v.var_parent_id = vp.id
	                                join VARAlias va on va.id = v.var_alias_id
	                                where va.status = 1) as T1
	                                group by t1.VAR_Alias, t1.vaid
	                            /* create the report output from the temp tables */
	                            select g.GEO, t.VAR_Alias as [VAR], ISNULL(f.fteVal,0) as [FTE Value], ISNULL((SUM(t.alignment_points) + ISNULL(b.bonus, 0)), 0) as TotalAchievement
	                                , CONVERT(DECIMAL(6,1),ISNULL((SUM(t.alignment_points) + ISNULL(b.bonus,0))/NULLIF(f.fteVal,0),0)) as WeightedAchievement
	                                , ISNULL(tf.fteVal, 0) as [Tech FTEs], ISNULL(tt.alignment_points,0) as [TechTotalAchievement]
	                                , CONVERT(decimal(6,1),ISNULL(tt.alignment_points/NULLIF(tf.fteVal,0), 0)) as [Tech Weighted Achievement]
	                                from #tempTable t join GEOs g on t.GEOID = g.ID
	                                full outer join #ftes f on f.vID = t.vID
	                                left outer join #bonus b on b.vID = t.vID
	                                full outer join #techFTEs tf on tf.vID = t.vID
	                                /* need to sum the tech points before joining */
	                                left outer join (select ISNULL(SUM(ttt.alignment_points),0) as [alignment_points], ttt.vID from #tempTechTable ttt group by ttt.vID) as tt on tt.vID = t.vID
	                                where t.VAR_Alias is not null group by t.VAR_Alias, g.GEO, t.vID, f.fteVal, b.bonus, tf.fteVal, tt.alignment_points
	                                ORDER BY g.GEO, t.VAR_Alias";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            cmd.Parameters.AddWithValue("@varFilter", varFilter);
            cmd.Parameters.AddWithValue("@roleFilter", roleFilter);
            cmd.Parameters.AddWithValue("@geoFilter", geoFilter);
            cmd.Parameters.AddWithValue("@salesGoal", SalesAlignmentGoal);
            return TableQuery(cmd);

        }

        /// <summary>
        /// OBSOLETE: VAR Alignment report table for VAR dashboards
        /// </summary>
        /// <param name="varFilter"></param>
        /// <returns>Report in a DataTable</returns>
        internal DataTable getAlignmentReport(string varFilter)
        {
            string query;
            double fteVal;
            double tfteVal;
            SqlCommand cmd;
            DataTable dtFTE = GetFTEVals6(varFilter: varFilter, style: VARGrouping.byVARAlias);
            DataTable dttechFTE = GetTechFTEVals(varFilter, VARGrouping.byVARAlias);

            fteVal = dtFTE.Rows.Count == 0 ? 0 : (double)(decimal)dtFTE.Rows[0]["FTE Value"];
            tfteVal = dttechFTE.Rows.Count == 0 ? 0 : (int)dttechFTE.Rows[0]["FTE Value"];

            query = @"declare @roleFilter nvarchar(10), @kpi int, @salespoints float = 0, @techPoints float = 0, @salesWeighted float = 0, @techWeighted float = 0,
                    @salesKPIPoints int = 0, @techKPIPoints int = 0, @salesColor nvarchar(10) = 'orange', @techColor nvarchar(10) = 'orange';
                    set @roleFilter = '[siv]%';
                    set @kpi = 1;
                    
                    select distinct CourseAlias.CourseNameAlias
                        , Learners.Name
                        , CourseAlias.alignment_points
                        , VARAlias.VAR_Alias
                        , Activities.status, Learners.Role, CourseAlias.kpi
						INTO #tempTable
                        from Learners
                        join VARs on VARs.ID = Learners.var_id
                        join VARAlias on VARAlias.ID = VARs.var_alias_id
                        join Activities on Activities.learner_id = Learners.ID
                        join Courses on Courses.ID = Activities.course_id
                        join CourseAlias on CourseAlias.ID = Courses.alias_id
                        where VAR_Alias like @varFilter
                        AND Activities.status = 'Completed'
                        AND Learners.userState = 'ACTIVE'	                    
                        AND alignment_points > 0	                    
                        AND CourseAlias.domain_cert != 1;

					/* calculate bonus points for old CSSP completions */
					declare @cFilter varchar(10) = '%CSS%', @cssp varchar(10) = '%CSSP%', @bonus int;
						select @bonus = (select COUNT(one.Name) * 40 as bonus from #tempTable tt
						JOIN ( /* count the number of name instances after filtering on CSS% */
							select COUNT(t.Name) as cnt, t.Name --, t.CourseNameAlias
							FROM #tempTable t where t.CourseNameAlias like @cFilter AND t.Role like @roleFilter
							group by t.Name having COUNT(t.Name) = 1 ) as one on one.Name = tt.Name
						where tt.CourseNameAlias like @cssp)

                    /* sales points plus bonus */
                    select @salespoints = (ISNULL(SUM(alignment_points), 0) + @bonus) from (
                        select alignment_points from #tempTable t
					    where t.[Role] like @roleFilter
					    AND t.kpi = @kpi) as t1;

                    /* tech sales points */
                    set @roleFilter = 'tech%';
                    set @kpi = 2;
                    select @techPoints = ISNULL(SUM(alignment_points), 0) from (
                    select * from #tempTable t
	                    WHERE t.Role like @roleFilter 
						AND t.Role not like '%support'	
						AND t.kpi = @kpi
                        ) as t1;

                    set @salesWeighted = FORMAT(ISNULL(@salesPoints / NULLIF(@FTEval, 0),0), '#.#');
                    set @techWeighted = FORMAT(ISNULL(@techPoints / NULLIF(@techFTE, 0),0), '#.#');

                    IF @salesWeighted >= @salesGoal
                    BEGIN
	                    set @salesKPIPoints = 10
	                    set @salesColor = 'green'
                    END
                    IF @techWeighted >= @techGoal
                    BEGIN
	                    set @techKPIPoints = 10
	                    set @techColor = 'green'
                    END

                    --Formatted table
                    select @FTEval as [FTE], @salesPoints as TotalPoints, VAR_Alias as [VAR], 
                        @salesWeighted as [Weighted Achievement], (@salesGoal) AS Goal
	                    , @salesKPIPoints as [Points], @techFTE as [TechFTE], @techPoints as [TechTotalPoints], @techWeighted as [TechWeightedAchievement],
	                    @techKPIPoints as [TechPoints], @salesColor as [sBackground], @techColor as [tBackground]
                        from VARAlias
                        where VAR_Alias like @varFilter";

            cmd = new SqlCommand(query, dbConn);
            cmd.Parameters.AddWithValue("@varFilter", varFilter);
            cmd.Parameters.AddWithValue("@FTEval", fteVal);
            cmd.Parameters.AddWithValue("@TechFTE", tfteVal);
            cmd.Parameters.AddWithValue("@salesGoal", SalesAlignmentGoal);
            cmd.Parameters.AddWithValue("@techGoal", TechSalesAlignmentGoal);

            SqlDataAdapter sqlDa = new SqlDataAdapter();
            sqlDa.SelectCommand = cmd;
            DataTable dt = new DataTable();
            //TODO: remove Try after debug
            try
            {
                sqlDa.Fill(dt);
            }
            catch (Exception x)
            {

                throw;
            }

            if (dt.Rows.Count == 0)  //this should never happen with the new query method
            {
                //enter zeros for all but FTE and Goal
                dt.Rows.Add(new object[] { fteVal, 0, varFilter, 0, SalesAlignmentGoal, 0, tfteVal, 0, 0, 0, "orange", "orange" });
            }
            return dt;
        }

        /// <summary>
        /// VAR Alignment report table for VAR dashboards
        /// </summary>
        /// <param name="varFilter"></param>
        /// <returns>Report in a DataTable</returns>
        internal DataTable getAlignmentReport2(string varFilter)
        {
            //query row out of AlignmentData
            var dataRow = AlignmentData.AsEnumerable().Where(x => x.Field<string>("VAR") == varFilter);            
            DataTable dt = dataRow.CopyToDataTable<DataRow>();
            return dt;            
        }


        /// <summary>
        /// Get the total points from a var for a given KPI role
        /// </summary>
        /// <param name="varFilter">var name to query</param>
        /// <param name="roleFilter">String value, [siv]% for sales, tech-sa% for tech sales</param>
        /// <param name="role">from KPIRole enum</param>
        /// <returns></returns>
        private double GetVARTotalPoints(string varFilter, string roleFilter, KPIRole role)
        {
            string query = @"select SUM(alignment_points) as Points from (
                            select distinct CourseAlias.CourseNameAlias
                            , Learners.Name
                            , CourseAlias.alignment_points
                            , VARAlias.VAR_Alias
                            , Activities.status
                            from Learners
                            join VARs on VARs.ID = Learners.var_id
                            join VARAlias on VARAlias.ID = VARs.var_alias_id
                            join Activities on Activities.learner_id = Learners.ID
                            join Courses on Courses.ID = Activities.course_id
                            join CourseAlias on CourseAlias.ID = Courses.alias_id
                            where VAR_Alias like @varFilter
                            AND Activities.status = 'Completed'
                            AND Learners.userState = 'ACTIVE'                            
		                    AND Learners.Role like @roleFilter		                        
                            AND alignment_points > 0
	                        AND CourseAlias.kpi = @kpi     
                            AND CourseAlias.domain_cert != 1) as t1;";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            cmd.Parameters.AddWithValue("@varFilter", varFilter);
            cmd.Parameters.AddWithValue("@roleFilter", roleFilter);
            cmd.Parameters.AddWithValue("@kpi", (int)role);
            var r = cmd.ExecuteScalar();
            return Convert.ToDouble(r);
        }

        private DataTable GetTechFTEVals(string varFilter, VARGrouping style, string geoFilter = "%")
        {
            DataTable table = new DataTable();
            string query;
            query = (style == VARGrouping.byVARAlias) ?
                @"SELECT * FROM FTETechSalesValuesAlias WHERE VAR like @VARFilter and GEO like @GEOFilter ORDER BY GEO, VAR" :
                @"SELECT * FROM FTETechSalesValuesParent WHERE VAR like @VARFilter and GEO like @GEOFilter ORDER BY GEO, VAR";
            using (var cmd = new SqlCommand(query, dbConn))
            {
                cmd.Parameters.Add("@VARFilter", SqlDbType.VarChar).Value = varFilter;
                cmd.Parameters.Add("@GEOFilter", SqlDbType.VarChar).Value = geoFilter;
                return TableQuery(cmd, query);
            }
        }

        internal void CleanOrphanVARs()
        {
            string query = @"delete 
                            from vars where vars.id in (
	                            select v.id as varid 
	                            from
	                            Learners l
	                            full outer join VARs v on v.id = l.var_id
	                            where l.id is null
                            )";
            SimpleQuery(query);

            query = @"delete
                            from VARParents where VARParents.ID in (
	                            select vp.id as vpID
	                            from VARs v
	                            full outer join VARParents vp on vp.id = v.var_parent_id
	                            where v.id is null
                            )";
            SimpleQuery(query);
        }
        public bool setUserStateByEmail(string email, bool deletedStatus)
        {
            string delVal;
            if (deletedStatus)
            {
                delVal = "DELETED";
            }
            else
            {
                delVal = "ACTIVE";
            }

            string query = "UPDATE Learners SET userState = '" + delVal + "' WHERE email LIKE '" + email + "'";
            Debug.Print(query);

            using (var cmd = new SqlCommand(query, dbConn))
            {
                int rowCount = 0;
                try
                {
                    rowCount = cmd.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    //ignore errors, could be a bad email address format
                }

                Debug.Print("Updated " + rowCount.ToString() + " rows.");
                if (rowCount != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void SetUserName(string userName, string newName)
        {
            string query = "UPDATE Learners SET [Name] = '" + newName + "' WHERE [Name] LIKE '" + userName + "'";

            using (var cmd = new SqlCommand(query, dbConn))
            {
                var rowCount = cmd.ExecuteNonQuery();
            }
        }

        public string CreateTempTable(rbTables tbType)
        {
            string tableName = "";
            string query = "";
            switch (tbType)
            {
                case rbTables.VARnames:
                    tableName = "ImportVAR";
                    query = @"CREATE TABLE dbo." + tableName + @"
                            (ID int IDENTITY (1,1),
                             VARName nvarchar(max) NOT NULL,
                             alias_id int,
                             parent_id int);";
                    break;
                case rbTables.Learners:
                    tableName = "ImportLearners";
                    query = @"CREATE TABLE dbo." + tableName + @"
                            (ID int IDENTITY (1,1),
                             Name nvarchar(max) NOT NULL,
                             email nvarchar(max) NOT NULL,
                             Company nvarchar(max),
                             Country nvarchar(max),
                             Role nvarchar(max),
                             GEO nvarchar(max),
                             Region nvarchar(max),
                             userState nvarchar(max),
                             Type nvarchar(max));";
                    break;
                case rbTables.Courses:
                    tableName = "ImportCourses";
                    query = @"CREATE TABLE dbo." + tableName + @"
                            (ID int IDENTITY (1,1),
                            CourseName nvarchar(max) NOT NULL,
                            CourseGroup nvarchar(max),
                            CourseType nvarchar(max)
                            );";
                    break;
                case rbTables.VARParents:
                    tableName = "VARParents";
                    query = @"CREATE TABLE dbo." + tableName + @"
                            (ID int IDENTITY (1,1),
                            ParentName nvarchar(max) NOT NULL
                            );";
                    break;
                case rbTables.Activities:
                    tableName = "ImportActivities";
                    query = @"CREATE TABLE dbo." + tableName + @"
                            (ID int IDENTITY (1,1),
                            StartDate datetime,
                            CompletionDate datetime,
                            Status nvarchar(max) NOT NULL,
                            LearnerName nvarchar(max) NOT NULL, 
                            email nvarchar(max) NOT NULL,
                            Company nvarchar(max) NOT NULL
                            );";
                    break;
                default:
                    break;
            }
            if (query != "")
            {
                using (var cmd = new SqlCommand(query, dbConn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }
            return tableName;
        }

        public DataTable getRemainingFTEs(string varFilter = "%")
        {
            DataTable table = new DataTable();
            string query = @"select Learners.ID, Learners.Name, Learners.email, Learners.Role, VARAlias.VAR_Alias as [VAR Alias], VARs.Name as company, Countries.Country, GEOs.GEO, null TotalAchievement, Learners.fte_value as [FTE Value]
                        from Learners 
                        left outer Join Activities on Activities.learner_id = Learners.ID
                        join VARs on VARs.ID = Learners.var_id
                        join VARAlias on VARAlias.ID = VARs.var_alias_id
                        join Countries on Countries.id = Learners.country_id
                        join GEOs on GEOs.ID = Learners.geo_id
                        where fte_value > 0 AND VAR_Alias like @var AND Learners.userState = 'ACTIVE'
                        group by Learners.ID, Learners.Name, Learners.Email, Learners.Role, VARAlias.VAR_Alias, VARs.Name, Countries.Country, GEOs.GEO, Learners.fte_value
                        having count(Activities.ID) = 0;";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            cmd.Parameters.AddWithValue("@var", varFilter);
            table = TableQuery(cmd, query);
            table.Columns.Remove("ID");
            return table;
        }

        //Obsolete
        public DataTable getTransposedReport4(string varFilter = "%", string profileFilter = "%", string geoFilter = "%")
        {
            DataTable table = new DataTable();
            //Sept. 7, 2021 - added CASE WHEN filter for 'Unenrolled' learners so they won't show as In Progress.
            //Oct. 21, 2021 - removed domain_id < 1 (0 and -1 should be ignored in this report)
            string query = @"DECLARE @cd nvarchar(max), @courses nvarchar(max), @coursesPTS nvarchar(max), @query1 nvarchar(max), @query2 nvarchar(max), @query3 nvarchar(max);
                            set @coursesPTS = STUFF(
	                            REPLACE(
                                REPLACE((select Header from ((select ','+QUOTENAME([CourseNameAlias])+' As '+QUOTENAME(concat(CourseNameAlias, ' (', alignment_points, ' pts.)')) as Header, domain_id from CourseAlias where alignment_points > 0 and domain_id Is Not Null and domain_cert != 1)  -- added domain_cert filter 8/10/2021
	                            UNION (select ','+QUOTENAME(CourseNameAlias) as Header, domain_id from CourseAlias where alignment_points = 0 and domain_id Is Not Null AND domain_id < 12 AND domain_id > 0 AND domain_cert != 1)) as tbl order by domain_id, Header FOR XML PATH('')), '<Header>', '')  -- added domain_cert filter 8/10/2021
	                            , '</Header>', '' )
	                            , 1, 1, '');
                            set @courses = STUFF(
                                (SELECT DISTINCT ','+QUOTENAME([CourseNameAlias])
                            FROM [CourseAlias] c WHERE c.domain_cert != 1 AND c.domain_id < 12 AND c.domain_id > 0 FOR XML PATH('')), 1, 1, '');  -- added domain_cert filter 8/10/2021, filtering out SIMULIA Legacy 10/11/2021
                            --print @courses;
                            --print @coursesPTS;
							set @cd = 'CASE WHEN MAX(Activities.status) = ''Unenrolled'' THEN null  WHEN MAX(completionDate) is null THEN ''In Progress'' ELSE convert(varchar,MAX(completionDate), 101) END as ''Completion Date'''; 
							--print @cd;
                            set @query1 = 'SELECT tblScore.ID, tblScore.Name,  email, Role, [FTE Value], [VAR Alias], company, Country, GEO, TotalAchievement, '+@coursesPTS+'
                            FROM (select Learners.ID, Learners.Name, Learners.email, Learners.fte_value as [FTE Value], Learners.[Role], VARAlias.VAR_Alias as ''VAR Alias'', VARs.Name as ''company'', Learners.Country, GEOs.GEO, '+@cd+', CourseAlias.CourseNameAlias
                            from Activities join Learners on Learners.ID = Activities.learner_id join VARs on VARs.ID = Learners.var_id
                            join VARAlias on VARAlias.ID = VARs.var_alias_id join GEOs on GEOs.ID = Learners.geo_id left outer join Courses on Courses.ID = Activities.course_id
                            left outer join CourseAlias on CourseAlias.ID = Courses.alias_id
                            where (Activities.status != ''Not Started'' OR Courses.ID IS NULL ) AND Learners.userState = ''ACTIVE'' AND Learners.profile like '''+@profile+''' AND GEOs.GEO like '''+@geo+''' AND VARAlias.VAR_Alias like '''+@var+'''
                            Group by Learners.ID, Learners.Name, Learners.email, Learners.fte_value, Learners.[Role], VARAlias.VAR_Alias, VARs.Name, Learners.Country, GEOs.GEO, CourseAlias.CourseNameAlias
                            ) AS SourceTable PIVOT(MAX([Completion Date]) FOR [CourseNameAlias] IN('+@courses+')) AS PT '
                            set @query2 = 'join (select SUM(alignment_points) as [TotalAchievement], ID, Name
                            From (select aa.ID, aa.Name, aa.email, aa.Role, aa.[VAR Alias], aa.company, aa.Country, aa.GEO, aa.[Completion Date], aa.CourseNameAlias
                            ,CASE WHEN aa.[Completion Date] = ''In Progress'' THEN 0 ELSE aa.alignment_points END AS [alignment_points] 
                            from (select Learners.ID, Learners.Name, Learners.email, Learners.[Role], VARAlias.VAR_Alias as [VAR Alias], 
                            VARs.Name as [company], Learners.Country, GEOs.GEO,  '+@cd+', 
                            CourseAlias.CourseNameAlias
							, CourseAlias.alignment_points
                            from Activities join Learners on Learners.ID = Activities.learner_id join VARs on VARs.ID = Learners.var_id join VARAlias on VARAlias.ID = VARs.var_alias_id
                            join GEOs on GEOs.ID = Learners.geo_id join Courses on Courses.ID = Activities.course_id join CourseAlias on CourseAlias.ID = Courses.alias_id
                            where (Activities.status = ''In Progress'' OR Activities.status = ''Completed'') AND Learners.userState = ''ACTIVE'' AND Learners.profile like '''+@profile+''' 
                            AND GEOs.GEO like '''+@geo+''' AND VARAlias.VAR_Alias like '''+@var+'''
                            Group by Learners.ID, Learners.Name, Learners.email, Learners.[Role], VARAlias.VAR_Alias, VARs.Name, 
                            Learners.Country, GEOs.GEO, CourseAlias.CourseNameAlias, CourseAlias.alignment_points) as aa) as tbl 
                            Group By ID, Name) as tblScore on tblScore.ID = PT.ID'
                            set @query3 = ' UNION ALL SELECT Learners.Name, Learners.email, Learners.Role, VAR_Alias as [VAR Alias], VARs.Name as [company], 
	                        Learners.Country, GEOs.GEO,  0 as TotalAchievement, '+@courses+'
	                        FROM Learners
	                        join VARs on VARs.ID = Learners.var_id
	                        join VARAlias on VARAlias.ID = VARs.var_alias_id
	                        left outer join Activities on Activities.learner_id = Learners.ID
	                        join GEOs on GEOs.ID = Learners.geo_id
	                         where VARAlias.VAR_Alias like '''+@var+'''
	                         AND Activities.ID is null'
                            --print @query1
                            --print @query2;
                            execute (@query1+@query2)
                            --execute (@query1+@query2+@query3)
                            ;";
            //@query3 UNION doesn't work as-is. Would need to change each course column (@courses) to 'null = [columname]'
            using (var cmd = new SqlCommand(query, dbConn))
            {
                cmd.Parameters.Add("@var", SqlDbType.VarChar).Value = varFilter;
                cmd.Parameters.Add("@profile", SqlDbType.VarChar).Value = profileFilter;
                cmd.Parameters.Add("@geo", SqlDbType.VarChar).Value = geoFilter;
                table = TableQuery(cmd, query);
            }
            //using (var da = new SqlDataAdapter(cmd))
            //{

            //    cmd.CommandType = CommandType.Text;
            //    da.Fill(table);
            //}

            table.Columns.Remove("ID");
            return table;
        }

        /// <summary>
        /// Clean up common FTE table problems before learners table update
        /// </summary>
        /// <param name="tableName">Name of the FTETemp table to clean</param>
        internal void cleanFTETable(string tableName)
        {
            string query = @"--step 1
                        --delete innactive accounts from the FTETemp table
                        delete from FTETemp
                        where Account in (
	                        select distinct account
	                        from FTETemp f
	                        join VARAlias va on va.VAR_Alias = f.Account
	                        where va.status = 0)
                        or Account like '%terminate%'
                        or email like '%cancel%'

                        --email corrections
                        update FTETemp set email = 'chrisk@hawkridgesys.com' where email = 'ckresic@design-point.com'
                        update FTETemp set email = 'aaron.weninger@gsc-3d.com' where email = 'aaron.weninger@gxsc.com'
                        update FTETemp set email = 'Anthony.DAMBRA@visiativ.com' where email = 'adambra@axemble.com'
                        update FTETemp set email = 'benoit.frin@visiativ.com' where email = 'bfrin@axemble.com'
                        update FTETemp set email = 'claude.ribagnac@visiativ.com' where email = 'cribagnac@axemble.com'
                        update FTETemp set email = 'kevin.poulain@visiativ.com' where email = 'kpoulain@axemble.com'
                        update FTETemp set email = 'michael.fenaut@visiativ.com' where email = 'mfenaut@axemble.com'
                        update FTETemp set email = 'philippe.couet@visiativ.com' where email = 'pcouet@axemble.com'
                        delete from FTETemp where email = 'praveen.rao@beacoa.n-indicom' --duplicate
                        update FTETemp set email = 'seamus@solidsolutions.ie' where email = 'sshanahan@solidsolutions.ie'
                        update FTETemp set email = 'sylvain.pourprix@visiativ.com' where email = 'spourprix@axemble.com'
                        update FTETemp set email = 'cigdem.murat@solidline.de' where email = 'cmurat@solidline.de'
                        update FTETemp set email = 'julien.chapuis@visiativ.com' where email = 'jchapuis@axemble.com'
                        update FTETemp set email = 'tony.bustos@cati.com' where email = 'tony.bustos@cati.com;support@cati.com'";
            SimpleQuery(query);

        }

        /// <summary>
        /// Clear all FTE values EXCEPT Japan learners (fte@varname.com)
        /// </summary>
        internal void clearAllFTEValues()
        {
            string query = @"UPDATE Learners
                            SET Learners.fte_value = 0
                            WHERE Learners.email NOT LIKE 'fte@%'";  //ignore japan FTE learners            
            SimpleQuery(query);
        }

        /// <summary>
        /// Reset and Load Sales and Tech FTE values into VARParents table
        /// </summary>
        internal void SetVARParentFTEValues()
        {
            string query = @"update varparents set ftetech = 0, ftesales = 0
                            update VARParents set VARParents.FTETech = FTETechSalesValuesParent.[FTE Value] 
                            from FTETechSalesValuesParent where VARParents.ID = FTETechSalesValuesParent.ID;
                            update varparents set varparents.ftesales = ftevaluesparent.[fte value]
                            from ftevaluesparent where varparents.id = ftevaluesparent.id;";
            SimpleQuery(query);
        }

        /// <summary>
        /// New Method to get data from temp table - faster for repetitive queries
        /// </summary>
        /// <param name="varFilter"></param>
        /// <returns></returns>
        public DataTable getTransposedReport5(string varFilter = "%", string profileFilter = "%", string geoFilter = "%",
            TableTools.Type TableType = TableTools.Type.tableSalesTransposed)
        {
            DataTable table = new DataTable();

            string tableName = TableType == TableTools.Type.tableSalesTransposed ? TempTableName : TempVTTableName;
            string query = @"select * from " + tableName + " where [VAR Alias] like @var";

            //query = query.Replace("#TEMPTABLENAME#", TempTableName);
            using (var cmd = new SqlCommand(query, dbConn))
            {
                cmd.Parameters.Add("@var", SqlDbType.VarChar).Value = varFilter;
                table = TableQuery(cmd, query);
            }

            table.Columns.Remove("ID");
            return table;
        }

        /// <summary>
        /// OBSOLETE: Create a transposed table to speed up repetitive queries - for VAR reporting, splits tech sales and sales points based on role
        /// </summary>
        /// <param name="varFilter">% for all, or goengin% </param>
        /// <param name="profileFilter">Reseller%, Employee% or % for all</param>
        /// <param name="geoFilter">% for all or AP South%</param>
        /// <returns>True if successful</returns>
        public bool CreateTransposedTempTable(string varFilter = "%", string profileFilter = "%", string geoFilter = "%")
        {
            int rows = 0;
            string query = @"DECLARE @cd nvarchar(max), @courses nvarchar(max), @coursesPTS nvarchar(max), @query1 nvarchar(max), @query2 nvarchar(max), @query3 nvarchar(max), @query0 nvarchar(max);
                            set @query0 = 'SELECT * Into '+@TableName+' FROM ('
                            set @coursesPTS = STUFF(
	                            REPLACE(
                                REPLACE((select Header from ((select ','+QUOTENAME([CourseNameAlias])+' As '+QUOTENAME(concat(CourseNameAlias, ' (', alignment_points, ' pts.)')) as Header, domain_id from CourseAlias where alignment_points > 0 and domain_id Is Not Null and domain_cert != 1 and domain_id < 12)  -- added domain_cert filter 8/10/2021
	                            UNION (select ','+QUOTENAME(CourseNameAlias) as Header, domain_id from CourseAlias where alignment_points = 0 and domain_id Is Not Null AND domain_id < 12 AND domain_id > 0 AND domain_cert != 1)) as tbl order by domain_id, Header FOR XML PATH('')), '<Header>', '')  -- added domain_cert filter 8/10/2021
	                            , '</Header>', '' )
	                            , 1, 1, '');
                            set @courses = STUFF(
                                (SELECT DISTINCT ','+QUOTENAME([CourseNameAlias])
                            FROM [CourseAlias] c WHERE c.domain_cert != 1 AND c.domain_id < 12 AND c.domain_id > 0 FOR XML PATH('')), 1, 1, '');  -- added domain_cert filter 8/10/2021, filtering out SIMULIA Legacy 10/11/2021
                            --print @courses;
                            --print @coursesPTS;
							set @cd = 'CASE WHEN MAX(Activities.status) = ''Unenrolled'' THEN null  WHEN MAX(completionDate) is null THEN ''In Progress'' ELSE convert(varchar,MAX(completionDate), 101) END as ''Completion Date'''; 
							--print @cd;                            
                            set @query1 = 'SELECT PT.ID, PT.Name,  email, Role, [FTE Value], [VAR Alias], company, Country, GEO, ISNULL(TotalAchievement,0) as TotalAchievement, '+@coursesPTS+'
                            FROM (select Learners.ID, Learners.Name, Learners.email, Learners.fte_value as [FTE Value], Learners.[Role], VARAlias.VAR_Alias as ''VAR Alias'', VARs.Name as ''company'', Learners.Country, GEOs.GEO, '+@cd+', CourseAlias.CourseNameAlias
                            from Activities join Learners on Learners.ID = Activities.learner_id join VARs on VARs.ID = Learners.var_id
                            join VARAlias on VARAlias.ID = VARs.var_alias_id join GEOs on GEOs.ID = Learners.geo_id left outer join Courses on Courses.ID = Activities.course_id
                            left outer join CourseAlias on CourseAlias.ID = Courses.alias_id
                            where (Activities.status != ''Not Started'' OR Courses.ID IS NULL ) AND Learners.userState = ''ACTIVE'' AND Learners.profile like '''+@profile+''' AND GEOs.GEO like '''+@geo+''' AND VARAlias.VAR_Alias like '''+@var+'''
                            Group by Learners.ID, Learners.Name, Learners.email, Learners.fte_value, Learners.[Role], VARAlias.VAR_Alias, VARs.Name, Learners.Country, GEOs.GEO, CourseAlias.CourseNameAlias
                            ) AS SourceTable PIVOT(MAX([Completion Date]) FOR [CourseNameAlias] IN('+@courses+')) AS PT '
                            set @query2 = 'left outer join (SELECT SUM ( CASE WHEN domain_id <> 9 and Role like ''[siv]%''
							THEN alignment_points ELSE 0 END) +  --add the two values
							SUM ( CASE WHEN domain_id = 9 and Role like ''tech%'' and Role not like ''%support''
							THEN alignment_points ELSE 0 END) as TotalAchievement, ID, Name
                            From (select aa.domain_id, aa.ID, aa.Name, aa.email, aa.Role, aa.[VAR Alias], aa.company, aa.Country, aa.GEO, aa.[Completion Date], aa.CourseNameAlias
                            ,CASE WHEN aa.[Completion Date] = ''In Progress'' THEN 0 ELSE aa.alignment_points END AS [alignment_points] 
                            from (select CourseAlias.domain_id, Learners.ID, Learners.Name, Learners.email, Learners.[Role], VARAlias.VAR_Alias as [VAR Alias], 
                            VARs.Name as [company], Learners.Country, GEOs.GEO,  '+@cd+', 
                            CourseAlias.CourseNameAlias
							, CourseAlias.alignment_points
                            from Activities join Learners on Learners.ID = Activities.learner_id join VARs on VARs.ID = Learners.var_id join VARAlias on VARAlias.ID = VARs.var_alias_id
                            join GEOs on GEOs.ID = Learners.geo_id join Courses on Courses.ID = Activities.course_id join CourseAlias on CourseAlias.ID = Courses.alias_id
                            where (Activities.status = ''In Progress'' OR Activities.status = ''Completed'') AND Learners.userState = ''ACTIVE'' AND Learners.profile like '''+@profile+''' 
                            AND GEOs.GEO like '''+@geo+''' AND VARAlias.VAR_Alias like '''+@var+'''
                            Group by CourseAlias.domain_id, Learners.ID, Learners.Name, Learners.email, Learners.[Role], VARAlias.VAR_Alias, VARs.Name, 
                            Learners.Country, GEOs.GEO, CourseAlias.CourseNameAlias, CourseAlias.alignment_points) as aa) as tbl 
                            Group By ID, Name) as tblScore on tblScore.ID = PT.ID
                            ) AS temp'                               
                            --print @query1
                            --print @query2;
                            execute (@query0+@query1+@query2);";
            using (var cmd = new SqlCommand(query, dbConn))
            {
                cmd.Parameters.Add("@var", SqlDbType.VarChar).Value = varFilter;
                cmd.Parameters.Add("@profile", SqlDbType.VarChar).Value = profileFilter;
                cmd.Parameters.Add("@geo", SqlDbType.VarChar).Value = geoFilter;
                cmd.Parameters.AddWithValue("@TableName", TempTableName);
                rows = cmd.ExecuteNonQuery();
            }
            if (rows > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Create transposed table to speed up repetitive queries
        /// Include CSSP 40 bonus if no CSSS
        /// Separates Sales and Tech Sales points
        /// </summary>
        /// <param name="varFilter"></param>
        /// <param name="profileFilter"></param>
        /// <param name="geoFilter"></param>
        /// <returns></returns>
        public bool CreateTransposedTempTable2(string varFilter = "%", string profileFilter = "%", string geoFilter = "%")
        {
            int rows = 0;
            //changed CASE WHEN a.status.  The ELSE was 'In Progress', now  a.status to show expired courses (row 4)
            string query = @"Declare @roleFilter varchar(10) = '[siv]%', @kpi int;
                --create initial completions temp table
                select a.[order], a.domain_id, l.ID, l.Name, l.email, l.fte_value as [FTE Value], l.Role, l.VAR_Alias as [VAR Alias], l.company, l.Country, l.GEO, 
                CASE WHEN a.status = 'completed' THEN CONVERT(varchar, MAX(a.completionDate), 101) ELSE a.status END as [Completion Date], a.CourseNameAlias, a.alignment_points
                INTO #completions
                FROM ActivitiesDetail a RIGHT OUTER JOIN AllActiveLearners l on l.ID = a.learner_id
                --WHERE l.profile = 'reseller' AND l.GEO like @geoFilter AND l.VAR_Alias like @varFilter
                WHERE l.GEO like @geoFilter AND l.VAR_Alias like @varFilter  --removed reseller profile filter
                GROUP BY a.[order], a.domain_id, l.ID, l.Name, l.email, l.fte_value, l.Role, l.VAR_Alias, l.company, l.Country, l.GEO, a.CourseNameAlias, a.alignment_points, a.status

                --make a bonus temp table
                set @kpi = 1; --sales courses only
                select distinct a.CourseNameAlias, l.Name, l.ID, a.alignment_points, l.VAR_Alias, a.status, l.Role, a.kpi
	                INTO #tempTable from ActivitiesDetail a JOIN AllActiveLearners l on a.learner_id = l.ID
                    where VAR_Alias like @varFilter AND l.Role like @roleFilter AND a.alignment_points > 0 AND a.domain_cert != 1 AND a.kpi = @kpi;						

                /* calculate bonus points for old CSSP completions */
                declare @cFilter varchar(10) = '%CSS%', @cssp varchar(10) = '%CSSP%';
	                select COUNT(t.Name) as cnt, t.Name, t.ID, 40 as [bonus] --, t.CourseNameAlias
		                INTO #bonusTable FROM #tempTable t 
                        JOIN (/* count the number of name instances after filtering on CSS% */
				                select COUNT(tt.Name) as cnt, tt.Name --, t.CourseNameAlias
				                FROM #tempTable tt
				                where tt.CourseNameAlias like @cFilter and tt.Role like @roleFilter
				                group by tt.Name
				                having COUNT(tt.Name) = 1 ) as one on one.Name = t.Name
                        where t.CourseNameAlias like @cssp AND t.Role like @roleFilter
		                group by t.Name, t.ID having COUNT(t.Name) = 1;

                --create the course lists
                Declare @coursesPTS varchar(max), @courses varchar(max);
                set @coursesPTS = STUFF(
	                REPLACE(
                    REPLACE((select Header from ((select ','+QUOTENAME([CourseNameAlias])+' As '+QUOTENAME(concat(CourseNameAlias, ' (', alignment_points, ' pts.)')) as Header, domain_id from CourseAlias where alignment_points > 0 and domain_id Is Not Null and domain_cert != 1 and domain_id < 12)  -- added domain_cert filter 8/10/2021
	                UNION (select ','+QUOTENAME(CourseNameAlias) as Header, domain_id from CourseAlias where alignment_points = 0 and domain_id Is Not Null AND domain_id < 12 AND domain_id > 0 AND domain_cert != 1)) as tbl order by domain_id, Header FOR XML PATH('')), '<Header>', '')  -- added domain_cert filter 8/10/2021
	                , '</Header>', '' )
	                , 1, 1, '');
                set @courses = STUFF(
                    (SELECT DISTINCT ','+QUOTENAME([CourseNameAlias])
                FROM [CourseAlias] c WHERE c.domain_cert != 1 AND c.domain_id < 12 AND c.domain_id > 0 FOR XML PATH('')), 1, 1, '');  -- added domain_cert filter 8/10/2021, filtering out SIMULIA Legacy 10/11/2021

                --Create the pivot
                Declare @query varchar(max), @queryHead varchar(max), @queryTail varchar(max);
                set @queryHead = 'SELECT * Into '+@TableName+' FROM (';
                set @query = 'SELECT PT.ID, PT.Name,  email, Role, [FTE Value], [VAR Alias], company, Country, GEO, ISNULL(TotalAchievement,0) as TotalAchievement, ' + @coursesPTS + '
                    FROM (SELECT ID, Name, email, [FTE Value], Role, [VAR Alias], company, Country, GEO, [Completion Date], CourseNameAlias FROM  #completions ) AS T1 
	                PIVOT(MAX([Completion Date]) FOR [CourseNameAlias] IN(' + @courses + ')) AS PT 
	                LEFT OUTER JOIN ( SELECT SUM(TotalAchievement + ISNULL(bonus,0)) as TotalAchievement, ta.ID, ta.Name FROM(
			                SELECT SUM ( CASE WHEN domain_id <> 9 and Role like ''[siv]%''
			                THEN alignment_points ELSE 0 END) +  --add the two values
			                SUM ( CASE WHEN domain_id = 9 and Role like ''tech%'' and Role not like ''%support''
			                THEN alignment_points ELSE 0 END) as TotalAchievement, tbl.ID, tbl.Name
                            From (select c.[order], c.domain_id, c.ID, c.Name, c.email, c.[FTE Value], c.Role, c.[VAR Alias], c.company, c.Country, c.GEO, c.[Completion Date], c.coursenamealias
			                , CASE WHEN c.[Completion Date] = ''In Progress'' THEN 0 ELSE c.alignment_points END AS [alignment_points]
			                FROM #completions c) as tbl
                            Group By tbl.ID, tbl.Name) AS ta 
			                LEFT OUTER JOIN #bonusTable bt on bt.ID = ta.ID Group By ta.ID, ta.Name
			                ) as tblScore on tblScore.ID = PT.ID';
                set @queryTail = ') AS temp';

                exec (@queryHead + @query + @queryTail);";
            using (var cmd = new SqlCommand(query, dbConn))
            {
                cmd.Parameters.Add("@varFilter", SqlDbType.VarChar).Value = varFilter;
                cmd.Parameters.Add("@profile", SqlDbType.VarChar).Value = profileFilter;
                cmd.Parameters.Add("@geoFilter", SqlDbType.VarChar).Value = geoFilter;
                cmd.Parameters.AddWithValue("@TableName", TempTableName);
                rows = cmd.ExecuteNonQuery();
            }
            if (rows > 0)
                return true;
            else
                return false;

        }

        /// <summary>
        /// Create a transposed table to speed up repetitive queries - for VAR reporting
        /// Virtual Tester data
        /// </summary>
        /// <param name="varFilter">% for all, or goengin% </param>
        /// <param name="profileFilter">Reseller%, Employee% or % for all</param>
        /// <param name="geoFilter">% for all or AP South%</param>
        /// <returns>True if successful</returns>
        public bool CreateTransposedVTTempTable()
        {
            int rows = 0;
            string query = @"DECLARE @cd nvarchar(max), @courses nvarchar(max), @coursesPTS nvarchar(max), @query1 nvarchar(max), @query2 nvarchar(max), @query3 nvarchar(max), @query0 nvarchar(max);
                               set @coursesPTS = STUFF(
	                            REPLACE(
                                REPLACE((select Header from ((select ','+QUOTENAME([CourseNameAlias])+' As '+QUOTENAME(concat(CourseNameAlias, ' (', alignment_points, ' pts.)')) as Header, domain_id from CourseAlias where alignment_points > 0 and domain_id in (SELECT d.ID from Domains d where d.DomainName like @Domain) and domain_cert != 1)  -- added domain_cert filter 8/10/2021
	                            UNION (select ','+QUOTENAME(CourseNameAlias) as Header, domain_id from CourseAlias where alignment_points = 0 and domain_id in (SELECT d.ID from Domains d where d.DomainName like @Domain) AND domain_cert != 1)) as tbl order by domain_id, Header FOR XML PATH('')), '<Header>', '')  -- added domain_cert filter 8/10/2021
	                            , '</Header>', '' )
	                            , 1, 1, '');
                            set @courses = STUFF((SELECT DISTINCT ','+QUOTENAME([CourseNameAlias]) 
		                        FROM [CourseAlias] c WHERE c.domain_cert != 1 AND c.domain_id in (SELECT d.ID from Domains d where d.DomainName like @Domain) FOR XML PATH('')), 1, 1, '');
                            print @courses;
                            print @coursesPTS;
	                        set @cd = 'CASE WHEN MAX(Activities.status) = ''Unenrolled'' THEN null  WHEN MAX(completionDate) is null THEN ''In Progress'' ELSE convert(varchar,MAX(completionDate), 101) END as ''Completion Date'''; 
	                        --print @cd;   
                            set @query0 = 'SELECT * Into '+@TableName+' FROM ('                         
                            set @query1 = 'SELECT tblScore.ID, tblScore.Name,  email, Role, [FTE Value], [VAR Alias], company, Country, GEO, null as ''TotalAchievement'' ,'+@coursesPTS+'
                            FROM (select Learners.ID, Learners.Name, Learners.email, Learners.fte_value as [FTE Value], Learners.[Role], VARAlias.VAR_Alias as ''VAR Alias'', VARs.Name as ''company'', Learners.Country, GEOs.GEO, '+@cd+', CourseAlias.CourseNameAlias
                            from Activities join Learners on Learners.ID = Activities.learner_id join VARs on VARs.ID = Learners.var_id
                            join VARAlias on VARAlias.ID = VARs.var_alias_id join GEOs on GEOs.ID = Learners.geo_id left outer join Courses on Courses.ID = Activities.course_id
                            left outer join CourseAlias on CourseAlias.ID = Courses.alias_id
                            where (Activities.status != ''Not Started'' OR Courses.ID IS NULL ) AND Learners.userState = ''ACTIVE'' AND Learners.profile like '''+@profile+''' AND GEOs.GEO like '''+@geo+''' AND VARAlias.VAR_Alias like '''+@var+'''
                            Group by Learners.ID, Learners.Name, Learners.email, Learners.fte_value, Learners.[Role], VARAlias.VAR_Alias, VARs.Name, Learners.Country, GEOs.GEO, CourseAlias.CourseNameAlias
                            ) AS SourceTable PIVOT(MAX([Completion Date]) FOR [CourseNameAlias] IN('+@courses+')) AS PT '
                            set @query2 = 'join (select SUM(alignment_points) as [TotalAchievement], ID, Name
                            From (select aa.ID, aa.Name, aa.email, aa.Role, aa.[VAR Alias], aa.company, aa.Country, aa.GEO, aa.[Completion Date], aa.CourseNameAlias
                            ,CASE WHEN aa.[Completion Date] = ''In Progress'' THEN 0 ELSE aa.alignment_points END AS [alignment_points] 
                            from (select Learners.ID, Learners.Name, Learners.email, Learners.[Role], VARAlias.VAR_Alias as [VAR Alias], 
                            VARs.Name as [company], Learners.Country, GEOs.GEO,  '+@cd+', 
                            CourseAlias.CourseNameAlias
	                        , CourseAlias.alignment_points
                            from Activities join Learners on Learners.ID = Activities.learner_id join VARs on VARs.ID = Learners.var_id join VARAlias on VARAlias.ID = VARs.var_alias_id
                            join GEOs on GEOs.ID = Learners.geo_id join Courses on Courses.ID = Activities.course_id join CourseAlias on CourseAlias.ID = Courses.alias_id
                            where (Activities.status = ''In Progress'' OR Activities.status = ''Completed'') AND Learners.userState = ''ACTIVE'' AND Learners.profile like '''+@profile+''' AND Learners.Role like '''+@role+'''
                            AND GEOs.GEO like '''+@geo+''' AND VARAlias.VAR_Alias like '''+@var+'''
                            Group by Learners.ID, Learners.Name, Learners.email, Learners.[Role], VARAlias.VAR_Alias, VARs.Name, 
                            Learners.Country, GEOs.GEO, CourseAlias.CourseNameAlias, CourseAlias.alignment_points) as aa) as tbl 
                            Group By ID, Name) as tblScore on tblScore.ID = PT.ID
                            ) AS temp'                         
                            --print @query1
                            --print @query2;
                            execute (@query0+@query1+@query2);";
            using (var cmd = new SqlCommand(query, dbConn))
            {
                cmd.Parameters.AddWithValue("@Domain", "virtual%");
                cmd.Parameters.Add("@var", SqlDbType.VarChar).Value = "%";
                cmd.Parameters.Add("@profile", SqlDbType.VarChar).Value = "reseller%";  //resellers only
                cmd.Parameters.AddWithValue("@role", "%tech%");
                cmd.Parameters.Add("@geo", SqlDbType.VarChar).Value = "%";
                cmd.Parameters.AddWithValue("@TableName", TempVTTableName);
                rows = cmd.ExecuteNonQuery();
            }
            if (rows > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Specific report output by Sandrine's request
        /// </summary>
        /// <param name="varFilter"></param>
        /// <param name="profileFilter"></param>
        /// <param name="geoFilter"></param>
        /// <returns></returns>
        public DataTable GetTransposedReportByPoints(string varFilter = "%", string profileFilter = "%", string geoFilter = "%")
        {
            DataTable table = new DataTable();
            //Sept. 7, 2021 - added CASE WHEN filter for 'Unenrolled' learners so they won't show as In Progress.
            string query = @"Declare @roleFilter varchar(10) = '[siv]%', @kpi int;
                    --create initial completions temp table
                    select a.[order], a.domain_id, l.ID, l.Name, l.email, l.fte_value as [FTE Value], l.Role, l.VAR_Alias as [VAR Alias], l.company, l.Country, l.GEO, 
                    CASE WHEN a.status = 'completed' THEN CONVERT(varchar, MAX(a.completionDate), 101) ELSE 'In Progress' END as [Completion Date], a.CourseNameAlias, a.alignment_points
	                INTO #completions
                    FROM ActivitiesDetail a RIGHT OUTER JOIN AllActiveLearners l on l.ID = a.learner_id
                    --WHERE l.profile = 'reseller' AND l.GEO like @geoFilter AND l.VAR_Alias like @varFilter
                    WHERE l.GEO like @geoFilter AND l.VAR_Alias like @varFilter  --removed reseller profile filter
                    GROUP BY a.[order], a.domain_id, l.ID, l.Name, l.email, l.fte_value, l.Role, l.VAR_Alias, l.company, l.Country, l.GEO, a.CourseNameAlias, a.alignment_points, a.status

                    --make a bonus temp table
                    set @kpi = 1; --sales courses only
                    select distinct a.CourseNameAlias, l.Name, l.ID, a.alignment_points, l.VAR_Alias, a.status, l.Role, a.kpi
		                INTO #tempTable 
		                from ActivitiesDetail a JOIN AllActiveLearners l on a.learner_id = l.ID
                        where VAR_Alias like @varFilter AND l.Role like @roleFilter AND a.alignment_points > 0 AND a.domain_cert != 1 AND a.kpi = @kpi;						

                    /* calculate bonus points for old CSSP completions */
                    declare @cFilter varchar(10) = '%CSS%', @cssp varchar(10) = '%CSSP%';
	                    select COUNT(t.Name) as cnt, t.Name, t.ID, 40 as [bonus] --, t.CourseNameAlias
			                INTO #bonusTable 
			                FROM #tempTable t 
                            JOIN (/* count the number of name instances after filtering on CSS% */
				                    select COUNT(tt.Name) as cnt, tt.Name --, t.CourseNameAlias
				                    FROM #tempTable tt
				                    where tt.CourseNameAlias like @cFilter and tt.Role like @roleFilter
				                    group by tt.Name
				                    having COUNT(tt.Name) = 1 ) as one on one.Name = t.Name
                            where t.CourseNameAlias like @cssp AND t.Role like @roleFilter
		                    group by t.Name, t.ID having COUNT(t.Name) = 1;

                    --create the course lists
                    Declare @coursesPTS varchar(max), @courses varchar(max);
                    set @coursesPTS = STUFF(
	                    REPLACE(
                        REPLACE((select Header from ((select ','+QUOTENAME([CourseNameAlias])+' As '+QUOTENAME(concat(CourseNameAlias, ' (', alignment_points, ' pts.)')) as Header, domain_id from CourseAlias where alignment_points > 0 and domain_id Is Not Null and domain_cert != 1 and domain_id < 12)  -- added domain_cert filter 8/10/2021
	                    UNION (select ','+QUOTENAME(CourseNameAlias) as Header, domain_id from CourseAlias where alignment_points = 0 and domain_id Is Not Null AND domain_id < 12 AND domain_id > 0 AND domain_cert != 1)) as tbl order by domain_id, Header FOR XML PATH('')), '<Header>', '')  -- added domain_cert filter 8/10/2021
	                    , '</Header>', '' )
	                    , 1, 1, '');
                    set @courses = STUFF(
                        (SELECT DISTINCT ','+QUOTENAME([CourseNameAlias])
                    FROM [CourseAlias] c WHERE c.domain_cert != 1 AND c.domain_id < 12 AND c.domain_id > 0 FOR XML PATH('')), 1, 1, '');  -- added domain_cert filter 8/10/2021, filtering out SIMULIA Legacy 10/11/2021

                    --Create the pivot
                    Declare @query varchar(max);
                    set @query = 'SELECT PT.ID, PT.Name,  email, Role, [FTE Value], [VAR Alias], company, Country, GEO, ISNULL(TotalAchievement,0) as TotalAchievement, ' + @coursesPTS + '
                        FROM (SELECT ID, Name, email, [FTE Value], Role, [VAR Alias], company, Country, GEO, [alignment_points], CourseNameAlias FROM  #completions ) AS T1 
	                    PIVOT(MAX([alignment_points]) FOR [CourseNameAlias] IN(' + @courses + ')) AS PT 
	                    LEFT OUTER JOIN ( SELECT SUM(TotalAchievement + ISNULL(bonus,0)) as TotalAchievement, ta.ID, ta.Name FROM(
			                    SELECT SUM ( CASE WHEN domain_id <> 9 and Role like ''[siv]%''
			                    THEN alignment_points ELSE 0 END) +  --add the two values
			                    SUM ( CASE WHEN domain_id = 9 and (Role like ''tech%sales'' OR Role like ''tech%manager%'')
			                    THEN alignment_points ELSE 0 END) as TotalAchievement, tbl.ID, tbl.Name
                                From (select c.[order], c.domain_id, c.ID, c.Name, c.email, c.[FTE Value], c.Role, c.[VAR Alias], c.company, c.Country, c.GEO, c.[Completion Date], c.coursenamealias
			                    , CASE WHEN c.[Completion Date] = ''In Progress'' THEN 0 ELSE c.alignment_points END AS [alignment_points]
			                    FROM #completions c) as tbl
                                Group By tbl.ID, tbl.Name) AS ta 
			                    LEFT OUTER JOIN #bonusTable bt on bt.ID = ta.ID Group By ta.ID, ta.Name
			                    ) as tblScore on tblScore.ID = PT.ID';

                   exec (@query);";
            //@query3 UNION doesn't work as-is. Would need to change each course column (@courses) to 'null = [columname]'
            using (var cmd = new SqlCommand(query, dbConn))
            {
                cmd.Parameters.Add("@varFilter", SqlDbType.VarChar).Value = varFilter;
                //cmd.Parameters.Add("@profile", SqlDbType.VarChar).Value = profileFilter;
                cmd.Parameters.Add("@geoFilter", SqlDbType.VarChar).Value = geoFilter;
                table = TableQuery(cmd, query);
            }

            table.Columns.Remove("ID");
            return table;
        }

        //broken down by VAR Parent
        public DataTable GetFTEVals2(string varFilter = "%", string geoFilter = "%")
        {
            DataTable table = new DataTable();
            string query = @"select [VAR Alias], VAR_Parent as 'VAR Parent', SUM([FTE Value]) as 'FTE Value'
                            from (select VAR_Alias as [VAR Alias],
                            VARParents.VAR_Parent, 
                            MAX(FTE.[FTE Value]) as 'FTE Value',
                            (FTE.[First Name] + ' ' + FTE.[Last Name]) as 'Name'
                            from FTE
	                            join VARs on VARs.var_parent_id = FTE.parent_id
	                            join VARAlias on VARAlias.ID = VARs.var_alias_id
	                            join VARParents on VARParents.ID = FTE.parent_id
	                            join GEOs on GEOs.ID = VARAlias.geo_id
                            where VAR_Alias like @VARFilter 
                            AND FTE.[FTE Status] = 'Active' 
                            AND FTE.[Account Status] = 'Active' 
                            AND [FTE Value] > 0
                            AND GEOs.GEO like @GEOFilter
                            group by [VAR_Alias], VAR_Parent, [First Name], [Last Name]) as RES
                            group by [VAR Alias], VAR_Parent;";
            using (var cmd = new SqlCommand(query, dbConn))
            {
                cmd.Parameters.Add("@VARFilter", SqlDbType.VarChar).Value = varFilter;
                cmd.Parameters.Add("@GEOFilter", SqlDbType.VarChar).Value = geoFilter;
                return TableQuery(cmd, query);
            }
        }
        public DataTable getCourseCompletions5(string varFilter = "%",
            string profileFilter = "%", string geoFilter = "%", string domainFilter = "%",
            string roleFilter = "%", int domainType = 0, bool domainCertsOnly = true)
        {
            string query = "";
            int varID = 0;
            query = @"select t1.GEO, t1.VAR_Alias as [VAR Alias], t1.CourseNameAlias as Course, 
                    ISNULL(RES.Completed, 0) as Completed
                    , ISNULL(RES.[In Progress], 0) as [In Progress], ISNULL(RES.[Not Started], 0) as [Not Started], t1.Target 
                    ,(t1.CourseNameAlias + '/' + t1.GEO + '/' + t1.VAR_Alias) as 'Drilldown', t1.course_alias_id, t1.var_alias_id
                    from (select GEOs.GEO, VARAlias.VAR_Alias, VARAlias.ID as var_alias_id, CourseAlias.CourseNameAlias, 
                    CourseAlias.ID as course_alias_id, 
                    CAST(CourseAlias.Target AS decimal(5,1)) AS Target,
                    Domains.DomainType
                    from VARAlias
                    left join CourseAlias on 1=1
                    join GEOs on GEOs.ID = VARAlias.geo_id
                    join Domains on Domains.ID = CourseAlias.domain_id
                    where VARAlias.VAR_Alias like @VARFilter and GEO like @GeoFilter and 
                    #WHEREDOMAIN# and DomainName like @DomainFilter
                    AND VARAlias.VAR_Alias not like 'dassault%'
                    AND CourseAlias.domain_cert = @cert) as t1  -- removed Certifications
                    left join (
                    select GEO, [VAR Alias], [Course], [Completed], [In Progress], [Not Started], [Target], DomainName
	                    --added columns
	                    ,var_alias_id, course_alias_id
                    from
                    (select
	                     GEOs.GEO,
	                    VARAlias.VAR_Alias as 'VAR Alias',
	                    CourseAlias.CourseNameAlias as Course,
	                    Activities.[status] as completion,
                        CAST(CourseAlias.Target AS decimal(5,1)) AS Target,                    
	                    Learners.[Name] as lName, [Domains].DomainName
	                    --added columns
	                    ,VARAlias.ID as var_alias_id
	                    ,CourseAlias.ID as course_alias_id
                    from Activities
	                    join Learners on Learners.ID = Activities.learner_id
	                    join Courses on Courses.ID = Activities.course_id
	                    join VARs on VARs.ID = Learners.var_id
	                    join VARAlias on VARAlias.ID = VARs.var_alias_id
	                    join GEOs on GEOs.ID = Learners.geo_id
	                    join CourseAlias on CourseAlias.ID = Courses.alias_id
	                    join [Domains] on [Domains].ID = CourseAlias.domain_id
                    where
	                    VARAlias.VAR_Alias like @VARFilter
                        AND VARAlias.VAR_Alias not like 'dassault%'  --always ignore employees in count
	                    AND Learners.userState = 'ACTIVE'
	                    AND Learners.[Role] like @RoleFilter
	                    -- AND Learners.[Profile] like @ProfileFilter -- no need for profile if we ignore all DS employees in VARAlias filter above
	                    AND GEO like @GeoFilter
	                    AND IsNull([DomainName], '') like @DomainFilter                        
                        AND #WHEREDOMAIN#
	                    ) as SourceData PIVOT(COUNT(lName) FOR completion 
                            IN([Completed], [In Progress], [Not Started])) AS PT  
			            ) as RES on RES.var_alias_id = t1.var_alias_id AND RES.course_alias_id = t1.course_alias_id";

            string domainSub = (domainType == -1) ? "[DomainType] > @DomainType" : "[DomainType] = @DomainType";
            query = query.Replace("#WHEREDOMAIN#", domainSub);

            //DataTable table = new DataTable();
            using (var cmd = new SqlCommand(query, dbConn))
            {
                cmd.Parameters.AddWithValue("@cert", domainCertsOnly ? 1 : 0);
                cmd.Parameters.AddWithValue("@VARFilter", varFilter);
                cmd.Parameters.Add("@GeoFilter", SqlDbType.VarChar).Value = geoFilter;
                cmd.Parameters.Add("@DomainFilter", SqlDbType.VarChar).Value = domainFilter;
                cmd.Parameters.Add("@RoleFilter", SqlDbType.VarChar).Value = roleFilter;
                cmd.Parameters.Add("@DomainType", SqlDbType.Int).Value = domainType;
                return TableQuery(cmd, query);
            }
        }

        public DataTable getCourseCompletionsDrilldown(string varFilter, string domainFilter, int domainType,
            string profileFilter = "reseller", string roleFilter = "%sales%", string geoFilter = "%", bool includeCerts = true)
        {
            string query = @"select GEO, [VAR Alias], [Course], [Completed], [In Progress], [Not Started], [Target]
                            , ([Course] + '/' + lName) as [Drilldown] 
                             , DomainName
                            ,var_alias_id, course_alias_id
                            from
                            ( select distinct GEOs.GEO,
	                            VARAlias.VAR_Alias as 'VAR Alias',
	                            CourseAlias.CourseNameAlias as Course,
	                            Activities.[status] as completion,
                                CAST(CourseAlias.Target AS decimal(5,1)) AS Target,                    
	                            Learners.[Name] as lName, [Domains].DomainName
	                            --added columns
	                            ,VARAlias.ID as var_alias_id
	                            ,CourseAlias.ID as course_alias_id
                            from Activities
	                            join Learners on Learners.ID = Activities.learner_id
	                            join Courses on Courses.ID = Activities.course_id
	                            join VARs on VARs.ID = Learners.var_id
	                            join VARAlias on VARAlias.ID = VARs.var_alias_id
	                            join GEOs on GEOs.ID = Learners.geo_id
	                            join CourseAlias on CourseAlias.ID = Courses.alias_id
	                            join [Domains] on [Domains].ID = CourseAlias.domain_id
                            where
	                            VARAlias.VAR_Alias like @VARFilter
                                AND VARAlias.VAR_Alias not like 'dassault%'  --always ignore employees in count
	                            AND Learners.userState = 'ACTIVE'
	                            AND Learners.[Role] like @RoleFilter
	                            -- AND Learners.[Profile] like @ProfileFilter -- not needed because varalias is filtered 8/10/2021
	                            AND GEO like @GeoFilter
	                            AND IsNull([DomainName], '') like @DomainFilter                        
                                AND [DomainType] = @DomainType
                                AND CourseAlias.domain_cert <= @cert  -- added to filter certifications 8/10/2021
	                            ) as SourceData PIVOT(COUNT(completion) for completion
                                    IN([Completed], [In Progress], [Not Started])) AS PT";

            using (var cmd = new SqlCommand(query, dbConn))
            {
                cmd.Parameters.AddWithValue("@cert", includeCerts ? 1 : 0);  //added to filter certifications 8/10/2021
                cmd.Parameters.AddWithValue("@VARFilter", varFilter);
                cmd.Parameters.Add("@GeoFilter", SqlDbType.VarChar).Value = geoFilter;
                cmd.Parameters.Add("@DomainFilter", SqlDbType.VarChar).Value = domainFilter;
                cmd.Parameters.Add("@RoleFilter", SqlDbType.VarChar).Value = roleFilter;
                cmd.Parameters.Add("@DomainType", SqlDbType.Int).Value = domainType;
                return TableQuery(cmd, query);
            }
        }

        public DataTable getCustomTargets()
        {
            string query = @"select * from CustomTargets;";
            using (var cmd = new SqlCommand(query, dbConn))
                return TableQuery(cmd, query);
        }

        public DataTable getQuarterlyProgressTable2(string varFilter = "%",
            string profileFilter = "%", string geoFilter = "%", string domainFilter = "%",
            string roleFilter = "%sales%", int domainType = 0, DateFormat format = DateFormat.quarter)
        {
            DataTable table = new DataTable();
            //quarterly query string
            string query = @"DECLARE @courses nvarchar(max), @query nvarchar(max);
                        set @courses = STUFF(
                                (SELECT DISTINCT ','+QUOTENAME([CourseNameAlias])
                            FROM [CourseAlias] 
		                        join [Domains] on [Domains].ID = [CourseAlias].domain_id
	                        WHERE [Domains].DomainName LIKE @DomainFilter
	                        AND [Domains].DomainType = @DomainType
                            AND CourseAlias.domain_cert != 1 -- do not show certifications 8/10/2021
	                        FOR XML PATH('')), 1, 1, '');
                        set @query = '
                        select * from
                        (select GEOs.GEO, Learners.Name, CourseAlias.CourseNameAlias, --Activities.completionDate,
                        #DATE#
                        from
                        Activities
	                        join Learners on Learners.ID = Activities.learner_id
	                        join Courses on Courses.ID = Activities.course_id
	                        join CourseAlias on CourseAlias.ID = Courses.alias_id
	                        join GEOs on GEOs.ID = Learners.geo_id
	                        join VARs on VARs.ID = Learners.var_id
	                        join VARAlias on VARAlias.ID = VARs.var_alias_id
	                        join Domains on Domains.ID = CourseAlias.domain_id
                        where Learners.userState = ''ACTIVE''
                        AND Activities.[status] = ''Completed''
                        AND Learners.Role like '''+@RoleFilter+'''
                        AND VARAlias.VAR_Alias like '''+@VARFilter+'''
                        AND GEOs.GEO like '''+@GeoFilter+'''
                        AND IsNull([DomainName], '''') like '''+@DomainFilter+'''
                        AND [DomainType] = '+CONVERT(varchar(2), @DomainType)+'
                        AND CourseAlias.domain_cert != 1 -- ignore show certifications 8/10/2021
                        ) as SourceData PIVOT(COUNT([Name]) FOR [CourseNameAlias] IN('+@courses+')) as PT
                        ORDER BY [GEO];'
                        --PRINT @query;
                        EXECUTE (@query);";
            //new options to show monthly or quarterly data
            string monthly = "DATEADD(MONTH, DATEDIFF(MONTH, 0, completionDate), 0) AS Month";
            string quarterly = "(concat(datepart(year,completionDate),'' Q'',ceiling(datepart(month,(completionDate))/(3.0)))) as [quarter]";
            if (format == DateFormat.quarter)
                query = query.Replace("#DATE#", quarterly);
            else if (format == DateFormat.month)
                query = query.Replace("#DATE#", monthly);

            using (var cmd = new SqlCommand(query, dbConn))
            {
                cmd.Parameters.Add("@VARFilter", SqlDbType.VarChar).Value = varFilter;
                cmd.Parameters.Add("@ProfileFilter", SqlDbType.VarChar).Value = profileFilter;
                cmd.Parameters.Add("@GeoFilter", SqlDbType.VarChar).Value = geoFilter;
                cmd.Parameters.Add("@DomainFilter", SqlDbType.VarChar).Value = domainFilter;
                cmd.Parameters.Add("@RoleFilter", SqlDbType.VarChar).Value = roleFilter;
                cmd.Parameters.Add("@DomainType", SqlDbType.Int).Value = domainType;
                return TableQuery(cmd, query);
            }
        }

        public DataTable getQuarterlyProgressTableByCourse(string varFilter = "%",
            string profileFilter = "%", string geoFilter = "%", string courseFilter = "%",
            string roleFilter = "%sales%", DateFormat format = DateFormat.quarter)
        {
            DataTable table = new DataTable();
            //quarterly query string
            string query = @"DECLARE @courses nvarchar(max), @query nvarchar(max);
                        set @courses = STUFF(
                                (SELECT DISTINCT ','+QUOTENAME([CourseNameAlias])
                            FROM [CourseAlias] 
		                        join [Domains] on [Domains].ID = [CourseAlias].domain_id
	                        WHERE [CourseAlias].CourseNameAlias LIKE @courseFilter
                            AND CourseAlias.domain_cert != 1 -- do not show certifications 8/10/2021
	                        FOR XML PATH('')), 1, 1, '');
                        set @query = '
                        select * from
                        (select GEOs.GEO, Learners.Name, CourseAlias.CourseNameAlias, --Activities.completionDate,
                        #DATE#
                        from
                        Activities
	                        join Learners on Learners.ID = Activities.learner_id
	                        join Courses on Courses.ID = Activities.course_id
	                        join CourseAlias on CourseAlias.ID = Courses.alias_id
	                        join GEOs on GEOs.ID = Learners.geo_id
	                        join VARs on VARs.ID = Learners.var_id
	                        join VARAlias on VARAlias.ID = VARs.var_alias_id
	                        join Domains on Domains.ID = CourseAlias.domain_id
                        where Learners.userState = ''ACTIVE''
                        AND Activities.[status] = ''Completed''
                        AND Learners.Role like '''+@RoleFilter+'''
                        AND VARAlias.VAR_Alias like '''+@VARFilter+'''
                        AND GEOs.GEO like '''+@GeoFilter+'''
                        AND CourseAlias.CourseNameAlias LIKE '''+@courseFilter+'''
                        AND CourseAlias.domain_cert != 1 -- ignore show certifications 8/10/2021
                        ) as SourceData PIVOT(COUNT([Name]) FOR [CourseNameAlias] IN('+@courses+')) as PT
                        ORDER BY [GEO];'
                        --PRINT @query;
                        EXECUTE (@query);";
            //new options to show monthly or quarterly data
            string monthly = "DATEADD(MONTH, DATEDIFF(MONTH, 0, completionDate), 0) AS Month";
            string quarterly = "(concat(datepart(year,completionDate),'' Q'',ceiling(datepart(month,(completionDate))/(3.0)))) as [quarter]";
            if (format == DateFormat.quarter)
                query = query.Replace("#DATE#", quarterly);
            else if (format == DateFormat.month)
                query = query.Replace("#DATE#", monthly);

            using (var cmd = new SqlCommand(query, dbConn))
            {
                cmd.Parameters.Add("@VARFilter", SqlDbType.VarChar).Value = varFilter;
                cmd.Parameters.Add("@ProfileFilter", SqlDbType.VarChar).Value = profileFilter;
                cmd.Parameters.Add("@GeoFilter", SqlDbType.VarChar).Value = geoFilter;
                cmd.Parameters.Add("@RoleFilter", SqlDbType.VarChar).Value = roleFilter;
                cmd.Parameters.AddWithValue("@courseFilter", courseFilter);
                return TableQuery(cmd, query);
            }
        }

        public DataTable getQuarterlyProgressWithRole(string varFilter = "%",
            string profileFilter = "%", string geoFilter = "%", string domainFilter = "%",
            string roleFilter = "%", int domainType = 0)
        {
            DataTable table = new DataTable();
            string query = @"DECLARE @courses nvarchar(max), @query nvarchar(max), @query2 nvarchar(max);
                        set @courses = STUFF(
                                (SELECT DISTINCT ','+QUOTENAME([CourseNameAlias])
                            FROM [CourseAlias] 
		                        join [Domains] on [Domains].ID = [CourseAlias].domain_id
	                        WHERE [Domains].DomainName LIKE @DomainFilter
	                        AND [Domains].DomainType = @DomainType
                            AND CourseAlias.domain_cert != 1 -- do not show certifications 8/10/2021
	                        FOR XML PATH('')), 1, 1, '');
                        set @query = 'select * from
                        (select t2.GEO, t2.Role, t1.Name, t2.CourseNameAlias, t2.quarter from 
							(select GEOs.GEO, Learners.Role, Learners.Name, CourseAlias.CourseNameAlias, 
							(concat(datepart(year,completionDate),'' Q'',ceiling(datepart(month,(completionDate))/(3.0)))) as [quarter]
							from Activities
								join Learners on Learners.ID = Activities.learner_id
								join Courses on Courses.ID = Activities.course_id
								join CourseAlias on CourseAlias.ID = Courses.alias_id
								join GEOs on GEOs.ID = Learners.geo_id
								join VARs on VARs.ID = Learners.var_id
								join VARAlias on VARAlias.ID = VARs.var_alias_id
								join Domains on Domains.ID = CourseAlias.domain_id
							where Learners.userState = ''ACTIVE''
							AND Activities.[status] = ''Completed''
							AND Learners.Role like '''+@RoleFilter+'''
							AND VARAlias.VAR_Alias like '''+@VARFilter+'''
							AND GEOs.GEO like '''+@GeoFilter+'''
							AND IsNull([DomainName], '''') like '''+@DomainFilter+'''
							AND [DomainType] = '+CONVERT(varchar(2), @DomainType)+'
							AND CourseAlias.domain_cert != 1 -- ignore show certifications 8/10/2021;
							) as t1 '
							set @query2 = 'right outer join (
							/* All possible geo, role, course and quarter */						
							select distinct g.GEO, l.Role, null Name, c.CourseNameAlias, 
							q.quarter as quarter
							from (select distinct l.Role from Learners l 
							join Activities a on a.learner_id = l.id 
							join courses c on c.id = a.course_id
							join CourseAlias ca on ca.id = c.alias_id
							where ca.domain_id = 12 and A.status = ''COMPLETED'' AND l.profile = ''reseller'') as r
							join GEOs g on 1=1 
							join Learners l on l.Role = r.Role
							join CourseAlias c on 1=1
							right outer join (select distinct (concat(datepart(year,completionDate),'' Q'',ceiling(datepart(month,(completionDate))/(3.0)))) as [quarter]
							from Activities a 
							join Courses c on c.id = a.course_id
							join CourseAlias ca on ca.id = c.alias_id
							where ca.domain_id = 12 and completionDate is not null) as q on 1=1
							where c.domain_id = 12) as t2 on (t1.GEO = t2.GEO and t1.Role = T2.Role and t1.CourseNameAlias = t2.CourseNameAlias and t1.quarter = t2.quarter)							
                        ) as SourceData PIVOT(COUNT([Name]) FOR [CourseNameAlias] IN('+@courses+')) as PT
                        ORDER BY [GEO],  [role], [quarter] ;'
                        --PRINT @query + @query2;
                        EXECUTE (@query + @query2);";
            using (var cmd = new SqlCommand(query, dbConn))
            {
                cmd.Parameters.Add("@VARFilter", SqlDbType.VarChar).Value = varFilter;
                cmd.Parameters.Add("@ProfileFilter", SqlDbType.VarChar).Value = profileFilter;
                cmd.Parameters.Add("@GeoFilter", SqlDbType.VarChar).Value = geoFilter;
                cmd.Parameters.Add("@DomainFilter", SqlDbType.VarChar).Value = domainFilter;
                cmd.Parameters.Add("@RoleFilter", SqlDbType.VarChar).Value = roleFilter;
                cmd.Parameters.Add("@DomainType", SqlDbType.Int).Value = domainType;
                return TableQuery(cmd, query);
            }
        }

        internal DataTable GetSIMULIALegacyTransposedTable(string varAlias = "%")
        {
            string query = @"declare @courses nvarchar(max), @query1 nvarchar(max), @query2 nvarchar(max);
                                                        set @courses = STUFF(
                                (SELECT DISTINCT ','+QUOTENAME([CourseNameAlias])
                                FROM [CourseAlias] c WHERE c.domain_id = 12 FOR XML PATH('')), 1, 1, '');
                            set @query1 = 'select * from
                                (SELECT Learners.Name, Learners.email, Learners.Role, null [FTE Value], VARAlias.VAR_Alias as [VAR Alias], 
	                            VARs.Name as Company, Learners.Country, GEOs.GEO, null [TotalAchievement], CourseAlias.CourseNameAlias as [Course] --Courses.Name as [Course]
	                            , CASE WHEN MAX(Activities.status) = ''Unenrolled'' THEN null  WHEN MAX(completionDate) is null THEN ''In Progress'' ELSE convert(varchar,MAX(completionDate), 101) END as [Completion Date]
	                            --, Activities.completionDate as ''Completion Date'' 
                                FROM Activities join Learners on Learners.ID = Activities.learner_id
                                join Courses on Courses.ID = Activities.course_id '
                            set @query2 = 'join GEOs on GEOs.ID = Learners.geo_id
                                join VARs on VARs.ID = Learners.var_id
	                            join VARAlias on VARAlias.id = vars.var_alias_id
	                            join CourseAlias on CourseAlias.ID = Courses.alias_id                                
	                            where CourseAlias.domain_id = 12 --simulia legacy
                                AND Learners.userState = ''ACTIVE''
                                AND (Activities.status = ''Completed'' OR Activities.status = ''In Progress'' OR Activities.status = ''Unenrolled'')
                                AND VAR_Alias like '''+@varAlias+'''
	                            Group by Learners.ID, Learners.Name, Learners.email, Learners.[Role], VARAlias.VAR_Alias, VARs.Name, 
		                            Learners.Country, GEOs.GEO, CourseAlias.CourseNameAlias --Courses.Name
                                ) as SourceData PIVOT(MAX([Completion Date]) FOR [Course] IN('+@courses+')) as PT
                                order by GEO, Company, Name'
								--print @query1 + @query2;
	                            execute (@query1 + @query2);";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            cmd.Parameters.AddWithValue("@varAlias", varAlias);
            return TableQuery(cmd, query);
        }

        /// <summary>
        /// Reset all FTE values back to 0, run before updating FTE values.  Japan is managed differently.
        /// </summary>
        /// <param name="ignoreJapan">Typically true.  False will also reset Japan.</param>
        internal void ResetLearnerFTEValues(bool ignoreJapan = true)
        {
            string query = "update Learners set fte_value = 0 ";
            if (ignoreJapan)
            {
                query = query + " where geo_id != 7";
            }
            SqlCommand cmd = new SqlCommand(query, dbConn);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Used for Virtual Tester data import - technical certifications
        /// </summary>
        /// <param name="serverFilePath">File path to the tab delimited text file to import</param>
        /// <param name="tempTableName">#tablename creates a temp table</param>
        /// <returns></returns>
        internal bool importVTToTempTable(string serverFilePath, string tempTableName)
        {
            string query = @"CREATE TABLE #TABLENAME# (                            
                            	[Firstname] nvarchar(max) NULL,
	                            [Lastname] nvarchar(max) NULL,
                                [Name] nvarchar(max) NULL,
                                email NVARCHAR(100) NOT NULL,
                                [Adobe Captivate id] nvarchar(20) NULL,
                                [Certificate name] nvarchar(20) NULL,
                                [Certificate full name] nvarchar(100) NULL,
                                [Certification Date] datetime NULL,
                                [Status] nvarchar(20) NULL,
                                [Employee Status] int NULL,
	                            [Company Class] nvarchar(10) NULL,
                                Company NVARCHAR(100) NOT NULL,
                                [User GEO] NVARCHAR(25) NOT NULL,
                                Country NVARCHAR(50),
                                UNIQUE (email)
                            );";
            query = query.Replace("#TABLENAME#", tempTableName);

            SqlCommand cmd = new SqlCommand(query, dbConn);
            var rows = cmd.ExecuteNonQuery();

            query = "BULK INSERT " + tempTableName + " FROM '" + serverFilePath + "' WITH (CODEPAGE = '65001', FIELDTERMINATOR = '\t', ROWTERMINATOR = '0x0a', FirstRow=2, FORMAT='CSV');";
            cmd = new SqlCommand(query, dbConn);
            rows = cmd.ExecuteNonQuery();
            if (rows > 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// To import FTE data, a table must be created first
        /// </summary>
        /// <param name="serverFilePath">File path to the source csv file</param>
        /// <param name="tempTableName">#tablename creates a temp table</param>
        /// <returns>True if successful</returns>
        internal bool importFTECSVToTempTable(string serverFilePath, string tempTableName)
        {
            DropTableIfExists(tempTableName);
            //table must exist first
            string query = @"CREATE TABLE #TABLENAME# (                                
                                [Employment Status] nvarchar(20) NULL,
                                [Individual Quota?] nvarchar(20) NULL,
                                [FTE Role] NVARCHAR(30) NOT NULL,
                                [Secondary FTE Role] nvarchar(50) NULL,
                                [FTE Status] nvarchar(20) NULL,
                                [FTE Value] FLOAT NOT NULL,
                                [Login Name] nvarchar(50) NULL,
                                [First Name] NVARCHAR(MAX) NOT NULL,
                                [Last Name] NVARCHAR(MAX) NOT NULL,
                                [Contact Id] nvarchar(50) NULL,
                                [Account] NVARCHAR(100) NOT NULL,
                                [Account Status] nvarchar(50) NULL,
                                [Account Type] nvarchar(50) NULL,
                                [Organization] NVARCHAR(100) NOT NULL,
                                [Email] NVARCHAR(100) NOT NULL,
                                [Country] NVARCHAR(50),
                                [GEO] NVARCHAR(25) NOT NULL,
                                UNIQUE (Email) --will other code take care of this?  Will potentially cause errors, but we do not want duplicate emails
                                );";
            query = query.Replace("#TABLENAME#", tempTableName);

            SqlCommand cmd = new SqlCommand(query, dbConn);
            var rows = cmd.ExecuteNonQuery();

            query = "BULK INSERT " + tempTableName + " FROM '" + serverFilePath + "' WITH (CODEPAGE = '65001', FIELDTERMINATOR = '\t', FIELDQUOTE= '\u0022', ROWTERMINATOR = '0x0a', FirstRow=2, FORMAT='CSV');";
            cmd = new SqlCommand(query, dbConn);
            rows = cmd.ExecuteNonQuery();
            if (rows > 0)
            {
                CleanLeadingTrailingSpacesFromColumn(tempTableName, "GEO");
                return true;
            }
            return false;
        }

        internal void setFTEValsFromTempTable(string tableName)
        {
            string query = @"UPDATE Learners
                            SET Learners.fte_value = FTETemp.[FTE Value]
                            FROM Learners JOIN FTETemp on FTETemp.email = Learners.email";
            query = query.Replace("FTETemp", tableName);
            SimpleQuery(query);
        }

        internal void addLearnersFromTempTable(string tableName, ref Report r)
        {
            //get learners list for report
            string query = @"SELECT (FTETemp.[First Name] + ' ' + FTETemp.[Last Name]) as Name, FTETemp.email, FTETemp.[FTE Role] as Role, 
                            FTETemp.Country, 'reseller' as profile, 'ACTIVE' as userState, VARs.ID as var_id, 
                            GEOs.ID as geo_id, 0 as region_id, Countries.ID as country_id, FTETemp.[FTE Value]
                            ,VARs.Name as Company
                            FROM Learners RIGHT OUTER JOIN FTETemp on FTETemp.email = Learners.email 
                            JOIN GEOs on GEOs.GEO = FTETemp.GEO
                            JOIN Countries on Countries.Country = FTETemp.Country
                            LEFT OUTER JOIN VARs on VARs.Name = FTETemp.Account
                            WHERE Learners.ID is null AND FTETemp.[Account Status] = 'Active' 
                            AND FTETemp.email not like '%cancel%' 
                            AND FTETemp.Account not like '%terminate%';";
            query = query.Replace("FTETemp", tableName);
            SqlCommand cmd = new SqlCommand(query, dbConn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Learner l = new Learner(this);
                l.Name = reader["Name"].ToString();
                l.email = reader["email"].ToString();
                l.company = new Company(reader["Company"].ToString(), this);
                r.AddLine("LEARNER ADDED:\t" + l.Name + "\t" + l.email);
                r.AddLearner(l);
            }
            reader.Close();


            query = @"INSERT INTO Learners 
                            SELECT (FTETemp.[First Name] + ' ' + FTETemp.[Last Name]) as Name, FTETemp.email, FTETemp.[FTE Role] as Role, 
                            FTETemp.Country, 'reseller' as profile, 'ACTIVE' as userState, VARs.ID as var_id, 
                            GEOs.ID as geo_id, 0 as region_id, Countries.ID as country_id, FTETemp.[FTE Value] as fte_value, null as AdobeID
                            FROM Learners RIGHT OUTER JOIN FTETemp on FTETemp.email = Learners.email 
                            JOIN GEOs on GEOs.GEO = FTETemp.GEO
                            JOIN Countries on Countries.Country = FTETemp.Country
                            LEFT OUTER JOIN VARs on VARs.Name = FTETemp.Account
                            WHERE Learners.ID is null AND FTETemp.[Account Status] = 'Active' 
                            AND FTETemp.email not like '%cancel%' 
                            AND FTETemp.Account not like '%terminate%';";
            query = query.Replace("FTETemp", tableName);
            SimpleQuery(query);
        }

        internal void addLearnersFromVTTempTable(string tableName, ref Report r)
        {
            //get learners list for report
            string query = @"SELECT DISTINCT
	                        (tt.Firstname + ' ' + tt.Lastname) as [Name], tt.email, Role, tt.Country, profile, userState = 'Active', tt.var_id, geo_id, region_id, country_id, fte_value
	                        from Learners l
	                        right outer join 
	                        (select t.Firstname, t.Lastname, t.email, t.Country, v.id as var_id from TEMPTABLE t join vars v on v.Name = t.Company) as tt on tt.email = l.email
	                        where l.id is null;";
            query = query.Replace("TEMPTABLE", tableName);
            SqlCommand cmd = new SqlCommand(query, dbConn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Learner l = new Learner(this);
                l.Name = reader["Name"].ToString();
                l.email = reader["email"].ToString();
                l.company = new Company(this);
                l.company.GetByID(Convert.ToInt32(reader["var_id"]));
                //l.company = new Company(reader["Company"].ToString(), dbConn);
                r.AddLine("LEARNER ADDED:\t" + l.Name + "\t" + l.email);
                r.AddLearner(l);
            }
            reader.Close();

            query = @"INSERT INTO Learners
	                    SELECT DISTINCT
	                    (tt.Firstname + ' ' + tt.Lastname) as [Name], tt.email, Role, tt.Country, profile, userState = 'Active', tt.var_id, geo_id, region_id, country_id, fte_value
	                    from Learners l
	                    right outer join 
	                    (select t.Firstname, t.Lastname, t.email, t.Country, v.id as var_id from TEMPTABLE t join vars v on v.Name = t.Company) as tt on tt.email = l.email
	                    where l.id is null;";
            query = query.Replace("TEMPTABLE", tableName);
            SimpleQuery(query);
        }

        /// <summary>
        /// create missing course aliases from the temp table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="r"></param>
        internal void addCourseAliasFromVTTempTable(string tableName, ref Report r)
        {
            string query = @"select distinct tt.[Certificate name] as [CourseNameAlias]
	                        , [Target] = 0, domain_id = null, has_target = 0, alignment_points = 0, domain_cert = null
	                        from CourseAlias ca
	                        right outer join TEMPTABLE tt on tt.[Certificate name] = ca.CourseNameAlias
	                        where ca.id is null";
            query = query.Replace("TEMPTABLE", tableName);
            SqlCommand cmd = new SqlCommand(query, dbConn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                CourseAlias ca = new CourseAlias(this);
                ca.CourseNameAlias = reader["CourseNameAlias"].ToString();
                ca.Target = (float)Convert.ToDouble(reader["Target"]);
                ca.has_target = Convert.ToInt32(reader["has_target"]);
                ca.alignment_points = Convert.ToInt32(reader["alignment_points"]);
                r.AddLine("COURSE ALIAS ADDED:\t" + ca.CourseNameAlias);
                //don't store the aliases in the report
            }
            reader.Close();

            query = @"INSERT INTO CourseAlias
	                    select distinct tt.[Certificate name] as [CourseNameAlias]
	                    , [Target] = 0, domain_id = null, has_target = 0, alignment_points = 0, domain_cert = null
	                    from CourseAlias ca
	                    right outer join TEMPTABLE tt on tt.[Certificate name] = ca.CourseNameAlias
	                    where ca.id is null";
            SimpleQuery(query.Replace("TEMPTABLE", tableName));
        }

        /// <summary>
        /// create missing courses in the DB from temp table data
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="r"></param>
        internal void addCoursesFromVTTempTable(string tableName, ref Report r)
        {
            //get course list for report
            string query = @"select distinct tt.[Certificate full name] as [Name], [Type] = 'VT Certificate' 
                            from courses c
                            right outer join TEMPTABLE tt on tt.[Certificate full name] = c.Name
                            where c.id is null";
            query = query.Replace("TEMPTABLE", tableName);
            SqlCommand cmd = new SqlCommand(query, dbConn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Course c = new Course(this);
                c.Name = reader["Name"].ToString();
                c.Type = reader["Type"].ToString();
                r.AddLine("COURSE ADDED:\t" + c.Name + "\t" + c.Type);
                r.AddCourse(c);
            }
            reader.Close();


            query = @"INSERT INTO Courses
	            SELECT distinct mt.[Certificate full name] as [Name], alias_id = null, [Type] = 'VT Certificate' 
	            from courses c
	            right outer join TEMPTABLE mt on mt.[Certificate full name] = c.Name
	            where c.id is null;";
            query = query.Replace("TEMPTABLE", tableName);
            SimpleQuery(query);
        }

        /// <summary>
        /// create missing activities from the temp table
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="r"></param>
        internal void addActivitiesFromVTTempTable(string tableName, ref Report r)
        {
            //NO NEED to get missing activities list for reporting            
            string query = @"INSERT INTO Activities
	                        select distinct enrollDate, startDate, tt.[Certification Date] as completionDate, tt.status, tt.learner_id, tt.course_id, progress, quiz_score
	                        from Activities a
	                        right outer join 
	                        (select t.[Certification Date], [status] = 'Completed', l.id as [learner_id], c.id as [course_id]
	                        from TEMPTABLE t 
	                        join learners l on l.email = t.email
	                        join courses c on t.[Certificate full name] = c.Name) as tt on tt.course_id = a.course_id
	                        where a.id is null";
            query = query.Replace("TEMPTABLE", tableName);
            var rows = SimpleQuery(query);
            r.AddLine("ADDED ACTIVITIES:\t" + rows.ToString());

        }

        internal void addVARParentsFromTempTable(string tableName)
        {
            string query = @"INSERT INTO VARParents
                            SELECT DISTINCT Organization as VAR_Parent, null as FTESales, null as FTETech FROM VARParents RIGHT OUTER JOIN TEMPTABLE tt on tt.Organization = VARParents.VAR_Parent 
                            WHERE VARParents.ID is null AND tt.[Account Status] = 'Active' AND Organization not like '%terminated%';";
            query = query.Replace("TEMPTABLE", tableName);
            SimpleQuery(query);
        }

        internal void addVARsFromVTTempTable(string tableName, ref Report r)
        {
            string query = @"SELECT DISTINCT
	                            tt.Company as [Name], var_parent_id, var_alias_id, status = 1, companyID
	                            FROM VARs v
	                            right outer join TEMPTABLE tt on tt.Company = v.Name
	                            where v.id is null;";
            query = query.Replace("TEMPTABLE", tableName);
            SqlCommand cmd = new SqlCommand(query, dbConn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Company c = new Company(this);
                c.Name = reader["Name"].ToString();
                if (!reader.IsDBNull(reader.GetOrdinal("var_parent_id")))
                {
                    c.ParentNameId = (int)reader["var_parent_id"];
                }
                r.AddLine("NEW COMPANY:\t" + c.Name);
                r.AddCompany(c);
            }
            reader.Close();


            query = @"INSERT INTO VARs
	                    SELECT DISTINCT
	                    tt.Company as [Name], var_parent_id, var_alias_id, status = 1, companyID
	                    FROM VARs v
	                    right outer join TEMPTABLE tt on tt.Company = v.Name
	                    where v.id is null";
            query = query.Replace("TEMPTABLE", tableName);
            var rows = SimpleQuery(query);
            r.AddLine("ADDED VARS:\t" + rows.ToString());
        }

        internal void addVARsFromTempTable(string tableName, ref Report r)
        {
            //get VAR list for report
            string query = @"SELECT DISTINCT Account as Name, VARParents.ID as var_parent_id, 1 as status
                            FROM VARs RIGHT OUTER JOIN TEMPTABLE tt ON tt.Account = VARs.Name
                            LEFT OUTER JOIN VARParents on VARParents.VAR_Parent = tt.Organization
                            left outer JOIN Learners on Learners.email = tt.email
                            WHERE VARs.ID is null AND Account is not null AND Learners.ID is null 
                            AND tt.[Account Status] = 'Active' 
                            AND tt.email not like '%cancel%';";
            query = query.Replace("TEMPTABLE", tableName);
            SqlCommand cmd = new SqlCommand(query, dbConn);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Company c = new Company(this);
                c.Name = reader["Name"].ToString();
                if (!reader.IsDBNull(reader.GetOrdinal("var_parent_id")))
                {
                    c.ParentNameId = (int)reader["var_parent_id"];
                }
                r.AddLine("NEW COMPANY:\t" + c.Name);
                r.AddCompany(c);
            }
            reader.Close();

            query = @"INSERT INTO VARs (Name, var_parent_id, status)
                            SELECT DISTINCT Account as Name, VARParents.ID as var_parent_id, 1 as status
                            FROM VARs RIGHT OUTER JOIN TEMPTABLE tt ON tt.Account = VARs.Name
                            LEFT OUTER JOIN VARParents on VARParents.VAR_Parent = tt.Organization
                            left outer JOIN Learners on Learners.email = tt.email
                            WHERE VARs.ID is null AND Account is not null AND Learners.ID is null 
                            AND tt.[Account Status] = 'Active' 
                            AND tt.email not like '%cancel%';";
            query = query.Replace("TEMPTABLE", tableName);
            SimpleQuery(query);
        }



        #endregion

        #region Shared Queries

        internal DataTable GetCourseAliasList()
        {
            string query = "select ID, CourseNameAlias from CourseAlias where domain_cert = 0 ORDER BY CourseNameAlias";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            return TableQuery(cmd, query);

        }

        private DataTable GetGEOList()
        {
            string query = "select ID, GEO from GEOs ORDER BY GEO";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            return TableQuery(cmd, query);
        }

        public int GetCountryID(string country)
        {
            string query = "select ID from Countries WHERE Country = @country;";
            using (var cmd = new SqlCommand(query, dbConn))
            {
                cmd.Parameters.AddWithValue("@country", country);
                int count;               
                var r = cmd.ExecuteScalar();
                count = Convert.ToInt32(r);
                return count;
            }
            
        }

        //new version using FTE views
        public DataTable GetFTEVals6(string varFilter = "%", string geoFilter = "%",
            VARGrouping style = VARGrouping.byVARAlias)
        {
            DataTable table = new DataTable();
            string query;
            query = (style == VARGrouping.byVARAlias) ?
                @"SELECT * FROM FTEValuesAlias WHERE VAR like @VARFilter and GEO like @GEOFilter ORDER BY GEO, VAR" :
                @"SELECT * FROM FTEValuesParent WHERE VAR like @VARFilter and GEO like @GEOFilter ORDER BY GEO, VAR";
            using (var cmd = new SqlCommand(query, dbConn))
            {
                cmd.Parameters.Add("@VARFilter", SqlDbType.VarChar).Value = varFilter;
                cmd.Parameters.Add("@GEOFilter", SqlDbType.VarChar).Value = geoFilter;
                return TableQuery(cmd, query);
            }
        }

        /// <summary>
        /// Return the number of active learners by role
        /// </summary>
        /// <param name="roleFilter">% for all, [si]% for all sales</param>
        /// <param name="varFilter"></param>
        /// <returns></returns>
        public double GetRoleCount(string roleFilter = "%", string varFilter = "%")
        {
            double count = 0;
            string query = @"select count(distinct email) as Count from Learners 
                            join VARs on VARs.ID = Learners.var_id
                            join VARAlias on VARAlias.ID = VARs.var_alias_id
                            where userState = 'ACTIVE' and VARAlias.VAR_Alias like @varFilter AND Learners.Role like @roleFilter;";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            cmd.Parameters.AddWithValue("@varFilter", varFilter);
            cmd.Parameters.AddWithValue("@roleFilter", roleFilter);
            var res = cmd.ExecuteScalar();
            count = Convert.ToDouble(res);
            return count;
        }

        #endregion

        #region Data Cleaning
        /// <summary>
        /// Delete all activities where the user is unenrolled
        /// </summary>
        /// <returns>The number of deleted activities</returns>
        public int deleteUnenrolledActivities()
        {
            string query = "dbo.spDeleteUnenrolled";
            SqlCommand cmd = new SqlCommand(query, dbConn);
            cmd.CommandType = CommandType.StoredProcedure;
            int res = cmd.ExecuteNonQuery();
            return res;
        }
        internal void DropTableIfExists(string tempTableName)
        {
            string query = @"DROP TABLE IF EXISTS #TABLENAME#;";
            query = query.Replace("#TABLENAME#", tempTableName);
            SimpleQuery(query);
        }

        internal void CleanLeadingTrailingSpacesFromColumn(string table, string column)
        {
            string query = @"Update TEMPTABLE set GEO = LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(REPLACE(COLUMN, CHAR(10), CHAR(32)),CHAR(13), CHAR(32)),CHAR(160), CHAR(32)),CHAR(9),CHAR(32))))";
            query = query.Replace("TEMPTABLE", table);
            query = query.Replace("COLUMN", column);
            SimpleQuery(query);
        }


        #endregion

        #region Nikhil Queries

        public List<string> getAdminUsers()
        {
            string usersquery = "SELECT Trigram FROM dbo.Users WHERE [Admin] = 0";
            List<string> users = new List<string>();
            SqlCommand command = new SqlCommand(@usersquery, dbConn);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var trigrams = reader.GetString(0);

                    //hide menus from non-admins                                                                           
                    users.Add(trigrams);
                }
            }
            return users;
        }

        
        #endregion

    }
}


