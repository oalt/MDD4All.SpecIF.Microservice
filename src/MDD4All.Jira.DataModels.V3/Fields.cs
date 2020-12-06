using MDD4All.Jira.DataModels.V3.ADF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MDD4All.Jira.DataModels.V3
{
    public partial class Fields
    {
        [JsonProperty("statuscategorychangedate")]
        public string Statuscategorychangedate { get; set; }

        [JsonProperty("issuetype")]
        public IssueType IssueType { get; set; }

        [JsonProperty("timespent")]
        public object Timespent { get; set; }

        [JsonProperty("project")]
        public Project Project { get; set; }

        [JsonProperty("fixVersions")]
        public List<object> FixVersions { get; set; }

        [JsonProperty("aggregatetimespent")]
        public object Aggregatetimespent { get; set; }

        [JsonProperty("resolution")]
        public object Resolution { get; set; }

        [JsonProperty("customfield_10028")]
        public object Customfield10028 { get; set; }

        [JsonProperty("customfield_10029")]
        public object Customfield10029 { get; set; }

        [JsonProperty("resolutiondate")]
        public object Resolutiondate { get; set; }

        [JsonProperty("workratio")]
        public long Workratio { get; set; }

        [JsonProperty("issuerestriction")]
        public Issuerestriction Issuerestriction { get; set; }

        [JsonProperty("lastViewed")]
        public string LastViewed { get; set; }

        [JsonProperty("watches")]
        public Watches Watches { get; set; }

        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("customfield_10020")]
        public object Customfield10020 { get; set; }

        [JsonProperty("customfield_10021")]
        public object Customfield10021 { get; set; }

        [JsonProperty("customfield_10022")]
        public object Customfield10022 { get; set; }

        [JsonProperty("customfield_10023")]
        public object Customfield10023 { get; set; }

        [JsonProperty("priority")]
        public Priority Priority { get; set; }

        [JsonProperty("labels")]
        public List<object> Labels { get; set; }

        [JsonProperty("customfield_10016")]
        public object Customfield10016 { get; set; }

        [JsonProperty("customfield_10017")]
        public object Customfield10017 { get; set; }

        //[JsonProperty("customfield_10018")]
        //public Customfield10018 Customfield10018 { get; set; }

        [JsonProperty("customfield_10019")]
        public string Customfield10019 { get; set; }

        [JsonProperty("aggregatetimeoriginalestimate")]
        public object Aggregatetimeoriginalestimate { get; set; }

        [JsonProperty("timeestimate")]
        public object Timeestimate { get; set; }

        [JsonProperty("versions")]
        public List<object> Versions { get; set; }

        [JsonProperty("issuelinks")]
        public List<object> Issuelinks { get; set; }

        [JsonProperty("assignee")]
        public object Assignee { get; set; }

        [JsonProperty("updated")]
        public DateTime? Updated { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("components")]
        public List<object> Components { get; set; }

        [JsonProperty("timeoriginalestimate")]
        public object Timeoriginalestimate { get; set; }

        [JsonProperty("description")]
        public AtlassianDocumentFormat Description { get; set; }

        [JsonProperty("customfield_10010")]
        public object Customfield10010 { get; set; }

        [JsonProperty("customfield_10014")]
        public object Customfield10014 { get; set; }

        [JsonProperty("customfield_10015")]
        public object Customfield10015 { get; set; }

        [JsonProperty("timetracking")]
        public Timetracking Timetracking { get; set; }

        [JsonProperty("customfield_10005")]
        public object Customfield10005 { get; set; }

        [JsonProperty("customfield_10006")]
        public object Customfield10006 { get; set; }

        [JsonProperty("customfield_10007")]
        public object Customfield10007 { get; set; }

        [JsonProperty("security")]
        public object Security { get; set; }

        [JsonProperty("customfield_10008")]
        public object Customfield10008 { get; set; }

        [JsonProperty("aggregatetimeestimate")]
        public object Aggregatetimeestimate { get; set; }

        [JsonProperty("customfield_10009")]
        public object Customfield10009 { get; set; }

        [JsonProperty("attachment")]
        public List<object> Attachment { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("creator")]
        public Creator Creator { get; set; }

        [JsonProperty("subtasks")]
        public List<object> Subtasks { get; set; }

        [JsonProperty("reporter")]
        public Creator Reporter { get; set; }

        [JsonProperty("aggregateprogress")]
        public Progress Aggregateprogress { get; set; }

        [JsonProperty("customfield_10000")]
        public string Customfield10000 { get; set; }

        [JsonProperty("customfield_10001")]
        public object Customfield10001 { get; set; }

        [JsonProperty("customfield_10002")]
        public object Customfield10002 { get; set; }

        [JsonProperty("customfield_10003")]
        public object Customfield10003 { get; set; }

        [JsonProperty("customfield_10004")]
        public object Customfield10004 { get; set; }

        [JsonProperty("environment")]
        public object Environment { get; set; }

        [JsonProperty("duedate")]
        public object Duedate { get; set; }

        [JsonProperty("progress")]
        public Progress Progress { get; set; }

        [JsonProperty("votes")]
        public Votes Votes { get; set; }

        [JsonProperty("comment")]
        public ChangeLog Comment { get; set; }

        [JsonProperty("worklog")]
        public ChangeLog Worklog { get; set; }
    }
}
