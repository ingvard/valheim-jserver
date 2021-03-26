using System;
using System.Net;
using System.Net.Sockets;

// Token: 0x02000074 RID: 116
public class ZConnector : IDisposable
{
	// Token: 0x0600073E RID: 1854 RVA: 0x0003A704 File Offset: 0x00038904
	public ZConnector(string host, int port)
	{
		this.m_host = host;
		this.m_port = port;
		ZLog.Log(string.Concat(new object[]
		{
			"Zconnect ",
			host,
			" ",
			port
		}));
		Dns.BeginGetHostEntry(host, new AsyncCallback(this.OnHostLookupDone), null);
	}

	// Token: 0x0600073F RID: 1855 RVA: 0x0003A766 File Offset: 0x00038966
	public void Dispose()
	{
		this.Close();
	}

	// Token: 0x06000740 RID: 1856 RVA: 0x0003A770 File Offset: 0x00038970
	private void Close()
	{
		if (this.m_socket != null)
		{
			try
			{
				if (this.m_socket.Connected)
				{
					this.m_socket.Shutdown(SocketShutdown.Both);
				}
			}
			catch (Exception arg)
			{
				ZLog.Log("Some excepetion when shuting down ZConnector socket, ignoring:" + arg);
			}
			this.m_socket.Close();
			this.m_socket = null;
		}
		this.m_abort = true;
	}

	// Token: 0x06000741 RID: 1857 RVA: 0x0003A7DC File Offset: 0x000389DC
	public bool IsPeer(string host, int port)
	{
		return this.m_host == host && this.m_port == port;
	}

	// Token: 0x06000742 RID: 1858 RVA: 0x0003A7F8 File Offset: 0x000389F8
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
			ZLog.Log("ZConnector - result completed");
			return true;
		}
		this.m_timer += dt;
		if (this.m_timer > ZConnector.m_timeout)
		{
			ZLog.Log("ZConnector - timeout");
			this.Close();
			return true;
		}
		return false;
	}

	// Token: 0x06000743 RID: 1859 RVA: 0x0003A87C File Offset: 0x00038A7C
	public ZSocket Complete()
	{
		if (this.m_socket != null && this.m_socket.Connected)
		{
			ZSocket result = new ZSocket(this.m_socket, this.m_host);
			this.m_socket = null;
			return result;
		}
		this.Close();
		return null;
	}

	// Token: 0x06000744 RID: 1860 RVA: 0x0003A8B3 File Offset: 0x00038AB3
	public bool CompareEndPoint(IPEndPoint endpoint)
	{
		return this.m_endPoint.Equals(endpoint);
	}

	// Token: 0x06000745 RID: 1861 RVA: 0x0003A8C4 File Offset: 0x00038AC4
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
		ZLog.Log("Host lookup done , addresses: " + iphostEntry.AddressList.Length);
		foreach (IPAddress arg in iphostEntry.AddressList)
		{
			ZLog.Log(" " + arg);
		}
		this.m_socket = ZSocket.CreateSocket();
		this.m_result = this.m_socket.BeginConnect(iphostEntry.AddressList, this.m_port, null, null);
	}

	// Token: 0x06000746 RID: 1862 RVA: 0x0003A975 File Offset: 0x00038B75
	public string GetEndPointString()
	{
		return this.m_host + ":" + this.m_port;
	}

	// Token: 0x06000747 RID: 1863 RVA: 0x0003A992 File Offset: 0x00038B92
	public string GetHostName()
	{
		return this.m_host;
	}

	// Token: 0x06000748 RID: 1864 RVA: 0x0003A99A File Offset: 0x00038B9A
	public int GetHostPort()
	{
		return this.m_port;
	}

	// Token: 0x040007A8 RID: 1960
	private Socket m_socket;

	// Token: 0x040007A9 RID: 1961
	private IAsyncResult m_result;

	// Token: 0x040007AA RID: 1962
	private IPEndPoint m_endPoint;

	// Token: 0x040007AB RID: 1963
	private string m_host;

	// Token: 0x040007AC RID: 1964
	private int m_port;

	// Token: 0x040007AD RID: 1965
	private bool m_dnsError;

	// Token: 0x040007AE RID: 1966
	private bool m_abort;

	// Token: 0x040007AF RID: 1967
	private float m_timer;

	// Token: 0x040007B0 RID: 1968
	private static float m_timeout = 5f;
}
