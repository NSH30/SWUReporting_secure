using System;
using System.Collections.Generic;

namespace ReportBuilder
{

    public class Report
    {
        #region Properties
        public reportType Type = reportType.unknown;
        public bool CountOnly = false;
        protected List<string> messages = null;
        protected List<Learner> learners = null;
        protected List<Course> courses = null;
        protected List<Company> companies = null;

        public enum elementType
        {
            learner,
            course,
            company
        }

        public enum reportType
        {
            import,
            delete,
            unknown
        }

        public string Message
        {
            get
            {
                if (messages == null) { return "No new objects created."; }
                int count = messages.Count;
                string s = null;
                string s2 = null;
                switch (Type)
                {
                    case reportType.import:
                        s = "Imported ";
                        s2 = " Learners";
                        break;
                    case reportType.delete:
                        s = "Marked ";
                        s2 = " Learners DELETED";
                        break;
                    case reportType.unknown:
                        break;
                }
                string total = s + (count - 1).ToString() + s2;
                messages.Insert(1, total);
                if (CountOnly)
                {
                    //return (messages.Count - 1).ToString();
                    return messages[1];
                }
                else
                {
                    return string.Join("\n", messages);
                }
            }
        }
        #endregion

        public void AddLearner(Learner l)
        {
            if (learners == null) { learners = new List<Learner>(); }
            learners.Add(l);
        }
        public void AddCourse(Course c)
        {
            if (courses == null)
            {
                courses = new List<Course>();
            }
            courses.Add(c);
        }
        public void AddCompany(Company c)
        {
            if (companies == null)
            {
                companies = new List<Company>();
            }
            companies.Add(c);
        }

        public string GetAddedIDs(elementType e)
        {
            string output = ","; //Query requires initial and trailing comma
            dynamic stuff = null;
            switch (e)
            {
                case elementType.learner:
                    stuff = learners;
                    break;
                case elementType.course:
                    stuff = courses;
                    break;
                case elementType.company:
                    stuff = companies;
                    break;
                default:
                    return null;
            }
            if (stuff != null)
            {
                foreach (var s in stuff)
                {
                    output = output + s.ID.ToString() + ",";
                }
                return output;
            }
            else
            {
                return null;
            }

        }




        private void addHeader()
        {
            string m = null;
            messages = new List<string>();
            switch (Type)
            {
                case reportType.import:
                    m = "Learner Transcript Import Report - ";
                    break;
                case reportType.delete:
                    m = "Batch Delete Report - ";
                    break;
                case reportType.unknown:
                    break;
            }
            messages.Add(m + DateTime.Now.ToString());
        }
        public void AddLine(string value)
        {
            if (messages == null) { addHeader(); }
            messages.Add(value);
        }

    }
}