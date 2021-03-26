using System;
using UnityEngine;

// Token: 0x020000BB RID: 187
public class Billboard : MonoBehaviour
{
	// Token: 0x06000C7D RID: 3197 RVA: 0x000594F9 File Offset: 0x000576F9
	private void Awake()
	{
		this.m_normal = base.transform.up;
	}

	// Token: 0x06000C7E RID: 3198 RVA: 0x0005950C File Offset: 0x0005770C
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

	// Token: 0x04000B62 RID: 2914
	public bool m_vertical = true;

	// Token: 0x04000B63 RID: 2915
	public bool m_invert;

	// Token: 0x04000B64 RID: 2916
	private Vector3 m_normal;
}
