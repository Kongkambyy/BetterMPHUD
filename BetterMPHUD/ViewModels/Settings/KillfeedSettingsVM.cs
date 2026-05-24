using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using BetterMPHUD.Core;

namespace BetterMPHUD.ViewModels.Settings
{
    public class KillfeedSettingsVM : BaseSettingsVM
    {
        public Action<bool> OnWarbandKillfeedToggled;
        public Action OnPreviewUpdate;

        public KillfeedSettingsVM(HudSettings settings, Action onSettingsChanged) 
            : base(settings, onSettingsChanged)
        {
            NormalizeKillfeedMode();
            ApplyNativeKillfeedSetting();
        }

        private void NormalizeKillfeedMode()
        {
            if (Settings.KillfeedMode == KillfeedMode.Warband &&
                Settings.NativeKillfeedEnabled &&
                !Settings.WarbandKillfeedEnabled)
            {
                Settings.KillfeedMode = KillfeedMode.Native;
            }
        }

        private void ApplyNativeKillfeedSetting()
        {
            SyncLegacyKillfeedFlags();
            ManagedOptions.SetConfig(ManagedOptions.ManagedOptionsType.ReportCasualtiesType, 0f);
        }

        private void SyncLegacyKillfeedFlags()
        {
            Settings.NativeKillfeedEnabled = Settings.KillfeedMode == KillfeedMode.Native;
            Settings.WarbandKillfeedEnabled = Settings.KillfeedMode != KillfeedMode.Native;
        }

        private void SetKillfeedMode(KillfeedMode mode)
        {
            if (Settings.KillfeedMode == mode)
                return;

            Settings.KillfeedMode = mode;
            ApplyNativeKillfeedSetting();
            RefreshModeProperties();
            OnWarbandKillfeedToggled?.Invoke(Settings.KillfeedMode != KillfeedMode.Native);
            NotifyChanged();
            NotifyPreviewUpdate();
        }

        public void ExecuteSelectNativeMode()
        {
            SetKillfeedMode(KillfeedMode.Native);
        }

        public void ExecuteSelectWarbandMode()
        {
            SetKillfeedMode(KillfeedMode.Warband);
        }

        public void ExecuteSelectNativePlusMode()
        {
            SetKillfeedMode(KillfeedMode.NativePlus);
        }

        private void RefreshModeProperties()
        {
            OnPropertyChangedWithValue(IsNativeKillfeedModeSelected, "IsNativeKillfeedModeSelected");
            OnPropertyChangedWithValue(IsWarbandKillfeedModeSelected, "IsWarbandKillfeedModeSelected");
            OnPropertyChangedWithValue(IsNativePlusKillfeedModeSelected, "IsNativePlusKillfeedModeSelected");
            OnPropertyChangedWithValue(NativeKillfeedEnabled, "NativeKillfeedEnabled");
            OnPropertyChangedWithValue(WarbandKillfeedEnabled, "WarbandKillfeedEnabled");
        }

        private void NotifyPreviewUpdate()
        {
            OnPreviewUpdate?.Invoke();
        }

        [DataSourceProperty]
        public bool NativeKillfeedEnabled
        {
            get => Settings.KillfeedMode == KillfeedMode.Native;
            set
            {
                if (value)
                    SetKillfeedMode(KillfeedMode.Native);
            }
        }

        [DataSourceProperty]
        public bool WarbandKillfeedEnabled
        {
            get => Settings.KillfeedMode != KillfeedMode.Native;
            set
            {
                if (value)
                    SetKillfeedMode(KillfeedMode.Warband);
                else
                    SetKillfeedMode(KillfeedMode.Native);
            }
        }

        [DataSourceProperty]
        public bool IsNativeKillfeedModeSelected
        {
            get => Settings.KillfeedMode == KillfeedMode.Native;
            set { if (value) SetKillfeedMode(KillfeedMode.Native); }
        }

        [DataSourceProperty]
        public bool IsWarbandKillfeedModeSelected
        {
            get => Settings.KillfeedMode == KillfeedMode.Warband;
            set { if (value) SetKillfeedMode(KillfeedMode.Warband); }
        }

        [DataSourceProperty]
        public bool IsNativePlusKillfeedModeSelected
        {
            get => Settings.KillfeedMode == KillfeedMode.NativePlus;
            set { if (value) SetKillfeedMode(KillfeedMode.NativePlus); }
        }

        [DataSourceProperty]
        public bool KillfeedBackgroundEnabled
        {
            get => Settings.KillfeedBackgroundEnabled;
            set
            {
                if (Settings.KillfeedBackgroundEnabled != value)
                {
                    Settings.KillfeedBackgroundEnabled = value;
                    OnPropertyChangedWithValue(value, "KillfeedBackgroundEnabled");
                    NotifyChanged();
                    NotifyPreviewUpdate();
                }
            }
        }

        [DataSourceProperty]
        public string OffsetXText => Settings.KillfeedCustom.OffsetX.ToString("F0");

        [DataSourceProperty]
        public string OffsetYText => Settings.KillfeedCustom.OffsetY.ToString("F0");

        [DataSourceProperty]
        public string ScaleText => (Settings.KillfeedCustom.Scale * 100).ToString("F0") + "%";

        [DataSourceProperty]
        public string FadeoutTimeText => Settings.KillfeedFadeoutTime.ToString("F0") + "s";

        [DataSourceProperty]
        public string MaxEntriesText => Settings.KillfeedMaxEntries.ToString();

        [DataSourceProperty]
        public string BackgroundOpacityText => (Settings.KillfeedBackgroundOpacity * 100).ToString("F0") + "%";

        [DataSourceProperty]
        public string BackgroundColorText => Settings.KillfeedBackgroundColor;

        [DataSourceProperty]
        public bool OnlyShowMyKillsDeaths
        {
            get => Settings.KillfeedOnlyShowMyKillsDeaths;
            set
            {
                if (Settings.KillfeedOnlyShowMyKillsDeaths != value)
                {
                    Settings.KillfeedOnlyShowMyKillsDeaths = value;
                    OnPropertyChangedWithValue(value, "OnlyShowMyKillsDeaths");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public bool HideTeamkills
        {
            get => Settings.KillfeedHideTeamkills;
            set
            {
                if (Settings.KillfeedHideTeamkills != value)
                {
                    Settings.KillfeedHideTeamkills = value;
                    OnPropertyChangedWithValue(value, "HideTeamkills");
                    NotifyChanged();
                }
            }
        }

        public void ExecuteIncreaseOffsetX() 
        { 
            AdjustOffset(Settings.KillfeedCustom, Constants.Adjustment.PositionStep, 0, RefreshDisplay); 
            NotifyPreviewUpdate(); 
        }
        
        public void ExecuteDecreaseOffsetX() 
        { 
            AdjustOffset(Settings.KillfeedCustom, -Constants.Adjustment.PositionStep, 0, RefreshDisplay); 
            NotifyPreviewUpdate(); 
        }
        
        public void ExecuteIncreaseOffsetY() 
        { 
            AdjustOffset(Settings.KillfeedCustom, 0, Constants.Adjustment.PositionStep, RefreshDisplay); 
            NotifyPreviewUpdate(); 
        }
        
        public void ExecuteDecreaseOffsetY() 
        { 
            AdjustOffset(Settings.KillfeedCustom, 0, -Constants.Adjustment.PositionStep, RefreshDisplay); 
            NotifyPreviewUpdate(); 
        }
        
        public void ExecuteIncreaseOffsetXLarge() 
        { 
            AdjustOffset(Settings.KillfeedCustom, Constants.Adjustment.PositionStepLarge, 0, RefreshDisplay); 
            NotifyPreviewUpdate(); 
        }
        
        public void ExecuteDecreaseOffsetXLarge() 
        { 
            AdjustOffset(Settings.KillfeedCustom, -Constants.Adjustment.PositionStepLarge, 0, RefreshDisplay); 
            NotifyPreviewUpdate(); 
        }
        
        public void ExecuteIncreaseOffsetYLarge() 
        { 
            AdjustOffset(Settings.KillfeedCustom, 0, Constants.Adjustment.PositionStepLarge, RefreshDisplay); 
            NotifyPreviewUpdate(); 
        }
        
        public void ExecuteDecreaseOffsetYLarge() 
        { 
            AdjustOffset(Settings.KillfeedCustom, 0, -Constants.Adjustment.PositionStepLarge, RefreshDisplay); 
            NotifyPreviewUpdate(); 
        }

        public void ExecuteIncreaseScale() 
        { 
            AdjustScale(Settings.KillfeedCustom, Constants.Adjustment.ScaleStep, RefreshDisplay); 
            NotifyPreviewUpdate(); 
        }
        
        public void ExecuteDecreaseScale() 
        { 
            AdjustScale(Settings.KillfeedCustom, -Constants.Adjustment.ScaleStep, RefreshDisplay); 
            NotifyPreviewUpdate(); 
        }
        
        public void ExecuteIncreaseScaleLarge() 
        { 
            AdjustScale(Settings.KillfeedCustom, Constants.Adjustment.ScaleStepLarge, RefreshDisplay); 
            NotifyPreviewUpdate(); 
        }
        
        public void ExecuteDecreaseScaleLarge() 
        { 
            AdjustScale(Settings.KillfeedCustom, -Constants.Adjustment.ScaleStepLarge, RefreshDisplay); 
            NotifyPreviewUpdate(); 
        }

        public void ExecuteIncreaseFadeoutTime() => AdjustFadeout(Constants.Adjustment.FadeoutStep);
        public void ExecuteDecreaseFadeoutTime() => AdjustFadeout(-Constants.Adjustment.FadeoutStep);
        public void ExecuteIncreaseFadeoutTimeLarge() => AdjustFadeout(5f);
        public void ExecuteDecreaseFadeoutTimeLarge() => AdjustFadeout(-5f);

        private void AdjustFadeout(float delta)
        {
            float newTime = Settings.KillfeedFadeoutTime + delta;
            if (newTime >= Constants.Adjustment.MinFadeout && newTime <= Constants.Adjustment.MaxFadeout)
            {
                Settings.KillfeedFadeoutTime = newTime;
                OnPropertyChangedWithValue(FadeoutTimeText, "FadeoutTimeText");
                NotifyChanged();
            }
        }

        public void ExecuteIncreaseMaxEntries() => AdjustMaxEntries(1);
        public void ExecuteDecreaseMaxEntries() => AdjustMaxEntries(-1);
        public void ExecuteIncreaseMaxEntriesLarge() => AdjustMaxEntries(5);
        public void ExecuteDecreaseMaxEntriesLarge() => AdjustMaxEntries(-5);

        private void AdjustMaxEntries(int delta)
        {
            int newValue = Settings.KillfeedMaxEntries + delta;
            if (newValue >= 1 && newValue <= 15)
            {
                Settings.KillfeedMaxEntries = newValue;
                OnPropertyChangedWithValue(MaxEntriesText, "MaxEntriesText");
                NotifyChanged();
            }
        }

        public void ExecuteIncreaseBackgroundOpacity() 
        { 
            AdjustBackgroundOpacity(0.05f); 
            NotifyPreviewUpdate(); 
        }
        
        public void ExecuteDecreaseBackgroundOpacity() 
        { 
            AdjustBackgroundOpacity(-0.05f); 
            NotifyPreviewUpdate(); 
        }
        
        public void ExecuteIncreaseBackgroundOpacityLarge() 
        { 
            AdjustBackgroundOpacity(0.1f); 
            NotifyPreviewUpdate(); 
        }
        
        public void ExecuteDecreaseBackgroundOpacityLarge() 
        { 
            AdjustBackgroundOpacity(-0.1f); 
            NotifyPreviewUpdate(); 
        }

        private void AdjustBackgroundOpacity(float delta)
        {
            float newValue = Settings.KillfeedBackgroundOpacity + delta;
            if (newValue >= 0f && newValue <= 1f)
            {
                Settings.KillfeedBackgroundOpacity = (float)Math.Round(newValue, 2);
                RefreshDisplay();
            }
        }

        public void ExecuteReset()
        {
            Settings.KillfeedCustom.Reset();
            Settings.KillfeedMode = KillfeedMode.Warband;
            ApplyNativeKillfeedSetting();
            Settings.KillfeedFadeoutTime = Constants.Adjustment.DefaultFadeout;
            Settings.KillfeedBackgroundEnabled = true;
            Settings.KillfeedBackgroundOpacity = 0.7f;
            Settings.KillfeedMaxEntries = 15;
            Settings.KillfeedOnlyShowMyKillsDeaths = false;
            Settings.KillfeedHideTeamkills = false;
            RefreshAll();
            NotifyPreviewUpdate();
        }

        private void RefreshDisplay()
        {
            OnPropertyChangedWithValue(OffsetXText, "OffsetXText");
            OnPropertyChangedWithValue(OffsetYText, "OffsetYText");
            OnPropertyChangedWithValue(ScaleText, "ScaleText");
            OnPropertyChangedWithValue(BackgroundOpacityText, "BackgroundOpacityText");
            OnPropertyChangedWithValue(BackgroundColorText, "BackgroundColorText");
            OnPropertyChangedWithValue(KillfeedBackgroundEnabled, "KillfeedBackgroundEnabled");
            OnPropertyChangedWithValue(MaxEntriesText, "MaxEntriesText");
            OnPropertyChangedWithValue(OnlyShowMyKillsDeaths, "OnlyShowMyKillsDeaths");
            OnPropertyChangedWithValue(HideTeamkills, "HideTeamkills");
            NotifyChanged();
        }

        public override void RefreshAll()
        {
            ApplyNativeKillfeedSetting();
            RefreshModeProperties();
            OnPropertyChangedWithValue(FadeoutTimeText, "FadeoutTimeText");
            RefreshDisplay();
        }
    }
}
