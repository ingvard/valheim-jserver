using System;
using UnityEngine;

// Token: 0x020000D5 RID: 213
public class HoverText : MonoBehaviour, Hoverable
{
	// Token: 0x06000DC9 RID: 3529 RVA: 0x00062AA7 File Offset: 0x00060CA7
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_text);
	}

	// Token: 0x06000DCA RID: 3530 RVA: 0x00062AA7 File Offset: 0x00060CA7
	public string GetHoverName()
	{
		return Localization.instance.Localize(this.m_text);
	}

	// Token: 0x04000C7D RID: 3197
	public string m_text = "";
}
