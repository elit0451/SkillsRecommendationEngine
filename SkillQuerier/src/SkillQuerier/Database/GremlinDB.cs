using System;
using System.Collections.Generic;
using Gremlin.Net.Process.Traversal;
using Gremlin.Net.Structure;
using Newtonsoft.Json;
using SkillQuerier.Models;

namespace SkillQuerier.Database
{
    public class GremlinDB
    {
        private readonly GremlinConnector _gremlinConnector;
        private readonly GraphTraversalSource _graph;

        public GremlinDB(string endpoint, int port = 8182)
        {
            _gremlinConnector = new GremlinConnector(endpoint, port);
            _graph = _gremlinConnector.GetGraph();
        }

        public List<Skill> GetRelatedSkills(string skillName, int limit)
        {
            List<Skill> jsonRelatedSkills;

            try
            {
                // find vertices with the same skill name from the graph
                var v1 = _graph.V().HasLabel("skill").Has("name", skillName).Next();

                // find top {limit} related skills 
                var relatedSkills = _graph.V(v1).OutE().As("e").Order().By("count", Order.Decr).InV().Limit<int>(limit)
                    .Project<object>("name", "category", "weight").By("name").By("category").By(__.Select<object>("e")
                    .Values<object>("count")).ToList();

                jsonRelatedSkills = JsonConvert.DeserializeObject<List<Skill>>(JsonConvert.SerializeObject(relatedSkills));
            }
            catch(Exception e)
            {
                jsonRelatedSkills = new List<Skill>();
            }

            return jsonRelatedSkills;
        }
    }
}