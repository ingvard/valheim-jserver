using System;
using UnityEngine;

// Token: 0x02000025 RID: 37
public class SE_Finder : StatusEffect
{
	// Token: 0x060003A6 RID: 934 RVA: 0x0001F128 File Offset: 0x0001D328
	public override void UpdateStatusEffect(float dt)
	{
		this.m_updateBeaconTimer += dt;
		if (this.m_updateBeaconTimer > 1f)
		{
			this.m_updateBeaconTimer = 0f;
			Beacon beacon = Beacon.FindClosestBeaconInRange(this.m_character.transform.position);
			if (beacon != this.m_beacon)
			{
				this.m_beacon = beacon;
				if (this.m_beacon)
				{
					this.m_lastDistance = Utils.DistanceXZ(this.m_character.transform.position, this.m_beacon.transform.position);
					this.m_pingTimer = 0f;
				}
			}
		}
		if (this.m_beacon != null)
		{
			float num = Utils.DistanceXZ(this.m_character.transform.position, this.m_beacon.transform.position);
			float num2 = Mathf.Clamp01(num / this.m_beacon.m_range);
			float num3 = Mathf.Lerp(this.m_closeFrequency, this.m_distantFrequency, num2);
			this.m_pingTimer += dt;
			if (this.m_pingTimer > num3)
			{
				this.m_pingTimer = 0f;
				if (num2 < 0.2f)
				{
					this.m_pingEffectNear.Create(this.m_character.transform.position, this.m_character.transform.rotation, this.m_character.transform, 1f);
				}
				else if (num2 < 0.6f)
				{
					this.m_pingEffectMed.Create(this.m_character.transform.position, this.m_character.transform.rotation, this.m_character.transform, 1f);
				}
				else
				{
					this.m_pingEffectFar.Create(this.m_character.transform.position, this.m_character.transform.rotation, this.m_character.transform, 1f);
				}
				this.m_lastDistance = num;
			}
		}
	}

	// Token: 0x0400038B RID: 907
	[Header("SE_Finder")]
	public EffectList m_pingEffectNear = new EffectList();

	// Token: 0x0400038C RID: 908
	public EffectList m_pingEffectMed = new EffectList();

	// Token: 0x0400038D RID: 909
	public EffectList m_pingEffectFar = new EffectList();

	// Token: 0x0400038E RID: 910
	public float m_closerTriggerDistance = 2f;

	// Token: 0x0400038F RID: 911
	public float m_furtherTriggerDistance = 4f;

	// Token: 0x04000390 RID: 912
	public float m_closeFrequency = 1f;

	// Token: 0x04000391 RID: 913
	public float m_distantFrequency = 5f;

	// Token: 0x04000392 RID: 914
	private float m_updateBeaconTimer;

	// Token: 0x04000393 RID: 915
	private float m_pingTimer;

	// Token: 0x04000394 RID: 916
	private Beacon m_beacon;

	// Token: 0x04000395 RID: 917
	private float m_lastDistance;
}
