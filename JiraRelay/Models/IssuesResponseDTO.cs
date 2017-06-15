using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JiraRelay.Models
{
    public class IssueResponseDTO
    {
        public string project { get; set; }
        public List<IssueSection> issueSections { get; set; }
    }

    public class IssueSection
    {
        public string name { get; set; }

        public int count { get; set; }

        public List<Issue> issues { get; set; }

    }

}