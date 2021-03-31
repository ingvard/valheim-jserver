using System;
using UnityEngine;

// Token: 0x020000E0 RID: 224
public class MenuShipMovement : MonoBehaviour
{
	// Token: 0x06000E0C RID: 3596 RVA: 0x000641AD File Offset: 0x000623AD
	private void Start()
	{
		this.m_time = (float)UnityEngine.Random.Range(0, 10);
	}

	// Token: 0x06000E0D RID: 3597 RVA: 0x000641C0 File Offset: 0x000623C0
	private void Update()
	{
		this.m_time += Time.deltaTime;
		base.transform.rotation = Quaternion.Euler(Mathf.Sin(this.m_time * this.m_freq) * this.m_xAngle, 0f, Mathf.Sin(this.m_time * 1.5341234f * this.m_freq) * this.m_zAngle);
	}

	// Token: 0x04000CC0 RID: 3264
	public float m_freq = 1f;

	// Token: 0x04000CC1 RID: 3265
	public float m_xAngle = 5f;

	// Token: 0x04000CC2 RID: 3266
	public float m_zAngle = 5f;

	// Token: 0x04000CC3 RID: 3267
	private float m_time;
}
