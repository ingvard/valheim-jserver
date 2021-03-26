using System;
using System.Collections.Generic;
using System.Threading;
using Steamworks;

// Token: 0x02000090 RID: 144
public class ZSteamSocketOLD : IDisposable, ISocket
{
	// Token: 0x0600099E RID: 2462 RVA: 0x000464BC File Offset: 0x000446BC
	public ZSteamSocketOLD()
	{
		ZSteamSocketOLD.m_sockets.Add(this);
		ZSteamSocketOLD.RegisterGlobalCallbacks();
	}

	// Token: 0x0600099F RID: 2463 RVA: 0x0004650C File Offset: 0x0004470C
	public ZSteamSocketOLD(CSteamID peerID)
	{
		ZSteamSocketOLD.m_sockets.Add(this);
		this.m_peerID = peerID;
		ZSteamSocketOLD.RegisterGlobalCallbacks();
	}

	// Token: 0x060009A0 RID: 2464 RVA: 0x00046564 File Offset: 0x00044764
	private static void RegisterGlobalCallbacks()
	{
		if (ZSteamSocketOLD.m_connectionFailed == null)
		{
			ZLog.Log("ZSteamSocketOLD  Registering global callbacks");
			ZSteamSocketOLD.m_connectionFailed = Callback<P2PSessionConnectFail_t>.Create(new Callback<P2PSessionConnectFail_t>.DispatchDelegate(ZSteamSocketOLD.OnConnectionFailed));
		}
		if (ZSteamSocketOLD.m_SessionRequest == null)
		{
			ZSteamSocketOLD.m_SessionRequest = Callback<P2PSessionRequest_t>.Create(new Callback<P2PSessionRequest_t>.DispatchDelegate(ZSteamSocketOLD.OnSessionRequest));
		}
	}

	// Token: 0x060009A1 RID: 2465 RVA: 0x000465B8 File Offset: 0x000447B8
	private static void UnregisterGlobalCallbacks()
	{
		ZLog.Log("ZSteamSocket  UnregisterGlobalCallbacks, existing sockets:" + ZSteamSocketOLD.m_sockets.Count);
		if (ZSteamSocketOLD.m_connectionFailed != null)
		{
			ZSteamSocketOLD.m_connectionFailed.Dispose();
			ZSteamSocketOLD.m_connectionFailed = null;
		}
		if (ZSteamSocketOLD.m_SessionRequest != null)
		{
			ZSteamSocketOLD.m_SessionRequest.Dispose();
			ZSteamSocketOLD.m_SessionRequest = null;
		}
	}

	// Token: 0x060009A2 RID: 2466 RVA: 0x00046614 File Offset: 0x00044814
	private static void OnConnectionFailed(P2PSessionConnectFail_t data)
	{
		ZLog.Log("Got connection failed callback: " + data.m_steamIDRemote);
		foreach (ZSteamSocketOLD zsteamSocketOLD in ZSteamSocketOLD.m_sockets)
		{
			if (zsteamSocketOLD.IsPeer(data.m_steamIDRemote))
			{
				zsteamSocketOLD.Close();
			}
		}
	}

	// Token: 0x060009A3 RID: 2467 RVA: 0x00046690 File Offset: 0x00044890
	private static void OnSessionRequest(P2PSessionRequest_t data)
	{
		ZLog.Log("Got session request from " + data.m_steamIDRemote);
		if (SteamNetworking.AcceptP2PSessionWithUser(data.m_steamIDRemote))
		{
			ZSteamSocketOLD listner = ZSteamSocketOLD.GetListner();
			if (listner != null)
			{
				listner.QueuePendingConnection(data.m_steamIDRemote);
			}
		}
	}

	// Token: 0x060009A4 RID: 2468 RVA: 0x000466DC File Offset: 0x000448DC
	public void Dispose()
	{
		ZLog.Log("Disposing socket");
		this.Close();
		this.m_pkgQueue.Clear();
		ZSteamSocketOLD.m_sockets.Remove(this);
		if (ZSteamSocketOLD.m_sockets.Count == 0)
		{
			ZLog.Log("Last socket, unregistering callback");
			ZSteamSocketOLD.UnregisterGlobalCallbacks();
		}
	}

	// Token: 0x060009A5 RID: 2469 RVA: 0x0004672C File Offset: 0x0004492C
	public void Close()
	{
		ZLog.Log("Closing socket " + this.GetEndPointString());
		if (this.m_peerID != CSteamID.Nil)
		{
			this.Flush();
			ZLog.Log("  send queue size:" + this.m_sendQueue.Count);
			Thread.Sleep(100);
			P2PSessionState_t p2PSessionState_t;
			SteamNetworking.GetP2PSessionState(this.m_peerID, out p2PSessionState_t);
			ZLog.Log("  P2P state, bytes in send queue:" + p2PSessionState_t.m_nBytesQueuedForSend);
			SteamNetworking.CloseP2PSessionWithUser(this.m_peerID);
			SteamUser.EndAuthSession(this.m_peerID);
			this.m_peerID = CSteamID.Nil;
		}
		this.m_listner = false;
	}

	// Token: 0x060009A6 RID: 2470 RVA: 0x000467DE File Offset: 0x000449DE
	public bool StartHost()
	{
		this.m_listner = true;
		this.m_pendingConnections.Clear();
		return true;
	}

	// Token: 0x060009A7 RID: 2471 RVA: 0x000467F4 File Offset: 0x000449F4
	private ZSteamSocketOLD QueuePendingConnection(CSteamID id)
	{
		foreach (ZSteamSocketOLD zsteamSocketOLD in this.m_pendingConnections)
		{
			if (zsteamSocketOLD.IsPeer(id))
			{
				return zsteamSocketOLD;
			}
		}
		ZSteamSocketOLD zsteamSocketOLD2 = new ZSteamSocketOLD(id);
		this.m_pendingConnections.Enqueue(zsteamSocketOLD2);
		return zsteamSocketOLD2;
	}

	// Token: 0x060009A8 RID: 2472 RVA: 0x00046864 File Offset: 0x00044A64
	public ISocket Accept()
	{
		if (!this.m_listner)
		{
			return null;
		}
		if (this.m_pendingConnections.Count > 0)
		{
			return this.m_pendingConnections.Dequeue();
		}
		return null;
	}

	// Token: 0x060009A9 RID: 2473 RVA: 0x0004688B File Offset: 0x00044A8B
	public bool IsConnected()
	{
		return this.m_peerID != CSteamID.Nil;
	}

	// Token: 0x060009AA RID: 2474 RVA: 0x000468A0 File Offset: 0x00044AA0
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
		byte[] bytes = BitConverter.GetBytes(array.Length);
		byte[] array2 = new byte[array.Length + bytes.Length];
		bytes.CopyTo(array2, 0);
		array.CopyTo(array2, 4);
		this.m_sendQueue.Enqueue(array);
		this.SendQueuedPackages();
	}

	// Token: 0x060009AB RID: 2475 RVA: 0x000468FE File Offset: 0x00044AFE
	public bool Flush()
	{
		this.SendQueuedPackages();
		return this.m_sendQueue.Count == 0;
	}

	// Token: 0x060009AC RID: 2476 RVA: 0x00046914 File Offset: 0x00044B14
	private void SendQueuedPackages()
	{
		if (!this.IsConnected())
		{
			return;
		}
		while (this.m_sendQueue.Count > 0)
		{
			byte[] array = this.m_sendQueue.Peek();
			EP2PSend eP2PSendType = EP2PSend.k_EP2PSendReliable;
			if (!SteamNetworking.SendP2PPacket(this.m_peerID, array, (uint)array.Length, eP2PSendType, 0))
			{
				break;
			}
			this.m_totalSent += array.Length;
			this.m_sendQueue.Dequeue();
		}
	}

	// Token: 0x060009AD RID: 2477 RVA: 0x00046978 File Offset: 0x00044B78
	public static void Update()
	{
		foreach (ZSteamSocketOLD zsteamSocketOLD in ZSteamSocketOLD.m_sockets)
		{
			zsteamSocketOLD.SendQueuedPackages();
		}
		ZSteamSocketOLD.ReceivePackages();
	}

	// Token: 0x060009AE RID: 2478 RVA: 0x000469CC File Offset: 0x00044BCC
	private static void ReceivePackages()
	{
		uint num;
		while (SteamNetworking.IsP2PPacketAvailable(out num, 0))
		{
			byte[] array = new byte[num];
			uint num2;
			CSteamID sender;
			if (!SteamNetworking.ReadP2PPacket(array, num, out num2, out sender, 0))
			{
				break;
			}
			ZSteamSocketOLD.QueueNewPkg(sender, array);
		}
	}

	// Token: 0x060009AF RID: 2479 RVA: 0x00046A04 File Offset: 0x00044C04
	private static void QueueNewPkg(CSteamID sender, byte[] data)
	{
		foreach (ZSteamSocketOLD zsteamSocketOLD in ZSteamSocketOLD.m_sockets)
		{
			if (zsteamSocketOLD.IsPeer(sender))
			{
				zsteamSocketOLD.QueuePackage(data);
				return;
			}
		}
		ZSteamSocketOLD listner = ZSteamSocketOLD.GetListner();
		if (listner != null)
		{
			ZLog.Log("Got package from unconnected peer " + sender);
			listner.QueuePendingConnection(sender).QueuePackage(data);
			return;
		}
		ZLog.Log("Got package from unkown peer " + sender + " but no active listner");
	}

	// Token: 0x060009B0 RID: 2480 RVA: 0x00046AA8 File Offset: 0x00044CA8
	private static ZSteamSocketOLD GetListner()
	{
		foreach (ZSteamSocketOLD zsteamSocketOLD in ZSteamSocketOLD.m_sockets)
		{
			if (zsteamSocketOLD.IsHost())
			{
				return zsteamSocketOLD;
			}
		}
		return null;
	}

	// Token: 0x060009B1 RID: 2481 RVA: 0x00046B04 File Offset: 0x00044D04
	private void QueuePackage(byte[] data)
	{
		ZPackage item = new ZPackage(data);
		this.m_pkgQueue.Enqueue(item);
		this.m_gotData = true;
		this.m_totalRecv += data.Length;
	}

	// Token: 0x060009B2 RID: 2482 RVA: 0x00046B3B File Offset: 0x00044D3B
	public ZPackage Recv()
	{
		if (!this.IsConnected())
		{
			return null;
		}
		if (this.m_pkgQueue.Count > 0)
		{
			return this.m_pkgQueue.Dequeue();
		}
		return null;
	}

	// Token: 0x060009B3 RID: 2483 RVA: 0x00046B62 File Offset: 0x00044D62
	public string GetEndPointString()
	{
		return this.m_peerID.ToString();
	}

	// Token: 0x060009B4 RID: 2484 RVA: 0x00046B62 File Offset: 0x00044D62
	public string GetHostName()
	{
		return this.m_peerID.ToString();
	}

	// Token: 0x060009B5 RID: 2485 RVA: 0x00046B75 File Offset: 0x00044D75
	public CSteamID GetPeerID()
	{
		return this.m_peerID;
	}

	// Token: 0x060009B6 RID: 2486 RVA: 0x00046B7D File Offset: 0x00044D7D
	public bool IsPeer(CSteamID peer)
	{
		return this.IsConnected() && peer == this.m_peerID;
	}

	// Token: 0x060009B7 RID: 2487 RVA: 0x00046B95 File Offset: 0x00044D95
	public bool IsHost()
	{
		return this.m_listner;
	}

	// Token: 0x060009B8 RID: 2488 RVA: 0x00046BA0 File Offset: 0x00044DA0
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
		return num;
	}

	// Token: 0x060009B9 RID: 2489 RVA: 0x00046C00 File Offset: 0x00044E00
	public bool IsSending()
	{
		return this.IsConnected() && this.m_sendQueue.Count > 0;
	}

	// Token: 0x060009BA RID: 2490 RVA: 0x00044C3E File Offset: 0x00042E3E
	public void GetConnectionQuality(out float localQuality, out float remoteQuality, out int ping, out float outByteSec, out float inByteSec)
	{
		localQuality = 0f;
		remoteQuality = 0f;
		ping = 0;
		outByteSec = 0f;
		inByteSec = 0f;
	}

	// Token: 0x060009BB RID: 2491 RVA: 0x00046C1A File Offset: 0x00044E1A
	public void GetAndResetStats(out int totalSent, out int totalRecv)
	{
		totalSent = this.m_totalSent;
		totalRecv = this.m_totalRecv;
		this.m_totalSent = 0;
		this.m_totalRecv = 0;
	}

	// Token: 0x060009BC RID: 2492 RVA: 0x00046C3A File Offset: 0x00044E3A
	public bool GotNewData()
	{
		bool gotData = this.m_gotData;
		this.m_gotData = false;
		return gotData;
	}

	// Token: 0x060009BD RID: 2493 RVA: 0x000023E2 File Offset: 0x000005E2
	public int GetCurrentSendRate()
	{
		return 0;
	}

	// Token: 0x060009BE RID: 2494 RVA: 0x000023E2 File Offset: 0x000005E2
	public int GetAverageSendRate()
	{
		return 0;
	}

	// Token: 0x060009BF RID: 2495 RVA: 0x00046C49 File Offset: 0x00044E49
	public int GetHostPort()
	{
		if (this.IsHost())
		{
			return 1;
		}
		return -1;
	}

	// Token: 0x040008C4 RID: 2244
	private static List<ZSteamSocketOLD> m_sockets = new List<ZSteamSocketOLD>();

	// Token: 0x040008C5 RID: 2245
	private static Callback<P2PSessionRequest_t> m_SessionRequest;

	// Token: 0x040008C6 RID: 2246
	private static Callback<P2PSessionConnectFail_t> m_connectionFailed;

	// Token: 0x040008C7 RID: 2247
	private Queue<ZSteamSocketOLD> m_pendingConnections = new Queue<ZSteamSocketOLD>();

	// Token: 0x040008C8 RID: 2248
	private CSteamID m_peerID = CSteamID.Nil;

	// Token: 0x040008C9 RID: 2249
	private bool m_listner;

	// Token: 0x040008CA RID: 2250
	private Queue<ZPackage> m_pkgQueue = new Queue<ZPackage>();

	// Token: 0x040008CB RID: 2251
	private Queue<byte[]> m_sendQueue = new Queue<byte[]>();

	// Token: 0x040008CC RID: 2252
	private int m_totalSent;

	// Token: 0x040008CD RID: 2253
	private int m_totalRecv;

	// Token: 0x040008CE RID: 2254
	private bool m_gotData;
}
