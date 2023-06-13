using Serilog;

namespace SyncLeetcodeGithub
{
    internal class Program
    {
        private static LeetGitController controller;
        public static async Task Main(string[] args)
        {
            // Setup LOG
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("SyncLeetcodeGithub.log")
                .CreateLogger();

            try
            {
                controller = new LeetGitController();
                controller.initialize().Wait();
                controller.start();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
    }
}