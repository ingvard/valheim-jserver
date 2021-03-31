using System;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityStandardAssets.ImageEffects;

// Token: 0x02000033 RID: 51
public class CameraEffects : MonoBehaviour
{
	// Token: 0x17000001 RID: 1
	// (get) Token: 0x06000405 RID: 1029 RVA: 0x00020EAE File Offset: 0x0001F0AE
	public static CameraEffects instance
	{
		get
		{
			return CameraEffects.m_instance;
		}
	}

	// Token: 0x06000406 RID: 1030 RVA: 0x00020EB5 File Offset: 0x0001F0B5
	private void Awake()
	{
		CameraEffects.m_instance = this;
		this.m_postProcessing = base.GetComponent<PostProcessingBehaviour>();
		this.m_dof = base.GetComponent<DepthOfField>();
		this.ApplySettings();
	}

	// Token: 0x06000407 RID: 1031 RVA: 0x00020EDB File Offset: 0x0001F0DB
	private void OnDestroy()
	{
		if (CameraEffects.m_instance == this)
		{
			CameraEffects.m_instance = null;
		}
	}

	// Token: 0x06000408 RID: 1032 RVA: 0x00020EF0 File Offset: 0x0001F0F0
	public void ApplySettings()
	{
		this.SetDof(PlayerPrefs.GetInt("DOF", 1) == 1);
		this.SetBloom(PlayerPrefs.GetInt("Bloom", 1) == 1);
		this.SetSSAO(PlayerPrefs.GetInt("SSAO", 1) == 1);
		this.SetSunShafts(PlayerPrefs.GetInt("SunShafts", 1) == 1);
		this.SetAntiAliasing(PlayerPrefs.GetInt("AntiAliasing", 1) == 1);
		this.SetCA(PlayerPrefs.GetInt("ChromaticAberration", 1) == 1);
		this.SetMotionBlur(PlayerPrefs.GetInt("MotionBlur", 1) == 1);
	}

	// Token: 0x06000409 RID: 1033 RVA: 0x00020FA8 File Offset: 0x0001F1A8
	public void SetSunShafts(bool enabled)
	{
		SunShafts component = base.GetComponent<SunShafts>();
		if (component != null)
		{
			component.enabled = enabled;
		}
	}

	// Token: 0x0600040A RID: 1034 RVA: 0x00020FCC File Offset: 0x0001F1CC
	private void SetBloom(bool enabled)
	{
		this.m_postProcessing.profile.bloom.enabled = enabled;
	}

	// Token: 0x0600040B RID: 1035 RVA: 0x00020FE4 File Offset: 0x0001F1E4
	private void SetSSAO(bool enabled)
	{
		this.m_postProcessing.profile.ambientOcclusion.enabled = enabled;
	}

	// Token: 0x0600040C RID: 1036 RVA: 0x00020FFC File Offset: 0x0001F1FC
	private void SetMotionBlur(bool enabled)
	{
		this.m_postProcessing.profile.motionBlur.enabled = enabled;
	}

	// Token: 0x0600040D RID: 1037 RVA: 0x00021014 File Offset: 0x0001F214
	private void SetAntiAliasing(bool enabled)
	{
		this.m_postProcessing.profile.antialiasing.enabled = enabled;
	}

	// Token: 0x0600040E RID: 1038 RVA: 0x0002102C File Offset: 0x0001F22C
	private void SetCA(bool enabled)
	{
		this.m_postProcessing.profile.chromaticAberration.enabled = enabled;
	}

	// Token: 0x0600040F RID: 1039 RVA: 0x00021044 File Offset: 0x0001F244
	private void SetDof(bool enabled)
	{
		this.m_dof.enabled = (enabled || this.m_forceDof);
	}

	// Token: 0x06000410 RID: 1040 RVA: 0x0002105D File Offset: 0x0001F25D
	private void LateUpdate()
	{
		this.UpdateDOF();
	}

	// Token: 0x06000411 RID: 1041 RVA: 0x00021065 File Offset: 0x0001F265
	private bool ControllingShip()
	{
		return Player.m_localPlayer == null || Player.m_localPlayer.GetControlledShip() != null;
	}

	// Token: 0x06000412 RID: 1042 RVA: 0x0002108C File Offset: 0x0001F28C
	private void UpdateDOF()
	{
		if (!this.m_dof.enabled || !this.m_dofAutoFocus)
		{
			return;
		}
		float num = this.m_dofMaxDistance;
		RaycastHit raycastHit;
		if (Physics.Raycast(base.transform.position, base.transform.forward, out raycastHit, this.m_dofMaxDistance, this.m_dofRayMask))
		{
			num = raycastHit.distance;
		}
		if (this.ControllingShip() && num < this.m_dofMinDistanceShip)
		{
			num = this.m_dofMinDistanceShip;
		}
		if (num < this.m_dofMinDistance)
		{
			num = this.m_dofMinDistance;
		}
		this.m_dof.focalLength = Mathf.Lerp(this.m_dof.focalLength, num, 0.2f);
	}

	// Token: 0x040003FD RID: 1021
	private static CameraEffects m_instance;

	// Token: 0x040003FE RID: 1022
	public bool m_forceDof;

	// Token: 0x040003FF RID: 1023
	public LayerMask m_dofRayMask;

	// Token: 0x04000400 RID: 1024
	public bool m_dofAutoFocus;

	// Token: 0x04000401 RID: 1025
	public float m_dofMinDistance = 50f;

	// Token: 0x04000402 RID: 1026
	public float m_dofMinDistanceShip = 50f;

	// Token: 0x04000403 RID: 1027
	public float m_dofMaxDistance = 3000f;

	// Token: 0x04000404 RID: 1028
	private PostProcessingBehaviour m_postProcessing;

	// Token: 0x04000405 RID: 1029
	private DepthOfField m_dof;
}
