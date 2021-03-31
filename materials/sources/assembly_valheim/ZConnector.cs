using System;
using System.Net;
using System.Net.Sockets;

// Token: 0x02000074 RID: 116
public class ZConnector : IDisposable
{
	// Token: 0x0600073F RID: 1855 RVA: 0x0003A7B8 File Offset: 0x000389B8
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

	// Token: 0x06000740 RID: 1856 RVA: 0x0003A81A File Offset: 0x00038A1A
	public void Dispose()
	{
		this.Close();
	}

	// Token: 0x06000741 RID: 1857 RVA: 0x0003A824 File Offset: 0x00038A24
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

	// Token: 0x06000742 RID: 1858 RVA: 0x0003A890 File Offset: 0x00038A90
	public bool IsPeer(string host, int port)
	{
		return this.m_host == host && this.m_port == port;
	}

	// Token: 0x06000743 RID: 1859 RVA: 0x0003A8AC File Offset: 0x00038AAC
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

	// Token: 0x06000744 RID: 1860 RVA: 0x0003A930 File Offset: 0x00038B30
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

	// Token: 0x06000745 RID: 1861 RVA: 0x0003A967 File Offset: 0x00038B67
	public bool CompareEndPoint(IPEndPoint endpoint)
	{
		return this.m_endPoint.Equals(endpoint);
	}

	// Token: 0x06000746 RID: 1862 RVA: 0x0003A978 File Offset: 0x00038B78
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

	// Token: 0x06000747 RID: 1863 RVA: 0x0003AA29 File Offset: 0x00038C29
	public string GetEndPointString()
	{
		return this.m_host + ":" + this.m_port;
	}

	// Token: 0x06000748 RID: 1864 RVA: 0x0003AA46 File Offset: 0x00038C46
	public string GetHostName()
	{
		return this.m_host;
	}

	// Token: 0x06000749 RID: 1865 RVA: 0x0003AA4E File Offset: 0x00038C4E
	public int GetHostPort()
	{
		return this.m_port;
	}

	// Token: 0x040007AC RID: 1964
	private Socket m_socket;

	// Token: 0x040007AD RID: 1965
	private IAsyncResult m_result;

	// Token: 0x040007AE RID: 1966
	private IPEndPoint m_endPoint;

	// Token: 0x040007AF RID: 1967
	private string m_host;

	// Token: 0x040007B0 RID: 1968
	private int m_port;

	// Token: 0x040007B1 RID: 1969
	private bool m_dnsError;

	// Token: 0x040007B2 RID: 1970
	private bool m_abort;

	// Token: 0x040007B3 RID: 1971
	private float m_timer;

	// Token: 0x040007B4 RID: 1972
	private static float m_timeout = 5f;
}
