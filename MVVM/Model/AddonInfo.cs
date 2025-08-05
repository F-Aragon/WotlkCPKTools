using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WotlkCPKTools.MVVM.Model
{
    public class AddonInfo
    {
        public string Name { get; set; } //ok
        public string GitHubUrl { get; set; } //ok
        public string LocalPath { get; set; }
        public string NewSha { get; set; } //ok
        public string OldSha { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime NewCommitDate { get; set; } //ok
    }



}
