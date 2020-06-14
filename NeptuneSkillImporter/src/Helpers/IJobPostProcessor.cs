using System.Collections.Generic;
using NeptuneSkillImporter.Models;

namespace NeptuneSkillImporter.Helpers
{
    public interface IJobPostProcessor
    {
        ICollection<ICollection<Skill>> ProcessJobPosts(ICollection<Skill> skills, ICollection<JobPost> jobPosts);
    }
}