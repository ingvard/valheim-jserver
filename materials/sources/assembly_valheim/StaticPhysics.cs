using System;
using UnityEngine;

// Token: 0x020000B1 RID: 177
public class StaticPhysics : SlowUpdate
{
	// Token: 0x06000BDF RID: 3039 RVA: 0x00054B4E File Offset: 0x00052D4E
	public override void Awake()
	{
		base.Awake();
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_createTime = Time.time;
	}

	// Token: 0x06000BE0 RID: 3040 RVA: 0x00054B6D File Offset: 0x00052D6D
	private bool ShouldUpdate()
	{
		return Time.time - this.m_createTime > 20f;
	}

	// Token: 0x06000BE1 RID: 3041 RVA: 0x00054B84 File Offset: 0x00052D84
	public override void SUpdate()
	{
		if (!this.ShouldUpdate() || ZNetScene.instance.OutsideActiveArea(base.transform.position) || this.m_falling)
		{
			return;
		}
		if (this.m_fall)
		{
			this.CheckFall();
		}
		if (this.m_pushUp)
		{
			this.PushUp();
		}
	}

	// Token: 0x06000BE2 RID: 3042 RVA: 0x00054BD8 File Offset: 0x00052DD8
	private void CheckFall()
	{
		float fallHeight = this.GetFallHeight();
		if (base.transform.position.y > fallHeight + 0.05f)
		{
			this.Fall();
		}
	}

	// Token: 0x06000BE3 RID: 3043 RVA: 0x00054C0C File Offset: 0x00052E0C
	private float GetFallHeight()
	{
		if (this.m_checkSolids)
		{
			float result;
			if (ZoneSystem.instance.GetSolidHeight(base.transform.position, this.m_fallCheckRadius, out result, base.transform))
			{
				return result;
			}
			return base.transform.position.y;
		}
		else
		{
			float result2;
			if (ZoneSystem.instance.GetGroundHeight(base.transform.position, out result2))
			{
				return result2;
			}
			return base.transform.position.y;
		}
	}

	// Token: 0x06000BE4 RID: 3044 RVA: 0x00054C84 File Offset: 0x00052E84
	private void Fall()
	{
		this.m_falling = true;
		base.gameObject.isStatic = false;
		base.InvokeRepeating("FallUpdate", 0.05f, 0.05f);
	}

	// Token: 0x06000BE5 RID: 3045 RVA: 0x00054CB0 File Offset: 0x00052EB0
	private void FallUpdate()
	{
		float fallHeight = this.GetFallHeight();
		Vector3 position = base.transform.position;
		position.y -= 0.2f;
		if (position.y <= fallHeight)
		{
			position.y = fallHeight;
			this.StopFalling();
		}
		base.transform.position = position;
		if (this.m_nview && this.m_nview.IsValid() && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().SetPosition(base.transform.position);
		}
	}

	// Token: 0x06000BE6 RID: 3046 RVA: 0x00054D45 File Offset: 0x00052F45
	private void StopFalling()
	{
		base.gameObject.isStatic = true;
		this.m_falling = false;
		base.CancelInvoke("FallUpdate");
	}

	// Token: 0x06000BE7 RID: 3047 RVA: 0x00054D68 File Offset: 0x00052F68
	private void PushUp()
	{
		float num;
		if (ZoneSystem.instance.GetGroundHeight(base.transform.position, out num) && base.transform.position.y < num - 0.05f)
		{
			base.gameObject.isStatic = false;
			Vector3 position = base.transform.position;
			position.y = num;
			base.transform.position = position;
			base.gameObject.isStatic = true;
			if (this.m_nview && this.m_nview.IsValid() && this.m_nview.IsOwner())
			{
				this.m_nview.GetZDO().SetPosition(base.transform.position);
			}
		}
	}

	// Token: 0x04000B02 RID: 2818
	public bool m_pushUp = true;

	// Token: 0x04000B03 RID: 2819
	public bool m_fall = true;

	// Token: 0x04000B04 RID: 2820
	public bool m_checkSolids;

	// Token: 0x04000B05 RID: 2821
	public float m_fallCheckRadius;

	// Token: 0x04000B06 RID: 2822
	private ZNetView m_nview;

	// Token: 0x04000B07 RID: 2823
	private const float m_fallSpeed = 4f;

	// Token: 0x04000B08 RID: 2824
	private const float m_fallStep = 0.05f;

	// Token: 0x04000B09 RID: 2825
	private float m_createTime;

	// Token: 0x04000B0A RID: 2826
	private bool m_falling;
}
