using System.Collections.Generic;

namespace SkillRecommendationApp.Models
{
    public class Skill
    {
        private string _name;
        private string _category;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value.ToLower().Trim();
            }
        }

        public string Category
        {
            get
            {
                return _category;
            }
            set
            {
                _category = value.ToLower().Trim();
            }
        }

        public int Weight { get; set; }
    }
}