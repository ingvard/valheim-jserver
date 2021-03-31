using System;
using UnityEngine;

// Token: 0x02000108 RID: 264
public class Tracker : MonoBehaviour
{
	// Token: 0x06000FAC RID: 4012 RVA: 0x0006E93C File Offset: 0x0006CB3C
	private void Awake()
	{
		ZNetView component = base.GetComponent<ZNetView>();
		if (component && component.IsOwner())
		{
			this.m_active = true;
			ZNet.instance.SetReferencePosition(base.transform.position);
		}
	}

	// Token: 0x06000FAD RID: 4013 RVA: 0x0006E97C File Offset: 0x0006CB7C
	public void SetActive(bool active)
	{
		this.m_active = active;
	}

	// Token: 0x06000FAE RID: 4014 RVA: 0x0006E985 File Offset: 0x0006CB85
	private void OnDestroy()
	{
		this.m_active = false;
	}

	// Token: 0x06000FAF RID: 4015 RVA: 0x0006E98E File Offset: 0x0006CB8E
	private void FixedUpdate()
	{
		if (this.m_active)
		{
			ZNet.instance.SetReferencePosition(base.transform.position);
		}
	}

	// Token: 0x04000E77 RID: 3703
	private bool m_active;
}
