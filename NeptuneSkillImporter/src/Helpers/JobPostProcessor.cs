using System.Collections.Generic;
using System.Text.RegularExpressions;
using NeptuneSkillImporter.Models;

namespace NeptuneSkillImporter.Helpers
{
    public class JobPostProcessor : IJobPostProcessor
    {
        public ICollection<ICollection<Skill>> ProcessJobPosts(ICollection<Skill> skills, ICollection<JobPost> jobPosts)
        {
            var processedSkills = new List<ICollection<Skill>>();

            var skillsRegex = new Dictionary<string, Regex>();
            const RegexOptions regexOptions = RegexOptions.Compiled | RegexOptions.Multiline;

            foreach (var skill in skills)
            {
                var skillName = Regex.Escape(skill.Name);
                var pattern = $"([^A-Za-z]|^)({skillName})([^A-Za-z]|$)";
                skillsRegex.Add(skill.Name, new Regex(pattern, regexOptions));
            }

            foreach (var jobPost in jobPosts)
            {
                var foundSkills = new List<Skill>();

                if (jobPost.Keywords.Count != 0)
                {
                    foreach (var keyword in jobPost.Keywords)
                    {
                        foundSkills.Add(new Skill()
                        {
                            Name = keyword,
                            Weight = 10
                        });
                    }
                }
                else
                {
                    foreach (var skill in skills)
                    {
                        if (skillsRegex[skill.Name].IsMatch(jobPost.Header) || skillsRegex[skill.Name].IsMatch(jobPost.Body))
                        {
                            skill.Weight = 1;
                            foundSkills.Add(skill);
                        }
                    }
                }

                processedSkills.Add(foundSkills);
            }

            return processedSkills;
        }
    }
}