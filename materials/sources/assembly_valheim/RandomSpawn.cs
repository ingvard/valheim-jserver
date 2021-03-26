using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000EC RID: 236
public class RandomSpawn : MonoBehaviour
{
	// Token: 0x06000E95 RID: 3733 RVA: 0x00068570 File Offset: 0x00066770
	public void Randomize()
	{
		bool spawned = UnityEngine.Random.Range(0f, 100f) <= this.m_chanceToSpawn;
		this.SetSpawned(spawned);
	}

	// Token: 0x06000E96 RID: 3734 RVA: 0x0006859F File Offset: 0x0006679F
	public void Reset()
	{
		this.SetSpawned(true);
	}

	// Token: 0x06000E97 RID: 3735 RVA: 0x000685A8 File Offset: 0x000667A8
	private void SetSpawned(bool doSpawn)
	{
		if (!doSpawn)
		{
			base.gameObject.SetActive(false);
			using (List<ZNetView>.Enumerator enumerator = this.m_childNetViews.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ZNetView znetView = enumerator.Current;
					znetView.gameObject.SetActive(false);
				}
				goto IL_62;
			}
		}
		if (this.m_nview == null)
		{
			base.gameObject.SetActive(true);
		}
		IL_62:
		if (this.m_OffObject != null)
		{
			this.m_OffObject.SetActive(!doSpawn);
		}
	}

	// Token: 0x06000E98 RID: 3736 RVA: 0x00068644 File Offset: 0x00066844
	public void Prepare()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_childNetViews = new List<ZNetView>();
		foreach (ZNetView znetView in base.gameObject.GetComponentsInChildren<ZNetView>(true))
		{
			if (Utils.IsEnabledInheirarcy(znetView.gameObject, base.gameObject))
			{
				this.m_childNetViews.Add(znetView);
			}
		}
	}

	// Token: 0x04000D7C RID: 3452
	public GameObject m_OffObject;

	// Token: 0x04000D7D RID: 3453
	[Range(0f, 100f)]
	public float m_chanceToSpawn = 50f;

	// Token: 0x04000D7E RID: 3454
	private List<ZNetView> m_childNetViews;

	// Token: 0x04000D7F RID: 3455
	private ZNetView m_nview;
}
