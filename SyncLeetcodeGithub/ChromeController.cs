using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using SeleniumUndetectedChromeDriver;
using SyncLeetcodeGithub.Config;
using SyncLeetcodeGithub.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncLeetcodeGithub
{
    internal class ChromeController
    {
        private IConfigurationRoot config = ConfigHolder.getConfig();
        private UndetectedChromeDriver driver;

        public async Task startLeetcodeMonitor(bool useCookie)
        {
            driver = UndetectedChromeDriver.Create(driverExecutablePath: await new ChromeDriverInstaller().Auto());

            if (useCookie)
            {
                driver.GoToUrl("https://leetcode.com");
                addCookiesToBrowser("leetcode_cookie.json");
            } 
            else
            {
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
            int countPage = firstPageNotFoundSubmission() - 1;


            //driver.GoToUrl("https://leetcode.com/submissions/#/1");
            //IWebElement tableElement = driver.FindElement(By.XPath("//tbody"));
            //IList<IWebElement> columnElement = tableElement.FindElements(By.XPath("./child::*"));
            //string temp = columnElement[0].FindElements(By.XPath("./child::*"))[2].FindElement(By.TagName("a")).GetAttribute("href");

            await Task.Delay(TimeSpan.FromSeconds(100));
            //IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            //await scheduler.Start();
            //ITrigger trigger = TriggerBuilder.Create().WithCronSchedule("0 0 8 ? * FRI *").StartNow().Build();
            //IJobDetail job = JobBuilder.Create<ScappingJob>().Build();
            //await scheduler.ScheduleJob(job, trigger);

            driver.Close();
            await Task.Delay(Timeout.Infinite);
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

        public void addCookiesToBrowser(string jsonFilePath)
        {
            var json = File.ReadAllText(jsonFilePath);
            var cookies = JsonConvert.DeserializeObject<List<CookieItem>>(json);
            foreach (var cookieItem in cookies!)
            {
                var cookie = new Cookie(cookieItem.name,
                    cookieItem.value,
                    cookieItem.domain,
                    cookieItem.path,
                    DateTimeOffset.FromUnixTimeSeconds((long)cookieItem.expirationDate).DateTime);

                driver.Manage().Cookies.AddCookie(cookie);
            }
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
                catch (Exception ex) { }

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
                catch (Exception ex) { }

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
