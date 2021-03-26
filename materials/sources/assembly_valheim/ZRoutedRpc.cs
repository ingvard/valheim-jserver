using System;
using System.Collections.Generic;

// Token: 0x0200008A RID: 138
public class ZRoutedRpc
{
	// Token: 0x1700001C RID: 28
	// (get) Token: 0x060008F1 RID: 2289 RVA: 0x00042C3B File Offset: 0x00040E3B
	public static ZRoutedRpc instance
	{
		get
		{
			return ZRoutedRpc.m_instance;
		}
	}

	// Token: 0x060008F2 RID: 2290 RVA: 0x00042C42 File Offset: 0x00040E42
	public ZRoutedRpc(bool server)
	{
		ZRoutedRpc.m_instance = this;
		this.m_server = server;
	}

	// Token: 0x060008F3 RID: 2291 RVA: 0x00042C74 File Offset: 0x00040E74
	public void SetUID(long uid)
	{
		this.m_id = uid;
	}

	// Token: 0x060008F4 RID: 2292 RVA: 0x00042C80 File Offset: 0x00040E80
	public void AddPeer(ZNetPeer peer)
	{
		this.m_peers.Add(peer);
		peer.m_rpc.Register<ZPackage>("RoutedRPC", new Action<ZRpc, ZPackage>(this.RPC_RoutedRPC));
		if (this.m_onNewPeer != null)
		{
			this.m_onNewPeer(peer.m_uid);
		}
	}

	// Token: 0x060008F5 RID: 2293 RVA: 0x00042CCE File Offset: 0x00040ECE
	public void RemovePeer(ZNetPeer peer)
	{
		this.m_peers.Remove(peer);
	}

	// Token: 0x060008F6 RID: 2294 RVA: 0x00042CE0 File Offset: 0x00040EE0
	private ZNetPeer GetPeer(long uid)
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

	// Token: 0x060008F7 RID: 2295 RVA: 0x00042D3C File Offset: 0x00040F3C
	public void InvokeRoutedRPC(long targetPeerID, string methodName, params object[] parameters)
	{
		this.InvokeRoutedRPC(targetPeerID, ZDOID.None, methodName, parameters);
	}

	// Token: 0x060008F8 RID: 2296 RVA: 0x00042D4C File Offset: 0x00040F4C
	public void InvokeRoutedRPC(string methodName, params object[] parameters)
	{
		this.InvokeRoutedRPC(this.GetServerPeerID(), methodName, parameters);
	}

	// Token: 0x060008F9 RID: 2297 RVA: 0x00042D5C File Offset: 0x00040F5C
	private long GetServerPeerID()
	{
		if (this.m_server)
		{
			return this.m_id;
		}
		if (this.m_peers.Count > 0)
		{
			return this.m_peers[0].m_uid;
		}
		return 0L;
	}

	// Token: 0x060008FA RID: 2298 RVA: 0x00042D90 File Offset: 0x00040F90
	public void InvokeRoutedRPC(long targetPeerID, ZDOID targetZDO, string methodName, params object[] parameters)
	{
		ZRoutedRpc.RoutedRPCData routedRPCData = new ZRoutedRpc.RoutedRPCData();
		ZRoutedRpc.RoutedRPCData routedRPCData2 = routedRPCData;
		long id = this.m_id;
		int rpcMsgID = this.m_rpcMsgID;
		this.m_rpcMsgID = rpcMsgID + 1;
		routedRPCData2.m_msgID = id + (long)rpcMsgID;
		routedRPCData.m_senderPeerID = this.m_id;
		routedRPCData.m_targetPeerID = targetPeerID;
		routedRPCData.m_targetZDO = targetZDO;
		routedRPCData.m_methodHash = methodName.GetStableHashCode();
		ZRpc.Serialize(parameters, ref routedRPCData.m_parameters);
		routedRPCData.m_parameters.SetPos(0);
		if (targetPeerID == this.m_id || targetPeerID == 0L)
		{
			this.HandleRoutedRPC(routedRPCData);
		}
		if (targetPeerID != this.m_id)
		{
			this.RouteRPC(routedRPCData);
		}
	}

	// Token: 0x060008FB RID: 2299 RVA: 0x00042E24 File Offset: 0x00041024
	private void RouteRPC(ZRoutedRpc.RoutedRPCData rpcData)
	{
		ZPackage zpackage = new ZPackage();
		rpcData.Serialize(zpackage);
		if (this.m_server)
		{
			if (rpcData.m_targetPeerID != 0L)
			{
				ZNetPeer peer = this.GetPeer(rpcData.m_targetPeerID);
				if (peer != null && peer.IsReady())
				{
					peer.m_rpc.Invoke("RoutedRPC", new object[]
					{
						zpackage
					});
					return;
				}
				return;
			}
			else
			{
				using (List<ZNetPeer>.Enumerator enumerator = this.m_peers.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ZNetPeer znetPeer = enumerator.Current;
						if (rpcData.m_senderPeerID != znetPeer.m_uid && znetPeer.IsReady())
						{
							znetPeer.m_rpc.Invoke("RoutedRPC", new object[]
							{
								zpackage
							});
						}
					}
					return;
				}
			}
		}
		foreach (ZNetPeer znetPeer2 in this.m_peers)
		{
			if (znetPeer2.IsReady())
			{
				znetPeer2.m_rpc.Invoke("RoutedRPC", new object[]
				{
					zpackage
				});
			}
		}
	}

	// Token: 0x060008FC RID: 2300 RVA: 0x00042F5C File Offset: 0x0004115C
	private void RPC_RoutedRPC(ZRpc rpc, ZPackage pkg)
	{
		ZRoutedRpc.RoutedRPCData routedRPCData = new ZRoutedRpc.RoutedRPCData();
		routedRPCData.Deserialize(pkg);
		if (routedRPCData.m_targetPeerID == this.m_id || routedRPCData.m_targetPeerID == 0L)
		{
			this.HandleRoutedRPC(routedRPCData);
		}
		if (this.m_server && routedRPCData.m_targetPeerID != this.m_id)
		{
			this.RouteRPC(routedRPCData);
		}
	}

	// Token: 0x060008FD RID: 2301 RVA: 0x00042FB0 File Offset: 0x000411B0
	private void HandleRoutedRPC(ZRoutedRpc.RoutedRPCData data)
	{
		if (data.m_targetZDO.IsNone())
		{
			RoutedMethodBase routedMethodBase;
			if (this.m_functions.TryGetValue(data.m_methodHash, out routedMethodBase))
			{
				routedMethodBase.Invoke(data.m_senderPeerID, data.m_parameters);
				return;
			}
		}
		else
		{
			ZDO zdo = ZDOMan.instance.GetZDO(data.m_targetZDO);
			if (zdo != null)
			{
				ZNetView znetView = ZNetScene.instance.FindInstance(zdo);
				if (znetView != null)
				{
					znetView.HandleRoutedRPC(data);
				}
			}
		}
	}

	// Token: 0x060008FE RID: 2302 RVA: 0x00043022 File Offset: 0x00041222
	public void Register(string name, Action<long> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod(f));
	}

	// Token: 0x060008FF RID: 2303 RVA: 0x0004303B File Offset: 0x0004123B
	public void Register<T>(string name, Action<long, T> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod<T>(f));
	}

	// Token: 0x06000900 RID: 2304 RVA: 0x00043054 File Offset: 0x00041254
	public void Register<T, U>(string name, Action<long, T, U> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod<T, U>(f));
	}

	// Token: 0x06000901 RID: 2305 RVA: 0x0004306D File Offset: 0x0004126D
	public void Register<T, U, V>(string name, Action<long, T, U, V> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod<T, U, V>(f));
	}

	// Token: 0x06000902 RID: 2306 RVA: 0x00043086 File Offset: 0x00041286
	public void Register<T, U, V, B>(string name, RoutedMethod<T, U, V, B>.Method f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod<T, U, V, B>(f));
	}

	// Token: 0x0400085A RID: 2138
	public static long Everybody;

	// Token: 0x0400085B RID: 2139
	public Action<long> m_onNewPeer;

	// Token: 0x0400085C RID: 2140
	private int m_rpcMsgID = 1;

	// Token: 0x0400085D RID: 2141
	private bool m_server;

	// Token: 0x0400085E RID: 2142
	private long m_id;

	// Token: 0x0400085F RID: 2143
	private List<ZNetPeer> m_peers = new List<ZNetPeer>();

	// Token: 0x04000860 RID: 2144
	private Dictionary<int, RoutedMethodBase> m_functions = new Dictionary<int, RoutedMethodBase>();

	// Token: 0x04000861 RID: 2145
	private static ZRoutedRpc m_instance;

	// Token: 0x02000170 RID: 368
	public class RoutedRPCData
	{
		// Token: 0x0600115F RID: 4447 RVA: 0x000785A4 File Offset: 0x000767A4
		public void Serialize(ZPackage pkg)
		{
			pkg.Write(this.m_msgID);
			pkg.Write(this.m_senderPeerID);
			pkg.Write(this.m_targetPeerID);
			pkg.Write(this.m_targetZDO);
			pkg.Write(this.m_methodHash);
			pkg.Write(this.m_parameters);
		}

		// Token: 0x06001160 RID: 4448 RVA: 0x000785FC File Offset: 0x000767FC
		public void Deserialize(ZPackage pkg)
		{
			this.m_msgID = pkg.ReadLong();
			this.m_senderPeerID = pkg.ReadLong();
			this.m_targetPeerID = pkg.ReadLong();
			this.m_targetZDO = pkg.ReadZDOID();
			this.m_methodHash = pkg.ReadInt();
			this.m_parameters = pkg.ReadPackage();
		}

		// Token: 0x0400117D RID: 4477
		public long m_msgID;

		// Token: 0x0400117E RID: 4478
		public long m_senderPeerID;

		// Token: 0x0400117F RID: 4479
		public long m_targetPeerID;

		// Token: 0x04001180 RID: 4480
		public ZDOID m_targetZDO;

		// Token: 0x04001181 RID: 4481
		public int m_methodHash;

		// Token: 0x04001182 RID: 4482
		public ZPackage m_parameters = new ZPackage();
	}
}
