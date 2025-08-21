using System;
using System.IO;

namespace WotlkCPKTools.Services
{
    public static class Pathing
    {
        // Relative to CPKTools.exe

        //WoW Folders
        //Base WoW Folder (*.exe)
        public static readonly string WoWFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
        //WTF
        public static readonly string WTF = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WTF");
        //AddOns
        public static readonly string AddOns = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Interface", "AddOns");



        //CPKTools Folders

        // Carpeta base CPKTools relativa al ejecutable
        public static readonly string BaseFolder = Path.Combine(WoWFolder, "CPKTools");
        // Subcarpeta para archivos temporales
        public static readonly string TempFolder = Path.Combine(BaseFolder, "temp");
        // Subcarpeta para backups
        public static readonly string BackupsFolder = Path.Combine(BaseFolder, "BackUps");
        // storedAddons.json folder
        public static readonly string storedAddons = Path.Combine(WoWFolder, "CPKTools");
        // storedAddons.json file
        public static readonly string storedAddonsFile = Path.Combine(storedAddons, "storedAddons.json");

        // Asegura que existan todas las carpetas necesarias
        static Pathing()
        {
            Directory.CreateDirectory(BaseFolder);
            Directory.CreateDirectory(TempFolder);
            Directory.CreateDirectory(BackupsFolder);

            // WoW Folders
            Directory.CreateDirectory(WTF);
            Directory.CreateDirectory(AddOns);

        }
    }
}
