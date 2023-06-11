using RestSharp;
using SyncLeetcodeGithub.Config;
using System.Text;

namespace SyncLeetcodeGithub
{
    internal class GithubSubmitter
    {
        private const int BLOB_MAX_SIZE_MEGABYTE = 5;
        public static async Task<RestResponse?> CommitAndPushGithub(string filePath, string commitMessage)
        {
            var config = ConfigHolder.getConfig();
            var githubToken = config["github:api_token"];
            var owner = config["github:owner"];
            var repo = config["github:repo"];
            var branchName = config["github:main"];
            var committerName = config["github:committer:name"];
            var committerEmail = config["github:committer:email"];

            if (!File.Exists(filePath)) return null;
            var fileSize = new FileInfo(filePath).Length;
            double fileSizeInMegabytes = fileSize / (1024.0 * 1024.0);
            if (fileSizeInMegabytes > BLOB_MAX_SIZE_MEGABYTE) return null;
            string fileContent = File.ReadAllText(filePath);
            var encodedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileContent));

            var client = new RestClient("https://api.github.com");
            var request = new RestRequest($"/repos/{owner}/{repo}/contents/{filePath}", Method.Put);
            request.AddHeader("Accept", "application/vnd.github+json");
            request.AddHeader("Authorization", $"Bearer {githubToken}");
            request.AddHeader("X-GitHub-Api-Version", "2022-11-28");
            request.AddJsonBody(new
            {
                message = commitMessage,
                content = encodedContent,
                branch = branchName,
                committer = new
                {
                    name = committerName,
                    email = committerEmail
                }
            });

            return await client.ExecuteAsync(request);
        }
    }
}
