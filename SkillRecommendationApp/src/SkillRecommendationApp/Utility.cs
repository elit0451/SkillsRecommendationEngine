using System.Collections.Specialized;
using System.Web;
using Newtonsoft.Json.Linq;

namespace SkillRecommendationApp
{
    public class Utility
    {
        public static JObject GetBodyJObject(string body)
        {
            NameValueCollection qscoll = GetParameterCollection(body);

            var jsonObj = new JObject
            {
                { "command", qscoll["command"] },
                { "text", qscoll["text"] }
            };

            //Convert the JSON string from the key payload into a JObject
            return jsonObj;
        }

        private static NameValueCollection GetParameterCollection(string queryString)
        {
            string base64Decoded;

            try
            {
                //Convert the base64 encoded queryString into a string
                string base64Encoded = queryString;
                byte[] data = System.Convert.FromBase64String(base64Encoded);
                base64Decoded = System.Text.Encoding.ASCII.GetString(data);
            }
            catch
            {
                base64Decoded = queryString;
            }

            //Convert the resulting query string into a collection separated by keys
            return HttpUtility.ParseQueryString(base64Decoded);
        }
    }
}