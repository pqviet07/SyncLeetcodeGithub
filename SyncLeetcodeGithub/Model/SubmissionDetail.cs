using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncLeetcodeGithub.Model
{
    internal class SubmissionDetail
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Runtime { get; set; }
        public string? Memory { get; set; }
        public string? Submited { get; set; }
        public string? RuntimeBeatPercentage { get; set; }
        public string? MemoryBeatPercentage { get; set; }
        public string? Code { get; set; }
    }
}
