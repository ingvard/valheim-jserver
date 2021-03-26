using System;
using UnityEngine;

// Token: 0x02000048 RID: 72
public class StateController : StateMachineBehaviour
{
	// Token: 0x06000492 RID: 1170 RVA: 0x000248F4 File Offset: 0x00022AF4
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (this.m_enterEffect.HasEffects())
		{
			this.m_enterEffect.Create(this.GetEffectPos(animator), animator.transform.rotation, null, 1f);
		}
		if (this.m_enterDisableChildren)
		{
			for (int i = 0; i < animator.transform.childCount; i++)
			{
				animator.transform.GetChild(i).gameObject.SetActive(false);
			}
		}
		if (this.m_enterEnableChildren)
		{
			for (int j = 0; j < animator.transform.childCount; j++)
			{
				animator.transform.GetChild(j).gameObject.SetActive(true);
			}
		}
	}

	// Token: 0x06000493 RID: 1171 RVA: 0x0002499C File Offset: 0x00022B9C
	private Vector3 GetEffectPos(Animator animator)
	{
		if (this.m_effectJoint.Length == 0)
		{
			return animator.transform.position;
		}
		if (this.m_effectJoinT == null)
		{
			this.m_effectJoinT = Utils.FindChild(animator.transform, this.m_effectJoint);
		}
		return this.m_effectJoinT.position;
	}

	// Token: 0x040004A5 RID: 1189
	public string m_effectJoint = "";

	// Token: 0x040004A6 RID: 1190
	public EffectList m_enterEffect = new EffectList();

	// Token: 0x040004A7 RID: 1191
	public bool m_enterDisableChildren;

	// Token: 0x040004A8 RID: 1192
	public bool m_enterEnableChildren;

	// Token: 0x040004A9 RID: 1193
	public GameObject[] m_enterDisable = new GameObject[0];

	// Token: 0x040004AA RID: 1194
	public GameObject[] m_enterEnable = new GameObject[0];

	// Token: 0x040004AB RID: 1195
	private Transform m_effectJoinT;
}
