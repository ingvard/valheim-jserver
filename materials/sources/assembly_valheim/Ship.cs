using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000F1 RID: 241
public class Ship : MonoBehaviour
{
	// Token: 0x06000ECD RID: 3789 RVA: 0x00069B0C File Offset: 0x00067D0C
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_body = base.GetComponent<Rigidbody>();
		WearNTear component = base.GetComponent<WearNTear>();
		if (component)
		{
			WearNTear wearNTear = component;
			wearNTear.m_onDestroyed = (Action)Delegate.Combine(wearNTear.m_onDestroyed, new Action(this.OnDestroyed));
		}
		if (this.m_nview.GetZDO() == null)
		{
			base.enabled = false;
		}
		this.m_body.maxDepenetrationVelocity = 2f;
		Heightmap.ForceGenerateAll();
		this.m_sailCloth = this.m_sailObject.GetComponentInChildren<Cloth>();
	}

	// Token: 0x06000ECE RID: 3790 RVA: 0x00069B9C File Offset: 0x00067D9C
	public bool CanBeRemoved()
	{
		return this.m_players.Count == 0;
	}

	// Token: 0x06000ECF RID: 3791 RVA: 0x00069BAC File Offset: 0x00067DAC
	private void Start()
	{
		this.m_nview.Register("Stop", new Action<long>(this.RPC_Stop));
		this.m_nview.Register("Forward", new Action<long>(this.RPC_Forward));
		this.m_nview.Register("Backward", new Action<long>(this.RPC_Backward));
		this.m_nview.Register<float>("Rudder", new Action<long, float>(this.RPC_Rudder));
		base.InvokeRepeating("UpdateOwner", 2f, 2f);
	}

	// Token: 0x06000ED0 RID: 3792 RVA: 0x00069C40 File Offset: 0x00067E40
	private void PrintStats()
	{
		if (this.m_players.Count == 0)
		{
			return;
		}
		ZLog.Log("Vel:" + this.m_body.velocity.magnitude.ToString("0.0"));
	}

	// Token: 0x06000ED1 RID: 3793 RVA: 0x00069C8C File Offset: 0x00067E8C
	public void ApplyMovementControlls(Vector3 dir)
	{
		bool flag = (double)dir.z > 0.5;
		bool flag2 = (double)dir.z < -0.5;
		if (flag && !this.m_forwardPressed)
		{
			this.Forward();
		}
		if (flag2 && !this.m_backwardPressed)
		{
			this.Backward();
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		float num = Mathf.Lerp(0.5f, 1f, Mathf.Abs(this.m_rudderValue));
		this.m_rudder = dir.x * num;
		this.m_rudderValue += this.m_rudder * this.m_rudderSpeed * fixedDeltaTime;
		this.m_rudderValue = Mathf.Clamp(this.m_rudderValue, -1f, 1f);
		if (Time.time - this.m_sendRudderTime > 0.2f)
		{
			this.m_sendRudderTime = Time.time;
			this.m_nview.InvokeRPC("Rudder", new object[]
			{
				this.m_rudderValue
			});
		}
		this.m_forwardPressed = flag;
		this.m_backwardPressed = flag2;
	}

	// Token: 0x06000ED2 RID: 3794 RVA: 0x00069D97 File Offset: 0x00067F97
	public void Forward()
	{
		this.m_nview.InvokeRPC("Forward", Array.Empty<object>());
	}

	// Token: 0x06000ED3 RID: 3795 RVA: 0x00069DAE File Offset: 0x00067FAE
	public void Backward()
	{
		this.m_nview.InvokeRPC("Backward", Array.Empty<object>());
	}

	// Token: 0x06000ED4 RID: 3796 RVA: 0x00069DC5 File Offset: 0x00067FC5
	public void Rudder(float rudder)
	{
		this.m_nview.Invoke("Rudder", rudder);
	}

	// Token: 0x06000ED5 RID: 3797 RVA: 0x00069DD8 File Offset: 0x00067FD8
	private void RPC_Rudder(long sender, float value)
	{
		this.m_rudderValue = value;
	}

	// Token: 0x06000ED6 RID: 3798 RVA: 0x00069DE1 File Offset: 0x00067FE1
	public void Stop()
	{
		this.m_nview.InvokeRPC("Stop", Array.Empty<object>());
	}

	// Token: 0x06000ED7 RID: 3799 RVA: 0x00069DF8 File Offset: 0x00067FF8
	private void RPC_Stop(long sender)
	{
		this.m_speed = Ship.Speed.Stop;
	}

	// Token: 0x06000ED8 RID: 3800 RVA: 0x00069E04 File Offset: 0x00068004
	private void RPC_Forward(long sender)
	{
		switch (this.m_speed)
		{
		case Ship.Speed.Stop:
			this.m_speed = Ship.Speed.Slow;
			return;
		case Ship.Speed.Back:
			this.m_speed = Ship.Speed.Stop;
			break;
		case Ship.Speed.Slow:
			this.m_speed = Ship.Speed.Half;
			return;
		case Ship.Speed.Half:
			this.m_speed = Ship.Speed.Full;
			return;
		case Ship.Speed.Full:
			break;
		default:
			return;
		}
	}

	// Token: 0x06000ED9 RID: 3801 RVA: 0x00069E54 File Offset: 0x00068054
	private void RPC_Backward(long sender)
	{
		switch (this.m_speed)
		{
		case Ship.Speed.Stop:
			this.m_speed = Ship.Speed.Back;
			return;
		case Ship.Speed.Back:
			break;
		case Ship.Speed.Slow:
			this.m_speed = Ship.Speed.Stop;
			return;
		case Ship.Speed.Half:
			this.m_speed = Ship.Speed.Slow;
			return;
		case Ship.Speed.Full:
			this.m_speed = Ship.Speed.Half;
			break;
		default:
			return;
		}
	}

	// Token: 0x06000EDA RID: 3802 RVA: 0x00069EA4 File Offset: 0x000680A4
	private void FixedUpdate()
	{
		bool flag = this.HaveControllingPlayer();
		this.UpdateControlls(Time.fixedDeltaTime);
		this.UpdateSail(Time.fixedDeltaTime);
		this.UpdateRudder(Time.fixedDeltaTime, flag);
		if (this.m_nview && !this.m_nview.IsOwner())
		{
			return;
		}
		this.UpdateUpsideDmg(Time.fixedDeltaTime);
		if (this.m_players.Count == 0)
		{
			this.m_speed = Ship.Speed.Stop;
			this.m_rudderValue = 0f;
		}
		if (!flag && (this.m_speed == Ship.Speed.Slow || this.m_speed == Ship.Speed.Back))
		{
			this.m_speed = Ship.Speed.Stop;
		}
		float waveFactor = 1f;
		Vector3 worldCenterOfMass = this.m_body.worldCenterOfMass;
		Vector3 vector = this.m_floatCollider.transform.position + this.m_floatCollider.transform.forward * this.m_floatCollider.size.z / 2f;
		Vector3 vector2 = this.m_floatCollider.transform.position - this.m_floatCollider.transform.forward * this.m_floatCollider.size.z / 2f;
		Vector3 vector3 = this.m_floatCollider.transform.position - this.m_floatCollider.transform.right * this.m_floatCollider.size.x / 2f;
		Vector3 vector4 = this.m_floatCollider.transform.position + this.m_floatCollider.transform.right * this.m_floatCollider.size.x / 2f;
		float waterLevel = WaterVolume.GetWaterLevel(worldCenterOfMass, waveFactor);
		float waterLevel2 = WaterVolume.GetWaterLevel(vector3, waveFactor);
		float waterLevel3 = WaterVolume.GetWaterLevel(vector4, waveFactor);
		float waterLevel4 = WaterVolume.GetWaterLevel(vector, waveFactor);
		float waterLevel5 = WaterVolume.GetWaterLevel(vector2, waveFactor);
		float num = (waterLevel + waterLevel2 + waterLevel3 + waterLevel4 + waterLevel5) / 5f;
		float num2 = worldCenterOfMass.y - num - this.m_waterLevelOffset;
		if (num2 > this.m_disableLevel)
		{
			return;
		}
		this.m_body.WakeUp();
		this.UpdateWaterForce(num2, Time.fixedDeltaTime);
		ref Vector3 ptr = new Vector3(vector3.x, waterLevel2, vector3.z);
		Vector3 vector5 = new Vector3(vector4.x, waterLevel3, vector4.z);
		ref Vector3 ptr2 = new Vector3(vector.x, waterLevel4, vector.z);
		Vector3 vector6 = new Vector3(vector2.x, waterLevel5, vector2.z);
		float fixedDeltaTime = Time.fixedDeltaTime;
		float d = fixedDeltaTime * 50f;
		float num3 = Mathf.Clamp01(Mathf.Abs(num2) / this.m_forceDistance);
		Vector3 a = Vector3.up * this.m_force * num3;
		this.m_body.AddForceAtPosition(a * d, worldCenterOfMass, ForceMode.VelocityChange);
		float num4 = Vector3.Dot(this.m_body.velocity, base.transform.forward);
		float num5 = Vector3.Dot(this.m_body.velocity, base.transform.right);
		Vector3 vector7 = this.m_body.velocity;
		float value = vector7.y * vector7.y * Mathf.Sign(vector7.y) * this.m_damping * num3;
		float value2 = num4 * num4 * Mathf.Sign(num4) * this.m_dampingForward * num3;
		float value3 = num5 * num5 * Mathf.Sign(num5) * this.m_dampingSideway * num3;
		vector7.y -= Mathf.Clamp(value, -1f, 1f);
		vector7 -= base.transform.forward * Mathf.Clamp(value2, -1f, 1f);
		vector7 -= base.transform.right * Mathf.Clamp(value3, -1f, 1f);
		if (vector7.magnitude > this.m_body.velocity.magnitude)
		{
			vector7 = vector7.normalized * this.m_body.velocity.magnitude;
		}
		if (this.m_players.Count == 0)
		{
			vector7.x *= 0.1f;
			vector7.z *= 0.1f;
		}
		this.m_body.velocity = vector7;
		this.m_body.angularVelocity = this.m_body.angularVelocity - this.m_body.angularVelocity * this.m_angularDamping * num3;
		float num6 = 0.15f;
		float num7 = 0.5f;
		float num8 = Mathf.Clamp((ptr2.y - vector.y) * num6, -num7, num7);
		float num9 = Mathf.Clamp((vector6.y - vector2.y) * num6, -num7, num7);
		float num10 = Mathf.Clamp((ptr.y - vector3.y) * num6, -num7, num7);
		float num11 = Mathf.Clamp((vector5.y - vector4.y) * num6, -num7, num7);
		num8 = Mathf.Sign(num8) * Mathf.Abs(Mathf.Pow(num8, 2f));
		num9 = Mathf.Sign(num9) * Mathf.Abs(Mathf.Pow(num9, 2f));
		num10 = Mathf.Sign(num10) * Mathf.Abs(Mathf.Pow(num10, 2f));
		num11 = Mathf.Sign(num11) * Mathf.Abs(Mathf.Pow(num11, 2f));
		this.m_body.AddForceAtPosition(Vector3.up * num8 * d, vector, ForceMode.VelocityChange);
		this.m_body.AddForceAtPosition(Vector3.up * num9 * d, vector2, ForceMode.VelocityChange);
		this.m_body.AddForceAtPosition(Vector3.up * num10 * d, vector3, ForceMode.VelocityChange);
		this.m_body.AddForceAtPosition(Vector3.up * num11 * d, vector4, ForceMode.VelocityChange);
		float sailSize = 0f;
		if (this.m_speed == Ship.Speed.Full)
		{
			sailSize = 1f;
		}
		else if (this.m_speed == Ship.Speed.Half)
		{
			sailSize = 0.5f;
		}
		Vector3 sailForce = this.GetSailForce(sailSize, fixedDeltaTime);
		Vector3 position = worldCenterOfMass + base.transform.up * this.m_sailForceOffset;
		this.m_body.AddForceAtPosition(sailForce, position, ForceMode.VelocityChange);
		Vector3 position2 = base.transform.position + base.transform.forward * this.m_stearForceOffset;
		float d2 = num4 * this.m_stearVelForceFactor;
		this.m_body.AddForceAtPosition(base.transform.right * d2 * -this.m_rudderValue * fixedDeltaTime, position2, ForceMode.VelocityChange);
		Vector3 a2 = Vector3.zero;
		Ship.Speed speed = this.m_speed;
		if (speed != Ship.Speed.Back)
		{
			if (speed == Ship.Speed.Slow)
			{
				a2 += base.transform.forward * this.m_backwardForce * (1f - Mathf.Abs(this.m_rudderValue));
			}
		}
		else
		{
			a2 += -base.transform.forward * this.m_backwardForce * (1f - Mathf.Abs(this.m_rudderValue));
		}
		if (this.m_speed == Ship.Speed.Back || this.m_speed == Ship.Speed.Slow)
		{
			float d3 = (float)((this.m_speed == Ship.Speed.Back) ? -1 : 1);
			a2 += base.transform.right * this.m_stearForce * -this.m_rudderValue * d3;
		}
		this.m_body.AddForceAtPosition(a2 * fixedDeltaTime, position2, ForceMode.VelocityChange);
		this.ApplyEdgeForce(Time.fixedDeltaTime);
	}

	// Token: 0x06000EDB RID: 3803 RVA: 0x0006A684 File Offset: 0x00068884
	private void UpdateUpsideDmg(float dt)
	{
		if (base.transform.up.y < 0f)
		{
			this.m_upsideDownDmgTimer += dt;
			if (this.m_upsideDownDmgTimer > this.m_upsideDownDmgInterval)
			{
				this.m_upsideDownDmgTimer = 0f;
				IDestructible component = base.GetComponent<IDestructible>();
				if (component != null)
				{
					HitData hitData = new HitData();
					hitData.m_damage.m_blunt = this.m_upsideDownDmg;
					hitData.m_point = base.transform.position;
					hitData.m_dir = Vector3.up;
					component.Damage(hitData);
				}
			}
		}
	}

	// Token: 0x06000EDC RID: 3804 RVA: 0x0006A714 File Offset: 0x00068914
	private Vector3 GetSailForce(float sailSize, float dt)
	{
		Vector3 windDir = EnvMan.instance.GetWindDir();
		float windIntensity = EnvMan.instance.GetWindIntensity();
		float num = Mathf.Lerp(0.25f, 1f, windIntensity);
		float num2 = this.GetWindAngleFactor();
		num2 *= num;
		Vector3 target = Vector3.Normalize(windDir + base.transform.forward) * num2 * this.m_sailForceFactor * sailSize;
		this.m_sailForce = Vector3.SmoothDamp(this.m_sailForce, target, ref this.windChangeVelocity, 1f, 99f);
		return this.m_sailForce;
	}

	// Token: 0x06000EDD RID: 3805 RVA: 0x0006A7A8 File Offset: 0x000689A8
	public float GetWindAngleFactor()
	{
		float num = Vector3.Dot(EnvMan.instance.GetWindDir(), -base.transform.forward);
		float num2 = Mathf.Lerp(0.7f, 1f, 1f - Mathf.Abs(num));
		float num3 = 1f - Utils.LerpStep(0.75f, 0.8f, num);
		return num2 * num3;
	}

	// Token: 0x06000EDE RID: 3806 RVA: 0x0006A80C File Offset: 0x00068A0C
	private void UpdateWaterForce(float depth, float dt)
	{
		if (this.m_lastDepth == -9999f)
		{
			this.m_lastDepth = depth;
			return;
		}
		float num = depth - this.m_lastDepth;
		this.m_lastDepth = depth;
		float num2 = num / dt;
		if (num2 > 0f)
		{
			return;
		}
		if (Mathf.Abs(num2) > this.m_minWaterImpactForce && Time.time - this.m_lastWaterImpactTime > this.m_minWaterImpactInterval)
		{
			this.m_lastWaterImpactTime = Time.time;
			this.m_waterImpactEffect.Create(base.transform.position, base.transform.rotation, null, 1f);
			if (this.m_players.Count > 0)
			{
				IDestructible component = base.GetComponent<IDestructible>();
				if (component != null)
				{
					HitData hitData = new HitData();
					hitData.m_damage.m_blunt = this.m_waterImpactDamage;
					hitData.m_point = base.transform.position;
					hitData.m_dir = Vector3.up;
					component.Damage(hitData);
				}
			}
		}
	}

	// Token: 0x06000EDF RID: 3807 RVA: 0x0006A8F8 File Offset: 0x00068AF8
	private void ApplyEdgeForce(float dt)
	{
		float magnitude = base.transform.position.magnitude;
		float num = 10420f;
		if (magnitude > num)
		{
			Vector3 a = Vector3.Normalize(base.transform.position);
			float d = Utils.LerpStep(num, 10500f, magnitude) * 8f;
			Vector3 a2 = a * d;
			this.m_body.AddForce(a2 * dt, ForceMode.VelocityChange);
		}
	}

	// Token: 0x06000EE0 RID: 3808 RVA: 0x0006A964 File Offset: 0x00068B64
	private void FixTilt()
	{
		float num = Mathf.Asin(base.transform.right.y);
		float num2 = Mathf.Asin(base.transform.forward.y);
		if (Mathf.Abs(num) > 0.5235988f)
		{
			if (num > 0f)
			{
				base.transform.RotateAround(base.transform.position, base.transform.forward, -Time.fixedDeltaTime * 20f);
			}
			else
			{
				base.transform.RotateAround(base.transform.position, base.transform.forward, Time.fixedDeltaTime * 20f);
			}
		}
		if (Mathf.Abs(num2) > 0.5235988f)
		{
			if (num2 > 0f)
			{
				base.transform.RotateAround(base.transform.position, base.transform.right, -Time.fixedDeltaTime * 20f);
				return;
			}
			base.transform.RotateAround(base.transform.position, base.transform.right, Time.fixedDeltaTime * 20f);
		}
	}

	// Token: 0x06000EE1 RID: 3809 RVA: 0x0006AA7C File Offset: 0x00068C7C
	private void UpdateControlls(float dt)
	{
		if (this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set("forward", (int)this.m_speed);
			this.m_nview.GetZDO().Set("rudder", this.m_rudderValue);
			return;
		}
		this.m_speed = (Ship.Speed)this.m_nview.GetZDO().GetInt("forward", 0);
		if (Time.time - this.m_sendRudderTime > 1f)
		{
			this.m_rudderValue = this.m_nview.GetZDO().GetFloat("rudder", 0f);
		}
	}

	// Token: 0x06000EE2 RID: 3810 RVA: 0x0006AB1C File Offset: 0x00068D1C
	public bool IsSailUp()
	{
		return this.m_speed == Ship.Speed.Half || this.m_speed == Ship.Speed.Full;
	}

	// Token: 0x06000EE3 RID: 3811 RVA: 0x0006AB34 File Offset: 0x00068D34
	private void UpdateSail(float dt)
	{
		this.UpdateSailSize(dt);
		Vector3 vector = EnvMan.instance.GetWindDir();
		vector = Vector3.Cross(Vector3.Cross(vector, base.transform.up), base.transform.up);
		if (this.m_speed == Ship.Speed.Full || this.m_speed == Ship.Speed.Half)
		{
			float t = 0.5f + Vector3.Dot(base.transform.forward, vector) * 0.5f;
			Quaternion to = Quaternion.LookRotation(-Vector3.Lerp(vector, Vector3.Normalize(vector - base.transform.forward), t), base.transform.up);
			this.m_mastObject.transform.rotation = Quaternion.RotateTowards(this.m_mastObject.transform.rotation, to, 30f * dt);
			return;
		}
		if (this.m_speed == Ship.Speed.Back)
		{
			Quaternion from = Quaternion.LookRotation(-base.transform.forward, base.transform.up);
			Quaternion to2 = Quaternion.LookRotation(-vector, base.transform.up);
			to2 = Quaternion.RotateTowards(from, to2, 80f);
			this.m_mastObject.transform.rotation = Quaternion.RotateTowards(this.m_mastObject.transform.rotation, to2, 30f * dt);
		}
	}

	// Token: 0x06000EE4 RID: 3812 RVA: 0x0006AC80 File Offset: 0x00068E80
	private void UpdateRudder(float dt, bool haveControllingPlayer)
	{
		if (this.m_rudderObject)
		{
			Quaternion quaternion = Quaternion.Euler(0f, this.m_rudderRotationMax * -this.m_rudderValue, 0f);
			if (haveControllingPlayer)
			{
				if (this.m_speed == Ship.Speed.Slow)
				{
					this.m_rudderPaddleTimer += dt;
					quaternion *= Quaternion.Euler(0f, Mathf.Sin(this.m_rudderPaddleTimer * 6f) * 20f, 0f);
				}
				else if (this.m_speed == Ship.Speed.Back)
				{
					this.m_rudderPaddleTimer += dt;
					quaternion *= Quaternion.Euler(0f, Mathf.Sin(this.m_rudderPaddleTimer * -3f) * 40f, 0f);
				}
			}
			this.m_rudderObject.transform.localRotation = Quaternion.Slerp(this.m_rudderObject.transform.localRotation, quaternion, 0.5f);
		}
	}

	// Token: 0x06000EE5 RID: 3813 RVA: 0x0006AD78 File Offset: 0x00068F78
	private void UpdateSailSize(float dt)
	{
		float num = 0f;
		switch (this.m_speed)
		{
		case Ship.Speed.Stop:
			num = 0.1f;
			break;
		case Ship.Speed.Back:
			num = 0.1f;
			break;
		case Ship.Speed.Slow:
			num = 0.1f;
			break;
		case Ship.Speed.Half:
			num = 0.5f;
			break;
		case Ship.Speed.Full:
			num = 1f;
			break;
		}
		Vector3 localScale = this.m_sailObject.transform.localScale;
		bool flag = Mathf.Abs(localScale.y - num) < 0.01f;
		if (!flag)
		{
			localScale.y = Mathf.MoveTowards(localScale.y, num, dt);
			this.m_sailObject.transform.localScale = localScale;
		}
		if (this.m_sailCloth)
		{
			if (this.m_speed == Ship.Speed.Stop || this.m_speed == Ship.Speed.Slow || this.m_speed == Ship.Speed.Back)
			{
				if (flag && this.m_sailCloth.enabled)
				{
					this.m_sailCloth.enabled = false;
				}
			}
			else if (flag)
			{
				if (!this.sailWasInPosition)
				{
					this.m_sailCloth.enabled = false;
					this.m_sailCloth.enabled = true;
				}
			}
			else
			{
				this.m_sailCloth.enabled = true;
			}
		}
		this.sailWasInPosition = flag;
	}

	// Token: 0x06000EE6 RID: 3814 RVA: 0x0006AEA0 File Offset: 0x000690A0
	private void UpdateOwner()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (Player.m_localPlayer == null)
		{
			return;
		}
		if (this.m_players.Count > 0 && !this.IsPlayerInBoat(Player.m_localPlayer))
		{
			long owner = this.m_players[0].GetOwner();
			this.m_nview.GetZDO().SetOwner(owner);
			ZLog.Log("Changing ship owner to " + owner);
		}
	}

	// Token: 0x06000EE7 RID: 3815 RVA: 0x0006AF2C File Offset: 0x0006912C
	private void OnTriggerEnter(Collider collider)
	{
		Player component = collider.GetComponent<Player>();
		if (component)
		{
			this.m_players.Add(component);
			ZLog.Log("Player onboard, total onboard " + this.m_players.Count);
			if (component == Player.m_localPlayer)
			{
				Ship.m_currentShips.Add(this);
			}
		}
	}

	// Token: 0x06000EE8 RID: 3816 RVA: 0x0006AF8C File Offset: 0x0006918C
	private void OnTriggerExit(Collider collider)
	{
		Player component = collider.GetComponent<Player>();
		if (component)
		{
			this.m_players.Remove(component);
			ZLog.Log("Player over board, players left " + this.m_players.Count);
			if (component == Player.m_localPlayer)
			{
				Ship.m_currentShips.Remove(this);
			}
		}
	}

	// Token: 0x06000EE9 RID: 3817 RVA: 0x0006AFF0 File Offset: 0x000691F0
	public bool IsPlayerInBoat(ZDOID zdoid)
	{
		using (List<Player>.Enumerator enumerator = this.m_players.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.GetZDOID() == zdoid)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000EEA RID: 3818 RVA: 0x0006B050 File Offset: 0x00069250
	public bool IsPlayerInBoat(Player player)
	{
		return this.m_players.Contains(player);
	}

	// Token: 0x06000EEB RID: 3819 RVA: 0x0006B05E File Offset: 0x0006925E
	public bool HasPlayerOnboard()
	{
		return this.m_players.Count > 0;
	}

	// Token: 0x06000EEC RID: 3820 RVA: 0x0006B070 File Offset: 0x00069270
	private void OnDestroyed()
	{
		if (this.m_nview.IsValid() && this.m_nview.IsOwner())
		{
			Gogan.LogEvent("Game", "ShipDestroyed", base.gameObject.name, 0L);
		}
		Ship.m_currentShips.Remove(this);
	}

	// Token: 0x06000EED RID: 3821 RVA: 0x0006B0C0 File Offset: 0x000692C0
	public bool IsWindControllActive()
	{
		using (List<Player>.Enumerator enumerator = this.m_players.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.GetSEMan().HaveStatusAttribute(StatusEffect.StatusAttribute.SailingPower))
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000EEE RID: 3822 RVA: 0x0006B120 File Offset: 0x00069320
	public static Ship GetLocalShip()
	{
		if (Ship.m_currentShips.Count == 0)
		{
			return null;
		}
		return Ship.m_currentShips[Ship.m_currentShips.Count - 1];
	}

	// Token: 0x06000EEF RID: 3823 RVA: 0x0006B146 File Offset: 0x00069346
	public bool HaveControllingPlayer()
	{
		return this.m_players.Count != 0 && this.m_shipControlls.HaveValidUser();
	}

	// Token: 0x06000EF0 RID: 3824 RVA: 0x0006B162 File Offset: 0x00069362
	public bool IsOwner()
	{
		return this.m_nview.IsValid() && this.m_nview.IsOwner();
	}

	// Token: 0x06000EF1 RID: 3825 RVA: 0x0006B17E File Offset: 0x0006937E
	public float GetSpeed()
	{
		return Vector3.Dot(this.m_body.velocity, base.transform.forward);
	}

	// Token: 0x06000EF2 RID: 3826 RVA: 0x0006B19B File Offset: 0x0006939B
	public Ship.Speed GetSpeedSetting()
	{
		return this.m_speed;
	}

	// Token: 0x06000EF3 RID: 3827 RVA: 0x0006B1A3 File Offset: 0x000693A3
	public float GetRudder()
	{
		return this.m_rudder;
	}

	// Token: 0x06000EF4 RID: 3828 RVA: 0x0006B1AB File Offset: 0x000693AB
	public float GetRudderValue()
	{
		return this.m_rudderValue;
	}

	// Token: 0x06000EF5 RID: 3829 RVA: 0x0006B1B4 File Offset: 0x000693B4
	public float GetShipYawAngle()
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return 0f;
		}
		return -Utils.YawFromDirection(mainCamera.transform.InverseTransformDirection(base.transform.forward));
	}

	// Token: 0x06000EF6 RID: 3830 RVA: 0x0006B1F4 File Offset: 0x000693F4
	public float GetWindAngle()
	{
		Vector3 windDir = EnvMan.instance.GetWindDir();
		return -Utils.YawFromDirection(base.transform.InverseTransformDirection(windDir));
	}

	// Token: 0x06000EF7 RID: 3831 RVA: 0x0006B220 File Offset: 0x00069420
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(base.transform.position + base.transform.forward * this.m_stearForceOffset, 0.25f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(base.transform.position + base.transform.up * this.m_sailForceOffset, 0.25f);
	}

	// Token: 0x04000DBF RID: 3519
	private bool m_forwardPressed;

	// Token: 0x04000DC0 RID: 3520
	private bool m_backwardPressed;

	// Token: 0x04000DC1 RID: 3521
	private float m_sendRudderTime;

	// Token: 0x04000DC2 RID: 3522
	private Vector3 windChangeVelocity = Vector3.zero;

	// Token: 0x04000DC3 RID: 3523
	private bool sailWasInPosition;

	// Token: 0x04000DC4 RID: 3524
	[Header("Objects")]
	public GameObject m_sailObject;

	// Token: 0x04000DC5 RID: 3525
	public GameObject m_mastObject;

	// Token: 0x04000DC6 RID: 3526
	public GameObject m_rudderObject;

	// Token: 0x04000DC7 RID: 3527
	public ShipControlls m_shipControlls;

	// Token: 0x04000DC8 RID: 3528
	public Transform m_controlGuiPos;

	// Token: 0x04000DC9 RID: 3529
	[Header("Misc")]
	public BoxCollider m_floatCollider;

	// Token: 0x04000DCA RID: 3530
	public float m_waterLevelOffset;

	// Token: 0x04000DCB RID: 3531
	public float m_forceDistance = 1f;

	// Token: 0x04000DCC RID: 3532
	public float m_force = 0.5f;

	// Token: 0x04000DCD RID: 3533
	public float m_damping = 0.05f;

	// Token: 0x04000DCE RID: 3534
	public float m_dampingSideway = 0.05f;

	// Token: 0x04000DCF RID: 3535
	public float m_dampingForward = 0.01f;

	// Token: 0x04000DD0 RID: 3536
	public float m_angularDamping = 0.01f;

	// Token: 0x04000DD1 RID: 3537
	public float m_disableLevel = -0.5f;

	// Token: 0x04000DD2 RID: 3538
	public float m_sailForceOffset;

	// Token: 0x04000DD3 RID: 3539
	public float m_sailForceFactor = 0.1f;

	// Token: 0x04000DD4 RID: 3540
	public float m_rudderSpeed = 0.5f;

	// Token: 0x04000DD5 RID: 3541
	public float m_stearForceOffset = -10f;

	// Token: 0x04000DD6 RID: 3542
	public float m_stearForce = 0.5f;

	// Token: 0x04000DD7 RID: 3543
	public float m_stearVelForceFactor = 0.1f;

	// Token: 0x04000DD8 RID: 3544
	public float m_backwardForce = 50f;

	// Token: 0x04000DD9 RID: 3545
	public float m_rudderRotationMax = 30f;

	// Token: 0x04000DDA RID: 3546
	public float m_rudderRotationSpeed = 30f;

	// Token: 0x04000DDB RID: 3547
	public float m_minWaterImpactForce = 2.5f;

	// Token: 0x04000DDC RID: 3548
	public float m_minWaterImpactInterval = 2f;

	// Token: 0x04000DDD RID: 3549
	public float m_waterImpactDamage = 10f;

	// Token: 0x04000DDE RID: 3550
	public float m_upsideDownDmgInterval = 1f;

	// Token: 0x04000DDF RID: 3551
	public float m_upsideDownDmg = 20f;

	// Token: 0x04000DE0 RID: 3552
	public EffectList m_waterImpactEffect = new EffectList();

	// Token: 0x04000DE1 RID: 3553
	private Ship.Speed m_speed;

	// Token: 0x04000DE2 RID: 3554
	private float m_rudder;

	// Token: 0x04000DE3 RID: 3555
	private float m_rudderValue;

	// Token: 0x04000DE4 RID: 3556
	private Vector3 m_sailForce = Vector3.zero;

	// Token: 0x04000DE5 RID: 3557
	private List<Player> m_players = new List<Player>();

	// Token: 0x04000DE6 RID: 3558
	private static List<Ship> m_currentShips = new List<Ship>();

	// Token: 0x04000DE7 RID: 3559
	private Rigidbody m_body;

	// Token: 0x04000DE8 RID: 3560
	private ZNetView m_nview;

	// Token: 0x04000DE9 RID: 3561
	private Cloth m_sailCloth;

	// Token: 0x04000DEA RID: 3562
	private float m_lastDepth = -9999f;

	// Token: 0x04000DEB RID: 3563
	private float m_lastWaterImpactTime;

	// Token: 0x04000DEC RID: 3564
	private float m_upsideDownDmgTimer;

	// Token: 0x04000DED RID: 3565
	private float m_rudderPaddleTimer;

	// Token: 0x020001AA RID: 426
	public enum Speed
	{
		// Token: 0x0400130C RID: 4876
		Stop,
		// Token: 0x0400130D RID: 4877
		Back,
		// Token: 0x0400130E RID: 4878
		Slow,
		// Token: 0x0400130F RID: 4879
		Half,
		// Token: 0x04001310 RID: 4880
		Full
	}
}
