using System;
using UnityEngine;

// Token: 0x0200010F RID: 271
public class WaterTrigger : MonoBehaviour
{
	// Token: 0x06000FF6 RID: 4086 RVA: 0x000704CC File Offset: 0x0006E6CC
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

	// Token: 0x04000EDA RID: 3802
	public EffectList m_effects = new EffectList();

	// Token: 0x04000EDB RID: 3803
	public float m_cooldownDelay = 2f;

	// Token: 0x04000EDC RID: 3804
	private float m_cooldownTimer;
}
