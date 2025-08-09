using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WotlkCPKTools.MVVM.Model
{
    internal class StoredAddonInfo
    {
        public string Name { get; set; }
        public string GitHubUrl { get; set; }
        public string LocalPath { get; set; }
        public string Sha { get; set; }
        public DateTime? LastUpdated { get; set; }

    }
}
