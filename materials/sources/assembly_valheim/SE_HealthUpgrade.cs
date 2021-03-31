using System;
using UnityEngine;

// Token: 0x02000028 RID: 40
public class SE_HealthUpgrade : StatusEffect
{
	// Token: 0x060003B1 RID: 945 RVA: 0x0001EF30 File Offset: 0x0001D130
	public override void Setup(Character character)
	{
		base.Setup(character);
	}

	// Token: 0x060003B2 RID: 946 RVA: 0x0001F810 File Offset: 0x0001DA10
	public override void Stop()
	{
		base.Stop();
		Player player = this.m_character as Player;
		if (!player)
		{
			return;
		}
		if (this.m_moreHealth > 0f)
		{
			player.SetMaxHealth(this.m_character.GetMaxHealth() + this.m_moreHealth, true);
			player.SetHealth(this.m_character.GetMaxHealth());
		}
		if (this.m_moreStamina > 0f)
		{
			player.SetMaxStamina(this.m_character.GetMaxStamina() + this.m_moreStamina, true);
		}
		this.m_upgradeEffect.Create(this.m_character.transform.position, Quaternion.identity, null, 1f);
	}

	// Token: 0x040003A3 RID: 931
	[Header("Health")]
	public float m_moreHealth;

	// Token: 0x040003A4 RID: 932
	[Header("Stamina")]
	public float m_moreStamina;

	// Token: 0x040003A5 RID: 933
	public EffectList m_upgradeEffect = new EffectList();
}
