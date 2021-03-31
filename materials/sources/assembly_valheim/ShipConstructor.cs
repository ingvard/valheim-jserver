using System;
using UnityEngine;

// Token: 0x020000F2 RID: 242
public class ShipConstructor : MonoBehaviour
{
	// Token: 0x06000EFA RID: 3834 RVA: 0x0006B3D8 File Offset: 0x000695D8
	private void Start()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview == null || this.m_nview.GetZDO() == null)
		{
			return;
		}
		if (this.m_nview.IsOwner() && this.m_nview.GetZDO().GetLong("spawntime", 0L) == 0L)
		{
			this.m_nview.GetZDO().Set("spawntime", ZNet.instance.GetTime().Ticks);
		}
		base.InvokeRepeating("UpdateConstruction", 5f, 1f);
		if (this.IsBuilt())
		{
			this.m_hideWhenConstructed.SetActive(false);
			return;
		}
	}

	// Token: 0x06000EFB RID: 3835 RVA: 0x0006B484 File Offset: 0x00069684
	private bool IsBuilt()
	{
		return this.m_nview.GetZDO().GetBool("done", false);
	}

	// Token: 0x06000EFC RID: 3836 RVA: 0x0006B49C File Offset: 0x0006969C
	private void UpdateConstruction()
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.IsBuilt())
		{
			this.m_hideWhenConstructed.SetActive(false);
			return;
		}
		DateTime time = ZNet.instance.GetTime();
		DateTime d = new DateTime(this.m_nview.GetZDO().GetLong("spawntime", 0L));
		if ((time - d).TotalMinutes > (double)this.m_constructionTimeMinutes)
		{
			this.m_hideWhenConstructed.SetActive(false);
			UnityEngine.Object.Instantiate<GameObject>(this.m_shipPrefab, this.m_spawnPoint.position, this.m_spawnPoint.rotation);
			this.m_nview.GetZDO().Set("done", true);
		}
	}

	// Token: 0x04000DEE RID: 3566
	public GameObject m_shipPrefab;

	// Token: 0x04000DEF RID: 3567
	public GameObject m_hideWhenConstructed;

	// Token: 0x04000DF0 RID: 3568
	public Transform m_spawnPoint;

	// Token: 0x04000DF1 RID: 3569
	public long m_constructionTimeMinutes = 1L;

	// Token: 0x04000DF2 RID: 3570
	private ZNetView m_nview;
}
