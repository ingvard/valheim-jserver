using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000046 RID: 70
public class SmokeRenderer : MonoBehaviour
{
	// Token: 0x06000489 RID: 1161 RVA: 0x0002482D File Offset: 0x00022A2D
	private void Start()
	{
		this.m_instanceRenderer = base.GetComponent<InstanceRenderer>();
	}

	// Token: 0x0600048A RID: 1162 RVA: 0x0002483B File Offset: 0x00022A3B
	private void Update()
	{
		if (Utils.GetMainCamera() == null)
		{
			return;
		}
		this.UpdateInstances();
	}

	// Token: 0x0600048B RID: 1163 RVA: 0x000027E0 File Offset: 0x000009E0
	private void UpdateInstances()
	{
	}

	// Token: 0x0400049E RID: 1182
	private InstanceRenderer m_instanceRenderer;

	// Token: 0x0400049F RID: 1183
	private List<Vector4> tempTransforms = new List<Vector4>();
}
