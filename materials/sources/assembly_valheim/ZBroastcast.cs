using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

// Token: 0x02000073 RID: 115
public class ZBroastcast : IDisposable
{
	// Token: 0x17000015 RID: 21
	// (get) Token: 0x06000734 RID: 1844 RVA: 0x0003A3F0 File Offset: 0x000385F0
	public static ZBroastcast instance
	{
		get
		{
			return ZBroastcast.m_instance;
		}
	}

	// Token: 0x06000735 RID: 1845 RVA: 0x0003A3F7 File Offset: 0x000385F7
	public static void Initialize()
	{
		if (ZBroastcast.m_instance == null)
		{
			ZBroastcast.m_instance = new ZBroastcast();
		}
	}

	// Token: 0x06000736 RID: 1846 RVA: 0x0003A40C File Offset: 0x0003860C
	private ZBroastcast()
	{
		ZLog.Log("opening zbroadcast");
		this.m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		this.m_socket.EnableBroadcast = true;
		try
		{
			this.m_listner = new UdpClient(6542);
			this.m_listner.EnableBroadcast = true;
			this.m_listner.BeginReceive(new AsyncCallback(this.GotPackage), null);
		}
		catch (Exception ex)
		{
			this.m_listner = null;
			ZLog.Log("Error creating zbroadcast socket " + ex.ToString());
		}
	}

	// Token: 0x06000737 RID: 1847 RVA: 0x0003A4C0 File Offset: 0x000386C0
	public void SetServerPort(int port)
	{
		this.m_myPort = port;
	}

	// Token: 0x06000738 RID: 1848 RVA: 0x0003A4CC File Offset: 0x000386CC
	public void Dispose()
	{
		ZLog.Log("Clozing zbroadcast");
		if (this.m_listner != null)
		{
			this.m_listner.Close();
		}
		this.m_socket.Close();
		this.m_lock.Close();
		if (ZBroastcast.m_instance == this)
		{
			ZBroastcast.m_instance = null;
		}
	}

	// Token: 0x06000739 RID: 1849 RVA: 0x0003A51A File Offset: 0x0003871A
	public void Update(float dt)
	{
		this.m_timer -= dt;
		if (this.m_timer <= 0f)
		{
			this.m_timer = 5f;
			if (this.m_myPort != 0)
			{
				this.Ping();
			}
		}
		this.TimeoutHosts(dt);
	}

	// Token: 0x0600073A RID: 1850 RVA: 0x0003A558 File Offset: 0x00038758
	private void GotPackage(IAsyncResult ar)
	{
		IPEndPoint ipendPoint = new IPEndPoint(0L, 0);
		byte[] array;
		try
		{
			array = this.m_listner.EndReceive(ar, ref ipendPoint);
		}
		catch (ObjectDisposedException)
		{
			return;
		}
		if (array.Length < 5)
		{
			return;
		}
		ZPackage zpackage = new ZPackage(array);
		if (zpackage.ReadChar() != 'F')
		{
			return;
		}
		if (zpackage.ReadChar() != 'E')
		{
			return;
		}
		if (zpackage.ReadChar() != 'J')
		{
			return;
		}
		if (zpackage.ReadChar() != 'D')
		{
			return;
		}
		int port = zpackage.ReadInt();
		this.m_lock.WaitOne();
		this.AddHost(ipendPoint.Address.ToString(), port);
		this.m_lock.ReleaseMutex();
		this.m_listner.BeginReceive(new AsyncCallback(this.GotPackage), null);
	}

	// Token: 0x0600073B RID: 1851 RVA: 0x0003A618 File Offset: 0x00038818
	private void Ping()
	{
		IPEndPoint remoteEP = new IPEndPoint(IPAddress.Broadcast, 6542);
		ZPackage zpackage = new ZPackage();
		zpackage.Write('F');
		zpackage.Write('E');
		zpackage.Write('J');
		zpackage.Write('D');
		zpackage.Write(this.m_myPort);
		this.m_socket.SendTo(zpackage.GetArray(), remoteEP);
	}

	// Token: 0x0600073C RID: 1852 RVA: 0x0003A67C File Offset: 0x0003887C
	private void AddHost(string host, int port)
	{
		foreach (ZBroastcast.HostData hostData in this.m_hosts)
		{
			if (hostData.m_port == port && hostData.m_host == host)
			{
				hostData.m_timeout = 0f;
				return;
			}
		}
		ZBroastcast.HostData hostData2 = new ZBroastcast.HostData();
		hostData2.m_host = host;
		hostData2.m_port = port;
		hostData2.m_timeout = 0f;
		this.m_hosts.Add(hostData2);
	}

	// Token: 0x0600073D RID: 1853 RVA: 0x0003A718 File Offset: 0x00038918
	private void TimeoutHosts(float dt)
	{
		this.m_lock.WaitOne();
		foreach (ZBroastcast.HostData hostData in this.m_hosts)
		{
			hostData.m_timeout += dt;
			if (hostData.m_timeout > 10f)
			{
				this.m_hosts.Remove(hostData);
				return;
			}
		}
		this.m_lock.ReleaseMutex();
	}

	// Token: 0x0600073E RID: 1854 RVA: 0x0003A7A8 File Offset: 0x000389A8
	public void GetHostList(List<ZBroastcast.HostData> hosts)
	{
		hosts.AddRange(this.m_hosts);
	}

	// Token: 0x040007A2 RID: 1954
	private List<ZBroastcast.HostData> m_hosts = new List<ZBroastcast.HostData>();

	// Token: 0x040007A3 RID: 1955
	private static ZBroastcast m_instance;

	// Token: 0x040007A4 RID: 1956
	private const int m_port = 6542;

	// Token: 0x040007A5 RID: 1957
	private const float m_pingInterval = 5f;

	// Token: 0x040007A6 RID: 1958
	private const float m_hostTimeout = 10f;

	// Token: 0x040007A7 RID: 1959
	private float m_timer;

	// Token: 0x040007A8 RID: 1960
	private int m_myPort;

	// Token: 0x040007A9 RID: 1961
	private Socket m_socket;

	// Token: 0x040007AA RID: 1962
	private UdpClient m_listner;

	// Token: 0x040007AB RID: 1963
	private Mutex m_lock = new Mutex();

	// Token: 0x02000167 RID: 359
	public class HostData
	{
		// Token: 0x04001160 RID: 4448
		public string m_host;

		// Token: 0x04001161 RID: 4449
		public int m_port;

		// Token: 0x04001162 RID: 4450
		public float m_timeout;
	}
}
