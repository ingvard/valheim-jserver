using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000021 RID: 33
public class MonsterAI : BaseAI
{
	// Token: 0x06000365 RID: 869 RVA: 0x0001CF48 File Offset: 0x0001B148
	protected override void Awake()
	{
		base.Awake();
		this.m_tamable = base.GetComponent<Tameable>();
		this.m_despawnInDay = this.m_nview.GetZDO().GetBool("DespawnInDay", this.m_despawnInDay);
		this.m_eventCreature = this.m_nview.GetZDO().GetBool("EventCreature", this.m_eventCreature);
		this.m_animator.SetBool("sleeping", this.IsSleeping());
		this.m_interceptTime = UnityEngine.Random.Range(this.m_interceptTimeMin, this.m_interceptTimeMax);
		this.m_pauseTimer = UnityEngine.Random.Range(0f, this.m_circleTargetInterval);
		this.m_updateTargetTimer = UnityEngine.Random.Range(0f, 3f);
		if (this.m_enableHuntPlayer)
		{
			base.SetHuntPlayer(true);
		}
	}

	// Token: 0x06000366 RID: 870 RVA: 0x0001D010 File Offset: 0x0001B210
	private void Start()
	{
		if (this.m_nview && this.m_nview.IsValid() && this.m_nview.IsOwner())
		{
			Humanoid humanoid = this.m_character as Humanoid;
			if (humanoid)
			{
				humanoid.EquipBestWeapon(null, null, null, null);
			}
		}
	}

	// Token: 0x06000367 RID: 871 RVA: 0x0001D062 File Offset: 0x0001B262
	protected override void OnDamaged(float damage, Character attacker)
	{
		base.OnDamaged(damage, attacker);
		this.SetAlerted(true);
		this.SetTarget(attacker);
	}

	// Token: 0x06000368 RID: 872 RVA: 0x0001D07C File Offset: 0x0001B27C
	private void SetTarget(Character attacker)
	{
		if (attacker != null && this.m_targetCreature == null)
		{
			if (attacker.IsPlayer() && this.m_character.IsTamed())
			{
				return;
			}
			this.m_targetCreature = attacker;
			this.m_lastKnownTargetPos = attacker.transform.position;
			this.m_beenAtLastPos = false;
			this.m_havePathToTarget = base.HavePath(this.m_targetCreature.transform.position);
			this.m_targetStatic = null;
		}
	}

	// Token: 0x06000369 RID: 873 RVA: 0x0001D0F8 File Offset: 0x0001B2F8
	protected override void RPC_OnNearProjectileHit(long sender, Vector3 center, float range, ZDOID attackerID)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		this.SetAlerted(true);
		if (this.m_fleeIfNotAlerted)
		{
			return;
		}
		GameObject gameObject = ZNetScene.instance.FindInstance(attackerID);
		if (gameObject != null)
		{
			Character component = gameObject.GetComponent<Character>();
			if (component)
			{
				this.SetTarget(component);
			}
		}
	}

	// Token: 0x0600036A RID: 874 RVA: 0x0001D14F File Offset: 0x0001B34F
	public void MakeTame()
	{
		this.m_character.SetTamed(true);
		this.SetAlerted(false);
		this.m_targetCreature = null;
		this.m_targetStatic = null;
	}

	// Token: 0x0600036B RID: 875 RVA: 0x0001D174 File Offset: 0x0001B374
	private void UpdateTarget(Humanoid humanoid, float dt, out bool canHearTarget, out bool canSeeTarget)
	{
		this.m_updateTargetTimer -= dt;
		if (this.m_updateTargetTimer <= 0f && !this.m_character.InAttack())
		{
			this.m_updateTargetTimer = (Character.IsCharacterInRange(base.transform.position, 32f) ? 3f : 10f);
			Character character = base.FindEnemy();
			if (character)
			{
				this.m_targetCreature = character;
				this.m_targetStatic = null;
			}
			if (this.m_targetCreature != null)
			{
				this.m_havePathToTarget = base.HavePath(this.m_targetCreature.transform.position);
			}
			if (!this.m_character.IsTamed() && (this.m_attackPlayerObjects || (this.m_attackPlayerObjectsWhenAlerted && base.IsAlerted())) && (this.m_targetCreature == null || (this.m_targetCreature && !this.m_havePathToTarget)))
			{
				StaticTarget staticTarget = base.FindClosestStaticPriorityTarget(99999f);
				if (staticTarget)
				{
					this.m_targetStatic = staticTarget;
					this.m_targetCreature = null;
				}
				if (this.m_targetStatic != null)
				{
					this.m_havePathToTarget = base.HavePath(this.m_targetStatic.transform.position);
				}
				if ((!staticTarget || (this.m_targetStatic && !this.m_havePathToTarget)) && base.IsAlerted())
				{
					StaticTarget staticTarget2 = base.FindRandomStaticTarget(10f, false);
					if (staticTarget2)
					{
						this.m_targetStatic = staticTarget2;
						this.m_targetCreature = null;
					}
				}
			}
		}
		if (this.m_targetCreature && this.m_character.IsTamed())
		{
			Vector3 b;
			if (base.GetPatrolPoint(out b))
			{
				if (Vector3.Distance(this.m_targetCreature.transform.position, b) > this.m_alertRange)
				{
					this.m_targetCreature = null;
				}
			}
			else if (this.m_follow && Vector3.Distance(this.m_targetCreature.transform.position, this.m_follow.transform.position) > this.m_alertRange)
			{
				this.m_targetCreature = null;
			}
		}
		if (this.m_targetCreature && this.m_targetCreature.IsDead())
		{
			this.m_targetCreature = null;
		}
		canHearTarget = false;
		canSeeTarget = false;
		if (this.m_targetCreature)
		{
			canHearTarget = base.CanHearTarget(this.m_targetCreature);
			canSeeTarget = base.CanSeeTarget(this.m_targetCreature);
			if (canSeeTarget | canHearTarget)
			{
				this.m_timeSinceSensedTargetCreature = 0f;
			}
			if (this.m_targetCreature.IsPlayer())
			{
				this.m_targetCreature.OnTargeted(canSeeTarget | canHearTarget, base.IsAlerted());
			}
			base.SetTargetInfo(this.m_targetCreature.GetZDOID());
		}
		else
		{
			base.SetTargetInfo(ZDOID.None);
		}
		this.m_timeSinceSensedTargetCreature += dt;
		if (base.IsAlerted() || this.m_targetCreature != null)
		{
			this.m_timeSinceAttacking += dt;
			float num = this.m_character.IsBoss() ? 15f : 15f;
			float num2 = num * 2f;
			float num3 = Vector3.Distance(this.m_spawnPoint, base.transform.position);
			bool flag = this.HuntPlayer() && this.m_targetCreature && this.m_targetCreature.IsPlayer();
			if (this.m_timeSinceSensedTargetCreature > num || (!flag && (this.m_timeSinceAttacking > num2 || (this.m_maxChaseDistance > 0f && this.m_timeSinceSensedTargetCreature > 1f && num3 > this.m_maxChaseDistance))))
			{
				this.SetAlerted(false);
				this.m_targetCreature = null;
				this.m_targetStatic = null;
				this.m_timeSinceAttacking = 0f;
				this.m_updateTargetTimer = 5f;
			}
		}
	}

	// Token: 0x0600036C RID: 876 RVA: 0x0001D540 File Offset: 0x0001B740
	protected override void UpdateAI(float dt)
	{
		base.UpdateAI(dt);
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.IsSleeping())
		{
			this.UpdateSleep(dt);
			return;
		}
		this.m_aiStatus = "";
		Humanoid humanoid = this.m_character as Humanoid;
		bool flag;
		bool flag2;
		this.UpdateTarget(humanoid, dt, out flag, out flag2);
		if (this.m_avoidLand && !this.m_character.IsSwiming())
		{
			this.m_aiStatus = "Move to water";
			base.MoveToWater(dt, 20f);
			return;
		}
		if (((this.DespawnInDay() && EnvMan.instance.IsDay()) || (this.IsEventCreature() && !RandEventSystem.HaveActiveEvent())) && (this.m_targetCreature == null || !flag2))
		{
			base.MoveAwayAndDespawn(dt, true);
			this.m_aiStatus = "Trying to despawn ";
			return;
		}
		if (this.m_fleeIfNotAlerted && !this.HuntPlayer() && this.m_targetCreature && !base.IsAlerted() && Vector3.Distance(this.m_targetCreature.transform.position, base.transform.position) - this.m_targetCreature.GetRadius() > this.m_alertRange)
		{
			base.Flee(dt, this.m_targetCreature.transform.position);
			this.m_aiStatus = "Avoiding conflict";
			return;
		}
		if (this.m_fleeIfLowHealth > 0f && this.m_character.GetHealthPercentage() < this.m_fleeIfLowHealth && this.m_timeSinceHurt < 20f && this.m_targetCreature != null)
		{
			base.Flee(dt, this.m_targetCreature.transform.position);
			this.m_aiStatus = "Low health, flee";
			return;
		}
		if ((this.m_afraidOfFire || this.m_avoidFire) && base.AvoidFire(dt, this.m_targetCreature, this.m_afraidOfFire))
		{
			if (this.m_afraidOfFire)
			{
				this.m_targetStatic = null;
				this.m_targetCreature = null;
			}
			this.m_aiStatus = "Avoiding fire";
			return;
		}
		if (this.m_circleTargetInterval > 0f && this.m_targetCreature)
		{
			if (this.m_targetCreature)
			{
				this.m_pauseTimer += dt;
				if (this.m_pauseTimer > this.m_circleTargetInterval)
				{
					if (this.m_pauseTimer > this.m_circleTargetInterval + this.m_circleTargetDuration)
					{
						this.m_pauseTimer = 0f;
					}
					base.RandomMovementArroundPoint(dt, this.m_targetCreature.transform.position, this.m_circleTargetDistance, base.IsAlerted());
					this.m_aiStatus = "Attack pause";
					return;
				}
			}
			else
			{
				this.m_pauseTimer = 0f;
			}
		}
		if (this.m_targetCreature != null)
		{
			if (EffectArea.IsPointInsideArea(this.m_targetCreature.transform.position, EffectArea.Type.NoMonsters, 0f))
			{
				base.Flee(dt, this.m_targetCreature.transform.position);
				this.m_aiStatus = "Avoid no-monster area";
				return;
			}
		}
		else
		{
			EffectArea effectArea = EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.NoMonsters, 15f);
			if (effectArea != null)
			{
				base.Flee(dt, effectArea.transform.position);
				this.m_aiStatus = "Avoid no-monster area";
				return;
			}
		}
		if (this.m_fleeIfHurtWhenTargetCantBeReached && this.m_targetCreature != null && !this.m_havePathToTarget && this.m_timeSinceHurt < 20f)
		{
			this.m_aiStatus = "Hide from unreachable target";
			base.Flee(dt, this.m_targetCreature.transform.position);
			return;
		}
		if ((!base.IsAlerted() || (this.m_targetStatic == null && this.m_targetCreature == null)) && this.UpdateConsumeItem(humanoid, dt))
		{
			this.m_aiStatus = "Consume item";
			return;
		}
		ItemDrop.ItemData itemData = this.SelectBestAttack(humanoid, dt);
		bool flag3 = itemData != null && Time.time - itemData.m_lastAttackTime > itemData.m_shared.m_aiAttackInterval && Time.time - this.m_lastAttackTime > this.m_minAttackInterval && !base.IsTakingOff();
		if ((this.m_character.IsFlying() ? this.m_circulateWhileChargingFlying : this.m_circulateWhileCharging) && (this.m_targetStatic != null || this.m_targetCreature != null) && itemData != null && !flag3 && !this.m_character.InAttack())
		{
			this.m_aiStatus = "Move around target weapon ready:" + flag3.ToString();
			if (itemData != null)
			{
				this.m_aiStatus = this.m_aiStatus + " Weapon:" + itemData.m_shared.m_name;
			}
			Vector3 point = this.m_targetCreature ? this.m_targetCreature.transform.position : this.m_targetStatic.transform.position;
			base.RandomMovementArroundPoint(dt, point, this.m_randomMoveRange, base.IsAlerted());
			return;
		}
		if ((!(this.m_targetStatic == null) || !(this.m_targetCreature == null)) && itemData != null)
		{
			if (itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.Enemy)
			{
				if (this.m_targetStatic)
				{
					Vector3 vector = this.m_targetStatic.FindClosestPoint(base.transform.position);
					if (Vector3.Distance(vector, base.transform.position) >= itemData.m_shared.m_aiAttackRange || !base.CanSeeTarget(this.m_targetStatic))
					{
						this.m_aiStatus = "Move to static target";
						base.MoveTo(dt, vector, 0f, base.IsAlerted());
						return;
					}
					base.LookAt(this.m_targetStatic.GetCenter());
					if (base.IsLookingAt(this.m_targetStatic.GetCenter(), itemData.m_shared.m_aiAttackMaxAngle) && flag3)
					{
						this.m_aiStatus = "Attacking piece";
						this.DoAttack(null, false);
						return;
					}
					base.StopMoving();
					return;
				}
				else if (this.m_targetCreature)
				{
					if (flag || flag2 || (this.HuntPlayer() && this.m_targetCreature.IsPlayer()))
					{
						this.m_beenAtLastPos = false;
						this.m_lastKnownTargetPos = this.m_targetCreature.transform.position;
						float num = Vector3.Distance(this.m_lastKnownTargetPos, base.transform.position) - this.m_targetCreature.GetRadius();
						float num2 = this.m_alertRange * this.m_targetCreature.GetStealthFactor();
						if ((flag2 && num < num2) || this.HuntPlayer())
						{
							this.SetAlerted(true);
						}
						bool flag4 = num < itemData.m_shared.m_aiAttackRange;
						if (!flag4 || !flag2 || itemData.m_shared.m_aiAttackRangeMin < 0f || !base.IsAlerted())
						{
							this.m_aiStatus = "Move closer";
							Vector3 velocity = this.m_targetCreature.GetVelocity();
							Vector3 vector2 = velocity * this.m_interceptTime;
							Vector3 vector3 = this.m_lastKnownTargetPos;
							if (num > vector2.magnitude / 4f)
							{
								vector3 += velocity * this.m_interceptTime;
							}
							if (base.MoveTo(dt, vector3, 0f, base.IsAlerted()))
							{
								flag4 = true;
							}
						}
						else
						{
							base.StopMoving();
						}
						if (flag4 && flag2 && base.IsAlerted())
						{
							this.m_aiStatus = "In attack range";
							base.LookAt(this.m_targetCreature.GetTopPoint());
							if (flag3 && base.IsLookingAt(this.m_lastKnownTargetPos, itemData.m_shared.m_aiAttackMaxAngle))
							{
								this.m_aiStatus = "Attacking creature";
								this.DoAttack(this.m_targetCreature, false);
								return;
							}
						}
					}
					else
					{
						this.m_aiStatus = "Searching for target";
						if (this.m_beenAtLastPos)
						{
							base.RandomMovement(dt, this.m_lastKnownTargetPos);
							return;
						}
						if (base.MoveTo(dt, this.m_lastKnownTargetPos, 0f, base.IsAlerted()))
						{
							this.m_beenAtLastPos = true;
							return;
						}
					}
				}
			}
			else if (itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.FriendHurt || itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.Friend)
			{
				this.m_aiStatus = "Helping friend";
				Character character = (itemData.m_shared.m_aiTargetType == ItemDrop.ItemData.AiTarget.FriendHurt) ? base.HaveHurtFriendInRange(this.m_viewRange) : base.HaveFriendInRange(this.m_viewRange);
				if (character)
				{
					if (Vector3.Distance(character.transform.position, base.transform.position) >= itemData.m_shared.m_aiAttackRange)
					{
						base.MoveTo(dt, character.transform.position, 0f, base.IsAlerted());
						return;
					}
					if (flag3)
					{
						base.StopMoving();
						base.LookAt(character.transform.position);
						this.DoAttack(character, true);
						return;
					}
					base.RandomMovement(dt, character.transform.position);
					return;
				}
				else
				{
					base.RandomMovement(dt, base.transform.position);
				}
			}
			return;
		}
		if (this.m_follow)
		{
			base.Follow(this.m_follow, dt);
			this.m_aiStatus = "Follow";
			return;
		}
		this.m_aiStatus = string.Concat(new object[]
		{
			"Random movement (weapon: ",
			(itemData != null) ? itemData.m_shared.m_name : "none",
			") (targetpiece: ",
			this.m_targetStatic,
			") (target: ",
			this.m_targetCreature ? this.m_targetCreature.gameObject.name : "none",
			")"
		});
		base.IdleMovement(dt);
	}

	// Token: 0x0600036D RID: 877 RVA: 0x0001DEA4 File Offset: 0x0001C0A4
	private bool UpdateConsumeItem(Humanoid humanoid, float dt)
	{
		if (this.m_consumeItems == null || this.m_consumeItems.Count == 0)
		{
			return false;
		}
		this.m_consumeSearchTimer += dt;
		if (this.m_consumeSearchTimer > this.m_consumeSearchInterval)
		{
			this.m_consumeSearchTimer = 0f;
			if (this.m_tamable && !this.m_tamable.IsHungry())
			{
				return false;
			}
			this.m_consumeTarget = this.FindClosestConsumableItem(this.m_consumeSearchRange);
		}
		if (this.m_consumeTarget)
		{
			if (base.MoveTo(dt, this.m_consumeTarget.transform.position, this.m_consumeRange, false))
			{
				base.LookAt(this.m_consumeTarget.transform.position);
				if (base.IsLookingAt(this.m_consumeTarget.transform.position, 20f) && this.m_consumeTarget.RemoveOne())
				{
					if (this.m_onConsumedItem != null)
					{
						this.m_onConsumedItem(this.m_consumeTarget);
					}
					humanoid.m_consumeItemEffects.Create(base.transform.position, Quaternion.identity, null, 1f);
					this.m_animator.SetTrigger("consume");
					this.m_consumeTarget = null;
					if (this.m_consumeHeal > 0f)
					{
						this.m_character.Heal(this.m_consumeHeal, true);
					}
				}
			}
			return true;
		}
		return false;
	}

	// Token: 0x0600036E RID: 878 RVA: 0x0001E004 File Offset: 0x0001C204
	private ItemDrop FindClosestConsumableItem(float maxRange)
	{
		if (MonsterAI.m_itemMask == 0)
		{
			MonsterAI.m_itemMask = LayerMask.GetMask(new string[]
			{
				"item"
			});
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, maxRange, MonsterAI.m_itemMask);
		ItemDrop itemDrop = null;
		float num = 999999f;
		foreach (Collider collider in array)
		{
			if (collider.attachedRigidbody)
			{
				ItemDrop component = collider.attachedRigidbody.GetComponent<ItemDrop>();
				if (!(component == null) && component.GetComponent<ZNetView>().IsValid() && this.CanConsume(component.m_itemData))
				{
					float num2 = Vector3.Distance(component.transform.position, base.transform.position);
					if (itemDrop == null || num2 < num)
					{
						itemDrop = component;
						num = num2;
					}
				}
			}
		}
		if (itemDrop && base.HavePath(itemDrop.transform.position))
		{
			return itemDrop;
		}
		return null;
	}

	// Token: 0x0600036F RID: 879 RVA: 0x0001E0F8 File Offset: 0x0001C2F8
	private bool CanConsume(ItemDrop.ItemData item)
	{
		using (List<ItemDrop>.Enumerator enumerator = this.m_consumeItems.GetEnumerator())
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

	// Token: 0x06000370 RID: 880 RVA: 0x0001E16C File Offset: 0x0001C36C
	private ItemDrop.ItemData SelectBestAttack(Humanoid humanoid, float dt)
	{
		if (this.m_targetCreature || this.m_targetStatic)
		{
			this.m_updateWeaponTimer -= dt;
			if (this.m_updateWeaponTimer <= 0f && !this.m_character.InAttack())
			{
				this.m_updateWeaponTimer = 1f;
				Character hurtFriend;
				Character friend;
				base.HaveFriendsInRange(this.m_viewRange, out hurtFriend, out friend);
				humanoid.EquipBestWeapon(this.m_targetCreature, this.m_targetStatic, hurtFriend, friend);
			}
		}
		return humanoid.GetCurrentWeapon();
	}

	// Token: 0x06000371 RID: 881 RVA: 0x0001E1F0 File Offset: 0x0001C3F0
	private bool DoAttack(Character target, bool isFriend)
	{
		ItemDrop.ItemData currentWeapon = (this.m_character as Humanoid).GetCurrentWeapon();
		if (currentWeapon == null)
		{
			return false;
		}
		if (!BaseAI.CanUseAttack(this.m_character, currentWeapon))
		{
			return false;
		}
		bool flag = this.m_character.StartAttack(target, false);
		if (flag)
		{
			this.m_timeSinceAttacking = 0f;
			this.m_lastAttackTime = Time.time;
		}
		return flag;
	}

	// Token: 0x06000372 RID: 882 RVA: 0x0001E249 File Offset: 0x0001C449
	public void SetDespawnInDay(bool despawn)
	{
		this.m_despawnInDay = despawn;
		this.m_nview.GetZDO().Set("DespawnInDay", despawn);
	}

	// Token: 0x06000373 RID: 883 RVA: 0x0001E268 File Offset: 0x0001C468
	public bool DespawnInDay()
	{
		if (Time.time - this.m_lastDespawnInDayCheck > 4f)
		{
			this.m_lastDespawnInDayCheck = Time.time;
			this.m_despawnInDay = this.m_nview.GetZDO().GetBool("DespawnInDay", this.m_despawnInDay);
		}
		return this.m_despawnInDay;
	}

	// Token: 0x06000374 RID: 884 RVA: 0x0001E2BA File Offset: 0x0001C4BA
	public void SetEventCreature(bool despawn)
	{
		this.m_eventCreature = despawn;
		this.m_nview.GetZDO().Set("EventCreature", despawn);
	}

	// Token: 0x06000375 RID: 885 RVA: 0x0001E2DC File Offset: 0x0001C4DC
	public bool IsEventCreature()
	{
		if (Time.time - this.m_lastEventCreatureCheck > 4f)
		{
			this.m_lastEventCreatureCheck = Time.time;
			this.m_eventCreature = this.m_nview.GetZDO().GetBool("EventCreature", this.m_eventCreature);
		}
		return this.m_eventCreature;
	}

	// Token: 0x06000376 RID: 886 RVA: 0x0001E32E File Offset: 0x0001C52E
	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
	}

	// Token: 0x06000377 RID: 887 RVA: 0x0001E336 File Offset: 0x0001C536
	public override Character GetTargetCreature()
	{
		return this.m_targetCreature;
	}

	// Token: 0x06000378 RID: 888 RVA: 0x0001E340 File Offset: 0x0001C540
	private void UpdateSleep(float dt)
	{
		if (!this.IsSleeping())
		{
			return;
		}
		this.m_sleepTimer += dt;
		if (this.m_sleepTimer < 0.5f)
		{
			return;
		}
		if (this.HuntPlayer())
		{
			this.Wakeup();
			return;
		}
		if (this.m_wakeupRange > 0f)
		{
			Player closestPlayer = Player.GetClosestPlayer(base.transform.position, this.m_wakeupRange);
			if (closestPlayer && !closestPlayer.InGhostMode() && !closestPlayer.IsDebugFlying())
			{
				this.Wakeup();
				return;
			}
		}
		if (this.m_noiseWakeup)
		{
			Player playerNoiseRange = Player.GetPlayerNoiseRange(base.transform.position, this.m_noiseRangeScale);
			if (playerNoiseRange && !playerNoiseRange.InGhostMode() && !playerNoiseRange.IsDebugFlying())
			{
				this.Wakeup();
				return;
			}
		}
	}

	// Token: 0x06000379 RID: 889 RVA: 0x0001E404 File Offset: 0x0001C604
	private void Wakeup()
	{
		if (!this.IsSleeping())
		{
			return;
		}
		this.m_animator.SetBool("sleeping", false);
		this.m_nview.GetZDO().Set("sleeping", false);
		this.m_wakeupEffects.Create(base.transform.position, base.transform.rotation, null, 1f);
	}

	// Token: 0x0600037A RID: 890 RVA: 0x0001E469 File Offset: 0x0001C669
	public override bool IsSleeping()
	{
		return this.m_nview.IsValid() && this.m_nview.GetZDO().GetBool("sleeping", this.m_sleeping);
	}

	// Token: 0x0600037B RID: 891 RVA: 0x0001E495 File Offset: 0x0001C695
	protected override void SetAlerted(bool alert)
	{
		if (alert)
		{
			this.m_timeSinceSensedTargetCreature = 0f;
		}
		base.SetAlerted(alert);
	}

	// Token: 0x0600037C RID: 892 RVA: 0x0001E4AC File Offset: 0x0001C6AC
	public override bool HuntPlayer()
	{
		return base.HuntPlayer() && (!this.IsEventCreature() || RandEventSystem.InEvent()) && (!this.DespawnInDay() || !EnvMan.instance.IsDay());
	}

	// Token: 0x0600037D RID: 893 RVA: 0x0001E4E0 File Offset: 0x0001C6E0
	public GameObject GetFollowTarget()
	{
		return this.m_follow;
	}

	// Token: 0x0600037E RID: 894 RVA: 0x0001E4E8 File Offset: 0x0001C6E8
	public void SetFollowTarget(GameObject go)
	{
		this.m_follow = go;
	}

	// Token: 0x0400033F RID: 831
	private float m_lastDespawnInDayCheck = -9999f;

	// Token: 0x04000340 RID: 832
	private float m_lastEventCreatureCheck = -9999f;

	// Token: 0x04000341 RID: 833
	public Action<ItemDrop> m_onConsumedItem;

	// Token: 0x04000342 RID: 834
	private const float m_giveUpTime = 15f;

	// Token: 0x04000343 RID: 835
	private const float m_bossGiveUpTime = 15f;

	// Token: 0x04000344 RID: 836
	private const float m_updateTargetFarRange = 32f;

	// Token: 0x04000345 RID: 837
	private const float m_updateTargetIntervalNear = 3f;

	// Token: 0x04000346 RID: 838
	private const float m_updateTargetIntervalFar = 10f;

	// Token: 0x04000347 RID: 839
	private const float m_updateWeaponInterval = 1f;

	// Token: 0x04000348 RID: 840
	[Header("Monster AI")]
	public float m_alertRange = 9999f;

	// Token: 0x04000349 RID: 841
	private const float m_alertOthersRange = 10f;

	// Token: 0x0400034A RID: 842
	public bool m_fleeIfHurtWhenTargetCantBeReached = true;

	// Token: 0x0400034B RID: 843
	public bool m_fleeIfNotAlerted;

	// Token: 0x0400034C RID: 844
	public float m_fleeIfLowHealth;

	// Token: 0x0400034D RID: 845
	public bool m_circulateWhileCharging;

	// Token: 0x0400034E RID: 846
	public bool m_circulateWhileChargingFlying;

	// Token: 0x0400034F RID: 847
	public bool m_enableHuntPlayer;

	// Token: 0x04000350 RID: 848
	public bool m_attackPlayerObjects = true;

	// Token: 0x04000351 RID: 849
	public bool m_attackPlayerObjectsWhenAlerted = true;

	// Token: 0x04000352 RID: 850
	public float m_interceptTimeMax;

	// Token: 0x04000353 RID: 851
	public float m_interceptTimeMin;

	// Token: 0x04000354 RID: 852
	public float m_maxChaseDistance;

	// Token: 0x04000355 RID: 853
	public float m_minAttackInterval;

	// Token: 0x04000356 RID: 854
	[Header("Circle target")]
	public float m_circleTargetInterval;

	// Token: 0x04000357 RID: 855
	public float m_circleTargetDuration = 5f;

	// Token: 0x04000358 RID: 856
	public float m_circleTargetDistance = 10f;

	// Token: 0x04000359 RID: 857
	[Header("Sleep")]
	public bool m_sleeping;

	// Token: 0x0400035A RID: 858
	public bool m_noiseWakeup;

	// Token: 0x0400035B RID: 859
	public float m_noiseRangeScale = 1f;

	// Token: 0x0400035C RID: 860
	public float m_wakeupRange = 5f;

	// Token: 0x0400035D RID: 861
	public EffectList m_wakeupEffects = new EffectList();

	// Token: 0x0400035E RID: 862
	[Header("Other")]
	public bool m_avoidLand;

	// Token: 0x0400035F RID: 863
	[Header("Consume items")]
	public List<ItemDrop> m_consumeItems;

	// Token: 0x04000360 RID: 864
	public float m_consumeRange = 2f;

	// Token: 0x04000361 RID: 865
	public float m_consumeSearchRange = 5f;

	// Token: 0x04000362 RID: 866
	public float m_consumeSearchInterval = 10f;

	// Token: 0x04000363 RID: 867
	public float m_consumeHeal;

	// Token: 0x04000364 RID: 868
	private ItemDrop m_consumeTarget;

	// Token: 0x04000365 RID: 869
	private float m_consumeSearchTimer;

	// Token: 0x04000366 RID: 870
	private static int m_itemMask;

	// Token: 0x04000367 RID: 871
	private string m_aiStatus = "";

	// Token: 0x04000368 RID: 872
	private bool m_despawnInDay;

	// Token: 0x04000369 RID: 873
	private bool m_eventCreature;

	// Token: 0x0400036A RID: 874
	private Character m_targetCreature;

	// Token: 0x0400036B RID: 875
	private bool m_havePathToTarget;

	// Token: 0x0400036C RID: 876
	private Vector3 m_lastKnownTargetPos = Vector3.zero;

	// Token: 0x0400036D RID: 877
	private bool m_beenAtLastPos;

	// Token: 0x0400036E RID: 878
	private StaticTarget m_targetStatic;

	// Token: 0x0400036F RID: 879
	private float m_timeSinceAttacking;

	// Token: 0x04000370 RID: 880
	private float m_timeSinceSensedTargetCreature;

	// Token: 0x04000371 RID: 881
	private float m_updateTargetTimer;

	// Token: 0x04000372 RID: 882
	private float m_updateWeaponTimer;

	// Token: 0x04000373 RID: 883
	private float m_lastAttackTime = -1000f;

	// Token: 0x04000374 RID: 884
	private float m_interceptTime;

	// Token: 0x04000375 RID: 885
	private float m_pauseTimer;

	// Token: 0x04000376 RID: 886
	private bool m_goingHome;

	// Token: 0x04000377 RID: 887
	private float m_sleepTimer;

	// Token: 0x04000378 RID: 888
	private GameObject m_follow;

	// Token: 0x04000379 RID: 889
	private Tameable m_tamable;
}
