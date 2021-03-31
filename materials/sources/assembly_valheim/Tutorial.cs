using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000065 RID: 101
public class Tutorial : MonoBehaviour
{
	// Token: 0x17000013 RID: 19
	// (get) Token: 0x06000663 RID: 1635 RVA: 0x00035ECE File Offset: 0x000340CE
	public static Tutorial instance
	{
		get
		{
			return Tutorial.m_instance;
		}
	}

	// Token: 0x06000664 RID: 1636 RVA: 0x00035ED5 File Offset: 0x000340D5
	private void Awake()
	{
		Tutorial.m_instance = this;
		this.m_windowRoot.gameObject.SetActive(false);
	}

	// Token: 0x06000665 RID: 1637 RVA: 0x000027E0 File Offset: 0x000009E0
	private void Update()
	{
	}

	// Token: 0x06000666 RID: 1638 RVA: 0x00035EF0 File Offset: 0x000340F0
	public void ShowText(string name, bool force)
	{
		Tutorial.TutorialText tutorialText = this.m_texts.Find((Tutorial.TutorialText x) => x.m_name == name);
		if (tutorialText != null)
		{
			this.SpawnRaven(tutorialText.m_name, tutorialText.m_topic, tutorialText.m_text, tutorialText.m_label);
		}
	}

	// Token: 0x06000667 RID: 1639 RVA: 0x00035F43 File Offset: 0x00034143
	private void SpawnRaven(string key, string topic, string text, string label)
	{
		if (!Raven.IsInstantiated())
		{
			UnityEngine.Object.Instantiate<GameObject>(this.m_ravenPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
		}
		Raven.AddTempText(key, topic, text, label, false);
	}

	// Token: 0x04000723 RID: 1827
	public List<Tutorial.TutorialText> m_texts = new List<Tutorial.TutorialText>();

	// Token: 0x04000724 RID: 1828
	public RectTransform m_windowRoot;

	// Token: 0x04000725 RID: 1829
	public Text m_topic;

	// Token: 0x04000726 RID: 1830
	public Text m_text;

	// Token: 0x04000727 RID: 1831
	public GameObject m_ravenPrefab;

	// Token: 0x04000728 RID: 1832
	private static Tutorial m_instance;

	// Token: 0x04000729 RID: 1833
	private Queue<string> m_tutQueue = new Queue<string>();

	// Token: 0x02000160 RID: 352
	[Serializable]
	public class TutorialText
	{
		// Token: 0x04001143 RID: 4419
		public string m_name;

		// Token: 0x04001144 RID: 4420
		public string m_topic = "";

		// Token: 0x04001145 RID: 4421
		public string m_label = "";

		// Token: 0x04001146 RID: 4422
		[TextArea]
		public string m_text = "";
	}
}
