using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Steamworks;

// Token: 0x0200008E RID: 142
public class ZSteamMatchmaking
{
	// Token: 0x1700001D RID: 29
	// (get) Token: 0x06000955 RID: 2389 RVA: 0x00044C9C File Offset: 0x00042E9C
	public static ZSteamMatchmaking instance
	{
		get
		{
			return ZSteamMatchmaking.m_instance;
		}
	}

	// Token: 0x06000956 RID: 2390 RVA: 0x00044CA3 File Offset: 0x00042EA3
	public static void Initialize()
	{
		if (ZSteamMatchmaking.m_instance == null)
		{
			ZSteamMatchmaking.m_instance = new ZSteamMatchmaking();
		}
	}

	// Token: 0x06000957 RID: 2391 RVA: 0x00044CB8 File Offset: 0x00042EB8
	private ZSteamMatchmaking()
	{
		this.m_steamServerCallbackHandler = new ISteamMatchmakingServerListResponse(new ISteamMatchmakingServerListResponse.ServerResponded(this.OnServerResponded), new ISteamMatchmakingServerListResponse.ServerFailedToRespond(this.OnServerFailedToRespond), new ISteamMatchmakingServerListResponse.RefreshComplete(this.OnRefreshComplete));
		this.m_joinServerCallbackHandler = new ISteamMatchmakingPingResponse(new ISteamMatchmakingPingResponse.ServerResponded(this.OnJoinServerRespond), new ISteamMatchmakingPingResponse.ServerFailedToRespond(this.OnJoinServerFailed));
		this.m_lobbyCreated = CallResult<LobbyCreated_t>.Create(new CallResult<LobbyCreated_t>.APIDispatchDelegate(this.OnLobbyCreated));
		this.m_lobbyMatchList = CallResult<LobbyMatchList_t>.Create(new CallResult<LobbyMatchList_t>.APIDispatchDelegate(this.OnLobbyMatchList));
		this.m_changeServer = Callback<GameServerChangeRequested_t>.Create(new Callback<GameServerChangeRequested_t>.DispatchDelegate(this.OnChangeServerRequest));
		this.m_joinRequest = Callback<GameLobbyJoinRequested_t>.Create(new Callback<GameLobbyJoinRequested_t>.DispatchDelegate(this.OnJoinRequest));
		this.m_lobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(new Callback<LobbyDataUpdate_t>.DispatchDelegate(this.OnLobbyDataUpdate));
		this.m_authSessionTicketResponse = Callback<GetAuthSessionTicketResponse_t>.Create(new Callback<GetAuthSessionTicketResponse_t>.DispatchDelegate(this.OnAuthSessionTicketResponse));
	}

	// Token: 0x06000958 RID: 2392 RVA: 0x00044E28 File Offset: 0x00043028
	public byte[] RequestSessionTicket()
	{
		this.ReleaseSessionTicket();
		byte[] array = new byte[1024];
		uint num = 0U;
		this.m_authTicket = SteamUser.GetAuthSessionTicket(array, 1024, out num);
		if (this.m_authTicket == HAuthTicket.Invalid)
		{
			return null;
		}
		byte[] array2 = new byte[num];
		Buffer.BlockCopy(array, 0, array2, 0, (int)num);
		return array2;
	}

	// Token: 0x06000959 RID: 2393 RVA: 0x00044E81 File Offset: 0x00043081
	public void ReleaseSessionTicket()
	{
		if (this.m_authTicket == HAuthTicket.Invalid)
		{
			return;
		}
		SteamUser.CancelAuthTicket(this.m_authTicket);
		this.m_authTicket = HAuthTicket.Invalid;
		ZLog.Log("Released session ticket");
	}

	// Token: 0x0600095A RID: 2394 RVA: 0x00044EB6 File Offset: 0x000430B6
	public bool VerifySessionTicket(byte[] ticket, CSteamID steamID)
	{
		return SteamUser.BeginAuthSession(ticket, ticket.Length, steamID) == EBeginAuthSessionResult.k_EBeginAuthSessionResultOK;
	}

	// Token: 0x0600095B RID: 2395 RVA: 0x00044EC5 File Offset: 0x000430C5
	private void OnAuthSessionTicketResponse(GetAuthSessionTicketResponse_t data)
	{
		ZLog.Log("Session auth respons callback");
	}

	// Token: 0x0600095C RID: 2396 RVA: 0x00044ED1 File Offset: 0x000430D1
	private void OnSteamServersConnected(SteamServersConnected_t data)
	{
		ZLog.Log("Game server connected");
	}

	// Token: 0x0600095D RID: 2397 RVA: 0x00044EDD File Offset: 0x000430DD
	private void OnSteamServersDisconnected(SteamServersDisconnected_t data)
	{
		ZLog.LogWarning("Game server disconnected");
	}

	// Token: 0x0600095E RID: 2398 RVA: 0x00044EE9 File Offset: 0x000430E9
	private void OnSteamServersConnectFail(SteamServerConnectFailure_t data)
	{
		ZLog.LogWarning("Game server connected failed");
	}

	// Token: 0x0600095F RID: 2399 RVA: 0x00044EF5 File Offset: 0x000430F5
	private void OnChangeServerRequest(GameServerChangeRequested_t data)
	{
		ZLog.Log("ZSteamMatchmaking got change server request to:" + data.m_rgchServer);
		this.QueueServerJoin(data.m_rgchServer);
	}

	// Token: 0x06000960 RID: 2400 RVA: 0x00044F18 File Offset: 0x00043118
	private void OnJoinRequest(GameLobbyJoinRequested_t data)
	{
		ZLog.Log(string.Concat(new object[]
		{
			"ZSteamMatchmaking got join request friend:",
			data.m_steamIDFriend,
			"  lobby:",
			data.m_steamIDLobby
		}));
		if (Game.instance)
		{
			return;
		}
		this.QueueLobbyJoin(data.m_steamIDLobby);
	}

	// Token: 0x06000961 RID: 2401 RVA: 0x00044F7C File Offset: 0x0004317C
	private IPAddress FindIP(string host)
	{
		IPAddress result;
		try
		{
			IPAddress ipaddress;
			if (IPAddress.TryParse(host, out ipaddress))
			{
				result = ipaddress;
			}
			else
			{
				ZLog.Log("Not an ip address " + host + " doing dns lookup");
				IPHostEntry hostEntry = Dns.GetHostEntry(host);
				if (hostEntry.AddressList.Length == 0)
				{
					ZLog.Log("Dns lookup failed");
					result = null;
				}
				else
				{
					ZLog.Log("Got dns entries: " + hostEntry.AddressList.Length);
					foreach (IPAddress ipaddress2 in hostEntry.AddressList)
					{
						if (ipaddress2.AddressFamily == AddressFamily.InterNetwork)
						{
							return ipaddress2;
						}
					}
					result = null;
				}
			}
		}
		catch (Exception ex)
		{
			ZLog.Log("Exception while finding ip:" + ex.ToString());
			result = null;
		}
		return result;
	}

	// Token: 0x06000962 RID: 2402 RVA: 0x00045048 File Offset: 0x00043248
	public void QueueServerJoin(string addr)
	{
		try
		{
			string[] array = addr.Split(new char[]
			{
				':'
			});
			if (array.Length >= 2)
			{
				IPAddress ipaddress = this.FindIP(array[0]);
				if (ipaddress == null)
				{
					ZLog.Log("Invalid address " + array[0]);
				}
				else
				{
					uint nIP = (uint)IPAddress.HostToNetworkOrder(BitConverter.ToInt32(ipaddress.GetAddressBytes(), 0));
					int num = int.Parse(array[1]);
					ZLog.Log(string.Concat(new object[]
					{
						"connect to ip:",
						ipaddress.ToString(),
						" port:",
						num
					}));
					this.m_joinAddr.SetIPv4(nIP, (ushort)num);
					this.m_haveJoinAddr = true;
				}
			}
		}
		catch (Exception arg)
		{
			ZLog.Log("Server join exception:" + arg);
		}
	}

	// Token: 0x06000963 RID: 2403 RVA: 0x0004511C File Offset: 0x0004331C
	private void OnJoinServerRespond(gameserveritem_t serverData)
	{
		ZLog.Log(string.Concat(new object[]
		{
			"Got join server data ",
			serverData.GetServerName(),
			"  ",
			serverData.m_steamID
		}));
		this.m_joinAddr.SetIPv4(serverData.m_NetAdr.GetIP(), serverData.m_NetAdr.GetConnectionPort());
		this.m_haveJoinAddr = true;
	}

	// Token: 0x06000964 RID: 2404 RVA: 0x00045188 File Offset: 0x00043388
	private void OnJoinServerFailed()
	{
		ZLog.Log("Failed to get join server data");
	}

	// Token: 0x06000965 RID: 2405 RVA: 0x00045194 File Offset: 0x00043394
	public void QueueLobbyJoin(CSteamID lobbyID)
	{
		uint num;
		ushort num2;
		CSteamID csteamID;
		if (SteamMatchmaking.GetLobbyGameServer(lobbyID, out num, out num2, out csteamID))
		{
			ZLog.Log("  hostid: " + csteamID);
			this.m_joinUserID = csteamID;
			this.m_queuedJoinLobby = CSteamID.Nil;
			return;
		}
		ZLog.Log("Failed to get lobby data for lobby " + lobbyID + ", requesting lobby data");
		this.m_queuedJoinLobby = lobbyID;
		SteamMatchmaking.RequestLobbyData(lobbyID);
	}

	// Token: 0x06000966 RID: 2406 RVA: 0x00045200 File Offset: 0x00043400
	private void OnLobbyDataUpdate(LobbyDataUpdate_t data)
	{
		CSteamID csteamID = new CSteamID(data.m_ulSteamIDLobby);
		if (csteamID == this.m_queuedJoinLobby)
		{
			ZLog.Log("Got lobby data, for queued lobby");
			uint num;
			ushort num2;
			CSteamID joinUserID;
			if (SteamMatchmaking.GetLobbyGameServer(csteamID, out num, out num2, out joinUserID))
			{
				this.m_joinUserID = joinUserID;
			}
			this.m_queuedJoinLobby = CSteamID.Nil;
			return;
		}
		ZLog.Log("Got requested lobby data");
		foreach (KeyValuePair<CSteamID, string> keyValuePair in this.m_requestedFriendGames)
		{
			if (keyValuePair.Key == csteamID)
			{
				ServerData lobbyServerData = this.GetLobbyServerData(csteamID);
				if (lobbyServerData != null)
				{
					lobbyServerData.m_name = keyValuePair.Value + " [" + lobbyServerData.m_name + "]";
					this.m_friendServers.Add(lobbyServerData);
					this.m_serverListRevision++;
				}
			}
		}
	}

	// Token: 0x06000967 RID: 2407 RVA: 0x000452FC File Offset: 0x000434FC
	public void RegisterServer(string name, bool password, string version, bool publicServer, string worldName)
	{
		this.UnregisterServer();
		SteamAPICall_t hAPICall = SteamMatchmaking.CreateLobby(publicServer ? ELobbyType.k_ELobbyTypePublic : ELobbyType.k_ELobbyTypeFriendsOnly, 32);
		this.m_lobbyCreated.Set(hAPICall, null);
		this.m_registerServerName = name;
		this.m_registerPassword = password;
		this.m_registerVerson = version;
		ZLog.Log("Registering lobby");
	}

	// Token: 0x06000968 RID: 2408 RVA: 0x0004534C File Offset: 0x0004354C
	private void OnLobbyCreated(LobbyCreated_t data, bool ioError)
	{
		ZLog.Log(string.Concat(new object[]
		{
			"Lobby was created ",
			data.m_eResult,
			"  ",
			data.m_ulSteamIDLobby,
			"  error:",
			ioError.ToString()
		}));
		if (ioError)
		{
			return;
		}
		this.m_myLobby = new CSteamID(data.m_ulSteamIDLobby);
		SteamMatchmaking.SetLobbyData(this.m_myLobby, "name", this.m_registerServerName);
		SteamMatchmaking.SetLobbyData(this.m_myLobby, "password", this.m_registerPassword ? "1" : "0");
		SteamMatchmaking.SetLobbyData(this.m_myLobby, "version", this.m_registerVerson);
		SteamMatchmaking.SetLobbyGameServer(this.m_myLobby, 0U, 0, SteamUser.GetSteamID());
	}

	// Token: 0x06000969 RID: 2409 RVA: 0x00045421 File Offset: 0x00043621
	private void OnLobbyEnter(LobbyEnter_t data, bool ioError)
	{
		ZLog.LogWarning("Entering lobby " + data.m_ulSteamIDLobby);
	}

	// Token: 0x0600096A RID: 2410 RVA: 0x0004543D File Offset: 0x0004363D
	public void UnregisterServer()
	{
		if (this.m_myLobby != CSteamID.Nil)
		{
			SteamMatchmaking.SetLobbyJoinable(this.m_myLobby, false);
			SteamMatchmaking.LeaveLobby(this.m_myLobby);
			this.m_myLobby = CSteamID.Nil;
		}
	}

	// Token: 0x0600096B RID: 2411 RVA: 0x00045474 File Offset: 0x00043674
	public void RequestServerlist()
	{
		this.RequestFriendGames();
		this.RequestPublicLobbies();
		this.RequestDedicatedServers();
	}

	// Token: 0x0600096C RID: 2412 RVA: 0x00045488 File Offset: 0x00043688
	private void RequestFriendGames()
	{
		this.m_friendServers.Clear();
		this.m_requestedFriendGames.Clear();
		int num = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
		if (num == -1)
		{
			ZLog.Log("GetFriendCount returned -1, the current user is not logged in.");
			num = 0;
		}
		for (int i = 0; i < num; i++)
		{
			CSteamID friendByIndex = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
			string friendPersonaName = SteamFriends.GetFriendPersonaName(friendByIndex);
			FriendGameInfo_t friendGameInfo_t;
			if (SteamFriends.GetFriendGamePlayed(friendByIndex, out friendGameInfo_t) && friendGameInfo_t.m_gameID == (CGameID)((ulong)SteamManager.APP_ID) && friendGameInfo_t.m_steamIDLobby != CSteamID.Nil)
			{
				ZLog.Log("Friend is in our game");
				this.m_requestedFriendGames.Add(new KeyValuePair<CSteamID, string>(friendGameInfo_t.m_steamIDLobby, friendPersonaName));
				SteamMatchmaking.RequestLobbyData(friendGameInfo_t.m_steamIDLobby);
			}
		}
		this.m_serverListRevision++;
	}

	// Token: 0x0600096D RID: 2413 RVA: 0x0004554C File Offset: 0x0004374C
	private void RequestPublicLobbies()
	{
		SteamAPICall_t hAPICall = SteamMatchmaking.RequestLobbyList();
		this.m_lobbyMatchList.Set(hAPICall, null);
		this.m_refreshingPublicGames = true;
	}

	// Token: 0x0600096E RID: 2414 RVA: 0x00045574 File Offset: 0x00043774
	private void RequestDedicatedServers()
	{
		if (this.m_haveListRequest)
		{
			SteamMatchmakingServers.ReleaseRequest(this.m_serverListRequest);
			this.m_haveListRequest = false;
		}
		this.m_dedicatedServers.Clear();
		this.m_serverListRequest = SteamMatchmakingServers.RequestInternetServerList(SteamUtils.GetAppID(), new MatchMakingKeyValuePair_t[0], 0U, this.m_steamServerCallbackHandler);
		this.m_refreshingDedicatedServers = true;
		this.m_haveListRequest = true;
	}

	// Token: 0x0600096F RID: 2415 RVA: 0x000455D4 File Offset: 0x000437D4
	private void OnLobbyMatchList(LobbyMatchList_t data, bool ioError)
	{
		this.m_refreshingPublicGames = false;
		this.m_matchmakingServers.Clear();
		int num = 0;
		while ((long)num < (long)((ulong)data.m_nLobbiesMatching))
		{
			CSteamID lobbyByIndex = SteamMatchmaking.GetLobbyByIndex(num);
			ServerData lobbyServerData = this.GetLobbyServerData(lobbyByIndex);
			if (lobbyServerData != null)
			{
				this.m_matchmakingServers.Add(lobbyServerData);
			}
			num++;
		}
		this.m_serverListRevision++;
	}

	// Token: 0x06000970 RID: 2416 RVA: 0x00045634 File Offset: 0x00043834
	private ServerData GetLobbyServerData(CSteamID lobbyID)
	{
		string lobbyData = SteamMatchmaking.GetLobbyData(lobbyID, "name");
		bool password = SteamMatchmaking.GetLobbyData(lobbyID, "password") == "1";
		string lobbyData2 = SteamMatchmaking.GetLobbyData(lobbyID, "version");
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
		uint num;
		ushort num2;
		CSteamID that;
		if (SteamMatchmaking.GetLobbyGameServer(lobbyID, out num, out num2, out that))
		{
			return new ServerData
			{
				m_name = lobbyData,
				m_password = password,
				m_version = lobbyData2,
				m_players = numLobbyMembers,
				m_steamHostID = (ulong)that
			};
		}
		ZLog.Log("Failed to get lobby gameserver");
		return null;
	}

	// Token: 0x06000971 RID: 2417 RVA: 0x000456BE File Offset: 0x000438BE
	public void GetServers(List<ServerData> allServers)
	{
		if (this.m_friendsFilter)
		{
			this.FilterServers(this.m_friendServers, allServers);
			return;
		}
		this.FilterServers(this.m_matchmakingServers, allServers);
		this.FilterServers(this.m_dedicatedServers, allServers);
	}

	// Token: 0x06000972 RID: 2418 RVA: 0x000456F0 File Offset: 0x000438F0
	private void FilterServers(List<ServerData> input, List<ServerData> allServers)
	{
		string text = this.m_nameFilter.ToLowerInvariant();
		foreach (ServerData serverData in input)
		{
			if (text.Length == 0 || serverData.m_name.ToLowerInvariant().Contains(text))
			{
				allServers.Add(serverData);
			}
			if (allServers.Count >= 200)
			{
				break;
			}
		}
	}

	// Token: 0x06000973 RID: 2419 RVA: 0x00045774 File Offset: 0x00043974
	public bool GetJoinHost(out CSteamID steamID, out SteamNetworkingIPAddr addr)
	{
		steamID = this.m_joinUserID;
		addr = this.m_joinAddr;
		if (this.m_joinUserID.IsValid() || this.m_haveJoinAddr)
		{
			this.m_joinUserID = CSteamID.Nil;
			this.m_haveJoinAddr = false;
			this.m_joinAddr.Clear();
			return true;
		}
		return false;
	}

	// Token: 0x06000974 RID: 2420 RVA: 0x000457D0 File Offset: 0x000439D0
	private void OnServerResponded(HServerListRequest request, int iServer)
	{
		gameserveritem_t serverDetails = SteamMatchmakingServers.GetServerDetails(request, iServer);
		string serverName = serverDetails.GetServerName();
		ServerData serverData = new ServerData();
		serverData.m_name = serverName;
		serverData.m_steamHostAddr.SetIPv4(serverDetails.m_NetAdr.GetIP(), serverDetails.m_NetAdr.GetConnectionPort());
		serverData.m_password = serverDetails.m_bPassword;
		serverData.m_players = serverDetails.m_nPlayers;
		serverData.m_version = serverDetails.GetGameTags();
		this.m_dedicatedServers.Add(serverData);
		this.m_updateTriggerAccumulator++;
		if (this.m_updateTriggerAccumulator > 100)
		{
			this.m_updateTriggerAccumulator = 0;
			this.m_serverListRevision++;
		}
	}

	// Token: 0x06000975 RID: 2421 RVA: 0x000027E0 File Offset: 0x000009E0
	private void OnServerFailedToRespond(HServerListRequest request, int iServer)
	{
	}

	// Token: 0x06000976 RID: 2422 RVA: 0x00045878 File Offset: 0x00043A78
	private void OnRefreshComplete(HServerListRequest request, EMatchMakingServerResponse response)
	{
		ZLog.Log(string.Concat(new object[]
		{
			"Refresh complete ",
			this.m_dedicatedServers.Count,
			"  ",
			response
		}));
		this.m_refreshingDedicatedServers = false;
		this.m_serverListRevision++;
	}

	// Token: 0x06000977 RID: 2423 RVA: 0x000458D6 File Offset: 0x00043AD6
	public void SetNameFilter(string filter)
	{
		if (this.m_nameFilter == filter)
		{
			return;
		}
		this.m_nameFilter = filter;
		this.m_serverListRevision++;
	}

	// Token: 0x06000978 RID: 2424 RVA: 0x000458FC File Offset: 0x00043AFC
	public void SetFriendFilter(bool enabled)
	{
		if (this.m_friendsFilter == enabled)
		{
			return;
		}
		this.m_friendsFilter = enabled;
		this.m_serverListRevision++;
	}

	// Token: 0x06000979 RID: 2425 RVA: 0x0004591D File Offset: 0x00043B1D
	public int GetServerListRevision()
	{
		return this.m_serverListRevision;
	}

	// Token: 0x0600097A RID: 2426 RVA: 0x00045925 File Offset: 0x00043B25
	public bool IsUpdating()
	{
		return this.m_refreshingDedicatedServers || this.m_refreshingPublicGames;
	}

	// Token: 0x0600097B RID: 2427 RVA: 0x00045937 File Offset: 0x00043B37
	public int GetTotalNrOfServers()
	{
		return this.m_matchmakingServers.Count + this.m_dedicatedServers.Count + this.m_friendServers.Count;
	}

	// Token: 0x04000892 RID: 2194
	private static ZSteamMatchmaking m_instance;

	// Token: 0x04000893 RID: 2195
	private const int maxServers = 200;

	// Token: 0x04000894 RID: 2196
	private List<ServerData> m_matchmakingServers = new List<ServerData>();

	// Token: 0x04000895 RID: 2197
	private List<ServerData> m_dedicatedServers = new List<ServerData>();

	// Token: 0x04000896 RID: 2198
	private List<ServerData> m_friendServers = new List<ServerData>();

	// Token: 0x04000897 RID: 2199
	private int m_serverListRevision;

	// Token: 0x04000898 RID: 2200
	private int m_updateTriggerAccumulator;

	// Token: 0x04000899 RID: 2201
	private CallResult<LobbyCreated_t> m_lobbyCreated;

	// Token: 0x0400089A RID: 2202
	private CallResult<LobbyMatchList_t> m_lobbyMatchList;

	// Token: 0x0400089B RID: 2203
	private CallResult<LobbyEnter_t> m_lobbyEntered;

	// Token: 0x0400089C RID: 2204
	private Callback<GameServerChangeRequested_t> m_changeServer;

	// Token: 0x0400089D RID: 2205
	private Callback<GameLobbyJoinRequested_t> m_joinRequest;

	// Token: 0x0400089E RID: 2206
	private Callback<LobbyDataUpdate_t> m_lobbyDataUpdate;

	// Token: 0x0400089F RID: 2207
	private Callback<GetAuthSessionTicketResponse_t> m_authSessionTicketResponse;

	// Token: 0x040008A0 RID: 2208
	private Callback<SteamServerConnectFailure_t> m_steamServerConnectFailure;

	// Token: 0x040008A1 RID: 2209
	private Callback<SteamServersConnected_t> m_steamServersConnected;

	// Token: 0x040008A2 RID: 2210
	private Callback<SteamServersDisconnected_t> m_steamServersDisconnected;

	// Token: 0x040008A3 RID: 2211
	private CSteamID m_myLobby = CSteamID.Nil;

	// Token: 0x040008A4 RID: 2212
	private CSteamID m_joinUserID = CSteamID.Nil;

	// Token: 0x040008A5 RID: 2213
	private CSteamID m_queuedJoinLobby = CSteamID.Nil;

	// Token: 0x040008A6 RID: 2214
	private bool m_haveJoinAddr;

	// Token: 0x040008A7 RID: 2215
	private SteamNetworkingIPAddr m_joinAddr;

	// Token: 0x040008A8 RID: 2216
	private List<KeyValuePair<CSteamID, string>> m_requestedFriendGames = new List<KeyValuePair<CSteamID, string>>();

	// Token: 0x040008A9 RID: 2217
	private ISteamMatchmakingServerListResponse m_steamServerCallbackHandler;

	// Token: 0x040008AA RID: 2218
	private ISteamMatchmakingPingResponse m_joinServerCallbackHandler;

	// Token: 0x040008AB RID: 2219
	private HServerQuery m_joinQuery;

	// Token: 0x040008AC RID: 2220
	private HServerListRequest m_serverListRequest;

	// Token: 0x040008AD RID: 2221
	private bool m_haveListRequest;

	// Token: 0x040008AE RID: 2222
	private bool m_refreshingDedicatedServers;

	// Token: 0x040008AF RID: 2223
	private bool m_refreshingPublicGames;

	// Token: 0x040008B0 RID: 2224
	private string m_registerServerName = "";

	// Token: 0x040008B1 RID: 2225
	private bool m_registerPassword;

	// Token: 0x040008B2 RID: 2226
	private string m_registerVerson = "";

	// Token: 0x040008B3 RID: 2227
	private string m_nameFilter = "";

	// Token: 0x040008B4 RID: 2228
	private bool m_friendsFilter = true;

	// Token: 0x040008B5 RID: 2229
	private HAuthTicket m_authTicket = HAuthTicket.Invalid;
}
