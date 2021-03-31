using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000072 RID: 114
public class MasterClient
{
	// Token: 0x17000014 RID: 20
	// (get) Token: 0x06000724 RID: 1828 RVA: 0x00039DF2 File Offset: 0x00037FF2
	public static MasterClient instance
	{
		get
		{
			return MasterClient.m_instance;
		}
	}

	// Token: 0x06000725 RID: 1829 RVA: 0x00039DF9 File Offset: 0x00037FF9
	public static void Initialize()
	{
		if (MasterClient.m_instance == null)
		{
			MasterClient.m_instance = new MasterClient();
		}
	}

	// Token: 0x06000726 RID: 1830 RVA: 0x00039E0C File Offset: 0x0003800C
	public MasterClient()
	{
		this.m_sessionUID = Utils.GenerateUID();
	}

	// Token: 0x06000727 RID: 1831 RVA: 0x00039E4C File Offset: 0x0003804C
	public void Dispose()
	{
		if (this.m_socket != null)
		{
			this.m_socket.Dispose();
		}
		if (this.m_connector != null)
		{
			this.m_connector.Dispose();
		}
		if (this.m_rpc != null)
		{
			this.m_rpc.Dispose();
		}
		if (MasterClient.m_instance == this)
		{
			MasterClient.m_instance = null;
		}
	}

	// Token: 0x06000728 RID: 1832 RVA: 0x00039EA0 File Offset: 0x000380A0
	public void Update(float dt)
	{
		if (this.m_rpc == null)
		{
			if (this.m_connector == null)
			{
				this.m_connector = new ZConnector2(this.m_msHost, this.m_msPort);
				return;
			}
			if (this.m_connector.UpdateStatus(dt, false))
			{
				this.m_socket = this.m_connector.Complete();
				if (this.m_socket != null)
				{
					this.m_rpc = new ZRpc(this.m_socket);
					this.m_rpc.Register<ZPackage>("ServerList", new Action<ZRpc, ZPackage>(this.RPC_ServerList));
					if (this.m_registerPkg != null)
					{
						this.m_rpc.Invoke("RegisterServer2", new object[]
						{
							this.m_registerPkg
						});
					}
				}
				this.m_connector.Dispose();
				this.m_connector = null;
			}
		}
		ZRpc rpc = this.m_rpc;
		if (rpc != null)
		{
			rpc.Update(dt);
			if (!rpc.IsConnected())
			{
				this.m_rpc.Dispose();
				this.m_rpc = null;
			}
		}
		if (this.m_rpc != null)
		{
			this.m_sendStatsTimer += dt;
			if (this.m_sendStatsTimer > 60f)
			{
				this.m_sendStatsTimer = 0f;
				this.SendStats(60f);
			}
		}
	}

	// Token: 0x06000729 RID: 1833 RVA: 0x00039FCC File Offset: 0x000381CC
	private void SendStats(float duration)
	{
		ZPackage zpackage = new ZPackage();
		zpackage.Write(2);
		zpackage.Write(this.m_sessionUID);
		zpackage.Write(Time.time);
		bool flag = Player.m_localPlayer != null;
		zpackage.Write(flag ? duration : 0f);
		bool flag2 = ZNet.instance && !ZNet.instance.IsServer();
		zpackage.Write(flag2 ? duration : 0f);
		zpackage.Write(global::Version.GetVersionString());
		bool flag3 = ZNet.instance && ZNet.instance.IsServer();
		zpackage.Write(flag3);
		if (flag3)
		{
			zpackage.Write(ZNet.instance.GetWorldUID());
			zpackage.Write(duration);
			int num = ZNet.instance.GetPeerConnections();
			if (Player.m_localPlayer != null)
			{
				num++;
			}
			zpackage.Write(num);
			bool data = ZNet.instance.GetZNat() != null && ZNet.instance.GetZNat().GetStatus();
			zpackage.Write(data);
		}
		PlayerProfile playerProfile = (Game.instance != null) ? Game.instance.GetPlayerProfile() : null;
		if (playerProfile != null)
		{
			zpackage.Write(true);
			zpackage.Write(playerProfile.GetPlayerID());
			zpackage.Write(playerProfile.m_playerStats.m_kills);
			zpackage.Write(playerProfile.m_playerStats.m_deaths);
			zpackage.Write(playerProfile.m_playerStats.m_crafts);
			zpackage.Write(playerProfile.m_playerStats.m_builds);
		}
		else
		{
			zpackage.Write(false);
		}
		this.m_rpc.Invoke("Stats", new object[]
		{
			zpackage
		});
	}

	// Token: 0x0600072A RID: 1834 RVA: 0x0003A17C File Offset: 0x0003837C
	public void RegisterServer(string name, string host, int port, bool password, bool upnp, long worldUID, string version)
	{
		this.m_registerPkg = new ZPackage();
		this.m_registerPkg.Write(1);
		this.m_registerPkg.Write(name);
		this.m_registerPkg.Write(host);
		this.m_registerPkg.Write(port);
		this.m_registerPkg.Write(password);
		this.m_registerPkg.Write(upnp);
		this.m_registerPkg.Write(worldUID);
		this.m_registerPkg.Write(version);
		if (this.m_rpc != null)
		{
			this.m_rpc.Invoke("RegisterServer2", new object[]
			{
				this.m_registerPkg
			});
		}
		ZLog.Log(string.Concat(new object[]
		{
			"Registering server ",
			name,
			"  ",
			host,
			":",
			port
		}));
	}

	// Token: 0x0600072B RID: 1835 RVA: 0x0003A258 File Offset: 0x00038458
	public void UnregisterServer()
	{
		if (this.m_registerPkg == null)
		{
			return;
		}
		if (this.m_rpc != null)
		{
			this.m_rpc.Invoke("UnregisterServer", Array.Empty<object>());
		}
		this.m_registerPkg = null;
	}

	// Token: 0x0600072C RID: 1836 RVA: 0x0003A287 File Offset: 0x00038487
	public List<ServerData> GetServers()
	{
		return this.m_servers;
	}

	// Token: 0x0600072D RID: 1837 RVA: 0x0003A28F File Offset: 0x0003848F
	public bool GetServers(List<ServerData> servers)
	{
		if (!this.m_haveServerlist)
		{
			return false;
		}
		servers.Clear();
		servers.AddRange(this.m_servers);
		return true;
	}

	// Token: 0x0600072E RID: 1838 RVA: 0x0003A2AE File Offset: 0x000384AE
	public void RequestServerlist()
	{
		if (this.m_rpc != null)
		{
			this.m_rpc.Invoke("RequestServerlist2", Array.Empty<object>());
		}
	}

	// Token: 0x0600072F RID: 1839 RVA: 0x0003A2D0 File Offset: 0x000384D0
	private void RPC_ServerList(ZRpc rpc, ZPackage pkg)
	{
		this.m_haveServerlist = true;
		this.m_serverListRevision++;
		pkg.ReadInt();
		int num = pkg.ReadInt();
		this.m_servers.Clear();
		for (int i = 0; i < num; i++)
		{
			ServerData serverData = new ServerData();
			serverData.m_name = pkg.ReadString();
			serverData.m_host = pkg.ReadString();
			serverData.m_port = pkg.ReadInt();
			serverData.m_password = pkg.ReadBool();
			serverData.m_upnp = pkg.ReadBool();
			pkg.ReadLong();
			serverData.m_version = pkg.ReadString();
			serverData.m_players = pkg.ReadInt();
			if (this.m_nameFilter.Length <= 0 || serverData.m_name.Contains(this.m_nameFilter))
			{
				this.m_servers.Add(serverData);
			}
		}
		if (this.m_onServerList != null)
		{
			this.m_onServerList(this.m_servers);
		}
	}

	// Token: 0x06000730 RID: 1840 RVA: 0x0003A3C4 File Offset: 0x000385C4
	public int GetServerListRevision()
	{
		return this.m_serverListRevision;
	}

	// Token: 0x06000731 RID: 1841 RVA: 0x0003A3CC File Offset: 0x000385CC
	public bool IsConnected()
	{
		return this.m_rpc != null;
	}

	// Token: 0x06000732 RID: 1842 RVA: 0x0003A3D7 File Offset: 0x000385D7
	public void SetNameFilter(string filter)
	{
		this.m_nameFilter = filter;
		ZLog.Log("filter is " + filter);
	}

	// Token: 0x04000793 RID: 1939
	private const int statVersion = 2;

	// Token: 0x04000794 RID: 1940
	public Action<List<ServerData>> m_onServerList;

	// Token: 0x04000795 RID: 1941
	private string m_msHost = "dvoid.noip.me";

	// Token: 0x04000796 RID: 1942
	private int m_msPort = 9983;

	// Token: 0x04000797 RID: 1943
	private long m_sessionUID;

	// Token: 0x04000798 RID: 1944
	private ZConnector2 m_connector;

	// Token: 0x04000799 RID: 1945
	private ZSocket2 m_socket;

	// Token: 0x0400079A RID: 1946
	private ZRpc m_rpc;

	// Token: 0x0400079B RID: 1947
	private bool m_haveServerlist;

	// Token: 0x0400079C RID: 1948
	private List<ServerData> m_servers = new List<ServerData>();

	// Token: 0x0400079D RID: 1949
	private ZPackage m_registerPkg;

	// Token: 0x0400079E RID: 1950
	private float m_sendStatsTimer;

	// Token: 0x0400079F RID: 1951
	private int m_serverListRevision;

	// Token: 0x040007A0 RID: 1952
	private string m_nameFilter = "";

	// Token: 0x040007A1 RID: 1953
	private static MasterClient m_instance;
}
