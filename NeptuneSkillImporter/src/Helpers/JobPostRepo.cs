using System.Collections.Generic;
using NeptuneSkillImporter.Models;

namespace NeptuneSkillImporter.Helpers
{
    public static class JobPostRepo
    {
        private static readonly List<JobPost> _jobPosts = new List<JobPost>();
        public static List<JobPost> GetJobPosts()
        {
            const string title1 = "Senior Software Developer";
            const string post1 = "Job reference number:\n32091\n\n\nSalary:\n.\n\n\nEmployment Type:\nPermanent\n\n\nCountry:\nFinland\n\n\nLocality:\nHelsinki\n\n\nPost Date:\n10-03-2020 10:54 AM\n\n\n\nApply\nJob Alert\n\n\nSibling is recruting for a global chemicals company in the centre of Helsinki We are looking for an experienced\xa0Software Developer & Architect, Digital Solutions, IT\xa0to take a leading role in providing new approaches and continuous improvements of customer facing IT solutions. You will be part of the Digital Solutions unit located in our HQ in Helsinki. You have an opportunity to work in the center of my clients Digital business development and operations from a software development point of view, ranging from technical work to architectural design and technological decisions. You will join an internal team of senior fellow coders to help you onboard in the beginning and solve issues in the future, in addition to a selected group of external experts to support you on your way. Take the unique opportunity to join the community of ambitious and experienced IT colleagues and active business stakeholders a make your own mark on a successfully developed, perfect software solution. As a Software Developer & Architect, you will have responsibility for the development, design and support of customer facing IoT & Analytics platform for connected customers and internal field services as well as for various customer facing web & mobile applications to different stakeholders. We run with DevOps mentality, focus on minimizing the amount of unnecessary overhead, so you have the time to make the perfect code. Going forward we offer a relatively large independency to balance your daily work between projects. We use esblished CI/CD Jenkins pipelines and other supporting tools by Atlassian (Jira, Bitbucket), defined code review and branching practices aiming to make your life easy to develop, test and deploy.  What we look for  Experience with following technologies: Java, C#, .NET Bonus for Vaadin, React, Node.js and Azure/Cloud experience At least 5 years of experience in relevant process area (Software/service development) Significant understanding and experience on establishing and running customer facing services Desire to do hands on work on solving technical challenges, testing the development increments Capability to work with internal and/or external customers with strong communication and influencing skills Ability to multi-task and coordinate multiple priorities Can-do attitude, independent and long-distance team working skills Fluency in spoken and written English Preferred minimum of Bachelor's degree in suitable field (e.g. Information Technology)  What we offer  Experienced teammates and co-workers Relaxed yet focused working environment Impact on decision regarding current and future products, tools and processes. Independency in defining your work/life -balance Competitive salary and benefits";

            const string title2 = "Lead .NET Developer - Copenhagen - In House";
            const string post2 = @"Lead Full-Stack .NET Developer - 60.000 Basic - Copenhagen\nIf you're looking for a new challenge, you've found your match.\nThis exciting position offers the successful candidate the opportunity to work on a complex systems in a niche market while growing their career and be a leading part of a dynamic, result-driven team who are passionate about what they do.\nThis position will have you based at the office, working on an in-house basis where technology product development is their core business.\nYou'll also enjoy perks such as:\n\npension\nbenefits\nbreakfast everyday\nexcellent coffee, cold drinks and treats in the office\ncontinuous training and development across all skill sets\nmodern office space\nestablished team with support and structure\nsome of the nicest people you'll ever work with\n\nWe work well with people who are passionate and logical thinkers, if you have the following experience, get in touch as soon as possible:\n\nMinimum 3-year IT-related degree or diploma\n5+ years of solid industry experience with C# and .NET\nStrong technical understanding of .NET framework, Win32 architecture and application design\nExperience in WPF and the MVVM design pattern\nExperience in .NET Core 2.0, EF Core 2.0, ASP.NET MVC with Razor and Angular/Typescript.\nSQL & relational database programming skills\nExperience in HTML, CSS and JavaScript development will be advantageous\nExperience in web back-end technologies (e.g. SOAP, REST) will be advantageous\nExperience in mobile development in Xamarin and/or NativeScript and exposure to Microsoft Azure would be advantageous\nExposure to the GIT version control system will be advantageous\nExposure to Scrum and Agile methodologies will be advantageous\nStrong analytical and logical problem-solving skills\n\nYour daily responsibilities will include:\n\nLead a Development team and direct those in their daily tasks whilst remaining hands-on\nFollowing Agile development methodologies\nContribute to the architecture, design, development, and maintenance of Web and Desktop applications using the C# language and the Microsoft .NET framework\nFollow best software engineering practices.\nExercise version control discipline to maintain source code.\n\nInterested? Call Eddie on 0044 191 338 7671 or send your CV to e.leith@nigelfrank.com for a confidential chat! \n\nApply Now";

            const string title3 = "Back - End BI Consultant";
            const string post3 = "Back-End BI Consultant \n \nMy client is seeking a skilled Back-End BI Developer who has experience with cloud technologies such as Data Factory and SQL DWH. The ideal candidate will have solid experience with the on-prem stack and always be looking to keep up date with the latest technologies. \n \nThey are a leading provider of cloud solutions with 80% of their new projects won being cloud based. They are a company with a very strong social culture, great work/life balance and plenty of opportunities for career development. \n \nIf you are looking for a role where you can be professionally challenged whilst being surrounded by some of Denmarks' strongest architects and consultants this is a great opportunity. All consultants engage in sparring on a daily basis in order to develop best practices and continuously improve.\n \nKey Skills:\n \n\n5+ years' experience with T-SQL and SSIS\n2+ years' experience with SSAS (Tabular and MDX)\nExperience with Azure (Data Factory, Data Bricks, SQL DWH)\nConsultancy experience (Preferred) \n\n \nKey Benefits:\n \n\nSocial Culture\nWork/Life Balance\nPension\nCareer Development\n\nIf you have an interest in this role then applications are now being welcomed. Please feel free to email a.curran@nigelfrank.com or call +45 8987 1186 for more information. \n\nApply Now";

            var jobPostWithSkills = new JobPost("Best Job", "Good Job");
            jobPostWithSkills.Keywords.Add("c#");
            jobPostWithSkills.Keywords.Add(".net");
            //jobPostWithSkills.Keywords.Add("freemarker");

            var jobPosts = new List<JobPost>
            {
                new JobPost("c#", ".net"),
                new JobPost(".net", "c#"),
                new JobPost("c# .net", ""),
                jobPostWithSkills
            };

            return jobPosts;
        }

        public static void Add(ICollection<JobPost> jobPosts)
        {
            _jobPosts.AddRange(jobPosts);
        }

        public static ICollection<JobPost> Get()
        {
            return _jobPosts;
        }
    }
}