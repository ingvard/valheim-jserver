using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Steamworks;
using UnityEngine;

// Token: 0x0200008F RID: 143
public class ZSteamSocket : IDisposable, ISocket
{
	// Token: 0x0600097D RID: 2429 RVA: 0x00045A10 File Offset: 0x00043C10
	public ZSteamSocket()
	{
		ZSteamSocket.RegisterGlobalCallbacks();
		ZSteamSocket.m_sockets.Add(this);
	}

	// Token: 0x0600097E RID: 2430 RVA: 0x00045A6C File Offset: 0x00043C6C
	public ZSteamSocket(SteamNetworkingIPAddr host)
	{
		ZSteamSocket.RegisterGlobalCallbacks();
		string str;
		host.ToString(out str, true);
		ZLog.Log("Starting to connect to " + str);
		this.m_con = SteamNetworkingSockets.ConnectByIPAddress(ref host, 0, null);
		ZSteamSocket.m_sockets.Add(this);
	}

	// Token: 0x0600097F RID: 2431 RVA: 0x00045AF0 File Offset: 0x00043CF0
	public ZSteamSocket(CSteamID peerID)
	{
		ZSteamSocket.RegisterGlobalCallbacks();
		this.m_peerID.SetSteamID(peerID);
		this.m_con = SteamNetworkingSockets.ConnectP2P(ref this.m_peerID, 0, 0, null);
		ZLog.Log("Connecting to " + this.m_peerID.GetSteamID().ToString());
		ZSteamSocket.m_sockets.Add(this);
	}

	// Token: 0x06000980 RID: 2432 RVA: 0x00045B94 File Offset: 0x00043D94
	public ZSteamSocket(HSteamNetConnection con)
	{
		ZSteamSocket.RegisterGlobalCallbacks();
		this.m_con = con;
		SteamNetConnectionInfo_t steamNetConnectionInfo_t;
		SteamNetworkingSockets.GetConnectionInfo(this.m_con, out steamNetConnectionInfo_t);
		this.m_peerID = steamNetConnectionInfo_t.m_identityRemote;
		ZLog.Log("Connecting to " + this.m_peerID.ToString());
		ZSteamSocket.m_sockets.Add(this);
	}

	// Token: 0x06000981 RID: 2433 RVA: 0x00045C30 File Offset: 0x00043E30
	private static void RegisterGlobalCallbacks()
	{
		if (ZSteamSocket.m_statusChanged == null)
		{
			ZSteamSocket.m_statusChanged = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(new Callback<SteamNetConnectionStatusChangedCallback_t>.DispatchDelegate(ZSteamSocket.OnStatusChanged));
			GCHandle gchandle = GCHandle.Alloc(30000f, GCHandleType.Pinned);
			GCHandle gchandle2 = GCHandle.Alloc(1, GCHandleType.Pinned);
			GCHandle gchandle3 = GCHandle.Alloc(153600, GCHandleType.Pinned);
			SteamNetworkingUtils.SetConfigValue(ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutConnected, ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global, IntPtr.Zero, ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Float, gchandle.AddrOfPinnedObject());
			SteamNetworkingUtils.SetConfigValue(ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_IP_AllowWithoutAuth, ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global, IntPtr.Zero, ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32, gchandle2.AddrOfPinnedObject());
			SteamNetworkingUtils.SetConfigValue(ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_SendRateMin, ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global, IntPtr.Zero, ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32, gchandle3.AddrOfPinnedObject());
			SteamNetworkingUtils.SetConfigValue(ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_SendRateMax, ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global, IntPtr.Zero, ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32, gchandle3.AddrOfPinnedObject());
			gchandle.Free();
			gchandle2.Free();
			gchandle3.Free();
		}
	}

	// Token: 0x06000982 RID: 2434 RVA: 0x00045CF9 File Offset: 0x00043EF9
	private static void UnregisterGlobalCallbacks()
	{
		ZLog.Log("ZSteamSocket  UnregisterGlobalCallbacks, existing sockets:" + ZSteamSocket.m_sockets.Count);
		if (ZSteamSocket.m_statusChanged != null)
		{
			ZSteamSocket.m_statusChanged.Dispose();
			ZSteamSocket.m_statusChanged = null;
		}
	}

	// Token: 0x06000983 RID: 2435 RVA: 0x00045D30 File Offset: 0x00043F30
	private static void OnStatusChanged(SteamNetConnectionStatusChangedCallback_t data)
	{
		ZLog.Log("Got status changed msg " + data.m_info.m_eState);
		if (data.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected && data.m_eOldState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting)
		{
			ZLog.Log("Connected");
			ZSteamSocket zsteamSocket = ZSteamSocket.FindSocket(data.m_hConn);
			if (zsteamSocket != null)
			{
				SteamNetConnectionInfo_t steamNetConnectionInfo_t;
				if (SteamNetworkingSockets.GetConnectionInfo(data.m_hConn, out steamNetConnectionInfo_t))
				{
					zsteamSocket.m_peerID = steamNetConnectionInfo_t.m_identityRemote;
				}
				ZLog.Log("Got connection SteamID " + zsteamSocket.m_peerID.GetSteamID().ToString());
			}
		}
		if (data.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting && data.m_eOldState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_None)
		{
			ZLog.Log("New connection");
			ZSteamSocket listner = ZSteamSocket.GetListner();
			if (listner != null)
			{
				listner.OnNewConnection(data.m_hConn);
			}
		}
		if (data.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally)
		{
			ZLog.Log(string.Concat(new object[]
			{
				"Got problem ",
				data.m_info.m_eEndReason,
				":",
				data.m_info.m_szEndDebug
			}));
			ZSteamSocket zsteamSocket2 = ZSteamSocket.FindSocket(data.m_hConn);
			if (zsteamSocket2 != null)
			{
				ZLog.Log("  Closing socket " + zsteamSocket2.GetHostName());
				zsteamSocket2.Close();
			}
		}
		if (data.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer)
		{
			ZLog.Log("Socket closed by peer " + data);
			ZSteamSocket zsteamSocket3 = ZSteamSocket.FindSocket(data.m_hConn);
			if (zsteamSocket3 != null)
			{
				ZLog.Log("  Closing socket " + zsteamSocket3.GetHostName());
				zsteamSocket3.Close();
			}
		}
	}

	// Token: 0x06000984 RID: 2436 RVA: 0x00045ED4 File Offset: 0x000440D4
	private static ZSteamSocket FindSocket(HSteamNetConnection con)
	{
		foreach (ZSteamSocket zsteamSocket in ZSteamSocket.m_sockets)
		{
			if (zsteamSocket.m_con == con)
			{
				return zsteamSocket;
			}
		}
		return null;
	}

	// Token: 0x06000985 RID: 2437 RVA: 0x00045F34 File Offset: 0x00044134
	public void Dispose()
	{
		ZLog.Log("Disposing socket");
		this.Close();
		this.m_pkgQueue.Clear();
		ZSteamSocket.m_sockets.Remove(this);
		if (ZSteamSocket.m_sockets.Count == 0)
		{
			ZLog.Log("Last socket, unregistering callback");
			ZSteamSocket.UnregisterGlobalCallbacks();
		}
	}

	// Token: 0x06000986 RID: 2438 RVA: 0x00045F84 File Offset: 0x00044184
	public void Close()
	{
		if (this.m_con != HSteamNetConnection.Invalid)
		{
			ZLog.Log("Closing socket " + this.GetEndPointString());
			this.Flush();
			ZLog.Log("  send queue size:" + this.m_sendQueue.Count);
			Thread.Sleep(100);
			CSteamID steamID = this.m_peerID.GetSteamID();
			SteamNetworkingSockets.CloseConnection(this.m_con, 0, "", false);
			SteamUser.EndAuthSession(steamID);
			this.m_con = HSteamNetConnection.Invalid;
		}
		if (this.m_listenSocket != HSteamListenSocket.Invalid)
		{
			ZLog.Log("Stopping listening socket");
			SteamNetworkingSockets.CloseListenSocket(this.m_listenSocket);
			this.m_listenSocket = HSteamListenSocket.Invalid;
		}
		if (ZSteamSocket.m_hostSocket == this)
		{
			ZSteamSocket.m_hostSocket = null;
		}
		this.m_peerID.Clear();
	}

	// Token: 0x06000987 RID: 2439 RVA: 0x0004605F File Offset: 0x0004425F
	public bool StartHost()
	{
		if (ZSteamSocket.m_hostSocket != null)
		{
			ZLog.Log("Listen socket already started");
			return false;
		}
		this.m_listenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, 0, null);
		ZSteamSocket.m_hostSocket = this;
		this.m_pendingConnections.Clear();
		return true;
	}

	// Token: 0x06000988 RID: 2440 RVA: 0x00046094 File Offset: 0x00044294
	private void OnNewConnection(HSteamNetConnection con)
	{
		EResult eresult = SteamNetworkingSockets.AcceptConnection(con);
		ZLog.Log("Accepting connection " + eresult);
		if (eresult == EResult.k_EResultOK)
		{
			this.QueuePendingConnection(con);
		}
	}

	// Token: 0x06000989 RID: 2441 RVA: 0x000460C8 File Offset: 0x000442C8
	private void QueuePendingConnection(HSteamNetConnection con)
	{
		ZSteamSocket item = new ZSteamSocket(con);
		this.m_pendingConnections.Enqueue(item);
	}

	// Token: 0x0600098A RID: 2442 RVA: 0x000460E8 File Offset: 0x000442E8
	public ISocket Accept()
	{
		if (this.m_listenSocket == HSteamListenSocket.Invalid)
		{
			return null;
		}
		if (this.m_pendingConnections.Count > 0)
		{
			return this.m_pendingConnections.Dequeue();
		}
		return null;
	}

	// Token: 0x0600098B RID: 2443 RVA: 0x00046119 File Offset: 0x00044319
	public bool IsConnected()
	{
		return this.m_con != HSteamNetConnection.Invalid;
	}

	// Token: 0x0600098C RID: 2444 RVA: 0x0004612C File Offset: 0x0004432C
	public void Send(ZPackage pkg)
	{
		if (pkg.Size() == 0)
		{
			return;
		}
		if (!this.IsConnected())
		{
			return;
		}
		byte[] array = pkg.GetArray();
		this.m_sendQueue.Enqueue(array);
		this.SendQueuedPackages();
	}

	// Token: 0x0600098D RID: 2445 RVA: 0x00046164 File Offset: 0x00044364
	public bool Flush()
	{
		this.SendQueuedPackages();
		HSteamNetConnection con = this.m_con;
		SteamNetworkingSockets.FlushMessagesOnConnection(this.m_con);
		return this.m_sendQueue.Count == 0;
	}

	// Token: 0x0600098E RID: 2446 RVA: 0x00046190 File Offset: 0x00044390
	private void SendQueuedPackages()
	{
		if (!this.IsConnected())
		{
			return;
		}
		while (this.m_sendQueue.Count > 0)
		{
			byte[] array = this.m_sendQueue.Peek();
			IntPtr intPtr = Marshal.AllocHGlobal(array.Length);
			Marshal.Copy(array, 0, intPtr, array.Length);
			long num;
			EResult eresult = SteamNetworkingSockets.SendMessageToConnection(this.m_con, intPtr, (uint)array.Length, 8, out num);
			Marshal.FreeHGlobal(intPtr);
			if (eresult != EResult.k_EResultOK)
			{
				ZLog.Log("Failed to send data " + eresult);
				return;
			}
			this.m_totalSent += array.Length;
			this.m_sendQueue.Dequeue();
		}
	}

	// Token: 0x0600098F RID: 2447 RVA: 0x00046228 File Offset: 0x00044428
	public static void UpdateAllSockets(float dt)
	{
		foreach (ZSteamSocket zsteamSocket in ZSteamSocket.m_sockets)
		{
			zsteamSocket.Update(dt);
		}
	}

	// Token: 0x06000990 RID: 2448 RVA: 0x00046278 File Offset: 0x00044478
	private void Update(float dt)
	{
		this.SendQueuedPackages();
	}

	// Token: 0x06000991 RID: 2449 RVA: 0x00046280 File Offset: 0x00044480
	private static ZSteamSocket GetListner()
	{
		return ZSteamSocket.m_hostSocket;
	}

	// Token: 0x06000992 RID: 2450 RVA: 0x00046288 File Offset: 0x00044488
	public ZPackage Recv()
	{
		if (!this.IsConnected())
		{
			return null;
		}
		IntPtr[] array = new IntPtr[1];
		if (SteamNetworkingSockets.ReceiveMessagesOnConnection(this.m_con, array, 1) == 1)
		{
			SteamNetworkingMessage_t steamNetworkingMessage_t = Marshal.PtrToStructure<SteamNetworkingMessage_t>(array[0]);
			byte[] array2 = new byte[steamNetworkingMessage_t.m_cbSize];
			Marshal.Copy(steamNetworkingMessage_t.m_pData, array2, 0, steamNetworkingMessage_t.m_cbSize);
			ZPackage zpackage = new ZPackage(array2);
			steamNetworkingMessage_t.m_pfnRelease = array[0];
			steamNetworkingMessage_t.Release();
			this.m_totalRecv += zpackage.Size();
			this.m_gotData = true;
			return zpackage;
		}
		return null;
	}

	// Token: 0x06000993 RID: 2451 RVA: 0x00046314 File Offset: 0x00044514
	public string GetEndPointString()
	{
		return this.m_peerID.GetSteamID().ToString();
	}

	// Token: 0x06000994 RID: 2452 RVA: 0x0004633C File Offset: 0x0004453C
	public string GetHostName()
	{
		return this.m_peerID.GetSteamID().ToString();
	}

	// Token: 0x06000995 RID: 2453 RVA: 0x00046362 File Offset: 0x00044562
	public CSteamID GetPeerID()
	{
		return this.m_peerID.GetSteamID();
	}

	// Token: 0x06000996 RID: 2454 RVA: 0x0004636F File Offset: 0x0004456F
	public bool IsHost()
	{
		return ZSteamSocket.m_hostSocket != null;
	}

	// Token: 0x06000997 RID: 2455 RVA: 0x0004637C File Offset: 0x0004457C
	public int GetSendQueueSize()
	{
		if (!this.IsConnected())
		{
			return 0;
		}
		int num = 0;
		foreach (byte[] array in this.m_sendQueue)
		{
			num += array.Length;
		}
		SteamNetworkingQuickConnectionStatus steamNetworkingQuickConnectionStatus;
		if (SteamNetworkingSockets.GetQuickConnectionStatus(this.m_con, out steamNetworkingQuickConnectionStatus))
		{
			num += steamNetworkingQuickConnectionStatus.m_cbPendingReliable + steamNetworkingQuickConnectionStatus.m_cbPendingUnreliable + steamNetworkingQuickConnectionStatus.m_cbSentUnackedReliable;
		}
		return num;
	}

	// Token: 0x06000998 RID: 2456 RVA: 0x00046404 File Offset: 0x00044604
	public int GetCurrentSendRate()
	{
		SteamNetworkingQuickConnectionStatus steamNetworkingQuickConnectionStatus;
		if (!SteamNetworkingSockets.GetQuickConnectionStatus(this.m_con, out steamNetworkingQuickConnectionStatus))
		{
			return 0;
		}
		int num = steamNetworkingQuickConnectionStatus.m_cbPendingReliable + steamNetworkingQuickConnectionStatus.m_cbPendingUnreliable + steamNetworkingQuickConnectionStatus.m_cbSentUnackedReliable;
		foreach (byte[] array in this.m_sendQueue)
		{
			num += array.Length;
		}
		return num / Mathf.Clamp(steamNetworkingQuickConnectionStatus.m_nPing, 5, 250) * 1000;
	}

	// Token: 0x06000999 RID: 2457 RVA: 0x00046498 File Offset: 0x00044698
	public void GetConnectionQuality(out float localQuality, out float remoteQuality, out int ping, out float outByteSec, out float inByteSec)
	{
		SteamNetworkingQuickConnectionStatus steamNetworkingQuickConnectionStatus;
		if (SteamNetworkingSockets.GetQuickConnectionStatus(this.m_con, out steamNetworkingQuickConnectionStatus))
		{
			localQuality = steamNetworkingQuickConnectionStatus.m_flConnectionQualityLocal;
			remoteQuality = steamNetworkingQuickConnectionStatus.m_flConnectionQualityRemote;
			ping = steamNetworkingQuickConnectionStatus.m_nPing;
			outByteSec = steamNetworkingQuickConnectionStatus.m_flOutBytesPerSec;
			inByteSec = steamNetworkingQuickConnectionStatus.m_flInBytesPerSec;
			return;
		}
		localQuality = 0f;
		remoteQuality = 0f;
		ping = 0;
		outByteSec = 0f;
		inByteSec = 0f;
	}

	// Token: 0x0600099A RID: 2458 RVA: 0x00046500 File Offset: 0x00044700
	public void GetAndResetStats(out int totalSent, out int totalRecv)
	{
		totalSent = this.m_totalSent;
		totalRecv = this.m_totalRecv;
		this.m_totalSent = 0;
		this.m_totalRecv = 0;
	}

	// Token: 0x0600099B RID: 2459 RVA: 0x00046520 File Offset: 0x00044720
	public bool GotNewData()
	{
		bool gotData = this.m_gotData;
		this.m_gotData = false;
		return gotData;
	}

	// Token: 0x0600099C RID: 2460 RVA: 0x0004652F File Offset: 0x0004472F
	public int GetHostPort()
	{
		if (this.IsHost())
		{
			return 1;
		}
		return -1;
	}

	// Token: 0x0600099D RID: 2461 RVA: 0x0004653C File Offset: 0x0004473C
	public static void SetDataPort(int port)
	{
		ZSteamSocket.m_steamDataPort = port;
	}

	// Token: 0x040008BA RID: 2234
	private static List<ZSteamSocket> m_sockets = new List<ZSteamSocket>();

	// Token: 0x040008BB RID: 2235
	private static Callback<SteamNetConnectionStatusChangedCallback_t> m_statusChanged;

	// Token: 0x040008BC RID: 2236
	private static int m_steamDataPort = 2459;

	// Token: 0x040008BD RID: 2237
	private Queue<ZSteamSocket> m_pendingConnections = new Queue<ZSteamSocket>();

	// Token: 0x040008BE RID: 2238
	private HSteamNetConnection m_con = HSteamNetConnection.Invalid;

	// Token: 0x040008BF RID: 2239
	private SteamNetworkingIdentity m_peerID;

	// Token: 0x040008C0 RID: 2240
	private Queue<ZPackage> m_pkgQueue = new Queue<ZPackage>();

	// Token: 0x040008C1 RID: 2241
	private Queue<byte[]> m_sendQueue = new Queue<byte[]>();

	// Token: 0x040008C2 RID: 2242
	private int m_totalSent;

	// Token: 0x040008C3 RID: 2243
	private int m_totalRecv;

	// Token: 0x040008C4 RID: 2244
	private bool m_gotData;

	// Token: 0x040008C5 RID: 2245
	private HSteamListenSocket m_listenSocket = HSteamListenSocket.Invalid;

	// Token: 0x040008C6 RID: 2246
	private static ZSteamSocket m_hostSocket;

	// Token: 0x040008C7 RID: 2247
	private static ESteamNetworkingConfigValue[] m_configValues = new ESteamNetworkingConfigValue[1];
}
