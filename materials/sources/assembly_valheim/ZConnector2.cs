using System;
using System.Net;
using System.Net.Sockets;

// Token: 0x02000075 RID: 117
public class ZConnector2 : IDisposable
{
	// Token: 0x0600074A RID: 1866 RVA: 0x0003A9AE File Offset: 0x00038BAE
	public ZConnector2(string host, int port)
	{
		this.m_host = host;
		this.m_port = port;
		Dns.BeginGetHostEntry(host, new AsyncCallback(this.OnHostLookupDone), null);
	}

	// Token: 0x0600074B RID: 1867 RVA: 0x0003A9D8 File Offset: 0x00038BD8
	public void Dispose()
	{
		this.Close();
	}

	// Token: 0x0600074C RID: 1868 RVA: 0x0003A9E0 File Offset: 0x00038BE0
	private void Close()
	{
		if (this.m_socket != null)
		{
			this.m_socket.Close();
			this.m_socket = null;
		}
		this.m_abort = true;
	}

	// Token: 0x0600074D RID: 1869 RVA: 0x0003AA03 File Offset: 0x00038C03
	public bool IsPeer(string host, int port)
	{
		return this.m_host == host && this.m_port == port;
	}

	// Token: 0x0600074E RID: 1870 RVA: 0x0003AA20 File Offset: 0x00038C20
	public bool UpdateStatus(float dt, bool logErrors = false)
	{
		if (this.m_abort)
		{
			ZLog.Log("ZConnector - Abort");
			return true;
		}
		if (this.m_dnsError)
		{
			ZLog.Log("ZConnector - dns error");
			return true;
		}
		if (this.m_result != null && this.m_result.IsCompleted)
		{
			return true;
		}
		this.m_timer += dt;
		if (this.m_timer > ZConnector2.m_timeout)
		{
			this.Close();
			return true;
		}
		return false;
	}

	// Token: 0x0600074F RID: 1871 RVA: 0x0003AA90 File Offset: 0x00038C90
	public ZSocket2 Complete()
	{
		if (this.m_socket != null && this.m_socket.Connected)
		{
			ZSocket2 result = new ZSocket2(this.m_socket, this.m_host);
			this.m_socket = null;
			return result;
		}
		this.Close();
		return null;
	}

	// Token: 0x06000750 RID: 1872 RVA: 0x0003AAC7 File Offset: 0x00038CC7
	public bool CompareEndPoint(IPEndPoint endpoint)
	{
		return this.m_endPoint.Equals(endpoint);
	}

	// Token: 0x06000751 RID: 1873 RVA: 0x0003AAD8 File Offset: 0x00038CD8
	private void OnHostLookupDone(IAsyncResult res)
	{
		IPHostEntry iphostEntry = Dns.EndGetHostEntry(res);
		if (this.m_abort)
		{
			ZLog.Log("Host lookup abort");
			return;
		}
		if (iphostEntry.AddressList.Length == 0)
		{
			this.m_dnsError = true;
			ZLog.Log("Host lookup adress list empty");
			return;
		}
		this.m_socket = ZSocket2.CreateSocket();
		this.m_result = this.m_socket.BeginConnect(iphostEntry.AddressList, this.m_port, null, null);
	}

	// Token: 0x06000752 RID: 1874 RVA: 0x0003AB44 File Offset: 0x00038D44
	public string GetEndPointString()
	{
		return this.m_host + ":" + this.m_port;
	}

	// Token: 0x06000753 RID: 1875 RVA: 0x0003AB61 File Offset: 0x00038D61
	public string GetHostName()
	{
		return this.m_host;
	}

	// Token: 0x06000754 RID: 1876 RVA: 0x0003AB69 File Offset: 0x00038D69
	public int GetHostPort()
	{
		return this.m_port;
	}

	// Token: 0x040007B1 RID: 1969
	private TcpClient m_socket;

	// Token: 0x040007B2 RID: 1970
	private IAsyncResult m_result;

	// Token: 0x040007B3 RID: 1971
	private IPEndPoint m_endPoint;

	// Token: 0x040007B4 RID: 1972
	private string m_host;

	// Token: 0x040007B5 RID: 1973
	private int m_port;

	// Token: 0x040007B6 RID: 1974
	private bool m_dnsError;

	// Token: 0x040007B7 RID: 1975
	private bool m_abort;

	// Token: 0x040007B8 RID: 1976
	private float m_timer;

	// Token: 0x040007B9 RID: 1977
	private static float m_timeout = 5f;
}
