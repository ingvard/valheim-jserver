using System;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityStandardAssets.ImageEffects;

// Token: 0x02000033 RID: 51
public class CameraEffects : MonoBehaviour
{
	// Token: 0x17000001 RID: 1
	// (get) Token: 0x06000404 RID: 1028 RVA: 0x00020DFA File Offset: 0x0001EFFA
	public static CameraEffects instance
	{
		get
		{
			return CameraEffects.m_instance;
		}
	}

	// Token: 0x06000405 RID: 1029 RVA: 0x00020E01 File Offset: 0x0001F001
	private void Awake()
	{
		CameraEffects.m_instance = this;
		this.m_postProcessing = base.GetComponent<PostProcessingBehaviour>();
		this.m_dof = base.GetComponent<DepthOfField>();
		this.ApplySettings();
	}

	// Token: 0x06000406 RID: 1030 RVA: 0x00020E27 File Offset: 0x0001F027
	private void OnDestroy()
	{
		if (CameraEffects.m_instance == this)
		{
			CameraEffects.m_instance = null;
		}
	}

	// Token: 0x06000407 RID: 1031 RVA: 0x00020E3C File Offset: 0x0001F03C
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

	// Token: 0x06000408 RID: 1032 RVA: 0x00020EF4 File Offset: 0x0001F0F4
	public void SetSunShafts(bool enabled)
	{
		SunShafts component = base.GetComponent<SunShafts>();
		if (component != null)
		{
			component.enabled = enabled;
		}
	}

	// Token: 0x06000409 RID: 1033 RVA: 0x00020F18 File Offset: 0x0001F118
	private void SetBloom(bool enabled)
	{
		this.m_postProcessing.profile.bloom.enabled = enabled;
	}

	// Token: 0x0600040A RID: 1034 RVA: 0x00020F30 File Offset: 0x0001F130
	private void SetSSAO(bool enabled)
	{
		this.m_postProcessing.profile.ambientOcclusion.enabled = enabled;
	}

	// Token: 0x0600040B RID: 1035 RVA: 0x00020F48 File Offset: 0x0001F148
	private void SetMotionBlur(bool enabled)
	{
		this.m_postProcessing.profile.motionBlur.enabled = enabled;
	}

	// Token: 0x0600040C RID: 1036 RVA: 0x00020F60 File Offset: 0x0001F160
	private void SetAntiAliasing(bool enabled)
	{
		this.m_postProcessing.profile.antialiasing.enabled = enabled;
	}

	// Token: 0x0600040D RID: 1037 RVA: 0x00020F78 File Offset: 0x0001F178
	private void SetCA(bool enabled)
	{
		this.m_postProcessing.profile.chromaticAberration.enabled = enabled;
	}

	// Token: 0x0600040E RID: 1038 RVA: 0x00020F90 File Offset: 0x0001F190
	private void SetDof(bool enabled)
	{
		this.m_dof.enabled = (enabled || this.m_forceDof);
	}

	// Token: 0x0600040F RID: 1039 RVA: 0x00020FA9 File Offset: 0x0001F1A9
	private void LateUpdate()
	{
		this.UpdateDOF();
	}

	// Token: 0x06000410 RID: 1040 RVA: 0x00020FB1 File Offset: 0x0001F1B1
	private bool ControllingShip()
	{
		return Player.m_localPlayer == null || Player.m_localPlayer.GetControlledShip() != null;
	}

	// Token: 0x06000411 RID: 1041 RVA: 0x00020FD8 File Offset: 0x0001F1D8
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

	// Token: 0x040003F9 RID: 1017
	private static CameraEffects m_instance;

	// Token: 0x040003FA RID: 1018
	public bool m_forceDof;

	// Token: 0x040003FB RID: 1019
	public LayerMask m_dofRayMask;

	// Token: 0x040003FC RID: 1020
	public bool m_dofAutoFocus;

	// Token: 0x040003FD RID: 1021
	public float m_dofMinDistance = 50f;

	// Token: 0x040003FE RID: 1022
	public float m_dofMinDistanceShip = 50f;

	// Token: 0x040003FF RID: 1023
	public float m_dofMaxDistance = 3000f;

	// Token: 0x04000400 RID: 1024
	private PostProcessingBehaviour m_postProcessing;

	// Token: 0x04000401 RID: 1025
	private DepthOfField m_dof;
}
