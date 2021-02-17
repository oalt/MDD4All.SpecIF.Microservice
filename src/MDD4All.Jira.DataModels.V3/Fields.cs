/*
 * Copyright (c) MDD4All.de, Dr. Oliver Alt
 */
using MDD4All.Jira.DataModels.V3.ADF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MDD4All.Jira.DataModels.V3
{
    public partial class Fields
    {
        public Fields()
        {

        }

        public Fields(Dictionary<string, object> fieldDictionary)
        {
            FieldDictionary = fieldDictionary;
        }

        

        [JsonIgnore]
        public Dictionary<string, object> FieldDictionary { get; set; } = new Dictionary<string, object>();


        [JsonProperty("statuscategorychangedate")]
        public string Statuscategorychangedate 
        { 
            get
            {
                string result = "";

                if(FieldDictionary.ContainsKey("statuscategorychangedate"))
                {
                    result = FieldDictionary["statuscategorychangedate"].ToString();
                }

                return result;
            }

            set
            {
                if(FieldDictionary.ContainsKey("statuscategorychangedate"))
                {
                    FieldDictionary["statuscategorychangedate"] = value;
                }
                else
                {
                    FieldDictionary.Add("statuscategorychangedate", value);
                }
            }
                
        }

        [JsonProperty("issuetype")]
        public IssueType IssueType
        {
            get
            {
                IssueType result = null;

                if (FieldDictionary.ContainsKey("issuetype"))
                {
                    string json = FieldDictionary["issuetype"].ToString();

                    result = JsonConvert.DeserializeObject<IssueType>(json);
                }

                return result;
            }

            set
            {
                if (FieldDictionary.ContainsKey("issuetype"))
                {
                    FieldDictionary["issuetype"] = value;
                }
                else
                {
                    FieldDictionary.Add("issuetype", value);
                }
            }
        }

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


        [JsonProperty("priority")]
        public Priority Priority { get; set; }

        [JsonProperty("labels")]
        public List<object> Labels { get; set; }

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
        public DateTime? Updated
        {
            get
            {
                DateTime result = new DateTime();

                if (FieldDictionary.ContainsKey("updated"))
                {
                    result = Convert.ToDateTime(FieldDictionary["updated"].ToString());
                }

                return result;
            }

            set
            {
                if (FieldDictionary.ContainsKey("updated"))
                {
                    FieldDictionary["updated"] = value;
                }
                else
                {
                    FieldDictionary.Add("updated", value);
                }
            }
        }

        [JsonProperty("status")]
        public Status Status
        {
            get
            {
                Status result = null;

                if (FieldDictionary.ContainsKey("status"))
                {
                    if (FieldDictionary["status"] != null)
                    {
                        string json = FieldDictionary["status"].ToString();

                        result = JsonConvert.DeserializeObject<Status>(json);
                    }
                }

                return result;
            }

            set
            {
                if (FieldDictionary.ContainsKey("status"))
                {
                    FieldDictionary["status"] = value;
                }
                else
                {
                    FieldDictionary.Add("status", value);
                }
            }
        }

        [JsonProperty("components")]
        public List<object> Components { get; set; }

        [JsonProperty("timeoriginalestimate")]
        public object Timeoriginalestimate { get; set; }

        [JsonProperty("description")]
        public AtlassianDocumentFormat Description
        {
            get
            {
                AtlassianDocumentFormat result = null;

                if (FieldDictionary.ContainsKey("description"))
                {
                    if (FieldDictionary["description"] != null)
                    {
                        string json = FieldDictionary["description"].ToString();

                        result = JsonConvert.DeserializeObject<AtlassianDocumentFormat>(json);
                    } 
                }

                return result;
            }

            set
            {
                if (FieldDictionary.ContainsKey("description"))
                {
                    FieldDictionary["description"] = value;
                }
                else
                {
                    FieldDictionary.Add("description", value);
                }
            }
        }

        [JsonProperty("timetracking")]
        public Timetracking Timetracking { get; set; }

        [JsonProperty("security")]
        public object Security { get; set; }

        [JsonProperty("aggregatetimeestimate")]
        public object Aggregatetimeestimate { get; set; }

        [JsonProperty("attachment")]
        public List<object> Attachment { get; set; }

        [JsonProperty("summary")]
        public string Summary
        {
            get
            {
                string result = "";

                if (FieldDictionary.ContainsKey("summary"))
                {
                    result = FieldDictionary["summary"].ToString();
                }

                return result;
            }

            set
            {
                if (FieldDictionary.ContainsKey("summary"))
                {
                    FieldDictionary["summary"] = value;
                }
                else
                {
                    FieldDictionary.Add("summary", value);
                }
            }

        }

        [JsonProperty("creator")]
        public Creator Creator
        {
            get
            {
                Creator result = null;

                if (FieldDictionary.ContainsKey("creator") && FieldDictionary["creator"] != null)
                {
                    string json = FieldDictionary["creator"].ToString();

                    result = JsonConvert.DeserializeObject<Creator>(json);
                }

                return result;
            }

            set
            {
                if (FieldDictionary.ContainsKey("creator"))
                {
                    FieldDictionary["creator"] = value;
                }
                else
                {
                    FieldDictionary.Add("creator", value);
                }
            }
        }

        [JsonProperty("subtasks")]
        public List<object> Subtasks { get; set; }

        [JsonProperty("reporter")]
        public Creator Reporter { get; set; }

        [JsonProperty("aggregateprogress")]
        public Progress Aggregateprogress { get; set; }

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
