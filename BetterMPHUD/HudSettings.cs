using System;
using System.IO;
using System.Xml.Serialization;
using TaleWorlds.Library;

namespace BetterMPHUD
{
    /// <summary>
    /// Serializable settings class that gets saved to/loaded from XML
    /// </summary>
    [Serializable]
    public class HudSettings
    {
        // Killfeed Settings
        public bool NativeKillfeedEnabled { get; set; } = false;
        public bool WarbandKillfeedEnabled { get; set; } = true;

        // Top Bar Settings
        public bool ShowTimeAndScores { get; set; } = true;
        public bool ShowAvatars { get; set; } = true;
        public bool ShowEnemyScore { get; set; } = true;
        public bool ShowBanners { get; set; } = true;
        public bool ShowMorale { get; set; } = true;

        /// <summary>
        /// Default constructor with default values (required for XML serialization)
        /// </summary>
        public HudSettings() { }
    }

    /// <summary>
    /// Handles saving and loading of HudSettings to/from XML file
    /// </summary>
    public static class ConfigManager
    {
        private static readonly string ConfigFileName = "BetterMPHUD_Config.xml";
        private static string _configPath;

        /// <summary>
        /// Gets the full path to the config file.
        /// Saves to: Documents/Mount and Blade II Bannerlord/Configs/BetterMPHUD_Config.xml
        /// </summary>
        private static string ConfigPath
        {
            get
            {
                if (string.IsNullOrEmpty(_configPath))
                {
                    // Get Bannerlord's config directory
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string bannerlordConfigPath = Path.Combine(documentsPath, "Mount and Blade II Bannerlord", "Configs");
                    
                    // Ensure directory exists
                    if (!Directory.Exists(bannerlordConfigPath))
                    {
                        Directory.CreateDirectory(bannerlordConfigPath);
                    }
                    
                    _configPath = Path.Combine(bannerlordConfigPath, ConfigFileName);
                }
                return _configPath;
            }
        }

        /// <summary>
        /// Load settings from XML file. Returns default settings if file doesn't exist or is corrupted.
        /// </summary>
        public static HudSettings LoadSettings()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(HudSettings));
                    using (FileStream stream = new FileStream(ConfigPath, FileMode.Open))
                    {
                        HudSettings settings = (HudSettings)serializer.Deserialize(stream);
                        InformationManager.DisplayMessage(new InformationMessage("[BetterMPHUD] Settings loaded.", Colors.Green));
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage($"[BetterMPHUD] Failed to load config: {ex.Message}", Colors.Red));
            }

            // Return default settings if file doesn't exist or failed to load
            return new HudSettings();
        }

        /// <summary>
        /// Save settings to XML file.
        /// </summary>
        public static void SaveSettings(HudSettings settings)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(HudSettings));
                using (FileStream stream = new FileStream(ConfigPath, FileMode.Create))
                {
                    serializer.Serialize(stream, settings);
                }
                // Optional: Uncomment for save confirmation (can be spammy)
                // InformationManager.DisplayMessage(new InformationMessage("[BetterMPHUD] Settings saved.", Colors.Green));
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage($"[BetterMPHUD] Failed to save config: {ex.Message}", Colors.Red));
            }
        }
    }
}