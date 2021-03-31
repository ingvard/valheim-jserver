using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Steamworks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// Token: 0x0200007F RID: 127
public class ZNet : MonoBehaviour
{
	// Token: 0x17000019 RID: 25
	// (get) Token: 0x0600080C RID: 2060 RVA: 0x0003ECA6 File Offset: 0x0003CEA6
	public static ZNet instance
	{
		get
		{
			return ZNet.m_instance;
		}
	}

	// Token: 0x0600080D RID: 2061 RVA: 0x0003ECB0 File Offset: 0x0003CEB0
	private void Awake()
	{
		ZNet.m_instance = this;
		this.m_routedRpc = new ZRoutedRpc(ZNet.m_isServer);
		this.m_zdoMan = new ZDOMan(this.m_zdoSectorsWidth);
		this.m_passwordDialog.gameObject.SetActive(false);
		this.m_connectingDialog.gameObject.SetActive(false);
		WorldGenerator.Deitialize();
		if (SteamManager.Initialize())
		{
			string personaName = SteamFriends.GetPersonaName();
			ZLog.Log("Steam initialized, persona:" + personaName);
			ZSteamMatchmaking.Initialize();
			if (ZNet.m_isServer)
			{
				this.m_adminList = new SyncedList(Utils.GetSaveDataPath() + "/adminlist.txt", "List admin players ID  ONE per line");
				this.m_bannedList = new SyncedList(Utils.GetSaveDataPath() + "/bannedlist.txt", "List banned players ID  ONE per line");
				this.m_permittedList = new SyncedList(Utils.GetSaveDataPath() + "/permittedlist.txt", "List permitted players ID ONE per line");
				if (ZNet.m_world == null)
				{
					ZNet.m_publicServer = false;
					ZNet.m_world = World.GetDevWorld();
				}
				if (ZNet.m_openServer)
				{
					ZSteamSocket zsteamSocket = new ZSteamSocket();
					zsteamSocket.StartHost();
					this.m_hostSocket = zsteamSocket;
					bool password = ZNet.m_serverPassword != "";
					string versionString = global::Version.GetVersionString();
					ZSteamMatchmaking.instance.RegisterServer(ZNet.m_ServerName, password, versionString, ZNet.m_publicServer, ZNet.m_world.m_seedName);
				}
				WorldGenerator.Initialize(ZNet.m_world);
				this.LoadWorld();
				ZNet.m_connectionStatus = ZNet.ConnectionStatus.Connected;
			}
			this.m_routedRpc.SetUID(this.m_zdoMan.GetMyID());
			if (this.IsServer())
			{
				this.SendPlayerList();
			}
			return;
		}
	}

	// Token: 0x0600080E RID: 2062 RVA: 0x0003EE3C File Offset: 0x0003D03C
	private void Start()
	{
		if (!ZNet.m_isServer)
		{
			if (ZNet.m_serverSteamID != 0UL)
			{
				ZLog.Log("Connecting to server " + ZNet.m_serverSteamID);
				this.Connect(new CSteamID(ZNet.m_serverSteamID));
				return;
			}
			string str;
			ZNet.m_serverIPAddr.ToString(out str, true);
			ZLog.Log("Connecting to server " + str);
			this.Connect(ZNet.m_serverIPAddr);
		}
	}

	// Token: 0x0600080F RID: 2063 RVA: 0x0003EEAC File Offset: 0x0003D0AC
	private string GetPublicIP()
	{
		string result;
		try
		{
			string text = Utils.DownloadString("http://checkip.dyndns.org/", 5000);
			text = new Regex("\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}").Matches(text)[0].ToString();
			ZLog.Log("Got public ip respons:" + text);
			result = text;
		}
		catch (Exception ex)
		{
			ZLog.Log("Failed to get public ip:" + ex.ToString());
			result = "";
		}
		return result;
	}

	// Token: 0x06000810 RID: 2064 RVA: 0x0003EF28 File Offset: 0x0003D128
	public void Shutdown()
	{
		ZLog.Log("ZNet Shutdown");
		this.Save(true);
		this.StopAll();
		base.enabled = false;
	}

	// Token: 0x06000811 RID: 2065 RVA: 0x0003EF48 File Offset: 0x0003D148
	private void StopAll()
	{
		if (this.m_haveStoped)
		{
			return;
		}
		this.m_haveStoped = true;
		if (this.m_saveThread != null && this.m_saveThread.IsAlive)
		{
			this.m_saveThread.Join();
			this.m_saveThread = null;
		}
		this.m_zdoMan.ShutDown();
		this.SendDisconnect();
		ZSteamMatchmaking.instance.ReleaseSessionTicket();
		ZSteamMatchmaking.instance.UnregisterServer();
		if (this.m_hostSocket != null)
		{
			this.m_hostSocket.Dispose();
		}
		if (this.m_serverConnector != null)
		{
			this.m_serverConnector.Dispose();
		}
		foreach (ZNetPeer znetPeer in this.m_peers)
		{
			znetPeer.Dispose();
		}
		this.m_peers.Clear();
	}

	// Token: 0x06000812 RID: 2066 RVA: 0x0003F028 File Offset: 0x0003D228
	private void OnDestroy()
	{
		ZLog.Log("ZNet OnDestroy");
		if (ZNet.m_instance == this)
		{
			ZNet.m_instance = null;
		}
	}

	// Token: 0x06000813 RID: 2067 RVA: 0x0003F048 File Offset: 0x0003D248
	public void Connect(CSteamID hostID)
	{
		ZNetPeer peer = new ZNetPeer(new ZSteamSocket(hostID), true);
		this.OnNewConnection(peer);
		ZNet.m_connectionStatus = ZNet.ConnectionStatus.Connecting;
		this.m_connectingDialog.gameObject.SetActive(true);
	}

	// Token: 0x06000814 RID: 2068 RVA: 0x0003F080 File Offset: 0x0003D280
	public void Connect(SteamNetworkingIPAddr host)
	{
		ZNetPeer peer = new ZNetPeer(new ZSteamSocket(host), true);
		this.OnNewConnection(peer);
		ZNet.m_connectionStatus = ZNet.ConnectionStatus.Connecting;
		this.m_connectingDialog.gameObject.SetActive(true);
	}

	// Token: 0x06000815 RID: 2069 RVA: 0x0003F0B8 File Offset: 0x0003D2B8
	private void UpdateClientConnector(float dt)
	{
		if (this.m_serverConnector != null && this.m_serverConnector.UpdateStatus(dt, true))
		{
			ZSocket2 zsocket = this.m_serverConnector.Complete();
			if (zsocket != null)
			{
				ZLog.Log("Connection established to " + this.m_serverConnector.GetEndPointString());
				ZNetPeer peer = new ZNetPeer(zsocket, true);
				this.OnNewConnection(peer);
			}
			else
			{
				ZNet.m_connectionStatus = ZNet.ConnectionStatus.ErrorConnectFailed;
				ZLog.Log("Failed to connect to server");
			}
			this.m_serverConnector.Dispose();
			this.m_serverConnector = null;
		}
	}

	// Token: 0x06000816 RID: 2070 RVA: 0x0003F138 File Offset: 0x0003D338
	private void OnNewConnection(ZNetPeer peer)
	{
		this.m_peers.Add(peer);
		peer.m_rpc.Register<ZPackage>("PeerInfo", new Action<ZRpc, ZPackage>(this.RPC_PeerInfo));
		peer.m_rpc.Register("Disconnect", new ZRpc.RpcMethod.Method(this.RPC_Disconnect));
		if (ZNet.m_isServer)
		{
			peer.m_rpc.Register("ServerHandshake", new ZRpc.RpcMethod.Method(this.RPC_ServerHandshake));
			return;
		}
		peer.m_rpc.Register<int>("Error", new Action<ZRpc, int>(this.RPC_Error));
		peer.m_rpc.Register<bool>("ClientHandshake", new Action<ZRpc, bool>(this.RPC_ClientHandshake));
		peer.m_rpc.Invoke("ServerHandshake", Array.Empty<object>());
	}

	// Token: 0x06000817 RID: 2071 RVA: 0x0003F1FC File Offset: 0x0003D3FC
	private void RPC_ServerHandshake(ZRpc rpc)
	{
		ZNetPeer peer = this.GetPeer(rpc);
		if (peer == null)
		{
			return;
		}
		ZLog.Log("Got handshake from client " + peer.m_socket.GetEndPointString());
		this.ClearPlayerData(peer);
		bool flag = !string.IsNullOrEmpty(ZNet.m_serverPassword);
		peer.m_rpc.Invoke("ClientHandshake", new object[]
		{
			flag
		});
	}

	// Token: 0x06000818 RID: 2072 RVA: 0x0003F263 File Offset: 0x0003D463
	private void UpdatePassword()
	{
		if (this.m_passwordDialog.gameObject.activeSelf)
		{
			this.m_passwordDialog.GetComponentInChildren<InputField>().ActivateInputField();
		}
	}

	// Token: 0x06000819 RID: 2073 RVA: 0x0003F287 File Offset: 0x0003D487
	public bool InPasswordDialog()
	{
		return this.m_passwordDialog.gameObject.activeSelf;
	}

	// Token: 0x0600081A RID: 2074 RVA: 0x0003F29C File Offset: 0x0003D49C
	private void RPC_ClientHandshake(ZRpc rpc, bool needPassword)
	{
		this.m_connectingDialog.gameObject.SetActive(false);
		if (needPassword)
		{
			this.m_passwordDialog.gameObject.SetActive(true);
			InputField componentInChildren = this.m_passwordDialog.GetComponentInChildren<InputField>();
			componentInChildren.text = "";
			componentInChildren.ActivateInputField();
			this.m_passwordDialog.GetComponentInChildren<InputFieldSubmit>().m_onSubmit = new Action<string>(this.OnPasswordEnter);
			this.m_tempPasswordRPC = rpc;
			return;
		}
		this.SendPeerInfo(rpc, "");
	}

	// Token: 0x0600081B RID: 2075 RVA: 0x0003F319 File Offset: 0x0003D519
	private void OnPasswordEnter(string pwd)
	{
		if (!this.m_tempPasswordRPC.IsConnected())
		{
			return;
		}
		this.m_passwordDialog.gameObject.SetActive(false);
		this.SendPeerInfo(this.m_tempPasswordRPC, pwd);
		this.m_tempPasswordRPC = null;
	}

	// Token: 0x0600081C RID: 2076 RVA: 0x0003F350 File Offset: 0x0003D550
	private void SendPeerInfo(ZRpc rpc, string password = "")
	{
		ZPackage zpackage = new ZPackage();
		zpackage.Write(this.GetUID());
		zpackage.Write(global::Version.GetVersionString());
		zpackage.Write(this.m_referencePosition);
		zpackage.Write(Game.instance.GetPlayerProfile().GetName());
		if (this.IsServer())
		{
			zpackage.Write(ZNet.m_world.m_name);
			zpackage.Write(ZNet.m_world.m_seed);
			zpackage.Write(ZNet.m_world.m_seedName);
			zpackage.Write(ZNet.m_world.m_uid);
			zpackage.Write(ZNet.m_world.m_worldGenVersion);
			zpackage.Write(this.m_netTime);
		}
		else
		{
			string data = string.IsNullOrEmpty(password) ? "" : ZNet.HashPassword(password);
			zpackage.Write(data);
			byte[] array = ZSteamMatchmaking.instance.RequestSessionTicket();
			if (array == null)
			{
				ZNet.m_connectionStatus = ZNet.ConnectionStatus.ErrorConnectFailed;
				return;
			}
			zpackage.Write(array);
		}
		rpc.Invoke("PeerInfo", new object[]
		{
			zpackage
		});
	}

	// Token: 0x0600081D RID: 2077 RVA: 0x0003F450 File Offset: 0x0003D650
	private void RPC_PeerInfo(ZRpc rpc, ZPackage pkg)
	{
		ZNetPeer peer = this.GetPeer(rpc);
		if (peer == null)
		{
			return;
		}
		long num = pkg.ReadLong();
		string text = pkg.ReadString();
		string endPointString = peer.m_socket.GetEndPointString();
		string hostName = peer.m_socket.GetHostName();
		ZLog.Log("VERSION check their:" + text + "  mine:" + global::Version.GetVersionString());
		if (text != global::Version.GetVersionString())
		{
			if (ZNet.m_isServer)
			{
				rpc.Invoke("Error", new object[]
				{
					3
				});
			}
			else
			{
				ZNet.m_connectionStatus = ZNet.ConnectionStatus.ErrorVersion;
			}
			ZLog.Log(string.Concat(new string[]
			{
				"Peer ",
				endPointString,
				" has incompatible version, mine:",
				global::Version.GetVersionString(),
				" remote ",
				text
			}));
			return;
		}
		Vector3 refPos = pkg.ReadVector3();
		string text2 = pkg.ReadString();
		if (ZNet.m_isServer)
		{
			if (!this.IsAllowed(hostName, text2))
			{
				rpc.Invoke("Error", new object[]
				{
					8
				});
				ZLog.Log(string.Concat(new string[]
				{
					"Player ",
					text2,
					" : ",
					hostName,
					" is blacklisted or not in whitelist."
				}));
				return;
			}
			string b = pkg.ReadString();
			ZSteamSocket zsteamSocket = peer.m_socket as ZSteamSocket;
			byte[] ticket = pkg.ReadByteArray();
			if (!ZSteamMatchmaking.instance.VerifySessionTicket(ticket, zsteamSocket.GetPeerID()))
			{
				ZLog.Log("Peer " + endPointString + " has invalid session ticket");
				rpc.Invoke("Error", new object[]
				{
					8
				});
				return;
			}
			if (this.GetNrOfPlayers() >= this.m_serverPlayerLimit)
			{
				rpc.Invoke("Error", new object[]
				{
					9
				});
				ZLog.Log("Peer " + endPointString + " disconnected due to server is full");
				return;
			}
			if (ZNet.m_serverPassword != b)
			{
				rpc.Invoke("Error", new object[]
				{
					6
				});
				ZLog.Log("Peer " + endPointString + " has wrong password");
				return;
			}
			if (this.IsConnected(num))
			{
				rpc.Invoke("Error", new object[]
				{
					7
				});
				ZLog.Log(string.Concat(new object[]
				{
					"Already connected to peer with UID:",
					num,
					"  ",
					endPointString
				}));
				return;
			}
		}
		else
		{
			ZNet.m_world = new World();
			ZNet.m_world.m_name = pkg.ReadString();
			ZNet.m_world.m_seed = pkg.ReadInt();
			ZNet.m_world.m_seedName = pkg.ReadString();
			ZNet.m_world.m_uid = pkg.ReadLong();
			ZNet.m_world.m_worldGenVersion = pkg.ReadInt();
			WorldGenerator.Initialize(ZNet.m_world);
			this.m_netTime = pkg.ReadDouble();
		}
		peer.m_refPos = refPos;
		peer.m_uid = num;
		peer.m_playerName = text2;
		rpc.Register<Vector3, bool>("RefPos", new Action<ZRpc, Vector3, bool>(this.RPC_RefPos));
		rpc.Register<ZPackage>("PlayerList", new Action<ZRpc, ZPackage>(this.RPC_PlayerList));
		rpc.Register<string>("RemotePrint", new Action<ZRpc, string>(this.RPC_RemotePrint));
		if (ZNet.m_isServer)
		{
			rpc.Register<ZDOID>("CharacterID", new Action<ZRpc, ZDOID>(this.RPC_CharacterID));
			rpc.Register<string>("Kick", new Action<ZRpc, string>(this.RPC_Kick));
			rpc.Register<string>("Ban", new Action<ZRpc, string>(this.RPC_Ban));
			rpc.Register<string>("Unban", new Action<ZRpc, string>(this.RPC_Unban));
			rpc.Register("Save", new ZRpc.RpcMethod.Method(this.RPC_Save));
			rpc.Register("PrintBanned", new ZRpc.RpcMethod.Method(this.RPC_PrintBanned));
		}
		else
		{
			rpc.Register<double>("NetTime", new Action<ZRpc, double>(this.RPC_NetTime));
		}
		if (ZNet.m_isServer)
		{
			this.SendPeerInfo(rpc, "");
			this.SendPlayerList();
		}
		else
		{
			ZNet.m_connectionStatus = ZNet.ConnectionStatus.Connected;
		}
		this.m_zdoMan.AddPeer(peer);
		this.m_routedRpc.AddPeer(peer);
	}

	// Token: 0x0600081E RID: 2078 RVA: 0x0003F874 File Offset: 0x0003DA74
	private void SendDisconnect()
	{
		ZLog.Log("Sending disconnect msg");
		foreach (ZNetPeer peer in this.m_peers)
		{
			this.SendDisconnect(peer);
		}
	}

	// Token: 0x0600081F RID: 2079 RVA: 0x0003F8D4 File Offset: 0x0003DAD4
	private void SendDisconnect(ZNetPeer peer)
	{
		if (peer.m_rpc != null)
		{
			ZLog.Log("Sent to " + peer.m_socket.GetEndPointString());
			peer.m_rpc.Invoke("Disconnect", Array.Empty<object>());
		}
	}

	// Token: 0x06000820 RID: 2080 RVA: 0x0003F910 File Offset: 0x0003DB10
	private void RPC_Disconnect(ZRpc rpc)
	{
		ZLog.Log("RPC_Disconnect ");
		ZNetPeer peer = this.GetPeer(rpc);
		if (peer != null)
		{
			if (peer.m_server)
			{
				ZNet.m_connectionStatus = ZNet.ConnectionStatus.ErrorDisconnected;
			}
			this.Disconnect(peer);
		}
	}

	// Token: 0x06000821 RID: 2081 RVA: 0x0003F948 File Offset: 0x0003DB48
	private void RPC_Error(ZRpc rpc, int error)
	{
		ZNet.m_connectionStatus = (ZNet.ConnectionStatus)error;
		ZLog.Log("Got connectoin error msg " + (ZNet.ConnectionStatus)error);
	}

	// Token: 0x06000822 RID: 2082 RVA: 0x0003F974 File Offset: 0x0003DB74
	public bool IsConnected(long uid)
	{
		if (uid == this.GetUID())
		{
			return true;
		}
		using (List<ZNetPeer>.Enumerator enumerator = this.m_peers.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_uid == uid)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000823 RID: 2083 RVA: 0x0003F9DC File Offset: 0x0003DBDC
	private void ClearPlayerData(ZNetPeer peer)
	{
		this.m_routedRpc.RemovePeer(peer);
		this.m_zdoMan.RemovePeer(peer);
	}

	// Token: 0x06000824 RID: 2084 RVA: 0x0003F9F6 File Offset: 0x0003DBF6
	public void Disconnect(ZNetPeer peer)
	{
		this.ClearPlayerData(peer);
		this.m_peers.Remove(peer);
		peer.Dispose();
		if (ZNet.m_isServer)
		{
			this.SendPlayerList();
		}
	}

	// Token: 0x06000825 RID: 2085 RVA: 0x0003FA1F File Offset: 0x0003DC1F
	private void FixedUpdate()
	{
		this.UpdateNetTime(Time.fixedDeltaTime);
	}

	// Token: 0x06000826 RID: 2086 RVA: 0x0003FA2C File Offset: 0x0003DC2C
	private void Update()
	{
		float deltaTime = Time.deltaTime;
		ZSteamSocket.UpdateAllSockets(deltaTime);
		if (this.IsServer())
		{
			this.UpdateBanList(deltaTime);
		}
		this.CheckForIncommingServerConnections();
		this.UpdatePeers(deltaTime);
		this.SendPeriodicData(deltaTime);
		this.m_zdoMan.Update(deltaTime);
		this.UpdateSave();
		this.UpdatePassword();
	}

	// Token: 0x06000827 RID: 2087 RVA: 0x0003FA80 File Offset: 0x0003DC80
	private void UpdateNetTime(float dt)
	{
		if (this.IsServer())
		{
			if (this.GetNrOfPlayers() > 0)
			{
				this.m_netTime += (double)dt;
				return;
			}
		}
		else
		{
			this.m_netTime += (double)dt;
		}
	}

	// Token: 0x06000828 RID: 2088 RVA: 0x0003FAB4 File Offset: 0x0003DCB4
	private void UpdateBanList(float dt)
	{
		this.m_banlistTimer += dt;
		if (this.m_banlistTimer > 5f)
		{
			this.m_banlistTimer = 0f;
			this.CheckWhiteList();
			foreach (string user in this.m_bannedList.GetList())
			{
				this.InternalKick(user);
			}
		}
	}

	// Token: 0x06000829 RID: 2089 RVA: 0x0003FB38 File Offset: 0x0003DD38
	private void CheckWhiteList()
	{
		if (this.m_permittedList.Count() == 0)
		{
			return;
		}
		bool flag = false;
		while (!flag)
		{
			flag = true;
			foreach (ZNetPeer znetPeer in this.m_peers)
			{
				if (znetPeer.IsReady())
				{
					string hostName = znetPeer.m_socket.GetHostName();
					if (!this.m_permittedList.Contains(hostName))
					{
						ZLog.Log("Kicking player not in permitted list " + znetPeer.m_playerName + " host: " + hostName);
						this.InternalKick(znetPeer);
						flag = false;
						break;
					}
				}
			}
		}
	}

	// Token: 0x0600082A RID: 2090 RVA: 0x0003FBE4 File Offset: 0x0003DDE4
	public bool IsSaving()
	{
		return this.m_saveThread != null;
	}

	// Token: 0x0600082B RID: 2091 RVA: 0x0003FBF0 File Offset: 0x0003DDF0
	public void ConsoleSave()
	{
		if (this.IsServer())
		{
			this.RPC_Save(null);
			return;
		}
		ZRpc serverRPC = this.GetServerRPC();
		if (serverRPC != null)
		{
			serverRPC.Invoke("Save", Array.Empty<object>());
		}
	}

	// Token: 0x0600082C RID: 2092 RVA: 0x0003FC27 File Offset: 0x0003DE27
	private void RPC_Save(ZRpc rpc)
	{
		if (rpc != null && !this.m_adminList.Contains(rpc.GetSocket().GetHostName()))
		{
			this.RemotePrint(rpc, "You are not admin");
			return;
		}
		this.RemotePrint(rpc, "Saving..");
		this.Save(false);
	}

	// Token: 0x0600082D RID: 2093 RVA: 0x0003FC64 File Offset: 0x0003DE64
	public void Save(bool sync)
	{
		if (this.m_loadError || ZoneSystem.instance.SkipSaving() || DungeonDB.instance.SkipSaving())
		{
			ZLog.LogWarning("Skipping world save");
			return;
		}
		if (ZNet.m_isServer && ZNet.m_world != null)
		{
			this.SaveWorld(sync);
		}
	}

	// Token: 0x0600082E RID: 2094 RVA: 0x0003FCB4 File Offset: 0x0003DEB4
	private void SendPeriodicData(float dt)
	{
		this.m_periodicSendTimer += dt;
		if (this.m_periodicSendTimer >= 2f)
		{
			this.m_periodicSendTimer = 0f;
			if (this.IsServer())
			{
				this.SendNetTime();
				this.SendPlayerList();
				return;
			}
			foreach (ZNetPeer znetPeer in this.m_peers)
			{
				if (znetPeer.IsReady())
				{
					znetPeer.m_rpc.Invoke("RefPos", new object[]
					{
						this.m_referencePosition,
						this.m_publicReferencePosition
					});
				}
			}
		}
	}

	// Token: 0x0600082F RID: 2095 RVA: 0x0003FD78 File Offset: 0x0003DF78
	private void SendNetTime()
	{
		foreach (ZNetPeer znetPeer in this.m_peers)
		{
			if (znetPeer.IsReady())
			{
				znetPeer.m_rpc.Invoke("NetTime", new object[]
				{
					this.m_netTime
				});
			}
		}
	}

	// Token: 0x06000830 RID: 2096 RVA: 0x0003FDF0 File Offset: 0x0003DFF0
	private void RPC_NetTime(ZRpc rpc, double time)
	{
		this.m_netTime = time;
	}

	// Token: 0x06000831 RID: 2097 RVA: 0x0003FDFC File Offset: 0x0003DFFC
	private void RPC_RefPos(ZRpc rpc, Vector3 pos, bool publicRefPos)
	{
		ZNetPeer peer = this.GetPeer(rpc);
		if (peer != null)
		{
			peer.m_refPos = pos;
			peer.m_publicRefPos = publicRefPos;
		}
	}

	// Token: 0x06000832 RID: 2098 RVA: 0x0003FE24 File Offset: 0x0003E024
	private void UpdatePeers(float dt)
	{
		foreach (ZNetPeer znetPeer in this.m_peers)
		{
			if (!znetPeer.m_rpc.IsConnected())
			{
				if (znetPeer.m_server)
				{
					if (ZNet.m_connectionStatus == ZNet.ConnectionStatus.Connecting)
					{
						ZNet.m_connectionStatus = ZNet.ConnectionStatus.ErrorConnectFailed;
					}
					else
					{
						ZNet.m_connectionStatus = ZNet.ConnectionStatus.ErrorDisconnected;
					}
				}
				this.Disconnect(znetPeer);
				break;
			}
		}
		ZNetPeer[] array = this.m_peers.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].m_rpc.Update(dt);
		}
	}

	// Token: 0x06000833 RID: 2099 RVA: 0x0003FED0 File Offset: 0x0003E0D0
	private void CheckForIncommingServerConnections()
	{
		if (this.m_hostSocket == null)
		{
			return;
		}
		ISocket socket = this.m_hostSocket.Accept();
		if (socket != null)
		{
			if (!socket.IsConnected())
			{
				socket.Dispose();
				return;
			}
			ZNetPeer peer = new ZNetPeer(socket, false);
			this.OnNewConnection(peer);
		}
	}

	// Token: 0x06000834 RID: 2100 RVA: 0x0003FF14 File Offset: 0x0003E114
	public ZNetPeer GetPeerByPlayerName(string name)
	{
		foreach (ZNetPeer znetPeer in this.m_peers)
		{
			if (znetPeer.IsReady() && znetPeer.m_playerName == name)
			{
				return znetPeer;
			}
		}
		return null;
	}

	// Token: 0x06000835 RID: 2101 RVA: 0x0003FF80 File Offset: 0x0003E180
	public ZNetPeer GetPeerByHostName(string endpoint)
	{
		foreach (ZNetPeer znetPeer in this.m_peers)
		{
			if (znetPeer.IsReady() && znetPeer.m_socket.GetHostName() == endpoint)
			{
				return znetPeer;
			}
		}
		return null;
	}

	// Token: 0x06000836 RID: 2102 RVA: 0x0003FFF0 File Offset: 0x0003E1F0
	public ZNetPeer GetPeer(long uid)
	{
		foreach (ZNetPeer znetPeer in this.m_peers)
		{
			if (znetPeer.m_uid == uid)
			{
				return znetPeer;
			}
		}
		return null;
	}

	// Token: 0x06000837 RID: 2103 RVA: 0x0004004C File Offset: 0x0003E24C
	private ZNetPeer GetPeer(ZRpc rpc)
	{
		foreach (ZNetPeer znetPeer in this.m_peers)
		{
			if (znetPeer.m_rpc == rpc)
			{
				return znetPeer;
			}
		}
		return null;
	}

	// Token: 0x06000838 RID: 2104 RVA: 0x000400A8 File Offset: 0x0003E2A8
	public List<ZNetPeer> GetConnectedPeers()
	{
		return new List<ZNetPeer>(this.m_peers);
	}

	// Token: 0x06000839 RID: 2105 RVA: 0x000400B8 File Offset: 0x0003E2B8
	private void SaveWorld(bool sync)
	{
		if (this.m_saveThread != null && this.m_saveThread.IsAlive)
		{
			this.m_saveThread.Join();
			this.m_saveThread = null;
		}
		this.m_saveStartTime = Time.realtimeSinceStartup;
		this.m_zdoMan.PrepareSave();
		ZoneSystem.instance.PrepareSave();
		RandEventSystem.instance.PrepareSave();
		this.m_saveThreadStartTime = Time.realtimeSinceStartup;
		this.m_saveThread = new Thread(new ThreadStart(this.SaveWorldThread));
		this.m_saveThread.Start();
		if (sync)
		{
			this.m_saveThread.Join();
			this.m_saveThread = null;
		}
	}

	// Token: 0x0600083A RID: 2106 RVA: 0x00040158 File Offset: 0x0003E358
	private void UpdateSave()
	{
		if (this.m_saveThread != null && !this.m_saveThread.IsAlive)
		{
			this.m_saveThread = null;
			float num = this.m_saveThreadStartTime - this.m_saveStartTime;
			float num2 = Time.realtimeSinceStartup - this.m_saveThreadStartTime;
			MessageHud.instance.ShowMessage(MessageHud.MessageType.TopLeft, string.Concat(new string[]
			{
				"$msg_worldsaved ( ",
				num.ToString("0.00"),
				"+",
				num2.ToString("0.00"),
				"s )"
			}), 0, null);
		}
	}

	// Token: 0x0600083B RID: 2107 RVA: 0x000401EC File Offset: 0x0003E3EC
	private void SaveWorldThread()
	{
		DateTime now = DateTime.Now;
		string dbpath = ZNet.m_world.GetDBPath();
		string text = dbpath + ".new";
		string text2 = dbpath + ".old";
		FileStream fileStream = File.Create(text);
		BinaryWriter binaryWriter = new BinaryWriter(fileStream);
		binaryWriter.Write(global::Version.m_worldVersion);
		binaryWriter.Write(this.m_netTime);
		this.m_zdoMan.SaveAsync(binaryWriter);
		ZoneSystem.instance.SaveASync(binaryWriter);
		RandEventSystem.instance.SaveAsync(binaryWriter);
		binaryWriter.Flush();
		fileStream.Flush(true);
		fileStream.Close();
		fileStream.Dispose();
		ZNet.m_world.SaveWorldMetaData();
		if (File.Exists(dbpath))
		{
			if (File.Exists(text2))
			{
				File.Delete(text2);
			}
			File.Move(dbpath, text2);
		}
		File.Move(text, dbpath);
		ZLog.Log("World saved ( " + (DateTime.Now - now).TotalMilliseconds.ToString() + "ms )");
	}

	// Token: 0x0600083C RID: 2108 RVA: 0x000402E0 File Offset: 0x0003E4E0
	private void LoadWorld()
	{
		ZLog.Log("Load world " + ZNet.m_world.m_name);
		string dbpath = ZNet.m_world.GetDBPath();
		FileStream fileStream;
		try
		{
			fileStream = File.OpenRead(dbpath);
		}
		catch
		{
			ZLog.Log("  missing world.dat");
			return;
		}
		BinaryReader binaryReader = new BinaryReader(fileStream);
		try
		{
			int num;
			if (!this.CheckDataVersion(binaryReader, out num))
			{
				ZLog.Log("  incompatible data version " + num);
				binaryReader.Close();
				fileStream.Dispose();
				return;
			}
			if (num >= 4)
			{
				this.m_netTime = binaryReader.ReadDouble();
			}
			this.m_zdoMan.Load(binaryReader, num);
			if (num >= 12)
			{
				ZoneSystem.instance.Load(binaryReader, num);
			}
			if (num >= 15)
			{
				RandEventSystem.instance.Load(binaryReader, num);
			}
			binaryReader.Close();
			fileStream.Dispose();
		}
		catch (Exception ex)
		{
			ZLog.LogError("Exception while loading world " + dbpath + ":" + ex.ToString());
			this.m_loadError = true;
			Application.Quit();
		}
		GC.Collect();
	}

	// Token: 0x0600083D RID: 2109 RVA: 0x000403FC File Offset: 0x0003E5FC
	private bool CheckDataVersion(BinaryReader reader, out int version)
	{
		version = reader.ReadInt32();
		return global::Version.IsWorldVersionCompatible(version);
	}

	// Token: 0x0600083E RID: 2110 RVA: 0x00040412 File Offset: 0x0003E612
	public int GetHostPort()
	{
		if (this.m_hostSocket != null)
		{
			return this.m_hostSocket.GetHostPort();
		}
		return 0;
	}

	// Token: 0x0600083F RID: 2111 RVA: 0x00040429 File Offset: 0x0003E629
	public long GetUID()
	{
		return this.m_zdoMan.GetMyID();
	}

	// Token: 0x06000840 RID: 2112 RVA: 0x00040436 File Offset: 0x0003E636
	public long GetWorldUID()
	{
		return ZNet.m_world.m_uid;
	}

	// Token: 0x06000841 RID: 2113 RVA: 0x00040442 File Offset: 0x0003E642
	public string GetWorldName()
	{
		if (ZNet.m_world != null)
		{
			return ZNet.m_world.m_name;
		}
		return null;
	}

	// Token: 0x06000842 RID: 2114 RVA: 0x00040457 File Offset: 0x0003E657
	public void SetCharacterID(ZDOID id)
	{
		this.m_characterID = id;
		if (!ZNet.m_isServer)
		{
			this.m_peers[0].m_rpc.Invoke("CharacterID", new object[]
			{
				id
			});
		}
	}

	// Token: 0x06000843 RID: 2115 RVA: 0x00040494 File Offset: 0x0003E694
	private void RPC_CharacterID(ZRpc rpc, ZDOID characterID)
	{
		ZNetPeer peer = this.GetPeer(rpc);
		if (peer != null)
		{
			peer.m_characterID = characterID;
			ZLog.Log(string.Concat(new object[]
			{
				"Got character ZDOID from ",
				peer.m_playerName,
				" : ",
				characterID
			}));
		}
	}

	// Token: 0x06000844 RID: 2116 RVA: 0x000404E5 File Offset: 0x0003E6E5
	public void SetPublicReferencePosition(bool pub)
	{
		this.m_publicReferencePosition = pub;
	}

	// Token: 0x06000845 RID: 2117 RVA: 0x000404EE File Offset: 0x0003E6EE
	public bool IsReferencePositionPublic()
	{
		return this.m_publicReferencePosition;
	}

	// Token: 0x06000846 RID: 2118 RVA: 0x000404F6 File Offset: 0x0003E6F6
	public void SetReferencePosition(Vector3 pos)
	{
		this.m_referencePosition = pos;
	}

	// Token: 0x06000847 RID: 2119 RVA: 0x000404FF File Offset: 0x0003E6FF
	public Vector3 GetReferencePosition()
	{
		return this.m_referencePosition;
	}

	// Token: 0x06000848 RID: 2120 RVA: 0x00040508 File Offset: 0x0003E708
	public List<ZDO> GetAllCharacterZDOS()
	{
		List<ZDO> list = new List<ZDO>();
		ZDO zdo = this.m_zdoMan.GetZDO(this.m_characterID);
		if (zdo != null)
		{
			list.Add(zdo);
		}
		foreach (ZNetPeer znetPeer in this.m_peers)
		{
			if (znetPeer.IsReady() && !znetPeer.m_characterID.IsNone())
			{
				ZDO zdo2 = this.m_zdoMan.GetZDO(znetPeer.m_characterID);
				if (zdo2 != null)
				{
					list.Add(zdo2);
				}
			}
		}
		return list;
	}

	// Token: 0x06000849 RID: 2121 RVA: 0x000405AC File Offset: 0x0003E7AC
	public int GetPeerConnections()
	{
		int num = 0;
		for (int i = 0; i < this.m_peers.Count; i++)
		{
			if (this.m_peers[i].IsReady())
			{
				num++;
			}
		}
		return num;
	}

	// Token: 0x0600084A RID: 2122 RVA: 0x000405E9 File Offset: 0x0003E7E9
	public ZNat GetZNat()
	{
		return this.m_nat;
	}

	// Token: 0x0600084B RID: 2123 RVA: 0x000405F1 File Offset: 0x0003E7F1
	public static void SetServer(bool server, bool openServer, bool publicServer, string serverName, string password, World world)
	{
		ZNet.m_isServer = server;
		ZNet.m_openServer = openServer;
		ZNet.m_publicServer = publicServer;
		ZNet.m_serverPassword = (string.IsNullOrEmpty(password) ? "" : ZNet.HashPassword(password));
		ZNet.m_ServerName = serverName;
		ZNet.m_world = world;
	}

	// Token: 0x0600084C RID: 2124 RVA: 0x00040630 File Offset: 0x0003E830
	private static string HashPassword(string password)
	{
		byte[] bytes = Encoding.ASCII.GetBytes(password);
		byte[] bytes2 = new MD5CryptoServiceProvider().ComputeHash(bytes);
		return Encoding.ASCII.GetString(bytes2);
	}

	// Token: 0x0600084D RID: 2125 RVA: 0x00040660 File Offset: 0x0003E860
	public static void ResetServerHost()
	{
		ZNet.m_serverSteamID = 0UL;
		ZNet.m_serverIPAddr.Clear();
	}

	// Token: 0x0600084E RID: 2126 RVA: 0x00040673 File Offset: 0x0003E873
	public static void SetServerHost(ulong serverID)
	{
		ZNet.m_serverSteamID = serverID;
		ZNet.m_serverIPAddr.Clear();
	}

	// Token: 0x0600084F RID: 2127 RVA: 0x00040685 File Offset: 0x0003E885
	public static void SetServerHost(SteamNetworkingIPAddr serverAddr)
	{
		ZNet.m_serverSteamID = 0UL;
		ZNet.m_serverIPAddr = serverAddr;
	}

	// Token: 0x06000850 RID: 2128 RVA: 0x00040694 File Offset: 0x0003E894
	public static string GetServerString()
	{
		return ZNet.m_serverSteamID + "/" + ZNet.m_serverIPAddr.ToString();
	}

	// Token: 0x06000851 RID: 2129 RVA: 0x000406BA File Offset: 0x0003E8BA
	public bool IsServer()
	{
		return ZNet.m_isServer;
	}

	// Token: 0x06000852 RID: 2130 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool IsDedicated()
	{
		return false;
	}

	// Token: 0x06000853 RID: 2131 RVA: 0x000406C4 File Offset: 0x0003E8C4
	private void UpdatePlayerList()
	{
		this.m_players.Clear();
		if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null)
		{
			ZNet.PlayerInfo playerInfo = new ZNet.PlayerInfo
			{
				m_name = Game.instance.GetPlayerProfile().GetName(),
				m_host = "",
				m_characterID = this.m_characterID,
				m_publicPosition = this.m_publicReferencePosition
			};
			if (playerInfo.m_publicPosition)
			{
				playerInfo.m_position = this.m_referencePosition;
			}
			this.m_players.Add(playerInfo);
		}
		foreach (ZNetPeer znetPeer in this.m_peers)
		{
			if (znetPeer.IsReady())
			{
				ZNet.PlayerInfo playerInfo2 = new ZNet.PlayerInfo
				{
					m_characterID = znetPeer.m_characterID,
					m_name = znetPeer.m_playerName,
					m_host = znetPeer.m_socket.GetHostName(),
					m_publicPosition = znetPeer.m_publicRefPos
				};
				if (playerInfo2.m_publicPosition)
				{
					playerInfo2.m_position = znetPeer.m_refPos;
				}
				this.m_players.Add(playerInfo2);
			}
		}
	}

	// Token: 0x06000854 RID: 2132 RVA: 0x000407F4 File Offset: 0x0003E9F4
	private void SendPlayerList()
	{
		this.UpdatePlayerList();
		if (this.m_peers.Count > 0)
		{
			ZPackage zpackage = new ZPackage();
			zpackage.Write(this.m_players.Count);
			foreach (ZNet.PlayerInfo playerInfo in this.m_players)
			{
				zpackage.Write(playerInfo.m_name);
				zpackage.Write(playerInfo.m_host);
				zpackage.Write(playerInfo.m_characterID);
				zpackage.Write(playerInfo.m_publicPosition);
				if (playerInfo.m_publicPosition)
				{
					zpackage.Write(playerInfo.m_position);
				}
			}
			foreach (ZNetPeer znetPeer in this.m_peers)
			{
				if (znetPeer.IsReady())
				{
					znetPeer.m_rpc.Invoke("PlayerList", new object[]
					{
						zpackage
					});
				}
			}
		}
	}

	// Token: 0x06000855 RID: 2133 RVA: 0x00040914 File Offset: 0x0003EB14
	private void RPC_PlayerList(ZRpc rpc, ZPackage pkg)
	{
		this.m_players.Clear();
		int num = pkg.ReadInt();
		for (int i = 0; i < num; i++)
		{
			ZNet.PlayerInfo playerInfo = new ZNet.PlayerInfo
			{
				m_name = pkg.ReadString(),
				m_host = pkg.ReadString(),
				m_characterID = pkg.ReadZDOID(),
				m_publicPosition = pkg.ReadBool()
			};
			if (playerInfo.m_publicPosition)
			{
				playerInfo.m_position = pkg.ReadVector3();
			}
			this.m_players.Add(playerInfo);
		}
	}

	// Token: 0x06000856 RID: 2134 RVA: 0x0004099C File Offset: 0x0003EB9C
	public List<ZNet.PlayerInfo> GetPlayerList()
	{
		return this.m_players;
	}

	// Token: 0x06000857 RID: 2135 RVA: 0x000409A4 File Offset: 0x0003EBA4
	public void GetOtherPublicPlayers(List<ZNet.PlayerInfo> playerList)
	{
		foreach (ZNet.PlayerInfo playerInfo in this.m_players)
		{
			if (playerInfo.m_publicPosition)
			{
				ZDOID characterID = playerInfo.m_characterID;
				if (!characterID.IsNone() && !(playerInfo.m_characterID == this.m_characterID))
				{
					playerList.Add(playerInfo);
				}
			}
		}
	}

	// Token: 0x06000858 RID: 2136 RVA: 0x00040A24 File Offset: 0x0003EC24
	public int GetNrOfPlayers()
	{
		return this.m_players.Count;
	}

	// Token: 0x06000859 RID: 2137 RVA: 0x00040A34 File Offset: 0x0003EC34
	public void GetNetStats(out float localQuality, out float remoteQuality, out int ping, out float outByteSec, out float inByteSec)
	{
		localQuality = 0f;
		remoteQuality = 0f;
		ping = 0;
		outByteSec = 0f;
		inByteSec = 0f;
		if (this.IsServer())
		{
			int num = 0;
			foreach (ZNetPeer znetPeer in this.m_peers)
			{
				if (znetPeer.IsReady())
				{
					num++;
					float num2;
					float num3;
					int num4;
					float num5;
					float num6;
					znetPeer.m_socket.GetConnectionQuality(out num2, out num3, out num4, out num5, out num6);
					localQuality += num2;
					remoteQuality += num3;
					ping += num4;
					outByteSec += num5;
					inByteSec += num6;
				}
			}
			if (num > 0)
			{
				localQuality /= (float)num;
				remoteQuality /= (float)num;
				ping /= num;
			}
			return;
		}
		if (ZNet.m_connectionStatus == ZNet.ConnectionStatus.Connected)
		{
			foreach (ZNetPeer znetPeer2 in this.m_peers)
			{
				if (znetPeer2.IsReady())
				{
					znetPeer2.m_socket.GetConnectionQuality(out localQuality, out remoteQuality, out ping, out outByteSec, out inByteSec);
					break;
				}
			}
		}
	}

	// Token: 0x0600085A RID: 2138 RVA: 0x00040B70 File Offset: 0x0003ED70
	public void SetNetTime(double time)
	{
		this.m_netTime = time;
	}

	// Token: 0x0600085B RID: 2139 RVA: 0x00040B7C File Offset: 0x0003ED7C
	public DateTime GetTime()
	{
		long ticks = (long)(this.m_netTime * 1000.0 * 10000.0);
		return new DateTime(ticks);
	}

	// Token: 0x0600085C RID: 2140 RVA: 0x00040BAB File Offset: 0x0003EDAB
	public float GetWrappedDayTimeSeconds()
	{
		return (float)(this.m_netTime % 86400.0);
	}

	// Token: 0x0600085D RID: 2141 RVA: 0x00040BBE File Offset: 0x0003EDBE
	public double GetTimeSeconds()
	{
		return this.m_netTime;
	}

	// Token: 0x0600085E RID: 2142 RVA: 0x00040BC6 File Offset: 0x0003EDC6
	public static ZNet.ConnectionStatus GetConnectionStatus()
	{
		if (ZNet.m_instance != null && ZNet.m_instance.IsServer())
		{
			return ZNet.ConnectionStatus.Connected;
		}
		return ZNet.m_connectionStatus;
	}

	// Token: 0x0600085F RID: 2143 RVA: 0x00040BE8 File Offset: 0x0003EDE8
	public bool HasBadConnection()
	{
		return this.GetServerPing() > this.m_badConnectionPing;
	}

	// Token: 0x06000860 RID: 2144 RVA: 0x00040BF8 File Offset: 0x0003EDF8
	public float GetServerPing()
	{
		if (this.IsServer())
		{
			return 0f;
		}
		if (ZNet.m_connectionStatus == ZNet.ConnectionStatus.Connecting || ZNet.m_connectionStatus == ZNet.ConnectionStatus.None)
		{
			return 0f;
		}
		if (ZNet.m_connectionStatus == ZNet.ConnectionStatus.Connected)
		{
			foreach (ZNetPeer znetPeer in this.m_peers)
			{
				if (znetPeer.IsReady())
				{
					return znetPeer.m_rpc.GetTimeSinceLastPing();
				}
			}
		}
		return 0f;
	}

	// Token: 0x06000861 RID: 2145 RVA: 0x00040C8C File Offset: 0x0003EE8C
	public ZNetPeer GetServerPeer()
	{
		if (this.IsServer())
		{
			return null;
		}
		if (ZNet.m_connectionStatus == ZNet.ConnectionStatus.Connecting || ZNet.m_connectionStatus == ZNet.ConnectionStatus.None)
		{
			return null;
		}
		if (ZNet.m_connectionStatus == ZNet.ConnectionStatus.Connected)
		{
			foreach (ZNetPeer znetPeer in this.m_peers)
			{
				if (znetPeer.IsReady())
				{
					return znetPeer;
				}
			}
		}
		return null;
	}

	// Token: 0x06000862 RID: 2146 RVA: 0x00040D0C File Offset: 0x0003EF0C
	public ZRpc GetServerRPC()
	{
		ZNetPeer serverPeer = this.GetServerPeer();
		if (serverPeer != null)
		{
			return serverPeer.m_rpc;
		}
		return null;
	}

	// Token: 0x06000863 RID: 2147 RVA: 0x00040D2B File Offset: 0x0003EF2B
	public List<ZNetPeer> GetPeers()
	{
		return this.m_peers;
	}

	// Token: 0x06000864 RID: 2148 RVA: 0x00040D33 File Offset: 0x0003EF33
	public void RemotePrint(ZRpc rpc, string text)
	{
		if (rpc == null)
		{
			if (global::Console.instance)
			{
				global::Console.instance.Print(text);
				return;
			}
		}
		else
		{
			rpc.Invoke("RemotePrint", new object[]
			{
				text
			});
		}
	}

	// Token: 0x06000865 RID: 2149 RVA: 0x00040D65 File Offset: 0x0003EF65
	private void RPC_RemotePrint(ZRpc rpc, string text)
	{
		if (global::Console.instance)
		{
			global::Console.instance.Print(text);
		}
	}

	// Token: 0x06000866 RID: 2150 RVA: 0x00040D80 File Offset: 0x0003EF80
	public void Kick(string user)
	{
		if (this.IsServer())
		{
			this.InternalKick(user);
			return;
		}
		ZRpc serverRPC = this.GetServerRPC();
		if (serverRPC != null)
		{
			serverRPC.Invoke("Kick", new object[]
			{
				user
			});
		}
	}

	// Token: 0x06000867 RID: 2151 RVA: 0x00040DBC File Offset: 0x0003EFBC
	private void RPC_Kick(ZRpc rpc, string user)
	{
		if (!this.m_adminList.Contains(rpc.GetSocket().GetHostName()))
		{
			this.RemotePrint(rpc, "You are not admin");
			return;
		}
		this.RemotePrint(rpc, "Kicking user " + user);
		this.InternalKick(user);
	}

	// Token: 0x06000868 RID: 2152 RVA: 0x00040DFC File Offset: 0x0003EFFC
	private void InternalKick(string user)
	{
		if (user == "")
		{
			return;
		}
		ZNetPeer znetPeer = this.GetPeerByHostName(user);
		if (znetPeer == null)
		{
			znetPeer = this.GetPeerByPlayerName(user);
		}
		if (znetPeer != null)
		{
			this.InternalKick(znetPeer);
		}
	}

	// Token: 0x06000869 RID: 2153 RVA: 0x00040E34 File Offset: 0x0003F034
	private void InternalKick(ZNetPeer peer)
	{
		if (!this.IsServer())
		{
			return;
		}
		if (peer != null)
		{
			ZLog.Log("Kicking " + peer.m_playerName);
			this.SendDisconnect(peer);
			this.Disconnect(peer);
		}
	}

	// Token: 0x0600086A RID: 2154 RVA: 0x00040E65 File Offset: 0x0003F065
	public bool IsAllowed(string hostName, string playerName)
	{
		return !this.m_bannedList.Contains(hostName) && !this.m_bannedList.Contains(playerName) && (this.m_permittedList.Count() <= 0 || this.m_permittedList.Contains(hostName));
	}

	// Token: 0x0600086B RID: 2155 RVA: 0x00040EA4 File Offset: 0x0003F0A4
	public void Ban(string user)
	{
		if (this.IsServer())
		{
			this.InternalBan(null, user);
			return;
		}
		ZRpc serverRPC = this.GetServerRPC();
		if (serverRPC != null)
		{
			serverRPC.Invoke("Ban", new object[]
			{
				user
			});
		}
	}

	// Token: 0x0600086C RID: 2156 RVA: 0x00040EE1 File Offset: 0x0003F0E1
	private void RPC_Ban(ZRpc rpc, string user)
	{
		if (!this.m_adminList.Contains(rpc.GetSocket().GetHostName()))
		{
			this.RemotePrint(rpc, "You are not admin");
			return;
		}
		this.InternalBan(rpc, user);
	}

	// Token: 0x0600086D RID: 2157 RVA: 0x00040F10 File Offset: 0x0003F110
	private void InternalBan(ZRpc rpc, string user)
	{
		if (!this.IsServer())
		{
			return;
		}
		if (user == "")
		{
			return;
		}
		ZNetPeer peerByPlayerName = this.GetPeerByPlayerName(user);
		if (peerByPlayerName != null)
		{
			user = peerByPlayerName.m_socket.GetHostName();
		}
		this.RemotePrint(rpc, "Banning user " + user);
		this.m_bannedList.Add(user);
	}

	// Token: 0x0600086E RID: 2158 RVA: 0x00040F6C File Offset: 0x0003F16C
	public void Unban(string user)
	{
		if (this.IsServer())
		{
			this.InternalUnban(null, user);
			return;
		}
		ZRpc serverRPC = this.GetServerRPC();
		if (serverRPC != null)
		{
			serverRPC.Invoke("Unban", new object[]
			{
				user
			});
		}
	}

	// Token: 0x0600086F RID: 2159 RVA: 0x00040FA9 File Offset: 0x0003F1A9
	private void RPC_Unban(ZRpc rpc, string user)
	{
		if (!this.m_adminList.Contains(rpc.GetSocket().GetHostName()))
		{
			this.RemotePrint(rpc, "You are not admin");
			return;
		}
		this.InternalUnban(rpc, user);
	}

	// Token: 0x06000870 RID: 2160 RVA: 0x00040FD8 File Offset: 0x0003F1D8
	private void InternalUnban(ZRpc rpc, string user)
	{
		if (!this.IsServer())
		{
			return;
		}
		if (user == "")
		{
			return;
		}
		this.RemotePrint(rpc, "Unbanning user " + user);
		this.m_bannedList.Remove(user);
	}

	// Token: 0x06000871 RID: 2161 RVA: 0x00041010 File Offset: 0x0003F210
	public void PrintBanned()
	{
		if (this.IsServer())
		{
			this.InternalPrintBanned(null);
			return;
		}
		ZRpc serverRPC = this.GetServerRPC();
		if (serverRPC != null)
		{
			serverRPC.Invoke("PrintBanned", Array.Empty<object>());
		}
	}

	// Token: 0x06000872 RID: 2162 RVA: 0x00041047 File Offset: 0x0003F247
	private void RPC_PrintBanned(ZRpc rpc)
	{
		if (!this.m_adminList.Contains(rpc.GetSocket().GetHostName()))
		{
			this.RemotePrint(rpc, "You are not admin");
			return;
		}
		this.InternalPrintBanned(rpc);
	}

	// Token: 0x06000873 RID: 2163 RVA: 0x00041078 File Offset: 0x0003F278
	private void InternalPrintBanned(ZRpc rpc)
	{
		this.RemotePrint(rpc, "Banned users");
		List<string> list = this.m_bannedList.GetList();
		if (list.Count == 0)
		{
			this.RemotePrint(rpc, "-");
		}
		else
		{
			for (int i = 0; i < list.Count; i++)
			{
				this.RemotePrint(rpc, i.ToString() + ": " + list[i]);
			}
		}
		this.RemotePrint(rpc, "");
		this.RemotePrint(rpc, "Permitted users");
		List<string> list2 = this.m_permittedList.GetList();
		if (list2.Count == 0)
		{
			this.RemotePrint(rpc, "All");
			return;
		}
		for (int j = 0; j < list2.Count; j++)
		{
			this.RemotePrint(rpc, j.ToString() + ": " + list2[j]);
		}
	}

	// Token: 0x0400080E RID: 2062
	private float m_banlistTimer;

	// Token: 0x0400080F RID: 2063
	private static ZNet m_instance;

	// Token: 0x04000810 RID: 2064
	public int m_hostPort = 2456;

	// Token: 0x04000811 RID: 2065
	public RectTransform m_passwordDialog;

	// Token: 0x04000812 RID: 2066
	public RectTransform m_connectingDialog;

	// Token: 0x04000813 RID: 2067
	public float m_badConnectionPing = 5f;

	// Token: 0x04000814 RID: 2068
	public int m_zdoSectorsWidth = 512;

	// Token: 0x04000815 RID: 2069
	public int m_serverPlayerLimit = 10;

	// Token: 0x04000816 RID: 2070
	private ZConnector2 m_serverConnector;

	// Token: 0x04000817 RID: 2071
	private ISocket m_hostSocket;

	// Token: 0x04000818 RID: 2072
	private List<ZNetPeer> m_peers = new List<ZNetPeer>();

	// Token: 0x04000819 RID: 2073
	private Thread m_saveThread;

	// Token: 0x0400081A RID: 2074
	private float m_saveStartTime;

	// Token: 0x0400081B RID: 2075
	private float m_saveThreadStartTime;

	// Token: 0x0400081C RID: 2076
	private bool m_loadError;

	// Token: 0x0400081D RID: 2077
	private ZDOMan m_zdoMan;

	// Token: 0x0400081E RID: 2078
	private ZRoutedRpc m_routedRpc;

	// Token: 0x0400081F RID: 2079
	private ZNat m_nat;

	// Token: 0x04000820 RID: 2080
	private double m_netTime = 2040.0;

	// Token: 0x04000821 RID: 2081
	private ZDOID m_characterID = ZDOID.None;

	// Token: 0x04000822 RID: 2082
	private Vector3 m_referencePosition = Vector3.zero;

	// Token: 0x04000823 RID: 2083
	private bool m_publicReferencePosition;

	// Token: 0x04000824 RID: 2084
	private float m_periodicSendTimer;

	// Token: 0x04000825 RID: 2085
	private bool m_haveStoped;

	// Token: 0x04000826 RID: 2086
	private static bool m_isServer = true;

	// Token: 0x04000827 RID: 2087
	private static World m_world = null;

	// Token: 0x04000828 RID: 2088
	private static ulong m_serverSteamID = 0UL;

	// Token: 0x04000829 RID: 2089
	private static SteamNetworkingIPAddr m_serverIPAddr;

	// Token: 0x0400082A RID: 2090
	private static bool m_openServer = true;

	// Token: 0x0400082B RID: 2091
	private static bool m_publicServer = true;

	// Token: 0x0400082C RID: 2092
	private static string m_serverPassword = "";

	// Token: 0x0400082D RID: 2093
	private static string m_ServerName = "";

	// Token: 0x0400082E RID: 2094
	private static ZNet.ConnectionStatus m_connectionStatus = ZNet.ConnectionStatus.None;

	// Token: 0x0400082F RID: 2095
	private SyncedList m_adminList;

	// Token: 0x04000830 RID: 2096
	private SyncedList m_bannedList;

	// Token: 0x04000831 RID: 2097
	private SyncedList m_permittedList;

	// Token: 0x04000832 RID: 2098
	private List<ZNet.PlayerInfo> m_players = new List<ZNet.PlayerInfo>();

	// Token: 0x04000833 RID: 2099
	private ZRpc m_tempPasswordRPC;

	// Token: 0x0200016C RID: 364
	public enum ConnectionStatus
	{
		// Token: 0x04001173 RID: 4467
		None,
		// Token: 0x04001174 RID: 4468
		Connecting,
		// Token: 0x04001175 RID: 4469
		Connected,
		// Token: 0x04001176 RID: 4470
		ErrorVersion,
		// Token: 0x04001177 RID: 4471
		ErrorDisconnected,
		// Token: 0x04001178 RID: 4472
		ErrorConnectFailed,
		// Token: 0x04001179 RID: 4473
		ErrorPassword,
		// Token: 0x0400117A RID: 4474
		ErrorAlreadyConnected,
		// Token: 0x0400117B RID: 4475
		ErrorBanned,
		// Token: 0x0400117C RID: 4476
		ErrorFull
	}

	// Token: 0x0200016D RID: 365
	public struct PlayerInfo
	{
		// Token: 0x0400117D RID: 4477
		public string m_name;

		// Token: 0x0400117E RID: 4478
		public string m_host;

		// Token: 0x0400117F RID: 4479
		public ZDOID m_characterID;

		// Token: 0x04001180 RID: 4480
		public bool m_publicPosition;

		// Token: 0x04001181 RID: 4481
		public Vector3 m_position;
	}
}
