using System;
using UnityEngine;

// Token: 0x020000FC RID: 252
public class Switch : MonoBehaviour, Interactable, Hoverable
{
	// Token: 0x06000F63 RID: 3939 RVA: 0x0006D624 File Offset: 0x0006B824
	public bool Interact(Humanoid character, bool hold)
	{
		if (hold)
		{
			if (this.m_holdRepeatInterval <= 0f)
			{
				return false;
			}
			if (Time.time - this.m_lastUseTime < this.m_holdRepeatInterval)
			{
				return false;
			}
		}
		this.m_lastUseTime = Time.time;
		return this.m_onUse != null && this.m_onUse(this, character, null);
	}

	// Token: 0x06000F64 RID: 3940 RVA: 0x0006D67C File Offset: 0x0006B87C
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return this.m_onUse != null && this.m_onUse(this, user, item);
	}

	// Token: 0x06000F65 RID: 3941 RVA: 0x0006D696 File Offset: 0x0006B896
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_hoverText);
	}

	// Token: 0x06000F66 RID: 3942 RVA: 0x0006D6A8 File Offset: 0x0006B8A8
	public string GetHoverName()
	{
		return Localization.instance.Localize(this.m_name);
	}

	// Token: 0x04000E34 RID: 3636
	public Switch.Callback m_onUse;

	// Token: 0x04000E35 RID: 3637
	public string m_hoverText = "";

	// Token: 0x04000E36 RID: 3638
	public string m_name = "";

	// Token: 0x04000E37 RID: 3639
	public float m_holdRepeatInterval = -1f;

	// Token: 0x04000E38 RID: 3640
	private float m_lastUseTime;

	// Token: 0x020001AC RID: 428
	// (Invoke) Token: 0x060011C3 RID: 4547
	public delegate bool Callback(Switch caller, Humanoid user, ItemDrop.ItemData item);
}
