﻿using System;
using UnityEngine;

// Token: 0x020000AA RID: 170
public class ReflectionUpdate : MonoBehaviour
{
	// Token: 0x1700002C RID: 44
	// (get) Token: 0x06000BAB RID: 2987 RVA: 0x00053744 File Offset: 0x00051944
	public static ReflectionUpdate instance
	{
		get
		{
			return ReflectionUpdate.m_instance;
		}
	}

	// Token: 0x06000BAC RID: 2988 RVA: 0x0005374B File Offset: 0x0005194B
	private void Start()
	{
		ReflectionUpdate.m_instance = this;
		this.m_current = this.m_probe1;
	}

	// Token: 0x06000BAD RID: 2989 RVA: 0x0005375F File Offset: 0x0005195F
	private void OnDestroy()
	{
		ReflectionUpdate.m_instance = null;
	}

	// Token: 0x06000BAE RID: 2990 RVA: 0x00053768 File Offset: 0x00051968
	public void UpdateReflection()
	{
		Vector3 vector = ZNet.instance.GetReferencePosition();
		vector += Vector3.up * this.m_reflectionHeight;
		this.m_current = ((this.m_current == this.m_probe1) ? this.m_probe2 : this.m_probe1);
		this.m_current.transform.position = vector;
		this.m_renderID = this.m_current.RenderProbe();
	}

	// Token: 0x06000BAF RID: 2991 RVA: 0x000537E0 File Offset: 0x000519E0
	private void Update()
	{
		float deltaTime = Time.deltaTime;
		this.m_updateTimer += deltaTime;
		if (this.m_updateTimer > this.m_interval)
		{
			this.m_updateTimer = 0f;
			this.UpdateReflection();
		}
		if (this.m_current.IsFinishedRendering(this.m_renderID))
		{
			float num = Mathf.Clamp01(this.m_updateTimer / this.m_transitionDuration);
			num = Mathf.Pow(num, this.m_power);
			if (this.m_probe1 == this.m_current)
			{
				this.m_probe1.importance = 1;
				this.m_probe2.importance = 0;
				Vector3 size = this.m_probe1.size;
				size.x = 2000f * num;
				size.y = 1000f * num;
				size.z = 2000f * num;
				this.m_probe1.size = size;
				this.m_probe2.size = new Vector3(2001f, 1001f, 2001f);
				return;
			}
			this.m_probe1.importance = 0;
			this.m_probe2.importance = 1;
			Vector3 size2 = this.m_probe2.size;
			size2.x = 2000f * num;
			size2.y = 1000f * num;
			size2.z = 2000f * num;
			this.m_probe2.size = size2;
			this.m_probe1.size = new Vector3(2001f, 1001f, 2001f);
		}
	}

	// Token: 0x04000AE0 RID: 2784
	private static ReflectionUpdate m_instance;

	// Token: 0x04000AE1 RID: 2785
	public ReflectionProbe m_probe1;

	// Token: 0x04000AE2 RID: 2786
	public ReflectionProbe m_probe2;

	// Token: 0x04000AE3 RID: 2787
	public float m_interval = 3f;

	// Token: 0x04000AE4 RID: 2788
	public float m_reflectionHeight = 5f;

	// Token: 0x04000AE5 RID: 2789
	public float m_transitionDuration = 3f;

	// Token: 0x04000AE6 RID: 2790
	public float m_power = 1f;

	// Token: 0x04000AE7 RID: 2791
	private ReflectionProbe m_current;

	// Token: 0x04000AE8 RID: 2792
	private int m_renderID;

	// Token: 0x04000AE9 RID: 2793
	private float m_updateTimer;
}
