using System;
using UnityEngine;

// Token: 0x0200003D RID: 61
public class LightFlicker : MonoBehaviour
{
	// Token: 0x06000438 RID: 1080 RVA: 0x000224F8 File Offset: 0x000206F8
	private void Awake()
	{
		this.m_light = base.GetComponent<Light>();
		this.m_baseIntensity = this.m_light.intensity;
		this.m_basePosition = base.transform.localPosition;
		this.m_flickerOffset = UnityEngine.Random.Range(0f, 10f);
	}

	// Token: 0x06000439 RID: 1081 RVA: 0x00022548 File Offset: 0x00020748
	private void OnEnable()
	{
		this.m_time = 0f;
		if (this.m_light)
		{
			this.m_light.intensity = 0f;
		}
	}

	// Token: 0x0600043A RID: 1082 RVA: 0x00022574 File Offset: 0x00020774
	private void Update()
	{
		if (!this.m_light)
		{
			return;
		}
		this.m_time += Time.deltaTime;
		float num = this.m_flickerOffset + Time.time * this.m_flickerSpeed;
		float num2 = 1f + Mathf.Sin(num) * Mathf.Sin(num * 0.56436f) * Mathf.Cos(num * 0.758348f) * this.m_flickerIntensity;
		if (this.m_fadeInDuration > 0f)
		{
			num2 *= Utils.LerpStep(0f, this.m_fadeInDuration, this.m_time);
		}
		if (this.m_ttl > 0f)
		{
			if (this.m_time > this.m_ttl)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			float l = this.m_ttl - this.m_fadeDuration;
			num2 *= 1f - Utils.LerpStep(l, this.m_ttl, this.m_time);
		}
		this.m_light.intensity = this.m_baseIntensity * num2;
		Vector3 b = new Vector3(Mathf.Sin(num) * Mathf.Sin(num * 0.56436f), Mathf.Sin(num * 0.56436f) * Mathf.Sin(num * 0.688742f), Mathf.Cos(num * 0.758348f) * Mathf.Cos(num * 0.4563696f)) * this.m_movement;
		base.transform.localPosition = this.m_basePosition + b;
	}

	// Token: 0x04000455 RID: 1109
	public float m_flickerIntensity = 0.1f;

	// Token: 0x04000456 RID: 1110
	public float m_flickerSpeed = 10f;

	// Token: 0x04000457 RID: 1111
	public float m_movement = 0.1f;

	// Token: 0x04000458 RID: 1112
	public float m_ttl;

	// Token: 0x04000459 RID: 1113
	public float m_fadeDuration = 0.2f;

	// Token: 0x0400045A RID: 1114
	public float m_fadeInDuration;

	// Token: 0x0400045B RID: 1115
	private Light m_light;

	// Token: 0x0400045C RID: 1116
	private float m_baseIntensity = 1f;

	// Token: 0x0400045D RID: 1117
	private Vector3 m_basePosition = Vector3.zero;

	// Token: 0x0400045E RID: 1118
	private float m_time;

	// Token: 0x0400045F RID: 1119
	private float m_flickerOffset;
}
