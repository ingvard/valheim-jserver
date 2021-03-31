using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

// Token: 0x0200008D RID: 141
public class ZSocket2 : IDisposable, ISocket
{
	// Token: 0x06000936 RID: 2358 RVA: 0x000443DD File Offset: 0x000425DD
	public ZSocket2()
	{
	}

	// Token: 0x06000937 RID: 2359 RVA: 0x0004441D File Offset: 0x0004261D
	public static TcpClient CreateSocket()
	{
		TcpClient tcpClient = new TcpClient(AddressFamily.InterNetwork);
		ZSocket2.ConfigureSocket(tcpClient);
		return tcpClient;
	}

	// Token: 0x06000938 RID: 2360 RVA: 0x0004442B File Offset: 0x0004262B
	private static void ConfigureSocket(TcpClient socket)
	{
		socket.NoDelay = true;
		socket.SendBufferSize = 2048;
	}

	// Token: 0x06000939 RID: 2361 RVA: 0x00044440 File Offset: 0x00042640
	public ZSocket2(TcpClient socket, string originalHostName = null)
	{
		this.m_socket = socket;
		this.m_originalHostName = originalHostName;
		try
		{
			this.m_endpoint = (this.m_socket.Client.RemoteEndPoint as IPEndPoint);
		}
		catch
		{
			this.Close();
			return;
		}
		this.BeginReceive();
	}

	// Token: 0x0600093A RID: 2362 RVA: 0x000444D8 File Offset: 0x000426D8
	public void Dispose()
	{
		this.Close();
		this.m_mutex.Close();
		this.m_sendMutex.Close();
		this.m_recvBuffer = null;
	}

	// Token: 0x0600093B RID: 2363 RVA: 0x00044500 File Offset: 0x00042700
	public void Close()
	{
		ZLog.Log("Closing socket " + this.GetEndPointString());
		if (this.m_listner != null)
		{
			this.m_listner.Stop();
			this.m_listner = null;
		}
		if (this.m_socket != null)
		{
			this.m_socket.Close();
			this.m_socket = null;
		}
		this.m_endpoint = null;
	}

	// Token: 0x0600093C RID: 2364 RVA: 0x00043BEC File Offset: 0x00041DEC
	public static IPEndPoint GetEndPoint(string host, int port)
	{
		return new IPEndPoint(Dns.GetHostEntry(host).AddressList[0], port);
	}

	// Token: 0x0600093D RID: 2365 RVA: 0x0004455D File Offset: 0x0004275D
	public bool StartHost(int port)
	{
		if (this.m_listner != null)
		{
			this.m_listner.Stop();
			this.m_listner = null;
		}
		if (!this.BindSocket(port, port + 10))
		{
			ZLog.LogWarning("Failed to bind socket");
			return false;
		}
		return true;
	}

	// Token: 0x0600093E RID: 2366 RVA: 0x00044594 File Offset: 0x00042794
	private bool BindSocket(int startPort, int endPort)
	{
		for (int i = startPort; i <= endPort; i++)
		{
			try
			{
				this.m_listner = new TcpListener(IPAddress.Any, i);
				this.m_listner.Start();
				this.m_listenPort = i;
				ZLog.Log("Bound socket port " + i);
				return true;
			}
			catch
			{
				ZLog.Log("Failed to bind port:" + i);
				this.m_listner = null;
			}
		}
		return false;
	}

	// Token: 0x0600093F RID: 2367 RVA: 0x0004461C File Offset: 0x0004281C
	private void BeginReceive()
	{
		this.m_recvSizeOffset = 0;
		this.m_socket.GetStream().BeginRead(this.m_recvSizeBuffer, 0, this.m_recvSizeBuffer.Length, new AsyncCallback(this.PkgSizeReceived), this.m_socket);
	}

	// Token: 0x06000940 RID: 2368 RVA: 0x00044658 File Offset: 0x00042858
	private void PkgSizeReceived(IAsyncResult res)
	{
		if (this.m_socket == null || !this.m_socket.Connected)
		{
			ZLog.LogWarning("PkgSizeReceived socket closed");
			this.Close();
			return;
		}
		int num;
		try
		{
			num = this.m_socket.GetStream().EndRead(res);
		}
		catch (Exception ex)
		{
			ZLog.LogWarning("PkgSizeReceived exception " + ex.ToString());
			this.Close();
			return;
		}
		if (num == 0)
		{
			ZLog.LogWarning("PkgSizeReceived Got 0 bytes data,closing socket");
			this.Close();
			return;
		}
		this.m_gotData = true;
		this.m_recvSizeOffset += num;
		if (this.m_recvSizeOffset < this.m_recvSizeBuffer.Length)
		{
			int count = this.m_recvSizeBuffer.Length - this.m_recvOffset;
			this.m_socket.GetStream().BeginRead(this.m_recvSizeBuffer, this.m_recvSizeOffset, count, new AsyncCallback(this.PkgSizeReceived), this.m_socket);
			return;
		}
		int num2 = BitConverter.ToInt32(this.m_recvSizeBuffer, 0);
		if (num2 == 0 || num2 > 10485760)
		{
			ZLog.LogError("PkgSizeReceived Invalid pkg size " + num2);
			return;
		}
		this.m_lastRecvPkgSize = num2;
		this.m_recvOffset = 0;
		this.m_lastRecvPkgSize = num2;
		if (this.m_recvBuffer == null)
		{
			this.m_recvBuffer = new byte[ZSocket2.m_maxRecvBuffer];
		}
		this.m_socket.GetStream().BeginRead(this.m_recvBuffer, this.m_recvOffset, this.m_lastRecvPkgSize, new AsyncCallback(this.PkgReceived), this.m_socket);
	}

	// Token: 0x06000941 RID: 2369 RVA: 0x000447DC File Offset: 0x000429DC
	private void PkgReceived(IAsyncResult res)
	{
		int num;
		try
		{
			num = this.m_socket.GetStream().EndRead(res);
		}
		catch (Exception ex)
		{
			ZLog.Log("PkgReceived error " + ex.ToString());
			this.Close();
			return;
		}
		if (num == 0)
		{
			ZLog.LogWarning("PkgReceived: Got 0 bytes data,closing socket");
			this.Close();
			return;
		}
		this.m_gotData = true;
		this.m_totalRecv += num;
		this.m_recvOffset += num;
		if (this.m_recvOffset < this.m_lastRecvPkgSize)
		{
			int count = this.m_lastRecvPkgSize - this.m_recvOffset;
			if (this.m_recvBuffer == null)
			{
				this.m_recvBuffer = new byte[ZSocket2.m_maxRecvBuffer];
			}
			this.m_socket.GetStream().BeginRead(this.m_recvBuffer, this.m_recvOffset, count, new AsyncCallback(this.PkgReceived), this.m_socket);
			return;
		}
		ZPackage item = new ZPackage(this.m_recvBuffer, this.m_lastRecvPkgSize);
		this.m_mutex.WaitOne();
		this.m_pkgQueue.Enqueue(item);
		this.m_mutex.ReleaseMutex();
		this.BeginReceive();
	}

	// Token: 0x06000942 RID: 2370 RVA: 0x00044908 File Offset: 0x00042B08
	public ISocket Accept()
	{
		if (this.m_listner == null)
		{
			return null;
		}
		if (!this.m_listner.Pending())
		{
			return null;
		}
		TcpClient socket = this.m_listner.AcceptTcpClient();
		ZSocket2.ConfigureSocket(socket);
		return new ZSocket2(socket, null);
	}

	// Token: 0x06000943 RID: 2371 RVA: 0x0004493A File Offset: 0x00042B3A
	public bool IsConnected()
	{
		return this.m_socket != null && this.m_socket.Connected;
	}

	// Token: 0x06000944 RID: 2372 RVA: 0x00044954 File Offset: 0x00042B54
	public void Send(ZPackage pkg)
	{
		if (pkg.Size() == 0)
		{
			return;
		}
		if (this.m_socket == null || !this.m_socket.Connected)
		{
			return;
		}
		byte[] array = pkg.GetArray();
		byte[] bytes = BitConverter.GetBytes(array.Length);
		byte[] array2 = new byte[array.Length + bytes.Length];
		bytes.CopyTo(array2, 0);
		array.CopyTo(array2, 4);
		this.m_sendMutex.WaitOne();
		if (!this.m_isSending)
		{
			if (array2.Length > 10485760)
			{
				ZLog.LogError("Too big data package: " + array2.Length);
			}
			try
			{
				this.m_totalSent += array2.Length;
				this.m_socket.GetStream().BeginWrite(array2, 0, array2.Length, new AsyncCallback(this.PkgSent), this.m_socket);
				this.m_isSending = true;
				goto IL_E6;
			}
			catch (Exception arg)
			{
				ZLog.Log("Handled exception in ZSocket:Send:" + arg);
				this.Close();
				goto IL_E6;
			}
		}
		this.m_sendQueue.Enqueue(array2);
		IL_E6:
		this.m_sendMutex.ReleaseMutex();
	}

	// Token: 0x06000945 RID: 2373 RVA: 0x00044A64 File Offset: 0x00042C64
	private void PkgSent(IAsyncResult res)
	{
		try
		{
			this.m_socket.GetStream().EndWrite(res);
		}
		catch (Exception ex)
		{
			ZLog.Log("PkgSent error " + ex.ToString());
			this.Close();
			return;
		}
		this.m_sendMutex.WaitOne();
		if (this.m_sendQueue.Count > 0 && this.IsConnected())
		{
			byte[] array = this.m_sendQueue.Dequeue();
			try
			{
				this.m_totalSent += array.Length;
				this.m_socket.GetStream().BeginWrite(array, 0, array.Length, new AsyncCallback(this.PkgSent), this.m_socket);
				goto IL_C3;
			}
			catch (Exception arg)
			{
				ZLog.Log("Handled exception in pkgsent:" + arg);
				this.m_isSending = false;
				this.Close();
				goto IL_C3;
			}
		}
		this.m_isSending = false;
		IL_C3:
		this.m_sendMutex.ReleaseMutex();
	}

	// Token: 0x06000946 RID: 2374 RVA: 0x00044B5C File Offset: 0x00042D5C
	public ZPackage Recv()
	{
		if (this.m_socket == null)
		{
			return null;
		}
		if (this.m_pkgQueue.Count == 0)
		{
			return null;
		}
		ZPackage result = null;
		this.m_mutex.WaitOne();
		if (this.m_pkgQueue.Count > 0)
		{
			result = this.m_pkgQueue.Dequeue();
		}
		this.m_mutex.ReleaseMutex();
		return result;
	}

	// Token: 0x06000947 RID: 2375 RVA: 0x00044BB6 File Offset: 0x00042DB6
	public string GetEndPointString()
	{
		if (this.m_endpoint != null)
		{
			return this.m_endpoint.ToString();
		}
		return "None";
	}

	// Token: 0x06000948 RID: 2376 RVA: 0x00044BD1 File Offset: 0x00042DD1
	public string GetHostName()
	{
		if (this.m_endpoint != null)
		{
			return this.m_endpoint.Address.ToString();
		}
		return "None";
	}

	// Token: 0x06000949 RID: 2377 RVA: 0x00044BF1 File Offset: 0x00042DF1
	public IPEndPoint GetEndPoint()
	{
		return this.m_endpoint;
	}

	// Token: 0x0600094A RID: 2378 RVA: 0x00044BFC File Offset: 0x00042DFC
	public bool IsPeer(string host, int port)
	{
		if (!this.IsConnected())
		{
			return false;
		}
		if (this.m_endpoint == null)
		{
			return false;
		}
		IPEndPoint endpoint = this.m_endpoint;
		return (endpoint.Address.ToString() == host && endpoint.Port == port) || (this.m_originalHostName != null && this.m_originalHostName == host && endpoint.Port == port);
	}

	// Token: 0x0600094B RID: 2379 RVA: 0x00044C64 File Offset: 0x00042E64
	public bool IsHost()
	{
		return this.m_listenPort != 0;
	}

	// Token: 0x0600094C RID: 2380 RVA: 0x00044C6F File Offset: 0x00042E6F
	public int GetHostPort()
	{
		return this.m_listenPort;
	}

	// Token: 0x0600094D RID: 2381 RVA: 0x00044C78 File Offset: 0x00042E78
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

	// Token: 0x0600094E RID: 2382 RVA: 0x00044CD8 File Offset: 0x00042ED8
	public bool IsSending()
	{
		return this.m_isSending || this.m_sendQueue.Count > 0;
	}

	// Token: 0x0600094F RID: 2383 RVA: 0x00044CF2 File Offset: 0x00042EF2
	public void GetConnectionQuality(out float localQuality, out float remoteQuality, out int ping, out float outByteSec, out float inByteSec)
	{
		localQuality = 0f;
		remoteQuality = 0f;
		ping = 0;
		outByteSec = 0f;
		inByteSec = 0f;
	}

	// Token: 0x06000950 RID: 2384 RVA: 0x00044D15 File Offset: 0x00042F15
	public void GetAndResetStats(out int totalSent, out int totalRecv)
	{
		totalSent = this.m_totalSent;
		totalRecv = this.m_totalRecv;
		this.m_totalSent = 0;
		this.m_totalRecv = 0;
	}

	// Token: 0x06000951 RID: 2385 RVA: 0x00044D35 File Offset: 0x00042F35
	public bool GotNewData()
	{
		bool gotData = this.m_gotData;
		this.m_gotData = false;
		return gotData;
	}

	// Token: 0x06000952 RID: 2386 RVA: 0x000027E2 File Offset: 0x000009E2
	public bool Flush()
	{
		return true;
	}

	// Token: 0x06000953 RID: 2387 RVA: 0x000023E2 File Offset: 0x000005E2
	public int GetCurrentSendRate()
	{
		return 0;
	}

	// Token: 0x06000954 RID: 2388 RVA: 0x000023E2 File Offset: 0x000005E2
	public int GetAverageSendRate()
	{
		return 0;
	}

	// Token: 0x04000883 RID: 2179
	private TcpListener m_listner;

	// Token: 0x04000884 RID: 2180
	private TcpClient m_socket;

	// Token: 0x04000885 RID: 2181
	private Mutex m_mutex = new Mutex();

	// Token: 0x04000886 RID: 2182
	private Mutex m_sendMutex = new Mutex();

	// Token: 0x04000887 RID: 2183
	private static int m_maxRecvBuffer = 10485760;

	// Token: 0x04000888 RID: 2184
	private int m_recvOffset;

	// Token: 0x04000889 RID: 2185
	private byte[] m_recvBuffer;

	// Token: 0x0400088A RID: 2186
	private int m_recvSizeOffset;

	// Token: 0x0400088B RID: 2187
	private byte[] m_recvSizeBuffer = new byte[4];

	// Token: 0x0400088C RID: 2188
	private Queue<ZPackage> m_pkgQueue = new Queue<ZPackage>();

	// Token: 0x0400088D RID: 2189
	private bool m_isSending;

	// Token: 0x0400088E RID: 2190
	private Queue<byte[]> m_sendQueue = new Queue<byte[]>();

	// Token: 0x0400088F RID: 2191
	private IPEndPoint m_endpoint;

	// Token: 0x04000890 RID: 2192
	private string m_originalHostName;

	// Token: 0x04000891 RID: 2193
	private int m_listenPort;

	// Token: 0x04000892 RID: 2194
	private int m_lastRecvPkgSize;

	// Token: 0x04000893 RID: 2195
	private int m_totalSent;

	// Token: 0x04000894 RID: 2196
	private int m_totalRecv;

	// Token: 0x04000895 RID: 2197
	private bool m_gotData;
}
