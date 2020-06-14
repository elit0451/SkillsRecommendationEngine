using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Remote;
using Gremlin.Net.Process.Traversal;
using static Gremlin.Net.Process.Traversal.AnonymousTraversalSource;

namespace SkillQuerier.Database
{
    public class GremlinConnector
    {
        private readonly GremlinClient _gremlinClient;

        public GremlinConnector(string endpoint, int port)
        {
            var gremlinServer = new GremlinServer(endpoint, port, true);
            _gremlinClient = new GremlinClient(gremlinServer);
        }

        public GraphTraversalSource GetGraph()
        {
            return Traversal().WithRemote(new DriverRemoteConnection(_gremlinClient));
        }

        public GremlinClient GetClient()
        {
            return _gremlinClient;
        }
    }
}