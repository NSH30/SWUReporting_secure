using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace SWUReporting
{
    public class Importer
    {
        #region fields
        private DataTable importedLearners;
        private DataTable importedCourses;
        #endregion

        #region Properties
        public DB DB { get; set; }
        public DataTable ImportedLearners
        {
            get { return importedLearners; }
        }
        public DataTable ImportedCourses
        {
            get { return importedCourses; }
        }
        #endregion
        #region Constructors
        public Importer (DB db)
        {
            DB = db;
        }
        #endregion

        #region Import Learner Transcripts

        /// <summary>
        /// Import Learner Transcripts from CSV saved on the server
        /// </summary>
        /// <param name="csvPath">full path to CSV file on the server</param>
        /// <param name="db">connected instance of DB class</param>
        /// <returns></returns>
        public static Report ImportCSV(string csvPath, DB db)
        {
            //Read the contents of CSV file.  
            string csvData = File.ReadAllText(csvPath);

            //generate unique lists
            List<Company> companies = new List<Company>();  //this needs user intervention - mapping to alias and parent
            List<Learner> learners = new List<Learner>();
            List<Course> courses = new List<Course>();
            //weekly will be csv (comma separated)
            List<Activity> activities = parseCSV(csvData, out companies, out learners, out courses, db, delimiter: ReportIO.sep); // "\t");

            //run the database imports            
            var res = batchImport(activities, companies, learners, courses, db);
            return res;
        }

        /// <summary>
        /// Batch import all records from lists, then do database cleanup
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="companies"></param>
        /// <param name="learners"></param>
        /// <param name="courses"></param>
        /// <returns></returns>
        protected static Report batchImport(List<Activity> activities, List<Company> companies,
            List<Learner> learners, List<Course> courses, DB db)
        {
            int companyCount = 0;
            int learnerCount = 0;
            Report r = new Report();
            r.CountOnly = false;

            //company
            foreach (var c in companies)
            {
                if (c.Insert())
                {
                    companyCount++;
                    r.AddLine("NEW COMPANY: " + c.Name);
                    r.AddCompany(c);
                }
            }
            //learner
            foreach (var l in learners)
            {
                if (l.Insert())
                {
                    learnerCount++;
                    r.AddLine("NEW LEARNER: " + l.Name + " " + l.email);
                    r.AddLearner(l);
                }
            }
            //course
            foreach (var cs in courses)
            {
                if (cs.Insert())
                {
                    r.AddLine("NEW COURSE: " + cs.Name);
                    r.AddCourse(cs);
                }
            }
            //activity
            //TODO: Creating duplicate activities July 5, 2022
            foreach (var a in activities)
            {
                //a.Insert();
                a.Insert2();
            }
            //cleanup unenrolled activities
            try
            {
                db.deleteUnenrolledActivities();
            }
            catch (Exception ex)
            {
                //ignore the error                
                r.AddLine("Failed to remove unenrollments: " + ex.Message);
            }
            try
            {
                db.CleanOrphanVARs();
            }
            catch (Exception ex2)
            {
                //ignore the error                
                r.AddLine("Failed to clean VARs: " + ex2.Message);
            }

            return r;
        }

        #endregion
        #region Virtual Tester Importer

        public void ImportVT(string filepath)
        {
            //ImportedLearners table is set during the import process
            //These are learners who didn't match by AdobeID, email or a single name match
            //They may include duplicate learners and learners without Prime accounts.
            string tableName = "var_employees";

            try
            {
                ImportVTFileToTable(filepath, tableName);
            }
            catch (Exception ex)
            {

                throw;
            }

            try
            {
                ImportActivitiesFromVTTable(tableName);
            }
            catch (Exception ex)
            {
                throw;
            }
            
        }



        /// <summary>
        /// Build a table from virtual tester csv data
        /// </summary>
        /// <param name="filepath">Full path to the CSV file</param>
        /// <param name="tableName">Name of the table to create</param>
        private void ImportVTFileToTable(string filepath, string tableName)
        {
            string query = @"DROP TABLE IF EXISTS #TABLENAME#;
                            CREATE TABLE #TABLENAME# 
	                            ([Firstname] nvarchar(max) NULL,
	                                [Lastname] nvarchar(max) NULL,
                                    [Name] nvarchar(max) NULL,
                                    [email] NVARCHAR(100) NOT NULL,
                                    [Adobe Captivate id] nvarchar(20) NULL,
                                    [Certificate name] nvarchar(50) NULL,
                                    [Certificate full name] nvarchar(max) NULL,
                                    [Completion Date (UTC TimeZone)] date NULL,
                                    [Status] int NULL,
                                    [Employee Status] nvarchar(50) NULL,
                                    [Company] NVARCHAR(max) NULL,
                                    [User GEO] NVARCHAR(50) NULL,
                                    [Country] NVARCHAR(50) NULL);";
            query = query.Replace("#TABLENAME#", tableName);            
            DB.SimpleQuery(query);

            query = "BULK INSERT " + tableName + " FROM '" + filepath + "' WITH (CODEPAGE = '65001', FirstRow=2, FORMAT = 'CSV');";

            DB.SimpleQuery(query);
        }

        /// <summary>
        /// Import activities from the Virtual Tester table.
        /// This assumes the table exists and only imports for users that already exist and AdobeID mapping is done
        /// </summary>
        /// <param name="tableName">string name of the table containing vt data</param>
        private void ImportActivitiesFromVTTable(string tableName)
        {

            //first, clean the table to prepare for import
            CleanVTTable(tableName);

            //store the learners imported that may have duplicates or no match in Prime
            importedLearners = ImportRemainingLearners(tableName);  //save for later reporting
            SetLearnerIDByEmail(tableName);  //run one more time to set the remaining learnerIDs to the newly added IDs.

            //import courses
            ImportCourses(tableName);

            //import actitivities
            ImportActivities(tableName);
            
        }

        private void ImportCourses(string tableName)
        {
            //save the list of courses to import for later reporting
            string query = @"select Name,null as alias_id, Type from (SELECT Distinct ve.[Certificate name] as Name, 'VirtualTester' as Type
                            FROM var_employees as ve
                            EXCEPT SELECT Name, Type FROM Courses) as t1";
            importedCourses = DB.TableQuery(new SqlCommand("", DB.dbConn), query);

            //import all courses...
            query = @"INSERT INTO Courses
                            select Name,null as alias_id, Type from (
                            SELECT Distinct ve.[Certificate name] as Name, 'VirtualTester' as Type
                            FROM var_employees as ve
                            EXCEPT SELECT Name,  Type FROM Courses) as t1";
            query = query.Replace("#TABLENAME#", tableName);
            DB.SimpleQuery(query);
        }

        #region Virtual Test Import Helpers

        private void ImportActivities(string tableName)
        {
            //import all activities...
            string query = @"INSERT INTO Activities
                            SELECT ve.[Completion Date (UTC TimeZone)] as enrollDate, ve.[Completion Date (UTC TimeZone)] as startDate, ve.[Completion Date (UTC TimeZone)] as completionDate,
                            'Completed' as status, l.ID as learner_id, c.ID as course_id, null as progress, null as quiz_score
                            FROM #TABLENAME# ve
                            join Learners l on l.ID = ve.learners_id
                            join Courses c on c.Name = ve.[Certificate name]
                            EXCEPT SELECT enrollDate, startDate, completionDate, status, learner_id, course_id, progress, quiz_score from Activities";
            query = query.Replace("#TABLENAME#", tableName);
            DB.SimpleQuery(query);
        }
        private void CleanVTTable(string tableName)
        {
            //delete extra unwanted content
            DeleteExtraRows(tableName);
            AddLearnersIDColumn(tableName);
            //set learner_id mapping
            SetLearnerIDByAdobeID(tableName);
            SetLearnerIDByEmail(tableName);
            SetLearnerIDByName(tableName);
            UpdateCountries(tableName);
        }

        private void AddLearnersIDColumn(string tableName)
        {
            string query = @"ALTER TABLE " +  tableName + " ADD learners_id int NULL;";
            DB.SimpleQuery(query);
        }

        private void UpdateCountries(string tableName)
        {
            string query = @"UPDATE " + tableName + " SET Country = 'USA' WHERE Country = 'United States';";
            query += @" UPDATE " + tableName + " SET Country = 'South Korea' WHERE Country = 'Korea, Republic of';";
            query += @" UPDATE " + tableName + " SET Country = 'Vietnam' WHERE Country = 'Viet Nam';";
            
            DB.SimpleQuery(query);
        }

        private void DeleteExtraRows(string tableName)
        {
            string query;
            /* data cleanup after import */
            query = @"delete from #TABLENAME# where[Certificate name] like 'SAMPLE%'--sample exams -no interest
                    delete from #TABLENAME# where [Certificate name] like 'SALES%'--sales exams already in SWUReporting
                    delete from #TABLENAME# where [Certificate name] like 'old%'--old exams
                    delete from #TABLENAME# where[Certificate full name] like '%segment%'--CSWP segments, not CSWP
                    delete from #TABLENAME# where [Certificate name] like 'EDU%'--EDU exams (CSWA)
                    delete from #TABLENAME# where [Certificate name] like 'CSWX%'--CSWE pre - requisities
                    delete from #TABLENAME# where [Certificate name] like '%TSO%' or[Certificate name] like '%TSA%'--TSO and TSA already reported in SWUReporting
                    delete from #TABLENAME# where [Certificate name] = '^3DXX'--pre - requisite course material
                    delete from #TABLENAME# where  [Certificate name] like '%CAT%'  --CATIA 
                    delete from #TABLENAME# where  [Certificate name] like '%V6%'  
                    delete from #TABLENAME# where  [Certificate name] like '%V5%'
                    delete from #TABLENAME# where [Certificate name] like 'EVENT%'  --event attendance";
            query = query.Replace("#TABLENAME#", tableName);
            DB.SimpleQuery(query);
        }

        /// <summary>
        /// Set learners_id field in the specified table based on an AdobeID match.         
        /// </summary>
        /// <param name="tableName">Name of the VT imported table</param>
        private void SetLearnerIDByAdobeID(string tableName)
        {
            string query = @"update #TABLENAME#
                              set #TABLENAME#.learners_id = Learners.ID
                              from #TABLENAME#
                              join Learners on Learners.AdobeID = #TABLENAME#.[Adobe Captivate id]
                              where #TABLENAME#.[Adobe Captivate id] != 0";
            query = query.Replace("#TABLENAME#", tableName);
            DB.SimpleQuery(query);
        }
        /// <summary>
        /// Set learners_id field in the specified table based on an email match.         
        /// </summary>
        /// <param name="tableName">Name of the VT imported table</param>
        private void SetLearnerIDByEmail(string tableName)
        {
            string query = @"update #TABLENAME#
                              set #TABLENAME#.learners_id = Learners.ID
                              from #TABLENAME#
                              join Learners on Learners.email = #TABLENAME#.email
                              where #TABLENAME#.learners_id is null
                              and #TABLENAME#.email = Learners.email";
            query = query.Replace("#TABLENAME#", tableName);
            DB.SimpleQuery(query);
        }
        private void SetLearnerIDByName(string tableName)
        {
            /* count names and only return single results  */
            /* These are all Learner IDs where we can copy the AdobeID from #TABLENAME# to Learners
               and map learner IDs to the #TABLENAME# table */
            string query = @"update #TABLENAME# set learners_id = Learners.ID
                            --select*
                            from #TABLENAME# Join Learners on Learners.Name = #TABLENAME#.Name
                            where #TABLENAME#.learners_id is null and Learners.ID in (
                            select t1.id
                            from(select distinct ve.email, l.email as lemail, l.ID,
                            ve.Name, ve.[Adobe Captivate id], ve.Company, l.AdobeID
                            from #TABLENAME# ve join learners l on l.name = ve.name
                            where userState = 'ACTIVE' and l.AdobeID is null or AdobeID = 0 or ve.[Adobe Captivate id] = 0) as t1
                            group by t1.name, t1.ID
                            having count(t1.Name) = 1);";
            query = query.Replace("#TABLENAME#", tableName);
            DB.SimpleQuery(query);
        }

        private DataTable ImportRemainingLearners(string tableName)
        {
            string query = @"select distinct ve.Name, ve.email, 'VT tech-sales' as Role, ve.Country, 'reseller' as profile, 'ACTIVE' as userState, 
	                        vars.ID as var_id, GEOs.ID as geo_id, Countries.region_id as region_id, Countries.ID as country_id,  
	                        0 as fte_value, ve.[Adobe Captivate id] as AdobeID, null as [Contact ID]
	                        from #TABLENAME# ve 
	                        left outer join vars on vars.Name = ve.Company
	                        left outer join GEOs on GEOs.GEO = ve.[User GEO]
	                        left outer join Countries on Countries.Country = ve.Country
	                        where ve.learners_id is null";
            query = query.Replace("#TABLENAME#", tableName);
            DataTable dt = DB.TableQuery(new SqlCommand(query, DB.dbConn), query);
            if (dt.Rows.Count > 0)
            { 
                string insertQuery = @"INSERT INTO Learners SELECT * FROM(" + query + ") as t1;";
                DB.SimpleQuery(insertQuery);
            }
            return dt;
        }

        #endregion
        #endregion

        #region Data Tools
        //activities are the primary returned List, companies, learners and courses are also returned
        //companies, learners and courses are empty coming in
        /// <summary>
        /// Transfer CSV data into structures - no filtering for existing activities
        /// </summary>
        /// <param name="csvData">CSV data read to a string</param>
        /// <param name="companies">Output List of Company</param>
        /// <param name="learners">Output List of Learner</param>
        /// <param name="courses">Output List of Course</param>
        /// <param name="delimiter">CSV delimiter character</param>
        /// <returns>List of Activity</returns>
        protected static List<Activity> parseCSV(string csvData, out List<Company> companies,
            out List<Learner> learners, out List<Course> courses, DB db, string delimiter = ",")
        {
            TextFieldParser parser = new TextFieldParser(new StringReader(csvData));

            parser.HasFieldsEnclosedInQuotes = true;
            parser.SetDelimiters(delimiter);

            List<Activity> activities = new List<Activity>();
            companies = new List<Company>();
            learners = new List<Learner>();
            courses = new List<Course>();
            string[] data;
            string[] headers = null;
            bool firstLine = true;
            while (!parser.EndOfData)
            {
                data = parser.ReadFields();
                if (firstLine)
                {
                    //headers - ignore for now
                    headers = data;
                }
                else
                {

                    Learner l = new Learner(db);
                    Activity a = new Activity(db);
                    Company c = new Company(db);
                    Course cs = new Course(db);

                    //company data
                    c.Name = data[TableTools.getHeaderIndex(headers, "Company DS")];  //field changed October 18 2021 to "Company DS"
                    var t = companies.Find(x => (x.Name == c.Name));
                    if (t == null)
                    {
                        companies.Add(c);
                    }
                    else
                    {
                        c = t;
                    }
                    //course data                    
                    cs.Type = data[TableTools.getHeaderIndex(headers, "Type")];
                    if (data[TableTools.getHeaderIndex(headers, "Course")] == "")
                    {
                        switch (cs.Type)
                        {
                            case "Course":
                                cs.Name = data[TableTools.getHeaderIndex(headers, "Course")]; //data[6];
                                break;
                            case "Learning Path":
                                cs.Name = data[TableTools.getHeaderIndex(headers, "LP/Certification/Course")]; // data[4];
                                break;
                            case "Certification":
                                cs.Name = data[TableTools.getHeaderIndex(headers, "LP/Certification/Course")]; // data[4];
                                break;
                            default:
                                //shouldn't happen
                                cs.Name = data[TableTools.getHeaderIndex(headers, "Course")]; //data[6];
                                break;
                        }
                    }
                    else
                    {
                        cs.Name = data[TableTools.getHeaderIndex(headers, "Course")]; //data[6];
                    }
                    var csTemp = courses.Find(x => (x.Name == cs.Name && x.Type == cs.Type));
                    if (csTemp == null)
                    {
                        courses.Add(cs);
                    }
                    else
                    {
                        cs = csTemp;
                    }
                    //Learner data
                    l.Name = data[TableTools.getHeaderIndex(headers, "Name")]; //data[0];
                    l.email = data[TableTools.getHeaderIndex(headers, "email")]; //data[1];
                    l.company = c;
                    l.Region = data[TableTools.getHeaderIndex(headers, "Region")]; //data[32];
                    l.Country = data[TableTools.getHeaderIndex(headers, "Country")]; //data[34];
                    l.Role = data[TableTools.getHeaderIndex(headers, "Role")]; //data[35];
                    l.GEO = data[TableTools.getHeaderIndex(headers, "GEO")]; //data[33];
                    l.Profile = data[TableTools.getHeaderIndex(headers, "profile")]; //data[29];
                    l.userState = data[TableTools.getHeaderIndex(headers, "userState")]; //data[28];

                    var lTemp = learners.Find(x => (x.Name == l.Name && x.email == l.email && x.company == l.company));
                    if (lTemp == null)
                    {
                        learners.Add(l);
                    }
                    else
                    {
                        l = lTemp;
                    }
                    //activity data
                    a.course = cs;
                    a.learner = l;
                    a.enrollDate = Converters.getDateFromString(data[TableTools.getHeaderIndex(headers, "Enrollment Date (UTC TimeZone)")]); // DateTime.Parse(data[15]);
                    a.startDate = Converters.getDateFromString(data[TableTools.getHeaderIndex(headers, "Started Date (UTC TimeZone)")]); // DateTime.Parse(data[16]);
                    a.completionDate = Converters.getDateFromString(data[TableTools.getHeaderIndex(headers, "Completion Date (UTC TimeZone)")]); // DateTime.Parse(data[17]);
                    a.status = data[TableTools.getHeaderIndex(headers, "Status")]; //data[20];
                    //new fields for DELMIAWORKS team Sept. 23, 2021
                    a.progress = Converters.getDecimalFromString(data[TableTools.getHeaderIndex(headers, "Progress %")]);
                    a.quizScore = Converters.getDecimalFromString(data[TableTools.getHeaderIndex(headers, "Quiz_score")]);
                    a.SelectionCriteria = data[TableTools.getHeaderIndex(headers, "Selection Criteria")];
                    activities.Add(a);
                }
                firstLine = false;
            }
            parser.Close();

            //now have unique lists of learners, companies, courses and activities
            return activities;
        }


        #endregion
    }
}