using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// Token: 0x020000A3 RID: 163
public class MusicMan : MonoBehaviour
{
	// Token: 0x17000028 RID: 40
	// (get) Token: 0x06000B0A RID: 2826 RVA: 0x0004F85C File Offset: 0x0004DA5C
	public static MusicMan instance
	{
		get
		{
			return MusicMan.m_instance;
		}
	}

	// Token: 0x06000B0B RID: 2827 RVA: 0x0004F864 File Offset: 0x0004DA64
	private void Awake()
	{
		if (MusicMan.m_instance)
		{
			return;
		}
		MusicMan.m_instance = this;
		GameObject gameObject = new GameObject("music");
		gameObject.transform.SetParent(base.transform);
		this.m_musicSource = gameObject.AddComponent<AudioSource>();
		this.m_musicSource.loop = true;
		this.m_musicSource.spatialBlend = 0f;
		this.m_musicSource.outputAudioMixerGroup = this.m_musicMixer;
		this.m_musicSource.bypassReverbZones = true;
		this.m_randomAmbientInterval = UnityEngine.Random.Range(this.m_randomMusicIntervalMin, this.m_randomMusicIntervalMax);
		MusicMan.m_masterMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
		this.ApplySettings();
		foreach (MusicMan.NamedMusic namedMusic in this.m_music)
		{
			foreach (AudioClip audioClip in namedMusic.m_clips)
			{
				if (audioClip == null || !audioClip)
				{
					namedMusic.m_enabled = false;
					ZLog.LogWarning("Missing audio clip in music " + namedMusic.m_name);
					break;
				}
			}
		}
	}

	// Token: 0x06000B0C RID: 2828 RVA: 0x0004F9A8 File Offset: 0x0004DBA8
	public void ApplySettings()
	{
		bool flag = PlayerPrefs.GetInt("ContinousMusic", 1) == 1;
		foreach (MusicMan.NamedMusic namedMusic in this.m_music)
		{
			if (namedMusic.m_ambientMusic)
			{
				namedMusic.m_loop = flag;
				if (!flag && this.GetCurrentMusic() == namedMusic.m_name && this.m_musicSource.loop)
				{
					this.StopMusic();
				}
			}
		}
	}

	// Token: 0x06000B0D RID: 2829 RVA: 0x0004FA40 File Offset: 0x0004DC40
	private void OnDestroy()
	{
		if (MusicMan.m_instance == this)
		{
			MusicMan.m_instance = null;
		}
	}

	// Token: 0x06000B0E RID: 2830 RVA: 0x0004FA58 File Offset: 0x0004DC58
	private void Update()
	{
		if (MusicMan.m_instance != this)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		this.UpdateCurrentMusic(deltaTime);
		this.UpdateCombatMusic(deltaTime);
		this.UpdateMusic(deltaTime);
	}

	// Token: 0x06000B0F RID: 2831 RVA: 0x0004FA90 File Offset: 0x0004DC90
	private void UpdateCurrentMusic(float dt)
	{
		string currentMusic = this.GetCurrentMusic();
		if (Game.instance != null)
		{
			if (Player.m_localPlayer == null)
			{
				this.StartMusic("respawn");
				return;
			}
			if (currentMusic == "respawn")
			{
				this.StopMusic();
			}
		}
		if (Player.m_localPlayer && Player.m_localPlayer.InIntro())
		{
			this.StartMusic("intro");
			return;
		}
		if (currentMusic == "intro")
		{
			this.StopMusic();
		}
		if (this.HandleEventMusic(currentMusic))
		{
			return;
		}
		if (this.HandleSailingMusic(dt, currentMusic))
		{
			return;
		}
		if (this.HandleTriggerMusic(currentMusic))
		{
			return;
		}
		this.HandleEnvironmentMusic(dt, currentMusic);
	}

	// Token: 0x06000B10 RID: 2832 RVA: 0x0004FB3C File Offset: 0x0004DD3C
	private bool HandleEnvironmentMusic(float dt, string currentMusic)
	{
		if (!EnvMan.instance)
		{
			return false;
		}
		MusicMan.NamedMusic environmentMusic = this.GetEnvironmentMusic();
		if (environmentMusic == null || (!environmentMusic.m_loop && environmentMusic.m_name != this.GetCurrentMusic()))
		{
			this.StopMusic();
			return true;
		}
		if (!environmentMusic.m_loop)
		{
			if (Time.time - this.m_lastAmbientMusicTime < this.m_randomAmbientInterval)
			{
				return false;
			}
			this.m_randomAmbientInterval = UnityEngine.Random.Range(this.m_randomMusicIntervalMin, this.m_randomMusicIntervalMax);
			this.m_lastAmbientMusicTime = Time.time;
		}
		this.StartMusic(environmentMusic);
		return true;
	}

	// Token: 0x06000B11 RID: 2833 RVA: 0x0004FBCC File Offset: 0x0004DDCC
	private MusicMan.NamedMusic GetEnvironmentMusic()
	{
		string name;
		if (Player.m_localPlayer && Player.m_localPlayer.IsSafeInHome())
		{
			name = "home";
		}
		else
		{
			name = EnvMan.instance.GetAmbientMusic();
		}
		return this.FindMusic(name);
	}

	// Token: 0x06000B12 RID: 2834 RVA: 0x0004FC10 File Offset: 0x0004DE10
	private bool HandleTriggerMusic(string currentMusic)
	{
		if (this.m_triggerMusic != null)
		{
			this.StartMusic(this.m_triggerMusic);
			this.m_triggeredMusic = this.m_triggerMusic;
			this.m_triggerMusic = null;
			return true;
		}
		if (this.m_triggeredMusic != null)
		{
			if (currentMusic == this.m_triggeredMusic)
			{
				return true;
			}
			this.m_triggeredMusic = null;
		}
		return false;
	}

	// Token: 0x06000B13 RID: 2835 RVA: 0x0004FC68 File Offset: 0x0004DE68
	private bool HandleEventMusic(string currentMusic)
	{
		if (RandEventSystem.instance)
		{
			string musicOverride = RandEventSystem.instance.GetMusicOverride();
			if (musicOverride != null)
			{
				this.StartMusic(musicOverride);
				this.m_randomEventMusic = musicOverride;
				return true;
			}
			if (currentMusic == this.m_randomEventMusic)
			{
				this.m_randomEventMusic = null;
				this.StopMusic();
			}
		}
		return false;
	}

	// Token: 0x06000B14 RID: 2836 RVA: 0x0004FCBB File Offset: 0x0004DEBB
	private bool HandleCombatMusic(string currentMusic)
	{
		if (this.InCombat())
		{
			this.StartMusic("combat");
			return true;
		}
		if (currentMusic == "combat")
		{
			this.StopMusic();
		}
		return false;
	}

	// Token: 0x06000B15 RID: 2837 RVA: 0x0004FCE8 File Offset: 0x0004DEE8
	private bool HandleSailingMusic(float dt, string currentMusic)
	{
		if (this.IsSailing())
		{
			this.m_notSailDuration = 0f;
			this.m_sailDuration += dt;
			if (this.m_sailDuration > this.m_sailMusicMinSailTime)
			{
				this.StartMusic("sailing");
				return true;
			}
		}
		else
		{
			this.m_sailDuration = 0f;
			this.m_notSailDuration += dt;
			if (this.m_notSailDuration > this.m_sailMusicMinSailTime / 2f && currentMusic == "sailing")
			{
				this.StopMusic();
			}
		}
		return false;
	}

	// Token: 0x06000B16 RID: 2838 RVA: 0x0004FD74 File Offset: 0x0004DF74
	private bool IsSailing()
	{
		if (!Player.m_localPlayer)
		{
			return false;
		}
		Ship localShip = Ship.GetLocalShip();
		return localShip && localShip.GetSpeed() > this.m_sailMusicShipSpeedThreshold;
	}

	// Token: 0x06000B17 RID: 2839 RVA: 0x0004FDB0 File Offset: 0x0004DFB0
	private void UpdateMusic(float dt)
	{
		if (this.m_queuedMusic != null || this.m_stopMusic)
		{
			if (this.m_musicSource.isPlaying && this.m_currentMusicVol > 0f)
			{
				float num = (this.m_queuedMusic != null) ? Mathf.Min(this.m_queuedMusic.m_fadeInTime, this.m_musicFadeTime) : this.m_musicFadeTime;
				this.m_currentMusicVol = Mathf.MoveTowards(this.m_currentMusicVol, 0f, dt / num);
				this.m_musicSource.volume = Utils.SmoothStep(0f, 1f, this.m_currentMusicVol) * this.m_musicVolume * MusicMan.m_masterMusicVolume;
				return;
			}
			if (this.m_musicSource.isPlaying && this.m_currentMusic != null && this.m_currentMusic.m_loop && this.m_currentMusic.m_resume)
			{
				this.m_currentMusic.m_lastPlayedTime = Time.time;
				this.m_currentMusic.m_savedPlaybackPos = this.m_musicSource.timeSamples;
				ZLog.Log(string.Concat(new object[]
				{
					"Stoped music ",
					this.m_currentMusic.m_name,
					" at ",
					this.m_currentMusic.m_savedPlaybackPos
				}));
			}
			this.m_musicSource.Stop();
			this.m_stopMusic = false;
			this.m_currentMusic = null;
			if (this.m_queuedMusic != null)
			{
				this.m_musicSource.clip = this.m_queuedMusic.m_clips[UnityEngine.Random.Range(0, this.m_queuedMusic.m_clips.Length)];
				this.m_musicSource.loop = this.m_queuedMusic.m_loop;
				this.m_musicSource.volume = 0f;
				this.m_musicSource.timeSamples = 0;
				this.m_musicSource.Play();
				if (this.m_queuedMusic.m_loop && this.m_queuedMusic.m_resume && Time.time - this.m_queuedMusic.m_lastPlayedTime < this.m_musicSource.clip.length * 2f)
				{
					this.m_musicSource.timeSamples = this.m_queuedMusic.m_savedPlaybackPos;
					ZLog.Log(string.Concat(new object[]
					{
						"Resumed music ",
						this.m_queuedMusic.m_name,
						" at ",
						this.m_queuedMusic.m_savedPlaybackPos
					}));
				}
				this.m_currentMusicVol = 0f;
				this.m_musicVolume = this.m_queuedMusic.m_volume;
				this.m_musicFadeTime = this.m_queuedMusic.m_fadeInTime;
				this.m_alwaysFadeout = this.m_queuedMusic.m_alwaysFadeout;
				this.m_currentMusic = this.m_queuedMusic;
				this.m_queuedMusic = null;
				return;
			}
		}
		else if (this.m_musicSource.isPlaying)
		{
			float num2 = this.m_musicSource.clip.length - this.m_musicSource.time;
			if (this.m_alwaysFadeout && !this.m_musicSource.loop && num2 < this.m_musicFadeTime)
			{
				this.m_currentMusicVol = Mathf.MoveTowards(this.m_currentMusicVol, 0f, dt / this.m_musicFadeTime);
				this.m_musicSource.volume = Utils.SmoothStep(0f, 1f, this.m_currentMusicVol) * this.m_musicVolume * MusicMan.m_masterMusicVolume;
				return;
			}
			this.m_currentMusicVol = Mathf.MoveTowards(this.m_currentMusicVol, 1f, dt / this.m_musicFadeTime);
			this.m_musicSource.volume = Utils.SmoothStep(0f, 1f, this.m_currentMusicVol) * this.m_musicVolume * MusicMan.m_masterMusicVolume;
			return;
		}
		else if (this.m_currentMusic != null && !this.m_musicSource.isPlaying)
		{
			this.m_currentMusic = null;
		}
	}

	// Token: 0x06000B18 RID: 2840 RVA: 0x00050173 File Offset: 0x0004E373
	private void UpdateCombatMusic(float dt)
	{
		if (this.m_combatTimer > 0f)
		{
			this.m_combatTimer -= Time.deltaTime;
		}
	}

	// Token: 0x06000B19 RID: 2841 RVA: 0x00050194 File Offset: 0x0004E394
	public void ResetCombatTimer()
	{
		this.m_combatTimer = this.m_combatMusicTimeout;
	}

	// Token: 0x06000B1A RID: 2842 RVA: 0x000501A2 File Offset: 0x0004E3A2
	private bool InCombat()
	{
		return this.m_combatTimer > 0f;
	}

	// Token: 0x06000B1B RID: 2843 RVA: 0x000501B1 File Offset: 0x0004E3B1
	public void TriggerMusic(string name)
	{
		this.m_triggerMusic = name;
	}

	// Token: 0x06000B1C RID: 2844 RVA: 0x000501BC File Offset: 0x0004E3BC
	private void StartMusic(string name)
	{
		if (this.GetCurrentMusic() == name)
		{
			return;
		}
		MusicMan.NamedMusic music = this.FindMusic(name);
		this.StartMusic(music);
	}

	// Token: 0x06000B1D RID: 2845 RVA: 0x000501E7 File Offset: 0x0004E3E7
	private void StartMusic(MusicMan.NamedMusic music)
	{
		if (music != null && this.GetCurrentMusic() == music.m_name)
		{
			return;
		}
		if (music != null)
		{
			this.m_queuedMusic = music;
			this.m_stopMusic = false;
			return;
		}
		this.StopMusic();
	}

	// Token: 0x06000B1E RID: 2846 RVA: 0x00050218 File Offset: 0x0004E418
	private MusicMan.NamedMusic FindMusic(string name)
	{
		if (name == null || name.Length == 0)
		{
			return null;
		}
		foreach (MusicMan.NamedMusic namedMusic in this.m_music)
		{
			if (namedMusic.m_name == name && namedMusic.m_enabled && namedMusic.m_clips.Length != 0 && namedMusic.m_clips[0])
			{
				return namedMusic;
			}
		}
		return null;
	}

	// Token: 0x06000B1F RID: 2847 RVA: 0x000502A8 File Offset: 0x0004E4A8
	public bool IsPlaying()
	{
		return this.m_musicSource.isPlaying;
	}

	// Token: 0x06000B20 RID: 2848 RVA: 0x000502B5 File Offset: 0x0004E4B5
	private string GetCurrentMusic()
	{
		if (this.m_stopMusic)
		{
			return "";
		}
		if (this.m_queuedMusic != null)
		{
			return this.m_queuedMusic.m_name;
		}
		if (this.m_currentMusic != null)
		{
			return this.m_currentMusic.m_name;
		}
		return "";
	}

	// Token: 0x06000B21 RID: 2849 RVA: 0x000502F2 File Offset: 0x0004E4F2
	private void StopMusic()
	{
		this.m_queuedMusic = null;
		this.m_stopMusic = true;
	}

	// Token: 0x06000B22 RID: 2850 RVA: 0x00050302 File Offset: 0x0004E502
	public void Reset()
	{
		this.StopMusic();
		this.m_combatTimer = 0f;
		this.m_randomEventMusic = null;
		this.m_triggerMusic = null;
	}

	// Token: 0x04000A72 RID: 2674
	private string m_triggeredMusic = "";

	// Token: 0x04000A73 RID: 2675
	private static MusicMan m_instance;

	// Token: 0x04000A74 RID: 2676
	public static float m_masterMusicVolume = 1f;

	// Token: 0x04000A75 RID: 2677
	public AudioMixerGroup m_musicMixer;

	// Token: 0x04000A76 RID: 2678
	public List<MusicMan.NamedMusic> m_music = new List<MusicMan.NamedMusic>();

	// Token: 0x04000A77 RID: 2679
	[Header("Combat")]
	public float m_combatMusicTimeout = 4f;

	// Token: 0x04000A78 RID: 2680
	[Header("Sailing")]
	public float m_sailMusicShipSpeedThreshold = 3f;

	// Token: 0x04000A79 RID: 2681
	public float m_sailMusicMinSailTime = 20f;

	// Token: 0x04000A7A RID: 2682
	[Header("Ambient music")]
	public float m_randomMusicIntervalMin = 300f;

	// Token: 0x04000A7B RID: 2683
	public float m_randomMusicIntervalMax = 500f;

	// Token: 0x04000A7C RID: 2684
	private MusicMan.NamedMusic m_queuedMusic;

	// Token: 0x04000A7D RID: 2685
	private MusicMan.NamedMusic m_currentMusic;

	// Token: 0x04000A7E RID: 2686
	private float m_musicVolume = 1f;

	// Token: 0x04000A7F RID: 2687
	private float m_musicFadeTime = 3f;

	// Token: 0x04000A80 RID: 2688
	private bool m_alwaysFadeout;

	// Token: 0x04000A81 RID: 2689
	private bool m_stopMusic;

	// Token: 0x04000A82 RID: 2690
	private string m_randomEventMusic;

	// Token: 0x04000A83 RID: 2691
	private float m_lastAmbientMusicTime;

	// Token: 0x04000A84 RID: 2692
	private float m_randomAmbientInterval;

	// Token: 0x04000A85 RID: 2693
	private string m_triggerMusic;

	// Token: 0x04000A86 RID: 2694
	private float m_combatTimer;

	// Token: 0x04000A87 RID: 2695
	private AudioSource m_musicSource;

	// Token: 0x04000A88 RID: 2696
	private float m_currentMusicVol;

	// Token: 0x04000A89 RID: 2697
	private float m_sailDuration;

	// Token: 0x04000A8A RID: 2698
	private float m_notSailDuration;

	// Token: 0x0200017C RID: 380
	[Serializable]
	public class NamedMusic
	{
		// Token: 0x0400119B RID: 4507
		public string m_name = "";

		// Token: 0x0400119C RID: 4508
		public AudioClip[] m_clips;

		// Token: 0x0400119D RID: 4509
		public float m_volume = 1f;

		// Token: 0x0400119E RID: 4510
		public float m_fadeInTime = 3f;

		// Token: 0x0400119F RID: 4511
		public bool m_alwaysFadeout;

		// Token: 0x040011A0 RID: 4512
		public bool m_loop;

		// Token: 0x040011A1 RID: 4513
		public bool m_resume;

		// Token: 0x040011A2 RID: 4514
		public bool m_enabled = true;

		// Token: 0x040011A3 RID: 4515
		public bool m_ambientMusic;

		// Token: 0x040011A4 RID: 4516
		[NonSerialized]
		public int m_savedPlaybackPos;

		// Token: 0x040011A5 RID: 4517
		[NonSerialized]
		public float m_lastPlayedTime;
	}
}
