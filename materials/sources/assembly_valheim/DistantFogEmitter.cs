using System;
using UnityEngine;

// Token: 0x02000035 RID: 53
public class DistantFogEmitter : MonoBehaviour
{
	// Token: 0x06000417 RID: 1047 RVA: 0x00021336 File Offset: 0x0001F536
	public void SetEmit(bool emit)
	{
		this.m_emit = emit;
	}

	// Token: 0x06000418 RID: 1048 RVA: 0x00021340 File Offset: 0x0001F540
	private void Update()
	{
		if (!this.m_emit)
		{
			return;
		}
		if (WorldGenerator.instance == null)
		{
			return;
		}
		this.m_placeTimer += Time.deltaTime;
		if (this.m_placeTimer > this.m_interval)
		{
			this.m_placeTimer = 0f;
			int num = Mathf.Max(0, this.m_particles - this.TotalNrOfParticles());
			num /= 4;
			for (int i = 0; i < num; i++)
			{
				this.PlaceOne();
			}
		}
	}

	// Token: 0x06000419 RID: 1049 RVA: 0x000213B4 File Offset: 0x0001F5B4
	private int TotalNrOfParticles()
	{
		int num = 0;
		foreach (ParticleSystem particleSystem in this.m_psystems)
		{
			num += particleSystem.particleCount;
		}
		return num;
	}

	// Token: 0x0600041A RID: 1050 RVA: 0x000213E8 File Offset: 0x0001F5E8
	private void PlaceOne()
	{
		Vector3 a;
		if (this.GetRandomPoint(base.transform.position, out a))
		{
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			emitParams.position = a + Vector3.up * this.m_placeOffset;
			this.m_psystems[UnityEngine.Random.Range(0, this.m_psystems.Length)].Emit(emitParams, 1);
		}
	}

	// Token: 0x0600041B RID: 1051 RVA: 0x0002144C File Offset: 0x0001F64C
	private bool GetRandomPoint(Vector3 center, out Vector3 p)
	{
		float f = UnityEngine.Random.value * 3.1415927f * 2f;
		float num = Mathf.Sqrt(UnityEngine.Random.value) * (this.m_maxRadius - this.m_minRadius) + this.m_minRadius;
		p = center + new Vector3(Mathf.Sin(f) * num, 0f, Mathf.Cos(f) * num);
		p.y = WorldGenerator.instance.GetHeight(p.x, p.z);
		if (p.y < ZoneSystem.instance.m_waterLevel)
		{
			if (this.m_skipWater)
			{
				return false;
			}
			if (UnityEngine.Random.value > this.m_waterSpawnChance)
			{
				return false;
			}
			p.y = ZoneSystem.instance.m_waterLevel;
		}
		else if (p.y > this.m_mountainLimit)
		{
			if (UnityEngine.Random.value > this.m_mountainSpawnChance)
			{
				return false;
			}
		}
		else if (UnityEngine.Random.value > this.m_landSpawnChance)
		{
			return false;
		}
		return true;
	}

	// Token: 0x04000407 RID: 1031
	public float m_interval = 1f;

	// Token: 0x04000408 RID: 1032
	public float m_minRadius = 100f;

	// Token: 0x04000409 RID: 1033
	public float m_maxRadius = 500f;

	// Token: 0x0400040A RID: 1034
	public float m_mountainSpawnChance = 1f;

	// Token: 0x0400040B RID: 1035
	public float m_landSpawnChance = 0.5f;

	// Token: 0x0400040C RID: 1036
	public float m_waterSpawnChance = 0.25f;

	// Token: 0x0400040D RID: 1037
	public float m_mountainLimit = 120f;

	// Token: 0x0400040E RID: 1038
	public float m_emitStep = 10f;

	// Token: 0x0400040F RID: 1039
	public int m_emitPerStep = 10;

	// Token: 0x04000410 RID: 1040
	public int m_particles = 100;

	// Token: 0x04000411 RID: 1041
	public float m_placeOffset = 1f;

	// Token: 0x04000412 RID: 1042
	public ParticleSystem[] m_psystems;

	// Token: 0x04000413 RID: 1043
	public bool m_skipWater;

	// Token: 0x04000414 RID: 1044
	private float m_placeTimer;

	// Token: 0x04000415 RID: 1045
	private bool m_emit = true;

	// Token: 0x04000416 RID: 1046
	private Vector3 m_lastPosition = Vector3.zero;
}
