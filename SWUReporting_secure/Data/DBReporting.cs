using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SWUReporting;

namespace ReportBuilder
{

    class DBReporting
    {
        #region Properties and Fields

        static string varFilter = "%";
        static string geoFilter = "%";
        static string roleFilter = "%";
        static string profileFilter = "%";
        public static bool keepSheet1 = false;
        static DataTable l_ftes;
        static string varAlias = "";
        public static DB db;
        

        //default FTE output
        //combine with a VARFilter for just one parent org.
        public static DB.VARGrouping style = DB.VARGrouping.byVARAlias;

        public string VARAlias { get { return varAlias; } }

        public string VARFilter {
            get { return varFilter; }
            set { varFilter = value; }
        }

        public string GEOFilter
        {
            get { return geoFilter; }
            set {geoFilter = value; }
        }

        public string RoleFilter
        {
            get { return roleFilter; }
            set { roleFilter = value; }
        }

        public string ProfileFilter
        {
            get { return profileFilter; }
            set { profileFilter = value; }
        }

        #endregion


        #region obsolete and unused functions
        public static void ImportCompanies(List<Company> companies, string conString)
        {
            string tName = db.CreateTempTable(DB.rbTables.VARnames);
            SqlCommand cmd =
                new SqlCommand(
                    "INSERT INTO [" + tName + "] (VARName) " +
                    " VALUES (@Name) " +
                    " WHERE NOT EXISTS (SELECT VARName FROM " + tName + " WHERE VARName = @Word)", db.dbConn);
            foreach (var company in companies)
            {
                cmd.Parameters.AddWithValue("@Name", company.Name);
                var rows = cmd.ExecuteNonQuery();
                if (rows == 1)
                {

                }
            }
        }
        //file must be tab delimited and UTF-8 format
        public Report ImportFTEs(string csvPath)
        {
            //Read the contents of CSV file.  
            string csvData = File.ReadAllText(csvPath, Encoding.UTF8);  //.GetEncoding("iso-8859-1"));
            //generate a list         
            List<Company> companies = new List<Company>();
            List<Learner> Learners = parseFTECSVtoLearners(csvData, out companies);
            db.ResetLearnerFTEValues(ignoreJapan: true);
            var res = BatchImportLearners(Learners, companies);
            return res;
        }

        //obsolete method
        private static void setQuarterlySheet(string geoFilter, string domainFilter,
            string profileFilter, string sheetName, string roleFilter = "%sales%")
        {
            dynamic sht = null;
            var dt = db.getQuarterlyProgressTable2(geoFilter: geoFilter,
                domainFilter: domainFilter, profileFilter: profileFilter, roleFilter: roleFilter);
            //add geo quarterly values to get cummulative totals            
            dt = setQuarterlyIncrease(dt);
            string[,] vals;
            vals = TableTools.GetArrayFromTable(dt);
            //sht = Excel.AddExcelSheet(sheetName, keepSheet1);
            sht.Range(sht.Cells(1, 1), sht.Cells(vals.GetUpperBound(0) + 1, vals.GetUpperBound(1) + 1)).Value = vals;
        }

        internal string CreateDraftSightReport(out bool status)
        {
            string path = "";
            try
            {
                DataTable t = db.GetDraftSightTransposedTable();  //new format including In Progress users
                string csvOutput = ReportIO.getCSVFromTable(t);

                string name = "DraftSight Certifications.csv";

                path = ReportIO.WriteToFileEncoded(csvOutput, name);
                status = true;
            }
            catch (Exception e)
            {
                status = false;
                return e.Message;
            }
            return path;
        }

        //obsolete method
        private static DataTable setSheetData(string domainFilter, int domainType, string sheetName)
        {
            //dynamic sht = null;
            var fRes = new DataTable();
            string[,] vals;
            fRes = getSheet(domainFilter: domainFilter, domainType: domainType);
            setDTColumnHeaders(fRes);
            vals = TableTools.GetArrayFromTable(fRes);
            //sht = Excel.AddExcelSheet(sheetName, keepSheet1);
            //sht.Range(sht.Cells(1, 1), sht.Cells(vals.GetUpperBound(0) + 1, vals.GetUpperBound(1) + 1)).Value = vals;
            try
            {
                varAlias = vals[2, 1]; //store to use in the save operation
            }
            catch (Exception)
            {
                //do nothing
            }
            return fRes;
        }

        public static DataTable ftes
        {
            get
            {
                if (l_ftes == null)
                {
                    //assumes varFilter and geoFilter are pre-set
                    l_ftes = db.GetFTEVals2(varFilter: varFilter, geoFilter: geoFilter);
                }
                return l_ftes;
            }
        }

        public List<string> ReportMulipleVARs(List<string> vars, out Report report, string folderName = "")
        {
            Report r = new Report();
            List<string> files = new List<string>();
            foreach (string var in vars)
            {
                varFilter = var;
                string vRep = ReportVAR(folderName: folderName);
                if (vRep.StartsWith("ERROR:"))
                {
                    //one of the reports failed!!  How to report?
                    r.AddLine(vRep);
                }
                else
                {
                    files.Add(vRep);
                }
            }
            report = r;
            return files;
        }
        #endregion


        #region Importers
        public Report ImportCSV(string csvPath)
        {
            //Read the contents of CSV file.  
            string csvData = File.ReadAllText(csvPath);

            //generate unique lists
            List<Company> companies = new List<Company>();  //this needs user intervention - mapping to alias and parent
            List<Learner> learners = new List<Learner>();
            List<Course> courses = new List<Course>();
            //weekly will be csv (comma separated)
            List<Activity> activities = parseCSV(csvData, out companies, out learners, out courses, delimiter: ReportIO.sep); // "\t");

            //run the database imports            
            var res = BatchImport(activities, companies, learners, courses);
            return res;
        }
        #endregion

        #region Report Creator Functions
        public List<string> ReportMulipleVARs2(List<string> vars, out Report report, string folderName = "")
        {
            Report r = new Report();
            List<string> files = new List<string>();
            //first, create the temp table(s) to be used by all VAR queries to speed it up
            try
            {
                db.CreateTransposedTempTable2(); //include everything
            }
            catch (Exception x)
            {

                throw;
            }
            
            db.CreateTransposedVTTempTable();  //technical cert table
            foreach (string var in vars)
            {
                varFilter = var;
                string vRep = ReportVAR(folderName: folderName);
                if (vRep.StartsWith("ERROR:"))
                {
                    //one of the reports failed!!  How to report?
                    r.AddLine(vRep);
                }
                else
                {
                    files.Add(vRep);
                }
            }
            report = r;
            return files;
        }

        public string ReportVAR(string folderName = "") //added folderName option 8/17/2021
        {
            //updated to use Sales role filter for the sales course reports 5/17/2021
            RoleFilter = "%sales%";
            ProfileFilter = "reseller";
            db.reportType = DB.ReportType.VAR;

            string[] sheetNames = new string[] { "Design and Governance", "Simulation", "Manufacturing and Marketing", "Sales Skills",
                "Desktop","Marketing", "Tech Sales", "Tech Support", "Transposed", "Alignment", "SIMULIA Legacy", "TechTransposed" };
            keepSheet1 = true;
            DataSet ds = new DataSet();
            bool useCustomTargets = true;
            //do stuff on sheet 1 (design domain)
            DataTable dt1 = setSheetData2("[dg]%", 0, useCustomTargets: useCustomTargets);
            dt1.TableName = sheetNames[0];
            ds.Tables.Add(dt1);

            //do stuff on sheet 2 (Simulation)
            //updated to exclude SIMULIA legacy courses
            DataTable dt2 = setSheetData2("Sim%3%", -1, useCustomTargets: useCustomTargets);
            dt2.TableName = sheetNames[1];
            ds.Tables.Add(dt2);

            //sheet 3 marketing/sales and Manufacturing
            DataTable dt3 = setSheetData2("M%", 0, useCustomTargets: useCustomTargets);
            dt3.TableName = sheetNames[2];
            ds.Tables.Add(dt3);

            //sheet 4 business skills
            DataTable dt4 = setSheetData2("business%", 1, useCustomTargets: useCustomTargets);
            dt4.TableName = sheetNames[3];
            ds.Tables.Add(dt4);

            //sheet 5 Desktop
            DataTable dtDesk = setSheetData2("desk%", 1, useCustomTargets: useCustomTargets);
            dtDesk.TableName = sheetNames[4];
            ds.Tables.Add(dtDesk);

            RoleFilter = "%";
            //sheet 6 marketing
            DataTable dtMarketing = setSheetData2("Marketing%", 1, useCustomTargets: useCustomTargets);
            dtMarketing.TableName = sheetNames[5];
            ds.Tables.Add(dtMarketing);

            //sheet 7 tech sales and tech support
            DataTable dtTechSales = setSheetData2("tech sa%", -1, useCustomTargets: useCustomTargets);
            dtTechSales.TableName = sheetNames[6];
            ds.Tables.Add(dtTechSales);

            //sheet 8 tech sales and tech support
            DataTable dtTechSupport = setSheetData2("tech su%", -1, useCustomTargets: useCustomTargets);
            dtTechSupport.TableName = sheetNames[7];
            ds.Tables.Add(dtTechSupport);

            //sheet 9 transposed            
            //DataTable dtTrans = db.getTransposedReport3(varFilter, geoFilter: geoFilter);
            DataTable dtTrans = GetTransposedDataAllFTEs2(varFilter);
            dtTrans.TableName = sheetNames[8];
            ds.Tables.Add(dtTrans);

            //sheet 10 FTE and Alignment
            DataTable dtA = db.getAlignmentReport(varFilter: varFilter);
            dtA.TableName = sheetNames[9];
            ds.Tables.Add(dtA);

            //sheet 11 SIMULIA Legacy
            DataTable dtSL = GetVARSimLegacyTable(varFilter);
            dtSL.TableName = sheetNames[10];
            ds.Tables.Add(dtSL);

            //sheet 12 TechTransposed
            DataTable dtTechTrans = GetTransposedDataAllFTEs2(varFilter, TableTools.Type.tableTechTransposed);
            dtTechTrans.TableName = sheetNames[11];
            ds.Tables.Add(dtTechTrans);


            bool status;
            string saveRes;
            try
            {
                //saveRes = Excel.SaveExcel(fileName: varAlias + " Dashboard Data.xlsx", status: out status);
                //OExcelNew oen = new OExcelNew();
                folderName = (folderName != "") ? folderName + "\\" : folderName;
                saveRes = OExcelNew.exportDocument(fileName: varAlias + " Dashboard Data.xlsx", tableSet: ds, sheetNames: sheetNames, folderName: folderName);
            }
            catch (Exception e)
            {
                return "ERROR: " + e.Message;
            }

            return saveRes;

        }
        public string[] CreateGEOReportFiles(out bool status)
        {
            string path = "";
            //output Excel data
            DataSet ds = new DataSet();
            try
            {
                string[] sheetNames;
                ds = ReportGEO(out sheetNames);
                //OExcelNew oen = new OExcelNew();
                string fileName = "Master Learner Transcript.xlsx";
                DataSet smallDS = new DataSet();
                string[] smallSheetNames = new string[3];
                //get first and last table from ds

                smallDS.Tables.Add(ds.Tables[0].Copy());
                smallSheetNames[0] = smallDS.Tables[0].TableName;
                //add SIMULIA Legacy progress table
                smallDS.Tables.Add(ds.Tables[1].Copy());
                smallSheetNames[1] = smallDS.Tables[1].TableName;
                smallDS.Tables.Add(ds.Tables[ds.Tables.Count - 1].Copy());
                smallSheetNames[2] = smallDS.Tables[2].TableName;

                path = OExcelNew.exportDocument(fileName, smallDS, smallSheetNames);
                status = true;
            }
            catch (Exception e)
            {

                status = false;
            }

            List<string> files = new List<string>();
            files.Add(path);
            //create CSV Files
            for (int i = 1; i < ds.Tables.Count - 1; i++) //ignore first and last tables
            {
                string filePath = ReportIO.CreateCSVFileFromDT(ds.Tables[i]);
                files.Add(filePath);
            }
            return files.ToArray();
        }

        public string CreateSimLegacyReport(out bool status)
        {
            string path = "";
            try
            {
                DataTable t = db.GetSIMULIALegacyTransposedTable();  //new format including In Progress users
                string csvOutput = ReportIO.getCSVFromTable(t);

                string name = "SIMULIA Legacy Certifications.csv";

                path = ReportIO.WriteToFileEncoded(csvOutput, name);
                status = true;
            }
            catch (Exception e)
            {
                status = false;
                return e.Message;
            }
            return path;
        }

        /// <summary>
        /// Create Primary GEO Transposed report csv file
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public string CreateGEOTransposedReport(out bool status)
        {
            string path = "";
            try
            {
                string csvData = getTransposedCSVData2(varFilter: VARFilter, geoFilter: GEOFilter);

                string name = "Master Learner Transcript Transposed.csv";

                path = ReportIO.WriteToFileEncoded(csvData, name);
                if (path == string.Empty)
                {
                    //failed to save the file
                    status = false;
                    return path;
                }
                    
                status = true;
            }
            catch (Exception e)
            {
                status = false;
                return e.Message;
            }
            return path;
        }

        public string CreatePAPUserReport(ReportType Type, out bool status)
        {
            string path = string.Empty;
            string csvData = string.Empty;
            string name = string.Empty;
            try
            {
                switch (Type)
                {
                    case ReportType.SandrineIndividualScoreCSV:
                        csvData = GetSandrineReportCSV(geoFilter: GEOFilter);
                        name = "PAP - User Status Report.csv";
                        break;
                    case ReportType.VARDashboardExcel:
                        throw new NotImplementedException();
                        break;
                    default:
                        break;
                }

                path = ReportIO.WriteToFileEncoded(csvData, name);
                status = true;
            }
            catch (Exception e)
            {
                status = false;
                return e.Message;
            }
            return path;
        }
        #endregion



        #region Table Generators
        public DataTable GetVARSimLegacyTable(string varAlias = "%")
        {
            return db.GetSIMULIALegacyTransposedTable(varAlias);
        }


        /// <summary>
        /// Generate all GEO reports into one dataset containing individual datatables
        /// </summary>
        /// <param name="sheetNames"></param>
        /// <returns></returns>
        public DataSet ReportGEO(out string[] sheetNames)
        {
            VARFilter = "%";
            sheetNames = new string[] { "3DEXP Certification Quarterly", "3DX Export", "Sales Skills", "Other Products",
                "Tech Sales", "Marketing", "All Simulation", "Alignment", "SIMULIA Legacy", "DraftSight", "Domain Certs" }; 
            DataSet ds = new DataSet();
            keepSheet1 = false;
            RoleFilter = "%sales%";
            ProfileFilter = "reseller";
            db.reportType = DB.ReportType.GEO;
            //quarterly report
            //excel
            DataTable dt1 = new DataTable();
            dt1 = setQuarterlySheet2(geoFilter: "%", domainFilter: "%", profileFilter: "%", domainType: 0, roleFilter: "%", format: DateFormat.quarter);
            dt1.TableName = sheetNames[0];
            ds.Tables.Add(dt1);
            bool useCustomTargets = false;

            //SIMULIA Legacy quarterly progress
            //added Dec. 14, 2021 SPENS
            DataTable dtSL = new DataTable();
            dtSL = setQuarterlySheet2(geoFilter: "%", domainFilter: "%", profileFilter: "%", domainType: 2, roleFilter: "%", 
                format: DateFormat.quarter, includeRole: true);
            dtSL.TableName = sheetNames[8];
            ds.Tables.Add(dtSL);

            //Domain Certifications
            //added June 9, 2022 SPENS
            //CSV
            DataTable dtDC = setSheetData2(domainFilter: "%", domainType: 0, useCustomTargets: useCustomTargets, domainCertsOnly: true);
            dtDC.TableName = sheetNames[10];
            ds.Tables.Add(dtDC);

            //all 3DEXPERIENCE - "3DX Export"
            //CSV
            DataTable dt2 = setSheetData2(domainFilter: "%", domainType: 0, useCustomTargets: useCustomTargets);
            dt2.TableName = sheetNames[1];
            ds.Tables.Add(dt2);

            //Sales Skills 
            //CSV
            DataTable dt3 = setSheetData2(domainFilter: "busi%", domainType: 1, useCustomTargets: useCustomTargets);
            dt3.TableName = sheetNames[2];
            ds.Tables.Add(dt3);

            //Other products
            //CSV
            DataTable dt4 = setSheetData2(domainFilter: "[sd]%", domainType: 1, useCustomTargets: useCustomTargets); //Desktop products only (no Simulation)
            dt4.TableName = sheetNames[3];
            ds.Tables.Add(dt4);

            //All Simulation
            //csv
            DataTable dt7 = setSheetData2(domainFilter: "sim%", domainType: -1, useCustomTargets: useCustomTargets);
            dt7.TableName = sheetNames[6];
            //add the last datatable to the dataset last

            //technical
            //csv
            RoleFilter = "%";
            DataTable dt5 = setSheetData2(domainFilter: "tech%", domainType: 1, useCustomTargets: useCustomTargets);
            dt5.TableName = sheetNames[4];
            ds.Tables.Add(dt5);

            //marketing
            //csv
            DataTable dt6 = setSheetData2(domainFilter: "Market%", domainType: 1, useCustomTargets: useCustomTargets);
            dt6.TableName = sheetNames[5];
            ds.Tables.Add(dt6);
            ds.Tables.Add(dt7);

            //DraftSight Progress 
            DataTable dtDS = new DataTable();
            dtDS = setQuarterlySheetByCourse(geoFilter: "%", courseFilter: "%draftsight%", profileFilter: "%", domainType: 2, roleFilter: "%",
                format: DateFormat.quarter);
            dtDS.TableName = sheetNames[9];
            ds.Tables.Add(dtDS);

            //alignment
            //excel
            DataTable dtAlign = db.getAlignmentByGEO4(varFilter: "%", geoFilter: "%");
            dtAlign.TableName = sheetNames[7];
            db.Disconnect();
            ds.Tables.Add(dtAlign);

            
            return ds;
        }

        #endregion


        #region SheetDefinitions
        private static DataTable getSheet(string domainFilter, int domainType = 0, bool useCustomTargets = false, bool domainCertsOnly = false)
        {
            DataTable t = db.getCourseCompletions5(varFilter: varFilter,
                domainFilter: domainFilter, domainType: domainType, geoFilter: geoFilter, 
                roleFilter: roleFilter, profileFilter: profileFilter, domainCertsOnly: domainCertsOnly);  //option for domain certs added June 9, 2022 (no option for both)

            //get custom targets datatable
            DataTable cTargets = db.getCustomTargets();
            //DataTable ftes = db.getFTEVals4(varFilter: varFilter, geoFilter: geoFilter, style: style);
            DataTable ftes = db.GetFTEVals6(varFilter: varFilter, geoFilter: geoFilter, style: style);

            var TargetRes = setCustomTargets(t, cTargets);
            t = TargetRes;
            var fRes = setTargetVals(ftes, t);
            return fRes;            
        }


        #endregion

        #region User Edit Tools
        public string setUserStateBatch(List<string> emails, bool deletedStatus)
        {
            Report r = new Report();
            r.CountOnly = true;
            r.Type = Report.reportType.delete;

            foreach (var email in emails)
            {
                var res = db.setUserStateByEmail(email: email, deletedStatus: deletedStatus);
                if (res) { r.AddLine(email); }
            }
            return r.Message;
        }

        public void setUserNameBatch(List<User> users, string conString)
        {
            foreach (var userDef in users)
            {
                db.SetUserName(userName: userDef.userName, newName: userDef.newName);
            }
        }
        #endregion

        #region ReportManipulation

        //for quarterly incremental increase reports
        private static DataTable setQuarterlyIncrease(DataTable dt)
        {
            
            string geo = "";
            string role = "";
            
            DataRow dr = null;
            bool hasRoles = dt.Columns.Contains("Role");
            int iVal = hasRoles == true ? 3 : 2;
            //start from the second row.  prev vals are r - 1
            for (int r = 1; r < dt.Rows.Count; r++)
            {
                dr = dt.Rows[r];
                geo = dr.ItemArray[0].ToString();
                role = hasRoles == true ? dr.ItemArray[1].ToString() : string.Empty;
                                    
                var vals = dr.ItemArray;
                var pVals = dt.Rows[r - 1].ItemArray;
                if (geo == pVals[0].ToString())
                {
                    if (!hasRoles || role == pVals[1].ToString())
                    {     
                        //add the previous value to this value
                        for (int i = iVal; i < vals.Length; i++)
                        {
                            vals[i] = (int)pVals[i] + (int)vals[i];
                        }
                        dr.ItemArray = vals;
                    }
                }
            }
            return dt;
        }

        //sets custom targets
        private static DataTable setCustomTargets(DataTable completions, DataTable cTargets)
        {
            DataTable results = new DataTable();
            DataRow dr = null;
            if (cTargets.Rows.Count > 0)
            {
                for (int r = 0; r < cTargets.Rows.Count; r++)
                {
                    dr = cTargets.Rows[r];
                    var vals = dr.ItemArray;
                    int varID = (int)vals[3]; 
                    int courseID = (int)vals[2];
                    //check for the var in the completions table
                    if (completions.Select(String.Format("var_alias_id = '{0}'", varID.ToString())).Length !=0)
                    {
                        foreach (DataRow row in completions.Rows)
                        {
                            if ((int)row["var_alias_id"] == varID && (long)row["course_alias_id"] == courseID)
                            {
                                row["Target"] = vals[1];  //new target value
                            }
                        }

                    }
                }
            }
            
            return completions;
        }

        //sets Target based on VAR Alias, not VAR Parent (Organization)
        private static DataTable setTargetVals(DataTable FTEs, DataTable completions)
        {            
            if (FTEs.Rows.Count > 0)
            {
                //merget the tables
                //FTEs table column "VAR" = "VAR Alias"
                //var results = from table2 in completions.AsEnumerable() 
                //    join table1 in FTEs.AsEnumerable() on (string)table2["VAR Alias"] equals (string)table1["VAR"]
                //    select new
                //    {
                //        GEO = (string)table2["GEO"],
                //        VARname = (string)table2["VAR Alias"],
                //        Course = (string)table2["Course"],
                //        Completed = (Int32)table2["Completed"],
                //        inProgress = (Int32)table2["In Progress"],
                //        notStarted = (Int32)table2["Not Started"],
                //        Target = Math.Round((double)table1["FTE Value"] * (double)table2["Target"]),
                //        Drilldown = (string)table2["Drilldown"]
                //    };

                var results = from table2 in completions.AsEnumerable()
                              join table1 in FTEs.AsEnumerable() on (string)table2["VAR Alias"] equals (string)table1["VAR"] into tempJoin
                              from leftJoin in tempJoin.DefaultIfEmpty()
                              select new
                              {
                                  GEO = (string)table2["GEO"],
                                  VARname = (string)table2["VAR Alias"],
                                  Course = (string)table2["Course"],
                                  Completed = (Int32)table2["Completed"],
                                  inProgress = (Int32)table2["In Progress"],
                                  notStarted = (Int32)table2["Not Started"],
                                  Target = leftJoin == null ? 0 : Math.Round(leftJoin.Field<decimal>("FTE Value") * (decimal)table2["Target"]),
                                  Drilldown = (string)table2["Drilldown"]
                              };
                return ToDataTable(results);
            }
            else
            {
                //set Target column to zero, no FTE data found
                var results = from table in completions.AsEnumerable()
                              select new
                              {
                                  GEO = (string)table["GEO"],
                                  VARname = (string)table["VAR Alias"],
                                  Course = (string)table["Course"],
                                  Completed = (Int32)table["Completed"],
                                  inProgress = (Int32)table["In Progress"],
                                  notStarted = (Int32)table["Not Started"],
                                  Target = 0,
                                  Drilldown = (string)table["Drilldown"]
                              };
                return ToDataTable(results);
            }                        
        }

        private static DataTable ToDataTable<T>(IEnumerable<T> items)
        {
            var tb = new DataTable(typeof(T).Name);

            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            return tb;
        }

        #endregion

        #region DataManipluation
        /// <summary>
        /// Batch import all records from lists, then do database cleanup
        /// </summary>
        /// <param name="activities"></param>
        /// <param name="companies"></param>
        /// <param name="learners"></param>
        /// <param name="courses"></param>
        /// <returns></returns>
        protected Report BatchImport(List<Activity> activities, List<Company> companies,
            List<Learner> learners, List<Course> courses)
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
                a.Insert();
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

        private Report BatchImportLearners(List<Learner> learners, List<Company> companies)
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

            //learners
            foreach (var l in learners)
            {
                if (l.Insert())
                {
                    learnerCount++;
                    r.AddLine("NEW LEARNER: " + l.Name + " " + l.email);
                    r.AddLearner(l);
                }
            }            
            return r;
        }





        /// <summary>
        /// Convert Tab Delimited CSV file to a List of FTEs
        /// </summary>
        /// <param name="csvData">Tab delimited CSV file in UTF-8 encoding</param>
        /// <returns></returns>
        protected List<FTE> parseFTECSV(string csvData)
        {
            TextFieldParser parser = new TextFieldParser(new StringReader(csvData));

            parser.HasFieldsEnclosedInQuotes = true;
            parser.SetDelimiters("\t");

            List<FTE> FTEs = new List<FTE>();

            dynamic[] data;
            bool firstLine = true;
            while (!parser.EndOfData)
            {
                data = parser.ReadFields();
                if (firstLine)
                {
                    //headers - ignore for now
                }
                else
                {
                    FTE f = new FTE(db);                    
                    f.Account = data[10];
                    f.AccountStatus = data[11];
                    f.AccountType = data[12];
                    f.contactID = data[9];
                    f.firstName = data[7];
                    f.lastName = data[8];
                    f.FTEStatus = data[4];
                    f.FTEValue = Convert.ToDouble(data[5]);  //double value
                    f.GEO = data[16];                   
                    f.Organization = data[13];
                    //if (f.contactID == "1-9LU27C4")
                    //{
                    //    var x = 1;
                    //}
                    FTEs.Add(f);
                    
                }
                firstLine = false;
            }
            parser.Close();
            return FTEs;
        }

        /// <summary>
        /// Convert Tab Delimited CSV file to a List of Learner
        /// </summary>
        /// <param name="csvData">Tab delimited CSV file in UTF-8 encoding</param>
        /// <returns></returns>
        protected List<Learner> parseFTECSVtoLearners(string csvData, out List<Company> companies)
        {
            TextFieldParser parser = new TextFieldParser(new StringReader(csvData));

            parser.HasFieldsEnclosedInQuotes = true;
            parser.SetDelimiters("\t");

            List<Learner> Learners = new List<Learner>();
            companies = new List<Company>();

            dynamic[] data;
            bool firstLine = true;
            while (!parser.EndOfData)
            {
                data = parser.ReadFields();
                if (firstLine)
                {
                    //headers - ignore
                }
                else if (data[4] == "Active" && data[11] == "Active")  //only active FTEs and active Accounts
                {
                    Learner l = new Learner(db);
                    //company data
                    Company c = new Company(db);
                    c.Name = data[10];
                    c.status = 1;
                    //check if it already exists in the list
                    var t = companies.Find(x => (x.Name == c.Name));
                    if (t == null)
                    {
                        companies.Add(c);
                    }
                    else
                    {
                        c = t;
                    }
                    
                    l.company = c;                   
                    l.Country = data[15];
                    l.email = data[14];
                    l.fte_value = Convert.ToDouble( data[5]);
                    l.GEO = data[16];
                    l.Name = data[7] + " " + data[8];                    
                    Learners.Add(l);
                }
                firstLine = false;
            }
            parser.Close();
            return Learners;
        }

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
            out List<Learner> learners, out List<Course> courses, string delimiter = ",")
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
                                cs.Name = data[TableTools.getHeaderIndex(headers, "Course" )]; //data[6];
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
                    a.quizScore = Converters.getDecimalFromString( data[TableTools.getHeaderIndex(headers, "Quiz_score")]);
                    activities.Add(a);
                }
                firstLine = false;
            }
            parser.Close();

            //now have unique lists of learners, companies, courses and activities
            return activities;
        }


        #endregion

        #region ExcelFunctions

        private static void setDTColumnHeaders(DataTable dt)
        {
            //GEO, VAR Alias, Completed, In Progress, Not Started, Target, Drilldown
            dt.Columns[1].ColumnName = "VAR Alias";
            dt.Columns[4].ColumnName = "In Progress";
            dt.Columns[5].ColumnName = "Not Started";
        }


        private static DataTable setQuarterlySheet2(string geoFilter, string domainFilter,
            string profileFilter, string roleFilter = "%sales%", int domainType = -1, DateFormat format = DateFormat.quarter,
            bool includeRole = false)
        {
            DataTable dt = new DataTable();
            if (!includeRole)
                dt = db.getQuarterlyProgressTable2(geoFilter: geoFilter,
                    domainFilter: domainFilter, profileFilter: profileFilter, roleFilter: roleFilter, domainType: domainType, format: format);
            else
                dt = db.getQuarterlyProgressWithRole(geoFilter: geoFilter,
                    domainFilter: domainFilter, profileFilter: profileFilter, roleFilter: roleFilter, domainType: domainType);
            //add geo quarterly values to get cummulative totals            
            dt = setQuarterlyIncrease(dt);
            dt = addMissingRows(dt, format, includeRole);
            if (format == DateFormat.month)
                dt = Converters.setDateDisplay(dt);
            return dt;
        }

        private static DataTable setQuarterlySheetByCourse(string geoFilter, string courseFilter,
            string profileFilter, string roleFilter = "%sales%", int domainType = -1, DateFormat format = DateFormat.quarter)
        {
            DataTable dt = new DataTable();
            //if (!includeRole)
                dt = db.getQuarterlyProgressTableByCourse(geoFilter: geoFilter, courseFilter: courseFilter, 
                    profileFilter: profileFilter, roleFilter: roleFilter, format: format);
            //else
            //    dt = db.getQuarterlyProgressWithRole(geoFilter: geoFilter,
            //        domainFilter: domainFilter, profileFilter: profileFilter, roleFilter: roleFilter, domainType: domainType);
            //add geo quarterly values to get cummulative totals            
            dt = setQuarterlyIncrease(dt);
            dt = addMissingRows(dt, format); //, includeRole);
            if (format == DateFormat.month)
                dt = Converters.setDateDisplay(dt);
            return dt;
        }

        private static DataTable addMissingRows(DataTable dt, DateFormat format, bool includeRole = false)
        {
            DataRow dr = null;
            DataRow prevrow = null;
            int qCol = includeRole == true ? 2 : 1;
            for (int r = 0; r< dt.Rows.Count; r++)
            {
                dr = dt.Rows[r];
                bool lastRow = r == (dt.Rows.Count - 1) ? true : false;
                if (prevrow != null)
                {
                    if (!lastRow && dr.ItemArray[0].ToString() == prevrow.ItemArray[0].ToString())  //if not the last row and it's the same geo...
                    {
                        if (!includeRole || (includeRole && dr.ItemArray[1].ToString() == prevrow.ItemArray[1].ToString()))  //if it is 
                        {
                            if (!Converters.IsNextQuarter(prevrow.ItemArray[qCol].ToString(), dr.ItemArray[qCol].ToString()))
                            {
                                //insert a row copy between previous row and current row
                                DataRow newRow = dt.NewRow();
                                //newRow.ItemArray = prevrow.ItemArray;
                                var items = prevrow.ItemArray;
                                string newval = Converters.GetNextQuarterValue(prevrow.ItemArray[qCol].ToString());
                                items[qCol] = newval;
                                newRow.ItemArray = items;
                                dt.Rows.InsertAt(newRow, r);
                                r++;  //increment r since we just added a new row  
                                      //dr = newRow;                         
                            }
                        }
                    }
                    else
                    {
                        //was the last quarter the latest?
                        var diff = !lastRow ? Converters.GetQuarterDifference(prevrow.ItemArray[qCol].ToString()) :
                            Converters.GetQuarterDifference(dr.ItemArray[qCol].ToString());
                        if (diff > 0)
                        {
                            for (int i = 1; i <= diff; i++)
                            {
                                //insert a row copy between previous row and current row
                                DataRow newRow = dt.NewRow();
                                //newRow.ItemArray = prevrow.ItemArray;
                                var items = !lastRow ? prevrow.ItemArray : dr.ItemArray;
                                string newval = Converters.GetNextQuarterValue(items[qCol].ToString());                                
                                items[qCol] = newval;
                                newRow.ItemArray = items;
                                if (!lastRow)
                                    dt.Rows.InsertAt(newRow, r); 
                                else
                                    dt.Rows.InsertAt(newRow, r+1);
                                r++;
                                prevrow = newRow;
                            }
                        }
                    }
                }
                prevrow = dr;
            }

           
            return dt;
        }





        private static DataTable setSheetData2(string domainFilter, int domainType, bool useCustomTargets, bool domainCertsOnly = false)
        {
            var fRes = new DataTable();
            string[,] vals;

            fRes = getSheet(domainFilter: domainFilter, domainType: domainType, useCustomTargets: useCustomTargets, domainCertsOnly: domainCertsOnly);

            setDTColumnHeaders(fRes);
            vals = TableTools.GetArrayFromTable(fRes);
            try
            {
                varAlias = vals[2, 1]; //store to use in the save operation
            }
            catch (Exception)
            {
                varAlias = varFilter;
            }
            return fRes;
        }

        /// <summary>
        /// Get transposed data for all VAR reports
        /// OBSOLETE - created duplicate rows with the April 2022 transposed report method
        /// </summary>
        /// <param name="varFilter"></param>
        /// <returns></returns>
        public DataTable GetTransposedDataAllFTEs(string varFilter = "%", 
            TableTools.Type TableType = TableTools.Type.tableSalesTransposed)
        {
            DataTable trans = new DataTable();
            DataTable ftes = new DataTable();
            if (db.dbConn.State != ConnectionState.Open)
                db.Connect(); 
            trans = db.getTransposedReport5(varFilter, TableType: TableType);  //using new optimized method for VAR reports
            if (TableType == TableTools.Type.tableSalesTransposed)  //only add ftes to sales table
            {
                ftes = db.getRemainingFTEs(varFilter);
                trans.Merge(ftes);
            }
            return trans;
            
        }

        /// <summary>
        /// Get transposed data for all VAR reports
        /// April 2022 update 
        /// </summary>
        /// <param name="varFilter"></param>
        /// <returns></returns>
        public DataTable GetTransposedDataAllFTEs2(string varFilter = "%",
            TableTools.Type TableType = TableTools.Type.tableSalesTransposed)
        {
            DataTable trans = new DataTable();
            DataTable ftes = new DataTable();
            if (db.dbConn.State != ConnectionState.Open)
                db.Connect();
            trans = db.getTransposedReport5(varFilter, TableType: TableType);  //using new optimized method for VAR reports
            //no need to merge in additional users.  The new transposed report already gets them.
            //if (TableType == TableTools.Type.tableSalesTransposed)  //only add ftes to sales table
            //{
            //    ftes = db.getRemainingFTEs(varFilter);
            //    trans.Merge(ftes);
            //}
            return trans;

        }



        private static string getTransposedCSVData2(string varFilter, string geoFilter = "%")
        {
            //Create a temp table first
            db.CreateTransposedTempTable2(varFilter, geoFilter: geoFilter);
            DataTable t = db.getTransposedReport5(varFilter, geoFilter: geoFilter, TableType: TableTools.Type.tableSalesTransposed);
            string csvOutput = ReportIO.getCSVFromTable(t);
            return csvOutput;
        }

        public static string GetSandrineReportCSV(string geoFilter = "%")
        {
            DataTable t = db.GetTransposedReportSandrine(geoFilter: geoFilter);
            string csvOut = ReportIO.getCSVFromTable(t);
            return csvOut;
        }

        /// <summary>
        /// Import data into the database from a temp table for Virtual Tester technical certifications
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        internal Report setVTValuesByCSVFile(string filepath)
        {
            string tableName = "#VTTemp";
            DB db = new DB();
            db.Connect();
            Report r = new Report();
            r.AddLine("VT Data Import");
            //assumes the text file is on the server already
            if (db.importVTToTempTable(filepath, tableName))
            {
                //will this make a mess of the DB????  new var names for learners imported from VT?  Will need to map the new VARs
                db.addVARsFromVTTempTable(tableName, ref r);
                db.addLearnersFromVTTempTable(tableName, ref r);
                //create course aliases from temp table
                db.addCourseAliasFromVTTempTable(tableName, ref r);
                //create courses from the temp table
                db.addCoursesFromVTTempTable(tableName, ref r);
                //create activities or update them from the temp table
                db.addActivitiesFromVTTempTable(tableName, ref r);  //reports only text count of added activities
                db.Disconnect();  //removes the temp table
                return r;
            }
            else
            {
                db.Disconnect();
                return null;
            }
        }
        /// <summary>
        /// Import FTE database information into a temp table, then into full database
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        internal Report setFTEValuesByCSVFile(string filepath)
        {
            string tableName = "SWUReporting.dbo.FTETemp";
            DB db = new DB();
            db.Connect();
            Report r = new Report();
            r.AddLine("FTE Data Import");
            //assumes the text file is on the server already
            if (db.importFTECSVToTempTable(filepath, tableName))
            {
                //run cleanup first
                db.cleanFTETable(tableName);
                db.addVARParentsFromTempTable(tableName);
                db.addVARsFromTempTable(tableName, ref r);
                db.addLearnersFromTempTable(tableName, ref r);
                //update  the FTE values
                db.clearAllFTEValues();
                db.setFTEValsFromTempTable(tableName);
                db.DropTableIfExists(tableName);
                db.Disconnect();
                return r;
            }
            else
            {
                db.Disconnect();
                return null;
            }
        }

        #endregion
    }


}
