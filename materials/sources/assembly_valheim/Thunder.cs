using System;
using UnityEngine;

// Token: 0x02000049 RID: 73
public class Thunder : MonoBehaviour
{
	// Token: 0x06000496 RID: 1174 RVA: 0x00024ADC File Offset: 0x00022CDC
	private void Start()
	{
		this.m_strikeTimer = UnityEngine.Random.Range(this.m_strikeIntervalMin, this.m_strikeIntervalMax);
	}

	// Token: 0x06000497 RID: 1175 RVA: 0x00024AF8 File Offset: 0x00022CF8
	private void Update()
	{
		if (this.m_strikeTimer > 0f)
		{
			this.m_strikeTimer -= Time.deltaTime;
			if (this.m_strikeTimer <= 0f)
			{
				this.DoFlash();
			}
		}
		if (this.m_thunderTimer > 0f)
		{
			this.m_thunderTimer -= Time.deltaTime;
			if (this.m_thunderTimer <= 0f)
			{
				this.DoThunder();
				this.m_strikeTimer = UnityEngine.Random.Range(this.m_strikeIntervalMin, this.m_strikeIntervalMax);
			}
		}
		if (this.m_spawnThor)
		{
			this.m_thorTimer += Time.deltaTime;
			if (this.m_thorTimer > this.m_thorInterval)
			{
				this.m_thorTimer = 0f;
				if (UnityEngine.Random.value <= this.m_thorChance && (this.m_requiredGlobalKey == "" || ZoneSystem.instance.GetGlobalKey(this.m_requiredGlobalKey)))
				{
					this.SpawnThor();
				}
			}
		}
	}

	// Token: 0x06000498 RID: 1176 RVA: 0x00024BEC File Offset: 0x00022DEC
	private void SpawnThor()
	{
		float num = UnityEngine.Random.value * 6.2831855f;
		Vector3 vector = base.transform.position + new Vector3(Mathf.Sin(num), 0f, Mathf.Cos(num)) * this.m_thorSpawnDistance;
		vector.y += UnityEngine.Random.Range(this.m_thorSpawnAltitudeMin, this.m_thorSpawnAltitudeMax);
		float groundHeight = ZoneSystem.instance.GetGroundHeight(vector);
		if (vector.y < groundHeight)
		{
			vector.y = groundHeight + 50f;
		}
		float f = num + 180f + (float)UnityEngine.Random.Range(-45, 45);
		Vector3 vector2 = base.transform.position + new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f)) * this.m_thorSpawnDistance;
		vector2.y += UnityEngine.Random.Range(this.m_thorSpawnAltitudeMin, this.m_thorSpawnAltitudeMax);
		float groundHeight2 = ZoneSystem.instance.GetGroundHeight(vector2);
		if (vector.y < groundHeight2)
		{
			vector.y = groundHeight2 + 50f;
		}
		Vector3 normalized = (vector2 - vector).normalized;
		UnityEngine.Object.Instantiate<GameObject>(this.m_thorPrefab, vector, Quaternion.LookRotation(normalized));
	}

	// Token: 0x06000499 RID: 1177 RVA: 0x00024D28 File Offset: 0x00022F28
	private void DoFlash()
	{
		float f = UnityEngine.Random.value * 6.2831855f;
		float d = UnityEngine.Random.Range(this.m_flashDistanceMin, this.m_flashDistanceMax);
		this.m_flashPos = base.transform.position + new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f)) * d;
		this.m_flashPos.y = this.m_flashPos.y + this.m_flashAltitude;
		Quaternion rotation = Quaternion.LookRotation((base.transform.position - this.m_flashPos).normalized);
		GameObject[] array = this.m_flashEffect.Create(this.m_flashPos, Quaternion.identity, null, 1f);
		for (int i = 0; i < array.Length; i++)
		{
			Light[] componentsInChildren = array[i].GetComponentsInChildren<Light>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].transform.rotation = rotation;
			}
		}
		this.m_thunderTimer = UnityEngine.Random.Range(this.m_thunderDelayMin, this.m_thunderDelayMax);
	}

	// Token: 0x0600049A RID: 1178 RVA: 0x00024E35 File Offset: 0x00023035
	private void DoThunder()
	{
		this.m_thunderEffect.Create(this.m_flashPos, Quaternion.identity, null, 1f);
	}

	// Token: 0x040004B0 RID: 1200
	public float m_strikeIntervalMin = 3f;

	// Token: 0x040004B1 RID: 1201
	public float m_strikeIntervalMax = 10f;

	// Token: 0x040004B2 RID: 1202
	public float m_thunderDelayMin = 3f;

	// Token: 0x040004B3 RID: 1203
	public float m_thunderDelayMax = 5f;

	// Token: 0x040004B4 RID: 1204
	public float m_flashDistanceMin = 50f;

	// Token: 0x040004B5 RID: 1205
	public float m_flashDistanceMax = 200f;

	// Token: 0x040004B6 RID: 1206
	public float m_flashAltitude = 100f;

	// Token: 0x040004B7 RID: 1207
	public EffectList m_flashEffect = new EffectList();

	// Token: 0x040004B8 RID: 1208
	public EffectList m_thunderEffect = new EffectList();

	// Token: 0x040004B9 RID: 1209
	[Header("Thor")]
	public bool m_spawnThor;

	// Token: 0x040004BA RID: 1210
	public string m_requiredGlobalKey = "";

	// Token: 0x040004BB RID: 1211
	public GameObject m_thorPrefab;

	// Token: 0x040004BC RID: 1212
	public float m_thorSpawnDistance = 300f;

	// Token: 0x040004BD RID: 1213
	public float m_thorSpawnAltitudeMax = 100f;

	// Token: 0x040004BE RID: 1214
	public float m_thorSpawnAltitudeMin = 100f;

	// Token: 0x040004BF RID: 1215
	public float m_thorInterval = 10f;

	// Token: 0x040004C0 RID: 1216
	public float m_thorChance = 1f;

	// Token: 0x040004C1 RID: 1217
	private Vector3 m_flashPos = Vector3.zero;

	// Token: 0x040004C2 RID: 1218
	private float m_strikeTimer = -1f;

	// Token: 0x040004C3 RID: 1219
	private float m_thunderTimer = -1f;

	// Token: 0x040004C4 RID: 1220
	private float m_thorTimer;
}
