using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using NeptuneSkillImporter.Database;
using NeptuneSkillImporter.Helpers;
using NeptuneSkillImporter.Models;
using Amazon.S3;
using Amazon;
using Amazon.S3.Model;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;

namespace NeptuneSkillImporter
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            const string bucketName = "jobposts-scraped";
            var s3Connector = new S3Connector(RegionEndpoint.EUWest1, bucketName);
            ICollection<S3Object> jobPostsKeys = await s3Connector.GetFiles();
            ICollection<JobPost> jobPostsObjs = await s3Connector.GetFileContents(jobPostsKeys);

            JobPostRepo.Add(jobPostsObjs);
            Console.WriteLine("Starting the RUN()");
            if (args.Length > 0 && !string.IsNullOrEmpty(args[0]) && !string.IsNullOrEmpty(args[1]))
                new Program().Run(args[0], Convert.ToInt32(args[1]));
            else
                new Program().Run();
        }

        public void Run(string endpoint = "localhost", int port = 8182)
        {
            try
            {
                var skills = new List<Skill>();

                // This uses the default Neptune and Gremlin port, 8182
                var gremlinDB = new GremlinDB(endpoint, port);

                // Drop entire DB
                gremlinDB.Drop();

                // get job posts
                var jobPosts = JobPostRepo.Get();
                //var jobPosts = JobPostRepo.GetJobPosts();

                // load csv data for skills
                skills = LoadDataToMemory();
                // skills into DB
                Stopwatch stopWatch = new Stopwatch();
                Stopwatch stopWatch1 = new Stopwatch();
                stopWatch.Start();
                stopWatch1.Start();
                gremlinDB.InsertNodes(skills);
                Console.WriteLine(stopWatch.Elapsed);
                Console.WriteLine("\tEND inserting NODES\n");

                // edges into DB
                IJobPostProcessor jobPostProcessor = new JobPostProcessor();
                Console.WriteLine("Start processing JOB POSTS");
                stopWatch.Restart();
                var jobPostsSkills = jobPostProcessor.ProcessJobPosts(skills, jobPosts);
                Console.WriteLine(stopWatch.Elapsed);
                Console.WriteLine("\tEND iprocessing JOB POSTS\n");

                Console.WriteLine("Start inserting EDGES");
                stopWatch.Restart();
                gremlinDB.InsertEdges(jobPostsSkills);
                Console.WriteLine(stopWatch.Elapsed);
                Console.WriteLine("\tEND inserting EDGES\n");

                // get related skills
                const string skillNameForSearch = "c#";
                const int limit = 10;

                Console.WriteLine("Start RELATED skills");
                var relatedSkills = gremlinDB.GetRelatedSkills(skillNameForSearch, limit);
                Console.WriteLine(stopWatch1.Elapsed);

                Console.WriteLine($"Top {limit} skills related to {skillNameForSearch}:\n");
                foreach (var skill in relatedSkills)
                    Console.WriteLine($"Name: {skill.Name}, Category: {skill.Category}, Weight: {skill.Weight}");

                Console.WriteLine("\n\nTotal number of skills: {0}", gremlinDB.CountNodes());

                Console.WriteLine("Finished");
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e);
            }
        }

        public List<Skill> LoadDataToMemory()
        {
            //TODO: add the file to S3 bucket and get it URI
            const string filePath = "./data/skills-dataset.csv";
            List<Skill> records = null;

            using (StreamReader sr = new StreamReader(filePath, Encoding.UTF8))
            {
                using (var csv = new CsvReader(sr, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.BadDataFound = null;
                    csv.Configuration.HasHeaderRecord = true;

                    records = csv.GetRecords<Skill>().GroupBy(x => x.Name).Select(x => x.First()).ToList();
                }
            }

            return records;
        }
    }
}
