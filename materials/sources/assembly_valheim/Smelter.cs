using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000F7 RID: 247
public class Smelter : MonoBehaviour
{
	// Token: 0x06000F21 RID: 3873 RVA: 0x0006BFF4 File Offset: 0x0006A1F4
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview == null || this.m_nview.GetZDO() == null)
		{
			return;
		}
		Switch addOreSwitch = this.m_addOreSwitch;
		addOreSwitch.m_onUse = (Switch.Callback)Delegate.Combine(addOreSwitch.m_onUse, new Switch.Callback(this.OnAddOre));
		if (this.m_addWoodSwitch)
		{
			Switch addWoodSwitch = this.m_addWoodSwitch;
			addWoodSwitch.m_onUse = (Switch.Callback)Delegate.Combine(addWoodSwitch.m_onUse, new Switch.Callback(this.OnAddFuel));
		}
		if (this.m_emptyOreSwitch)
		{
			Switch emptyOreSwitch = this.m_emptyOreSwitch;
			emptyOreSwitch.m_onUse = (Switch.Callback)Delegate.Combine(emptyOreSwitch.m_onUse, new Switch.Callback(this.OnEmpty));
		}
		this.m_nview.Register<string>("AddOre", new Action<long, string>(this.RPC_AddOre));
		this.m_nview.Register("AddFuel", new Action<long>(this.RPC_AddFuel));
		this.m_nview.Register("EmptyProcessed", new Action<long>(this.RPC_EmptyProcessed));
		WearNTear component = base.GetComponent<WearNTear>();
		if (component)
		{
			WearNTear wearNTear = component;
			wearNTear.m_onDestroyed = (Action)Delegate.Combine(wearNTear.m_onDestroyed, new Action(this.OnDestroyed));
		}
		base.InvokeRepeating("UpdateSmelter", 1f, 1f);
	}

	// Token: 0x06000F22 RID: 3874 RVA: 0x0006C154 File Offset: 0x0006A354
	private void DropAllItems()
	{
		this.SpawnProcessed();
		if (this.m_fuelItem != null)
		{
			float @float = this.m_nview.GetZDO().GetFloat("fuel", 0f);
			for (int i = 0; i < (int)@float; i++)
			{
				Vector3 position = base.transform.position + Vector3.up + UnityEngine.Random.insideUnitSphere * 0.3f;
				Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
				UnityEngine.Object.Instantiate<GameObject>(this.m_fuelItem.gameObject, position, rotation);
			}
		}
		while (this.GetQueueSize() > 0)
		{
			string queuedOre = this.GetQueuedOre();
			this.RemoveOneOre();
			Smelter.ItemConversion itemConversion = this.GetItemConversion(queuedOre);
			if (itemConversion != null)
			{
				Vector3 position2 = base.transform.position + Vector3.up + UnityEngine.Random.insideUnitSphere * 0.3f;
				Quaternion rotation2 = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
				UnityEngine.Object.Instantiate<GameObject>(itemConversion.m_from.gameObject, position2, rotation2);
			}
			else
			{
				ZLog.Log("Invalid ore in smelter " + queuedOre);
			}
		}
	}

	// Token: 0x06000F23 RID: 3875 RVA: 0x0006C297 File Offset: 0x0006A497
	private void OnDestroyed()
	{
		if (this.m_nview.IsOwner())
		{
			this.DropAllItems();
		}
	}

	// Token: 0x06000F24 RID: 3876 RVA: 0x0006C2AC File Offset: 0x0006A4AC
	private bool IsItemAllowed(ItemDrop.ItemData item)
	{
		return this.IsItemAllowed(item.m_dropPrefab.name);
	}

	// Token: 0x06000F25 RID: 3877 RVA: 0x0006C2C0 File Offset: 0x0006A4C0
	private bool IsItemAllowed(string itemName)
	{
		using (List<Smelter.ItemConversion>.Enumerator enumerator = this.m_conversion.GetEnumerator())
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

	// Token: 0x06000F26 RID: 3878 RVA: 0x0006C32C File Offset: 0x0006A52C
	private ItemDrop.ItemData FindCookableItem(Inventory inventory)
	{
		foreach (Smelter.ItemConversion itemConversion in this.m_conversion)
		{
			ItemDrop.ItemData item = inventory.GetItem(itemConversion.m_from.m_itemData.m_shared.m_name);
			if (item != null)
			{
				return item;
			}
		}
		return null;
	}

	// Token: 0x06000F27 RID: 3879 RVA: 0x0006C3A0 File Offset: 0x0006A5A0
	private bool OnAddOre(Switch sw, Humanoid user, ItemDrop.ItemData item)
	{
		if (item == null)
		{
			item = this.FindCookableItem(user.GetInventory());
			if (item == null)
			{
				user.Message(MessageHud.MessageType.Center, "$msg_noprocessableitems", 0, null);
				return false;
			}
		}
		if (!this.IsItemAllowed(item.m_dropPrefab.name))
		{
			user.Message(MessageHud.MessageType.Center, "$msg_wontwork", 0, null);
			return false;
		}
		ZLog.Log("trying to add " + item.m_shared.m_name);
		if (this.GetQueueSize() >= this.m_maxOre)
		{
			user.Message(MessageHud.MessageType.Center, "$msg_itsfull", 0, null);
			return false;
		}
		user.Message(MessageHud.MessageType.Center, "$msg_added " + item.m_shared.m_name, 0, null);
		user.GetInventory().RemoveItem(item, 1);
		this.m_nview.InvokeRPC("AddOre", new object[]
		{
			item.m_dropPrefab.name
		});
		return true;
	}

	// Token: 0x06000F28 RID: 3880 RVA: 0x0006C47D File Offset: 0x0006A67D
	private float GetBakeTimer()
	{
		return this.m_nview.GetZDO().GetFloat("bakeTimer", 0f);
	}

	// Token: 0x06000F29 RID: 3881 RVA: 0x0006C499 File Offset: 0x0006A699
	private void SetBakeTimer(float t)
	{
		this.m_nview.GetZDO().Set("bakeTimer", t);
	}

	// Token: 0x06000F2A RID: 3882 RVA: 0x0006C4B1 File Offset: 0x0006A6B1
	private float GetFuel()
	{
		return this.m_nview.GetZDO().GetFloat("fuel", 0f);
	}

	// Token: 0x06000F2B RID: 3883 RVA: 0x0006C4CD File Offset: 0x0006A6CD
	private void SetFuel(float fuel)
	{
		this.m_nview.GetZDO().Set("fuel", fuel);
	}

	// Token: 0x06000F2C RID: 3884 RVA: 0x0006C4E5 File Offset: 0x0006A6E5
	private int GetQueueSize()
	{
		return this.m_nview.GetZDO().GetInt("queued", 0);
	}

	// Token: 0x06000F2D RID: 3885 RVA: 0x0006C500 File Offset: 0x0006A700
	private void RPC_AddOre(long sender, string name)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.IsItemAllowed(name))
		{
			ZLog.Log("Item not allowed " + name);
			return;
		}
		this.QueueOre(name);
		this.m_oreAddedEffects.Create(base.transform.position, base.transform.rotation, null, 1f);
		ZLog.Log("Added ore " + name);
	}

	// Token: 0x06000F2E RID: 3886 RVA: 0x0006C574 File Offset: 0x0006A774
	private void QueueOre(string name)
	{
		int queueSize = this.GetQueueSize();
		this.m_nview.GetZDO().Set("item" + queueSize, name);
		this.m_nview.GetZDO().Set("queued", queueSize + 1);
	}

	// Token: 0x06000F2F RID: 3887 RVA: 0x0006C5C1 File Offset: 0x0006A7C1
	private string GetQueuedOre()
	{
		if (this.GetQueueSize() == 0)
		{
			return "";
		}
		return this.m_nview.GetZDO().GetString("item0", "");
	}

	// Token: 0x06000F30 RID: 3888 RVA: 0x0006C5EC File Offset: 0x0006A7EC
	private void RemoveOneOre()
	{
		int queueSize = this.GetQueueSize();
		if (queueSize == 0)
		{
			return;
		}
		for (int i = 0; i < queueSize; i++)
		{
			string @string = this.m_nview.GetZDO().GetString("item" + (i + 1), "");
			this.m_nview.GetZDO().Set("item" + i, @string);
		}
		this.m_nview.GetZDO().Set("queued", queueSize - 1);
	}

	// Token: 0x06000F31 RID: 3889 RVA: 0x0006C671 File Offset: 0x0006A871
	private bool OnEmpty(Switch sw, Humanoid user, ItemDrop.ItemData item)
	{
		if (this.GetProcessedQueueSize() <= 0)
		{
			return false;
		}
		this.m_nview.InvokeRPC("EmptyProcessed", Array.Empty<object>());
		return true;
	}

	// Token: 0x06000F32 RID: 3890 RVA: 0x0006C694 File Offset: 0x0006A894
	private void RPC_EmptyProcessed(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		this.SpawnProcessed();
	}

	// Token: 0x06000F33 RID: 3891 RVA: 0x0006C6AC File Offset: 0x0006A8AC
	private bool OnAddFuel(Switch sw, Humanoid user, ItemDrop.ItemData item)
	{
		if (item != null && item.m_shared.m_name != this.m_fuelItem.m_itemData.m_shared.m_name)
		{
			user.Message(MessageHud.MessageType.Center, "$msg_wrongitem", 0, null);
			return false;
		}
		if (this.GetFuel() > (float)(this.m_maxFuel - 1))
		{
			user.Message(MessageHud.MessageType.Center, "$msg_itsfull", 0, null);
			return false;
		}
		if (!user.GetInventory().HaveItem(this.m_fuelItem.m_itemData.m_shared.m_name))
		{
			user.Message(MessageHud.MessageType.Center, "$msg_donthaveany " + this.m_fuelItem.m_itemData.m_shared.m_name, 0, null);
			return false;
		}
		user.Message(MessageHud.MessageType.Center, "$msg_added " + this.m_fuelItem.m_itemData.m_shared.m_name, 0, null);
		user.GetInventory().RemoveItem(this.m_fuelItem.m_itemData.m_shared.m_name, 1);
		this.m_nview.InvokeRPC("AddFuel", Array.Empty<object>());
		return true;
	}

	// Token: 0x06000F34 RID: 3892 RVA: 0x0006C7C0 File Offset: 0x0006A9C0
	private void RPC_AddFuel(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		float fuel = this.GetFuel();
		this.SetFuel(fuel + 1f);
		this.m_fuelAddedEffects.Create(base.transform.position, base.transform.rotation, base.transform, 1f);
	}

	// Token: 0x06000F35 RID: 3893 RVA: 0x0006C81C File Offset: 0x0006AA1C
	private double GetDeltaTime()
	{
		DateTime time = ZNet.instance.GetTime();
		DateTime d = new DateTime(this.m_nview.GetZDO().GetLong("StartTime", time.Ticks));
		double totalSeconds = (time - d).TotalSeconds;
		this.m_nview.GetZDO().Set("StartTime", time.Ticks);
		return totalSeconds;
	}

	// Token: 0x06000F36 RID: 3894 RVA: 0x0006C882 File Offset: 0x0006AA82
	private float GetAccumulator()
	{
		return this.m_nview.GetZDO().GetFloat("accTime", 0f);
	}

	// Token: 0x06000F37 RID: 3895 RVA: 0x0006C89E File Offset: 0x0006AA9E
	private void SetAccumulator(float t)
	{
		this.m_nview.GetZDO().Set("accTime", t);
	}

	// Token: 0x06000F38 RID: 3896 RVA: 0x0006C8B6 File Offset: 0x0006AAB6
	private void UpdateRoof()
	{
		if (this.m_requiresRoof)
		{
			this.m_haveRoof = Cover.IsUnderRoof(this.m_roofCheckPoint.position);
		}
	}

	// Token: 0x06000F39 RID: 3897 RVA: 0x0006C8D8 File Offset: 0x0006AAD8
	private void UpdateSmelter()
	{
		this.UpdateRoof();
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		double deltaTime = this.GetDeltaTime();
		float num = this.GetAccumulator();
		num += (float)deltaTime;
		float num2 = this.m_windmill ? this.m_windmill.GetPowerOutput() : 1f;
		while (num >= 1f)
		{
			num -= 1f;
			float num3 = this.GetFuel();
			string queuedOre = this.GetQueuedOre();
			if ((this.m_maxFuel == 0 || num3 > 0f) && queuedOre != "" && this.m_secPerProduct > 0f && (!this.m_requiresRoof || this.m_haveRoof))
			{
				float num4 = 1f * num2;
				if (this.m_maxFuel > 0)
				{
					float num5 = this.m_secPerProduct / (float)this.m_fuelPerProduct;
					num3 -= num4 / num5;
					if (num3 < 0f)
					{
						num3 = 0f;
					}
					this.SetFuel(num3);
				}
				float num6 = this.GetBakeTimer();
				num6 += num4;
				this.SetBakeTimer(num6);
				if (num6 > this.m_secPerProduct)
				{
					this.SetBakeTimer(0f);
					this.RemoveOneOre();
					this.QueueProcessed(queuedOre);
				}
			}
		}
		if (this.GetQueuedOre() == "" || ((float)this.m_maxFuel > 0f && this.GetFuel() == 0f))
		{
			this.SpawnProcessed();
		}
		this.SetAccumulator(num);
	}

	// Token: 0x06000F3A RID: 3898 RVA: 0x0006CA58 File Offset: 0x0006AC58
	private void QueueProcessed(string ore)
	{
		if (!this.m_spawnStack)
		{
			this.Spawn(ore, 1);
			return;
		}
		string @string = this.m_nview.GetZDO().GetString("SpawnOre", "");
		int num = this.m_nview.GetZDO().GetInt("SpawnAmount", 0);
		if (@string.Length <= 0)
		{
			this.m_nview.GetZDO().Set("SpawnOre", ore);
			this.m_nview.GetZDO().Set("SpawnAmount", 1);
			return;
		}
		if (@string != ore)
		{
			this.SpawnProcessed();
			this.m_nview.GetZDO().Set("SpawnOre", ore);
			this.m_nview.GetZDO().Set("SpawnAmount", 1);
			return;
		}
		num++;
		Smelter.ItemConversion itemConversion = this.GetItemConversion(ore);
		if (itemConversion == null || num >= itemConversion.m_to.m_itemData.m_shared.m_maxStackSize)
		{
			this.Spawn(ore, num);
			this.m_nview.GetZDO().Set("SpawnOre", "");
			this.m_nview.GetZDO().Set("SpawnAmount", 0);
			return;
		}
		this.m_nview.GetZDO().Set("SpawnAmount", num);
	}

	// Token: 0x06000F3B RID: 3899 RVA: 0x0006CB94 File Offset: 0x0006AD94
	private void SpawnProcessed()
	{
		int @int = this.m_nview.GetZDO().GetInt("SpawnAmount", 0);
		if (@int > 0)
		{
			string @string = this.m_nview.GetZDO().GetString("SpawnOre", "");
			this.Spawn(@string, @int);
			this.m_nview.GetZDO().Set("SpawnOre", "");
			this.m_nview.GetZDO().Set("SpawnAmount", 0);
		}
	}

	// Token: 0x06000F3C RID: 3900 RVA: 0x0006CC0F File Offset: 0x0006AE0F
	private int GetProcessedQueueSize()
	{
		return this.m_nview.GetZDO().GetInt("SpawnAmount", 0);
	}

	// Token: 0x06000F3D RID: 3901 RVA: 0x0006CC28 File Offset: 0x0006AE28
	private void Spawn(string ore, int stack)
	{
		Smelter.ItemConversion itemConversion = this.GetItemConversion(ore);
		if (itemConversion != null)
		{
			this.m_produceEffects.Create(base.transform.position, base.transform.rotation, null, 1f);
			UnityEngine.Object.Instantiate<GameObject>(itemConversion.m_to.gameObject, this.m_outputPoint.position, this.m_outputPoint.rotation).GetComponent<ItemDrop>().m_itemData.m_stack = stack;
		}
	}

	// Token: 0x06000F3E RID: 3902 RVA: 0x0006CC9E File Offset: 0x0006AE9E
	private void FixedUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.UpdateState();
		this.UpdateHoverTexts();
	}

	// Token: 0x06000F3F RID: 3903 RVA: 0x0006CCBC File Offset: 0x0006AEBC
	private Smelter.ItemConversion GetItemConversion(string itemName)
	{
		foreach (Smelter.ItemConversion itemConversion in this.m_conversion)
		{
			if (itemConversion.m_from.gameObject.name == itemName)
			{
				return itemConversion;
			}
		}
		return null;
	}

	// Token: 0x06000F40 RID: 3904 RVA: 0x0006CD28 File Offset: 0x0006AF28
	private void UpdateState()
	{
		bool flag = this.IsActive();
		this.m_enabledObject.SetActive(flag);
		foreach (Animator animator in this.m_animators)
		{
			if (animator.gameObject.activeInHierarchy)
			{
				animator.SetBool("active", flag);
			}
		}
	}

	// Token: 0x06000F41 RID: 3905 RVA: 0x0006CD7A File Offset: 0x0006AF7A
	public bool IsActive()
	{
		return (this.m_maxFuel == 0 || this.GetFuel() > 0f) && this.GetQueueSize() > 0 && (!this.m_requiresRoof || this.m_haveRoof);
	}

	// Token: 0x06000F42 RID: 3906 RVA: 0x0006CDAC File Offset: 0x0006AFAC
	private void UpdateHoverTexts()
	{
		if (this.m_addWoodSwitch)
		{
			float fuel = this.GetFuel();
			this.m_addWoodSwitch.m_hoverText = string.Concat(new object[]
			{
				this.m_name,
				" (",
				this.m_fuelItem.m_itemData.m_shared.m_name,
				" ",
				Mathf.Ceil(fuel),
				"/",
				this.m_maxFuel,
				")\n[<color=yellow><b>$KEY_Use</b></color>] $piece_smelter_add ",
				this.m_fuelItem.m_itemData.m_shared.m_name
			});
		}
		if (this.m_emptyOreSwitch && this.m_spawnStack)
		{
			int processedQueueSize = this.GetProcessedQueueSize();
			this.m_emptyOreSwitch.m_hoverText = string.Concat(new object[]
			{
				this.m_name,
				" (",
				processedQueueSize,
				" $piece_smelter_ready \n[<color=yellow><b>$KEY_Use</b></color>] ",
				this.m_emptyOreTooltip
			});
		}
		int queueSize = this.GetQueueSize();
		this.m_addOreSwitch.m_hoverText = string.Concat(new object[]
		{
			this.m_name,
			" (",
			queueSize,
			"/",
			this.m_maxOre,
			") "
		});
		if (this.m_requiresRoof && !this.m_haveRoof && Mathf.Sin(Time.time * 10f) > 0f)
		{
			Switch addOreSwitch = this.m_addOreSwitch;
			addOreSwitch.m_hoverText += " <color=yellow>$piece_smelter_reqroof</color>";
		}
		Switch addOreSwitch2 = this.m_addOreSwitch;
		addOreSwitch2.m_hoverText = addOreSwitch2.m_hoverText + "\n[<color=yellow><b>$KEY_Use</b></color>] " + this.m_addOreTooltip;
	}

	// Token: 0x04000E12 RID: 3602
	public string m_name = "Smelter";

	// Token: 0x04000E13 RID: 3603
	public string m_addOreTooltip = "$piece_smelter_additem";

	// Token: 0x04000E14 RID: 3604
	public string m_emptyOreTooltip = "$piece_smelter_empty";

	// Token: 0x04000E15 RID: 3605
	public Switch m_addWoodSwitch;

	// Token: 0x04000E16 RID: 3606
	public Switch m_addOreSwitch;

	// Token: 0x04000E17 RID: 3607
	public Switch m_emptyOreSwitch;

	// Token: 0x04000E18 RID: 3608
	public Transform m_outputPoint;

	// Token: 0x04000E19 RID: 3609
	public Transform m_roofCheckPoint;

	// Token: 0x04000E1A RID: 3610
	public GameObject m_enabledObject;

	// Token: 0x04000E1B RID: 3611
	public Animator[] m_animators;

	// Token: 0x04000E1C RID: 3612
	public ItemDrop m_fuelItem;

	// Token: 0x04000E1D RID: 3613
	public int m_maxOre = 10;

	// Token: 0x04000E1E RID: 3614
	public int m_maxFuel = 10;

	// Token: 0x04000E1F RID: 3615
	public int m_fuelPerProduct = 4;

	// Token: 0x04000E20 RID: 3616
	public float m_secPerProduct = 10f;

	// Token: 0x04000E21 RID: 3617
	public bool m_spawnStack;

	// Token: 0x04000E22 RID: 3618
	public bool m_requiresRoof;

	// Token: 0x04000E23 RID: 3619
	public Windmill m_windmill;

	// Token: 0x04000E24 RID: 3620
	public List<Smelter.ItemConversion> m_conversion = new List<Smelter.ItemConversion>();

	// Token: 0x04000E25 RID: 3621
	public EffectList m_oreAddedEffects = new EffectList();

	// Token: 0x04000E26 RID: 3622
	public EffectList m_fuelAddedEffects = new EffectList();

	// Token: 0x04000E27 RID: 3623
	public EffectList m_produceEffects = new EffectList();

	// Token: 0x04000E28 RID: 3624
	private ZNetView m_nview;

	// Token: 0x04000E29 RID: 3625
	private bool m_haveRoof;

	// Token: 0x020001AB RID: 427
	[Serializable]
	public class ItemConversion
	{
		// Token: 0x04001311 RID: 4881
		public ItemDrop m_from;

		// Token: 0x04001312 RID: 4882
		public ItemDrop m_to;
	}
}
