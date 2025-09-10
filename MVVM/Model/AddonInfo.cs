namespace WotlkCPKTools.MVVM.Model
{
    public class AddonInfo 
    {
        public string? Name { get; set; } 
        public string GitHubUrl { get; set; } 
        public string? LocalPath { get; set; } 
        public string? NewSha { get; set; } 
        public string? OldSha { get; set; }  
        public DateTime? OldCommitDate { get; set; } 
        public DateTime? NewCommitDate { get; set; } 
        public List<string?> LocalFolders { get; set; } = null;
        public bool IsUpdated { get; set; } 
        public DateTime? LastUpdateDate { get; set; } 


        public void RefreshUpdateStatus()
        {
            IsUpdated = NewSha == OldSha;
        }


    }



}
