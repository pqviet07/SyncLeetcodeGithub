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
            int countPage = firstPageNotFoundSubmission() - 1;
            // 
            await createUpdateCookieCronJob("0 0 6/12 ? * * *"); // update every 12 hours starting at hour 06
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

        public async Task createUpdateCookieCronJob(string cronPattern)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            JobDataMap jobDataMap = new JobDataMap();
            jobDataMap.Put("UndetectedChromeDriver", driver);

            IJobDetail job = JobBuilder.Create<CookieUpdater>()
                .UsingJobData(jobDataMap)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithCronSchedule(cronPattern)
                .StartNow()
                .Build();

            await scheduler.ScheduleJob(job, trigger);
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

        public int firstPageNotFoundSubmission()
        {
            int curIndex = 1;
            int lastIndexFound = 1;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            IWebElement? noMoreSubmissionElement = null;
            // index cuối cùng của page có data và index đầu tiên của page không có data chính là left và right để tìm nhị phân
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
                    noMoreSubmissionElement = null;
                    break;
                }
                else
                {
                    lastIndexFound = curIndex;
                    curIndex *= 2;
                }
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            }

            // chặt nhị phân giữa lastIndexFound và curIndex
            int left = lastIndexFound;
            int right = curIndex - 1;
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
    }
}
