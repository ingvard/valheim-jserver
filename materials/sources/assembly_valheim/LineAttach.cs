using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200003E RID: 62
[ExecuteInEditMode]
public class LineAttach : MonoBehaviour
{
	// Token: 0x0600043B RID: 1083 RVA: 0x0002267D File Offset: 0x0002087D
	private void Start()
	{
		this.m_lineRenderer = base.GetComponent<LineRenderer>();
	}

	// Token: 0x0600043C RID: 1084 RVA: 0x0002268C File Offset: 0x0002088C
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

	// Token: 0x0400045C RID: 1116
	public List<Transform> m_attachments = new List<Transform>();

	// Token: 0x0400045D RID: 1117
	private LineRenderer m_lineRenderer;
}
