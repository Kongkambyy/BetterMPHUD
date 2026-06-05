using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Scoreboard;

namespace MBWarlords.LastConcord.GauntletUI.MissionViews;

public sealed class WLMultiplayerScoreboardPlayerVM : MPPlayerVM
{
	internal const string ScoreboardBannerHeaderId = "banner";

	internal const string AvatarHeaderId = "avatar";

	private readonly MissionPeer _lobbyPeer;

	private readonly Action<WLMultiplayerScoreboardPlayerVM> _executeActivate;

	private readonly ChatBox _chatBox;

	private readonly WLMultiplayerScoreboardStatItemVM _scoreRankStat;

	private readonly string[] _attributeHeaderIDs;

	private WarlordsPeerComponent _warlordsPeerComponent;

	private int _ping;

	private bool _isPlayer;

	private bool _isVoiceMuted;

	private bool _isTextMuted;

	private BannerImageIdentifierVM _scoreboardBanner;

	private BannerImageIdentifierVM _scoreboardFactionBanner;

	private bool _hasScoreboardFactionBanner;

	private bool _isClanManagerSubscribed;

	private MBBindingList<WLMultiplayerScoreboardStatItemVM> _stats;

	private MBBindingList<MissionScoreboardMVPItemVM> _mvpBadges;

	public int Score { get; private set; }

	public bool IsBot { get; private set; }

	public bool IsMine => _lobbyPeer != null && ((PeerComponent)_lobbyPeer).IsMine;

	public bool IsTeammate => _lobbyPeer != null && _lobbyPeer.Team.IsPlayerTeam;

	[DataSourceProperty]
	public int Ping
	{
		get
		{
			return _ping;
		}
		set
		{
			if (value != _ping)
			{
				_ping = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Ping");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPlayer
	{
		get
		{
			return _isPlayer;
		}
		set
		{
			if (value != _isPlayer)
			{
				_isPlayer = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPlayer");
			}
		}
	}

	[DataSourceProperty]
	public bool IsVoiceMuted
	{
		get
		{
			return _isVoiceMuted;
		}
		set
		{
			if (value != _isVoiceMuted)
			{
				_isVoiceMuted = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsVoiceMuted");
			}
		}
	}

	[DataSourceProperty]
	public bool IsTextMuted
	{
		get
		{
			return _isTextMuted;
		}
		set
		{
			if (value != _isTextMuted)
			{
				_isTextMuted = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsTextMuted");
			}
		}
	}

	[DataSourceProperty]
	public BannerImageIdentifierVM ScoreboardBanner
	{
		get
		{
			return _scoreboardBanner;
		}
		set
		{
			if (value != _scoreboardBanner)
			{
				BannerImageIdentifierVM scoreboardBanner = _scoreboardBanner;
				_scoreboardBanner = value;
				((ViewModel)this).OnPropertyChangedWithValue<BannerImageIdentifierVM>(value, "ScoreboardBanner");
				if (scoreboardBanner != null)
				{
					((ViewModel)scoreboardBanner).OnFinalize();
				}
			}
		}
	}

	[DataSourceProperty]
	public BannerImageIdentifierVM ScoreboardFactionBanner
	{
		get
		{
			return _scoreboardFactionBanner;
		}
		set
		{
			if (value != _scoreboardFactionBanner)
			{
				BannerImageIdentifierVM scoreboardFactionBanner = _scoreboardFactionBanner;
				_scoreboardFactionBanner = value;
				((ViewModel)this).OnPropertyChangedWithValue<BannerImageIdentifierVM>(value, "ScoreboardFactionBanner");
				if (scoreboardFactionBanner != null)
				{
					((ViewModel)scoreboardFactionBanner).OnFinalize();
				}
			}
		}
	}

	[DataSourceProperty]
	public bool HasScoreboardFactionBanner
	{
		get
		{
			return _hasScoreboardFactionBanner;
		}
		set
		{
			if (value != _hasScoreboardFactionBanner)
			{
				_hasScoreboardFactionBanner = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "HasScoreboardFactionBanner");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<WLMultiplayerScoreboardStatItemVM> Stats
	{
		get
		{
			return _stats;
		}
		set
		{
			if (value != _stats)
			{
				_stats = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<WLMultiplayerScoreboardStatItemVM>>(value, "Stats");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MissionScoreboardMVPItemVM> MVPBadges
	{
		get
		{
			return _mvpBadges;
		}
		set
		{
			if (value != _mvpBadges)
			{
				_mvpBadges = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MissionScoreboardMVPItemVM>>(value, "MVPBadges");
			}
		}
	}

	public WLMultiplayerScoreboardPlayerVM(MissionPeer peer, string[] attributes, string[] headerIDs, int score, Action<WLMultiplayerScoreboardPlayerVM> executeActivate)
		: base(peer)
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		_chatBox = Game.Current.GetGameHandler<ChatBox>();
		_executeActivate = executeActivate;
		_lobbyPeer = peer;
		_attributeHeaderIDs = headerIDs ?? Array.Empty<string>();
		_warlordsPeerComponent = ResolveWarlordsPeerComponent();
		Stats = new MBBindingList<WLMultiplayerScoreboardStatItemVM>();
		((Collection<WLMultiplayerScoreboardStatItemVM>)(object)Stats).Add(new WLMultiplayerScoreboardStatItemVM(this, "index", string.Empty));
		string[] array = BuildDisplayedHeaderIDs(_attributeHeaderIDs);
		foreach (string headerID in array)
		{
			((Collection<WLMultiplayerScoreboardStatItemVM>)(object)Stats).Add(new WLMultiplayerScoreboardStatItemVM(this, headerID, string.Empty));
		}
		_scoreRankStat = ((Collection<WLMultiplayerScoreboardStatItemVM>)(object)Stats)[0];
		SubscribeToScoreboardBannerEvents();
		RefreshScoreboardBanner();
		UpdateAttributes(attributes, score);
		IsPlayer = IsMine;
		MVPBadges = new MBBindingList<MissionScoreboardMVPItemVM>();
		((MPPlayerVM)this).Peer.SetMuted(PermaMuteList.IsPlayerMuted(((PeerComponent)peer).Peer.Id));
		UpdateIsMuted();
		RefreshDeadState();
	}

	public WLMultiplayerScoreboardPlayerVM(string[] attributes, string[] headerIDs, int score, Action<WLMultiplayerScoreboardPlayerVM> executeActivate)
		: base((Agent)null)
	{
		_executeActivate = executeActivate;
		_attributeHeaderIDs = headerIDs ?? Array.Empty<string>();
		Stats = new MBBindingList<WLMultiplayerScoreboardStatItemVM>();
		((Collection<WLMultiplayerScoreboardStatItemVM>)(object)Stats).Add(new WLMultiplayerScoreboardStatItemVM(this, "index", string.Empty));
		string[] array = BuildDisplayedHeaderIDs(_attributeHeaderIDs);
		foreach (string headerID in array)
		{
			((Collection<WLMultiplayerScoreboardStatItemVM>)(object)Stats).Add(new WLMultiplayerScoreboardStatItemVM(this, headerID, string.Empty));
		}
		_scoreRankStat = ((Collection<WLMultiplayerScoreboardStatItemVM>)(object)Stats)[0];
		IsBot = true;
		RefreshScoreboardBanner();
		UpdateAttributes(attributes, score);
		IsPlayer = false;
		MVPBadges = new MBBindingList<MissionScoreboardMVPItemVM>();
		RefreshDeadState();
	}

	public override void OnFinalize()
	{
		UnsubscribeFromScoreboardBannerEvents();
		BannerImageIdentifierVM scoreboardBanner = ScoreboardBanner;
		if (scoreboardBanner != null)
		{
			((ViewModel)scoreboardBanner).OnFinalize();
		}
		BannerImageIdentifierVM scoreboardFactionBanner = ScoreboardFactionBanner;
		if (scoreboardFactionBanner != null)
		{
			((ViewModel)scoreboardFactionBanner).OnFinalize();
		}
		_scoreboardBanner = null;
		_scoreboardFactionBanner = null;
		((ViewModel)this).OnFinalize();
	}

	public void Tick(float dt)
	{
		EnsureScoreboardBannerEventsSubscription();
		RefreshDeadState();
	}

	public void UpdateAttributes(string[] attributes, int score)
	{
		if (attributes != null)
		{
			int num = Math.Min(attributes.Length, _attributeHeaderIDs.Length);
			for (int i = 0; i < num; i++)
			{
				string headerId = _attributeHeaderIDs[i] ?? string.Empty;
				WLMultiplayerScoreboardStatItemVM wLMultiplayerScoreboardStatItemVM = ((IEnumerable<WLMultiplayerScoreboardStatItemVM>)Stats).FirstOrDefault((WLMultiplayerScoreboardStatItemVM entry) => entry.HeaderID == headerId);
				if (wLMultiplayerScoreboardStatItemVM != null)
				{
					wLMultiplayerScoreboardStatItemVM.Item = attributes[i] ?? string.Empty;
				}
			}
		}
		Score = score;
		RefreshPing();
		RefreshDeadState();
	}

	public void RefreshDeadState(bool forceNotify = false)
	{
		bool isDead = ((MPPlayerVM)this).IsDead;
		if (!IsBot)
		{
			((MPPlayerVM)this).IsDead = _lobbyPeer == null || !_lobbyPeer.IsControlledAgentActive;
			if (forceNotify && isDead == ((MPPlayerVM)this).IsDead)
			{
				((ViewModel)this).OnPropertyChangedWithValue(((MPPlayerVM)this).IsDead, "IsDead");
			}
		}
		else
		{
			((MPPlayerVM)this).IsDead = false;
			if (forceNotify && isDead == ((MPPlayerVM)this).IsDead)
			{
				((ViewModel)this).OnPropertyChangedWithValue(((MPPlayerVM)this).IsDead, "IsDead");
			}
		}
	}

	public string GetStatValue(string headerId)
	{
		return ((IEnumerable<WLMultiplayerScoreboardStatItemVM>)Stats).FirstOrDefault((WLMultiplayerScoreboardStatItemVM entry) => entry.HeaderID == headerId)?.Item ?? string.Empty;
	}

	public void SetScoreRank(int scoreRank)
	{
		_scoreRankStat.Item = ((scoreRank > 0) ? scoreRank.ToString(CultureInfo.CurrentCulture) : string.Empty);
	}

	public void ExecuteSelection()
	{
		_executeActivate?.Invoke(this);
	}

	public void UpdateIsMuted()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		MissionPeer lobbyPeer = _lobbyPeer;
		if (((lobbyPeer != null) ? ((PeerComponent)lobbyPeer).Peer : null) != null)
		{
			bool flag = PermaMuteList.IsPlayerMuted(((PeerComponent)_lobbyPeer).Peer.Id);
			IsTextMuted = flag || _chatBox.IsPlayerMuted(((PeerComponent)_lobbyPeer).Peer.Id);
			IsVoiceMuted = flag || ((MPPlayerVM)this).Peer.IsMutedFromGameOrPlatform;
		}
	}

	public void SetMVPBadgeCount(int badgeCount)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		((Collection<MissionScoreboardMVPItemVM>)(object)MVPBadges).Clear();
		for (int i = 0; i < badgeCount; i++)
		{
			((Collection<MissionScoreboardMVPItemVM>)(object)MVPBadges).Add(new MissionScoreboardMVPItemVM());
		}
	}

	private void RefreshPing()
	{
		if (int.TryParse(GetStatValue("ping"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			Ping = result;
		}
	}

	private WarlordsPeerComponent ResolveWarlordsPeerComponent()
	{
		MissionPeer lobbyPeer = _lobbyPeer;
		object result;
		if (lobbyPeer == null)
		{
			result = null;
		}
		else
		{
			VirtualPlayer peer = ((PeerComponent)lobbyPeer).Peer;
			result = ((peer != null) ? peer.GetComponent<WarlordsPeerComponent>() : null);
		}
		return (WarlordsPeerComponent)result;
	}

	private void EnsureScoreboardBannerEventsSubscription()
	{
		WarlordsPeerComponent warlordsPeerComponent = ResolveWarlordsPeerComponent();
		if (warlordsPeerComponent != _warlordsPeerComponent)
		{
			UnsubscribeFromScoreboardBannerEvents();
			_warlordsPeerComponent = warlordsPeerComponent;
			SubscribeToScoreboardBannerEvents();
			RefreshScoreboardBanner();
		}
	}

	private string[] BuildDisplayedHeaderIDs(string[] headerIDs)
	{
		string[] array = headerIDs ?? Array.Empty<string>();
		List<string> list = new List<string>(array.Length + 1);
		bool flag = false;
		string[] array2 = array;
		foreach (string text in array2)
		{
			string text2 = text ?? string.Empty;
			if (string.Equals(text2, "banner", StringComparison.Ordinal))
			{
				if (!flag)
				{
					list.Add(text2);
					flag = true;
				}
				continue;
			}
			if (!flag && string.Equals(text2, "avatar", StringComparison.Ordinal))
			{
				list.Add("banner");
				flag = true;
			}
			list.Add(text2);
		}
		if (!flag)
		{
			list.Insert(0, "banner");
		}
		return list.ToArray();
	}

	private void SubscribeToScoreboardBannerEvents()
	{
		if (_warlordsPeerComponent != null)
		{
			WarlordsPeerComponent warlordsPeerComponent = _warlordsPeerComponent;
			warlordsPeerComponent.OnWLAccountBannerCodeChanged = (Action<string>)Delegate.Combine(warlordsPeerComponent.OnWLAccountBannerCodeChanged, new Action<string>(OnScoreboardBannerCodeChanged));
			WarlordsPeerComponent warlordsPeerComponent2 = _warlordsPeerComponent;
			warlordsPeerComponent2.OnWLAccountClanIdChanged = (Action<string>)Delegate.Combine(warlordsPeerComponent2.OnWLAccountClanIdChanged, new Action<string>(OnScoreboardBannerCodeChanged));
			WarlordsPeerComponent warlordsPeerComponent3 = _warlordsPeerComponent;
			warlordsPeerComponent3.OnWLClanUpdated = (Action<WarlordsClanObject>)Delegate.Combine(warlordsPeerComponent3.OnWLClanUpdated, new Action<WarlordsClanObject>(OnScoreboardBannerClanUpdated));
			WarlordsPeerComponent warlordsPeerComponent4 = _warlordsPeerComponent;
			warlordsPeerComponent4.OnWLClanAppearanceChanged = (Action<WarlordsClanObject>)Delegate.Combine(warlordsPeerComponent4.OnWLClanAppearanceChanged, new Action<WarlordsClanObject>(OnScoreboardBannerClanAppearanceChanged));
			WarlordsPeerComponent warlordsPeerComponent5 = _warlordsPeerComponent;
			warlordsPeerComponent5.OnWLInspectColorPreferenceChanged = (Action<WarlordsNetwork.InspectColorPreferenceCode>)Delegate.Combine(warlordsPeerComponent5.OnWLInspectColorPreferenceChanged, new Action<WarlordsNetwork.InspectColorPreferenceCode>(OnScoreboardBannerInspectColorPreferenceChanged));
			WLClansManager.Instance.ClanLoaded += OnScoreboardClanLoaded;
			_isClanManagerSubscribed = true;
		}
	}

	private void UnsubscribeFromScoreboardBannerEvents()
	{
		if (_warlordsPeerComponent != null)
		{
			WarlordsPeerComponent warlordsPeerComponent = _warlordsPeerComponent;
			warlordsPeerComponent.OnWLAccountBannerCodeChanged = (Action<string>)Delegate.Remove(warlordsPeerComponent.OnWLAccountBannerCodeChanged, new Action<string>(OnScoreboardBannerCodeChanged));
			WarlordsPeerComponent warlordsPeerComponent2 = _warlordsPeerComponent;
			warlordsPeerComponent2.OnWLAccountClanIdChanged = (Action<string>)Delegate.Remove(warlordsPeerComponent2.OnWLAccountClanIdChanged, new Action<string>(OnScoreboardBannerCodeChanged));
			WarlordsPeerComponent warlordsPeerComponent3 = _warlordsPeerComponent;
			warlordsPeerComponent3.OnWLClanUpdated = (Action<WarlordsClanObject>)Delegate.Remove(warlordsPeerComponent3.OnWLClanUpdated, new Action<WarlordsClanObject>(OnScoreboardBannerClanUpdated));
			WarlordsPeerComponent warlordsPeerComponent4 = _warlordsPeerComponent;
			warlordsPeerComponent4.OnWLClanAppearanceChanged = (Action<WarlordsClanObject>)Delegate.Remove(warlordsPeerComponent4.OnWLClanAppearanceChanged, new Action<WarlordsClanObject>(OnScoreboardBannerClanAppearanceChanged));
			WarlordsPeerComponent warlordsPeerComponent5 = _warlordsPeerComponent;
			warlordsPeerComponent5.OnWLInspectColorPreferenceChanged = (Action<WarlordsNetwork.InspectColorPreferenceCode>)Delegate.Remove(warlordsPeerComponent5.OnWLInspectColorPreferenceChanged, new Action<WarlordsNetwork.InspectColorPreferenceCode>(OnScoreboardBannerInspectColorPreferenceChanged));
			if (_isClanManagerSubscribed)
			{
				WLClansManager.Instance.ClanLoaded -= OnScoreboardClanLoaded;
				_isClanManagerSubscribed = false;
			}
		}
	}

	private void RefreshScoreboardBanner()
	{
		ScoreboardBanner = CreateScoreboardBanner();
		RefreshScoreboardFactionBanner();
	}

	private BannerImageIdentifierVM CreateScoreboardBanner()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		try
		{
			if (_warlordsPeerComponent != null)
			{
				return new BannerImageIdentifierVM(_warlordsPeerComponent.CreateRuntimeAccountSigilBanner(), true);
			}
			return new BannerImageIdentifierVM(new Banner(), true);
		}
		catch
		{
			return new BannerImageIdentifierVM(new Banner(), true);
		}
	}

	private void RefreshScoreboardFactionBanner()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		if (TryCreateScoreboardFactionBanner(out var banner))
		{
			ScoreboardFactionBanner = banner;
			HasScoreboardFactionBanner = true;
		}
		else
		{
			HasScoreboardFactionBanner = false;
			ScoreboardFactionBanner = new BannerImageIdentifierVM(new Banner(), true);
		}
	}

	private bool TryCreateScoreboardFactionBanner(out BannerImageIdentifierVM banner)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		banner = null;
		WarlordsClanObject warlordsClanObject = _warlordsPeerComponent?.Clan;
		if (warlordsClanObject == null || string.IsNullOrWhiteSpace(warlordsClanObject.FactionId))
		{
			return false;
		}
		string text = warlordsClanObject.FactionBanner;
		string bannerCode;
		if (WLClansManager.Instance.TryGetClan(warlordsClanObject.FactionId, WarlordsOrganizationKind.Faction, out var clan) && clan != null && clan.HasPublicListing)
		{
			text = clan.Banner;
		}
		else if (string.IsNullOrWhiteSpace(text) && WLClansManager.Instance.TryGetBanner(warlordsClanObject.FactionId, out bannerCode))
		{
			text = bannerCode;
		}
		string text2 = WarlordsNetwork.NormalizeBannerCodeOrDefault(text, string.Empty);
		if (string.IsNullOrWhiteSpace(text2))
		{
			return false;
		}
		banner = new BannerImageIdentifierVM(new Banner(text2), true);
		return true;
	}

	private void OnScoreboardBannerCodeChanged(string bannerCode)
	{
		RefreshScoreboardBanner();
	}

	private void OnScoreboardBannerClanUpdated(WarlordsClanObject clan)
	{
		RefreshScoreboardBanner();
	}

	private void OnScoreboardBannerClanAppearanceChanged(WarlordsClanObject clan)
	{
		RefreshScoreboardBanner();
	}

	private void OnScoreboardBannerInspectColorPreferenceChanged(WarlordsNetwork.InspectColorPreferenceCode inspectColorPreference)
	{
		RefreshScoreboardBanner();
	}

	private void OnScoreboardClanLoaded(WarlordsClanObject clan)
	{
		WarlordsClanObject warlordsClanObject = _warlordsPeerComponent?.Clan;
		if (clan != null && warlordsClanObject != null && (string.Equals(clan.Id ?? string.Empty, warlordsClanObject.Id ?? string.Empty, StringComparison.OrdinalIgnoreCase) || string.Equals(clan.Id ?? string.Empty, warlordsClanObject.FactionId ?? string.Empty, StringComparison.OrdinalIgnoreCase)))
		{
			RefreshScoreboardBanner();
		}
	}
}
You are not using the latest version of the tool, please update.
Latest version is '10.0.1.8346' (yours is '10.0.0.8330')
