using System;
using TaleWorlds.Library;

namespace BetterMPHUD.ViewModels.Settings
{
    public class MiscSettingsVM : BaseSettingsVM
    {
        private bool _isCapturingConfigMenuHotkey;

        private static readonly string[] ConfigMenuHotkeys =
        {
            "F1",
            "F2",
            "F3",
            "F4",
            "F5",
            "F6",
            "F7",
            "F8",
            "F9",
            "F10",
            "F11",
            "F12",
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "I",
            "J",
            "K",
            "L",
            "M",
            "N",
            "O",
            "P",
            "Q",
            "R",
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z",
            "Numpad0",
            "Numpad1",
            "Numpad2",
            "Numpad3",
            "Numpad4",
            "Numpad5",
            "Numpad6",
            "Numpad7",
            "Numpad8",
            "Numpad9",
            "NumpadPlus",
            "NumpadMinus",
            "NumpadMultiply",
            "NumpadSlash",
            "NumpadPeriod",
            "NumpadEnter",
        };

        public MiscSettingsVM(HudSettings settings, Action onSettingsChanged) 
            : base(settings, onSettingsChanged)
        {
        }

        [DataSourceProperty]
        public bool CameraSnapbackEnabled
        {
            get => Settings.CameraSnapbackEnabled;
            set
            {
                if (Settings.CameraSnapbackEnabled != value)
                {
                    Settings.CameraSnapbackEnabled = value;
                    OnPropertyChangedWithValue(value, "CameraSnapbackEnabled");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public string ConfigMenuHotkeyText
        {
            get
            {
                if (_isCapturingConfigMenuHotkey)
                    return "Press key...";

                EnsureConfigMenuHotkey();
                return Settings.ConfigMenuHotkey;
            }
        }

        public bool IsCapturingConfigMenuHotkey
        {
            get => _isCapturingConfigMenuHotkey;
        }

        public override void RefreshAll()
        {
            OnPropertyChangedWithValue(CameraSnapbackEnabled, "CameraSnapbackEnabled");
            OnPropertyChangedWithValue(ConfigMenuHotkeyText, "ConfigMenuHotkeyText");
            NotifyChanged();
        }

        public void ExecuteCaptureConfigMenuHotkey()
        {
            _isCapturingConfigMenuHotkey = true;
            OnPropertyChangedWithValue(ConfigMenuHotkeyText, "ConfigMenuHotkeyText");
            NotifyPropertyRefresh();
        }

        public void ExecutePreviousConfigMenuHotkey()
        {
            AdjustConfigMenuHotkey(-1);
        }

        public void ExecuteNextConfigMenuHotkey()
        {
            AdjustConfigMenuHotkey(1);
        }

        public void ExecuteResetConfigMenuHotkey()
        {
            _isCapturingConfigMenuHotkey = false;
            if (Settings.ConfigMenuHotkey == "F10")
            {
                OnPropertyChangedWithValue(ConfigMenuHotkeyText, "ConfigMenuHotkeyText");
                NotifyPropertyRefresh();
                return;
            }

            Settings.ConfigMenuHotkey = "F10";
            OnPropertyChangedWithValue(ConfigMenuHotkeyText, "ConfigMenuHotkeyText");
            NotifyChanged();
        }

        public void CancelConfigMenuHotkeyCapture()
        {
            if (!_isCapturingConfigMenuHotkey)
                return;

            _isCapturingConfigMenuHotkey = false;
            OnPropertyChangedWithValue(ConfigMenuHotkeyText, "ConfigMenuHotkeyText");
            NotifyPropertyRefresh();
        }

        public bool TrySetCapturedConfigMenuHotkey(string keyName)
        {
            if (!_isCapturingConfigMenuHotkey || !IsBindableConfigMenuHotkey(keyName))
                return false;

            Settings.ConfigMenuHotkey = keyName;
            _isCapturingConfigMenuHotkey = false;
            OnPropertyChangedWithValue(ConfigMenuHotkeyText, "ConfigMenuHotkeyText");
            NotifyChanged();
            return true;
        }

        private void AdjustConfigMenuHotkey(int delta)
        {
            _isCapturingConfigMenuHotkey = false;
            EnsureConfigMenuHotkey();
            int index = Array.IndexOf(ConfigMenuHotkeys, Settings.ConfigMenuHotkey);
            if (index < 0)
                index = Array.IndexOf(ConfigMenuHotkeys, "F10");

            index += delta;
            if (index < 0)
                index = ConfigMenuHotkeys.Length - 1;
            else if (index >= ConfigMenuHotkeys.Length)
                index = 0;

            Settings.ConfigMenuHotkey = ConfigMenuHotkeys[index];
            OnPropertyChangedWithValue(ConfigMenuHotkeyText, "ConfigMenuHotkeyText");
            NotifyChanged();
        }

        private void EnsureConfigMenuHotkey()
        {
            if (!IsBindableConfigMenuHotkey(Settings.ConfigMenuHotkey))
                Settings.ConfigMenuHotkey = "F10";
        }

        private static bool IsBindableConfigMenuHotkey(string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName))
                return false;

            if (keyName == "Escape" || keyName == "Extended" || keyName == "Invalid")
                return false;

            if (keyName.IndexOf("Mouse", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            if (keyName.IndexOf("Controller", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            return true;
        }
    }
}
