using System;
using UnityEngine;

// Token: 0x0200001F RID: 31
public class AnimalAI : BaseAI
{
	// Token: 0x06000314 RID: 788 RVA: 0x0001A4AC File Offset: 0x000186AC
	protected override void Awake()
	{
		base.Awake();
		this.m_updateTargetTimer = UnityEngine.Random.Range(0f, 2f);
	}

	// Token: 0x06000315 RID: 789 RVA: 0x0001A4C9 File Offset: 0x000186C9
	protected override void OnDamaged(float damage, Character attacker)
	{
		base.OnDamaged(damage, attacker);
		this.SetAlerted(true);
	}

	// Token: 0x06000316 RID: 790 RVA: 0x0001A4DC File Offset: 0x000186DC
	protected override void UpdateAI(float dt)
	{
		base.UpdateAI(dt);
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_afraidOfFire && base.AvoidFire(dt, null, true))
		{
			return;
		}
		this.m_updateTargetTimer -= dt;
		if (this.m_updateTargetTimer <= 0f)
		{
			this.m_updateTargetTimer = (Character.IsCharacterInRange(base.transform.position, 32f) ? 2f : 10f);
			Character character = base.FindEnemy();
			if (character)
			{
				this.m_target = character;
			}
		}
		if (this.m_target && this.m_target.IsDead())
		{
			this.m_target = null;
		}
		if (this.m_target)
		{
			bool flag = base.CanSenseTarget(this.m_target);
			base.SetTargetInfo(this.m_target.GetZDOID());
			if (flag)
			{
				this.SetAlerted(true);
			}
		}
		else
		{
			base.SetTargetInfo(ZDOID.None);
		}
		if (base.IsAlerted())
		{
			this.m_inDangerTimer += dt;
			if (this.m_inDangerTimer > this.m_timeToSafe)
			{
				this.m_target = null;
				this.SetAlerted(false);
			}
		}
		if (this.m_target)
		{
			base.Flee(dt, this.m_target.transform.position);
			this.m_target.OnTargeted(false, false);
			return;
		}
		base.IdleMovement(dt);
	}

	// Token: 0x06000317 RID: 791 RVA: 0x0001A638 File Offset: 0x00018838
	protected override void SetAlerted(bool alert)
	{
		if (alert)
		{
			this.m_inDangerTimer = 0f;
		}
		base.SetAlerted(alert);
	}

	// Token: 0x040002EB RID: 747
	private const float m_updateTargetFarRange = 32f;

	// Token: 0x040002EC RID: 748
	private const float m_updateTargetIntervalNear = 2f;

	// Token: 0x040002ED RID: 749
	private const float m_updateTargetIntervalFar = 10f;

	// Token: 0x040002EE RID: 750
	public float m_timeToSafe = 4f;

	// Token: 0x040002EF RID: 751
	private Character m_target;

	// Token: 0x040002F0 RID: 752
	private float m_inDangerTimer;

	// Token: 0x040002F1 RID: 753
	private float m_updateTargetTimer;
}
