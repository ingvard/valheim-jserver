using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000F6 RID: 246
public class SleepText : MonoBehaviour
{
	// Token: 0x06000F1C RID: 3868 RVA: 0x0006BEE0 File Offset: 0x0006A0E0
	private void OnEnable()
	{
		this.m_textField.canvasRenderer.SetAlpha(0f);
		this.m_textField.CrossFadeAlpha(1f, 1f, true);
		this.m_dreamField.enabled = false;
		base.Invoke("HideZZZ", 2f);
		base.Invoke("ShowDreamText", 4f);
	}

	// Token: 0x06000F1D RID: 3869 RVA: 0x0006BF44 File Offset: 0x0006A144
	private void HideZZZ()
	{
		this.m_textField.CrossFadeAlpha(0f, 2f, true);
	}

	// Token: 0x06000F1E RID: 3870 RVA: 0x0006BF5C File Offset: 0x0006A15C
	private void ShowDreamText()
	{
		DreamTexts.DreamText randomDreamText = this.m_dreamTexts.GetRandomDreamText();
		if (randomDreamText == null)
		{
			return;
		}
		this.m_dreamField.enabled = true;
		this.m_dreamField.canvasRenderer.SetAlpha(0f);
		this.m_dreamField.CrossFadeAlpha(1f, 1.5f, true);
		this.m_dreamField.text = Localization.instance.Localize(randomDreamText.m_text);
		base.Invoke("HideDreamText", 6.5f);
	}

	// Token: 0x06000F1F RID: 3871 RVA: 0x0006BFDB File Offset: 0x0006A1DB
	private void HideDreamText()
	{
		this.m_dreamField.CrossFadeAlpha(0f, 1.5f, true);
	}

	// Token: 0x04000E0F RID: 3599
	public Text m_textField;

	// Token: 0x04000E10 RID: 3600
	public Text m_dreamField;

	// Token: 0x04000E11 RID: 3601
	public DreamTexts m_dreamTexts;
}
