using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace NeptuneSkillImporter.Models
{
    public class JobPost
    {
        [JsonProperty("Title")]
        public string Header { get; set; }
        public List<string> Keywords { get; set; }

        [JsonProperty("FullJobPost")]
        public string Body { get; set; }

        public JobPost(string header, string body)
        {
            Header = header.ToLower();
            Body = body.ToLower();
            Keywords = new List<string>();
        }

        public override bool Equals(object obj)
        {
            var jobPost = (JobPost)obj;

            return this.Header == jobPost.Header && this.Body == jobPost.Body
                && this.Keywords.Count == jobPost.Keywords.Count && this.Keywords.All(jobPost.Keywords.Contains);
        }
    }
}