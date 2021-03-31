using System;
using UnityEngine;

// Token: 0x02000106 RID: 262
public class TimedDestruction : MonoBehaviour
{
	// Token: 0x06000FA1 RID: 4001 RVA: 0x0006E829 File Offset: 0x0006CA29
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_triggerOnAwake)
		{
			this.Trigger();
		}
	}

	// Token: 0x06000FA2 RID: 4002 RVA: 0x0006E845 File Offset: 0x0006CA45
	public void Trigger()
	{
		base.InvokeRepeating("DestroyNow", this.m_timeout, 1f);
	}

	// Token: 0x06000FA3 RID: 4003 RVA: 0x00006EA4 File Offset: 0x000050A4
	public void Trigger(float timeout)
	{
		base.InvokeRepeating("DestroyNow", timeout, 1f);
	}

	// Token: 0x06000FA4 RID: 4004 RVA: 0x0006E860 File Offset: 0x0006CA60
	private void DestroyNow()
	{
		if (!this.m_nview)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		ZNetScene.instance.Destroy(base.gameObject);
	}

	// Token: 0x04000E6D RID: 3693
	public float m_timeout = 1f;

	// Token: 0x04000E6E RID: 3694
	public bool m_triggerOnAwake;

	// Token: 0x04000E6F RID: 3695
	private ZNetView m_nview;
}
