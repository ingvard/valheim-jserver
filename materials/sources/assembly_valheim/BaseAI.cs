using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000020 RID: 32
public class BaseAI : MonoBehaviour
{
	// Token: 0x06000319 RID: 793 RVA: 0x0001A664 File Offset: 0x00018864
	protected virtual void Awake()
	{
		BaseAI.m_instances.Add(this);
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_character = base.GetComponent<Character>();
		this.m_animator = base.GetComponent<ZSyncAnimation>();
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_solidRayMask = LayerMask.GetMask(new string[]
		{
			"Default",
			"static_solid",
			"Default_small",
			"piece",
			"terrain",
			"vehicle"
		});
		this.m_viewBlockMask = LayerMask.GetMask(new string[]
		{
			"Default",
			"static_solid",
			"Default_small",
			"piece",
			"terrain",
			"viewblock",
			"vehicle"
		});
		this.m_monsterTargetRayMask = LayerMask.GetMask(new string[]
		{
			"piece",
			"piece_nonsolid",
			"Default",
			"static_solid",
			"Default_small",
			"vehicle"
		});
		Character character = this.m_character;
		character.m_onDamaged = (Action<float, Character>)Delegate.Combine(character.m_onDamaged, new Action<float, Character>(this.OnDamaged));
		Character character2 = this.m_character;
		character2.m_onDeath = (Action)Delegate.Combine(character2.m_onDeath, new Action(this.OnDeath));
		if (this.m_nview.IsOwner() && this.m_nview.GetZDO().GetLong(BaseAI.spawnTimeHash, 0L) == 0L)
		{
			this.m_nview.GetZDO().Set(BaseAI.spawnTimeHash, ZNet.instance.GetTime().Ticks);
			if (!string.IsNullOrEmpty(this.m_spawnMessage))
			{
				MessageHud.instance.MessageAll(MessageHud.MessageType.Center, this.m_spawnMessage);
			}
		}
		this.m_randomMoveUpdateTimer = UnityEngine.Random.Range(0f, this.m_randomMoveInterval);
		this.m_nview.Register("Alert", new Action<long>(this.RPC_Alert));
		this.m_nview.Register<Vector3, float, ZDOID>("OnNearProjectileHit", new Action<long, Vector3, float, ZDOID>(this.RPC_OnNearProjectileHit));
		this.m_huntPlayer = this.m_nview.GetZDO().GetBool("huntplayer", this.m_huntPlayer);
		this.m_spawnPoint = this.m_nview.GetZDO().GetVec3("spawnpoint", base.transform.position);
		if (this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set("spawnpoint", this.m_spawnPoint);
		}
		base.InvokeRepeating("DoIdleSound", this.m_idleSoundInterval, this.m_idleSoundInterval);
	}

	// Token: 0x0600031A RID: 794 RVA: 0x0001A907 File Offset: 0x00018B07
	private void OnDestroy()
	{
		BaseAI.m_instances.Remove(this);
	}

	// Token: 0x0600031B RID: 795 RVA: 0x0001A915 File Offset: 0x00018B15
	public void SetPatrolPoint()
	{
		this.SetPatrolPoint(base.transform.position);
	}

	// Token: 0x0600031C RID: 796 RVA: 0x0001A928 File Offset: 0x00018B28
	public void SetPatrolPoint(Vector3 point)
	{
		this.m_patrol = true;
		this.m_patrolPoint = point;
		this.m_nview.GetZDO().Set("patrolPoint", point);
		this.m_nview.GetZDO().Set("patrol", true);
	}

	// Token: 0x0600031D RID: 797 RVA: 0x0001A964 File Offset: 0x00018B64
	public void ResetPatrolPoint()
	{
		this.m_patrol = false;
		this.m_nview.GetZDO().Set("patrol", false);
	}

	// Token: 0x0600031E RID: 798 RVA: 0x0001A984 File Offset: 0x00018B84
	public bool GetPatrolPoint(out Vector3 point)
	{
		if (Time.time - this.m_patrolPointUpdateTime > 1f)
		{
			this.m_patrolPointUpdateTime = Time.time;
			this.m_patrol = this.m_nview.GetZDO().GetBool("patrol", false);
			if (this.m_patrol)
			{
				this.m_patrolPoint = this.m_nview.GetZDO().GetVec3("patrolPoint", this.m_patrolPoint);
			}
		}
		point = this.m_patrolPoint;
		return this.m_patrol;
	}

	// Token: 0x0600031F RID: 799 RVA: 0x0001AA08 File Offset: 0x00018C08
	private void FixedUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.m_updateTimer += Time.fixedDeltaTime;
		if (this.m_updateTimer >= 0.05f)
		{
			this.UpdateAI(0.05f);
			this.m_updateTimer -= 0.05f;
		}
	}

	// Token: 0x06000320 RID: 800 RVA: 0x0001AA60 File Offset: 0x00018C60
	protected virtual void UpdateAI(float dt)
	{
		if (this.m_nview.IsOwner())
		{
			this.UpdateTakeoffLanding(dt);
			if (this.m_jumpInterval > 0f)
			{
				this.m_jumpTimer += dt;
			}
			if (this.m_randomMoveUpdateTimer > 0f)
			{
				this.m_randomMoveUpdateTimer -= dt;
			}
			this.UpdateRegeneration(dt);
			this.m_timeSinceHurt += dt;
			return;
		}
		this.m_alerted = this.m_nview.GetZDO().GetBool("alert", false);
	}

	// Token: 0x06000321 RID: 801 RVA: 0x0001AAEC File Offset: 0x00018CEC
	private void UpdateRegeneration(float dt)
	{
		this.m_regenTimer += dt;
		if (this.m_regenTimer > 1f)
		{
			this.m_regenTimer = 0f;
			float num = this.m_character.GetMaxHealth() / 3600f;
			float worldTimeDelta = this.GetWorldTimeDelta();
			this.m_character.Heal(num * worldTimeDelta, false);
		}
	}

	// Token: 0x06000322 RID: 802 RVA: 0x0001AB47 File Offset: 0x00018D47
	public bool IsTakingOff()
	{
		return this.m_randomFly && this.m_character.IsFlying() && this.m_randomFlyTimer < this.m_takeoffTime;
	}

	// Token: 0x06000323 RID: 803 RVA: 0x0001AB70 File Offset: 0x00018D70
	public void UpdateTakeoffLanding(float dt)
	{
		if (!this.m_randomFly)
		{
			return;
		}
		this.m_randomFlyTimer += dt;
		if (this.m_character.InAttack() || this.m_character.IsStaggering())
		{
			return;
		}
		if (this.m_character.IsFlying())
		{
			if (this.m_randomFlyTimer > this.m_airDuration && this.GetAltitude() < this.m_maxLandAltitude)
			{
				this.m_randomFlyTimer = 0f;
				if (UnityEngine.Random.value <= this.m_chanceToLand)
				{
					this.m_character.m_flying = false;
					this.m_animator.SetTrigger("fly_land");
					return;
				}
			}
		}
		else if (this.m_randomFlyTimer > this.m_groundDuration)
		{
			this.m_randomFlyTimer = 0f;
			if (UnityEngine.Random.value <= this.m_chanceToTakeoff)
			{
				this.m_character.m_flying = true;
				this.m_character.m_jumpEffects.Create(this.m_character.transform.position, Quaternion.identity, null, 1f);
				this.m_animator.SetTrigger("fly_takeoff");
			}
		}
	}

	// Token: 0x06000324 RID: 804 RVA: 0x0001AC84 File Offset: 0x00018E84
	private float GetWorldTimeDelta()
	{
		DateTime time = ZNet.instance.GetTime();
		long @long = this.m_nview.GetZDO().GetLong(BaseAI.worldTimeHash, 0L);
		if (@long == 0L)
		{
			this.m_nview.GetZDO().Set(BaseAI.worldTimeHash, time.Ticks);
			return 0f;
		}
		DateTime d = new DateTime(@long);
		TimeSpan timeSpan = time - d;
		this.m_nview.GetZDO().Set(BaseAI.worldTimeHash, time.Ticks);
		return (float)timeSpan.TotalSeconds;
	}

	// Token: 0x06000325 RID: 805 RVA: 0x0001AD10 File Offset: 0x00018F10
	public TimeSpan GetTimeSinceSpawned()
	{
		long num = this.m_nview.GetZDO().GetLong("spawntime", 0L);
		if (num == 0L)
		{
			num = ZNet.instance.GetTime().Ticks;
			this.m_nview.GetZDO().Set("spawntime", num);
		}
		DateTime d = new DateTime(num);
		return ZNet.instance.GetTime() - d;
	}

	// Token: 0x06000326 RID: 806 RVA: 0x0001AD79 File Offset: 0x00018F79
	private void DoIdleSound()
	{
		if (this.IsSleeping())
		{
			return;
		}
		if (UnityEngine.Random.value > this.m_idleSoundChance)
		{
			return;
		}
		this.m_idleSound.Create(base.transform.position, Quaternion.identity, null, 1f);
	}

	// Token: 0x06000327 RID: 807 RVA: 0x0001ADB4 File Offset: 0x00018FB4
	protected void Follow(GameObject go, float dt)
	{
		float num = Vector3.Distance(go.transform.position, base.transform.position);
		bool run = num > 10f;
		if (num < 3f)
		{
			this.StopMoving();
			return;
		}
		this.MoveTo(dt, go.transform.position, 0f, run);
	}

	// Token: 0x06000328 RID: 808 RVA: 0x0001AE0C File Offset: 0x0001900C
	protected void MoveToWater(float dt, float maxRange)
	{
		float num = this.m_haveWaterPosition ? 2f : 0.5f;
		if (Time.time - this.m_lastMoveToWaterUpdate > num)
		{
			this.m_lastMoveToWaterUpdate = Time.time;
			Vector3 vector = base.transform.position;
			for (int i = 0; i < 10; i++)
			{
				Vector3 b = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f) * Vector3.forward * UnityEngine.Random.Range(4f, maxRange);
				Vector3 vector2 = base.transform.position + b;
				vector2.y = ZoneSystem.instance.GetSolidHeight(vector2);
				if (vector2.y < vector.y)
				{
					vector = vector2;
				}
			}
			if (vector.y < ZoneSystem.instance.m_waterLevel)
			{
				this.m_moveToWaterPosition = vector;
				this.m_haveWaterPosition = true;
			}
			else
			{
				this.m_haveWaterPosition = false;
			}
		}
		if (this.m_haveWaterPosition)
		{
			this.MoveTowards(this.m_moveToWaterPosition - base.transform.position, true);
		}
	}

	// Token: 0x06000329 RID: 809 RVA: 0x0001AF20 File Offset: 0x00019120
	protected void MoveAwayAndDespawn(float dt, bool run)
	{
		Player closestPlayer = Player.GetClosestPlayer(base.transform.position, 40f);
		if (closestPlayer != null)
		{
			Vector3 normalized = (closestPlayer.transform.position - base.transform.position).normalized;
			this.MoveTo(dt, base.transform.position - normalized * 5f, 0f, run);
			return;
		}
		this.m_nview.Destroy();
	}

	// Token: 0x0600032A RID: 810 RVA: 0x0001AFA8 File Offset: 0x000191A8
	protected void IdleMovement(float dt)
	{
		Vector3 centerPoint = this.m_character.IsTamed() ? base.transform.position : this.m_spawnPoint;
		Vector3 vector;
		if (this.GetPatrolPoint(out vector))
		{
			centerPoint = vector;
		}
		this.RandomMovement(dt, centerPoint);
	}

	// Token: 0x0600032B RID: 811 RVA: 0x0001AFEC File Offset: 0x000191EC
	protected void RandomMovement(float dt, Vector3 centerPoint)
	{
		if (this.m_randomMoveUpdateTimer <= 0f)
		{
			if (Utils.DistanceXZ(centerPoint, base.transform.position) > this.m_randomMoveRange * 2f)
			{
				Vector3 vector = centerPoint - base.transform.position;
				vector.y = 0f;
				vector.Normalize();
				vector = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(-30, 30), 0f) * vector;
				this.m_randomMoveTarget = base.transform.position + vector * this.m_randomMoveRange * 2f;
			}
			else
			{
				Vector3 b = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f) * base.transform.forward * UnityEngine.Random.Range(this.m_randomMoveRange * 0.7f, this.m_randomMoveRange);
				this.m_randomMoveTarget = centerPoint + b;
			}
			float waterLevel;
			if (this.m_character.IsFlying() && ZoneSystem.instance.GetSolidHeight(this.m_randomMoveTarget, out waterLevel))
			{
				if (waterLevel < ZoneSystem.instance.m_waterLevel)
				{
					waterLevel = ZoneSystem.instance.m_waterLevel;
				}
				this.m_randomMoveTarget.y = waterLevel + UnityEngine.Random.Range(this.m_flyAltitudeMin, this.m_flyAltitudeMax);
			}
			if (!this.IsValidRandomMovePoint(this.m_randomMoveTarget))
			{
				return;
			}
			this.m_randomMoveUpdateTimer = UnityEngine.Random.Range(this.m_randomMoveInterval, this.m_randomMoveInterval + this.m_randomMoveInterval / 2f);
			if (this.m_avoidWater && this.m_character.IsSwiming())
			{
				this.m_randomMoveUpdateTimer /= 4f;
			}
		}
		bool flag = this.IsAlerted() || Utils.DistanceXZ(base.transform.position, centerPoint) > this.m_randomMoveRange * 2f;
		if (this.MoveTo(dt, this.m_randomMoveTarget, 0f, flag) && flag)
		{
			this.m_randomMoveUpdateTimer = 0f;
		}
	}

	// Token: 0x0600032C RID: 812 RVA: 0x0001B1F0 File Offset: 0x000193F0
	protected void Flee(float dt, Vector3 from)
	{
		float time = Time.time;
		if (time - this.m_fleeTargetUpdateTime > 2f)
		{
			this.m_fleeTargetUpdateTime = time;
			Vector3 point = -(from - base.transform.position);
			point.y = 0f;
			point.Normalize();
			bool flag = false;
			for (int i = 0; i < 4; i++)
			{
				this.m_fleeTarget = base.transform.position + Quaternion.Euler(0f, UnityEngine.Random.Range(-45f, 45f), 0f) * point * 25f;
				if (this.HavePath(this.m_fleeTarget) && (!this.m_avoidWater || this.m_character.IsSwiming() || ZoneSystem.instance.GetSolidHeight(this.m_fleeTarget) >= ZoneSystem.instance.m_waterLevel))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				this.m_fleeTarget = base.transform.position + Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f) * Vector3.forward * 25f;
			}
		}
		this.MoveTo(dt, this.m_fleeTarget, 0f, this.IsAlerted());
	}

	// Token: 0x0600032D RID: 813 RVA: 0x0001B344 File Offset: 0x00019544
	protected bool AvoidFire(float dt, Character moveToTarget, bool superAfraid)
	{
		if (superAfraid)
		{
			EffectArea effectArea = EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.Fire, 3f);
			if (effectArea)
			{
				this.m_nearFireTime = Time.time;
				this.m_nearFireArea = effectArea;
			}
			if (Time.time - this.m_nearFireTime < 6f && this.m_nearFireArea)
			{
				this.SetAlerted(true);
				this.Flee(dt, this.m_nearFireArea.transform.position);
				return true;
			}
		}
		else
		{
			EffectArea effectArea2 = EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.Fire, 3f);
			if (effectArea2)
			{
				if (moveToTarget != null && EffectArea.IsPointInsideArea(moveToTarget.transform.position, EffectArea.Type.Fire, 0f))
				{
					this.RandomMovementArroundPoint(dt, effectArea2.transform.position, effectArea2.GetRadius() + 3f + 1f, this.IsAlerted());
					return true;
				}
				this.RandomMovementArroundPoint(dt, effectArea2.transform.position, (effectArea2.GetRadius() + 3f) * 1.5f, this.IsAlerted());
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600032E RID: 814 RVA: 0x0001B46C File Offset: 0x0001966C
	protected void RandomMovementArroundPoint(float dt, Vector3 point, float distance, bool run)
	{
		float time = Time.time;
		if (time - this.aroundPointUpdateTime > this.m_randomCircleInterval)
		{
			this.aroundPointUpdateTime = time;
			Vector3 point2 = base.transform.position - point;
			point2.y = 0f;
			point2.Normalize();
			float num;
			if (Vector3.Distance(base.transform.position, point) < distance / 2f)
			{
				num = (float)(((double)UnityEngine.Random.value > 0.5) ? 90 : -90);
			}
			else
			{
				num = (float)(((double)UnityEngine.Random.value > 0.5) ? 40 : -40);
			}
			Vector3 a = Quaternion.Euler(0f, num, 0f) * point2;
			this.arroundPointTarget = point + a * distance;
			if (Vector3.Dot(base.transform.forward, this.arroundPointTarget - base.transform.position) < 0f)
			{
				a = Quaternion.Euler(0f, -num, 0f) * point2;
				this.arroundPointTarget = point + a * distance;
				if (this.m_serpentMovement && Vector3.Distance(point, base.transform.position) > distance / 2f && Vector3.Dot(base.transform.forward, this.arroundPointTarget - base.transform.position) < 0f)
				{
					this.arroundPointTarget = point - a * distance;
				}
			}
			if (this.m_character.IsFlying())
			{
				this.arroundPointTarget.y = this.arroundPointTarget.y + UnityEngine.Random.Range(this.m_flyAltitudeMin, this.m_flyAltitudeMax);
			}
		}
		if (this.MoveTo(dt, this.arroundPointTarget, 0f, run))
		{
			if (run)
			{
				this.aroundPointUpdateTime = 0f;
			}
			if (!this.m_serpentMovement && !run)
			{
				this.LookAt(point);
			}
		}
	}

	// Token: 0x0600032F RID: 815 RVA: 0x0001B660 File Offset: 0x00019860
	private bool GetSolidHeight(Vector3 p, out float height, float maxYDistance)
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(p + Vector3.up * maxYDistance, Vector3.down, out raycastHit, maxYDistance * 2f, this.m_solidRayMask))
		{
			height = raycastHit.point.y;
			return true;
		}
		height = 0f;
		return false;
	}

	// Token: 0x06000330 RID: 816 RVA: 0x0001B6B4 File Offset: 0x000198B4
	protected bool IsValidRandomMovePoint(Vector3 point)
	{
		if (this.m_character.IsFlying())
		{
			return true;
		}
		float num;
		if (this.m_avoidWater && this.GetSolidHeight(point, out num, 50f))
		{
			if (this.m_character.IsSwiming())
			{
				float num2;
				if (this.GetSolidHeight(base.transform.position, out num2, 50f) && num < num2)
				{
					return false;
				}
			}
			else if (num < ZoneSystem.instance.m_waterLevel)
			{
				return false;
			}
		}
		return (!this.m_afraidOfFire && !this.m_avoidFire) || !EffectArea.IsPointInsideArea(point, EffectArea.Type.Fire, 0f);
	}

	// Token: 0x06000331 RID: 817 RVA: 0x0001B74A File Offset: 0x0001994A
	protected virtual void OnDamaged(float damage, Character attacker)
	{
		this.m_timeSinceHurt = 0f;
	}

	// Token: 0x06000332 RID: 818 RVA: 0x0001B757 File Offset: 0x00019957
	protected virtual void OnDeath()
	{
		if (!string.IsNullOrEmpty(this.m_deathMessage))
		{
			MessageHud.instance.MessageAll(MessageHud.MessageType.Center, this.m_deathMessage);
		}
	}

	// Token: 0x06000333 RID: 819 RVA: 0x0001B777 File Offset: 0x00019977
	public bool CanSenseTarget(Character target)
	{
		return this.CanHearTarget(target) || this.CanSeeTarget(target);
	}

	// Token: 0x06000334 RID: 820 RVA: 0x0001B790 File Offset: 0x00019990
	public bool CanHearTarget(Character target)
	{
		if (target.IsPlayer())
		{
			Player player = target as Player;
			if (player.InDebugFlyMode() || player.InGhostMode())
			{
				return false;
			}
		}
		float num = Vector3.Distance(target.transform.position, base.transform.position);
		float num2 = this.m_hearRange;
		if (this.m_character.InInterior())
		{
			num2 = Mathf.Min(8f, num2);
		}
		return num <= num2 && num < target.GetNoiseRange();
	}

	// Token: 0x06000335 RID: 821 RVA: 0x0001B80C File Offset: 0x00019A0C
	public bool CanSeeTarget(Character target)
	{
		if (target.IsPlayer())
		{
			Player player = target as Player;
			if (player.InDebugFlyMode() || player.InGhostMode())
			{
				return false;
			}
		}
		float num = Vector3.Distance(target.transform.position, base.transform.position);
		if (num > this.m_viewRange)
		{
			return false;
		}
		float factor = 1f - num / this.m_viewRange;
		float stealthFactor = target.GetStealthFactor();
		float num2 = this.m_viewRange * stealthFactor;
		if (num > num2)
		{
			target.OnStealthSuccess(this.m_character, factor);
			return false;
		}
		if (!this.IsAlerted() && Vector3.Angle(target.transform.position - this.m_character.transform.position, base.transform.forward) > this.m_viewAngle)
		{
			target.OnStealthSuccess(this.m_character, factor);
			return false;
		}
		Vector3 vector = (target.IsCrouching() ? target.GetCenterPoint() : target.m_eye.position) - this.m_character.m_eye.position;
		if (Physics.Raycast(this.m_character.m_eye.position, vector.normalized, vector.magnitude, this.m_viewBlockMask))
		{
			target.OnStealthSuccess(this.m_character, factor);
			return false;
		}
		return true;
	}

	// Token: 0x06000336 RID: 822 RVA: 0x0001B954 File Offset: 0x00019B54
	public bool CanSeeTarget(StaticTarget target)
	{
		Vector3 center = target.GetCenter();
		if (Vector3.Distance(center, base.transform.position) > this.m_viewRange)
		{
			return false;
		}
		Vector3 rhs = center - this.m_character.m_eye.position;
		if (!this.IsAlerted() && Vector3.Dot(base.transform.forward, rhs) < 0f)
		{
			return false;
		}
		List<Collider> allColliders = target.GetAllColliders();
		int num = Physics.RaycastNonAlloc(this.m_character.m_eye.position, rhs.normalized, BaseAI.m_tempRaycastHits, rhs.magnitude, this.m_viewBlockMask);
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = BaseAI.m_tempRaycastHits[i];
			if (!allColliders.Contains(raycastHit.collider))
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000337 RID: 823 RVA: 0x0001BA24 File Offset: 0x00019C24
	protected void MoveTowardsSwoop(Vector3 dir, bool run, float distance)
	{
		dir = dir.normalized;
		float num = Mathf.Clamp01(Vector3.Dot(dir, this.m_character.transform.forward));
		num *= num;
		float num2 = Mathf.Clamp01(distance / this.m_serpentTurnRadius);
		float num3 = 1f - (1f - num2) * (1f - num);
		num3 = num3 * 0.9f + 0.1f;
		Vector3 moveDir = base.transform.forward * num3;
		this.LookTowards(dir);
		this.m_character.SetMoveDir(moveDir);
		this.m_character.SetRun(run);
	}

	// Token: 0x06000338 RID: 824 RVA: 0x0001BAC0 File Offset: 0x00019CC0
	protected void MoveTowards(Vector3 dir, bool run)
	{
		dir = dir.normalized;
		this.LookTowards(dir);
		if (this.m_smoothMovement)
		{
			float num = Vector3.Angle(dir, base.transform.forward);
			float d = 1f - Mathf.Clamp01(num / this.m_moveMinAngle);
			Vector3 moveDir = base.transform.forward * d;
			moveDir.y = dir.y;
			this.m_character.SetMoveDir(moveDir);
			this.m_character.SetRun(run);
			if (this.m_jumpInterval > 0f && this.m_jumpTimer >= this.m_jumpInterval)
			{
				this.m_jumpTimer = 0f;
				this.m_character.Jump();
				return;
			}
		}
		else if (this.IsLookingTowards(dir, this.m_moveMinAngle))
		{
			this.m_character.SetMoveDir(dir);
			this.m_character.SetRun(run);
			if (this.m_jumpInterval > 0f && this.m_jumpTimer >= this.m_jumpInterval)
			{
				this.m_jumpTimer = 0f;
				this.m_character.Jump();
				return;
			}
		}
		else
		{
			this.StopMoving();
		}
	}

	// Token: 0x06000339 RID: 825 RVA: 0x0001BBDC File Offset: 0x00019DDC
	protected void LookAt(Vector3 point)
	{
		Vector3 vector = point - this.m_character.m_eye.position;
		if (Utils.LengthXZ(vector) < 0.01f)
		{
			return;
		}
		vector.Normalize();
		this.LookTowards(vector);
	}

	// Token: 0x0600033A RID: 826 RVA: 0x0001BC1C File Offset: 0x00019E1C
	protected void LookTowards(Vector3 dir)
	{
		this.m_character.SetLookDir(dir);
	}

	// Token: 0x0600033B RID: 827 RVA: 0x0001BC2C File Offset: 0x00019E2C
	protected bool IsLookingAt(Vector3 point, float minAngle)
	{
		return this.IsLookingTowards((point - base.transform.position).normalized, minAngle);
	}

	// Token: 0x0600033C RID: 828 RVA: 0x0001BC5C File Offset: 0x00019E5C
	protected bool IsLookingTowards(Vector3 dir, float minAngle)
	{
		dir.y = 0f;
		Vector3 forward = base.transform.forward;
		forward.y = 0f;
		return Vector3.Angle(dir, forward) < minAngle;
	}

	// Token: 0x0600033D RID: 829 RVA: 0x0001BC97 File Offset: 0x00019E97
	protected void StopMoving()
	{
		this.m_character.SetMoveDir(Vector3.zero);
	}

	// Token: 0x0600033E RID: 830 RVA: 0x0001BCAC File Offset: 0x00019EAC
	protected bool HavePath(Vector3 target)
	{
		if (this.m_character.IsFlying())
		{
			return true;
		}
		float time = Time.time;
		float num = time - this.m_lastHavePathTime;
		Vector3 position = base.transform.position;
		if (Vector3.Distance(position, this.m_havePathFrom) > 2f || Vector3.Distance(target, this.m_havePathTarget) > 1f || num > 5f)
		{
			this.m_havePathFrom = position;
			this.m_havePathTarget = target;
			this.m_lastHavePathTime = time;
			this.m_lastHavePathResult = Pathfinding.instance.HavePath(position, target, this.m_pathAgentType);
		}
		return this.m_lastHavePathResult;
	}

	// Token: 0x0600033F RID: 831 RVA: 0x0001BD44 File Offset: 0x00019F44
	protected bool FindPath(Vector3 target)
	{
		float time = Time.time;
		float num = time - this.m_lastFindPathTime;
		if (num < 1f)
		{
			return this.m_lastFindPathResult;
		}
		if (Vector3.Distance(target, this.m_lastFindPathTarget) < 1f && num < 5f)
		{
			return this.m_lastFindPathResult;
		}
		this.m_lastFindPathTarget = target;
		this.m_lastFindPathTime = time;
		this.m_lastFindPathResult = Pathfinding.instance.GetPath(base.transform.position, target, this.m_path, this.m_pathAgentType, false, true);
		return this.m_lastFindPathResult;
	}

	// Token: 0x06000340 RID: 832 RVA: 0x0001BDCF File Offset: 0x00019FCF
	protected bool FoundPath()
	{
		return this.m_lastFindPathResult;
	}

	// Token: 0x06000341 RID: 833 RVA: 0x0001BDD8 File Offset: 0x00019FD8
	protected bool MoveTo(float dt, Vector3 point, float dist, bool run)
	{
		if (this.m_character.m_flying)
		{
			dist = Mathf.Max(dist, 1f);
			float num;
			if (ZoneSystem.instance.GetSolidHeight(point, out num))
			{
				point.y = Mathf.Max(point.y, num + this.m_flyAltitudeMin);
			}
			return this.MoveAndAvoid(dt, point, dist, run);
		}
		float num2 = run ? 1f : 0.5f;
		if (this.m_serpentMovement)
		{
			num2 = 3f;
		}
		if (Utils.DistanceXZ(point, base.transform.position) < Mathf.Max(dist, num2))
		{
			this.StopMoving();
			return true;
		}
		if (!this.FindPath(point))
		{
			this.StopMoving();
			return true;
		}
		if (this.m_path.Count == 0)
		{
			this.StopMoving();
			return true;
		}
		Vector3 vector = this.m_path[0];
		if (Utils.DistanceXZ(vector, base.transform.position) < num2)
		{
			this.m_path.RemoveAt(0);
			if (this.m_path.Count == 0)
			{
				this.StopMoving();
				return true;
			}
		}
		else if (this.m_serpentMovement)
		{
			float distance = Vector3.Distance(vector, base.transform.position);
			Vector3 normalized = (vector - base.transform.position).normalized;
			this.MoveTowardsSwoop(normalized, run, distance);
		}
		else
		{
			Vector3 normalized2 = (vector - base.transform.position).normalized;
			this.MoveTowards(normalized2, run);
		}
		return false;
	}

	// Token: 0x06000342 RID: 834 RVA: 0x0001BF48 File Offset: 0x0001A148
	protected bool MoveAndAvoid(float dt, Vector3 point, float dist, bool run)
	{
		Vector3 vector = point - base.transform.position;
		if (this.m_character.IsFlying())
		{
			if (vector.magnitude < dist)
			{
				this.StopMoving();
				return true;
			}
		}
		else
		{
			vector.y = 0f;
			if (vector.magnitude < dist)
			{
				this.StopMoving();
				return true;
			}
		}
		vector.Normalize();
		float radius = this.m_character.GetRadius();
		float num = radius + 1f;
		if (!this.m_character.InAttack())
		{
			this.m_getOutOfCornerTimer -= dt;
			if (this.m_getOutOfCornerTimer > 0f)
			{
				Vector3 dir = Quaternion.Euler(0f, this.m_getOutOfCornerAngle, 0f) * -vector;
				this.MoveTowards(dir, run);
				return false;
			}
			this.m_stuckTimer += Time.fixedDeltaTime;
			if (this.m_stuckTimer > 1.5f)
			{
				if (Vector3.Distance(base.transform.position, this.m_lastPosition) < 0.2f)
				{
					this.m_getOutOfCornerTimer = 4f;
					this.m_getOutOfCornerAngle = UnityEngine.Random.Range(-20f, 20f);
					this.m_stuckTimer = 0f;
					return false;
				}
				this.m_stuckTimer = 0f;
				this.m_lastPosition = base.transform.position;
			}
		}
		if (this.CanMove(vector, radius, num))
		{
			this.MoveTowards(vector, run);
		}
		else
		{
			Vector3 forward = base.transform.forward;
			if (this.m_character.IsFlying())
			{
				forward.y = 0.2f;
				forward.Normalize();
			}
			Vector3 b = base.transform.right * radius * 0.75f;
			float num2 = num * 1.5f;
			Vector3 centerPoint = this.m_character.GetCenterPoint();
			float num3 = this.Raycast(centerPoint - b, forward, num2, 0.1f);
			float num4 = this.Raycast(centerPoint + b, forward, num2, 0.1f);
			if (num3 >= num2 && num4 >= num2)
			{
				this.MoveTowards(forward, run);
			}
			else
			{
				Vector3 dir2 = Quaternion.Euler(0f, -20f, 0f) * forward;
				Vector3 dir3 = Quaternion.Euler(0f, 20f, 0f) * forward;
				if (num3 > num4)
				{
					this.MoveTowards(dir2, run);
				}
				else
				{
					this.MoveTowards(dir3, run);
				}
			}
		}
		return false;
	}

	// Token: 0x06000343 RID: 835 RVA: 0x0001C1B8 File Offset: 0x0001A3B8
	private bool CanMove(Vector3 dir, float checkRadius, float distance)
	{
		Vector3 centerPoint = this.m_character.GetCenterPoint();
		Vector3 right = base.transform.right;
		return this.Raycast(centerPoint, dir, distance, 0.1f) >= distance && this.Raycast(centerPoint - right * (checkRadius - 0.1f), dir, distance, 0.1f) >= distance && this.Raycast(centerPoint + right * (checkRadius - 0.1f), dir, distance, 0.1f) >= distance;
	}

	// Token: 0x06000344 RID: 836 RVA: 0x0001C23C File Offset: 0x0001A43C
	public float Raycast(Vector3 p, Vector3 dir, float distance, float radius)
	{
		if (radius == 0f)
		{
			RaycastHit raycastHit;
			if (Physics.Raycast(p, dir, out raycastHit, distance, this.m_solidRayMask))
			{
				return raycastHit.distance;
			}
			return distance;
		}
		else
		{
			RaycastHit raycastHit2;
			if (Physics.SphereCast(p, radius, dir, out raycastHit2, distance, this.m_solidRayMask))
			{
				return raycastHit2.distance;
			}
			return distance;
		}
	}

	// Token: 0x06000345 RID: 837 RVA: 0x0001C28B File Offset: 0x0001A48B
	public bool IsEnemey(Character other)
	{
		return BaseAI.IsEnemy(this.m_character, other);
	}

	// Token: 0x06000346 RID: 838 RVA: 0x0001C29C File Offset: 0x0001A49C
	public static bool IsEnemy(Character a, Character b)
	{
		if (a == b)
		{
			return false;
		}
		Character.Faction faction = a.GetFaction();
		Character.Faction faction2 = b.GetFaction();
		if (faction == faction2)
		{
			return false;
		}
		bool flag = a.IsTamed();
		bool flag2 = b.IsTamed();
		if (flag || flag2)
		{
			return (!flag || !flag2) && (!flag || faction2 != Character.Faction.Players) && (!flag2 || faction != Character.Faction.Players);
		}
		switch (faction)
		{
		case Character.Faction.Players:
			return true;
		case Character.Faction.AnimalsVeg:
			return true;
		case Character.Faction.ForestMonsters:
			return faction2 != Character.Faction.AnimalsVeg && faction2 != Character.Faction.Boss;
		case Character.Faction.Undead:
			return faction2 != Character.Faction.Demon && faction2 != Character.Faction.Boss;
		case Character.Faction.Demon:
			return faction2 != Character.Faction.Undead && faction2 != Character.Faction.Boss;
		case Character.Faction.MountainMonsters:
			return faction2 != Character.Faction.Boss;
		case Character.Faction.SeaMonsters:
			return faction2 != Character.Faction.Boss;
		case Character.Faction.PlainsMonsters:
			return faction2 != Character.Faction.Boss;
		case Character.Faction.Boss:
			return faction2 == Character.Faction.Players;
		default:
			return false;
		}
	}

	// Token: 0x06000347 RID: 839 RVA: 0x0001C368 File Offset: 0x0001A568
	protected StaticTarget FindRandomStaticTarget(float maxDistance, bool priorityTargetsOnly)
	{
		float radius = this.m_character.GetRadius();
		Collider[] array = Physics.OverlapSphere(base.transform.position, radius + maxDistance, this.m_monsterTargetRayMask);
		if (array.Length == 0)
		{
			return null;
		}
		List<StaticTarget> list = new List<StaticTarget>();
		Collider[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			StaticTarget componentInParent = array2[i].GetComponentInParent<StaticTarget>();
			if (!(componentInParent == null) && componentInParent.IsValidMonsterTarget())
			{
				if (priorityTargetsOnly)
				{
					if (!componentInParent.m_primaryTarget)
					{
						goto IL_80;
					}
				}
				else if (!componentInParent.m_randomTarget)
				{
					goto IL_80;
				}
				if (this.CanSeeTarget(componentInParent))
				{
					list.Add(componentInParent);
				}
			}
			IL_80:;
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	// Token: 0x06000348 RID: 840 RVA: 0x0001C420 File Offset: 0x0001A620
	protected StaticTarget FindClosestStaticPriorityTarget(float maxDistance)
	{
		float num = Mathf.Min(maxDistance, this.m_viewRange);
		Collider[] array = Physics.OverlapSphere(base.transform.position, num, this.m_monsterTargetRayMask);
		if (array.Length == 0)
		{
			return null;
		}
		StaticTarget result = null;
		float num2 = num;
		Collider[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			StaticTarget componentInParent = array2[i].GetComponentInParent<StaticTarget>();
			if (!(componentInParent == null) && componentInParent.IsValidMonsterTarget() && componentInParent.m_primaryTarget)
			{
				float num3 = Vector3.Distance(base.transform.position, componentInParent.GetCenter());
				if (num3 < num2 && this.CanSeeTarget(componentInParent))
				{
					result = componentInParent;
					num2 = num3;
				}
			}
		}
		return result;
	}

	// Token: 0x06000349 RID: 841 RVA: 0x0001C4CC File Offset: 0x0001A6CC
	protected void HaveFriendsInRange(float range, out Character hurtFriend, out Character friend)
	{
		List<Character> allCharacters = Character.GetAllCharacters();
		friend = this.HaveFriendInRange(allCharacters, range);
		hurtFriend = this.HaveHurtFriendInRange(allCharacters, range);
	}

	// Token: 0x0600034A RID: 842 RVA: 0x0001C4F4 File Offset: 0x0001A6F4
	private Character HaveFriendInRange(List<Character> characters, float range)
	{
		foreach (Character character in characters)
		{
			if (!(character == this.m_character) && !BaseAI.IsEnemy(this.m_character, character) && Vector3.Distance(character.transform.position, base.transform.position) <= range)
			{
				return character;
			}
		}
		return null;
	}

	// Token: 0x0600034B RID: 843 RVA: 0x0001C57C File Offset: 0x0001A77C
	protected Character HaveFriendInRange(float range)
	{
		List<Character> allCharacters = Character.GetAllCharacters();
		return this.HaveFriendInRange(allCharacters, range);
	}

	// Token: 0x0600034C RID: 844 RVA: 0x0001C598 File Offset: 0x0001A798
	private Character HaveHurtFriendInRange(List<Character> characters, float range)
	{
		foreach (Character character in characters)
		{
			if (!BaseAI.IsEnemy(this.m_character, character) && Vector3.Distance(character.transform.position, base.transform.position) <= range && character.GetHealth() < character.GetMaxHealth())
			{
				return character;
			}
		}
		return null;
	}

	// Token: 0x0600034D RID: 845 RVA: 0x0001C620 File Offset: 0x0001A820
	protected Character HaveHurtFriendInRange(float range)
	{
		List<Character> allCharacters = Character.GetAllCharacters();
		return this.HaveHurtFriendInRange(allCharacters, range);
	}

	// Token: 0x0600034E RID: 846 RVA: 0x0001C63C File Offset: 0x0001A83C
	protected Character FindEnemy()
	{
		List<Character> allCharacters = Character.GetAllCharacters();
		Character character = null;
		float num = 99999f;
		foreach (Character character2 in allCharacters)
		{
			if (BaseAI.IsEnemy(this.m_character, character2) && !character2.IsDead())
			{
				BaseAI baseAI = character2.GetBaseAI();
				if ((!(baseAI != null) || !baseAI.IsSleeping()) && this.CanSenseTarget(character2))
				{
					float num2 = Vector3.Distance(character2.transform.position, base.transform.position);
					if (num2 < num || character == null)
					{
						character = character2;
						num = num2;
					}
				}
			}
		}
		if (!(character == null) || !this.HuntPlayer())
		{
			return character;
		}
		Player closestPlayer = Player.GetClosestPlayer(base.transform.position, 200f);
		if (closestPlayer && (closestPlayer.InDebugFlyMode() || closestPlayer.InGhostMode()))
		{
			return null;
		}
		return closestPlayer;
	}

	// Token: 0x0600034F RID: 847 RVA: 0x0001C744 File Offset: 0x0001A944
	public void SetHuntPlayer(bool hunt)
	{
		if (this.m_huntPlayer == hunt)
		{
			return;
		}
		this.m_huntPlayer = hunt;
		if (this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set("huntplayer", this.m_huntPlayer);
		}
	}

	// Token: 0x06000350 RID: 848 RVA: 0x0001C77F File Offset: 0x0001A97F
	public virtual bool HuntPlayer()
	{
		return this.m_huntPlayer;
	}

	// Token: 0x06000351 RID: 849 RVA: 0x0001C788 File Offset: 0x0001A988
	protected bool HaveAlertedCreatureInRange(float range)
	{
		foreach (BaseAI baseAI in BaseAI.m_instances)
		{
			if (Vector3.Distance(base.transform.position, baseAI.transform.position) < range && baseAI.IsAlerted())
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000352 RID: 850 RVA: 0x0001C800 File Offset: 0x0001AA00
	public static void DoProjectileHitNoise(Vector3 center, float range, Character attacker)
	{
		foreach (BaseAI baseAI in BaseAI.m_instances)
		{
			if ((!attacker || baseAI.IsEnemey(attacker)) && Vector3.Distance(baseAI.transform.position, center) < range && baseAI.m_nview && baseAI.m_nview.IsValid())
			{
				baseAI.m_nview.InvokeRPC("OnNearProjectileHit", new object[]
				{
					center,
					range,
					attacker ? attacker.GetZDOID() : ZDOID.None
				});
			}
		}
	}

	// Token: 0x06000353 RID: 851 RVA: 0x0001C8D8 File Offset: 0x0001AAD8
	protected virtual void RPC_OnNearProjectileHit(long sender, Vector3 center, float range, ZDOID attacker)
	{
		this.Alert();
	}

	// Token: 0x06000354 RID: 852 RVA: 0x0001C8E0 File Offset: 0x0001AAE0
	public void Alert()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.IsAlerted())
		{
			return;
		}
		if (this.m_nview.IsOwner())
		{
			this.SetAlerted(true);
			return;
		}
		this.m_nview.InvokeRPC("Alert", Array.Empty<object>());
	}

	// Token: 0x06000355 RID: 853 RVA: 0x0001C92E File Offset: 0x0001AB2E
	private void RPC_Alert(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		this.SetAlerted(true);
	}

	// Token: 0x06000356 RID: 854 RVA: 0x0001C948 File Offset: 0x0001AB48
	protected virtual void SetAlerted(bool alert)
	{
		if (this.m_alerted == alert)
		{
			return;
		}
		this.m_alerted = alert;
		this.m_animator.SetBool("alert", this.m_alerted);
		if (this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set("alert", this.m_alerted);
		}
		if (this.m_alerted)
		{
			this.m_alertedEffects.Create(base.transform.position, Quaternion.identity, null, 1f);
		}
	}

	// Token: 0x06000357 RID: 855 RVA: 0x0001C9D0 File Offset: 0x0001ABD0
	public static bool InStealthRange(Character me)
	{
		bool result = false;
		foreach (BaseAI baseAI in BaseAI.GetAllInstances())
		{
			if (BaseAI.IsEnemy(me, baseAI.m_character))
			{
				float num = Vector3.Distance(me.transform.position, baseAI.transform.position);
				if (num < baseAI.m_viewRange || num < 10f)
				{
					if (baseAI.IsAlerted())
					{
						return false;
					}
					result = true;
				}
			}
		}
		return result;
	}

	// Token: 0x06000358 RID: 856 RVA: 0x0001CA6C File Offset: 0x0001AC6C
	public static Character FindClosestEnemy(Character me, Vector3 point, float maxDistance)
	{
		Character character = null;
		float num = maxDistance;
		foreach (Character character2 in Character.GetAllCharacters())
		{
			if (BaseAI.IsEnemy(me, character2))
			{
				float num2 = Vector3.Distance(character2.transform.position, point);
				if (character == null || num2 < num)
				{
					character = character2;
					num = num2;
				}
			}
		}
		return character;
	}

	// Token: 0x06000359 RID: 857 RVA: 0x0001CAEC File Offset: 0x0001ACEC
	public static Character FindRandomEnemy(Character me, Vector3 point, float maxDistance)
	{
		List<Character> list = new List<Character>();
		foreach (Character character in Character.GetAllCharacters())
		{
			if (BaseAI.IsEnemy(me, character) && Vector3.Distance(character.transform.position, point) < maxDistance)
			{
				list.Add(character);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	// Token: 0x0600035A RID: 858 RVA: 0x0001CB80 File Offset: 0x0001AD80
	public bool IsAlerted()
	{
		return this.m_alerted;
	}

	// Token: 0x0600035B RID: 859 RVA: 0x0001CB88 File Offset: 0x0001AD88
	protected void SetTargetInfo(ZDOID targetID)
	{
		this.m_nview.GetZDO().Set(BaseAI.havetTargetHash, !targetID.IsNone());
	}

	// Token: 0x0600035C RID: 860 RVA: 0x0001CBA9 File Offset: 0x0001ADA9
	public bool HaveTarget()
	{
		return this.m_nview.IsValid() && this.m_nview.GetZDO().GetBool(BaseAI.havetTargetHash, false);
	}

	// Token: 0x0600035D RID: 861 RVA: 0x0001CBD0 File Offset: 0x0001ADD0
	protected float GetAltitude()
	{
		float groundHeight = ZoneSystem.instance.GetGroundHeight(this.m_character.transform.position);
		return this.m_character.transform.position.y - groundHeight;
	}

	// Token: 0x0600035E RID: 862 RVA: 0x0001CC0F File Offset: 0x0001AE0F
	public static List<BaseAI> GetAllInstances()
	{
		return BaseAI.m_instances;
	}

	// Token: 0x0600035F RID: 863 RVA: 0x0001CC18 File Offset: 0x0001AE18
	protected virtual void OnDrawGizmosSelected()
	{
		if (this.m_lastFindPathResult)
		{
			Gizmos.color = Color.yellow;
			for (int i = 0; i < this.m_path.Count - 1; i++)
			{
				Vector3 a = this.m_path[i];
				Vector3 a2 = this.m_path[i + 1];
				Gizmos.DrawLine(a + Vector3.up * 0.1f, a2 + Vector3.up * 0.1f);
			}
			Gizmos.color = Color.cyan;
			foreach (Vector3 a3 in this.m_path)
			{
				Gizmos.DrawSphere(a3 + Vector3.up * 0.1f, 0.1f);
			}
			Gizmos.color = Color.green;
			Gizmos.DrawLine(base.transform.position, this.m_lastFindPathTarget);
			Gizmos.DrawSphere(this.m_lastFindPathTarget, 0.2f);
			return;
		}
		Gizmos.color = Color.red;
		Gizmos.DrawLine(base.transform.position, this.m_lastFindPathTarget);
		Gizmos.DrawSphere(this.m_lastFindPathTarget, 0.2f);
	}

	// Token: 0x06000360 RID: 864 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool IsSleeping()
	{
		return false;
	}

	// Token: 0x06000361 RID: 865 RVA: 0x0001CD64 File Offset: 0x0001AF64
	public bool HasZDOOwner()
	{
		return this.m_nview.IsValid() && this.m_nview.GetZDO().HasOwner();
	}

	// Token: 0x06000362 RID: 866 RVA: 0x0001CD88 File Offset: 0x0001AF88
	public static bool CanUseAttack(Character character, ItemDrop.ItemData item)
	{
		bool flag = character.IsFlying();
		bool flag2 = character.IsSwiming();
		return (item.m_shared.m_aiWhenFlying && flag) || (item.m_shared.m_aiWhenWalking && !flag && !flag2) || (item.m_shared.m_aiWhenSwiming && flag2);
	}

	// Token: 0x06000363 RID: 867 RVA: 0x000058CD File Offset: 0x00003ACD
	public virtual Character GetTargetCreature()
	{
		return null;
	}

	// Token: 0x040002F2 RID: 754
	private float m_lastMoveToWaterUpdate;

	// Token: 0x040002F3 RID: 755
	private bool m_haveWaterPosition;

	// Token: 0x040002F4 RID: 756
	private Vector3 m_moveToWaterPosition = Vector3.zero;

	// Token: 0x040002F5 RID: 757
	private float m_fleeTargetUpdateTime;

	// Token: 0x040002F6 RID: 758
	private Vector3 m_fleeTarget = Vector3.zero;

	// Token: 0x040002F7 RID: 759
	private float m_nearFireTime;

	// Token: 0x040002F8 RID: 760
	private EffectArea m_nearFireArea;

	// Token: 0x040002F9 RID: 761
	private float aroundPointUpdateTime;

	// Token: 0x040002FA RID: 762
	private Vector3 arroundPointTarget = Vector3.zero;

	// Token: 0x040002FB RID: 763
	private const bool m_debugDraw = false;

	// Token: 0x040002FC RID: 764
	public float m_viewRange = 50f;

	// Token: 0x040002FD RID: 765
	public float m_viewAngle = 90f;

	// Token: 0x040002FE RID: 766
	public float m_hearRange = 9999f;

	// Token: 0x040002FF RID: 767
	private const float m_interiorMaxHearRange = 8f;

	// Token: 0x04000300 RID: 768
	private const float m_despawnDistance = 80f;

	// Token: 0x04000301 RID: 769
	private const float m_regenAllHPTime = 3600f;

	// Token: 0x04000302 RID: 770
	public EffectList m_alertedEffects = new EffectList();

	// Token: 0x04000303 RID: 771
	public EffectList m_idleSound = new EffectList();

	// Token: 0x04000304 RID: 772
	public float m_idleSoundInterval = 5f;

	// Token: 0x04000305 RID: 773
	public float m_idleSoundChance = 0.5f;

	// Token: 0x04000306 RID: 774
	public Pathfinding.AgentType m_pathAgentType = Pathfinding.AgentType.Humanoid;

	// Token: 0x04000307 RID: 775
	public float m_moveMinAngle = 10f;

	// Token: 0x04000308 RID: 776
	public bool m_smoothMovement = true;

	// Token: 0x04000309 RID: 777
	public bool m_serpentMovement;

	// Token: 0x0400030A RID: 778
	public float m_serpentTurnRadius = 20f;

	// Token: 0x0400030B RID: 779
	public float m_jumpInterval;

	// Token: 0x0400030C RID: 780
	[Header("Random circle")]
	public float m_randomCircleInterval = 2f;

	// Token: 0x0400030D RID: 781
	[Header("Random movement")]
	public float m_randomMoveInterval = 5f;

	// Token: 0x0400030E RID: 782
	public float m_randomMoveRange = 4f;

	// Token: 0x0400030F RID: 783
	[Header("Fly behaviour")]
	public bool m_randomFly;

	// Token: 0x04000310 RID: 784
	public float m_chanceToTakeoff = 1f;

	// Token: 0x04000311 RID: 785
	public float m_chanceToLand = 1f;

	// Token: 0x04000312 RID: 786
	public float m_groundDuration = 10f;

	// Token: 0x04000313 RID: 787
	public float m_airDuration = 10f;

	// Token: 0x04000314 RID: 788
	public float m_maxLandAltitude = 5f;

	// Token: 0x04000315 RID: 789
	public float m_flyAltitudeMin = 3f;

	// Token: 0x04000316 RID: 790
	public float m_flyAltitudeMax = 10f;

	// Token: 0x04000317 RID: 791
	public float m_takeoffTime = 5f;

	// Token: 0x04000318 RID: 792
	[Header("Other")]
	public bool m_avoidFire;

	// Token: 0x04000319 RID: 793
	public bool m_afraidOfFire;

	// Token: 0x0400031A RID: 794
	public bool m_avoidWater = true;

	// Token: 0x0400031B RID: 795
	public string m_spawnMessage = "";

	// Token: 0x0400031C RID: 796
	public string m_deathMessage = "";

	// Token: 0x0400031D RID: 797
	private bool m_patrol;

	// Token: 0x0400031E RID: 798
	private Vector3 m_patrolPoint = Vector3.zero;

	// Token: 0x0400031F RID: 799
	private float m_patrolPointUpdateTime;

	// Token: 0x04000320 RID: 800
	protected ZNetView m_nview;

	// Token: 0x04000321 RID: 801
	protected Character m_character;

	// Token: 0x04000322 RID: 802
	protected ZSyncAnimation m_animator;

	// Token: 0x04000323 RID: 803
	protected Rigidbody m_body;

	// Token: 0x04000324 RID: 804
	private float m_updateTimer;

	// Token: 0x04000325 RID: 805
	private int m_solidRayMask;

	// Token: 0x04000326 RID: 806
	private int m_viewBlockMask;

	// Token: 0x04000327 RID: 807
	private int m_monsterTargetRayMask;

	// Token: 0x04000328 RID: 808
	private Vector3 m_randomMoveTarget = Vector3.zero;

	// Token: 0x04000329 RID: 809
	private float m_randomMoveUpdateTimer;

	// Token: 0x0400032A RID: 810
	private float m_jumpTimer;

	// Token: 0x0400032B RID: 811
	private float m_randomFlyTimer;

	// Token: 0x0400032C RID: 812
	private float m_regenTimer;

	// Token: 0x0400032D RID: 813
	protected bool m_alerted;

	// Token: 0x0400032E RID: 814
	protected bool m_huntPlayer;

	// Token: 0x0400032F RID: 815
	protected Vector3 m_spawnPoint = Vector3.zero;

	// Token: 0x04000330 RID: 816
	private const float m_getOfOfCornerMaxAngle = 20f;

	// Token: 0x04000331 RID: 817
	private float m_getOutOfCornerTimer;

	// Token: 0x04000332 RID: 818
	private float m_getOutOfCornerAngle;

	// Token: 0x04000333 RID: 819
	private Vector3 m_lastPosition = Vector3.zero;

	// Token: 0x04000334 RID: 820
	private float m_stuckTimer;

	// Token: 0x04000335 RID: 821
	protected float m_timeSinceHurt = 99999f;

	// Token: 0x04000336 RID: 822
	private Vector3 m_havePathTarget = new Vector3(-999999f, -999999f, -999999f);

	// Token: 0x04000337 RID: 823
	private Vector3 m_havePathFrom = new Vector3(-999999f, -999999f, -999999f);

	// Token: 0x04000338 RID: 824
	private float m_lastHavePathTime;

	// Token: 0x04000339 RID: 825
	private bool m_lastHavePathResult;

	// Token: 0x0400033A RID: 826
	private Vector3 m_lastFindPathTarget = new Vector3(-999999f, -999999f, -999999f);

	// Token: 0x0400033B RID: 827
	private float m_lastFindPathTime;

	// Token: 0x0400033C RID: 828
	private bool m_lastFindPathResult;

	// Token: 0x0400033D RID: 829
	private List<Vector3> m_path = new List<Vector3>();

	// Token: 0x0400033E RID: 830
	private static RaycastHit[] m_tempRaycastHits = new RaycastHit[128];

	// Token: 0x0400033F RID: 831
	private static List<BaseAI> m_instances = new List<BaseAI>();

	// Token: 0x04000340 RID: 832
	private static int worldTimeHash = "lastWorldTime".GetStableHashCode();

	// Token: 0x04000341 RID: 833
	private static int spawnTimeHash = "spawntime".GetStableHashCode();

	// Token: 0x04000342 RID: 834
	private static int havetTargetHash = "haveTarget".GetStableHashCode();
}
