using System;
using UnityEngine;

// Token: 0x02000036 RID: 54
public class EffectFade : MonoBehaviour
{
	// Token: 0x0600041E RID: 1054 RVA: 0x00021684 File Offset: 0x0001F884
	private void Awake()
	{
		this.m_particles = base.gameObject.GetComponentsInChildren<ParticleSystem>();
		this.m_light = base.gameObject.GetComponentInChildren<Light>();
		this.m_audioSource = base.gameObject.GetComponentInChildren<AudioSource>();
		if (this.m_light)
		{
			this.m_lightBaseIntensity = this.m_light.intensity;
			this.m_light.intensity = 0f;
		}
		if (this.m_audioSource)
		{
			this.m_baseVolume = this.m_audioSource.volume;
			this.m_audioSource.volume = 0f;
		}
		this.SetActive(false);
	}

	// Token: 0x0600041F RID: 1055 RVA: 0x00021728 File Offset: 0x0001F928
	private void Update()
	{
		this.m_intensity = Mathf.MoveTowards(this.m_intensity, this.m_active ? 1f : 0f, Time.deltaTime / this.m_fadeDuration);
		if (this.m_light)
		{
			this.m_light.intensity = this.m_intensity * this.m_lightBaseIntensity;
			this.m_light.enabled = (this.m_light.intensity > 0f);
		}
		if (this.m_audioSource)
		{
			this.m_audioSource.volume = this.m_intensity * this.m_baseVolume;
		}
	}

	// Token: 0x06000420 RID: 1056 RVA: 0x000217D0 File Offset: 0x0001F9D0
	public void SetActive(bool active)
	{
		if (this.m_active == active)
		{
			return;
		}
		this.m_active = active;
		ParticleSystem[] particles = this.m_particles;
		for (int i = 0; i < particles.Length; i++)
		{
			particles[i].emission.enabled = active;
		}
	}

	// Token: 0x0400041B RID: 1051
	public float m_fadeDuration = 1f;

	// Token: 0x0400041C RID: 1052
	private ParticleSystem[] m_particles;

	// Token: 0x0400041D RID: 1053
	private Light m_light;

	// Token: 0x0400041E RID: 1054
	private AudioSource m_audioSource;

	// Token: 0x0400041F RID: 1055
	private float m_baseVolume;

	// Token: 0x04000420 RID: 1056
	private float m_lightBaseIntensity;

	// Token: 0x04000421 RID: 1057
	private bool m_active = true;

	// Token: 0x04000422 RID: 1058
	private float m_intensity;
}
