using System.Collections.Generic;
using CsvHelper.Configuration.Attributes;

namespace NeptuneSkillImporter.Models
{
    public class Skill
    {
        private string _name;
        private string _category;

        [Index(1)]
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

        [Index(2)]
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

        [Ignore]
        public int Weight { get; set; }
    }
}