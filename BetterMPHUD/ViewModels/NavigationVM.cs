using System;
using TaleWorlds.Library;

namespace BetterMPHUD.ViewModels
{
    public class NavigationVM : ViewModel
    {
        private string _currentPage = "Killfeed";
        private Action _onPageChanged;

        public void SetOnPageChanged(Action onPageChanged)
        {
            _onPageChanged = onPageChanged;
        }

        public void SetPage(string page)
        {
            _currentPage = page;
            RefreshPageVisibility();
            _onPageChanged?.Invoke();
        }

        public void ExecuteOpenKillfeedPage() => SetPage("Killfeed");
        public void ExecuteOpenTopBarPage() => SetPage("TopBar");
        public void ExecuteOpenVisibilityPage() => SetPage("Visibility");
        public void ExecuteOpenCustomizePage() => SetPage("Customize");
        public void ExecuteOpenChatPage() => SetPage("Chat");
        public void ExecuteOpenLeaderboardPage() => SetPage("Leaderboard");
        public void ExecuteOpenHPPage() => SetPage("HP");
        public void ExecuteOpenHPVisibilityPage() => SetPage("HPVisibility");
        public void ExecuteOpenHPCustomizePage() => SetPage("HPCustomize");
        public void ExecuteOpenCrosshairPage() => SetPage("Crosshair");
        public void ExecuteOpenCrosshairSettingsPage() => SetPage("CrosshairSettings");
        public void ExecuteOpenDotSettingsPage() => SetPage("DotSettings");
        public void ExecuteOpenMiscPage() => SetPage("Misc");
        public void ExecuteOpenProfilesPage() => SetPage("Profiles");
        public void ExecuteOpenAvatarSidesPage() => SetPage("AvatarSides");
        public void ExecuteOpenScoreboardPage() => SetPage("Scoreboard");

        public void ExecuteBackToTopBar() => SetPage("TopBar");
        public void ExecuteBackToHP() => SetPage("HP");
        public void ExecuteBackToCrosshair() => SetPage("Crosshair");
        public void ExecuteBackToCustomize() => SetPage("Customize");

        [DataSourceProperty] public bool IsKillfeedPageOpen => _currentPage == "Killfeed";
        [DataSourceProperty] public bool IsTopBarPageOpen => _currentPage == "TopBar";
        [DataSourceProperty] public bool IsVisibilityPageOpen => _currentPage == "Visibility";
        [DataSourceProperty] public bool IsCustomizePageOpen => _currentPage == "Customize";
        [DataSourceProperty] public bool IsChatPageOpen => _currentPage == "Chat";
        [DataSourceProperty] public bool IsLeaderboardPageOpen => _currentPage == "Leaderboard";
        [DataSourceProperty] public bool IsHPPageOpen => _currentPage == "HP";
        [DataSourceProperty] public bool IsHPVisibilityPageOpen => _currentPage == "HPVisibility";
        [DataSourceProperty] public bool IsHPCustomizePageOpen => _currentPage == "HPCustomize";
        [DataSourceProperty] public bool IsCrosshairPageOpen => _currentPage == "Crosshair";
        [DataSourceProperty] public bool IsCrosshairSettingsPageOpen => _currentPage == "CrosshairSettings";
        [DataSourceProperty] public bool IsDotSettingsPageOpen => _currentPage == "DotSettings";
        [DataSourceProperty] public bool IsMiscPageOpen => _currentPage == "Misc";
        [DataSourceProperty] public bool IsProfilesPageOpen => _currentPage == "Profiles";
        [DataSourceProperty] public bool IsAvatarSidesPageOpen => _currentPage == "AvatarSides";
        [DataSourceProperty] public bool IsScoreboardPageOpen => _currentPage == "Scoreboard";

        [DataSourceProperty] 
        public bool ShouldShiftMenuRight => IsCrosshairSettingsPageOpen || IsDotSettingsPageOpen;

        [DataSourceProperty]
        public float MenuHorizontalOffset => ShouldShiftMenuRight ? 610f : 0f;

        private void RefreshPageVisibility()
        {
            OnPropertyChangedWithValue(IsKillfeedPageOpen, "IsKillfeedPageOpen");
            OnPropertyChangedWithValue(IsTopBarPageOpen, "IsTopBarPageOpen");
            OnPropertyChangedWithValue(IsVisibilityPageOpen, "IsVisibilityPageOpen");
            OnPropertyChangedWithValue(IsCustomizePageOpen, "IsCustomizePageOpen");
            OnPropertyChangedWithValue(IsChatPageOpen, "IsChatPageOpen");
            OnPropertyChangedWithValue(IsLeaderboardPageOpen, "IsLeaderboardPageOpen");
            OnPropertyChangedWithValue(IsHPPageOpen, "IsHPPageOpen");
            OnPropertyChangedWithValue(IsHPVisibilityPageOpen, "IsHPVisibilityPageOpen");
            OnPropertyChangedWithValue(IsHPCustomizePageOpen, "IsHPCustomizePageOpen");
            OnPropertyChangedWithValue(IsCrosshairPageOpen, "IsCrosshairPageOpen");
            OnPropertyChangedWithValue(IsCrosshairSettingsPageOpen, "IsCrosshairSettingsPageOpen");
            OnPropertyChangedWithValue(IsDotSettingsPageOpen, "IsDotSettingsPageOpen");
            OnPropertyChangedWithValue(IsMiscPageOpen, "IsMiscPageOpen");
            OnPropertyChangedWithValue(IsProfilesPageOpen, "IsProfilesPageOpen");
            OnPropertyChangedWithValue(IsAvatarSidesPageOpen, "IsAvatarSidesPageOpen");
            OnPropertyChangedWithValue(IsScoreboardPageOpen, "IsScoreboardPageOpen");
            OnPropertyChangedWithValue(ShouldShiftMenuRight, "ShouldShiftMenuRight");
            OnPropertyChangedWithValue(MenuHorizontalOffset, "MenuHorizontalOffset");
        }
    }
}