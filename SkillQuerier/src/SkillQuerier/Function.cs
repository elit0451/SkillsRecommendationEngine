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
                _db = new GremlinDB("tf-20200508130339734800000002.cjpaettbkbiu.eu-west-1.neptune.amazonaws.com");
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
            JObject bodyObj = JObject.Parse(request.Body);
            int limit = bodyObj.SelectToken("limit").Value<int>();
            string skillName = bodyObj.SelectToken("skillName").Value<string>();

            List<Skill> skills = _db.GetRelatedSkills(skillName, limit);

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
