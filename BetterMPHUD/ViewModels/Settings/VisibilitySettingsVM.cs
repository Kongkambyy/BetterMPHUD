using System;
using TaleWorlds.Library;

namespace BetterMPHUD.ViewModels.Settings
{
    public class VisibilitySettingsVM : BaseSettingsVM
    {
        public VisibilitySettingsVM(HudSettings settings, Action onSettingsChanged) 
            : base(settings, onSettingsChanged)
        {
        }

        #region TopBar Visibility

        [DataSourceProperty]
        public bool ShowTimeAndScores
        {
            get => Settings.ShowTimeAndScores;
            set { if (Settings.ShowTimeAndScores != value) { Settings.ShowTimeAndScores = value; OnPropertyChangedWithValue(value, "ShowTimeAndScores"); NotifyChanged(); } }
        }

        [DataSourceProperty]
        public bool ShowAvatars
        {
            get => Settings.ShowAvatars;
            set { if (Settings.ShowAvatars != value) { Settings.ShowAvatars = value; OnPropertyChangedWithValue(value, "ShowAvatars"); NotifyChanged(); } }
        }

        [DataSourceProperty]
        public bool ShowEnemyScore
        {
            get => Settings.ShowEnemyScore;
            set { if (Settings.ShowEnemyScore != value) { Settings.ShowEnemyScore = value; OnPropertyChangedWithValue(value, "ShowEnemyScore"); NotifyChanged(); } }
        }

        [DataSourceProperty]
        public bool ShowBanners
        {
            get => Settings.ShowBanners;
            set { if (Settings.ShowBanners != value) { Settings.ShowBanners = value; OnPropertyChangedWithValue(value, "ShowBanners"); NotifyChanged(); } }
        }

        [DataSourceProperty]
        public bool ShowMorale
        {
            get => Settings.ShowMorale;
            set { if (Settings.ShowMorale != value) { Settings.ShowMorale = value; OnPropertyChangedWithValue(value, "ShowMorale"); NotifyChanged(); } }
        }

        [DataSourceProperty]
        public bool ShowPowerLevel
        {
            get => Settings.ShowPowerLevel;
            set { if (Settings.ShowPowerLevel != value) { Settings.ShowPowerLevel = value; OnPropertyChangedWithValue(value, "ShowPowerLevel"); NotifyChanged(); } }
        }

        #endregion

        #region Agent Status Visibility

        [DataSourceProperty]
        public bool ShowAgentHealth
        {
            get => Settings.ShowAgentHealth;
            set { if (Settings.ShowAgentHealth != value) { Settings.ShowAgentHealth = value; OnPropertyChangedWithValue(value, "ShowAgentHealth"); NotifyChanged(); } }
        }

        [DataSourceProperty]
        public bool ShowMountHealth
        {
            get => Settings.ShowMountHealth;
            set { if (Settings.ShowMountHealth != value) { Settings.ShowMountHealth = value; OnPropertyChangedWithValue(value, "ShowMountHealth"); NotifyChanged(); } }
        }

        [DataSourceProperty]
        public bool ShowShieldHealth
        {
            get => Settings.ShowShieldHealth;
            set { if (Settings.ShowShieldHealth != value) { Settings.ShowShieldHealth = value; OnPropertyChangedWithValue(value, "ShowShieldHealth"); NotifyChanged(); } }
        }

        [DataSourceProperty]
        public bool ShowWeaponInfo
        {
            get => Settings.ShowWeaponInfo;
            set { if (Settings.ShowWeaponInfo != value) { Settings.ShowWeaponInfo = value; OnPropertyChangedWithValue(value, "ShowWeaponInfo"); NotifyChanged(); } }
        }

        [DataSourceProperty]
        public bool ShowAmmoCount
        {
            get => Settings.ShowAmmoCount;
            set { if (Settings.ShowAmmoCount != value) { Settings.ShowAmmoCount = value; OnPropertyChangedWithValue(value, "ShowAmmoCount"); NotifyChanged(); } }
        }

        [DataSourceProperty]
        public bool ShowGoldAmount
        {
            get => Settings.ShowGoldAmount;
            set { if (Settings.ShowGoldAmount != value) { Settings.ShowGoldAmount = value; OnPropertyChangedWithValue(value, "ShowGoldAmount"); NotifyChanged(); } }
        }

        [DataSourceProperty]
        public bool ShowTroopCount
        {
            get => Settings.ShowTroopCount;
            set { if (Settings.ShowTroopCount != value) { Settings.ShowTroopCount = value; OnPropertyChangedWithValue(value, "ShowTroopCount"); NotifyChanged(); } }
        }

        [DataSourceProperty]
        public bool ShowCouchLanceState
        {
            get => Settings.ShowCouchLanceState;
            set { if (Settings.ShowCouchLanceState != value) { Settings.ShowCouchLanceState = value; OnPropertyChangedWithValue(value, "ShowCouchLanceState"); NotifyChanged(); } }
        }

        [DataSourceProperty]
        public bool ShowDamageFeed
        {
            get => Settings.ShowDamageFeed;
            set { if (Settings.ShowDamageFeed != value) { Settings.ShowDamageFeed = value; OnPropertyChangedWithValue(value, "ShowDamageFeed"); NotifyChanged(); } }
        }

        #endregion

        #region Health Numbers Visibility

        [DataSourceProperty]
        public bool ShowHealthNumbers
        {
            get => Settings.ShowHealthNumbers;
            set { if (Settings.ShowHealthNumbers != value) { Settings.ShowHealthNumbers = value; OnPropertyChangedWithValue(value, "ShowHealthNumbers"); NotifyChanged(); } }
        }

        [DataSourceProperty]
        public bool ShowMountHealthNumbers
        {
            get => Settings.ShowMountHealthNumbers;
            set { if (Settings.ShowMountHealthNumbers != value) { Settings.ShowMountHealthNumbers = value; OnPropertyChangedWithValue(value, "ShowMountHealthNumbers"); NotifyChanged(); } }
        }

        [DataSourceProperty]
        public bool ShowShieldHealthNumbers
        {
            get => Settings.ShowShieldHealthNumbers;
            set { if (Settings.ShowShieldHealthNumbers != value) { Settings.ShowShieldHealthNumbers = value; OnPropertyChangedWithValue(value, "ShowShieldHealthNumbers"); NotifyChanged(); } }
        }

        #endregion

        #region Chat Visibility

        [DataSourceProperty]
        public bool ShowChat
        {
            get => Settings.ShowChat;
            set { if (Settings.ShowChat != value) { Settings.ShowChat = value; OnPropertyChangedWithValue(value, "ShowChat"); NotifyChanged(); } }
        }

        #endregion

        public override void RefreshAll()
        {
            OnPropertyChangedWithValue(ShowTimeAndScores, "ShowTimeAndScores");
            OnPropertyChangedWithValue(ShowAvatars, "ShowAvatars");
            OnPropertyChangedWithValue(ShowEnemyScore, "ShowEnemyScore");
            OnPropertyChangedWithValue(ShowBanners, "ShowBanners");
            OnPropertyChangedWithValue(ShowMorale, "ShowMorale");
            OnPropertyChangedWithValue(ShowPowerLevel, "ShowPowerLevel");
            OnPropertyChangedWithValue(ShowAgentHealth, "ShowAgentHealth");
            OnPropertyChangedWithValue(ShowMountHealth, "ShowMountHealth");
            OnPropertyChangedWithValue(ShowShieldHealth, "ShowShieldHealth");
            OnPropertyChangedWithValue(ShowWeaponInfo, "ShowWeaponInfo");
            OnPropertyChangedWithValue(ShowAmmoCount, "ShowAmmoCount");
            OnPropertyChangedWithValue(ShowGoldAmount, "ShowGoldAmount");
            OnPropertyChangedWithValue(ShowTroopCount, "ShowTroopCount");
            OnPropertyChangedWithValue(ShowCouchLanceState, "ShowCouchLanceState");
            OnPropertyChangedWithValue(ShowDamageFeed, "ShowDamageFeed");
            OnPropertyChangedWithValue(ShowHealthNumbers, "ShowHealthNumbers");
            OnPropertyChangedWithValue(ShowMountHealthNumbers, "ShowMountHealthNumbers");
            OnPropertyChangedWithValue(ShowShieldHealthNumbers, "ShowShieldHealthNumbers");
            OnPropertyChangedWithValue(ShowChat, "ShowChat");
            NotifyChanged();
        }
    }
}