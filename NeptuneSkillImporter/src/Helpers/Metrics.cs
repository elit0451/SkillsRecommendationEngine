using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;

namespace NeptuneSkillImporter.Helpers
{
    public static class Metrics
    {
        static private List<MetricDatum> _data;
        private static readonly IAmazonCloudWatch _amazonCloudWatch;

        static Metrics()
        {
            _data = new List<MetricDatum>();
            _amazonCloudWatch = new AmazonCloudWatchClient(RegionEndpoint.EUWest1);
        }

        public static void AddData(MetricDatum data)
        {
            _data.Add(data);
        }

        public static async Task CommitDataAsync()
        {
            if (_data.Count == 0)
                return;
            await _amazonCloudWatch.PutMetricDataAsync(new PutMetricDataRequest
            {
                Namespace = "Slack App",
                MetricData = _data
            });
            _data = new List<MetricDatum>();
        }
    }
}