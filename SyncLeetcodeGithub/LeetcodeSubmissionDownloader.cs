using OpenQA.Selenium;
using SeleniumUndetectedChromeDriver;
using SyncLeetcodeGithub.Model;

namespace SyncLeetcodeGithub
{
    internal class LeetcodeSubmissionDownloader
    {
        private const string ORIGIN_SUBMISSION_URL = "https://leetcode.com/submissions/#/";
        private const string AC_LABEL_CLASS_NAME = "text-success"; // accepted label
        private const string WA_LABEL_CLASS_NAME = "text-danger"; // wrong answer, TLE, compile error ...
        private const string NO_SUBMISSION_LABEL_CLASS_NAME = "placeholder-text"; // no submission found
        private const int TIMEOUT = 15; // second
        private UndetectedChromeDriver driver;
        private WaitUtility waitUtility;
        private int totalPage;

        public LeetcodeSubmissionDownloader(UndetectedChromeDriver driver)
        {
            this.driver = driver;
            waitUtility = new WaitUtility(driver);
        }

        public List<SubmissionDetail>? downloadLeetcodeSubmissions()
        {
            totalPage = firstPageNotFoundSubmission() - 1;
            var acceptedSubmissionUrls = getAllUrlOfAcceptedSubmission();
            var submissionDetails = getAllAcceptedSubmissionDetail(acceptedSubmissionUrls);
            return submissionDetails;
        }

        private int firstPageNotFoundSubmission()
        {
            (int left, int right) = findSegmentContainLastSubmissionPage();
            bool flag = false;
            while (true)
            {
                int index = (left + right) / 2;
                driver.GoToUrl(ORIGIN_SUBMISSION_URL + Convert.ToString(index));
                if (left == right)
                    break;

                IWebElement? element = waitSubmissionTableLoadingAt(index);
                if (element != null && element.GetAttribute("class").Contains(NO_SUBMISSION_LABEL_CLASS_NAME))
                {
                    right = index - 1;
                    flag = false;
                }
                else
                {
                    left = index + 1;
                    flag = true;
                }
                Task.Delay(TimeSpan.FromSeconds(3)).Wait();
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
            while (true)
            {
                IWebElement? element = waitSubmissionTableLoadingAt(curIndex);
                if (element != null && element.GetAttribute("class").Contains(NO_SUBMISSION_LABEL_CLASS_NAME))
                {
                    break;
                }
                else
                {
                    lastIndexFound = curIndex;
                    curIndex *= 2;
                }
                Task.Delay(TimeSpan.FromSeconds(3)).Wait();
            }
            return (lastIndexFound, curIndex - 1);
        }

        private IWebElement? waitSubmissionTableLoadingAt(int indexPage)
        {
            string targetUrl = ORIGIN_SUBMISSION_URL + Convert.ToString(indexPage);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            var elementsToFind = new ElementsToFind();
            elementsToFind.Add(By.ClassName(AC_LABEL_CLASS_NAME), 0);
            elementsToFind.Add(By.ClassName(WA_LABEL_CLASS_NAME), 0);
            elementsToFind.Add(By.ClassName(NO_SUBMISSION_LABEL_CLASS_NAME), 0);
            return waitUtility.waitOneOfElement(targetUrl, TIMEOUT, elementsToFind.ElementDict, cancellationToken);
        }

        private List<string> getAllUrlOfAcceptedSubmission()
        {
            var acceptedSubmissionUrls = new List<string>();
            for (int index = 1; index <= totalPage; index++)
            {
                var list = getUrlsAcceptedSubmissionAt(indexPage: index);
                acceptedSubmissionUrls.AddRange(list);
                Task.Delay(TimeSpan.FromSeconds(3)).Wait();
            }
            return acceptedSubmissionUrls;
        }

        private List<string> getUrlsAcceptedSubmissionAt(int indexPage)
        {
            string targetUrl = ORIGIN_SUBMISSION_URL + Convert.ToString(indexPage);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            var elementsToFind = new ElementsToFind();
            elementsToFind.Add(By.ClassName(AC_LABEL_CLASS_NAME), 0);
            elementsToFind.Add(By.ClassName(WA_LABEL_CLASS_NAME), 0);
            waitUtility.waitOneOfElement(targetUrl, TIMEOUT, elementsToFind.ElementDict, cancellationToken);
            var acceptedElements = driver.FindElements(By.ClassName(AC_LABEL_CLASS_NAME));
            var submissionUrls = new List<string>();
            foreach (var e in acceptedElements)
            {
                submissionUrls.Add(e.GetAttribute("href"));
            }
            return submissionUrls;
        }

        private List<SubmissionDetail> getAllAcceptedSubmissionDetail(List<string> acceptedSubmissionUrls)
        {
            var submissionDetails = new List<SubmissionDetail>();
            for (int i = 0; i < acceptedSubmissionUrls.Count; i++)
            {
                var submissionDetail = getStatsOfSubmissionAt(acceptedSubmissionUrls[i]);
                submissionDetails.Add(submissionDetail);
            }
            return submissionDetails;
        }

        private SubmissionDetail getStatsOfSubmissionAt(string submissionUrl)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var elementsToFind = new ElementsToFind();
            elementsToFind.Add(By.Id("result_runtime"), 0);
            elementsToFind.Add(By.Id("result_memory"), 0);
            elementsToFind.Add(By.Id("submission-app"), 0);
            elementsToFind.Add(By.Id("result_date"), 0);
            elementsToFind.Add(By.ClassName("ace_text-layer"), 0);
            var listElement = waitUtility.waitAllOfElement(submissionUrl, TIMEOUT, elementsToFind.ElementDict, cancellationToken);
            var submissionId = submissionUrl.Trim('/').Split('/').LastOrDefault();
            var runtime = driver.FindElements(By.Id("result_runtime")).FirstOrDefault(e => e.Displayed)?.Text;
            var memory = driver.FindElements(By.Id("result_memory")).FirstOrDefault(e => e.Displayed)?.Text;
            var nameProblem = driver.FindElements(By.Id("submission-app")).FirstOrDefault(e => e.Displayed)?.Text.Split("\r\n")[0].Trim();
            var datetimeSubmit = driver.FindElements(By.Id("result_date")).FirstOrDefault(e => e.Displayed)?.Text;
            var rawCode = driver.FindElement(By.ClassName("ace_text-layer")).Text;

            return new SubmissionDetail
            {
                Id = submissionId,
                Name = nameProblem,
                Runtime = runtime,
                Memory = memory,
                Submited = datetimeSubmit,
                Code = $@"{rawCode}"
            };
        }

        class ElementsToFind
        {
            public Dictionary<By, int> ElementDict { get; set; }
            public ElementsToFind()
            {
                ElementDict = new Dictionary<By, int>();
            }

            public void Add(By by, int order)
            {
                ElementDict.Add(by, order);
            }
        }
    }
}
