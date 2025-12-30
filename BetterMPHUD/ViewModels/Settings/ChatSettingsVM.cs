using System;
using TaleWorlds.Library;
using BetterMPHUD.Core;

namespace BetterMPHUD.ViewModels.Settings
{
    public class ChatSettingsVM : BaseSettingsVM
    {
        public Action<bool> OnChatToggled;
        public Action OnDebugChatStructure;

        public ChatSettingsVM(HudSettings settings, Action onSettingsChanged) 
            : base(settings, onSettingsChanged)
        {
        }

        [DataSourceProperty]
        public bool ShowChat
        {
            get => Settings.ShowChat;
            set
            {
                if (Settings.ShowChat != value)
                {
                    Settings.ShowChat = value;
                    OnPropertyChangedWithValue(value, "ShowChat");
                    OnChatToggled?.Invoke(value);
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public bool MinimalMode
        {
            get => Settings.ChatMinimalMode;
            set
            {
                if (Settings.ChatMinimalMode != value)
                {
                    Settings.ChatMinimalMode = value;
                    OnPropertyChangedWithValue(value, "MinimalMode");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public string OffsetXText => Settings.ChatCustom.OffsetX.ToString("F0");

        [DataSourceProperty]
        public string OffsetYText => Settings.ChatCustom.OffsetY.ToString("F0");

        [DataSourceProperty]
        public string ScaleText => (Settings.ChatCustom.Scale * 100).ToString("F0") + "%";

        public void ExecuteIncreaseOffsetX() => AdjustOffset(Settings.ChatCustom, Constants.Adjustment.PositionStep, 0, RefreshDisplay);
        public void ExecuteDecreaseOffsetX() => AdjustOffset(Settings.ChatCustom, -Constants.Adjustment.PositionStep, 0, RefreshDisplay);
        public void ExecuteIncreaseOffsetY() => AdjustOffset(Settings.ChatCustom, 0, Constants.Adjustment.PositionStep, RefreshDisplay);
        public void ExecuteDecreaseOffsetY() => AdjustOffset(Settings.ChatCustom, 0, -Constants.Adjustment.PositionStep, RefreshDisplay);
        public void ExecuteIncreaseOffsetXLarge() => AdjustOffset(Settings.ChatCustom, Constants.Adjustment.PositionStepLarge, 0, RefreshDisplay);
        public void ExecuteDecreaseOffsetXLarge() => AdjustOffset(Settings.ChatCustom, -Constants.Adjustment.PositionStepLarge, 0, RefreshDisplay);
        public void ExecuteIncreaseOffsetYLarge() => AdjustOffset(Settings.ChatCustom, 0, Constants.Adjustment.PositionStepLarge, RefreshDisplay);
        public void ExecuteDecreaseOffsetYLarge() => AdjustOffset(Settings.ChatCustom, 0, -Constants.Adjustment.PositionStepLarge, RefreshDisplay);

        public void ExecuteIncreaseScale() => AdjustScale(Settings.ChatCustom, Constants.Adjustment.ScaleStep, RefreshDisplay);
        public void ExecuteDecreaseScale() => AdjustScale(Settings.ChatCustom, -Constants.Adjustment.ScaleStep, RefreshDisplay);
        public void ExecuteIncreaseScaleLarge() => AdjustScale(Settings.ChatCustom, Constants.Adjustment.ScaleStepLarge, RefreshDisplay);
        public void ExecuteDecreaseScaleLarge() => AdjustScale(Settings.ChatCustom, -Constants.Adjustment.ScaleStepLarge, RefreshDisplay);

        public void ExecuteDebugStructure()
        {
            OnDebugChatStructure?.Invoke();
        }

        public void ExecuteReset()
        {
            Settings.ChatCustom.Reset();
            Settings.ChatMinimalMode = false;
            RefreshAll();
        }

        private void RefreshDisplay()
        {
            OnPropertyChangedWithValue(OffsetXText, "OffsetXText");
            OnPropertyChangedWithValue(OffsetYText, "OffsetYText");
            OnPropertyChangedWithValue(ScaleText, "ScaleText");
            NotifyChanged();
        }

        public override void RefreshAll()
        {
            OnPropertyChangedWithValue(ShowChat, "ShowChat");
            OnPropertyChangedWithValue(MinimalMode, "MinimalMode");
            RefreshDisplay();
        }
    }
}