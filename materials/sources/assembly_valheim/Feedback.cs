using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000051 RID: 81
public class Feedback : MonoBehaviour
{
	// Token: 0x060004E9 RID: 1257 RVA: 0x00028D6F File Offset: 0x00026F6F
	private void Awake()
	{
		Feedback.m_instance = this;
	}

	// Token: 0x060004EA RID: 1258 RVA: 0x00028D77 File Offset: 0x00026F77
	private void OnDestroy()
	{
		if (Feedback.m_instance == this)
		{
			Feedback.m_instance = null;
		}
	}

	// Token: 0x060004EB RID: 1259 RVA: 0x00028D8C File Offset: 0x00026F8C
	public static bool IsVisible()
	{
		return Feedback.m_instance != null;
	}

	// Token: 0x060004EC RID: 1260 RVA: 0x00028D99 File Offset: 0x00026F99
	private void LateUpdate()
	{
		this.m_sendButton.interactable = this.IsValid();
		if (Feedback.IsVisible() && (Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("JoyMenu")))
		{
			this.OnBack();
		}
	}

	// Token: 0x060004ED RID: 1261 RVA: 0x00028DCE File Offset: 0x00026FCE
	private bool IsValid()
	{
		return this.m_subject.text.Length != 0 && this.m_text.text.Length != 0;
	}

	// Token: 0x060004EE RID: 1262 RVA: 0x00028DF9 File Offset: 0x00026FF9
	public void OnBack()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	// Token: 0x060004EF RID: 1263 RVA: 0x00028E08 File Offset: 0x00027008
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

	// Token: 0x060004F0 RID: 1264 RVA: 0x00028E58 File Offset: 0x00027058
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

	// Token: 0x04000533 RID: 1331
	private static Feedback m_instance;

	// Token: 0x04000534 RID: 1332
	public Text m_subject;

	// Token: 0x04000535 RID: 1333
	public Text m_text;

	// Token: 0x04000536 RID: 1334
	public Button m_sendButton;

	// Token: 0x04000537 RID: 1335
	public Toggle m_catBug;

	// Token: 0x04000538 RID: 1336
	public Toggle m_catFeedback;

	// Token: 0x04000539 RID: 1337
	public Toggle m_catIdea;
}
