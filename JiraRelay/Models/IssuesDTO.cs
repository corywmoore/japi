using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JiraRelay.Models
{
    public class IssuesDTO
    {
        public string jql { get; set; }

        public int startAt { get; set; }

        public int maxResults { get; set; }

        public string[] fields { get; set; }

    }
}