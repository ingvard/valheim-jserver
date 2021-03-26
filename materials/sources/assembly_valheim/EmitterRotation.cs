using System;
using UnityEngine;

// Token: 0x02000038 RID: 56
public class EmitterRotation : MonoBehaviour
{
	// Token: 0x06000424 RID: 1060 RVA: 0x000218F6 File Offset: 0x0001FAF6
	private void Start()
	{
		this.m_lastPos = base.transform.position;
		this.m_ps = base.GetComponentInChildren<ParticleSystem>();
	}

	// Token: 0x06000425 RID: 1061 RVA: 0x00021918 File Offset: 0x0001FB18
	private void Update()
	{
		if (!this.m_ps.emission.enabled)
		{
			return;
		}
		Vector3 position = base.transform.position;
		Vector3 vector = position - this.m_lastPos;
		this.m_lastPos = position;
		float t = Mathf.Clamp01(vector.magnitude / Time.deltaTime / this.m_maxSpeed);
		if (vector == Vector3.zero)
		{
			vector = Vector3.up;
		}
		Quaternion a = Quaternion.LookRotation(Vector3.up);
		Quaternion b = Quaternion.LookRotation(vector);
		Quaternion to = Quaternion.Lerp(a, b, t);
		base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, Time.deltaTime * this.m_rotSpeed);
	}

	// Token: 0x04000420 RID: 1056
	public float m_maxSpeed = 10f;

	// Token: 0x04000421 RID: 1057
	public float m_rotSpeed = 90f;

	// Token: 0x04000422 RID: 1058
	private Vector3 m_lastPos;

	// Token: 0x04000423 RID: 1059
	private ParticleSystem m_ps;
}
