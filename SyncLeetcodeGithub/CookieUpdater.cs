using Newtonsoft.Json;
using Quartz;
using SeleniumUndetectedChromeDriver;

namespace SyncLeetcodeGithub
{
    internal class CookieUpdater : IJob
    {
        private UndetectedChromeDriver? driver;
        public string? cookiePath { get; set; }

        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.MergedJobDataMap;
            cookiePath = dataMap.GetString("CookiePath");
            driver = dataMap["Driver"] as UndetectedChromeDriver;
            if (driver != null && cookiePath != null)
            {
                await update();
            }
        }

        public async Task update()
        {
            if (driver != null && driver.Url.Contains("leetcode"))
            {
                var cookies = driver.Manage().Cookies.AllCookies;
                string cookiesJson = JsonConvert.SerializeObject(cookies, Formatting.Indented);
                File.WriteAllText(cookiePath!, cookiesJson);
            }
        }
    }
}
