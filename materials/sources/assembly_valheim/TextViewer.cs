using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000062 RID: 98
public class TextViewer : MonoBehaviour
{
	// Token: 0x06000647 RID: 1607 RVA: 0x00035410 File Offset: 0x00033610
	private void Awake()
	{
		TextViewer.m_instance = this;
		this.m_root.SetActive(true);
		this.m_introRoot.SetActive(true);
		this.m_ravenRoot.SetActive(true);
		this.m_animator = this.m_root.GetComponent<Animator>();
		this.m_animatorIntro = this.m_introRoot.GetComponent<Animator>();
		this.m_animatorRaven = this.m_ravenRoot.GetComponent<Animator>();
	}

	// Token: 0x06000648 RID: 1608 RVA: 0x0003547A File Offset: 0x0003367A
	private void OnDestroy()
	{
		TextViewer.m_instance = null;
	}

	// Token: 0x17000012 RID: 18
	// (get) Token: 0x06000649 RID: 1609 RVA: 0x00035482 File Offset: 0x00033682
	public static TextViewer instance
	{
		get
		{
			return TextViewer.m_instance;
		}
	}

	// Token: 0x0600064A RID: 1610 RVA: 0x0003548C File Offset: 0x0003368C
	private void LateUpdate()
	{
		if (!this.IsVisible())
		{
			return;
		}
		this.m_showTime += Time.deltaTime;
		if (this.m_showTime > 0.2f)
		{
			if (this.m_autoHide && Player.m_localPlayer && Vector3.Distance(Player.m_localPlayer.transform.position, this.m_openPlayerPos) > 3f)
			{
				this.Hide();
			}
			if (ZInput.GetButtonDown("Use") || ZInput.GetButtonDown("JoyUse") || Input.GetKeyDown(KeyCode.Escape))
			{
				this.Hide();
			}
		}
	}

	// Token: 0x0600064B RID: 1611 RVA: 0x00035524 File Offset: 0x00033724
	public void ShowText(TextViewer.Style style, string topic, string text, bool autoHide)
	{
		if (Player.m_localPlayer == null)
		{
			return;
		}
		topic = Localization.instance.Localize(topic);
		text = Localization.instance.Localize(text);
		if (style == TextViewer.Style.Rune)
		{
			this.m_topic.text = topic;
			this.m_text.text = text;
			this.m_runeText.text = text;
			this.m_animator.SetBool(TextViewer.m_visibleID, true);
		}
		else if (style == TextViewer.Style.Intro)
		{
			this.m_introTopic.text = topic;
			this.m_introText.text = text;
			this.m_animatorIntro.SetTrigger("play");
			ZLog.Log("Show intro " + Time.frameCount);
		}
		else if (style == TextViewer.Style.Raven)
		{
			this.m_ravenTopic.text = topic;
			this.m_ravenText.text = text;
			this.m_animatorRaven.SetBool(TextViewer.m_visibleID, true);
		}
		this.m_autoHide = autoHide;
		this.m_openPlayerPos = Player.m_localPlayer.transform.position;
		this.m_showTime = 0f;
		ZLog.Log("Show text " + topic + ":" + text);
	}

	// Token: 0x0600064C RID: 1612 RVA: 0x00035645 File Offset: 0x00033845
	public void Hide()
	{
		this.m_autoHide = false;
		this.m_animator.SetBool(TextViewer.m_visibleID, false);
		this.m_animatorRaven.SetBool(TextViewer.m_visibleID, false);
	}

	// Token: 0x0600064D RID: 1613 RVA: 0x00035670 File Offset: 0x00033870
	public bool IsVisible()
	{
		return TextViewer.m_instance.m_animatorIntro.GetCurrentAnimatorStateInfo(0).tagHash == TextViewer.m_animatorTagVisible || this.m_animator.GetBool(TextViewer.m_visibleID) || this.m_animatorIntro.GetBool(TextViewer.m_visibleID) || this.m_animatorRaven.GetBool(TextViewer.m_visibleID);
	}

	// Token: 0x0600064E RID: 1614 RVA: 0x000356D4 File Offset: 0x000338D4
	public static bool IsShowingIntro()
	{
		return TextViewer.m_instance != null && TextViewer.m_instance.m_animatorIntro.GetCurrentAnimatorStateInfo(0).tagHash == TextViewer.m_animatorTagVisible;
	}

	// Token: 0x040006FE RID: 1790
	private static TextViewer m_instance;

	// Token: 0x040006FF RID: 1791
	private Animator m_animator;

	// Token: 0x04000700 RID: 1792
	private Animator m_animatorIntro;

	// Token: 0x04000701 RID: 1793
	private Animator m_animatorRaven;

	// Token: 0x04000702 RID: 1794
	[Header("Rune")]
	public GameObject m_root;

	// Token: 0x04000703 RID: 1795
	public Text m_topic;

	// Token: 0x04000704 RID: 1796
	public Text m_text;

	// Token: 0x04000705 RID: 1797
	public Text m_runeText;

	// Token: 0x04000706 RID: 1798
	public GameObject m_closeText;

	// Token: 0x04000707 RID: 1799
	[Header("Intro")]
	public GameObject m_introRoot;

	// Token: 0x04000708 RID: 1800
	public Text m_introTopic;

	// Token: 0x04000709 RID: 1801
	public Text m_introText;

	// Token: 0x0400070A RID: 1802
	[Header("Raven")]
	public GameObject m_ravenRoot;

	// Token: 0x0400070B RID: 1803
	public Text m_ravenTopic;

	// Token: 0x0400070C RID: 1804
	public Text m_ravenText;

	// Token: 0x0400070D RID: 1805
	private static int m_visibleID = Animator.StringToHash("visible");

	// Token: 0x0400070E RID: 1806
	private static int m_animatorTagVisible = Animator.StringToHash("visible");

	// Token: 0x0400070F RID: 1807
	private float m_showTime;

	// Token: 0x04000710 RID: 1808
	private bool m_autoHide;

	// Token: 0x04000711 RID: 1809
	private Vector3 m_openPlayerPos = Vector3.zero;

	// Token: 0x0200015C RID: 348
	public enum Style
	{
		// Token: 0x04001131 RID: 4401
		Rune,
		// Token: 0x04001132 RID: 4402
		Intro,
		// Token: 0x04001133 RID: 4403
		Raven
	}
}
