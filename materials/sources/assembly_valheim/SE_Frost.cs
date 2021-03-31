using System;
using UnityEngine;

// Token: 0x02000026 RID: 38
public class SE_Frost : StatusEffect
{
	// Token: 0x060003A8 RID: 936 RVA: 0x0001F380 File Offset: 0x0001D580
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
	}

	// Token: 0x060003A9 RID: 937 RVA: 0x0001F38C File Offset: 0x0001D58C
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

	// Token: 0x060003AA RID: 938 RVA: 0x0001F3F0 File Offset: 0x0001D5F0
	public override void ModifySpeed(ref float speed)
	{
		float num = Mathf.Clamp01(this.m_time / this.m_ttl);
		num = Mathf.Pow(num, 2f);
		speed *= Mathf.Clamp(num, this.m_minSpeedFactor, 1f);
	}

	// Token: 0x04000396 RID: 918
	[Header("SE_Frost")]
	public float m_freezeTimeEnemy = 10f;

	// Token: 0x04000397 RID: 919
	public float m_freezeTimePlayer = 10f;

	// Token: 0x04000398 RID: 920
	public float m_minSpeedFactor = 0.1f;
}
