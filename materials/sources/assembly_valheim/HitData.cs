using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000D4 RID: 212
public class HitData
{
	// Token: 0x06000DBB RID: 3515 RVA: 0x000622B4 File Offset: 0x000604B4
	public void Serialize(ref ZPackage pkg)
	{
		pkg.Write(this.m_damage.m_damage);
		pkg.Write(this.m_damage.m_blunt);
		pkg.Write(this.m_damage.m_slash);
		pkg.Write(this.m_damage.m_pierce);
		pkg.Write(this.m_damage.m_chop);
		pkg.Write(this.m_damage.m_pickaxe);
		pkg.Write(this.m_damage.m_fire);
		pkg.Write(this.m_damage.m_frost);
		pkg.Write(this.m_damage.m_lightning);
		pkg.Write(this.m_damage.m_poison);
		pkg.Write(this.m_damage.m_spirit);
		pkg.Write(this.m_toolTier);
		pkg.Write(this.m_pushForce);
		pkg.Write(this.m_backstabBonus);
		pkg.Write(this.m_staggerMultiplier);
		pkg.Write(this.m_dodgeable);
		pkg.Write(this.m_blockable);
		pkg.Write(this.m_point);
		pkg.Write(this.m_dir);
		pkg.Write(this.m_statusEffect);
		pkg.Write(this.m_attacker);
		pkg.Write((int)this.m_skill);
	}

	// Token: 0x06000DBC RID: 3516 RVA: 0x00062418 File Offset: 0x00060618
	public void Deserialize(ref ZPackage pkg)
	{
		this.m_damage.m_damage = pkg.ReadSingle();
		this.m_damage.m_blunt = pkg.ReadSingle();
		this.m_damage.m_slash = pkg.ReadSingle();
		this.m_damage.m_pierce = pkg.ReadSingle();
		this.m_damage.m_chop = pkg.ReadSingle();
		this.m_damage.m_pickaxe = pkg.ReadSingle();
		this.m_damage.m_fire = pkg.ReadSingle();
		this.m_damage.m_frost = pkg.ReadSingle();
		this.m_damage.m_lightning = pkg.ReadSingle();
		this.m_damage.m_poison = pkg.ReadSingle();
		this.m_damage.m_spirit = pkg.ReadSingle();
		this.m_toolTier = pkg.ReadInt();
		this.m_pushForce = pkg.ReadSingle();
		this.m_backstabBonus = pkg.ReadSingle();
		this.m_staggerMultiplier = pkg.ReadSingle();
		this.m_dodgeable = pkg.ReadBool();
		this.m_blockable = pkg.ReadBool();
		this.m_point = pkg.ReadVector3();
		this.m_dir = pkg.ReadVector3();
		this.m_statusEffect = pkg.ReadString();
		this.m_attacker = pkg.ReadZDOID();
		this.m_skill = (Skills.SkillType)pkg.ReadInt();
	}

	// Token: 0x06000DBD RID: 3517 RVA: 0x0006257A File Offset: 0x0006077A
	public float GetTotalPhysicalDamage()
	{
		return this.m_damage.GetTotalPhysicalDamage();
	}

	// Token: 0x06000DBE RID: 3518 RVA: 0x00062587 File Offset: 0x00060787
	public float GetTotalElementalDamage()
	{
		return this.m_damage.GetTotalElementalDamage();
	}

	// Token: 0x06000DBF RID: 3519 RVA: 0x00062594 File Offset: 0x00060794
	public float GetTotalDamage()
	{
		return this.m_damage.GetTotalDamage();
	}

	// Token: 0x06000DC0 RID: 3520 RVA: 0x000625A4 File Offset: 0x000607A4
	private float ApplyModifier(float baseDamage, HitData.DamageModifier mod, ref float normalDmg, ref float resistantDmg, ref float weakDmg, ref float immuneDmg)
	{
		if (mod == HitData.DamageModifier.Ignore)
		{
			return 0f;
		}
		float num = baseDamage;
		switch (mod)
		{
		case HitData.DamageModifier.Resistant:
			num /= 2f;
			resistantDmg += baseDamage;
			return num;
		case HitData.DamageModifier.Weak:
			num *= 1.5f;
			weakDmg += baseDamage;
			return num;
		case HitData.DamageModifier.Immune:
			num = 0f;
			immuneDmg += baseDamage;
			return num;
		case HitData.DamageModifier.VeryResistant:
			num /= 4f;
			resistantDmg += baseDamage;
			return num;
		case HitData.DamageModifier.VeryWeak:
			num *= 2f;
			weakDmg += baseDamage;
			return num;
		}
		normalDmg += baseDamage;
		return num;
	}

	// Token: 0x06000DC1 RID: 3521 RVA: 0x00062640 File Offset: 0x00060840
	public void ApplyResistance(HitData.DamageModifiers modifiers, out HitData.DamageModifier significantModifier)
	{
		float damage = this.m_damage.m_damage;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		this.m_damage.m_blunt = this.ApplyModifier(this.m_damage.m_blunt, modifiers.m_blunt, ref damage, ref num, ref num2, ref num3);
		this.m_damage.m_slash = this.ApplyModifier(this.m_damage.m_slash, modifiers.m_slash, ref damage, ref num, ref num2, ref num3);
		this.m_damage.m_pierce = this.ApplyModifier(this.m_damage.m_pierce, modifiers.m_pierce, ref damage, ref num, ref num2, ref num3);
		this.m_damage.m_chop = this.ApplyModifier(this.m_damage.m_chop, modifiers.m_chop, ref damage, ref num, ref num2, ref num3);
		this.m_damage.m_pickaxe = this.ApplyModifier(this.m_damage.m_pickaxe, modifiers.m_pickaxe, ref damage, ref num, ref num2, ref num3);
		this.m_damage.m_fire = this.ApplyModifier(this.m_damage.m_fire, modifiers.m_fire, ref damage, ref num, ref num2, ref num3);
		this.m_damage.m_frost = this.ApplyModifier(this.m_damage.m_frost, modifiers.m_frost, ref damage, ref num, ref num2, ref num3);
		this.m_damage.m_lightning = this.ApplyModifier(this.m_damage.m_lightning, modifiers.m_lightning, ref damage, ref num, ref num2, ref num3);
		this.m_damage.m_poison = this.ApplyModifier(this.m_damage.m_poison, modifiers.m_poison, ref damage, ref num, ref num2, ref num3);
		this.m_damage.m_spirit = this.ApplyModifier(this.m_damage.m_spirit, modifiers.m_spirit, ref damage, ref num, ref num2, ref num3);
		significantModifier = HitData.DamageModifier.Immune;
		if (num3 >= num && num3 >= num2 && num3 >= damage)
		{
			significantModifier = HitData.DamageModifier.Immune;
		}
		if (damage >= num && damage >= num2 && damage >= num3)
		{
			significantModifier = HitData.DamageModifier.Normal;
		}
		if (num >= num2 && num >= num3 && num >= damage)
		{
			significantModifier = HitData.DamageModifier.Resistant;
		}
		if (num2 >= num && num2 >= num3 && num2 >= damage)
		{
			significantModifier = HitData.DamageModifier.Weak;
		}
	}

	// Token: 0x06000DC2 RID: 3522 RVA: 0x0006284E File Offset: 0x00060A4E
	public void ApplyArmor(float ac)
	{
		this.m_damage.ApplyArmor(ac);
	}

	// Token: 0x06000DC3 RID: 3523 RVA: 0x0006285C File Offset: 0x00060A5C
	public void ApplyModifier(float multiplier)
	{
		this.m_damage.m_blunt = this.m_damage.m_blunt * multiplier;
		this.m_damage.m_slash = this.m_damage.m_slash * multiplier;
		this.m_damage.m_pierce = this.m_damage.m_pierce * multiplier;
		this.m_damage.m_chop = this.m_damage.m_chop * multiplier;
		this.m_damage.m_pickaxe = this.m_damage.m_pickaxe * multiplier;
		this.m_damage.m_fire = this.m_damage.m_fire * multiplier;
		this.m_damage.m_frost = this.m_damage.m_frost * multiplier;
		this.m_damage.m_lightning = this.m_damage.m_lightning * multiplier;
		this.m_damage.m_poison = this.m_damage.m_poison * multiplier;
		this.m_damage.m_spirit = this.m_damage.m_spirit * multiplier;
	}

	// Token: 0x06000DC4 RID: 3524 RVA: 0x0006290C File Offset: 0x00060B0C
	public float GetTotalBlockableDamage()
	{
		return this.m_damage.m_blunt + this.m_damage.m_slash + this.m_damage.m_pierce + this.m_damage.m_fire + this.m_damage.m_frost + this.m_damage.m_lightning + this.m_damage.m_poison + this.m_damage.m_spirit;
	}

	// Token: 0x06000DC5 RID: 3525 RVA: 0x00062978 File Offset: 0x00060B78
	public void BlockDamage(float damage)
	{
		float totalBlockableDamage = this.GetTotalBlockableDamage();
		float num = Mathf.Max(0f, totalBlockableDamage - damage);
		if (totalBlockableDamage <= 0f)
		{
			return;
		}
		float num2 = num / totalBlockableDamage;
		this.m_damage.m_blunt = this.m_damage.m_blunt * num2;
		this.m_damage.m_slash = this.m_damage.m_slash * num2;
		this.m_damage.m_pierce = this.m_damage.m_pierce * num2;
		this.m_damage.m_fire = this.m_damage.m_fire * num2;
		this.m_damage.m_frost = this.m_damage.m_frost * num2;
		this.m_damage.m_lightning = this.m_damage.m_lightning * num2;
		this.m_damage.m_poison = this.m_damage.m_poison * num2;
		this.m_damage.m_spirit = this.m_damage.m_spirit * num2;
	}

	// Token: 0x06000DC6 RID: 3526 RVA: 0x00062A27 File Offset: 0x00060C27
	public bool HaveAttacker()
	{
		return !this.m_attacker.IsNone();
	}

	// Token: 0x06000DC7 RID: 3527 RVA: 0x00062A38 File Offset: 0x00060C38
	public Character GetAttacker()
	{
		if (this.m_attacker.IsNone())
		{
			return null;
		}
		if (ZNetScene.instance == null)
		{
			return null;
		}
		GameObject gameObject = ZNetScene.instance.FindInstance(this.m_attacker);
		if (gameObject == null)
		{
			return null;
		}
		return gameObject.GetComponent<Character>();
	}

	// Token: 0x06000DC8 RID: 3528 RVA: 0x00062A85 File Offset: 0x00060C85
	public void SetAttacker(Character attacker)
	{
		if (attacker)
		{
			this.m_attacker = attacker.GetZDOID();
			return;
		}
		this.m_attacker = ZDOID.None;
	}

	// Token: 0x04000C70 RID: 3184
	public HitData.DamageTypes m_damage;

	// Token: 0x04000C71 RID: 3185
	public int m_toolTier;

	// Token: 0x04000C72 RID: 3186
	public bool m_dodgeable;

	// Token: 0x04000C73 RID: 3187
	public bool m_blockable;

	// Token: 0x04000C74 RID: 3188
	public float m_pushForce;

	// Token: 0x04000C75 RID: 3189
	public float m_backstabBonus = 1f;

	// Token: 0x04000C76 RID: 3190
	public float m_staggerMultiplier = 1f;

	// Token: 0x04000C77 RID: 3191
	public Vector3 m_point = Vector3.zero;

	// Token: 0x04000C78 RID: 3192
	public Vector3 m_dir = Vector3.zero;

	// Token: 0x04000C79 RID: 3193
	public string m_statusEffect = "";

	// Token: 0x04000C7A RID: 3194
	public ZDOID m_attacker = ZDOID.None;

	// Token: 0x04000C7B RID: 3195
	public Skills.SkillType m_skill;

	// Token: 0x04000C7C RID: 3196
	public Collider m_hitCollider;

	// Token: 0x0200019B RID: 411
	[Flags]
	public enum DamageType
	{
		// Token: 0x040012AD RID: 4781
		Blunt = 1,
		// Token: 0x040012AE RID: 4782
		Slash = 2,
		// Token: 0x040012AF RID: 4783
		Pierce = 4,
		// Token: 0x040012B0 RID: 4784
		Chop = 8,
		// Token: 0x040012B1 RID: 4785
		Pickaxe = 16,
		// Token: 0x040012B2 RID: 4786
		Fire = 32,
		// Token: 0x040012B3 RID: 4787
		Frost = 64,
		// Token: 0x040012B4 RID: 4788
		Lightning = 128,
		// Token: 0x040012B5 RID: 4789
		Poison = 256,
		// Token: 0x040012B6 RID: 4790
		Spirit = 512,
		// Token: 0x040012B7 RID: 4791
		Physical = 31,
		// Token: 0x040012B8 RID: 4792
		Elemental = 224
	}

	// Token: 0x0200019C RID: 412
	public enum DamageModifier
	{
		// Token: 0x040012BA RID: 4794
		Normal,
		// Token: 0x040012BB RID: 4795
		Resistant,
		// Token: 0x040012BC RID: 4796
		Weak,
		// Token: 0x040012BD RID: 4797
		Immune,
		// Token: 0x040012BE RID: 4798
		Ignore,
		// Token: 0x040012BF RID: 4799
		VeryResistant,
		// Token: 0x040012C0 RID: 4800
		VeryWeak
	}

	// Token: 0x0200019D RID: 413
	[Serializable]
	public struct DamageModPair
	{
		// Token: 0x040012C1 RID: 4801
		public HitData.DamageType m_type;

		// Token: 0x040012C2 RID: 4802
		public HitData.DamageModifier m_modifier;
	}

	// Token: 0x0200019E RID: 414
	[Serializable]
	public struct DamageModifiers
	{
		// Token: 0x060011A3 RID: 4515 RVA: 0x0007936A File Offset: 0x0007756A
		public HitData.DamageModifiers Clone()
		{
			return (HitData.DamageModifiers)base.MemberwiseClone();
		}

		// Token: 0x060011A4 RID: 4516 RVA: 0x00079384 File Offset: 0x00077584
		public void Apply(List<HitData.DamageModPair> modifiers)
		{
			foreach (HitData.DamageModPair damageModPair in modifiers)
			{
				HitData.DamageType type = damageModPair.m_type;
				if (type <= HitData.DamageType.Fire)
				{
					if (type <= HitData.DamageType.Chop)
					{
						switch (type)
						{
						case HitData.DamageType.Blunt:
							this.ApplyIfBetter(ref this.m_blunt, damageModPair.m_modifier);
							break;
						case HitData.DamageType.Slash:
							this.ApplyIfBetter(ref this.m_slash, damageModPair.m_modifier);
							break;
						case HitData.DamageType.Blunt | HitData.DamageType.Slash:
							break;
						case HitData.DamageType.Pierce:
							this.ApplyIfBetter(ref this.m_pierce, damageModPair.m_modifier);
							break;
						default:
							if (type == HitData.DamageType.Chop)
							{
								this.ApplyIfBetter(ref this.m_chop, damageModPair.m_modifier);
							}
							break;
						}
					}
					else if (type != HitData.DamageType.Pickaxe)
					{
						if (type == HitData.DamageType.Fire)
						{
							this.ApplyIfBetter(ref this.m_fire, damageModPair.m_modifier);
						}
					}
					else
					{
						this.ApplyIfBetter(ref this.m_pickaxe, damageModPair.m_modifier);
					}
				}
				else if (type <= HitData.DamageType.Lightning)
				{
					if (type != HitData.DamageType.Frost)
					{
						if (type == HitData.DamageType.Lightning)
						{
							this.ApplyIfBetter(ref this.m_lightning, damageModPair.m_modifier);
						}
					}
					else
					{
						this.ApplyIfBetter(ref this.m_frost, damageModPair.m_modifier);
					}
				}
				else if (type != HitData.DamageType.Poison)
				{
					if (type == HitData.DamageType.Spirit)
					{
						this.ApplyIfBetter(ref this.m_spirit, damageModPair.m_modifier);
					}
				}
				else
				{
					this.ApplyIfBetter(ref this.m_poison, damageModPair.m_modifier);
				}
			}
		}

		// Token: 0x060011A5 RID: 4517 RVA: 0x00079530 File Offset: 0x00077730
		public HitData.DamageModifier GetModifier(HitData.DamageType type)
		{
			if (type <= HitData.DamageType.Fire)
			{
				if (type <= HitData.DamageType.Chop)
				{
					switch (type)
					{
					case HitData.DamageType.Blunt:
						return this.m_blunt;
					case HitData.DamageType.Slash:
						return this.m_slash;
					case HitData.DamageType.Blunt | HitData.DamageType.Slash:
						break;
					case HitData.DamageType.Pierce:
						return this.m_pierce;
					default:
						if (type == HitData.DamageType.Chop)
						{
							return this.m_chop;
						}
						break;
					}
				}
				else
				{
					if (type == HitData.DamageType.Pickaxe)
					{
						return this.m_pickaxe;
					}
					if (type == HitData.DamageType.Fire)
					{
						return this.m_fire;
					}
				}
			}
			else if (type <= HitData.DamageType.Lightning)
			{
				if (type == HitData.DamageType.Frost)
				{
					return this.m_frost;
				}
				if (type == HitData.DamageType.Lightning)
				{
					return this.m_lightning;
				}
			}
			else
			{
				if (type == HitData.DamageType.Poison)
				{
					return this.m_poison;
				}
				if (type == HitData.DamageType.Spirit)
				{
					return this.m_spirit;
				}
			}
			return HitData.DamageModifier.Normal;
		}

		// Token: 0x060011A6 RID: 4518 RVA: 0x000795E0 File Offset: 0x000777E0
		private void ApplyIfBetter(ref HitData.DamageModifier original, HitData.DamageModifier mod)
		{
			if (this.ShouldOverride(original, mod))
			{
				original = mod;
			}
		}

		// Token: 0x060011A7 RID: 4519 RVA: 0x000795F0 File Offset: 0x000777F0
		private bool ShouldOverride(HitData.DamageModifier a, HitData.DamageModifier b)
		{
			return a != HitData.DamageModifier.Ignore && (b == HitData.DamageModifier.Immune || ((a != HitData.DamageModifier.VeryResistant || b != HitData.DamageModifier.Resistant) && (a != HitData.DamageModifier.VeryWeak || b != HitData.DamageModifier.Weak)));
		}

		// Token: 0x060011A8 RID: 4520 RVA: 0x00079614 File Offset: 0x00077814
		public void Print()
		{
			ZLog.Log("m_blunt " + this.m_blunt);
			ZLog.Log("m_slash " + this.m_slash);
			ZLog.Log("m_pierce " + this.m_pierce);
			ZLog.Log("m_chop " + this.m_chop);
			ZLog.Log("m_pickaxe " + this.m_pickaxe);
			ZLog.Log("m_fire " + this.m_fire);
			ZLog.Log("m_frost " + this.m_frost);
			ZLog.Log("m_lightning " + this.m_lightning);
			ZLog.Log("m_poison " + this.m_poison);
			ZLog.Log("m_spirit " + this.m_spirit);
		}

		// Token: 0x040012C3 RID: 4803
		public HitData.DamageModifier m_blunt;

		// Token: 0x040012C4 RID: 4804
		public HitData.DamageModifier m_slash;

		// Token: 0x040012C5 RID: 4805
		public HitData.DamageModifier m_pierce;

		// Token: 0x040012C6 RID: 4806
		public HitData.DamageModifier m_chop;

		// Token: 0x040012C7 RID: 4807
		public HitData.DamageModifier m_pickaxe;

		// Token: 0x040012C8 RID: 4808
		public HitData.DamageModifier m_fire;

		// Token: 0x040012C9 RID: 4809
		public HitData.DamageModifier m_frost;

		// Token: 0x040012CA RID: 4810
		public HitData.DamageModifier m_lightning;

		// Token: 0x040012CB RID: 4811
		public HitData.DamageModifier m_poison;

		// Token: 0x040012CC RID: 4812
		public HitData.DamageModifier m_spirit;
	}

	// Token: 0x0200019F RID: 415
	[Serializable]
	public struct DamageTypes
	{
		// Token: 0x060011A9 RID: 4521 RVA: 0x00079728 File Offset: 0x00077928
		public bool HaveDamage()
		{
			return this.m_damage > 0f || this.m_blunt > 0f || this.m_slash > 0f || this.m_pierce > 0f || this.m_chop > 0f || this.m_pickaxe > 0f || this.m_fire > 0f || this.m_frost > 0f || this.m_lightning > 0f || this.m_poison > 0f || this.m_spirit > 0f;
		}

		// Token: 0x060011AA RID: 4522 RVA: 0x000797C9 File Offset: 0x000779C9
		public float GetTotalPhysicalDamage()
		{
			return this.m_blunt + this.m_slash + this.m_pierce;
		}

		// Token: 0x060011AB RID: 4523 RVA: 0x000797DF File Offset: 0x000779DF
		public float GetTotalElementalDamage()
		{
			return this.m_fire + this.m_frost + this.m_lightning;
		}

		// Token: 0x060011AC RID: 4524 RVA: 0x000797F8 File Offset: 0x000779F8
		public float GetTotalDamage()
		{
			return this.m_damage + this.m_blunt + this.m_slash + this.m_pierce + this.m_chop + this.m_pickaxe + this.m_fire + this.m_frost + this.m_lightning + this.m_poison + this.m_spirit;
		}

		// Token: 0x060011AD RID: 4525 RVA: 0x00079851 File Offset: 0x00077A51
		public HitData.DamageTypes Clone()
		{
			return (HitData.DamageTypes)base.MemberwiseClone();
		}

		// Token: 0x060011AE RID: 4526 RVA: 0x00079868 File Offset: 0x00077A68
		public void Add(HitData.DamageTypes other, int multiplier = 1)
		{
			this.m_damage += other.m_damage * (float)multiplier;
			this.m_blunt += other.m_blunt * (float)multiplier;
			this.m_slash += other.m_slash * (float)multiplier;
			this.m_pierce += other.m_pierce * (float)multiplier;
			this.m_chop += other.m_chop * (float)multiplier;
			this.m_pickaxe += other.m_pickaxe * (float)multiplier;
			this.m_fire += other.m_fire * (float)multiplier;
			this.m_frost += other.m_frost * (float)multiplier;
			this.m_lightning += other.m_lightning * (float)multiplier;
			this.m_poison += other.m_poison * (float)multiplier;
			this.m_spirit += other.m_spirit * (float)multiplier;
		}

		// Token: 0x060011AF RID: 4527 RVA: 0x00079968 File Offset: 0x00077B68
		public void Modify(float multiplier)
		{
			this.m_damage *= multiplier;
			this.m_blunt *= multiplier;
			this.m_slash *= multiplier;
			this.m_pierce *= multiplier;
			this.m_chop *= multiplier;
			this.m_pickaxe *= multiplier;
			this.m_fire *= multiplier;
			this.m_frost *= multiplier;
			this.m_lightning *= multiplier;
			this.m_poison *= multiplier;
			this.m_spirit *= multiplier;
		}

		// Token: 0x060011B0 RID: 4528 RVA: 0x00079A10 File Offset: 0x00077C10
		private float ApplyArmor(float dmg, float ac)
		{
			float result = Mathf.Clamp01(dmg / (ac * 4f)) * dmg;
			if (ac < dmg / 2f)
			{
				result = dmg - ac;
			}
			return result;
		}

		// Token: 0x060011B1 RID: 4529 RVA: 0x00079A40 File Offset: 0x00077C40
		public void ApplyArmor(float ac)
		{
			if (ac <= 0f)
			{
				return;
			}
			float num = this.m_blunt + this.m_chop + this.m_pickaxe + this.m_slash + this.m_pierce + this.m_fire + this.m_frost + this.m_lightning + this.m_spirit;
			if (num <= 0f)
			{
				return;
			}
			float num2 = this.ApplyArmor(num, ac) / num;
			this.m_blunt *= num2;
			this.m_chop *= num2;
			this.m_pickaxe *= num2;
			this.m_slash *= num2;
			this.m_pierce *= num2;
			this.m_fire *= num2;
			this.m_frost *= num2;
			this.m_lightning *= num2;
			this.m_spirit *= num2;
		}

		// Token: 0x060011B2 RID: 4530 RVA: 0x00079B28 File Offset: 0x00077D28
		private string DamageRange(float damage, float minFactor, float maxFactor)
		{
			int num = Mathf.RoundToInt(damage * minFactor);
			int num2 = Mathf.RoundToInt(damage * maxFactor);
			return string.Concat(new object[]
			{
				"<color=orange>",
				Mathf.RoundToInt(damage),
				"</color> <color=yellow>(",
				num.ToString(),
				"-",
				num2.ToString(),
				") </color>"
			});
		}

		// Token: 0x060011B3 RID: 4531 RVA: 0x00079B94 File Offset: 0x00077D94
		public string GetTooltipString(Skills.SkillType skillType = Skills.SkillType.None)
		{
			if (Player.m_localPlayer == null)
			{
				return "";
			}
			float minFactor;
			float maxFactor;
			Player.m_localPlayer.GetSkills().GetRandomSkillRange(out minFactor, out maxFactor, skillType);
			string text = "";
			if (this.m_damage != 0f)
			{
				text = text + "\n$inventory_damage: " + this.DamageRange(this.m_damage, minFactor, maxFactor);
			}
			if (this.m_blunt != 0f)
			{
				text = text + "\n$inventory_blunt: " + this.DamageRange(this.m_blunt, minFactor, maxFactor);
			}
			if (this.m_slash != 0f)
			{
				text = text + "\n$inventory_slash: " + this.DamageRange(this.m_slash, minFactor, maxFactor);
			}
			if (this.m_pierce != 0f)
			{
				text = text + "\n$inventory_pierce: " + this.DamageRange(this.m_pierce, minFactor, maxFactor);
			}
			if (this.m_fire != 0f)
			{
				text = text + "\n$inventory_fire: " + this.DamageRange(this.m_fire, minFactor, maxFactor);
			}
			if (this.m_frost != 0f)
			{
				text = text + "\n$inventory_frost: " + this.DamageRange(this.m_frost, minFactor, maxFactor);
			}
			if (this.m_lightning != 0f)
			{
				text = text + "\n$inventory_lightning: " + this.DamageRange(this.m_lightning, minFactor, maxFactor);
			}
			if (this.m_poison != 0f)
			{
				text = text + "\n$inventory_poison: " + this.DamageRange(this.m_poison, minFactor, maxFactor);
			}
			if (this.m_spirit != 0f)
			{
				text = text + "\n$inventory_spirit: " + this.DamageRange(this.m_spirit, minFactor, maxFactor);
			}
			return text;
		}

		// Token: 0x060011B4 RID: 4532 RVA: 0x00079D30 File Offset: 0x00077F30
		public string GetTooltipString()
		{
			string text = "";
			if (this.m_damage != 0f)
			{
				text = text + "\n$inventory_damage: <color=yellow>" + this.m_damage.ToString() + "</color>";
			}
			if (this.m_blunt != 0f)
			{
				text = text + "\n$inventory_blunt: <color=yellow>" + this.m_blunt.ToString() + "</color>";
			}
			if (this.m_slash != 0f)
			{
				text = text + "\n$inventory_slash: <color=yellow>" + this.m_slash.ToString() + "</color>";
			}
			if (this.m_pierce != 0f)
			{
				text = text + "\n$inventory_pierce: <color=yellow>" + this.m_pierce.ToString() + "</color>";
			}
			if (this.m_fire != 0f)
			{
				text = text + "\n$inventory_fire: <color=yellow>" + this.m_fire.ToString() + "</color>";
			}
			if (this.m_frost != 0f)
			{
				text = text + "\n$inventory_frost: <color=yellow>" + this.m_frost.ToString() + "</color>";
			}
			if (this.m_lightning != 0f)
			{
				text = text + "\n$inventory_lightning: <color=yellow>" + this.m_frost.ToString() + "</color>";
			}
			if (this.m_poison != 0f)
			{
				text = text + "\n$inventory_poison: <color=yellow>" + this.m_poison.ToString() + "</color>";
			}
			if (this.m_spirit != 0f)
			{
				text = text + "\n$inventory_spirit: <color=yellow>" + this.m_spirit.ToString() + "</color>";
			}
			return text;
		}

		// Token: 0x040012CD RID: 4813
		public float m_damage;

		// Token: 0x040012CE RID: 4814
		public float m_blunt;

		// Token: 0x040012CF RID: 4815
		public float m_slash;

		// Token: 0x040012D0 RID: 4816
		public float m_pierce;

		// Token: 0x040012D1 RID: 4817
		public float m_chop;

		// Token: 0x040012D2 RID: 4818
		public float m_pickaxe;

		// Token: 0x040012D3 RID: 4819
		public float m_fire;

		// Token: 0x040012D4 RID: 4820
		public float m_frost;

		// Token: 0x040012D5 RID: 4821
		public float m_lightning;

		// Token: 0x040012D6 RID: 4822
		public float m_poison;

		// Token: 0x040012D7 RID: 4823
		public float m_spirit;
	}
}
