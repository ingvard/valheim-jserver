using System;
using System.Collections;
using UnityEngine;

// Token: 0x020000DC RID: 220
public class LightLod : MonoBehaviour
{
	// Token: 0x06000DF7 RID: 3575 RVA: 0x00063AA0 File Offset: 0x00061CA0
	private void Awake()
	{
		this.m_light = base.GetComponent<Light>();
		this.m_baseRange = this.m_light.range;
		this.m_baseShadowStrength = this.m_light.shadowStrength;
		if (this.m_shadowLod && this.m_light.shadows == LightShadows.None)
		{
			this.m_shadowLod = false;
		}
		if (this.m_lightLod)
		{
			this.m_light.range = 0f;
			this.m_light.enabled = false;
		}
		if (this.m_shadowLod)
		{
			this.m_light.shadowStrength = 0f;
			this.m_light.shadows = LightShadows.None;
		}
	}

	// Token: 0x06000DF8 RID: 3576 RVA: 0x00053B07 File Offset: 0x00051D07
	private void OnEnable()
	{
		base.StartCoroutine("UpdateLoop");
	}

	// Token: 0x06000DF9 RID: 3577 RVA: 0x00063B3F File Offset: 0x00061D3F
	private IEnumerator UpdateLoop()
	{
		for (;;)
		{
			Camera mainCamera = Utils.GetMainCamera();
			if (mainCamera && this.m_light)
			{
				float distance = Vector3.Distance(mainCamera.transform.position, base.transform.position);
				if (this.m_lightLod)
				{
					if (distance < this.m_lightDistance)
					{
						while (this.m_light)
						{
							if (this.m_light.range >= this.m_baseRange && this.m_light.enabled)
							{
								break;
							}
							this.m_light.enabled = true;
							this.m_light.range = Mathf.Min(this.m_baseRange, this.m_light.range + Time.deltaTime * this.m_baseRange);
							yield return null;
						}
					}
					else
					{
						while (this.m_light && (this.m_light.range > 0f || this.m_light.enabled))
						{
							this.m_light.range = Mathf.Max(0f, this.m_light.range - Time.deltaTime * this.m_baseRange);
							if (this.m_light.range <= 0f)
							{
								this.m_light.enabled = false;
							}
							yield return null;
						}
					}
				}
				if (this.m_shadowLod)
				{
					if (distance < this.m_shadowDistance)
					{
						while (this.m_light)
						{
							if (this.m_light.shadowStrength >= this.m_baseShadowStrength && this.m_light.shadows != LightShadows.None)
							{
								break;
							}
							this.m_light.shadows = LightShadows.Soft;
							this.m_light.shadowStrength = Mathf.Min(this.m_baseShadowStrength, this.m_light.shadowStrength + Time.deltaTime * this.m_baseShadowStrength);
							yield return null;
						}
					}
					else
					{
						while (this.m_light && (this.m_light.shadowStrength > 0f || this.m_light.shadows != LightShadows.None))
						{
							this.m_light.shadowStrength = Mathf.Max(0f, this.m_light.shadowStrength - Time.deltaTime * this.m_baseShadowStrength);
							if (this.m_light.shadowStrength <= 0f)
							{
								this.m_light.shadows = LightShadows.None;
							}
							yield return null;
						}
					}
				}
			}
			yield return new WaitForSeconds(1f);
		}
		yield break;
	}

	// Token: 0x04000CA6 RID: 3238
	public bool m_lightLod = true;

	// Token: 0x04000CA7 RID: 3239
	public float m_lightDistance = 40f;

	// Token: 0x04000CA8 RID: 3240
	public bool m_shadowLod = true;

	// Token: 0x04000CA9 RID: 3241
	public float m_shadowDistance = 20f;

	// Token: 0x04000CAA RID: 3242
	private Light m_light;

	// Token: 0x04000CAB RID: 3243
	private float m_baseRange;

	// Token: 0x04000CAC RID: 3244
	private float m_baseShadowStrength;
}
