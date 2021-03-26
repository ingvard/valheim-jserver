using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000046 RID: 70
public class SmokeRenderer : MonoBehaviour
{
	// Token: 0x06000488 RID: 1160 RVA: 0x00024779 File Offset: 0x00022979
	private void Start()
	{
		this.m_instanceRenderer = base.GetComponent<InstanceRenderer>();
	}

	// Token: 0x06000489 RID: 1161 RVA: 0x00024787 File Offset: 0x00022987
	private void Update()
	{
		if (Utils.GetMainCamera() == null)
		{
			return;
		}
		this.UpdateInstances();
	}

	// Token: 0x0600048A RID: 1162 RVA: 0x000027E0 File Offset: 0x000009E0
	private void UpdateInstances()
	{
	}

	// Token: 0x0400049A RID: 1178
	private InstanceRenderer m_instanceRenderer;

	// Token: 0x0400049B RID: 1179
	private List<Vector4> tempTransforms = new List<Vector4>();
}
