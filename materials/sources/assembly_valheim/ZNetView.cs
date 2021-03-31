using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// Token: 0x02000087 RID: 135
public class ZNetView : MonoBehaviour
{
	// Token: 0x060008A1 RID: 2209 RVA: 0x00041F5C File Offset: 0x0004015C
	private void Awake()
	{
		if (ZNetView.m_forceDisableInit)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		this.m_body = base.GetComponent<Rigidbody>();
		if (ZNetView.m_useInitZDO && ZNetView.m_initZDO == null)
		{
			ZLog.LogWarning("Double ZNetview when initializing object " + base.gameObject.name);
		}
		if (ZNetView.m_initZDO != null)
		{
			this.m_zdo = ZNetView.m_initZDO;
			ZNetView.m_initZDO = null;
			if (this.m_zdo.m_type != this.m_type && this.m_zdo.IsOwner())
			{
				this.m_zdo.SetType(this.m_type);
			}
			if (this.m_zdo.m_distant != this.m_distant && this.m_zdo.IsOwner())
			{
				this.m_zdo.SetDistant(this.m_distant);
			}
			if (this.m_syncInitialScale)
			{
				Vector3 vec = this.m_zdo.GetVec3("scale", base.transform.localScale);
				base.transform.localScale = vec;
			}
			if (this.m_body)
			{
				this.m_body.Sleep();
			}
		}
		else
		{
			string prefabName = this.GetPrefabName();
			this.m_zdo = ZDOMan.instance.CreateNewZDO(base.transform.position);
			this.m_zdo.m_persistent = this.m_persistent;
			this.m_zdo.m_type = this.m_type;
			this.m_zdo.m_distant = this.m_distant;
			this.m_zdo.SetPrefab(prefabName.GetStableHashCode());
			this.m_zdo.SetRotation(base.transform.rotation);
			if (this.m_syncInitialScale)
			{
				this.m_zdo.Set("scale", base.transform.localScale);
			}
			if (ZNetView.m_ghostInit)
			{
				this.m_ghost = true;
				return;
			}
		}
		ZNetScene.instance.AddInstance(this.m_zdo, this);
	}

	// Token: 0x060008A2 RID: 2210 RVA: 0x00042138 File Offset: 0x00040338
	public void SetLocalScale(Vector3 scale)
	{
		base.transform.localScale = scale;
		if (this.m_zdo != null && this.m_syncInitialScale && this.IsOwner())
		{
			this.m_zdo.Set("scale", base.transform.localScale);
		}
	}

	// Token: 0x060008A3 RID: 2211 RVA: 0x00042184 File Offset: 0x00040384
	private void OnDestroy()
	{
		ZNetScene.instance;
	}

	// Token: 0x060008A4 RID: 2212 RVA: 0x00042191 File Offset: 0x00040391
	public void SetPersistent(bool persistent)
	{
		this.m_zdo.m_persistent = persistent;
	}

	// Token: 0x060008A5 RID: 2213 RVA: 0x0004219F File Offset: 0x0004039F
	public string GetPrefabName()
	{
		return ZNetView.GetPrefabName(base.gameObject);
	}

	// Token: 0x060008A6 RID: 2214 RVA: 0x000421AC File Offset: 0x000403AC
	public static string GetPrefabName(GameObject gameObject)
	{
		string name = gameObject.name;
		char[] anyOf = new char[]
		{
			'(',
			' '
		};
		int num = name.IndexOfAny(anyOf);
		if (num != -1)
		{
			return name.Remove(num);
		}
		return name;
	}

	// Token: 0x060008A7 RID: 2215 RVA: 0x000421E6 File Offset: 0x000403E6
	public void Destroy()
	{
		ZNetScene.instance.Destroy(base.gameObject);
	}

	// Token: 0x060008A8 RID: 2216 RVA: 0x000421F8 File Offset: 0x000403F8
	public bool IsOwner()
	{
		return this.m_zdo.IsOwner();
	}

	// Token: 0x060008A9 RID: 2217 RVA: 0x00042205 File Offset: 0x00040405
	public bool HasOwner()
	{
		return this.m_zdo.HasOwner();
	}

	// Token: 0x060008AA RID: 2218 RVA: 0x00042212 File Offset: 0x00040412
	public void ClaimOwnership()
	{
		if (this.IsOwner())
		{
			return;
		}
		this.m_zdo.SetOwner(ZDOMan.instance.GetMyID());
	}

	// Token: 0x060008AB RID: 2219 RVA: 0x00042232 File Offset: 0x00040432
	public ZDO GetZDO()
	{
		return this.m_zdo;
	}

	// Token: 0x060008AC RID: 2220 RVA: 0x0004223A File Offset: 0x0004043A
	public bool IsValid()
	{
		return this.m_zdo != null && this.m_zdo.IsValid();
	}

	// Token: 0x060008AD RID: 2221 RVA: 0x00042251 File Offset: 0x00040451
	public void ResetZDO()
	{
		this.m_zdo = null;
	}

	// Token: 0x060008AE RID: 2222 RVA: 0x0004225A File Offset: 0x0004045A
	public void Register(string name, Action<long> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod(f));
	}

	// Token: 0x060008AF RID: 2223 RVA: 0x00042273 File Offset: 0x00040473
	public void Register<T>(string name, Action<long, T> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod<T>(f));
	}

	// Token: 0x060008B0 RID: 2224 RVA: 0x0004228C File Offset: 0x0004048C
	public void Register<T, U>(string name, Action<long, T, U> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod<T, U>(f));
	}

	// Token: 0x060008B1 RID: 2225 RVA: 0x000422A5 File Offset: 0x000404A5
	public void Register<T, U, V>(string name, Action<long, T, U, V> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod<T, U, V>(f));
	}

	// Token: 0x060008B2 RID: 2226 RVA: 0x000422C0 File Offset: 0x000404C0
	public void Unregister(string name)
	{
		int stableHashCode = name.GetStableHashCode();
		this.m_functions.Remove(stableHashCode);
	}

	// Token: 0x060008B3 RID: 2227 RVA: 0x000422E4 File Offset: 0x000404E4
	public void HandleRoutedRPC(ZRoutedRpc.RoutedRPCData rpcData)
	{
		RoutedMethodBase routedMethodBase;
		if (this.m_functions.TryGetValue(rpcData.m_methodHash, out routedMethodBase))
		{
			routedMethodBase.Invoke(rpcData.m_senderPeerID, rpcData.m_parameters);
			return;
		}
		ZLog.LogWarning("Failed to find rpc method " + rpcData.m_methodHash);
	}

	// Token: 0x060008B4 RID: 2228 RVA: 0x00042333 File Offset: 0x00040533
	public void InvokeRPC(long targetID, string method, params object[] parameters)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC(targetID, this.m_zdo.m_uid, method, parameters);
	}

	// Token: 0x060008B5 RID: 2229 RVA: 0x0004234D File Offset: 0x0004054D
	public void InvokeRPC(string method, params object[] parameters)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC(this.m_zdo.m_owner, this.m_zdo.m_uid, method, parameters);
	}

	// Token: 0x060008B6 RID: 2230 RVA: 0x00042374 File Offset: 0x00040574
	public static object[] Deserialize(long callerID, ParameterInfo[] paramInfo, ZPackage pkg)
	{
		List<object> list = new List<object>();
		list.Add(callerID);
		ZRpc.Deserialize(paramInfo, pkg, ref list);
		return list.ToArray();
	}

	// Token: 0x060008B7 RID: 2231 RVA: 0x000423A2 File Offset: 0x000405A2
	public static void StartGhostInit()
	{
		ZNetView.m_ghostInit = true;
	}

	// Token: 0x060008B8 RID: 2232 RVA: 0x000423AA File Offset: 0x000405AA
	public static void FinishGhostInit()
	{
		ZNetView.m_ghostInit = false;
	}

	// Token: 0x04000848 RID: 2120
	public static long Everybody;

	// Token: 0x04000849 RID: 2121
	public bool m_persistent;

	// Token: 0x0400084A RID: 2122
	public bool m_distant;

	// Token: 0x0400084B RID: 2123
	public ZDO.ObjectType m_type;

	// Token: 0x0400084C RID: 2124
	public bool m_syncInitialScale;

	// Token: 0x0400084D RID: 2125
	private ZDO m_zdo;

	// Token: 0x0400084E RID: 2126
	private Rigidbody m_body;

	// Token: 0x0400084F RID: 2127
	private Dictionary<int, RoutedMethodBase> m_functions = new Dictionary<int, RoutedMethodBase>();

	// Token: 0x04000850 RID: 2128
	private bool m_ghost;

	// Token: 0x04000851 RID: 2129
	public static bool m_useInitZDO;

	// Token: 0x04000852 RID: 2130
	public static ZDO m_initZDO;

	// Token: 0x04000853 RID: 2131
	public static bool m_forceDisableInit;

	// Token: 0x04000854 RID: 2132
	private static bool m_ghostInit;
}
