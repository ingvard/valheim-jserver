using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200002E RID: 46
public class SE_Stats : StatusEffect
{
	// Token: 0x060003C8 RID: 968 RVA: 0x0001FE38 File Offset: 0x0001E038
	public override void Setup(Character character)
	{
		base.Setup(character);
		if (this.m_healthOverTime > 0f && this.m_healthOverTimeInterval > 0f)
		{
			if (this.m_healthOverTimeDuration <= 0f)
			{
				this.m_healthOverTimeDuration = this.m_ttl;
			}
			this.m_healthOverTimeTicks = this.m_healthOverTimeDuration / this.m_healthOverTimeInterval;
			this.m_healthOverTimeTickHP = this.m_healthOverTime / this.m_healthOverTimeTicks;
		}
		if (this.m_staminaOverTime > 0f && this.m_staminaOverTimeDuration <= 0f)
		{
			this.m_staminaOverTimeDuration = this.m_ttl;
		}
	}

	// Token: 0x060003C9 RID: 969 RVA: 0x0001FECC File Offset: 0x0001E0CC
	public override void UpdateStatusEffect(float dt)
	{
		base.UpdateStatusEffect(dt);
		if (this.m_tickInterval > 0f)
		{
			this.m_tickTimer += dt;
			if (this.m_tickTimer >= this.m_tickInterval)
			{
				this.m_tickTimer = 0f;
				if (this.m_character.GetHealthPercentage() >= this.m_healthPerTickMinHealthPercentage)
				{
					if (this.m_healthPerTick > 0f)
					{
						this.m_character.Heal(this.m_healthPerTick, true);
					}
					else
					{
						HitData hitData = new HitData();
						hitData.m_damage.m_damage = -this.m_healthPerTick;
						hitData.m_point = this.m_character.GetTopPoint();
						this.m_character.Damage(hitData);
					}
				}
			}
		}
		if (this.m_healthOverTimeTicks > 0f)
		{
			this.m_healthOverTimeTimer += dt;
			if (this.m_healthOverTimeTimer > this.m_healthOverTimeInterval)
			{
				this.m_healthOverTimeTimer = 0f;
				this.m_healthOverTimeTicks -= 1f;
				this.m_character.Heal(this.m_healthOverTimeTickHP, true);
			}
		}
		if (this.m_staminaOverTime != 0f && this.m_time <= this.m_staminaOverTimeDuration)
		{
			float num = this.m_staminaOverTimeDuration / dt;
			this.m_character.AddStamina(this.m_staminaOverTime / num);
		}
		if (this.m_staminaDrainPerSec > 0f)
		{
			this.m_character.UseStamina(this.m_staminaDrainPerSec * dt);
		}
	}

	// Token: 0x060003CA RID: 970 RVA: 0x0002002F File Offset: 0x0001E22F
	public override void ModifyHealthRegen(ref float regenMultiplier)
	{
		if (this.m_healthRegenMultiplier > 1f)
		{
			regenMultiplier += this.m_healthRegenMultiplier - 1f;
			return;
		}
		regenMultiplier *= this.m_healthRegenMultiplier;
	}

	// Token: 0x060003CB RID: 971 RVA: 0x0002005B File Offset: 0x0001E25B
	public override void ModifyStaminaRegen(ref float staminaRegen)
	{
		if (this.m_staminaRegenMultiplier > 1f)
		{
			staminaRegen += this.m_staminaRegenMultiplier - 1f;
			return;
		}
		staminaRegen *= this.m_staminaRegenMultiplier;
	}

	// Token: 0x060003CC RID: 972 RVA: 0x00020087 File Offset: 0x0001E287
	public override void ModifyDamageMods(ref HitData.DamageModifiers modifiers)
	{
		modifiers.Apply(this.m_mods);
	}

	// Token: 0x060003CD RID: 973 RVA: 0x00020095 File Offset: 0x0001E295
	public override void ModifyRaiseSkill(Skills.SkillType skill, ref float value)
	{
		if (this.m_raiseSkill == Skills.SkillType.None)
		{
			return;
		}
		if (this.m_raiseSkill == Skills.SkillType.All || this.m_raiseSkill == skill)
		{
			value += this.m_raiseSkillModifier;
		}
	}

	// Token: 0x060003CE RID: 974 RVA: 0x000200C1 File Offset: 0x0001E2C1
	public override void ModifyNoise(float baseNoise, ref float noise)
	{
		noise += baseNoise * this.m_noiseModifier;
	}

	// Token: 0x060003CF RID: 975 RVA: 0x000200D0 File Offset: 0x0001E2D0
	public override void ModifyStealth(float baseStealth, ref float stealth)
	{
		stealth += baseStealth * this.m_stealthModifier;
	}

	// Token: 0x060003D0 RID: 976 RVA: 0x000200DF File Offset: 0x0001E2DF
	public override void ModifyMaxCarryWeight(float baseLimit, ref float limit)
	{
		limit += this.m_addMaxCarryWeight;
		if (limit < 0f)
		{
			limit = 0f;
		}
	}

	// Token: 0x060003D1 RID: 977 RVA: 0x000200FC File Offset: 0x0001E2FC
	public override void ModifyAttack(Skills.SkillType skill, ref HitData hitData)
	{
		if (skill == this.m_modifyAttackSkill || this.m_modifyAttackSkill == Skills.SkillType.All)
		{
			hitData.m_damage.Modify(this.m_damageModifier);
		}
	}

	// Token: 0x060003D2 RID: 978 RVA: 0x00020126 File Offset: 0x0001E326
	public override void ModifyRunStaminaDrain(float baseDrain, ref float drain)
	{
		drain += baseDrain * this.m_runStaminaDrainModifier;
	}

	// Token: 0x060003D3 RID: 979 RVA: 0x00020135 File Offset: 0x0001E335
	public override void ModifyJumpStaminaUsage(float baseStaminaUse, ref float staminaUse)
	{
		staminaUse += baseStaminaUse * this.m_jumpStaminaUseModifier;
	}

	// Token: 0x060003D4 RID: 980 RVA: 0x00020144 File Offset: 0x0001E344
	public override string GetTooltipString()
	{
		string text = "";
		if (this.m_tooltip.Length > 0)
		{
			text = text + this.m_tooltip + "\n";
		}
		if (this.m_jumpStaminaUseModifier != 0f)
		{
			text = text + "$se_jumpstamina: " + (this.m_jumpStaminaUseModifier * 100f).ToString("+0;-0") + "%\n";
		}
		if (this.m_runStaminaDrainModifier != 0f)
		{
			text = text + "$se_runstamina: " + (this.m_runStaminaDrainModifier * 100f).ToString("+0;-0") + "%\n";
		}
		if (this.m_healthOverTime != 0f)
		{
			text = text + "$se_health: " + this.m_healthOverTime.ToString() + "\n";
		}
		if (this.m_staminaOverTime != 0f)
		{
			text = text + "$se_stamina: " + this.m_staminaOverTime.ToString() + "\n";
		}
		if (this.m_healthRegenMultiplier != 1f)
		{
			text = text + "$se_healthregen " + ((this.m_healthRegenMultiplier - 1f) * 100f).ToString("+0;-0") + "%\n";
		}
		if (this.m_staminaRegenMultiplier != 1f)
		{
			text = text + "$se_staminaregen " + ((this.m_staminaRegenMultiplier - 1f) * 100f).ToString("+0;-0") + "%\n";
		}
		if (this.m_addMaxCarryWeight != 0f)
		{
			text = text + "$se_max_carryweight " + this.m_addMaxCarryWeight.ToString("+0;-0") + "\n";
		}
		if (this.m_mods.Count > 0)
		{
			text += SE_Stats.GetDamageModifiersTooltipString(this.m_mods);
		}
		if (this.m_noiseModifier != 0f)
		{
			text = text + "$se_noisemod " + (this.m_noiseModifier * 100f).ToString("+0;-0") + "%\n";
		}
		if (this.m_stealthModifier != 0f)
		{
			text = text + "$se_sneakmod " + (-this.m_stealthModifier * 100f).ToString("+0;-0") + "%\n";
		}
		return text;
	}

	// Token: 0x060003D5 RID: 981 RVA: 0x00020370 File Offset: 0x0001E570
	public static string GetDamageModifiersTooltipString(List<HitData.DamageModPair> mods)
	{
		if (mods.Count == 0)
		{
			return "";
		}
		string text = "";
		foreach (HitData.DamageModPair damageModPair in mods)
		{
			if (damageModPair.m_modifier != HitData.DamageModifier.Ignore && damageModPair.m_modifier != HitData.DamageModifier.Normal)
			{
				switch (damageModPair.m_modifier)
				{
				case HitData.DamageModifier.Resistant:
					text += "\n$inventory_dmgmod: <color=orange>$inventory_resistant</color> VS ";
					break;
				case HitData.DamageModifier.Weak:
					text += "\n$inventory_dmgmod: <color=orange>$inventory_weak</color> VS ";
					break;
				case HitData.DamageModifier.Immune:
					text += "\n$inventory_dmgmod: <color=orange>$inventory_immune</color> VS ";
					break;
				case HitData.DamageModifier.VeryResistant:
					text += "\n$inventory_dmgmod: <color=orange>$inventory_veryresistant</color> VS ";
					break;
				case HitData.DamageModifier.VeryWeak:
					text += "\n$inventory_dmgmod: <color=orange>$inventory_veryweak</color> VS ";
					break;
				}
				text += "<color=orange>";
				HitData.DamageType type = damageModPair.m_type;
				if (type <= HitData.DamageType.Fire)
				{
					if (type <= HitData.DamageType.Chop)
					{
						switch (type)
						{
						case HitData.DamageType.Blunt:
							text += "$inventory_blunt";
							break;
						case HitData.DamageType.Slash:
							text += "$inventory_slash";
							break;
						case HitData.DamageType.Blunt | HitData.DamageType.Slash:
							break;
						case HitData.DamageType.Pierce:
							text += "$inventory_pierce";
							break;
						default:
							if (type == HitData.DamageType.Chop)
							{
								text += "$inventory_chop";
							}
							break;
						}
					}
					else if (type != HitData.DamageType.Pickaxe)
					{
						if (type == HitData.DamageType.Fire)
						{
							text += "$inventory_fire";
						}
					}
					else
					{
						text += "$inventory_pickaxe";
					}
				}
				else if (type <= HitData.DamageType.Lightning)
				{
					if (type != HitData.DamageType.Frost)
					{
						if (type == HitData.DamageType.Lightning)
						{
							text += "$inventory_lightning";
						}
					}
					else
					{
						text += "$inventory_frost";
					}
				}
				else if (type != HitData.DamageType.Poison)
				{
					if (type == HitData.DamageType.Spirit)
					{
						text += "$inventory_spirit";
					}
				}
				else
				{
					text += "$inventory_poison";
				}
				text += "</color>";
			}
		}
		return text;
	}

	// Token: 0x040003BB RID: 955
	[Header("__SE_Stats__")]
	[Header("HP per tick")]
	public float m_tickInterval;

	// Token: 0x040003BC RID: 956
	public float m_healthPerTickMinHealthPercentage;

	// Token: 0x040003BD RID: 957
	public float m_healthPerTick;

	// Token: 0x040003BE RID: 958
	[Header("Health over time")]
	public float m_healthOverTime;

	// Token: 0x040003BF RID: 959
	public float m_healthOverTimeDuration;

	// Token: 0x040003C0 RID: 960
	public float m_healthOverTimeInterval = 5f;

	// Token: 0x040003C1 RID: 961
	[Header("Stamina")]
	public float m_staminaOverTime;

	// Token: 0x040003C2 RID: 962
	public float m_staminaOverTimeDuration;

	// Token: 0x040003C3 RID: 963
	public float m_staminaDrainPerSec;

	// Token: 0x040003C4 RID: 964
	public float m_runStaminaDrainModifier;

	// Token: 0x040003C5 RID: 965
	public float m_jumpStaminaUseModifier;

	// Token: 0x040003C6 RID: 966
	[Header("Regen modifiers")]
	public float m_healthRegenMultiplier = 1f;

	// Token: 0x040003C7 RID: 967
	public float m_staminaRegenMultiplier = 1f;

	// Token: 0x040003C8 RID: 968
	[Header("Modify raise skill")]
	public Skills.SkillType m_raiseSkill;

	// Token: 0x040003C9 RID: 969
	public float m_raiseSkillModifier;

	// Token: 0x040003CA RID: 970
	[Header("Hit modifier")]
	public List<HitData.DamageModPair> m_mods = new List<HitData.DamageModPair>();

	// Token: 0x040003CB RID: 971
	[Header("Attack")]
	public Skills.SkillType m_modifyAttackSkill;

	// Token: 0x040003CC RID: 972
	public float m_damageModifier = 1f;

	// Token: 0x040003CD RID: 973
	[Header("Sneak")]
	public float m_noiseModifier;

	// Token: 0x040003CE RID: 974
	public float m_stealthModifier;

	// Token: 0x040003CF RID: 975
	[Header("Carry weight")]
	public float m_addMaxCarryWeight;

	// Token: 0x040003D0 RID: 976
	private float m_tickTimer;

	// Token: 0x040003D1 RID: 977
	private float m_healthOverTimeTimer;

	// Token: 0x040003D2 RID: 978
	private float m_healthOverTimeTicks;

	// Token: 0x040003D3 RID: 979
	private float m_healthOverTimeTickHP;
}
