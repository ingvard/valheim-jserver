using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200003E RID: 62
[ExecuteInEditMode]
public class LineAttach : MonoBehaviour
{
	// Token: 0x0600043C RID: 1084 RVA: 0x00022731 File Offset: 0x00020931
	private void Start()
	{
		this.m_lineRenderer = base.GetComponent<LineRenderer>();
	}

	// Token: 0x0600043D RID: 1085 RVA: 0x00022740 File Offset: 0x00020940
	private void LateUpdate()
	{
		for (int i = 0; i < this.m_attachments.Count; i++)
		{
			Transform transform = this.m_attachments[i];
			if (transform)
			{
				this.m_lineRenderer.SetPosition(i, base.transform.InverseTransformPoint(transform.position));
			}
		}
	}

	// Token: 0x04000460 RID: 1120
	public List<Transform> m_attachments = new List<Transform>();

	// Token: 0x04000461 RID: 1121
	private LineRenderer m_lineRenderer;
}
