using Quartz.Impl;
using Quartz;
using Serilog;
using SyncLeetcodeGithub.Config;
using SeleniumUndetectedChromeDriver;
using OpenQA.Selenium;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;

namespace SyncLeetcodeGithub
{
    internal class Program
    {
        private static List<SubmissionDetail>? submissionDetails;
        private static long lastSubmissionId;
        private static IConfigurationRoot config = ConfigHolder.getConfig();

        public static async Task Main(string[] args)
        {
            // Setup LOG
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("SyncLeetcodeGithub.log")
                .CreateLogger();

            await startLeetcodeMonitor();
        }

        public static async Task startLeetcodeMonitor()
        {
            var driver = UndetectedChromeDriver.Create(driverExecutablePath: await new ChromeDriverInstaller().Auto());
            driver.GoToUrl("https://leetcode.com/accounts/login/");

            IWebElement usernameElement = driver.FindElement(By.Id("id_login"));
            usernameElement.SendKeys(config["leetcode:username"]);

            IWebElement passwordElement = driver.FindElement(By.Id("id_password"));
            passwordElement.SendKeys(config["leetcode:password"]);

            await Task.Delay(TimeSpan.FromSeconds(5));
            IWebElement signinElement = driver.FindElement(By.Id("signin_btn"));
            signinElement.Click();

            await Task.Delay(TimeSpan.FromSeconds(5));
            driver.GoToUrl("https://leetcode.com/submissions/#/1");

            IWebElement tableElement = driver.FindElement(By.XPath("//tbody"));
            IList<IWebElement> columnElement = tableElement.FindElements(By.XPath("./child::*"));
            string temp = columnElement[0].FindElements(By.XPath("./child::*"))[2].FindElement(By.TagName("a")).GetAttribute("href");
            Log.Warning(temp);
           
            if (Convert.ToBoolean(config["crawl_history:enable"]))
            {

            }
            //IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            //await scheduler.Start();
            //ITrigger trigger = TriggerBuilder.Create().WithCronSchedule("0 0 8 ? * FRI *").StartNow().Build();
            //IJobDetail job = JobBuilder.Create<ScappingJob>().Build();
            //await scheduler.ScheduleJob(job, trigger);

            await Task.Delay(TimeSpan.FromSeconds(10000));
            driver.Close();
            await Task.Delay(Timeout.Infinite);
        }

        public static async Task processSubmissionHistory()
        {
            bool isCrawlOldSubmissionInMidNight = Convert.ToBoolean(config["crawl_history:run_mid_night"]);
        }

        public static async Task updateSubmission()
        {

        }

        public static void startCommitAndPushGithub()
        {

        }
    }

    class SubmissionDetail
    {
        public long id { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string runtime { get; set; }
        public string memory { get; set; }
        public string runtimeBeatPercentage { get; set; }
        public string memoryBeatPercentage { get; set; }
        public string code { get; set; }
    }
}