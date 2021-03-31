using System;
using System.IO;

// Token: 0x0200007B RID: 123
public class ZNat : IDisposable
{
	// Token: 0x060007FF RID: 2047 RVA: 0x000027E0 File Offset: 0x000009E0
	public void Dispose()
	{
	}

	// Token: 0x06000800 RID: 2048 RVA: 0x0003EB47 File Offset: 0x0003CD47
	public void SetPort(int port)
	{
		if (this.m_port == port)
		{
			return;
		}
		this.m_port = port;
	}

	// Token: 0x06000801 RID: 2049 RVA: 0x000027E0 File Offset: 0x000009E0
	public void Update(float dt)
	{
	}

	// Token: 0x06000802 RID: 2050 RVA: 0x0003EB5A File Offset: 0x0003CD5A
	public bool GetStatus()
	{
		return this.m_mappingOK;
	}

	// Token: 0x040007F9 RID: 2041
	private FileStream m_output;

	// Token: 0x040007FA RID: 2042
	private bool m_mappingOK;

	// Token: 0x040007FB RID: 2043
	private int m_port;
}
