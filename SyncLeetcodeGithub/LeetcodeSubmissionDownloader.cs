using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using SyncLeetcodeGithub.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeleniumUndetectedChromeDriver;

namespace SyncLeetcodeGithub
{
    internal class LeetcodeSubmissionDownloader
    {
        private UndetectedChromeDriver? driver;
        public LeetcodeSubmissionDownloader(UndetectedChromeDriver driver) {
            this.driver = driver;
        }
        public async Task<List<SubmissionDetail>?> downloadLeetcodeSubmissions()
        {
            int countPage = firstPageNotFoundSubmission() - 1;
            List<SubmissionDetail>? submissionDetails = null;
            // start download
            // ..........
            return submissionDetails;
        }
        private int firstPageNotFoundSubmission()
        {
            (int left, int right) = findSegmentContainLastSubmissionPage();
            IWebElement? noMoreSubmissionElement = null;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            while (true)
            {
                int index = (left + right) / 2;
                driver!.GoToUrl("https://leetcode.com/submissions/#/" + Convert.ToString(index));
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
        private (int left, int right) findSegmentContainLastSubmissionPage()
        {
            int curIndex = 1;
            int lastIndexFound = 1;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            IWebElement? noMoreSubmissionElement = null;

            while (true)
            {
                driver!.GoToUrl("https://leetcode.com/submissions/#/" + Convert.ToString(curIndex));
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
    }
}
