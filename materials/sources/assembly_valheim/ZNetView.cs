using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// Token: 0x02000087 RID: 135
public class ZNetView : MonoBehaviour
{
	// Token: 0x060008A0 RID: 2208 RVA: 0x00041EA8 File Offset: 0x000400A8
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

	// Token: 0x060008A1 RID: 2209 RVA: 0x00042084 File Offset: 0x00040284
	public void SetLocalScale(Vector3 scale)
	{
		base.transform.localScale = scale;
		if (this.m_zdo != null && this.m_syncInitialScale && this.IsOwner())
		{
			this.m_zdo.Set("scale", base.transform.localScale);
		}
	}

	// Token: 0x060008A2 RID: 2210 RVA: 0x000420D0 File Offset: 0x000402D0
	private void OnDestroy()
	{
		ZNetScene.instance;
	}

	// Token: 0x060008A3 RID: 2211 RVA: 0x000420DD File Offset: 0x000402DD
	public void SetPersistent(bool persistent)
	{
		this.m_zdo.m_persistent = persistent;
	}

	// Token: 0x060008A4 RID: 2212 RVA: 0x000420EB File Offset: 0x000402EB
	public string GetPrefabName()
	{
		return ZNetView.GetPrefabName(base.gameObject);
	}

	// Token: 0x060008A5 RID: 2213 RVA: 0x000420F8 File Offset: 0x000402F8
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

	// Token: 0x060008A6 RID: 2214 RVA: 0x00042132 File Offset: 0x00040332
	public void Destroy()
	{
		ZNetScene.instance.Destroy(base.gameObject);
	}

	// Token: 0x060008A7 RID: 2215 RVA: 0x00042144 File Offset: 0x00040344
	public bool IsOwner()
	{
		return this.m_zdo.IsOwner();
	}

	// Token: 0x060008A8 RID: 2216 RVA: 0x00042151 File Offset: 0x00040351
	public bool HasOwner()
	{
		return this.m_zdo.HasOwner();
	}

	// Token: 0x060008A9 RID: 2217 RVA: 0x0004215E File Offset: 0x0004035E
	public void ClaimOwnership()
	{
		if (this.IsOwner())
		{
			return;
		}
		this.m_zdo.SetOwner(ZDOMan.instance.GetMyID());
	}

	// Token: 0x060008AA RID: 2218 RVA: 0x0004217E File Offset: 0x0004037E
	public ZDO GetZDO()
	{
		return this.m_zdo;
	}

	// Token: 0x060008AB RID: 2219 RVA: 0x00042186 File Offset: 0x00040386
	public bool IsValid()
	{
		return this.m_zdo != null && this.m_zdo.IsValid();
	}

	// Token: 0x060008AC RID: 2220 RVA: 0x0004219D File Offset: 0x0004039D
	public void ResetZDO()
	{
		this.m_zdo = null;
	}

	// Token: 0x060008AD RID: 2221 RVA: 0x000421A6 File Offset: 0x000403A6
	public void Register(string name, Action<long> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod(f));
	}

	// Token: 0x060008AE RID: 2222 RVA: 0x000421BF File Offset: 0x000403BF
	public void Register<T>(string name, Action<long, T> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod<T>(f));
	}

	// Token: 0x060008AF RID: 2223 RVA: 0x000421D8 File Offset: 0x000403D8
	public void Register<T, U>(string name, Action<long, T, U> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod<T, U>(f));
	}

	// Token: 0x060008B0 RID: 2224 RVA: 0x000421F1 File Offset: 0x000403F1
	public void Register<T, U, V>(string name, Action<long, T, U, V> f)
	{
		this.m_functions.Add(name.GetStableHashCode(), new RoutedMethod<T, U, V>(f));
	}

	// Token: 0x060008B1 RID: 2225 RVA: 0x0004220C File Offset: 0x0004040C
	public void Unregister(string name)
	{
		int stableHashCode = name.GetStableHashCode();
		this.m_functions.Remove(stableHashCode);
	}

	// Token: 0x060008B2 RID: 2226 RVA: 0x00042230 File Offset: 0x00040430
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

	// Token: 0x060008B3 RID: 2227 RVA: 0x0004227F File Offset: 0x0004047F
	public void InvokeRPC(long targetID, string method, params object[] parameters)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC(targetID, this.m_zdo.m_uid, method, parameters);
	}

	// Token: 0x060008B4 RID: 2228 RVA: 0x00042299 File Offset: 0x00040499
	public void InvokeRPC(string method, params object[] parameters)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC(this.m_zdo.m_owner, this.m_zdo.m_uid, method, parameters);
	}

	// Token: 0x060008B5 RID: 2229 RVA: 0x000422C0 File Offset: 0x000404C0
	public static object[] Deserialize(long callerID, ParameterInfo[] paramInfo, ZPackage pkg)
	{
		List<object> list = new List<object>();
		list.Add(callerID);
		ZRpc.Deserialize(paramInfo, pkg, ref list);
		return list.ToArray();
	}

	// Token: 0x060008B6 RID: 2230 RVA: 0x000422EE File Offset: 0x000404EE
	public static void StartGhostInit()
	{
		ZNetView.m_ghostInit = true;
	}

	// Token: 0x060008B7 RID: 2231 RVA: 0x000422F6 File Offset: 0x000404F6
	public static void FinishGhostInit()
	{
		ZNetView.m_ghostInit = false;
	}

	// Token: 0x04000844 RID: 2116
	public static long Everybody;

	// Token: 0x04000845 RID: 2117
	public bool m_persistent;

	// Token: 0x04000846 RID: 2118
	public bool m_distant;

	// Token: 0x04000847 RID: 2119
	public ZDO.ObjectType m_type;

	// Token: 0x04000848 RID: 2120
	public bool m_syncInitialScale;

	// Token: 0x04000849 RID: 2121
	private ZDO m_zdo;

	// Token: 0x0400084A RID: 2122
	private Rigidbody m_body;

	// Token: 0x0400084B RID: 2123
	private Dictionary<int, RoutedMethodBase> m_functions = new Dictionary<int, RoutedMethodBase>();

	// Token: 0x0400084C RID: 2124
	private bool m_ghost;

	// Token: 0x0400084D RID: 2125
	public static bool m_useInitZDO;

	// Token: 0x0400084E RID: 2126
	public static ZDO m_initZDO;

	// Token: 0x0400084F RID: 2127
	public static bool m_forceDisableInit;

	// Token: 0x04000850 RID: 2128
	private static bool m_ghostInit;
}
