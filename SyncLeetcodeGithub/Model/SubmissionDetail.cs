using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncLeetcodeGithub.Model
{
    internal class SubmissionDetail
    {
        public long id { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string runtime { get; set; }
        public string memory { get; set; }
        public string runtimeBeatPercentage { get; set; }
        public string memoryBeatPercentage { get; set; }
        public string code { get; set; }
    }
}
