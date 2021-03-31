using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x020000C0 RID: 192
public class CookingStation : MonoBehaviour, Interactable, Hoverable
{
	// Token: 0x06000CBE RID: 3262 RVA: 0x0005B3FC File Offset: 0x000595FC
	private void Awake()
	{
		this.m_nview = base.gameObject.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.m_ps = new ParticleSystem[this.m_slots.Length];
		this.m_as = new AudioSource[this.m_slots.Length];
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			this.m_ps[i] = this.m_slots[i].GetComponentInChildren<ParticleSystem>();
			this.m_as[i] = this.m_slots[i].GetComponentInChildren<AudioSource>();
		}
		this.m_nview.Register("RemoveDoneItem", new Action<long>(this.RPC_RemoveDoneItem));
		this.m_nview.Register<string>("AddItem", new Action<long, string>(this.RPC_AddItem));
		this.m_nview.Register<int, string>("SetSlotVisual", new Action<long, int, string>(this.RPC_SetSlotVisual));
		base.InvokeRepeating("UpdateCooking", 0f, 1f);
	}

	// Token: 0x06000CBF RID: 3263 RVA: 0x0005B4F4 File Offset: 0x000596F4
	private void UpdateCooking()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview.IsOwner() && this.IsFireLit())
		{
			for (int i = 0; i < this.m_slots.Length; i++)
			{
				string text;
				float num;
				this.GetSlot(i, out text, out num);
				if (text != "" && text != this.m_overCookedItem.name)
				{
					CookingStation.ItemConversion itemConversion = this.GetItemConversion(text);
					if (text == null)
					{
						this.SetSlot(i, "", 0f);
					}
					else
					{
						num += 1f;
						if (num > itemConversion.m_cookTime * 2f)
						{
							this.m_overcookedEffect.Create(this.m_slots[i].position, Quaternion.identity, null, 1f);
							this.SetSlot(i, this.m_overCookedItem.name, num);
						}
						else if (num > itemConversion.m_cookTime && text == itemConversion.m_from.name)
						{
							this.m_doneEffect.Create(this.m_slots[i].position, Quaternion.identity, null, 1f);
							this.SetSlot(i, itemConversion.m_to.name, num);
						}
						else
						{
							this.SetSlot(i, text, num);
						}
					}
				}
			}
		}
		this.UpdateVisual();
	}

	// Token: 0x06000CC0 RID: 3264 RVA: 0x0005B64C File Offset: 0x0005984C
	private void UpdateVisual()
	{
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			string item;
			float num;
			this.GetSlot(i, out item, out num);
			this.SetSlotVisual(i, item);
		}
	}

	// Token: 0x06000CC1 RID: 3265 RVA: 0x0005B67F File Offset: 0x0005987F
	private void RPC_SetSlotVisual(long sender, int slot, string item)
	{
		this.SetSlotVisual(slot, item);
	}

	// Token: 0x06000CC2 RID: 3266 RVA: 0x0005B68C File Offset: 0x0005988C
	private void SetSlotVisual(int i, string item)
	{
		if (item == "")
		{
			this.m_ps[i].emission.enabled = false;
			this.m_as[i].mute = true;
			if (this.m_slots[i].childCount > 0)
			{
				UnityEngine.Object.Destroy(this.m_slots[i].GetChild(0).gameObject);
				return;
			}
		}
		else
		{
			this.m_ps[i].emission.enabled = true;
			this.m_as[i].mute = false;
			if (this.m_slots[i].childCount == 0 || this.m_slots[i].GetChild(0).name != item)
			{
				if (this.m_slots[i].childCount > 0)
				{
					UnityEngine.Object.Destroy(this.m_slots[i].GetChild(0).gameObject);
				}
				Component component = ObjectDB.instance.GetItemPrefab(item).transform.Find("attach");
				Transform transform = this.m_slots[i];
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(component.gameObject, transform.position, transform.rotation, transform);
				gameObject.name = item;
				Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].shadowCastingMode = ShadowCastingMode.Off;
				}
			}
		}
	}

	// Token: 0x06000CC3 RID: 3267 RVA: 0x0005B7D4 File Offset: 0x000599D4
	private void RPC_RemoveDoneItem(long sender)
	{
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			string text;
			float num;
			this.GetSlot(i, out text, out num);
			if (text != "" && this.IsItemDone(text))
			{
				this.SpawnItem(text);
				this.SetSlot(i, "", 0f);
				this.m_nview.InvokeRPC(ZNetView.Everybody, "SetSlotVisual", new object[]
				{
					i,
					""
				});
				return;
			}
		}
	}

	// Token: 0x06000CC4 RID: 3268 RVA: 0x0005B85C File Offset: 0x00059A5C
	private bool HaveDoneItem()
	{
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			string text;
			float num;
			this.GetSlot(i, out text, out num);
			if (text != "" && this.IsItemDone(text))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000CC5 RID: 3269 RVA: 0x0005B8A0 File Offset: 0x00059AA0
	private bool IsItemDone(string itemName)
	{
		if (itemName == this.m_overCookedItem.name)
		{
			return true;
		}
		CookingStation.ItemConversion itemConversion = this.GetItemConversion(itemName);
		return itemConversion != null && itemName == itemConversion.m_to.name;
	}

	// Token: 0x06000CC6 RID: 3270 RVA: 0x0005B8E8 File Offset: 0x00059AE8
	private void SpawnItem(string name)
	{
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(name);
		Vector3 vector = base.transform.position + Vector3.up * this.m_spawnOffset;
		Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
		UnityEngine.Object.Instantiate<GameObject>(itemPrefab, vector, rotation).GetComponent<Rigidbody>().velocity = Vector3.up * this.m_spawnForce;
		this.m_pickEffector.Create(vector, Quaternion.identity, null, 1f);
	}

	// Token: 0x06000CC7 RID: 3271 RVA: 0x0005B976 File Offset: 0x00059B76
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_cstand_cook\n[<color=yellow><b>1-8</b></color>] $piece_cstand_cook");
	}

	// Token: 0x06000CC8 RID: 3272 RVA: 0x0005B992 File Offset: 0x00059B92
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000CC9 RID: 3273 RVA: 0x0005B99C File Offset: 0x00059B9C
	public bool Interact(Humanoid user, bool hold)
	{
		if (hold)
		{
			return false;
		}
		if (this.HaveDoneItem())
		{
			this.m_nview.InvokeRPC("RemoveDoneItem", Array.Empty<object>());
			return true;
		}
		ItemDrop.ItemData itemData = this.FindCookableItem(user.GetInventory());
		if (itemData == null)
		{
			user.Message(MessageHud.MessageType.Center, "$msg_nocookitems", 0, null);
			return false;
		}
		this.UseItem(user, itemData);
		return true;
	}

	// Token: 0x06000CCA RID: 3274 RVA: 0x0005B9F7 File Offset: 0x00059BF7
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		if (!this.IsFireLit())
		{
			user.Message(MessageHud.MessageType.Center, "$msg_needfire", 0, null);
			return false;
		}
		if (this.GetFreeSlot() == -1)
		{
			user.Message(MessageHud.MessageType.Center, "$msg_nocookroom", 0, null);
			return false;
		}
		return this.CookItem(user.GetInventory(), item);
	}

	// Token: 0x06000CCB RID: 3275 RVA: 0x0005BA37 File Offset: 0x00059C37
	private bool IsFireLit()
	{
		return EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.Burning, 0.25f);
	}

	// Token: 0x06000CCC RID: 3276 RVA: 0x0005BA5C File Offset: 0x00059C5C
	private ItemDrop.ItemData FindCookableItem(Inventory inventory)
	{
		foreach (CookingStation.ItemConversion itemConversion in this.m_conversion)
		{
			ItemDrop.ItemData item = inventory.GetItem(itemConversion.m_from.m_itemData.m_shared.m_name);
			if (item != null)
			{
				return item;
			}
		}
		return null;
	}

	// Token: 0x06000CCD RID: 3277 RVA: 0x0005BAD0 File Offset: 0x00059CD0
	private bool CookItem(Inventory inventory, ItemDrop.ItemData item)
	{
		string name = item.m_dropPrefab.name;
		if (!this.m_nview.HasOwner())
		{
			this.m_nview.ClaimOwnership();
		}
		if (!this.IsItemAllowed(item))
		{
			return false;
		}
		if (this.GetFreeSlot() == -1)
		{
			return false;
		}
		inventory.RemoveOneItem(item);
		this.m_nview.InvokeRPC("AddItem", new object[]
		{
			name
		});
		return true;
	}

	// Token: 0x06000CCE RID: 3278 RVA: 0x0005BB3C File Offset: 0x00059D3C
	private void RPC_AddItem(long sender, string itemName)
	{
		if (!this.IsItemAllowed(itemName))
		{
			return;
		}
		int freeSlot = this.GetFreeSlot();
		if (freeSlot == -1)
		{
			return;
		}
		this.SetSlot(freeSlot, itemName, 0f);
		this.m_nview.InvokeRPC(ZNetView.Everybody, "SetSlotVisual", new object[]
		{
			freeSlot,
			itemName
		});
		this.m_addEffect.Create(this.m_slots[freeSlot].position, Quaternion.identity, null, 1f);
	}

	// Token: 0x06000CCF RID: 3279 RVA: 0x0005BBB8 File Offset: 0x00059DB8
	private void SetSlot(int slot, string itemName, float cookedTime)
	{
		this.m_nview.GetZDO().Set("slot" + slot, itemName);
		this.m_nview.GetZDO().Set("slot" + slot, cookedTime);
	}

	// Token: 0x06000CD0 RID: 3280 RVA: 0x0005BC08 File Offset: 0x00059E08
	private void GetSlot(int slot, out string itemName, out float cookedTime)
	{
		itemName = this.m_nview.GetZDO().GetString("slot" + slot, "");
		cookedTime = this.m_nview.GetZDO().GetFloat("slot" + slot, 0f);
	}

	// Token: 0x06000CD1 RID: 3281 RVA: 0x0005BC64 File Offset: 0x00059E64
	private int GetFreeSlot()
	{
		for (int i = 0; i < this.m_slots.Length; i++)
		{
			if (this.m_nview.GetZDO().GetString("slot" + i, "") == "")
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x06000CD2 RID: 3282 RVA: 0x0005BCB8 File Offset: 0x00059EB8
	private bool IsItemAllowed(ItemDrop.ItemData item)
	{
		return this.IsItemAllowed(item.m_dropPrefab.name);
	}

	// Token: 0x06000CD3 RID: 3283 RVA: 0x0005BCCC File Offset: 0x00059ECC
	private bool IsItemAllowed(string itemName)
	{
		using (List<CookingStation.ItemConversion>.Enumerator enumerator = this.m_conversion.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_from.gameObject.name == itemName)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000CD4 RID: 3284 RVA: 0x0005BD38 File Offset: 0x00059F38
	private CookingStation.ItemConversion GetItemConversion(string itemName)
	{
		foreach (CookingStation.ItemConversion itemConversion in this.m_conversion)
		{
			if (itemConversion.m_from.gameObject.name == itemName || itemConversion.m_to.gameObject.name == itemName)
			{
				return itemConversion;
			}
		}
		return null;
	}

	// Token: 0x04000BA6 RID: 2982
	private const float cookDelta = 1f;

	// Token: 0x04000BA7 RID: 2983
	public EffectList m_addEffect = new EffectList();

	// Token: 0x04000BA8 RID: 2984
	public EffectList m_doneEffect = new EffectList();

	// Token: 0x04000BA9 RID: 2985
	public EffectList m_overcookedEffect = new EffectList();

	// Token: 0x04000BAA RID: 2986
	public EffectList m_pickEffector = new EffectList();

	// Token: 0x04000BAB RID: 2987
	public float m_spawnOffset = 0.5f;

	// Token: 0x04000BAC RID: 2988
	public float m_spawnForce = 5f;

	// Token: 0x04000BAD RID: 2989
	public ItemDrop m_overCookedItem;

	// Token: 0x04000BAE RID: 2990
	public List<CookingStation.ItemConversion> m_conversion = new List<CookingStation.ItemConversion>();

	// Token: 0x04000BAF RID: 2991
	public Transform[] m_slots;

	// Token: 0x04000BB0 RID: 2992
	public string m_name = "";

	// Token: 0x04000BB1 RID: 2993
	private ZNetView m_nview;

	// Token: 0x04000BB2 RID: 2994
	private ParticleSystem[] m_ps;

	// Token: 0x04000BB3 RID: 2995
	private AudioSource[] m_as;

	// Token: 0x02000193 RID: 403
	[Serializable]
	public class ItemConversion
	{
		// Token: 0x0400127D RID: 4733
		public ItemDrop m_from;

		// Token: 0x0400127E RID: 4734
		public ItemDrop m_to;

		// Token: 0x0400127F RID: 4735
		public float m_cookTime = 10f;
	}
}
