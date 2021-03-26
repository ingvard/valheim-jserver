using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200000A RID: 10
public class Humanoid : Character
{
	// Token: 0x0600010A RID: 266 RVA: 0x00007E50 File Offset: 0x00006050
	protected override void Awake()
	{
		base.Awake();
		this.m_visEquipment = base.GetComponent<VisEquipment>();
		if (Humanoid.statef == 0)
		{
			Humanoid.statef = ZSyncAnimation.GetHash("statef");
			Humanoid.statei = ZSyncAnimation.GetHash("statei");
			Humanoid.blocking = ZSyncAnimation.GetHash("blocking");
		}
		if (Humanoid.isBlockingHash == 0)
		{
			Humanoid.isBlockingHash = "IsBlocking".GetStableHashCode();
		}
	}

	// Token: 0x0600010B RID: 267 RVA: 0x00007EB9 File Offset: 0x000060B9
	protected override void Start()
	{
		base.Start();
		if (!this.IsPlayer())
		{
			this.GiveDefaultItems();
		}
	}

	// Token: 0x0600010C RID: 268 RVA: 0x00007ED0 File Offset: 0x000060D0
	public void GiveDefaultItems()
	{
		foreach (GameObject prefab in this.m_defaultItems)
		{
			this.GiveDefaultItem(prefab);
		}
		if (this.m_randomWeapon.Length != 0 || this.m_randomArmor.Length != 0 || this.m_randomShield.Length != 0 || this.m_randomSets.Length != 0)
		{
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState(this.m_nview.GetZDO().m_uid.GetHashCode());
			if (this.m_randomShield.Length != 0)
			{
				GameObject gameObject = this.m_randomShield[UnityEngine.Random.Range(0, this.m_randomShield.Length)];
				if (gameObject)
				{
					this.GiveDefaultItem(gameObject);
				}
			}
			if (this.m_randomWeapon.Length != 0)
			{
				GameObject gameObject2 = this.m_randomWeapon[UnityEngine.Random.Range(0, this.m_randomWeapon.Length)];
				if (gameObject2)
				{
					this.GiveDefaultItem(gameObject2);
				}
			}
			if (this.m_randomArmor.Length != 0)
			{
				GameObject gameObject3 = this.m_randomArmor[UnityEngine.Random.Range(0, this.m_randomArmor.Length)];
				if (gameObject3)
				{
					this.GiveDefaultItem(gameObject3);
				}
			}
			if (this.m_randomSets.Length != 0)
			{
				foreach (GameObject prefab2 in this.m_randomSets[UnityEngine.Random.Range(0, this.m_randomSets.Length)].m_items)
				{
					this.GiveDefaultItem(prefab2);
				}
			}
			UnityEngine.Random.state = state;
		}
	}

	// Token: 0x0600010D RID: 269 RVA: 0x00008024 File Offset: 0x00006224
	private void GiveDefaultItem(GameObject prefab)
	{
		ItemDrop.ItemData itemData = this.PickupPrefab(prefab, 0);
		if (itemData != null && !itemData.IsWeapon())
		{
			this.EquipItem(itemData, false);
		}
	}

	// Token: 0x0600010E RID: 270 RVA: 0x00008050 File Offset: 0x00006250
	protected override void FixedUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview == null || this.m_nview.IsOwner())
		{
			this.UpdateAttack(Time.fixedDeltaTime);
			this.UpdateEquipment(Time.fixedDeltaTime);
			this.UpdateBlock(Time.fixedDeltaTime);
		}
		base.FixedUpdate();
	}

	// Token: 0x0600010F RID: 271 RVA: 0x000080B0 File Offset: 0x000062B0
	public override bool InAttack()
	{
		if (this.m_animator.IsInTransition(0))
		{
			return this.m_animator.GetCurrentAnimatorStateInfo(0).tagHash == Humanoid.m_animatorTagAttack || this.m_animator.GetNextAnimatorStateInfo(0).tagHash == Humanoid.m_animatorTagAttack;
		}
		return this.m_animator.GetCurrentAnimatorStateInfo(0).tagHash == Humanoid.m_animatorTagAttack;
	}

	// Token: 0x06000110 RID: 272 RVA: 0x00008128 File Offset: 0x00006328
	public override bool StartAttack(Character target, bool secondaryAttack)
	{
		this.AbortEquipQueue();
		if ((this.InAttack() && !this.HaveQueuedChain()) || this.InDodge() || !this.CanMove() || base.IsKnockedBack() || base.IsStaggering() || this.InMinorAction())
		{
			return false;
		}
		ItemDrop.ItemData currentWeapon = this.GetCurrentWeapon();
		if (currentWeapon == null)
		{
			return false;
		}
		if (this.m_currentAttack != null)
		{
			this.m_currentAttack.Stop();
			this.m_previousAttack = this.m_currentAttack;
			this.m_currentAttack = null;
		}
		Attack attack;
		if (secondaryAttack)
		{
			if (!currentWeapon.HaveSecondaryAttack())
			{
				return false;
			}
			attack = currentWeapon.m_shared.m_secondaryAttack.Clone();
		}
		else
		{
			if (!currentWeapon.HavePrimaryAttack())
			{
				return false;
			}
			attack = currentWeapon.m_shared.m_attack.Clone();
		}
		if (attack.Start(this, this.m_body, this.m_zanim, this.m_animEvent, this.m_visEquipment, currentWeapon, this.m_previousAttack, this.m_timeSinceLastAttack, this.GetAttackDrawPercentage()))
		{
			this.m_currentAttack = attack;
			this.m_lastCombatTimer = 0f;
			return true;
		}
		return false;
	}

	// Token: 0x06000111 RID: 273 RVA: 0x0000822C File Offset: 0x0000642C
	public float GetAttackDrawPercentage()
	{
		ItemDrop.ItemData currentWeapon = this.GetCurrentWeapon();
		if (currentWeapon == null || currentWeapon.m_shared.m_holdDurationMin <= 0f || this.m_attackDrawTime <= 0f)
		{
			return 0f;
		}
		float skillFactor = this.GetSkillFactor(currentWeapon.m_shared.m_skillType);
		float num = currentWeapon.m_shared.m_holdDurationMin * (1f - skillFactor);
		if (num <= 0f)
		{
			return 1f;
		}
		return Mathf.Clamp01(this.m_attackDrawTime / num);
	}

	// Token: 0x06000112 RID: 274 RVA: 0x000082AC File Offset: 0x000064AC
	private void UpdateEquipment(float dt)
	{
		if (!this.IsPlayer())
		{
			return;
		}
		if (base.IsSwiming() && !base.IsOnGround())
		{
			this.HideHandItems();
		}
		if (this.m_rightItem != null && this.m_rightItem.m_shared.m_useDurability)
		{
			this.DrainEquipedItemDurability(this.m_rightItem, dt);
		}
		if (this.m_leftItem != null && this.m_leftItem.m_shared.m_useDurability)
		{
			this.DrainEquipedItemDurability(this.m_leftItem, dt);
		}
		if (this.m_chestItem != null && this.m_chestItem.m_shared.m_useDurability)
		{
			this.DrainEquipedItemDurability(this.m_chestItem, dt);
		}
		if (this.m_legItem != null && this.m_legItem.m_shared.m_useDurability)
		{
			this.DrainEquipedItemDurability(this.m_legItem, dt);
		}
		if (this.m_helmetItem != null && this.m_helmetItem.m_shared.m_useDurability)
		{
			this.DrainEquipedItemDurability(this.m_helmetItem, dt);
		}
		if (this.m_shoulderItem != null && this.m_shoulderItem.m_shared.m_useDurability)
		{
			this.DrainEquipedItemDurability(this.m_shoulderItem, dt);
		}
		if (this.m_utilityItem != null && this.m_utilityItem.m_shared.m_useDurability)
		{
			this.DrainEquipedItemDurability(this.m_utilityItem, dt);
		}
	}

	// Token: 0x06000113 RID: 275 RVA: 0x000083EC File Offset: 0x000065EC
	private void DrainEquipedItemDurability(ItemDrop.ItemData item, float dt)
	{
		item.m_durability -= item.m_shared.m_durabilityDrain * dt;
		if (item.m_durability <= 0f)
		{
			this.Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize("$msg_broke", new string[]
			{
				item.m_shared.m_name
			}), 0, item.GetIcon());
			this.UnequipItem(item, false);
			if (item.m_shared.m_destroyBroken)
			{
				this.m_inventory.RemoveItem(item);
			}
		}
	}

	// Token: 0x06000114 RID: 276 RVA: 0x00008475 File Offset: 0x00006675
	protected override void OnDamaged(HitData hit)
	{
		this.SetCrouch(false);
	}

	// Token: 0x06000115 RID: 277 RVA: 0x00008480 File Offset: 0x00006680
	protected override void DamageArmorDurability(HitData hit)
	{
		List<ItemDrop.ItemData> list = new List<ItemDrop.ItemData>();
		if (this.m_chestItem != null)
		{
			list.Add(this.m_chestItem);
		}
		if (this.m_legItem != null)
		{
			list.Add(this.m_legItem);
		}
		if (this.m_helmetItem != null)
		{
			list.Add(this.m_helmetItem);
		}
		if (this.m_shoulderItem != null)
		{
			list.Add(this.m_shoulderItem);
		}
		if (list.Count == 0)
		{
			return;
		}
		float num = hit.GetTotalPhysicalDamage() + hit.GetTotalElementalDamage();
		if (num <= 0f)
		{
			return;
		}
		int index = UnityEngine.Random.Range(0, list.Count);
		ItemDrop.ItemData itemData = list[index];
		itemData.m_durability = Mathf.Max(0f, itemData.m_durability - num);
	}

	// Token: 0x06000116 RID: 278 RVA: 0x00008530 File Offset: 0x00006730
	public ItemDrop.ItemData GetCurrentWeapon()
	{
		if (this.m_rightItem != null && this.m_rightItem.IsWeapon())
		{
			return this.m_rightItem;
		}
		if (this.m_leftItem != null && this.m_leftItem.IsWeapon() && this.m_leftItem.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Torch)
		{
			return this.m_leftItem;
		}
		if (this.m_unarmedWeapon)
		{
			return this.m_unarmedWeapon.m_itemData;
		}
		return null;
	}

	// Token: 0x06000117 RID: 279 RVA: 0x000085A3 File Offset: 0x000067A3
	protected ItemDrop.ItemData GetCurrentBlocker()
	{
		if (this.m_leftItem != null)
		{
			return this.m_leftItem;
		}
		return this.GetCurrentWeapon();
	}

	// Token: 0x06000118 RID: 280 RVA: 0x000085BC File Offset: 0x000067BC
	private void UpdateAttack(float dt)
	{
		this.m_lastCombatTimer += dt;
		if (this.GetCurrentWeapon() != null && this.m_currentAttack != null)
		{
			this.m_currentAttack.Update(dt);
		}
		if (this.InAttack())
		{
			this.m_timeSinceLastAttack = 0f;
			return;
		}
		this.m_timeSinceLastAttack += dt;
	}

	// Token: 0x06000119 RID: 281 RVA: 0x00008615 File Offset: 0x00006815
	protected override float GetAttackSpeedFactorMovement()
	{
		if (!this.InAttack() || this.m_currentAttack == null)
		{
			return 1f;
		}
		if (!base.IsFlying() && !base.IsOnGround())
		{
			return 1f;
		}
		return this.m_currentAttack.m_speedFactor;
	}

	// Token: 0x0600011A RID: 282 RVA: 0x0000864E File Offset: 0x0000684E
	protected override float GetAttackSpeedFactorRotation()
	{
		if (this.InAttack() && this.m_currentAttack != null)
		{
			return this.m_currentAttack.m_speedFactorRotation;
		}
		return 1f;
	}

	// Token: 0x0600011B RID: 283 RVA: 0x000023E2 File Offset: 0x000005E2
	protected virtual bool HaveQueuedChain()
	{
		return false;
	}

	// Token: 0x0600011C RID: 284 RVA: 0x00008671 File Offset: 0x00006871
	public override void OnWeaponTrailStart()
	{
		if (this.m_nview.IsValid() && this.m_nview.IsOwner() && this.GetCurrentWeapon() != null && this.m_currentAttack != null)
		{
			this.m_currentAttack.OnTrailStart();
		}
	}

	// Token: 0x0600011D RID: 285 RVA: 0x000086A8 File Offset: 0x000068A8
	public override void OnAttackTrigger()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (this.GetCurrentWeapon() != null && this.m_currentAttack != null)
		{
			this.m_currentAttack.OnAttackTrigger();
		}
	}

	// Token: 0x0600011E RID: 286 RVA: 0x000086E0 File Offset: 0x000068E0
	public override void OnStopMoving()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.InAttack())
		{
			return;
		}
		if (this.GetCurrentWeapon() != null && this.m_currentAttack != null)
		{
			this.m_currentAttack.m_speedFactorRotation = 0f;
			this.m_currentAttack.m_speedFactorRotation = 0f;
		}
	}

	// Token: 0x0600011F RID: 287 RVA: 0x00008741 File Offset: 0x00006941
	public virtual Vector3 GetAimDir(Vector3 fromPoint)
	{
		return base.GetLookDir();
	}

	// Token: 0x06000120 RID: 288 RVA: 0x0000874C File Offset: 0x0000694C
	public ItemDrop.ItemData PickupPrefab(GameObject prefab, int stackSize = 0)
	{
		ZNetView.m_forceDisableInit = true;
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(prefab);
		ZNetView.m_forceDisableInit = false;
		if (stackSize > 0)
		{
			ItemDrop component = gameObject.GetComponent<ItemDrop>();
			component.m_itemData.m_stack = Mathf.Clamp(stackSize, 1, component.m_itemData.m_shared.m_maxStackSize);
		}
		if (this.Pickup(gameObject))
		{
			return gameObject.GetComponent<ItemDrop>().m_itemData;
		}
		UnityEngine.Object.Destroy(gameObject);
		return null;
	}

	// Token: 0x06000121 RID: 289 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool HaveUniqueKey(string name)
	{
		return false;
	}

	// Token: 0x06000122 RID: 290 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void AddUniqueKey(string name)
	{
	}

	// Token: 0x06000123 RID: 291 RVA: 0x000087B8 File Offset: 0x000069B8
	public bool Pickup(GameObject go)
	{
		if (this.IsTeleporting())
		{
			return false;
		}
		ItemDrop component = go.GetComponent<ItemDrop>();
		if (component == null)
		{
			return false;
		}
		if (!component.CanPickup())
		{
			return false;
		}
		if (this.m_inventory.ContainsItem(component.m_itemData))
		{
			return false;
		}
		if (component.m_itemData.m_shared.m_questItem && this.HaveUniqueKey(component.m_itemData.m_shared.m_name))
		{
			this.Message(MessageHud.MessageType.Center, "$msg_cantpickup", 0, null);
			return false;
		}
		bool flag = this.m_inventory.AddItem(component.m_itemData);
		if (this.m_nview.GetZDO() == null)
		{
			UnityEngine.Object.Destroy(go);
			return true;
		}
		if (!flag)
		{
			this.Message(MessageHud.MessageType.Center, "$msg_noroom", 0, null);
			return false;
		}
		if (component.m_itemData.m_shared.m_questItem)
		{
			this.AddUniqueKey(component.m_itemData.m_shared.m_name);
		}
		ZNetScene.instance.Destroy(go);
		if (flag && this.IsPlayer() && this.m_rightItem == null && this.m_hiddenRightItem == null && component.m_itemData.IsWeapon())
		{
			this.EquipItem(component.m_itemData, true);
		}
		this.m_pickupEffects.Create(base.transform.position, Quaternion.identity, null, 1f);
		if (this.IsPlayer())
		{
			base.ShowPickupMessage(component.m_itemData, component.m_itemData.m_stack);
		}
		return flag;
	}

	// Token: 0x06000124 RID: 292 RVA: 0x00008920 File Offset: 0x00006B20
	public void EquipBestWeapon(Character targetCreature, StaticTarget targetStatic, Character hurtFriend, Character friend)
	{
		List<ItemDrop.ItemData> allItems = this.m_inventory.GetAllItems();
		if (allItems.Count == 0)
		{
			return;
		}
		if (this.InAttack())
		{
			return;
		}
		float num = 0f;
		if (targetCreature)
		{
			float radius = targetCreature.GetRadius();
			num = Vector3.Distance(targetCreature.transform.position, base.transform.position) - radius;
		}
		else if (targetStatic)
		{
			num = Vector3.Distance(targetStatic.transform.position, base.transform.position);
		}
		float time = Time.time;
		base.IsFlying();
		base.IsSwiming();
		Humanoid.optimalWeapons.Clear();
		Humanoid.outofRangeWeapons.Clear();
		Humanoid.allWeapons.Clear();
		foreach (ItemDrop.ItemData itemData in allItems)
		{
			if (itemData.IsWeapon() && BaseAI.CanUseAttack(this, itemData))
			{
				if (itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.Enemy)
				{
					if (num >= itemData.m_shared.m_aiAttackRangeMin)
					{
						Humanoid.allWeapons.Add(itemData);
						if ((!(targetCreature == null) || !(targetStatic == null)) && time - itemData.m_lastAttackTime >= itemData.m_shared.m_aiAttackInterval)
						{
							if (num > itemData.m_shared.m_aiAttackRange)
							{
								Humanoid.outofRangeWeapons.Add(itemData);
							}
							else
							{
								if (itemData.m_shared.m_aiPrioritized)
								{
									this.EquipItem(itemData, true);
									return;
								}
								Humanoid.optimalWeapons.Add(itemData);
							}
						}
					}
				}
				else if (itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.FriendHurt)
				{
					if (!(hurtFriend == null) && time - itemData.m_lastAttackTime >= itemData.m_shared.m_aiAttackInterval)
					{
						if (itemData.m_shared.m_aiPrioritized)
						{
							this.EquipItem(itemData, true);
							return;
						}
						Humanoid.optimalWeapons.Add(itemData);
					}
				}
				else if (itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.Friend && !(friend == null) && time - itemData.m_lastAttackTime >= itemData.m_shared.m_aiAttackInterval)
				{
					if (itemData.m_shared.m_aiPrioritized)
					{
						this.EquipItem(itemData, true);
						return;
					}
					Humanoid.optimalWeapons.Add(itemData);
				}
			}
		}
		if (Humanoid.optimalWeapons.Count > 0)
		{
			this.EquipItem(Humanoid.optimalWeapons[UnityEngine.Random.Range(0, Humanoid.optimalWeapons.Count)], true);
			return;
		}
		if (Humanoid.outofRangeWeapons.Count > 0)
		{
			this.EquipItem(Humanoid.outofRangeWeapons[UnityEngine.Random.Range(0, Humanoid.outofRangeWeapons.Count)], true);
			return;
		}
		if (Humanoid.allWeapons.Count > 0)
		{
			this.EquipItem(Humanoid.allWeapons[UnityEngine.Random.Range(0, Humanoid.allWeapons.Count)], true);
			return;
		}
		ItemDrop.ItemData currentWeapon = this.GetCurrentWeapon();
		if (currentWeapon != null)
		{
			this.UnequipItem(currentWeapon, false);
		}
	}

	// Token: 0x06000125 RID: 293 RVA: 0x00008C48 File Offset: 0x00006E48
	public bool DropItem(Inventory inventory, ItemDrop.ItemData item, int amount)
	{
		if (amount == 0)
		{
			return false;
		}
		if (item.m_shared.m_questItem)
		{
			this.Message(MessageHud.MessageType.Center, "$msg_cantdrop", 0, null);
			return false;
		}
		if (amount > item.m_stack)
		{
			amount = item.m_stack;
		}
		this.RemoveFromEquipQueue(item);
		this.UnequipItem(item, false);
		if (this.m_hiddenLeftItem == item)
		{
			this.m_hiddenLeftItem = null;
			this.SetupVisEquipment(this.m_visEquipment, false);
		}
		if (this.m_hiddenRightItem == item)
		{
			this.m_hiddenRightItem = null;
			this.SetupVisEquipment(this.m_visEquipment, false);
		}
		if (amount == item.m_stack)
		{
			ZLog.Log(string.Concat(new object[]
			{
				"drop all ",
				amount,
				"  ",
				item.m_stack
			}));
			if (!inventory.RemoveItem(item))
			{
				ZLog.Log("Was not removed");
				return false;
			}
		}
		else
		{
			ZLog.Log(string.Concat(new object[]
			{
				"drop some ",
				amount,
				"  ",
				item.m_stack
			}));
			inventory.RemoveItem(item, amount);
		}
		ItemDrop itemDrop = ItemDrop.DropItem(item, amount, base.transform.position + base.transform.forward + base.transform.up, base.transform.rotation);
		if (this.IsPlayer())
		{
			itemDrop.OnPlayerDrop();
		}
		itemDrop.GetComponent<Rigidbody>().velocity = (base.transform.forward + Vector3.up) * 5f;
		this.m_zanim.SetTrigger("interact");
		this.m_dropEffects.Create(base.transform.position, Quaternion.identity, null, 1f);
		this.Message(MessageHud.MessageType.TopLeft, "$msg_dropped " + itemDrop.m_itemData.m_shared.m_name, itemDrop.m_itemData.m_stack, itemDrop.m_itemData.GetIcon());
		return true;
	}

	// Token: 0x06000126 RID: 294 RVA: 0x000027E0 File Offset: 0x000009E0
	protected virtual void SetPlaceMode(PieceTable buildPieces)
	{
	}

	// Token: 0x06000127 RID: 295 RVA: 0x00008E46 File Offset: 0x00007046
	public Inventory GetInventory()
	{
		return this.m_inventory;
	}

	// Token: 0x06000128 RID: 296 RVA: 0x00008E50 File Offset: 0x00007050
	public void UseItem(Inventory inventory, ItemDrop.ItemData item, bool fromInventoryGui)
	{
		if (inventory == null)
		{
			inventory = this.m_inventory;
		}
		if (!inventory.ContainsItem(item))
		{
			return;
		}
		GameObject hoverObject = this.GetHoverObject();
		Hoverable hoverable = hoverObject ? hoverObject.GetComponentInParent<Hoverable>() : null;
		if (hoverable != null && !fromInventoryGui)
		{
			Interactable componentInParent = hoverObject.GetComponentInParent<Interactable>();
			if (componentInParent != null && componentInParent.UseItem(this, item))
			{
				return;
			}
		}
		if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable)
		{
			if (this.ConsumeItem(inventory, item))
			{
				this.m_consumeItemEffects.Create(Player.m_localPlayer.transform.position, Quaternion.identity, null, 1f);
				this.m_zanim.SetTrigger("eat");
			}
			return;
		}
		if (inventory == this.m_inventory && this.ToggleEquiped(item))
		{
			return;
		}
		if (!fromInventoryGui)
		{
			if (hoverable != null)
			{
				this.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_cantuseon", new string[]
				{
					item.m_shared.m_name,
					hoverable.GetHoverName()
				}), 0, null);
				return;
			}
			this.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_useonwhat", new string[]
			{
				item.m_shared.m_name
			}), 0, null);
		}
	}

	// Token: 0x06000129 RID: 297 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void AbortEquipQueue()
	{
	}

	// Token: 0x0600012A RID: 298 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void RemoveFromEquipQueue(ItemDrop.ItemData item)
	{
	}

	// Token: 0x0600012B RID: 299 RVA: 0x00008F70 File Offset: 0x00007170
	protected virtual bool ToggleEquiped(ItemDrop.ItemData item)
	{
		if (!item.IsEquipable())
		{
			return false;
		}
		if (this.InAttack())
		{
			return true;
		}
		if (this.IsItemEquiped(item))
		{
			this.UnequipItem(item, true);
		}
		else
		{
			this.EquipItem(item, true);
		}
		return true;
	}

	// Token: 0x0600012C RID: 300 RVA: 0x00008FA3 File Offset: 0x000071A3
	public virtual bool CanConsumeItem(ItemDrop.ItemData item)
	{
		return item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable;
	}

	// Token: 0x0600012D RID: 301 RVA: 0x00008FB6 File Offset: 0x000071B6
	public virtual bool ConsumeItem(Inventory inventory, ItemDrop.ItemData item)
	{
		this.CanConsumeItem(item);
		return false;
	}

	// Token: 0x0600012E RID: 302 RVA: 0x00008FC4 File Offset: 0x000071C4
	public bool EquipItem(ItemDrop.ItemData item, bool triggerEquipEffects = true)
	{
		if (this.IsItemEquiped(item))
		{
			return false;
		}
		if (!this.m_inventory.ContainsItem(item))
		{
			return false;
		}
		if (this.InAttack() || this.InDodge())
		{
			return false;
		}
		if (this.IsPlayer() && !this.IsDead() && base.IsSwiming() && !base.IsOnGround())
		{
			return false;
		}
		if (item.m_shared.m_useDurability && item.m_durability <= 0f)
		{
			return false;
		}
		if (item.m_shared.m_dlc.Length > 0 && !DLCMan.instance.IsDLCInstalled(item.m_shared.m_dlc))
		{
			this.Message(MessageHud.MessageType.Center, "$msg_dlcrequired", 0, null);
			return false;
		}
		if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Tool)
		{
			this.UnequipItem(this.m_rightItem, triggerEquipEffects);
			this.UnequipItem(this.m_leftItem, triggerEquipEffects);
			this.m_rightItem = item;
			this.m_hiddenRightItem = null;
			this.m_hiddenLeftItem = null;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Torch)
		{
			if (this.m_rightItem != null && this.m_leftItem == null && this.m_rightItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.OneHandedWeapon)
			{
				this.m_leftItem = item;
			}
			else
			{
				this.UnequipItem(this.m_rightItem, triggerEquipEffects);
				if (this.m_leftItem != null && this.m_leftItem.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Shield)
				{
					this.UnequipItem(this.m_leftItem, triggerEquipEffects);
				}
				this.m_rightItem = item;
			}
			this.m_hiddenRightItem = null;
			this.m_hiddenLeftItem = null;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.OneHandedWeapon)
		{
			if (this.m_rightItem != null && this.m_rightItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Torch && this.m_leftItem == null)
			{
				ItemDrop.ItemData rightItem = this.m_rightItem;
				this.UnequipItem(this.m_rightItem, triggerEquipEffects);
				this.m_leftItem = rightItem;
				this.m_leftItem.m_equiped = true;
			}
			this.UnequipItem(this.m_rightItem, triggerEquipEffects);
			if (this.m_leftItem != null && this.m_leftItem.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Shield && this.m_leftItem.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Torch)
			{
				this.UnequipItem(this.m_leftItem, triggerEquipEffects);
			}
			this.m_rightItem = item;
			this.m_hiddenRightItem = null;
			this.m_hiddenLeftItem = null;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield)
		{
			this.UnequipItem(this.m_leftItem, triggerEquipEffects);
			if (this.m_rightItem != null && this.m_rightItem.m_shared.m_itemType != ItemDrop.ItemData.ItemType.OneHandedWeapon && this.m_rightItem.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Torch)
			{
				this.UnequipItem(this.m_rightItem, triggerEquipEffects);
			}
			this.m_leftItem = item;
			this.m_hiddenRightItem = null;
			this.m_hiddenLeftItem = null;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Bow)
		{
			this.UnequipItem(this.m_leftItem, triggerEquipEffects);
			this.UnequipItem(this.m_rightItem, triggerEquipEffects);
			this.m_leftItem = item;
			this.m_hiddenRightItem = null;
			this.m_hiddenLeftItem = null;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.TwoHandedWeapon)
		{
			this.UnequipItem(this.m_leftItem, triggerEquipEffects);
			this.UnequipItem(this.m_rightItem, triggerEquipEffects);
			this.m_rightItem = item;
			this.m_hiddenRightItem = null;
			this.m_hiddenLeftItem = null;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Chest)
		{
			this.UnequipItem(this.m_chestItem, triggerEquipEffects);
			this.m_chestItem = item;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Legs)
		{
			this.UnequipItem(this.m_legItem, triggerEquipEffects);
			this.m_legItem = item;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Ammo)
		{
			this.UnequipItem(this.m_ammoItem, triggerEquipEffects);
			this.m_ammoItem = item;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Helmet)
		{
			this.UnequipItem(this.m_helmetItem, triggerEquipEffects);
			this.m_helmetItem = item;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shoulder)
		{
			this.UnequipItem(this.m_shoulderItem, triggerEquipEffects);
			this.m_shoulderItem = item;
		}
		else if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Utility)
		{
			this.UnequipItem(this.m_utilityItem, triggerEquipEffects);
			this.m_utilityItem = item;
		}
		if (this.IsItemEquiped(item))
		{
			item.m_equiped = true;
		}
		this.SetupEquipment();
		if (triggerEquipEffects)
		{
			this.TriggerEquipEffect(item);
		}
		return true;
	}

	// Token: 0x0600012F RID: 303 RVA: 0x000093FC File Offset: 0x000075FC
	public void UnequipItem(ItemDrop.ItemData item, bool triggerEquipEffects = true)
	{
		if (item == null)
		{
			return;
		}
		if (this.m_hiddenLeftItem == item)
		{
			this.m_hiddenLeftItem = null;
			this.SetupVisEquipment(this.m_visEquipment, false);
		}
		if (this.m_hiddenRightItem == item)
		{
			this.m_hiddenRightItem = null;
			this.SetupVisEquipment(this.m_visEquipment, false);
		}
		if (this.IsItemEquiped(item))
		{
			if (item.IsWeapon())
			{
				if (this.m_currentAttack != null && this.m_currentAttack.GetWeapon() == item)
				{
					this.m_currentAttack.Stop();
					this.m_previousAttack = this.m_currentAttack;
					this.m_currentAttack = null;
				}
				if (!string.IsNullOrEmpty(item.m_shared.m_holdAnimationState))
				{
					this.m_zanim.SetBool(item.m_shared.m_holdAnimationState, false);
				}
				this.m_attackDrawTime = 0f;
			}
			if (this.m_rightItem == item)
			{
				this.m_rightItem = null;
			}
			else if (this.m_leftItem == item)
			{
				this.m_leftItem = null;
			}
			else if (this.m_chestItem == item)
			{
				this.m_chestItem = null;
			}
			else if (this.m_legItem == item)
			{
				this.m_legItem = null;
			}
			else if (this.m_ammoItem == item)
			{
				this.m_ammoItem = null;
			}
			else if (this.m_helmetItem == item)
			{
				this.m_helmetItem = null;
			}
			else if (this.m_shoulderItem == item)
			{
				this.m_shoulderItem = null;
			}
			else if (this.m_utilityItem == item)
			{
				this.m_utilityItem = null;
			}
			item.m_equiped = false;
			this.SetupEquipment();
			if (triggerEquipEffects)
			{
				this.TriggerEquipEffect(item);
			}
		}
	}

	// Token: 0x06000130 RID: 304 RVA: 0x00009568 File Offset: 0x00007768
	private void TriggerEquipEffect(ItemDrop.ItemData item)
	{
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		if (Time.frameCount == this.m_lastEquipEffectFrame)
		{
			return;
		}
		this.m_lastEquipEffectFrame = Time.frameCount;
		this.m_equipEffects.Create(base.transform.position, Quaternion.identity, null, 1f);
	}

	// Token: 0x06000131 RID: 305 RVA: 0x000095C0 File Offset: 0x000077C0
	public void UnequipAllItems()
	{
		if (this.m_rightItem != null)
		{
			this.UnequipItem(this.m_rightItem, false);
		}
		if (this.m_leftItem != null)
		{
			this.UnequipItem(this.m_leftItem, false);
		}
		if (this.m_chestItem != null)
		{
			this.UnequipItem(this.m_chestItem, false);
		}
		if (this.m_legItem != null)
		{
			this.UnequipItem(this.m_legItem, false);
		}
		if (this.m_helmetItem != null)
		{
			this.UnequipItem(this.m_helmetItem, false);
		}
		if (this.m_ammoItem != null)
		{
			this.UnequipItem(this.m_ammoItem, false);
		}
		if (this.m_shoulderItem != null)
		{
			this.UnequipItem(this.m_shoulderItem, false);
		}
		if (this.m_utilityItem != null)
		{
			this.UnequipItem(this.m_utilityItem, false);
		}
	}

	// Token: 0x06000132 RID: 306 RVA: 0x00009678 File Offset: 0x00007878
	protected override void OnRagdollCreated(Ragdoll ragdoll)
	{
		VisEquipment component = ragdoll.GetComponent<VisEquipment>();
		if (component)
		{
			this.SetupVisEquipment(component, true);
		}
	}

	// Token: 0x06000133 RID: 307 RVA: 0x0000969C File Offset: 0x0000789C
	protected virtual void SetupVisEquipment(VisEquipment visEq, bool isRagdoll)
	{
		if (!isRagdoll)
		{
			visEq.SetLeftItem((this.m_leftItem != null) ? this.m_leftItem.m_dropPrefab.name : "", (this.m_leftItem != null) ? this.m_leftItem.m_variant : 0);
			visEq.SetRightItem((this.m_rightItem != null) ? this.m_rightItem.m_dropPrefab.name : "");
			if (this.IsPlayer())
			{
				visEq.SetLeftBackItem((this.m_hiddenLeftItem != null) ? this.m_hiddenLeftItem.m_dropPrefab.name : "", (this.m_hiddenLeftItem != null) ? this.m_hiddenLeftItem.m_variant : 0);
				visEq.SetRightBackItem((this.m_hiddenRightItem != null) ? this.m_hiddenRightItem.m_dropPrefab.name : "");
			}
		}
		visEq.SetChestItem((this.m_chestItem != null) ? this.m_chestItem.m_dropPrefab.name : "");
		visEq.SetLegItem((this.m_legItem != null) ? this.m_legItem.m_dropPrefab.name : "");
		visEq.SetHelmetItem((this.m_helmetItem != null) ? this.m_helmetItem.m_dropPrefab.name : "");
		visEq.SetShoulderItem((this.m_shoulderItem != null) ? this.m_shoulderItem.m_dropPrefab.name : "", (this.m_shoulderItem != null) ? this.m_shoulderItem.m_variant : 0);
		visEq.SetUtilityItem((this.m_utilityItem != null) ? this.m_utilityItem.m_dropPrefab.name : "");
		if (this.IsPlayer())
		{
			visEq.SetBeardItem(this.m_beardItem);
			visEq.SetHairItem(this.m_hairItem);
		}
	}

	// Token: 0x06000134 RID: 308 RVA: 0x00009868 File Offset: 0x00007A68
	private void SetupEquipment()
	{
		if (this.m_visEquipment && (this.m_nview.GetZDO() == null || this.m_nview.IsOwner()))
		{
			this.SetupVisEquipment(this.m_visEquipment, false);
		}
		if (this.m_nview.GetZDO() != null)
		{
			this.UpdateEquipmentStatusEffects();
			if (this.m_rightItem != null && this.m_rightItem.m_shared.m_buildPieces)
			{
				this.SetPlaceMode(this.m_rightItem.m_shared.m_buildPieces);
			}
			else
			{
				this.SetPlaceMode(null);
			}
			this.SetupAnimationState();
		}
	}

	// Token: 0x06000135 RID: 309 RVA: 0x00009900 File Offset: 0x00007B00
	private void SetupAnimationState()
	{
		if (this.m_leftItem != null)
		{
			if (this.m_leftItem.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Torch)
			{
				this.SetAnimationState(ItemDrop.ItemData.AnimationState.LeftTorch);
				return;
			}
			this.SetAnimationState(this.m_leftItem.m_shared.m_animationState);
			return;
		}
		else
		{
			if (this.m_rightItem != null)
			{
				this.SetAnimationState(this.m_rightItem.m_shared.m_animationState);
				return;
			}
			if (this.m_unarmedWeapon != null)
			{
				this.SetAnimationState(this.m_unarmedWeapon.m_itemData.m_shared.m_animationState);
			}
			return;
		}
	}

	// Token: 0x06000136 RID: 310 RVA: 0x00009990 File Offset: 0x00007B90
	private void SetAnimationState(ItemDrop.ItemData.AnimationState state)
	{
		this.m_zanim.SetFloat(Humanoid.statef, (float)state);
		this.m_zanim.SetInt(Humanoid.statei, (int)state);
	}

	// Token: 0x06000137 RID: 311 RVA: 0x000099B8 File Offset: 0x00007BB8
	public bool IsSitting()
	{
		return this.m_animator.GetCurrentAnimatorStateInfo(0).tagHash == Character.m_animatorTagSitting;
	}

	// Token: 0x06000138 RID: 312 RVA: 0x000099E0 File Offset: 0x00007BE0
	private void UpdateEquipmentStatusEffects()
	{
		HashSet<StatusEffect> hashSet = new HashSet<StatusEffect>();
		if (this.m_leftItem != null && this.m_leftItem.m_shared.m_equipStatusEffect)
		{
			hashSet.Add(this.m_leftItem.m_shared.m_equipStatusEffect);
		}
		if (this.m_rightItem != null && this.m_rightItem.m_shared.m_equipStatusEffect)
		{
			hashSet.Add(this.m_rightItem.m_shared.m_equipStatusEffect);
		}
		if (this.m_chestItem != null && this.m_chestItem.m_shared.m_equipStatusEffect)
		{
			hashSet.Add(this.m_chestItem.m_shared.m_equipStatusEffect);
		}
		if (this.m_legItem != null && this.m_legItem.m_shared.m_equipStatusEffect)
		{
			hashSet.Add(this.m_legItem.m_shared.m_equipStatusEffect);
		}
		if (this.m_helmetItem != null && this.m_helmetItem.m_shared.m_equipStatusEffect)
		{
			hashSet.Add(this.m_helmetItem.m_shared.m_equipStatusEffect);
		}
		if (this.m_shoulderItem != null && this.m_shoulderItem.m_shared.m_equipStatusEffect)
		{
			hashSet.Add(this.m_shoulderItem.m_shared.m_equipStatusEffect);
		}
		if (this.m_utilityItem != null && this.m_utilityItem.m_shared.m_equipStatusEffect)
		{
			hashSet.Add(this.m_utilityItem.m_shared.m_equipStatusEffect);
		}
		if (this.HaveSetEffect(this.m_leftItem))
		{
			hashSet.Add(this.m_leftItem.m_shared.m_setStatusEffect);
		}
		if (this.HaveSetEffect(this.m_rightItem))
		{
			hashSet.Add(this.m_rightItem.m_shared.m_setStatusEffect);
		}
		if (this.HaveSetEffect(this.m_chestItem))
		{
			hashSet.Add(this.m_chestItem.m_shared.m_setStatusEffect);
		}
		if (this.HaveSetEffect(this.m_legItem))
		{
			hashSet.Add(this.m_legItem.m_shared.m_setStatusEffect);
		}
		if (this.HaveSetEffect(this.m_helmetItem))
		{
			hashSet.Add(this.m_helmetItem.m_shared.m_setStatusEffect);
		}
		if (this.HaveSetEffect(this.m_shoulderItem))
		{
			hashSet.Add(this.m_shoulderItem.m_shared.m_setStatusEffect);
		}
		if (this.HaveSetEffect(this.m_utilityItem))
		{
			hashSet.Add(this.m_utilityItem.m_shared.m_setStatusEffect);
		}
		foreach (StatusEffect statusEffect in this.m_eqipmentStatusEffects)
		{
			if (!hashSet.Contains(statusEffect))
			{
				this.m_seman.RemoveStatusEffect(statusEffect.name, false);
			}
		}
		foreach (StatusEffect statusEffect2 in hashSet)
		{
			if (!this.m_eqipmentStatusEffects.Contains(statusEffect2))
			{
				this.m_seman.AddStatusEffect(statusEffect2, false);
			}
		}
		this.m_eqipmentStatusEffects.Clear();
		this.m_eqipmentStatusEffects.UnionWith(hashSet);
	}

	// Token: 0x06000139 RID: 313 RVA: 0x00009D34 File Offset: 0x00007F34
	private bool HaveSetEffect(ItemDrop.ItemData item)
	{
		return item != null && !(item.m_shared.m_setStatusEffect == null) && item.m_shared.m_setName.Length != 0 && item.m_shared.m_setSize > 1 && this.GetSetCount(item.m_shared.m_setName) >= item.m_shared.m_setSize;
	}

	// Token: 0x0600013A RID: 314 RVA: 0x00009D9C File Offset: 0x00007F9C
	private int GetSetCount(string setName)
	{
		int num = 0;
		if (this.m_leftItem != null && this.m_leftItem.m_shared.m_setName == setName)
		{
			num++;
		}
		if (this.m_rightItem != null && this.m_rightItem.m_shared.m_setName == setName)
		{
			num++;
		}
		if (this.m_chestItem != null && this.m_chestItem.m_shared.m_setName == setName)
		{
			num++;
		}
		if (this.m_legItem != null && this.m_legItem.m_shared.m_setName == setName)
		{
			num++;
		}
		if (this.m_helmetItem != null && this.m_helmetItem.m_shared.m_setName == setName)
		{
			num++;
		}
		if (this.m_shoulderItem != null && this.m_shoulderItem.m_shared.m_setName == setName)
		{
			num++;
		}
		if (this.m_utilityItem != null && this.m_utilityItem.m_shared.m_setName == setName)
		{
			num++;
		}
		return num;
	}

	// Token: 0x0600013B RID: 315 RVA: 0x00009EA8 File Offset: 0x000080A8
	public void SetBeard(string name)
	{
		this.m_beardItem = name;
		this.SetupEquipment();
	}

	// Token: 0x0600013C RID: 316 RVA: 0x00009EB7 File Offset: 0x000080B7
	public string GetBeard()
	{
		return this.m_beardItem;
	}

	// Token: 0x0600013D RID: 317 RVA: 0x00009EBF File Offset: 0x000080BF
	public void SetHair(string hair)
	{
		this.m_hairItem = hair;
		this.SetupEquipment();
	}

	// Token: 0x0600013E RID: 318 RVA: 0x00009ECE File Offset: 0x000080CE
	public string GetHair()
	{
		return this.m_hairItem;
	}

	// Token: 0x0600013F RID: 319 RVA: 0x00009ED8 File Offset: 0x000080D8
	public bool IsItemEquiped(ItemDrop.ItemData item)
	{
		return this.m_rightItem == item || this.m_leftItem == item || this.m_chestItem == item || this.m_legItem == item || this.m_ammoItem == item || this.m_helmetItem == item || this.m_shoulderItem == item || this.m_utilityItem == item;
	}

	// Token: 0x06000140 RID: 320 RVA: 0x00009F3E File Offset: 0x0000813E
	public ItemDrop.ItemData GetRightItem()
	{
		return this.m_rightItem;
	}

	// Token: 0x06000141 RID: 321 RVA: 0x00009F46 File Offset: 0x00008146
	public ItemDrop.ItemData GetLeftItem()
	{
		return this.m_leftItem;
	}

	// Token: 0x06000142 RID: 322 RVA: 0x00009F4E File Offset: 0x0000814E
	protected override bool CheckRun(Vector3 moveDir, float dt)
	{
		return !this.IsHoldingAttack() && !this.IsBlocking() && base.CheckRun(moveDir, dt);
	}

	// Token: 0x06000143 RID: 323 RVA: 0x00009F6C File Offset: 0x0000816C
	public override bool IsHoldingAttack()
	{
		ItemDrop.ItemData currentWeapon = this.GetCurrentWeapon();
		return currentWeapon != null && currentWeapon.m_shared.m_holdDurationMin > 0f && this.m_attackDrawTime > 0f;
	}

	// Token: 0x06000144 RID: 324 RVA: 0x00009FA8 File Offset: 0x000081A8
	protected override bool BlockAttack(HitData hit, Character attacker)
	{
		if (Vector3.Dot(hit.m_dir, base.transform.forward) > 0f)
		{
			return false;
		}
		ItemDrop.ItemData currentBlocker = this.GetCurrentBlocker();
		if (currentBlocker == null)
		{
			return false;
		}
		bool flag = currentBlocker.m_shared.m_timedBlockBonus > 1f && this.m_blockTimer != -1f && this.m_blockTimer < 0.25f;
		float skillFactor = this.GetSkillFactor(Skills.SkillType.Blocking);
		float num = currentBlocker.GetBlockPower(skillFactor);
		if (flag)
		{
			num *= currentBlocker.m_shared.m_timedBlockBonus;
		}
		float totalBlockableDamage = hit.GetTotalBlockableDamage();
		float num2 = Mathf.Min(totalBlockableDamage, num);
		float num3 = Mathf.Clamp01(num2 / num);
		float stamina = this.m_blockStaminaDrain * num3;
		this.UseStamina(stamina);
		bool flag2 = this.HaveStamina(0f);
		bool flag3 = flag2 && num >= totalBlockableDamage;
		if (flag2)
		{
			hit.m_statusEffect = "";
			hit.BlockDamage(num2);
			DamageText.instance.ShowText(DamageText.TextType.Blocked, hit.m_point + Vector3.up * 0.5f, num2, false);
		}
		if (!flag2 || !flag3)
		{
			base.Stagger(hit.m_dir);
		}
		if (currentBlocker.m_shared.m_useDurability)
		{
			float num4 = currentBlocker.m_shared.m_useDurabilityDrain * num3;
			currentBlocker.m_durability -= num4;
		}
		this.RaiseSkill(Skills.SkillType.Blocking, flag ? 2f : 1f);
		currentBlocker.m_shared.m_blockEffect.Create(hit.m_point, Quaternion.identity, null, 1f);
		if (attacker && flag && flag3)
		{
			this.m_perfectBlockEffect.Create(hit.m_point, Quaternion.identity, null, 1f);
			if (attacker.m_staggerWhenBlocked)
			{
				attacker.Stagger(-hit.m_dir);
			}
		}
		if (flag3)
		{
			float num5 = Mathf.Clamp01(num3 * 0.5f);
			hit.m_pushForce *= num5;
			if (attacker && flag)
			{
				HitData hitData = new HitData();
				hitData.m_pushForce = currentBlocker.GetDeflectionForce() * (1f - num5);
				hitData.m_dir = attacker.transform.position - base.transform.position;
				hitData.m_dir.y = 0f;
				hitData.m_dir.Normalize();
				attacker.Damage(hitData);
			}
		}
		return true;
	}

	// Token: 0x06000145 RID: 325 RVA: 0x0000A20C File Offset: 0x0000840C
	public override bool IsBlocking()
	{
		if (this.m_nview.IsValid() && !this.m_nview.IsOwner())
		{
			return this.m_nview.GetZDO().GetBool(Humanoid.isBlockingHash, false);
		}
		return this.m_blocking && !this.InAttack() && !this.InDodge() && !this.InPlaceMode() && !this.IsEncumbered() && !this.InMinorAction();
	}

	// Token: 0x06000146 RID: 326 RVA: 0x0000A280 File Offset: 0x00008480
	private void UpdateBlock(float dt)
	{
		if (!this.IsBlocking())
		{
			if (this.m_internalBlockingState)
			{
				this.m_internalBlockingState = false;
				this.m_nview.GetZDO().Set(Humanoid.isBlockingHash, false);
				this.m_zanim.SetBool(Humanoid.blocking, false);
			}
			this.m_blockTimer = -1f;
			return;
		}
		if (!this.m_internalBlockingState)
		{
			this.m_internalBlockingState = true;
			this.m_nview.GetZDO().Set(Humanoid.isBlockingHash, true);
			this.m_zanim.SetBool(Humanoid.blocking, true);
		}
		if (this.m_blockTimer < 0f)
		{
			this.m_blockTimer = 0f;
			return;
		}
		this.m_blockTimer += dt;
	}

	// Token: 0x06000147 RID: 327 RVA: 0x0000A334 File Offset: 0x00008534
	public void HideHandItems()
	{
		if (this.m_leftItem == null && this.m_rightItem == null)
		{
			return;
		}
		ItemDrop.ItemData leftItem = this.m_leftItem;
		ItemDrop.ItemData rightItem = this.m_rightItem;
		this.UnequipItem(this.m_leftItem, true);
		this.UnequipItem(this.m_rightItem, true);
		this.m_hiddenLeftItem = leftItem;
		this.m_hiddenRightItem = rightItem;
		this.SetupVisEquipment(this.m_visEquipment, false);
		this.m_zanim.SetTrigger("equip_hip");
	}

	// Token: 0x06000148 RID: 328 RVA: 0x0000A3A8 File Offset: 0x000085A8
	public void ShowHandItems()
	{
		ItemDrop.ItemData hiddenLeftItem = this.m_hiddenLeftItem;
		ItemDrop.ItemData hiddenRightItem = this.m_hiddenRightItem;
		if (hiddenLeftItem == null && hiddenRightItem == null)
		{
			return;
		}
		this.m_hiddenLeftItem = null;
		this.m_hiddenRightItem = null;
		if (hiddenLeftItem != null)
		{
			this.EquipItem(hiddenLeftItem, true);
		}
		if (hiddenRightItem != null)
		{
			this.EquipItem(hiddenRightItem, true);
		}
		this.m_zanim.SetTrigger("equip_hip");
	}

	// Token: 0x06000149 RID: 329 RVA: 0x0000A400 File Offset: 0x00008600
	public ItemDrop.ItemData GetAmmoItem()
	{
		return this.m_ammoItem;
	}

	// Token: 0x0600014A RID: 330 RVA: 0x000058A9 File Offset: 0x00003AA9
	public virtual GameObject GetHoverObject()
	{
		return null;
	}

	// Token: 0x0600014B RID: 331 RVA: 0x0000A408 File Offset: 0x00008608
	public bool IsTeleportable()
	{
		return this.m_inventory.IsTeleportable();
	}

	// Token: 0x0600014C RID: 332 RVA: 0x0000A418 File Offset: 0x00008618
	public override bool UseMeleeCamera()
	{
		ItemDrop.ItemData currentWeapon = this.GetCurrentWeapon();
		return currentWeapon != null && currentWeapon.m_shared.m_centerCamera;
	}

	// Token: 0x0600014D RID: 333 RVA: 0x0000A43C File Offset: 0x0000863C
	public float GetEquipmentWeight()
	{
		float num = 0f;
		if (this.m_rightItem != null)
		{
			num += this.m_rightItem.m_shared.m_weight;
		}
		if (this.m_leftItem != null)
		{
			num += this.m_leftItem.m_shared.m_weight;
		}
		if (this.m_chestItem != null)
		{
			num += this.m_chestItem.m_shared.m_weight;
		}
		if (this.m_legItem != null)
		{
			num += this.m_legItem.m_shared.m_weight;
		}
		if (this.m_helmetItem != null)
		{
			num += this.m_helmetItem.m_shared.m_weight;
		}
		if (this.m_shoulderItem != null)
		{
			num += this.m_shoulderItem.m_shared.m_weight;
		}
		if (this.m_utilityItem != null)
		{
			num += this.m_utilityItem.m_shared.m_weight;
		}
		return num;
	}

	// Token: 0x040000E4 RID: 228
	private static List<ItemDrop.ItemData> optimalWeapons = new List<ItemDrop.ItemData>();

	// Token: 0x040000E5 RID: 229
	private static List<ItemDrop.ItemData> outofRangeWeapons = new List<ItemDrop.ItemData>();

	// Token: 0x040000E6 RID: 230
	private static List<ItemDrop.ItemData> allWeapons = new List<ItemDrop.ItemData>();

	// Token: 0x040000E7 RID: 231
	[Header("Humanoid")]
	public float m_equipStaminaDrain = 10f;

	// Token: 0x040000E8 RID: 232
	public float m_blockStaminaDrain = 25f;

	// Token: 0x040000E9 RID: 233
	[Header("Default items")]
	public GameObject[] m_defaultItems;

	// Token: 0x040000EA RID: 234
	public GameObject[] m_randomWeapon;

	// Token: 0x040000EB RID: 235
	public GameObject[] m_randomArmor;

	// Token: 0x040000EC RID: 236
	public GameObject[] m_randomShield;

	// Token: 0x040000ED RID: 237
	public Humanoid.ItemSet[] m_randomSets;

	// Token: 0x040000EE RID: 238
	public ItemDrop m_unarmedWeapon;

	// Token: 0x040000EF RID: 239
	[Header("Effects")]
	public EffectList m_pickupEffects = new EffectList();

	// Token: 0x040000F0 RID: 240
	public EffectList m_dropEffects = new EffectList();

	// Token: 0x040000F1 RID: 241
	public EffectList m_consumeItemEffects = new EffectList();

	// Token: 0x040000F2 RID: 242
	public EffectList m_equipEffects = new EffectList();

	// Token: 0x040000F3 RID: 243
	public EffectList m_perfectBlockEffect = new EffectList();

	// Token: 0x040000F4 RID: 244
	protected Inventory m_inventory = new Inventory("Inventory", null, 8, 4);

	// Token: 0x040000F5 RID: 245
	protected ItemDrop.ItemData m_rightItem;

	// Token: 0x040000F6 RID: 246
	protected ItemDrop.ItemData m_leftItem;

	// Token: 0x040000F7 RID: 247
	protected ItemDrop.ItemData m_chestItem;

	// Token: 0x040000F8 RID: 248
	protected ItemDrop.ItemData m_legItem;

	// Token: 0x040000F9 RID: 249
	protected ItemDrop.ItemData m_ammoItem;

	// Token: 0x040000FA RID: 250
	protected ItemDrop.ItemData m_helmetItem;

	// Token: 0x040000FB RID: 251
	protected ItemDrop.ItemData m_shoulderItem;

	// Token: 0x040000FC RID: 252
	protected ItemDrop.ItemData m_utilityItem;

	// Token: 0x040000FD RID: 253
	protected string m_beardItem = "";

	// Token: 0x040000FE RID: 254
	protected string m_hairItem = "";

	// Token: 0x040000FF RID: 255
	private int m_lastEquipEffectFrame;

	// Token: 0x04000100 RID: 256
	protected ItemDrop.ItemData m_hiddenLeftItem;

	// Token: 0x04000101 RID: 257
	protected ItemDrop.ItemData m_hiddenRightItem;

	// Token: 0x04000102 RID: 258
	protected Attack m_currentAttack;

	// Token: 0x04000103 RID: 259
	protected Attack m_previousAttack;

	// Token: 0x04000104 RID: 260
	private float m_timeSinceLastAttack;

	// Token: 0x04000105 RID: 261
	private bool m_internalBlockingState;

	// Token: 0x04000106 RID: 262
	private float m_blockTimer = 9999f;

	// Token: 0x04000107 RID: 263
	private const float m_perfectBlockInterval = 0.25f;

	// Token: 0x04000108 RID: 264
	protected float m_attackDrawTime;

	// Token: 0x04000109 RID: 265
	protected float m_lastCombatTimer = 999f;

	// Token: 0x0400010A RID: 266
	protected VisEquipment m_visEquipment;

	// Token: 0x0400010B RID: 267
	private static int statef = 0;

	// Token: 0x0400010C RID: 268
	private static int statei = 0;

	// Token: 0x0400010D RID: 269
	private static int blocking = 0;

	// Token: 0x0400010E RID: 270
	private static int isBlockingHash = 0;

	// Token: 0x0400010F RID: 271
	private HashSet<StatusEffect> m_eqipmentStatusEffects = new HashSet<StatusEffect>();

	// Token: 0x04000110 RID: 272
	protected static int m_animatorTagAttack = Animator.StringToHash("attack");

	// Token: 0x02000122 RID: 290
	[Serializable]
	public class ItemSet
	{
		// Token: 0x04000FD8 RID: 4056
		public string m_name = "";

		// Token: 0x04000FD9 RID: 4057
		public GameObject[] m_items = new GameObject[0];
	}
}
