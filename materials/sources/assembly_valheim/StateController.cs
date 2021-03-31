using System;
using UnityEngine;

// Token: 0x02000048 RID: 72
public class StateController : StateMachineBehaviour
{
	// Token: 0x06000493 RID: 1171 RVA: 0x000249A8 File Offset: 0x00022BA8
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

	// Token: 0x06000494 RID: 1172 RVA: 0x00024A50 File Offset: 0x00022C50
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

	// Token: 0x040004A9 RID: 1193
	public string m_effectJoint = "";

	// Token: 0x040004AA RID: 1194
	public EffectList m_enterEffect = new EffectList();

	// Token: 0x040004AB RID: 1195
	public bool m_enterDisableChildren;

	// Token: 0x040004AC RID: 1196
	public bool m_enterEnableChildren;

	// Token: 0x040004AD RID: 1197
	public GameObject[] m_enterDisable = new GameObject[0];

	// Token: 0x040004AE RID: 1198
	public GameObject[] m_enterEnable = new GameObject[0];

	// Token: 0x040004AF RID: 1199
	private Transform m_effectJoinT;
}
