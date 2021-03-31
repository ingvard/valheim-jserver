using System;
using Steamworks;

// Token: 0x0200007C RID: 124
public class ServerData
{
	// Token: 0x06000803 RID: 2051 RVA: 0x0003EB64 File Offset: 0x0003CD64
	public override bool Equals(object obj)
	{
		ServerData serverData = obj as ServerData;
		return serverData != null && serverData.m_steamHostID == this.m_steamHostID && serverData.m_steamHostAddr.Equals(this.m_steamHostAddr);
	}

	// Token: 0x06000804 RID: 2052 RVA: 0x0003EBA0 File Offset: 0x0003CDA0
	public override string ToString()
	{
		if (this.m_steamHostID != 0UL)
		{
			return this.m_steamHostID.ToString();
		}
		string result;
		this.m_steamHostAddr.ToString(out result, true);
		return result;
	}

	// Token: 0x040007FC RID: 2044
	public string m_name;

	// Token: 0x040007FD RID: 2045
	public string m_host;

	// Token: 0x040007FE RID: 2046
	public int m_port;

	// Token: 0x040007FF RID: 2047
	public bool m_password;

	// Token: 0x04000800 RID: 2048
	public bool m_upnp;

	// Token: 0x04000801 RID: 2049
	public string m_version;

	// Token: 0x04000802 RID: 2050
	public int m_players;

	// Token: 0x04000803 RID: 2051
	public ulong m_steamHostID;

	// Token: 0x04000804 RID: 2052
	public SteamNetworkingIPAddr m_steamHostAddr;
}
