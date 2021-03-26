using System;
using UnityEngine;

// Token: 0x020000A2 RID: 162
public class LocationProxy : MonoBehaviour
{
	// Token: 0x06000B06 RID: 2822 RVA: 0x0004F759 File Offset: 0x0004D959
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.SpawnLocation();
	}

	// Token: 0x06000B07 RID: 2823 RVA: 0x0004F770 File Offset: 0x0004D970
	public void SetLocation(string location, int seed, bool spawnNow, int pgw)
	{
		int stableHashCode = location.GetStableHashCode();
		this.m_nview.GetZDO().Set("location", stableHashCode);
		this.m_nview.GetZDO().Set("seed", seed);
		this.m_nview.GetZDO().SetPGWVersion(pgw);
		if (spawnNow)
		{
			this.SpawnLocation();
		}
	}

	// Token: 0x06000B08 RID: 2824 RVA: 0x0004F7CC File Offset: 0x0004D9CC
	private bool SpawnLocation()
	{
		int @int = this.m_nview.GetZDO().GetInt("location", 0);
		int int2 = this.m_nview.GetZDO().GetInt("seed", 0);
		if (@int == 0)
		{
			return false;
		}
		this.m_instance = ZoneSystem.instance.SpawnProxyLocation(@int, int2, base.transform.position, base.transform.rotation);
		if (this.m_instance == null)
		{
			return false;
		}
		this.m_instance.transform.SetParent(base.transform, true);
		return true;
	}

	// Token: 0x04000A70 RID: 2672
	private GameObject m_instance;

	// Token: 0x04000A71 RID: 2673
	private ZNetView m_nview;
}
