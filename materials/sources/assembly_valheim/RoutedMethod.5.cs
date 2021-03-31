using System;

// Token: 0x02000086 RID: 134
public class RoutedMethod<T, U, V, B> : RoutedMethodBase
{
	// Token: 0x0600089F RID: 2207 RVA: 0x00041F25 File Offset: 0x00040125
	public RoutedMethod(RoutedMethod<T, U, V, B>.Method action)
	{
		this.m_action = action;
	}

	// Token: 0x060008A0 RID: 2208 RVA: 0x00041F34 File Offset: 0x00040134
	public void Invoke(long rpc, ZPackage pkg)
	{
		this.m_action.DynamicInvoke(ZNetView.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
	}

	// Token: 0x04000847 RID: 2119
	private RoutedMethod<T, U, V, B>.Method m_action;

	// Token: 0x0200016F RID: 367
	// (Invoke) Token: 0x0600115D RID: 4445
	public delegate void Method(long sender, T p0, U p1, V p2, B p3);
}
