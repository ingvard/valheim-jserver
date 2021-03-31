using System;
using UnityEngine;
using UnityEngine.Serialization;

// Token: 0x0200009A RID: 154
[Serializable]
public class EnvSetup
{
	// Token: 0x06000A16 RID: 2582 RVA: 0x00048F6E File Offset: 0x0004716E
	public EnvSetup Clone()
	{
		return base.MemberwiseClone() as EnvSetup;
	}

	// Token: 0x04000942 RID: 2370
	public string m_name = "";

	// Token: 0x04000943 RID: 2371
	public bool m_default;

	// Token: 0x04000944 RID: 2372
	[Header("Gameplay")]
	public bool m_isWet;

	// Token: 0x04000945 RID: 2373
	public bool m_isFreezing;

	// Token: 0x04000946 RID: 2374
	public bool m_isFreezingAtNight;

	// Token: 0x04000947 RID: 2375
	public bool m_isCold;

	// Token: 0x04000948 RID: 2376
	public bool m_isColdAtNight = true;

	// Token: 0x04000949 RID: 2377
	public bool m_alwaysDark;

	// Token: 0x0400094A RID: 2378
	[Header("Ambience")]
	public Color m_ambColorNight = Color.white;

	// Token: 0x0400094B RID: 2379
	public Color m_ambColorDay = Color.white;

	// Token: 0x0400094C RID: 2380
	[Header("Fog-ambient")]
	public Color m_fogColorNight = Color.white;

	// Token: 0x0400094D RID: 2381
	public Color m_fogColorMorning = Color.white;

	// Token: 0x0400094E RID: 2382
	public Color m_fogColorDay = Color.white;

	// Token: 0x0400094F RID: 2383
	public Color m_fogColorEvening = Color.white;

	// Token: 0x04000950 RID: 2384
	[Header("Fog-sun")]
	public Color m_fogColorSunNight = Color.white;

	// Token: 0x04000951 RID: 2385
	public Color m_fogColorSunMorning = Color.white;

	// Token: 0x04000952 RID: 2386
	public Color m_fogColorSunDay = Color.white;

	// Token: 0x04000953 RID: 2387
	public Color m_fogColorSunEvening = Color.white;

	// Token: 0x04000954 RID: 2388
	[Header("Fog-distance")]
	public float m_fogDensityNight = 0.01f;

	// Token: 0x04000955 RID: 2389
	public float m_fogDensityMorning = 0.01f;

	// Token: 0x04000956 RID: 2390
	public float m_fogDensityDay = 0.01f;

	// Token: 0x04000957 RID: 2391
	public float m_fogDensityEvening = 0.01f;

	// Token: 0x04000958 RID: 2392
	[Header("Sun")]
	public Color m_sunColorNight = Color.white;

	// Token: 0x04000959 RID: 2393
	public Color m_sunColorMorning = Color.white;

	// Token: 0x0400095A RID: 2394
	public Color m_sunColorDay = Color.white;

	// Token: 0x0400095B RID: 2395
	public Color m_sunColorEvening = Color.white;

	// Token: 0x0400095C RID: 2396
	public float m_lightIntensityDay = 1.2f;

	// Token: 0x0400095D RID: 2397
	public float m_lightIntensityNight;

	// Token: 0x0400095E RID: 2398
	public float m_sunAngle = 60f;

	// Token: 0x0400095F RID: 2399
	[Header("Wind")]
	public float m_windMin;

	// Token: 0x04000960 RID: 2400
	public float m_windMax = 1f;

	// Token: 0x04000961 RID: 2401
	[Header("Effects")]
	public GameObject m_envObject;

	// Token: 0x04000962 RID: 2402
	public GameObject[] m_psystems;

	// Token: 0x04000963 RID: 2403
	public bool m_psystemsOutsideOnly;

	// Token: 0x04000964 RID: 2404
	public float m_rainCloudAlpha;

	// Token: 0x04000965 RID: 2405
	[Header("Audio")]
	public AudioClip m_ambientLoop;

	// Token: 0x04000966 RID: 2406
	public float m_ambientVol = 0.3f;

	// Token: 0x04000967 RID: 2407
	public string m_ambientList = "";

	// Token: 0x04000968 RID: 2408
	[Header("Music overrides")]
	public string m_musicMorning = "";

	// Token: 0x04000969 RID: 2409
	public string m_musicEvening = "";

	// Token: 0x0400096A RID: 2410
	[FormerlySerializedAs("m_musicRandomDay")]
	public string m_musicDay = "";

	// Token: 0x0400096B RID: 2411
	[FormerlySerializedAs("m_musicRandomNight")]
	public string m_musicNight = "";
}
