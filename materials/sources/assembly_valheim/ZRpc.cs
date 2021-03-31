using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// Token: 0x0200008B RID: 139
public class ZRpc : IDisposable
{
	// Token: 0x06000905 RID: 2309 RVA: 0x00043153 File Offset: 0x00041353
	public ZRpc(ISocket socket)
	{
		this.m_socket = socket;
	}

	// Token: 0x06000906 RID: 2310 RVA: 0x00043178 File Offset: 0x00041378
	public void Dispose()
	{
		this.m_socket.Dispose();
	}

	// Token: 0x06000907 RID: 2311 RVA: 0x00043185 File Offset: 0x00041385
	public ISocket GetSocket()
	{
		return this.m_socket;
	}

	// Token: 0x06000908 RID: 2312 RVA: 0x00043190 File Offset: 0x00041390
	public bool Update(float dt)
	{
		if (!this.m_socket.IsConnected())
		{
			return false;
		}
		for (ZPackage zpackage = this.m_socket.Recv(); zpackage != null; zpackage = this.m_socket.Recv())
		{
			this.m_recvPackages++;
			this.m_recvData += zpackage.Size();
			try
			{
				this.HandlePackage(zpackage);
			}
			catch (Exception arg)
			{
				ZLog.Log("Exception in ZRpc::HandlePackage: " + arg);
			}
		}
		this.UpdatePing(dt);
		return true;
	}

	// Token: 0x06000909 RID: 2313 RVA: 0x00043220 File Offset: 0x00041420
	private void UpdatePing(float dt)
	{
		this.m_pingTimer += dt;
		if (this.m_pingTimer > ZRpc.m_pingInterval)
		{
			this.m_pingTimer = 0f;
			this.m_pkg.Clear();
			this.m_pkg.Write(0);
			this.m_pkg.Write(true);
			this.SendPackage(this.m_pkg);
		}
		this.m_timeSinceLastPing += dt;
		if (this.m_timeSinceLastPing > ZRpc.m_timeout)
		{
			ZLog.LogWarning("ZRpc timeout detected");
			this.m_socket.Close();
		}
	}

	// Token: 0x0600090A RID: 2314 RVA: 0x000432B4 File Offset: 0x000414B4
	private void ReceivePing(ZPackage package)
	{
		if (package.ReadBool())
		{
			this.m_pkg.Clear();
			this.m_pkg.Write(0);
			this.m_pkg.Write(false);
			this.SendPackage(this.m_pkg);
			return;
		}
		this.m_timeSinceLastPing = 0f;
	}

	// Token: 0x0600090B RID: 2315 RVA: 0x00043304 File Offset: 0x00041504
	public float GetTimeSinceLastPing()
	{
		return this.m_timeSinceLastPing;
	}

	// Token: 0x0600090C RID: 2316 RVA: 0x0004330C File Offset: 0x0004150C
	public bool IsConnected()
	{
		return this.m_socket.IsConnected();
	}

	// Token: 0x0600090D RID: 2317 RVA: 0x0004331C File Offset: 0x0004151C
	private void HandlePackage(ZPackage package)
	{
		int num = package.ReadInt();
		if (num == 0)
		{
			this.ReceivePing(package);
			return;
		}
		ZRpc.RpcMethodBase rpcMethodBase2;
		if (ZRpc.m_DEBUG)
		{
			package.ReadString();
			ZRpc.RpcMethodBase rpcMethodBase;
			if (this.m_functions.TryGetValue(num, out rpcMethodBase))
			{
				rpcMethodBase.Invoke(this, package);
				return;
			}
		}
		else if (this.m_functions.TryGetValue(num, out rpcMethodBase2))
		{
			rpcMethodBase2.Invoke(this, package);
		}
	}

	// Token: 0x0600090E RID: 2318 RVA: 0x0004337C File Offset: 0x0004157C
	public void Register(string name, ZRpc.RpcMethod.Method f)
	{
		int stableHashCode = name.GetStableHashCode();
		this.m_functions.Remove(stableHashCode);
		this.m_functions.Add(stableHashCode, new ZRpc.RpcMethod(f));
	}

	// Token: 0x0600090F RID: 2319 RVA: 0x000433B0 File Offset: 0x000415B0
	public void Register<T>(string name, Action<ZRpc, T> f)
	{
		int stableHashCode = name.GetStableHashCode();
		this.m_functions.Remove(stableHashCode);
		this.m_functions.Add(stableHashCode, new ZRpc.RpcMethod<T>(f));
	}

	// Token: 0x06000910 RID: 2320 RVA: 0x000433E4 File Offset: 0x000415E4
	public void Register<T, U>(string name, Action<ZRpc, T, U> f)
	{
		int stableHashCode = name.GetStableHashCode();
		this.m_functions.Remove(stableHashCode);
		this.m_functions.Add(stableHashCode, new ZRpc.RpcMethod<T, U>(f));
	}

	// Token: 0x06000911 RID: 2321 RVA: 0x00043418 File Offset: 0x00041618
	public void Register<T, U, V>(string name, Action<ZRpc, T, U, V> f)
	{
		int stableHashCode = name.GetStableHashCode();
		this.m_functions.Remove(stableHashCode);
		this.m_functions.Add(stableHashCode, new ZRpc.RpcMethod<T, U, V>(f));
	}

	// Token: 0x06000912 RID: 2322 RVA: 0x0004344C File Offset: 0x0004164C
	public void Register<T, U, V, W>(string name, ZRpc.RpcMethod<T, U, V, W>.Method f)
	{
		int stableHashCode = name.GetStableHashCode();
		this.m_functions.Remove(stableHashCode);
		this.m_functions.Add(stableHashCode, new ZRpc.RpcMethod<T, U, V, W>(f));
	}

	// Token: 0x06000913 RID: 2323 RVA: 0x00043480 File Offset: 0x00041680
	public void Unregister(string name)
	{
		int stableHashCode = name.GetStableHashCode();
		this.m_functions.Remove(stableHashCode);
	}

	// Token: 0x06000914 RID: 2324 RVA: 0x000434A4 File Offset: 0x000416A4
	public void Invoke(string method, params object[] parameters)
	{
		if (!this.IsConnected())
		{
			return;
		}
		this.m_pkg.Clear();
		int stableHashCode = method.GetStableHashCode();
		this.m_pkg.Write(stableHashCode);
		if (ZRpc.m_DEBUG)
		{
			this.m_pkg.Write(method);
		}
		ZRpc.Serialize(parameters, ref this.m_pkg);
		this.SendPackage(this.m_pkg);
	}

	// Token: 0x06000915 RID: 2325 RVA: 0x00043503 File Offset: 0x00041703
	private void SendPackage(ZPackage pkg)
	{
		this.m_sentPackages++;
		this.m_sentData += pkg.Size();
		this.m_socket.Send(this.m_pkg);
	}

	// Token: 0x06000916 RID: 2326 RVA: 0x00043538 File Offset: 0x00041738
	public static void Serialize(object[] parameters, ref ZPackage pkg)
	{
		foreach (object obj in parameters)
		{
			if (obj is int)
			{
				pkg.Write((int)obj);
			}
			else if (obj is uint)
			{
				pkg.Write((uint)obj);
			}
			else if (obj is long)
			{
				pkg.Write((long)obj);
			}
			else if (obj is float)
			{
				pkg.Write((float)obj);
			}
			else if (obj is double)
			{
				pkg.Write((double)obj);
			}
			else if (obj is bool)
			{
				pkg.Write((bool)obj);
			}
			else if (obj is string)
			{
				pkg.Write((string)obj);
			}
			else if (obj is ZPackage)
			{
				pkg.Write((ZPackage)obj);
			}
			else
			{
				if (obj is List<string>)
				{
					List<string> list = obj as List<string>;
					pkg.Write(list.Count);
					using (List<string>.Enumerator enumerator = list.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							string data = enumerator.Current;
							pkg.Write(data);
						}
						goto IL_1EE;
					}
				}
				if (obj is Vector3)
				{
					pkg.Write(((Vector3)obj).x);
					pkg.Write(((Vector3)obj).y);
					pkg.Write(((Vector3)obj).z);
				}
				else if (obj is Quaternion)
				{
					pkg.Write(((Quaternion)obj).x);
					pkg.Write(((Quaternion)obj).y);
					pkg.Write(((Quaternion)obj).z);
					pkg.Write(((Quaternion)obj).w);
				}
				else if (obj is ZDOID)
				{
					pkg.Write((ZDOID)obj);
				}
				else if (obj is HitData)
				{
					(obj as HitData).Serialize(ref pkg);
				}
			}
			IL_1EE:;
		}
	}

	// Token: 0x06000917 RID: 2327 RVA: 0x00043750 File Offset: 0x00041950
	public static object[] Deserialize(ZRpc rpc, ParameterInfo[] paramInfo, ZPackage pkg)
	{
		List<object> list = new List<object>();
		list.Add(rpc);
		ZRpc.Deserialize(paramInfo, pkg, ref list);
		return list.ToArray();
	}

	// Token: 0x06000918 RID: 2328 RVA: 0x0004377C File Offset: 0x0004197C
	public static void Deserialize(ParameterInfo[] paramInfo, ZPackage pkg, ref List<object> parameters)
	{
		for (int i = 1; i < paramInfo.Length; i++)
		{
			ParameterInfo parameterInfo = paramInfo[i];
			if (parameterInfo.ParameterType == typeof(int))
			{
				parameters.Add(pkg.ReadInt());
			}
			else if (parameterInfo.ParameterType == typeof(uint))
			{
				parameters.Add(pkg.ReadUInt());
			}
			else if (parameterInfo.ParameterType == typeof(long))
			{
				parameters.Add(pkg.ReadLong());
			}
			else if (parameterInfo.ParameterType == typeof(float))
			{
				parameters.Add(pkg.ReadSingle());
			}
			else if (parameterInfo.ParameterType == typeof(double))
			{
				parameters.Add(pkg.ReadDouble());
			}
			else if (parameterInfo.ParameterType == typeof(bool))
			{
				parameters.Add(pkg.ReadBool());
			}
			else if (parameterInfo.ParameterType == typeof(string))
			{
				parameters.Add(pkg.ReadString());
			}
			else if (parameterInfo.ParameterType == typeof(ZPackage))
			{
				parameters.Add(pkg.ReadPackage());
			}
			else if (parameterInfo.ParameterType == typeof(List<string>))
			{
				int num = pkg.ReadInt();
				List<string> list = new List<string>(num);
				for (int j = 0; j < num; j++)
				{
					list.Add(pkg.ReadString());
				}
				parameters.Add(list);
			}
			else if (parameterInfo.ParameterType == typeof(Vector3))
			{
				Vector3 vector = new Vector3(pkg.ReadSingle(), pkg.ReadSingle(), pkg.ReadSingle());
				parameters.Add(vector);
			}
			else if (parameterInfo.ParameterType == typeof(Quaternion))
			{
				Quaternion quaternion = new Quaternion(pkg.ReadSingle(), pkg.ReadSingle(), pkg.ReadSingle(), pkg.ReadSingle());
				parameters.Add(quaternion);
			}
			else if (parameterInfo.ParameterType == typeof(ZDOID))
			{
				parameters.Add(pkg.ReadZDOID());
			}
			else if (parameterInfo.ParameterType == typeof(HitData))
			{
				HitData hitData = new HitData();
				hitData.Deserialize(ref pkg);
				parameters.Add(hitData);
			}
		}
	}

	// Token: 0x04000866 RID: 2150
	private ISocket m_socket;

	// Token: 0x04000867 RID: 2151
	private ZPackage m_pkg = new ZPackage();

	// Token: 0x04000868 RID: 2152
	private Dictionary<int, ZRpc.RpcMethodBase> m_functions = new Dictionary<int, ZRpc.RpcMethodBase>();

	// Token: 0x04000869 RID: 2153
	private int m_sentPackages;

	// Token: 0x0400086A RID: 2154
	private int m_sentData;

	// Token: 0x0400086B RID: 2155
	private int m_recvPackages;

	// Token: 0x0400086C RID: 2156
	private int m_recvData;

	// Token: 0x0400086D RID: 2157
	private float m_pingTimer;

	// Token: 0x0400086E RID: 2158
	private float m_timeSinceLastPing;

	// Token: 0x0400086F RID: 2159
	private static float m_pingInterval = 1f;

	// Token: 0x04000870 RID: 2160
	private static float m_timeout = 30f;

	// Token: 0x04000871 RID: 2161
	private static bool m_DEBUG = false;

	// Token: 0x02000171 RID: 369
	private interface RpcMethodBase
	{
		// Token: 0x06001163 RID: 4451
		void Invoke(ZRpc rpc, ZPackage pkg);
	}

	// Token: 0x02000172 RID: 370
	public class RpcMethod : ZRpc.RpcMethodBase
	{
		// Token: 0x06001164 RID: 4452 RVA: 0x000787EC File Offset: 0x000769EC
		public RpcMethod(ZRpc.RpcMethod.Method action)
		{
			this.m_action = action;
		}

		// Token: 0x06001165 RID: 4453 RVA: 0x000787FB File Offset: 0x000769FB
		public void Invoke(ZRpc rpc, ZPackage pkg)
		{
			this.m_action(rpc);
		}

		// Token: 0x0400118A RID: 4490
		private ZRpc.RpcMethod.Method m_action;

		// Token: 0x020001C5 RID: 453
		// (Invoke) Token: 0x060011E5 RID: 4581
		public delegate void Method(ZRpc RPC);
	}

	// Token: 0x02000173 RID: 371
	private class RpcMethod<T> : ZRpc.RpcMethodBase
	{
		// Token: 0x06001166 RID: 4454 RVA: 0x00078809 File Offset: 0x00076A09
		public RpcMethod(Action<ZRpc, T> action)
		{
			this.m_action = action;
		}

		// Token: 0x06001167 RID: 4455 RVA: 0x00078818 File Offset: 0x00076A18
		public void Invoke(ZRpc rpc, ZPackage pkg)
		{
			this.m_action.DynamicInvoke(ZRpc.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
		}

		// Token: 0x0400118B RID: 4491
		private Action<ZRpc, T> m_action;
	}

	// Token: 0x02000174 RID: 372
	private class RpcMethod<T, U> : ZRpc.RpcMethodBase
	{
		// Token: 0x06001168 RID: 4456 RVA: 0x0007883D File Offset: 0x00076A3D
		public RpcMethod(Action<ZRpc, T, U> action)
		{
			this.m_action = action;
		}

		// Token: 0x06001169 RID: 4457 RVA: 0x0007884C File Offset: 0x00076A4C
		public void Invoke(ZRpc rpc, ZPackage pkg)
		{
			this.m_action.DynamicInvoke(ZRpc.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
		}

		// Token: 0x0400118C RID: 4492
		private Action<ZRpc, T, U> m_action;
	}

	// Token: 0x02000175 RID: 373
	private class RpcMethod<T, U, V> : ZRpc.RpcMethodBase
	{
		// Token: 0x0600116A RID: 4458 RVA: 0x00078871 File Offset: 0x00076A71
		public RpcMethod(Action<ZRpc, T, U, V> action)
		{
			this.m_action = action;
		}

		// Token: 0x0600116B RID: 4459 RVA: 0x00078880 File Offset: 0x00076A80
		public void Invoke(ZRpc rpc, ZPackage pkg)
		{
			this.m_action.DynamicInvoke(ZRpc.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
		}

		// Token: 0x0400118D RID: 4493
		private Action<ZRpc, T, U, V> m_action;
	}

	// Token: 0x02000176 RID: 374
	public class RpcMethod<T, U, V, B> : ZRpc.RpcMethodBase
	{
		// Token: 0x0600116C RID: 4460 RVA: 0x000788A5 File Offset: 0x00076AA5
		public RpcMethod(ZRpc.RpcMethod<T, U, V, B>.Method action)
		{
			this.m_action = action;
		}

		// Token: 0x0600116D RID: 4461 RVA: 0x000788B4 File Offset: 0x00076AB4
		public void Invoke(ZRpc rpc, ZPackage pkg)
		{
			this.m_action.DynamicInvoke(ZRpc.Deserialize(rpc, this.m_action.Method.GetParameters(), pkg));
		}

		// Token: 0x0400118E RID: 4494
		private ZRpc.RpcMethod<T, U, V, B>.Method m_action;

		// Token: 0x020001C6 RID: 454
		// (Invoke) Token: 0x060011E9 RID: 4585
		public delegate void Method(ZRpc RPC, T p0, U p1, V p2, B p3);
	}
}
