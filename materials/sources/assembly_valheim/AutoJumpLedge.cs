using System;
using UnityEngine;

// Token: 0x020000B7 RID: 183
public class AutoJumpLedge : MonoBehaviour
{
	// Token: 0x06000C56 RID: 3158 RVA: 0x00058BE8 File Offset: 0x00056DE8
	private void OnTriggerStay(Collider collider)
	{
		Character component = collider.GetComponent<Character>();
		if (component)
		{
			component.OnAutoJump(base.transform.forward, this.m_upVel, this.m_forwardVel);
		}
	}

	// Token: 0x04000B55 RID: 2901
	public bool m_forwardOnly = true;

	// Token: 0x04000B56 RID: 2902
	public float m_upVel = 1f;

	// Token: 0x04000B57 RID: 2903
	public float m_forwardVel = 1f;
}
