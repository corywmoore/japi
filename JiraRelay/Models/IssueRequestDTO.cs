using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JiraRelay.Models
{
    public class IssueRequestDTO
    {
        public string project { get; set; }
        public Int32 startAt { get; set; }
        public string domain { get; set; }
        public string itemTypes { get; set; }
        public List<IssueCategories> issueCategories { get; set; }
    }

    public class IssueCategories
    {
        public string name { get; set; }
        public List<Criterion> criteria { get; set; }
    }

    public class Criterion
    {
        public string subtaskName { get; set; }
        public string statusName { get; set; }
        public string relation { get; set; }
    }
}