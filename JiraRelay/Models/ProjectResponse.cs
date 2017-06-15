using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace JiraRelay.Models
{

    public class ProjectResponse
    {
        private List<Project> Projects { get; set; }
    }
    public class AvatarUrls
    {
        [JsonProperty("48x48")]
        public string __invalid_name__48x48 { get; set; }

        [JsonProperty("24x24")]
        public string __invalid_name__24x24 { get; set; }

        [JsonProperty("16x16")]
        public string __invalid_name__16x16 { get; set; }

        [JsonProperty("32x32")]
        public string __invalid_name__32x32 { get; set; }
    }

    public class ProjectCategory
    {
        public string self { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    public class Project
    {
        public string self { get; set; }
        public string id { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public AvatarUrls avatarUrls { get; set; }
        public ProjectCategory projectCategory { get; set; }
    }
}