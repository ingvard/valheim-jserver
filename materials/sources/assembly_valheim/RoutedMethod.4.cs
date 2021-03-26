using System;

// Token: 0x02000085 RID: 133
internal class RoutedMethod<T, U, V> : RoutedMethodBase
{
	// Token: 0x0600089C RID: 2204 RVA: 0x00041E3D File Offset: 0x0004003D
	public RoutedMethod(Action<long, T, U, V> action)
	{
		this.m_action = action;
	}

	// Token: 0x0600089D RID: 2205 RVA: 0x00041E4C File Offset: 0x0004004C
	public void Invoke(long rpc, ZPackage pkg)
	{
		this.m_action.DynamicInvoke(ZNetView.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
	}

	// Token: 0x04000842 RID: 2114
	private Action<long, T, U, V> m_action;
}
