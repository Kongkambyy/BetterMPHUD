using TaleWorlds.Library;

namespace BetterMPHUD.ViewModels
{
    public class ReloadPhaseItemVM : ViewModel
    {
        private float _progress;
        private float _relativeDurationToMaxDuration;

        public ReloadPhaseItemVM(float progress, float relativeDurationToMaxDuration)
        {
            Update(progress, relativeDurationToMaxDuration);
        }

        public void Update(float progress, float relativeDurationToMaxDuration)
        {
            Progress = progress;
            RelativeDurationToMaxDuration = relativeDurationToMaxDuration;
        }

        [DataSourceProperty]
        public float Progress
        {
            get { return _progress; }
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChangedWithValue(value, "Progress");
                }
            }
        }

        [DataSourceProperty]
        public float RelativeDurationToMaxDuration
        {
            get { return _relativeDurationToMaxDuration; }
            set
            {
                if (_relativeDurationToMaxDuration != value)
                {
                    _relativeDurationToMaxDuration = value;
                    OnPropertyChangedWithValue(value, "RelativeDurationToMaxDuration");
                }
            }
        }
    }
}