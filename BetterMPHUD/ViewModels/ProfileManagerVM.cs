using System;
using TaleWorlds.Library;

namespace BetterMPHUD.ViewModels
{
    public class ProfileManagerVM : ViewModel
    {
        private int _selectedProfileIndex;
        private int _newProfileCounter = 1;
        private string _tempProfileName;
        private readonly Action _onProfileChanged;
        private Action _onPropertyRefresh;

        public ProfileManagerVM(Action onProfileChanged)
        {
            _onProfileChanged = onProfileChanged;
            _selectedProfileIndex = ProfileManager.GetActiveProfileIndex();
            _tempProfileName = ProfileManager.GetActiveProfile().Name;
        }

        public void SetOnPropertyRefresh(Action onPropertyRefresh)
        {
            _onPropertyRefresh = onPropertyRefresh;
        }

        [DataSourceProperty]
        public string CurrentProfileName
        {
            get => _tempProfileName;
            set
            {
                if (_tempProfileName != value)
                {
                    _tempProfileName = value;
                    OnPropertyChanged("CurrentProfileName");
                    OnPropertyChanged("IsSaveNameButtonVisible");
                    _onPropertyRefresh?.Invoke();
                }
            }
        }

        [DataSourceProperty]
        public bool IsSaveNameButtonVisible => _tempProfileName != ProfileManager.GetActiveProfile().Name;

        [DataSourceProperty]
        public string ProfileCountText => (_selectedProfileIndex + 1) + "/" + ProfileManager.GetProfileCount();

        [DataSourceProperty]
        public bool CanDeleteProfile => ProfileManager.GetProfileCount() > 1;

        public void ExecuteNextProfile()
        {
            int count = ProfileManager.GetProfileCount();
            if (count <= 1) return;

            _selectedProfileIndex = (_selectedProfileIndex + 1) % count;
            ProfileManager.SetActiveProfileByIndex(_selectedProfileIndex);
            OnProfileSwitched();
        }

        public void ExecutePreviousProfile()
        {
            int count = ProfileManager.GetProfileCount();
            if (count <= 1) return;

            _selectedProfileIndex = (_selectedProfileIndex - 1 + count) % count;
            ProfileManager.SetActiveProfileByIndex(_selectedProfileIndex);
            OnProfileSwitched();
        }

        public void ExecuteCreateProfile()
        {
            string newName = "Profile " + _newProfileCounter;
            while (ProfileManager.GetProfileNames().Contains(newName))
            {
                _newProfileCounter++;
                newName = "Profile " + _newProfileCounter;
            }

            if (ProfileManager.CreateProfile(newName))
            {
                _newProfileCounter++;
                RefreshDisplay();
            }
        }

        public void ExecuteDuplicateProfile()
        {
            string currentName = ProfileManager.GetActiveProfile().Name;
            string newName = currentName + " Copy";
            int copyNum = 1;

            while (ProfileManager.GetProfileNames().Contains(newName))
            {
                copyNum++;
                newName = currentName + " Copy " + copyNum;
            }

            if (ProfileManager.DuplicateProfile(currentName, newName))
            {
                ProfileManager.SetActiveProfile(newName);
                _selectedProfileIndex = ProfileManager.GetActiveProfileIndex();
                OnProfileSwitched();
            }
        }

        public void ExecuteDeleteProfile()
        {
            string currentName = ProfileManager.GetActiveProfile().Name;
            if (ProfileManager.DeleteProfile(currentName))
            {
                _selectedProfileIndex = ProfileManager.GetActiveProfileIndex();
                OnProfileSwitched();
            }
        }

        public void ExecuteSaveProfileName()
        {
            string currentRealName = ProfileManager.GetActiveProfile().Name;
            bool success = ProfileManager.RenameProfile(currentRealName, _tempProfileName);

            if (!success)
            {
                _tempProfileName = currentRealName;
                OnPropertyChanged("CurrentProfileName");
            }
            
            OnPropertyChanged("IsSaveNameButtonVisible");
            _onPropertyRefresh?.Invoke();
        }

        private void OnProfileSwitched()
        {
            _tempProfileName = ProfileManager.GetActiveProfile().Name;
            RefreshDisplay();
            _onProfileChanged?.Invoke();
        }

        private void RefreshDisplay()
        {
            OnPropertyChangedWithValue(CurrentProfileName, "CurrentProfileName");
            OnPropertyChangedWithValue(ProfileCountText, "ProfileCountText");
            OnPropertyChangedWithValue(CanDeleteProfile, "CanDeleteProfile");
            OnPropertyChangedWithValue(IsSaveNameButtonVisible, "IsSaveNameButtonVisible");
            _onPropertyRefresh?.Invoke();
        }

        public void RefreshAll()
        {
            _selectedProfileIndex = ProfileManager.GetActiveProfileIndex();
            _tempProfileName = ProfileManager.GetActiveProfile().Name;
            RefreshDisplay();
        }
    }
}