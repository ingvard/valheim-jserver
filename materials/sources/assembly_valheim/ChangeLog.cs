using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200004B RID: 75
public class ChangeLog : MonoBehaviour
{
	// Token: 0x060004A4 RID: 1188 RVA: 0x000253A0 File Offset: 0x000235A0
	private void Start()
	{
		string text = this.m_changeLog.text;
		this.m_textField.text = text;
	}

	// Token: 0x040004DE RID: 1246
	public Text m_textField;

	// Token: 0x040004DF RID: 1247
	public TextAsset m_changeLog;
}
