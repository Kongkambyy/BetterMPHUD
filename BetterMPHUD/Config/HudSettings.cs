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
        public float KillfeedFadeoutTime { get; set; } = 8f;

        public bool ShowTimeAndScores { get; set; } = true;
        public bool ShowAvatars { get; set; } = true;
        public bool ShowEnemyScore { get; set; } = true;
        public bool ShowBanners { get; set; } = true;
        public bool ShowMorale { get; set; } = true;

        public bool ShowChat { get; set; } = true;
        public bool ChatAlwaysVisible { get; set; } = false;

        public ElementCustomization TimeAndScoresCustom { get; set; }
        public ElementCustomization TeamAvatarsCustom { get; set; }
        public ElementCustomization MoraleCustom { get; set; }
        public ElementCustomization KillfeedCustom { get; set; }
        public ElementCustomization ChatCustom { get; set; }
        
        public HudSettings() 
        {
            TimeAndScoresCustom = new ElementCustomization();
            TeamAvatarsCustom = new ElementCustomization();
            MoraleCustom = new ElementCustomization();
            KillfeedCustom = new ElementCustomization();
        }

        public ElementCustomization GetCustomization(HudElement element)
        {
            switch (element)
            {
                case HudElement.TimeAndScores: return TimeAndScoresCustom ?? (TimeAndScoresCustom = new ElementCustomization());
                case HudElement.TeamAvatars: return TeamAvatarsCustom ?? (TeamAvatarsCustom = new ElementCustomization());
                case HudElement.Morale: return MoraleCustom ?? (MoraleCustom = new ElementCustomization());
                case HudElement.Killfeed: return KillfeedCustom ?? (KillfeedCustom = new ElementCustomization());
                default: return new ElementCustomization();
            }
        }

        public void EnsureCustomizationsExist()
        {
            if (TimeAndScoresCustom == null) TimeAndScoresCustom = new ElementCustomization();
            if (TeamAvatarsCustom == null) TeamAvatarsCustom = new ElementCustomization();
            if (MoraleCustom == null) MoraleCustom = new ElementCustomization();
            if (KillfeedCustom == null) KillfeedCustom = new ElementCustomization();
            if (ChatCustom == null) ChatCustom = new ElementCustomization();
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
                    if (!Directory.Exists(bannerlordConfigPath)) Directory.CreateDirectory(bannerlordConfigPath);
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
                        settings.EnsureCustomizationsExist();
                        
                        if (settings.KillfeedFadeoutTime <= 0f) 
                            settings.KillfeedFadeoutTime = 8f;
                        
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
                settings.EnsureCustomizationsExist();
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