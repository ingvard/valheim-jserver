using System;
using UnityEngine;

// Token: 0x020000EB RID: 235
public class RandomMovement : MonoBehaviour
{
	// Token: 0x06000E93 RID: 3731 RVA: 0x00068635 File Offset: 0x00066835
	private void Start()
	{
		this.m_basePosition = base.transform.localPosition;
	}

	// Token: 0x06000E94 RID: 3732 RVA: 0x00068648 File Offset: 0x00066848
	private void Update()
	{
		float num = Time.time * this.m_frequency;
		Vector3 b = new Vector3(Mathf.Sin(num) * Mathf.Sin(num * 0.56436f), Mathf.Sin(num * 0.56436f) * Mathf.Sin(num * 0.688742f), Mathf.Cos(num * 0.758348f) * Mathf.Cos(num * 0.4563696f)) * this.m_movement;
		base.transform.localPosition = this.m_basePosition + b;
	}

	// Token: 0x04000D7F RID: 3455
	public float m_frequency = 10f;

	// Token: 0x04000D80 RID: 3456
	public float m_movement = 0.1f;

	// Token: 0x04000D81 RID: 3457
	private Vector3 m_basePosition = Vector3.zero;
}
