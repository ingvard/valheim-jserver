using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Token: 0x0200001A RID: 26
public class Aoe : MonoBehaviour, IProjectile
{
	// Token: 0x060002D4 RID: 724 RVA: 0x00016E90 File Offset: 0x00015090
	private void Awake()
	{
		this.m_nview = base.GetComponentInParent<ZNetView>();
		this.m_rayMask = 0;
		if (this.m_hitCharacters)
		{
			this.m_rayMask |= LayerMask.GetMask(new string[]
			{
				"character",
				"character_net",
				"character_ghost"
			});
		}
		if (this.m_hitProps)
		{
			this.m_rayMask |= LayerMask.GetMask(new string[]
			{
				"Default",
				"static_solid",
				"Default_small",
				"piece",
				"hitbox",
				"character_noenv",
				"vehicle"
			});
		}
	}

	// Token: 0x060002D5 RID: 725 RVA: 0x00016F40 File Offset: 0x00015140
	public HitData.DamageTypes GetDamage()
	{
		return this.GetDamage(this.m_level);
	}

	// Token: 0x060002D6 RID: 726 RVA: 0x00016F50 File Offset: 0x00015150
	public HitData.DamageTypes GetDamage(int itemQuality)
	{
		if (itemQuality <= 1)
		{
			return this.m_damage;
		}
		HitData.DamageTypes damage = this.m_damage;
		damage.Add(this.m_damagePerLevel, itemQuality - 1);
		return damage;
	}

	// Token: 0x060002D7 RID: 727 RVA: 0x00016F80 File Offset: 0x00015180
	public string GetTooltipString(int itemQuality)
	{
		StringBuilder stringBuilder = new StringBuilder(256);
		stringBuilder.Append("AOE");
		stringBuilder.Append(this.GetDamage(itemQuality).GetTooltipString());
		stringBuilder.AppendFormat("\n$item_knockback: <color=orange>{0}</color>", this.m_attackForce);
		stringBuilder.AppendFormat("\n$item_backstab: <color=orange>{0}x</color>", this.m_backstabBonus);
		return stringBuilder.ToString();
	}

	// Token: 0x060002D8 RID: 728 RVA: 0x00016FEC File Offset: 0x000151EC
	private void Start()
	{
		if (this.m_nview != null && (!this.m_nview.IsValid() || !this.m_nview.IsOwner()))
		{
			return;
		}
		if (!this.m_useTriggers && this.m_hitInterval <= 0f)
		{
			this.CheckHits();
		}
	}

	// Token: 0x060002D9 RID: 729 RVA: 0x00017040 File Offset: 0x00015240
	private void FixedUpdate()
	{
		if (this.m_nview != null && (!this.m_nview.IsValid() || !this.m_nview.IsOwner()))
		{
			return;
		}
		if (this.m_hitInterval > 0f)
		{
			this.m_hitTimer -= Time.fixedDeltaTime;
			if (this.m_hitTimer <= 0f)
			{
				this.m_hitTimer = this.m_hitInterval;
				if (this.m_useTriggers)
				{
					this.m_hitList.Clear();
				}
				else
				{
					this.CheckHits();
				}
			}
		}
		if (this.m_owner != null && this.m_attachToCaster)
		{
			base.transform.position = this.m_owner.transform.TransformPoint(this.m_offset);
			base.transform.rotation = this.m_owner.transform.rotation * this.m_localRot;
		}
		if (this.m_ttl > 0f)
		{
			this.m_ttl -= Time.fixedDeltaTime;
			if (this.m_ttl <= 0f)
			{
				ZNetScene.instance.Destroy(base.gameObject);
			}
		}
	}

	// Token: 0x060002DA RID: 730 RVA: 0x00017164 File Offset: 0x00015364
	private void CheckHits()
	{
		this.m_hitList.Clear();
		Collider[] array = Physics.OverlapSphere(base.transform.position, this.m_radius, this.m_rayMask);
		bool flag = false;
		foreach (Collider collider in array)
		{
			if (this.OnHit(collider, collider.transform.position))
			{
				flag = true;
			}
		}
		if (flag && this.m_owner && this.m_owner.IsPlayer() && this.m_skill != Skills.SkillType.None)
		{
			this.m_owner.RaiseSkill(this.m_skill, 1f);
		}
	}

	// Token: 0x060002DB RID: 731 RVA: 0x00017200 File Offset: 0x00015400
	public void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item)
	{
		this.m_owner = owner;
		if (item != null)
		{
			this.m_level = item.m_quality;
		}
		if (this.m_attachToCaster && owner != null)
		{
			this.m_offset = owner.transform.InverseTransformPoint(base.transform.position);
			this.m_localRot = Quaternion.Inverse(owner.transform.rotation) * base.transform.rotation;
		}
		if (hitData != null && this.m_useAttackSettings)
		{
			this.m_damage = hitData.m_damage;
			this.m_blockable = hitData.m_blockable;
			this.m_dodgeable = hitData.m_dodgeable;
			this.m_attackForce = hitData.m_pushForce;
			this.m_backstabBonus = hitData.m_backstabBonus;
			this.m_statusEffect = hitData.m_statusEffect;
			this.m_toolTier = hitData.m_toolTier;
		}
	}

	// Token: 0x060002DC RID: 732 RVA: 0x000172E0 File Offset: 0x000154E0
	private void OnTriggerEnter(Collider collider)
	{
		if (!this.m_triggerEnterOnly)
		{
			return;
		}
		if (!this.m_useTriggers)
		{
			ZLog.LogWarning("AOE got OnTriggerStay but trigger damage is disabled in " + base.gameObject.name);
			return;
		}
		if (this.m_nview != null && (!this.m_nview.IsValid() || !this.m_nview.IsOwner()))
		{
			return;
		}
		this.OnHit(collider, collider.transform.position);
	}

	// Token: 0x060002DD RID: 733 RVA: 0x00017358 File Offset: 0x00015558
	private void OnTriggerStay(Collider collider)
	{
		if (this.m_triggerEnterOnly)
		{
			return;
		}
		if (!this.m_useTriggers)
		{
			ZLog.LogWarning("AOE got OnTriggerStay but trigger damage is disabled in " + base.gameObject.name);
			return;
		}
		if (this.m_nview != null && (!this.m_nview.IsValid() || !this.m_nview.IsOwner()))
		{
			return;
		}
		this.OnHit(collider, collider.transform.position);
	}

	// Token: 0x060002DE RID: 734 RVA: 0x000173D0 File Offset: 0x000155D0
	private bool OnHit(Collider collider, Vector3 hitPoint)
	{
		GameObject gameObject = Projectile.FindHitObject(collider);
		if (this.m_hitList.Contains(gameObject))
		{
			return false;
		}
		this.m_hitList.Add(gameObject);
		float num = 1f;
		if (this.m_owner && this.m_owner.IsPlayer() && this.m_skill != Skills.SkillType.None)
		{
			num = this.m_owner.GetRandomSkillFactor(this.m_skill);
		}
		bool result = false;
		IDestructible component = gameObject.GetComponent<IDestructible>();
		if (component != null)
		{
			Character character = component as Character;
			if (character)
			{
				if (this.m_nview == null && !character.IsOwner())
				{
					return false;
				}
				if (this.m_owner != null)
				{
					if (!this.m_hitOwner && character == this.m_owner)
					{
						return false;
					}
					if (!this.m_hitSame && character.m_name == this.m_owner.m_name)
					{
						return false;
					}
					bool flag = BaseAI.IsEnemy(this.m_owner, character);
					if (!this.m_hitFriendly && !flag)
					{
						return false;
					}
					if (!this.m_hitEnemy && flag)
					{
						return false;
					}
				}
				if (!this.m_hitCharacters)
				{
					return false;
				}
				if (this.m_dodgeable && character.IsDodgeInvincible())
				{
					return false;
				}
			}
			else if (!this.m_hitProps)
			{
				return false;
			}
			Vector3 dir = this.m_attackForceForward ? base.transform.forward : (hitPoint - base.transform.position).normalized;
			HitData hitData = new HitData();
			hitData.m_hitCollider = collider;
			hitData.m_damage = this.GetDamage();
			hitData.m_pushForce = this.m_attackForce * num;
			hitData.m_backstabBonus = this.m_backstabBonus;
			hitData.m_point = hitPoint;
			hitData.m_dir = dir;
			hitData.m_statusEffect = this.m_statusEffect;
			hitData.m_dodgeable = this.m_dodgeable;
			hitData.m_blockable = this.m_blockable;
			hitData.m_toolTier = this.m_toolTier;
			hitData.SetAttacker(this.m_owner);
			hitData.m_damage.Modify(num);
			component.Damage(hitData);
			if (this.m_damageSelf > 0f)
			{
				IDestructible componentInParent = base.GetComponentInParent<IDestructible>();
				if (componentInParent != null)
				{
					HitData hitData2 = new HitData();
					hitData2.m_damage.m_damage = this.m_damageSelf;
					hitData2.m_point = hitPoint;
					hitData2.m_blockable = false;
					hitData2.m_dodgeable = false;
					componentInParent.Damage(hitData2);
				}
			}
			result = true;
		}
		this.m_hitEffects.Create(hitPoint, Quaternion.identity, null, 1f);
		return result;
	}

	// Token: 0x060002DF RID: 735 RVA: 0x00017652 File Offset: 0x00015852
	private void OnDrawGizmos()
	{
		bool useTriggers = this.m_useTriggers;
	}

	// Token: 0x04000254 RID: 596
	[Header("Attack (overridden by item )")]
	public bool m_useAttackSettings = true;

	// Token: 0x04000255 RID: 597
	public HitData.DamageTypes m_damage;

	// Token: 0x04000256 RID: 598
	public bool m_dodgeable;

	// Token: 0x04000257 RID: 599
	public bool m_blockable;

	// Token: 0x04000258 RID: 600
	public int m_toolTier;

	// Token: 0x04000259 RID: 601
	public float m_attackForce;

	// Token: 0x0400025A RID: 602
	public float m_backstabBonus = 4f;

	// Token: 0x0400025B RID: 603
	public string m_statusEffect = "";

	// Token: 0x0400025C RID: 604
	[Header("Attack (other)")]
	public HitData.DamageTypes m_damagePerLevel;

	// Token: 0x0400025D RID: 605
	public bool m_attackForceForward;

	// Token: 0x0400025E RID: 606
	[Header("Damage self")]
	public float m_damageSelf;

	// Token: 0x0400025F RID: 607
	[Header("Ignore targets")]
	public bool m_hitOwner;

	// Token: 0x04000260 RID: 608
	public bool m_hitSame;

	// Token: 0x04000261 RID: 609
	public bool m_hitFriendly = true;

	// Token: 0x04000262 RID: 610
	public bool m_hitEnemy = true;

	// Token: 0x04000263 RID: 611
	public bool m_hitCharacters = true;

	// Token: 0x04000264 RID: 612
	public bool m_hitProps = true;

	// Token: 0x04000265 RID: 613
	[Header("Other")]
	public Skills.SkillType m_skill;

	// Token: 0x04000266 RID: 614
	public bool m_useTriggers;

	// Token: 0x04000267 RID: 615
	public bool m_triggerEnterOnly;

	// Token: 0x04000268 RID: 616
	public float m_radius = 4f;

	// Token: 0x04000269 RID: 617
	public float m_ttl = 4f;

	// Token: 0x0400026A RID: 618
	public float m_hitInterval = 1f;

	// Token: 0x0400026B RID: 619
	public EffectList m_hitEffects = new EffectList();

	// Token: 0x0400026C RID: 620
	public bool m_attachToCaster;

	// Token: 0x0400026D RID: 621
	private ZNetView m_nview;

	// Token: 0x0400026E RID: 622
	private Character m_owner;

	// Token: 0x0400026F RID: 623
	private List<GameObject> m_hitList = new List<GameObject>();

	// Token: 0x04000270 RID: 624
	private float m_hitTimer;

	// Token: 0x04000271 RID: 625
	private Vector3 m_offset = Vector3.zero;

	// Token: 0x04000272 RID: 626
	private Quaternion m_localRot = Quaternion.identity;

	// Token: 0x04000273 RID: 627
	private int m_level;

	// Token: 0x04000274 RID: 628
	private int m_rayMask;
}
