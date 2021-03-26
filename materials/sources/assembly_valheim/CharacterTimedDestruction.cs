using System;
using UnityEngine;

// Token: 0x02000005 RID: 5
public class CharacterTimedDestruction : MonoBehaviour
{
	// Token: 0x060000E3 RID: 227 RVA: 0x00006E41 File Offset: 0x00005041
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_triggerOnAwake)
		{
			this.Trigger();
		}
	}

	// Token: 0x060000E4 RID: 228 RVA: 0x00006E5D File Offset: 0x0000505D
	public void Trigger()
	{
		base.InvokeRepeating("DestroyNow", UnityEngine.Random.Range(this.m_timeoutMin, this.m_timeoutMax), 1f);
	}

	// Token: 0x060000E5 RID: 229 RVA: 0x00006E80 File Offset: 0x00005080
	public void Trigger(float timeout)
	{
		base.InvokeRepeating("DestroyNow", timeout, 1f);
	}

	// Token: 0x060000E6 RID: 230 RVA: 0x00006E94 File Offset: 0x00005094
	private void DestroyNow()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		Character component = base.GetComponent<Character>();
		HitData hitData = new HitData();
		hitData.m_damage.m_damage = 99999f;
		hitData.m_point = base.transform.position;
		component.ApplyDamage(hitData, false, true, HitData.DamageModifier.Normal);
	}

	// Token: 0x040000B6 RID: 182
	public float m_timeoutMin = 1f;

	// Token: 0x040000B7 RID: 183
	public float m_timeoutMax = 1f;

	// Token: 0x040000B8 RID: 184
	public bool m_triggerOnAwake;

	// Token: 0x040000B9 RID: 185
	private ZNetView m_nview;

	// Token: 0x040000BA RID: 186
	private Character m_character;
}
