using System;
using UnityEngine;

// Token: 0x020000DB RID: 219
public class Leviathan : MonoBehaviour
{
	// Token: 0x06000DF2 RID: 3570 RVA: 0x000637F4 File Offset: 0x000619F4
	private void Awake()
	{
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_zanimator = base.GetComponent<ZSyncAnimation>();
		this.m_animator = base.GetComponentInChildren<Animator>();
		if (base.GetComponent<MineRock>())
		{
			MineRock mineRock = this.m_mineRock;
			mineRock.m_onHit = (Action)Delegate.Combine(mineRock.m_onHit, new Action(this.OnHit));
		}
	}

	// Token: 0x06000DF3 RID: 3571 RVA: 0x00063868 File Offset: 0x00061A68
	private void FixedUpdate()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		float waterLevel = WaterVolume.GetWaterLevel(base.transform.position, this.m_waveScale);
		if (waterLevel > -100f)
		{
			Vector3 position = this.m_body.position;
			float num = Mathf.Clamp((waterLevel - (position.y + this.m_floatOffset)) * this.m_movementSpeed * Time.fixedDeltaTime, -this.m_maxSpeed, this.m_maxSpeed);
			position.y += num;
			this.m_body.MovePosition(position);
		}
		else
		{
			Vector3 position2 = this.m_body.position;
			position2.y = 0f;
			this.m_body.MovePosition(Vector3.MoveTowards(this.m_body.position, position2, Time.deltaTime));
		}
		if (this.m_animator.GetCurrentAnimatorStateInfo(0).IsTag("submerged"))
		{
			this.m_nview.Destroy();
		}
	}

	// Token: 0x06000DF4 RID: 3572 RVA: 0x00063964 File Offset: 0x00061B64
	private void OnHit()
	{
		if (UnityEngine.Random.value <= this.m_hitReactionChance)
		{
			if (this.m_left)
			{
				return;
			}
			this.m_reactionEffects.Create(base.transform.position, base.transform.rotation, null, 1f);
			this.m_zanimator.SetTrigger("shake");
			base.Invoke("Leave", (float)this.m_leaveDelay);
		}
	}

	// Token: 0x06000DF5 RID: 3573 RVA: 0x000639D4 File Offset: 0x00061BD4
	private void Leave()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_left)
		{
			return;
		}
		this.m_left = true;
		this.m_leaveEffects.Create(base.transform.position, base.transform.rotation, null, 1f);
		this.m_zanimator.SetTrigger("dive");
	}

	// Token: 0x04000C98 RID: 3224
	public float m_waveScale = 0.5f;

	// Token: 0x04000C99 RID: 3225
	public float m_floatOffset;

	// Token: 0x04000C9A RID: 3226
	public float m_movementSpeed = 0.1f;

	// Token: 0x04000C9B RID: 3227
	public float m_maxSpeed = 1f;

	// Token: 0x04000C9C RID: 3228
	public MineRock m_mineRock;

	// Token: 0x04000C9D RID: 3229
	public float m_hitReactionChance = 0.25f;

	// Token: 0x04000C9E RID: 3230
	public int m_leaveDelay = 5;

	// Token: 0x04000C9F RID: 3231
	public EffectList m_reactionEffects = new EffectList();

	// Token: 0x04000CA0 RID: 3232
	public EffectList m_leaveEffects = new EffectList();

	// Token: 0x04000CA1 RID: 3233
	private Rigidbody m_body;

	// Token: 0x04000CA2 RID: 3234
	private ZNetView m_nview;

	// Token: 0x04000CA3 RID: 3235
	private ZSyncAnimation m_zanimator;

	// Token: 0x04000CA4 RID: 3236
	private Animator m_animator;

	// Token: 0x04000CA5 RID: 3237
	private bool m_left;
}
