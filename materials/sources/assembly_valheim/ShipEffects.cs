using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000F4 RID: 244
public class ShipEffects : MonoBehaviour
{
	// Token: 0x06000F0C RID: 3852 RVA: 0x0006B7B8 File Offset: 0x000699B8
	private void Awake()
	{
		ZNetView componentInParent = base.GetComponentInParent<ZNetView>();
		if (componentInParent && componentInParent.GetZDO() == null)
		{
			base.enabled = false;
			return;
		}
		this.m_body = base.GetComponentInParent<Rigidbody>();
		this.m_ship = base.GetComponentInParent<Ship>();
		if (this.m_speedWakeRoot)
		{
			this.m_wakeParticles = this.m_speedWakeRoot.GetComponentsInChildren<ParticleSystem>();
		}
		if (this.m_wakeSoundRoot)
		{
			foreach (AudioSource audioSource in this.m_wakeSoundRoot.GetComponentsInChildren<AudioSource>())
			{
				audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
				this.m_wakeSounds.Add(new KeyValuePair<AudioSource, float>(audioSource, audioSource.volume));
			}
		}
		if (this.m_inWaterSoundRoot)
		{
			foreach (AudioSource audioSource2 in this.m_inWaterSoundRoot.GetComponentsInChildren<AudioSource>())
			{
				audioSource2.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
				this.m_inWaterSounds.Add(new KeyValuePair<AudioSource, float>(audioSource2, audioSource2.volume));
			}
		}
		if (this.m_sailSound)
		{
			this.m_sailBaseVol = this.m_sailSound.volume;
			this.m_sailSound.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
		}
	}

	// Token: 0x06000F0D RID: 3853 RVA: 0x0006B904 File Offset: 0x00069B04
	private void LateUpdate()
	{
		float waterLevel = WaterVolume.GetWaterLevel(base.transform.position, 1f);
		ref Vector3 position = base.transform.position;
		float deltaTime = Time.deltaTime;
		if (position.y > waterLevel)
		{
			this.m_shadow.gameObject.SetActive(false);
			this.SetWake(false, deltaTime);
			this.FadeSounds(this.m_inWaterSounds, false, deltaTime);
			return;
		}
		this.m_shadow.gameObject.SetActive(true);
		bool enabled = this.m_body.velocity.magnitude > this.m_minimumWakeVel;
		this.FadeSounds(this.m_inWaterSounds, true, deltaTime);
		this.SetWake(enabled, deltaTime);
		if (this.m_sailSound)
		{
			float target = this.m_ship.IsSailUp() ? this.m_sailBaseVol : 0f;
			this.FadeSound(this.m_sailSound, target, this.m_sailFadeDuration, deltaTime);
		}
		if (this.m_splashEffects != null)
		{
			this.m_splashEffects.SetActive(this.m_ship.HasPlayerOnboard());
		}
	}

	// Token: 0x06000F0E RID: 3854 RVA: 0x0006BA10 File Offset: 0x00069C10
	private void SetWake(bool enabled, float dt)
	{
		ParticleSystem[] wakeParticles = this.m_wakeParticles;
		for (int i = 0; i < wakeParticles.Length; i++)
		{
			wakeParticles[i].emission.enabled = enabled;
		}
		this.FadeSounds(this.m_wakeSounds, enabled, dt);
	}

	// Token: 0x06000F0F RID: 3855 RVA: 0x0006BA54 File Offset: 0x00069C54
	private void FadeSounds(List<KeyValuePair<AudioSource, float>> sources, bool enabled, float dt)
	{
		foreach (KeyValuePair<AudioSource, float> keyValuePair in sources)
		{
			if (enabled)
			{
				this.FadeSound(keyValuePair.Key, keyValuePair.Value, this.m_audioFadeDuration, dt);
			}
			else
			{
				this.FadeSound(keyValuePair.Key, 0f, this.m_audioFadeDuration, dt);
			}
		}
	}

	// Token: 0x06000F10 RID: 3856 RVA: 0x0006BAD4 File Offset: 0x00069CD4
	private void FadeSound(AudioSource source, float target, float fadeDuration, float dt)
	{
		float maxDelta = dt / fadeDuration;
		if (target > 0f)
		{
			if (!source.isPlaying)
			{
				source.Play();
			}
			source.volume = Mathf.MoveTowards(source.volume, target, maxDelta);
			return;
		}
		if (source.isPlaying)
		{
			source.volume = Mathf.MoveTowards(source.volume, 0f, maxDelta);
			if (source.volume <= 0f)
			{
				source.Stop();
			}
		}
	}

	// Token: 0x04000DF4 RID: 3572
	public Transform m_shadow;

	// Token: 0x04000DF5 RID: 3573
	public float m_offset = 0.01f;

	// Token: 0x04000DF6 RID: 3574
	public float m_minimumWakeVel = 5f;

	// Token: 0x04000DF7 RID: 3575
	public GameObject m_speedWakeRoot;

	// Token: 0x04000DF8 RID: 3576
	public GameObject m_wakeSoundRoot;

	// Token: 0x04000DF9 RID: 3577
	public GameObject m_inWaterSoundRoot;

	// Token: 0x04000DFA RID: 3578
	public float m_audioFadeDuration = 2f;

	// Token: 0x04000DFB RID: 3579
	public AudioSource m_sailSound;

	// Token: 0x04000DFC RID: 3580
	public float m_sailFadeDuration = 1f;

	// Token: 0x04000DFD RID: 3581
	public GameObject m_splashEffects;

	// Token: 0x04000DFE RID: 3582
	private float m_sailBaseVol = 1f;

	// Token: 0x04000DFF RID: 3583
	private ParticleSystem[] m_wakeParticles;

	// Token: 0x04000E00 RID: 3584
	private List<KeyValuePair<AudioSource, float>> m_wakeSounds = new List<KeyValuePair<AudioSource, float>>();

	// Token: 0x04000E01 RID: 3585
	private List<KeyValuePair<AudioSource, float>> m_inWaterSounds = new List<KeyValuePair<AudioSource, float>>();

	// Token: 0x04000E02 RID: 3586
	private Rigidbody m_body;

	// Token: 0x04000E03 RID: 3587
	private Ship m_ship;
}
