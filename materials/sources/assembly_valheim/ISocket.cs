using System;

// Token: 0x02000071 RID: 113
public interface ISocket
{
	// Token: 0x06000714 RID: 1812
	bool IsConnected();

	// Token: 0x06000715 RID: 1813
	void Send(ZPackage pkg);

	// Token: 0x06000716 RID: 1814
	ZPackage Recv();

	// Token: 0x06000717 RID: 1815
	int GetSendQueueSize();

	// Token: 0x06000718 RID: 1816
	int GetCurrentSendRate();

	// Token: 0x06000719 RID: 1817
	bool IsHost();

	// Token: 0x0600071A RID: 1818
	void Dispose();

	// Token: 0x0600071B RID: 1819
	bool GotNewData();

	// Token: 0x0600071C RID: 1820
	void Close();

	// Token: 0x0600071D RID: 1821
	string GetEndPointString();

	// Token: 0x0600071E RID: 1822
	void GetAndResetStats(out int totalSent, out int totalRecv);

	// Token: 0x0600071F RID: 1823
	void GetConnectionQuality(out float localQuality, out float remoteQuality, out int ping, out float outByteSec, out float inByteSec);

	// Token: 0x06000720 RID: 1824
	ISocket Accept();

	// Token: 0x06000721 RID: 1825
	int GetHostPort();

	// Token: 0x06000722 RID: 1826
	bool Flush();

	// Token: 0x06000723 RID: 1827
	string GetHostName();
}
