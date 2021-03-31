using System;
using UnityEngine;

// Token: 0x02000105 RID: 261
public class ThorFly : MonoBehaviour
{
	// Token: 0x06000F9E RID: 3998 RVA: 0x000027E0 File Offset: 0x000009E0
	private void Start()
	{
	}

	// Token: 0x06000F9F RID: 3999 RVA: 0x0006E798 File Offset: 0x0006C998
	private void Update()
	{
		base.transform.position = base.transform.position + base.transform.forward * this.m_speed * Time.deltaTime;
		this.m_timer += Time.deltaTime;
		if (this.m_timer > this.m_ttl)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x04000E6A RID: 3690
	public float m_speed = 100f;

	// Token: 0x04000E6B RID: 3691
	public float m_ttl = 10f;

	// Token: 0x04000E6C RID: 3692
	private float m_timer;
}
