using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x02000061 RID: 97
public class TextInput : MonoBehaviour
{
	// Token: 0x0600063D RID: 1597 RVA: 0x00035299 File Offset: 0x00033499
	private void Awake()
	{
		TextInput.m_instance = this;
		this.m_panel.SetActive(false);
	}

	// Token: 0x17000011 RID: 17
	// (get) Token: 0x0600063E RID: 1598 RVA: 0x000352AD File Offset: 0x000334AD
	public static TextInput instance
	{
		get
		{
			return TextInput.m_instance;
		}
	}

	// Token: 0x0600063F RID: 1599 RVA: 0x000352B4 File Offset: 0x000334B4
	private void OnDestroy()
	{
		TextInput.m_instance = null;
	}

	// Token: 0x06000640 RID: 1600 RVA: 0x000352BC File Offset: 0x000334BC
	public static bool IsVisible()
	{
		return TextInput.m_instance && TextInput.m_instance.m_visibleFrame;
	}

	// Token: 0x06000641 RID: 1601 RVA: 0x000352D8 File Offset: 0x000334D8
	private void Update()
	{
		this.m_visibleFrame = TextInput.m_instance.m_panel.gameObject.activeSelf;
		if (!this.m_visibleFrame)
		{
			return;
		}
		if (global::Console.IsVisible() || Chat.instance.HasFocus())
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.Hide();
			return;
		}
		if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
		{
			string text = this.m_textField.text;
			this.OnEnter(text);
			this.Hide();
		}
		if (!this.m_textField.isFocused)
		{
			EventSystem.current.SetSelectedGameObject(this.m_textField.gameObject);
		}
	}

	// Token: 0x06000642 RID: 1602 RVA: 0x0003537C File Offset: 0x0003357C
	private void OnEnter(string text)
	{
		if (this.m_queuedSign != null)
		{
			this.m_queuedSign.SetText(text);
			this.m_queuedSign = null;
		}
	}

	// Token: 0x06000643 RID: 1603 RVA: 0x00035399 File Offset: 0x00033599
	public void RequestText(TextReceiver sign, string topic, int charLimit)
	{
		this.m_queuedSign = sign;
		this.Show(topic, sign.GetText(), charLimit);
	}

	// Token: 0x06000644 RID: 1604 RVA: 0x000353B0 File Offset: 0x000335B0
	private void Show(string topic, string text, int charLimit)
	{
		this.m_panel.SetActive(true);
		this.m_textField.text = text;
		this.m_topic.text = Localization.instance.Localize(topic);
		this.m_textField.ActivateInputField();
		this.m_textField.characterLimit = charLimit;
	}

	// Token: 0x06000645 RID: 1605 RVA: 0x00035402 File Offset: 0x00033602
	public void Hide()
	{
		this.m_panel.SetActive(false);
	}

	// Token: 0x040006F8 RID: 1784
	private static TextInput m_instance;

	// Token: 0x040006F9 RID: 1785
	public GameObject m_panel;

	// Token: 0x040006FA RID: 1786
	public InputField m_textField;

	// Token: 0x040006FB RID: 1787
	public Text m_topic;

	// Token: 0x040006FC RID: 1788
	private TextReceiver m_queuedSign;

	// Token: 0x040006FD RID: 1789
	private bool m_visibleFrame;
}
