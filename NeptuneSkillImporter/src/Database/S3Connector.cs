using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using NeptuneSkillImporter.Models;
using Newtonsoft.Json;

namespace NeptuneSkillImporter.Database
{
    public class S3Connector
    {
        private readonly AmazonS3Client _client;
        private readonly string _bucketName;
        public S3Connector(RegionEndpoint region, string bucketName, string accessKey = null, string secretAccessKey = null)
        {
            if (accessKey is null || secretAccessKey is null)
                _client = new AmazonS3Client(region);
            else
                _client = new AmazonS3Client(accessKey, secretAccessKey, region);

            _bucketName = bucketName;
        }

        public async Task<ICollection<S3Object>> GetFiles(DateTime? from = null, DateTime? to = null)
        {
            var jobPosts = new List<S3Object>();

            if (from is null)
                from = new DateTime();
            if (to is null)
                to = DateTime.Now;

            try
            {
                // with the Delimiter we will filter and only get the folders inside the specified bucket
                ListObjectsV2Request request = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Delimiter = "/"
                };

                ListObjectsV2Response response;

                do
                {
                    response = await _client.ListObjectsV2Async(request);
                    foreach (string folder in response.CommonPrefixes)
                    {
                        var entryDate = DateTime.Parse(folder.Substring(0, folder.Length - 1));
                        if (entryDate.Date >= from?.Date && entryDate.Date <= to?.Date)
                        {
                            ListObjectsV2Request innerRequest = new ListObjectsV2Request
                            {
                                BucketName = _bucketName,
                                Prefix = folder
                            };

                            ListObjectsV2Response innerResponse;

                            do
                            {
                                innerResponse = await _client.ListObjectsV2Async(innerRequest);
                                jobPosts.AddRange(innerResponse.S3Objects);

                                innerRequest.ContinuationToken = response.NextContinuationToken;
                            }
                            while (innerResponse.IsTruncated);
                        }
                    }
                    request.ContinuationToken = response.NextContinuationToken;
                }
                while (response.IsTruncated);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            // skip the folders name (like yyyy-mm-dd/)
            return jobPosts.Where(x => x.Key.Length > 11).ToList();
        }

        public async Task<ICollection<JobPost>> GetFileContents(ICollection<S3Object> jobPostKeys)
        {
            var jobPostObjs = new List<JobPost>();

            try
            {
                foreach (var jobPostKey in jobPostKeys)
                {
                    GetObjectRequest request = new GetObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = jobPostKey.Key
                    };

                    using (GetObjectResponse response = await _client.GetObjectAsync(request))
                    using (Stream responseStream = response.ResponseStream)
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        jobPostObjs.Add(JsonConvert.DeserializeObject<JobPost>(reader.ReadToEnd()));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return jobPostObjs;
        }
    }
}