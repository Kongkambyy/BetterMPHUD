using System;
using System.Linq;
using TaleWorlds.Library;
using BetterMPHUD.Core;

namespace BetterMPHUD.ViewModels
{
    public class KillfeedVM : ViewModel
    {
        private MBBindingList<KillfeedItemVM> _killList;
        private bool _isVisible;
        private bool _isPreviewMode;
        
        private const int BASE_FONT = 20;
        private const int BASE_ICON = 24;
        private const int BASE_SKULL = 34;
        private const int BASE_ROW = 32;

        public KillfeedVM()
        {
            _killList = new MBBindingList<KillfeedItemVM>();
            _isVisible = false;
            _isPreviewMode = false;
        }

        public bool IsPreviewMode => _isPreviewMode;

        public void ShowPreview(HudSettings settings)
        {
            if (_isPreviewMode) return;
            
            _isPreviewMode = true;
            Clear();
            
            float scale = settings.KillfeedCustom.Scale;
            GetScaledSizes(scale, out int font, out float icon, out float skull, out float row);

            string[] victims = new[]
            {
                "Pacemaker",
                "AXDER",
                "Arni",
                "Hairless",
                "Beast the Destiny Demon",
            };

            foreach (string victim in victims)
            {
                var item = new KillfeedItemVM(
                    "[IE] Kamby",
                    "[DM] " + victim,
                    Constants.Colors.FriendlyKill,
                    Constants.Sprites.Sword,
                    Constants.Sprites.Sword,
                    Constants.Sprites.Death,
                    float.MaxValue,
                    null
                );
                
                item.UpdateSizes(font, icon, skull, row);
                item.UpdateBackground(
                    settings.KillfeedBackgroundEnabled,
                    settings.KillfeedBackgroundColor,
                    settings.KillfeedBackgroundOpacity
                );
                
                _killList.Add(item);
            }
            
            IsVisible = true;
        }

        public void HidePreview()
        {
            if (!_isPreviewMode) return;
            
            _isPreviewMode = false;
            Clear();
        }

        public void UpdatePreview(HudSettings settings)
        {
            if (!_isPreviewMode) return;
            
            float scale = settings.KillfeedCustom.Scale;
            GetScaledSizes(scale, out int font, out float icon, out float skull, out float row);

            foreach (var item in _killList)
            {
                item.UpdateSizes(font, icon, skull, row);
                item.UpdateBackground(
                    settings.KillfeedBackgroundEnabled,
                    settings.KillfeedBackgroundColor,
                    settings.KillfeedBackgroundOpacity
                );
            }
        }

        public void UpdateScale(float scale)
        {
            if (scale < 0.1f) scale = 0.1f;

            int newFont = (int)Math.Max(1, BASE_FONT * scale);
            float newIcon = Math.Max(1, BASE_ICON * scale);
            float newSkull = Math.Max(1, BASE_SKULL * scale);
            float newRow = Math.Max(1, BASE_ROW * scale);

            foreach (var item in _killList.ToList())
            {
                item.UpdateSizes(newFont, newIcon, newSkull, newRow);
            }
        }
        
        public void GetScaledSizes(float scale, out int font, out float icon, out float skull, out float row)
        {
            if (scale < 0.1f) scale = 0.1f;
            
            font = (int)Math.Max(1, BASE_FONT * scale);
            icon = Math.Max(1, BASE_ICON * scale);
            skull = Math.Max(1, BASE_SKULL * scale);
            row = Math.Max(1, BASE_ROW * scale);
        }

        public void AddKill(KillfeedItemVM item, int maxEntries)
        {
            if (_isPreviewMode) return;
            
            if (_killList.Count >= maxEntries)
            {
                _killList.RemoveAt(0);
            }
            _killList.Add(item);
        }

        public void RemoveKill(KillfeedItemVM item)
        {
            if (_killList.Contains(item))
            {
                _killList.Remove(item);
            }
        }

        public void Clear()
        {
            _killList.Clear();
        }

        [DataSourceProperty]
        public MBBindingList<KillfeedItemVM> KillList
        {
            get => _killList;
            set
            {
                if (_killList != value)
                {
                    _killList = value;
                    OnPropertyChangedWithValue(value, "KillList");
                }
            }
        }

        [DataSourceProperty]
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChangedWithValue(value, "IsVisible");
                }
            }
        }

        public void UpdateBackgrounds(bool show, string color, float opacity)
        {
            foreach (var item in _killList)
            {
                item.UpdateBackground(show, color, opacity);
            }
        }
    }
}