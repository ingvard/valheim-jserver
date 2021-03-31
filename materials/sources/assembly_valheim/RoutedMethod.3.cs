using System;

// Token: 0x02000084 RID: 132
internal class RoutedMethod<T, U> : RoutedMethodBase
{
	// Token: 0x0600089B RID: 2203 RVA: 0x00041EBD File Offset: 0x000400BD
	public RoutedMethod(Action<long, T, U> action)
	{
		this.m_action = action;
	}

	// Token: 0x0600089C RID: 2204 RVA: 0x00041ECC File Offset: 0x000400CC
	public void Invoke(long rpc, ZPackage pkg)
	{
		this.m_action.DynamicInvoke(ZNetView.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
	}

	// Token: 0x04000845 RID: 2117
	private Action<long, T, U> m_action;
}
