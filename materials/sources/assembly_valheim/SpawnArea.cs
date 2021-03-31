using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000AF RID: 175
public class SpawnArea : MonoBehaviour
{
	// Token: 0x06000BC3 RID: 3011 RVA: 0x00053B1D File Offset: 0x00051D1D
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		base.InvokeRepeating("UpdateSpawn", 2f, 2f);
	}

	// Token: 0x06000BC4 RID: 3012 RVA: 0x00053B40 File Offset: 0x00051D40
	private void UpdateSpawn()
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (ZNetScene.instance.OutsideActiveArea(base.transform.position))
		{
			return;
		}
		if (!Player.IsPlayerInRange(base.transform.position, this.m_triggerDistance))
		{
			return;
		}
		this.m_spawnTimer += 2f;
		if (this.m_spawnTimer > this.m_spawnIntervalSec)
		{
			this.m_spawnTimer = 0f;
			this.SpawnOne();
		}
	}

	// Token: 0x06000BC5 RID: 3013 RVA: 0x00053BC0 File Offset: 0x00051DC0
	private bool SpawnOne()
	{
		int num;
		int num2;
		this.GetInstances(out num, out num2);
		if (num >= this.m_maxNear || num2 >= this.m_maxTotal)
		{
			return false;
		}
		SpawnArea.SpawnData spawnData = this.SelectWeightedPrefab();
		if (spawnData == null)
		{
			return false;
		}
		Vector3 position;
		if (!this.FindSpawnPoint(spawnData.m_prefab, out position))
		{
			return false;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(spawnData.m_prefab, position, Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f));
		if (this.m_setPatrolSpawnPoint)
		{
			BaseAI component = gameObject.GetComponent<BaseAI>();
			if (component != null)
			{
				component.SetPatrolPoint();
			}
		}
		Character component2 = gameObject.GetComponent<Character>();
		if (spawnData.m_maxLevel > 1)
		{
			int num3 = spawnData.m_minLevel;
			while (num3 < spawnData.m_maxLevel && UnityEngine.Random.Range(0f, 100f) <= this.m_levelupChance)
			{
				num3++;
			}
			if (num3 > 1)
			{
				component2.SetLevel(num3);
			}
		}
		Vector3 centerPoint = component2.GetCenterPoint();
		this.m_spawnEffects.Create(centerPoint, Quaternion.identity, null, 1f);
		return true;
	}

	// Token: 0x06000BC6 RID: 3014 RVA: 0x00053CC8 File Offset: 0x00051EC8
	private bool FindSpawnPoint(GameObject prefab, out Vector3 point)
	{
		prefab.GetComponent<BaseAI>();
		for (int i = 0; i < 10; i++)
		{
			Vector3 vector = base.transform.position + Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f) * Vector3.forward * UnityEngine.Random.Range(0f, this.m_spawnRadius);
			float num;
			if (ZoneSystem.instance.FindFloor(vector, out num) && (!this.m_onGroundOnly || !ZoneSystem.instance.IsBlocked(vector)))
			{
				vector.y = num + 0.1f;
				point = vector;
				return true;
			}
		}
		point = Vector3.zero;
		return false;
	}

	// Token: 0x06000BC7 RID: 3015 RVA: 0x00053D84 File Offset: 0x00051F84
	private SpawnArea.SpawnData SelectWeightedPrefab()
	{
		if (this.m_prefabs.Count == 0)
		{
			return null;
		}
		float num = 0f;
		foreach (SpawnArea.SpawnData spawnData in this.m_prefabs)
		{
			num += spawnData.m_weight;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		float num3 = 0f;
		foreach (SpawnArea.SpawnData spawnData2 in this.m_prefabs)
		{
			num3 += spawnData2.m_weight;
			if (num2 <= num3)
			{
				return spawnData2;
			}
		}
		return this.m_prefabs[this.m_prefabs.Count - 1];
	}

	// Token: 0x06000BC8 RID: 3016 RVA: 0x00053E70 File Offset: 0x00052070
	private void GetInstances(out int near, out int total)
	{
		near = 0;
		total = 0;
		Vector3 position = base.transform.position;
		foreach (BaseAI baseAI in BaseAI.GetAllInstances())
		{
			if (this.IsSpawnPrefab(baseAI.gameObject))
			{
				float num = Utils.DistanceXZ(baseAI.transform.position, position);
				if (num < this.m_nearRadius)
				{
					near++;
				}
				if (num < this.m_farRadius)
				{
					total++;
				}
			}
		}
	}

	// Token: 0x06000BC9 RID: 3017 RVA: 0x00053F0C File Offset: 0x0005210C
	private bool IsSpawnPrefab(GameObject go)
	{
		string name = go.name;
		foreach (SpawnArea.SpawnData spawnData in this.m_prefabs)
		{
			if (name.StartsWith(spawnData.m_prefab.name))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000BCA RID: 3018 RVA: 0x00053F7C File Offset: 0x0005217C
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(base.transform.position, this.m_spawnRadius);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(base.transform.position, this.m_nearRadius);
	}

	// Token: 0x04000AF0 RID: 2800
	private const float dt = 2f;

	// Token: 0x04000AF1 RID: 2801
	public List<SpawnArea.SpawnData> m_prefabs = new List<SpawnArea.SpawnData>();

	// Token: 0x04000AF2 RID: 2802
	public float m_levelupChance = 15f;

	// Token: 0x04000AF3 RID: 2803
	public float m_spawnIntervalSec = 30f;

	// Token: 0x04000AF4 RID: 2804
	public float m_triggerDistance = 256f;

	// Token: 0x04000AF5 RID: 2805
	public bool m_setPatrolSpawnPoint = true;

	// Token: 0x04000AF6 RID: 2806
	public float m_spawnRadius = 2f;

	// Token: 0x04000AF7 RID: 2807
	public float m_nearRadius = 10f;

	// Token: 0x04000AF8 RID: 2808
	public float m_farRadius = 1000f;

	// Token: 0x04000AF9 RID: 2809
	public int m_maxNear = 3;

	// Token: 0x04000AFA RID: 2810
	public int m_maxTotal = 20;

	// Token: 0x04000AFB RID: 2811
	public bool m_onGroundOnly;

	// Token: 0x04000AFC RID: 2812
	public EffectList m_spawnEffects = new EffectList();

	// Token: 0x04000AFD RID: 2813
	private ZNetView m_nview;

	// Token: 0x04000AFE RID: 2814
	private float m_spawnTimer;

	// Token: 0x02000185 RID: 389
	[Serializable]
	public class SpawnData
	{
		// Token: 0x040011E1 RID: 4577
		public GameObject m_prefab;

		// Token: 0x040011E2 RID: 4578
		public float m_weight;

		// Token: 0x040011E3 RID: 4579
		[Header("Level")]
		public int m_maxLevel = 1;

		// Token: 0x040011E4 RID: 4580
		public int m_minLevel = 1;
	}
}
