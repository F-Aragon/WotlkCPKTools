using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WotlkCPKTools.Core;
using WotlkCPKTools.MVVM.Model;
using WotlkCPKTools.MVVM.View;
using WotlkCPKTools.Services;

namespace WotlkCPKTools.MVVM.ViewModel
{
    class AddonsViewModel : ObservableObject
    {
        //For testing, remove later
        string testApiUrl = "https://api.github.com/repos/NoM0Re/WeakAuras-WotLK/commits";
        string testRepoUrl = "https://github.com/NoM0Re/WeakAuras-WotLK";
        string localAddonsFolder = "C:\\Users\\f\\Desktop\\TestWOW\\Interface\\AddOns";
        //Remove later


        AddonInfo addon = new AddonInfo();
        CommitInfo infoCommit;
        StoredAddonInfo oldInfo = new StoredAddonInfo();

        
        public RelayCommand OpenAddAddonWindowCommand { get; set; } //Button to open the AddAddonWindow




        public AddonsViewModel()
        {
            
           // LoadInfoAsync();

            OpenAddAddonWindowCommand = new RelayCommand(o =>
            {
                var window = new AddAddonWindow();
                window.ShowDialog(); 
            });

        }


        
        


    }
}
