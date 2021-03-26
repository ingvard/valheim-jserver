using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000043 RID: 67
[ExecuteInEditMode]
public class ParticleDecal : MonoBehaviour
{
	// Token: 0x0600047B RID: 1147 RVA: 0x00024326 File Offset: 0x00022526
	private void Awake()
	{
		this.part = base.GetComponent<ParticleSystem>();
		this.collisionEvents = new List<ParticleCollisionEvent>();
	}

	// Token: 0x0600047C RID: 1148 RVA: 0x00024340 File Offset: 0x00022540
	private void OnParticleCollision(GameObject other)
	{
		if (this.m_chance < 100f && UnityEngine.Random.Range(0f, 100f) > this.m_chance)
		{
			return;
		}
		int num = this.part.GetCollisionEvents(other, this.collisionEvents);
		for (int i = 0; i < num; i++)
		{
			ParticleCollisionEvent particleCollisionEvent = this.collisionEvents[i];
			Vector3 eulerAngles = Quaternion.LookRotation(particleCollisionEvent.normal).eulerAngles;
			eulerAngles.x = -eulerAngles.x + 180f;
			eulerAngles.y = -eulerAngles.y;
			eulerAngles.z = (float)UnityEngine.Random.Range(0, 360);
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			emitParams.position = particleCollisionEvent.intersection;
			emitParams.rotation3D = eulerAngles;
			emitParams.velocity = -particleCollisionEvent.normal * 0.001f;
			this.m_decalSystem.Emit(emitParams, 1);
		}
	}

	// Token: 0x0400048B RID: 1163
	public ParticleSystem m_decalSystem;

	// Token: 0x0400048C RID: 1164
	[Range(0f, 100f)]
	public float m_chance = 100f;

	// Token: 0x0400048D RID: 1165
	private ParticleSystem part;

	// Token: 0x0400048E RID: 1166
	private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
}
