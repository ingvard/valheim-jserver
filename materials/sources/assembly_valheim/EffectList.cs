using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000037 RID: 55
[Serializable]
public class EffectList
{
	// Token: 0x06000422 RID: 1058 RVA: 0x00021830 File Offset: 0x0001FA30
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

	// Token: 0x06000423 RID: 1059 RVA: 0x00021954 File Offset: 0x0001FB54
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

	// Token: 0x04000423 RID: 1059
	public EffectList.EffectData[] m_effectPrefabs = new EffectList.EffectData[0];

	// Token: 0x02000138 RID: 312
	[Serializable]
	public class EffectData
	{
		// Token: 0x04001055 RID: 4181
		public GameObject m_prefab;

		// Token: 0x04001056 RID: 4182
		public bool m_enabled = true;

		// Token: 0x04001057 RID: 4183
		public bool m_attach;

		// Token: 0x04001058 RID: 4184
		public bool m_inheritParentRotation;

		// Token: 0x04001059 RID: 4185
		public bool m_inheritParentScale;

		// Token: 0x0400105A RID: 4186
		public bool m_randomRotation;

		// Token: 0x0400105B RID: 4187
		public bool m_scale;
	}
}
