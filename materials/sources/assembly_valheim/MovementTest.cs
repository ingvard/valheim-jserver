using System;
using UnityEngine;

// Token: 0x020000E3 RID: 227
public class MovementTest : MonoBehaviour
{
	// Token: 0x06000E30 RID: 3632 RVA: 0x000654F0 File Offset: 0x000636F0
	private void Start()
	{
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_center = base.transform.position;
	}

	// Token: 0x06000E31 RID: 3633 RVA: 0x00065510 File Offset: 0x00063710
	private void FixedUpdate()
	{
		this.m_timer += Time.fixedDeltaTime;
		float num = 5f;
		Vector3 vector = this.m_center + new Vector3(Mathf.Sin(this.m_timer * this.m_speed) * num, 0f, Mathf.Cos(this.m_timer * this.m_speed) * num);
		this.m_vel = (vector - this.m_body.position) / Time.fixedDeltaTime;
		this.m_body.position = vector;
		this.m_body.velocity = this.m_vel;
	}

	// Token: 0x04000CE3 RID: 3299
	public float m_speed = 10f;

	// Token: 0x04000CE4 RID: 3300
	private float m_timer;

	// Token: 0x04000CE5 RID: 3301
	private Rigidbody m_body;

	// Token: 0x04000CE6 RID: 3302
	private Vector3 m_center;

	// Token: 0x04000CE7 RID: 3303
	private Vector3 m_vel;
}
