using System;
using UnityEngine;

// Token: 0x020000DF RID: 223
[ExecuteInEditMode]
public class MenuScene : MonoBehaviour
{
	// Token: 0x06000E08 RID: 3592 RVA: 0x00063E95 File Offset: 0x00062095
	private void Awake()
	{
		Shader.SetGlobalFloat("_Wet", 0f);
	}

	// Token: 0x06000E09 RID: 3593 RVA: 0x00063EA8 File Offset: 0x000620A8
	private void Update()
	{
		Shader.SetGlobalVector("_SkyboxSunDir", -this.m_dirLight.transform.forward);
		Shader.SetGlobalVector("_SunDir", -this.m_dirLight.transform.forward);
		Shader.SetGlobalColor("_SunFogColor", this.m_sunFogColor);
		Shader.SetGlobalColor("_SunColor", this.m_dirLight.color * this.m_dirLight.intensity);
		Shader.SetGlobalColor("_AmbientColor", RenderSettings.ambientLight);
		RenderSettings.fogColor = this.m_fogColor;
		RenderSettings.fogDensity = this.m_fogDensity;
		RenderSettings.ambientLight = this.m_ambientLightColor;
		Vector3 normalized = this.m_windDir.normalized;
		Shader.SetGlobalVector("_GlobalWindForce", normalized * this.m_windIntensity);
		Shader.SetGlobalVector("_GlobalWind1", new Vector4(normalized.x, normalized.y, normalized.z, this.m_windIntensity));
		Shader.SetGlobalVector("_GlobalWind2", Vector4.one);
		Shader.SetGlobalFloat("_GlobalWindAlpha", 0f);
	}

	// Token: 0x04000CB3 RID: 3251
	public Light m_dirLight;

	// Token: 0x04000CB4 RID: 3252
	public Color m_sunFogColor = Color.white;

	// Token: 0x04000CB5 RID: 3253
	public Color m_fogColor = Color.white;

	// Token: 0x04000CB6 RID: 3254
	public Color m_ambientLightColor = Color.white;

	// Token: 0x04000CB7 RID: 3255
	public float m_fogDensity = 1f;

	// Token: 0x04000CB8 RID: 3256
	public Vector3 m_windDir = Vector3.left;

	// Token: 0x04000CB9 RID: 3257
	public float m_windIntensity = 0.5f;
}
