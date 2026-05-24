using System;
using System.IO;
using System.Xml.Serialization;
using TaleWorlds.Library;

namespace BetterMPHUD
{
    public enum KillfeedMode
    {
        Native = 0,
        Warband = 1,
        NativePlus = 2
    }

    public enum ScoreboardMode
    {
        Native = 0,
        Custom = 1
    }

    [Serializable]
    public class HudSettings
    {
        public bool NativeKillfeedEnabled { get; set; } = false;
        public bool WarbandKillfeedEnabled { get; set; } = true;
        public KillfeedMode KillfeedMode { get; set; } = KillfeedMode.Warband;
        public float KillfeedFadeoutTime { get; set; } = 8f;
        public int KillfeedMaxEntries { get; set; } = 15;
        public bool KillfeedOnlyShowMyKillsDeaths { get; set; } = false;
        public bool KillfeedHideTeamkills { get; set; } = false;
        public bool ShowChat { get; set; } = true;

        public bool KillfeedBackgroundEnabled { get; set; } = true;
        public float KillfeedBackgroundOpacity { get; set; } = 0.7f;
        public string KillfeedBackgroundColor { get; set; } = "#000000FF";
        public bool BetterAvatarsEnabled { get; set; } = false;
        public bool AvatarSortingEnabled { get; set; } = false;

        public bool ChatMinimalMode { get; set; } = false;

        public bool ShowTimeAndScores { get; set; } = true;
        public bool ShowAvatars { get; set; } = true;
        public bool ShowEnemyScore { get; set; } = true;
        public bool ShowBanners { get; set; } = true;
        public bool ShowMorale { get; set; } = true;
        public bool TeamAvatarsVertical { get; set; } = false;
        public bool ShowPowerLevel { get; set; } = true;
        
        public bool AllyAvatarsVertical { get; set; } = false;
        public bool EnemyAvatarsVertical { get; set; } = false;
        public bool CameraSnapbackEnabled { get; set; } = false;

        public bool ShowAgentHealth { get; set; } = true;
        public bool ShowMountHealth { get; set; } = true;
        public bool ShowShieldHealth { get; set; } = true;
        public bool ShowWeaponInfo { get; set; } = true;
        public bool ShowAmmoCount { get; set; } = true;
        public bool ShowGoldAmount { get; set; } = true;
        public bool ShowTroopCount { get; set; } = true;
        public bool ShowCouchLanceState { get; set; } = true;
        public bool ShowDamageFeed { get; set; } = true;

        public bool ShowHealthNumbers { get; set; } = true;
        public bool ShowMountHealthNumbers { get; set; } = true;
        public bool ShowShieldHealthNumbers { get; set; } = true;
        
        public float AllyAvatarsSpacing { get; set; } = 0f;
        public float EnemyAvatarsSpacing { get; set; } = 0f;
        
        public bool ScoreboardBackgroundEnabled { get; set; } = true;
        public float ScoreboardBackgroundOpacity { get; set; } = 0.8f;
        public bool ScoreboardStripingEnabled { get; set; } = true;
        public float ScoreboardStripingOpacity { get; set; } = 0.2f;
        public float ScoreboardDeadPlayerOpacity { get; set; } = 0.3f;
        public string ScoreboardDeadPlayerColor { get; set; } = ""; 
        public bool ScoreboardDeadPlayerTintEnabled { get; set; } = false;
        public bool HideUIWhenScoreboardOpen { get; set; } = false;
        public bool ScoreboardSummaryEnabled { get; set; } = true;
        public ScoreboardMode ScoreboardMode { get; set; } = ScoreboardMode.Custom;
        public bool ScoreboardShowPing { get; set; } = true;
        public bool ScoreboardShowState { get; set; } = true;
        public bool ScoreboardShowWarlordsBanners { get; set; } = true;
        public float ScoreboardRowScale { get; set; } = 1f;
        public string ScoreboardDefaultSortColumn { get; set; } = "Score";
        public bool ScoreboardDefaultSortAscending { get; set; } = false;

        public ElementCustomization TimeAndScoresCustom { get; set; }
        public ElementCustomization TeamAvatarsCustom { get; set; }
        public ElementCustomization MoraleCustom { get; set; }
        public ElementCustomization KillfeedCustom { get; set; }
        public ElementCustomization ChatCustom { get; set; }
        
        public ElementCustomization PowerLevelCustom { get; set; }
        public ElementCustomization AllyAvatarsCustom { get; set; }
        public ElementCustomization EnemyAvatarsCustom { get; set; }
        
        public ElementCustomization AgentHealthCustom { get; set; }
        public ElementCustomization MountHealthCustom { get; set; }
        public ElementCustomization ShieldHealthCustom { get; set; }
        public ElementCustomization WeaponInfoCustom { get; set; }
        public ElementCustomization GoldAmountCustom { get; set; }
        public ElementCustomization TroopCountCustom { get; set; }
        public ElementCustomization DamageFeedCustom { get; set; }
        public CrosshairSettings CrosshairSettings { get; set; }
        
        public enum AvatarClassType
        {
            Cavalry = 0,
            Archer = 1,
            Infantry = 2,
            Unknown = 99
        }
        
        public HudSettings() 
        {
            TimeAndScoresCustom = new ElementCustomization();
            TeamAvatarsCustom = new ElementCustomization();
            MoraleCustom = new ElementCustomization();
            KillfeedCustom = new ElementCustomization();
            ChatCustom = new ElementCustomization();
    
            AgentHealthCustom = new ElementCustomization();
            MountHealthCustom = new ElementCustomization();
            ShieldHealthCustom = new ElementCustomization();
            WeaponInfoCustom = new ElementCustomization();
            GoldAmountCustom = new ElementCustomization();
            TroopCountCustom = new ElementCustomization();
            DamageFeedCustom = new ElementCustomization();

            CrosshairSettings = new CrosshairSettings();
    
            PowerLevelCustom = new ElementCustomization();
            AllyAvatarsCustom = new ElementCustomization();
            EnemyAvatarsCustom = new ElementCustomization();
        }

        public ElementCustomization GetCustomization(HudElement element)
        {
            switch (element)
            {
                case HudElement.TimeAndScores: return TimeAndScoresCustom ?? (TimeAndScoresCustom = new ElementCustomization());
                case HudElement.TeamAvatars: return TeamAvatarsCustom ?? (TeamAvatarsCustom = new ElementCustomization());
                case HudElement.Morale: return MoraleCustom ?? (MoraleCustom = new ElementCustomization());
                case HudElement.Killfeed: return KillfeedCustom ?? (KillfeedCustom = new ElementCustomization());
                case HudElement.AgentHealth: return AgentHealthCustom ?? (AgentHealthCustom = new ElementCustomization());
                case HudElement.MountHealth: return MountHealthCustom ?? (MountHealthCustom = new ElementCustomization());
                case HudElement.ShieldHealth: return ShieldHealthCustom ?? (ShieldHealthCustom = new ElementCustomization());
                case HudElement.WeaponInfo: return WeaponInfoCustom ?? (WeaponInfoCustom = new ElementCustomization());
                case HudElement.GoldAmount: return GoldAmountCustom ?? (GoldAmountCustom = new ElementCustomization());
                case HudElement.TroopCount: return TroopCountCustom ?? (TroopCountCustom = new ElementCustomization());
                case HudElement.DamageFeed: return DamageFeedCustom ?? (DamageFeedCustom = new ElementCustomization());
                default: return new ElementCustomization();
            }
        }

        public void EnsureCustomizationsExist()
        {
            if (TimeAndScoresCustom == null) TimeAndScoresCustom = new ElementCustomization();
            if (TeamAvatarsCustom == null) TeamAvatarsCustom = new ElementCustomization();
            if (MoraleCustom == null) MoraleCustom = new ElementCustomization();
            if (KillfeedCustom == null) KillfeedCustom = new ElementCustomization();
            
            if (AgentHealthCustom == null) AgentHealthCustom = new ElementCustomization();
            if (MountHealthCustom == null) MountHealthCustom = new ElementCustomization();
            if (ShieldHealthCustom == null) ShieldHealthCustom = new ElementCustomization();
            if (WeaponInfoCustom == null) WeaponInfoCustom = new ElementCustomization();
            if (GoldAmountCustom == null) GoldAmountCustom = new ElementCustomization();
            if (TroopCountCustom == null) TroopCountCustom = new ElementCustomization();
            if (DamageFeedCustom == null) DamageFeedCustom = new ElementCustomization();

            if (CrosshairSettings == null) CrosshairSettings = new CrosshairSettings();
            
            if (PowerLevelCustom == null) PowerLevelCustom = new ElementCustomization();
            if (AllyAvatarsCustom == null) AllyAvatarsCustom = new ElementCustomization();
            if (EnemyAvatarsCustom == null) EnemyAvatarsCustom = new ElementCustomization();
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
            var settings = ProfileManager.GetActiveSettings();
            if (settings.KillfeedFadeoutTime <= 0f)
                settings.KillfeedFadeoutTime = 8f;
            return settings;
        }
        
        public static void SaveSettings(HudSettings settings)
        {
            settings.EnsureCustomizationsExist();
            ProfileManager.SaveProfiles();
        }
    }
}
