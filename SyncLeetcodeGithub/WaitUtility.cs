using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SeleniumUndetectedChromeDriver;
using Serilog;


namespace SyncLeetcodeGithub
{
    internal class WaitUtility
    {
        private UndetectedChromeDriver driver;
        public WaitUtility(UndetectedChromeDriver driver)
        {
            this.driver = driver;
        }

        public IWebElement? waitOneOfElement(string url, int sec, Dictionary<By, int> inputElements, CancellationToken cancellationToken)
        {
            IWebElement? element = null;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(sec));
            while (element == null && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    driver.GoToUrl(url);
                    element = wait.Until((IWebDriver driver) =>
                    {
                        foreach (var (key, value) in inputElements)
                        {
                            var elements = driver.FindElements(key);
                            if (elements?.Count > 0 && value < elements.Count)
                            {
                                var e = elements.ElementAtOrDefault(value);
                                if (e != null)
                                {
                                    return e;
                                }
                            }
                        }

                        return null;
                    }, cancellationToken);
                }
                catch (Exception ex) 
                {
                    Log.Error(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return element;
        }

        public Dictionary<By, IWebElement>? waitAllOfElement(string url, int sec, Dictionary<By, int> inputElements, CancellationToken cancellationToken)
        {
            Dictionary<By, IWebElement>? elementDict = null;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(sec));
            while ((elementDict == null || elementDict.Count == 0) && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    driver.GoToUrl(url);
                    elementDict = wait.Until<Dictionary<By, IWebElement>>((IWebDriver driver) =>
                    {
                        var elementDictFound = new Dictionary<By, IWebElement>();
                        foreach (var (key, value) in inputElements)
                        {
                            var elements = driver.FindElements(key);
                            if (elements?.Count > 0 && value < elements.Count)
                            {
                                var e = elements.ElementAtOrDefault(value);
                                if (e != null)
                                {
                                    elementDictFound.Add(key, e);
                                }
                            }
                        }

                        if (elementDictFound.Count == inputElements.Count)
                        {
                            return elementDictFound;
                        }
                        elementDictFound.Clear();
                        return null;
                    }, cancellationToken);
                }
                catch (Exception ex) 
                {
                    Log.Error(ex.Message + "\n" + ex.StackTrace);
                }
            }
            return elementDict;
        }
    }
}
