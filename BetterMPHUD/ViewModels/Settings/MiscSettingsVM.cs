using System;
using TaleWorlds.Library;

namespace BetterMPHUD.ViewModels.Settings
{
    public class MiscSettingsVM : BaseSettingsVM
    {
        private bool _isCapturingConfigMenuHotkey;
        private bool _isCapturingScoreboardShowCursorHotkey;

        private static readonly string[] KeyboardHotkeys =
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

        private static readonly string[] ScoreboardShowCursorHotkeys =
        {
            "X1MouseButton",
            "X2MouseButton",
            "MiddleMouseButton",
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

        [DataSourceProperty]
        public string ScoreboardShowCursorHotkeyText
        {
            get
            {
                if (_isCapturingScoreboardShowCursorHotkey)
                    return "Press key...";

                EnsureScoreboardShowCursorHotkey();
                return FormatHotkeyText(Settings.ScoreboardShowCursorHotkey);
            }
        }

        public bool IsCapturingConfigMenuHotkey
        {
            get => _isCapturingConfigMenuHotkey;
        }

        public bool IsCapturingScoreboardShowCursorHotkey
        {
            get => _isCapturingScoreboardShowCursorHotkey;
        }

        public override void RefreshAll()
        {
            OnPropertyChangedWithValue(CameraSnapbackEnabled, "CameraSnapbackEnabled");
            OnPropertyChangedWithValue(ConfigMenuHotkeyText, "ConfigMenuHotkeyText");
            OnPropertyChangedWithValue(ScoreboardShowCursorHotkeyText, "ScoreboardShowCursorHotkeyText");
            NotifyChanged();
        }

        public void ExecuteCaptureConfigMenuHotkey()
        {
            _isCapturingScoreboardShowCursorHotkey = false;
            _isCapturingConfigMenuHotkey = true;
            OnPropertyChangedWithValue(ConfigMenuHotkeyText, "ConfigMenuHotkeyText");
            OnPropertyChangedWithValue(ScoreboardShowCursorHotkeyText, "ScoreboardShowCursorHotkeyText");
            NotifyPropertyRefresh();
        }

        public void ExecuteCaptureScoreboardShowCursorHotkey()
        {
            _isCapturingConfigMenuHotkey = false;
            _isCapturingScoreboardShowCursorHotkey = true;
            OnPropertyChangedWithValue(ConfigMenuHotkeyText, "ConfigMenuHotkeyText");
            OnPropertyChangedWithValue(ScoreboardShowCursorHotkeyText, "ScoreboardShowCursorHotkeyText");
            NotifyPropertyRefresh();
        }

        public void ExecutePreviousConfigMenuHotkey()
        {
            AdjustConfigMenuHotkey(-1, KeyboardHotkeys);
        }

        public void ExecuteNextConfigMenuHotkey()
        {
            AdjustConfigMenuHotkey(1, KeyboardHotkeys);
        }

        public void ExecutePreviousScoreboardShowCursorHotkey()
        {
            AdjustScoreboardShowCursorHotkey(-1);
        }

        public void ExecuteNextScoreboardShowCursorHotkey()
        {
            AdjustScoreboardShowCursorHotkey(1);
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

        public void ExecuteResetScoreboardShowCursorHotkey()
        {
            _isCapturingScoreboardShowCursorHotkey = false;
            if (Settings.ScoreboardShowCursorHotkey == "X1MouseButton")
            {
                OnPropertyChangedWithValue(ScoreboardShowCursorHotkeyText, "ScoreboardShowCursorHotkeyText");
                NotifyPropertyRefresh();
                return;
            }

            Settings.ScoreboardShowCursorHotkey = "X1MouseButton";
            OnPropertyChangedWithValue(ScoreboardShowCursorHotkeyText, "ScoreboardShowCursorHotkeyText");
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

        public void CancelScoreboardShowCursorHotkeyCapture()
        {
            if (!_isCapturingScoreboardShowCursorHotkey)
                return;

            _isCapturingScoreboardShowCursorHotkey = false;
            OnPropertyChangedWithValue(ScoreboardShowCursorHotkeyText, "ScoreboardShowCursorHotkeyText");
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

        public bool TrySetCapturedScoreboardShowCursorHotkey(string keyName)
        {
            if (!_isCapturingScoreboardShowCursorHotkey || !IsBindableScoreboardShowCursorHotkey(keyName))
                return false;

            Settings.ScoreboardShowCursorHotkey = keyName;
            _isCapturingScoreboardShowCursorHotkey = false;
            OnPropertyChangedWithValue(ScoreboardShowCursorHotkeyText, "ScoreboardShowCursorHotkeyText");
            NotifyChanged();
            return true;
        }

        private void AdjustConfigMenuHotkey(int delta, string[] hotkeys)
        {
            _isCapturingConfigMenuHotkey = false;
            EnsureConfigMenuHotkey();
            int index = Array.IndexOf(hotkeys, Settings.ConfigMenuHotkey);
            if (index < 0)
                index = Array.IndexOf(hotkeys, "F10");

            index += delta;
            if (index < 0)
                index = hotkeys.Length - 1;
            else if (index >= hotkeys.Length)
                index = 0;

            Settings.ConfigMenuHotkey = hotkeys[index];
            OnPropertyChangedWithValue(ConfigMenuHotkeyText, "ConfigMenuHotkeyText");
            NotifyChanged();
        }

        private void AdjustScoreboardShowCursorHotkey(int delta)
        {
            _isCapturingScoreboardShowCursorHotkey = false;
            EnsureScoreboardShowCursorHotkey();
            int index = Array.IndexOf(ScoreboardShowCursorHotkeys, Settings.ScoreboardShowCursorHotkey);
            if (index < 0)
                index = Array.IndexOf(ScoreboardShowCursorHotkeys, "X1MouseButton");

            index += delta;
            if (index < 0)
                index = ScoreboardShowCursorHotkeys.Length - 1;
            else if (index >= ScoreboardShowCursorHotkeys.Length)
                index = 0;

            Settings.ScoreboardShowCursorHotkey = ScoreboardShowCursorHotkeys[index];
            OnPropertyChangedWithValue(ScoreboardShowCursorHotkeyText, "ScoreboardShowCursorHotkeyText");
            NotifyChanged();
        }

        private void EnsureConfigMenuHotkey()
        {
            if (!IsBindableConfigMenuHotkey(Settings.ConfigMenuHotkey))
                Settings.ConfigMenuHotkey = "F10";
        }

        private void EnsureScoreboardShowCursorHotkey()
        {
            if (!IsBindableScoreboardShowCursorHotkey(Settings.ScoreboardShowCursorHotkey))
                Settings.ScoreboardShowCursorHotkey = "X1MouseButton";
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

        private static bool IsBindableScoreboardShowCursorHotkey(string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName))
                return false;

            if (keyName == "Escape" || keyName == "Extended" || keyName == "Invalid")
                return false;

            if (keyName.IndexOf("Controller", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            if (string.Equals(keyName, "LeftMouseButton", StringComparison.OrdinalIgnoreCase)
                || string.Equals(keyName, "RightMouseButton", StringComparison.OrdinalIgnoreCase)
                || string.Equals(keyName, "MouseScrollUp", StringComparison.OrdinalIgnoreCase)
                || string.Equals(keyName, "MouseScrollDown", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private static string FormatHotkeyText(string keyName)
        {
            if (string.Equals(keyName, "X1MouseButton", StringComparison.OrdinalIgnoreCase))
                return "M4";

            if (string.Equals(keyName, "X2MouseButton", StringComparison.OrdinalIgnoreCase))
                return "M5";

            if (string.Equals(keyName, "MiddleMouseButton", StringComparison.OrdinalIgnoreCase))
                return "M3";

            return keyName;
        }
    }
}
