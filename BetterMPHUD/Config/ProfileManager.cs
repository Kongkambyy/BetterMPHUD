using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using TaleWorlds.Library;

namespace BetterMPHUD
{
    [Serializable]
    public class HudProfile
    {
        public string Name { get; set; } = "Default";
        public HudSettings Settings { get; set; } = new HudSettings();
        
        public HudProfile() { }
        
        public HudProfile(string name, HudSettings settings)
        {
            Name = name;
            Settings = settings;
        }
    }

    [Serializable]
    public class ProfileData
    {
        public string ActiveProfileName { get; set; } = "Default";
        public List<HudProfile> Profiles { get; set; } = new List<HudProfile>();
        
        public ProfileData()
        {
            Profiles.Add(new HudProfile("Default", new HudSettings()));
        }
    }

    public static class ProfileManager
    {
        private static readonly string ProfileFileName = "BetterMPHUD_Profiles.xml";
        private static string _profilePath;
        private static ProfileData _profileData;

        private static string ProfilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_profilePath))
                {
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string bannerlordConfigPath = Path.Combine(documentsPath, "Mount and Blade II Bannerlord", "Configs");
                    if (!Directory.Exists(bannerlordConfigPath)) 
                        Directory.CreateDirectory(bannerlordConfigPath);
                    _profilePath = Path.Combine(bannerlordConfigPath, ProfileFileName);
                }
                return _profilePath;
            }
        }

        public static ProfileData GetProfileData()
        {
            if (_profileData == null)
                LoadProfiles();
            return _profileData;
        }

        public static void LoadProfiles()
        {
            try
            {
                if (File.Exists(ProfilePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ProfileData));
                    using (FileStream stream = new FileStream(ProfilePath, FileMode.Open))
                    {
                        _profileData = (ProfileData)serializer.Deserialize(stream);
                        
                        foreach (var profile in _profileData.Profiles)
                            profile.Settings.EnsureCustomizationsExist();
                        
                        InformationManager.DisplayMessage(new InformationMessage(
                            "[BetterMPHUD] Loaded " + _profileData.Profiles.Count + " profiles.", Colors.Green));
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[BetterMPHUD] Failed to load profiles: " + ex.Message, Colors.Red));
            }
            
            _profileData = new ProfileData();
            SaveProfiles();
        }

        public static void SaveProfiles()
        {
            try
            {
                foreach (var profile in _profileData.Profiles)
                    profile.Settings.EnsureCustomizationsExist();
                
                XmlSerializer serializer = new XmlSerializer(typeof(ProfileData));
                using (FileStream stream = new FileStream(ProfilePath, FileMode.Create))
                {
                    serializer.Serialize(stream, _profileData);
                }
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[BetterMPHUD] Failed to save profiles: " + ex.Message, Colors.Red));
            }
        }

        public static HudProfile GetActiveProfile()
        {
            var data = GetProfileData();
            var profile = data.Profiles.Find(p => p.Name == data.ActiveProfileName);
            if (profile == null && data.Profiles.Count > 0)
            {
                profile = data.Profiles[0];
                data.ActiveProfileName = profile.Name;
            }
            return profile ?? new HudProfile();
        }

        public static HudSettings GetActiveSettings()
        {
            return GetActiveProfile().Settings;
        }

        public static void SetActiveProfile(string name)
        {
            var data = GetProfileData();
            if (data.Profiles.Exists(p => p.Name == name))
            {
                data.ActiveProfileName = name;
                SaveProfiles();
                InformationManager.DisplayMessage(new InformationMessage(
                    "[BetterMPHUD] Switched to profile: " + name, Colors.Cyan));
            }
        }

        public static bool CreateProfile(string name)
        {
            var data = GetProfileData();
            
            if (string.IsNullOrWhiteSpace(name))
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[BetterMPHUD] Profile name cannot be empty.", Colors.Red));
                return false;
            }
            
            if (data.Profiles.Exists(p => p.Name == name))
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[BetterMPHUD] Profile '" + name + "' already exists.", Colors.Red));
                return false;
            }
            
            var newProfile = new HudProfile(name, new HudSettings());
            data.Profiles.Add(newProfile);
            SaveProfiles();
            
            InformationManager.DisplayMessage(new InformationMessage(
                "[BetterMPHUD] Created profile: " + name, Colors.Green));
            return true;
        }

        public static bool DuplicateProfile(string sourceName, string newName)
        {
            var data = GetProfileData();
            var source = data.Profiles.Find(p => p.Name == sourceName);
            
            if (source == null)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[BetterMPHUD] Source profile not found.", Colors.Red));
                return false;
            }
            
            if (data.Profiles.Exists(p => p.Name == newName))
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[BetterMPHUD] Profile '" + newName + "' already exists.", Colors.Red));
                return false;
            }
            
            var copy = DeepCopySettings(source.Settings);
            var newProfile = new HudProfile(newName, copy);
            data.Profiles.Add(newProfile);
            SaveProfiles();
            
            InformationManager.DisplayMessage(new InformationMessage(
                "[BetterMPHUD] Duplicated profile as: " + newName, Colors.Green));
            return true;
        }

        public static bool DeleteProfile(string name)
        {
            var data = GetProfileData();
            
            if (data.Profiles.Count <= 1)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[BetterMPHUD] Cannot delete the last profile.", Colors.Red));
                return false;
            }
            
            var profile = data.Profiles.Find(p => p.Name == name);
            if (profile == null)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[BetterMPHUD] Profile not found.", Colors.Red));
                return false;
            }
            
            data.Profiles.Remove(profile);
            
            if (data.ActiveProfileName == name)
                data.ActiveProfileName = data.Profiles[0].Name;
            
            SaveProfiles();
            
            InformationManager.DisplayMessage(new InformationMessage(
                "[BetterMPHUD] Deleted profile: " + name, Colors.Green));
            return true;
        }

        public static bool RenameProfile(string oldName, string newName)
        {
            var data = GetProfileData();
            
            if (string.IsNullOrWhiteSpace(newName))
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[BetterMPHUD] New name cannot be empty.", Colors.Red));
                return false;
            }
            
            if (data.Profiles.Exists(p => p.Name == newName))
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[BetterMPHUD] Profile '" + newName + "' already exists.", Colors.Red));
                return false;
            }
            
            var profile = data.Profiles.Find(p => p.Name == oldName);
            if (profile == null)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[BetterMPHUD] Profile not found.", Colors.Red));
                return false;
            }
            
            if (data.ActiveProfileName == oldName)
                data.ActiveProfileName = newName;
            
            profile.Name = newName;
            SaveProfiles();
            
            InformationManager.DisplayMessage(new InformationMessage(
                "[BetterMPHUD] Renamed profile to: " + newName, Colors.Green));
            return true;
        }

        public static List<string> GetProfileNames()
        {
            var names = new List<string>();
            foreach (var profile in GetProfileData().Profiles)
                names.Add(profile.Name);
            return names;
        }

        public static int GetProfileCount()
        {
            return GetProfileData().Profiles.Count;
        }

        public static int GetActiveProfileIndex()
        {
            var data = GetProfileData();
            for (int i = 0; i < data.Profiles.Count; i++)
            {
                if (data.Profiles[i].Name == data.ActiveProfileName)
                    return i;
            }
            return 0;
        }

        public static void SetActiveProfileByIndex(int index)
        {
            var data = GetProfileData();
            if (index >= 0 && index < data.Profiles.Count)
                SetActiveProfile(data.Profiles[index].Name);
        }

        private static HudSettings DeepCopySettings(HudSettings source)
        {
            var serializer = new XmlSerializer(typeof(HudSettings));
            using (var ms = new MemoryStream())
            {
                serializer.Serialize(ms, source);
                ms.Position = 0;
                var copy = (HudSettings)serializer.Deserialize(ms);
                copy.EnsureCustomizationsExist();
                return copy;
            }
        }
    }
}