using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000067 RID: 103
public class WaterMark : MonoBehaviour
{
	// Token: 0x0600066D RID: 1645 RVA: 0x00036140 File Offset: 0x00034340
	private void Awake()
	{
		this.m_text.text = "Version: " + global::Version.GetVersionString();
	}

	// Token: 0x04000730 RID: 1840
	public Text m_text;
}
