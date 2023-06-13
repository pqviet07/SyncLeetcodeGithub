using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenQA.Selenium;
using Quartz;
using Quartz.Impl;
using SeleniumUndetectedChromeDriver;
using SyncLeetcodeGithub.Config;
using SyncLeetcodeGithub.Model;
using System.Text.RegularExpressions;

namespace SyncLeetcodeGithub
{
    internal class LeetGitController
    {
        private const string cookieFilePath = "leetcode_cookie.json";
        private IConfigurationRoot config = ConfigHolder.getConfig();
        private UndetectedChromeDriver? driver;
        private LeetcodeSubmissionWatcher? submissionWatcher;
        private LeetcodeSubmissionDownloader? submissionDownloader;
        private GithubSubmitter? githubSubmitter;
        private List<SubmissionDetail>? submissionDetails;
        public bool inited { get; set; }

        public async Task<bool> initialize()
        {
            config = ConfigHolder.getConfig();
            string driverExecutablePath = new ChromeDriverInstaller().Auto().Result;
            driver = UndetectedChromeDriver.Create(driverExecutablePath: driverExecutablePath);
            if (driver == null) return false;
            submissionDownloader = new LeetcodeSubmissionDownloader(driver);
            submissionWatcher = new LeetcodeSubmissionWatcher(driver, submissionDetails);
            await createUpdateCookieCronJob("0 0 6/12 ? * * *"); // cronjob to update cookie every 12 hours starting at hour 06
            await submissionWatcher.createWatcherCronJob("0 0/5 0 ? * * *");
            inited = true;
            return true;
        }

        public void start()
        {
            if (!inited) return;
            bypassAuthentication(useCookie: true);
            submissionDetails = submissionDownloader!.downloadLeetcodeSubmissions();
            Comparison<SubmissionDetail> customComparison = (submission1, submission2) =>
            {
                int val = submission1.Name!.CompareTo(submission2.Name);
                if (val == 0)
                {
                    string numberString1 = Regex.Match(submission1.Runtime!, @"\d+").Value;
                    string numberString2 = Regex.Match(submission2.Runtime!, @"\d+").Value;
                    int runtime1 = int.Parse(numberString1);
                    int runtime2 = int.Parse(numberString2);
                    return runtime1 - runtime2;
                }
                return val;
            };
            var submissionDetailAfterRemoveDup = new List<SubmissionDetail>();
            if (submissionDetails != null)
            {
                submissionDetails.Sort(customComparison);
                submissionDetailAfterRemoveDup.Add(submissionDetails.First());
                for (int i = 1; i < submissionDetails?.Count; i++)
                {
                    if (submissionDetails[i].Name!.CompareTo(submissionDetails[i - 1].Name) != 0)
                    {
                        submissionDetailAfterRemoveDup.Add(submissionDetails[i]);
                    }
                }
            }
        }

        private void bypassAuthentication(bool useCookie)
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

                Task.Delay(TimeSpan.FromSeconds(5)).Wait();
                IWebElement signinElement = driver.FindElement(By.Id("signin_btn"));
                signinElement.Click();
            }

           Task.Delay(TimeSpan.FromSeconds(5)).Wait();
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
