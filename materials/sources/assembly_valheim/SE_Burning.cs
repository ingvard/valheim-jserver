using System;
using UnityEngine;

// Token: 0x02000023 RID: 35
public class SE_Burning : StatusEffect
{
	// Token: 0x0600039D RID: 925 RVA: 0x0001EF30 File Offset: 0x0001D130
	public override void Setup(Character character)
	{
		base.Setup(character);
	}

	// Token: 0x0600039E RID: 926 RVA: 0x0001EF3C File Offset: 0x0001D13C
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		if (this.m_character.GetSEMan().HaveStatusEffect("Wet"))
		{
			this.m_time += dt * 5f;
		}
		this.m_timer -= dt;
		if (this.m_timer <= 0f)
		{
			this.m_timer = this.m_damageInterval;
			HitData hitData = new HitData();
			hitData.m_point = this.m_character.GetCenterPoint();
			hitData.m_damage = this.m_damage.Clone();
			this.m_character.ApplyDamage(hitData, true, false, HitData.DamageModifier.Normal);
		}
	}

	// Token: 0x0600039F RID: 927 RVA: 0x0001EFDC File Offset: 0x0001D1DC
	public void AddFireDamage(float damage)
	{
		this.m_totalDamage = Mathf.Max(this.m_totalDamage, damage);
		int num = (int)(this.m_ttl / this.m_damageInterval);
		float fire = this.m_totalDamage / (float)num;
		this.m_damage.m_fire = fire;
		this.ResetTime();
	}

	// Token: 0x060003A0 RID: 928 RVA: 0x0001F028 File Offset: 0x0001D228
	public void AddSpiritDamage(float damage)
	{
		this.m_totalDamage = Mathf.Max(this.m_totalDamage, damage);
		int num = (int)(this.m_ttl / this.m_damageInterval);
		float spirit = this.m_totalDamage / (float)num;
		this.m_damage.m_spirit = spirit;
		this.ResetTime();
	}

	// Token: 0x04000383 RID: 899
	[Header("SE_Burning")]
	public float m_damageInterval = 1f;

	// Token: 0x04000384 RID: 900
	private float m_timer;

	// Token: 0x04000385 RID: 901
	private float m_totalDamage;

	// Token: 0x04000386 RID: 902
	private HitData.DamageTypes m_damage;
}
