using System;
using UnityEngine;

// Token: 0x02000038 RID: 56
public class EmitterRotation : MonoBehaviour
{
	// Token: 0x06000425 RID: 1061 RVA: 0x000219AA File Offset: 0x0001FBAA
	private void Start()
	{
		this.m_lastPos = base.transform.position;
		this.m_ps = base.GetComponentInChildren<ParticleSystem>();
	}

	// Token: 0x06000426 RID: 1062 RVA: 0x000219CC File Offset: 0x0001FBCC
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

	// Token: 0x04000424 RID: 1060
	public float m_maxSpeed = 10f;

	// Token: 0x04000425 RID: 1061
	public float m_rotSpeed = 90f;

	// Token: 0x04000426 RID: 1062
	private Vector3 m_lastPos;

	// Token: 0x04000427 RID: 1063
	private ParticleSystem m_ps;
}
