using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000067 RID: 103
public class WaterMark : MonoBehaviour
{
	// Token: 0x0600066C RID: 1644 RVA: 0x0003608C File Offset: 0x0003428C
	private void Awake()
	{
		this.m_text.text = "Version: " + global::Version.GetVersionString();
	}

	// Token: 0x0400072C RID: 1836
	public Text m_text;
}
