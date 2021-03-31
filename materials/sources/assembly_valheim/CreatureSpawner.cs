using System;
using UnityEngine;

// Token: 0x02000007 RID: 7
public class CreatureSpawner : MonoBehaviour
{
	// Token: 0x060000EE RID: 238 RVA: 0x00007111 File Offset: 0x00005311
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		base.InvokeRepeating("UpdateSpawner", UnityEngine.Random.Range(3f, 5f), 5f);
	}

	// Token: 0x060000EF RID: 239 RVA: 0x0000714C File Offset: 0x0000534C
	private void UpdateSpawner()
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		ZDOID zdoid = this.m_nview.GetZDO().GetZDOID("spawn_id");
		if (this.m_respawnTimeMinuts <= 0f && !zdoid.IsNone())
		{
			return;
		}
		if (!zdoid.IsNone() && ZDOMan.instance.GetZDO(zdoid) != null)
		{
			this.m_nview.GetZDO().Set("alive_time", ZNet.instance.GetTime().Ticks);
			return;
		}
		if (this.m_respawnTimeMinuts > 0f)
		{
			DateTime time = ZNet.instance.GetTime();
			DateTime d = new DateTime(this.m_nview.GetZDO().GetLong("alive_time", 0L));
			if ((time - d).TotalMinutes < (double)this.m_respawnTimeMinuts)
			{
				return;
			}
		}
		if (!this.m_spawnAtDay && EnvMan.instance.IsDay())
		{
			return;
		}
		if (!this.m_spawnAtNight && EnvMan.instance.IsNight())
		{
			return;
		}
		bool requireSpawnArea = this.m_requireSpawnArea;
		if (!this.m_spawnInPlayerBase && EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.PlayerBase, 0f))
		{
			return;
		}
		if (this.m_triggerNoise > 0f)
		{
			if (!Player.IsPlayerInRange(base.transform.position, this.m_triggerDistance, this.m_triggerNoise))
			{
				return;
			}
		}
		else if (!Player.IsPlayerInRange(base.transform.position, this.m_triggerDistance))
		{
			return;
		}
		this.Spawn();
	}

	// Token: 0x060000F0 RID: 240 RVA: 0x000072C4 File Offset: 0x000054C4
	private bool HasSpawned()
	{
		return !(this.m_nview == null) && this.m_nview.GetZDO() != null && !this.m_nview.GetZDO().GetZDOID("spawn_id").IsNone();
	}

	// Token: 0x060000F1 RID: 241 RVA: 0x00007310 File Offset: 0x00005510
	private ZNetView Spawn()
	{
		Vector3 position = base.transform.position;
		float num;
		if (ZoneSystem.instance.FindFloor(position, out num))
		{
			position.y = num + 0.25f;
		}
		Quaternion rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_creaturePrefab, position, rotation);
		ZNetView component = gameObject.GetComponent<ZNetView>();
		BaseAI component2 = gameObject.GetComponent<BaseAI>();
		if (component2 != null && this.m_setPatrolSpawnPoint)
		{
			component2.SetPatrolPoint();
		}
		if (this.m_maxLevel > 1)
		{
			Character component3 = gameObject.GetComponent<Character>();
			if (component3)
			{
				int num2 = this.m_minLevel;
				while (num2 < this.m_maxLevel && UnityEngine.Random.Range(0f, 100f) <= this.m_levelupChance)
				{
					num2++;
				}
				if (num2 > 1)
				{
					component3.SetLevel(num2);
				}
			}
		}
		component.GetZDO().SetPGWVersion(this.m_nview.GetZDO().GetPGWVersion());
		this.m_nview.GetZDO().Set("spawn_id", component.GetZDO().m_uid);
		this.m_nview.GetZDO().Set("alive_time", ZNet.instance.GetTime().Ticks);
		this.SpawnEffect(gameObject);
		return component;
	}

	// Token: 0x060000F2 RID: 242 RVA: 0x00007464 File Offset: 0x00005664
	private void SpawnEffect(GameObject spawnedObject)
	{
		Character component = spawnedObject.GetComponent<Character>();
		Vector3 pos = component ? component.GetCenterPoint() : (base.transform.position + Vector3.up * 0.75f);
		this.m_spawnEffects.Create(pos, Quaternion.identity, null, 1f);
	}

	// Token: 0x060000F3 RID: 243 RVA: 0x000074C0 File Offset: 0x000056C0
	private float GetRadius()
	{
		return 0.75f;
	}

	// Token: 0x060000F4 RID: 244 RVA: 0x000027E0 File Offset: 0x000009E0
	private void OnDrawGizmos()
	{
	}

	// Token: 0x040000C3 RID: 195
	private const float m_radius = 0.75f;

	// Token: 0x040000C4 RID: 196
	public GameObject m_creaturePrefab;

	// Token: 0x040000C5 RID: 197
	[Header("Level")]
	public int m_maxLevel = 1;

	// Token: 0x040000C6 RID: 198
	public int m_minLevel = 1;

	// Token: 0x040000C7 RID: 199
	public float m_levelupChance = 15f;

	// Token: 0x040000C8 RID: 200
	[Header("Spawn settings")]
	public float m_respawnTimeMinuts = 20f;

	// Token: 0x040000C9 RID: 201
	public float m_triggerDistance = 60f;

	// Token: 0x040000CA RID: 202
	public float m_triggerNoise;

	// Token: 0x040000CB RID: 203
	public bool m_spawnAtNight = true;

	// Token: 0x040000CC RID: 204
	public bool m_spawnAtDay = true;

	// Token: 0x040000CD RID: 205
	public bool m_requireSpawnArea;

	// Token: 0x040000CE RID: 206
	public bool m_spawnInPlayerBase;

	// Token: 0x040000CF RID: 207
	public bool m_setPatrolSpawnPoint;

	// Token: 0x040000D0 RID: 208
	public EffectList m_spawnEffects = new EffectList();

	// Token: 0x040000D1 RID: 209
	private ZNetView m_nview;
}
