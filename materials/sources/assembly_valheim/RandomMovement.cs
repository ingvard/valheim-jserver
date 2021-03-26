using System;
using UnityEngine;

// Token: 0x020000EB RID: 235
public class RandomMovement : MonoBehaviour
{
	// Token: 0x06000E92 RID: 3730 RVA: 0x000684AD File Offset: 0x000666AD
	private void Start()
	{
		this.m_basePosition = base.transform.localPosition;
	}

	// Token: 0x06000E93 RID: 3731 RVA: 0x000684C0 File Offset: 0x000666C0
	private void Update()
	{
		float num = Time.time * this.m_frequency;
		Vector3 b = new Vector3(Mathf.Sin(num) * Mathf.Sin(num * 0.56436f), Mathf.Sin(num * 0.56436f) * Mathf.Sin(num * 0.688742f), Mathf.Cos(num * 0.758348f) * Mathf.Cos(num * 0.4563696f)) * this.m_movement;
		base.transform.localPosition = this.m_basePosition + b;
	}

	// Token: 0x04000D79 RID: 3449
	public float m_frequency = 10f;

	// Token: 0x04000D7A RID: 3450
	public float m_movement = 0.1f;

	// Token: 0x04000D7B RID: 3451
	private Vector3 m_basePosition = Vector3.zero;
}
