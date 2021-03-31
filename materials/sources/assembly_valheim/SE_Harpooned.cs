using System;
using UnityEngine;

// Token: 0x02000027 RID: 39
public class SE_Harpooned : StatusEffect
{
	// Token: 0x060003AC RID: 940 RVA: 0x0001EF30 File Offset: 0x0001D130
	public override void Setup(Character character)
	{
		base.Setup(character);
	}

	// Token: 0x060003AD RID: 941 RVA: 0x0001F45C File Offset: 0x0001D65C
	public override void SetAttacker(Character attacker)
	{
		ZLog.Log("Setting attacker " + attacker.m_name);
		this.m_attacker = attacker;
		this.m_time = 0f;
		if (this.m_character.IsBoss())
		{
			this.m_broken = true;
			return;
		}
		if (Vector3.Distance(this.m_attacker.transform.position, this.m_character.transform.position) > this.m_maxDistance)
		{
			this.m_attacker.Message(MessageHud.MessageType.Center, "Target too far", 0, null);
			this.m_broken = true;
			return;
		}
		this.m_attacker.Message(MessageHud.MessageType.Center, this.m_character.m_name + " harpooned", 0, null);
		foreach (GameObject gameObject in this.m_startEffectInstances)
		{
			if (gameObject)
			{
				LineConnect component = gameObject.GetComponent<LineConnect>();
				if (component)
				{
					component.SetPeer(this.m_attacker.GetComponent<ZNetView>());
				}
			}
		}
	}

	// Token: 0x060003AE RID: 942 RVA: 0x0001F554 File Offset: 0x0001D754
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		if (!this.m_attacker)
		{
			return;
		}
		Rigidbody component = this.m_character.GetComponent<Rigidbody>();
		if (component)
		{
			Vector3 vector = this.m_attacker.transform.position - this.m_character.transform.position;
			Vector3 normalized = vector.normalized;
			float radius = this.m_character.GetRadius();
			float magnitude = vector.magnitude;
			float num = Mathf.Clamp01(Vector3.Dot(normalized, component.velocity));
			float t = Utils.LerpStep(this.m_minDistance, this.m_maxDistance, magnitude);
			float num2 = Mathf.Lerp(this.m_minForce, this.m_maxForce, t);
			float num3 = Mathf.Clamp01(this.m_maxMass / component.mass);
			float num4 = num2 * num3;
			if (magnitude - radius > this.m_minDistance && num < num4)
			{
				normalized.y = 0f;
				normalized.Normalize();
				if (this.m_character.GetStandingOnShip() == null && !this.m_character.IsAttached())
				{
					component.AddForce(normalized * num4, ForceMode.VelocityChange);
				}
				this.m_drainStaminaTimer += dt;
				if (this.m_drainStaminaTimer > this.m_staminaDrainInterval)
				{
					this.m_drainStaminaTimer = 0f;
					float num5 = 1f - Mathf.Clamp01(num / num2);
					this.m_attacker.UseStamina(this.m_staminaDrain * num5);
				}
			}
			if (magnitude > this.m_maxDistance)
			{
				this.m_broken = true;
				this.m_attacker.Message(MessageHud.MessageType.Center, "Line broke", 0, null);
			}
			if (!this.m_attacker.HaveStamina(0f))
			{
				this.m_broken = true;
				this.m_attacker.Message(MessageHud.MessageType.Center, this.m_character.m_name + " escaped", 0, null);
			}
		}
	}

	// Token: 0x060003AF RID: 943 RVA: 0x0001F730 File Offset: 0x0001D930
	public override bool IsDone()
	{
		if (base.IsDone())
		{
			return true;
		}
		if (this.m_broken)
		{
			return true;
		}
		if (!this.m_attacker)
		{
			return true;
		}
		if (this.m_time > 2f && (this.m_attacker.IsBlocking() || this.m_attacker.InAttack()))
		{
			this.m_attacker.Message(MessageHud.MessageType.Center, this.m_character.m_name + " released", 0, null);
			return true;
		}
		return false;
	}

	// Token: 0x04000399 RID: 921
	[Header("SE_Harpooned")]
	public float m_minForce = 2f;

	// Token: 0x0400039A RID: 922
	public float m_maxForce = 10f;

	// Token: 0x0400039B RID: 923
	public float m_minDistance = 6f;

	// Token: 0x0400039C RID: 924
	public float m_maxDistance = 30f;

	// Token: 0x0400039D RID: 925
	public float m_staminaDrain = 10f;

	// Token: 0x0400039E RID: 926
	public float m_staminaDrainInterval = 0.1f;

	// Token: 0x0400039F RID: 927
	public float m_maxMass = 50f;

	// Token: 0x040003A0 RID: 928
	private bool m_broken;

	// Token: 0x040003A1 RID: 929
	private Character m_attacker;

	// Token: 0x040003A2 RID: 930
	private float m_drainStaminaTimer;
}
