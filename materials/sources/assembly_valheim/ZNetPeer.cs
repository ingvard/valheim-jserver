using System;
using UnityEngine;

// Token: 0x0200007D RID: 125
public class ZNetPeer : IDisposable
{
	// Token: 0x06000806 RID: 2054 RVA: 0x0003EBD0 File Offset: 0x0003CDD0
	public ZNetPeer(ISocket socket, bool server)
	{
		this.m_socket = socket;
		this.m_rpc = new ZRpc(this.m_socket);
		this.m_server = server;
	}

	// Token: 0x06000807 RID: 2055 RVA: 0x0003EC23 File Offset: 0x0003CE23
	public void Dispose()
	{
		this.m_socket.Dispose();
		this.m_rpc.Dispose();
	}

	// Token: 0x06000808 RID: 2056 RVA: 0x0003EC3B File Offset: 0x0003CE3B
	public bool IsReady()
	{
		return this.m_uid != 0L;
	}

	// Token: 0x06000809 RID: 2057 RVA: 0x0003EC47 File Offset: 0x0003CE47
	public Vector3 GetRefPos()
	{
		return this.m_refPos;
	}

	// Token: 0x04000805 RID: 2053
	public ZRpc m_rpc;

	// Token: 0x04000806 RID: 2054
	public ISocket m_socket;

	// Token: 0x04000807 RID: 2055
	public long m_uid;

	// Token: 0x04000808 RID: 2056
	public bool m_server;

	// Token: 0x04000809 RID: 2057
	public Vector3 m_refPos = Vector3.zero;

	// Token: 0x0400080A RID: 2058
	public bool m_publicRefPos;

	// Token: 0x0400080B RID: 2059
	public ZDOID m_characterID = ZDOID.None;

	// Token: 0x0400080C RID: 2060
	public string m_playerName = "";
}
