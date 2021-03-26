using System;
using UnityEngine;

// Token: 0x02000105 RID: 261
public class ThorFly : MonoBehaviour
{
	// Token: 0x06000F9D RID: 3997 RVA: 0x000027E0 File Offset: 0x000009E0
	private void Start()
	{
	}

	// Token: 0x06000F9E RID: 3998 RVA: 0x0006E610 File Offset: 0x0006C810
	private void Update()
	{
		base.transform.position = base.transform.position + base.transform.forward * this.m_speed * Time.deltaTime;
		this.m_timer += Time.deltaTime;
		if (this.m_timer > this.m_ttl)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x04000E64 RID: 3684
	public float m_speed = 100f;

	// Token: 0x04000E65 RID: 3685
	public float m_ttl = 10f;

	// Token: 0x04000E66 RID: 3686
	private float m_timer;
}
