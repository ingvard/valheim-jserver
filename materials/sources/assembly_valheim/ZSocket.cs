using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

// Token: 0x0200008C RID: 140
public class ZSocket : IDisposable
{
	// Token: 0x06000919 RID: 2329 RVA: 0x000439A4 File Offset: 0x00041BA4
	public ZSocket()
	{
		this.m_socket = ZSocket.CreateSocket();
	}

	// Token: 0x0600091A RID: 2330 RVA: 0x00043A05 File Offset: 0x00041C05
	public static Socket CreateSocket()
	{
		return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
		{
			NoDelay = true
		};
	}

	// Token: 0x0600091B RID: 2331 RVA: 0x00043A18 File Offset: 0x00041C18
	public ZSocket(Socket socket, string originalHostName = null)
	{
		this.m_socket = socket;
		this.m_originalHostName = originalHostName;
		try
		{
			this.m_endpoint = (this.m_socket.RemoteEndPoint as IPEndPoint);
		}
		catch
		{
			this.Close();
			return;
		}
		this.BeginReceive();
	}

	// Token: 0x0600091C RID: 2332 RVA: 0x00043AB4 File Offset: 0x00041CB4
	public void Dispose()
	{
		this.Close();
		this.m_mutex.Close();
		this.m_sendMutex.Close();
		this.m_recvBuffer = null;
	}

	// Token: 0x0600091D RID: 2333 RVA: 0x00043ADC File Offset: 0x00041CDC
	public void Close()
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
			catch (Exception)
			{
			}
			this.m_socket.Close();
		}
		this.m_socket = null;
		this.m_endpoint = null;
	}

	// Token: 0x0600091E RID: 2334 RVA: 0x00043B38 File Offset: 0x00041D38
	public static IPEndPoint GetEndPoint(string host, int port)
	{
		return new IPEndPoint(Dns.GetHostEntry(host).AddressList[0], port);
	}

	// Token: 0x0600091F RID: 2335 RVA: 0x00043B50 File Offset: 0x00041D50
	public bool Connect(string host, int port)
	{
		ZLog.Log(string.Concat(new object[]
		{
			"Connecting to ",
			host,
			" : ",
			port
		}));
		IPEndPoint endPoint = ZSocket.GetEndPoint(host, port);
		this.m_socket.BeginConnect(endPoint, null, null).AsyncWaitHandle.WaitOne(3000, true);
		if (!this.m_socket.Connected)
		{
			return false;
		}
		try
		{
			this.m_endpoint = (this.m_socket.RemoteEndPoint as IPEndPoint);
		}
		catch
		{
			this.Close();
			return false;
		}
		this.BeginReceive();
		ZLog.Log(" connected");
		return true;
	}

	// Token: 0x06000920 RID: 2336 RVA: 0x00043C08 File Offset: 0x00041E08
	public bool StartHost(int port)
	{
		if (this.m_listenPort != 0)
		{
			this.Close();
		}
		if (!this.BindSocket(this.m_socket, IPAddress.Any, port, port + 10))
		{
			ZLog.LogWarning("Failed to bind socket");
			return false;
		}
		this.m_socket.Listen(100);
		this.m_socket.BeginAccept(new AsyncCallback(this.AcceptCallback), this.m_socket);
		return true;
	}

	// Token: 0x06000921 RID: 2337 RVA: 0x00043C74 File Offset: 0x00041E74
	private bool BindSocket(Socket socket, IPAddress ipAddress, int startPort, int endPort)
	{
		for (int i = startPort; i <= endPort; i++)
		{
			try
			{
				IPEndPoint localEP = new IPEndPoint(ipAddress, i);
				this.m_socket.Bind(localEP);
				this.m_listenPort = i;
				ZLog.Log("Bound socket port " + i);
				return true;
			}
			catch
			{
				ZLog.Log("Failed to bind port:" + i);
			}
		}
		return false;
	}

	// Token: 0x06000922 RID: 2338 RVA: 0x00043CF0 File Offset: 0x00041EF0
	private void BeginReceive()
	{
		this.m_socket.BeginReceive(this.m_recvSizeBuffer, 0, this.m_recvSizeBuffer.Length, SocketFlags.None, new AsyncCallback(this.PkgSizeReceived), this.m_socket);
	}

	// Token: 0x06000923 RID: 2339 RVA: 0x00043D20 File Offset: 0x00041F20
	private void PkgSizeReceived(IAsyncResult res)
	{
		int num;
		try
		{
			num = this.m_socket.EndReceive(res);
		}
		catch (Exception)
		{
			this.Disconnect();
			return;
		}
		this.m_totalRecv += num;
		if (num != 4)
		{
			this.Disconnect();
			return;
		}
		int num2 = BitConverter.ToInt32(this.m_recvSizeBuffer, 0);
		if (num2 == 0 || num2 > 10485760)
		{
			ZLog.LogError("Invalid pkg size " + num2);
			return;
		}
		this.m_lastRecvPkgSize = num2;
		this.m_recvOffset = 0;
		this.m_lastRecvPkgSize = num2;
		if (this.m_recvBuffer == null)
		{
			this.m_recvBuffer = new byte[ZSocket.m_maxRecvBuffer];
		}
		this.m_socket.BeginReceive(this.m_recvBuffer, this.m_recvOffset, this.m_lastRecvPkgSize, SocketFlags.None, new AsyncCallback(this.PkgReceived), this.m_socket);
	}

	// Token: 0x06000924 RID: 2340 RVA: 0x00043E00 File Offset: 0x00042000
	private void Disconnect()
	{
		if (this.m_socket != null)
		{
			try
			{
				this.m_socket.Disconnect(true);
			}
			catch
			{
			}
		}
	}

	// Token: 0x06000925 RID: 2341 RVA: 0x00043E38 File Offset: 0x00042038
	private void PkgReceived(IAsyncResult res)
	{
		int num;
		try
		{
			num = this.m_socket.EndReceive(res);
		}
		catch (Exception)
		{
			this.Disconnect();
			return;
		}
		this.m_totalRecv += num;
		this.m_recvOffset += num;
		if (this.m_recvOffset < this.m_lastRecvPkgSize)
		{
			int size = this.m_lastRecvPkgSize - this.m_recvOffset;
			if (this.m_recvBuffer == null)
			{
				this.m_recvBuffer = new byte[ZSocket.m_maxRecvBuffer];
			}
			this.m_socket.BeginReceive(this.m_recvBuffer, this.m_recvOffset, size, SocketFlags.None, new AsyncCallback(this.PkgReceived), this.m_socket);
			return;
		}
		ZPackage item = new ZPackage(this.m_recvBuffer, this.m_lastRecvPkgSize);
		this.m_mutex.WaitOne();
		this.m_pkgQueue.Enqueue(item);
		this.m_mutex.ReleaseMutex();
		this.BeginReceive();
	}

	// Token: 0x06000926 RID: 2342 RVA: 0x00043F28 File Offset: 0x00042128
	private void AcceptCallback(IAsyncResult res)
	{
		Socket item;
		try
		{
			item = this.m_socket.EndAccept(res);
		}
		catch
		{
			this.Disconnect();
			return;
		}
		this.m_mutex.WaitOne();
		this.m_newConnections.Enqueue(item);
		this.m_mutex.ReleaseMutex();
		this.m_socket.BeginAccept(new AsyncCallback(this.AcceptCallback), this.m_socket);
	}

	// Token: 0x06000927 RID: 2343 RVA: 0x00043FA0 File Offset: 0x000421A0
	public ZSocket Accept()
	{
		if (this.m_newConnections.Count == 0)
		{
			return null;
		}
		Socket socket = null;
		this.m_mutex.WaitOne();
		if (this.m_newConnections.Count > 0)
		{
			socket = this.m_newConnections.Dequeue();
		}
		this.m_mutex.ReleaseMutex();
		if (socket != null)
		{
			return new ZSocket(socket, null);
		}
		return null;
	}

	// Token: 0x06000928 RID: 2344 RVA: 0x00043FFB File Offset: 0x000421FB
	public bool IsConnected()
	{
		return this.m_socket != null && this.m_socket.Connected;
	}

	// Token: 0x06000929 RID: 2345 RVA: 0x00044014 File Offset: 0x00042214
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
		this.m_sendMutex.WaitOne();
		if (!this.m_isSending)
		{
			if (array.Length > 10485760)
			{
				ZLog.LogError("Too big data package: " + array.Length);
			}
			try
			{
				this.m_totalSent += bytes.Length;
				this.m_socket.BeginSend(bytes, 0, bytes.Length, SocketFlags.None, new AsyncCallback(this.PkgSent), null);
				this.m_isSending = true;
				this.m_sendQueue.Enqueue(array);
				goto IL_DA;
			}
			catch (Exception arg)
			{
				ZLog.Log("Handled exception in ZSocket:Send:" + arg);
				this.Disconnect();
				goto IL_DA;
			}
		}
		this.m_sendQueue.Enqueue(bytes);
		this.m_sendQueue.Enqueue(array);
		IL_DA:
		this.m_sendMutex.ReleaseMutex();
	}

	// Token: 0x0600092A RID: 2346 RVA: 0x00044118 File Offset: 0x00042318
	private void PkgSent(IAsyncResult res)
	{
		this.m_sendMutex.WaitOne();
		if (this.m_sendQueue.Count > 0 && this.IsConnected())
		{
			byte[] array = this.m_sendQueue.Dequeue();
			try
			{
				this.m_totalSent += array.Length;
				this.m_socket.BeginSend(array, 0, array.Length, SocketFlags.None, new AsyncCallback(this.PkgSent), null);
				goto IL_86;
			}
			catch (Exception arg)
			{
				ZLog.Log("Handled exception in pkgsent:" + arg);
				this.m_isSending = false;
				this.Disconnect();
				goto IL_86;
			}
		}
		this.m_isSending = false;
		IL_86:
		this.m_sendMutex.ReleaseMutex();
	}

	// Token: 0x0600092B RID: 2347 RVA: 0x000441C8 File Offset: 0x000423C8
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

	// Token: 0x0600092C RID: 2348 RVA: 0x00044222 File Offset: 0x00042422
	public string GetEndPointString()
	{
		if (this.m_endpoint != null)
		{
			return this.m_endpoint.ToString();
		}
		return "None";
	}

	// Token: 0x0600092D RID: 2349 RVA: 0x0004423D File Offset: 0x0004243D
	public string GetEndPointHost()
	{
		if (this.m_endpoint != null)
		{
			return this.m_endpoint.Address.ToString();
		}
		return "None";
	}

	// Token: 0x0600092E RID: 2350 RVA: 0x0004425D File Offset: 0x0004245D
	public IPEndPoint GetEndPoint()
	{
		return this.m_endpoint;
	}

	// Token: 0x0600092F RID: 2351 RVA: 0x00044268 File Offset: 0x00042468
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

	// Token: 0x06000930 RID: 2352 RVA: 0x000442D0 File Offset: 0x000424D0
	public bool IsHost()
	{
		return this.m_listenPort != 0;
	}

	// Token: 0x06000931 RID: 2353 RVA: 0x000442DB File Offset: 0x000424DB
	public int GetHostPort()
	{
		return this.m_listenPort;
	}

	// Token: 0x06000932 RID: 2354 RVA: 0x000442E3 File Offset: 0x000424E3
	public bool IsSending()
	{
		return this.m_isSending || this.m_sendQueue.Count > 0;
	}

	// Token: 0x06000933 RID: 2355 RVA: 0x000442FD File Offset: 0x000424FD
	public void GetAndResetStats(out int totalSent, out int totalRecv)
	{
		totalSent = this.m_totalSent;
		totalRecv = this.m_totalRecv;
		this.m_totalSent = 0;
		this.m_totalRecv = 0;
	}

	// Token: 0x0400086E RID: 2158
	private Socket m_socket;

	// Token: 0x0400086F RID: 2159
	private Mutex m_mutex = new Mutex();

	// Token: 0x04000870 RID: 2160
	private Mutex m_sendMutex = new Mutex();

	// Token: 0x04000871 RID: 2161
	private Queue<Socket> m_newConnections = new Queue<Socket>();

	// Token: 0x04000872 RID: 2162
	private static int m_maxRecvBuffer = 10485760;

	// Token: 0x04000873 RID: 2163
	private int m_recvOffset;

	// Token: 0x04000874 RID: 2164
	private byte[] m_recvBuffer;

	// Token: 0x04000875 RID: 2165
	private byte[] m_recvSizeBuffer = new byte[4];

	// Token: 0x04000876 RID: 2166
	private Queue<ZPackage> m_pkgQueue = new Queue<ZPackage>();

	// Token: 0x04000877 RID: 2167
	private bool m_isSending;

	// Token: 0x04000878 RID: 2168
	private Queue<byte[]> m_sendQueue = new Queue<byte[]>();

	// Token: 0x04000879 RID: 2169
	private IPEndPoint m_endpoint;

	// Token: 0x0400087A RID: 2170
	private string m_originalHostName;

	// Token: 0x0400087B RID: 2171
	private int m_listenPort;

	// Token: 0x0400087C RID: 2172
	private int m_lastRecvPkgSize;

	// Token: 0x0400087D RID: 2173
	private int m_totalSent;

	// Token: 0x0400087E RID: 2174
	private int m_totalRecv;
}
