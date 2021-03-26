using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200005E RID: 94
public class SkillsDialog : MonoBehaviour
{
	// Token: 0x0600061F RID: 1567 RVA: 0x00034474 File Offset: 0x00032674
	private void Awake()
	{
		this.m_baseListSize = this.m_listRoot.rect.height;
	}

	// Token: 0x06000620 RID: 1568 RVA: 0x0003449C File Offset: 0x0003269C
	public void Setup(Player player)
	{
		base.gameObject.SetActive(true);
		foreach (GameObject obj in this.m_elements)
		{
			UnityEngine.Object.Destroy(obj);
		}
		this.m_elements.Clear();
		List<Skills.Skill> skillList = player.GetSkills().GetSkillList();
		for (int i = 0; i < skillList.Count; i++)
		{
			Skills.Skill skill = skillList[i];
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_elementPrefab, Vector3.zero, Quaternion.identity, this.m_listRoot);
			gameObject.SetActive(true);
			(gameObject.transform as RectTransform).anchoredPosition = new Vector2(0f, (float)(-(float)i) * this.m_spacing);
			gameObject.GetComponentInChildren<UITooltip>().m_text = skill.m_info.m_description;
			Utils.FindChild(gameObject.transform, "icon").GetComponent<Image>().sprite = skill.m_info.m_icon;
			Utils.FindChild(gameObject.transform, "name").GetComponent<Text>().text = Localization.instance.Localize("$skill_" + skill.m_info.m_skill.ToString().ToLower());
			Utils.FindChild(gameObject.transform, "leveltext").GetComponent<Text>().text = ((int)skill.m_level).ToString();
			Utils.FindChild(gameObject.transform, "levelbar").GetComponent<GuiBar>().SetValue(skill.m_level / 100f);
			Utils.FindChild(gameObject.transform, "currentlevel").GetComponent<GuiBar>().SetValue(skill.GetLevelPercentage());
			this.m_elements.Add(gameObject);
		}
		float size = Mathf.Max(this.m_baseListSize, (float)skillList.Count * this.m_spacing);
		this.m_listRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
		this.m_totalSkillText.text = string.Concat(new string[]
		{
			"<color=orange>",
			player.GetSkills().GetTotalSkill().ToString("0"),
			"</color><color=white> / </color><color=orange>",
			player.GetSkills().GetTotalSkillCap().ToString("0"),
			"</color>"
		});
	}

	// Token: 0x06000621 RID: 1569 RVA: 0x00034714 File Offset: 0x00032914
	public void OnClose()
	{
		base.gameObject.SetActive(false);
	}

	// Token: 0x040006DE RID: 1758
	public RectTransform m_listRoot;

	// Token: 0x040006DF RID: 1759
	public GameObject m_elementPrefab;

	// Token: 0x040006E0 RID: 1760
	public Text m_totalSkillText;

	// Token: 0x040006E1 RID: 1761
	public float m_spacing = 80f;

	// Token: 0x040006E2 RID: 1762
	private float m_baseListSize;

	// Token: 0x040006E3 RID: 1763
	private List<GameObject> m_elements = new List<GameObject>();
}
