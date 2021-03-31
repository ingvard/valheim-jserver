using System;

// Token: 0x02000082 RID: 130
internal class RoutedMethod : RoutedMethodBase
{
	// Token: 0x06000897 RID: 2199 RVA: 0x00041E6C File Offset: 0x0004006C
	public RoutedMethod(Action<long> action)
	{
		this.m_action = action;
	}

	// Token: 0x06000898 RID: 2200 RVA: 0x00041E7B File Offset: 0x0004007B
	public void Invoke(long rpc, ZPackage pkg)
	{
		this.m_action(rpc);
	}

	// Token: 0x04000843 RID: 2115
	private Action<long> m_action;
}
