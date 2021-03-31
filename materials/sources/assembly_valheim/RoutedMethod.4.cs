using System;

// Token: 0x02000085 RID: 133
internal class RoutedMethod<T, U, V> : RoutedMethodBase
{
	// Token: 0x0600089D RID: 2205 RVA: 0x00041EF1 File Offset: 0x000400F1
	public RoutedMethod(Action<long, T, U, V> action)
	{
		this.m_action = action;
	}

	// Token: 0x0600089E RID: 2206 RVA: 0x00041F00 File Offset: 0x00040100
	public void Invoke(long rpc, ZPackage pkg)
	{
		this.m_action.DynamicInvoke(ZNetView.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
	}

	// Token: 0x04000846 RID: 2118
	private Action<long, T, U, V> m_action;
}
