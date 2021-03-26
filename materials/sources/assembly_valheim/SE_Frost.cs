using System;
using UnityEngine;

// Token: 0x02000026 RID: 38
public class SE_Frost : StatusEffect
{
	// Token: 0x060003A7 RID: 935 RVA: 0x0001F2CC File Offset: 0x0001D4CC
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
	}

	// Token: 0x060003A8 RID: 936 RVA: 0x0001F2D8 File Offset: 0x0001D4D8
	public void AddDamage(float damage)
	{
		float num = this.m_character.IsPlayer() ? this.m_freezeTimePlayer : this.m_freezeTimeEnemy;
		float num2 = Mathf.Clamp01(damage / this.m_character.GetMaxHealth()) * num;
		float num3 = this.m_ttl - this.m_time;
		if (num2 > num3)
		{
			this.m_ttl = num2;
			this.ResetTime();
			base.TriggerStartEffects();
		}
	}

	// Token: 0x060003A9 RID: 937 RVA: 0x0001F33C File Offset: 0x0001D53C
	public override void ModifySpeed(ref float speed)
	{
		float num = Mathf.Clamp01(this.m_time / this.m_ttl);
		num = Mathf.Pow(num, 2f);
		speed *= Mathf.Clamp(num, this.m_minSpeedFactor, 1f);
	}

	// Token: 0x04000392 RID: 914
	[Header("SE_Frost")]
	public float m_freezeTimeEnemy = 10f;

	// Token: 0x04000393 RID: 915
	public float m_freezeTimePlayer = 10f;

	// Token: 0x04000394 RID: 916
	public float m_minSpeedFactor = 0.1f;
}
