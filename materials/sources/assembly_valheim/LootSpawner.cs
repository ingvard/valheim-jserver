using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200006C RID: 108
public class LootSpawner : MonoBehaviour
{
	// Token: 0x060006E0 RID: 1760 RVA: 0x00038AE1 File Offset: 0x00036CE1
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		base.InvokeRepeating("UpdateSpawner", 10f, 2f);
	}

	// Token: 0x060006E1 RID: 1761 RVA: 0x00038B14 File Offset: 0x00036D14
	private void UpdateSpawner()
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.m_spawnAtDay && EnvMan.instance.IsDay())
		{
			return;
		}
		if (!this.m_spawnAtNight && EnvMan.instance.IsNight())
		{
			return;
		}
		if (this.m_spawnWhenEnemiesCleared)
		{
			bool flag = LootSpawner.IsMonsterInRange(base.transform.position, this.m_enemiesCheckRange);
			if (flag && !this.m_seenEnemies)
			{
				this.m_seenEnemies = true;
			}
			if (flag || !this.m_seenEnemies)
			{
				return;
			}
		}
		long @long = this.m_nview.GetZDO().GetLong("spawn_time", 0L);
		DateTime time = ZNet.instance.GetTime();
		DateTime d = new DateTime(@long);
		TimeSpan timeSpan = time - d;
		if (this.m_respawnTimeMinuts <= 0f && @long != 0L)
		{
			return;
		}
		if (timeSpan.TotalMinutes < (double)this.m_respawnTimeMinuts)
		{
			return;
		}
		if (!Player.IsPlayerInRange(base.transform.position, 20f))
		{
			return;
		}
		List<GameObject> dropList = this.m_items.GetDropList();
		for (int i = 0; i < dropList.Count; i++)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle * 0.3f;
			Vector3 position = base.transform.position + new Vector3(vector.x, 0.3f * (float)i, vector.y);
			Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
			UnityEngine.Object.Instantiate<GameObject>(dropList[i], position, rotation);
		}
		this.m_spawnEffect.Create(base.transform.position, Quaternion.identity, null, 1f);
		this.m_nview.GetZDO().Set("spawn_time", ZNet.instance.GetTime().Ticks);
		this.m_seenEnemies = false;
	}

	// Token: 0x060006E2 RID: 1762 RVA: 0x00038CE0 File Offset: 0x00036EE0
	public static bool IsMonsterInRange(Vector3 point, float range)
	{
		foreach (Character character in Character.GetAllCharacters())
		{
			if (character.IsMonsterFaction() && Vector3.Distance(character.transform.position, point) < range)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060006E3 RID: 1763 RVA: 0x000027E0 File Offset: 0x000009E0
	private void OnDrawGizmos()
	{
	}

	// Token: 0x04000763 RID: 1891
	public DropTable m_items = new DropTable();

	// Token: 0x04000764 RID: 1892
	public EffectList m_spawnEffect = new EffectList();

	// Token: 0x04000765 RID: 1893
	public float m_respawnTimeMinuts = 10f;

	// Token: 0x04000766 RID: 1894
	private const float m_triggerDistance = 20f;

	// Token: 0x04000767 RID: 1895
	public bool m_spawnAtNight = true;

	// Token: 0x04000768 RID: 1896
	public bool m_spawnAtDay = true;

	// Token: 0x04000769 RID: 1897
	public bool m_spawnWhenEnemiesCleared;

	// Token: 0x0400076A RID: 1898
	public float m_enemiesCheckRange = 30f;

	// Token: 0x0400076B RID: 1899
	private ZNetView m_nview;

	// Token: 0x0400076C RID: 1900
	private bool m_seenEnemies;
}
