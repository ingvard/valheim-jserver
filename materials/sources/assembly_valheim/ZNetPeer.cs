using System;
using UnityEngine;

// Token: 0x0200007D RID: 125
public class ZNetPeer : IDisposable
{
	// Token: 0x06000805 RID: 2053 RVA: 0x0003EB1C File Offset: 0x0003CD1C
	public ZNetPeer(ISocket socket, bool server)
	{
		this.m_socket = socket;
		this.m_rpc = new ZRpc(this.m_socket);
		this.m_server = server;
	}

	// Token: 0x06000806 RID: 2054 RVA: 0x0003EB6F File Offset: 0x0003CD6F
	public void Dispose()
	{
		this.m_socket.Dispose();
		this.m_rpc.Dispose();
	}

	// Token: 0x06000807 RID: 2055 RVA: 0x0003EB87 File Offset: 0x0003CD87
	public bool IsReady()
	{
		return this.m_uid != 0L;
	}

	// Token: 0x06000808 RID: 2056 RVA: 0x0003EB93 File Offset: 0x0003CD93
	public Vector3 GetRefPos()
	{
		return this.m_refPos;
	}

	// Token: 0x04000801 RID: 2049
	public ZRpc m_rpc;

	// Token: 0x04000802 RID: 2050
	public ISocket m_socket;

	// Token: 0x04000803 RID: 2051
	public long m_uid;

	// Token: 0x04000804 RID: 2052
	public bool m_server;

	// Token: 0x04000805 RID: 2053
	public Vector3 m_refPos = Vector3.zero;

	// Token: 0x04000806 RID: 2054
	public bool m_publicRefPos;

	// Token: 0x04000807 RID: 2055
	public ZDOID m_characterID = ZDOID.None;

	// Token: 0x04000808 RID: 2056
	public string m_playerName = "";
}
