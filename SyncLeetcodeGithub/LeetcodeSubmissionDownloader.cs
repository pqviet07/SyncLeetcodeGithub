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
            int countPage = firstPageNotFoundSubmission() - 1;
            List<SubmissionDetail>? submissionDetails = null;
 
            List<string> acceptedSubmissionUrls = new List<string>();
            for (int i = 1; i <= countPage; i++)
            {
                var temp =  getListSubmissions(i);
                acceptedSubmissionUrls.AddRange(temp);
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            }

            // start download
            // ..........
            return submissionDetails;
        }
        private int firstPageNotFoundSubmission()
        {
            (int left, int right) = findSegmentContainLastSubmissionPage();
            bool flag = false;
            while (true)
            {
                int index = (left + right) / 2;
                driver.GoToUrl("https://leetcode.com/submissions/#/" + Convert.ToString(index));
                if (left == right) 
                    break;

                IWebElement? element = waitSubmissionsLoadingAt(index);
                if (element.GetAttribute("class").Contains("placeholder-text"))
                {
                    right = index - 1;
                    flag = false;
                } 
                else
                {
                    left = index + 1;
                    flag = true;
                }
            }

            if (flag) return left;
            return right + 1;
        }

        // Cần tìm khoảng từ 'left' đến 'right', trong đó 'left' là chỉ số trang cuối cùng CÓ chứa dữ liệu,
        // và 'right' là chỉ số của trang đầu tiên KHÔNG chứa dữ liệu - 1
        private (int left, int right) findSegmentContainLastSubmissionPage()
        {
            int curIndex = 1;
            int lastIndexFound = 1;
            IWebElement? element = null;
            while (true)
            {
                element = waitSubmissionsLoadingAt(curIndex);
                if (element.GetAttribute("class").Contains("placeholder-text"))
                {
                    break;
                }
                else
                {
                    lastIndexFound = curIndex;
                    curIndex *= 2;
                }
            }
            return (lastIndexFound, curIndex - 1);
        }

        private IWebElement waitSubmissionsLoadingAt(int index)
        {
            IWebElement? element = null;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            while (element == null)
            {
                try
                {
                    driver.GoToUrl("https://leetcode.com/submissions/#/" + Convert.ToString(index));
                    element = wait.Until((IWebDriver driver) =>
                    {
                        var noSubmissionLabel = driver.FindElements(By.ClassName("placeholder-text")).FirstOrDefault(e => e.Displayed);
                        var dangerText = driver.FindElements(By.ClassName("text-danger")).FirstOrDefault(e => e.Displayed);
                        var successText = driver.FindElements(By.ClassName("text-success")).FirstOrDefault(e => e.Displayed);
                        var elementVisiable = noSubmissionLabel ?? successText ?? dangerText;
                        return elementVisiable;
                    });
                }
                catch (Exception) { }

            }
            return element;
        }

        private List<string> getListSubmissions(int index)
        {
            driver.GoToUrl("https://leetcode.com/submissions/#/" + Convert.ToString(index));
            IWebElement? element = null;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            while (element == null)
            {
                try
                {
                    driver.GoToUrl("https://leetcode.com/submissions/#/" + Convert.ToString(index));
                    element = wait.Until((IWebDriver driver) =>
                    {
                        var dangerText = driver.FindElements(By.ClassName("text-danger")).FirstOrDefault(e => e.Displayed);
                        var successText = driver.FindElements(By.ClassName("text-success")).FirstOrDefault(e => e.Displayed);
                        var elementVisiable = successText ?? dangerText;
                        return elementVisiable;
                    });
                }
                catch (Exception) { }
            }
            var acceptedElements = driver.FindElements(By.ClassName("text-success"));
            var submissionUrls = new List<string>();
            foreach (var e in acceptedElements)
            {
                submissionUrls.Add(e.GetAttribute("href"));
            }
            return submissionUrls;
        }
    }
}
