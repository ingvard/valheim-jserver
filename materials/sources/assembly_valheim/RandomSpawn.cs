using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000EC RID: 236
public class RandomSpawn : MonoBehaviour
{
	// Token: 0x06000E96 RID: 3734 RVA: 0x000686F8 File Offset: 0x000668F8
	public void Randomize()
	{
		bool spawned = UnityEngine.Random.Range(0f, 100f) <= this.m_chanceToSpawn;
		this.SetSpawned(spawned);
	}

	// Token: 0x06000E97 RID: 3735 RVA: 0x00068727 File Offset: 0x00066927
	public void Reset()
	{
		this.SetSpawned(true);
	}

	// Token: 0x06000E98 RID: 3736 RVA: 0x00068730 File Offset: 0x00066930
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

	// Token: 0x06000E99 RID: 3737 RVA: 0x000687CC File Offset: 0x000669CC
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

	// Token: 0x04000D82 RID: 3458
	public GameObject m_OffObject;

	// Token: 0x04000D83 RID: 3459
	[Range(0f, 100f)]
	public float m_chanceToSpawn = 50f;

	// Token: 0x04000D84 RID: 3460
	private List<ZNetView> m_childNetViews;

	// Token: 0x04000D85 RID: 3461
	private ZNetView m_nview;
}
