using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using SyncLeetcodeGithub.Model;
using SeleniumUndetectedChromeDriver;
using System.Net.Http.Headers;
using System.Xml.Linq;
using System;

namespace SyncLeetcodeGithub
{
    internal class LeetcodeSubmissionDownloader
    {
        private UndetectedChromeDriver driver;
        public LeetcodeSubmissionDownloader(UndetectedChromeDriver driver) {
            this.driver = driver;
        }
        public async Task<List<SubmissionDetail>?> downloadLeetcodeSubmissions()
        {
            for (int i = 0; i < 20; i++)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("window.open();");
            }

            int countPage = firstPageNotFoundSubmission() - 1;
            List<SubmissionDetail>? submissionDetails = null;
            //var tasks = new List<Task<string>>();
            //for (int i = 1; i <= countPage; i++)
            //{
            //    tasks.Add(getListSubmissions);
            //}
            // start download
            // ..........
            return submissionDetails;
        }
        private int firstPageNotFoundSubmission()
        {
            (int left, int right) = findSegmentContainLastSubmissionPage();
            IWebElement? element = null;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
            while (true)
            {
                int index = (left + right) / 2;
                driver.GoToUrl("https://leetcode.com/submissions/#/" + Convert.ToString(index));
                if (left == right) break;
retry1:
                try
                {
                    element = wait.Until((IWebDriver driver) =>
                    {
                        var noSubmissionLabel  = driver.FindElement(By.ClassName("placeholder-text"));
                        var dangerText = driver.FindElement(By.ClassName("text-danger"));
                        var successText = driver.FindElement(By.ClassName("text-success"));

                        if (noSubmissionLabel != null) return noSubmissionLabel;
                        if (dangerText != null) return dangerText;
                        if (successText != null) return successText;
                        return null;
                    });
                }
                catch (Exception) { }
                
                if (element == null)
                {
                    driver.GoToUrl("https://leetcode.com/submissions/#/" + Convert.ToString(index));
                    goto retry1;
                }
                
                if (element.GetAttribute("class").Contains("placeholder-text"))
                {
                    right = index - 1;
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
        private (int left, int right) findSegmentContainLastSubmissionPage()
        {
            int curIndex = 1;
            int lastIndexFound = 1;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
            IWebElement? element = null;

            while (true)
            {
                driver.GoToUrl("https://leetcode.com/submissions/#/" + Convert.ToString(curIndex));
retry2:               
                try
                {
                    element = wait.Until((IWebDriver driver) =>
                    {
                        IWebElement? noSubmissionLabel = null;
                        IWebElement? dangerText = null;
                        IWebElement? successText = null;
                        try
                            noSubmissionLabel = driver.FindElement(By.ClassName("placeholder-text"));
                        }
                        catch (Exception)
                        {

                        }

                        try
                        {
                            dangerText = driver.FindElement(By.ClassName("text-danger"));
                        }
                        catch (Exception)
                        {
                        }

                        try
                        {
                            successText = driver.FindElement(By.ClassName("text-success"));
                        }
                        catch (Exception)
                        {
                        }


                        if (noSubmissionLabel != null) return noSubmissionLabel;
                        if (dangerText != null) return dangerText;
                        if (successText != null) return successText;
                        return null;
                    });
                }
                catch (Exception) { }

                if (element == null)
                {
                    driver.GoToUrl("https://leetcode.com/submissions/#/" + Convert.ToString(curIndex));
                    goto retry2;
                }

                if (element.GetAttribute("class").Contains("placeholder-text"))
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

        
        private async Task<List<string>> getListSubmissions(int index)
        {
            driver.GoToUrl("https://leetcode.com/submissions/#/" + Convert.ToString(index));
            var accptedElements = driver.FindElements(By.ClassName("text-success"));
            var submissionUrls = new List<string>();
            foreach (var e in accptedElements)
            {
                submissionUrls.Add(e.GetAttribute("href"));
                Console.WriteLine(e.GetAttribute("href"));
            }
            return submissionUrls;
        }
    }
}
