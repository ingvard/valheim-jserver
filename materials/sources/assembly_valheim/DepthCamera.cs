using System;
using UnityEngine;

// Token: 0x02000097 RID: 151
public class DepthCamera : MonoBehaviour
{
	// Token: 0x06000A10 RID: 2576 RVA: 0x00048D13 File Offset: 0x00046F13
	private void Start()
	{
		this.m_camera = base.GetComponent<Camera>();
		base.InvokeRepeating("RenderDepth", this.m_updateInterval, this.m_updateInterval);
	}

	// Token: 0x06000A11 RID: 2577 RVA: 0x00048D38 File Offset: 0x00046F38
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

	// Token: 0x0400092F RID: 2351
	public Shader m_depthShader;

	// Token: 0x04000930 RID: 2352
	public float m_offset = 50f;

	// Token: 0x04000931 RID: 2353
	public RenderTexture m_texture;

	// Token: 0x04000932 RID: 2354
	public float m_updateInterval = 1f;

	// Token: 0x04000933 RID: 2355
	private Camera m_camera;
}
