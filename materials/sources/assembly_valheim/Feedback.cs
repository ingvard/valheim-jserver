using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000051 RID: 81
public class Feedback : MonoBehaviour
{
	// Token: 0x060004E8 RID: 1256 RVA: 0x00028CBB File Offset: 0x00026EBB
	private void Awake()
	{
		Feedback.m_instance = this;
	}

	// Token: 0x060004E9 RID: 1257 RVA: 0x00028CC3 File Offset: 0x00026EC3
	private void OnDestroy()
	{
		if (Feedback.m_instance == this)
		{
			Feedback.m_instance = null;
		}
	}

	// Token: 0x060004EA RID: 1258 RVA: 0x00028CD8 File Offset: 0x00026ED8
	public static bool IsVisible()
	{
		return Feedback.m_instance != null;
	}

	// Token: 0x060004EB RID: 1259 RVA: 0x00028CE5 File Offset: 0x00026EE5
	private void LateUpdate()
	{
		this.m_sendButton.interactable = this.IsValid();
		if (Feedback.IsVisible() && (Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("JoyMenu")))
		{
			this.OnBack();
		}
	}

	// Token: 0x060004EC RID: 1260 RVA: 0x00028D1A File Offset: 0x00026F1A
	private bool IsValid()
	{
		return this.m_subject.text.Length != 0 && this.m_text.text.Length != 0;
	}

	// Token: 0x060004ED RID: 1261 RVA: 0x00028D45 File Offset: 0x00026F45
	public void OnBack()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	// Token: 0x060004EE RID: 1262 RVA: 0x00028D54 File Offset: 0x00026F54
	public void OnSend()
	{
		if (!this.IsValid())
		{
			return;
		}
		string category = this.GetCategory();
		Gogan.LogEvent("Feedback_" + category, this.m_subject.text, this.m_text.text, 0L);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	// Token: 0x060004EF RID: 1263 RVA: 0x00028DA4 File Offset: 0x00026FA4
	private string GetCategory()
	{
		if (this.m_catBug.isOn)
		{
			return "Bug";
		}
		if (this.m_catFeedback.isOn)
		{
			return "Feedback";
		}
		if (this.m_catIdea.isOn)
		{
			return "Idea";
		}
		return "";
	}

	// Token: 0x0400052F RID: 1327
	private static Feedback m_instance;

	// Token: 0x04000530 RID: 1328
	public Text m_subject;

	// Token: 0x04000531 RID: 1329
	public Text m_text;

	// Token: 0x04000532 RID: 1330
	public Button m_sendButton;

	// Token: 0x04000533 RID: 1331
	public Toggle m_catBug;

	// Token: 0x04000534 RID: 1332
	public Toggle m_catFeedback;

	// Token: 0x04000535 RID: 1333
	public Toggle m_catIdea;
}
