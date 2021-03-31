using System;
using UnityEngine;

// Token: 0x02000013 RID: 19
public class RandomIdle : StateMachineBehaviour
{
	// Token: 0x06000267 RID: 615 RVA: 0x00013954 File Offset: 0x00011B54
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		int randomIdle = this.GetRandomIdle(animator);
		animator.SetFloat(this.m_valueName, (float)randomIdle);
		this.m_last = stateInfo.normalizedTime % 1f;
	}

	// Token: 0x06000268 RID: 616 RVA: 0x0001398C File Offset: 0x00011B8C
	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		float num = stateInfo.normalizedTime % 1f;
		if (num < this.m_last)
		{
			int randomIdle = this.GetRandomIdle(animator);
			animator.SetFloat(this.m_valueName, (float)randomIdle);
		}
		this.m_last = num;
	}

	// Token: 0x06000269 RID: 617 RVA: 0x000139D0 File Offset: 0x00011BD0
	private int GetRandomIdle(Animator animator)
	{
		if (!this.m_haveSetup)
		{
			this.m_haveSetup = true;
			this.m_baseAI = animator.GetComponentInParent<BaseAI>();
		}
		if (this.m_baseAI && this.m_alertedIdle >= 0 && this.m_baseAI.IsAlerted())
		{
			return this.m_alertedIdle;
		}
		return UnityEngine.Random.Range(0, this.m_animations);
	}

	// Token: 0x040001DA RID: 474
	public int m_animations = 4;

	// Token: 0x040001DB RID: 475
	public string m_valueName = "";

	// Token: 0x040001DC RID: 476
	public int m_alertedIdle = -1;

	// Token: 0x040001DD RID: 477
	private float m_last;

	// Token: 0x040001DE RID: 478
	private bool m_haveSetup;

	// Token: 0x040001DF RID: 479
	private BaseAI m_baseAI;
}
