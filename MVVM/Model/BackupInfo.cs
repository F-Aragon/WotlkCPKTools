using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WotlkCPKTools.MVVM.Model
{
    public class BackupInfo
    {
        public string FolderPath { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public double SizeMB { get; set; } 
        public bool IsFavorite { get; set; } = false;



        



    }


}
