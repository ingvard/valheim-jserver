using System;
using UnityEngine;

// Token: 0x020000E0 RID: 224
public class MenuShipMovement : MonoBehaviour
{
	// Token: 0x06000E0B RID: 3595 RVA: 0x00064025 File Offset: 0x00062225
	private void Start()
	{
		this.m_time = (float)UnityEngine.Random.Range(0, 10);
	}

	// Token: 0x06000E0C RID: 3596 RVA: 0x00064038 File Offset: 0x00062238
	private void Update()
	{
		this.m_time += Time.deltaTime;
		base.transform.rotation = Quaternion.Euler(Mathf.Sin(this.m_time * this.m_freq) * this.m_xAngle, 0f, Mathf.Sin(this.m_time * 1.5341234f * this.m_freq) * this.m_zAngle);
	}

	// Token: 0x04000CBA RID: 3258
	public float m_freq = 1f;

	// Token: 0x04000CBB RID: 3259
	public float m_xAngle = 5f;

	// Token: 0x04000CBC RID: 3260
	public float m_zAngle = 5f;

	// Token: 0x04000CBD RID: 3261
	private float m_time;
}
