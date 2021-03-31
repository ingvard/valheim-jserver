using System;
using UnityEngine;

// Token: 0x020000BB RID: 187
public class Billboard : MonoBehaviour
{
	// Token: 0x06000C7E RID: 3198 RVA: 0x00059681 File Offset: 0x00057881
	private void Awake()
	{
		this.m_normal = base.transform.up;
	}

	// Token: 0x06000C7F RID: 3199 RVA: 0x00059694 File Offset: 0x00057894
	private void LateUpdate()
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		Vector3 vector = mainCamera.transform.position;
		if (this.m_invert)
		{
			vector = base.transform.position - (vector - base.transform.position);
		}
		if (this.m_vertical)
		{
			vector.y = base.transform.position.y;
			base.transform.LookAt(vector, this.m_normal);
			return;
		}
		base.transform.LookAt(vector);
	}

	// Token: 0x04000B68 RID: 2920
	public bool m_vertical = true;

	// Token: 0x04000B69 RID: 2921
	public bool m_invert;

	// Token: 0x04000B6A RID: 2922
	private Vector3 m_normal;
}
