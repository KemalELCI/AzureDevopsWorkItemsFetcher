using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AzureWorkItemsFetcher
{
    public class AzureRestApiHelper
    {
        string _personalAccessToken;
        string _organization;
        string _project;

        public AzureRestApiHelper(string personalAccessToken, string organization, string project)
        {
            _personalAccessToken = personalAccessToken;
            _organization = organization;
            _project = project;
        }

        public async Task<WorkItemsDetailResult> GetWorkItemsDetail(string[] ids)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", "", _personalAccessToken))));

                using (HttpResponseMessage response = await client.GetAsync(
                             $"https://dev.azure.com/{_organization}/{_project}/_apis/wit/workitems?ids={string.Join(',', ids)}&api-version=5.1"))
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<WorkItemsDetailResult>(responseBody);
                }
            }
        }

        public async Task<WorkItem[]> GetCurrentIterationPBIs()
        {
            var query = "Select [System.Id], [System.Title], [System.State] From WorkItems Where [System.WorkItemType] = 'Product Backlog Item' "
                + "AND [State] <> 'Closed' AND [State] <> 'Removed' AND [Iteration Path] = @CurrentIteration order by [Microsoft.VSTS.Common.Priority] asc, [System.CreatedDate] desc";

            var result = await MakeWITQuery<WITResult>(query);
            return result.workItems;
        }

        public async Task<WorkItemRelation[]> GetChildWorkItemsFromParentWorkItemId(string Id)
        {
            var query = "select * from WorkItemLinks where ([System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Forward') "
                + $" and (Source.[System.Id] = {Id}) order by [System.Id] mode (ReturnMatchingChildren)";

            var result = await MakeWITQuery<WITRelationsResult>(query);
            return result.workItemRelations;
        }

        public async Task<T> MakeWITQuery<T>(string query)
        {
            var queryObj = new
            {
                query
            };

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", "", _personalAccessToken))));

                using (HttpResponseMessage response = await client.PostAsync(
                             $"https://dev.azure.com/{_organization}/{_project}/_apis/wit/wiql?api-version=5.1",
                             new StringContent(JsonConvert.SerializeObject(queryObj), Encoding.UTF8, "application/json")))
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(responseBody);
                }

            }
        }

        public async void GetProjects()
        {

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", "", _personalAccessToken))));

                using (HttpResponseMessage response = await client.GetAsync(
                             $"https://dev.azure.com/{_organization}/_apis/projects"))
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                }
            }
        }
    }

    public class WITResult
    {
        public WorkItem[] workItems { get; set; }
    }

    public class WITRelationsResult
    {
        public WorkItemRelation[] workItemRelations { get; set; }
    }

    public class WorkItem
    {
        public string id { get; set; }
        public string url { get; set; }

        public WorkItemDetail fields { get; set; }
    }

    public class WorkItemRelation
    {
        public string rel { get; set; }
        public WorkItem source { get; set; }
        public WorkItem target { get; set; }
    }

    public class WorkItemDetail
    {
        [JsonProperty(PropertyName = "System.Title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "Microsoft.VSTS.Common.Priority")]
        public string Priority { get; set; }
        [JsonProperty(PropertyName = "Custom.TShirtSize")]
        public string TShirtSize { get; set; }
        [JsonProperty(PropertyName = "System.State")]
        public string State { get; set; }
        [JsonProperty(PropertyName = "System.WorkItemType")]
        public string WorkItemType { get; set; }
        [JsonProperty(PropertyName = "System.IterationPath")]
        public string IterationPath { get; set; }

        [JsonProperty(PropertyName = "System.Tags")]
        public string Tags { get; set; }

        public string[] TagsList
        {
            get
            {
                return Tags?.Split(';');
            }
        }
    }


    public class WorkItemsDetailResult
    {
        public int count { get; set; }
        public WorkItem[] value { get; set; }
    }
}
