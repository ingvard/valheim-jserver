﻿using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000CA RID: 202
public class Fermenter : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000D08 RID: 3336 RVA: 0x0005CF6C File Offset: 0x0005B16C
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_fermentingObject.SetActive(false);
		this.m_readyObject.SetActive(false);
		this.m_topObject.SetActive(true);
		if (this.m_nview == null || this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.m_nview.Register<string>("AddItem", new Action<long, string>(this.RPC_AddItem));
		this.m_nview.Register("Tap", new Action<long>(this.RPC_Tap));
		base.InvokeRepeating("UpdateVis", 2f, 2f);
	}

	// Token: 0x06000D09 RID: 3337 RVA: 0x0005D012 File Offset: 0x0005B212
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000D0A RID: 3338 RVA: 0x0005D01C File Offset: 0x0005B21C
	public string GetHoverText()
	{
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, false, false))
		{
			return Localization.instance.Localize(this.m_name + "\n$piece_noaccess");
		}
		switch (this.GetStatus())
		{
		case Fermenter.Status.Empty:
			return Localization.instance.Localize(this.m_name + " ( $piece_container_empty )\n[<color=yellow><b>$KEY_Use</b></color>] $piece_fermenter_add");
		case Fermenter.Status.Fermenting:
		{
			string contentName = this.GetContentName();
			if (this.m_exposed)
			{
				return Localization.instance.Localize(this.m_name + " ( " + contentName + ", $piece_fermenter_exposed )");
			}
			return Localization.instance.Localize(this.m_name + " ( " + contentName + ", $piece_fermenter_fermenting )");
		}
		case Fermenter.Status.Ready:
		{
			string contentName2 = this.GetContentName();
			return Localization.instance.Localize(this.m_name + " ( " + contentName2 + ", $piece_fermenter_ready )\n[<color=yellow><b>$KEY_Use</b></color>] $piece_fermenter_tap");
		}
		}
		return this.m_name;
	}

	// Token: 0x06000D0B RID: 3339 RVA: 0x0005D11C File Offset: 0x0005B31C
	public bool Interact(Humanoid user, bool hold)
	{
		if (hold)
		{
			return false;
		}
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			return true;
		}
		Fermenter.Status status = this.GetStatus();
		if (status == Fermenter.Status.Empty)
		{
			ItemDrop.ItemData itemData = this.FindCookableItem(user.GetInventory());
			if (itemData == null)
			{
				user.Message(MessageHud.MessageType.Center, "$msg_noprocessableitems", 0, null);
				return true;
			}
			this.AddItem(user, itemData);
		}
		else if (status == Fermenter.Status.Ready)
		{
			this.m_nview.InvokeRPC("Tap", Array.Empty<object>());
		}
		return true;
	}

	// Token: 0x06000D0C RID: 3340 RVA: 0x0005D198 File Offset: 0x0005B398
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return PrivateArea.CheckAccess(base.transform.position, 0f, true, false) && this.AddItem(user, item);
	}

	// Token: 0x06000D0D RID: 3341 RVA: 0x0005D1C0 File Offset: 0x0005B3C0
	private void UpdateVis()
	{
		this.UpdateCover(2f);
		switch (this.GetStatus())
		{
		case Fermenter.Status.Empty:
			this.m_fermentingObject.SetActive(false);
			this.m_readyObject.SetActive(false);
			this.m_topObject.SetActive(false);
			return;
		case Fermenter.Status.Fermenting:
			this.m_readyObject.SetActive(false);
			this.m_topObject.SetActive(true);
			this.m_fermentingObject.SetActive(!this.m_exposed);
			return;
		case Fermenter.Status.Exposed:
			break;
		case Fermenter.Status.Ready:
			this.m_fermentingObject.SetActive(false);
			this.m_readyObject.SetActive(true);
			this.m_topObject.SetActive(true);
			break;
		default:
			return;
		}
	}

	// Token: 0x06000D0E RID: 3342 RVA: 0x0005D26C File Offset: 0x0005B46C
	private Fermenter.Status GetStatus()
	{
		if (string.IsNullOrEmpty(this.GetContent()))
		{
			return Fermenter.Status.Empty;
		}
		if (this.GetFermentationTime() > (double)this.m_fermentationDuration)
		{
			return Fermenter.Status.Ready;
		}
		return Fermenter.Status.Fermenting;
	}

	// Token: 0x06000D0F RID: 3343 RVA: 0x0005D290 File Offset: 0x0005B490
	private bool AddItem(Humanoid user, ItemDrop.ItemData item)
	{
		if (this.GetStatus() != Fermenter.Status.Empty)
		{
			return false;
		}
		if (!this.IsItemAllowed(item))
		{
			return false;
		}
		if (!user.GetInventory().RemoveOneItem(item))
		{
			return false;
		}
		this.m_nview.InvokeRPC("AddItem", new object[]
		{
			item.m_dropPrefab.name
		});
		return true;
	}

	// Token: 0x06000D10 RID: 3344 RVA: 0x0005D2E8 File Offset: 0x0005B4E8
	private void RPC_AddItem(long sender, string name)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.GetStatus() != Fermenter.Status.Empty)
		{
			return;
		}
		if (!this.IsItemAllowed(name))
		{
			ZLog.DevLog("Item not allowed");
			return;
		}
		this.m_addedEffects.Create(base.transform.position, base.transform.rotation, null, 1f);
		this.m_nview.GetZDO().Set("Content", name);
		this.m_nview.GetZDO().Set("StartTime", ZNet.instance.GetTime().Ticks);
	}

	// Token: 0x06000D11 RID: 3345 RVA: 0x0005D388 File Offset: 0x0005B588
	private void RPC_Tap(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.GetStatus() != Fermenter.Status.Ready)
		{
			return;
		}
		this.m_delayedTapItem = this.GetContent();
		base.Invoke("DelayedTap", this.m_tapDelay);
		this.m_tapEffects.Create(base.transform.position, base.transform.rotation, null, 1f);
		this.m_nview.GetZDO().Set("Content", "");
		this.m_nview.GetZDO().Set("StartTime", 0);
	}

	// Token: 0x06000D12 RID: 3346 RVA: 0x0005D424 File Offset: 0x0005B624
	private void DelayedTap()
	{
		this.m_spawnEffects.Create(this.m_outputPoint.transform.position, Quaternion.identity, null, 1f);
		Fermenter.ItemConversion itemConversion = this.GetItemConversion(this.m_delayedTapItem);
		if (itemConversion != null)
		{
			float d = 0.3f;
			for (int i = 0; i < itemConversion.m_producedItems; i++)
			{
				Vector3 position = this.m_outputPoint.position + Vector3.up * d;
				UnityEngine.Object.Instantiate<ItemDrop>(itemConversion.m_to, position, Quaternion.identity);
			}
		}
	}

	// Token: 0x06000D13 RID: 3347 RVA: 0x0005D4B0 File Offset: 0x0005B6B0
	private void ResetFermentationTimer()
	{
		if (this.GetStatus() == Fermenter.Status.Fermenting)
		{
			this.m_nview.GetZDO().Set("StartTime", ZNet.instance.GetTime().Ticks);
		}
	}

	// Token: 0x06000D14 RID: 3348 RVA: 0x0005D4F0 File Offset: 0x0005B6F0
	private double GetFermentationTime()
	{
		DateTime d = new DateTime(this.m_nview.GetZDO().GetLong("StartTime", 0L));
		if (d.Ticks == 0L)
		{
			return -1.0;
		}
		return (ZNet.instance.GetTime() - d).TotalSeconds;
	}

	// Token: 0x06000D15 RID: 3349 RVA: 0x0005D548 File Offset: 0x0005B748
	private string GetContentName()
	{
		string content = this.GetContent();
		if (string.IsNullOrEmpty(content))
		{
			return "";
		}
		Fermenter.ItemConversion itemConversion = this.GetItemConversion(content);
		if (itemConversion == null)
		{
			return "Invalid";
		}
		return itemConversion.m_from.m_itemData.m_shared.m_name;
	}

	// Token: 0x06000D16 RID: 3350 RVA: 0x0005D590 File Offset: 0x0005B790
	private string GetContent()
	{
		return this.m_nview.GetZDO().GetString("Content", "");
	}

	// Token: 0x06000D17 RID: 3351 RVA: 0x0005D5AC File Offset: 0x0005B7AC
	private void UpdateCover(float dt)
	{
		this.m_updateCoverTimer += dt;
		if (this.m_updateCoverTimer > 10f)
		{
			this.m_updateCoverTimer = 0f;
			float num;
			bool flag;
			Cover.GetCoverForPoint(this.m_roofCheckPoint.position, out num, out flag);
			this.m_exposed = (!flag || num < 0.7f);
			if (this.m_exposed && this.m_nview.IsOwner())
			{
				this.ResetFermentationTimer();
			}
		}
	}

	// Token: 0x06000D18 RID: 3352 RVA: 0x0005D622 File Offset: 0x0005B822
	private bool IsItemAllowed(ItemDrop.ItemData item)
	{
		return this.IsItemAllowed(item.m_dropPrefab.name);
	}

	// Token: 0x06000D19 RID: 3353 RVA: 0x0005D638 File Offset: 0x0005B838
	private bool IsItemAllowed(string itemName)
	{
		using (List<Fermenter.ItemConversion>.Enumerator enumerator = this.m_conversion.GetEnumerator())
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

	// Token: 0x06000D1A RID: 3354 RVA: 0x0005D6A4 File Offset: 0x0005B8A4
	private ItemDrop.ItemData FindCookableItem(Inventory inventory)
	{
		foreach (Fermenter.ItemConversion itemConversion in this.m_conversion)
		{
			ItemDrop.ItemData item = inventory.GetItem(itemConversion.m_from.m_itemData.m_shared.m_name);
			if (item != null)
			{
				return item;
			}
		}
		return null;
	}

	// Token: 0x06000D1B RID: 3355 RVA: 0x0005D718 File Offset: 0x0005B918
	private Fermenter.ItemConversion GetItemConversion(string itemName)
	{
		foreach (Fermenter.ItemConversion itemConversion in this.m_conversion)
		{
			if (itemConversion.m_from.gameObject.name == itemName)
			{
				return itemConversion;
			}
		}
		return null;
	}

	// Token: 0x04000BE2 RID: 3042
	private const float updateDT = 2f;

	// Token: 0x04000BE3 RID: 3043
	public string m_name = "Fermentation barrel";

	// Token: 0x04000BE4 RID: 3044
	public float m_fermentationDuration = 2400f;

	// Token: 0x04000BE5 RID: 3045
	public GameObject m_fermentingObject;

	// Token: 0x04000BE6 RID: 3046
	public GameObject m_readyObject;

	// Token: 0x04000BE7 RID: 3047
	public GameObject m_topObject;

	// Token: 0x04000BE8 RID: 3048
	public EffectList m_addedEffects = new EffectList();

	// Token: 0x04000BE9 RID: 3049
	public EffectList m_tapEffects = new EffectList();

	// Token: 0x04000BEA RID: 3050
	public EffectList m_spawnEffects = new EffectList();

	// Token: 0x04000BEB RID: 3051
	public Switch m_addSwitch;

	// Token: 0x04000BEC RID: 3052
	public Switch m_tapSwitch;

	// Token: 0x04000BED RID: 3053
	public float m_tapDelay = 1.5f;

	// Token: 0x04000BEE RID: 3054
	public Transform m_outputPoint;

	// Token: 0x04000BEF RID: 3055
	public Transform m_roofCheckPoint;

	// Token: 0x04000BF0 RID: 3056
	public List<Fermenter.ItemConversion> m_conversion = new List<Fermenter.ItemConversion>();

	// Token: 0x04000BF1 RID: 3057
	private ZNetView m_nview;

	// Token: 0x04000BF2 RID: 3058
	private float m_updateCoverTimer;

	// Token: 0x04000BF3 RID: 3059
	private bool m_exposed;

	// Token: 0x04000BF4 RID: 3060
	private string m_delayedTapItem = "";

	// Token: 0x02000196 RID: 406
	[Serializable]
	public class ItemConversion
	{
		// Token: 0x0400128C RID: 4748
		public ItemDrop m_from;

		// Token: 0x0400128D RID: 4749
		public ItemDrop m_to;

		// Token: 0x0400128E RID: 4750
		public int m_producedItems = 4;
	}

	// Token: 0x02000197 RID: 407
	private enum Status
	{
		// Token: 0x04001290 RID: 4752
		Empty,
		// Token: 0x04001291 RID: 4753
		Fermenting,
		// Token: 0x04001292 RID: 4754
		Exposed,
		// Token: 0x04001293 RID: 4755
		Ready
	}
}
