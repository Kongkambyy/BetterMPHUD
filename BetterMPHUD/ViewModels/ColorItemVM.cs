using System;
using TaleWorlds.Library;

namespace BetterMPHUD.ViewModels
{
    public class ColorItemVM : ViewModel
    {
        private bool _isSelected;
        private string _colorHex;
        private Action<ColorItemVM> _onSelected;

        public ColorItemVM(string colorHex, Action<ColorItemVM> onSelected)
        {
            _colorHex = colorHex;
            _onSelected = onSelected;
        }

        public void ExecuteSelectColor()
        {
            if (_onSelected != null)
                _onSelected(this);
        }

        [DataSourceProperty]
        public bool IsSelected
        {
            get { return _isSelected; }
            set 
            { 
                if (_isSelected != value) 
                { 
                    _isSelected = value; 
                    OnPropertyChangedWithValue(value, "IsSelected"); 
                } 
            }
        }

        [DataSourceProperty]
        public string ColorHex
        {
            get { return _colorHex; }
            set 
            { 
                if (_colorHex != value) 
                { 
                    _colorHex = value; 
                    OnPropertyChangedWithValue(value, "ColorHex"); 
                } 
            }
        }
    }
}