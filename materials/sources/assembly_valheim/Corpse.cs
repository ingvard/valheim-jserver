using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000006 RID: 6
public class Corpse : MonoBehaviour
{
	// Token: 0x060000E8 RID: 232 RVA: 0x00006F10 File Offset: 0x00005110
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_container = base.GetComponent<Container>();
		this.m_model = base.GetComponentInChildren<SkinnedMeshRenderer>();
		if (this.m_nview.IsOwner() && this.m_nview.GetZDO().GetLong("timeOfDeath", 0L) == 0L)
		{
			this.m_nview.GetZDO().Set("timeOfDeath", ZNet.instance.GetTime().Ticks);
		}
		base.InvokeRepeating("UpdateDespawn", Corpse.m_updateDt, Corpse.m_updateDt);
	}

	// Token: 0x060000E9 RID: 233 RVA: 0x00006FA4 File Offset: 0x000051A4
	public void SetEquipedItems(List<ItemDrop.ItemData> items)
	{
		foreach (ItemDrop.ItemData itemData in items)
		{
			if (itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Chest)
			{
				this.m_nview.GetZDO().Set("ChestItem", itemData.m_shared.m_name);
			}
			if (itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Legs)
			{
				this.m_nview.GetZDO().Set("LegItem", itemData.m_shared.m_name);
			}
		}
	}

	// Token: 0x060000EA RID: 234 RVA: 0x00007048 File Offset: 0x00005248
	private void UpdateDespawn()
	{
		if (this.m_nview.IsOwner() && !this.m_container.IsInUse())
		{
			if (this.m_container.GetInventory().NrOfItems() <= 0)
			{
				this.m_emptyTimer += Corpse.m_updateDt;
				if (this.m_emptyTimer >= this.m_emptyDespawnDelaySec)
				{
					ZLog.Log("Despawning looted corpse");
					this.m_nview.Destroy();
					return;
				}
			}
			else
			{
				this.m_emptyTimer = 0f;
			}
		}
	}

	// Token: 0x040000BB RID: 187
	private static float m_updateDt = 2f;

	// Token: 0x040000BC RID: 188
	public float m_emptyDespawnDelaySec = 10f;

	// Token: 0x040000BD RID: 189
	public float m_DespawnDelayMin = 20f;

	// Token: 0x040000BE RID: 190
	private float m_emptyTimer;

	// Token: 0x040000BF RID: 191
	private Container m_container;

	// Token: 0x040000C0 RID: 192
	private ZNetView m_nview;

	// Token: 0x040000C1 RID: 193
	private SkinnedMeshRenderer m_model;
}
