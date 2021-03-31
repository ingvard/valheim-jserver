using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200007A RID: 122
public class ZDOPool
{
	// Token: 0x060007F4 RID: 2036 RVA: 0x0003EA27 File Offset: 0x0003CC27
	public static ZDO Create(ZDOMan man, ZDOID id, Vector3 position)
	{
		ZDO zdo = ZDOPool.Get();
		zdo.Initialize(man, id, position);
		return zdo;
	}

	// Token: 0x060007F5 RID: 2037 RVA: 0x0003EA37 File Offset: 0x0003CC37
	public static ZDO Create(ZDOMan man)
	{
		ZDO zdo = ZDOPool.Get();
		zdo.Initialize(man);
		return zdo;
	}

	// Token: 0x060007F6 RID: 2038 RVA: 0x0003EA48 File Offset: 0x0003CC48
	public static void Release(Dictionary<ZDOID, ZDO> objects)
	{
		foreach (ZDO zdo in objects.Values)
		{
			ZDOPool.Release(zdo);
		}
	}

	// Token: 0x060007F7 RID: 2039 RVA: 0x0003EA98 File Offset: 0x0003CC98
	public static void Release(ZDO zdo)
	{
		zdo.Reset();
		ZDOPool.m_free.Push(zdo);
		ZDOPool.m_active--;
	}

	// Token: 0x060007F8 RID: 2040 RVA: 0x0003EAB8 File Offset: 0x0003CCB8
	private static ZDO Get()
	{
		if (ZDOPool.m_free.Count <= 0)
		{
			for (int i = 0; i < ZDOPool.BATCH_SIZE; i++)
			{
				ZDO item = new ZDO();
				ZDOPool.m_free.Push(item);
			}
		}
		ZDOPool.m_active++;
		return ZDOPool.m_free.Pop();
	}

	// Token: 0x060007F9 RID: 2041 RVA: 0x0003EB09 File Offset: 0x0003CD09
	public static int GetPoolSize()
	{
		return ZDOPool.m_free.Count;
	}

	// Token: 0x060007FA RID: 2042 RVA: 0x0003EB15 File Offset: 0x0003CD15
	public static int GetPoolActive()
	{
		return ZDOPool.m_active;
	}

	// Token: 0x060007FB RID: 2043 RVA: 0x0003EB1C File Offset: 0x0003CD1C
	public static int GetPoolTotal()
	{
		return ZDOPool.m_active + ZDOPool.m_free.Count;
	}

	// Token: 0x040007F6 RID: 2038
	private static int BATCH_SIZE = 64;

	// Token: 0x040007F7 RID: 2039
	private static Stack<ZDO> m_free = new Stack<ZDO>();

	// Token: 0x040007F8 RID: 2040
	private static int m_active = 0;
}
