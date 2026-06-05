using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MBWarlords.LastConcord.GauntletUI.Lobby;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Missions.Multiplayer;
using TaleWorlds.MountAndBlade.Multiplayer;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Scoreboard;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlatformService;
using TaleWorlds.PlayerServices;

namespace MBWarlords.LastConcord.GauntletUI.MissionViews;

public sealed class WLMultiplayerScoreboardContentVM : ViewModel
{
	private sealed class PlayerCardActionContext
	{
		public WLMultiplayerScoreboardPlayerVM Player { get; }

		public PlayerCardActionContext(WLMultiplayerScoreboardPlayerVM player)
		{
			Player = player;
		}
	}

	private const float AttributeRefreshDuration = 1f;

	private readonly ChatBox _chatBox;

	private readonly Dictionary<BattleSideEnum, WLMultiplayerScoreboardSideVM> _missionSides;

	private readonly Mission _mission;

	private readonly MissionScoreboardComponent _missionScoreboardComponent;

	private readonly MultiplayerPollComponent _missionPollComponent;

	private readonly Action<string, string> _openPlayerPopupAction;

	private readonly Action<WarlordsStoryCaptainSideData> _openStoryCaptainPopupAction;

	private readonly WLProximityVoiceChatHandler _proximityVoiceChatHandler;

	private MultiplayerPermissionHandler _permissionHandler;

	private VoiceChatHandler _voiceChatHandler;

	private float _attributeRefreshTimeElapsed;

	private bool _canStartKickPolls;

	private bool _hasMutedAll;

	private TextObject _muteAllText = new TextObject("{=AZSbwcG5}Mute All", (Dictionary<string, object>)null);

	private TextObject _unmuteAllText = new TextObject("{=SzRVIPeZ}Unmute All", (Dictionary<string, object>)null);

	private MPEndOfBattleVM _endOfBattle;

	private InputKeyItemVM _showMouseKey;

	private MBBindingList<WLMultiplayerScoreboardSideVM> _sides;

	private MBBindingList<StringPairItemWithActionVM> _playerActionList;

	private WLMultiplayerScoreboardSideVM _allySide;

	private WLMultiplayerScoreboardSideVM _enemySide;

	private WLSubPageClanElementVM _allyCaptainPortraitEntry;

	private WLSubPageClanElementVM _enemyCaptainPortraitEntry;

	private bool _allyHasCaptainPortrait;

	private bool _enemyHasCaptainPortrait;

	private string _spectators;

	private string _missionName;

	private string _gameModeText;

	private string _mapName;

	private string _serverName;

	private bool _isBotsEnabled;

	private bool _isSingleSide;

	private bool _isInitalizationOver;

	private bool _isUpdateOver;

	private bool _isMouseEnabled;

	private bool _isPlayerActionsActive;

	private bool _isActive;

	private string _toggleMuteText;

	[DataSourceProperty]
	public MPEndOfBattleVM EndOfBattle
	{
		get
		{
			return _endOfBattle;
		}
		set
		{
			if (value != _endOfBattle)
			{
				_endOfBattle = value;
				((ViewModel)this).OnPropertyChangedWithValue<MPEndOfBattleVM>(value, "EndOfBattle");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<StringPairItemWithActionVM> PlayerActionList
	{
		get
		{
			return _playerActionList;
		}
		set
		{
			if (value != _playerActionList)
			{
				_playerActionList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<StringPairItemWithActionVM>>(value, "PlayerActionList");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<WLMultiplayerScoreboardSideVM> Sides
	{
		get
		{
			return _sides;
		}
		set
		{
			if (value != _sides)
			{
				_sides = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<WLMultiplayerScoreboardSideVM>>(value, "Sides");
			}
		}
	}

	[DataSourceProperty]
	public WLMultiplayerScoreboardSideVM AllySide
	{
		get
		{
			return _allySide;
		}
		set
		{
			if (value != _allySide)
			{
				_allySide = value;
				((ViewModel)this).OnPropertyChangedWithValue<WLMultiplayerScoreboardSideVM>(value, "AllySide");
				HasAllySide = value != null;
			}
		}
	}

	[DataSourceProperty]
	public WLMultiplayerScoreboardSideVM EnemySide
	{
		get
		{
			return _enemySide;
		}
		set
		{
			if (value != _enemySide)
			{
				_enemySide = value;
				((ViewModel)this).OnPropertyChangedWithValue<WLMultiplayerScoreboardSideVM>(value, "EnemySide");
				HasEnemySide = value != null;
			}
		}
	}

	[DataSourceProperty]
	public WLSubPageClanElementVM AllyCaptainPortraitEntry
	{
		get
		{
			return _allyCaptainPortraitEntry;
		}
		set
		{
			if (value != _allyCaptainPortraitEntry)
			{
				_allyCaptainPortraitEntry = value;
				((ViewModel)this).OnPropertyChangedWithValue<WLSubPageClanElementVM>(value, "AllyCaptainPortraitEntry");
			}
		}
	}

	[DataSourceProperty]
	public WLSubPageClanElementVM EnemyCaptainPortraitEntry
	{
		get
		{
			return _enemyCaptainPortraitEntry;
		}
		set
		{
			if (value != _enemyCaptainPortraitEntry)
			{
				_enemyCaptainPortraitEntry = value;
				((ViewModel)this).OnPropertyChangedWithValue<WLSubPageClanElementVM>(value, "EnemyCaptainPortraitEntry");
			}
		}
	}

	[DataSourceProperty]
	public bool AllyHasCaptainPortrait
	{
		get
		{
			return _allyHasCaptainPortrait;
		}
		set
		{
			if (value != _allyHasCaptainPortrait)
			{
				_allyHasCaptainPortrait = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "AllyHasCaptainPortrait");
			}
		}
	}

	[DataSourceProperty]
	public bool EnemyHasCaptainPortrait
	{
		get
		{
			return _enemyHasCaptainPortrait;
		}
		set
		{
			if (value != _enemyHasCaptainPortrait)
			{
				_enemyHasCaptainPortrait = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "EnemyHasCaptainPortrait");
			}
		}
	}

	[DataSourceProperty]
	public bool HasAllySide
	{
		get
		{
			return _allySide != null;
		}
		set
		{
			((ViewModel)this).OnPropertyChangedWithValue(value, "HasAllySide");
		}
	}

	[DataSourceProperty]
	public bool HasEnemySide
	{
		get
		{
			return _enemySide != null;
		}
		set
		{
			((ViewModel)this).OnPropertyChangedWithValue(value, "HasEnemySide");
		}
	}

	[DataSourceProperty]
	public bool IsUpdateOver
	{
		get
		{
			return _isUpdateOver;
		}
		set
		{
			if (value != _isUpdateOver)
			{
				_isUpdateOver = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsUpdateOver");
			}
		}
	}

	[DataSourceProperty]
	public bool IsInitalizationOver
	{
		get
		{
			return _isInitalizationOver;
		}
		set
		{
			if (value != _isInitalizationOver)
			{
				_isInitalizationOver = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsInitalizationOver");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMouseEnabled
	{
		get
		{
			return _isMouseEnabled;
		}
		set
		{
			if (value != _isMouseEnabled)
			{
				_isMouseEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsMouseEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsActive
	{
		get
		{
			return _isActive;
		}
		set
		{
			if (value != _isActive)
			{
				_isActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsActive");
				if (value)
				{
					RefreshPlayerDeadStates();
				}
			}
		}
	}

	[DataSourceProperty]
	public bool IsPlayerActionsActive
	{
		get
		{
			return _isPlayerActionsActive;
		}
		set
		{
			if (value != _isPlayerActionsActive)
			{
				_isPlayerActionsActive = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPlayerActionsActive");
			}
		}
	}

	[DataSourceProperty]
	public string Spectators
	{
		get
		{
			return _spectators;
		}
		set
		{
			if (value != _spectators)
			{
				_spectators = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Spectators");
			}
		}
	}

	[DataSourceProperty]
	public InputKeyItemVM ShowMouseKey
	{
		get
		{
			return _showMouseKey;
		}
		set
		{
			if (value != _showMouseKey)
			{
				_showMouseKey = value;
				((ViewModel)this).OnPropertyChangedWithValue<InputKeyItemVM>(value, "ShowMouseKey");
			}
		}
	}

	[DataSourceProperty]
	public string MissionName
	{
		get
		{
			return _missionName;
		}
		set
		{
			if (value != _missionName)
			{
				_missionName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MissionName");
			}
		}
	}

	[DataSourceProperty]
	public string GameModeText
	{
		get
		{
			return _gameModeText;
		}
		set
		{
			if (value != _gameModeText)
			{
				_gameModeText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "GameModeText");
			}
		}
	}

	[DataSourceProperty]
	public string MapName
	{
		get
		{
			return _mapName;
		}
		set
		{
			if (value != _mapName)
			{
				_mapName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "MapName");
			}
		}
	}

	[DataSourceProperty]
	public string ServerName
	{
		get
		{
			return _serverName;
		}
		set
		{
			if (value != _serverName)
			{
				_serverName = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ServerName");
			}
		}
	}

	[DataSourceProperty]
	public bool IsBotsEnabled
	{
		get
		{
			return _isBotsEnabled;
		}
		set
		{
			if (value != _isBotsEnabled)
			{
				_isBotsEnabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsBotsEnabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSingleSide
	{
		get
		{
			return _isSingleSide;
		}
		set
		{
			if (value != _isSingleSide)
			{
				_isSingleSide = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSingleSide");
			}
		}
	}

	[DataSourceProperty]
	public string ToggleMuteText
	{
		get
		{
			return _toggleMuteText;
		}
		set
		{
			if (value != _toggleMuteText)
			{
				_toggleMuteText = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "ToggleMuteText");
			}
		}
	}

	public WLMultiplayerScoreboardContentVM(bool isSingleTeam, Mission mission, Action<string, string> openPlayerPopupAction, Action<WarlordsStoryCaptainSideData> openStoryCaptainPopupAction)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Invalid comparison between Unknown and I4
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Invalid comparison between Unknown and I4
		_openPlayerPopupAction = openPlayerPopupAction;
		_openStoryCaptainPopupAction = openStoryCaptainPopupAction;
		_chatBox = Game.Current.GetGameHandler<ChatBox>();
		_chatBox.OnPlayerMuteChanged += new PlayerMutedDelegate(OnPlayerMuteChanged);
		_mission = mission;
		MissionLobbyComponent missionBehavior = mission.GetMissionBehavior<MissionLobbyComponent>();
		_missionScoreboardComponent = mission.GetMissionBehavior<MissionScoreboardComponent>();
		_voiceChatHandler = _mission.GetMissionBehavior<VoiceChatHandler>();
		_proximityVoiceChatHandler = _mission.GetMissionBehavior<WLProximityVoiceChatHandler>();
		_permissionHandler = GameNetwork.GetNetworkComponent<MultiplayerPermissionHandler>();
		_canStartKickPolls = MultiplayerOptionsExtensions.GetBoolValue((OptionType)5, (MultiplayerOptionsAccessMode)1);
		if (_canStartKickPolls)
		{
			_missionPollComponent = mission.GetMissionBehavior<MultiplayerPollComponent>();
		}
		EndOfBattle = new MPEndOfBattleVM(mission, _missionScoreboardComponent, isSingleTeam);
		PlayerActionList = new MBBindingList<StringPairItemWithActionVM>();
		Sides = new MBBindingList<WLMultiplayerScoreboardSideVM>();
		_missionSides = new Dictionary<BattleSideEnum, WLMultiplayerScoreboardSideVM>();
		AllyCaptainPortraitEntry = CreateHiddenCaptainPortraitEntry();
		EnemyCaptainPortraitEntry = CreateHiddenCaptainPortraitEntry();
		IsSingleSide = isSingleTeam;
		InitSides();
		GameKey gameKey = HotKeyManager.GetCategory("ScoreboardHotKeyCategory").GetGameKey(35);
		ShowMouseKey = InputKeyItemVM.CreateFromGameKey(gameKey, false);
		MissionName = string.Empty;
		IsBotsEnabled = (int)missionBehavior.MissionType == 4 || (int)missionBehavior.MissionType == 3;
		RegisterEvents();
		((ViewModel)this).RefreshValues();
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		UnregisterEvents();
		foreach (WLMultiplayerScoreboardSideVM item in (Collection<WLMultiplayerScoreboardSideVM>)(object)Sides)
		{
			((ViewModel)item).OnFinalize();
		}
	}

	public override void RefreshValues()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		((ViewModel)this).RefreshValues();
		MissionLobbyComponent missionBehavior = _mission.GetMissionBehavior<MissionLobbyComponent>();
		UpdateToggleMuteText();
		GameModeText = ((object)GameTexts.FindText("str_multiplayer_game_type", ((object)missionBehavior.MissionType/*cast due to .constrained prefix*/).ToString())).ToString().ToLower();
		((ViewModel)EndOfBattle).RefreshValues();
		Sides.ApplyActionOnAllItems((Action<WLMultiplayerScoreboardSideVM>)delegate(WLMultiplayerScoreboardSideVM side)
		{
			((ViewModel)side).RefreshValues();
		});
		SyncCaptainPortraitEntries();
		TextObject val = default(TextObject);
		if (GameTexts.TryGetText("str_multiplayer_scene_name", ref val, ((MissionBehavior)missionBehavior).Mission.SceneName))
		{
			MapName = ((object)val).ToString();
		}
		else
		{
			MapName = ((MissionBehavior)missionBehavior).Mission.SceneName;
		}
		ServerName = MultiplayerOptionsExtensions.GetStrValue((OptionType)0, (MultiplayerOptionsAccessMode)1);
		InputKeyItemVM showMouseKey = ShowMouseKey;
		if (showMouseKey != null)
		{
			((ViewModel)showMouseKey).RefreshValues();
		}
	}

	public void SetMouseState(bool isMouseVisible)
	{
		IsMouseEnabled = isMouseVisible;
	}

	public void Tick(float dt)
	{
		if (!IsActive)
		{
			return;
		}
		MPEndOfBattleVM endOfBattle = EndOfBattle;
		if (endOfBattle != null)
		{
			endOfBattle.Tick(dt);
		}
		CheckAttributeRefresh(dt);
		foreach (WLMultiplayerScoreboardSideVM item in (Collection<WLMultiplayerScoreboardSideVM>)(object)Sides)
		{
			item.Tick(dt);
		}
		SyncCaptainPortraitEntries();
		foreach (WLMultiplayerScoreboardSideVM item2 in (Collection<WLMultiplayerScoreboardSideVM>)(object)Sides)
		{
			foreach (WLMultiplayerScoreboardPlayerVM item3 in (Collection<WLMultiplayerScoreboardPlayerVM>)(object)item2.Players)
			{
				((MPPlayerVM)item3).RefreshDivision(IsSingleSide);
			}
		}
	}

	public void RefreshPlayerDeadStates()
	{
		foreach (WLMultiplayerScoreboardSideVM item in (Collection<WLMultiplayerScoreboardSideVM>)(object)Sides)
		{
			item.RefreshPlayerDeadStates();
		}
	}

	public void DecreaseSpectatorCount(MissionPeer spectatedPeer)
	{
	}

	public void IncreaseSpectatorCount(MissionPeer spectatedPeer)
	{
	}

	public void ExecuteToggleMute()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		foreach (WLMultiplayerScoreboardSideVM item in (Collection<WLMultiplayerScoreboardSideVM>)(object)Sides)
		{
			foreach (WLMultiplayerScoreboardPlayerVM item2 in (Collection<WLMultiplayerScoreboardPlayerVM>)(object)item.Players)
			{
				if (!item2.IsMine && ((MPPlayerVM)item2).Peer != null)
				{
					_chatBox.SetPlayerMuted(((PeerComponent)((MPPlayerVM)item2).Peer).Peer.Id, !_hasMutedAll);
					((MPPlayerVM)item2).Peer.SetMuted(!_hasMutedAll);
					item2.UpdateIsMuted();
				}
			}
		}
		_hasMutedAll = !_hasMutedAll;
		UpdateToggleMuteText();
	}

	private void RegisterEvents()
	{
		if (_voiceChatHandler != null)
		{
			_voiceChatHandler.OnPeerMuteStatusUpdated += OnPeerMuteStatusUpdated;
		}
		if (_proximityVoiceChatHandler != null)
		{
			_proximityVoiceChatHandler.OnPeerMuteStatusUpdated += OnPeerMuteStatusUpdated;
		}
		if (_permissionHandler != null)
		{
			_permissionHandler.OnPlayerPlatformMuteChanged += OnPlayerPlatformMuteChanged;
		}
		_missionScoreboardComponent.OnPlayerSideChanged += OnPlayerSideChanged;
		_missionScoreboardComponent.OnPlayerPropertiesChanged += OnPlayerPropertiesChanged;
		_missionScoreboardComponent.OnBotPropertiesChanged += OnBotPropertiesChanged;
		_missionScoreboardComponent.OnRoundPropertiesChanged += OnRoundPropertiesChanged;
		_missionScoreboardComponent.OnScoreboardInitialized += OnScoreboardInitialized;
		_missionScoreboardComponent.OnMVPSelected += OnMVPSelected;
		WLLobbyClientDataManager.Instance.BattleStoryCaptainsLoaded += OnBattleStoryCaptainsLoaded;
	}

	private void UnregisterEvents()
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		_missionScoreboardComponent.OnPlayerSideChanged -= OnPlayerSideChanged;
		_missionScoreboardComponent.OnPlayerPropertiesChanged -= OnPlayerPropertiesChanged;
		_missionScoreboardComponent.OnBotPropertiesChanged -= OnBotPropertiesChanged;
		_missionScoreboardComponent.OnRoundPropertiesChanged -= OnRoundPropertiesChanged;
		_missionScoreboardComponent.OnScoreboardInitialized -= OnScoreboardInitialized;
		_missionScoreboardComponent.OnMVPSelected -= OnMVPSelected;
		WLLobbyClientDataManager.Instance.BattleStoryCaptainsLoaded -= OnBattleStoryCaptainsLoaded;
		_chatBox.OnPlayerMuteChanged -= new PlayerMutedDelegate(OnPlayerMuteChanged);
		if (_voiceChatHandler != null)
		{
			_voiceChatHandler.OnPeerMuteStatusUpdated -= OnPeerMuteStatusUpdated;
		}
		if (_proximityVoiceChatHandler != null)
		{
			_proximityVoiceChatHandler.OnPeerMuteStatusUpdated -= OnPeerMuteStatusUpdated;
		}
		if (_permissionHandler != null)
		{
			_permissionHandler.OnPlayerPlatformMuteChanged -= OnPlayerPlatformMuteChanged;
		}
	}

	private void ExecutePopulateActionList(WLMultiplayerScoreboardPlayerVM player)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Expected O, but got Unknown
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Expected O, but got Unknown
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Expected O, but got Unknown
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Expected O, but got Unknown
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Expected O, but got Unknown
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Expected O, but got Unknown
		((Collection<StringPairItemWithActionVM>)(object)PlayerActionList).Clear();
		if (((player != null) ? ((MPPlayerVM)player).Peer : null) != null && !player.IsMine && !player.IsBot)
		{
			PlayerId id = ((PeerComponent)((MPPlayerVM)player).Peer).Peer.Id;
			((Collection<StringPairItemWithActionVM>)(object)PlayerActionList).Add(new StringPairItemWithActionVM((Action<object>)ExecuteShowPlayerCard, ((object)new TextObject("{=wl_mission_player_card_action}Inspect Player Card", (Dictionary<string, object>)null)).ToString(), "ViewProfile", (object)new PlayerCardActionContext(player)));
			bool flag = _chatBox.IsPlayerMutedFromGame(id);
			bool flag2 = PermaMuteList.IsPlayerMuted(id);
			bool flag3 = _chatBox.IsPlayerMutedFromPlatform(id);
			bool isMutedFromPlatform = ((MPPlayerVM)player).Peer.IsMutedFromPlatform;
			if (!flag3)
			{
				if (!flag2)
				{
					if (PlatformServices.Instance.IsPermanentMuteAvailable)
					{
						((Collection<StringPairItemWithActionVM>)(object)PlayerActionList).Add(new StringPairItemWithActionVM((Action<object>)ExecutePermanentlyMute, ((object)new TextObject("{=77jmd4QF}Mute Permanently", (Dictionary<string, object>)null)).ToString(), "PermanentMute", (object)player));
					}
					string text = (flag ? ((object)GameTexts.FindText("str_mp_scoreboard_context_unmute_text", (string)null)).ToString() : ((object)GameTexts.FindText("str_mp_scoreboard_context_mute_text", (string)null)).ToString());
					((Collection<StringPairItemWithActionVM>)(object)PlayerActionList).Add(new StringPairItemWithActionVM((Action<object>)ExecuteMute, text, flag ? "UnmuteText" : "MuteText", (object)player));
				}
				else
				{
					((Collection<StringPairItemWithActionVM>)(object)PlayerActionList).Add(new StringPairItemWithActionVM((Action<object>)ExecuteLiftPermanentMute, ((object)new TextObject("{=CIVPNf2d}Remove Permanent Mute", (Dictionary<string, object>)null)).ToString(), "UnmuteText", (object)player));
				}
			}
			if (player.IsTeammate)
			{
				if (!isMutedFromPlatform && HasVoiceChatHandler() && !flag2)
				{
					string text2 = (((MPPlayerVM)player).Peer.IsMuted ? ((object)GameTexts.FindText("str_mp_scoreboard_context_unmute_voice", (string)null)).ToString() : ((object)GameTexts.FindText("str_mp_scoreboard_context_mute_voice", (string)null)).ToString());
					((Collection<StringPairItemWithActionVM>)(object)PlayerActionList).Add(new StringPairItemWithActionVM((Action<object>)ExecuteMuteVoice, text2, ((MPPlayerVM)player).Peer.IsMuted ? "UnmuteVoice" : "MuteVoice", (object)player));
				}
				if (_canStartKickPolls)
				{
					((Collection<StringPairItemWithActionVM>)(object)PlayerActionList).Add(new StringPairItemWithActionVM((Action<object>)ExecuteKick, ((object)GameTexts.FindText("str_mp_scoreboard_context_kick", (string)null)).ToString(), "StartKickPoll", (object)player));
				}
			}
			StringPairItemWithActionVM val = new StringPairItemWithActionVM((Action<object>)ExecuteReport, ((object)GameTexts.FindText("str_mp_scoreboard_context_report", (string)null)).ToString(), "Report", (object)player);
			if (MultiplayerReportPlayerManager.IsPlayerReportedOverLimit(id))
			{
				val.IsEnabled = false;
				val.Hint.HintText = new TextObject("{=klkYFik9}You've already reported this player.", (Dictionary<string, object>)null);
			}
			((Collection<StringPairItemWithActionVM>)(object)PlayerActionList).Add(val);
			MultiplayerPlayerContextMenuHelper.AddMissionViewProfileOptions((MPPlayerVM)(object)player, PlayerActionList);
		}
		if (((Collection<StringPairItemWithActionVM>)(object)PlayerActionList).Count > 0)
		{
			IsPlayerActionsActive = false;
			IsPlayerActionsActive = true;
		}
	}

	private void ExecuteShowPlayerCard(object contextObject)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (!(contextObject is PlayerCardActionContext playerCardActionContext))
		{
			return;
		}
		WLMultiplayerScoreboardPlayerVM player = playerCardActionContext.Player;
		object obj;
		if (player == null)
		{
			obj = null;
		}
		else
		{
			MissionPeer peer = ((MPPlayerVM)player).Peer;
			obj = ((peer != null) ? ((PeerComponent)peer).Peer : null);
		}
		if (obj != null)
		{
			string text = ((object)((PeerComponent)((MPPlayerVM)playerCardActionContext.Player).Peer).Peer.Id/*cast due to .constrained prefix*/).ToString();
			if (!string.IsNullOrWhiteSpace(text))
			{
				IsPlayerActionsActive = false;
				_openPlayerPopupAction?.Invoke(text, ((MPPlayerVM)playerCardActionContext.Player).Peer.DisplayedName);
			}
		}
	}

	private void ExecuteReport(object playerObj)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		WLMultiplayerScoreboardPlayerVM wLMultiplayerScoreboardPlayerVM = playerObj as WLMultiplayerScoreboardPlayerVM;
		MultiplayerReportPlayerManager.RequestReportPlayer(NetworkMain.GameClient.CurrentMatchId, ((PeerComponent)((MPPlayerVM)wLMultiplayerScoreboardPlayerVM).Peer).Peer.Id, ((MPPlayerVM)wLMultiplayerScoreboardPlayerVM).Peer.DisplayedName, true);
	}

	private void ExecuteMute(object playerObj)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		WLMultiplayerScoreboardPlayerVM wLMultiplayerScoreboardPlayerVM = playerObj as WLMultiplayerScoreboardPlayerVM;
		bool flag = _chatBox.IsPlayerMutedFromGame(((PeerComponent)((MPPlayerVM)wLMultiplayerScoreboardPlayerVM).Peer).Peer.Id);
		_chatBox.SetPlayerMuted(((PeerComponent)((MPPlayerVM)wLMultiplayerScoreboardPlayerVM).Peer).Peer.Id, !flag);
		GameTexts.SetVariable("PLAYER_NAME", ((MPPlayerVM)wLMultiplayerScoreboardPlayerVM).Peer.DisplayedName);
		InformationManager.DisplayMessage(new InformationMessage((!flag) ? ((object)GameTexts.FindText("str_mute_notification", (string)null)).ToString() : ((object)GameTexts.FindText("str_unmute_notification", (string)null)).ToString()));
	}

	private void ExecuteMuteVoice(object playerObj)
	{
		WLMultiplayerScoreboardPlayerVM wLMultiplayerScoreboardPlayerVM = playerObj as WLMultiplayerScoreboardPlayerVM;
		((MPPlayerVM)wLMultiplayerScoreboardPlayerVM).Peer.SetMuted(!((MPPlayerVM)wLMultiplayerScoreboardPlayerVM).Peer.IsMuted);
		wLMultiplayerScoreboardPlayerVM.UpdateIsMuted();
	}

	private void ExecutePermanentlyMute(object playerObj)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		WLMultiplayerScoreboardPlayerVM wLMultiplayerScoreboardPlayerVM = playerObj as WLMultiplayerScoreboardPlayerVM;
		PermaMuteList.MutePlayer(((PeerComponent)((MPPlayerVM)wLMultiplayerScoreboardPlayerVM).Peer).Peer.Id, ((PeerComponent)((MPPlayerVM)wLMultiplayerScoreboardPlayerVM).Peer).Name);
		((MPPlayerVM)wLMultiplayerScoreboardPlayerVM).Peer.SetMuted(true);
		wLMultiplayerScoreboardPlayerVM.UpdateIsMuted();
		GameTexts.SetVariable("PLAYER_NAME", ((MPPlayerVM)wLMultiplayerScoreboardPlayerVM).Peer.DisplayedName);
		InformationManager.DisplayMessage(new InformationMessage(((object)GameTexts.FindText("str_permanent_mute_notification", (string)null)).ToString()));
	}

	private void ExecuteLiftPermanentMute(object playerObj)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		WLMultiplayerScoreboardPlayerVM wLMultiplayerScoreboardPlayerVM = playerObj as WLMultiplayerScoreboardPlayerVM;
		PermaMuteList.RemoveMutedPlayer(((PeerComponent)((MPPlayerVM)wLMultiplayerScoreboardPlayerVM).Peer).Peer.Id);
		((MPPlayerVM)wLMultiplayerScoreboardPlayerVM).Peer.SetMuted(false);
		wLMultiplayerScoreboardPlayerVM.UpdateIsMuted();
		GameTexts.SetVariable("PLAYER_NAME", ((MPPlayerVM)wLMultiplayerScoreboardPlayerVM).Peer.DisplayedName);
		InformationManager.DisplayMessage(new InformationMessage(((object)GameTexts.FindText("str_unmute_notification", (string)null)).ToString()));
	}

	private void ExecuteKick(object playerObj)
	{
		WLMultiplayerScoreboardPlayerVM wLMultiplayerScoreboardPlayerVM = playerObj as WLMultiplayerScoreboardPlayerVM;
		_missionPollComponent.RequestKickPlayerPoll(PeerExtensions.GetNetworkPeer((PeerComponent)(object)((MPPlayerVM)wLMultiplayerScoreboardPlayerVM).Peer), false);
	}

	private void CheckAttributeRefresh(float dt)
	{
		_attributeRefreshTimeElapsed += dt;
		if (_attributeRefreshTimeElapsed >= 1f)
		{
			UpdateSideAllPlayersAttributes((BattleSideEnum)1);
			UpdateSideAllPlayersAttributes((BattleSideEnum)0);
			_attributeRefreshTimeElapsed = 0f;
		}
	}

	private void UpdateSideAllPlayersAttributes(BattleSideEnum battleSide)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		MissionScoreboardSide val = _missionScoreboardComponent.Sides.FirstOrDefault((MissionScoreboardSide entry) => entry != null && entry.Side == battleSide);
		if (val == null)
		{
			return;
		}
		foreach (MissionPeer player in val.Players)
		{
			OnPlayerPropertiesChanged(battleSide, player);
		}
	}

	private void OnPlayerPlatformMuteChanged(PlayerId playerId, bool isPlayerMuted)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		foreach (WLMultiplayerScoreboardSideVM item in (Collection<WLMultiplayerScoreboardSideVM>)(object)Sides)
		{
			foreach (WLMultiplayerScoreboardPlayerVM item2 in (Collection<WLMultiplayerScoreboardPlayerVM>)(object)item.Players)
			{
				if (!item2.IsBot)
				{
					PlayerId id = ((PeerComponent)((MPPlayerVM)item2).Peer).Peer.Id;
					if (((PlayerId)(ref id)).Equals(playerId))
					{
						item2.UpdateIsMuted();
						return;
					}
				}
			}
		}
	}

	private void OnPlayerMuteChanged(PlayerId playerId, bool isMuted)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		foreach (WLMultiplayerScoreboardSideVM item in (Collection<WLMultiplayerScoreboardSideVM>)(object)Sides)
		{
			foreach (WLMultiplayerScoreboardPlayerVM item2 in (Collection<WLMultiplayerScoreboardPlayerVM>)(object)item.Players)
			{
				if (!item2.IsBot)
				{
					PlayerId id = ((PeerComponent)((MPPlayerVM)item2).Peer).Peer.Id;
					if (((PlayerId)(ref id)).Equals(playerId))
					{
						item2.UpdateIsMuted();
						return;
					}
				}
			}
		}
	}

	private void OnPlayerSideChanged(Team curTeam, Team nextTeam, MissionPeer client)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		if (((PeerComponent)client).IsMine && nextTeam != null && IsSideValid(nextTeam.Side))
		{
			InitSides();
			return;
		}
		if (curTeam != null && IsSideValid(curTeam.Side))
		{
			_missionSides[_missionScoreboardComponent.GetSideSafe(curTeam.Side).Side].RemovePlayer(client);
		}
		if (nextTeam != null && IsSideValid(nextTeam.Side))
		{
			_missionSides[_missionScoreboardComponent.GetSideSafe(nextTeam.Side).Side].AddPlayer(client);
		}
		SyncCaptainPortraitEntries();
	}

	private void OnRoundPropertiesChanged()
	{
		foreach (WLMultiplayerScoreboardSideVM value in _missionSides.Values)
		{
			value.UpdateRoundAttributes();
		}
	}

	private void OnPlayerPropertiesChanged(BattleSideEnum side, MissionPeer client)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (IsSideValid(side))
		{
			_missionSides[_missionScoreboardComponent.GetSideSafe(side).Side].UpdatePlayerAttributes(client);
			SyncCaptainPortraitEntries();
		}
	}

	private void OnBotPropertiesChanged(BattleSideEnum side)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		BattleSideEnum side2 = _missionScoreboardComponent.GetSideSafe(side).Side;
		if (IsSideValid(side2))
		{
			_missionSides[side2].UpdateBotAttributes();
			SyncCaptainPortraitEntries();
		}
	}

	private void OnScoreboardInitialized()
	{
		InitSides();
	}

	private void OnMVPSelected(MissionPeer mvpPeer, int mvpCount)
	{
		foreach (WLMultiplayerScoreboardSideVM item in (Collection<WLMultiplayerScoreboardSideVM>)(object)Sides)
		{
			foreach (WLMultiplayerScoreboardPlayerVM item2 in (Collection<WLMultiplayerScoreboardPlayerVM>)(object)item.Players)
			{
				if (((MPPlayerVM)item2).Peer == mvpPeer)
				{
					item2.SetMVPBadgeCount(mvpCount);
					break;
				}
			}
		}
	}

	private void OnBattleStoryCaptainsLoaded(WarlordsStoryCaptainScenarioData captains)
	{
		Sides.ApplyActionOnAllItems((Action<WLMultiplayerScoreboardSideVM>)delegate(WLMultiplayerScoreboardSideVM side)
		{
			((ViewModel)side).RefreshValues();
		});
		SyncCaptainPortraitEntries();
	}

	private bool IsSideValid(BattleSideEnum side)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Invalid comparison between Unknown and I4
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		if (IsSingleSide)
		{
			return _missionScoreboardComponent != null && (int)side != -1 && (int)side != 2;
		}
		return _missionScoreboardComponent != null && (int)side != -1 && (int)side != 2 && _missionScoreboardComponent.Sides.Any((MissionScoreboardSide entry) => entry != null && entry.Side == side);
	}

	private void InitSides()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Invalid comparison between Unknown and I4
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Invalid comparison between Unknown and I4
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Invalid comparison between Unknown and I4
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Invalid comparison between Unknown and I4
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		((Collection<WLMultiplayerScoreboardSideVM>)(object)Sides).Clear();
		_missionSides.Clear();
		AllySide = null;
		EnemySide = null;
		AllyCaptainPortraitEntry = CreateHiddenCaptainPortraitEntry();
		EnemyCaptainPortraitEntry = CreateHiddenCaptainPortraitEntry();
		AllyHasCaptainPortrait = false;
		EnemyHasCaptainPortrait = false;
		if (IsSingleSide)
		{
			MissionScoreboardSide sideSafe = _missionScoreboardComponent.GetSideSafe((BattleSideEnum)0);
			MultiplayerBattleColors val = MultiplayerBattleColors.CreateWith(ResolveScoreboardSideCulture(sideSafe), (BasicCultureObject)null);
			WLMultiplayerScoreboardSideVM wLMultiplayerScoreboardSideVM = new WLMultiplayerScoreboardSideVM(sideSafe, ExecutePopulateActionList, _openPlayerPopupAction, _openStoryCaptainPopupAction, IsSingleSide, isSecondSide: false, val.AttackerColors);
			((Collection<WLMultiplayerScoreboardSideVM>)(object)Sides).Add(wLMultiplayerScoreboardSideVM);
			_missionSides.Add(sideSafe.Side, wLMultiplayerScoreboardSideVM);
			AllySide = wLMultiplayerScoreboardSideVM;
			AllyCaptainPortraitEntry = wLMultiplayerScoreboardSideVM.CaptainPortraitEntry ?? CreateHiddenCaptainPortraitEntry();
			AllyHasCaptainPortrait = wLMultiplayerScoreboardSideVM.HasCaptainPortrait;
			return;
		}
		NetworkCommunicator myPeer = GameNetwork.MyPeer;
		MissionPeer val2 = ((myPeer != null) ? PeerExtensions.GetComponent<MissionPeer>(myPeer) : null);
		MissionScoreboardSide val3 = _missionScoreboardComponent.Sides.FirstOrDefault((MissionScoreboardSide entry) => entry != null && (int)entry.Side == 1);
		MissionScoreboardSide val4 = _missionScoreboardComponent.Sides.FirstOrDefault((MissionScoreboardSide entry) => entry != null && (int)entry.Side == 0);
		MissionScoreboardSide obj;
		if (val2 != null)
		{
			Team team = val2.Team;
			if ((int)((team != null) ? new BattleSideEnum?(team.Side) : ((BattleSideEnum?)null)).GetValueOrDefault() == 1)
			{
				obj = val3;
				goto IL_01ac;
			}
		}
		obj = val4;
		goto IL_01ac;
		IL_026b:
		MultiplayerCultureColorInfo val5;
		MultiplayerCultureColorInfo cultureColorInfo = (MultiplayerCultureColorInfo)val5;
		MissionScoreboardSide val6;
		WLMultiplayerScoreboardSideVM wLMultiplayerScoreboardSideVM2 = new WLMultiplayerScoreboardSideVM(val6, ExecutePopulateActionList, _openPlayerPopupAction, _openStoryCaptainPopupAction, IsSingleSide, isSecondSide: false, cultureColorInfo);
		((Collection<WLMultiplayerScoreboardSideVM>)(object)Sides).Add(wLMultiplayerScoreboardSideVM2);
		_missionSides.Add(val6.Side, wLMultiplayerScoreboardSideVM2);
		AllySide = wLMultiplayerScoreboardSideVM2;
		AllyCaptainPortraitEntry = wLMultiplayerScoreboardSideVM2.CaptainPortraitEntry ?? CreateHiddenCaptainPortraitEntry();
		AllyHasCaptainPortrait = wLMultiplayerScoreboardSideVM2.HasCaptainPortrait;
		goto IL_02e8;
		IL_0354:
		MultiplayerCultureColorInfo val7;
		cultureColorInfo = (MultiplayerCultureColorInfo)val7;
		MissionScoreboardSide val8;
		WLMultiplayerScoreboardSideVM wLMultiplayerScoreboardSideVM3 = new WLMultiplayerScoreboardSideVM(val8, ExecutePopulateActionList, _openPlayerPopupAction, _openStoryCaptainPopupAction, IsSingleSide, isSecondSide: true, cultureColorInfo);
		((Collection<WLMultiplayerScoreboardSideVM>)(object)Sides).Add(wLMultiplayerScoreboardSideVM3);
		_missionSides.Add(val8.Side, wLMultiplayerScoreboardSideVM3);
		EnemySide = wLMultiplayerScoreboardSideVM3;
		EnemyCaptainPortraitEntry = wLMultiplayerScoreboardSideVM3.CaptainPortraitEntry ?? CreateHiddenCaptainPortraitEntry();
		EnemyHasCaptainPortrait = wLMultiplayerScoreboardSideVM3.HasCaptainPortrait;
		return;
		IL_02e8:
		if (val8 == null)
		{
			return;
		}
		BasicCultureObject val9;
		BasicCultureObject val10;
		MultiplayerBattleColors val11;
		if (!(((MBObjectBase)val9).StringId != ((MBObjectBase)val10).StringId))
		{
			if (val2 != null)
			{
				Team team2 = val2.Team;
				if ((int)((team2 != null) ? new BattleSideEnum?(team2.Side) : ((BattleSideEnum?)null)).GetValueOrDefault() == 1)
				{
					val7 = val11.DefenderColors;
					goto IL_0354;
				}
			}
			val7 = val11.AttackerColors;
		}
		else
		{
			val7 = val11.DefenderColors;
		}
		goto IL_0354;
		IL_01ac:
		val6 = obj;
		MissionScoreboardSide obj2;
		if (val2 != null)
		{
			Team team3 = val2.Team;
			if ((int)((team3 != null) ? new BattleSideEnum?(team3.Side) : ((BattleSideEnum?)null)).GetValueOrDefault() == 1)
			{
				obj2 = val4;
				goto IL_01e0;
			}
		}
		obj2 = val3;
		goto IL_01e0;
		IL_01e0:
		val8 = obj2;
		val9 = ResolveScoreboardSideCulture(val6);
		val10 = ResolveScoreboardSideCulture(val8);
		val11 = MultiplayerBattleColors.CreateWith(val9, val10);
		if (val6 != null)
		{
			if (!(((MBObjectBase)val9).StringId != ((MBObjectBase)val10).StringId))
			{
				if (val2 != null)
				{
					Team team4 = val2.Team;
					if ((int)((team4 != null) ? new BattleSideEnum?(team4.Side) : ((BattleSideEnum?)null)).GetValueOrDefault() == 1)
					{
						val5 = val11.AttackerColors;
						goto IL_026b;
					}
				}
				val5 = val11.DefenderColors;
			}
			else
			{
				val5 = val11.AttackerColors;
			}
			goto IL_026b;
		}
		goto IL_02e8;
	}

	private void SyncCaptainPortraitEntries()
	{
		AllyCaptainPortraitEntry = AllySide?.CaptainPortraitEntry ?? CreateHiddenCaptainPortraitEntry();
		EnemyCaptainPortraitEntry = EnemySide?.CaptainPortraitEntry ?? CreateHiddenCaptainPortraitEntry();
		AllyHasCaptainPortrait = AllySide?.HasCaptainPortrait ?? false;
		EnemyHasCaptainPortrait = EnemySide?.HasCaptainPortrait ?? false;
	}

	private WLSubPageClanElementVM CreateHiddenCaptainPortraitEntry()
	{
		return null;
	}

	private void UpdateToggleMuteText()
	{
		ToggleMuteText = (_hasMutedAll ? ((object)_unmuteAllText).ToString() : ((object)_muteAllText).ToString());
	}

	private bool HasVoiceChatHandler()
	{
		return _voiceChatHandler != null || _proximityVoiceChatHandler != null;
	}

	private BasicCultureObject ResolveScoreboardSideCulture(MissionScoreboardSide side)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		if (side == null)
		{
			return null;
		}
		string text = (((int)side.Side == 1) ? MultiplayerOptionsExtensions.GetStrValue((OptionType)14, (MultiplayerOptionsAccessMode)1) : MultiplayerOptionsExtensions.GetStrValue((OptionType)15, (MultiplayerOptionsAccessMode)1));
		if (string.IsNullOrWhiteSpace(text))
		{
			return null;
		}
		return MBObjectManager.Instance.GetObject<BasicCultureObject>(text);
	}

	private void OnPeerMuteStatusUpdated(MissionPeer peer)
	{
		foreach (WLMultiplayerScoreboardSideVM item in (Collection<WLMultiplayerScoreboardSideVM>)(object)Sides)
		{
			foreach (WLMultiplayerScoreboardPlayerVM item2 in (Collection<WLMultiplayerScoreboardPlayerVM>)(object)item.Players)
			{
				if (((MPPlayerVM)item2).Peer == peer)
				{
					item2.UpdateIsMuted();
					break;
				}
			}
		}
	}
}
You are not using the latest version of the tool, please update.
Latest version is '10.0.1.8346' (yours is '10.0.0.8330')
