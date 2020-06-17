using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Linq;
using SkillQuerier.Database;
using SkillQuerier.Models;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SkillQuerier
{
    public class Functions
    {
        private readonly GremlinDB _db;
        public Functions()
        {
            try
            {
                _db = new GremlinDB(Environment.GetEnvironmentVariable("NEPTUNE_ENDPOINT"));
            }
            catch (Exception)
            {
                _db = null;
            }
        }

        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The list of blogs</returns>
        public APIGatewayProxyResponse Get(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine(Environment.GetEnvironmentVariable("NEPTUNE_ENDPOINT"));
            JObject bodyObj = JObject.Parse(request.Body);
            int limit = bodyObj.SelectToken("limit").Value<int>();
            string skillName = bodyObj.SelectToken("skillName").Value<string>();
            context.Logger.LogLine("HERE!");
            List<Skill> skills = _db.GetRelatedSkills(skillName, limit);
            context.Logger.LogLine("Here now");
            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(skills),
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            };

            return response;
        }
    }
}
