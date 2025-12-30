using System;
using TaleWorlds.Library;
using BetterMPHUD.Core;

namespace BetterMPHUD.ViewModels.Settings
{
    public class AvatarSettingsVM : BaseSettingsVM
    {
        private int _selectedSide; // 0 = Ally, 1 = Enemy

        public Action<bool> OnBetterAvatarsToggled;
        public Action OnCleanupAvatarsRequested;

        public AvatarSettingsVM(HudSettings settings, Action onSettingsChanged) 
            : base(settings, onSettingsChanged)
        {
            SelectAlly();
        }

        private ElementCustomization CurrentCustomization => 
            _selectedSide == 0 ? Settings.AllyAvatarsCustom : Settings.EnemyAvatarsCustom;

        private bool CurrentVertical
        {
            get => _selectedSide == 0 ? Settings.AllyAvatarsVertical : Settings.EnemyAvatarsVertical;
            set
            {
                if (_selectedSide == 0)
                    Settings.AllyAvatarsVertical = value;
                else
                    Settings.EnemyAvatarsVertical = value;
            }
        }

        private float CurrentSpacing
        {
            get => _selectedSide == 0 ? Settings.AllyAvatarsSpacing : Settings.EnemyAvatarsSpacing;
            set
            {
                if (_selectedSide == 0)
                    Settings.AllyAvatarsSpacing = value;
                else
                    Settings.EnemyAvatarsSpacing = value;
            }
        }

        public void SelectAlly()
        {
            _selectedSide = 0;
            RefreshDisplay();
        }

        public void SelectEnemy()
        {
            _selectedSide = 1;
            RefreshDisplay();
        }

        [DataSourceProperty]
        public string SelectedName => _selectedSide == 0 ? "Ally (Left)" : "Enemy (Right)";

        [DataSourceProperty]
        public bool IsAllySelected => _selectedSide == 0;

        [DataSourceProperty]
        public bool IsEnemySelected => _selectedSide == 1;

        [DataSourceProperty]
        public string OffsetXText => CurrentCustomization.OffsetX.ToString("F0");

        [DataSourceProperty]
        public string OffsetYText => CurrentCustomization.OffsetY.ToString("F0");

        [DataSourceProperty]
        public string ScaleText => (CurrentCustomization.Scale * 100).ToString("F0") + "%";

        [DataSourceProperty]
        public string OrientationText => CurrentVertical ? "Vertical" : "Horizontal";

        [DataSourceProperty]
        public string SpacingText => CurrentSpacing.ToString("F0");

        [DataSourceProperty]
        public bool BetterAvatarsEnabled
        {
            get => Settings.BetterAvatarsEnabled;
            set
            {
                if (Settings.BetterAvatarsEnabled != value)
                {
                    Settings.BetterAvatarsEnabled = value;
                    OnPropertyChangedWithValue(value, "BetterAvatarsEnabled");
                    OnBetterAvatarsToggled?.Invoke(value);
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public bool AvatarSortingEnabled
        {
            get => Settings.AvatarSortingEnabled;
            set
            {
                if (Settings.AvatarSortingEnabled != value)
                {
                    Settings.AvatarSortingEnabled = value;
                    OnPropertyChangedWithValue(value, "AvatarSortingEnabled");
                    NotifyChanged();
                }
            }
        }

        public void ExecuteIncreaseOffsetX() => AdjustOffset(CurrentCustomization, Constants.Adjustment.PositionStep, 0, RefreshDisplay);
        public void ExecuteDecreaseOffsetX() => AdjustOffset(CurrentCustomization, -Constants.Adjustment.PositionStep, 0, RefreshDisplay);
        public void ExecuteIncreaseOffsetY() => AdjustOffset(CurrentCustomization, 0, Constants.Adjustment.PositionStep, RefreshDisplay);
        public void ExecuteDecreaseOffsetY() => AdjustOffset(CurrentCustomization, 0, -Constants.Adjustment.PositionStep, RefreshDisplay);
        public void ExecuteIncreaseOffsetXLarge() => AdjustOffset(CurrentCustomization, Constants.Adjustment.PositionStepLarge, 0, RefreshDisplay);
        public void ExecuteDecreaseOffsetXLarge() => AdjustOffset(CurrentCustomization, -Constants.Adjustment.PositionStepLarge, 0, RefreshDisplay);
        public void ExecuteIncreaseOffsetYLarge() => AdjustOffset(CurrentCustomization, 0, Constants.Adjustment.PositionStepLarge, RefreshDisplay);
        public void ExecuteDecreaseOffsetYLarge() => AdjustOffset(CurrentCustomization, 0, -Constants.Adjustment.PositionStepLarge, RefreshDisplay);

        public void ExecuteIncreaseScale() => AdjustScale(CurrentCustomization, Constants.Adjustment.ScaleStep, RefreshDisplay);
        public void ExecuteDecreaseScale() => AdjustScale(CurrentCustomization, -Constants.Adjustment.ScaleStep, RefreshDisplay);
        public void ExecuteIncreaseScaleLarge() => AdjustScale(CurrentCustomization, Constants.Adjustment.ScaleStepLarge, RefreshDisplay);
        public void ExecuteDecreaseScaleLarge() => AdjustScale(CurrentCustomization, -Constants.Adjustment.ScaleStepLarge, RefreshDisplay);

        public void ExecuteIncreaseSpacing() => AdjustSpacing(1f);
        public void ExecuteDecreaseSpacing() => AdjustSpacing(-1f);
        public void ExecuteIncreaseSpacingLarge() => AdjustSpacing(5f);
        public void ExecuteDecreaseSpacingLarge() => AdjustSpacing(-5f);

        private void AdjustSpacing(float delta)
        {
            float newValue = CurrentSpacing + delta;
            if (newValue >= -20f && newValue <= 50f)
            {
                CurrentSpacing = newValue;
                OnPropertyChangedWithValue(SpacingText, "SpacingText");
                NotifyChanged();
            }
        }

        public void ExecuteToggleOrientation()
        {
            CurrentVertical = !CurrentVertical;
            OnPropertyChangedWithValue(OrientationText, "OrientationText");
            NotifyChanged();
        }

        public void ExecuteCleanupAvatars()
        {
            OnCleanupAvatarsRequested?.Invoke();
            InformationManager.DisplayMessage(new InformationMessage("[BetterMPHUD] Cleaned up disconnected avatars.", Colors.Green));
        }

        public void ExecuteResetCurrent()
        {
            CurrentCustomization.Reset();
            CurrentVertical = false;
            CurrentSpacing = 0f;
            RefreshDisplay();
        }

        public void ExecuteResetAll()
        {
            Settings.AllyAvatarsCustom.Reset();
            Settings.EnemyAvatarsCustom.Reset();
            Settings.AllyAvatarsVertical = false;
            Settings.EnemyAvatarsVertical = false;
            Settings.AllyAvatarsSpacing = 0f;
            Settings.EnemyAvatarsSpacing = 0f;
            SelectAlly();
            NotifyChanged();
        }

        private void RefreshDisplay()
        {
            OnPropertyChangedWithValue(SelectedName, "SelectedName");
            OnPropertyChangedWithValue(IsAllySelected, "IsAllySelected");
            OnPropertyChangedWithValue(IsEnemySelected, "IsEnemySelected");
            OnPropertyChangedWithValue(OffsetXText, "OffsetXText");
            OnPropertyChangedWithValue(OffsetYText, "OffsetYText");
            OnPropertyChangedWithValue(ScaleText, "ScaleText");
            OnPropertyChangedWithValue(OrientationText, "OrientationText");
            OnPropertyChangedWithValue(SpacingText, "SpacingText");
            NotifyChanged();
        }

        public override void RefreshAll()
        {
            OnPropertyChangedWithValue(BetterAvatarsEnabled, "BetterAvatarsEnabled");
            OnPropertyChangedWithValue(AvatarSortingEnabled, "AvatarSortingEnabled");
            RefreshDisplay();
        }
    }
}