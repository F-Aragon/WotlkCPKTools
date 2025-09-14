using System.IO;


namespace WotlkCPKTools.Services
{
    public static class Pathing
    {
        // Relative to CPKTools.exe
        
        //WoW Folders
        //Base WoW Folder (*.exe)
        public static readonly string WoWFolder = Path.GetDirectoryName(Environment.ProcessPath)!;
        //WTF
        public static readonly string WTF = Path.Combine(WoWFolder, "WTF");
        //AddOns
        public static readonly string AddOns = Path.Combine(WoWFolder, "Interface", "AddOns");
        //Realmlist.wtf folder
        public static readonly string RealmlistFolder = Path.Combine(WoWFolder, "Data", "enUS");



        //CPKTools Folders

        // Base CPKTools folder, relative to WoW Folder
        public static readonly string BaseFolder = Path.Combine(WoWFolder, "CPKTools");
        // Subfolder for temp files
        public static readonly string TempFolder = Path.Combine(BaseFolder, "temp");
        // Subfolder for backups
        public static readonly string BackupsFolder = Path.Combine(BaseFolder, "BackUps");
        // storedAddons.json folder
        public static readonly string storedAddons = BaseFolder;
        // storedAddons.json file
        public static readonly string storedAddonsFile = Path.Combine(storedAddons, "storedAddons.json");
        // SubFolder for Data
        public static readonly string DataFolder = Path.Combine(BaseFolder, "Data");
        // fastAddAddons.json file
        public static readonly string fastAddAddonsFile = Path.Combine(storedAddons, "fastAddAddons.json");
        // CustomAddOns Folder
        public static readonly string CustomAddOnsLists = Path.Combine(BaseFolder, "Data", "CustomAddOnsLists");
        // Recommended.txt File
        public static readonly string RecommendedFile = Path.Combine(BaseFolder, "Data", "CustomAddOnsLists", "Recommended.txt");
        // AppConfig.txt File
        public static readonly string AppConfigFile = Path.Combine(BaseFolder, "AppConfig.txt");



        // URLs
        // Recommended List
        public static readonly string RecommendedListUrl = "https://raw.githubusercontent.com/FranciscoRAragon/WotlkCPKTools/master/Recommended.txt";

        static Pathing()
        {
            Directory.CreateDirectory(BaseFolder);
            Directory.CreateDirectory(TempFolder);
            Directory.CreateDirectory(BackupsFolder);
            Directory.CreateDirectory(DataFolder);
            Directory.CreateDirectory(CustomAddOnsLists);
            Directory.CreateDirectory(BackupsFolder);

            // WoW Folders
            Directory.CreateDirectory(WTF);
            Directory.CreateDirectory(AddOns);

            // Ensure JSON files exist
            if (!File.Exists(storedAddonsFile))
                File.WriteAllText(storedAddonsFile, "[]");

            if (!File.Exists(fastAddAddonsFile))
                File.WriteAllText(fastAddAddonsFile, "[]");
        }
    }
}
