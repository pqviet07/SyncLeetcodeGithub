using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools.V111.Profiler;
using OpenQA.Selenium.Support.UI;
using Quartz;
using Quartz.Impl;
using SeleniumExtras.WaitHelpers;
using SeleniumUndetectedChromeDriver;
using SyncLeetcodeGithub.Config;
using SyncLeetcodeGithub.Model;

namespace SyncLeetcodeGithub
{
    internal class LeetcodeGithubSyncController
    {
        private const string cookieFilePath = "leetcode_cookie.json";
        private IConfigurationRoot config = ConfigHolder.getConfig();
        private UndetectedChromeDriver? driver;
        private LeetcodeSubmissionWatcher? submissionWatcher;
        private LeetcodeSubmissionDownloader? submissionDownloader;
        private GithubSubmitter? githubSubmitter;
        private static List<SubmissionDetail>? submissionDetails;

        public bool inited { get; set; }
        public async Task<bool> initialize()
        {
            config = ConfigHolder.getConfig();
            string driverExecutablePath = new ChromeDriverInstaller().Auto().Result;
            driver = UndetectedChromeDriver.Create(driverExecutablePath: driverExecutablePath);
            if (driver == null) return false;
            submissionDownloader = new LeetcodeSubmissionDownloader(driver);
            submissionWatcher = new LeetcodeSubmissionWatcher(driver);
            (IScheduler scheduler, IJobDetail job) = await createUpdateCookieCronJob("0 0 6/12 ? * * *"); // cronjob to update cookie every 12 hours starting at hour 06
            inited = true;
            return true;
        }

        public async Task start()
        {
            await bypassAuthentication(useCookie: true);
            await submissionDownloader.downloadLeetcodeSubmissions();
        }
        private async Task bypassAuthentication(bool useCookie)
        {
            if (useCookie)
            {
                driver!.GoToUrl("https://leetcode.com");
                addCookiesToBrowser(cookieFilePath);
            }
            else
            {
                // maybe faild if you spam login, leetcode will force you solve capcha (possible bypass by some capcha resolver tool)
                driver!.GoToUrl("https://leetcode.com/accounts/login/");

                IWebElement usernameElement = driver.FindElement(By.Id("id_login"));
                usernameElement.SendKeys(config["leetcode:username"]);

                IWebElement passwordElement = driver.FindElement(By.Id("id_password"));
                passwordElement.SendKeys(config["leetcode:password"]);

                await Task.Delay(TimeSpan.FromSeconds(5));
                IWebElement signinElement = driver.FindElement(By.Id("signin_btn"));
                signinElement.Click();
            }

            await Task.Delay(TimeSpan.FromSeconds(5));
        }

        private void addCookiesToBrowser(string jsonFilePath)
        {
            var json = File.ReadAllText(jsonFilePath);
            var cookies = JsonConvert.DeserializeObject<List<CookieItem>>(json);
            foreach (var cookieItem in cookies!)
            {
                var cookie = new Cookie(cookieItem.Name,
                    cookieItem.Value,
                    cookieItem.Domain,
                    cookieItem.Path,
                    DateTimeOffset.FromUnixTimeSeconds((long)cookieItem.Expiry).DateTime);

                driver!.Manage().Cookies.AddCookie(cookie);
            }
        }

        private async Task<(IScheduler, IJobDetail)> createUpdateCookieCronJob(string cronPattern)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            JobDataMap jobDataMap = new JobDataMap();
            jobDataMap.Put("driver", driver!);
            jobDataMap.Put("cookiePath", cookieFilePath);

            IJobDetail job = JobBuilder.Create<CookieUpdater>()
                .UsingJobData(jobDataMap)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithCronSchedule(cronPattern)
                .StartNow()
                .Build();

            await scheduler.ScheduleJob(job, trigger);
            return (scheduler, job);
        }
    }
}
