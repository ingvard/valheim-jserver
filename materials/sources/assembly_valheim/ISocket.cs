using System;

// Token: 0x02000071 RID: 113
public interface ISocket
{
	// Token: 0x06000713 RID: 1811
	bool IsConnected();

	// Token: 0x06000714 RID: 1812
	void Send(ZPackage pkg);

	// Token: 0x06000715 RID: 1813
	ZPackage Recv();

	// Token: 0x06000716 RID: 1814
	int GetSendQueueSize();

	// Token: 0x06000717 RID: 1815
	int GetCurrentSendRate();

	// Token: 0x06000718 RID: 1816
	bool IsHost();

	// Token: 0x06000719 RID: 1817
	void Dispose();

	// Token: 0x0600071A RID: 1818
	bool GotNewData();

	// Token: 0x0600071B RID: 1819
	void Close();

	// Token: 0x0600071C RID: 1820
	string GetEndPointString();

	// Token: 0x0600071D RID: 1821
	void GetAndResetStats(out int totalSent, out int totalRecv);

	// Token: 0x0600071E RID: 1822
	void GetConnectionQuality(out float localQuality, out float remoteQuality, out int ping, out float outByteSec, out float inByteSec);

	// Token: 0x0600071F RID: 1823
	ISocket Accept();

	// Token: 0x06000720 RID: 1824
	int GetHostPort();

	// Token: 0x06000721 RID: 1825
	bool Flush();

	// Token: 0x06000722 RID: 1826
	string GetHostName();
}
