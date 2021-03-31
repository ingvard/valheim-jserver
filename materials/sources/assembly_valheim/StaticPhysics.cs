using System;
using UnityEngine;

// Token: 0x020000B1 RID: 177
public class StaticPhysics : SlowUpdate
{
	// Token: 0x06000BE0 RID: 3040 RVA: 0x00054CD6 File Offset: 0x00052ED6
	public override void Awake()
	{
		base.Awake();
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_createTime = Time.time;
	}

	// Token: 0x06000BE1 RID: 3041 RVA: 0x00054CF5 File Offset: 0x00052EF5
	private bool ShouldUpdate()
	{
		return Time.time - this.m_createTime > 20f;
	}

	// Token: 0x06000BE2 RID: 3042 RVA: 0x00054D0C File Offset: 0x00052F0C
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

	// Token: 0x06000BE3 RID: 3043 RVA: 0x00054D60 File Offset: 0x00052F60
	private void CheckFall()
	{
		float fallHeight = this.GetFallHeight();
		if (base.transform.position.y > fallHeight + 0.05f)
		{
			this.Fall();
		}
	}

	// Token: 0x06000BE4 RID: 3044 RVA: 0x00054D94 File Offset: 0x00052F94
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

	// Token: 0x06000BE5 RID: 3045 RVA: 0x00054E0C File Offset: 0x0005300C
	private void Fall()
	{
		this.m_falling = true;
		base.gameObject.isStatic = false;
		base.InvokeRepeating("FallUpdate", 0.05f, 0.05f);
	}

	// Token: 0x06000BE6 RID: 3046 RVA: 0x00054E38 File Offset: 0x00053038
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

	// Token: 0x06000BE7 RID: 3047 RVA: 0x00054ECD File Offset: 0x000530CD
	private void StopFalling()
	{
		base.gameObject.isStatic = true;
		this.m_falling = false;
		base.CancelInvoke("FallUpdate");
	}

	// Token: 0x06000BE8 RID: 3048 RVA: 0x00054EF0 File Offset: 0x000530F0
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

	// Token: 0x04000B08 RID: 2824
	public bool m_pushUp = true;

	// Token: 0x04000B09 RID: 2825
	public bool m_fall = true;

	// Token: 0x04000B0A RID: 2826
	public bool m_checkSolids;

	// Token: 0x04000B0B RID: 2827
	public float m_fallCheckRadius;

	// Token: 0x04000B0C RID: 2828
	private ZNetView m_nview;

	// Token: 0x04000B0D RID: 2829
	private const float m_fallSpeed = 4f;

	// Token: 0x04000B0E RID: 2830
	private const float m_fallStep = 0.05f;

	// Token: 0x04000B0F RID: 2831
	private float m_createTime;

	// Token: 0x04000B10 RID: 2832
	private bool m_falling;
}
