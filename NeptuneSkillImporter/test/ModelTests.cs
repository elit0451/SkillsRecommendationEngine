using System;
using NeptuneSkillImporter.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace NeptuneSkillImporterTests
{
    public class ModelTests
    {
        [Fact]
        public void JobPostObjFromCtorTest()
        {
            Console.WriteLine("ModelTests - JobPostObjFromCtorTest");
            const string title = "Best Job";
            const string body = ".Net developer is needed";

            var jobPost = new JobPost(title, body);

            Assert.Equal(title.ToLower(), jobPost.Header);
            Assert.Equal(body.ToLower(), jobPost.Body);
            Assert.True(jobPost.Keywords.Count == 0);
            
            Console.WriteLine(jobPost.Keywords.Count);
        }

        [Fact]
        public void JobPostObjFromCtorWithKeywordsTest()
        {
            const string title = "Best Job";
            const string body = ".Net developer is needed";

            var jobPost = new JobPost(title, body);
            jobPost.Keywords.Add("c#");
            jobPost.Keywords.Add("dotnet core");

            Assert.Equal(title.ToLower(), jobPost.Header);
            Assert.Equal(body.ToLower(), jobPost.Body);
            Assert.True(jobPost.Keywords.Count == 2);
        }

        [Fact]
        public void JobPostObjFromJSONTest()
        {
            const string jobPostJsonString = "{\"Title\":\"Senior Software Developer\",\"FullJobPost\":\"Contact us as soon as possible\"}";

            var jobPost = JsonConvert.DeserializeObject<JobPost>(jobPostJsonString);

            Assert.Equal("senior software developer", jobPost.Header);
            Assert.Equal("contact us as soon as possible", jobPost.Body);
            Assert.True(jobPost.Keywords.Count == 0);

            // with keywords
            const string jobPostJsonStringWithKeywords = "{\"Title\":\"Senior Software Developer\",\"FullJobPost\":\"Contact us as soon as possible\",\"Keywords\":[\"c#\",\"dotnet\",\"full stack\"]}";

            var jobPostWithKeywords = JsonConvert.DeserializeObject<JobPost>(jobPostJsonStringWithKeywords);

            Assert.Equal("senior software developer", jobPostWithKeywords.Header);
            Assert.Equal("contact us as soon as possible", jobPostWithKeywords.Body);
            Assert.True(jobPostWithKeywords.Keywords.Count == 3);
        }

        [Fact]
        public void SkillEmptyObjTest()
        {
            var skill = new Skill();

            Assert.False(skill is null);
            Assert.True(string.IsNullOrEmpty(skill.Name));
            Assert.True(string.IsNullOrEmpty(skill.Category));
            Assert.True(skill.Weight == 0);
        }

        [Fact]
        public void SkillObjTest()
        {
            var skill = new Skill
            {
                Name = "C# ",
                Category = " Languages",
                Weight = 10
            };

            Assert.Equal("c#", skill.Name);
            Assert.Equal("languages", skill.Category);
            Assert.True(skill.Weight == 10);
        }
    }
}
