using System;
using UnityEngine;

// Token: 0x0200004A RID: 74
public class ZSFX : MonoBehaviour
{
	// Token: 0x0600049C RID: 1180 RVA: 0x00024F2D File Offset: 0x0002312D
	public void Awake()
	{
		this.m_delay = UnityEngine.Random.Range(this.m_minDelay, this.m_maxDelay);
		this.m_audioSource = base.GetComponent<AudioSource>();
		this.m_baseSpread = this.m_audioSource.spread;
	}

	// Token: 0x0600049D RID: 1181 RVA: 0x00024F64 File Offset: 0x00023164
	private void OnDisable()
	{
		if (this.m_playOnAwake && this.m_audioSource.loop)
		{
			this.m_time = 0f;
			this.m_delay = UnityEngine.Random.Range(this.m_minDelay, this.m_maxDelay);
			this.m_audioSource.Stop();
		}
	}

	// Token: 0x0600049E RID: 1182 RVA: 0x00024FB4 File Offset: 0x000231B4
	public void Update()
	{
		if (this.m_audioSource == null)
		{
			return;
		}
		this.m_time += Time.deltaTime;
		if (this.m_delay >= 0f && this.m_time >= this.m_delay)
		{
			this.m_delay = -1f;
			if (this.m_playOnAwake)
			{
				this.Play();
			}
		}
		if (this.m_audioSource.isPlaying)
		{
			if (this.m_distanceReverb && this.m_audioSource.loop)
			{
				this.m_updateReverbTimer += Time.deltaTime;
				if (this.m_updateReverbTimer > 1f)
				{
					this.m_updateReverbTimer = 0f;
					this.UpdateReverb();
				}
			}
			if (this.m_fadeOutOnAwake && this.m_time > this.m_fadeOutDelay)
			{
				this.m_fadeOutOnAwake = false;
				this.FadeOut();
			}
			if (this.m_fadeOutTimer >= 0f)
			{
				this.m_fadeOutTimer += Time.deltaTime;
				if (this.m_fadeOutTimer >= this.m_fadeOutDuration)
				{
					this.m_audioSource.volume = 0f;
					this.Stop();
					return;
				}
				float num = Mathf.Clamp01(this.m_fadeOutTimer / this.m_fadeOutDuration);
				this.m_audioSource.volume = (1f - num) * this.m_vol;
				return;
			}
			else if (this.m_fadeInTimer >= 0f)
			{
				this.m_fadeInTimer += Time.deltaTime;
				float num2 = Mathf.Clamp01(this.m_fadeInTimer / this.m_fadeInDuration);
				this.m_audioSource.volume = num2 * this.m_vol;
				if (this.m_fadeInTimer > this.m_fadeInDuration)
				{
					this.m_fadeInTimer = -1f;
				}
			}
		}
	}

	// Token: 0x0600049F RID: 1183 RVA: 0x0002515D File Offset: 0x0002335D
	public void FadeOut()
	{
		if (this.m_fadeOutTimer < 0f)
		{
			this.m_fadeOutTimer = 0f;
		}
	}

	// Token: 0x060004A0 RID: 1184 RVA: 0x00025177 File Offset: 0x00023377
	public void Stop()
	{
		if (this.m_audioSource != null)
		{
			this.m_audioSource.Stop();
		}
	}

	// Token: 0x060004A1 RID: 1185 RVA: 0x00025192 File Offset: 0x00023392
	public bool IsPlaying()
	{
		return !(this.m_audioSource == null) && this.m_audioSource.isPlaying;
	}

	// Token: 0x060004A2 RID: 1186 RVA: 0x000251B0 File Offset: 0x000233B0
	private void UpdateReverb()
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (this.m_distanceReverb && this.m_audioSource.spatialBlend != 0f && mainCamera != null)
		{
			float num = Vector3.Distance(mainCamera.transform.position, base.transform.position);
			float num2 = this.m_useCustomReverbDistance ? this.m_customReverbDistance : 64f;
			float a = Mathf.Clamp01(num / num2);
			float b = Mathf.Clamp01(this.m_audioSource.maxDistance / num2) * Mathf.Clamp01(num / this.m_audioSource.maxDistance);
			float num3 = Mathf.Max(a, b);
			this.m_audioSource.bypassReverbZones = false;
			this.m_audioSource.reverbZoneMix = num3;
			if (this.m_baseSpread < 120f)
			{
				float a2 = Mathf.Max(this.m_baseSpread, 45f);
				this.m_audioSource.spread = Mathf.Lerp(a2, 120f, num3);
				return;
			}
		}
		else
		{
			this.m_audioSource.bypassReverbZones = true;
		}
	}

	// Token: 0x060004A3 RID: 1187 RVA: 0x000252B8 File Offset: 0x000234B8
	public void Play()
	{
		if (this.m_audioSource == null)
		{
			return;
		}
		if (this.m_audioClips.Length == 0)
		{
			return;
		}
		if (!this.m_audioSource.gameObject.activeInHierarchy)
		{
			return;
		}
		int num = UnityEngine.Random.Range(0, this.m_audioClips.Length);
		this.m_audioSource.clip = this.m_audioClips[num];
		this.m_audioSource.pitch = UnityEngine.Random.Range(this.m_minPitch, this.m_maxPitch);
		if (this.m_randomPan)
		{
			this.m_audioSource.panStereo = UnityEngine.Random.Range(this.m_minPan, this.m_maxPan);
		}
		this.m_vol = UnityEngine.Random.Range(this.m_minVol, this.m_maxVol);
		if (this.m_fadeInDuration > 0f)
		{
			this.m_audioSource.volume = 0f;
			this.m_fadeInTimer = 0f;
		}
		else
		{
			this.m_audioSource.volume = this.m_vol;
		}
		this.UpdateReverb();
		this.m_audioSource.Play();
	}

	// Token: 0x040004C5 RID: 1221
	public bool m_playOnAwake = true;

	// Token: 0x040004C6 RID: 1222
	[Header("Clips")]
	public AudioClip[] m_audioClips = new AudioClip[0];

	// Token: 0x040004C7 RID: 1223
	[Header("Random")]
	public float m_maxPitch = 1f;

	// Token: 0x040004C8 RID: 1224
	public float m_minPitch = 1f;

	// Token: 0x040004C9 RID: 1225
	public float m_maxVol = 1f;

	// Token: 0x040004CA RID: 1226
	public float m_minVol = 1f;

	// Token: 0x040004CB RID: 1227
	[Header("Fade")]
	public float m_fadeInDuration;

	// Token: 0x040004CC RID: 1228
	public float m_fadeOutDuration;

	// Token: 0x040004CD RID: 1229
	public float m_fadeOutDelay;

	// Token: 0x040004CE RID: 1230
	public bool m_fadeOutOnAwake;

	// Token: 0x040004CF RID: 1231
	[Header("Pan")]
	public bool m_randomPan;

	// Token: 0x040004D0 RID: 1232
	public float m_minPan = -1f;

	// Token: 0x040004D1 RID: 1233
	public float m_maxPan = 1f;

	// Token: 0x040004D2 RID: 1234
	[Header("Delay")]
	public float m_maxDelay;

	// Token: 0x040004D3 RID: 1235
	public float m_minDelay;

	// Token: 0x040004D4 RID: 1236
	[Header("Reverb")]
	public bool m_distanceReverb = true;

	// Token: 0x040004D5 RID: 1237
	public bool m_useCustomReverbDistance;

	// Token: 0x040004D6 RID: 1238
	public float m_customReverbDistance = 10f;

	// Token: 0x040004D7 RID: 1239
	private const float m_globalReverbDistance = 64f;

	// Token: 0x040004D8 RID: 1240
	private const float m_minReverbSpread = 45f;

	// Token: 0x040004D9 RID: 1241
	private const float m_maxReverbSpread = 120f;

	// Token: 0x040004DA RID: 1242
	private float m_delay;

	// Token: 0x040004DB RID: 1243
	private float m_time;

	// Token: 0x040004DC RID: 1244
	private float m_fadeOutTimer = -1f;

	// Token: 0x040004DD RID: 1245
	private float m_fadeInTimer = -1f;

	// Token: 0x040004DE RID: 1246
	private float m_vol = 1f;

	// Token: 0x040004DF RID: 1247
	private float m_baseSpread;

	// Token: 0x040004E0 RID: 1248
	private float m_updateReverbTimer;

	// Token: 0x040004E1 RID: 1249
	private AudioSource m_audioSource;
}
