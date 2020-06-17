using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Remote;
using Gremlin.Net.Process.Traversal;
using static Gremlin.Net.Process.Traversal.AnonymousTraversalSource;

namespace NeptuneSkillImporter.Database
{
    public class GremlinConnector
    {
        private readonly GremlinClient _gremlinClient;

        public GremlinConnector(string endpoint, int port, bool ssl)
        {
            var gremlinServer = new GremlinServer(endpoint, port, ssl);
            _gremlinClient = new GremlinClient(gremlinServer);
        }

        public GraphTraversalSource GetGraph()
        {
            return Traversal().WithRemote(new DriverRemoteConnection(_gremlinClient));
        }
    }
}