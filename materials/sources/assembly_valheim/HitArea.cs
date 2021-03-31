using System;
using UnityEngine;

// Token: 0x020000D3 RID: 211
public class HitArea : MonoBehaviour, IDestructible
{
	// Token: 0x06000DB7 RID: 3511 RVA: 0x000027E2 File Offset: 0x000009E2
	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Default;
	}

	// Token: 0x06000DB8 RID: 3512 RVA: 0x00062232 File Offset: 0x00060432
	public void Damage(HitData hit)
	{
		if (this.m_onHit != null)
		{
			this.m_onHit(hit, this);
		}
	}

	// Token: 0x04000C6D RID: 3181
	public Action<HitData, HitArea> m_onHit;

	// Token: 0x04000C6E RID: 3182
	public float m_health = 1f;

	// Token: 0x04000C6F RID: 3183
	[NonSerialized]
	public GameObject m_parentObject;
}
