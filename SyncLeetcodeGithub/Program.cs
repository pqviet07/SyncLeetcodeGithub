using Serilog;
using SyncLeetcodeGithub.Model;

namespace SyncLeetcodeGithub
{
    internal class Program
    {
        private static List<SubmissionDetail>? submissionDetails;
        public static async Task Main(string[] args)
        {
            // Setup LOG
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("SyncLeetcodeGithub.log")
                .CreateLogger();

            ChromeController chromeController = new ChromeController();
            try
            {
                await chromeController.initialize();
                submissionDetails = await chromeController.downloadLeetcodeSubmissions(useCookie: true);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
    }
}