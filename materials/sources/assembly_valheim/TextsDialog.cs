using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000063 RID: 99
public class TextsDialog : MonoBehaviour
{
	// Token: 0x06000651 RID: 1617 RVA: 0x00035744 File Offset: 0x00033944
	private void Awake()
	{
		this.m_baseListSize = this.m_listRoot.rect.height;
	}

	// Token: 0x06000652 RID: 1618 RVA: 0x0003576C File Offset: 0x0003396C
	public void Setup(Player player)
	{
		base.gameObject.SetActive(true);
		this.FillTextList();
		if (this.m_texts.Count > 0)
		{
			this.ShowText(this.m_texts[0]);
			return;
		}
		this.m_textAreaTopic.text = "";
		this.m_textArea.text = "";
	}

	// Token: 0x06000653 RID: 1619 RVA: 0x000357CC File Offset: 0x000339CC
	private void Update()
	{
		this.UpdateGamepadInput();
	}

	// Token: 0x06000654 RID: 1620 RVA: 0x000357D4 File Offset: 0x000339D4
	private void FillTextList()
	{
		foreach (TextsDialog.TextInfo textInfo in this.m_texts)
		{
			UnityEngine.Object.Destroy(textInfo.m_listElement);
		}
		this.m_texts.Clear();
		this.UpdateTextsList();
		for (int i = 0; i < this.m_texts.Count; i++)
		{
			TextsDialog.TextInfo text = this.m_texts[i];
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_elementPrefab, Vector3.zero, Quaternion.identity, this.m_listRoot);
			gameObject.SetActive(true);
			(gameObject.transform as RectTransform).anchoredPosition = new Vector2(0f, (float)(-(float)i) * this.m_spacing);
			Utils.FindChild(gameObject.transform, "name").GetComponent<Text>().text = Localization.instance.Localize(text.m_topic);
			text.m_listElement = gameObject;
			text.m_selected = Utils.FindChild(gameObject.transform, "selected").gameObject;
			text.m_selected.SetActive(false);
			gameObject.GetComponent<Button>().onClick.AddListener(delegate
			{
				this.OnSelectText(text);
			});
		}
		float size = Mathf.Max(this.m_baseListSize, (float)this.m_texts.Count * this.m_spacing);
		this.m_listRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
		if (this.m_texts.Count > 0)
		{
			this.m_recipeEnsureVisible.CenterOnItem(this.m_texts[0].m_listElement.transform as RectTransform);
		}
	}

	// Token: 0x06000655 RID: 1621 RVA: 0x000359AC File Offset: 0x00033BAC
	private void UpdateGamepadInput()
	{
		if (this.m_texts.Count > 0)
		{
			if (ZInput.GetButtonDown("JoyLStickDown"))
			{
				this.ShowText(Mathf.Min(this.m_texts.Count - 1, this.GetSelectedText() + 1));
			}
			if (ZInput.GetButtonDown("JoyLStickUp"))
			{
				this.ShowText(Mathf.Max(0, this.GetSelectedText() - 1));
			}
		}
	}

	// Token: 0x06000656 RID: 1622 RVA: 0x00035A13 File Offset: 0x00033C13
	private void OnSelectText(TextsDialog.TextInfo text)
	{
		this.ShowText(text);
	}

	// Token: 0x06000657 RID: 1623 RVA: 0x00035A1C File Offset: 0x00033C1C
	private int GetSelectedText()
	{
		for (int i = 0; i < this.m_texts.Count; i++)
		{
			if (this.m_texts[i].m_selected.activeSelf)
			{
				return i;
			}
		}
		return 0;
	}

	// Token: 0x06000658 RID: 1624 RVA: 0x00035A5A File Offset: 0x00033C5A
	private void ShowText(int i)
	{
		this.ShowText(this.m_texts[i]);
	}

	// Token: 0x06000659 RID: 1625 RVA: 0x00035A70 File Offset: 0x00033C70
	private void ShowText(TextsDialog.TextInfo text)
	{
		this.m_textAreaTopic.text = Localization.instance.Localize(text.m_topic);
		this.m_textArea.text = Localization.instance.Localize(text.m_text);
		foreach (TextsDialog.TextInfo textInfo in this.m_texts)
		{
			textInfo.m_selected.SetActive(false);
		}
		text.m_selected.SetActive(true);
	}

	// Token: 0x0600065A RID: 1626 RVA: 0x00034714 File Offset: 0x00032914
	public void OnClose()
	{
		base.gameObject.SetActive(false);
	}

	// Token: 0x0600065B RID: 1627 RVA: 0x00035B08 File Offset: 0x00033D08
	private void UpdateTextsList()
	{
		this.m_texts.Clear();
		foreach (KeyValuePair<string, string> keyValuePair in Player.m_localPlayer.GetKnownTexts())
		{
			this.m_texts.Add(new TextsDialog.TextInfo(Localization.instance.Localize(keyValuePair.Key), Localization.instance.Localize(keyValuePair.Value)));
		}
		this.m_texts.Sort((TextsDialog.TextInfo a, TextsDialog.TextInfo b) => a.m_topic.CompareTo(b.m_topic));
		this.AddLog();
		this.AddActiveEffects();
	}

	// Token: 0x0600065C RID: 1628 RVA: 0x00035BCC File Offset: 0x00033DCC
	private void AddLog()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string str in MessageHud.instance.GetLog())
		{
			stringBuilder.Append(str + "\n\n");
		}
		this.m_texts.Insert(0, new TextsDialog.TextInfo(Localization.instance.Localize("$inventory_logs"), stringBuilder.ToString()));
	}

	// Token: 0x0600065D RID: 1629 RVA: 0x00035C5C File Offset: 0x00033E5C
	private void AddActiveEffects()
	{
		if (!Player.m_localPlayer)
		{
			return;
		}
		List<StatusEffect> list = new List<StatusEffect>();
		Player.m_localPlayer.GetSEMan().GetHUDStatusEffects(list);
		StringBuilder stringBuilder = new StringBuilder(256);
		foreach (StatusEffect statusEffect in list)
		{
			stringBuilder.Append("<color=orange>" + Localization.instance.Localize(statusEffect.m_name) + "</color>\n");
			stringBuilder.Append(Localization.instance.Localize(statusEffect.GetTooltipString()));
			stringBuilder.Append("\n\n");
		}
		StatusEffect statusEffect2;
		float num;
		Player.m_localPlayer.GetGuardianPowerHUD(out statusEffect2, out num);
		if (statusEffect2)
		{
			stringBuilder.Append("<color=yellow>" + Localization.instance.Localize("$inventory_selectedgp") + "</color>\n");
			stringBuilder.Append("<color=orange>" + Localization.instance.Localize(statusEffect2.m_name) + "</color>\n");
			stringBuilder.Append(Localization.instance.Localize(statusEffect2.GetTooltipString()));
		}
		this.m_texts.Insert(0, new TextsDialog.TextInfo(Localization.instance.Localize("$inventory_activeeffects"), stringBuilder.ToString()));
	}

	// Token: 0x04000712 RID: 1810
	public RectTransform m_listRoot;

	// Token: 0x04000713 RID: 1811
	public GameObject m_elementPrefab;

	// Token: 0x04000714 RID: 1812
	public Text m_totalSkillText;

	// Token: 0x04000715 RID: 1813
	public float m_spacing = 80f;

	// Token: 0x04000716 RID: 1814
	public Text m_textAreaTopic;

	// Token: 0x04000717 RID: 1815
	public Text m_textArea;

	// Token: 0x04000718 RID: 1816
	public ScrollRectEnsureVisible m_recipeEnsureVisible;

	// Token: 0x04000719 RID: 1817
	private List<TextsDialog.TextInfo> m_texts = new List<TextsDialog.TextInfo>();

	// Token: 0x0400071A RID: 1818
	private float m_baseListSize;

	// Token: 0x0200015D RID: 349
	public class TextInfo
	{
		// Token: 0x06001123 RID: 4387 RVA: 0x00077908 File Offset: 0x00075B08
		public TextInfo(string topic, string text)
		{
			this.m_topic = topic;
			this.m_text = text;
		}

		// Token: 0x04001134 RID: 4404
		public string m_topic;

		// Token: 0x04001135 RID: 4405
		public string m_text;

		// Token: 0x04001136 RID: 4406
		public GameObject m_listElement;

		// Token: 0x04001137 RID: 4407
		public GameObject m_selected;
	}
}
