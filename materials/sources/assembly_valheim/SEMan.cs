using System;
using System.Collections.Generic;

// Token: 0x02000022 RID: 34
public class SEMan
{
	// Token: 0x06000382 RID: 898 RVA: 0x0001E66C File Offset: 0x0001C86C
	public SEMan(Character character, ZNetView nview)
	{
		this.m_character = character;
		this.m_nview = nview;
		this.m_nview.Register<string, bool>("AddStatusEffect", new Action<long, string, bool>(this.RPC_AddStatusEffect));
	}

	// Token: 0x06000383 RID: 899 RVA: 0x0001E6C0 File Offset: 0x0001C8C0
	public void OnDestroy()
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.OnDestroy();
		}
		this.m_statusEffects.Clear();
	}

	// Token: 0x06000384 RID: 900 RVA: 0x0001E71C File Offset: 0x0001C91C
	public void ApplyStatusEffectSpeedMods(ref float speed)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifySpeed(ref speed);
		}
	}

	// Token: 0x06000385 RID: 901 RVA: 0x0001E770 File Offset: 0x0001C970
	public void ApplyDamageMods(ref HitData.DamageModifiers mods)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyDamageMods(ref mods);
		}
	}

	// Token: 0x06000386 RID: 902 RVA: 0x0001E7C4 File Offset: 0x0001C9C4
	public void Update(float dt)
	{
		this.m_statusEffectAttributes = 0;
		int count = this.m_statusEffects.Count;
		for (int i = 0; i < count; i++)
		{
			StatusEffect statusEffect = this.m_statusEffects[i];
			statusEffect.UpdateStatusEffect(dt);
			if (statusEffect.IsDone())
			{
				this.m_removeStatusEffects.Add(statusEffect);
			}
			else
			{
				this.m_statusEffectAttributes |= (int)statusEffect.m_attributes;
			}
		}
		if (this.m_removeStatusEffects.Count > 0)
		{
			foreach (StatusEffect statusEffect2 in this.m_removeStatusEffects)
			{
				statusEffect2.Stop();
				this.m_statusEffects.Remove(statusEffect2);
			}
			this.m_removeStatusEffects.Clear();
		}
		this.m_nview.GetZDO().Set("seAttrib", this.m_statusEffectAttributes);
	}

	// Token: 0x06000387 RID: 903 RVA: 0x0001E8B8 File Offset: 0x0001CAB8
	public StatusEffect AddStatusEffect(string name, bool resetTime = false)
	{
		if (this.m_nview.IsOwner())
		{
			return this.Internal_AddStatusEffect(name, resetTime);
		}
		this.m_nview.InvokeRPC("AddStatusEffect", new object[]
		{
			name,
			resetTime
		});
		return null;
	}

	// Token: 0x06000388 RID: 904 RVA: 0x0001E8F4 File Offset: 0x0001CAF4
	private void RPC_AddStatusEffect(long sender, string name, bool resetTime)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		this.Internal_AddStatusEffect(name, resetTime);
	}

	// Token: 0x06000389 RID: 905 RVA: 0x0001E910 File Offset: 0x0001CB10
	private StatusEffect Internal_AddStatusEffect(string name, bool resetTime)
	{
		StatusEffect statusEffect = this.GetStatusEffect(name);
		if (statusEffect)
		{
			if (resetTime)
			{
				statusEffect.ResetTime();
			}
			return null;
		}
		StatusEffect statusEffect2 = ObjectDB.instance.GetStatusEffect(name);
		if (statusEffect2 == null)
		{
			return null;
		}
		return this.AddStatusEffect(statusEffect2, false);
	}

	// Token: 0x0600038A RID: 906 RVA: 0x0001E958 File Offset: 0x0001CB58
	public StatusEffect AddStatusEffect(StatusEffect statusEffect, bool resetTime = false)
	{
		StatusEffect statusEffect2 = this.GetStatusEffect(statusEffect.name);
		if (statusEffect2)
		{
			if (resetTime)
			{
				statusEffect2.ResetTime();
			}
			return null;
		}
		if (!statusEffect.CanAdd(this.m_character))
		{
			return null;
		}
		StatusEffect statusEffect3 = statusEffect.Clone();
		this.m_statusEffects.Add(statusEffect3);
		statusEffect3.Setup(this.m_character);
		if (this.m_character.IsPlayer())
		{
			Gogan.LogEvent("Game", "StatusEffect", statusEffect.name, 0L);
		}
		return statusEffect3;
	}

	// Token: 0x0600038B RID: 907 RVA: 0x0001E9D9 File Offset: 0x0001CBD9
	public bool RemoveStatusEffect(StatusEffect se, bool quiet = false)
	{
		return this.RemoveStatusEffect(se.name, quiet);
	}

	// Token: 0x0600038C RID: 908 RVA: 0x0001E9E8 File Offset: 0x0001CBE8
	public bool RemoveStatusEffect(string name, bool quiet = false)
	{
		for (int i = 0; i < this.m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = this.m_statusEffects[i];
			if (statusEffect.name == name)
			{
				if (quiet)
				{
					statusEffect.m_stopMessage = "";
				}
				statusEffect.Stop();
				this.m_statusEffects.Remove(statusEffect);
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600038D RID: 909 RVA: 0x0001EA4C File Offset: 0x0001CC4C
	public bool HaveStatusEffectCategory(string cat)
	{
		if (cat.Length == 0)
		{
			return false;
		}
		for (int i = 0; i < this.m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = this.m_statusEffects[i];
			if (statusEffect.m_category.Length > 0 && statusEffect.m_category == cat)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x0600038E RID: 910 RVA: 0x0001EAA8 File Offset: 0x0001CCA8
	public bool HaveStatusAttribute(StatusEffect.StatusAttribute value)
	{
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		if (this.m_nview.IsOwner())
		{
			return (this.m_statusEffectAttributes & (int)value) != 0;
		}
		return (this.m_nview.GetZDO().GetInt("seAttrib", 0) & (int)value) != 0;
	}

	// Token: 0x0600038F RID: 911 RVA: 0x0001EAF8 File Offset: 0x0001CCF8
	public bool HaveStatusEffect(string name)
	{
		for (int i = 0; i < this.m_statusEffects.Count; i++)
		{
			if (this.m_statusEffects[i].name == name)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000390 RID: 912 RVA: 0x0001EB37 File Offset: 0x0001CD37
	public List<StatusEffect> GetStatusEffects()
	{
		return this.m_statusEffects;
	}

	// Token: 0x06000391 RID: 913 RVA: 0x0001EB40 File Offset: 0x0001CD40
	public StatusEffect GetStatusEffect(string name)
	{
		for (int i = 0; i < this.m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = this.m_statusEffects[i];
			if (statusEffect.name == name)
			{
				return statusEffect;
			}
		}
		return null;
	}

	// Token: 0x06000392 RID: 914 RVA: 0x0001EB84 File Offset: 0x0001CD84
	public void GetHUDStatusEffects(List<StatusEffect> effects)
	{
		for (int i = 0; i < this.m_statusEffects.Count; i++)
		{
			StatusEffect statusEffect = this.m_statusEffects[i];
			if (statusEffect.m_icon)
			{
				effects.Add(statusEffect);
			}
		}
	}

	// Token: 0x06000393 RID: 915 RVA: 0x0001EBC8 File Offset: 0x0001CDC8
	public void ModifyNoise(float baseNoise, ref float noise)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyNoise(baseNoise, ref noise);
		}
	}

	// Token: 0x06000394 RID: 916 RVA: 0x0001EC1C File Offset: 0x0001CE1C
	public void ModifyRaiseSkill(Skills.SkillType skill, ref float multiplier)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyRaiseSkill(skill, ref multiplier);
		}
	}

	// Token: 0x06000395 RID: 917 RVA: 0x0001EC70 File Offset: 0x0001CE70
	public void ModifyStaminaRegen(ref float staminaMultiplier)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyStaminaRegen(ref staminaMultiplier);
		}
	}

	// Token: 0x06000396 RID: 918 RVA: 0x0001ECC4 File Offset: 0x0001CEC4
	public void ModifyHealthRegen(ref float regenMultiplier)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyHealthRegen(ref regenMultiplier);
		}
	}

	// Token: 0x06000397 RID: 919 RVA: 0x0001ED18 File Offset: 0x0001CF18
	public void ModifyMaxCarryWeight(float baseLimit, ref float limit)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyMaxCarryWeight(baseLimit, ref limit);
		}
	}

	// Token: 0x06000398 RID: 920 RVA: 0x0001ED6C File Offset: 0x0001CF6C
	public void ModifyStealth(float baseStealth, ref float stealth)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyStealth(baseStealth, ref stealth);
		}
	}

	// Token: 0x06000399 RID: 921 RVA: 0x0001EDC0 File Offset: 0x0001CFC0
	public void ModifyAttack(Skills.SkillType skill, ref HitData hitData)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyAttack(skill, ref hitData);
		}
	}

	// Token: 0x0600039A RID: 922 RVA: 0x0001EE14 File Offset: 0x0001D014
	public void ModifyRunStaminaDrain(float baseDrain, ref float drain)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyRunStaminaDrain(baseDrain, ref drain);
		}
		if (drain < 0f)
		{
			drain = 0f;
		}
	}

	// Token: 0x0600039B RID: 923 RVA: 0x0001EE78 File Offset: 0x0001D078
	public void ModifyJumpStaminaUsage(float baseStaminaUse, ref float staminaUse)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.ModifyJumpStaminaUsage(baseStaminaUse, ref staminaUse);
		}
		if (staminaUse < 0f)
		{
			staminaUse = 0f;
		}
	}

	// Token: 0x0600039C RID: 924 RVA: 0x0001EEDC File Offset: 0x0001D0DC
	public void OnDamaged(HitData hit, Character attacker)
	{
		foreach (StatusEffect statusEffect in this.m_statusEffects)
		{
			statusEffect.OnDamaged(hit, attacker);
		}
	}

	// Token: 0x0400037E RID: 894
	protected List<StatusEffect> m_statusEffects = new List<StatusEffect>();

	// Token: 0x0400037F RID: 895
	private List<StatusEffect> m_removeStatusEffects = new List<StatusEffect>();

	// Token: 0x04000380 RID: 896
	private int m_statusEffectAttributes;

	// Token: 0x04000381 RID: 897
	private Character m_character;

	// Token: 0x04000382 RID: 898
	private ZNetView m_nview;
}
