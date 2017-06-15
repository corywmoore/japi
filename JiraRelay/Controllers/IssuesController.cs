using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JiraRelay.Models;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;

namespace JiraRelay.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("issues")]
    public class IssuesController : ApiController
    {
        [HttpPost]
        [Route("getissues")]
        public async Task<IHttpActionResult> GetIssues([FromBody] IssueRequestDTO issueReqeust)
        {
            try
            {
                string token = string.Empty;
                var re = Request;
                var headers = re.Headers;

                if (headers.Contains("Authorization"))
                {
                    token = headers.GetValues("Authorization").First();
                }
                else
                {
                    return Unauthorized();
                }

                var issues = await IssueClient(issueReqeust, token);
                var response = new IssueResponseDTO();

                response.project = issueReqeust.project;
                response.issueSections = new List<IssueSection>();

                issueReqeust.issueCategories.ForEach(category =>
                {
                    var subIssues = issues;
                    var subResponse = new IssueSection();

                    subResponse.name = category.name;

                    category.criteria.ForEach(criteria =>
                    {
                        criteria.subtaskName = criteria.subtaskName.ToLower();
                        criteria.statusName = criteria.statusName.ToLower();

                        switch (criteria.relation)
                        {
                            case "==":
                                subIssues = subIssues.Where(i =>
                                    i.fields.subtasks.Exists(s =>
                                        s.fields.summary.ToLower() == criteria.subtaskName && s.fields.status.name.ToLower() == criteria.statusName
                                    )
                                ).ToList();
                                break;

                            case "!=":
                                subIssues = subIssues.Where(i =>
                                    i.fields.subtasks.Exists(s =>
                                        s.fields.summary.ToLower() == criteria.subtaskName && s.fields.status.name.ToLower() != criteria.statusName
                                    )
                                ).ToList();
                                break;

                                //case "[]":
                                //    subIssues = subIssues.FindAll(i => (i.fields.subtasks.Exists(s => s.fields.summary == criteria.subtaskName && s.fields.status.name == criteria.statusName)));
                                //    break;

                                //case "![]":
                                //    subIssues = subIssues.FindAll(i => (i.fields.subtasks.Exists(s => s.fields.summary == criteria.subtaskName && s.fields.status.name == criteria.statusName)));
                                //    break;
                        }
                    });

                    subResponse.issues = subIssues;
                    subResponse.count = subIssues.Count;

                    response.issueSections.Add(subResponse);
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("getallissues")]
        public async Task<IHttpActionResult> GetAllIssues([FromBody] IssueRequestDTO issueRequest)
        {
            try
            {
                string token = string.Empty;
                var re = Request;
                var headers = re.Headers;

                if (headers.Contains("Authorization"))
                {
                    token = headers.GetValues("Authorization").First();
                }
                else
                {
                    return Unauthorized();
                }

                var issues = await IssueClient(issueRequest, token);

                return Ok(issues);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("login")]
        public async Task<IHttpActionResult> Login([FromBody] LoginRequestDTO Login)
        {
            var auth = new AuthResponse();
            auth.token = Encoder(Login.username + ":" + Login.password);

            var res = await JiraValidation(auth, Login.domain);

            switch (res.StatusCode)
            {
                case HttpStatusCode.OK:
                    return Ok(auth);
                case HttpStatusCode.Unauthorized:
                    return Unauthorized();
                case HttpStatusCode.Forbidden:
                    return Unauthorized();
                default:
                    return BadRequest();
            }
        }

        [HttpPost]
        [Route("validate")]
        public async Task<bool> Validate([FromBody] LoginRequestDTO Login)
        {
            var auth = new AuthResponse();
            var re = Request;
            var headers = re.Headers;

            if (headers.Contains("Authorization"))
            {
                auth.token = headers.GetValues("Authorization").First();
            }
            else
            {
                return false;
            }

            var res = await JiraValidation(auth, Login.domain);

            switch (res.StatusCode)
            {
                case HttpStatusCode.OK:
                    return true;
                default:
                    return false;
            }
        }

        [HttpPost]
        [Route("projects")]
        public async Task<HttpResponseMessage> GetProjects([FromBody] IssueRequestDTO issueRequest)
        {
            try
            {
                string token = string.Empty;
                var re = Request;
                var headers = re.Headers;

                if (headers.Contains("Authorization"))
                {
                    token = headers.GetValues("Authorization").First();
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }


                var projects = await ProjectsClient(issueRequest, token);
                return projects;

            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }

        private static async Task<HttpResponseMessage> JiraValidation(AuthResponse auth, string domain)
        {
            using (var client = new HttpClient())
            {
                var url = string.Format("https://{0}/rest/api/2/", domain);

                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth.token);

                var response = await client.GetAsync("mypermissions");

                return response;
            }
        }

        private string Encoder(string value)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(value);
            return System.Convert.ToBase64String(toEncodeAsBytes);
        }

        private async Task<List<Issue>> IssueClient(IssueRequestDTO issueReqeust, string token)
        {
            JiraResults results = null;
            List<Issue> issues = new List<Issue>();

            int total = -1;
            int startAt = issueReqeust.startAt;
            int maxResults = 1000;

            do
            {

                using (HttpClient client = new HttpClient())
                {


                    var url = string.Format("https://{0}/rest/api/2/", issueReqeust.domain);

                    client.BaseAddress = new Uri(url);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);

                    var data = new IssuesDTO();

                    data.jql = String.Format("project = {0} AND issuetype in ({1})", issueReqeust.project, issueReqeust.itemTypes);
                    data.startAt = startAt;
                    data.maxResults = maxResults;

                    var content = new StringContent(JsonConvert.SerializeObject(data).ToString(), Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync("search", content);
                    if (response.IsSuccessStatusCode)
                    {
                        results = await response.Content.ReadAsAsync<JiraResults>();
                        total = results.total;
                        startAt = startAt + maxResults;
                        issues = issues.Concat(results.issues).ToList();
                    }
                }

            } while (total >= startAt);


            return issues;
        }

        private async Task<HttpResponseMessage> ProjectsClient(IssueRequestDTO issueRequest, string token)
        {
            using (HttpClient client = new HttpClient())
            {


                var url = string.Format("https://{0}/rest/api/2/", issueRequest.domain);

                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);

                HttpResponseMessage response = await client.GetAsync("project");

                return response;
            }
        }
    }
}
