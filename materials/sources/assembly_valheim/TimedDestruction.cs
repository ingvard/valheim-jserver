using System;
using UnityEngine;

// Token: 0x02000106 RID: 262
public class TimedDestruction : MonoBehaviour
{
	// Token: 0x06000FA0 RID: 4000 RVA: 0x0006E6A1 File Offset: 0x0006C8A1
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_triggerOnAwake)
		{
			this.Trigger();
		}
	}

	// Token: 0x06000FA1 RID: 4001 RVA: 0x0006E6BD File Offset: 0x0006C8BD
	public void Trigger()
	{
		base.InvokeRepeating("DestroyNow", this.m_timeout, 1f);
	}

	// Token: 0x06000FA2 RID: 4002 RVA: 0x00006E80 File Offset: 0x00005080
	public void Trigger(float timeout)
	{
		base.InvokeRepeating("DestroyNow", timeout, 1f);
	}

	// Token: 0x06000FA3 RID: 4003 RVA: 0x0006E6D8 File Offset: 0x0006C8D8
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

	// Token: 0x04000E67 RID: 3687
	public float m_timeout = 1f;

	// Token: 0x04000E68 RID: 3688
	public bool m_triggerOnAwake;

	// Token: 0x04000E69 RID: 3689
	private ZNetView m_nview;
}
