using System;
using UnityEngine;

// Token: 0x020000A1 RID: 161
public class InstantiatePrefab : MonoBehaviour
{
	// Token: 0x06000B05 RID: 2821 RVA: 0x0004F8A0 File Offset: 0x0004DAA0
	private void Awake()
	{
		if (this.m_attach)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.m_prefab, base.transform).transform.SetAsFirstSibling();
			return;
		}
		UnityEngine.Object.Instantiate<GameObject>(this.m_prefab);
	}

	// Token: 0x04000A73 RID: 2675
	public GameObject m_prefab;

	// Token: 0x04000A74 RID: 2676
	public bool m_attach = true;

	// Token: 0x04000A75 RID: 2677
	public bool m_moveToTop;
}
