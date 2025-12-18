using TaleWorlds.Library;

namespace BetterMPHUD.ViewModels
{
    public class KillfeedVM : ViewModel
    {
        private MBBindingList<KillfeedItemVM> _killList;
        private bool _isVisible;

        public KillfeedVM()
        {
            _killList = new MBBindingList<KillfeedItemVM>();
            _isVisible = false;
        }

        public void AddKill(KillfeedItemVM item)
        {
            if (_killList.Count >= 15)
            {
                _killList.RemoveAt(0);
            }
            _killList.Add(item);
        }

        public void RemoveKill(KillfeedItemVM item)
        {
            _killList.Remove(item);
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
    }
}