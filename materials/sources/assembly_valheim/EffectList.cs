using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000037 RID: 55
[Serializable]
public class EffectList
{
	// Token: 0x06000421 RID: 1057 RVA: 0x0002177C File Offset: 0x0001F97C
	public GameObject[] Create(Vector3 pos, Quaternion rot, Transform parent = null, float scale = 1f)
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < this.m_effectPrefabs.Length; i++)
		{
			EffectList.EffectData effectData = this.m_effectPrefabs[i];
			if (effectData.m_enabled)
			{
				if (parent && this.m_effectPrefabs[i].m_inheritParentRotation)
				{
					rot = parent.rotation;
				}
				if (effectData.m_randomRotation)
				{
					rot = UnityEngine.Random.rotation;
				}
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(effectData.m_prefab, pos, rot);
				if (effectData.m_scale)
				{
					if (parent && this.m_effectPrefabs[i].m_inheritParentScale)
					{
						Vector3 localScale = parent.localScale * scale;
						gameObject.transform.localScale = localScale;
					}
					else
					{
						gameObject.transform.localScale = new Vector3(scale, scale, scale);
					}
				}
				else if (parent && this.m_effectPrefabs[i].m_inheritParentScale)
				{
					gameObject.transform.localScale = parent.localScale;
				}
				if (effectData.m_attach && parent != null)
				{
					gameObject.transform.SetParent(parent);
				}
				list.Add(gameObject);
			}
		}
		return list.ToArray();
	}

	// Token: 0x06000422 RID: 1058 RVA: 0x000218A0 File Offset: 0x0001FAA0
	public bool HasEffects()
	{
		if (this.m_effectPrefabs == null || this.m_effectPrefabs.Length == 0)
		{
			return false;
		}
		EffectList.EffectData[] effectPrefabs = this.m_effectPrefabs;
		for (int i = 0; i < effectPrefabs.Length; i++)
		{
			if (effectPrefabs[i].m_enabled)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x0400041F RID: 1055
	public EffectList.EffectData[] m_effectPrefabs = new EffectList.EffectData[0];

	// Token: 0x02000138 RID: 312
	[Serializable]
	public class EffectData
	{
		// Token: 0x0400104E RID: 4174
		public GameObject m_prefab;

		// Token: 0x0400104F RID: 4175
		public bool m_enabled = true;

		// Token: 0x04001050 RID: 4176
		public bool m_attach;

		// Token: 0x04001051 RID: 4177
		public bool m_inheritParentRotation;

		// Token: 0x04001052 RID: 4178
		public bool m_inheritParentScale;

		// Token: 0x04001053 RID: 4179
		public bool m_randomRotation;

		// Token: 0x04001054 RID: 4180
		public bool m_scale;
	}
}
