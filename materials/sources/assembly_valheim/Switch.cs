using System;
using UnityEngine;

// Token: 0x020000FC RID: 252
public class Switch : MonoBehaviour, Interactable, Hoverable
{
	// Token: 0x06000F64 RID: 3940 RVA: 0x0006D7AC File Offset: 0x0006B9AC
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

	// Token: 0x06000F65 RID: 3941 RVA: 0x0006D804 File Offset: 0x0006BA04
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return this.m_onUse != null && this.m_onUse(this, user, item);
	}

	// Token: 0x06000F66 RID: 3942 RVA: 0x0006D81E File Offset: 0x0006BA1E
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_hoverText);
	}

	// Token: 0x06000F67 RID: 3943 RVA: 0x0006D830 File Offset: 0x0006BA30
	public string GetHoverName()
	{
		return Localization.instance.Localize(this.m_name);
	}

	// Token: 0x04000E3A RID: 3642
	public Switch.Callback m_onUse;

	// Token: 0x04000E3B RID: 3643
	public string m_hoverText = "";

	// Token: 0x04000E3C RID: 3644
	public string m_name = "";

	// Token: 0x04000E3D RID: 3645
	public float m_holdRepeatInterval = -1f;

	// Token: 0x04000E3E RID: 3646
	private float m_lastUseTime;

	// Token: 0x020001AC RID: 428
	// (Invoke) Token: 0x060011C4 RID: 4548
	public delegate bool Callback(Switch caller, Humanoid user, ItemDrop.ItemData item);
}
