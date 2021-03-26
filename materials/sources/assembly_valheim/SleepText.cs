using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000F6 RID: 246
public class SleepText : MonoBehaviour
{
	// Token: 0x06000F1B RID: 3867 RVA: 0x0006BD58 File Offset: 0x00069F58
	private void OnEnable()
	{
		this.m_textField.canvasRenderer.SetAlpha(0f);
		this.m_textField.CrossFadeAlpha(1f, 1f, true);
		this.m_dreamField.enabled = false;
		base.Invoke("HideZZZ", 2f);
		base.Invoke("ShowDreamText", 4f);
	}

	// Token: 0x06000F1C RID: 3868 RVA: 0x0006BDBC File Offset: 0x00069FBC
	private void HideZZZ()
	{
		this.m_textField.CrossFadeAlpha(0f, 2f, true);
	}

	// Token: 0x06000F1D RID: 3869 RVA: 0x0006BDD4 File Offset: 0x00069FD4
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

	// Token: 0x06000F1E RID: 3870 RVA: 0x0006BE53 File Offset: 0x0006A053
	private void HideDreamText()
	{
		this.m_dreamField.CrossFadeAlpha(0f, 1.5f, true);
	}

	// Token: 0x04000E09 RID: 3593
	public Text m_textField;

	// Token: 0x04000E0A RID: 3594
	public Text m_dreamField;

	// Token: 0x04000E0B RID: 3595
	public DreamTexts m_dreamTexts;
}
