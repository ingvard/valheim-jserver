using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200004B RID: 75
public class ChangeLog : MonoBehaviour
{
	// Token: 0x060004A5 RID: 1189 RVA: 0x00025454 File Offset: 0x00023654
	private void Start()
	{
		string text = this.m_changeLog.text;
		this.m_textField.text = text;
	}

	// Token: 0x040004E2 RID: 1250
	public Text m_textField;

	// Token: 0x040004E3 RID: 1251
	public TextAsset m_changeLog;
}
