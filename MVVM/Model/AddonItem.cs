using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WotlkCPKTools.MVVM.Model
{
    public class AddonItem
    {
        public string Name { get; set; }
        public string IconPath { get; set; }
        public string GitHubUrl { get; set; }
        public bool IsUpdated { get; set; }

        public DateTime? LastUpdate { get; set; }
        //public ObservableCollection<AddonFolder> Folders { get; set; } = new();

    }
}
