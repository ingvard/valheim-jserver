using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000DA RID: 218
public class Ledge : MonoBehaviour
{
	// Token: 0x06000DEE RID: 3566 RVA: 0x00063580 File Offset: 0x00061780
	private void Awake()
	{
		if (base.GetComponent<ZNetView>().GetZDO() == null)
		{
			return;
		}
		this.m_collider.enabled = true;
		TriggerTracker above = this.m_above;
		above.m_changed = (Action)Delegate.Combine(above.m_changed, new Action(this.Changed));
	}

	// Token: 0x06000DEF RID: 3567 RVA: 0x000635D0 File Offset: 0x000617D0
	private void Changed()
	{
		List<Collider> colliders = this.m_above.GetColliders();
		if (colliders.Count == 0)
		{
			this.m_collider.enabled = true;
			return;
		}
		bool enabled = false;
		using (List<Collider>.Enumerator enumerator = colliders.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.transform.position.y > base.transform.position.y)
				{
					enabled = true;
					break;
				}
			}
		}
		this.m_collider.enabled = enabled;
	}

	// Token: 0x04000C90 RID: 3216
	public Collider m_collider;

	// Token: 0x04000C91 RID: 3217
	public TriggerTracker m_above;
}
