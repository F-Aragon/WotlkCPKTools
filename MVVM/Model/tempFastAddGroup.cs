using System.Collections.ObjectModel;

namespace WotlkCPKTools.MVVM.Model
{
    public class tempFastAddGroup
    {
        public string GroupName { get; set; }
        public ObservableCollection<FastAddAddonInfo> Addons { get; set; } = new();
    }
}

