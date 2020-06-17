using System;
using System.Collections.Generic;
using System.Linq;
using Gremlin.Net.Process.Traversal;
using Gremlin.Net.Structure;
using NeptuneSkillImporter.Models;
using Newtonsoft.Json;

namespace NeptuneSkillImporter.Database
{
    public class GremlinDB
    {
        private readonly GremlinConnector _gremlinConnector;
        private readonly GraphTraversalSource _graph;
        private readonly Dictionary<string, Vertex> _nodes;

        public GremlinDB(string endpoint, int port, bool ssl = true)
        {
            _gremlinConnector = new GremlinConnector(endpoint, port, ssl);
            _graph = _gremlinConnector.GetGraph();

            _nodes = new Dictionary<string, Vertex>();
        }

        public void Drop()
        {
            _graph.V().Drop().Iterate();
        }

        public void InsertNodes(List<Skill> skills)
        {
            foreach (var skill in skills)
            {
                if (_nodes.ContainsKey(skill.Name))
                    continue;

                var node = _graph.V().Has("skill", "name", skill.Name).Fold().Coalesce<Vertex>(__.Unfold<Vertex>(), _graph.AddV("skill").Property("name", skill.Name).Property("category", skill.Category)).Next();
                _nodes.Add(skill.Name, node);
            }
        }
        public void InsertEdges(ICollection<ICollection<Skill>> jobPostsSkills)
        {
            foreach (var jobPostSkills in jobPostsSkills)
            {
                Skill[] skills = jobPostSkills.ToArray();
                //string[] skillsStreing = jobPostSkills.Select(x=>x.Name).ToArray();
                //Console.WriteLine("[{0}]", string.Join(", ", skillsStreing));

                for (int i = 0; i < skills.Length - 1; i++)
                {
                    for (int j = i + 1; j < skills.Length; j++)
                    {
                        // find vertices with the same skill name from the graph
                        var v1 = _graph.V().HasLabel("skill").Has("name", skills[i].Name).Next();
                        var v2 = _graph.V().HasLabel("skill").Has("name", skills[j].Name).Next();

                        // insert biderectional edges between 2 skills
                        // set initial edge weight count to 0
                        _graph.V(v2).As("v2").V(v1).As("v1").Not(__.Both("weight").Where(P.Eq("v2")))
                            .AddE("weight").Property("count", 0).From("v2").To("v1").OutV()
                            .AddE("weight").Property("count", 0).From("v1").To("v2").Iterate();

                        // increase edge weight count when 2 skills are found in the same job post
                        _graph.V(v1).BothE().Where(__.BothV().HasId(v2.Id))
                          .Property("count", __.Union<int>(__.Values<int>("count"), __.Constant(skills[i].Weight)).Sum<int>()).Next();
                    }
                }
            }
        }
        /*
        private async Task RunQueryAsync(GremlinClient gremlinClient)
         {
             var count = await gremlinClient.SubmitWithSingleResultAsync<long>("g.V().count().next()");

             Console.WriteLine("\n\nTotal number of skills: {0}", count);
         } */

        /*
                public async Task InsertEdgesAsync(ICollection<ICollection<Skill>> jobPostsSkills)
                {
                    var query = "g";

                    foreach (var jobPostSkills in jobPostsSkills)
                    {
                        Skill[] skills = jobPostSkills.ToArray();

                        for (int i = 0; i < skills.Length - 1; i++)
                        {
                            var v1 = _nodes[skills[i].Name].Id;
                            query += $".V({v1}).as('{skills[i].Name}')";


                            for (int j = i + 1; j < skills.Length; j++)
                            {
                                // find vertices with the same skill name from the graph
                                var v2 = _nodes[skills[j].Name].Id;

                                // insert biderectional edges between 2 skills
                                // set initial edge weight count to 0
                                query += $".V({v2}).as('{skills[j].Name}').not(__.both('weight').where(eq('{skills[i].Name}'))).addE('weight').property('count', 0).from('{skills[j].Name}').to('{skills[i].Name}').outV().addE('weight').property('count', 0).from('{skills[i].Name}').to('{skills[j].Name}')";

                                // increase edge weight count when 2 skills are found in the same job post
                                //_graph.V(v1).BothE().Where(__.BothV().HasId(v2.Id))
                                //  .Property("count", __.Union<int>(__.Values<int>("count"), __.Constant(skills[i].Weight)).Sum<int>()).Next();
                            }

                            var anotherQuery = query + ".iterate()";
                            await _gremlinConnector.GetClient().SubmitAsync(anotherQuery);
                            query = "g";
                        }
                    }
                }

        */
        public int CountNodes()
        {
            var count = _graph.V().Count().Next();

            return (int)count;
        }

        public int CountEdges()
        {
            var count = _graph.E().Count().Next();

            return (int)count;
        }

        public List<Skill> GetRelatedSkills(string skillName, int limit)
        {
            var jsonRelatedSkills = new List<Skill>();

            // find vertices with the same skill name from the graph
            var v1 = _graph.V().HasLabel("skill").Has("name", skillName).Next();

            try
            {
                // find top {limit} related skills 
                var relatedSkills = _graph.V(v1).OutE().As("e").Order().By("count", Order.Decr).InV().Limit<int>(limit)
                    .Project<object>("name", "category", "weight").By("name").By("category").By(__.Select<object>("e")
                    .Values<object>("count")).ToList();

                jsonRelatedSkills = JsonConvert.DeserializeObject<List<Skill>>(JsonConvert.SerializeObject(relatedSkills));
            }
            catch (Exception) { }

            return jsonRelatedSkills;
        }
    }
}