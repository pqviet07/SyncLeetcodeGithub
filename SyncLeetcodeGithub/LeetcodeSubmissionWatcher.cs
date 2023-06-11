using Quartz;
using Quartz.Impl;
using SeleniumUndetectedChromeDriver;

namespace SyncLeetcodeGithub
{
    internal class LeetcodeSubmissionWatcher 
    {
        private UndetectedChromeDriver? driver;
        public LeetcodeSubmissionWatcher(UndetectedChromeDriver driver)
        {
            this.driver = driver;
        }

        private async Task<(IScheduler, IJobDetail)> createWatcherCronJob(string cronPattern)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

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
            public Task Execute(IJobExecutionContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
