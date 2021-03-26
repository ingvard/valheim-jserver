using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Token: 0x0200006B RID: 107
public class ItemDrop : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x060006C1 RID: 1729 RVA: 0x00037FB8 File Offset: 0x000361B8
	private void Awake()
	{
		this.m_myIndex = ItemDrop.m_instances.Count;
		ItemDrop.m_instances.Add(this);
		string prefabName = this.GetPrefabName(base.gameObject.name);
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(prefabName);
		this.m_itemData.m_dropPrefab = itemPrefab;
		if (Application.isEditor)
		{
			this.m_itemData.m_shared = itemPrefab.GetComponent<ItemDrop>().m_itemData.m_shared;
		}
		Rigidbody component = base.GetComponent<Rigidbody>();
		if (component)
		{
			component.maxDepenetrationVelocity = 1f;
		}
		this.m_spawnTime = Time.time;
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview && this.m_nview.IsValid())
		{
			if (this.m_nview.IsOwner())
			{
				DateTime dateTime = new DateTime(this.m_nview.GetZDO().GetLong("SpawnTime", 0L));
				if (dateTime.Ticks == 0L)
				{
					this.m_nview.GetZDO().Set("SpawnTime", ZNet.instance.GetTime().Ticks);
				}
			}
			this.m_nview.Register("RequestOwn", new Action<long>(this.RPC_RequestOwn));
			this.Load();
			base.InvokeRepeating("SlowUpdate", UnityEngine.Random.Range(1f, 2f), 10f);
		}
	}

	// Token: 0x060006C2 RID: 1730 RVA: 0x0003811C File Offset: 0x0003631C
	private void OnDestroy()
	{
		ItemDrop.m_instances[this.m_myIndex] = ItemDrop.m_instances[ItemDrop.m_instances.Count - 1];
		ItemDrop.m_instances[this.m_myIndex].m_myIndex = this.m_myIndex;
		ItemDrop.m_instances.RemoveAt(ItemDrop.m_instances.Count - 1);
	}

	// Token: 0x060006C3 RID: 1731 RVA: 0x00038180 File Offset: 0x00036380
	private void Start()
	{
		this.Save();
		IEquipmentVisual componentInChildren = base.gameObject.GetComponentInChildren<IEquipmentVisual>();
		if (componentInChildren != null)
		{
			componentInChildren.Setup(this.m_itemData.m_variant);
		}
	}

	// Token: 0x060006C4 RID: 1732 RVA: 0x000381B4 File Offset: 0x000363B4
	private double GetTimeSinceSpawned()
	{
		DateTime d = new DateTime(this.m_nview.GetZDO().GetLong("SpawnTime", 0L));
		return (ZNet.instance.GetTime() - d).TotalSeconds;
	}

	// Token: 0x060006C5 RID: 1733 RVA: 0x000381F8 File Offset: 0x000363F8
	private void SlowUpdate()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		this.TerrainCheck();
		if (this.m_autoDestroy)
		{
			this.TimedDestruction();
		}
		if (ItemDrop.m_instances.Count > 200)
		{
			this.AutoStackItems();
		}
	}

	// Token: 0x060006C6 RID: 1734 RVA: 0x0003824C File Offset: 0x0003644C
	private void TerrainCheck()
	{
		float groundHeight = ZoneSystem.instance.GetGroundHeight(base.transform.position);
		if (base.transform.position.y - groundHeight < -0.5f)
		{
			Vector3 position = base.transform.position;
			position.y = groundHeight + 0.5f;
			base.transform.position = position;
			Rigidbody component = base.GetComponent<Rigidbody>();
			if (component)
			{
				component.velocity = Vector3.zero;
			}
		}
	}

	// Token: 0x060006C7 RID: 1735 RVA: 0x000382C8 File Offset: 0x000364C8
	private void TimedDestruction()
	{
		if (this.IsInsideBase())
		{
			return;
		}
		if (Player.IsPlayerInRange(base.transform.position, 25f))
		{
			return;
		}
		if (this.GetTimeSinceSpawned() < 3600.0)
		{
			return;
		}
		this.m_nview.Destroy();
	}

	// Token: 0x060006C8 RID: 1736 RVA: 0x00038308 File Offset: 0x00036508
	private bool IsInsideBase()
	{
		return base.transform.position.y > ZoneSystem.instance.m_waterLevel + -2f && EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.PlayerBase, 0f);
	}

	// Token: 0x060006C9 RID: 1737 RVA: 0x00038358 File Offset: 0x00036558
	private void AutoStackItems()
	{
		if (this.m_itemData.m_shared.m_maxStackSize <= 1 || this.m_itemData.m_stack >= this.m_itemData.m_shared.m_maxStackSize)
		{
			return;
		}
		if (this.m_haveAutoStacked)
		{
			return;
		}
		this.m_haveAutoStacked = true;
		if (ItemDrop.m_itemMask == 0)
		{
			ItemDrop.m_itemMask = LayerMask.GetMask(new string[]
			{
				"item"
			});
		}
		bool flag = false;
		foreach (Collider collider in Physics.OverlapSphere(base.transform.position, 4f, ItemDrop.m_itemMask))
		{
			if (collider.attachedRigidbody)
			{
				ItemDrop component = collider.attachedRigidbody.GetComponent<ItemDrop>();
				if (!(component == null) && !(component == this) && !(component.m_nview == null) && component.m_nview.IsValid() && component.m_nview.IsOwner() && !(component.m_itemData.m_shared.m_name != this.m_itemData.m_shared.m_name) && component.m_itemData.m_quality == this.m_itemData.m_quality)
				{
					int num = this.m_itemData.m_shared.m_maxStackSize - this.m_itemData.m_stack;
					if (num == 0)
					{
						break;
					}
					if (component.m_itemData.m_stack <= num)
					{
						this.m_itemData.m_stack += component.m_itemData.m_stack;
						flag = true;
						component.m_nview.Destroy();
					}
				}
			}
		}
		if (flag)
		{
			this.Save();
		}
	}

	// Token: 0x060006CA RID: 1738 RVA: 0x00038510 File Offset: 0x00036710
	public string GetHoverText()
	{
		string text = this.m_itemData.m_shared.m_name;
		if (this.m_itemData.m_quality > 1)
		{
			text = string.Concat(new object[]
			{
				text,
				"[",
				this.m_itemData.m_quality,
				"] "
			});
		}
		if (this.m_itemData.m_stack > 1)
		{
			text = text + " x" + this.m_itemData.m_stack.ToString();
		}
		return Localization.instance.Localize(text + "\n[<color=yellow><b>$KEY_Use</b></color>] $inventory_pickup");
	}

	// Token: 0x060006CB RID: 1739 RVA: 0x000385AE File Offset: 0x000367AE
	public string GetHoverName()
	{
		return this.m_itemData.m_shared.m_name;
	}

	// Token: 0x060006CC RID: 1740 RVA: 0x000385C0 File Offset: 0x000367C0
	private string GetPrefabName(string name)
	{
		char[] anyOf = new char[]
		{
			'(',
			' '
		};
		int num = name.IndexOfAny(anyOf);
		string result;
		if (num >= 0)
		{
			result = name.Substring(0, num);
		}
		else
		{
			result = name;
		}
		return result;
	}

	// Token: 0x060006CD RID: 1741 RVA: 0x000385F8 File Offset: 0x000367F8
	public bool Interact(Humanoid character, bool repeat)
	{
		if (repeat)
		{
			return false;
		}
		this.Pickup(character);
		return true;
	}

	// Token: 0x060006CE RID: 1742 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x060006CF RID: 1743 RVA: 0x00038608 File Offset: 0x00036808
	public void SetStack(int stack)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		this.m_itemData.m_stack = stack;
		if (this.m_itemData.m_stack > this.m_itemData.m_shared.m_maxStackSize)
		{
			this.m_itemData.m_stack = this.m_itemData.m_shared.m_maxStackSize;
		}
		this.Save();
	}

	// Token: 0x060006D0 RID: 1744 RVA: 0x0003867C File Offset: 0x0003687C
	public void Pickup(Humanoid character)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.CanPickup())
		{
			this.Load();
			character.Pickup(base.gameObject);
			this.Save();
			return;
		}
		this.m_pickupRequester = character;
		base.CancelInvoke("PickupUpdate");
		float num = 0.05f;
		base.InvokeRepeating("PickupUpdate", num, num);
		this.RequestOwn();
	}

	// Token: 0x060006D1 RID: 1745 RVA: 0x000386E4 File Offset: 0x000368E4
	public void RequestOwn()
	{
		if (Time.time - this.m_lastOwnerRequest < 0.2f)
		{
			return;
		}
		if (this.m_nview.IsOwner())
		{
			return;
		}
		this.m_lastOwnerRequest = Time.time;
		this.m_nview.InvokeRPC("RequestOwn", Array.Empty<object>());
	}

	// Token: 0x060006D2 RID: 1746 RVA: 0x00038734 File Offset: 0x00036934
	public bool RemoveOne()
	{
		if (!this.CanPickup())
		{
			this.RequestOwn();
			return false;
		}
		if (this.m_itemData.m_stack <= 1)
		{
			this.m_nview.Destroy();
			return true;
		}
		this.m_itemData.m_stack--;
		this.Save();
		return true;
	}

	// Token: 0x060006D3 RID: 1747 RVA: 0x00038786 File Offset: 0x00036986
	public void OnPlayerDrop()
	{
		this.m_autoPickup = false;
	}

	// Token: 0x060006D4 RID: 1748 RVA: 0x00038790 File Offset: 0x00036990
	public bool CanPickup()
	{
		return this.m_nview == null || !this.m_nview.IsValid() || ((double)(Time.time - this.m_spawnTime) >= 0.5 && this.m_nview.IsOwner());
	}

	// Token: 0x060006D5 RID: 1749 RVA: 0x000387E0 File Offset: 0x000369E0
	private void RPC_RequestOwn(long uid)
	{
		ZLog.Log(string.Concat(new object[]
		{
			"Player ",
			uid,
			" wants to pickup ",
			base.gameObject.name,
			"   im: ",
			ZDOMan.instance.GetMyID()
		}));
		if (!this.m_nview.IsOwner())
		{
			ZLog.Log("  but im not the owner");
			return;
		}
		this.m_nview.GetZDO().SetOwner(uid);
	}

	// Token: 0x060006D6 RID: 1750 RVA: 0x00038868 File Offset: 0x00036A68
	private void PickupUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.CanPickup())
		{
			ZLog.Log("Im finally the owner");
			base.CancelInvoke("PickupUpdate");
			this.Load();
			(this.m_pickupRequester as Player).Pickup(base.gameObject);
			this.Save();
			return;
		}
		ZLog.Log("Im still nto the owner");
	}

	// Token: 0x060006D7 RID: 1751 RVA: 0x000388D0 File Offset: 0x00036AD0
	private void Save()
	{
		if (this.m_nview == null || !this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview.IsOwner())
		{
			ItemDrop.SaveToZDO(this.m_itemData, this.m_nview.GetZDO());
		}
	}

	// Token: 0x060006D8 RID: 1752 RVA: 0x0003891C File Offset: 0x00036B1C
	private void Load()
	{
		ItemDrop.LoadFromZDO(this.m_itemData, this.m_nview.GetZDO());
	}

	// Token: 0x060006D9 RID: 1753 RVA: 0x00038934 File Offset: 0x00036B34
	public void LoadFromExternalZDO(ZDO zdo)
	{
		ItemDrop.LoadFromZDO(this.m_itemData, zdo);
		ItemDrop.SaveToZDO(this.m_itemData, this.m_nview.GetZDO());
	}

	// Token: 0x060006DA RID: 1754 RVA: 0x00038958 File Offset: 0x00036B58
	public static void SaveToZDO(ItemDrop.ItemData itemData, ZDO zdo)
	{
		zdo.Set("durability", itemData.m_durability);
		zdo.Set("stack", itemData.m_stack);
		zdo.Set("quality", itemData.m_quality);
		zdo.Set("variant", itemData.m_variant);
		zdo.Set("crafterID", itemData.m_crafterID);
		zdo.Set("crafterName", itemData.m_crafterName);
	}

	// Token: 0x060006DB RID: 1755 RVA: 0x000389CC File Offset: 0x00036BCC
	public static void LoadFromZDO(ItemDrop.ItemData itemData, ZDO zdo)
	{
		itemData.m_durability = zdo.GetFloat("durability", itemData.m_durability);
		itemData.m_stack = zdo.GetInt("stack", itemData.m_stack);
		itemData.m_quality = zdo.GetInt("quality", itemData.m_quality);
		itemData.m_variant = zdo.GetInt("variant", itemData.m_variant);
		itemData.m_crafterID = zdo.GetLong("crafterID", itemData.m_crafterID);
		itemData.m_crafterName = zdo.GetString("crafterName", itemData.m_crafterName);
	}

	// Token: 0x060006DC RID: 1756 RVA: 0x00038A64 File Offset: 0x00036C64
	public static ItemDrop DropItem(ItemDrop.ItemData item, int amount, Vector3 position, Quaternion rotation)
	{
		ItemDrop component = UnityEngine.Object.Instantiate<GameObject>(item.m_dropPrefab, position, rotation).GetComponent<ItemDrop>();
		component.m_itemData = item.Clone();
		if (amount > 0)
		{
			component.m_itemData.m_stack = amount;
		}
		component.Save();
		return component;
	}

	// Token: 0x060006DD RID: 1757 RVA: 0x000027E0 File Offset: 0x000009E0
	private void OnDrawGizmos()
	{
	}

	// Token: 0x04000753 RID: 1875
	private static List<ItemDrop> m_instances = new List<ItemDrop>();

	// Token: 0x04000754 RID: 1876
	private int m_myIndex = -1;

	// Token: 0x04000755 RID: 1877
	public bool m_autoPickup = true;

	// Token: 0x04000756 RID: 1878
	public bool m_autoDestroy = true;

	// Token: 0x04000757 RID: 1879
	public ItemDrop.ItemData m_itemData = new ItemDrop.ItemData();

	// Token: 0x04000758 RID: 1880
	private ZNetView m_nview;

	// Token: 0x04000759 RID: 1881
	private Character m_pickupRequester;

	// Token: 0x0400075A RID: 1882
	private float m_lastOwnerRequest;

	// Token: 0x0400075B RID: 1883
	private float m_spawnTime;

	// Token: 0x0400075C RID: 1884
	private const double m_autoDestroyTimeout = 3600.0;

	// Token: 0x0400075D RID: 1885
	private const double m_autoPickupDelay = 0.5;

	// Token: 0x0400075E RID: 1886
	private const float m_autoDespawnBaseMinAltitude = -2f;

	// Token: 0x0400075F RID: 1887
	private const int m_autoStackTreshold = 200;

	// Token: 0x04000760 RID: 1888
	private const float m_autoStackRange = 4f;

	// Token: 0x04000761 RID: 1889
	private static int m_itemMask = 0;

	// Token: 0x04000762 RID: 1890
	private bool m_haveAutoStacked;

	// Token: 0x02000165 RID: 357
	[Serializable]
	public class ItemData
	{
		// Token: 0x06001133 RID: 4403 RVA: 0x00077A0A File Offset: 0x00075C0A
		public ItemDrop.ItemData Clone()
		{
			return base.MemberwiseClone() as ItemDrop.ItemData;
		}

		// Token: 0x06001134 RID: 4404 RVA: 0x00077A18 File Offset: 0x00075C18
		public bool IsEquipable()
		{
			return this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Tool || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.OneHandedWeapon || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.TwoHandedWeapon || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Bow || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Helmet || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Chest || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Legs || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shoulder || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Ammo || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Torch || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Utility;
		}

		// Token: 0x06001135 RID: 4405 RVA: 0x00077ADF File Offset: 0x00075CDF
		public bool IsWeapon()
		{
			return this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.OneHandedWeapon || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Bow || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.TwoHandedWeapon || this.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Torch;
		}

		// Token: 0x06001136 RID: 4406 RVA: 0x00077B1D File Offset: 0x00075D1D
		public bool HavePrimaryAttack()
		{
			return !string.IsNullOrEmpty(this.m_shared.m_attack.m_attackAnimation);
		}

		// Token: 0x06001137 RID: 4407 RVA: 0x00077B37 File Offset: 0x00075D37
		public bool HaveSecondaryAttack()
		{
			return !string.IsNullOrEmpty(this.m_shared.m_secondaryAttack.m_attackAnimation);
		}

		// Token: 0x06001138 RID: 4408 RVA: 0x00077B51 File Offset: 0x00075D51
		public float GetArmor()
		{
			return this.GetArmor(this.m_quality);
		}

		// Token: 0x06001139 RID: 4409 RVA: 0x00077B5F File Offset: 0x00075D5F
		public float GetArmor(int quality)
		{
			return this.m_shared.m_armor + (float)Mathf.Max(0, quality - 1) * this.m_shared.m_armorPerLevel;
		}

		// Token: 0x0600113A RID: 4410 RVA: 0x00077B83 File Offset: 0x00075D83
		public int GetValue()
		{
			return this.m_shared.m_value * this.m_stack;
		}

		// Token: 0x0600113B RID: 4411 RVA: 0x00077B97 File Offset: 0x00075D97
		public float GetWeight()
		{
			return this.m_shared.m_weight * (float)this.m_stack;
		}

		// Token: 0x0600113C RID: 4412 RVA: 0x00077BAC File Offset: 0x00075DAC
		public HitData.DamageTypes GetDamage()
		{
			return this.GetDamage(this.m_quality);
		}

		// Token: 0x0600113D RID: 4413 RVA: 0x00077BBC File Offset: 0x00075DBC
		public float GetDurabilityPercentage()
		{
			float maxDurability = this.GetMaxDurability();
			if (maxDurability == 0f)
			{
				return 1f;
			}
			return Mathf.Clamp01(this.m_durability / maxDurability);
		}

		// Token: 0x0600113E RID: 4414 RVA: 0x00077BEB File Offset: 0x00075DEB
		public float GetMaxDurability()
		{
			return this.GetMaxDurability(this.m_quality);
		}

		// Token: 0x0600113F RID: 4415 RVA: 0x00077BF9 File Offset: 0x00075DF9
		public float GetMaxDurability(int quality)
		{
			return this.m_shared.m_maxDurability + (float)Mathf.Max(0, quality - 1) * this.m_shared.m_durabilityPerLevel;
		}

		// Token: 0x06001140 RID: 4416 RVA: 0x00077C20 File Offset: 0x00075E20
		public HitData.DamageTypes GetDamage(int quality)
		{
			HitData.DamageTypes damages = this.m_shared.m_damages;
			if (quality > 1)
			{
				damages.Add(this.m_shared.m_damagesPerLevel, quality - 1);
			}
			return damages;
		}

		// Token: 0x06001141 RID: 4417 RVA: 0x00077C53 File Offset: 0x00075E53
		public float GetBaseBlockPower()
		{
			return this.GetBaseBlockPower(this.m_quality);
		}

		// Token: 0x06001142 RID: 4418 RVA: 0x00077C61 File Offset: 0x00075E61
		public float GetBaseBlockPower(int quality)
		{
			return this.m_shared.m_blockPower + (float)Mathf.Max(0, quality - 1) * this.m_shared.m_blockPowerPerLevel;
		}

		// Token: 0x06001143 RID: 4419 RVA: 0x00077C85 File Offset: 0x00075E85
		public float GetBlockPower(float skillFactor)
		{
			return this.GetBlockPower(this.m_quality, skillFactor);
		}

		// Token: 0x06001144 RID: 4420 RVA: 0x00077C94 File Offset: 0x00075E94
		public float GetBlockPower(int quality, float skillFactor)
		{
			float baseBlockPower = this.GetBaseBlockPower(quality);
			return baseBlockPower + baseBlockPower * skillFactor * 0.5f;
		}

		// Token: 0x06001145 RID: 4421 RVA: 0x00077CA8 File Offset: 0x00075EA8
		public float GetBlockPowerTooltip(int quality)
		{
			if (Player.m_localPlayer == null)
			{
				return 0f;
			}
			float skillFactor = Player.m_localPlayer.GetSkillFactor(Skills.SkillType.Blocking);
			return this.GetBlockPower(quality, skillFactor);
		}

		// Token: 0x06001146 RID: 4422 RVA: 0x00077CDC File Offset: 0x00075EDC
		public float GetDeflectionForce()
		{
			return this.GetDeflectionForce(this.m_quality);
		}

		// Token: 0x06001147 RID: 4423 RVA: 0x00077CEA File Offset: 0x00075EEA
		public float GetDeflectionForce(int quality)
		{
			return this.m_shared.m_deflectionForce + (float)Mathf.Max(0, quality - 1) * this.m_shared.m_deflectionForcePerLevel;
		}

		// Token: 0x06001148 RID: 4424 RVA: 0x00077D0E File Offset: 0x00075F0E
		public string GetTooltip()
		{
			return ItemDrop.ItemData.GetTooltip(this, this.m_quality, false);
		}

		// Token: 0x06001149 RID: 4425 RVA: 0x00077D1D File Offset: 0x00075F1D
		public Sprite GetIcon()
		{
			return this.m_shared.m_icons[this.m_variant];
		}

		// Token: 0x0600114A RID: 4426 RVA: 0x00077D34 File Offset: 0x00075F34
		private static void AddHandedTip(ItemDrop.ItemData item, StringBuilder text)
		{
			ItemDrop.ItemData.ItemType itemType = item.m_shared.m_itemType;
			if (itemType <= ItemDrop.ItemData.ItemType.TwoHandedWeapon)
			{
				switch (itemType)
				{
				case ItemDrop.ItemData.ItemType.OneHandedWeapon:
				case ItemDrop.ItemData.ItemType.Shield:
					break;
				case ItemDrop.ItemData.ItemType.Bow:
					goto IL_43;
				default:
					if (itemType != ItemDrop.ItemData.ItemType.TwoHandedWeapon)
					{
						return;
					}
					goto IL_43;
				}
			}
			else if (itemType != ItemDrop.ItemData.ItemType.Torch)
			{
				if (itemType != ItemDrop.ItemData.ItemType.Tool)
				{
					return;
				}
				goto IL_43;
			}
			text.Append("\n$item_onehanded");
			return;
			IL_43:
			text.Append("\n$item_twohanded");
		}

		// Token: 0x0600114B RID: 4427 RVA: 0x00077D90 File Offset: 0x00075F90
		public static string GetTooltip(ItemDrop.ItemData item, int qualityLevel, bool crafting)
		{
			Player localPlayer = Player.m_localPlayer;
			StringBuilder stringBuilder = new StringBuilder(256);
			stringBuilder.Append(item.m_shared.m_description);
			stringBuilder.Append("\n\n");
			if (item.m_shared.m_dlc.Length > 0)
			{
				stringBuilder.Append("\n<color=aqua>$item_dlc</color>");
			}
			ItemDrop.ItemData.AddHandedTip(item, stringBuilder);
			if (item.m_crafterID != 0L)
			{
				stringBuilder.AppendFormat("\n$item_crafter: <color=orange>{0}</color>", item.m_crafterName);
			}
			if (!item.m_shared.m_teleportable)
			{
				stringBuilder.Append("\n<color=orange>$item_noteleport</color>");
			}
			if (item.m_shared.m_value > 0)
			{
				stringBuilder.AppendFormat("\n$item_value: <color=orange>{0}  ({1})</color>", item.GetValue(), item.m_shared.m_value);
			}
			stringBuilder.AppendFormat("\n$item_weight: <color=orange>{0}</color>", item.GetWeight().ToString("0.0"));
			if (item.m_shared.m_maxQuality > 1)
			{
				stringBuilder.AppendFormat("\n$item_quality: <color=orange>{0}</color>", qualityLevel);
			}
			if (item.m_shared.m_useDurability)
			{
				if (crafting)
				{
					float maxDurability = item.GetMaxDurability(qualityLevel);
					stringBuilder.AppendFormat("\n$item_durability: <color=orange>{0}</color>", maxDurability);
				}
				else
				{
					float maxDurability2 = item.GetMaxDurability(qualityLevel);
					float durability = item.m_durability;
					stringBuilder.AppendFormat("\n$item_durability: <color=orange>{0}%</color> <color=yellow>({1}/{2})</color>", (item.GetDurabilityPercentage() * 100f).ToString("0"), durability.ToString("0"), maxDurability2.ToString("0"));
				}
				if (item.m_shared.m_canBeReparied)
				{
					Recipe recipe = ObjectDB.instance.GetRecipe(item);
					if (recipe != null)
					{
						int minStationLevel = recipe.m_minStationLevel;
						stringBuilder.AppendFormat("\n$item_repairlevel: <color=orange>{0}</color>", minStationLevel.ToString());
					}
				}
			}
			switch (item.m_shared.m_itemType)
			{
			case ItemDrop.ItemData.ItemType.Consumable:
			{
				if (item.m_shared.m_food > 0f)
				{
					stringBuilder.AppendFormat("\n$item_food_health: <color=orange>{0}</color>", item.m_shared.m_food);
					stringBuilder.AppendFormat("\n$item_food_stamina: <color=orange>{0}</color>", item.m_shared.m_foodStamina);
					stringBuilder.AppendFormat("\n$item_food_duration: <color=orange>{0}s</color>", item.m_shared.m_foodBurnTime);
					stringBuilder.AppendFormat("\n$item_food_regen: <color=orange>{0} hp/tick</color>", item.m_shared.m_foodRegen);
				}
				string statusEffectTooltip = item.GetStatusEffectTooltip();
				if (statusEffectTooltip.Length > 0)
				{
					stringBuilder.Append("\n\n");
					stringBuilder.Append(statusEffectTooltip);
				}
				break;
			}
			case ItemDrop.ItemData.ItemType.OneHandedWeapon:
			case ItemDrop.ItemData.ItemType.Bow:
			case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
			case ItemDrop.ItemData.ItemType.Torch:
			{
				stringBuilder.Append(item.GetDamage(qualityLevel).GetTooltipString(item.m_shared.m_skillType));
				stringBuilder.AppendFormat("\n$item_blockpower: <color=orange>{0}</color> <color=yellow>({1})</color>", item.GetBaseBlockPower(qualityLevel), item.GetBlockPowerTooltip(qualityLevel).ToString("0"));
				if (item.m_shared.m_timedBlockBonus > 1f)
				{
					stringBuilder.AppendFormat("\n$item_deflection: <color=orange>{0}</color>", item.GetDeflectionForce(qualityLevel));
					stringBuilder.AppendFormat("\n$item_parrybonus: <color=orange>{0}x</color>", item.m_shared.m_timedBlockBonus);
				}
				stringBuilder.AppendFormat("\n$item_knockback: <color=orange>{0}</color>", item.m_shared.m_attackForce);
				stringBuilder.AppendFormat("\n$item_backstab: <color=orange>{0}x</color>", item.m_shared.m_backstabBonus);
				string projectileTooltip = item.GetProjectileTooltip(qualityLevel);
				if (projectileTooltip.Length > 0)
				{
					stringBuilder.Append("\n\n");
					stringBuilder.Append(projectileTooltip);
				}
				string statusEffectTooltip2 = item.GetStatusEffectTooltip();
				if (statusEffectTooltip2.Length > 0)
				{
					stringBuilder.Append("\n\n");
					stringBuilder.Append(statusEffectTooltip2);
				}
				break;
			}
			case ItemDrop.ItemData.ItemType.Shield:
				stringBuilder.AppendFormat("\n$item_blockpower: <color=orange>{0}</color> <color=yellow>({1})</color>", item.GetBaseBlockPower(qualityLevel), item.GetBlockPowerTooltip(qualityLevel).ToString("0"));
				if (item.m_shared.m_timedBlockBonus > 1f)
				{
					stringBuilder.AppendFormat("\n$item_deflection: <color=orange>{0}</color>", item.GetDeflectionForce(qualityLevel));
					stringBuilder.AppendFormat("\n$item_parrybonus: <color=orange>{0}x</color>", item.m_shared.m_timedBlockBonus);
				}
				break;
			case ItemDrop.ItemData.ItemType.Helmet:
			case ItemDrop.ItemData.ItemType.Chest:
			case ItemDrop.ItemData.ItemType.Legs:
			case ItemDrop.ItemData.ItemType.Shoulder:
			{
				stringBuilder.AppendFormat("\n$item_armor: <color=orange>{0}</color>", item.GetArmor(qualityLevel));
				string damageModifiersTooltipString = SE_Stats.GetDamageModifiersTooltipString(item.m_shared.m_damageModifiers);
				if (damageModifiersTooltipString.Length > 0)
				{
					stringBuilder.Append(damageModifiersTooltipString);
				}
				string statusEffectTooltip3 = item.GetStatusEffectTooltip();
				if (statusEffectTooltip3.Length > 0)
				{
					stringBuilder.Append("\n\n");
					stringBuilder.Append(statusEffectTooltip3);
				}
				break;
			}
			case ItemDrop.ItemData.ItemType.Ammo:
				stringBuilder.Append(item.GetDamage(qualityLevel).GetTooltipString(item.m_shared.m_skillType));
				stringBuilder.AppendFormat("\n$item_knockback: <color=orange>{0}</color>", item.m_shared.m_attackForce);
				break;
			}
			if (item.m_shared.m_movementModifier != 0f && localPlayer != null)
			{
				float equipmentMovementModifier = localPlayer.GetEquipmentMovementModifier();
				stringBuilder.AppendFormat("\n$item_movement_modifier: <color=orange>{0}%</color> ($item_total:<color=yellow>{1}%</color>)", (item.m_shared.m_movementModifier * 100f).ToString("+0;-0"), (equipmentMovementModifier * 100f).ToString("+0;-0"));
			}
			string setStatusEffectTooltip = item.GetSetStatusEffectTooltip();
			if (setStatusEffectTooltip.Length > 0)
			{
				stringBuilder.AppendFormat("\n\n$item_seteffect (<color=orange>{0}</color> $item_parts):<color=orange>{1}</color>", item.m_shared.m_setSize, setStatusEffectTooltip);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x0600114C RID: 4428 RVA: 0x00078340 File Offset: 0x00076540
		private string GetStatusEffectTooltip()
		{
			if (this.m_shared.m_attackStatusEffect)
			{
				return this.m_shared.m_attackStatusEffect.GetTooltipString();
			}
			if (this.m_shared.m_consumeStatusEffect)
			{
				return this.m_shared.m_consumeStatusEffect.GetTooltipString();
			}
			return "";
		}

		// Token: 0x0600114D RID: 4429 RVA: 0x00078398 File Offset: 0x00076598
		private string GetSetStatusEffectTooltip()
		{
			if (this.m_shared.m_setStatusEffect)
			{
				StatusEffect setStatusEffect = this.m_shared.m_setStatusEffect;
				if (setStatusEffect != null)
				{
					return setStatusEffect.GetTooltipString();
				}
			}
			return "";
		}

		// Token: 0x0600114E RID: 4430 RVA: 0x000783D8 File Offset: 0x000765D8
		private string GetProjectileTooltip(int itemQuality)
		{
			string text = "";
			if (this.m_shared.m_attack.m_attackProjectile)
			{
				IProjectile component = this.m_shared.m_attack.m_attackProjectile.GetComponent<IProjectile>();
				if (component != null)
				{
					text += component.GetTooltipString(itemQuality);
				}
			}
			if (this.m_shared.m_spawnOnHit)
			{
				IProjectile component2 = this.m_shared.m_spawnOnHit.GetComponent<IProjectile>();
				if (component2 != null)
				{
					text += component2.GetTooltipString(itemQuality);
				}
			}
			return text;
		}

		// Token: 0x0400114A RID: 4426
		public int m_stack = 1;

		// Token: 0x0400114B RID: 4427
		public float m_durability = 100f;

		// Token: 0x0400114C RID: 4428
		public int m_quality = 1;

		// Token: 0x0400114D RID: 4429
		public int m_variant;

		// Token: 0x0400114E RID: 4430
		public ItemDrop.ItemData.SharedData m_shared;

		// Token: 0x0400114F RID: 4431
		[NonSerialized]
		public long m_crafterID;

		// Token: 0x04001150 RID: 4432
		[NonSerialized]
		public string m_crafterName = "";

		// Token: 0x04001151 RID: 4433
		[NonSerialized]
		public Vector2i m_gridPos = Vector2i.zero;

		// Token: 0x04001152 RID: 4434
		[NonSerialized]
		public bool m_equiped;

		// Token: 0x04001153 RID: 4435
		[NonSerialized]
		public GameObject m_dropPrefab;

		// Token: 0x04001154 RID: 4436
		[NonSerialized]
		public float m_lastAttackTime;

		// Token: 0x04001155 RID: 4437
		[NonSerialized]
		public GameObject m_lastProjectile;

		// Token: 0x020001C0 RID: 448
		public enum ItemType
		{
			// Token: 0x0400134D RID: 4941
			None,
			// Token: 0x0400134E RID: 4942
			Material,
			// Token: 0x0400134F RID: 4943
			Consumable,
			// Token: 0x04001350 RID: 4944
			OneHandedWeapon,
			// Token: 0x04001351 RID: 4945
			Bow,
			// Token: 0x04001352 RID: 4946
			Shield,
			// Token: 0x04001353 RID: 4947
			Helmet,
			// Token: 0x04001354 RID: 4948
			Chest,
			// Token: 0x04001355 RID: 4949
			Ammo = 9,
			// Token: 0x04001356 RID: 4950
			Customization,
			// Token: 0x04001357 RID: 4951
			Legs,
			// Token: 0x04001358 RID: 4952
			Hands,
			// Token: 0x04001359 RID: 4953
			Trophie,
			// Token: 0x0400135A RID: 4954
			TwoHandedWeapon,
			// Token: 0x0400135B RID: 4955
			Torch,
			// Token: 0x0400135C RID: 4956
			Misc,
			// Token: 0x0400135D RID: 4957
			Shoulder,
			// Token: 0x0400135E RID: 4958
			Utility,
			// Token: 0x0400135F RID: 4959
			Tool,
			// Token: 0x04001360 RID: 4960
			Attach_Atgeir
		}

		// Token: 0x020001C1 RID: 449
		public enum AnimationState
		{
			// Token: 0x04001362 RID: 4962
			Unarmed,
			// Token: 0x04001363 RID: 4963
			OneHanded,
			// Token: 0x04001364 RID: 4964
			TwoHandedClub,
			// Token: 0x04001365 RID: 4965
			Bow,
			// Token: 0x04001366 RID: 4966
			Shield,
			// Token: 0x04001367 RID: 4967
			Torch,
			// Token: 0x04001368 RID: 4968
			LeftTorch,
			// Token: 0x04001369 RID: 4969
			Atgeir,
			// Token: 0x0400136A RID: 4970
			TwoHandedAxe,
			// Token: 0x0400136B RID: 4971
			FishingRod
		}

		// Token: 0x020001C2 RID: 450
		public enum AiTarget
		{
			// Token: 0x0400136D RID: 4973
			Enemy,
			// Token: 0x0400136E RID: 4974
			FriendHurt,
			// Token: 0x0400136F RID: 4975
			Friend
		}

		// Token: 0x020001C3 RID: 451
		[Serializable]
		public class SharedData
		{
			// Token: 0x04001370 RID: 4976
			public string m_name = "";

			// Token: 0x04001371 RID: 4977
			public string m_dlc = "";

			// Token: 0x04001372 RID: 4978
			public ItemDrop.ItemData.ItemType m_itemType = ItemDrop.ItemData.ItemType.Misc;

			// Token: 0x04001373 RID: 4979
			public Sprite[] m_icons = new Sprite[0];

			// Token: 0x04001374 RID: 4980
			public ItemDrop.ItemData.ItemType m_attachOverride;

			// Token: 0x04001375 RID: 4981
			[TextArea]
			public string m_description = "";

			// Token: 0x04001376 RID: 4982
			public int m_maxStackSize = 1;

			// Token: 0x04001377 RID: 4983
			public int m_maxQuality = 1;

			// Token: 0x04001378 RID: 4984
			public float m_weight = 1f;

			// Token: 0x04001379 RID: 4985
			public int m_value;

			// Token: 0x0400137A RID: 4986
			public bool m_teleportable = true;

			// Token: 0x0400137B RID: 4987
			public bool m_questItem;

			// Token: 0x0400137C RID: 4988
			public float m_equipDuration = 1f;

			// Token: 0x0400137D RID: 4989
			public int m_variants;

			// Token: 0x0400137E RID: 4990
			public Vector2Int m_trophyPos = Vector2Int.zero;

			// Token: 0x0400137F RID: 4991
			public PieceTable m_buildPieces;

			// Token: 0x04001380 RID: 4992
			public bool m_centerCamera;

			// Token: 0x04001381 RID: 4993
			public string m_setName = "";

			// Token: 0x04001382 RID: 4994
			public int m_setSize;

			// Token: 0x04001383 RID: 4995
			public StatusEffect m_setStatusEffect;

			// Token: 0x04001384 RID: 4996
			public StatusEffect m_equipStatusEffect;

			// Token: 0x04001385 RID: 4997
			public float m_movementModifier;

			// Token: 0x04001386 RID: 4998
			[Header("Food settings")]
			public float m_food;

			// Token: 0x04001387 RID: 4999
			public float m_foodStamina;

			// Token: 0x04001388 RID: 5000
			public float m_foodBurnTime;

			// Token: 0x04001389 RID: 5001
			public float m_foodRegen;

			// Token: 0x0400138A RID: 5002
			public Color m_foodColor = Color.white;

			// Token: 0x0400138B RID: 5003
			[Header("Armor settings")]
			public Material m_armorMaterial;

			// Token: 0x0400138C RID: 5004
			public bool m_helmetHideHair = true;

			// Token: 0x0400138D RID: 5005
			public float m_armor = 10f;

			// Token: 0x0400138E RID: 5006
			public float m_armorPerLevel = 1f;

			// Token: 0x0400138F RID: 5007
			public List<HitData.DamageModPair> m_damageModifiers = new List<HitData.DamageModPair>();

			// Token: 0x04001390 RID: 5008
			[Header("Shield settings")]
			public float m_blockPower = 10f;

			// Token: 0x04001391 RID: 5009
			public float m_blockPowerPerLevel;

			// Token: 0x04001392 RID: 5010
			public float m_deflectionForce;

			// Token: 0x04001393 RID: 5011
			public float m_deflectionForcePerLevel;

			// Token: 0x04001394 RID: 5012
			public float m_timedBlockBonus = 1.5f;

			// Token: 0x04001395 RID: 5013
			[Header("Weapon")]
			public ItemDrop.ItemData.AnimationState m_animationState = ItemDrop.ItemData.AnimationState.OneHanded;

			// Token: 0x04001396 RID: 5014
			public Skills.SkillType m_skillType = Skills.SkillType.Swords;

			// Token: 0x04001397 RID: 5015
			public int m_toolTier;

			// Token: 0x04001398 RID: 5016
			public HitData.DamageTypes m_damages;

			// Token: 0x04001399 RID: 5017
			public HitData.DamageTypes m_damagesPerLevel;

			// Token: 0x0400139A RID: 5018
			public float m_attackForce = 30f;

			// Token: 0x0400139B RID: 5019
			public float m_backstabBonus = 4f;

			// Token: 0x0400139C RID: 5020
			public bool m_dodgeable;

			// Token: 0x0400139D RID: 5021
			public bool m_blockable;

			// Token: 0x0400139E RID: 5022
			public StatusEffect m_attackStatusEffect;

			// Token: 0x0400139F RID: 5023
			public GameObject m_spawnOnHit;

			// Token: 0x040013A0 RID: 5024
			public GameObject m_spawnOnHitTerrain;

			// Token: 0x040013A1 RID: 5025
			[Header("Attacks")]
			public Attack m_attack;

			// Token: 0x040013A2 RID: 5026
			public Attack m_secondaryAttack;

			// Token: 0x040013A3 RID: 5027
			[Header("Durability")]
			public bool m_useDurability;

			// Token: 0x040013A4 RID: 5028
			public bool m_destroyBroken = true;

			// Token: 0x040013A5 RID: 5029
			public bool m_canBeReparied = true;

			// Token: 0x040013A6 RID: 5030
			public float m_maxDurability = 100f;

			// Token: 0x040013A7 RID: 5031
			public float m_durabilityPerLevel = 50f;

			// Token: 0x040013A8 RID: 5032
			public float m_useDurabilityDrain = 1f;

			// Token: 0x040013A9 RID: 5033
			public float m_durabilityDrain;

			// Token: 0x040013AA RID: 5034
			[Header("Hold")]
			public float m_holdDurationMin;

			// Token: 0x040013AB RID: 5035
			public float m_holdStaminaDrain;

			// Token: 0x040013AC RID: 5036
			public string m_holdAnimationState = "";

			// Token: 0x040013AD RID: 5037
			[Header("Ammo")]
			public string m_ammoType = "";

			// Token: 0x040013AE RID: 5038
			[Header("AI")]
			public float m_aiAttackRange = 2f;

			// Token: 0x040013AF RID: 5039
			public float m_aiAttackRangeMin;

			// Token: 0x040013B0 RID: 5040
			public float m_aiAttackInterval = 2f;

			// Token: 0x040013B1 RID: 5041
			public float m_aiAttackMaxAngle = 5f;

			// Token: 0x040013B2 RID: 5042
			public bool m_aiWhenFlying = true;

			// Token: 0x040013B3 RID: 5043
			public bool m_aiWhenWalking = true;

			// Token: 0x040013B4 RID: 5044
			public bool m_aiWhenSwiming = true;

			// Token: 0x040013B5 RID: 5045
			public bool m_aiPrioritized;

			// Token: 0x040013B6 RID: 5046
			public ItemDrop.ItemData.AiTarget m_aiTargetType;

			// Token: 0x040013B7 RID: 5047
			[Header("Effects")]
			public EffectList m_hitEffect = new EffectList();

			// Token: 0x040013B8 RID: 5048
			public EffectList m_hitTerrainEffect = new EffectList();

			// Token: 0x040013B9 RID: 5049
			public EffectList m_blockEffect = new EffectList();

			// Token: 0x040013BA RID: 5050
			public EffectList m_startEffect = new EffectList();

			// Token: 0x040013BB RID: 5051
			public EffectList m_holdStartEffect = new EffectList();

			// Token: 0x040013BC RID: 5052
			public EffectList m_triggerEffect = new EffectList();

			// Token: 0x040013BD RID: 5053
			public EffectList m_trailStartEffect = new EffectList();

			// Token: 0x040013BE RID: 5054
			[Header("Consumable")]
			public StatusEffect m_consumeStatusEffect;
		}
	}
}
