using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WotlkCPKTools.Core;

namespace WotlkCPKTools.MVVM.Model
{
    public class AddonInfo 
    {
        public string? Name { get; set; } //ok
        public string GitHubUrl { get; set; } //ok
        public string? LocalPath { get; set; } //ok
        public string? NewSha { get; set; } //net
        public string? OldSha { get; set; }  //ok
        public DateTime? OldCommitDate { get; set; } //ok
        public DateTime? NewCommitDate { get; set; } //net
        public List<string?> LocalFolders { get; set; } = null;
        public bool IsUpdated { get; set; } 
        public DateTime? LastUpdateDate { get; set; } //ok


        public void RefreshUpdateStatus()
        {
            IsUpdated = NewSha == OldSha;
        }


    }



}
