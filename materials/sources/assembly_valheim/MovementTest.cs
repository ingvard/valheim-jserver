using System;
using UnityEngine;

// Token: 0x020000E3 RID: 227
public class MovementTest : MonoBehaviour
{
	// Token: 0x06000E31 RID: 3633 RVA: 0x00065678 File Offset: 0x00063878
	private void Start()
	{
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_center = base.transform.position;
	}

	// Token: 0x06000E32 RID: 3634 RVA: 0x00065698 File Offset: 0x00063898
	private void FixedUpdate()
	{
		this.m_timer += Time.fixedDeltaTime;
		float num = 5f;
		Vector3 vector = this.m_center + new Vector3(Mathf.Sin(this.m_timer * this.m_speed) * num, 0f, Mathf.Cos(this.m_timer * this.m_speed) * num);
		this.m_vel = (vector - this.m_body.position) / Time.fixedDeltaTime;
		this.m_body.position = vector;
		this.m_body.velocity = this.m_vel;
	}

	// Token: 0x04000CE9 RID: 3305
	public float m_speed = 10f;

	// Token: 0x04000CEA RID: 3306
	private float m_timer;

	// Token: 0x04000CEB RID: 3307
	private Rigidbody m_body;

	// Token: 0x04000CEC RID: 3308
	private Vector3 m_center;

	// Token: 0x04000CED RID: 3309
	private Vector3 m_vel;
}
