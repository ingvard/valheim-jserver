using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

// Token: 0x02000095 RID: 149
public class AudioMan : MonoBehaviour
{
	// Token: 0x17000022 RID: 34
	// (get) Token: 0x060009F1 RID: 2545 RVA: 0x00047F46 File Offset: 0x00046146
	public static AudioMan instance
	{
		get
		{
			return AudioMan.m_instance;
		}
	}

	// Token: 0x060009F2 RID: 2546 RVA: 0x00047F50 File Offset: 0x00046150
	private void Awake()
	{
		if (AudioMan.m_instance != null)
		{
			ZLog.Log("Audioman already exist, destroying self");
			UnityEngine.Object.DestroyImmediate(base.gameObject);
			return;
		}
		AudioMan.m_instance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		GameObject gameObject = new GameObject("ocean_ambient_loop");
		gameObject.transform.SetParent(base.transform);
		this.m_oceanAmbientSource = gameObject.AddComponent<AudioSource>();
		this.m_oceanAmbientSource.loop = true;
		this.m_oceanAmbientSource.spatialBlend = 0.75f;
		this.m_oceanAmbientSource.outputAudioMixerGroup = this.m_ambientMixer;
		this.m_oceanAmbientSource.maxDistance = 128f;
		this.m_oceanAmbientSource.minDistance = 40f;
		this.m_oceanAmbientSource.spread = 90f;
		this.m_oceanAmbientSource.rolloffMode = AudioRolloffMode.Linear;
		this.m_oceanAmbientSource.clip = this.m_oceanAudio;
		this.m_oceanAmbientSource.bypassReverbZones = true;
		this.m_oceanAmbientSource.dopplerLevel = 0f;
		this.m_oceanAmbientSource.volume = 0f;
		this.m_oceanAmbientSource.Play();
		GameObject gameObject2 = new GameObject("ambient_loop");
		gameObject2.transform.SetParent(base.transform);
		this.m_ambientLoopSource = gameObject2.AddComponent<AudioSource>();
		this.m_ambientLoopSource.loop = true;
		this.m_ambientLoopSource.spatialBlend = 0f;
		this.m_ambientLoopSource.outputAudioMixerGroup = this.m_ambientMixer;
		this.m_ambientLoopSource.bypassReverbZones = true;
		this.m_ambientLoopSource.volume = 0f;
		GameObject gameObject3 = new GameObject("wind_loop");
		gameObject3.transform.SetParent(base.transform);
		this.m_windLoopSource = gameObject3.AddComponent<AudioSource>();
		this.m_windLoopSource.loop = true;
		this.m_windLoopSource.spatialBlend = 0f;
		this.m_windLoopSource.outputAudioMixerGroup = this.m_ambientMixer;
		this.m_windLoopSource.bypassReverbZones = true;
		this.m_windLoopSource.clip = this.m_windAudio;
		this.m_windLoopSource.volume = 0f;
		this.m_windLoopSource.Play();
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
		{
			AudioListener.volume = 0f;
			return;
		}
		AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", AudioListener.volume);
		AudioMan.SetSFXVolume(PlayerPrefs.GetFloat("SfxVolume", AudioMan.GetSFXVolume()));
	}

	// Token: 0x060009F3 RID: 2547 RVA: 0x000481A3 File Offset: 0x000463A3
	private void OnDestroy()
	{
		if (AudioMan.m_instance == this)
		{
			AudioMan.m_instance = null;
		}
	}

	// Token: 0x060009F4 RID: 2548 RVA: 0x000481B8 File Offset: 0x000463B8
	private void Update()
	{
		float deltaTime = Time.deltaTime;
		this.UpdateAmbientLoop(deltaTime);
		this.UpdateRandomAmbient(deltaTime);
		this.UpdateSnapshots(deltaTime);
	}

	// Token: 0x060009F5 RID: 2549 RVA: 0x000481E0 File Offset: 0x000463E0
	private void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		this.UpdateOceanAmbiance(fixedDeltaTime);
		this.UpdateWindAmbience(fixedDeltaTime);
	}

	// Token: 0x060009F6 RID: 2550 RVA: 0x00048204 File Offset: 0x00046404
	public static float GetSFXVolume()
	{
		if (AudioMan.m_instance == null)
		{
			return 1f;
		}
		float num;
		AudioMan.m_instance.m_masterMixer.GetFloat("SfxVol", out num);
		return Mathf.Pow(10f, num / 20f);
	}

	// Token: 0x060009F7 RID: 2551 RVA: 0x0004824C File Offset: 0x0004644C
	public static void SetSFXVolume(float vol)
	{
		if (AudioMan.m_instance == null)
		{
			return;
		}
		float value = Mathf.Log(Mathf.Clamp(vol, 0.001f, 1f)) * 10f;
		AudioMan.m_instance.m_masterMixer.SetFloat("SfxVol", value);
	}

	// Token: 0x060009F8 RID: 2552 RVA: 0x0004829C File Offset: 0x0004649C
	private void UpdateRandomAmbient(float dt)
	{
		if (this.InMenu())
		{
			return;
		}
		this.m_randomAmbientTimer += dt;
		if (this.m_randomAmbientTimer > this.m_randomAmbientInterval)
		{
			this.m_randomAmbientTimer = 0f;
			if (UnityEngine.Random.value <= this.m_randomAmbientChance)
			{
				AudioClip audioClip = this.SelectRandomAmbientClip();
				if (audioClip)
				{
					Vector3 randomAmbiencePoint = this.GetRandomAmbiencePoint();
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_randomAmbientPrefab, randomAmbiencePoint, Quaternion.identity, base.transform);
					gameObject.GetComponent<AudioSource>().pitch = UnityEngine.Random.Range(this.m_randomMinPitch, this.m_randomMaxPitch);
					ZSFX component = gameObject.GetComponent<ZSFX>();
					component.m_audioClips = new AudioClip[]
					{
						audioClip
					};
					component.Play();
					component.FadeOut();
				}
			}
		}
	}

	// Token: 0x060009F9 RID: 2553 RVA: 0x00048354 File Offset: 0x00046554
	private Vector3 GetRandomAmbiencePoint()
	{
		Vector3 a = Vector3.zero;
		Camera mainCamera = Utils.GetMainCamera();
		if (Player.m_localPlayer)
		{
			a = Player.m_localPlayer.transform.position;
		}
		else if (mainCamera)
		{
			a = mainCamera.transform.position;
		}
		float f = UnityEngine.Random.value * 3.1415927f * 2f;
		float num = UnityEngine.Random.Range(this.m_randomMinDistance, this.m_randomMaxDistance);
		return a + new Vector3(Mathf.Sin(f) * num, 0f, Mathf.Cos(f) * num);
	}

	// Token: 0x060009FA RID: 2554 RVA: 0x000483E4 File Offset: 0x000465E4
	private AudioClip SelectRandomAmbientClip()
	{
		if (EnvMan.instance == null)
		{
			return null;
		}
		EnvSetup currentEnvironment = EnvMan.instance.GetCurrentEnvironment();
		AudioMan.BiomeAmbients biomeAmbients;
		if (currentEnvironment != null && !string.IsNullOrEmpty(currentEnvironment.m_ambientList))
		{
			biomeAmbients = this.GetAmbients(currentEnvironment.m_ambientList);
		}
		else
		{
			biomeAmbients = this.GetBiomeAmbients(EnvMan.instance.GetCurrentBiome());
		}
		if (biomeAmbients == null)
		{
			return null;
		}
		List<AudioClip> list = new List<AudioClip>(biomeAmbients.m_randomAmbientClips);
		List<AudioClip> collection = EnvMan.instance.IsDaylight() ? biomeAmbients.m_randomAmbientClipsDay : biomeAmbients.m_randomAmbientClipsNight;
		list.AddRange(collection);
		if (list.Count == 0)
		{
			return null;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	// Token: 0x060009FB RID: 2555 RVA: 0x0004848C File Offset: 0x0004668C
	private void UpdateAmbientLoop(float dt)
	{
		if (EnvMan.instance == null)
		{
			this.m_ambientLoopSource.Stop();
			return;
		}
		if (this.m_queuedAmbientLoop || this.m_stopAmbientLoop)
		{
			if (this.m_ambientLoopSource.isPlaying && this.m_ambientLoopSource.volume > 0f)
			{
				this.m_ambientLoopSource.volume = Mathf.MoveTowards(this.m_ambientLoopSource.volume, 0f, dt / this.m_ambientFadeTime);
				return;
			}
			this.m_ambientLoopSource.Stop();
			this.m_stopAmbientLoop = false;
			if (this.m_queuedAmbientLoop)
			{
				this.m_ambientLoopSource.clip = this.m_queuedAmbientLoop;
				this.m_ambientLoopSource.volume = 0f;
				this.m_ambientLoopSource.Play();
				this.m_ambientVol = this.m_queuedAmbientVol;
				this.m_queuedAmbientLoop = null;
				return;
			}
		}
		else if (this.m_ambientLoopSource.isPlaying)
		{
			this.m_ambientLoopSource.volume = Mathf.MoveTowards(this.m_ambientLoopSource.volume, this.m_ambientVol, dt / this.m_ambientFadeTime);
		}
	}

	// Token: 0x060009FC RID: 2556 RVA: 0x000485AA File Offset: 0x000467AA
	public void SetIndoor(bool indoor)
	{
		this.m_indoor = indoor;
	}

	// Token: 0x060009FD RID: 2557 RVA: 0x000485B3 File Offset: 0x000467B3
	private bool InMenu()
	{
		return FejdStartup.instance != null || Menu.IsVisible() || (Game.instance && Game.instance.WaitingForRespawn()) || TextViewer.IsShowingIntro();
	}

	// Token: 0x060009FE RID: 2558 RVA: 0x000485E8 File Offset: 0x000467E8
	private void UpdateSnapshots(float dt)
	{
		if (this.InMenu())
		{
			this.SetSnapshot(AudioMan.Snapshot.Menu);
			return;
		}
		if (this.m_indoor)
		{
			this.SetSnapshot(AudioMan.Snapshot.Indoor);
			return;
		}
		this.SetSnapshot(AudioMan.Snapshot.Default);
	}

	// Token: 0x060009FF RID: 2559 RVA: 0x00048614 File Offset: 0x00046814
	private void SetSnapshot(AudioMan.Snapshot snapshot)
	{
		if (this.m_currentSnapshot == snapshot)
		{
			return;
		}
		this.m_currentSnapshot = snapshot;
		switch (snapshot)
		{
		case AudioMan.Snapshot.Default:
			this.m_masterMixer.FindSnapshot("Default").TransitionTo(this.m_snapshotTransitionTime);
			return;
		case AudioMan.Snapshot.Menu:
			this.m_masterMixer.FindSnapshot("Menu").TransitionTo(this.m_snapshotTransitionTime);
			return;
		case AudioMan.Snapshot.Indoor:
			this.m_masterMixer.FindSnapshot("Indoor").TransitionTo(this.m_snapshotTransitionTime);
			return;
		default:
			return;
		}
	}

	// Token: 0x06000A00 RID: 2560 RVA: 0x00048698 File Offset: 0x00046898
	public void StopAmbientLoop()
	{
		this.m_queuedAmbientLoop = null;
		this.m_stopAmbientLoop = true;
	}

	// Token: 0x06000A01 RID: 2561 RVA: 0x000486A8 File Offset: 0x000468A8
	public void QueueAmbientLoop(AudioClip clip, float vol)
	{
		if (this.m_queuedAmbientLoop == clip && this.m_queuedAmbientVol == vol)
		{
			return;
		}
		if (this.m_queuedAmbientLoop == null && this.m_ambientLoopSource.clip == clip && this.m_ambientVol == vol)
		{
			return;
		}
		this.m_queuedAmbientLoop = clip;
		this.m_queuedAmbientVol = vol;
		this.m_stopAmbientLoop = false;
	}

	// Token: 0x06000A02 RID: 2562 RVA: 0x00048710 File Offset: 0x00046910
	private void UpdateWindAmbience(float dt)
	{
		if (ZoneSystem.instance == null)
		{
			this.m_windLoopSource.volume = 0f;
			return;
		}
		float num = EnvMan.instance.GetWindIntensity();
		num = Mathf.Pow(num, this.m_windIntensityPower);
		num += num * Mathf.Sin(Time.time) * Mathf.Sin(Time.time * 1.54323f) * Mathf.Sin(Time.time * 2.31237f) * this.m_windVariation;
		this.m_windLoopSource.volume = Mathf.Lerp(this.m_windMinVol, this.m_windMaxVol, num);
		this.m_windLoopSource.pitch = Mathf.Lerp(this.m_windMinPitch, this.m_windMaxPitch, num);
	}

	// Token: 0x06000A03 RID: 2563 RVA: 0x000487C8 File Offset: 0x000469C8
	private void UpdateOceanAmbiance(float dt)
	{
		if (ZoneSystem.instance == null)
		{
			this.m_oceanAmbientSource.volume = 0f;
			return;
		}
		this.m_oceanUpdateTimer += dt;
		if (this.m_oceanUpdateTimer > 2f)
		{
			this.m_oceanUpdateTimer = 0f;
			this.m_haveOcean = this.FindAverageOceanPoint(out this.m_avgOceanPoint);
		}
		if (this.m_haveOcean)
		{
			float windIntensity = EnvMan.instance.GetWindIntensity();
			float target = Mathf.Lerp(this.m_oceanVolumeMin, this.m_oceanVolumeMax, windIntensity);
			this.m_oceanAmbientSource.volume = Mathf.MoveTowards(this.m_oceanAmbientSource.volume, target, this.m_oceanFadeSpeed * dt);
			this.m_oceanAmbientSource.transform.position = Vector3.Lerp(this.m_oceanAmbientSource.transform.position, this.m_avgOceanPoint, this.m_oceanMoveSpeed);
			return;
		}
		this.m_oceanAmbientSource.volume = Mathf.MoveTowards(this.m_oceanAmbientSource.volume, 0f, this.m_oceanFadeSpeed * dt);
	}

	// Token: 0x06000A04 RID: 2564 RVA: 0x000488D0 File Offset: 0x00046AD0
	private bool FindAverageOceanPoint(out Vector3 point)
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			point = Vector3.zero;
			return false;
		}
		Vector3 vector = Vector3.zero;
		int num = 0;
		Vector3 position = mainCamera.transform.position;
		Vector2i zone = ZoneSystem.instance.GetZone(position);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				Vector2i id = zone;
				id.x += j;
				id.y += i;
				Vector3 zonePos = ZoneSystem.instance.GetZonePos(id);
				if (this.IsOceanZone(zonePos))
				{
					num++;
					vector += zonePos;
				}
			}
		}
		if (num > 0)
		{
			vector /= (float)num;
			point = vector;
			point.y = ZoneSystem.instance.m_waterLevel;
			return true;
		}
		point = Vector3.zero;
		return false;
	}

	// Token: 0x06000A05 RID: 2565 RVA: 0x000489B0 File Offset: 0x00046BB0
	private bool IsOceanZone(Vector3 centerPos)
	{
		float groundHeight = ZoneSystem.instance.GetGroundHeight(centerPos);
		return ZoneSystem.instance.m_waterLevel - groundHeight > this.m_oceanDepthTreshold;
	}

	// Token: 0x06000A06 RID: 2566 RVA: 0x000489E0 File Offset: 0x00046BE0
	private AudioMan.BiomeAmbients GetAmbients(string name)
	{
		foreach (AudioMan.BiomeAmbients biomeAmbients in this.m_randomAmbients)
		{
			if (biomeAmbients.m_name == name)
			{
				return biomeAmbients;
			}
		}
		return null;
	}

	// Token: 0x06000A07 RID: 2567 RVA: 0x00048A44 File Offset: 0x00046C44
	private AudioMan.BiomeAmbients GetBiomeAmbients(Heightmap.Biome biome)
	{
		foreach (AudioMan.BiomeAmbients biomeAmbients in this.m_randomAmbients)
		{
			if ((biomeAmbients.m_biome & biome) != Heightmap.Biome.None)
			{
				return biomeAmbients;
			}
		}
		return null;
	}

	// Token: 0x04000905 RID: 2309
	private static AudioMan m_instance;

	// Token: 0x04000906 RID: 2310
	[Header("Mixers")]
	public AudioMixerGroup m_ambientMixer;

	// Token: 0x04000907 RID: 2311
	public AudioMixer m_masterMixer;

	// Token: 0x04000908 RID: 2312
	public float m_snapshotTransitionTime = 2f;

	// Token: 0x04000909 RID: 2313
	[Header("Wind")]
	public AudioClip m_windAudio;

	// Token: 0x0400090A RID: 2314
	public float m_windMinVol;

	// Token: 0x0400090B RID: 2315
	public float m_windMaxVol = 1f;

	// Token: 0x0400090C RID: 2316
	public float m_windMinPitch = 0.5f;

	// Token: 0x0400090D RID: 2317
	public float m_windMaxPitch = 1.5f;

	// Token: 0x0400090E RID: 2318
	public float m_windVariation = 0.2f;

	// Token: 0x0400090F RID: 2319
	public float m_windIntensityPower = 1.5f;

	// Token: 0x04000910 RID: 2320
	[Header("Ocean")]
	public AudioClip m_oceanAudio;

	// Token: 0x04000911 RID: 2321
	public float m_oceanVolumeMax = 1f;

	// Token: 0x04000912 RID: 2322
	public float m_oceanVolumeMin = 1f;

	// Token: 0x04000913 RID: 2323
	public float m_oceanFadeSpeed = 0.1f;

	// Token: 0x04000914 RID: 2324
	public float m_oceanMoveSpeed = 0.1f;

	// Token: 0x04000915 RID: 2325
	public float m_oceanDepthTreshold = 10f;

	// Token: 0x04000916 RID: 2326
	[Header("Random ambients")]
	public float m_ambientFadeTime = 2f;

	// Token: 0x04000917 RID: 2327
	public float m_randomAmbientInterval = 5f;

	// Token: 0x04000918 RID: 2328
	public float m_randomAmbientChance = 0.5f;

	// Token: 0x04000919 RID: 2329
	public float m_randomMinPitch = 0.9f;

	// Token: 0x0400091A RID: 2330
	public float m_randomMaxPitch = 1.1f;

	// Token: 0x0400091B RID: 2331
	public float m_randomMinVol = 0.2f;

	// Token: 0x0400091C RID: 2332
	public float m_randomMaxVol = 0.4f;

	// Token: 0x0400091D RID: 2333
	public float m_randomPan = 0.2f;

	// Token: 0x0400091E RID: 2334
	public float m_randomFadeIn = 0.2f;

	// Token: 0x0400091F RID: 2335
	public float m_randomFadeOut = 2f;

	// Token: 0x04000920 RID: 2336
	public float m_randomMinDistance = 5f;

	// Token: 0x04000921 RID: 2337
	public float m_randomMaxDistance = 20f;

	// Token: 0x04000922 RID: 2338
	public List<AudioMan.BiomeAmbients> m_randomAmbients = new List<AudioMan.BiomeAmbients>();

	// Token: 0x04000923 RID: 2339
	public GameObject m_randomAmbientPrefab;

	// Token: 0x04000924 RID: 2340
	private AudioSource m_oceanAmbientSource;

	// Token: 0x04000925 RID: 2341
	private AudioSource m_ambientLoopSource;

	// Token: 0x04000926 RID: 2342
	private AudioSource m_windLoopSource;

	// Token: 0x04000927 RID: 2343
	private AudioClip m_queuedAmbientLoop;

	// Token: 0x04000928 RID: 2344
	private float m_queuedAmbientVol;

	// Token: 0x04000929 RID: 2345
	private float m_ambientVol;

	// Token: 0x0400092A RID: 2346
	private float m_randomAmbientTimer;

	// Token: 0x0400092B RID: 2347
	private bool m_stopAmbientLoop;

	// Token: 0x0400092C RID: 2348
	private bool m_indoor;

	// Token: 0x0400092D RID: 2349
	private float m_oceanUpdateTimer;

	// Token: 0x0400092E RID: 2350
	private bool m_haveOcean;

	// Token: 0x0400092F RID: 2351
	private Vector3 m_avgOceanPoint = Vector3.zero;

	// Token: 0x04000930 RID: 2352
	private AudioMan.Snapshot m_currentSnapshot;

	// Token: 0x02000177 RID: 375
	[Serializable]
	public class BiomeAmbients
	{
		// Token: 0x0400118F RID: 4495
		public string m_name = "";

		// Token: 0x04001190 RID: 4496
		[BitMask(typeof(Heightmap.Biome))]
		public Heightmap.Biome m_biome;

		// Token: 0x04001191 RID: 4497
		public List<AudioClip> m_randomAmbientClips = new List<AudioClip>();

		// Token: 0x04001192 RID: 4498
		public List<AudioClip> m_randomAmbientClipsDay = new List<AudioClip>();

		// Token: 0x04001193 RID: 4499
		public List<AudioClip> m_randomAmbientClipsNight = new List<AudioClip>();
	}

	// Token: 0x02000178 RID: 376
	private enum Snapshot
	{
		// Token: 0x04001195 RID: 4501
		Default,
		// Token: 0x04001196 RID: 4502
		Menu,
		// Token: 0x04001197 RID: 4503
		Indoor
	}
}
