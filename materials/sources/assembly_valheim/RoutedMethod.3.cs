using System;

// Token: 0x02000084 RID: 132
internal class RoutedMethod<T, U> : RoutedMethodBase
{
	// Token: 0x0600089A RID: 2202 RVA: 0x00041E09 File Offset: 0x00040009
	public RoutedMethod(Action<long, T, U> action)
	{
		this.m_action = action;
	}

	// Token: 0x0600089B RID: 2203 RVA: 0x00041E18 File Offset: 0x00040018
	public void Invoke(long rpc, ZPackage pkg)
	{
		this.m_action.DynamicInvoke(ZNetView.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
	}

	// Token: 0x04000841 RID: 2113
	private Action<long, T, U> m_action;
}
