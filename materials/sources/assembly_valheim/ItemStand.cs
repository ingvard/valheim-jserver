using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000D8 RID: 216
public class ItemStand : MonoBehaviour, Interactable, Hoverable
{
	// Token: 0x06000DD0 RID: 3536 RVA: 0x00062ACC File Offset: 0x00060CCC
	private void Awake()
	{
		this.m_nview = (this.m_netViewOverride ? this.m_netViewOverride : base.gameObject.GetComponent<ZNetView>());
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		WearNTear component = base.GetComponent<WearNTear>();
		if (component)
		{
			WearNTear wearNTear = component;
			wearNTear.m_onDestroyed = (Action)Delegate.Combine(wearNTear.m_onDestroyed, new Action(this.OnDestroyed));
		}
		this.m_nview.Register("DropItem", new Action<long>(this.RPC_DropItem));
		this.m_nview.Register("RequestOwn", new Action<long>(this.RPC_RequestOwn));
		this.m_nview.Register("DestroyAttachment", new Action<long>(this.RPC_DestroyAttachment));
		this.m_nview.Register<string, int>("SetVisualItem", new Action<long, string, int>(this.RPC_SetVisualItem));
		base.InvokeRepeating("UpdateVisual", 1f, 4f);
	}

	// Token: 0x06000DD1 RID: 3537 RVA: 0x00062BC3 File Offset: 0x00060DC3
	private void OnDestroyed()
	{
		if (this.m_nview.IsOwner())
		{
			this.DropItem();
		}
	}

	// Token: 0x06000DD2 RID: 3538 RVA: 0x00062BD8 File Offset: 0x00060DD8
	public string GetHoverText()
	{
		if (!Player.m_localPlayer)
		{
			return "";
		}
		if (this.HaveAttachment())
		{
			if (this.m_canBeRemoved)
			{
				return Localization.instance.Localize(this.m_name + " ( " + this.m_currentItemName + " )\n[<color=yellow><b>$KEY_Use</b></color>] $piece_itemstand_take");
			}
			if (!(this.m_guardianPower != null))
			{
				return "";
			}
			if (base.IsInvoking("DelayedPowerActivation"))
			{
				return "";
			}
			if (this.IsGuardianPowerActive(Player.m_localPlayer))
			{
				return "";
			}
			string tooltipString = this.m_guardianPower.GetTooltipString();
			return Localization.instance.Localize(string.Concat(new string[]
			{
				"<color=orange>",
				this.m_guardianPower.m_name,
				"</color>\n",
				tooltipString,
				"\n\n[<color=yellow><b>$KEY_Use</b></color>] $guardianstone_hook_activate"
			}));
		}
		else
		{
			if (this.m_autoAttach && this.m_supportedItems.Count == 1)
			{
				return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_itemstand_attach");
			}
			return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>1-8</b></color>] $piece_itemstand_attach");
		}
	}

	// Token: 0x06000DD3 RID: 3539 RVA: 0x00062D04 File Offset: 0x00060F04
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000DD4 RID: 3540 RVA: 0x00062D0C File Offset: 0x00060F0C
	public bool Interact(Humanoid user, bool hold)
	{
		if (hold)
		{
			return false;
		}
		if (!this.HaveAttachment())
		{
			if (this.m_autoAttach && this.m_supportedItems.Count == 1)
			{
				ItemDrop.ItemData item = user.GetInventory().GetItem(this.m_supportedItems[0].m_itemData.m_shared.m_name);
				if (item != null)
				{
					this.UseItem(user, item);
					return true;
				}
				user.Message(MessageHud.MessageType.Center, "$piece_itemstand_missingitem", 0, null);
				return false;
			}
		}
		else
		{
			if (this.m_canBeRemoved)
			{
				this.m_nview.InvokeRPC("DropItem", Array.Empty<object>());
				return true;
			}
			if (this.m_guardianPower != null)
			{
				if (base.IsInvoking("DelayedPowerActivation"))
				{
					return false;
				}
				if (this.IsGuardianPowerActive(user))
				{
					return false;
				}
				user.Message(MessageHud.MessageType.Center, "$guardianstone_hook_power_activate ", 0, null);
				this.m_activatePowerEffects.Create(base.transform.position, base.transform.rotation, null, 1f);
				this.m_activatePowerEffectsPlayer.Create(user.transform.position, Quaternion.identity, user.transform, 1f);
				base.Invoke("DelayedPowerActivation", this.m_powerActivationDelay);
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000DD5 RID: 3541 RVA: 0x00062E42 File Offset: 0x00061042
	private bool IsGuardianPowerActive(Humanoid user)
	{
		return (user as Player).GetGuardianPowerName() == this.m_guardianPower.name;
	}

	// Token: 0x06000DD6 RID: 3542 RVA: 0x00062E60 File Offset: 0x00061060
	private void DelayedPowerActivation()
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null)
		{
			return;
		}
		localPlayer.SetGuardianPower(this.m_guardianPower.name);
	}

	// Token: 0x06000DD7 RID: 3543 RVA: 0x00062E90 File Offset: 0x00061090
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		if (this.HaveAttachment())
		{
			return false;
		}
		if (!this.CanAttach(item))
		{
			user.Message(MessageHud.MessageType.Center, "$piece_itemstand_cantattach", 0, null);
			return true;
		}
		if (!this.m_nview.IsOwner())
		{
			this.m_nview.InvokeRPC("RequestOwn", Array.Empty<object>());
		}
		this.m_queuedItem = item;
		base.CancelInvoke("UpdateAttach");
		base.InvokeRepeating("UpdateAttach", 0f, 0.1f);
		return true;
	}

	// Token: 0x06000DD8 RID: 3544 RVA: 0x00062F0A File Offset: 0x0006110A
	private void RPC_DropItem(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.m_canBeRemoved)
		{
			return;
		}
		this.DropItem();
	}

	// Token: 0x06000DD9 RID: 3545 RVA: 0x00062F29 File Offset: 0x00061129
	public void DestroyAttachment()
	{
		this.m_nview.InvokeRPC("DestroyAttachment", Array.Empty<object>());
	}

	// Token: 0x06000DDA RID: 3546 RVA: 0x00062F40 File Offset: 0x00061140
	public void RPC_DestroyAttachment(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.HaveAttachment())
		{
			return;
		}
		this.m_nview.GetZDO().Set("item", "");
		this.m_nview.InvokeRPC(ZNetView.Everybody, "SetVisualItem", new object[]
		{
			"",
			0
		});
		this.m_destroyEffects.Create(this.m_dropSpawnPoint.position, Quaternion.identity, null, 1f);
	}

	// Token: 0x06000DDB RID: 3547 RVA: 0x00062FCC File Offset: 0x000611CC
	private void DropItem()
	{
		if (!this.HaveAttachment())
		{
			return;
		}
		string @string = this.m_nview.GetZDO().GetString("item", "");
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(@string);
		if (itemPrefab)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(itemPrefab, this.m_dropSpawnPoint.position, this.m_dropSpawnPoint.rotation);
			gameObject.GetComponent<ItemDrop>().LoadFromExternalZDO(this.m_nview.GetZDO());
			gameObject.GetComponent<Rigidbody>().velocity = Vector3.up * 4f;
			this.m_effects.Create(this.m_dropSpawnPoint.position, Quaternion.identity, null, 1f);
		}
		this.m_nview.GetZDO().Set("item", "");
		this.m_nview.InvokeRPC(ZNetView.Everybody, "SetVisualItem", new object[]
		{
			"",
			0
		});
	}

	// Token: 0x06000DDC RID: 3548 RVA: 0x000630C4 File Offset: 0x000612C4
	private Transform GetAttach(ItemDrop.ItemData item)
	{
		return this.m_attachOther;
	}

	// Token: 0x06000DDD RID: 3549 RVA: 0x000630CC File Offset: 0x000612CC
	private void UpdateAttach()
	{
		if (this.m_nview.IsOwner())
		{
			base.CancelInvoke("UpdateAttach");
			Player localPlayer = Player.m_localPlayer;
			if (this.m_queuedItem != null && localPlayer != null && localPlayer.GetInventory().ContainsItem(this.m_queuedItem) && !this.HaveAttachment())
			{
				ItemDrop.ItemData itemData = this.m_queuedItem.Clone();
				itemData.m_stack = 1;
				this.m_nview.GetZDO().Set("item", this.m_queuedItem.m_dropPrefab.name);
				ItemDrop.SaveToZDO(itemData, this.m_nview.GetZDO());
				localPlayer.UnequipItem(this.m_queuedItem, true);
				localPlayer.GetInventory().RemoveOneItem(this.m_queuedItem);
				this.m_nview.InvokeRPC(ZNetView.Everybody, "SetVisualItem", new object[]
				{
					itemData.m_dropPrefab.name,
					itemData.m_variant
				});
				Transform attach = this.GetAttach(this.m_queuedItem);
				this.m_effects.Create(attach.transform.position, Quaternion.identity, null, 1f);
			}
			this.m_queuedItem = null;
		}
	}

	// Token: 0x06000DDE RID: 3550 RVA: 0x00063207 File Offset: 0x00061407
	private void RPC_RequestOwn(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		this.m_nview.GetZDO().SetOwner(sender);
	}

	// Token: 0x06000DDF RID: 3551 RVA: 0x00063228 File Offset: 0x00061428
	private void UpdateVisual()
	{
		string @string = this.m_nview.GetZDO().GetString("item", "");
		int @int = this.m_nview.GetZDO().GetInt("variant", 0);
		this.SetVisualItem(@string, @int);
	}

	// Token: 0x06000DE0 RID: 3552 RVA: 0x0006326F File Offset: 0x0006146F
	private void RPC_SetVisualItem(long sender, string itemName, int variant)
	{
		this.SetVisualItem(itemName, variant);
	}

	// Token: 0x06000DE1 RID: 3553 RVA: 0x0006327C File Offset: 0x0006147C
	private void SetVisualItem(string itemName, int variant)
	{
		if (this.m_visualName == itemName && this.m_visualVariant == variant)
		{
			return;
		}
		this.m_visualName = itemName;
		this.m_visualVariant = variant;
		this.m_currentItemName = "";
		if (this.m_visualName == "")
		{
			UnityEngine.Object.Destroy(this.m_visualItem);
			return;
		}
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemName);
		if (itemPrefab == null)
		{
			ZLog.LogWarning("Missing item prefab " + itemName);
			return;
		}
		GameObject attachPrefab = this.GetAttachPrefab(itemPrefab);
		if (attachPrefab == null)
		{
			ZLog.LogWarning("Failed to get attach prefab for item " + itemName);
			return;
		}
		ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
		this.m_currentItemName = component.m_itemData.m_shared.m_name;
		Transform attach = this.GetAttach(component.m_itemData);
		this.m_visualItem = UnityEngine.Object.Instantiate<GameObject>(attachPrefab, attach.position, attach.rotation, attach);
		this.m_visualItem.transform.localPosition = attachPrefab.transform.localPosition;
		this.m_visualItem.transform.localRotation = attachPrefab.transform.localRotation;
		IEquipmentVisual componentInChildren = this.m_visualItem.GetComponentInChildren<IEquipmentVisual>();
		if (componentInChildren != null)
		{
			componentInChildren.Setup(this.m_visualVariant);
		}
	}

	// Token: 0x06000DE2 RID: 3554 RVA: 0x000633B8 File Offset: 0x000615B8
	private GameObject GetAttachPrefab(GameObject item)
	{
		Transform transform = item.transform.Find("attach");
		if (transform)
		{
			return transform.gameObject;
		}
		return null;
	}

	// Token: 0x06000DE3 RID: 3555 RVA: 0x000633E8 File Offset: 0x000615E8
	private bool CanAttach(ItemDrop.ItemData item)
	{
		return !(this.GetAttachPrefab(item.m_dropPrefab) == null) && !this.IsUnsupported(item) && this.IsSupported(item) && this.m_supportedTypes.Contains(item.m_shared.m_itemType);
	}

	// Token: 0x06000DE4 RID: 3556 RVA: 0x00063438 File Offset: 0x00061638
	public bool IsUnsupported(ItemDrop.ItemData item)
	{
		using (List<ItemDrop>.Enumerator enumerator = this.m_unsupportedItems.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_itemData.m_shared.m_name == item.m_shared.m_name)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000DE5 RID: 3557 RVA: 0x000634AC File Offset: 0x000616AC
	public bool IsSupported(ItemDrop.ItemData item)
	{
		if (this.m_supportedItems.Count == 0)
		{
			return true;
		}
		using (List<ItemDrop>.Enumerator enumerator = this.m_supportedItems.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_itemData.m_shared.m_name == item.m_shared.m_name)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000DE6 RID: 3558 RVA: 0x00063530 File Offset: 0x00061730
	public bool HaveAttachment()
	{
		return this.m_nview.IsValid() && this.m_nview.GetZDO().GetString("item", "") != "";
	}

	// Token: 0x06000DE7 RID: 3559 RVA: 0x00063565 File Offset: 0x00061765
	public string GetAttachedItem()
	{
		if (!this.m_nview.IsValid())
		{
			return "";
		}
		return this.m_nview.GetZDO().GetString("item", "");
	}

	// Token: 0x04000C7E RID: 3198
	public ZNetView m_netViewOverride;

	// Token: 0x04000C7F RID: 3199
	public string m_name = "";

	// Token: 0x04000C80 RID: 3200
	public Transform m_attachOther;

	// Token: 0x04000C81 RID: 3201
	public Transform m_dropSpawnPoint;

	// Token: 0x04000C82 RID: 3202
	public bool m_canBeRemoved = true;

	// Token: 0x04000C83 RID: 3203
	public bool m_autoAttach;

	// Token: 0x04000C84 RID: 3204
	public List<ItemDrop.ItemData.ItemType> m_supportedTypes = new List<ItemDrop.ItemData.ItemType>();

	// Token: 0x04000C85 RID: 3205
	public List<ItemDrop> m_unsupportedItems = new List<ItemDrop>();

	// Token: 0x04000C86 RID: 3206
	public List<ItemDrop> m_supportedItems = new List<ItemDrop>();

	// Token: 0x04000C87 RID: 3207
	public EffectList m_effects = new EffectList();

	// Token: 0x04000C88 RID: 3208
	public EffectList m_destroyEffects = new EffectList();

	// Token: 0x04000C89 RID: 3209
	[Header("Guardian power")]
	public float m_powerActivationDelay = 2f;

	// Token: 0x04000C8A RID: 3210
	public StatusEffect m_guardianPower;

	// Token: 0x04000C8B RID: 3211
	public EffectList m_activatePowerEffects = new EffectList();

	// Token: 0x04000C8C RID: 3212
	public EffectList m_activatePowerEffectsPlayer = new EffectList();

	// Token: 0x04000C8D RID: 3213
	private string m_visualName = "";

	// Token: 0x04000C8E RID: 3214
	private int m_visualVariant;

	// Token: 0x04000C8F RID: 3215
	private GameObject m_visualItem;

	// Token: 0x04000C90 RID: 3216
	private string m_currentItemName = "";

	// Token: 0x04000C91 RID: 3217
	private ItemDrop.ItemData m_queuedItem;

	// Token: 0x04000C92 RID: 3218
	private ZNetView m_nview;
}
