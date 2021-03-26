using System;
using UnityEngine;

// Token: 0x020000B2 RID: 178
public class StealthSystem : MonoBehaviour
{
	// Token: 0x1700002E RID: 46
	// (get) Token: 0x06000BE9 RID: 3049 RVA: 0x00054E3A File Offset: 0x0005303A
	public static StealthSystem instance
	{
		get
		{
			return StealthSystem.m_instance;
		}
	}

	// Token: 0x06000BEA RID: 3050 RVA: 0x00054E41 File Offset: 0x00053041
	private void Awake()
	{
		StealthSystem.m_instance = this;
	}

	// Token: 0x06000BEB RID: 3051 RVA: 0x00054E49 File Offset: 0x00053049
	private void OnDestroy()
	{
		StealthSystem.m_instance = null;
	}

	// Token: 0x06000BEC RID: 3052 RVA: 0x00054E54 File Offset: 0x00053054
	public float GetLightFactor(Vector3 point)
	{
		float lightLevel = this.GetLightLevel(point);
		return Utils.LerpStep(this.m_minLightLevel, this.m_maxLightLevel, lightLevel);
	}

	// Token: 0x06000BED RID: 3053 RVA: 0x00054E7C File Offset: 0x0005307C
	public float GetLightLevel(Vector3 point)
	{
		if (Time.time - this.m_lastLightListUpdate > 1f)
		{
			this.m_lastLightListUpdate = Time.time;
			this.m_allLights = UnityEngine.Object.FindObjectsOfType<Light>();
		}
		float num = RenderSettings.ambientIntensity * RenderSettings.ambientLight.grayscale;
		foreach (Light light in this.m_allLights)
		{
			if (!(light == null))
			{
				if (light.type == LightType.Directional)
				{
					float num2 = 1f;
					if (light.shadows != LightShadows.None && (Physics.Raycast(point - light.transform.forward * 1000f, light.transform.forward, 1000f, this.m_shadowTestMask) || Physics.Raycast(point, -light.transform.forward, 1000f, this.m_shadowTestMask)))
					{
						num2 = 1f - light.shadowStrength;
					}
					float num3 = light.intensity * light.color.grayscale * num2;
					num += num3;
				}
				else
				{
					float num4 = Vector3.Distance(light.transform.position, point);
					if (num4 <= light.range)
					{
						float num5 = 1f;
						if (light.shadows != LightShadows.None)
						{
							Vector3 vector = point - light.transform.position;
							if (Physics.Raycast(light.transform.position, vector.normalized, vector.magnitude, this.m_shadowTestMask) || Physics.Raycast(point, -vector.normalized, vector.magnitude, this.m_shadowTestMask))
							{
								num5 = 1f - light.shadowStrength;
							}
						}
						float num6 = 1f - num4 / light.range;
						float num7 = light.intensity * light.color.grayscale * num6 * num5;
						num += num7;
					}
				}
			}
		}
		return num;
	}

	// Token: 0x04000B0B RID: 2827
	private static StealthSystem m_instance;

	// Token: 0x04000B0C RID: 2828
	public LayerMask m_shadowTestMask;

	// Token: 0x04000B0D RID: 2829
	public float m_minLightLevel = 0.2f;

	// Token: 0x04000B0E RID: 2830
	public float m_maxLightLevel = 1.6f;

	// Token: 0x04000B0F RID: 2831
	private Light[] m_allLights;

	// Token: 0x04000B10 RID: 2832
	private float m_lastLightListUpdate;

	// Token: 0x04000B11 RID: 2833
	private const float m_lightUpdateInterval = 1f;
}
