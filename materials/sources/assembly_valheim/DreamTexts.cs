using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000C5 RID: 197
public class DreamTexts : MonoBehaviour
{
	// Token: 0x06000CEF RID: 3311 RVA: 0x0005C924 File Offset: 0x0005AB24
	public DreamTexts.DreamText GetRandomDreamText()
	{
		List<DreamTexts.DreamText> list = new List<DreamTexts.DreamText>();
		foreach (DreamTexts.DreamText dreamText in this.m_texts)
		{
			if (this.HaveGlobalKeys(dreamText))
			{
				list.Add(dreamText);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		DreamTexts.DreamText dreamText2 = list[UnityEngine.Random.Range(0, list.Count)];
		if (UnityEngine.Random.value <= dreamText2.m_chanceToDream)
		{
			return dreamText2;
		}
		return null;
	}

	// Token: 0x06000CF0 RID: 3312 RVA: 0x0005C9B4 File Offset: 0x0005ABB4
	private bool HaveGlobalKeys(DreamTexts.DreamText dream)
	{
		foreach (string name in dream.m_trueKeys)
		{
			if (!ZoneSystem.instance.GetGlobalKey(name))
			{
				return false;
			}
		}
		foreach (string name2 in dream.m_falseKeys)
		{
			if (ZoneSystem.instance.GetGlobalKey(name2))
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x04000BD3 RID: 3027
	public List<DreamTexts.DreamText> m_texts = new List<DreamTexts.DreamText>();

	// Token: 0x02000194 RID: 404
	[Serializable]
	public class DreamText
	{
		// Token: 0x04001280 RID: 4736
		public string m_text = "Fluffy sheep";

		// Token: 0x04001281 RID: 4737
		public float m_chanceToDream = 0.1f;

		// Token: 0x04001282 RID: 4738
		public List<string> m_trueKeys = new List<string>();

		// Token: 0x04001283 RID: 4739
		public List<string> m_falseKeys = new List<string>();
	}
}
