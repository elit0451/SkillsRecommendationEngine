using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3.Model;
using NeptuneSkillImporter.Database;
using NeptuneSkillImporter.Models;
using Xunit;

namespace NeptuneSkillImporterTests
{
    public class S3ConnectorTests
    {
        internal string bucketName = "jobposts-scraped-tests";
        internal RegionEndpoint region = RegionEndpoint.EUWest1;

        [Fact]
        public void S3ConnectorCtorTest()
        {
            var s3Connector = new S3Connector(region, bucketName);

            Assert.False(s3Connector is null);
        }

        [Fact]
        public async Task GetNoFilesTest()
        {
            var s3Connector = new S3Connector(region, bucketName);

            ICollection<S3Object> jobPostsKeys = await s3Connector.GetFiles(to: new DateTime(2020, 05, 08));

            Assert.True(jobPostsKeys.Count == 0);
        }

        [Fact]
        public async Task GetAllFilesTest()
        {
            var s3Connector = new S3Connector(region, bucketName);

            ICollection<S3Object> jobPostsKeys = await s3Connector.GetFiles();

            // there are 2 folders with 1 file each
            Assert.True(jobPostsKeys.Count == 2);
        }

        [Fact]
        public async Task GetSingleFileTest()
        {
            var s3Connector = new S3Connector(region, bucketName);

            ICollection<S3Object> jobPostsKeys = await s3Connector.GetFiles(from: new DateTime(2020, 05, 13));

            // there is just one file from this date in S3
            Assert.True(jobPostsKeys.Count == 1);
        }

        [Fact]
        public async Task GetFileContentsTest()
        {
            var s3Connector = new S3Connector(region, bucketName);

            ICollection<S3Object> jobPostsKeys = await s3Connector.GetFiles();
            ICollection<JobPost> jobPostsObjs = await s3Connector.GetFileContents(jobPostsKeys);

            Assert.True(jobPostsObjs.Count == 2);

            var javaJobPost = new JobPost("Senior Java Developer", "Start as soon as possible");
            javaJobPost.Keywords.Add("java");
            javaJobPost.Keywords.Add("maven");
            javaJobPost.Keywords.Add("junit");

            var cSharpJobPost = new JobPost("Senior Software Developer", "Contact us as soon as possible");
            cSharpJobPost.Keywords.Add("c#");
            cSharpJobPost.Keywords.Add("dotnet");
            cSharpJobPost.Keywords.Add("full stack");

            Assert.Equal(javaJobPost, jobPostsObjs.First(jobPost => jobPost.Header == javaJobPost.Header));
            Assert.Equal(cSharpJobPost, jobPostsObjs.First(jobPost => jobPost.Header == cSharpJobPost.Header));
        }
    }
}
