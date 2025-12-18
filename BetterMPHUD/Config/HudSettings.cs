using System;
using System.IO;
using System.Xml.Serialization;
using TaleWorlds.Library;

namespace BetterMPHUD
{
    [Serializable]
    public class HudSettings
    {
        public bool NativeKillfeedEnabled { get; set; } = false;
        public bool WarbandKillfeedEnabled { get; set; } = true;

        public bool ShowTimeAndScores { get; set; } = true;
        public bool ShowAvatars { get; set; } = true;
        public bool ShowEnemyScore { get; set; } = true;
        public bool ShowBanners { get; set; } = true;
        public bool ShowMorale { get; set; } = true;

        public ElementCustomization TimeAndScoresCustom { get; set; } = new ElementCustomization();
        public ElementCustomization TeamAvatarsCustom { get; set; } = new ElementCustomization();
        public ElementCustomization MoraleCustom { get; set; } = new ElementCustomization();
        
        public HudSettings() { }

        public ElementCustomization GetCustomization(HudElement element)
        {
            switch (element)
            {
                case HudElement.TimeAndScores:
                    return TimeAndScoresCustom;
                case HudElement.TeamAvatars:
                    return TeamAvatarsCustom;
                case HudElement.Morale:
                    return MoraleCustom;
                default:
                    return new ElementCustomization();
            }
        }
    }
    
    public static class ConfigManager
    {
        private static readonly string ConfigFileName = "BetterMPHUD_Config.xml";
        private static string _configPath;
        
        private static string ConfigPath
        {
            get
            {
                if (string.IsNullOrEmpty(_configPath))
                {
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string bannerlordConfigPath = Path.Combine(documentsPath, "Mount and Blade II Bannerlord", "Configs");
                    
                    if (!Directory.Exists(bannerlordConfigPath))
                    {
                        Directory.CreateDirectory(bannerlordConfigPath);
                    }
                    
                    _configPath = Path.Combine(bannerlordConfigPath, ConfigFileName);
                }
                return _configPath;
            }
        }
        
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
                        
                        if (settings.TimeAndScoresCustom == null) settings.TimeAndScoresCustom = new ElementCustomization();
                        if (settings.TeamAvatarsCustom == null) settings.TeamAvatarsCustom = new ElementCustomization();
                        if (settings.MoraleCustom == null) settings.MoraleCustom = new ElementCustomization();
                        
                        InformationManager.DisplayMessage(new InformationMessage("[BetterMPHUD] Settings loaded.", Colors.Green));
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage($"[BetterMPHUD] Failed to load config: {ex.Message}", Colors.Red));
            }

            return new HudSettings();
        }
        
        public static void SaveSettings(HudSettings settings)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(HudSettings));
                using (FileStream stream = new FileStream(ConfigPath, FileMode.Create))
                {
                    serializer.Serialize(stream, settings);
                }
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage($"[BetterMPHUD] Failed to save config: {ex.Message}", Colors.Red));
            }
        }
    }
}