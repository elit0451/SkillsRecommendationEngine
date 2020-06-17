using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using SlackMessageBuilder;
using SkillRecommendationApp.Models;
using Newtonsoft.Json.Linq;
using Amazon.Lambda;
using Amazon;
using Amazon.Lambda.Model;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SkillRecommendationApp
{
    public class Functions
    {
        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
        }


        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The list of blogs</returns>
        public APIGatewayProxyResponse Get(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Get Request\n");

            var requestBody = Utility.GetBodyJObject(request.Body);
            var command = (string)requestBody.SelectToken("command");
            var commandParams = new string[1];
            var response = new APIGatewayProxyResponse();

            switch (command.ToLower().Trim())
            {
                case "/skill":
                    var commandText = (string)requestBody.SelectToken("text");

                    if (!string.IsNullOrEmpty(commandText))
                    {
                        commandParams = commandText.Trim().Split(" ");
                        var skillName = commandParams[0];
                        var amount = Convert.ToInt32(commandParams[1]);

                        var queryObj = new JObject
                        {
                            { "limit", amount },
                            { "skillName", skillName }
                        };

                        var httpClient = new HttpClient();
                        HttpResponseMessage httpResponse = httpClient.PostAsJsonAsync("https://7dq5d3f0yb.execute-api.eu-west-1.amazonaws.com/production/relatedskills", queryObj).Result;

                        var payload = httpResponse.Content.ReadAsJsonAsync<JArray>().Result;

                        var skills = JsonConvert.DeserializeObject<List<Skill>>(payload.ToString());

                        var slackBuilder = new BlocksBuilder();
                        slackBuilder.AddBlock(new Section(new Text(" ")));
                        slackBuilder.AddBlock(new Section(new Text($"`Skill: {skillName.ToUpper()}`", "mrkdwn")));
                        slackBuilder.AddBlock(new Divider());

                        var slackSection = new Section();

                        if (skills.Count > 0)
                        {
                            for (int i = 0; i < skills.Count; i++)
                            {
                                if (i % 10 == 0 && i != 0)
                                {
                                    slackBuilder.AddBlock(slackSection);
                                    slackSection = new Section();
                                }

                                slackSection.AddField($"```{skills[i].Name}```", "mrkdwn");
                            }

                            slackBuilder.AddBlock(slackSection);
                        }
                        else
                        {
                            slackBuilder.AddBlock(new Section(new Text("*No related skills found*", "mrkdwn")));
                        }

                        var slackPayload = slackBuilder.GetJObject();

                        context.Logger.LogLine(slackPayload.ToString());

                        response = new APIGatewayProxyResponse
                        {
                            StatusCode = (int)HttpStatusCode.OK,
                            Body = slackPayload.ToString(),
                            Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                        };
                    }
                    // TODO: return msg that skill doesn't exist
                    break;
            }

            return response;
        }
    }
}
