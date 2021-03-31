using System;
using UnityEngine;

// Token: 0x02000097 RID: 151
public class DepthCamera : MonoBehaviour
{
	// Token: 0x06000A11 RID: 2577 RVA: 0x00048DBF File Offset: 0x00046FBF
	private void Start()
	{
		this.m_camera = base.GetComponent<Camera>();
		base.InvokeRepeating("RenderDepth", this.m_updateInterval, this.m_updateInterval);
	}

	// Token: 0x06000A12 RID: 2578 RVA: 0x00048DE4 File Offset: 0x00046FE4
	private void RenderDepth()
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		Vector3 vector = (Player.m_localPlayer ? Player.m_localPlayer.transform.position : mainCamera.transform.position) + Vector3.up * this.m_offset;
		vector.x = Mathf.Round(vector.x);
		vector.y = Mathf.Round(vector.y);
		vector.z = Mathf.Round(vector.z);
		base.transform.position = vector;
		float lodBias = QualitySettings.lodBias;
		QualitySettings.lodBias = 10f;
		this.m_camera.RenderWithShader(this.m_depthShader, "RenderType");
		QualitySettings.lodBias = lodBias;
		Shader.SetGlobalTexture("_SkyAlphaTexture", this.m_texture);
		Shader.SetGlobalVector("_SkyAlphaPosition", base.transform.position);
	}

	// Token: 0x04000933 RID: 2355
	public Shader m_depthShader;

	// Token: 0x04000934 RID: 2356
	public float m_offset = 50f;

	// Token: 0x04000935 RID: 2357
	public RenderTexture m_texture;

	// Token: 0x04000936 RID: 2358
	public float m_updateInterval = 1f;

	// Token: 0x04000937 RID: 2359
	private Camera m_camera;
}
