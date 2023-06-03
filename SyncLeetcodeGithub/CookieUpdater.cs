using Newtonsoft.Json;
using Quartz;
using SeleniumUndetectedChromeDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncLeetcodeGithub
{
    internal class CookieUpdater : IJob
    {
        private UndetectedChromeDriver driver;
        public CookieUpdater(UndetectedChromeDriver driver) { 
            this.driver = driver;
        }

        public Task Execute(IJobExecutionContext context)
        {
            update();
        }

        public void update()
        {
            if (driver != null && driver.Url.Contains("leetcode"))
            {
                var cookies = driver.Manage().Cookies.AllCookies;
                string cookiesJson = JsonConvert.SerializeObject(cookies, Formatting.Indented);
                File.WriteAllText("leetcode_cookie.json", cookiesJson);
            }
        }
    }
}
