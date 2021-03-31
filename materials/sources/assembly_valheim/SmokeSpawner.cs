using System;
using UnityEngine;

// Token: 0x02000047 RID: 71
public class SmokeSpawner : MonoBehaviour
{
	// Token: 0x0600048D RID: 1165 RVA: 0x00024864 File Offset: 0x00022A64
	private void Start()
	{
		this.m_time = UnityEngine.Random.Range(0f, this.m_interval);
	}

	// Token: 0x0600048E RID: 1166 RVA: 0x0002487C File Offset: 0x00022A7C
	private void Update()
	{
		this.m_time += Time.deltaTime;
		if (this.m_time > this.m_interval)
		{
			this.m_time = 0f;
			this.Spawn();
		}
	}

	// Token: 0x0600048F RID: 1167 RVA: 0x000248B0 File Offset: 0x00022AB0
	private void Spawn()
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null || Vector3.Distance(localPlayer.transform.position, base.transform.position) > 64f)
		{
			this.m_lastSpawnTime = Time.time;
			return;
		}
		if (this.TestBlocked())
		{
			return;
		}
		if (Smoke.GetTotalSmoke() > 100)
		{
			Smoke.FadeOldest();
		}
		UnityEngine.Object.Instantiate<GameObject>(this.m_smokePrefab, base.transform.position, UnityEngine.Random.rotation);
		this.m_lastSpawnTime = Time.time;
	}

	// Token: 0x06000490 RID: 1168 RVA: 0x00024938 File Offset: 0x00022B38
	private bool TestBlocked()
	{
		return Physics.CheckSphere(base.transform.position, this.m_testRadius, this.m_testMask.value);
	}

	// Token: 0x06000491 RID: 1169 RVA: 0x00024960 File Offset: 0x00022B60
	public bool IsBlocked()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return this.TestBlocked();
		}
		return Time.time - this.m_lastSpawnTime > 4f;
	}

	// Token: 0x040004A0 RID: 1184
	private const float m_minPlayerDistance = 64f;

	// Token: 0x040004A1 RID: 1185
	private const int m_maxGlobalSmoke = 100;

	// Token: 0x040004A2 RID: 1186
	private const float m_blockedMinTime = 4f;

	// Token: 0x040004A3 RID: 1187
	public GameObject m_smokePrefab;

	// Token: 0x040004A4 RID: 1188
	public float m_interval = 0.5f;

	// Token: 0x040004A5 RID: 1189
	public LayerMask m_testMask;

	// Token: 0x040004A6 RID: 1190
	public float m_testRadius = 0.5f;

	// Token: 0x040004A7 RID: 1191
	private float m_lastSpawnTime;

	// Token: 0x040004A8 RID: 1192
	private float m_time;
}
