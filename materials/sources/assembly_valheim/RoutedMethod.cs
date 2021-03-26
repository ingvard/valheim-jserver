using System;

// Token: 0x02000082 RID: 130
internal class RoutedMethod : RoutedMethodBase
{
	// Token: 0x06000896 RID: 2198 RVA: 0x00041DB8 File Offset: 0x0003FFB8
	public RoutedMethod(Action<long> action)
	{
		this.m_action = action;
	}

	// Token: 0x06000897 RID: 2199 RVA: 0x00041DC7 File Offset: 0x0003FFC7
	public void Invoke(long rpc, ZPackage pkg)
	{
		this.m_action(rpc);
	}

	// Token: 0x0400083F RID: 2111
	private Action<long> m_action;
}
