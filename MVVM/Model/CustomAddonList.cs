using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WotlkCPKTools.MVVM.Model
{
    public class CustomAddonList
    {
        public string ListName { get; set; }  // FileName
        public ObservableCollection<FastAddAddonInfo> Addons { get; set; }
    }
}

