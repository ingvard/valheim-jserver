using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000DA RID: 218
public class Ledge : MonoBehaviour
{
	// Token: 0x06000DEF RID: 3567 RVA: 0x00063708 File Offset: 0x00061908
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

	// Token: 0x06000DF0 RID: 3568 RVA: 0x00063758 File Offset: 0x00061958
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

	// Token: 0x04000C96 RID: 3222
	public Collider m_collider;

	// Token: 0x04000C97 RID: 3223
	public TriggerTracker m_above;
}
