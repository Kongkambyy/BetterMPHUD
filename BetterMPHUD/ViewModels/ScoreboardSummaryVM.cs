using System;
using TaleWorlds.Library;

namespace BetterMPHUD.ViewModels
{
    public class ScoreboardSummaryVM : ViewModel
    {
        private bool _isVisible;
        private string _teamOneName = "Team 1";
        private string _teamTwoName = "Team 2";
        private string _teamOneTotals = "Alive 0 | K 0 | D 0 | Avg 0";
        private string _teamTwoTotals = "Alive 0 | K 0 | D 0 | Avg 0";
        private string _teamOneClasses = "Inf 0 | Arc 0 | Cav 0";
        private string _teamTwoClasses = "Inf 0 | Arc 0 | Cav 0";
        private string _teamOneRoundScore = "0";
        private string _teamTwoRoundScore = "0";
        private string _showCursorHotkeyText = "M4";
        private string _teamOneSearchText = "";
        private string _teamTwoSearchText = "";
        private string _toggleMuteText = "Mute All";
        private bool _hasMutedAll;
        private bool _backgroundVisible = true;
        private float _backgroundOpacity = 0.8f;
        private bool _teamSummaryVisible = true;
        private bool _showPing = true;
        private bool _showStateColumn = true;
        private bool _showWarlordsBanners;
        private bool _useAdvancedStats;
        private string _teamOneSortColumn = "Score";
        private string _teamTwoSortColumn = "Score";
        private bool _teamOneSortAscending;
        private bool _teamTwoSortAscending;
        private string _defaultSortColumn = "Score";
        private bool _defaultSortAscending;
        private bool _teamOneManualSort;
        private bool _teamTwoManualSort;
        private bool _isPlayerActionsActive;
        private string _selectedPlayerName = "";
        private ScoreboardPlayerVM _selectedPlayer;
        private bool _showWarlordsPlayerActions;
        private MBBindingList<ScoreboardPlayerActionVM> _playerActionList = new MBBindingList<ScoreboardPlayerActionVM>();
        private MBBindingList<ScoreboardPlayerVM> _teamOnePlayers = new MBBindingList<ScoreboardPlayerVM>();
        private MBBindingList<ScoreboardPlayerVM> _teamTwoPlayers = new MBBindingList<ScoreboardPlayerVM>();

        public Action OnShowCursorRequested;
        public Action<bool> OnToggleMuteRequested;
        public Action<string> OnInspectPlayerCardRequested;
        public Action<string> OnMutePermanentlyRequested;
        public Action<string> OnMuteTextRequested;
        public Action<string> OnMuteVoiceRequested;
        public Action<string> OnReportRequested;
        public Action<string, string> OnScoreboardPlayerActionRequested;

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

        [DataSourceProperty]
        public string TeamOneName
        {
            get => _teamOneName;
            set
            {
                if (_teamOneName != value)
                {
                    _teamOneName = value;
                    OnPropertyChangedWithValue(value, "TeamOneName");
                }
            }
        }

        [DataSourceProperty]
        public string TeamTwoName
        {
            get => _teamTwoName;
            set
            {
                if (_teamTwoName != value)
                {
                    _teamTwoName = value;
                    OnPropertyChangedWithValue(value, "TeamTwoName");
                }
            }
        }

        [DataSourceProperty]
        public string TeamOneTotals
        {
            get => _teamOneTotals;
            set
            {
                if (_teamOneTotals != value)
                {
                    _teamOneTotals = value;
                    OnPropertyChangedWithValue(value, "TeamOneTotals");
                }
            }
        }

        [DataSourceProperty]
        public string TeamTwoTotals
        {
            get => _teamTwoTotals;
            set
            {
                if (_teamTwoTotals != value)
                {
                    _teamTwoTotals = value;
                    OnPropertyChangedWithValue(value, "TeamTwoTotals");
                }
            }
        }

        [DataSourceProperty]
        public string TeamOneClasses
        {
            get => _teamOneClasses;
            set
            {
                if (_teamOneClasses != value)
                {
                    _teamOneClasses = value;
                    OnPropertyChangedWithValue(value, "TeamOneClasses");
                }
            }
        }

        [DataSourceProperty]
        public string TeamTwoClasses
        {
            get => _teamTwoClasses;
            set
            {
                if (_teamTwoClasses != value)
                {
                    _teamTwoClasses = value;
                    OnPropertyChangedWithValue(value, "TeamTwoClasses");
                }
            }
        }

        [DataSourceProperty]
        public string TeamOneRoundScore
        {
            get => _teamOneRoundScore;
            set
            {
                if (_teamOneRoundScore != value)
                {
                    _teamOneRoundScore = value;
                    OnPropertyChangedWithValue(value, "TeamOneRoundScore");
                }
            }
        }

        [DataSourceProperty]
        public string TeamTwoRoundScore
        {
            get => _teamTwoRoundScore;
            set
            {
                if (_teamTwoRoundScore != value)
                {
                    _teamTwoRoundScore = value;
                    OnPropertyChangedWithValue(value, "TeamTwoRoundScore");
                }
            }
        }

        [DataSourceProperty]
        public string ShowCursorHotkeyText
        {
            get => _showCursorHotkeyText;
            set
            {
                if (_showCursorHotkeyText != value)
                {
                    _showCursorHotkeyText = value;
                    OnPropertyChangedWithValue(value, "ShowCursorHotkeyText");
                }
            }
        }

        [DataSourceProperty]
        public string TeamOneSearchText
        {
            get => _teamOneSearchText;
            set
            {
                if (_teamOneSearchText != value)
                {
                    _teamOneSearchText = value;
                    OnPropertyChangedWithValue(value, "TeamOneSearchText");
                }
            }
        }

        [DataSourceProperty]
        public string TeamTwoSearchText
        {
            get => _teamTwoSearchText;
            set
            {
                if (_teamTwoSearchText != value)
                {
                    _teamTwoSearchText = value;
                    OnPropertyChangedWithValue(value, "TeamTwoSearchText");
                }
            }
        }

        public void ClearSearch()
        {
            ExecuteClearTeamOneSearch();
            ExecuteClearTeamTwoSearch();
        }

        [DataSourceProperty]
        public string ToggleMuteText
        {
            get => _toggleMuteText;
            set
            {
                if (_toggleMuteText != value)
                {
                    _toggleMuteText = value;
                    OnPropertyChangedWithValue(value, "ToggleMuteText");
                }
            }
        }

        [DataSourceProperty]
        public bool HasMutedAll
        {
            get => _hasMutedAll;
            set
            {
                if (_hasMutedAll != value)
                {
                    _hasMutedAll = value;
                    OnPropertyChangedWithValue(value, "HasMutedAll");
                }
            }
        }

        [DataSourceProperty]
        public bool BackgroundVisible
        {
            get => _backgroundVisible;
            set
            {
                if (_backgroundVisible != value)
                {
                    _backgroundVisible = value;
                    OnPropertyChangedWithValue(value, "BackgroundVisible");
                }
            }
        }

        [DataSourceProperty]
        public float BackgroundOpacity
        {
            get => _backgroundOpacity;
            set
            {
                if (Math.Abs(_backgroundOpacity - value) > 0.001f)
                {
                    _backgroundOpacity = value;
                    OnPropertyChangedWithValue(value, "BackgroundOpacity");
                }
            }
        }

        [DataSourceProperty]
        public bool TeamSummaryVisible
        {
            get => _teamSummaryVisible;
            set
            {
                if (_teamSummaryVisible != value)
                {
                    _teamSummaryVisible = value;
                    OnPropertyChangedWithValue(value, "TeamSummaryVisible");
                }
            }
        }

        [DataSourceProperty]
        public bool ShowPing
        {
            get => _showPing;
            set
            {
                if (_showPing != value)
                {
                    _showPing = value;
                    OnPropertyChangedWithValue(value, "ShowPing");
                }
            }
        }

        [DataSourceProperty]
        public bool ShowStateColumn
        {
            get => _showStateColumn;
            set
            {
                if (_showStateColumn != value)
                {
                    _showStateColumn = value;
                    OnPropertyChangedWithValue(value, "ShowStateColumn");
                    RefreshColumnLayout();
                }
            }
        }

        [DataSourceProperty]
        public bool ShowWarlordsBanners
        {
            get => _showWarlordsBanners;
            set
            {
                if (_showWarlordsBanners != value)
                {
                    _showWarlordsBanners = value;
                    OnPropertyChangedWithValue(value, "ShowWarlordsBanners");
                    RefreshColumnLayout();
                }
            }
        }

        [DataSourceProperty] public bool ShowStateAndBannerLayout => ShowStateColumn && ShowWarlordsBanners;
        [DataSourceProperty] public bool ShowStateOnlyLayout => ShowStateColumn && !ShowWarlordsBanners;
        [DataSourceProperty] public bool ShowBannerOnlyLayout => !ShowStateColumn && ShowWarlordsBanners;
        [DataSourceProperty] public bool ShowNameOnlyLayout => !ShowStateColumn && !ShowWarlordsBanners;

        [DataSourceProperty]
        public bool UseAdvancedStats
        {
            get => _useAdvancedStats;
            set
            {
                if (_useAdvancedStats != value)
                {
                    _useAdvancedStats = value;
                    OnPropertyChangedWithValue(value, "UseAdvancedStats");
                    OnPropertyChangedWithValue(UseBasicStats, "UseBasicStats");
                    RefreshSortHeaders();
                }
            }
        }

        [DataSourceProperty] public bool UseBasicStats => !UseAdvancedStats;

        [DataSourceProperty]
        public bool ShowWarlordsPlayerActions
        {
            get => _showWarlordsPlayerActions;
            set
            {
                if (_showWarlordsPlayerActions != value)
                {
                    _showWarlordsPlayerActions = value;
                    OnPropertyChangedWithValue(value, "ShowWarlordsPlayerActions");
                }
            }
        }

        [DataSourceProperty]
        public bool IsPlayerActionsActive
        {
            get => _isPlayerActionsActive;
            set
            {
                if (_isPlayerActionsActive != value)
                {
                    _isPlayerActionsActive = value;
                    OnPropertyChangedWithValue(value, "IsPlayerActionsActive");
                    if (!value)
                        ClearSelectedPlayer();
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<ScoreboardPlayerActionVM> PlayerActionList
        {
            get => _playerActionList;
            set
            {
                if (_playerActionList != value)
                {
                    _playerActionList = value;
                    OnPropertyChangedWithValue(value, "PlayerActionList");
                }
            }
        }

        public string GetSortColumn(bool first)
        {
            return first ? _teamOneSortColumn : _teamTwoSortColumn;
        }

        public bool GetSortAscending(bool first)
        {
            return first ? _teamOneSortAscending : _teamTwoSortAscending;
        }

        public void ApplyDefaultSort(string column, bool ascending)
        {
            string normalizedColumn = NormalizeSortColumn(column);
            if (_defaultSortColumn != normalizedColumn || _defaultSortAscending != ascending)
            {
                _defaultSortColumn = normalizedColumn;
                _defaultSortAscending = ascending;
                _teamOneManualSort = false;
                _teamTwoManualSort = false;
            }

            if (!_teamOneManualSort)
            {
                _teamOneSortColumn = _defaultSortColumn;
                _teamOneSortAscending = _defaultSortAscending;
            }

            if (!_teamTwoManualSort)
            {
                _teamTwoSortColumn = _defaultSortColumn;
                _teamTwoSortAscending = _defaultSortAscending;
            }

            RefreshSortHeaders();
        }

        [DataSourceProperty] public string TeamOneNameHeader => GetHeaderText(true, "Name", "Name");
        [DataSourceProperty] public string TeamOnePingHeader => GetHeaderText(true, "Ping", "Ping");
        [DataSourceProperty] public string TeamOneKillsHeader => GetHeaderText(true, "Kills", "K");
        [DataSourceProperty] public string TeamOneDeathsHeader => GetHeaderText(true, "Deaths", "D");
        [DataSourceProperty] public string TeamOneAssistsHeader => GetHeaderText(true, "Assists", "A");
        [DataSourceProperty] public string TeamOneScoreHeader => GetHeaderText(true, "Score", "S");
        [DataSourceProperty] public string TeamOneKdaHeader => GetHeaderText(true, "KDA", "KDA");
        [DataSourceProperty] public string TeamOneSprHeader => GetHeaderText(true, "SPR", "SPR");
        [DataSourceProperty] public string TeamTwoNameHeader => GetHeaderText(false, "Name", "Name");
        [DataSourceProperty] public string TeamTwoPingHeader => GetHeaderText(false, "Ping", "Ping");
        [DataSourceProperty] public string TeamTwoKillsHeader => GetHeaderText(false, "Kills", "K");
        [DataSourceProperty] public string TeamTwoDeathsHeader => GetHeaderText(false, "Deaths", "D");
        [DataSourceProperty] public string TeamTwoAssistsHeader => GetHeaderText(false, "Assists", "A");
        [DataSourceProperty] public string TeamTwoScoreHeader => GetHeaderText(false, "Score", "S");
        [DataSourceProperty] public string TeamTwoKdaHeader => GetHeaderText(false, "KDA", "KDA");
        [DataSourceProperty] public string TeamTwoSprHeader => GetHeaderText(false, "SPR", "SPR");

        private void RefreshColumnLayout()
        {
            OnPropertyChangedWithValue(ShowStateAndBannerLayout, "ShowStateAndBannerLayout");
            OnPropertyChangedWithValue(ShowStateOnlyLayout, "ShowStateOnlyLayout");
            OnPropertyChangedWithValue(ShowBannerOnlyLayout, "ShowBannerOnlyLayout");
            OnPropertyChangedWithValue(ShowNameOnlyLayout, "ShowNameOnlyLayout");
        }

        [DataSourceProperty]
        public MBBindingList<ScoreboardPlayerVM> TeamOnePlayers
        {
            get => _teamOnePlayers;
            set
            {
                if (_teamOnePlayers != value)
                {
                    _teamOnePlayers = value;
                    OnPropertyChangedWithValue(value, "TeamOnePlayers");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<ScoreboardPlayerVM> TeamTwoPlayers
        {
            get => _teamTwoPlayers;
            set
            {
                if (_teamTwoPlayers != value)
                {
                    _teamTwoPlayers = value;
                    OnPropertyChangedWithValue(value, "TeamTwoPlayers");
                }
            }
        }

        public void OpenPlayerActions(ScoreboardPlayerVM player, MBBindingList<ScoreboardPlayerActionVM> actions)
        {
            if (player == null || string.IsNullOrWhiteSpace(player.NameValue))
                return;

            ClearSelectedPlayer();
            _selectedPlayer = player;
            _selectedPlayer.IsActionSelected = true;
            _selectedPlayerName = player.NameValue;
            MBBindingList<ScoreboardPlayerActionVM> reboundActions = new MBBindingList<ScoreboardPlayerActionVM>();
            if (actions != null)
            {
                foreach (ScoreboardPlayerActionVM action in actions)
                {
                    if (action != null)
                        reboundActions.Add(new ScoreboardPlayerActionVM(action.Id, action.Definition, action.Value, action.IsEnabled, ExecutePlayerAction));
                }
            }

            PlayerActionList = reboundActions;
            IsPlayerActionsActive = true;
        }

        private void ClearSelectedPlayer()
        {
            if (_selectedPlayer != null)
                _selectedPlayer.IsActionSelected = false;

            _selectedPlayer = null;
        }

        private void ExecuteShowCursor()
        {
            OnShowCursorRequested?.Invoke();
        }

        private void ExecuteClearTeamOneSearch()
        {
            TeamOneSearchText = "";
        }

        private void ExecuteClearTeamTwoSearch()
        {
            TeamTwoSearchText = "";
        }

        private void ExecuteToggleMute()
        {
            HasMutedAll = !HasMutedAll;
            ToggleMuteText = HasMutedAll ? "Unmute All" : "Mute All";
            OnToggleMuteRequested?.Invoke(HasMutedAll);
        }

        private void ExecutePlayerAction(string actionId)
        {
            string playerName = _selectedPlayerName;
            IsPlayerActionsActive = false;

            switch (actionId)
            {
                case string nativeAction when nativeAction.StartsWith("scoreboard:", StringComparison.Ordinal):
                    OnScoreboardPlayerActionRequested?.Invoke(playerName, nativeAction.Substring("scoreboard:".Length));
                    break;
                case "inspect":
                    OnInspectPlayerCardRequested?.Invoke(playerName);
                    break;
                case "mute_permanent":
                    OnMutePermanentlyRequested?.Invoke(playerName);
                    break;
                case "unmute_permanent":
                    OnScoreboardPlayerActionRequested?.Invoke(playerName, "Unmute Permanently");
                    break;
                case "mute_text":
                    OnMuteTextRequested?.Invoke(playerName);
                    break;
                case "unmute_text":
                    OnScoreboardPlayerActionRequested?.Invoke(playerName, "Unmute Text");
                    break;
                case "mute_voice":
                    OnMuteVoiceRequested?.Invoke(playerName);
                    break;
                case "unmute_voice":
                    OnScoreboardPlayerActionRequested?.Invoke(playerName, "Unmute Voice");
                    break;
                case "report":
                    OnReportRequested?.Invoke(playerName);
                    break;
            }
        }

        private void ExecuteSortTeamOneByName() => Sort(true, "Name");
        private void ExecuteSortTeamOneByPing() => Sort(true, "Ping");
        private void ExecuteSortTeamOneByKills() => Sort(true, "Kills");
        private void ExecuteSortTeamOneByDeaths() => Sort(true, "Deaths");
        private void ExecuteSortTeamOneByAssists() => Sort(true, "Assists");
        private void ExecuteSortTeamOneByScore() => Sort(true, "Score");
        private void ExecuteSortTeamOneByKda() => Sort(true, "KDA");
        private void ExecuteSortTeamOneBySpr() => Sort(true, "SPR");

        private void ExecuteSortTeamTwoByName() => Sort(false, "Name");
        private void ExecuteSortTeamTwoByPing() => Sort(false, "Ping");
        private void ExecuteSortTeamTwoByKills() => Sort(false, "Kills");
        private void ExecuteSortTeamTwoByDeaths() => Sort(false, "Deaths");
        private void ExecuteSortTeamTwoByAssists() => Sort(false, "Assists");
        private void ExecuteSortTeamTwoByScore() => Sort(false, "Score");
        private void ExecuteSortTeamTwoByKda() => Sort(false, "KDA");
        private void ExecuteSortTeamTwoBySpr() => Sort(false, "SPR");

        private void Sort(bool first, string column)
        {
            if (first)
            {
                if (_teamOneSortColumn == column)
                    _teamOneSortAscending = !_teamOneSortAscending;
                else
                {
                    _teamOneSortColumn = column;
                    _teamOneSortAscending = column == "Name";
                }

                _teamOneManualSort = true;
            }
            else
            {
                if (_teamTwoSortColumn == column)
                    _teamTwoSortAscending = !_teamTwoSortAscending;
                else
                {
                    _teamTwoSortColumn = column;
                    _teamTwoSortAscending = column == "Name";
                }

                _teamTwoManualSort = true;
            }

            RefreshSortHeaders();
        }

        private string GetHeaderText(bool first, string column, string baseText)
        {
            if (GetSortColumn(first) != column)
                return baseText;

            return baseText + (GetSortAscending(first) ? " ^" : " v");
        }

        private static string NormalizeSortColumn(string column)
        {
            switch (column)
            {
                case "Name":
                case "Kills":
                case "Score":
                case "KDA":
                case "SPR":
                    return column;
                default:
                    return "Score";
            }
        }

        private void RefreshSortHeaders()
        {
            OnPropertyChangedWithValue(TeamOneNameHeader, "TeamOneNameHeader");
            OnPropertyChangedWithValue(TeamOnePingHeader, "TeamOnePingHeader");
            OnPropertyChangedWithValue(TeamOneKillsHeader, "TeamOneKillsHeader");
            OnPropertyChangedWithValue(TeamOneDeathsHeader, "TeamOneDeathsHeader");
            OnPropertyChangedWithValue(TeamOneAssistsHeader, "TeamOneAssistsHeader");
            OnPropertyChangedWithValue(TeamOneScoreHeader, "TeamOneScoreHeader");
            OnPropertyChangedWithValue(TeamOneKdaHeader, "TeamOneKdaHeader");
            OnPropertyChangedWithValue(TeamOneSprHeader, "TeamOneSprHeader");
            OnPropertyChangedWithValue(TeamTwoNameHeader, "TeamTwoNameHeader");
            OnPropertyChangedWithValue(TeamTwoPingHeader, "TeamTwoPingHeader");
            OnPropertyChangedWithValue(TeamTwoKillsHeader, "TeamTwoKillsHeader");
            OnPropertyChangedWithValue(TeamTwoDeathsHeader, "TeamTwoDeathsHeader");
            OnPropertyChangedWithValue(TeamTwoAssistsHeader, "TeamTwoAssistsHeader");
            OnPropertyChangedWithValue(TeamTwoScoreHeader, "TeamTwoScoreHeader");
            OnPropertyChangedWithValue(TeamTwoKdaHeader, "TeamTwoKdaHeader");
            OnPropertyChangedWithValue(TeamTwoSprHeader, "TeamTwoSprHeader");
        }
    }
}
