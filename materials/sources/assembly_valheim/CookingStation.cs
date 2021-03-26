using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x020000C0 RID: 192
public class CookingStation : MonoBehaviour, Interactable, Hoverable
{
	// Token: 0x06000CBD RID: 3261 RVA: 0x0005B274 File Offset: 0x00059474
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

	// Token: 0x06000CBE RID: 3262 RVA: 0x0005B36C File Offset: 0x0005956C
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

	// Token: 0x06000CBF RID: 3263 RVA: 0x0005B4C4 File Offset: 0x000596C4
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

	// Token: 0x06000CC0 RID: 3264 RVA: 0x0005B4F7 File Offset: 0x000596F7
	private void RPC_SetSlotVisual(long sender, int slot, string item)
	{
		this.SetSlotVisual(slot, item);
	}

	// Token: 0x06000CC1 RID: 3265 RVA: 0x0005B504 File Offset: 0x00059704
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

	// Token: 0x06000CC2 RID: 3266 RVA: 0x0005B64C File Offset: 0x0005984C
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

	// Token: 0x06000CC3 RID: 3267 RVA: 0x0005B6D4 File Offset: 0x000598D4
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

	// Token: 0x06000CC4 RID: 3268 RVA: 0x0005B718 File Offset: 0x00059918
	private bool IsItemDone(string itemName)
	{
		if (itemName == this.m_overCookedItem.name)
		{
			return true;
		}
		CookingStation.ItemConversion itemConversion = this.GetItemConversion(itemName);
		return itemConversion != null && itemName == itemConversion.m_to.name;
	}

	// Token: 0x06000CC5 RID: 3269 RVA: 0x0005B760 File Offset: 0x00059960
	private void SpawnItem(string name)
	{
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(name);
		Vector3 vector = base.transform.position + Vector3.up * this.m_spawnOffset;
		Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
		UnityEngine.Object.Instantiate<GameObject>(itemPrefab, vector, rotation).GetComponent<Rigidbody>().velocity = Vector3.up * this.m_spawnForce;
		this.m_pickEffector.Create(vector, Quaternion.identity, null, 1f);
	}

	// Token: 0x06000CC6 RID: 3270 RVA: 0x0005B7EE File Offset: 0x000599EE
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_cstand_cook\n[<color=yellow><b>1-8</b></color>] $piece_cstand_cook");
	}

	// Token: 0x06000CC7 RID: 3271 RVA: 0x0005B80A File Offset: 0x00059A0A
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000CC8 RID: 3272 RVA: 0x0005B814 File Offset: 0x00059A14
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

	// Token: 0x06000CC9 RID: 3273 RVA: 0x0005B86F File Offset: 0x00059A6F
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

	// Token: 0x06000CCA RID: 3274 RVA: 0x0005B8AF File Offset: 0x00059AAF
	private bool IsFireLit()
	{
		return EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.Burning, 0.25f);
	}

	// Token: 0x06000CCB RID: 3275 RVA: 0x0005B8D4 File Offset: 0x00059AD4
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

	// Token: 0x06000CCC RID: 3276 RVA: 0x0005B948 File Offset: 0x00059B48
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

	// Token: 0x06000CCD RID: 3277 RVA: 0x0005B9B4 File Offset: 0x00059BB4
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

	// Token: 0x06000CCE RID: 3278 RVA: 0x0005BA30 File Offset: 0x00059C30
	private void SetSlot(int slot, string itemName, float cookedTime)
	{
		this.m_nview.GetZDO().Set("slot" + slot, itemName);
		this.m_nview.GetZDO().Set("slot" + slot, cookedTime);
	}

	// Token: 0x06000CCF RID: 3279 RVA: 0x0005BA80 File Offset: 0x00059C80
	private void GetSlot(int slot, out string itemName, out float cookedTime)
	{
		itemName = this.m_nview.GetZDO().GetString("slot" + slot, "");
		cookedTime = this.m_nview.GetZDO().GetFloat("slot" + slot, 0f);
	}

	// Token: 0x06000CD0 RID: 3280 RVA: 0x0005BADC File Offset: 0x00059CDC
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

	// Token: 0x06000CD1 RID: 3281 RVA: 0x0005BB30 File Offset: 0x00059D30
	private bool IsItemAllowed(ItemDrop.ItemData item)
	{
		return this.IsItemAllowed(item.m_dropPrefab.name);
	}

	// Token: 0x06000CD2 RID: 3282 RVA: 0x0005BB44 File Offset: 0x00059D44
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

	// Token: 0x06000CD3 RID: 3283 RVA: 0x0005BBB0 File Offset: 0x00059DB0
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

	// Token: 0x04000BA0 RID: 2976
	private const float cookDelta = 1f;

	// Token: 0x04000BA1 RID: 2977
	public EffectList m_addEffect = new EffectList();

	// Token: 0x04000BA2 RID: 2978
	public EffectList m_doneEffect = new EffectList();

	// Token: 0x04000BA3 RID: 2979
	public EffectList m_overcookedEffect = new EffectList();

	// Token: 0x04000BA4 RID: 2980
	public EffectList m_pickEffector = new EffectList();

	// Token: 0x04000BA5 RID: 2981
	public float m_spawnOffset = 0.5f;

	// Token: 0x04000BA6 RID: 2982
	public float m_spawnForce = 5f;

	// Token: 0x04000BA7 RID: 2983
	public ItemDrop m_overCookedItem;

	// Token: 0x04000BA8 RID: 2984
	public List<CookingStation.ItemConversion> m_conversion = new List<CookingStation.ItemConversion>();

	// Token: 0x04000BA9 RID: 2985
	public Transform[] m_slots;

	// Token: 0x04000BAA RID: 2986
	public string m_name = "";

	// Token: 0x04000BAB RID: 2987
	private ZNetView m_nview;

	// Token: 0x04000BAC RID: 2988
	private ParticleSystem[] m_ps;

	// Token: 0x04000BAD RID: 2989
	private AudioSource[] m_as;

	// Token: 0x02000193 RID: 403
	[Serializable]
	public class ItemConversion
	{
		// Token: 0x04001276 RID: 4726
		public ItemDrop m_from;

		// Token: 0x04001277 RID: 4727
		public ItemDrop m_to;

		// Token: 0x04001278 RID: 4728
		public float m_cookTime = 10f;
	}
}
