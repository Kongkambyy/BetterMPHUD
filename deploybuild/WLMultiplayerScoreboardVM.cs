using System;
using System.Collections.Generic;
using System.Linq;
using MBWarlords.LastConcord.GauntletUI.Lobby;
using MBWarlords.LastConcord.Messages;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace MBWarlords.LastConcord.GauntletUI.MissionViews;

public sealed class WLMultiplayerScoreboardVM : ViewModel
{
	private readonly Action<string> _openClanAction;

	private readonly WLCommunityPlayerPopupVM _playerPopup;

	private readonly WLStoryCaptainPopupVM _storyCaptainPopup;

	private readonly WLMultiplayerScoreboardContentVM _scoreboard;

	private WarlordsPeerComponent _peerComponent;

	private string _playerPopupClanId = string.Empty;

	private string _playerPopupPlayerId = string.Empty;

	private bool _isPlayerPopupOpen;

	private bool _isStoryCaptainPopupOpen;

	[DataSourceProperty]
	public WLMultiplayerScoreboardContentVM Scoreboard => _scoreboard;

	[DataSourceProperty]
	public WLCommunityPlayerPopupVM PlayerPopup => _playerPopup;

	[DataSourceProperty]
	public WLStoryCaptainPopupVM StoryCaptainPopup => _storyCaptainPopup;

	[DataSourceProperty]
	public bool IsPlayerPopupOpen
	{
		get
		{
			return _isPlayerPopupOpen;
		}
		set
		{
			if (value != _isPlayerPopupOpen)
			{
				_isPlayerPopupOpen = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPlayerPopupOpen");
			}
		}
	}

	[DataSourceProperty]
	public bool IsStoryCaptainPopupOpen
	{
		get
		{
			return _isStoryCaptainPopupOpen;
		}
		set
		{
			if (value != _isStoryCaptainPopupOpen)
			{
				_isStoryCaptainPopupOpen = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsStoryCaptainPopupOpen");
			}
		}
	}

	public WLMultiplayerScoreboardVM(bool isSingleTeam, Mission mission, Action<string> openClanAction)
	{
		_openClanAction = openClanAction;
		_scoreboard = new WLMultiplayerScoreboardContentVM(isSingleTeam, mission, OpenPlayerPopup, OpenStoryCaptainPopup);
		_playerPopup = new WLCommunityPlayerPopupVM(ClosePlayerPopup, OpenClanFromPlayerPopup);
		_storyCaptainPopup = new WLStoryCaptainPopupVM(CloseStoryCaptainPopup);
		WLPlayersManager.Instance.PlayerLoaded += OnPlayerLoaded;
		WLClansManager.Instance.ClanLoaded += OnClanLoaded;
		EnsurePeerComponentSubscription();
		((ViewModel)this).RefreshValues();
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		WLPlayersManager.Instance.PlayerLoaded -= OnPlayerLoaded;
		WLClansManager.Instance.ClanLoaded -= OnClanLoaded;
		if (_peerComponent != null)
		{
			WarlordsPeerComponent peerComponent = _peerComponent;
			peerComponent.OnWLPlayerBattleStatsUpdated = (Action<string, WarlordsLeaderboardEntry, bool, string>)Delegate.Remove(peerComponent.OnWLPlayerBattleStatsUpdated, new Action<string, WarlordsLeaderboardEntry, bool, string>(OnPlayerBattleStatsUpdated));
			_peerComponent = null;
		}
		((ViewModel)_scoreboard).OnFinalize();
		((ViewModel)_playerPopup).OnFinalize();
		((ViewModel)_storyCaptainPopup).OnFinalize();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		((ViewModel)_scoreboard).RefreshValues();
		((ViewModel)_playerPopup).RefreshValues();
		((ViewModel)_storyCaptainPopup).RefreshValues();
	}

	public void Tick(float dt)
	{
		EnsurePeerComponentSubscription();
		_scoreboard.Tick(dt);
	}

	public void ClosePlayerPopup()
	{
		_playerPopupPlayerId = string.Empty;
		_playerPopupClanId = string.Empty;
		_playerPopup.IsPlayerPopupOpen = false;
		IsPlayerPopupOpen = false;
	}

	public void CloseStoryCaptainPopup()
	{
		_storyCaptainPopup.IsStoryCaptainPopupOpen = false;
		IsStoryCaptainPopupOpen = false;
	}

	private void OpenPlayerPopup(string playerId, string displayNameHint)
	{
		if (_scoreboard.IsActive)
		{
			string text = (playerId ?? string.Empty).Trim();
			if (!string.IsNullOrWhiteSpace(text))
			{
				_scoreboard.IsPlayerActionsActive = false;
				CloseStoryCaptainPopup();
				_playerPopupPlayerId = text;
				_playerPopupClanId = string.Empty;
				_playerPopup.PrepareForOpen(text, displayNameHint);
				_playerPopup.IsPlayerPopupOpen = true;
				IsPlayerPopupOpen = true;
				TryRefreshOpenPlayerPopupForPlayerId(text);
				TryRefreshOpenPlayerPopupBattleStatsForPlayerId(text);
				RequestPlayerPopupHydration(text);
				RequestPlayerPopupBattleStats(text);
			}
		}
	}

	private void OpenStoryCaptainPopup(WarlordsStoryCaptainSideData captain)
	{
		if (_scoreboard.IsActive && captain != null)
		{
			_scoreboard.IsPlayerActionsActive = false;
			ClosePlayerPopup();
			_storyCaptainPopup.PrepareForOpen(captain);
			_storyCaptainPopup.IsStoryCaptainPopupOpen = true;
			IsStoryCaptainPopupOpen = true;
		}
	}

	private void OpenClanFromPlayerPopup(string clanId)
	{
		string text = (clanId ?? string.Empty).Trim();
		if (!string.IsNullOrWhiteSpace(text))
		{
			ClosePlayerPopup();
			_openClanAction?.Invoke(text);
		}
	}

	private void EnsurePeerComponentSubscription()
	{
		NetworkCommunicator myPeer = GameNetwork.MyPeer;
		WarlordsPeerComponent warlordsPeerComponent = ((myPeer != null) ? PeerExtensions.GetComponent<WarlordsPeerComponent>(myPeer) : null);
		if (warlordsPeerComponent != _peerComponent)
		{
			if (_peerComponent != null)
			{
				WarlordsPeerComponent peerComponent = _peerComponent;
				peerComponent.OnWLPlayerBattleStatsUpdated = (Action<string, WarlordsLeaderboardEntry, bool, string>)Delegate.Remove(peerComponent.OnWLPlayerBattleStatsUpdated, new Action<string, WarlordsLeaderboardEntry, bool, string>(OnPlayerBattleStatsUpdated));
			}
			_peerComponent = warlordsPeerComponent;
			if (_peerComponent != null)
			{
				WarlordsPeerComponent peerComponent2 = _peerComponent;
				peerComponent2.OnWLPlayerBattleStatsUpdated = (Action<string, WarlordsLeaderboardEntry, bool, string>)Delegate.Combine(peerComponent2.OnWLPlayerBattleStatsUpdated, new Action<string, WarlordsLeaderboardEntry, bool, string>(OnPlayerBattleStatsUpdated));
			}
		}
	}

	private void OnPlayerLoaded(WarlordsPublicPlayerObject player)
	{
		if (player != null)
		{
			TryRefreshOpenPlayerPopupForPlayerId(player.PlayerId);
		}
	}

	private void OnClanLoaded(WarlordsClanObject clan)
	{
		if (clan != null)
		{
			TryRefreshOpenPlayerPopupForClanId(clan.Id);
		}
	}

	private void OnPlayerBattleStatsUpdated(string playerId, WarlordsLeaderboardEntry entry, bool success, string errorText)
	{
		TryRefreshOpenPlayerPopupBattleStatsForPlayerId(playerId);
	}

	private void TryRefreshOpenPlayerPopupForPlayerId(string playerId)
	{
		if (IsPlayerPopupOpen && !string.IsNullOrWhiteSpace(_playerPopupPlayerId) && string.Equals(_playerPopupPlayerId, playerId, StringComparison.OrdinalIgnoreCase) && WLPlayersManager.Instance.TryGetPlayer(_playerPopupPlayerId, out var player) && player != null && player.HasPublicDetails)
		{
			_playerPopup.ApplyPlayer(player);
			if (string.IsNullOrWhiteSpace(player.ClanId))
			{
				_playerPopupClanId = string.Empty;
				_playerPopup.HideClan();
				return;
			}
			_playerPopupClanId = player.ClanId;
			_playerPopup.SetClanLoading(player.ClanId);
			RequestPlayerPopupClanBannerIfNeeded(player.ClanId);
			TryRefreshOpenPlayerPopupForClanId(player.ClanId);
			RequestPlayerPopupClanHydration(player.ClanId);
		}
	}

	private void TryRefreshOpenPlayerPopupForClanId(string clanId)
	{
		if (IsPlayerPopupOpen && !string.IsNullOrWhiteSpace(_playerPopupClanId) && string.Equals(_playerPopupClanId, clanId, StringComparison.OrdinalIgnoreCase) && WLClansManager.Instance.TryGetClan(_playerPopupClanId, out var clan) && clan != null && clan.HasPublicDetails)
		{
			_playerPopup.ApplyClan(clan);
		}
	}

	private void TryRefreshOpenPlayerPopupBattleStatsForPlayerId(string playerId)
	{
		if (IsPlayerPopupOpen && !string.IsNullOrWhiteSpace(_playerPopupPlayerId) && string.Equals(_playerPopupPlayerId, playerId, StringComparison.OrdinalIgnoreCase) && _peerComponent != null)
		{
			_peerComponent.TryGetPublicPlayerBattleStats(playerId, out var entry, out var hasLoaded, out var errorText);
			_playerPopup.ApplyLeaderboardStats(playerId, entry, hasLoaded, errorText);
		}
	}

	private void RequestPlayerPopupHydration(string playerId)
	{
		IReadOnlyList<PlayerHydrationRequestEntry> readOnlyList = WLPlayersManager.Instance.BuildHydrationRequests(string.Empty, new string[1] { playerId }, PlayerHydrationStage.PublicInspect);
		if (readOnlyList != null && readOnlyList.Count != 0)
		{
			WarlordsNetwork.RequestPlayerHydrationBatch(string.Empty, readOnlyList);
		}
	}

	private void RequestPlayerPopupBattleStats(string playerId)
	{
		if (!string.IsNullOrWhiteSpace(playerId))
		{
			WarlordsNetworkComponent.RequestPlayerBattleStats(playerId);
		}
	}

	private void RequestPlayerPopupClanHydration(string clanId)
	{
		if (!GameNetwork.IsClientOrReplay || !WLClansManager.Instance.ShouldRequestClanHydration(clanId))
		{
			return;
		}
		ClanHydrationRequestEntry[] array = (from request in WLClansManager.Instance.BuildHydrationRequests(new string[1] { clanId }, ClanHydrationStage.PublicInspect)
			where request != null
			select request).ToArray();
		if (array.Length != 0)
		{
			int warlordsResolvedClanBatchCount = CompressionWarlords.WarlordsResolvedClanBatchCount;
			for (int num = 0; num < array.Length; num += warlordsResolvedClanBatchCount)
			{
				int num2 = Math.Min(warlordsResolvedClanBatchCount, array.Length - num);
				ClanHydrationRequestEntry[] array2 = new ClanHydrationRequestEntry[num2];
				Array.Copy(array, num, array2, 0, num2);
				GameNetwork.BeginModuleEventAsClient();
				GameNetwork.WriteMessage((GameNetworkMessage)(object)new RequestWLClanHydrationBatchMessage(array2));
				GameNetwork.EndModuleEventAsClient();
			}
		}
	}

	private void RequestPlayerPopupClanBannerIfNeeded(string clanId)
	{
		if (GameNetwork.IsClientOrReplay && !string.IsNullOrWhiteSpace(clanId) && !WLClansManager.Instance.TryGetBanner(clanId, out var _) && WLClansManager.Instance.TryMarkBannerRequestPending(clanId))
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage((GameNetworkMessage)(object)new RequestWLClanBannerMessage(clanId));
			GameNetwork.EndModuleEventAsClient();
		}
	}
}
You are not using the latest version of the tool, please update.
Latest version is '10.0.1.8346' (yours is '10.0.0.8330')
