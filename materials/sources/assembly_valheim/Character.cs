using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000002 RID: 2
public class Character : MonoBehaviour, IDestructible, Hoverable, IWaterInteractable
{
	// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
	protected virtual void Awake()
	{
		Character.m_characters.Add(this);
		this.m_collider = base.GetComponent<CapsuleCollider>();
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_zanim = base.GetComponent<ZSyncAnimation>();
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_animator = base.GetComponentInChildren<Animator>();
		this.m_animEvent = this.m_animator.GetComponent<CharacterAnimEvent>();
		this.m_baseAI = base.GetComponent<BaseAI>();
		this.m_animator.logWarnings = false;
		this.m_visual = base.transform.Find("Visual").gameObject;
		this.m_lodGroup = this.m_visual.GetComponent<LODGroup>();
		this.m_head = this.m_animator.GetBoneTransform(HumanBodyBones.Head);
		this.m_body.maxDepenetrationVelocity = 2f;
		if (Character.m_smokeRayMask == 0)
		{
			Character.m_smokeRayMask = LayerMask.GetMask(new string[]
			{
				"smoke"
			});
			Character.m_characterLayer = LayerMask.NameToLayer("character");
			Character.m_characterNetLayer = LayerMask.NameToLayer("character_net");
			Character.m_characterGhostLayer = LayerMask.NameToLayer("character_ghost");
		}
		if (Character.forward_speed == 0)
		{
			Character.forward_speed = ZSyncAnimation.GetHash("forward_speed");
			Character.sideway_speed = ZSyncAnimation.GetHash("sideway_speed");
			Character.turn_speed = ZSyncAnimation.GetHash("turn_speed");
			Character.inWater = ZSyncAnimation.GetHash("inWater");
			Character.onGround = ZSyncAnimation.GetHash("onGround");
			Character.encumbered = ZSyncAnimation.GetHash("encumbered");
			Character.flying = ZSyncAnimation.GetHash("flying");
		}
		if (this.m_lodGroup)
		{
			this.m_originalLocalRef = this.m_lodGroup.localReferencePoint;
		}
		this.m_seman = new SEMan(this, this.m_nview);
		if (this.m_nview.GetZDO() != null)
		{
			if (!this.IsPlayer())
			{
				this.m_tamed = this.m_nview.GetZDO().GetBool("tamed", this.m_tamed);
				this.m_level = this.m_nview.GetZDO().GetInt("level", 1);
				if (this.m_nview.IsOwner() && this.GetHealth() == this.GetMaxHealth())
				{
					this.SetupMaxHealth();
				}
			}
			this.m_nview.Register<HitData>("Damage", new Action<long, HitData>(this.RPC_Damage));
			this.m_nview.Register<float, bool>("Heal", new Action<long, float, bool>(this.RPC_Heal));
			this.m_nview.Register<float>("AddNoise", new Action<long, float>(this.RPC_AddNoise));
			this.m_nview.Register<Vector3>("Stagger", new Action<long, Vector3>(this.RPC_Stagger));
			this.m_nview.Register("ResetCloth", new Action<long>(this.RPC_ResetCloth));
			this.m_nview.Register<bool>("SetTamed", new Action<long, bool>(this.RPC_SetTamed));
		}
	}

	// Token: 0x06000002 RID: 2 RVA: 0x00002328 File Offset: 0x00000528
	private void SetupMaxHealth()
	{
		int level = this.GetLevel();
		float difficultyHealthScale = Game.instance.GetDifficultyHealthScale(base.transform.position);
		this.SetMaxHealth(this.m_health * difficultyHealthScale * (float)level);
	}

	// Token: 0x06000003 RID: 3 RVA: 0x00002363 File Offset: 0x00000563
	protected virtual void Start()
	{
		this.m_nview.GetZDO();
	}

	// Token: 0x06000004 RID: 4 RVA: 0x00002371 File Offset: 0x00000571
	public virtual void OnDestroy()
	{
		this.m_seman.OnDestroy();
		Character.m_characters.Remove(this);
	}

	// Token: 0x06000005 RID: 5 RVA: 0x0000238C File Offset: 0x0000058C
	public void SetLevel(int level)
	{
		if (level < 1)
		{
			return;
		}
		this.m_level = level;
		this.m_nview.GetZDO().Set("level", level);
		this.SetupMaxHealth();
		if (this.m_onLevelSet != null)
		{
			this.m_onLevelSet(this.m_level);
		}
	}

	// Token: 0x06000006 RID: 6 RVA: 0x000023DA File Offset: 0x000005DA
	public int GetLevel()
	{
		return this.m_level;
	}

	// Token: 0x06000007 RID: 7 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool IsPlayer()
	{
		return false;
	}

	// Token: 0x06000008 RID: 8 RVA: 0x000023E5 File Offset: 0x000005E5
	public Character.Faction GetFaction()
	{
		return this.m_faction;
	}

	// Token: 0x06000009 RID: 9 RVA: 0x000023F0 File Offset: 0x000005F0
	protected virtual void FixedUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		this.UpdateLayer();
		this.UpdateContinousEffects();
		this.UpdateWater(fixedDeltaTime);
		this.UpdateGroundTilt(fixedDeltaTime);
		this.SetVisible(this.m_nview.HasOwner());
		if (this.m_nview.IsOwner())
		{
			this.UpdateGroundContact(fixedDeltaTime);
			this.UpdateNoise(fixedDeltaTime);
			this.m_seman.Update(fixedDeltaTime);
			this.UpdateStagger(fixedDeltaTime);
			this.UpdatePushback(fixedDeltaTime);
			this.UpdateMotion(fixedDeltaTime);
			this.UpdateSmoke(fixedDeltaTime);
			this.UnderWorldCheck(fixedDeltaTime);
			this.SyncVelocity();
			this.CheckDeath();
		}
	}

	// Token: 0x0600000A RID: 10 RVA: 0x00002494 File Offset: 0x00000694
	private void UpdateLayer()
	{
		if (this.m_collider.gameObject.layer == Character.m_characterLayer || this.m_collider.gameObject.layer == Character.m_characterNetLayer)
		{
			if (this.m_nview.IsOwner())
			{
				this.m_collider.gameObject.layer = (this.IsAttached() ? Character.m_characterNetLayer : Character.m_characterLayer);
				return;
			}
			this.m_collider.gameObject.layer = Character.m_characterNetLayer;
		}
	}

	// Token: 0x0600000B RID: 11 RVA: 0x00002518 File Offset: 0x00000718
	private void UnderWorldCheck(float dt)
	{
		if (this.IsDead())
		{
			return;
		}
		this.m_underWorldCheckTimer += dt;
		if (this.m_underWorldCheckTimer > 5f || this.IsPlayer())
		{
			this.m_underWorldCheckTimer = 0f;
			float groundHeight = ZoneSystem.instance.GetGroundHeight(base.transform.position);
			if (base.transform.position.y < groundHeight - 1f)
			{
				Vector3 position = base.transform.position;
				position.y = groundHeight + 0.5f;
				base.transform.position = position;
				this.m_body.position = position;
				this.m_body.velocity = Vector3.zero;
			}
		}
	}

	// Token: 0x0600000C RID: 12 RVA: 0x000025D0 File Offset: 0x000007D0
	private void UpdateSmoke(float dt)
	{
		if (this.m_tolerateSmoke)
		{
			return;
		}
		this.m_smokeCheckTimer += dt;
		if (this.m_smokeCheckTimer > 2f)
		{
			this.m_smokeCheckTimer = 0f;
			if (Physics.CheckSphere(this.GetTopPoint() + Vector3.up * 0.1f, 0.5f, Character.m_smokeRayMask))
			{
				this.m_seman.AddStatusEffect("Smoked", true);
				return;
			}
			this.m_seman.RemoveStatusEffect("Smoked", true);
		}
	}

	// Token: 0x0600000D RID: 13 RVA: 0x0000265C File Offset: 0x0000085C
	private void UpdateContinousEffects()
	{
		this.SetupContinousEffect(base.transform.position, this.m_sliding, this.m_slideEffects, ref this.m_slideEffects_instances);
		Vector3 position = base.transform.position;
		position.y = this.m_waterLevel + 0.05f;
		this.SetupContinousEffect(position, this.InWater(), this.m_waterEffects, ref this.m_waterEffects_instances);
	}

	// Token: 0x0600000E RID: 14 RVA: 0x000026C4 File Offset: 0x000008C4
	private void SetupContinousEffect(Vector3 point, bool enabled, EffectList effects, ref GameObject[] instances)
	{
		if (!effects.HasEffects())
		{
			return;
		}
		if (!enabled)
		{
			if (instances != null)
			{
				foreach (GameObject gameObject in instances)
				{
					if (gameObject)
					{
						foreach (ParticleSystem particleSystem in gameObject.GetComponentsInChildren<ParticleSystem>())
						{
							particleSystem.emission.enabled = false;
							particleSystem.Stop();
						}
						CamShaker componentInChildren = gameObject.GetComponentInChildren<CamShaker>();
						if (componentInChildren)
						{
							UnityEngine.Object.Destroy(componentInChildren);
						}
						ZSFX componentInChildren2 = gameObject.GetComponentInChildren<ZSFX>();
						if (componentInChildren2)
						{
							componentInChildren2.FadeOut();
						}
						TimedDestruction component = gameObject.GetComponent<TimedDestruction>();
						if (component)
						{
							component.Trigger();
						}
						else
						{
							UnityEngine.Object.Destroy(gameObject);
						}
					}
				}
				instances = null;
			}
			return;
		}
		if (instances == null)
		{
			instances = effects.Create(point, Quaternion.identity, base.transform, 1f);
			return;
		}
		foreach (GameObject gameObject2 in instances)
		{
			if (gameObject2)
			{
				gameObject2.transform.position = point;
			}
		}
	}

	// Token: 0x0600000F RID: 15 RVA: 0x000027E0 File Offset: 0x000009E0
	protected virtual void OnSwiming(Vector3 targetVel, float dt)
	{
	}

	// Token: 0x06000010 RID: 16 RVA: 0x000027E0 File Offset: 0x000009E0
	protected virtual void OnSneaking(float dt)
	{
	}

	// Token: 0x06000011 RID: 17 RVA: 0x000027E0 File Offset: 0x000009E0
	protected virtual void OnJump()
	{
	}

	// Token: 0x06000012 RID: 18 RVA: 0x000027E2 File Offset: 0x000009E2
	protected virtual bool TakeInput()
	{
		return true;
	}

	// Token: 0x06000013 RID: 19 RVA: 0x000027E5 File Offset: 0x000009E5
	private float GetSlideAngle()
	{
		if (!this.IsPlayer())
		{
			return 90f;
		}
		return 38f;
	}

	// Token: 0x06000014 RID: 20 RVA: 0x000027FC File Offset: 0x000009FC
	private void ApplySlide(float dt, ref Vector3 currentVel, Vector3 bodyVel, bool running)
	{
		bool flag = this.CanWallRun();
		float num = Mathf.Acos(Mathf.Clamp01(this.m_lastGroundNormal.y)) * 57.29578f;
		Vector3 lastGroundNormal = this.m_lastGroundNormal;
		lastGroundNormal.y = 0f;
		lastGroundNormal.Normalize();
		Vector3 velocity = this.m_body.velocity;
		Vector3 rhs = Vector3.Cross(this.m_lastGroundNormal, Vector3.up);
		Vector3 a = Vector3.Cross(this.m_lastGroundNormal, rhs);
		bool flag2 = currentVel.magnitude > 0.1f;
		if (num > this.GetSlideAngle())
		{
			if (running && flag && flag2)
			{
				this.UseStamina(10f * dt);
				this.m_slippage = 0f;
				this.m_wallRunning = true;
			}
			else
			{
				this.m_slippage = Mathf.MoveTowards(this.m_slippage, 1f, 1f * dt);
			}
			Vector3 b = a * 5f;
			currentVel = Vector3.Lerp(currentVel, b, this.m_slippage);
			this.m_sliding = (this.m_slippage > 0.5f);
			return;
		}
		this.m_slippage = 0f;
	}

	// Token: 0x06000015 RID: 21 RVA: 0x00002918 File Offset: 0x00000B18
	private void UpdateMotion(float dt)
	{
		this.UpdateBodyFriction();
		this.m_sliding = false;
		this.m_wallRunning = false;
		this.m_running = false;
		if (this.IsDead())
		{
			return;
		}
		if (this.IsDebugFlying())
		{
			this.UpdateDebugFly(dt);
			return;
		}
		if (this.InIntro())
		{
			this.m_maxAirAltitude = base.transform.position.y;
			this.m_body.velocity = Vector3.zero;
			this.m_body.angularVelocity = Vector3.zero;
		}
		if (!this.InWaterSwimDepth() && !this.IsOnGround())
		{
			float y = base.transform.position.y;
			this.m_maxAirAltitude = Mathf.Max(this.m_maxAirAltitude, y);
		}
		if (this.IsSwiming())
		{
			this.UpdateSwiming(dt);
		}
		else if (this.m_flying)
		{
			this.UpdateFlying(dt);
		}
		else
		{
			this.UpdateWalking(dt);
		}
		this.m_lastGroundTouch += Time.fixedDeltaTime;
		this.m_jumpTimer += Time.fixedDeltaTime;
	}

	// Token: 0x06000016 RID: 22 RVA: 0x00002A18 File Offset: 0x00000C18
	private void UpdateDebugFly(float dt)
	{
		float num = (float)(this.m_run ? 50 : 20);
		Vector3 b = this.m_moveDir * num;
		if (this.TakeInput())
		{
			if (ZInput.GetButton("Jump"))
			{
				b.y = num;
			}
			else if (Input.GetKey(KeyCode.LeftControl))
			{
				b.y = -num;
			}
		}
		this.m_currentVel = Vector3.Lerp(this.m_currentVel, b, 0.5f);
		this.m_body.velocity = this.m_currentVel;
		this.m_body.useGravity = false;
		this.m_lastGroundTouch = 0f;
		this.m_maxAirAltitude = base.transform.position.y;
		this.m_body.rotation = Quaternion.RotateTowards(base.transform.rotation, this.m_lookYaw, this.m_turnSpeed * dt);
		this.m_body.angularVelocity = Vector3.zero;
		this.UpdateEyeRotation();
	}

	// Token: 0x06000017 RID: 23 RVA: 0x00002B0C File Offset: 0x00000D0C
	private void UpdateSwiming(float dt)
	{
		bool flag = this.IsOnGround();
		if (Mathf.Max(0f, this.m_maxAirAltitude - base.transform.position.y) > 0.5f && this.m_onLand != null)
		{
			this.m_onLand(new Vector3(base.transform.position.x, this.m_waterLevel, base.transform.position.z));
		}
		this.m_maxAirAltitude = base.transform.position.y;
		float d = this.m_swimSpeed * this.GetAttackSpeedFactorMovement();
		if (this.InMinorAction())
		{
			d = 0f;
		}
		this.m_seman.ApplyStatusEffectSpeedMods(ref d);
		Vector3 vector = this.m_moveDir * d;
		if (vector.magnitude > 0f && this.IsOnGround())
		{
			vector = Vector3.ProjectOnPlane(vector, this.m_lastGroundNormal).normalized * vector.magnitude;
		}
		if (this.IsPlayer())
		{
			this.m_currentVel = Vector3.Lerp(this.m_currentVel, vector, this.m_swimAcceleration);
		}
		else
		{
			float num = vector.magnitude;
			float magnitude = this.m_currentVel.magnitude;
			if (num > magnitude)
			{
				num = Mathf.MoveTowards(magnitude, num, this.m_swimAcceleration);
				vector = vector.normalized * num;
			}
			this.m_currentVel = Vector3.Lerp(this.m_currentVel, vector, 0.5f);
		}
		if (vector.magnitude > 0.1f)
		{
			this.AddNoise(15f);
		}
		this.AddPushbackForce(ref this.m_currentVel);
		Vector3 force = this.m_currentVel - this.m_body.velocity;
		force.y = 0f;
		if (force.magnitude > 20f)
		{
			force = force.normalized * 20f;
		}
		this.m_body.AddForce(force, ForceMode.VelocityChange);
		float num2 = this.m_waterLevel - this.m_swimDepth;
		if (base.transform.position.y < num2)
		{
			float t = Mathf.Clamp01((num2 - base.transform.position.y) / 2f);
			float target = Mathf.Lerp(0f, 10f, t);
			Vector3 velocity = this.m_body.velocity;
			velocity.y = Mathf.MoveTowards(velocity.y, target, 50f * dt);
			this.m_body.velocity = velocity;
		}
		else
		{
			float t2 = Mathf.Clamp01(-(num2 - base.transform.position.y) / 1f);
			float num3 = Mathf.Lerp(0f, 10f, t2);
			Vector3 velocity2 = this.m_body.velocity;
			velocity2.y = Mathf.MoveTowards(velocity2.y, -num3, 30f * dt);
			this.m_body.velocity = velocity2;
		}
		float target2 = 0f;
		if (this.m_moveDir.magnitude > 0.1f || this.AlwaysRotateCamera())
		{
			float swimTurnSpeed = this.m_swimTurnSpeed;
			this.m_seman.ApplyStatusEffectSpeedMods(ref swimTurnSpeed);
			target2 = this.UpdateRotation(swimTurnSpeed, dt);
		}
		this.m_body.angularVelocity = Vector3.zero;
		this.UpdateEyeRotation();
		this.m_body.useGravity = true;
		float num4 = Vector3.Dot(this.m_currentVel, base.transform.forward);
		float value = Vector3.Dot(this.m_currentVel, base.transform.right);
		float num5 = Vector3.Dot(this.m_body.velocity, base.transform.forward);
		this.m_currentTurnVel = Mathf.SmoothDamp(this.m_currentTurnVel, target2, ref this.m_currentTurnVelChange, 0.5f, 99f);
		this.m_zanim.SetFloat(Character.forward_speed, this.IsPlayer() ? num4 : num5);
		this.m_zanim.SetFloat(Character.sideway_speed, value);
		this.m_zanim.SetFloat(Character.turn_speed, this.m_currentTurnVel);
		this.m_zanim.SetBool(Character.inWater, !flag);
		this.m_zanim.SetBool(Character.onGround, false);
		this.m_zanim.SetBool(Character.encumbered, false);
		this.m_zanim.SetBool(Character.flying, false);
		if (!flag)
		{
			this.OnSwiming(vector, dt);
		}
	}

	// Token: 0x06000018 RID: 24 RVA: 0x00002F58 File Offset: 0x00001158
	private void UpdateFlying(float dt)
	{
		float d = (this.m_run ? this.m_flyFastSpeed : this.m_flySlowSpeed) * this.GetAttackSpeedFactorMovement();
		Vector3 b = this.CanMove() ? (this.m_moveDir * d) : Vector3.zero;
		this.m_currentVel = Vector3.Lerp(this.m_currentVel, b, this.m_acceleration);
		this.m_maxAirAltitude = base.transform.position.y;
		this.ApplyRootMotion(ref this.m_currentVel);
		this.AddPushbackForce(ref this.m_currentVel);
		Vector3 force = this.m_currentVel - this.m_body.velocity;
		if (force.magnitude > 20f)
		{
			force = force.normalized * 20f;
		}
		this.m_body.AddForce(force, ForceMode.VelocityChange);
		float target = 0f;
		if ((this.m_moveDir.magnitude > 0.1f || this.AlwaysRotateCamera()) && !this.InDodge() && this.CanMove())
		{
			float flyTurnSpeed = this.m_flyTurnSpeed;
			this.m_seman.ApplyStatusEffectSpeedMods(ref flyTurnSpeed);
			target = this.UpdateRotation(flyTurnSpeed, dt);
		}
		this.m_body.angularVelocity = Vector3.zero;
		this.UpdateEyeRotation();
		this.m_body.useGravity = false;
		float num = Vector3.Dot(this.m_currentVel, base.transform.forward);
		float value = Vector3.Dot(this.m_currentVel, base.transform.right);
		float num2 = Vector3.Dot(this.m_body.velocity, base.transform.forward);
		this.m_currentTurnVel = Mathf.SmoothDamp(this.m_currentTurnVel, target, ref this.m_currentTurnVelChange, 0.5f, 99f);
		this.m_zanim.SetFloat(Character.forward_speed, this.IsPlayer() ? num : num2);
		this.m_zanim.SetFloat(Character.sideway_speed, value);
		this.m_zanim.SetFloat(Character.turn_speed, this.m_currentTurnVel);
		this.m_zanim.SetBool(Character.inWater, false);
		this.m_zanim.SetBool(Character.onGround, false);
		this.m_zanim.SetBool(Character.encumbered, false);
		this.m_zanim.SetBool(Character.flying, true);
	}

	// Token: 0x06000019 RID: 25 RVA: 0x00003194 File Offset: 0x00001394
	private void UpdateWalking(float dt)
	{
		Vector3 moveDir = this.m_moveDir;
		bool flag = this.IsCrouching();
		this.m_running = this.CheckRun(moveDir, dt);
		float num = this.m_speed * this.GetJogSpeedFactor();
		if ((this.m_walk || this.InMinorAction()) && !flag)
		{
			num = this.m_walkSpeed;
		}
		else if (this.m_running)
		{
			bool flag2 = this.InWaterDepth() > 0.4f;
			float num2 = this.m_runSpeed * this.GetRunSpeedFactor();
			num = (flag2 ? Mathf.Lerp(num, num2, 0.33f) : num2);
			if (this.IsPlayer() && moveDir.magnitude > 0f)
			{
				moveDir.Normalize();
			}
		}
		else if (flag || this.IsEncumbered())
		{
			num = this.m_crouchSpeed;
		}
		num *= this.GetAttackSpeedFactorMovement();
		this.m_seman.ApplyStatusEffectSpeedMods(ref num);
		Vector3 vector = this.CanMove() ? (moveDir * num) : Vector3.zero;
		if (vector.magnitude > 0f && this.IsOnGround())
		{
			vector = Vector3.ProjectOnPlane(vector, this.m_lastGroundNormal).normalized * vector.magnitude;
		}
		float num3 = vector.magnitude;
		float magnitude = this.m_currentVel.magnitude;
		if (num3 > magnitude)
		{
			num3 = Mathf.MoveTowards(magnitude, num3, this.m_acceleration);
			vector = vector.normalized * num3;
		}
		else if (this.IsPlayer())
		{
			num3 = Mathf.MoveTowards(magnitude, num3, this.m_acceleration * 2f);
			vector = ((vector.magnitude > 0f) ? (vector.normalized * num3) : (this.m_currentVel.normalized * num3));
		}
		this.m_currentVel = Vector3.Lerp(this.m_currentVel, vector, 0.5f);
		Vector3 velocity = this.m_body.velocity;
		Vector3 currentVel = this.m_currentVel;
		currentVel.y = velocity.y;
		if (this.IsOnGround() && this.m_lastAttachBody == null)
		{
			this.ApplySlide(dt, ref currentVel, velocity, this.m_running);
		}
		this.ApplyRootMotion(ref currentVel);
		this.AddPushbackForce(ref currentVel);
		this.ApplyGroundForce(ref currentVel, vector);
		Vector3 vector2 = currentVel - velocity;
		if (!this.IsOnGround())
		{
			if (vector.magnitude > 0.1f)
			{
				vector2 *= this.m_airControl;
			}
			else
			{
				vector2 = Vector3.zero;
			}
		}
		if (this.IsAttached())
		{
			vector2 = Vector3.zero;
		}
		if (this.IsSneaking())
		{
			this.OnSneaking(dt);
		}
		if (vector2.magnitude > 20f)
		{
			vector2 = vector2.normalized * 20f;
		}
		if (vector2.magnitude > 0.01f)
		{
			this.m_body.AddForce(vector2, ForceMode.VelocityChange);
		}
		if (this.m_lastGroundBody && this.m_lastGroundBody.gameObject.layer != base.gameObject.layer && this.m_lastGroundBody.mass > this.m_body.mass)
		{
			float d = this.m_body.mass / this.m_lastGroundBody.mass;
			this.m_lastGroundBody.AddForceAtPosition(-vector2 * d, base.transform.position, ForceMode.VelocityChange);
		}
		float target = 0f;
		if ((moveDir.magnitude > 0.1f || this.AlwaysRotateCamera()) && !this.InDodge() && this.CanMove())
		{
			float turnSpeed = this.m_run ? this.m_runTurnSpeed : this.m_turnSpeed;
			this.m_seman.ApplyStatusEffectSpeedMods(ref turnSpeed);
			target = this.UpdateRotation(turnSpeed, dt);
		}
		this.UpdateEyeRotation();
		this.m_body.useGravity = true;
		float num4 = Vector3.Dot(this.m_currentVel, Vector3.ProjectOnPlane(base.transform.forward, this.m_lastGroundNormal).normalized);
		float value = Vector3.Dot(this.m_currentVel, Vector3.ProjectOnPlane(base.transform.right, this.m_lastGroundNormal).normalized);
		float num5 = Vector3.Dot(this.m_body.velocity, base.transform.forward);
		this.m_currentTurnVel = Mathf.SmoothDamp(this.m_currentTurnVel, target, ref this.m_currentTurnVelChange, 0.5f, 99f);
		this.m_zanim.SetFloat(Character.forward_speed, this.IsPlayer() ? num4 : num5);
		this.m_zanim.SetFloat(Character.sideway_speed, value);
		this.m_zanim.SetFloat(Character.turn_speed, this.m_currentTurnVel);
		this.m_zanim.SetBool(Character.inWater, false);
		this.m_zanim.SetBool(Character.onGround, this.IsOnGround());
		this.m_zanim.SetBool(Character.encumbered, this.IsEncumbered());
		this.m_zanim.SetBool(Character.flying, false);
		if (this.m_currentVel.magnitude > 0.1f)
		{
			if (this.m_running)
			{
				this.AddNoise(30f);
				return;
			}
			if (!flag)
			{
				this.AddNoise(15f);
			}
		}
	}

	// Token: 0x0600001A RID: 26 RVA: 0x000036A0 File Offset: 0x000018A0
	public bool IsSneaking()
	{
		return this.IsCrouching() && this.m_currentVel.magnitude > 0.1f && this.IsOnGround();
	}

	// Token: 0x0600001B RID: 27 RVA: 0x000036C4 File Offset: 0x000018C4
	private float GetSlopeAngle()
	{
		if (!this.IsOnGround())
		{
			return 0f;
		}
		float num = Vector3.SignedAngle(base.transform.forward, this.m_lastGroundNormal, base.transform.right);
		return -(90f - -num);
	}

	// Token: 0x0600001C RID: 28 RVA: 0x0000370C File Offset: 0x0000190C
	protected void AddPushbackForce(ref Vector3 velocity)
	{
		if (this.m_pushForce != Vector3.zero)
		{
			Vector3 normalized = this.m_pushForce.normalized;
			float num = Vector3.Dot(normalized, velocity);
			if (num < 10f)
			{
				velocity += normalized * (10f - num);
			}
			if (this.IsSwiming() || this.m_flying)
			{
				velocity *= 0.5f;
			}
		}
	}

	// Token: 0x0600001D RID: 29 RVA: 0x00003790 File Offset: 0x00001990
	private void ApplyPushback(HitData hit)
	{
		if (hit.m_pushForce != 0f)
		{
			float num = hit.m_pushForce * Mathf.Clamp01(1f + this.GetEquipmentMovementModifier() * 1.5f);
			float d = Mathf.Min(40f, num / this.m_body.mass * 5f);
			Vector3 pushForce = hit.m_dir * d;
			pushForce.y = 0f;
			if (this.m_pushForce.magnitude < pushForce.magnitude)
			{
				this.m_pushForce = pushForce;
			}
		}
	}

	// Token: 0x0600001E RID: 30 RVA: 0x0000381B File Offset: 0x00001A1B
	private void UpdatePushback(float dt)
	{
		this.m_pushForce = Vector3.MoveTowards(this.m_pushForce, Vector3.zero, 100f * dt);
	}

	// Token: 0x0600001F RID: 31 RVA: 0x0000383C File Offset: 0x00001A3C
	private void ApplyGroundForce(ref Vector3 vel, Vector3 targetVel)
	{
		Vector3 vector = Vector3.zero;
		if (this.IsOnGround() && this.m_lastGroundBody)
		{
			vector = this.m_lastGroundBody.GetPointVelocity(base.transform.position);
			vector.y = 0f;
		}
		Ship standingOnShip = this.GetStandingOnShip();
		if (standingOnShip != null)
		{
			if (targetVel.magnitude > 0.01f)
			{
				this.m_lastAttachBody = null;
			}
			else if (this.m_lastAttachBody != this.m_lastGroundBody)
			{
				this.m_lastAttachBody = this.m_lastGroundBody;
				this.m_lastAttachPos = this.m_lastAttachBody.transform.InverseTransformPoint(this.m_body.position);
			}
			if (this.m_lastAttachBody)
			{
				Vector3 vector2 = this.m_lastAttachBody.transform.TransformPoint(this.m_lastAttachPos);
				Vector3 a = vector2 - this.m_body.position;
				if (a.magnitude < 4f)
				{
					Vector3 position = vector2;
					position.y = this.m_body.position.y;
					if (standingOnShip.IsOwner())
					{
						a.y = 0f;
						vector += a * 10f;
					}
					else
					{
						this.m_body.position = position;
					}
				}
				else
				{
					this.m_lastAttachBody = null;
				}
			}
		}
		else
		{
			this.m_lastAttachBody = null;
		}
		vel += vector;
	}

	// Token: 0x06000020 RID: 32 RVA: 0x000039AC File Offset: 0x00001BAC
	private float UpdateRotation(float turnSpeed, float dt)
	{
		Quaternion quaternion = this.AlwaysRotateCamera() ? this.m_lookYaw : Quaternion.LookRotation(this.m_moveDir);
		float yawDeltaAngle = Utils.GetYawDeltaAngle(base.transform.rotation, quaternion);
		float num = 1f;
		if (!this.IsPlayer())
		{
			num = Mathf.Clamp01(Mathf.Abs(yawDeltaAngle) / 90f);
			num = Mathf.Pow(num, 0.5f);
		}
		float num2 = turnSpeed * this.GetAttackSpeedFactorRotation() * num;
		Quaternion rotation = Quaternion.RotateTowards(base.transform.rotation, quaternion, num2 * dt);
		if (Mathf.Abs(yawDeltaAngle) > 0.001f)
		{
			base.transform.rotation = rotation;
		}
		return num2 * Mathf.Sign(yawDeltaAngle) * 0.017453292f;
	}

	// Token: 0x06000021 RID: 33 RVA: 0x00003A5C File Offset: 0x00001C5C
	private void UpdateGroundTilt(float dt)
	{
		if (this.m_visual == null)
		{
			return;
		}
		if (this.m_nview.IsOwner())
		{
			if (this.m_groundTilt != Character.GroundTiltType.None)
			{
				if (!this.IsFlying() && this.IsOnGround())
				{
					Vector3 vector = this.m_lastGroundNormal;
					if (this.m_groundTilt == Character.GroundTiltType.PitchRaycast || this.m_groundTilt == Character.GroundTiltType.FullRaycast)
					{
						Vector3 p = base.transform.position + base.transform.forward * this.m_collider.radius;
						Vector3 p2 = base.transform.position - base.transform.forward * this.m_collider.radius;
						float num;
						Vector3 b;
						ZoneSystem.instance.GetSolidHeight(p, out num, out b);
						float num2;
						Vector3 b2;
						ZoneSystem.instance.GetSolidHeight(p2, out num2, out b2);
						vector = (vector + b + b2).normalized;
					}
					Vector3 vector2 = base.transform.InverseTransformVector(vector);
					vector2 = Vector3.RotateTowards(Vector3.up, vector2, 0.87266463f, 1f);
					this.m_groundTiltNormal = Vector3.Lerp(this.m_groundTiltNormal, vector2, 0.05f);
					Vector3 vector3;
					if (this.m_groundTilt == Character.GroundTiltType.Pitch || this.m_groundTilt == Character.GroundTiltType.PitchRaycast)
					{
						Vector3 b3 = Vector3.Project(this.m_groundTiltNormal, Vector3.right);
						vector3 = this.m_groundTiltNormal - b3;
					}
					else
					{
						vector3 = this.m_groundTiltNormal;
					}
					Vector3 forward = Vector3.Cross(vector3, Vector3.left);
					this.m_visual.transform.localRotation = Quaternion.LookRotation(forward, vector3);
				}
				else
				{
					this.m_visual.transform.localRotation = Quaternion.RotateTowards(this.m_visual.transform.localRotation, Quaternion.identity, dt * 200f);
				}
				this.m_nview.GetZDO().Set("tiltrot", this.m_visual.transform.localRotation);
				return;
			}
			if (this.CanWallRun())
			{
				if (this.m_wallRunning)
				{
					Vector3 vector4 = Vector3.Lerp(Vector3.up, this.m_lastGroundNormal, 0.65f);
					Vector3 forward2 = Vector3.ProjectOnPlane(base.transform.forward, vector4);
					forward2.Normalize();
					Quaternion to = Quaternion.LookRotation(forward2, vector4);
					this.m_visual.transform.rotation = Quaternion.RotateTowards(this.m_visual.transform.rotation, to, 30f * dt);
				}
				else
				{
					this.m_visual.transform.localRotation = Quaternion.RotateTowards(this.m_visual.transform.localRotation, Quaternion.identity, 100f * dt);
				}
				this.m_nview.GetZDO().Set("tiltrot", this.m_visual.transform.localRotation);
				return;
			}
		}
		else if (this.m_groundTilt != Character.GroundTiltType.None || this.CanWallRun())
		{
			Quaternion quaternion = this.m_nview.GetZDO().GetQuaternion("tiltrot", Quaternion.identity);
			this.m_visual.transform.localRotation = quaternion;
		}
	}

	// Token: 0x06000022 RID: 34 RVA: 0x00003D60 File Offset: 0x00001F60
	public bool IsWallRunning()
	{
		return this.m_wallRunning;
	}

	// Token: 0x06000023 RID: 35 RVA: 0x000023E2 File Offset: 0x000005E2
	private bool IsOnSnow()
	{
		return false;
	}

	// Token: 0x06000024 RID: 36 RVA: 0x00003D68 File Offset: 0x00001F68
	public void Heal(float hp, bool showText = true)
	{
		if (hp <= 0f)
		{
			return;
		}
		if (this.m_nview.IsOwner())
		{
			this.RPC_Heal(0L, hp, showText);
			return;
		}
		this.m_nview.InvokeRPC("Heal", new object[]
		{
			hp,
			showText
		});
	}

	// Token: 0x06000025 RID: 37 RVA: 0x00003DC0 File Offset: 0x00001FC0
	private void RPC_Heal(long sender, float hp, bool showText)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		float health = this.GetHealth();
		if (health <= 0f || this.IsDead())
		{
			return;
		}
		float num = Mathf.Min(health + hp, this.GetMaxHealth());
		if (num > health)
		{
			this.SetHealth(num);
			if (showText)
			{
				Vector3 topPoint = this.GetTopPoint();
				DamageText.instance.ShowText(DamageText.TextType.Heal, topPoint, hp, this.IsPlayer());
			}
		}
	}

	// Token: 0x06000026 RID: 38 RVA: 0x00003E2C File Offset: 0x0000202C
	public Vector3 GetTopPoint()
	{
		Vector3 center = this.m_collider.bounds.center;
		center.y = this.m_collider.bounds.max.y;
		return center;
	}

	// Token: 0x06000027 RID: 39 RVA: 0x00003E6D File Offset: 0x0000206D
	public float GetRadius()
	{
		return this.m_collider.radius;
	}

	// Token: 0x06000028 RID: 40 RVA: 0x00003E7A File Offset: 0x0000207A
	public Vector3 GetHeadPoint()
	{
		return this.m_head.position;
	}

	// Token: 0x06000029 RID: 41 RVA: 0x00003E87 File Offset: 0x00002087
	public Vector3 GetEyePoint()
	{
		return this.m_eye.position;
	}

	// Token: 0x0600002A RID: 42 RVA: 0x00003E94 File Offset: 0x00002094
	public Vector3 GetCenterPoint()
	{
		return this.m_collider.bounds.center;
	}

	// Token: 0x0600002B RID: 43 RVA: 0x00003EB4 File Offset: 0x000020B4
	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Character;
	}

	// Token: 0x0600002C RID: 44 RVA: 0x00003EB7 File Offset: 0x000020B7
	public void Damage(HitData hit)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.m_nview.InvokeRPC("Damage", new object[]
		{
			hit
		});
	}

	// Token: 0x0600002D RID: 45 RVA: 0x00003EE4 File Offset: 0x000020E4
	private void RPC_Damage(long sender, HitData hit)
	{
		if (this.IsDebugFlying())
		{
			return;
		}
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.GetHealth() <= 0f || this.IsDead() || this.IsTeleporting() || this.InCutscene())
		{
			return;
		}
		if (hit.m_dodgeable && this.IsDodgeInvincible())
		{
			return;
		}
		Character attacker = hit.GetAttacker();
		if (hit.HaveAttacker() && attacker == null)
		{
			return;
		}
		if (this.IsPlayer() && !this.IsPVPEnabled() && attacker != null && attacker.IsPlayer())
		{
			return;
		}
		if (attacker != null && !attacker.IsPlayer())
		{
			float difficultyDamageScale = Game.instance.GetDifficultyDamageScale(base.transform.position);
			hit.ApplyModifier(difficultyDamageScale);
		}
		this.m_seman.OnDamaged(hit, attacker);
		if (this.m_baseAI != null && !this.m_baseAI.IsAlerted() && hit.m_backstabBonus > 1f && Time.time - this.m_backstabTime > 300f)
		{
			this.m_backstabTime = Time.time;
			hit.ApplyModifier(hit.m_backstabBonus);
			this.m_backstabHitEffects.Create(hit.m_point, Quaternion.identity, base.transform, 1f);
		}
		if (this.IsStaggering() && !this.IsPlayer())
		{
			hit.ApplyModifier(2f);
			this.m_critHitEffects.Create(hit.m_point, Quaternion.identity, base.transform, 1f);
		}
		if (hit.m_blockable && this.IsBlocking())
		{
			this.BlockAttack(hit, attacker);
		}
		this.ApplyPushback(hit);
		if (!string.IsNullOrEmpty(hit.m_statusEffect))
		{
			StatusEffect statusEffect = this.m_seman.GetStatusEffect(hit.m_statusEffect);
			if (statusEffect == null)
			{
				statusEffect = this.m_seman.AddStatusEffect(hit.m_statusEffect, false);
			}
			if (statusEffect != null && attacker != null)
			{
				statusEffect.SetAttacker(attacker);
			}
		}
		HitData.DamageModifiers damageModifiers = this.GetDamageModifiers();
		HitData.DamageModifier mod;
		hit.ApplyResistance(damageModifiers, out mod);
		if (this.IsPlayer())
		{
			float bodyArmor = this.GetBodyArmor();
			hit.ApplyArmor(bodyArmor);
			this.DamageArmorDurability(hit);
		}
		float poison = hit.m_damage.m_poison;
		float fire = hit.m_damage.m_fire;
		float spirit = hit.m_damage.m_spirit;
		hit.m_damage.m_poison = 0f;
		hit.m_damage.m_fire = 0f;
		hit.m_damage.m_spirit = 0f;
		this.ApplyDamage(hit, true, true, mod);
		this.AddFireDamage(fire);
		this.AddSpiritDamage(spirit);
		this.AddPoisonDamage(poison);
		this.AddFrostDamage(hit.m_damage.m_frost);
		this.AddLightningDamage(hit.m_damage.m_lightning);
	}

	// Token: 0x0600002E RID: 46 RVA: 0x000041AC File Offset: 0x000023AC
	protected HitData.DamageModifier GetDamageModifier(HitData.DamageType damageType)
	{
		return this.GetDamageModifiers().GetModifier(damageType);
	}

	// Token: 0x0600002F RID: 47 RVA: 0x000041C8 File Offset: 0x000023C8
	protected HitData.DamageModifiers GetDamageModifiers()
	{
		HitData.DamageModifiers result = this.m_damageModifiers.Clone();
		this.ApplyArmorDamageMods(ref result);
		this.m_seman.ApplyDamageMods(ref result);
		return result;
	}

	// Token: 0x06000030 RID: 48 RVA: 0x000041F8 File Offset: 0x000023F8
	public void ApplyDamage(HitData hit, bool showDamageText, bool triggerEffects, HitData.DamageModifier mod = HitData.DamageModifier.Normal)
	{
		if (this.IsDebugFlying() || this.IsDead() || this.IsTeleporting() || this.InCutscene())
		{
			return;
		}
		float totalDamage = hit.GetTotalDamage();
		if (showDamageText && (totalDamage > 0f || !this.IsPlayer()))
		{
			DamageText.instance.ShowText(mod, hit.m_point, totalDamage, this.IsPlayer());
		}
		if (totalDamage <= 0f)
		{
			return;
		}
		if (!this.InGodMode() && !this.InGhostMode())
		{
			float num = this.GetHealth();
			num -= totalDamage;
			this.SetHealth(num);
		}
		float totalPhysicalDamage = hit.m_damage.GetTotalPhysicalDamage();
		this.AddStaggerDamage(totalPhysicalDamage * hit.m_staggerMultiplier, hit.m_dir);
		if (triggerEffects && totalDamage > 2f)
		{
			this.DoDamageCameraShake(hit);
			if (hit.m_damage.GetTotalPhysicalDamage() > 0f)
			{
				this.m_hitEffects.Create(hit.m_point, Quaternion.identity, base.transform, 1f);
			}
		}
		this.OnDamaged(hit);
		if (this.m_onDamaged != null)
		{
			this.m_onDamaged(totalDamage, hit.GetAttacker());
		}
		if (Character.m_dpsDebugEnabled)
		{
			Character.AddDPS(totalDamage, this);
		}
	}

	// Token: 0x06000031 RID: 49 RVA: 0x000027E0 File Offset: 0x000009E0
	protected virtual void DoDamageCameraShake(HitData hit)
	{
	}

	// Token: 0x06000032 RID: 50 RVA: 0x000027E0 File Offset: 0x000009E0
	protected virtual void DamageArmorDurability(HitData hit)
	{
	}

	// Token: 0x06000033 RID: 51 RVA: 0x0000431C File Offset: 0x0000251C
	private void AddFireDamage(float damage)
	{
		if (damage <= 0f)
		{
			return;
		}
		SE_Burning se_Burning = this.m_seman.GetStatusEffect("Burning") as SE_Burning;
		if (se_Burning == null)
		{
			se_Burning = (this.m_seman.AddStatusEffect("Burning", false) as SE_Burning);
		}
		se_Burning.AddFireDamage(damage);
	}

	// Token: 0x06000034 RID: 52 RVA: 0x00004370 File Offset: 0x00002570
	private void AddSpiritDamage(float damage)
	{
		if (damage <= 0f)
		{
			return;
		}
		SE_Burning se_Burning = this.m_seman.GetStatusEffect("Spirit") as SE_Burning;
		if (se_Burning == null)
		{
			se_Burning = (this.m_seman.AddStatusEffect("Spirit", false) as SE_Burning);
		}
		se_Burning.AddSpiritDamage(damage);
	}

	// Token: 0x06000035 RID: 53 RVA: 0x000043C4 File Offset: 0x000025C4
	private void AddPoisonDamage(float damage)
	{
		if (damage <= 0f)
		{
			return;
		}
		SE_Poison se_Poison = this.m_seman.GetStatusEffect("Poison") as SE_Poison;
		if (se_Poison == null)
		{
			se_Poison = (this.m_seman.AddStatusEffect("Poison", false) as SE_Poison);
		}
		se_Poison.AddDamage(damage);
	}

	// Token: 0x06000036 RID: 54 RVA: 0x00004418 File Offset: 0x00002618
	private void AddFrostDamage(float damage)
	{
		if (damage <= 0f)
		{
			return;
		}
		SE_Frost se_Frost = this.m_seman.GetStatusEffect("Frost") as SE_Frost;
		if (se_Frost == null)
		{
			se_Frost = (this.m_seman.AddStatusEffect("Frost", false) as SE_Frost);
		}
		se_Frost.AddDamage(damage);
	}

	// Token: 0x06000037 RID: 55 RVA: 0x0000446B File Offset: 0x0000266B
	private void AddLightningDamage(float damage)
	{
		if (damage <= 0f)
		{
			return;
		}
		this.m_seman.AddStatusEffect("Lightning", true);
	}

	// Token: 0x06000038 RID: 56 RVA: 0x00004488 File Offset: 0x00002688
	private void AddStaggerDamage(float damage, Vector3 forceDirection)
	{
		if (this.m_staggerDamageFactor <= 0f && !this.IsPlayer())
		{
			return;
		}
		this.m_staggerDamage += damage;
		this.m_staggerTimer = 0f;
		float maxHealth = this.GetMaxHealth();
		float num = this.IsPlayer() ? (maxHealth / 2f) : (maxHealth * this.m_staggerDamageFactor);
		if (this.m_staggerDamage >= num)
		{
			this.m_staggerDamage = 0f;
			this.Stagger(forceDirection);
		}
	}

	// Token: 0x06000039 RID: 57 RVA: 0x00004500 File Offset: 0x00002700
	private static void AddDPS(float damage, Character me)
	{
		if (me == Player.m_localPlayer)
		{
			Character.CalculateDPS("To-you ", Character.m_playerDamage, damage);
			return;
		}
		Character.CalculateDPS("To-others ", Character.m_enemyDamage, damage);
	}

	// Token: 0x0600003A RID: 58 RVA: 0x00004530 File Offset: 0x00002730
	private static void CalculateDPS(string name, List<KeyValuePair<float, float>> damages, float damage)
	{
		float time = Time.time;
		if (damages.Count > 0 && Time.time - damages[damages.Count - 1].Key > 5f)
		{
			damages.Clear();
		}
		damages.Add(new KeyValuePair<float, float>(time, damage));
		float num = Time.time - damages[0].Key;
		if (num < 0.01f)
		{
			return;
		}
		float num2 = 0f;
		foreach (KeyValuePair<float, float> keyValuePair in damages)
		{
			num2 += keyValuePair.Value;
		}
		float num3 = num2 / num;
		string text = string.Concat(new object[]
		{
			"DPS ",
			name,
			" (",
			damages.Count,
			" attacks): ",
			num3.ToString("0.0")
		});
		ZLog.Log(text);
		MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, text, 0, null);
	}

	// Token: 0x0600003B RID: 59 RVA: 0x00004650 File Offset: 0x00002850
	private void UpdateStagger(float dt)
	{
		if (this.m_staggerDamageFactor <= 0f && !this.IsPlayer())
		{
			return;
		}
		this.m_staggerTimer += dt;
		if (this.m_staggerTimer > 3f)
		{
			this.m_staggerDamage = 0f;
		}
	}

	// Token: 0x0600003C RID: 60 RVA: 0x0000468E File Offset: 0x0000288E
	public void Stagger(Vector3 forceDirection)
	{
		if (this.m_nview.IsOwner())
		{
			this.RPC_Stagger(0L, forceDirection);
			return;
		}
		this.m_nview.InvokeRPC("Stagger", new object[]
		{
			forceDirection
		});
	}

	// Token: 0x0600003D RID: 61 RVA: 0x000046C8 File Offset: 0x000028C8
	private void RPC_Stagger(long sender, Vector3 forceDirection)
	{
		if (!this.IsStaggering())
		{
			if (forceDirection.magnitude > 0.01f)
			{
				forceDirection.y = 0f;
				base.transform.rotation = Quaternion.LookRotation(-forceDirection);
			}
			this.m_zanim.SetTrigger("stagger");
		}
	}

	// Token: 0x0600003E RID: 62 RVA: 0x000027E0 File Offset: 0x000009E0
	protected virtual void ApplyArmorDamageMods(ref HitData.DamageModifiers mods)
	{
	}

	// Token: 0x0600003F RID: 63 RVA: 0x0000471D File Offset: 0x0000291D
	public virtual float GetBodyArmor()
	{
		return 0f;
	}

	// Token: 0x06000040 RID: 64 RVA: 0x000023E2 File Offset: 0x000005E2
	protected virtual bool BlockAttack(HitData hit, Character attacker)
	{
		return false;
	}

	// Token: 0x06000041 RID: 65 RVA: 0x000027E0 File Offset: 0x000009E0
	protected virtual void OnDamaged(HitData hit)
	{
	}

	// Token: 0x06000042 RID: 66 RVA: 0x00004724 File Offset: 0x00002924
	private void OnCollisionStay(Collision collision)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_jumpTimer < 0.1f)
		{
			return;
		}
		foreach (ContactPoint contactPoint in collision.contacts)
		{
			float num = contactPoint.point.y - base.transform.position.y;
			if (contactPoint.normal.y > 0.1f && num < this.m_collider.radius)
			{
				if (contactPoint.normal.y > this.m_groundContactNormal.y || !this.m_groundContact)
				{
					this.m_groundContact = true;
					this.m_groundContactNormal = contactPoint.normal;
					this.m_groundContactPoint = contactPoint.point;
					this.m_lowestContactCollider = collision.collider;
				}
				else
				{
					Vector3 vector = Vector3.Normalize(this.m_groundContactNormal + contactPoint.normal);
					if (vector.y > this.m_groundContactNormal.y)
					{
						this.m_groundContactNormal = vector;
						this.m_groundContactPoint = (this.m_groundContactPoint + contactPoint.point) * 0.5f;
					}
				}
			}
		}
	}

	// Token: 0x06000043 RID: 67 RVA: 0x0000486C File Offset: 0x00002A6C
	private void UpdateGroundContact(float dt)
	{
		if (!this.m_groundContact)
		{
			return;
		}
		this.m_lastGroundCollider = this.m_lowestContactCollider;
		this.m_lastGroundNormal = this.m_groundContactNormal;
		this.m_lastGroundPoint = this.m_groundContactPoint;
		this.m_lastGroundBody = (this.m_lastGroundCollider ? this.m_lastGroundCollider.attachedRigidbody : null);
		if (!this.IsPlayer() && this.m_lastGroundBody != null && this.m_lastGroundBody.gameObject.layer == base.gameObject.layer)
		{
			this.m_lastGroundCollider = null;
			this.m_lastGroundBody = null;
		}
		float num = Mathf.Max(0f, this.m_maxAirAltitude - base.transform.position.y);
		if (num > 0.8f)
		{
			if (this.m_onLand != null)
			{
				Vector3 lastGroundPoint = this.m_lastGroundPoint;
				if (this.InWater())
				{
					lastGroundPoint.y = this.m_waterLevel;
				}
				this.m_onLand(this.m_lastGroundPoint);
			}
			this.ResetCloth();
		}
		if (this.IsPlayer() && num > 4f)
		{
			HitData hitData = new HitData();
			hitData.m_damage.m_damage = Mathf.Clamp01((num - 4f) / 16f) * 100f;
			hitData.m_point = this.m_lastGroundPoint;
			hitData.m_dir = this.m_lastGroundNormal;
			this.Damage(hitData);
		}
		this.ResetGroundContact();
		this.m_lastGroundTouch = 0f;
		this.m_maxAirAltitude = base.transform.position.y;
	}

	// Token: 0x06000044 RID: 68 RVA: 0x000049EC File Offset: 0x00002BEC
	private void ResetGroundContact()
	{
		this.m_lowestContactCollider = null;
		this.m_groundContact = false;
		this.m_groundContactNormal = Vector3.zero;
		this.m_groundContactPoint = Vector3.zero;
	}

	// Token: 0x06000045 RID: 69 RVA: 0x00004A12 File Offset: 0x00002C12
	public Ship GetStandingOnShip()
	{
		if (!this.IsOnGround())
		{
			return null;
		}
		if (this.m_lastGroundBody)
		{
			return this.m_lastGroundBody.GetComponent<Ship>();
		}
		return null;
	}

	// Token: 0x06000046 RID: 70 RVA: 0x00004A38 File Offset: 0x00002C38
	public bool IsOnGround()
	{
		return this.m_lastGroundTouch < 0.2f || this.m_body.IsSleeping();
	}

	// Token: 0x06000047 RID: 71 RVA: 0x00004A54 File Offset: 0x00002C54
	private void CheckDeath()
	{
		if (this.IsDead())
		{
			return;
		}
		if (this.GetHealth() <= 0f)
		{
			this.OnDeath();
			if (this.m_onDeath != null)
			{
				this.m_onDeath();
			}
		}
	}

	// Token: 0x06000048 RID: 72 RVA: 0x000027E0 File Offset: 0x000009E0
	protected virtual void OnRagdollCreated(Ragdoll ragdoll)
	{
	}

	// Token: 0x06000049 RID: 73 RVA: 0x00004A88 File Offset: 0x00002C88
	protected virtual void OnDeath()
	{
		GameObject[] array = this.m_deathEffects.Create(base.transform.position, base.transform.rotation, base.transform, 1f);
		for (int i = 0; i < array.Length; i++)
		{
			Ragdoll component = array[i].GetComponent<Ragdoll>();
			if (component)
			{
				CharacterDrop component2 = base.GetComponent<CharacterDrop>();
				LevelEffects componentInChildren = base.GetComponentInChildren<LevelEffects>();
				Vector3 velocity = this.m_body.velocity;
				if (this.m_pushForce.magnitude * 0.5f > velocity.magnitude)
				{
					velocity = this.m_pushForce * 0.5f;
				}
				float hue = 0f;
				float saturation = 0f;
				float value = 0f;
				if (componentInChildren)
				{
					componentInChildren.GetColorChanges(out hue, out saturation, out value);
				}
				component.Setup(velocity, hue, saturation, value, component2);
				this.OnRagdollCreated(component);
				if (component2)
				{
					component2.SetDropsEnabled(false);
				}
			}
		}
		if (!string.IsNullOrEmpty(this.m_defeatSetGlobalKey))
		{
			ZoneSystem.instance.SetGlobalKey(this.m_defeatSetGlobalKey);
		}
		ZNetScene.instance.Destroy(base.gameObject);
		Gogan.LogEvent("Game", "Killed", this.m_name, 0L);
	}

	// Token: 0x0600004A RID: 74 RVA: 0x00004BC8 File Offset: 0x00002DC8
	public float GetHealth()
	{
		ZDO zdo = this.m_nview.GetZDO();
		if (zdo == null)
		{
			return this.GetMaxHealth();
		}
		return zdo.GetFloat("health", this.GetMaxHealth());
	}

	// Token: 0x0600004B RID: 75 RVA: 0x00004BFC File Offset: 0x00002DFC
	public void SetHealth(float health)
	{
		ZDO zdo = this.m_nview.GetZDO();
		if (zdo == null || !this.m_nview.IsOwner())
		{
			return;
		}
		if (health < 0f)
		{
			health = 0f;
		}
		zdo.Set("health", health);
	}

	// Token: 0x0600004C RID: 76 RVA: 0x00004C41 File Offset: 0x00002E41
	public float GetHealthPercentage()
	{
		return this.GetHealth() / this.GetMaxHealth();
	}

	// Token: 0x0600004D RID: 77 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool IsDead()
	{
		return false;
	}

	// Token: 0x0600004E RID: 78 RVA: 0x00004C50 File Offset: 0x00002E50
	public void SetMaxHealth(float health)
	{
		if (this.m_nview.GetZDO() != null)
		{
			this.m_nview.GetZDO().Set("max_health", health);
		}
		if (this.GetHealth() > health)
		{
			this.SetHealth(health);
		}
	}

	// Token: 0x0600004F RID: 79 RVA: 0x00004C85 File Offset: 0x00002E85
	public float GetMaxHealth()
	{
		if (this.m_nview.GetZDO() != null)
		{
			return this.m_nview.GetZDO().GetFloat("max_health", this.m_health);
		}
		return this.m_health;
	}

	// Token: 0x06000050 RID: 80 RVA: 0x0000471D File Offset: 0x0000291D
	public virtual float GetMaxStamina()
	{
		return 0f;
	}

	// Token: 0x06000051 RID: 81 RVA: 0x00004CB6 File Offset: 0x00002EB6
	public virtual float GetStaminaPercentage()
	{
		return 1f;
	}

	// Token: 0x06000052 RID: 82 RVA: 0x00004CBD File Offset: 0x00002EBD
	public bool IsBoss()
	{
		return this.m_boss;
	}

	// Token: 0x06000053 RID: 83 RVA: 0x00004CC8 File Offset: 0x00002EC8
	public void SetLookDir(Vector3 dir)
	{
		if (dir.magnitude <= Mathf.Epsilon)
		{
			dir = base.transform.forward;
		}
		else
		{
			dir.Normalize();
		}
		this.m_lookDir = dir;
		dir.y = 0f;
		this.m_lookYaw = Quaternion.LookRotation(dir);
	}

	// Token: 0x06000054 RID: 84 RVA: 0x00004D18 File Offset: 0x00002F18
	public Vector3 GetLookDir()
	{
		return this.m_eye.forward;
	}

	// Token: 0x06000055 RID: 85 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void OnAttackTrigger()
	{
	}

	// Token: 0x06000056 RID: 86 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void OnStopMoving()
	{
	}

	// Token: 0x06000057 RID: 87 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void OnWeaponTrailStart()
	{
	}

	// Token: 0x06000058 RID: 88 RVA: 0x00004D25 File Offset: 0x00002F25
	public void SetMoveDir(Vector3 dir)
	{
		this.m_moveDir = dir;
	}

	// Token: 0x06000059 RID: 89 RVA: 0x00004D2E File Offset: 0x00002F2E
	public void SetRun(bool run)
	{
		this.m_run = run;
	}

	// Token: 0x0600005A RID: 90 RVA: 0x00004D37 File Offset: 0x00002F37
	public void SetWalk(bool walk)
	{
		this.m_walk = walk;
	}

	// Token: 0x0600005B RID: 91 RVA: 0x00004D40 File Offset: 0x00002F40
	public bool GetWalk()
	{
		return this.m_walk;
	}

	// Token: 0x0600005C RID: 92 RVA: 0x00004D48 File Offset: 0x00002F48
	protected virtual void UpdateEyeRotation()
	{
		this.m_eye.rotation = Quaternion.LookRotation(this.m_lookDir);
	}

	// Token: 0x0600005D RID: 93 RVA: 0x00004D60 File Offset: 0x00002F60
	public void OnAutoJump(Vector3 dir, float upVel, float forwardVel)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.IsOnGround() || this.IsDead() || this.InAttack() || this.InDodge() || this.IsKnockedBack())
		{
			return;
		}
		if (Time.time - this.m_lastAutoJumpTime < 0.5f)
		{
			return;
		}
		this.m_lastAutoJumpTime = Time.time;
		if (Vector3.Dot(this.m_moveDir, dir) < 0.5f)
		{
			return;
		}
		Vector3 vector = Vector3.zero;
		vector.y = upVel;
		vector += dir * forwardVel;
		this.m_body.velocity = vector;
		this.m_lastGroundTouch = 1f;
		this.m_jumpTimer = 0f;
		this.m_jumpEffects.Create(base.transform.position, base.transform.rotation, base.transform, 1f);
		this.SetCrouch(false);
		this.UpdateBodyFriction();
	}

	// Token: 0x0600005E RID: 94 RVA: 0x00004E5C File Offset: 0x0000305C
	public void Jump()
	{
		if (this.IsOnGround() && !this.IsDead() && !this.InAttack() && !this.IsEncumbered() && !this.InDodge() && !this.IsKnockedBack())
		{
			bool flag = false;
			if (!this.HaveStamina(this.m_jumpStaminaUsage))
			{
				if (this.IsPlayer())
				{
					Hud.instance.StaminaBarNoStaminaFlash();
				}
				flag = true;
			}
			float num = 0f;
			Skills skills = this.GetSkills();
			if (skills != null)
			{
				num = skills.GetSkillFactor(Skills.SkillType.Jump);
				if (!flag)
				{
					this.RaiseSkill(Skills.SkillType.Jump, 1f);
				}
			}
			Vector3 vector = this.m_body.velocity;
			Mathf.Acos(Mathf.Clamp01(this.m_lastGroundNormal.y));
			Vector3 normalized = (this.m_lastGroundNormal + Vector3.up).normalized;
			float num2 = 1f + num * 0.4f;
			float num3 = this.m_jumpForce * num2;
			float num4 = Vector3.Dot(normalized, vector);
			if (num4 < num3)
			{
				vector += normalized * (num3 - num4);
			}
			vector += this.m_moveDir * this.m_jumpForceForward * num2;
			if (flag)
			{
				vector *= this.m_jumpForceTiredFactor;
			}
			this.m_body.WakeUp();
			this.m_body.velocity = vector;
			this.ResetGroundContact();
			this.m_lastGroundTouch = 1f;
			this.m_jumpTimer = 0f;
			this.m_zanim.SetTrigger("jump");
			this.AddNoise(30f);
			this.m_jumpEffects.Create(base.transform.position, base.transform.rotation, base.transform, 1f);
			this.OnJump();
			this.SetCrouch(false);
			this.UpdateBodyFriction();
		}
	}

	// Token: 0x0600005F RID: 95 RVA: 0x00005038 File Offset: 0x00003238
	private void UpdateBodyFriction()
	{
		this.m_collider.material.frictionCombine = PhysicMaterialCombine.Multiply;
		if (this.IsDead())
		{
			this.m_collider.material.staticFriction = 1f;
			this.m_collider.material.dynamicFriction = 1f;
			this.m_collider.material.frictionCombine = PhysicMaterialCombine.Maximum;
			return;
		}
		if (this.IsSwiming())
		{
			this.m_collider.material.staticFriction = 0.2f;
			this.m_collider.material.dynamicFriction = 0.2f;
			return;
		}
		if (!this.IsOnGround())
		{
			this.m_collider.material.staticFriction = 0f;
			this.m_collider.material.dynamicFriction = 0f;
			return;
		}
		if (this.IsFlying())
		{
			this.m_collider.material.staticFriction = 0f;
			this.m_collider.material.dynamicFriction = 0f;
			return;
		}
		if (this.m_moveDir.magnitude < 0.1f)
		{
			this.m_collider.material.staticFriction = 0.8f * (1f - this.m_slippage);
			this.m_collider.material.dynamicFriction = 0.8f * (1f - this.m_slippage);
			this.m_collider.material.frictionCombine = PhysicMaterialCombine.Maximum;
			return;
		}
		this.m_collider.material.staticFriction = 0.4f * (1f - this.m_slippage);
		this.m_collider.material.dynamicFriction = 0.4f * (1f - this.m_slippage);
	}

	// Token: 0x06000060 RID: 96 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool StartAttack(Character target, bool charge)
	{
		return false;
	}

	// Token: 0x06000061 RID: 97 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void OnNearFire(Vector3 point)
	{
	}

	// Token: 0x06000062 RID: 98 RVA: 0x000051DF File Offset: 0x000033DF
	public ZDOID GetZDOID()
	{
		if (this.m_nview.IsValid())
		{
			return this.m_nview.GetZDO().m_uid;
		}
		return ZDOID.None;
	}

	// Token: 0x06000063 RID: 99 RVA: 0x00005204 File Offset: 0x00003404
	public bool IsOwner()
	{
		return this.m_nview.IsValid() && this.m_nview.IsOwner();
	}

	// Token: 0x06000064 RID: 100 RVA: 0x00005220 File Offset: 0x00003420
	public long GetOwner()
	{
		if (this.m_nview.IsValid())
		{
			return this.m_nview.GetZDO().m_owner;
		}
		return 0L;
	}

	// Token: 0x06000065 RID: 101 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool UseMeleeCamera()
	{
		return false;
	}

	// Token: 0x06000066 RID: 102 RVA: 0x000027E2 File Offset: 0x000009E2
	public virtual bool AlwaysRotateCamera()
	{
		return true;
	}

	// Token: 0x06000067 RID: 103 RVA: 0x00005242 File Offset: 0x00003442
	public void SetInWater(float depth)
	{
		this.m_waterLevel = depth;
	}

	// Token: 0x06000068 RID: 104 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool IsPVPEnabled()
	{
		return false;
	}

	// Token: 0x06000069 RID: 105 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool InIntro()
	{
		return false;
	}

	// Token: 0x0600006A RID: 106 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool InCutscene()
	{
		return false;
	}

	// Token: 0x0600006B RID: 107 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool IsCrouching()
	{
		return false;
	}

	// Token: 0x0600006C RID: 108 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool InBed()
	{
		return false;
	}

	// Token: 0x0600006D RID: 109 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool IsAttached()
	{
		return false;
	}

	// Token: 0x0600006E RID: 110 RVA: 0x000027E0 File Offset: 0x000009E0
	protected virtual void SetCrouch(bool crouch)
	{
	}

	// Token: 0x0600006F RID: 111 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void AttachStart(Transform attachPoint, bool hideWeapons, bool isBed, string attachAnimation, Vector3 detachOffset)
	{
	}

	// Token: 0x06000070 RID: 112 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void AttachStop()
	{
	}

	// Token: 0x06000071 RID: 113 RVA: 0x0000524C File Offset: 0x0000344C
	private void UpdateWater(float dt)
	{
		this.m_swimTimer += dt;
		if (this.InWaterSwimDepth())
		{
			if (this.m_nview.IsOwner())
			{
				this.m_seman.AddStatusEffect("Wet", true);
			}
			if (this.m_canSwim)
			{
				this.m_swimTimer = 0f;
			}
		}
	}

	// Token: 0x06000072 RID: 114 RVA: 0x000052A1 File Offset: 0x000034A1
	public bool IsSwiming()
	{
		return this.m_swimTimer < 0.5f;
	}

	// Token: 0x06000073 RID: 115 RVA: 0x000052B0 File Offset: 0x000034B0
	public bool InWaterSwimDepth()
	{
		return this.InWaterDepth() > Mathf.Max(0f, this.m_swimDepth - 0.4f);
	}

	// Token: 0x06000074 RID: 116 RVA: 0x000052D0 File Offset: 0x000034D0
	private float InWaterDepth()
	{
		if (this.GetStandingOnShip() != null)
		{
			return 0f;
		}
		return Mathf.Max(0f, this.m_waterLevel - base.transform.position.y);
	}

	// Token: 0x06000075 RID: 117 RVA: 0x00005307 File Offset: 0x00003507
	public bool InWater()
	{
		return this.InWaterDepth() > 0f;
	}

	// Token: 0x06000076 RID: 118 RVA: 0x00005316 File Offset: 0x00003516
	protected virtual bool CheckRun(Vector3 moveDir, float dt)
	{
		return this.m_run && moveDir.magnitude >= 0.1f && !this.IsCrouching() && !this.IsEncumbered() && !this.InDodge();
	}

	// Token: 0x06000077 RID: 119 RVA: 0x0000534F File Offset: 0x0000354F
	public bool IsRunning()
	{
		return this.m_running;
	}

	// Token: 0x06000078 RID: 120 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool InPlaceMode()
	{
		return false;
	}

	// Token: 0x06000079 RID: 121 RVA: 0x000027E2 File Offset: 0x000009E2
	public virtual bool HaveStamina(float amount = 0f)
	{
		return true;
	}

	// Token: 0x0600007A RID: 122 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void AddStamina(float v)
	{
	}

	// Token: 0x0600007B RID: 123 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void UseStamina(float stamina)
	{
	}

	// Token: 0x0600007C RID: 124 RVA: 0x00005358 File Offset: 0x00003558
	public bool IsStaggering()
	{
		return this.m_animator.GetCurrentAnimatorStateInfo(0).tagHash == Character.m_animatorTagStagger;
	}

	// Token: 0x0600007D RID: 125 RVA: 0x00005380 File Offset: 0x00003580
	public virtual bool CanMove()
	{
		AnimatorStateInfo animatorStateInfo = this.m_animator.IsInTransition(0) ? this.m_animator.GetNextAnimatorStateInfo(0) : this.m_animator.GetCurrentAnimatorStateInfo(0);
		return animatorStateInfo.tagHash != Character.m_animatorTagFreeze && animatorStateInfo.tagHash != Character.m_animatorTagStagger && animatorStateInfo.tagHash != Character.m_animatorTagSitting;
	}

	// Token: 0x0600007E RID: 126 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool IsEncumbered()
	{
		return false;
	}

	// Token: 0x0600007F RID: 127 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool IsTeleporting()
	{
		return false;
	}

	// Token: 0x06000080 RID: 128 RVA: 0x000053E3 File Offset: 0x000035E3
	private bool CanWallRun()
	{
		return this.IsPlayer();
	}

	// Token: 0x06000081 RID: 129 RVA: 0x000053EB File Offset: 0x000035EB
	public void ShowPickupMessage(ItemDrop.ItemData item, int amount)
	{
		this.Message(MessageHud.MessageType.TopLeft, "$msg_added " + item.m_shared.m_name, amount, item.GetIcon());
	}

	// Token: 0x06000082 RID: 130 RVA: 0x00005410 File Offset: 0x00003610
	public void ShowRemovedMessage(ItemDrop.ItemData item, int amount)
	{
		this.Message(MessageHud.MessageType.TopLeft, "$msg_removed " + item.m_shared.m_name, amount, item.GetIcon());
	}

	// Token: 0x06000083 RID: 131 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void Message(MessageHud.MessageType type, string msg, int amount = 0, Sprite icon = null)
	{
	}

	// Token: 0x06000084 RID: 132 RVA: 0x00005435 File Offset: 0x00003635
	public CapsuleCollider GetCollider()
	{
		return this.m_collider;
	}

	// Token: 0x06000085 RID: 133 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void OnStealthSuccess(Character character, float factor)
	{
	}

	// Token: 0x06000086 RID: 134 RVA: 0x00004CB6 File Offset: 0x00002EB6
	public virtual float GetStealthFactor()
	{
		return 1f;
	}

	// Token: 0x06000087 RID: 135 RVA: 0x00005440 File Offset: 0x00003640
	private void UpdateNoise(float dt)
	{
		this.m_noiseRange = Mathf.Max(0f, this.m_noiseRange - dt * 4f);
		this.m_syncNoiseTimer += dt;
		if (this.m_syncNoiseTimer > 0.5f)
		{
			this.m_syncNoiseTimer = 0f;
			this.m_nview.GetZDO().Set("noise", this.m_noiseRange);
		}
	}

	// Token: 0x06000088 RID: 136 RVA: 0x000054AC File Offset: 0x000036AC
	public void AddNoise(float range)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview.IsOwner())
		{
			this.RPC_AddNoise(0L, range);
			return;
		}
		this.m_nview.InvokeRPC("AddNoise", new object[]
		{
			range
		});
	}

	// Token: 0x06000089 RID: 137 RVA: 0x000054FD File Offset: 0x000036FD
	private void RPC_AddNoise(long sender, float range)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (range > this.m_noiseRange)
		{
			this.m_noiseRange = range;
			this.m_seman.ModifyNoise(this.m_noiseRange, ref this.m_noiseRange);
		}
	}

	// Token: 0x0600008A RID: 138 RVA: 0x00005534 File Offset: 0x00003734
	public float GetNoiseRange()
	{
		if (!this.m_nview.IsValid())
		{
			return 0f;
		}
		if (this.m_nview.IsOwner())
		{
			return this.m_noiseRange;
		}
		return this.m_nview.GetZDO().GetFloat("noise", 0f);
	}

	// Token: 0x0600008B RID: 139 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool InGodMode()
	{
		return false;
	}

	// Token: 0x0600008C RID: 140 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool InGhostMode()
	{
		return false;
	}

	// Token: 0x0600008D RID: 141 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool IsDebugFlying()
	{
		return false;
	}

	// Token: 0x0600008E RID: 142 RVA: 0x00005584 File Offset: 0x00003784
	public virtual string GetHoverText()
	{
		Tameable component = base.GetComponent<Tameable>();
		if (component)
		{
			return component.GetHoverText();
		}
		return "";
	}

	// Token: 0x0600008F RID: 143 RVA: 0x000055AC File Offset: 0x000037AC
	public virtual string GetHoverName()
	{
		return Localization.instance.Localize(this.m_name);
	}

	// Token: 0x06000090 RID: 144 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool IsHoldingAttack()
	{
		return false;
	}

	// Token: 0x06000091 RID: 145 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool InAttack()
	{
		return false;
	}

	// Token: 0x06000092 RID: 146 RVA: 0x000027E0 File Offset: 0x000009E0
	protected virtual void StopEmote()
	{
	}

	// Token: 0x06000093 RID: 147 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool InMinorAction()
	{
		return false;
	}

	// Token: 0x06000094 RID: 148 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool InDodge()
	{
		return false;
	}

	// Token: 0x06000095 RID: 149 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool IsDodgeInvincible()
	{
		return false;
	}

	// Token: 0x06000096 RID: 150 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool InEmote()
	{
		return false;
	}

	// Token: 0x06000097 RID: 151 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool IsBlocking()
	{
		return false;
	}

	// Token: 0x06000098 RID: 152 RVA: 0x000055BE File Offset: 0x000037BE
	public bool IsFlying()
	{
		return this.m_flying;
	}

	// Token: 0x06000099 RID: 153 RVA: 0x000055C6 File Offset: 0x000037C6
	public bool IsKnockedBack()
	{
		return this.m_pushForce != Vector3.zero;
	}

	// Token: 0x0600009A RID: 154 RVA: 0x000055D8 File Offset: 0x000037D8
	private void OnDrawGizmosSelected()
	{
		if (this.m_nview != null && this.m_nview.GetZDO() != null)
		{
			float @float = this.m_nview.GetZDO().GetFloat("noise", 0f);
			Gizmos.DrawWireSphere(base.transform.position, @float);
		}
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(base.transform.position + Vector3.up * this.m_swimDepth, new Vector3(1f, 0.05f, 1f));
		if (this.IsOnGround())
		{
			Gizmos.color = Color.green;
			Gizmos.DrawLine(this.m_lastGroundPoint, this.m_lastGroundPoint + this.m_lastGroundNormal);
		}
	}

	// Token: 0x0600009B RID: 155 RVA: 0x000023E2 File Offset: 0x000005E2
	public virtual bool TeleportTo(Vector3 pos, Quaternion rot, bool distantTeleport)
	{
		return false;
	}

	// Token: 0x0600009C RID: 156 RVA: 0x0000569D File Offset: 0x0000389D
	private void SyncVelocity()
	{
		this.m_nview.GetZDO().Set("BodyVelocity", this.m_body.velocity);
	}

	// Token: 0x0600009D RID: 157 RVA: 0x000056C0 File Offset: 0x000038C0
	public Vector3 GetVelocity()
	{
		if (!this.m_nview.IsValid())
		{
			return Vector3.zero;
		}
		if (this.m_nview.IsOwner())
		{
			return this.m_body.velocity;
		}
		return this.m_nview.GetZDO().GetVec3("BodyVelocity", Vector3.zero);
	}

	// Token: 0x0600009E RID: 158 RVA: 0x00005713 File Offset: 0x00003913
	public void AddRootMotion(Vector3 vel)
	{
		if (this.InDodge() || this.InAttack() || this.InEmote())
		{
			this.m_rootMotion += vel;
		}
	}

	// Token: 0x0600009F RID: 159 RVA: 0x00005740 File Offset: 0x00003940
	private void ApplyRootMotion(ref Vector3 vel)
	{
		Vector3 vector = this.m_rootMotion * 55f;
		if (vector.magnitude > vel.magnitude)
		{
			vel = vector;
		}
		this.m_rootMotion = Vector3.zero;
	}

	// Token: 0x060000A0 RID: 160 RVA: 0x00005780 File Offset: 0x00003980
	public static void GetCharactersInRange(Vector3 point, float radius, List<Character> characters)
	{
		foreach (Character character in Character.m_characters)
		{
			if (Vector3.Distance(character.transform.position, point) < radius)
			{
				characters.Add(character);
			}
		}
	}

	// Token: 0x060000A1 RID: 161 RVA: 0x000057E8 File Offset: 0x000039E8
	public static List<Character> GetAllCharacters()
	{
		return Character.m_characters;
	}

	// Token: 0x060000A2 RID: 162 RVA: 0x000057F0 File Offset: 0x000039F0
	public static bool IsCharacterInRange(Vector3 point, float range)
	{
		using (List<Character>.Enumerator enumerator = Character.m_characters.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (Vector3.Distance(enumerator.Current.transform.position, point) < range)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x060000A3 RID: 163 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void OnTargeted(bool sensed, bool alerted)
	{
	}

	// Token: 0x060000A4 RID: 164 RVA: 0x00005854 File Offset: 0x00003A54
	public GameObject GetVisual()
	{
		return this.m_visual;
	}

	// Token: 0x060000A5 RID: 165 RVA: 0x0000585C File Offset: 0x00003A5C
	protected void UpdateLodgroup()
	{
		if (this.m_lodGroup == null)
		{
			return;
		}
		Renderer[] componentsInChildren = this.m_visual.GetComponentsInChildren<Renderer>();
		LOD[] lods = this.m_lodGroup.GetLODs();
		lods[0].renderers = componentsInChildren;
		this.m_lodGroup.SetLODs(lods);
	}

	// Token: 0x060000A6 RID: 166 RVA: 0x0000471D File Offset: 0x0000291D
	public virtual float GetEquipmentMovementModifier()
	{
		return 0f;
	}

	// Token: 0x060000A7 RID: 167 RVA: 0x00004CB6 File Offset: 0x00002EB6
	protected virtual float GetJogSpeedFactor()
	{
		return 1f;
	}

	// Token: 0x060000A8 RID: 168 RVA: 0x00004CB6 File Offset: 0x00002EB6
	protected virtual float GetRunSpeedFactor()
	{
		return 1f;
	}

	// Token: 0x060000A9 RID: 169 RVA: 0x00004CB6 File Offset: 0x00002EB6
	protected virtual float GetAttackSpeedFactorMovement()
	{
		return 1f;
	}

	// Token: 0x060000AA RID: 170 RVA: 0x00004CB6 File Offset: 0x00002EB6
	protected virtual float GetAttackSpeedFactorRotation()
	{
		return 1f;
	}

	// Token: 0x060000AB RID: 171 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void RaiseSkill(Skills.SkillType skill, float value = 1f)
	{
	}

	// Token: 0x060000AC RID: 172 RVA: 0x000058A9 File Offset: 0x00003AA9
	public virtual Skills GetSkills()
	{
		return null;
	}

	// Token: 0x060000AD RID: 173 RVA: 0x0000471D File Offset: 0x0000291D
	public virtual float GetSkillFactor(Skills.SkillType skill)
	{
		return 0f;
	}

	// Token: 0x060000AE RID: 174 RVA: 0x000058AC File Offset: 0x00003AAC
	public virtual float GetRandomSkillFactor(Skills.SkillType skill)
	{
		return UnityEngine.Random.Range(0.75f, 1f);
	}

	// Token: 0x060000AF RID: 175 RVA: 0x000058C0 File Offset: 0x00003AC0
	public bool IsMonsterFaction()
	{
		return !this.IsTamed() && (this.m_faction == Character.Faction.ForestMonsters || this.m_faction == Character.Faction.Undead || this.m_faction == Character.Faction.Demon || this.m_faction == Character.Faction.PlainsMonsters || this.m_faction == Character.Faction.MountainMonsters || this.m_faction == Character.Faction.SeaMonsters);
	}

	// Token: 0x060000B0 RID: 176 RVA: 0x0000590F File Offset: 0x00003B0F
	public Transform GetTransform()
	{
		if (this == null)
		{
			return null;
		}
		return base.transform;
	}

	// Token: 0x060000B1 RID: 177 RVA: 0x00005922 File Offset: 0x00003B22
	public Collider GetLastGroundCollider()
	{
		return this.m_lastGroundCollider;
	}

	// Token: 0x060000B2 RID: 178 RVA: 0x0000592A File Offset: 0x00003B2A
	public Vector3 GetLastGroundNormal()
	{
		return this.m_groundContactNormal;
	}

	// Token: 0x060000B3 RID: 179 RVA: 0x00005932 File Offset: 0x00003B32
	public void ResetCloth()
	{
		this.m_nview.InvokeRPC(ZNetView.Everybody, "ResetCloth", Array.Empty<object>());
	}

	// Token: 0x060000B4 RID: 180 RVA: 0x00005950 File Offset: 0x00003B50
	private void RPC_ResetCloth(long sender)
	{
		foreach (Cloth cloth in base.GetComponentsInChildren<Cloth>())
		{
			if (cloth.enabled)
			{
				cloth.enabled = false;
				cloth.enabled = true;
			}
		}
	}

	// Token: 0x060000B5 RID: 181 RVA: 0x0000598C File Offset: 0x00003B8C
	public virtual bool GetRelativePosition(out ZDOID parent, out Vector3 relativePos, out Vector3 relativeVel)
	{
		relativeVel = Vector3.zero;
		if (this.IsOnGround() && this.m_lastGroundBody)
		{
			ZNetView component = this.m_lastGroundBody.GetComponent<ZNetView>();
			if (component && component.IsValid())
			{
				parent = component.GetZDO().m_uid;
				relativePos = component.transform.InverseTransformPoint(base.transform.position);
				relativeVel = component.transform.InverseTransformVector(this.m_body.velocity - this.m_lastGroundBody.velocity);
				return true;
			}
		}
		parent = ZDOID.None;
		relativePos = Vector3.zero;
		return false;
	}

	// Token: 0x060000B6 RID: 182 RVA: 0x00005A4A File Offset: 0x00003C4A
	public Quaternion GetLookYaw()
	{
		return this.m_lookYaw;
	}

	// Token: 0x060000B7 RID: 183 RVA: 0x00005A52 File Offset: 0x00003C52
	public Vector3 GetMoveDir()
	{
		return this.m_moveDir;
	}

	// Token: 0x060000B8 RID: 184 RVA: 0x00005A5A File Offset: 0x00003C5A
	public BaseAI GetBaseAI()
	{
		return this.m_baseAI;
	}

	// Token: 0x060000B9 RID: 185 RVA: 0x00005A62 File Offset: 0x00003C62
	public float GetMass()
	{
		return this.m_body.mass;
	}

	// Token: 0x060000BA RID: 186 RVA: 0x00005A70 File Offset: 0x00003C70
	protected void SetVisible(bool visible)
	{
		if (this.m_lodGroup == null)
		{
			return;
		}
		if (this.m_lodVisible == visible)
		{
			return;
		}
		this.m_lodVisible = visible;
		if (this.m_lodVisible)
		{
			this.m_lodGroup.localReferencePoint = this.m_originalLocalRef;
			return;
		}
		this.m_lodGroup.localReferencePoint = new Vector3(999999f, 999999f, 999999f);
	}

	// Token: 0x060000BB RID: 187 RVA: 0x00005AD6 File Offset: 0x00003CD6
	public void SetTamed(bool tamed)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_tamed == tamed)
		{
			return;
		}
		this.m_nview.InvokeRPC("SetTamed", new object[]
		{
			tamed
		});
	}

	// Token: 0x060000BC RID: 188 RVA: 0x00005B0F File Offset: 0x00003D0F
	private void RPC_SetTamed(long sender, bool tamed)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_tamed == tamed)
		{
			return;
		}
		this.m_tamed = tamed;
		this.m_nview.GetZDO().Set("tamed", this.m_tamed);
	}

	// Token: 0x060000BD RID: 189 RVA: 0x00005B4C File Offset: 0x00003D4C
	public bool IsTamed()
	{
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		if (!this.m_nview.IsOwner() && Time.time - this.m_lastTamedCheck > 1f)
		{
			this.m_lastTamedCheck = Time.time;
			this.m_tamed = this.m_nview.GetZDO().GetBool("tamed", this.m_tamed);
		}
		return this.m_tamed;
	}

	// Token: 0x060000BE RID: 190 RVA: 0x00005BBA File Offset: 0x00003DBA
	public SEMan GetSEMan()
	{
		return this.m_seman;
	}

	// Token: 0x060000BF RID: 191 RVA: 0x00005BC2 File Offset: 0x00003DC2
	public bool InInterior()
	{
		return base.transform.position.y > 3000f;
	}

	// Token: 0x060000C0 RID: 192 RVA: 0x00005BDB File Offset: 0x00003DDB
	public static void SetDPSDebug(bool enabled)
	{
		Character.m_dpsDebugEnabled = enabled;
	}

	// Token: 0x060000C1 RID: 193 RVA: 0x00005BE3 File Offset: 0x00003DE3
	public static bool IsDPSDebugEnabled()
	{
		return Character.m_dpsDebugEnabled;
	}

	// Token: 0x04000001 RID: 1
	private float m_underWorldCheckTimer;

	// Token: 0x04000002 RID: 2
	private Collider m_lowestContactCollider;

	// Token: 0x04000003 RID: 3
	private bool m_groundContact;

	// Token: 0x04000004 RID: 4
	private Vector3 m_groundContactPoint = Vector3.zero;

	// Token: 0x04000005 RID: 5
	private Vector3 m_groundContactNormal = Vector3.zero;

	// Token: 0x04000006 RID: 6
	public Action<float, Character> m_onDamaged;

	// Token: 0x04000007 RID: 7
	public Action m_onDeath;

	// Token: 0x04000008 RID: 8
	public Action<int> m_onLevelSet;

	// Token: 0x04000009 RID: 9
	public Action<Vector3> m_onLand;

	// Token: 0x0400000A RID: 10
	[Header("Character")]
	public string m_name = "";

	// Token: 0x0400000B RID: 11
	public Character.Faction m_faction = Character.Faction.AnimalsVeg;

	// Token: 0x0400000C RID: 12
	public bool m_boss;

	// Token: 0x0400000D RID: 13
	public string m_bossEvent = "";

	// Token: 0x0400000E RID: 14
	public string m_defeatSetGlobalKey = "";

	// Token: 0x0400000F RID: 15
	[Header("Movement & Physics")]
	public float m_crouchSpeed = 2f;

	// Token: 0x04000010 RID: 16
	public float m_walkSpeed = 5f;

	// Token: 0x04000011 RID: 17
	public float m_speed = 10f;

	// Token: 0x04000012 RID: 18
	public float m_turnSpeed = 300f;

	// Token: 0x04000013 RID: 19
	public float m_runSpeed = 20f;

	// Token: 0x04000014 RID: 20
	public float m_runTurnSpeed = 300f;

	// Token: 0x04000015 RID: 21
	public float m_flySlowSpeed = 5f;

	// Token: 0x04000016 RID: 22
	public float m_flyFastSpeed = 12f;

	// Token: 0x04000017 RID: 23
	public float m_flyTurnSpeed = 12f;

	// Token: 0x04000018 RID: 24
	public float m_acceleration = 1f;

	// Token: 0x04000019 RID: 25
	public float m_jumpForce = 10f;

	// Token: 0x0400001A RID: 26
	public float m_jumpForceForward;

	// Token: 0x0400001B RID: 27
	public float m_jumpForceTiredFactor = 0.7f;

	// Token: 0x0400001C RID: 28
	public float m_airControl = 0.1f;

	// Token: 0x0400001D RID: 29
	private const float m_slopeStaminaDrain = 10f;

	// Token: 0x0400001E RID: 30
	public const float m_minSlideDegreesPlayer = 38f;

	// Token: 0x0400001F RID: 31
	public const float m_minSlideDegreesMonster = 90f;

	// Token: 0x04000020 RID: 32
	private const float m_rootMotionMultiplier = 55f;

	// Token: 0x04000021 RID: 33
	private const float m_continousPushForce = 10f;

	// Token: 0x04000022 RID: 34
	private const float m_pushForcedissipation = 100f;

	// Token: 0x04000023 RID: 35
	private const float m_maxMoveForce = 20f;

	// Token: 0x04000024 RID: 36
	public bool m_canSwim = true;

	// Token: 0x04000025 RID: 37
	public float m_swimDepth = 2f;

	// Token: 0x04000026 RID: 38
	public float m_swimSpeed = 2f;

	// Token: 0x04000027 RID: 39
	public float m_swimTurnSpeed = 100f;

	// Token: 0x04000028 RID: 40
	public float m_swimAcceleration = 0.05f;

	// Token: 0x04000029 RID: 41
	public Character.GroundTiltType m_groundTilt;

	// Token: 0x0400002A RID: 42
	public bool m_flying;

	// Token: 0x0400002B RID: 43
	public float m_jumpStaminaUsage = 10f;

	// Token: 0x0400002C RID: 44
	[Header("Bodyparts")]
	public Transform m_eye;

	// Token: 0x0400002D RID: 45
	protected Transform m_head;

	// Token: 0x0400002E RID: 46
	[Header("Effects")]
	public EffectList m_hitEffects = new EffectList();

	// Token: 0x0400002F RID: 47
	public EffectList m_critHitEffects = new EffectList();

	// Token: 0x04000030 RID: 48
	public EffectList m_backstabHitEffects = new EffectList();

	// Token: 0x04000031 RID: 49
	public EffectList m_deathEffects = new EffectList();

	// Token: 0x04000032 RID: 50
	public EffectList m_waterEffects = new EffectList();

	// Token: 0x04000033 RID: 51
	public EffectList m_slideEffects = new EffectList();

	// Token: 0x04000034 RID: 52
	public EffectList m_jumpEffects = new EffectList();

	// Token: 0x04000035 RID: 53
	[Header("Health & Damage")]
	public bool m_tolerateWater = true;

	// Token: 0x04000036 RID: 54
	public bool m_tolerateFire;

	// Token: 0x04000037 RID: 55
	public bool m_tolerateSmoke = true;

	// Token: 0x04000038 RID: 56
	public float m_health = 10f;

	// Token: 0x04000039 RID: 57
	public HitData.DamageModifiers m_damageModifiers;

	// Token: 0x0400003A RID: 58
	public bool m_staggerWhenBlocked = true;

	// Token: 0x0400003B RID: 59
	public float m_staggerDamageFactor;

	// Token: 0x0400003C RID: 60
	private const float m_staggerResetTime = 3f;

	// Token: 0x0400003D RID: 61
	private float m_staggerDamage;

	// Token: 0x0400003E RID: 62
	private float m_staggerTimer;

	// Token: 0x0400003F RID: 63
	private float m_backstabTime = -99999f;

	// Token: 0x04000040 RID: 64
	private const float m_backstabResetTime = 300f;

	// Token: 0x04000041 RID: 65
	private GameObject[] m_waterEffects_instances;

	// Token: 0x04000042 RID: 66
	private GameObject[] m_slideEffects_instances;

	// Token: 0x04000043 RID: 67
	protected Vector3 m_moveDir = Vector3.zero;

	// Token: 0x04000044 RID: 68
	protected Vector3 m_lookDir = Vector3.forward;

	// Token: 0x04000045 RID: 69
	protected Quaternion m_lookYaw = Quaternion.identity;

	// Token: 0x04000046 RID: 70
	protected bool m_run;

	// Token: 0x04000047 RID: 71
	protected bool m_walk;

	// Token: 0x04000048 RID: 72
	protected bool m_attack;

	// Token: 0x04000049 RID: 73
	protected bool m_attackDraw;

	// Token: 0x0400004A RID: 74
	protected bool m_secondaryAttack;

	// Token: 0x0400004B RID: 75
	protected bool m_blocking;

	// Token: 0x0400004C RID: 76
	protected GameObject m_visual;

	// Token: 0x0400004D RID: 77
	protected LODGroup m_lodGroup;

	// Token: 0x0400004E RID: 78
	protected Rigidbody m_body;

	// Token: 0x0400004F RID: 79
	protected CapsuleCollider m_collider;

	// Token: 0x04000050 RID: 80
	protected ZNetView m_nview;

	// Token: 0x04000051 RID: 81
	protected ZSyncAnimation m_zanim;

	// Token: 0x04000052 RID: 82
	protected Animator m_animator;

	// Token: 0x04000053 RID: 83
	protected CharacterAnimEvent m_animEvent;

	// Token: 0x04000054 RID: 84
	protected BaseAI m_baseAI;

	// Token: 0x04000055 RID: 85
	private const float m_maxFallHeight = 20f;

	// Token: 0x04000056 RID: 86
	private const float m_minFallHeight = 4f;

	// Token: 0x04000057 RID: 87
	private const float m_maxFallDamage = 100f;

	// Token: 0x04000058 RID: 88
	private const float m_staggerDamageBonus = 2f;

	// Token: 0x04000059 RID: 89
	private const float m_baseVisualRange = 30f;

	// Token: 0x0400005A RID: 90
	private const float m_autoJumpInterval = 0.5f;

	// Token: 0x0400005B RID: 91
	private float m_jumpTimer;

	// Token: 0x0400005C RID: 92
	private float m_lastAutoJumpTime;

	// Token: 0x0400005D RID: 93
	private float m_lastGroundTouch;

	// Token: 0x0400005E RID: 94
	private Vector3 m_lastGroundNormal = Vector3.up;

	// Token: 0x0400005F RID: 95
	private Vector3 m_lastGroundPoint = Vector3.up;

	// Token: 0x04000060 RID: 96
	private Collider m_lastGroundCollider;

	// Token: 0x04000061 RID: 97
	private Rigidbody m_lastGroundBody;

	// Token: 0x04000062 RID: 98
	private Vector3 m_lastAttachPos = Vector3.zero;

	// Token: 0x04000063 RID: 99
	private Rigidbody m_lastAttachBody;

	// Token: 0x04000064 RID: 100
	protected float m_maxAirAltitude = -10000f;

	// Token: 0x04000065 RID: 101
	protected float m_waterLevel = -10000f;

	// Token: 0x04000066 RID: 102
	private float m_swimTimer = 999f;

	// Token: 0x04000067 RID: 103
	protected SEMan m_seman;

	// Token: 0x04000068 RID: 104
	private float m_noiseRange;

	// Token: 0x04000069 RID: 105
	private float m_syncNoiseTimer;

	// Token: 0x0400006A RID: 106
	private bool m_tamed;

	// Token: 0x0400006B RID: 107
	private float m_lastTamedCheck;

	// Token: 0x0400006C RID: 108
	private int m_level = 1;

	// Token: 0x0400006D RID: 109
	private Vector3 m_currentVel = Vector3.zero;

	// Token: 0x0400006E RID: 110
	private float m_currentTurnVel;

	// Token: 0x0400006F RID: 111
	private float m_currentTurnVelChange;

	// Token: 0x04000070 RID: 112
	private Vector3 m_groundTiltNormal = Vector3.up;

	// Token: 0x04000071 RID: 113
	protected Vector3 m_pushForce = Vector3.zero;

	// Token: 0x04000072 RID: 114
	private Vector3 m_rootMotion = Vector3.zero;

	// Token: 0x04000073 RID: 115
	private static int forward_speed = 0;

	// Token: 0x04000074 RID: 116
	private static int sideway_speed = 0;

	// Token: 0x04000075 RID: 117
	private static int turn_speed = 0;

	// Token: 0x04000076 RID: 118
	private static int inWater = 0;

	// Token: 0x04000077 RID: 119
	private static int onGround = 0;

	// Token: 0x04000078 RID: 120
	private static int encumbered = 0;

	// Token: 0x04000079 RID: 121
	private static int flying = 0;

	// Token: 0x0400007A RID: 122
	private float m_slippage;

	// Token: 0x0400007B RID: 123
	protected bool m_wallRunning;

	// Token: 0x0400007C RID: 124
	protected bool m_sliding;

	// Token: 0x0400007D RID: 125
	protected bool m_running;

	// Token: 0x0400007E RID: 126
	private Vector3 m_originalLocalRef;

	// Token: 0x0400007F RID: 127
	private bool m_lodVisible = true;

	// Token: 0x04000080 RID: 128
	private static int m_smokeRayMask = 0;

	// Token: 0x04000081 RID: 129
	private float m_smokeCheckTimer;

	// Token: 0x04000082 RID: 130
	private static bool m_dpsDebugEnabled = false;

	// Token: 0x04000083 RID: 131
	private static List<KeyValuePair<float, float>> m_enemyDamage = new List<KeyValuePair<float, float>>();

	// Token: 0x04000084 RID: 132
	private static List<KeyValuePair<float, float>> m_playerDamage = new List<KeyValuePair<float, float>>();

	// Token: 0x04000085 RID: 133
	private static List<Character> m_characters = new List<Character>();

	// Token: 0x04000086 RID: 134
	protected static int m_characterLayer = 0;

	// Token: 0x04000087 RID: 135
	protected static int m_characterNetLayer = 0;

	// Token: 0x04000088 RID: 136
	protected static int m_characterGhostLayer = 0;

	// Token: 0x04000089 RID: 137
	protected static int m_animatorTagFreeze = Animator.StringToHash("freeze");

	// Token: 0x0400008A RID: 138
	protected static int m_animatorTagStagger = Animator.StringToHash("stagger");

	// Token: 0x0400008B RID: 139
	protected static int m_animatorTagSitting = Animator.StringToHash("sitting");

	// Token: 0x0200011B RID: 283
	public enum Faction
	{
		// Token: 0x04000FA3 RID: 4003
		Players,
		// Token: 0x04000FA4 RID: 4004
		AnimalsVeg,
		// Token: 0x04000FA5 RID: 4005
		ForestMonsters,
		// Token: 0x04000FA6 RID: 4006
		Undead,
		// Token: 0x04000FA7 RID: 4007
		Demon,
		// Token: 0x04000FA8 RID: 4008
		MountainMonsters,
		// Token: 0x04000FA9 RID: 4009
		SeaMonsters,
		// Token: 0x04000FAA RID: 4010
		PlainsMonsters,
		// Token: 0x04000FAB RID: 4011
		Boss
	}

	// Token: 0x0200011C RID: 284
	public enum GroundTiltType
	{
		// Token: 0x04000FAD RID: 4013
		None,
		// Token: 0x04000FAE RID: 4014
		Pitch,
		// Token: 0x04000FAF RID: 4015
		Full,
		// Token: 0x04000FB0 RID: 4016
		PitchRaycast,
		// Token: 0x04000FB1 RID: 4017
		FullRaycast
	}
}
