using System;
using System.IO;

// Token: 0x0200007B RID: 123
public class ZNat : IDisposable
{
	// Token: 0x060007FE RID: 2046 RVA: 0x000027E0 File Offset: 0x000009E0
	public void Dispose()
	{
	}

	// Token: 0x060007FF RID: 2047 RVA: 0x0003EA93 File Offset: 0x0003CC93
	public void SetPort(int port)
	{
		if (this.m_port == port)
		{
			return;
		}
		this.m_port = port;
	}

	// Token: 0x06000800 RID: 2048 RVA: 0x000027E0 File Offset: 0x000009E0
	public void Update(float dt)
	{
	}

	// Token: 0x06000801 RID: 2049 RVA: 0x0003EAA6 File Offset: 0x0003CCA6
	public bool GetStatus()
	{
		return this.m_mappingOK;
	}

	// Token: 0x040007F5 RID: 2037
	private FileStream m_output;

	// Token: 0x040007F6 RID: 2038
	private bool m_mappingOK;

	// Token: 0x040007F7 RID: 2039
	private int m_port;
}
