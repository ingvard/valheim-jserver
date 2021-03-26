using System;
using UnityEngine;

// Token: 0x020000F2 RID: 242
public class ShipConstructor : MonoBehaviour
{
	// Token: 0x06000EF9 RID: 3833 RVA: 0x0006B250 File Offset: 0x00069450
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

	// Token: 0x06000EFA RID: 3834 RVA: 0x0006B2FC File Offset: 0x000694FC
	private bool IsBuilt()
	{
		return this.m_nview.GetZDO().GetBool("done", false);
	}

	// Token: 0x06000EFB RID: 3835 RVA: 0x0006B314 File Offset: 0x00069514
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

	// Token: 0x04000DE8 RID: 3560
	public GameObject m_shipPrefab;

	// Token: 0x04000DE9 RID: 3561
	public GameObject m_hideWhenConstructed;

	// Token: 0x04000DEA RID: 3562
	public Transform m_spawnPoint;

	// Token: 0x04000DEB RID: 3563
	public long m_constructionTimeMinutes = 1L;

	// Token: 0x04000DEC RID: 3564
	private ZNetView m_nview;
}
