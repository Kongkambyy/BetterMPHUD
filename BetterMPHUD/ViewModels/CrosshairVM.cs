using System;
using TaleWorlds.Library;

namespace BetterMPHUD.ViewModels
{
    public class CrosshairVM : ViewModel
    {
        private bool _customEnabled;
        private bool _isVisible;
        private bool _isTargetInvalid;
        private bool _isHitMarkerVisible;
        private bool _isVictimDead;
        private bool _isHumanoidHeadshot;

        private double _crosshairAccuracy;
        private double _crosshairScale = 1.0;
        private int _crosshairType;

        private string _crosshairColor = "#FF0000FF";
        private float _crosshairOpacity = 1f;
        private int _crosshairSizeHor = 10;
        private int _crosshairSizeVert = 10;
        private int _crosshairOffset = 10;

        private double _topArrowOpacity;
        private double _bottomArrowOpacity;
        private double _leftArrowOpacity;
        private double _rightArrowOpacity;

        public void ShowHitMarker(bool isFatal, bool isHeadshot)
        {
            IsHitMarkerVisible = true;
            IsVictimDead = isFatal;
            IsHumanoidHeadshot = isHeadshot;
        }

        [DataSourceProperty]
        public bool CustomEnabled
        {
            get { return _customEnabled; }
            set { if (_customEnabled != value) { _customEnabled = value; OnPropertyChangedWithValue(value, "CustomEnabled"); } }
        }

        [DataSourceProperty]
        public bool IsVisible
        {
            get { return _isVisible; }
            set { if (_isVisible != value) { _isVisible = value; OnPropertyChangedWithValue(value, "IsVisible"); } }
        }

        [DataSourceProperty]
        public bool IsTargetInvalid
        {
            get { return _isTargetInvalid; }
            set { if (_isTargetInvalid != value) { _isTargetInvalid = value; OnPropertyChangedWithValue(value, "IsTargetInvalid"); } }
        }

        [DataSourceProperty]
        public bool IsHitMarkerVisible
        {
            get { return _isHitMarkerVisible; }
            set { if (_isHitMarkerVisible != value) { _isHitMarkerVisible = value; OnPropertyChangedWithValue(value, "IsHitMarkerVisible"); } }
        }

        [DataSourceProperty]
        public bool IsVictimDead
        {
            get { return _isVictimDead; }
            set { if (_isVictimDead != value) { _isVictimDead = value; OnPropertyChangedWithValue(value, "IsVictimDead"); } }
        }

        [DataSourceProperty]
        public bool IsHumanoidHeadshot
        {
            get { return _isHumanoidHeadshot; }
            set { if (_isHumanoidHeadshot != value) { _isHumanoidHeadshot = value; OnPropertyChangedWithValue(value, "IsHumanoidHeadshot"); } }
        }

        [DataSourceProperty]
        public double CrosshairAccuracy
        {
            get { return _crosshairAccuracy; }
            set { if (_crosshairAccuracy != value) { _crosshairAccuracy = value; OnPropertyChangedWithValue(value, "CrosshairAccuracy"); } }
        }

        [DataSourceProperty]
        public double CrosshairScale
        {
            get { return _crosshairScale; }
            set { if (_crosshairScale != value) { _crosshairScale = value; OnPropertyChangedWithValue(value, "CrosshairScale"); } }
        }

        [DataSourceProperty]
        public int CrosshairType
        {
            get { return _crosshairType; }
            set { if (_crosshairType != value) { _crosshairType = value; OnPropertyChangedWithValue(value, "CrosshairType"); } }
        }

        [DataSourceProperty]
        public string CrosshairColor
        {
            get { return _crosshairColor; }
            set { if (_crosshairColor != value) { _crosshairColor = value; OnPropertyChangedWithValue(value, "CrosshairColor"); } }
        }

        [DataSourceProperty]
        public float CrosshairOpacity
        {
            get { return _crosshairOpacity; }
            set { if (_crosshairOpacity != value) { _crosshairOpacity = value; OnPropertyChangedWithValue(value, "CrosshairOpacity"); } }
        }

        [DataSourceProperty]
        public int CrosshairSizeHor
        {
            get { return _crosshairSizeHor; }
            set { if (_crosshairSizeHor != value) { _crosshairSizeHor = value; OnPropertyChangedWithValue(value, "CrosshairSizeHor"); OnPropertyChangedWithValue((float)value, "CrosshairSizeHorFloat"); } }
        }

        [DataSourceProperty]
        public int CrosshairSizeVert
        {
            get { return _crosshairSizeVert; }
            set { if (_crosshairSizeVert != value) { _crosshairSizeVert = value; OnPropertyChangedWithValue(value, "CrosshairSizeVert"); OnPropertyChangedWithValue((float)value, "CrosshairSizeVertFloat"); } }
        }

        [DataSourceProperty]
        public int CrosshairOffset
        {
            get { return _crosshairOffset; }
            set { if (_crosshairOffset != value) { _crosshairOffset = value; OnPropertyChangedWithValue(value, "CrosshairOffset"); OnPropertyChangedWithValue((float)value, "CrosshairOffsetFloat"); OnPropertyChangedWithValue((float)(-value), "CrosshairOffsetFloatInvert"); } }
        }

        [DataSourceProperty]
        public float CrosshairSizeHorFloat { get { return _crosshairSizeHor; } }

        [DataSourceProperty]
        public float CrosshairSizeVertFloat { get { return _crosshairSizeVert; } }

        [DataSourceProperty]
        public float CrosshairOffsetFloat { get { return _crosshairOffset; } }

        [DataSourceProperty]
        public float CrosshairOffsetFloatInvert { get { return -_crosshairOffset; } }

        [DataSourceProperty]
        public double TopArrowOpacity
        {
            get { return _topArrowOpacity; }
            set { if (_topArrowOpacity != value) { _topArrowOpacity = value; OnPropertyChangedWithValue(value, "TopArrowOpacity"); } }
        }

        [DataSourceProperty]
        public double BottomArrowOpacity
        {
            get { return _bottomArrowOpacity; }
            set { if (_bottomArrowOpacity != value) { _bottomArrowOpacity = value; OnPropertyChangedWithValue(value, "BottomArrowOpacity"); } }
        }

        [DataSourceProperty]
        public double LeftArrowOpacity
        {
            get { return _leftArrowOpacity; }
            set { if (_leftArrowOpacity != value) { _leftArrowOpacity = value; OnPropertyChangedWithValue(value, "LeftArrowOpacity"); } }
        }

        [DataSourceProperty]
        public double RightArrowOpacity
        {
            get { return _rightArrowOpacity; }
            set { if (_rightArrowOpacity != value) { _rightArrowOpacity = value; OnPropertyChangedWithValue(value, "RightArrowOpacity"); } }
        }
    }
}