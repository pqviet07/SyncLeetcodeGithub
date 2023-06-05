using Serilog;
using SyncLeetcodeGithub.Model;

namespace SyncLeetcodeGithub
{
    internal class Program
    {
        private static LeetcodeGithubSyncController controller;
        public static async Task Main(string[] args)
        {
            // Setup LOG
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("SyncLeetcodeGithub.log")
                .CreateLogger();

            try
            {
                controller = new LeetcodeGithubSyncController();
                await controller.initialize();
                await controller.start();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
    }
}