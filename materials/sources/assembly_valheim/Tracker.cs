using System;
using UnityEngine;

// Token: 0x02000108 RID: 264
public class Tracker : MonoBehaviour
{
	// Token: 0x06000FAB RID: 4011 RVA: 0x0006E7B4 File Offset: 0x0006C9B4
	private void Awake()
	{
		ZNetView component = base.GetComponent<ZNetView>();
		if (component && component.IsOwner())
		{
			this.m_active = true;
			ZNet.instance.SetReferencePosition(base.transform.position);
		}
	}

	// Token: 0x06000FAC RID: 4012 RVA: 0x0006E7F4 File Offset: 0x0006C9F4
	public void SetActive(bool active)
	{
		this.m_active = active;
	}

	// Token: 0x06000FAD RID: 4013 RVA: 0x0006E7FD File Offset: 0x0006C9FD
	private void OnDestroy()
	{
		this.m_active = false;
	}

	// Token: 0x06000FAE RID: 4014 RVA: 0x0006E806 File Offset: 0x0006CA06
	private void FixedUpdate()
	{
		if (this.m_active)
		{
			ZNet.instance.SetReferencePosition(base.transform.position);
		}
	}

	// Token: 0x04000E71 RID: 3697
	private bool m_active;
}
