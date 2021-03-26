using System;
using UnityEngine;

// Token: 0x020000B7 RID: 183
public class AutoJumpLedge : MonoBehaviour
{
	// Token: 0x06000C55 RID: 3157 RVA: 0x00058A60 File Offset: 0x00056C60
	private void OnTriggerStay(Collider collider)
	{
		Character component = collider.GetComponent<Character>();
		if (component)
		{
			component.OnAutoJump(base.transform.forward, this.m_upVel, this.m_forwardVel);
		}
	}

	// Token: 0x04000B4F RID: 2895
	public bool m_forwardOnly = true;

	// Token: 0x04000B50 RID: 2896
	public float m_upVel = 1f;

	// Token: 0x04000B51 RID: 2897
	public float m_forwardVel = 1f;
}
