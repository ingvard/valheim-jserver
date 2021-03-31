using System;
using UnityEngine;

// Token: 0x02000030 RID: 48
public class StatusEffect : ScriptableObject
{
	// Token: 0x060003DB RID: 987 RVA: 0x0002077A File Offset: 0x0001E97A
	public StatusEffect Clone()
	{
		return base.MemberwiseClone() as StatusEffect;
	}

	// Token: 0x060003DC RID: 988 RVA: 0x000027E2 File Offset: 0x000009E2
	public virtual bool CanAdd(Character character)
	{
		return true;
	}

	// Token: 0x060003DD RID: 989 RVA: 0x00020787 File Offset: 0x0001E987
	public virtual void Setup(Character character)
	{
		this.m_character = character;
		if (!string.IsNullOrEmpty(this.m_startMessage))
		{
			this.m_character.Message(this.m_startMessageType, this.m_startMessage, 0, null);
		}
		this.TriggerStartEffects();
	}

	// Token: 0x060003DE RID: 990 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void SetAttacker(Character attacker)
	{
	}

	// Token: 0x060003DF RID: 991 RVA: 0x000207BC File Offset: 0x0001E9BC
	public virtual string GetTooltipString()
	{
		return this.m_tooltip;
	}

	// Token: 0x060003E0 RID: 992 RVA: 0x000207C4 File Offset: 0x0001E9C4
	private void OnApplicationQuit()
	{
		this.m_startEffectInstances = null;
	}

	// Token: 0x060003E1 RID: 993 RVA: 0x000207CD File Offset: 0x0001E9CD
	public virtual void OnDestroy()
	{
		this.RemoveStartEffects();
	}

	// Token: 0x060003E2 RID: 994 RVA: 0x000207D8 File Offset: 0x0001E9D8
	protected void TriggerStartEffects()
	{
		this.RemoveStartEffects();
		float radius = this.m_character.GetRadius();
		this.m_startEffectInstances = this.m_startEffects.Create(this.m_character.GetCenterPoint(), this.m_character.transform.rotation, this.m_character.transform, radius * 2f);
	}

	// Token: 0x060003E3 RID: 995 RVA: 0x00020838 File Offset: 0x0001EA38
	private void RemoveStartEffects()
	{
		if (this.m_startEffectInstances != null && ZNetScene.instance != null)
		{
			foreach (GameObject gameObject in this.m_startEffectInstances)
			{
				if (gameObject)
				{
					ZNetView component = gameObject.GetComponent<ZNetView>();
					if (component.IsValid())
					{
						component.ClaimOwnership();
						component.Destroy();
					}
				}
			}
			this.m_startEffectInstances = null;
		}
	}

	// Token: 0x060003E4 RID: 996 RVA: 0x000208A0 File Offset: 0x0001EAA0
	public virtual void Stop()
	{
		this.RemoveStartEffects();
		this.m_stopEffects.Create(this.m_character.transform.position, this.m_character.transform.rotation, null, 1f);
		if (!string.IsNullOrEmpty(this.m_stopMessage))
		{
			this.m_character.Message(this.m_stopMessageType, this.m_stopMessage, 0, null);
		}
	}

	// Token: 0x060003E5 RID: 997 RVA: 0x0002090C File Offset: 0x0001EB0C
	public virtual void UpdateStatusEffect(float dt)
	{
		this.m_time += dt;
		if (this.m_repeatInterval > 0f && !string.IsNullOrEmpty(this.m_repeatMessage))
		{
			this.m_msgTimer += dt;
			if (this.m_msgTimer > this.m_repeatInterval)
			{
				this.m_msgTimer = 0f;
				this.m_character.Message(this.m_repeatMessageType, this.m_repeatMessage, 0, null);
			}
		}
	}

	// Token: 0x060003E6 RID: 998 RVA: 0x00020981 File Offset: 0x0001EB81
	public virtual bool IsDone()
	{
		return this.m_ttl > 0f && this.m_time > this.m_ttl;
	}

	// Token: 0x060003E7 RID: 999 RVA: 0x000209A1 File Offset: 0x0001EBA1
	public virtual void ResetTime()
	{
		this.m_time = 0f;
	}

	// Token: 0x060003E8 RID: 1000 RVA: 0x000209AE File Offset: 0x0001EBAE
	public float GetDuration()
	{
		return this.m_time;
	}

	// Token: 0x060003E9 RID: 1001 RVA: 0x000209B6 File Offset: 0x0001EBB6
	public float GetRemaningTime()
	{
		return this.m_ttl - this.m_time;
	}

	// Token: 0x060003EA RID: 1002 RVA: 0x000209C5 File Offset: 0x0001EBC5
	public virtual string GetIconText()
	{
		if (this.m_ttl > 0f)
		{
			return StatusEffect.GetTimeString(this.m_ttl - this.GetDuration(), false, false);
		}
		return "";
	}

	// Token: 0x060003EB RID: 1003 RVA: 0x000209F0 File Offset: 0x0001EBF0
	public static string GetTimeString(float time, bool sufix = false, bool alwaysShowMinutes = false)
	{
		if (time <= 0f)
		{
			return "";
		}
		int num = Mathf.CeilToInt(time);
		int num2 = (int)((float)num / 60f);
		int num3 = Mathf.Max(0, num - num2 * 60);
		if (sufix)
		{
			if (num2 > 0 || alwaysShowMinutes)
			{
				return string.Concat(new object[]
				{
					num2,
					"m:",
					num3.ToString("00"),
					"s"
				});
			}
			return num3.ToString() + "s";
		}
		else
		{
			if (num2 > 0 || alwaysShowMinutes)
			{
				return num2 + ":" + num3.ToString("00");
			}
			return num3.ToString();
		}
	}

	// Token: 0x060003EC RID: 1004 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void ModifyAttack(Skills.SkillType skill, ref HitData hitData)
	{
	}

	// Token: 0x060003ED RID: 1005 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void ModifyHealthRegen(ref float regenMultiplier)
	{
	}

	// Token: 0x060003EE RID: 1006 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void ModifyStaminaRegen(ref float staminaRegen)
	{
	}

	// Token: 0x060003EF RID: 1007 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void ModifyDamageMods(ref HitData.DamageModifiers modifiers)
	{
	}

	// Token: 0x060003F0 RID: 1008 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void ModifyRaiseSkill(Skills.SkillType skill, ref float value)
	{
	}

	// Token: 0x060003F1 RID: 1009 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void ModifySpeed(ref float speed)
	{
	}

	// Token: 0x060003F2 RID: 1010 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void ModifyNoise(float baseNoise, ref float noise)
	{
	}

	// Token: 0x060003F3 RID: 1011 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void ModifyStealth(float baseStealth, ref float stealth)
	{
	}

	// Token: 0x060003F4 RID: 1012 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void ModifyMaxCarryWeight(float baseLimit, ref float limit)
	{
	}

	// Token: 0x060003F5 RID: 1013 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void ModifyRunStaminaDrain(float baseDrain, ref float drain)
	{
	}

	// Token: 0x060003F6 RID: 1014 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void ModifyJumpStaminaUsage(float baseStaminaUse, ref float staminaUse)
	{
	}

	// Token: 0x060003F7 RID: 1015 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void OnDamaged(HitData hit, Character attacker)
	{
	}

	// Token: 0x060003F8 RID: 1016 RVA: 0x00020AA9 File Offset: 0x0001ECA9
	public bool HaveAttribute(StatusEffect.StatusAttribute value)
	{
		return (this.m_attributes & value) > StatusEffect.StatusAttribute.None;
	}

	// Token: 0x040003DB RID: 987
	[Header("__Common__")]
	public string m_name = "";

	// Token: 0x040003DC RID: 988
	public string m_category = "";

	// Token: 0x040003DD RID: 989
	public Sprite m_icon;

	// Token: 0x040003DE RID: 990
	public bool m_flashIcon;

	// Token: 0x040003DF RID: 991
	public bool m_cooldownIcon;

	// Token: 0x040003E0 RID: 992
	[TextArea]
	public string m_tooltip = "";

	// Token: 0x040003E1 RID: 993
	[BitMask(typeof(StatusEffect.StatusAttribute))]
	public StatusEffect.StatusAttribute m_attributes;

	// Token: 0x040003E2 RID: 994
	public MessageHud.MessageType m_startMessageType = MessageHud.MessageType.TopLeft;

	// Token: 0x040003E3 RID: 995
	public string m_startMessage = "";

	// Token: 0x040003E4 RID: 996
	public MessageHud.MessageType m_stopMessageType = MessageHud.MessageType.TopLeft;

	// Token: 0x040003E5 RID: 997
	public string m_stopMessage = "";

	// Token: 0x040003E6 RID: 998
	public MessageHud.MessageType m_repeatMessageType = MessageHud.MessageType.TopLeft;

	// Token: 0x040003E7 RID: 999
	public string m_repeatMessage = "";

	// Token: 0x040003E8 RID: 1000
	public float m_repeatInterval;

	// Token: 0x040003E9 RID: 1001
	public float m_ttl;

	// Token: 0x040003EA RID: 1002
	public EffectList m_startEffects = new EffectList();

	// Token: 0x040003EB RID: 1003
	public EffectList m_stopEffects = new EffectList();

	// Token: 0x040003EC RID: 1004
	[Header("__Guardian power__")]
	public float m_cooldown;

	// Token: 0x040003ED RID: 1005
	public string m_activationAnimation = "gpower";

	// Token: 0x040003EE RID: 1006
	[NonSerialized]
	public bool m_isNew = true;

	// Token: 0x040003EF RID: 1007
	private float m_msgTimer;

	// Token: 0x040003F0 RID: 1008
	protected Character m_character;

	// Token: 0x040003F1 RID: 1009
	protected float m_time;

	// Token: 0x040003F2 RID: 1010
	protected GameObject[] m_startEffectInstances;

	// Token: 0x02000136 RID: 310
	public enum StatusAttribute
	{
		// Token: 0x0400104D RID: 4173
		None,
		// Token: 0x0400104E RID: 4174
		ColdResistance,
		// Token: 0x0400104F RID: 4175
		DoubleImpactDamage,
		// Token: 0x04001050 RID: 4176
		SailingPower = 4
	}
}
