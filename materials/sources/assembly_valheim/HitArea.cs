using System;
using UnityEngine;

// Token: 0x020000D3 RID: 211
public class HitArea : MonoBehaviour, IDestructible
{
	// Token: 0x06000DB6 RID: 3510 RVA: 0x000027E2 File Offset: 0x000009E2
	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Default;
	}

	// Token: 0x06000DB7 RID: 3511 RVA: 0x000620AA File Offset: 0x000602AA
	public void Damage(HitData hit)
	{
		if (this.m_onHit != null)
		{
			this.m_onHit(hit, this);
		}
	}

	// Token: 0x04000C67 RID: 3175
	public Action<HitData, HitArea> m_onHit;

	// Token: 0x04000C68 RID: 3176
	public float m_health = 1f;

	// Token: 0x04000C69 RID: 3177
	[NonSerialized]
	public GameObject m_parentObject;
}
