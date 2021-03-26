using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityStandardAssets.ImageEffects;

// Token: 0x0200009B RID: 155
public class EnvMan : MonoBehaviour
{
	// Token: 0x17000024 RID: 36
	// (get) Token: 0x06000A17 RID: 2583 RVA: 0x0004901E File Offset: 0x0004721E
	public static EnvMan instance
	{
		get
		{
			return EnvMan.m_instance;
		}
	}

	// Token: 0x06000A18 RID: 2584 RVA: 0x00049028 File Offset: 0x00047228
	private void Awake()
	{
		EnvMan.m_instance = this;
		foreach (EnvSetup envSetup in this.m_environments)
		{
			this.SetParticleArrayEnabled(envSetup.m_psystems, false);
			if (envSetup.m_envObject)
			{
				envSetup.m_envObject.SetActive(false);
			}
		}
		foreach (BiomeEnvSetup biomeEnvSetup in this.m_biomes)
		{
			foreach (EnvEntry envEntry in biomeEnvSetup.m_environments)
			{
				envEntry.m_env = this.GetEnv(envEntry.m_environment);
			}
		}
		this.m_currentEnv = this.GetDefaultEnv();
	}

	// Token: 0x06000A19 RID: 2585 RVA: 0x00049138 File Offset: 0x00047338
	private void OnDestroy()
	{
		EnvMan.m_instance = null;
	}

	// Token: 0x06000A1A RID: 2586 RVA: 0x00049140 File Offset: 0x00047340
	private void SetParticleArrayEnabled(GameObject[] psystems, bool enabled)
	{
		foreach (GameObject gameObject in psystems)
		{
			ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].emission.enabled = enabled;
			}
			MistEmitter componentInChildren = gameObject.GetComponentInChildren<MistEmitter>();
			if (componentInChildren)
			{
				componentInChildren.enabled = enabled;
			}
		}
	}

	// Token: 0x06000A1B RID: 2587 RVA: 0x000491A8 File Offset: 0x000473A8
	private float RescaleDayFraction(float fraction)
	{
		if (fraction >= 0.15f && fraction <= 0.85f)
		{
			float num = (fraction - 0.15f) / 0.7f;
			fraction = 0.25f + num * 0.5f;
		}
		else if (fraction < 0.5f)
		{
			fraction = fraction / 0.15f * 0.25f;
		}
		else
		{
			float num2 = (fraction - 0.85f) / 0.15f;
			fraction = 0.75f + num2 * 0.25f;
		}
		return fraction;
	}

	// Token: 0x06000A1C RID: 2588 RVA: 0x0004921C File Offset: 0x0004741C
	private void Update()
	{
		Vector3 windForce = EnvMan.instance.GetWindForce();
		this.m_cloudOffset += windForce * Time.deltaTime * 0.01f;
		Shader.SetGlobalVector(this._CloudOffset, this.m_cloudOffset);
		Shader.SetGlobalVector(this._NetRefPos, ZNet.instance.GetReferencePosition());
	}

	// Token: 0x06000A1D RID: 2589 RVA: 0x0004928C File Offset: 0x0004748C
	private void FixedUpdate()
	{
		this.UpdateTimeSkip(Time.fixedDeltaTime);
		this.m_totalSeconds = ZNet.instance.GetTimeSeconds();
		long num = (long)this.m_totalSeconds;
		double num2 = this.m_totalSeconds * 1000.0;
		long num3 = this.m_dayLengthSec * 1000L;
		float num4 = Mathf.Clamp01((float)(num2 % (double)num3 / 1000.0) / (float)this.m_dayLengthSec);
		num4 = this.RescaleDayFraction(num4);
		float smoothDayFraction = this.m_smoothDayFraction;
		float t = Mathf.LerpAngle(this.m_smoothDayFraction * 360f, num4 * 360f, 0.01f);
		this.m_smoothDayFraction = Mathf.Repeat(t, 360f) / 360f;
		if (this.m_debugTimeOfDay)
		{
			this.m_smoothDayFraction = this.m_debugTime;
		}
		float num5 = Mathf.Pow(Mathf.Max(1f - Mathf.Clamp01(this.m_smoothDayFraction / 0.25f), Mathf.Clamp01((this.m_smoothDayFraction - 0.75f) / 0.25f)), 0.5f);
		float num6 = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(this.m_smoothDayFraction - 0.5f) / 0.25f), 0.5f);
		float num7 = Mathf.Min(Mathf.Clamp01(1f - (this.m_smoothDayFraction - 0.26f) / -this.m_sunHorizonTransitionL), Mathf.Clamp01(1f - (this.m_smoothDayFraction - 0.26f) / this.m_sunHorizonTransitionH));
		float num8 = Mathf.Min(Mathf.Clamp01(1f - (this.m_smoothDayFraction - 0.74f) / -this.m_sunHorizonTransitionH), Mathf.Clamp01(1f - (this.m_smoothDayFraction - 0.74f) / this.m_sunHorizonTransitionL));
		float num9 = 1f / (num5 + num6 + num7 + num8);
		num5 *= num9;
		num6 *= num9;
		num7 *= num9;
		num8 *= num9;
		Heightmap.Biome biome = this.GetBiome();
		this.UpdateTriggers(smoothDayFraction, this.m_smoothDayFraction, biome, Time.fixedDeltaTime);
		this.UpdateEnvironment(num, biome);
		this.InterpolateEnvironment(Time.fixedDeltaTime);
		this.UpdateWind(num, Time.fixedDeltaTime);
		if (!string.IsNullOrEmpty(this.m_forceEnv))
		{
			EnvSetup env = this.GetEnv(this.m_forceEnv);
			if (env != null)
			{
				this.SetEnv(env, num6, num5, num7, num8, Time.fixedDeltaTime);
				return;
			}
		}
		else
		{
			this.SetEnv(this.m_currentEnv, num6, num5, num7, num8, Time.fixedDeltaTime);
		}
	}

	// Token: 0x06000A1E RID: 2590 RVA: 0x000494FF File Offset: 0x000476FF
	private int GetCurrentDay()
	{
		return (int)(this.m_totalSeconds / (double)this.m_dayLengthSec);
	}

	// Token: 0x06000A1F RID: 2591 RVA: 0x00049510 File Offset: 0x00047710
	private void UpdateTriggers(float oldDayFraction, float newDayFraction, Heightmap.Biome biome, float dt)
	{
		if (Player.m_localPlayer == null || biome == Heightmap.Biome.None)
		{
			return;
		}
		EnvSetup currentEnvironment = this.GetCurrentEnvironment();
		if (currentEnvironment == null)
		{
			return;
		}
		this.UpdateAmbientMusic(biome, currentEnvironment, dt);
		if (oldDayFraction > 0.2f && oldDayFraction < 0.25f && newDayFraction > 0.25f && newDayFraction < 0.3f)
		{
			this.OnMorning(biome, currentEnvironment);
		}
		if (oldDayFraction > 0.7f && oldDayFraction < 0.75f && newDayFraction > 0.75f && newDayFraction < 0.8f)
		{
			this.OnEvening(biome, currentEnvironment);
		}
	}

	// Token: 0x06000A20 RID: 2592 RVA: 0x00049594 File Offset: 0x00047794
	private void UpdateAmbientMusic(Heightmap.Biome biome, EnvSetup currentEnv, float dt)
	{
		this.m_ambientMusicTimer += dt;
		if (this.m_ambientMusicTimer > 2f)
		{
			this.m_ambientMusicTimer = 0f;
			this.m_ambientMusic = null;
			BiomeEnvSetup biomeEnvSetup = this.GetBiomeEnvSetup(biome);
			if (this.IsDay())
			{
				if (currentEnv.m_musicDay.Length > 0)
				{
					this.m_ambientMusic = currentEnv.m_musicDay;
					return;
				}
				if (biomeEnvSetup.m_musicDay.Length > 0)
				{
					this.m_ambientMusic = biomeEnvSetup.m_musicDay;
					return;
				}
			}
			else
			{
				if (currentEnv.m_musicNight.Length > 0)
				{
					this.m_ambientMusic = currentEnv.m_musicNight;
					return;
				}
				if (biomeEnvSetup.m_musicNight.Length > 0)
				{
					this.m_ambientMusic = biomeEnvSetup.m_musicNight;
				}
			}
		}
	}

	// Token: 0x06000A21 RID: 2593 RVA: 0x0004964C File Offset: 0x0004784C
	public string GetAmbientMusic()
	{
		return this.m_ambientMusic;
	}

	// Token: 0x06000A22 RID: 2594 RVA: 0x00049654 File Offset: 0x00047854
	private void OnMorning(Heightmap.Biome biome, EnvSetup currentEnv)
	{
		string name = "morning";
		if (currentEnv.m_musicMorning.Length > 0)
		{
			name = currentEnv.m_musicMorning;
		}
		else
		{
			BiomeEnvSetup biomeEnvSetup = this.GetBiomeEnvSetup(biome);
			if (biomeEnvSetup.m_musicMorning.Length > 0)
			{
				name = biomeEnvSetup.m_musicMorning;
			}
		}
		MusicMan.instance.TriggerMusic(name);
		Player.m_localPlayer.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_newday", new string[]
		{
			this.GetCurrentDay().ToString()
		}), 0, null);
	}

	// Token: 0x06000A23 RID: 2595 RVA: 0x000496DC File Offset: 0x000478DC
	private void OnEvening(Heightmap.Biome biome, EnvSetup currentEnv)
	{
		string name = "evening";
		if (currentEnv.m_musicEvening.Length > 0)
		{
			name = currentEnv.m_musicEvening;
		}
		else
		{
			BiomeEnvSetup biomeEnvSetup = this.GetBiomeEnvSetup(biome);
			if (biomeEnvSetup.m_musicEvening.Length > 0)
			{
				name = biomeEnvSetup.m_musicEvening;
			}
		}
		MusicMan.instance.TriggerMusic(name);
	}

	// Token: 0x06000A24 RID: 2596 RVA: 0x00049730 File Offset: 0x00047930
	public void SetForceEnvironment(string env)
	{
		if (this.m_forceEnv == env)
		{
			return;
		}
		ZLog.Log("Setting forced environment " + env);
		this.m_forceEnv = env;
		this.FixedUpdate();
		if (ReflectionUpdate.instance)
		{
			ReflectionUpdate.instance.UpdateReflection();
		}
	}

	// Token: 0x06000A25 RID: 2597 RVA: 0x00049780 File Offset: 0x00047980
	private EnvSetup SelectWeightedEnvironment(List<EnvEntry> environments)
	{
		float num = 0f;
		foreach (EnvEntry envEntry in environments)
		{
			num += envEntry.m_weight;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		float num3 = 0f;
		foreach (EnvEntry envEntry2 in environments)
		{
			num3 += envEntry2.m_weight;
			if (num3 >= num2)
			{
				return envEntry2.m_env;
			}
		}
		return environments[environments.Count - 1].m_env;
	}

	// Token: 0x06000A26 RID: 2598 RVA: 0x00049850 File Offset: 0x00047A50
	private string GetEnvironmentOverride()
	{
		if (!string.IsNullOrEmpty(this.m_debugEnv))
		{
			return this.m_debugEnv;
		}
		if (Player.m_localPlayer != null && Player.m_localPlayer.InIntro())
		{
			return this.m_introEnvironment;
		}
		string envOverride = RandEventSystem.instance.GetEnvOverride();
		if (!string.IsNullOrEmpty(envOverride))
		{
			return envOverride;
		}
		string environment = EnvZone.GetEnvironment();
		if (!string.IsNullOrEmpty(environment))
		{
			return environment;
		}
		return null;
	}

	// Token: 0x06000A27 RID: 2599 RVA: 0x000498B8 File Offset: 0x00047AB8
	private void UpdateEnvironment(long sec, Heightmap.Biome biome)
	{
		string environmentOverride = this.GetEnvironmentOverride();
		if (!string.IsNullOrEmpty(environmentOverride))
		{
			this.m_environmentPeriod = -1L;
			this.m_currentBiome = this.GetBiome();
			this.QueueEnvironment(environmentOverride);
			return;
		}
		long num = sec / this.m_environmentDuration;
		if (this.m_environmentPeriod != num || this.m_currentBiome != biome)
		{
			this.m_environmentPeriod = num;
			this.m_currentBiome = biome;
			UnityEngine.Random.State state = UnityEngine.Random.state;
			UnityEngine.Random.InitState((int)num);
			List<EnvEntry> availableEnvironments = this.GetAvailableEnvironments(biome);
			if (availableEnvironments != null && availableEnvironments.Count > 0)
			{
				EnvSetup env = this.SelectWeightedEnvironment(availableEnvironments);
				this.QueueEnvironment(env);
			}
			UnityEngine.Random.state = state;
		}
	}

	// Token: 0x06000A28 RID: 2600 RVA: 0x00049950 File Offset: 0x00047B50
	private BiomeEnvSetup GetBiomeEnvSetup(Heightmap.Biome biome)
	{
		foreach (BiomeEnvSetup biomeEnvSetup in this.m_biomes)
		{
			if (biomeEnvSetup.m_biome == biome)
			{
				return biomeEnvSetup;
			}
		}
		return null;
	}

	// Token: 0x06000A29 RID: 2601 RVA: 0x000499AC File Offset: 0x00047BAC
	private List<EnvEntry> GetAvailableEnvironments(Heightmap.Biome biome)
	{
		BiomeEnvSetup biomeEnvSetup = this.GetBiomeEnvSetup(biome);
		if (biomeEnvSetup != null)
		{
			return biomeEnvSetup.m_environments;
		}
		return null;
	}

	// Token: 0x06000A2A RID: 2602 RVA: 0x000499CC File Offset: 0x00047BCC
	private Heightmap.Biome GetBiome()
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return Heightmap.Biome.None;
		}
		Vector3 position = mainCamera.transform.position;
		if (this.m_cachedHeightmap == null || !this.m_cachedHeightmap.IsPointInside(position, 0f))
		{
			this.m_cachedHeightmap = Heightmap.FindHeightmap(position);
		}
		if (this.m_cachedHeightmap)
		{
			return this.m_cachedHeightmap.GetBiome(position);
		}
		return Heightmap.Biome.None;
	}

	// Token: 0x06000A2B RID: 2603 RVA: 0x00049A40 File Offset: 0x00047C40
	private void InterpolateEnvironment(float dt)
	{
		if (this.m_nextEnv != null)
		{
			this.m_transitionTimer += dt;
			float num = Mathf.Clamp01(this.m_transitionTimer / this.m_transitionDuration);
			this.m_currentEnv = this.InterpolateEnvironment(this.m_prevEnv, this.m_nextEnv, num);
			if (num >= 1f)
			{
				this.m_currentEnv = this.m_nextEnv;
				this.m_prevEnv = null;
				this.m_nextEnv = null;
			}
		}
	}

	// Token: 0x06000A2C RID: 2604 RVA: 0x00049AB4 File Offset: 0x00047CB4
	private void QueueEnvironment(string name)
	{
		if (this.m_currentEnv.m_name == name)
		{
			return;
		}
		if (this.m_nextEnv != null && this.m_nextEnv.m_name == name)
		{
			return;
		}
		EnvSetup env = this.GetEnv(name);
		if (env != null)
		{
			this.QueueEnvironment(env);
		}
	}

	// Token: 0x06000A2D RID: 2605 RVA: 0x00049B03 File Offset: 0x00047D03
	private void QueueEnvironment(EnvSetup env)
	{
		if (this.m_firstEnv)
		{
			this.m_firstEnv = false;
			this.m_currentEnv = env;
			return;
		}
		this.m_prevEnv = this.m_currentEnv.Clone();
		this.m_nextEnv = env;
		this.m_transitionTimer = 0f;
	}

	// Token: 0x06000A2E RID: 2606 RVA: 0x00049B40 File Offset: 0x00047D40
	private EnvSetup InterpolateEnvironment(EnvSetup a, EnvSetup b, float i)
	{
		EnvSetup envSetup = a.Clone();
		envSetup.m_name = b.m_name;
		if (i >= 0.5f)
		{
			envSetup.m_isFreezingAtNight = b.m_isFreezingAtNight;
			envSetup.m_isFreezing = b.m_isFreezing;
			envSetup.m_isCold = b.m_isCold;
			envSetup.m_isColdAtNight = b.m_isColdAtNight;
			envSetup.m_isColdAtNight = b.m_isColdAtNight;
		}
		envSetup.m_ambColorDay = Color.Lerp(a.m_ambColorDay, b.m_ambColorDay, i);
		envSetup.m_ambColorNight = Color.Lerp(a.m_ambColorNight, b.m_ambColorNight, i);
		envSetup.m_fogColorDay = Color.Lerp(a.m_fogColorDay, b.m_fogColorDay, i);
		envSetup.m_fogColorEvening = Color.Lerp(a.m_fogColorEvening, b.m_fogColorEvening, i);
		envSetup.m_fogColorMorning = Color.Lerp(a.m_fogColorMorning, b.m_fogColorMorning, i);
		envSetup.m_fogColorNight = Color.Lerp(a.m_fogColorNight, b.m_fogColorNight, i);
		envSetup.m_fogColorSunDay = Color.Lerp(a.m_fogColorSunDay, b.m_fogColorSunDay, i);
		envSetup.m_fogColorSunEvening = Color.Lerp(a.m_fogColorSunEvening, b.m_fogColorSunEvening, i);
		envSetup.m_fogColorSunMorning = Color.Lerp(a.m_fogColorSunMorning, b.m_fogColorSunMorning, i);
		envSetup.m_fogColorSunNight = Color.Lerp(a.m_fogColorSunNight, b.m_fogColorSunNight, i);
		envSetup.m_fogDensityDay = Mathf.Lerp(a.m_fogDensityDay, b.m_fogDensityDay, i);
		envSetup.m_fogDensityEvening = Mathf.Lerp(a.m_fogDensityEvening, b.m_fogDensityEvening, i);
		envSetup.m_fogDensityMorning = Mathf.Lerp(a.m_fogDensityMorning, b.m_fogDensityMorning, i);
		envSetup.m_fogDensityNight = Mathf.Lerp(a.m_fogDensityNight, b.m_fogDensityNight, i);
		envSetup.m_sunColorDay = Color.Lerp(a.m_sunColorDay, b.m_sunColorDay, i);
		envSetup.m_sunColorEvening = Color.Lerp(a.m_sunColorEvening, b.m_sunColorEvening, i);
		envSetup.m_sunColorMorning = Color.Lerp(a.m_sunColorMorning, b.m_sunColorMorning, i);
		envSetup.m_sunColorNight = Color.Lerp(a.m_sunColorNight, b.m_sunColorNight, i);
		envSetup.m_lightIntensityDay = Mathf.Lerp(a.m_lightIntensityDay, b.m_lightIntensityDay, i);
		envSetup.m_lightIntensityNight = Mathf.Lerp(a.m_lightIntensityNight, b.m_lightIntensityNight, i);
		envSetup.m_sunAngle = Mathf.Lerp(a.m_sunAngle, b.m_sunAngle, i);
		envSetup.m_windMin = Mathf.Lerp(a.m_windMin, b.m_windMin, i);
		envSetup.m_windMax = Mathf.Lerp(a.m_windMax, b.m_windMax, i);
		envSetup.m_rainCloudAlpha = Mathf.Lerp(a.m_rainCloudAlpha, b.m_rainCloudAlpha, i);
		envSetup.m_ambientLoop = ((i > 0.75f) ? b.m_ambientLoop : a.m_ambientLoop);
		envSetup.m_ambientVol = ((i > 0.75f) ? b.m_ambientVol : a.m_ambientVol);
		envSetup.m_musicEvening = b.m_musicEvening;
		envSetup.m_musicMorning = b.m_musicMorning;
		envSetup.m_musicDay = b.m_musicDay;
		envSetup.m_musicNight = b.m_musicNight;
		return envSetup;
	}

	// Token: 0x06000A2F RID: 2607 RVA: 0x00049E50 File Offset: 0x00048050
	private void SetEnv(EnvSetup env, float dayInt, float nightInt, float morningInt, float eveningInt, float dt)
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		this.m_dirLight.transform.rotation = Quaternion.Euler(-90f + env.m_sunAngle, 0f, 0f) * Quaternion.Euler(0f, -90f, 0f) * Quaternion.Euler(-90f + 360f * this.m_smoothDayFraction, 0f, 0f);
		Vector3 v = -this.m_dirLight.transform.forward;
		this.m_dirLight.intensity = env.m_lightIntensityDay * dayInt;
		this.m_dirLight.intensity += env.m_lightIntensityNight * nightInt;
		if (nightInt > 0f)
		{
			this.m_dirLight.transform.rotation = this.m_dirLight.transform.rotation * Quaternion.Euler(180f, 0f, 0f);
		}
		this.m_dirLight.transform.position = mainCamera.transform.position - this.m_dirLight.transform.forward * 3000f;
		this.m_dirLight.color = new Color(0f, 0f, 0f, 0f);
		this.m_dirLight.color += env.m_sunColorNight * nightInt;
		if (dayInt > 0f)
		{
			this.m_dirLight.color += env.m_sunColorDay * dayInt;
			this.m_dirLight.color += env.m_sunColorMorning * morningInt;
			this.m_dirLight.color += env.m_sunColorEvening * eveningInt;
		}
		RenderSettings.fogColor = new Color(0f, 0f, 0f, 0f);
		RenderSettings.fogColor += env.m_fogColorNight * nightInt;
		RenderSettings.fogColor += env.m_fogColorDay * dayInt;
		RenderSettings.fogColor += env.m_fogColorMorning * morningInt;
		RenderSettings.fogColor += env.m_fogColorEvening * eveningInt;
		this.m_sunFogColor = new Color(0f, 0f, 0f, 0f);
		this.m_sunFogColor += env.m_fogColorSunNight * nightInt;
		if (dayInt > 0f)
		{
			this.m_sunFogColor += env.m_fogColorSunDay * dayInt;
			this.m_sunFogColor += env.m_fogColorSunMorning * morningInt;
			this.m_sunFogColor += env.m_fogColorSunEvening * eveningInt;
		}
		this.m_sunFogColor = Color.Lerp(RenderSettings.fogColor, this.m_sunFogColor, Mathf.Clamp01(Mathf.Max(nightInt, dayInt) * 3f));
		RenderSettings.fogDensity = 0f;
		RenderSettings.fogDensity += env.m_fogDensityNight * nightInt;
		RenderSettings.fogDensity += env.m_fogDensityDay * dayInt;
		RenderSettings.fogDensity += env.m_fogDensityMorning * morningInt;
		RenderSettings.fogDensity += env.m_fogDensityEvening * eveningInt;
		RenderSettings.ambientMode = AmbientMode.Flat;
		RenderSettings.ambientLight = Color.Lerp(env.m_ambColorNight, env.m_ambColorDay, dayInt);
		SunShafts component = mainCamera.GetComponent<SunShafts>();
		if (component)
		{
			component.sunColor = this.m_dirLight.color;
		}
		if (env.m_envObject != this.m_currentEnvObject)
		{
			if (this.m_currentEnvObject)
			{
				this.m_currentEnvObject.SetActive(false);
				this.m_currentEnvObject = null;
			}
			if (env.m_envObject)
			{
				this.m_currentEnvObject = env.m_envObject;
				this.m_currentEnvObject.SetActive(true);
			}
		}
		if (env.m_psystems != this.m_currentPSystems)
		{
			if (this.m_currentPSystems != null)
			{
				this.SetParticleArrayEnabled(this.m_currentPSystems, false);
				this.m_currentPSystems = null;
			}
			if (env.m_psystems != null && (!env.m_psystemsOutsideOnly || (Player.m_localPlayer && !Player.m_localPlayer.InShelter())))
			{
				this.SetParticleArrayEnabled(env.m_psystems, true);
				this.m_currentPSystems = env.m_psystems;
			}
		}
		this.m_clouds.material.SetFloat(this._Rain, env.m_rainCloudAlpha);
		if (env.m_ambientLoop)
		{
			AudioMan.instance.QueueAmbientLoop(env.m_ambientLoop, env.m_ambientVol);
		}
		else
		{
			AudioMan.instance.StopAmbientLoop();
		}
		Shader.SetGlobalVector(this._SkyboxSunDir, v);
		Shader.SetGlobalVector(this._SkyboxSunDir, v);
		Shader.SetGlobalVector(this._SunDir, -this.m_dirLight.transform.forward);
		Shader.SetGlobalColor(this._SunFogColor, this.m_sunFogColor);
		Shader.SetGlobalColor(this._SunColor, this.m_dirLight.color * this.m_dirLight.intensity);
		Shader.SetGlobalColor(this._AmbientColor, RenderSettings.ambientLight);
		float num = Shader.GetGlobalFloat(this._Wet);
		num = Mathf.MoveTowards(num, env.m_isWet ? 1f : 0f, dt / this.m_wetTransitionDuration);
		Shader.SetGlobalFloat(this._Wet, num);
	}

	// Token: 0x06000A30 RID: 2608 RVA: 0x0004A409 File Offset: 0x00048609
	private float GetDayFraction()
	{
		return this.m_smoothDayFraction;
	}

	// Token: 0x06000A31 RID: 2609 RVA: 0x0004A411 File Offset: 0x00048611
	public int GetDay(double time)
	{
		return (int)(time / (double)this.m_dayLengthSec);
	}

	// Token: 0x06000A32 RID: 2610 RVA: 0x0004A41D File Offset: 0x0004861D
	public double GetMorningStartSec(int day)
	{
		return (double)((float)((long)day * this.m_dayLengthSec) + (float)this.m_dayLengthSec * 0.15f);
	}

	// Token: 0x06000A33 RID: 2611 RVA: 0x0004A438 File Offset: 0x00048638
	private void UpdateTimeSkip(float dt)
	{
		if (!ZNet.instance.IsServer())
		{
			return;
		}
		if (this.m_skipTime)
		{
			double num = ZNet.instance.GetTimeSeconds();
			num += (double)dt * this.m_timeSkipSpeed;
			if (num >= this.m_skipToTime)
			{
				num = this.m_skipToTime;
				this.m_skipTime = false;
			}
			ZNet.instance.SetNetTime(num);
		}
	}

	// Token: 0x06000A34 RID: 2612 RVA: 0x0004A493 File Offset: 0x00048693
	public bool IsTimeSkipping()
	{
		return this.m_skipTime;
	}

	// Token: 0x06000A35 RID: 2613 RVA: 0x0004A49C File Offset: 0x0004869C
	public void SkipToMorning()
	{
		double timeSeconds = ZNet.instance.GetTimeSeconds();
		double time = timeSeconds - (double)((float)this.m_dayLengthSec * 0.15f);
		int day = this.GetDay(time);
		double morningStartSec = this.GetMorningStartSec(day + 1);
		this.m_skipTime = true;
		this.m_skipToTime = morningStartSec;
		double num = morningStartSec - timeSeconds;
		this.m_timeSkipSpeed = num / 12.0;
		ZLog.Log(string.Concat(new object[]
		{
			"Time ",
			timeSeconds,
			", day:",
			day,
			"    nextm:",
			morningStartSec,
			"  skipspeed:",
			this.m_timeSkipSpeed
		}));
	}

	// Token: 0x06000A36 RID: 2614 RVA: 0x0004A558 File Offset: 0x00048758
	public bool IsDay()
	{
		float dayFraction = this.GetDayFraction();
		return dayFraction >= 0.25f && dayFraction <= 0.75f;
	}

	// Token: 0x06000A37 RID: 2615 RVA: 0x0004A584 File Offset: 0x00048784
	public bool IsAfternoon()
	{
		float dayFraction = this.GetDayFraction();
		return dayFraction >= 0.5f && dayFraction <= 0.75f;
	}

	// Token: 0x06000A38 RID: 2616 RVA: 0x0004A5B0 File Offset: 0x000487B0
	public bool IsNight()
	{
		float dayFraction = this.GetDayFraction();
		return dayFraction <= 0.25f || dayFraction >= 0.75f;
	}

	// Token: 0x06000A39 RID: 2617 RVA: 0x0004A5DC File Offset: 0x000487DC
	public bool IsDaylight()
	{
		EnvSetup currentEnvironment = this.GetCurrentEnvironment();
		return (currentEnvironment == null || !currentEnvironment.m_alwaysDark) && this.IsDay();
	}

	// Token: 0x06000A3A RID: 2618 RVA: 0x0004A603 File Offset: 0x00048803
	public Heightmap.Biome GetCurrentBiome()
	{
		return this.m_currentBiome;
	}

	// Token: 0x06000A3B RID: 2619 RVA: 0x0004A60B File Offset: 0x0004880B
	public bool IsEnvironment(string name)
	{
		return this.GetCurrentEnvironment().m_name == name;
	}

	// Token: 0x06000A3C RID: 2620 RVA: 0x0004A620 File Offset: 0x00048820
	public bool IsEnvironment(List<string> names)
	{
		EnvSetup currentEnvironment = this.GetCurrentEnvironment();
		return names.Contains(currentEnvironment.m_name);
	}

	// Token: 0x06000A3D RID: 2621 RVA: 0x0004A640 File Offset: 0x00048840
	public EnvSetup GetCurrentEnvironment()
	{
		if (!string.IsNullOrEmpty(this.m_forceEnv))
		{
			EnvSetup env = this.GetEnv(this.m_forceEnv);
			if (env != null)
			{
				return env;
			}
		}
		return this.m_currentEnv;
	}

	// Token: 0x06000A3E RID: 2622 RVA: 0x0004A674 File Offset: 0x00048874
	public bool IsFreezing()
	{
		EnvSetup currentEnvironment = this.GetCurrentEnvironment();
		return currentEnvironment != null && (currentEnvironment.m_isFreezing || (currentEnvironment.m_isFreezingAtNight && !this.IsDay()));
	}

	// Token: 0x06000A3F RID: 2623 RVA: 0x0004A6AC File Offset: 0x000488AC
	public bool IsCold()
	{
		EnvSetup currentEnvironment = this.GetCurrentEnvironment();
		return currentEnvironment != null && (currentEnvironment.m_isCold || (currentEnvironment.m_isColdAtNight && !this.IsDay()));
	}

	// Token: 0x06000A40 RID: 2624 RVA: 0x0004A6E4 File Offset: 0x000488E4
	public bool IsWet()
	{
		EnvSetup currentEnvironment = this.GetCurrentEnvironment();
		return currentEnvironment != null && currentEnvironment.m_isWet;
	}

	// Token: 0x06000A41 RID: 2625 RVA: 0x0004A703 File Offset: 0x00048903
	public Color GetSunFogColor()
	{
		return this.m_sunFogColor;
	}

	// Token: 0x06000A42 RID: 2626 RVA: 0x0004A70B File Offset: 0x0004890B
	public Vector3 GetSunDirection()
	{
		return this.m_dirLight.transform.forward;
	}

	// Token: 0x06000A43 RID: 2627 RVA: 0x0004A720 File Offset: 0x00048920
	private EnvSetup GetEnv(string name)
	{
		foreach (EnvSetup envSetup in this.m_environments)
		{
			if (envSetup.m_name == name)
			{
				return envSetup;
			}
		}
		return null;
	}

	// Token: 0x06000A44 RID: 2628 RVA: 0x0004A784 File Offset: 0x00048984
	private EnvSetup GetDefaultEnv()
	{
		foreach (EnvSetup envSetup in this.m_environments)
		{
			if (envSetup.m_default)
			{
				return envSetup;
			}
		}
		return null;
	}

	// Token: 0x06000A45 RID: 2629 RVA: 0x0004A7E0 File Offset: 0x000489E0
	public void SetDebugWind(float angle, float intensity)
	{
		this.m_debugWind = true;
		this.m_debugWindAngle = angle;
		this.m_debugWindIntensity = Mathf.Clamp01(intensity);
	}

	// Token: 0x06000A46 RID: 2630 RVA: 0x0004A7FC File Offset: 0x000489FC
	public void ResetDebugWind()
	{
		this.m_debugWind = false;
	}

	// Token: 0x06000A47 RID: 2631 RVA: 0x0004A805 File Offset: 0x00048A05
	public Vector3 GetWindForce()
	{
		return this.GetWindDir() * this.m_wind.w;
	}

	// Token: 0x06000A48 RID: 2632 RVA: 0x0004A81D File Offset: 0x00048A1D
	public Vector3 GetWindDir()
	{
		return new Vector3(this.m_wind.x, this.m_wind.y, this.m_wind.z);
	}

	// Token: 0x06000A49 RID: 2633 RVA: 0x0004A845 File Offset: 0x00048A45
	public float GetWindIntensity()
	{
		return this.m_wind.w;
	}

	// Token: 0x06000A4A RID: 2634 RVA: 0x0004A854 File Offset: 0x00048A54
	private void UpdateWind(long timeSec, float dt)
	{
		if (this.m_debugWind)
		{
			float f = 0.017453292f * this.m_debugWindAngle;
			Vector3 dir = new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f));
			this.SetTargetWind(dir, this.m_debugWindIntensity);
		}
		else
		{
			EnvSetup currentEnvironment = this.GetCurrentEnvironment();
			if (currentEnvironment != null)
			{
				UnityEngine.Random.State state = UnityEngine.Random.state;
				float f2 = 0f;
				float num = 0.5f;
				this.AddWindOctave(timeSec, 1, ref f2, ref num);
				this.AddWindOctave(timeSec, 2, ref f2, ref num);
				this.AddWindOctave(timeSec, 4, ref f2, ref num);
				this.AddWindOctave(timeSec, 8, ref f2, ref num);
				UnityEngine.Random.state = state;
				Vector3 dir2 = new Vector3(Mathf.Sin(f2), 0f, Mathf.Cos(f2));
				num = Mathf.Lerp(currentEnvironment.m_windMin, currentEnvironment.m_windMax, num);
				if (Player.m_localPlayer)
				{
					float magnitude = Player.m_localPlayer.transform.position.magnitude;
					if (magnitude > 10500f - this.m_edgeOfWorldWidth)
					{
						float num2 = Utils.LerpStep(10500f - this.m_edgeOfWorldWidth, 10500f, magnitude);
						num2 = 1f - Mathf.Pow(1f - num2, 2f);
						dir2 = Player.m_localPlayer.transform.position.normalized;
						num = Mathf.Lerp(num, 1f, num2);
					}
					else
					{
						Ship localShip = Ship.GetLocalShip();
						if (localShip && localShip.IsWindControllActive())
						{
							dir2 = localShip.transform.forward;
						}
					}
				}
				this.SetTargetWind(dir2, num);
			}
		}
		this.UpdateWindTransition(dt);
	}

	// Token: 0x06000A4B RID: 2635 RVA: 0x0004A9F2 File Offset: 0x00048BF2
	private void AddWindOctave(long timeSec, int octave, ref float angle, ref float intensity)
	{
		UnityEngine.Random.InitState((int)(timeSec / (this.m_windPeriodDuration / (long)octave)));
		angle += UnityEngine.Random.value * (6.2831855f / (float)octave);
		intensity += -(0.5f / (float)octave) + UnityEngine.Random.value / (float)octave;
	}

	// Token: 0x06000A4C RID: 2636 RVA: 0x0004AA34 File Offset: 0x00048C34
	private void SetTargetWind(Vector3 dir, float intensity)
	{
		if (this.m_windTransitionTimer >= 0f)
		{
			return;
		}
		intensity = Mathf.Clamp(intensity, 0.05f, 1f);
		if (Mathf.Approximately(dir.x, this.m_windDir1.x) && Mathf.Approximately(dir.y, this.m_windDir1.y) && Mathf.Approximately(dir.z, this.m_windDir1.z) && Mathf.Approximately(intensity, this.m_windDir1.w))
		{
			return;
		}
		this.m_windTransitionTimer = 0f;
		this.m_windDir2 = new Vector4(dir.x, dir.y, dir.z, intensity);
	}

	// Token: 0x06000A4D RID: 2637 RVA: 0x0004AAE8 File Offset: 0x00048CE8
	private void UpdateWindTransition(float dt)
	{
		if (this.m_windTransitionTimer >= 0f)
		{
			this.m_windTransitionTimer += dt;
			float num = Mathf.Clamp01(this.m_windTransitionTimer / this.m_windTransitionDuration);
			Shader.SetGlobalVector(this._GlobalWind1, this.m_windDir1);
			Shader.SetGlobalVector(this._GlobalWind2, this.m_windDir2);
			Shader.SetGlobalFloat(this._GlobalWindAlpha, num);
			this.m_wind = Vector4.Lerp(this.m_windDir1, this.m_windDir2, num);
			if (num >= 1f)
			{
				this.m_windDir1 = this.m_windDir2;
				this.m_windTransitionTimer = -1f;
			}
		}
		else
		{
			Shader.SetGlobalVector(this._GlobalWind1, this.m_windDir1);
			Shader.SetGlobalFloat(this._GlobalWindAlpha, 0f);
			this.m_wind = this.m_windDir1;
		}
		Shader.SetGlobalVector(this._GlobalWindForce, this.GetWindForce());
	}

	// Token: 0x06000A4E RID: 2638 RVA: 0x0004ABD0 File Offset: 0x00048DD0
	public void GetWindData(out Vector4 wind1, out Vector4 wind2, out float alpha)
	{
		wind1 = this.m_windDir1;
		wind2 = this.m_windDir2;
		if (this.m_windTransitionTimer >= 0f)
		{
			alpha = Mathf.Clamp01(this.m_windTransitionTimer / this.m_windTransitionDuration);
			return;
		}
		alpha = 0f;
	}

	// Token: 0x04000968 RID: 2408
	private static EnvMan m_instance;

	// Token: 0x04000969 RID: 2409
	public Light m_dirLight;

	// Token: 0x0400096A RID: 2410
	public bool m_debugTimeOfDay;

	// Token: 0x0400096B RID: 2411
	[Range(0f, 1f)]
	public float m_debugTime = 0.5f;

	// Token: 0x0400096C RID: 2412
	public string m_debugEnv = "";

	// Token: 0x0400096D RID: 2413
	public bool m_debugWind;

	// Token: 0x0400096E RID: 2414
	[Range(0f, 360f)]
	public float m_debugWindAngle;

	// Token: 0x0400096F RID: 2415
	[Range(0f, 1f)]
	public float m_debugWindIntensity = 1f;

	// Token: 0x04000970 RID: 2416
	private const float m_morningL = 0.15f;

	// Token: 0x04000971 RID: 2417
	public float m_sunHorizonTransitionH = 0.08f;

	// Token: 0x04000972 RID: 2418
	public float m_sunHorizonTransitionL = 0.02f;

	// Token: 0x04000973 RID: 2419
	public long m_dayLengthSec = 1200L;

	// Token: 0x04000974 RID: 2420
	public float m_transitionDuration = 2f;

	// Token: 0x04000975 RID: 2421
	public long m_environmentDuration = 20L;

	// Token: 0x04000976 RID: 2422
	public long m_windPeriodDuration = 10L;

	// Token: 0x04000977 RID: 2423
	public float m_windTransitionDuration = 5f;

	// Token: 0x04000978 RID: 2424
	public List<EnvSetup> m_environments = new List<EnvSetup>();

	// Token: 0x04000979 RID: 2425
	public List<BiomeEnvSetup> m_biomes = new List<BiomeEnvSetup>();

	// Token: 0x0400097A RID: 2426
	public string m_introEnvironment = "ThunderStorm";

	// Token: 0x0400097B RID: 2427
	public float m_edgeOfWorldWidth = 500f;

	// Token: 0x0400097C RID: 2428
	[Header("Music")]
	public float m_randomMusicIntervalMin = 60f;

	// Token: 0x0400097D RID: 2429
	public float m_randomMusicIntervalMax = 200f;

	// Token: 0x0400097E RID: 2430
	[Header("Other")]
	public MeshRenderer m_clouds;

	// Token: 0x0400097F RID: 2431
	public MeshRenderer m_rainClouds;

	// Token: 0x04000980 RID: 2432
	public MeshRenderer m_rainCloudsDownside;

	// Token: 0x04000981 RID: 2433
	public float m_wetTransitionDuration = 15f;

	// Token: 0x04000982 RID: 2434
	private bool m_skipTime;

	// Token: 0x04000983 RID: 2435
	private double m_skipToTime;

	// Token: 0x04000984 RID: 2436
	private double m_timeSkipSpeed = 1.0;

	// Token: 0x04000985 RID: 2437
	private const double m_timeSkipDuration = 12.0;

	// Token: 0x04000986 RID: 2438
	private double m_totalSeconds;

	// Token: 0x04000987 RID: 2439
	private float m_smoothDayFraction;

	// Token: 0x04000988 RID: 2440
	private Color m_sunFogColor = Color.white;

	// Token: 0x04000989 RID: 2441
	private GameObject[] m_currentPSystems;

	// Token: 0x0400098A RID: 2442
	private GameObject m_currentEnvObject;

	// Token: 0x0400098B RID: 2443
	private Vector4 m_windDir1 = new Vector4(0f, 0f, -1f, 0f);

	// Token: 0x0400098C RID: 2444
	private Vector4 m_windDir2 = new Vector4(0f, 0f, -1f, 0f);

	// Token: 0x0400098D RID: 2445
	private Vector4 m_wind = new Vector4(0f, 0f, -1f, 0f);

	// Token: 0x0400098E RID: 2446
	private float m_windTransitionTimer = -1f;

	// Token: 0x0400098F RID: 2447
	private Vector3 m_cloudOffset = Vector3.zero;

	// Token: 0x04000990 RID: 2448
	private string m_forceEnv = "";

	// Token: 0x04000991 RID: 2449
	private EnvSetup m_currentEnv;

	// Token: 0x04000992 RID: 2450
	private EnvSetup m_prevEnv;

	// Token: 0x04000993 RID: 2451
	private EnvSetup m_nextEnv;

	// Token: 0x04000994 RID: 2452
	private string m_ambientMusic;

	// Token: 0x04000995 RID: 2453
	private float m_ambientMusicTimer;

	// Token: 0x04000996 RID: 2454
	private Heightmap m_cachedHeightmap;

	// Token: 0x04000997 RID: 2455
	private Heightmap.Biome m_currentBiome;

	// Token: 0x04000998 RID: 2456
	private long m_environmentPeriod;

	// Token: 0x04000999 RID: 2457
	private float m_transitionTimer;

	// Token: 0x0400099A RID: 2458
	private bool m_firstEnv = true;

	// Token: 0x0400099B RID: 2459
	private int _NetRefPos = Shader.PropertyToID("_NetRefPos");

	// Token: 0x0400099C RID: 2460
	private int _SkyboxSunDir = Shader.PropertyToID("_SkyboxSunDir");

	// Token: 0x0400099D RID: 2461
	private int _SunDir = Shader.PropertyToID("_SunDir");

	// Token: 0x0400099E RID: 2462
	private int _SunFogColor = Shader.PropertyToID("_SunFogColor");

	// Token: 0x0400099F RID: 2463
	private int _Wet = Shader.PropertyToID("_Wet");

	// Token: 0x040009A0 RID: 2464
	private int _SunColor = Shader.PropertyToID("_SunColor");

	// Token: 0x040009A1 RID: 2465
	private int _AmbientColor = Shader.PropertyToID("_AmbientColor");

	// Token: 0x040009A2 RID: 2466
	private int _GlobalWind1 = Shader.PropertyToID("_GlobalWind1");

	// Token: 0x040009A3 RID: 2467
	private int _GlobalWind2 = Shader.PropertyToID("_GlobalWind2");

	// Token: 0x040009A4 RID: 2468
	private int _GlobalWindAlpha = Shader.PropertyToID("_GlobalWindAlpha");

	// Token: 0x040009A5 RID: 2469
	private int _CloudOffset = Shader.PropertyToID("_CloudOffset");

	// Token: 0x040009A6 RID: 2470
	private int _GlobalWindForce = Shader.PropertyToID("_GlobalWindForce");

	// Token: 0x040009A7 RID: 2471
	private int _Rain = Shader.PropertyToID("_Rain");
}
