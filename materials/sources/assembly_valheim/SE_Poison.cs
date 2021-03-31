using System;
using UnityEngine;

// Token: 0x02000029 RID: 41
public class SE_Poison : StatusEffect
{
	// Token: 0x060003B4 RID: 948 RVA: 0x0001F8D0 File Offset: 0x0001DAD0
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		this.m_timer -= dt;
		if (this.m_timer <= 0f)
		{
			this.m_timer = this.m_damageInterval;
			HitData hitData = new HitData();
			hitData.m_point = this.m_character.GetCenterPoint();
			hitData.m_damage.m_poison = this.m_damagePerHit;
			this.m_damageLeft -= this.m_damagePerHit;
			this.m_character.ApplyDamage(hitData, true, false, HitData.DamageModifier.Normal);
		}
	}

	// Token: 0x060003B5 RID: 949 RVA: 0x0001F958 File Offset: 0x0001DB58
	public void AddDamage(float damage)
	{
		if (damage >= this.m_damageLeft)
		{
			this.m_damageLeft = damage;
			float num = this.m_character.IsPlayer() ? this.m_TTLPerDamagePlayer : this.m_TTLPerDamage;
			this.m_ttl = this.m_baseTTL + Mathf.Pow(this.m_damageLeft * num, this.m_TTLPower);
			int num2 = (int)(this.m_ttl / this.m_damageInterval);
			this.m_damagePerHit = this.m_damageLeft / (float)num2;
			ZLog.Log(string.Concat(new object[]
			{
				"Poison damage: ",
				this.m_damageLeft,
				" ttl:",
				this.m_ttl,
				" hits:",
				num2,
				" dmg perhit:",
				this.m_damagePerHit
			}));
			this.ResetTime();
		}
	}

	// Token: 0x040003A6 RID: 934
	[Header("SE_Poison")]
	public float m_damageInterval = 1f;

	// Token: 0x040003A7 RID: 935
	public float m_baseTTL = 2f;

	// Token: 0x040003A8 RID: 936
	public float m_TTLPerDamagePlayer = 2f;

	// Token: 0x040003A9 RID: 937
	public float m_TTLPerDamage = 2f;

	// Token: 0x040003AA RID: 938
	public float m_TTLPower = 0.5f;

	// Token: 0x040003AB RID: 939
	private float m_timer;

	// Token: 0x040003AC RID: 940
	private float m_damageLeft;

	// Token: 0x040003AD RID: 941
	private float m_damagePerHit;
}
