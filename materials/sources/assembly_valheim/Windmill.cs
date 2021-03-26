using System;
using UnityEngine;

// Token: 0x02000114 RID: 276
public class Windmill : MonoBehaviour
{
	// Token: 0x0600103B RID: 4155 RVA: 0x0007264F File Offset: 0x0007084F
	private void Start()
	{
		this.m_smelter = base.GetComponent<Smelter>();
		base.InvokeRepeating("CheckCover", 0.1f, 5f);
	}

	// Token: 0x0600103C RID: 4156 RVA: 0x00072674 File Offset: 0x00070874
	private void Update()
	{
		Quaternion to = Quaternion.LookRotation(-EnvMan.instance.GetWindDir());
		float powerOutput = this.GetPowerOutput();
		this.m_bom.rotation = Quaternion.RotateTowards(this.m_bom.rotation, to, this.m_bomRotationSpeed * powerOutput * Time.deltaTime);
		float num = powerOutput * this.m_propellerRotationSpeed;
		this.m_propAngle += num * Time.deltaTime;
		this.m_propeller.localRotation = Quaternion.Euler(0f, 0f, this.m_propAngle);
		if (this.m_smelter == null || this.m_smelter.IsActive())
		{
			this.m_grindStoneAngle += powerOutput * this.m_grindstoneRotationSpeed * Time.deltaTime;
		}
		this.m_grindstone.localRotation = Quaternion.Euler(0f, this.m_grindStoneAngle, 0f);
		this.m_propellerAOE.SetActive(Mathf.Abs(num) > this.m_minAOEPropellerSpeed);
		this.UpdateAudio(Time.deltaTime);
	}

	// Token: 0x0600103D RID: 4157 RVA: 0x00072780 File Offset: 0x00070980
	public float GetPowerOutput()
	{
		float num = Utils.LerpStep(this.m_minWindSpeed, 1f, EnvMan.instance.GetWindIntensity());
		return (1f - this.m_cover) * num;
	}

	// Token: 0x0600103E RID: 4158 RVA: 0x000727B8 File Offset: 0x000709B8
	private void CheckCover()
	{
		bool flag;
		Cover.GetCoverForPoint(this.m_propeller.transform.position, out this.m_cover, out flag);
	}

	// Token: 0x0600103F RID: 4159 RVA: 0x000727E4 File Offset: 0x000709E4
	private void UpdateAudio(float dt)
	{
		float powerOutput = this.GetPowerOutput();
		float target = Mathf.Lerp(this.m_minPitch, this.m_maxPitch, Mathf.Clamp01(powerOutput / this.m_maxPitchVel));
		float target2 = this.m_maxVol * Mathf.Clamp01(powerOutput / this.m_maxVolVel);
		foreach (AudioSource audioSource in this.m_sfxLoops)
		{
			audioSource.volume = Mathf.MoveTowards(audioSource.volume, target2, this.m_audioChangeSpeed * dt);
			audioSource.pitch = Mathf.MoveTowards(audioSource.pitch, target, this.m_audioChangeSpeed * dt);
		}
	}

	// Token: 0x04000F1E RID: 3870
	public Transform m_propeller;

	// Token: 0x04000F1F RID: 3871
	public Transform m_grindstone;

	// Token: 0x04000F20 RID: 3872
	public Transform m_bom;

	// Token: 0x04000F21 RID: 3873
	public AudioSource[] m_sfxLoops;

	// Token: 0x04000F22 RID: 3874
	public GameObject m_propellerAOE;

	// Token: 0x04000F23 RID: 3875
	public float m_minAOEPropellerSpeed = 5f;

	// Token: 0x04000F24 RID: 3876
	public float m_bomRotationSpeed = 10f;

	// Token: 0x04000F25 RID: 3877
	public float m_propellerRotationSpeed = 10f;

	// Token: 0x04000F26 RID: 3878
	public float m_grindstoneRotationSpeed = 10f;

	// Token: 0x04000F27 RID: 3879
	public float m_minWindSpeed = 0.1f;

	// Token: 0x04000F28 RID: 3880
	public float m_minPitch = 1f;

	// Token: 0x04000F29 RID: 3881
	public float m_maxPitch = 1.5f;

	// Token: 0x04000F2A RID: 3882
	public float m_maxPitchVel = 10f;

	// Token: 0x04000F2B RID: 3883
	public float m_maxVol = 1f;

	// Token: 0x04000F2C RID: 3884
	public float m_maxVolVel = 10f;

	// Token: 0x04000F2D RID: 3885
	public float m_audioChangeSpeed = 2f;

	// Token: 0x04000F2E RID: 3886
	private float m_cover;

	// Token: 0x04000F2F RID: 3887
	private float m_propAngle;

	// Token: 0x04000F30 RID: 3888
	private float m_grindStoneAngle;

	// Token: 0x04000F31 RID: 3889
	private Smelter m_smelter;
}
