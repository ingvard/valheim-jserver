using System;
using UnityEngine;

// Token: 0x0200002B RID: 43
public class SE_Shield : StatusEffect
{
	// Token: 0x060003BF RID: 959 RVA: 0x0001EE7C File Offset: 0x0001D07C
	public override void Setup(Character character)
	{
		base.Setup(character);
	}

	// Token: 0x060003C0 RID: 960 RVA: 0x0001FBE4 File Offset: 0x0001DDE4
	public override bool IsDone()
	{
		if (this.m_damage > this.m_absorbDamage)
		{
			this.m_breakEffects.Create(this.m_character.GetCenterPoint(), this.m_character.transform.rotation, this.m_character.transform, this.m_character.GetRadius() * 2f);
			return true;
		}
		return base.IsDone();
	}

	// Token: 0x060003C1 RID: 961 RVA: 0x0001FC4C File Offset: 0x0001DE4C
	public override void OnDamaged(HitData hit, Character attacker)
	{
		float totalDamage = hit.GetTotalDamage();
		this.m_damage += totalDamage;
		hit.ApplyModifier(0f);
		this.m_hitEffects.Create(hit.m_point, Quaternion.identity, null, 1f);
	}

	// Token: 0x040003AF RID: 943
	[Header("__SE_Shield__")]
	public float m_absorbDamage = 100f;

	// Token: 0x040003B0 RID: 944
	public EffectList m_breakEffects = new EffectList();

	// Token: 0x040003B1 RID: 945
	public EffectList m_hitEffects = new EffectList();

	// Token: 0x040003B2 RID: 946
	private float m_damage;
}
