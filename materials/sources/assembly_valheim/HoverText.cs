using System;
using UnityEngine;

// Token: 0x020000D5 RID: 213
public class HoverText : MonoBehaviour, Hoverable
{
	// Token: 0x06000DC8 RID: 3528 RVA: 0x0006291F File Offset: 0x00060B1F
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_text);
	}

	// Token: 0x06000DC9 RID: 3529 RVA: 0x0006291F File Offset: 0x00060B1F
	public string GetHoverName()
	{
		return Localization.instance.Localize(this.m_text);
	}

	// Token: 0x04000C77 RID: 3191
	public string m_text = "";
}
