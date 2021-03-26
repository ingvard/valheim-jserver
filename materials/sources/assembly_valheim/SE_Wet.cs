using System;
using UnityEngine;

// Token: 0x0200002F RID: 47
public class SE_Wet : SE_Stats
{
	// Token: 0x060003D7 RID: 983 RVA: 0x000205D3 File Offset: 0x0001E7D3
	public override void Setup(Character character)
	{
		base.Setup(character);
	}

	// Token: 0x060003D8 RID: 984 RVA: 0x000205DC File Offset: 0x0001E7DC
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		if (!this.m_character.m_tolerateWater)
		{
			this.m_timer += dt;
			if (this.m_timer > this.m_damageInterval)
			{
				this.m_timer = 0f;
				HitData hitData = new HitData();
				hitData.m_point = this.m_character.transform.position;
				hitData.m_damage.m_damage = this.m_waterDamage;
				this.m_character.Damage(hitData);
			}
		}
		if (this.m_character.GetSEMan().HaveStatusEffect("CampFire"))
		{
			this.m_time += dt * 10f;
		}
		if (this.m_character.GetSEMan().HaveStatusEffect("Burning"))
		{
			this.m_time += dt * 50f;
		}
	}

	// Token: 0x040003D4 RID: 980
	[Header("__SE_Wet__")]
	public float m_waterDamage;

	// Token: 0x040003D5 RID: 981
	public float m_damageInterval = 0.5f;

	// Token: 0x040003D6 RID: 982
	private float m_timer;
}
