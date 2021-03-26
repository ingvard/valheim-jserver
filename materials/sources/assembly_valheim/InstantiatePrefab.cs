using System;
using UnityEngine;

// Token: 0x020000A1 RID: 161
public class InstantiatePrefab : MonoBehaviour
{
	// Token: 0x06000B04 RID: 2820 RVA: 0x0004F718 File Offset: 0x0004D918
	private void Awake()
	{
		if (this.m_attach)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.m_prefab, base.transform).transform.SetAsFirstSibling();
			return;
		}
		UnityEngine.Object.Instantiate<GameObject>(this.m_prefab);
	}

	// Token: 0x04000A6D RID: 2669
	public GameObject m_prefab;

	// Token: 0x04000A6E RID: 2670
	public bool m_attach = true;

	// Token: 0x04000A6F RID: 2671
	public bool m_moveToTop;
}
