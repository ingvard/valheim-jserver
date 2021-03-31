using System;
using UnityEngine;

// Token: 0x02000009 RID: 9
public class Growup : MonoBehaviour
{
	// Token: 0x06000108 RID: 264 RVA: 0x00007D9E File Offset: 0x00005F9E
	private void Start()
	{
		this.m_baseAI = base.GetComponent<BaseAI>();
		this.m_nview = base.GetComponent<ZNetView>();
		base.InvokeRepeating("GrowUpdate", UnityEngine.Random.Range(10f, 15f), 10f);
	}

	// Token: 0x06000109 RID: 265 RVA: 0x00007DD8 File Offset: 0x00005FD8
	private void GrowUpdate()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_baseAI.GetTimeSinceSpawned().TotalSeconds > (double)this.m_growTime)
		{
			Character component = base.GetComponent<Character>();
			Character component2 = UnityEngine.Object.Instantiate<GameObject>(this.m_grownPrefab, base.transform.position, base.transform.rotation).GetComponent<Character>();
			if (component && component2)
			{
				component2.SetTamed(component.IsTamed());
				component2.SetLevel(component.GetLevel());
			}
			this.m_nview.Destroy();
		}
	}

	// Token: 0x040000E1 RID: 225
	public float m_growTime = 60f;

	// Token: 0x040000E2 RID: 226
	public GameObject m_grownPrefab;

	// Token: 0x040000E3 RID: 227
	private BaseAI m_baseAI;

	// Token: 0x040000E4 RID: 228
	private ZNetView m_nview;
}
