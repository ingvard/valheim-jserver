using System;
using UnityEngine;

// Token: 0x0200006D RID: 109
public class Pickable : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x060006E6 RID: 1766 RVA: 0x00038E54 File Offset: 0x00037054
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		ZDO zdo = this.m_nview.GetZDO();
		if (zdo == null)
		{
			return;
		}
		this.m_nview.Register<bool>("SetPicked", new Action<long, bool>(this.RPC_SetPicked));
		this.m_nview.Register("Pick", new Action<long>(this.RPC_Pick));
		this.m_picked = zdo.GetBool("picked", false);
		this.SetPicked(this.m_picked);
		if (this.m_respawnTimeMinutes > 0)
		{
			base.InvokeRepeating("UpdateRespawn", UnityEngine.Random.Range(1f, 5f), 60f);
		}
	}

	// Token: 0x060006E7 RID: 1767 RVA: 0x00038EFB File Offset: 0x000370FB
	public string GetHoverText()
	{
		if (this.m_picked)
		{
			return "";
		}
		return Localization.instance.Localize(this.GetHoverName() + "\n[<color=yellow><b>$KEY_Use</b></color>] $inventory_pickup");
	}

	// Token: 0x060006E8 RID: 1768 RVA: 0x00038F25 File Offset: 0x00037125
	public string GetHoverName()
	{
		if (!string.IsNullOrEmpty(this.m_overrideName))
		{
			return this.m_overrideName;
		}
		return this.m_itemPrefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_name;
	}

	// Token: 0x060006E9 RID: 1769 RVA: 0x00038F58 File Offset: 0x00037158
	private void UpdateRespawn()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.m_picked)
		{
			return;
		}
		long @long = this.m_nview.GetZDO().GetLong("picked_time", 0L);
		DateTime d = new DateTime(@long);
		if ((ZNet.instance.GetTime() - d).TotalMinutes > (double)this.m_respawnTimeMinutes)
		{
			this.m_nview.InvokeRPC(ZNetView.Everybody, "SetPicked", new object[]
			{
				false
			});
		}
	}

	// Token: 0x060006EA RID: 1770 RVA: 0x00038FEE File Offset: 0x000371EE
	public bool Interact(Humanoid character, bool repeat)
	{
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		this.m_nview.InvokeRPC("Pick", Array.Empty<object>());
		return this.m_useInteractAnimation;
	}

	// Token: 0x060006EB RID: 1771 RVA: 0x0003901C File Offset: 0x0003721C
	private void RPC_Pick(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_picked)
		{
			return;
		}
		Vector3 pos = this.m_pickEffectAtSpawnPoint ? (base.transform.position + Vector3.up * this.m_spawnOffset) : base.transform.position;
		this.m_pickEffector.Create(pos, Quaternion.identity, null, 1f);
		int num = 0;
		for (int i = 0; i < this.m_amount; i++)
		{
			this.Drop(this.m_itemPrefab, num++, 1);
		}
		if (!this.m_extraDrops.IsEmpty())
		{
			foreach (ItemDrop.ItemData itemData in this.m_extraDrops.GetDropListItems())
			{
				this.Drop(itemData.m_dropPrefab, num++, itemData.m_stack);
			}
		}
		this.m_nview.InvokeRPC(ZNetView.Everybody, "SetPicked", new object[]
		{
			true
		});
	}

	// Token: 0x060006EC RID: 1772 RVA: 0x00039144 File Offset: 0x00037344
	private void RPC_SetPicked(long sender, bool picked)
	{
		this.SetPicked(picked);
	}

	// Token: 0x060006ED RID: 1773 RVA: 0x00039150 File Offset: 0x00037350
	private void SetPicked(bool picked)
	{
		this.m_picked = picked;
		if (this.m_hideWhenPicked)
		{
			this.m_hideWhenPicked.SetActive(!picked);
		}
		if (this.m_nview.IsOwner())
		{
			if (this.m_respawnTimeMinutes > 0 || this.m_hideWhenPicked != null)
			{
				this.m_nview.GetZDO().Set("picked", this.m_picked);
				if (picked && this.m_respawnTimeMinutes > 0)
				{
					DateTime time = ZNet.instance.GetTime();
					this.m_nview.GetZDO().Set("picked_time", time.Ticks);
					return;
				}
			}
			else if (picked)
			{
				this.m_nview.Destroy();
			}
		}
	}

	// Token: 0x060006EE RID: 1774 RVA: 0x00039204 File Offset: 0x00037404
	private void Drop(GameObject prefab, int offset, int stack)
	{
		Vector2 vector = UnityEngine.Random.insideUnitCircle * 0.2f;
		Vector3 position = base.transform.position + Vector3.up * this.m_spawnOffset + new Vector3(vector.x, 0.5f * (float)offset, vector.y);
		Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab, position, rotation);
		gameObject.GetComponent<ItemDrop>().SetStack(stack);
		gameObject.GetComponent<Rigidbody>().velocity = Vector3.up * 4f;
	}

	// Token: 0x060006EF RID: 1775 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x04000771 RID: 1905
	public GameObject m_hideWhenPicked;

	// Token: 0x04000772 RID: 1906
	public GameObject m_itemPrefab;

	// Token: 0x04000773 RID: 1907
	public int m_amount = 1;

	// Token: 0x04000774 RID: 1908
	public DropTable m_extraDrops = new DropTable();

	// Token: 0x04000775 RID: 1909
	public string m_overrideName = "";

	// Token: 0x04000776 RID: 1910
	public int m_respawnTimeMinutes;

	// Token: 0x04000777 RID: 1911
	public float m_spawnOffset = 0.5f;

	// Token: 0x04000778 RID: 1912
	public EffectList m_pickEffector = new EffectList();

	// Token: 0x04000779 RID: 1913
	public bool m_pickEffectAtSpawnPoint;

	// Token: 0x0400077A RID: 1914
	public bool m_useInteractAnimation;

	// Token: 0x0400077B RID: 1915
	private ZNetView m_nview;

	// Token: 0x0400077C RID: 1916
	private bool m_picked;
}
