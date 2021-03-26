using System;
using UnityEngine;

// Token: 0x020000D0 RID: 208
public class GuidePoint : MonoBehaviour
{
	// Token: 0x06000D68 RID: 3432 RVA: 0x0005F78C File Offset: 0x0005D98C
	private void Start()
	{
		if (!Raven.IsInstantiated())
		{
			UnityEngine.Object.Instantiate<GameObject>(this.m_ravenPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
		}
		this.m_text.m_static = true;
		this.m_text.m_guidePoint = this;
		Raven.RegisterStaticText(this.m_text);
	}

	// Token: 0x06000D69 RID: 3433 RVA: 0x0005F7E8 File Offset: 0x0005D9E8
	private void OnDestroy()
	{
		Raven.UnregisterStaticText(this.m_text);
	}

	// Token: 0x06000D6A RID: 3434 RVA: 0x000027E0 File Offset: 0x000009E0
	private void OnDrawGizmos()
	{
	}

	// Token: 0x04000C44 RID: 3140
	public Raven.RavenText m_text = new Raven.RavenText();

	// Token: 0x04000C45 RID: 3141
	public GameObject m_ravenPrefab;
}
