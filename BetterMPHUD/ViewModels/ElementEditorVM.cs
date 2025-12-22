using System;
using TaleWorlds.Library;
using BetterMPHUD.Core;

namespace BetterMPHUD.ViewModels
{
    public class ElementEditorVM : ViewModel
    {
        private ElementCustomization _currentCustomization;
        private Action _onChanged;

        private int _selectedIndex;
        private string _selectedName;
        private float _offsetX;
        private float _offsetY;
        private float _scale;

        public ElementEditorVM(int initialIndex, ElementCustomization initialCustomization, string initialName)
        {
            _selectedIndex = initialIndex;
            _selectedName = initialName;
            _currentCustomization = initialCustomization;
            LoadValues();
        }

        public void SetOnChanged(Action onChanged)
        {
            _onChanged = onChanged;
        }

        public void SelectElement(int index, string name, ElementCustomization customization)
        {
            _selectedIndex = index;
            _selectedName = name;
            _currentCustomization = customization;
            OnPropertyChangedWithValue(_selectedIndex, "SelectedIndex");
            OnPropertyChangedWithValue(_selectedName, "SelectedName");
            LoadValues();
        }

        private void LoadValues()
        {
            if (_currentCustomization == null) return;
            
            _offsetX = _currentCustomization.OffsetX;
            _offsetY = _currentCustomization.OffsetY;
            _scale = _currentCustomization.Scale;
            RefreshAllDisplays();
        }

        private void SaveValues()
        {
            if (_currentCustomization == null) return;
            
            _currentCustomization.OffsetX = _offsetX;
            _currentCustomization.OffsetY = _offsetY;
            _currentCustomization.Scale = _scale;
            
            if (_onChanged != null)
                _onChanged();
        }

        private void RefreshAllDisplays()
        {
            OnPropertyChangedWithValue(_offsetX, "OffsetX");
            OnPropertyChangedWithValue(_offsetY, "OffsetY");
            OnPropertyChangedWithValue(_scale, "Scale");
            OnPropertyChangedWithValue(OffsetXText, "OffsetXText");
            OnPropertyChangedWithValue(OffsetYText, "OffsetYText");
            OnPropertyChangedWithValue(ScaleText, "ScaleText");
        }

        public void AdjustOffsetX(float delta) 
        { 
            _offsetX += delta; 
            RefreshOffset(); 
        }
        
        public void AdjustOffsetY(float delta) 
        { 
            _offsetY += delta; 
            RefreshOffset(); 
        }

        public void AdjustScale(float delta)
        {
            float newScale = _scale + delta;
            if (newScale >= Constants.Adjustment.MinScale && newScale <= Constants.Adjustment.MaxScale)
            {
                _scale = (float)Math.Round(newScale, 2);
                OnPropertyChangedWithValue(_scale, "Scale");
                OnPropertyChangedWithValue(ScaleText, "ScaleText");
                SaveValues();
            }
        }

        private void RefreshOffset()
        {
            OnPropertyChangedWithValue(_offsetX, "OffsetX");
            OnPropertyChangedWithValue(_offsetY, "OffsetY");
            OnPropertyChangedWithValue(OffsetXText, "OffsetXText");
            OnPropertyChangedWithValue(OffsetYText, "OffsetYText");
            SaveValues();
        }

        public void Reset()
        {
            _offsetX = 0f;
            _offsetY = 0f;
            _scale = 1f;
            RefreshAllDisplays();
            SaveValues();
        }

        [DataSourceProperty]
        public int SelectedIndex 
        { 
            get { return _selectedIndex; }
            set 
            { 
                if (_selectedIndex != value) 
                { 
                    _selectedIndex = value; 
                    OnPropertyChangedWithValue(value, "SelectedIndex"); 
                } 
            }
        }

        [DataSourceProperty]
        public string SelectedName 
        { 
            get { return _selectedName; }
            set 
            { 
                if (_selectedName != value) 
                { 
                    _selectedName = value; 
                    OnPropertyChangedWithValue(value, "SelectedName"); 
                } 
            }
        }

        [DataSourceProperty]
        public float OffsetX { get { return _offsetX; } }
        
        [DataSourceProperty]
        public float OffsetY { get { return _offsetY; } }
        
        [DataSourceProperty]
        public float Scale { get { return _scale; } }
        
        [DataSourceProperty]
        public string OffsetXText { get { return _offsetX.ToString("F0"); } }
        
        [DataSourceProperty]
        public string OffsetYText { get { return _offsetY.ToString("F0"); } }
        
        [DataSourceProperty]
        public string ScaleText { get { return (_scale * 100).ToString("F0") + "%"; } }
    }
}