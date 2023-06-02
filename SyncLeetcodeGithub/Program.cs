using Quartz.Impl;
using Quartz;
using Serilog;
using SyncLeetcodeGithub.Config;
using SeleniumUndetectedChromeDriver;
using OpenQA.Selenium;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using OpenQA.Selenium.Chrome;
using SyncLeetcodeGithub.Model;

namespace SyncLeetcodeGithub
{
    internal class Program
    {
        private static List<SubmissionDetail> submissionDetails;
        

        public static async Task Main(string[] args)
        {
            // Setup LOG
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("SyncLeetcodeGithub.log")
                .CreateLogger();

            ChromeController chromeController = new ChromeController();
            await chromeController.startLeetcodeMonitor(true);
        }
    }
}