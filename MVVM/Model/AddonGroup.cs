using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WotlkCPKTools.MVVM.Model
{
    public class AddonGroup
    {
        public string GroupName { get; set; }
        public ObservableCollection<AddonItem> Addons { get; set; } = new();
    }
}
