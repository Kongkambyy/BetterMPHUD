using System;
using TaleWorlds.Library;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;

namespace BetterMPHUD.ViewModels
{
    public class ScoreboardPlayerVM : ViewModel
    {
        private string _name;
        private string _score;
        private string _kills;
        private string _deaths;
        private string _assists;
        private string _kda;
        private string _spr;
        private string _ping;
        private string _status;
        private string _classSprite;
        private bool _hasClassSprite;
        private string _rowColor;
        private bool _showPing;
        private bool _useAdvancedStats;
        private bool _showStateColumn = true;
        private bool _isLocalPlayer;
        private bool _isHovered;
        private bool _isActionSelected;
        private bool _showWarlordsBannerColumn;
        private bool _showNativeNameLayout = true;
        private bool _hasWarlordsBanner;
        private bool _hasScoreboardFactionBanner;
        private ViewModel _warlordsPlayer;
        private BannerImageIdentifierVM _scoreboardBanner;
        private BannerImageIdentifierVM _scoreboardFactionBanner;
        private float _rowScale = 1f;

        public ScoreboardPlayerVM(string name, string peerId, int score, int kills, int deaths, int assists, float kda, float spr, int ping, bool alive, bool showPing, string classSprite, string rowColor)
        {
            NameValue = name;
            PeerIdValue = peerId;
            ScoreValue = score;
            KillsValue = kills;
            DeathsValue = deaths;
            AssistsValue = assists;
            KdaValue = kda;
            SprValue = spr;
            PingValue = ping;
            _name = NameValue;
            _score = ScoreValue.ToString();
            _kills = KillsValue.ToString();
            _deaths = DeathsValue.ToString();
            _assists = AssistsValue.ToString();
            _kda = FormatStat(KdaValue);
            _spr = FormatStat(SprValue);
            _ping = PingValue.ToString();
            _status = alive ? "Alive" : "Dead";
            _showPing = showPing;
            ClassSprite = classSprite;
            _rowColor = rowColor;
        }

        public string NameValue { get; private set; }
        public string PeerIdValue { get; private set; }
        public int ScoreValue { get; private set; }
        public int KillsValue { get; private set; }
        public int DeathsValue { get; private set; }
        public int AssistsValue { get; private set; }
        public float KdaValue { get; private set; }
        public float SprValue { get; private set; }
        public int PingValue { get; private set; }
        public Action<ScoreboardPlayerVM> OnActionsRequested { get; set; }

        [DataSourceProperty] public string Name { get => _name; set { _name = value; OnPropertyChangedWithValue(value, "Name"); } }
        [DataSourceProperty] public string Score { get => _score; set { _score = value; OnPropertyChangedWithValue(value, "Score"); } }
        [DataSourceProperty] public string Kills { get => _kills; set { _kills = value; OnPropertyChangedWithValue(value, "Kills"); } }
        [DataSourceProperty] public string Deaths { get => _deaths; set { _deaths = value; OnPropertyChangedWithValue(value, "Deaths"); } }
        [DataSourceProperty] public string Assists { get => _assists; set { _assists = value; OnPropertyChangedWithValue(value, "Assists"); } }
        [DataSourceProperty] public string KDA { get => _kda; set { _kda = value; OnPropertyChangedWithValue(value, "KDA"); } }
        [DataSourceProperty] public string SPR { get => _spr; set { _spr = value; OnPropertyChangedWithValue(value, "SPR"); } }
        [DataSourceProperty] public string Ping { get => _ping; set { _ping = value; OnPropertyChangedWithValue(value, "Ping"); } }
        [DataSourceProperty] public bool ShowPing { get => _showPing; set { _showPing = value; OnPropertyChangedWithValue(value, "ShowPing"); } }
        [DataSourceProperty] public bool UseAdvancedStats { get => _useAdvancedStats; set { _useAdvancedStats = value; OnPropertyChangedWithValue(value, "UseAdvancedStats"); OnPropertyChangedWithValue(UseBasicStats, "UseBasicStats"); } }
        [DataSourceProperty] public bool UseBasicStats => !UseAdvancedStats;
        [DataSourceProperty] public bool IsLocalPlayer { get => _isLocalPlayer; set { _isLocalPlayer = value; OnPropertyChangedWithValue(value, "IsLocalPlayer"); } }
        [DataSourceProperty] public bool IsHovered { get => _isHovered; set { _isHovered = value; OnPropertyChangedWithValue(value, "IsHovered"); OnPropertyChangedWithValue(IsRowHighlighted, "IsRowHighlighted"); } }
        [DataSourceProperty] public bool IsActionSelected { get => _isActionSelected; set { _isActionSelected = value; OnPropertyChangedWithValue(value, "IsActionSelected"); OnPropertyChangedWithValue(IsRowHighlighted, "IsRowHighlighted"); } }
        [DataSourceProperty] public bool ShowStateColumn { get => _showStateColumn; set { _showStateColumn = value; OnPropertyChangedWithValue(value, "ShowStateColumn"); OnPropertyChangedWithValue(ShowClassSprite, "ShowClassSprite"); OnPropertyChangedWithValue(ShowStateAndBannerLayout, "ShowStateAndBannerLayout"); OnPropertyChangedWithValue(ShowStateOnlyLayout, "ShowStateOnlyLayout"); OnPropertyChangedWithValue(ShowBannerOnlyLayout, "ShowBannerOnlyLayout"); OnPropertyChangedWithValue(ShowNameOnlyLayout, "ShowNameOnlyLayout"); } }
        [DataSourceProperty] public bool ShowWarlordsBannerColumn { get => _showWarlordsBannerColumn; set { _showWarlordsBannerColumn = value; OnPropertyChangedWithValue(value, "ShowWarlordsBannerColumn"); OnPropertyChangedWithValue(ShowStateAndBannerLayout, "ShowStateAndBannerLayout"); OnPropertyChangedWithValue(ShowStateOnlyLayout, "ShowStateOnlyLayout"); OnPropertyChangedWithValue(ShowBannerOnlyLayout, "ShowBannerOnlyLayout"); OnPropertyChangedWithValue(ShowNameOnlyLayout, "ShowNameOnlyLayout"); } }
        [DataSourceProperty] public bool ShowNativeNameLayout { get => _showNativeNameLayout; set { _showNativeNameLayout = value; OnPropertyChangedWithValue(value, "ShowNativeNameLayout"); } }
        [DataSourceProperty] public string Status { get => _status; set { _status = value; OnPropertyChangedWithValue(value, "Status"); } }
        [DataSourceProperty] public string ClassSprite { get => _classSprite; set { _classSprite = value; HasClassSprite = !string.IsNullOrWhiteSpace(value); OnPropertyChangedWithValue(value, "ClassSprite"); } }
        [DataSourceProperty] public bool HasClassSprite { get => _hasClassSprite; set { _hasClassSprite = value; OnPropertyChangedWithValue(value, "HasClassSprite"); OnPropertyChangedWithValue(ShowClassSprite, "ShowClassSprite"); } }
        [DataSourceProperty] public string RowColor { get => _rowColor; set { _rowColor = value; OnPropertyChangedWithValue(value, "RowColor"); } }
        [DataSourceProperty] public bool HasWarlordsBanner { get => _hasWarlordsBanner; set { _hasWarlordsBanner = value; OnPropertyChangedWithValue(value, "HasWarlordsBanner"); } }
        [DataSourceProperty] public bool HasScoreboardFactionBanner { get => _hasScoreboardFactionBanner; set { _hasScoreboardFactionBanner = value; OnPropertyChangedWithValue(value, "HasScoreboardFactionBanner"); } }
        [DataSourceProperty] public ViewModel WarlordsPlayer { get => _warlordsPlayer; set { _warlordsPlayer = value; OnPropertyChangedWithValue(value, "WarlordsPlayer"); } }
        [DataSourceProperty] public BannerImageIdentifierVM ScoreboardBanner { get => _scoreboardBanner; set { _scoreboardBanner = value; OnPropertyChangedWithValue(value, "ScoreboardBanner"); } }
        [DataSourceProperty] public BannerImageIdentifierVM ScoreboardFactionBanner { get => _scoreboardFactionBanner; set { _scoreboardFactionBanner = value; OnPropertyChangedWithValue(value, "ScoreboardFactionBanner"); } }
        [DataSourceProperty] public float RowScale { get => _rowScale; set { _rowScale = value; OnPropertyChangedWithValue(value, "RowScale"); RefreshScaledProperties(); } }
        [DataSourceProperty] public int RowHeight => Scale(38);
        [DataSourceProperty] public int StateIconSize => Scale(22);
        [DataSourceProperty] public int BannerWidth => Scale(28);
        [DataSourceProperty] public int BannerHeight => Scale(32);
        [DataSourceProperty] public int NameFontSize => Scale(16);
        [DataSourceProperty] public int ValueFontSize => Scale(15);
        [DataSourceProperty] public bool ShowClassSprite => ShowStateColumn && HasClassSprite;
        [DataSourceProperty] public bool IsRowHighlighted => IsHovered || IsActionSelected;
        [DataSourceProperty] public bool ShowStateAndBannerLayout => ShowStateColumn && ShowWarlordsBannerColumn;
        [DataSourceProperty] public bool ShowStateOnlyLayout => ShowStateColumn && !ShowWarlordsBannerColumn;
        [DataSourceProperty] public bool ShowBannerOnlyLayout => !ShowStateColumn && ShowWarlordsBannerColumn;
        [DataSourceProperty] public bool ShowNameOnlyLayout => !ShowStateColumn && !ShowWarlordsBannerColumn;

        private void ExecuteOpenActions()
        {
            OnActionsRequested?.Invoke(this);
        }

        public void Update(string name, string peerId, int score, int kills, int deaths, int assists, float kda, float spr, int ping, bool alive, bool showPing, string classSprite, string rowColor, bool isLocalPlayer, float rowScale)
        {
            NameValue = name;
            PeerIdValue = peerId;
            ScoreValue = score;
            KillsValue = kills;
            DeathsValue = deaths;
            AssistsValue = assists;
            KdaValue = kda;
            SprValue = spr;
            PingValue = ping;
            Name = name;
            Score = score.ToString();
            Kills = kills.ToString();
            Deaths = deaths.ToString();
            Assists = assists.ToString();
            KDA = FormatStat(kda);
            SPR = FormatStat(spr);
            Ping = ping.ToString();
            Status = alive ? "Alive" : "Dead";
            ShowPing = showPing;
            ClassSprite = classSprite;
            RowColor = rowColor;
            IsLocalPlayer = isLocalPlayer;
            RowScale = rowScale;
        }

        private void ExecuteHoverBegin()
        {
            IsHovered = true;
        }

        private void ExecuteHoverEnd()
        {
            IsHovered = false;
        }

        private int Scale(int value)
        {
            return Math.Max(1, (int)Math.Round(value * RowScale));
        }

        private static string FormatStat(float value)
        {
            return value.ToString("0.00");
        }

        private void RefreshScaledProperties()
        {
            OnPropertyChangedWithValue(RowHeight, "RowHeight");
            OnPropertyChangedWithValue(StateIconSize, "StateIconSize");
            OnPropertyChangedWithValue(BannerWidth, "BannerWidth");
            OnPropertyChangedWithValue(BannerHeight, "BannerHeight");
            OnPropertyChangedWithValue(NameFontSize, "NameFontSize");
            OnPropertyChangedWithValue(ValueFontSize, "ValueFontSize");
        }
    }
}
