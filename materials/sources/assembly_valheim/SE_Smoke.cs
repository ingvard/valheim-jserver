using System;
using UnityEngine;

// Token: 0x0200002C RID: 44
public class SE_Smoke : StatusEffect
{
	// Token: 0x060003C4 RID: 964 RVA: 0x0001FD73 File Offset: 0x0001DF73
	public override bool CanAdd(Character character)
	{
		return !character.m_tolerateSmoke && base.CanAdd(character);
	}

	// Token: 0x060003C5 RID: 965 RVA: 0x0001FD88 File Offset: 0x0001DF88
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		this.m_timer += dt;
		if (this.m_timer > this.m_damageInterval)
		{
			this.m_timer = 0f;
			HitData hitData = new HitData();
			hitData.m_point = this.m_character.GetCenterPoint();
			hitData.m_damage = this.m_damage;
			this.m_character.ApplyDamage(hitData, true, false, HitData.DamageModifier.Normal);
		}
	}

	// Token: 0x040003B7 RID: 951
	[Header("SE_Burning")]
	public HitData.DamageTypes m_damage;

	// Token: 0x040003B8 RID: 952
	public float m_damageInterval = 1f;

	// Token: 0x040003B9 RID: 953
	private float m_timer;
}
