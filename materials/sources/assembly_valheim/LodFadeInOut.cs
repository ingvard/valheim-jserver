using System;
using UnityEngine;

// Token: 0x020000DE RID: 222
public class LodFadeInOut : MonoBehaviour
{
	// Token: 0x06000E06 RID: 3590 RVA: 0x00063F68 File Offset: 0x00062168
	private void Awake()
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		if (Vector3.Distance(mainCamera.transform.position, base.transform.position) > 20f)
		{
			this.m_lodGroup = base.GetComponent<LODGroup>();
			if (this.m_lodGroup)
			{
				this.m_originalLocalRef = this.m_lodGroup.localReferencePoint;
				this.m_lodGroup.localReferencePoint = new Vector3(999999f, 999999f, 999999f);
				base.Invoke("FadeIn", UnityEngine.Random.Range(0.1f, 0.3f));
			}
		}
	}

	// Token: 0x06000E07 RID: 3591 RVA: 0x0006400A File Offset: 0x0006220A
	private void FadeIn()
	{
		this.m_lodGroup.localReferencePoint = this.m_originalLocalRef;
	}

	// Token: 0x04000CB6 RID: 3254
	private Vector3 m_originalLocalRef;

	// Token: 0x04000CB7 RID: 3255
	private LODGroup m_lodGroup;

	// Token: 0x04000CB8 RID: 3256
	private const float m_minTriggerDistance = 20f;
}
