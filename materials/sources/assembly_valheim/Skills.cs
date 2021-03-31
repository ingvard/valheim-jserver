using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000014 RID: 20
public class Skills : MonoBehaviour
{
	// Token: 0x0600026B RID: 619 RVA: 0x00013A4F File Offset: 0x00011C4F
	public void Awake()
	{
		this.m_player = base.GetComponent<Player>();
	}

	// Token: 0x0600026C RID: 620 RVA: 0x00013A60 File Offset: 0x00011C60
	public void Save(ZPackage pkg)
	{
		pkg.Write(2);
		pkg.Write(this.m_skillData.Count);
		foreach (KeyValuePair<Skills.SkillType, Skills.Skill> keyValuePair in this.m_skillData)
		{
			pkg.Write((int)keyValuePair.Value.m_info.m_skill);
			pkg.Write(keyValuePair.Value.m_level);
			pkg.Write(keyValuePair.Value.m_accumulator);
		}
	}

	// Token: 0x0600026D RID: 621 RVA: 0x00013B00 File Offset: 0x00011D00
	public void Load(ZPackage pkg)
	{
		int num = pkg.ReadInt();
		this.m_skillData.Clear();
		int num2 = pkg.ReadInt();
		for (int i = 0; i < num2; i++)
		{
			Skills.SkillType skillType = (Skills.SkillType)pkg.ReadInt();
			float level = pkg.ReadSingle();
			float accumulator = (num >= 2) ? pkg.ReadSingle() : 0f;
			if (this.IsSkillValid(skillType))
			{
				Skills.Skill skill = this.GetSkill(skillType);
				skill.m_level = level;
				skill.m_accumulator = accumulator;
			}
		}
	}

	// Token: 0x0600026E RID: 622 RVA: 0x00013B73 File Offset: 0x00011D73
	private bool IsSkillValid(Skills.SkillType type)
	{
		return Enum.IsDefined(typeof(Skills.SkillType), type);
	}

	// Token: 0x0600026F RID: 623 RVA: 0x00013B8A File Offset: 0x00011D8A
	public float GetSkillFactor(Skills.SkillType skillType)
	{
		if (skillType == Skills.SkillType.None)
		{
			return 0f;
		}
		return this.GetSkill(skillType).m_level / 100f;
	}

	// Token: 0x06000270 RID: 624 RVA: 0x00013BA8 File Offset: 0x00011DA8
	public void GetRandomSkillRange(out float min, out float max, Skills.SkillType skillType)
	{
		float skillFactor = this.GetSkillFactor(skillType);
		float num = Mathf.Lerp(0.4f, 1f, skillFactor);
		min = Mathf.Clamp01(num - 0.15f);
		max = Mathf.Clamp01(num + 0.15f);
	}

	// Token: 0x06000271 RID: 625 RVA: 0x00013BEC File Offset: 0x00011DEC
	public float GetRandomSkillFactor(Skills.SkillType skillType)
	{
		float skillFactor = this.GetSkillFactor(skillType);
		float num = Mathf.Lerp(0.4f, 1f, skillFactor);
		float a = Mathf.Clamp01(num - 0.15f);
		float b = Mathf.Clamp01(num + 0.15f);
		return Mathf.Lerp(a, b, UnityEngine.Random.value);
	}

	// Token: 0x06000272 RID: 626 RVA: 0x00013C38 File Offset: 0x00011E38
	public void CheatRaiseSkill(string name, float value)
	{
		foreach (object obj in Enum.GetValues(typeof(Skills.SkillType)))
		{
			Skills.SkillType skillType = (Skills.SkillType)obj;
			if (skillType.ToString().ToLower() == name)
			{
				Skills.Skill skill = this.GetSkill(skillType);
				skill.m_level += value;
				skill.m_level = Mathf.Clamp(skill.m_level, 0f, 100f);
				if (this.m_useSkillCap)
				{
					this.RebalanceSkills(skillType);
				}
				this.m_player.Message(MessageHud.MessageType.TopLeft, string.Concat(new object[]
				{
					"Skill incresed ",
					skill.m_info.m_skill.ToString(),
					": ",
					(int)skill.m_level
				}), 0, skill.m_info.m_icon);
				global::Console.instance.Print("Skill " + skillType.ToString() + " = " + skill.m_level.ToString());
				return;
			}
		}
		global::Console.instance.Print("Skill not found " + name);
	}

	// Token: 0x06000273 RID: 627 RVA: 0x00013DA8 File Offset: 0x00011FA8
	public void CheatResetSkill(string name)
	{
		foreach (object obj in Enum.GetValues(typeof(Skills.SkillType)))
		{
			Skills.SkillType skillType = (Skills.SkillType)obj;
			if (skillType.ToString().ToLower() == name)
			{
				this.ResetSkill(skillType);
				global::Console.instance.Print("Skill " + skillType.ToString() + " reset");
				return;
			}
		}
		global::Console.instance.Print("Skill not found " + name);
	}

	// Token: 0x06000274 RID: 628 RVA: 0x00013E64 File Offset: 0x00012064
	public void ResetSkill(Skills.SkillType skillType)
	{
		this.m_skillData.Remove(skillType);
	}

	// Token: 0x06000275 RID: 629 RVA: 0x00013E74 File Offset: 0x00012074
	public void RaiseSkill(Skills.SkillType skillType, float factor = 1f)
	{
		if (skillType == Skills.SkillType.None)
		{
			return;
		}
		Skills.Skill skill = this.GetSkill(skillType);
		float level = skill.m_level;
		if (skill.Raise(factor))
		{
			if (this.m_useSkillCap)
			{
				this.RebalanceSkills(skillType);
			}
			this.m_player.OnSkillLevelup(skillType, skill.m_level);
			MessageHud.MessageType type = ((int)level == 0) ? MessageHud.MessageType.Center : MessageHud.MessageType.TopLeft;
			this.m_player.Message(type, string.Concat(new object[]
			{
				"$msg_skillup $skill_",
				skill.m_info.m_skill.ToString().ToLower(),
				": ",
				(int)skill.m_level
			}), 0, skill.m_info.m_icon);
			Gogan.LogEvent("Game", "Levelup", skillType.ToString(), (long)((int)skill.m_level));
		}
	}

	// Token: 0x06000276 RID: 630 RVA: 0x00013F50 File Offset: 0x00012150
	private void RebalanceSkills(Skills.SkillType skillType)
	{
		if (this.GetTotalSkill() < this.m_totalSkillCap)
		{
			return;
		}
		float level = this.GetSkill(skillType).m_level;
		float num = this.m_totalSkillCap - level;
		float num2 = 0f;
		foreach (KeyValuePair<Skills.SkillType, Skills.Skill> keyValuePair in this.m_skillData)
		{
			if (keyValuePair.Key != skillType)
			{
				num2 += keyValuePair.Value.m_level;
			}
		}
		foreach (KeyValuePair<Skills.SkillType, Skills.Skill> keyValuePair2 in this.m_skillData)
		{
			if (keyValuePair2.Key != skillType)
			{
				keyValuePair2.Value.m_level = keyValuePair2.Value.m_level / num2 * num;
			}
		}
	}

	// Token: 0x06000277 RID: 631 RVA: 0x00014044 File Offset: 0x00012244
	public void Clear()
	{
		this.m_skillData.Clear();
	}

	// Token: 0x06000278 RID: 632 RVA: 0x00014051 File Offset: 0x00012251
	public void OnDeath()
	{
		this.LowerAllSkills(this.m_DeathLowerFactor);
	}

	// Token: 0x06000279 RID: 633 RVA: 0x00014060 File Offset: 0x00012260
	public void LowerAllSkills(float factor)
	{
		foreach (KeyValuePair<Skills.SkillType, Skills.Skill> keyValuePair in this.m_skillData)
		{
			float num = keyValuePair.Value.m_level * factor;
			keyValuePair.Value.m_level -= num;
			keyValuePair.Value.m_accumulator = 0f;
		}
		this.m_player.Message(MessageHud.MessageType.TopLeft, "$msg_skills_lowered", 0, null);
	}

	// Token: 0x0600027A RID: 634 RVA: 0x000140F4 File Offset: 0x000122F4
	private Skills.Skill GetSkill(Skills.SkillType skillType)
	{
		Skills.Skill skill;
		if (this.m_skillData.TryGetValue(skillType, out skill))
		{
			return skill;
		}
		skill = new Skills.Skill(this.GetSkillDef(skillType));
		this.m_skillData.Add(skillType, skill);
		return skill;
	}

	// Token: 0x0600027B RID: 635 RVA: 0x00014130 File Offset: 0x00012330
	private Skills.SkillDef GetSkillDef(Skills.SkillType type)
	{
		foreach (Skills.SkillDef skillDef in this.m_skills)
		{
			if (skillDef.m_skill == type)
			{
				return skillDef;
			}
		}
		return null;
	}

	// Token: 0x0600027C RID: 636 RVA: 0x0001418C File Offset: 0x0001238C
	public List<Skills.Skill> GetSkillList()
	{
		List<Skills.Skill> list = new List<Skills.Skill>();
		foreach (KeyValuePair<Skills.SkillType, Skills.Skill> keyValuePair in this.m_skillData)
		{
			list.Add(keyValuePair.Value);
		}
		return list;
	}

	// Token: 0x0600027D RID: 637 RVA: 0x000141EC File Offset: 0x000123EC
	public float GetTotalSkill()
	{
		float num = 0f;
		foreach (KeyValuePair<Skills.SkillType, Skills.Skill> keyValuePair in this.m_skillData)
		{
			num += keyValuePair.Value.m_level;
		}
		return num;
	}

	// Token: 0x0600027E RID: 638 RVA: 0x00014250 File Offset: 0x00012450
	public float GetTotalSkillCap()
	{
		return this.m_totalSkillCap;
	}

	// Token: 0x040001E0 RID: 480
	private const int dataVersion = 2;

	// Token: 0x040001E1 RID: 481
	private const float randomSkillRange = 0.15f;

	// Token: 0x040001E2 RID: 482
	private const float randomSkillMin = 0.4f;

	// Token: 0x040001E3 RID: 483
	public const float m_maxSkillLevel = 100f;

	// Token: 0x040001E4 RID: 484
	public const float m_skillCurve = 2f;

	// Token: 0x040001E5 RID: 485
	public bool m_useSkillCap;

	// Token: 0x040001E6 RID: 486
	public float m_totalSkillCap = 600f;

	// Token: 0x040001E7 RID: 487
	public List<Skills.SkillDef> m_skills = new List<Skills.SkillDef>();

	// Token: 0x040001E8 RID: 488
	public float m_DeathLowerFactor = 0.25f;

	// Token: 0x040001E9 RID: 489
	private Dictionary<Skills.SkillType, Skills.Skill> m_skillData = new Dictionary<Skills.SkillType, Skills.Skill>();

	// Token: 0x040001EA RID: 490
	private Player m_player;

	// Token: 0x0200012A RID: 298
	public enum SkillType
	{
		// Token: 0x04001009 RID: 4105
		None,
		// Token: 0x0400100A RID: 4106
		Swords,
		// Token: 0x0400100B RID: 4107
		Knives,
		// Token: 0x0400100C RID: 4108
		Clubs,
		// Token: 0x0400100D RID: 4109
		Polearms,
		// Token: 0x0400100E RID: 4110
		Spears,
		// Token: 0x0400100F RID: 4111
		Blocking,
		// Token: 0x04001010 RID: 4112
		Axes,
		// Token: 0x04001011 RID: 4113
		Bows,
		// Token: 0x04001012 RID: 4114
		FireMagic,
		// Token: 0x04001013 RID: 4115
		FrostMagic,
		// Token: 0x04001014 RID: 4116
		Unarmed,
		// Token: 0x04001015 RID: 4117
		Pickaxes,
		// Token: 0x04001016 RID: 4118
		WoodCutting,
		// Token: 0x04001017 RID: 4119
		Jump = 100,
		// Token: 0x04001018 RID: 4120
		Sneak,
		// Token: 0x04001019 RID: 4121
		Run,
		// Token: 0x0400101A RID: 4122
		Swim,
		// Token: 0x0400101B RID: 4123
		All = 999
	}

	// Token: 0x0200012B RID: 299
	[Serializable]
	public class SkillDef
	{
		// Token: 0x0400101C RID: 4124
		public Skills.SkillType m_skill = Skills.SkillType.Swords;

		// Token: 0x0400101D RID: 4125
		public Sprite m_icon;

		// Token: 0x0400101E RID: 4126
		public string m_description = "";

		// Token: 0x0400101F RID: 4127
		public float m_increseStep = 1f;
	}

	// Token: 0x0200012C RID: 300
	public class Skill
	{
		// Token: 0x060010C0 RID: 4288 RVA: 0x00076C7B File Offset: 0x00074E7B
		public Skill(Skills.SkillDef info)
		{
			this.m_info = info;
		}

		// Token: 0x060010C1 RID: 4289 RVA: 0x00076C8C File Offset: 0x00074E8C
		public bool Raise(float factor)
		{
			if (this.m_level >= 100f)
			{
				return false;
			}
			float num = this.m_info.m_increseStep * factor;
			this.m_accumulator += num;
			float nextLevelRequirement = this.GetNextLevelRequirement();
			if (this.m_accumulator >= nextLevelRequirement)
			{
				this.m_level += 1f;
				this.m_level = Mathf.Clamp(this.m_level, 0f, 100f);
				this.m_accumulator = 0f;
				return true;
			}
			return false;
		}

		// Token: 0x060010C2 RID: 4290 RVA: 0x00076D0F File Offset: 0x00074F0F
		private float GetNextLevelRequirement()
		{
			return Mathf.Pow(this.m_level + 1f, 1.5f) * 0.5f + 0.5f;
		}

		// Token: 0x060010C3 RID: 4291 RVA: 0x00076D34 File Offset: 0x00074F34
		public float GetLevelPercentage()
		{
			if (this.m_level >= 100f)
			{
				return 0f;
			}
			float nextLevelRequirement = this.GetNextLevelRequirement();
			return Mathf.Clamp01(this.m_accumulator / nextLevelRequirement);
		}

		// Token: 0x04001020 RID: 4128
		public Skills.SkillDef m_info;

		// Token: 0x04001021 RID: 4129
		public float m_level;

		// Token: 0x04001022 RID: 4130
		public float m_accumulator;
	}
}
