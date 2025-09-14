<div align="center">

# CPK WoW Tools


[![Discord](https://img.shields.io/discord/1415132752197451778?style=flat&logo=discord&logoColor=%23FFFFFF&label=%20&labelColor=%235865F2&color=%235865F2)](https://discord.com/9TyTnZZ8vZ)
[![Static Badge](https://img.shields.io/badge/Chupankii-black?style=flat&logo=Kick&logoColor=%2353FC19)](https://kick.com/chupankii)
[![Static Badge](https://img.shields.io/badge/VirusTotal-1?style=flat&logo=virustotal&logoColor=%23FFFFFF&color=%23394EFF)](https://www.virustotal.com/gui/file/170b81d362e823f8321f88395f305f331bbe22867516c436919fea599cda2283/detection)

CPK WoW Tools is a desktop app designed to simplify addon and backup management for World of Warcraft. Easily install, update, back up, and organize your addons and WTF folder, while also serving as a convenient game launcher.

Download the [latest release](https://github.com/FranciscoRAragon/WotlkCPKTools/releases) and drop the **CPKWoWTools.exe** inside your World of Warcraft folder.

**Note:** Your antivirus might detect the app as a potential threat.

![Logo](https://i.imgur.com/AeTELrV.png)

</div>


<div align="center">

# Features
  
</div>

## General Features
* Supports multiple World of Warcraft expansions. (Select realmlist and WTF folder in More tab)

## Addons Manager
* Add addons directly from GitHub.
* Check and update all your addons with a single click.
* Create, manage, and share your own CustomLists.
* Share addon lists with your guild.
* Easily add or remove addons.
* Safe deletion that respects folders shared by multiple addons.

## BackUp Manager
* Create backups of your WTF folder with a single click.
* Update existing backup information.
* Keep all your backups neatly organized.
* Remove outdated backups easily.
* Restore backups with one button.

## Launcher
* Launch World of Warcraft directly from the app.
* Quickly check which realmlist your client is using.
* Open the realmlist folder with one click.

<div align="center">

![Footer](https://i.imgur.com/xwq1Rxe.png)

</div>


## Getting Started  

1. **Download the app**  
   - Go to the [latest release](https://github.com/FranciscoRAragon/WotlkCPKTools/releases).  
   - Download the file **CPKWoWTools.exe**.  

2. **Place it in your World of Warcraft folder**  
   - Drop the `CPKWoWTools.exe` inside your main **World of Warcraft** folder.  
   - This ensures the app can properly manage your addons and settings.   

> ⚠️ Your antivirus might detect the app as a potential threat.  
> This is a **false positive** — add it as an exception in your antivirus.


## Custom Lists  

### What is a Custom List?  
A **Custom List** is a collection of links to different repositories hosting addons.  
These lists are designed to save addon packs, making it easy to keep them on hand and install them all with just a few clicks. Create a list and share it with your community or your guild.  

---

### How to add one?  
In the **Addons** tab, click on **Add Custom List**. You’ll have three options:  
1. Select a local `.txt` file.  
2. Paste a link to a `list.txt` hosted on GitHub.  
3. Create a list manually by adding entries one by one.  

> 🔗 A Custom List `.txt` can contain a link to its own online version in a GitHub repository.  
> This allows the app to check for differences and update the local file whenever the online list is modified (e.g., when new addons are added).  

You can always edit your lists by opening their corresponding `.txt` file.  

---

### Example of a complete list  
> 📝 The name of the `.txt` file will be the name of the list.  
> The link (`@`) is not mandatory.  

```txt
# a comment about this list
@https://github.com/anUser/testList/blob/main/customListOnlineName.txt

QuestHelper:https://github.com/example/QuestHelper
Auctionator:https://github.com/example/Auctionator
Details backport:https://github.com/example/Details
Bagnon 3.3.5:https://github.com/example/Bagnon
https://github.com/example/ElvUi   <-- If there is no name, the app will use the repo name

