using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

// Token: 0x02000088 RID: 136
public class ZNtp : IDisposable
{
	// Token: 0x1700001B RID: 27
	// (get) Token: 0x060008BB RID: 2235 RVA: 0x000423C5 File Offset: 0x000405C5
	public static ZNtp instance
	{
		get
		{
			return ZNtp.m_instance;
		}
	}

	// Token: 0x060008BC RID: 2236 RVA: 0x000423CC File Offset: 0x000405CC
	public ZNtp()
	{
		ZNtp.m_instance = this;
		this.m_ntpTime = DateTime.UtcNow;
		this.m_ntpThread = new Thread(new ThreadStart(this.NtpThread));
		this.m_ntpThread.Start();
	}

	// Token: 0x060008BD RID: 2237 RVA: 0x00042420 File Offset: 0x00040620
	public void Dispose()
	{
		if (this.m_ntpThread != null)
		{
			ZLog.Log("Stoping ntp thread");
			this.m_lock.WaitOne();
			this.m_stop = true;
			this.m_ntpThread.Abort();
			this.m_lock.ReleaseMutex();
			this.m_ntpThread = null;
		}
		if (this.m_lock != null)
		{
			this.m_lock.Close();
			this.m_lock = null;
		}
	}

	// Token: 0x060008BE RID: 2238 RVA: 0x00042489 File Offset: 0x00040689
	public bool GetStatus()
	{
		return this.m_status;
	}

	// Token: 0x060008BF RID: 2239 RVA: 0x00042491 File Offset: 0x00040691
	public void Update(float dt)
	{
		this.m_lock.WaitOne();
		this.m_ntpTime = this.m_ntpTime.AddSeconds((double)dt);
		this.m_lock.ReleaseMutex();
	}

	// Token: 0x060008C0 RID: 2240 RVA: 0x000424C0 File Offset: 0x000406C0
	private void NtpThread()
	{
		while (!this.m_stop)
		{
			DateTime ntpTime;
			if (this.GetNetworkTime("pool.ntp.org", out ntpTime))
			{
				this.m_status = true;
				this.m_lock.WaitOne();
				this.m_ntpTime = ntpTime;
				this.m_lock.ReleaseMutex();
			}
			else
			{
				this.m_status = false;
			}
			Thread.Sleep(60000);
		}
	}

	// Token: 0x060008C1 RID: 2241 RVA: 0x0004251E File Offset: 0x0004071E
	public DateTime GetTime()
	{
		return this.m_ntpTime;
	}

	// Token: 0x060008C2 RID: 2242 RVA: 0x00042528 File Offset: 0x00040728
	private bool GetNetworkTime(string ntpServer, out DateTime time)
	{
		byte[] array = new byte[48];
		array[0] = 27;
		IPAddress[] addressList;
		try
		{
			addressList = Dns.GetHostEntry(ntpServer).AddressList;
			if (addressList.Length == 0)
			{
				ZLog.Log("Dns lookup failed");
				time = DateTime.UtcNow;
				return false;
			}
		}
		catch
		{
			ZLog.Log("Failed ntp dns lookup");
			time = DateTime.UtcNow;
			return false;
		}
		IPEndPoint remoteEP = new IPEndPoint(addressList[0], 123);
		Socket socket = null;
		try
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket.ReceiveTimeout = 3000;
			socket.SendTimeout = 3000;
			socket.Connect(remoteEP);
			if (!socket.Connected)
			{
				ZLog.Log("Failed to connect to ntp");
				time = DateTime.UtcNow;
				socket.Close();
				return false;
			}
			socket.Send(array);
			socket.Receive(array);
			socket.Shutdown(SocketShutdown.Both);
			socket.Close();
		}
		catch
		{
			if (socket != null)
			{
				socket.Close();
			}
			time = DateTime.UtcNow;
			return false;
		}
		ulong num = (ulong)array[40] << 24 | (ulong)array[41] << 16 | (ulong)array[42] << 8 | (ulong)array[43];
		ulong num2 = (ulong)array[44] << 24 | (ulong)array[45] << 16 | (ulong)array[46] << 8 | (ulong)array[47];
		ulong num3 = num * 1000UL + num2 * 1000UL / 4294967296UL;
		time = new DateTime(1900, 1, 1).AddMilliseconds((double)num3);
		return true;
	}

	// Token: 0x04000855 RID: 2133
	private static ZNtp m_instance;

	// Token: 0x04000856 RID: 2134
	private DateTime m_ntpTime;

	// Token: 0x04000857 RID: 2135
	private bool m_status;

	// Token: 0x04000858 RID: 2136
	private bool m_stop;

	// Token: 0x04000859 RID: 2137
	private Thread m_ntpThread;

	// Token: 0x0400085A RID: 2138
	private Mutex m_lock = new Mutex();
}
