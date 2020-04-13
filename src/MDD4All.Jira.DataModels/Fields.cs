using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.Jira.DataModels
{
    public class Fields
    {
        [JsonProperty("statuscategorychangedate")]
        public DateTime? StatusCategoryChangeDate { get; set; }

        [JsonProperty("issuetype")]
        public IssueType IssueType { get; set; }

        [JsonProperty("lastViewed")]
        public DateTime? LastViewed { get; set; }

        [JsonProperty("created")]
        public DateTime? Created { get; set; }

        [JsonProperty("updated")]
        public DateTime? Updated { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("creator")]
        public User Creator { get; set; }

        public object timespent { get; set; }

        [JsonProperty("project")]
        public Project Project { get; set; }

        public List<object> fixVersions { get; set; }

        public object aggregatetimespent { get; set; }

        public object resolution { get; set; }

        public object resolutiondate { get; set; }

        public int? workratio { get; set; } = null;

        public Watches watches { get; set; }
        
        public object environment { get; set; }

        public object duedate { get; set; }

        public Progress progress { get; set; }

        public Votes votes { get; set; }

        public Priority priority { get; set; }

        public List<object> labels { get; set; }

        public object timeestimate { get; set; }

        public object aggregatetimeoriginalestimate { get; set; }

        public List<object> versions { get; set; }

        public List<Issuelink> issuelinks { get; set; }

        public object assignee { get; set; }

        public List<object> components { get; set; }

        public object timeoriginalestimate { get; set; }

        public Timetracking timetracking { get; set; }

        public object security { get; set; }

        public List<object> attachment { get; set; }

        public object aggregatetimeestimate { get; set; }
        
        

        public List<object> subtasks { get; set; }

        public User reporter { get; set; }

        public Aggregateprogress aggregateprogress { get; set; }

        //public string customfield_10000 { get; set; }
        //public object customfield_10001 { get; set; }
        //public object customfield_10002 { get; set; }
        //public object customfield_10003 { get; set; }
        //public object customfield_10004 { get; set; }
        //public object customfield_10005 { get; set; }
        //public object customfield_10006 { get; set; }
        //public object customfield_10007 { get; set; }
        //public object customfield_10008 { get; set; }
        //public object customfield_10009 { get; set; }
        //public object customfield_10010 { get; set; }
        //public object customfield_10014 { get; set; }
        //public object customfield_10015 { get; set; }
        //public object customfield_10016 { get; set; }
        //public object customfield_10017 { get; set; }
        ////public Customfield10018 customfield_10018 { get; set; }
        //public string customfield_10019 { get; set; }
        //public object customfield_10020 { get; set; }
        //public object customfield_10021 { get; set; }
        //public object customfield_10022 { get; set; }
        //public object customfield_10023 { get; set; }

        
    }
}
