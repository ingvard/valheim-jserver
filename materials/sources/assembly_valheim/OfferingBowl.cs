using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000E5 RID: 229
public class OfferingBowl : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000E36 RID: 3638 RVA: 0x000027E0 File Offset: 0x000009E0
	private void Awake()
	{
	}

	// Token: 0x06000E37 RID: 3639 RVA: 0x00065798 File Offset: 0x00063998
	public string GetHoverText()
	{
		if (this.m_useItemStands)
		{
			return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] ") + Localization.instance.Localize(this.m_useItemText);
		}
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>1-8</b></color>] " + this.m_useItemText);
	}

	// Token: 0x06000E38 RID: 3640 RVA: 0x000657FD File Offset: 0x000639FD
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000E39 RID: 3641 RVA: 0x00065808 File Offset: 0x00063A08
	public bool Interact(Humanoid user, bool hold)
	{
		if (hold)
		{
			return false;
		}
		if (this.IsBossSpawnQueued())
		{
			return false;
		}
		if (this.m_useItemStands)
		{
			List<ItemStand> list = this.FindItemStands();
			using (List<ItemStand>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.HaveAttachment())
					{
						user.Message(MessageHud.MessageType.Center, "$msg_incompleteoffering", 0, null);
						return false;
					}
				}
			}
			if (this.SpawnBoss(base.transform.position))
			{
				user.Message(MessageHud.MessageType.Center, "$msg_offerdone", 0, null);
				foreach (ItemStand itemStand in list)
				{
					itemStand.DestroyAttachment();
				}
				if (this.m_itemSpawnPoint)
				{
					this.m_fuelAddedEffects.Create(this.m_itemSpawnPoint.position, base.transform.rotation, null, 1f);
				}
			}
			return true;
		}
		return false;
	}

	// Token: 0x06000E3A RID: 3642 RVA: 0x00065920 File Offset: 0x00063B20
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		if (this.m_useItemStands)
		{
			return false;
		}
		if (this.IsBossSpawnQueued())
		{
			return true;
		}
		if (!(this.m_bossItem != null))
		{
			return false;
		}
		if (!(item.m_shared.m_name == this.m_bossItem.m_itemData.m_shared.m_name))
		{
			user.Message(MessageHud.MessageType.Center, "$msg_offerwrong", 0, null);
			return true;
		}
		int num = user.GetInventory().CountItems(this.m_bossItem.m_itemData.m_shared.m_name);
		if (num < this.m_bossItems)
		{
			user.Message(MessageHud.MessageType.Center, string.Concat(new string[]
			{
				"$msg_incompleteoffering: ",
				this.m_bossItem.m_itemData.m_shared.m_name,
				" ",
				num.ToString(),
				" / ",
				this.m_bossItems.ToString()
			}), 0, null);
			return true;
		}
		if (this.m_bossPrefab != null)
		{
			if (this.SpawnBoss(base.transform.position))
			{
				user.GetInventory().RemoveItem(item.m_shared.m_name, this.m_bossItems);
				user.ShowRemovedMessage(this.m_bossItem.m_itemData, this.m_bossItems);
				user.Message(MessageHud.MessageType.Center, "$msg_offerdone", 0, null);
				if (this.m_itemSpawnPoint)
				{
					this.m_fuelAddedEffects.Create(this.m_itemSpawnPoint.position, base.transform.rotation, null, 1f);
				}
			}
		}
		else if (this.m_itemPrefab != null && this.SpawnItem(this.m_itemPrefab, user as Player))
		{
			user.GetInventory().RemoveItem(item.m_shared.m_name, this.m_bossItems);
			user.ShowRemovedMessage(this.m_bossItem.m_itemData, this.m_bossItems);
			user.Message(MessageHud.MessageType.Center, "$msg_offerdone", 0, null);
			this.m_fuelAddedEffects.Create(this.m_itemSpawnPoint.position, base.transform.rotation, null, 1f);
		}
		if (!string.IsNullOrEmpty(this.m_setGlobalKey))
		{
			ZoneSystem.instance.SetGlobalKey(this.m_setGlobalKey);
		}
		return true;
	}

	// Token: 0x06000E3B RID: 3643 RVA: 0x00065B64 File Offset: 0x00063D64
	private bool SpawnItem(ItemDrop item, Player player)
	{
		if (item.m_itemData.m_shared.m_questItem && player.HaveUniqueKey(item.m_itemData.m_shared.m_name))
		{
			player.Message(MessageHud.MessageType.Center, "$msg_cantoffer", 0, null);
			return false;
		}
		UnityEngine.Object.Instantiate<ItemDrop>(item, this.m_itemSpawnPoint.position, Quaternion.identity);
		return true;
	}

	// Token: 0x06000E3C RID: 3644 RVA: 0x00065BC4 File Offset: 0x00063DC4
	private bool SpawnBoss(Vector3 point)
	{
		for (int i = 0; i < 100; i++)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle * this.m_spawnBossMaxDistance;
			Vector3 vector2 = point + new Vector3(vector.x, 0f, vector.y);
			float solidHeight = ZoneSystem.instance.GetSolidHeight(vector2);
			if (solidHeight >= 0f && Mathf.Abs(solidHeight - base.transform.position.y) <= this.m_spawnBossMaxYDistance)
			{
				vector2.y = solidHeight + this.m_spawnOffset;
				this.m_spawnBossStartEffects.Create(vector2, Quaternion.identity, null, 1f);
				this.m_bossSpawnPoint = vector2;
				base.Invoke("DelayedSpawnBoss", this.m_spawnBossDelay);
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000E3D RID: 3645 RVA: 0x00065C87 File Offset: 0x00063E87
	private bool IsBossSpawnQueued()
	{
		return base.IsInvoking("DelayedSpawnBoss");
	}

	// Token: 0x06000E3E RID: 3646 RVA: 0x00065C94 File Offset: 0x00063E94
	private void DelayedSpawnBoss()
	{
		BaseAI component = UnityEngine.Object.Instantiate<GameObject>(this.m_bossPrefab, this.m_bossSpawnPoint, Quaternion.identity).GetComponent<BaseAI>();
		if (component != null)
		{
			component.SetPatrolPoint();
		}
		this.m_spawnBossDoneffects.Create(this.m_bossSpawnPoint, Quaternion.identity, null, 1f);
	}

	// Token: 0x06000E3F RID: 3647 RVA: 0x00065CEC File Offset: 0x00063EEC
	private List<ItemStand> FindItemStands()
	{
		List<ItemStand> list = new List<ItemStand>();
		foreach (ItemStand itemStand in UnityEngine.Object.FindObjectsOfType<ItemStand>())
		{
			if (Vector3.Distance(base.transform.position, itemStand.transform.position) <= this.m_itemstandMaxRange && itemStand.gameObject.name.StartsWith(this.m_itemStandPrefix))
			{
				list.Add(itemStand);
			}
		}
		return list;
	}

	// Token: 0x04000CEE RID: 3310
	public string m_name = "Ancient bowl";

	// Token: 0x04000CEF RID: 3311
	public string m_useItemText = "Burn item";

	// Token: 0x04000CF0 RID: 3312
	public ItemDrop m_bossItem;

	// Token: 0x04000CF1 RID: 3313
	public int m_bossItems = 1;

	// Token: 0x04000CF2 RID: 3314
	public GameObject m_bossPrefab;

	// Token: 0x04000CF3 RID: 3315
	public ItemDrop m_itemPrefab;

	// Token: 0x04000CF4 RID: 3316
	public Transform m_itemSpawnPoint;

	// Token: 0x04000CF5 RID: 3317
	public string m_setGlobalKey = "";

	// Token: 0x04000CF6 RID: 3318
	[Header("Boss")]
	public float m_spawnBossDelay = 5f;

	// Token: 0x04000CF7 RID: 3319
	public float m_spawnBossMaxDistance = 40f;

	// Token: 0x04000CF8 RID: 3320
	public float m_spawnBossMaxYDistance = 9999f;

	// Token: 0x04000CF9 RID: 3321
	public float m_spawnOffset = 1f;

	// Token: 0x04000CFA RID: 3322
	[Header("Use itemstands")]
	public bool m_useItemStands;

	// Token: 0x04000CFB RID: 3323
	public string m_itemStandPrefix = "";

	// Token: 0x04000CFC RID: 3324
	public float m_itemstandMaxRange = 20f;

	// Token: 0x04000CFD RID: 3325
	[Header("Effects")]
	public EffectList m_fuelAddedEffects = new EffectList();

	// Token: 0x04000CFE RID: 3326
	public EffectList m_spawnBossStartEffects = new EffectList();

	// Token: 0x04000CFF RID: 3327
	public EffectList m_spawnBossDoneffects = new EffectList();

	// Token: 0x04000D00 RID: 3328
	private Vector3 m_bossSpawnPoint;
}
