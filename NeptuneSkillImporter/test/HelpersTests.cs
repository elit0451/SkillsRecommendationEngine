using System.Collections.Generic;
using NeptuneSkillImporter.Helpers;
using NeptuneSkillImporter.Models;
using Xunit;

namespace NeptuneSkillImporterTests
{
    public class HelpersTests
    {
        [Fact]
        public void JobPostRepoAddTest()
        {
            var jobPost1 = new JobPost("Best Job for you", "Full stack developer is needed");
            var jobPost2 = new JobPost("Best Job", ".Net developer is needed");
            var jobPosts = new List<JobPost>
            {
                jobPost1,
                jobPost2
            };

            Assert.Equal(0, JobPostRepo.Get().Count);

            JobPostRepo.Add(jobPosts);

            Assert.Equal(2, JobPostRepo.Get().Count);
        }

        [Fact]
        public void JobPostProcessorTest()
        {
            var simpleJobPost = new JobPost("Lead .NET Developer", "Many years of experience in C#");
            var processedJobPost = new JobPost("Senior in-house Jenkins lead", "Apply here")
            {
                Keywords = new List<string>() { "junit", "java", "maven" }
            };

            var jobPosts = new List<JobPost>
            {
                simpleJobPost,
                processedJobPost
            };

            var skills = new List<Skill>
            {
                new Skill() {Name = ".net"},
                new Skill() {Name = "c#"},
                new Skill() {Name = "java"},
                new Skill() {Name = "maven"},
                new Skill() {Name = "junit"},
                new Skill() {Name = "jenkins"},
                new Skill() {Name = "eclipse"}
            };

            var jobPostProcessor = new JobPostProcessor();
            var processedSkills = (List<ICollection<Skill>>)jobPostProcessor.ProcessJobPosts(skills, jobPosts);

            var simpleJobPostSkills = processedSkills[0];
            var processedJobPostSkills = processedSkills[1];

            Assert.True(simpleJobPostSkills.Count == 2);
            Assert.True(processedJobPostSkills.Count == 3);

            foreach (var skill in simpleJobPostSkills)
                Assert.True(skill.Weight == 1);

            foreach (var skill in processedJobPostSkills)
            {
                Assert.True(skill.Weight == 10);
                Assert.True(skill.Name != "jenkins");
            }
        }
    }
}