using Gremlin.Net.Driver;
using NeptuneSkillImporter.Database;
using Xunit;
using System.Collections.Generic;
using NeptuneSkillImporter.Models;
using System.Linq;

namespace NeptuneSkillImporterTests
{
    public class GremlinTests
    {
        [Theory]
        [InlineData("localhost", 8182)]
        [InlineData("1.2.3.4", 5678)]
        public void ShouldBuildCorrectUri(string host, int port)
        {
            var gremlinServer = new GremlinServer(host, port);

            var uri = gremlinServer.Uri;

            Assert.Equal($"ws://{host}:{port}/gremlin", uri.AbsoluteUri);
        }

        [Theory]
        [InlineData("localhost", 8182)]
        public void NoNodesTest(string host, int port)
        {
            var gremlinDB = new GremlinDB(host, port);

            gremlinDB.Drop();

            Assert.Equal(0, gremlinDB.CountNodes());
        }

        [Theory]
        [InlineData("localhost", 8182)]
        public void InsertNodesTest(string host, int port)
        {
            var gremlinDB = new GremlinDB(host, port);
            gremlinDB.Drop();

            var skills = new List<Skill>
            {
                new Skill() {Name = ".net", Category = "platforms"},
                new Skill() {Name = "c#", Category = "languages"},
                new Skill() {Name = "java", Category = "languages"},
                new Skill() {Name = "maven", Category = "development tools"},
                new Skill() {Name = "junit", Category = "development tools"},
                new Skill() {Name = "jenkins", Category = "development tools"},
                new Skill() {Name = "eclipse", Category = "development tools"}
            };

            gremlinDB.InsertNodes(skills);

            Assert.Equal(7, gremlinDB.CountNodes());

            gremlinDB.Drop();

            Assert.Equal(0, gremlinDB.CountNodes());
        }

        [Theory]
        [InlineData("localhost", 8182)]
        public void InsertEdgesTest(string host, int port)
        {
            var gremlinDB = new GremlinDB(host, port);
            gremlinDB.Drop();

            var skills = new List<Skill>
            {
                new Skill() {Name = "css", Category = "languages", Weight = 10},
                new Skill() {Name = "html", Category = "languages", Weight = 10},
                new Skill() {Name = "java", Category = "languages", Weight = 1},
                new Skill() {Name = "javascript", Category = "languages", Weight = 10},
                new Skill() {Name = "spring", Category = "technologies", Weight = 1},
                new Skill() {Name = "spring boot", Category = "technologies", Weight = 1}
            };

            gremlinDB.InsertNodes(skills);

            var jobPostSkills1 = new List<Skill>()
            {
                new Skill() {Name = "css", Category = "languages", Weight = 10},
                new Skill() {Name = "html", Category = "languages", Weight = 10},
                new Skill() {Name = "javascript", Category = "languages", Weight = 10}
            };

            var jobPostSkills2 = new List<Skill>()
            {
                new Skill() {Name = "java", Category = "languages", Weight = 1},
                new Skill() {Name = "spring boot", Category = "technologies", Weight = 1}
            };

            var processedJobPostsSkills = new List<ICollection<Skill>>
            {
                jobPostSkills1,
                jobPostSkills2
            };

            gremlinDB.InsertEdges(processedJobPostsSkills);

            // Total count (includes bidirectional edges)
            Assert.Equal(8, gremlinDB.CountEdges());
        }

        [Theory]
        [InlineData("localhost", 8182)]
        public void GetRelatedSkillsTest(string host, int port)
        {
            var gremlinDB = new GremlinDB(host, port);
            gremlinDB.Drop();

            var skills = new List<Skill>
            {
                new Skill() {Name = "css", Category = "languages", Weight = 10},
                new Skill() {Name = "html", Category = "languages", Weight = 10},
                new Skill() {Name = "java", Category = "languages", Weight = 1},
                new Skill() {Name = "javascript", Category = "languages", Weight = 10},
                new Skill() {Name = "spring", Category = "technologies", Weight = 1},
                new Skill() {Name = "spring boot", Category = "technologies", Weight = 1}
            };

            // Insert nodes ACT
            gremlinDB.InsertNodes(skills);

            var jobPostSkills1 = new List<Skill>()
            {
                new Skill() {Name = "css", Category = "languages", Weight = 10},
                new Skill() {Name = "html", Category = "languages", Weight = 10},
                new Skill() {Name = "javascript", Category = "languages", Weight = 10}
            };

            var jobPostSkills2 = new List<Skill>()
            {
                new Skill() {Name = "java", Category = "languages", Weight = 1},
                new Skill() {Name = "spring boot", Category = "technologies", Weight = 1}
            };

            var jobPostSkills3 = new List<Skill>()
            {
                new Skill() {Name = "css", Category = "languages", Weight = 10},
                new Skill() {Name = "javascript", Category = "languages", Weight = 10}
            };

            var processedJobPostsSkills = new List<ICollection<Skill>>
            {
                jobPostSkills1,
                jobPostSkills2,
                jobPostSkills3
            };

            // Insert edges ACT
            gremlinDB.InsertEdges(processedJobPostsSkills);

            // Related skills ACT
            var relatedSkills = gremlinDB.GetRelatedSkills("javascript", 10);
            var relatedSkillsNames = relatedSkills.Select(x => x.Name);

            Assert.Equal(2, relatedSkills.Count);
            Assert.Contains("css", relatedSkillsNames);
            Assert.Contains("html", relatedSkillsNames);
            Assert.True(relatedSkills.Find(skill => skill.Name == "css").Weight == 20);
        }
    }
}