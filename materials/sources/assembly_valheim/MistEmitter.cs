using System;
using UnityEngine;

// Token: 0x02000042 RID: 66
public class MistEmitter : MonoBehaviour
{
	// Token: 0x06000476 RID: 1142 RVA: 0x0002418A File Offset: 0x0002238A
	public void SetEmit(bool emit)
	{
		this.m_emit = emit;
	}

	// Token: 0x06000477 RID: 1143 RVA: 0x00024193 File Offset: 0x00022393
	private void Update()
	{
		if (!this.m_emit)
		{
			return;
		}
		this.m_placeTimer += Time.deltaTime;
		if (this.m_placeTimer > this.m_interval)
		{
			this.m_placeTimer = 0f;
			this.PlaceOne();
		}
	}

	// Token: 0x06000478 RID: 1144 RVA: 0x000241D0 File Offset: 0x000223D0
	private void PlaceOne()
	{
		Vector3 vector;
		if (this.GetRandomPoint(base.transform.position, this.m_totalRadius, out vector))
		{
			int num = 0;
			float num2 = 6.2831855f / (float)this.m_rays;
			for (int i = 0; i < this.m_rays; i++)
			{
				float angle = (float)i * num2;
				if ((double)this.GetPointOnEdge(vector, angle, this.m_testRadius).y < (double)vector.y - 0.1)
				{
					num++;
				}
			}
			if (num > this.m_rays / 4)
			{
				return;
			}
			if (EffectArea.IsPointInsideArea(vector, EffectArea.Type.Fire, this.m_testRadius))
			{
				return;
			}
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			emitParams.position = vector + Vector3.up * this.m_placeOffset;
			this.m_psystem.Emit(emitParams, 1);
		}
	}

	// Token: 0x06000479 RID: 1145 RVA: 0x000242A8 File Offset: 0x000224A8
	private bool GetRandomPoint(Vector3 center, float radius, out Vector3 p)
	{
		float f = UnityEngine.Random.value * 3.1415927f * 2f;
		float num = UnityEngine.Random.Range(0f, radius);
		p = center + new Vector3(Mathf.Sin(f) * num, 0f, Mathf.Cos(f) * num);
		float num2;
		if (!ZoneSystem.instance.GetGroundHeight(p, out num2))
		{
			return false;
		}
		if (num2 < ZoneSystem.instance.m_waterLevel)
		{
			return false;
		}
		p.y = num2;
		return true;
	}

	// Token: 0x0600047A RID: 1146 RVA: 0x00024328 File Offset: 0x00022528
	private Vector3 GetPointOnEdge(Vector3 center, float angle, float radius)
	{
		Vector3 vector = center + new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
		vector.y = ZoneSystem.instance.GetGroundHeight(vector);
		if (vector.y < ZoneSystem.instance.m_waterLevel)
		{
			vector.y = ZoneSystem.instance.m_waterLevel;
		}
		return vector;
	}

	// Token: 0x04000487 RID: 1159
	public float m_interval = 1f;

	// Token: 0x04000488 RID: 1160
	public float m_totalRadius = 30f;

	// Token: 0x04000489 RID: 1161
	public float m_testRadius = 5f;

	// Token: 0x0400048A RID: 1162
	public int m_rays = 10;

	// Token: 0x0400048B RID: 1163
	public float m_placeOffset = 1f;

	// Token: 0x0400048C RID: 1164
	public ParticleSystem m_psystem;

	// Token: 0x0400048D RID: 1165
	private float m_placeTimer;

	// Token: 0x0400048E RID: 1166
	private bool m_emit = true;
}
