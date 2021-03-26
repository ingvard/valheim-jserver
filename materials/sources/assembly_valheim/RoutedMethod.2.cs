using System;

// Token: 0x02000083 RID: 131
internal class RoutedMethod<T> : RoutedMethodBase
{
	// Token: 0x06000898 RID: 2200 RVA: 0x00041DD5 File Offset: 0x0003FFD5
	public RoutedMethod(Action<long, T> action)
	{
		this.m_action = action;
	}

	// Token: 0x06000899 RID: 2201 RVA: 0x00041DE4 File Offset: 0x0003FFE4
	public void Invoke(long rpc, ZPackage pkg)
	{
		this.m_action.DynamicInvoke(ZNetView.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
	}

	// Token: 0x04000840 RID: 2112
	private Action<long, T> m_action;
}
