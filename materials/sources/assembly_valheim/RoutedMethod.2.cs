using System;

// Token: 0x02000083 RID: 131
internal class RoutedMethod<T> : RoutedMethodBase
{
	// Token: 0x06000899 RID: 2201 RVA: 0x00041E89 File Offset: 0x00040089
	public RoutedMethod(Action<long, T> action)
	{
		this.m_action = action;
	}

	// Token: 0x0600089A RID: 2202 RVA: 0x00041E98 File Offset: 0x00040098
	public void Invoke(long rpc, ZPackage pkg)
	{
		this.m_action.DynamicInvoke(ZNetView.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
	}

	// Token: 0x04000844 RID: 2116
	private Action<long, T> m_action;
}
