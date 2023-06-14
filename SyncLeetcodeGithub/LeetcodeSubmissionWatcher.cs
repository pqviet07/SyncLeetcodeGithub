using Quartz;
using Quartz.Impl;
using SeleniumUndetectedChromeDriver;
using SyncLeetcodeGithub.Model;

namespace SyncLeetcodeGithub
{
    internal class LeetcodeSubmissionWatcher 
    {
        private UndetectedChromeDriver? driver;
        private List<SubmissionDetail>? submissionDetails;

        public LeetcodeSubmissionWatcher(UndetectedChromeDriver driver, List<SubmissionDetail>? submissionDetails)
        {
            this.driver = driver;
            this.submissionDetails = submissionDetails;
        }

        public async Task<(IScheduler, IJobDetail)> createWatcherCronJob(string cronPattern)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            JobDataMap jobDataMap = new JobDataMap();
            jobDataMap.Put("Driver", driver!);
            jobDataMap.Put("ListSubmissionDetail", submissionDetails);

            IJobDetail job = JobBuilder.Create<SubmissionWatcherJob>()
                                       .Build();

            ITrigger trigger = TriggerBuilder.Create()
                                             .WithCronSchedule(cronPattern)
                                             .StartNow()
                                             .Build();

            await scheduler.ScheduleJob(job, trigger);
            return (scheduler, job);
        }

        class SubmissionWatcherJob : IJob
        {
            private const string FIRST_SUBMISSION_PAGE_URL = "https://leetcode.com/submissions/#/1";
            private UndetectedChromeDriver? driver;
            private List<SubmissionDetail>? submissionDetails;

            public async Task Execute(IJobExecutionContext context)
            {
                JobDataMap dataMap = context.MergedJobDataMap;
                driver = dataMap["Driver"] as UndetectedChromeDriver;
                submissionDetails = dataMap["ListSubmissionDetail"] as List<SubmissionDetail>;
                if (driver != null)
                {
                    driver.GoToUrl(FIRST_SUBMISSION_PAGE_URL);
                }
            }
        }
    }
}
