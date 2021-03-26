using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000015 RID: 21
public class Tail : MonoBehaviour
{
	// Token: 0x0600027F RID: 639 RVA: 0x000141D8 File Offset: 0x000123D8
	private void Awake()
	{
		foreach (Transform transform in this.m_tailJoints)
		{
			float distance = Vector3.Distance(transform.parent.position, transform.position);
			Vector3 position = transform.position;
			Tail.TailSegment tailSegment = new Tail.TailSegment();
			tailSegment.transform = transform;
			tailSegment.pos = position;
			tailSegment.rot = transform.rotation;
			tailSegment.distance = distance;
			this.m_positions.Add(tailSegment);
		}
	}

	// Token: 0x06000280 RID: 640 RVA: 0x0001427C File Offset: 0x0001247C
	private void LateUpdate()
	{
		float deltaTime = Time.deltaTime;
		if (this.m_character)
		{
			this.m_character.IsSwiming();
		}
		for (int i = 0; i < this.m_positions.Count; i++)
		{
			Tail.TailSegment tailSegment = this.m_positions[i];
			if (this.m_waterSurfaceCheck)
			{
				float waterLevel = WaterVolume.GetWaterLevel(tailSegment.pos, 1f);
				if (tailSegment.pos.y + this.m_tailRadius > waterLevel)
				{
					Tail.TailSegment tailSegment2 = tailSegment;
					tailSegment2.pos.y = tailSegment2.pos.y - this.m_gravity * deltaTime;
				}
				else
				{
					Tail.TailSegment tailSegment3 = tailSegment;
					tailSegment3.pos.y = tailSegment3.pos.y - this.m_gravityInWater * deltaTime;
				}
			}
			else
			{
				Tail.TailSegment tailSegment4 = tailSegment;
				tailSegment4.pos.y = tailSegment4.pos.y - this.m_gravity * deltaTime;
			}
			Vector3 a = tailSegment.transform.parent.position + tailSegment.transform.parent.up * tailSegment.distance * 0.5f;
			Vector3 vector = Vector3.Normalize(a - tailSegment.pos);
			vector = Vector3.RotateTowards(-tailSegment.transform.parent.up, vector, 0.017453292f * this.m_maxAngle, 1f);
			Vector3 vector2 = a - vector * tailSegment.distance * 0.5f;
			if (this.m_groundCheck)
			{
				float groundHeight = ZoneSystem.instance.GetGroundHeight(vector2);
				if (vector2.y - this.m_tailRadius < groundHeight)
				{
					vector2.y = groundHeight + this.m_tailRadius;
				}
			}
			vector2 = Vector3.Lerp(tailSegment.pos, vector2, this.m_smoothness);
			Vector3 normalized = (a - vector2).normalized;
			Vector3 rhs = Vector3.Cross(Vector3.up, -normalized);
			Quaternion quaternion = Quaternion.LookRotation(Vector3.Cross(-normalized, rhs), -normalized);
			quaternion = Quaternion.Slerp(tailSegment.rot, quaternion, this.m_smoothness);
			tailSegment.transform.position = vector2;
			tailSegment.transform.rotation = quaternion;
			tailSegment.pos = vector2;
			tailSegment.rot = quaternion;
		}
		if (this.m_tailBody)
		{
			this.m_tailBody.velocity = Vector3.zero;
			this.m_tailBody.angularVelocity = Vector3.zero;
		}
	}

	// Token: 0x040001E7 RID: 487
	public List<Transform> m_tailJoints = new List<Transform>();

	// Token: 0x040001E8 RID: 488
	public float m_yMovementDistance = 0.5f;

	// Token: 0x040001E9 RID: 489
	public float m_yMovementFreq = 0.5f;

	// Token: 0x040001EA RID: 490
	public float m_yMovementOffset = 0.2f;

	// Token: 0x040001EB RID: 491
	public float m_maxAngle = 80f;

	// Token: 0x040001EC RID: 492
	public float m_gravity = 2f;

	// Token: 0x040001ED RID: 493
	public float m_gravityInWater = 0.1f;

	// Token: 0x040001EE RID: 494
	public bool m_waterSurfaceCheck;

	// Token: 0x040001EF RID: 495
	public bool m_groundCheck;

	// Token: 0x040001F0 RID: 496
	public float m_smoothness = 0.1f;

	// Token: 0x040001F1 RID: 497
	public float m_tailRadius;

	// Token: 0x040001F2 RID: 498
	public Character m_character;

	// Token: 0x040001F3 RID: 499
	public Rigidbody m_characterBody;

	// Token: 0x040001F4 RID: 500
	public Rigidbody m_tailBody;

	// Token: 0x040001F5 RID: 501
	private List<Tail.TailSegment> m_positions = new List<Tail.TailSegment>();

	// Token: 0x0200012D RID: 301
	private class TailSegment
	{
		// Token: 0x0400101C RID: 4124
		public Transform transform;

		// Token: 0x0400101D RID: 4125
		public Vector3 pos;

		// Token: 0x0400101E RID: 4126
		public Quaternion rot;

		// Token: 0x0400101F RID: 4127
		public float distance;
	}
}
