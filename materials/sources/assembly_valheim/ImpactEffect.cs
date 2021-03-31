using System;
using UnityEngine;

// Token: 0x0200003C RID: 60
public class ImpactEffect : MonoBehaviour
{
	// Token: 0x06000433 RID: 1075 RVA: 0x00022097 File Offset: 0x00020297
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_body = base.GetComponent<Rigidbody>();
		if (this.m_maxVelocity < this.m_minVelocity)
		{
			this.m_maxVelocity = this.m_minVelocity;
		}
	}

	// Token: 0x06000434 RID: 1076 RVA: 0x000220CC File Offset: 0x000202CC
	public void OnCollisionEnter(Collision info)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview && !this.m_nview.IsOwner())
		{
			return;
		}
		if (info.contacts.Length == 0)
		{
			return;
		}
		if (!this.m_hitEffectEnabled)
		{
			return;
		}
		if ((this.m_triggerMask.value & 1 << info.collider.gameObject.layer) == 0)
		{
			return;
		}
		float magnitude = info.relativeVelocity.magnitude;
		if (magnitude < this.m_minVelocity)
		{
			return;
		}
		ContactPoint contactPoint = info.contacts[0];
		Vector3 point = contactPoint.point;
		Vector3 pointVelocity = this.m_body.GetPointVelocity(point);
		this.m_hitEffectEnabled = false;
		base.Invoke("ResetHitTimer", this.m_interval);
		if (this.m_damages.HaveDamage())
		{
			GameObject gameObject = Projectile.FindHitObject(contactPoint.otherCollider);
			float num = Utils.LerpStep(this.m_minVelocity, this.m_maxVelocity, magnitude);
			IDestructible component = gameObject.GetComponent<IDestructible>();
			if (component != null)
			{
				Character character = component as Character;
				if (character)
				{
					if (!this.m_damagePlayers && character.IsPlayer())
					{
						return;
					}
					float num2 = Vector3.Dot(-info.relativeVelocity.normalized, pointVelocity);
					if (num2 < this.m_minVelocity)
					{
						return;
					}
					ZLog.Log("Rel vel " + num2);
					num = Utils.LerpStep(this.m_minVelocity, this.m_maxVelocity, num2);
					if (character.GetSEMan().HaveStatusAttribute(StatusEffect.StatusAttribute.DoubleImpactDamage))
					{
						num *= 2f;
					}
				}
				if (!this.m_damageFish && gameObject.GetComponent<Fish>())
				{
					return;
				}
				HitData hitData = new HitData();
				hitData.m_point = point;
				hitData.m_dir = pointVelocity.normalized;
				hitData.m_hitCollider = info.collider;
				hitData.m_toolTier = this.m_toolTier;
				hitData.m_damage = this.m_damages.Clone();
				hitData.m_damage.Modify(num);
				component.Damage(hitData);
			}
			if (this.m_damageToSelf)
			{
				IDestructible component2 = base.GetComponent<IDestructible>();
				if (component2 != null)
				{
					HitData hitData2 = new HitData();
					hitData2.m_point = point;
					hitData2.m_dir = -pointVelocity.normalized;
					hitData2.m_toolTier = this.m_toolTier;
					hitData2.m_damage = this.m_damages.Clone();
					hitData2.m_damage.Modify(num);
					component2.Damage(hitData2);
				}
			}
		}
		Vector3 rhs = Vector3.Cross(-Vector3.Normalize(info.relativeVelocity), contactPoint.normal);
		Vector3 vector = Vector3.Cross(contactPoint.normal, rhs);
		Quaternion rot = Quaternion.identity;
		if (vector != Vector3.zero && contactPoint.normal != Vector3.zero)
		{
			rot = Quaternion.LookRotation(vector, contactPoint.normal);
		}
		this.m_hitEffect.Create(point, rot, null, 1f);
		if (this.m_firstHit && this.m_hitDestroyChance > 0f && UnityEngine.Random.value <= this.m_hitDestroyChance)
		{
			this.m_destroyEffect.Create(point, rot, null, 1f);
			GameObject gameObject2 = base.gameObject;
			if (base.transform.parent)
			{
				Animator componentInParent = base.transform.GetComponentInParent<Animator>();
				if (componentInParent)
				{
					gameObject2 = componentInParent.gameObject;
				}
			}
			UnityEngine.Object.Destroy(gameObject2);
		}
		this.m_firstHit = false;
	}

	// Token: 0x06000435 RID: 1077 RVA: 0x00022440 File Offset: 0x00020640
	private Vector3 GetAVGPos(ContactPoint[] points)
	{
		ZLog.Log("Pooints " + points.Length);
		Vector3 vector = Vector3.zero;
		foreach (ContactPoint contactPoint in points)
		{
			ZLog.Log("P " + contactPoint.otherCollider.gameObject.name);
			vector += contactPoint.point;
		}
		return vector;
	}

	// Token: 0x06000436 RID: 1078 RVA: 0x000224B1 File Offset: 0x000206B1
	private void ResetHitTimer()
	{
		this.m_hitEffectEnabled = true;
	}

	// Token: 0x04000445 RID: 1093
	public EffectList m_hitEffect = new EffectList();

	// Token: 0x04000446 RID: 1094
	public EffectList m_destroyEffect = new EffectList();

	// Token: 0x04000447 RID: 1095
	public float m_hitDestroyChance;

	// Token: 0x04000448 RID: 1096
	public float m_minVelocity;

	// Token: 0x04000449 RID: 1097
	public float m_maxVelocity;

	// Token: 0x0400044A RID: 1098
	public bool m_damageToSelf;

	// Token: 0x0400044B RID: 1099
	public bool m_damagePlayers = true;

	// Token: 0x0400044C RID: 1100
	public bool m_damageFish;

	// Token: 0x0400044D RID: 1101
	public int m_toolTier;

	// Token: 0x0400044E RID: 1102
	public HitData.DamageTypes m_damages;

	// Token: 0x0400044F RID: 1103
	public LayerMask m_triggerMask;

	// Token: 0x04000450 RID: 1104
	public float m_interval = 0.5f;

	// Token: 0x04000451 RID: 1105
	private bool m_firstHit = true;

	// Token: 0x04000452 RID: 1106
	private bool m_hitEffectEnabled = true;

	// Token: 0x04000453 RID: 1107
	private ZNetView m_nview;

	// Token: 0x04000454 RID: 1108
	private Rigidbody m_body;
}
