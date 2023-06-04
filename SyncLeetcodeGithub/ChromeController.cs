using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Quartz;
using Quartz.Impl;
using SeleniumExtras.WaitHelpers;
using SeleniumUndetectedChromeDriver;
using SyncLeetcodeGithub.Config;
using SyncLeetcodeGithub.Model;
using static Quartz.Logging.OperationName;

namespace SyncLeetcodeGithub
{
    internal class ChromeController
    {
        private IConfigurationRoot config = ConfigHolder.getConfig();
        private UndetectedChromeDriver driver;

        public ChromeController()
        {
            config = ConfigHolder.getConfig();
            string driverExecutablePath = new ChromeDriverInstaller().Auto().Result;
            driver = UndetectedChromeDriver.Create(driverExecutablePath: driverExecutablePath);
        }

        public async Task startLeetcodeMonitor(bool useCookie)
        {
            await bypassAuthentication(useCookie);

            (IScheduler scheduler, IJobDetail job) = await createUpdateCookieCronJob("0 0 6/12 ? * * *"); // cronjob to update cookie every 12 hours starting at hour 06
            await scheduler.TriggerJob(job.Key); // trigger imtermediately
            
            int countPage = firstPageNotFoundSubmission() - 1;
            // start monitor
            // ..........


            await Task.Delay(Timeout.Infinite);
        }

        public async Task bypassAuthentication(bool useCookie)
        {
            if (useCookie)
            {
                driver.GoToUrl("https://leetcode.com");
                addCookiesToBrowser("leetcode_cookie.json");
            }
            else
            {
                // maybe faild if you spam login, leetcode will force you solve capcha (possible bypass by some capcha resolver tool)
                driver.GoToUrl("https://leetcode.com/accounts/login/");

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

        public void addCookiesToBrowser(string jsonFilePath)
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

                driver.Manage().Cookies.AddCookie(cookie);
            }
        }

        public async Task<(IScheduler, IJobDetail)> createUpdateCookieCronJob(string cronPattern)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            JobDataMap jobDataMap = new JobDataMap();
            jobDataMap.Put("driver", driver);
            jobDataMap.Put("cookiePath", "leetcode_cookie.json");

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

        public int firstPageNotFoundSubmission()
        {
            (int left, int right) = findSegmentContainLastSubmissionPage();
            IWebElement? noMoreSubmissionElement = null;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            while (true)
            {
                int index = (left + right) / 2;
                driver.GoToUrl("https://leetcode.com/submissions/#/" + Convert.ToString(index));
                if (left == right) break;

                try
                {
                    noMoreSubmissionElement = wait.Until(ExpectedConditions.ElementExists(By.ClassName("placeholder-text")));
                }
                catch (Exception) { }

                if (noMoreSubmissionElement != null)
                {
                    right = index - 1;
                    noMoreSubmissionElement = null;
                }
                else
                {
                    left = index + 1;
                }
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            }

            return left;
        }

        // Cần tìm khoảng từ 'left' đến 'right', trong đó 'left' là chỉ số trang cuối cùng CÓ chứa dữ liệu,
        // và 'right' là chỉ số của trang đầu tiên KHÔNG chứa dữ liệu - 1
        public (int left,int right) findSegmentContainLastSubmissionPage()
        {
            int curIndex = 1;
            int lastIndexFound = 1;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            IWebElement? noMoreSubmissionElement = null;

            while (true)
            {
                driver.GoToUrl("https://leetcode.com/submissions/#/" + Convert.ToString(curIndex));
                try
                {
                    noMoreSubmissionElement = wait.Until(ExpectedConditions.ElementExists(By.ClassName("placeholder-text")));
                }
                catch (Exception) { }

                if (noMoreSubmissionElement != null)
                {
                    break;
                }
                else
                {
                    lastIndexFound = curIndex;
                    curIndex *= 2;
                }
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            }
            return (lastIndexFound, curIndex - 1);
        }

        public async Task processSubmissionHistory()
        {
            bool isCrawlOldSubmissionInMidNight = Convert.ToBoolean(config["crawl_history:run_mid_night"]);
        }

        public async Task updateSubmission()
        {

        }

        public void startCommitAndPushGithub()
        {

        }
    }
}
