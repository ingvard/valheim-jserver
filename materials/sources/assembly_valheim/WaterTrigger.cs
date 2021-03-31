using System;
using UnityEngine;

// Token: 0x0200010F RID: 271
public class WaterTrigger : MonoBehaviour
{
	// Token: 0x06000FF7 RID: 4087 RVA: 0x00070654 File Offset: 0x0006E854
	private void Update()
	{
		this.m_cooldownTimer += Time.deltaTime;
		if (this.m_cooldownTimer > this.m_cooldownDelay)
		{
			float waterLevel = WaterVolume.GetWaterLevel(base.transform.position, 1f);
			if (base.transform.position.y < waterLevel)
			{
				this.m_effects.Create(base.transform.position, base.transform.rotation, base.transform, 1f);
				this.m_cooldownTimer = 0f;
			}
		}
	}

	// Token: 0x04000EE0 RID: 3808
	public EffectList m_effects = new EffectList();

	// Token: 0x04000EE1 RID: 3809
	public float m_cooldownDelay = 2f;

	// Token: 0x04000EE2 RID: 3810
	private float m_cooldownTimer;
}
