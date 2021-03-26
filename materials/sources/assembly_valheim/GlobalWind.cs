using System;
using UnityEngine;

// Token: 0x0200003B RID: 59
public class GlobalWind : MonoBehaviour
{
	// Token: 0x0600042F RID: 1071 RVA: 0x00021D70 File Offset: 0x0001FF70
	private void Start()
	{
		if (EnvMan.instance == null)
		{
			return;
		}
		this.m_ps = base.GetComponent<ParticleSystem>();
		this.m_cloth = base.GetComponent<Cloth>();
		if (this.m_checkPlayerShelter)
		{
			this.m_player = base.GetComponentInParent<Player>();
		}
		if (this.m_smoothUpdate)
		{
			base.InvokeRepeating("UpdateWind", 0f, 0.01f);
			return;
		}
		base.InvokeRepeating("UpdateWind", UnityEngine.Random.Range(1.5f, 2.5f), 2f);
		this.UpdateWind();
	}

	// Token: 0x06000430 RID: 1072 RVA: 0x00021DFC File Offset: 0x0001FFFC
	private void UpdateWind()
	{
		if (this.m_alignToWindDirection)
		{
			Vector3 windDir = EnvMan.instance.GetWindDir();
			base.transform.rotation = Quaternion.LookRotation(windDir, Vector3.up);
		}
		if (this.m_ps)
		{
			if (!this.m_ps.emission.enabled)
			{
				return;
			}
			Vector3 windForce = EnvMan.instance.GetWindForce();
			if (this.m_particleVelocity)
			{
				ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = this.m_ps.velocityOverLifetime;
				velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
				velocityOverLifetime.x = windForce.x * this.m_multiplier;
				velocityOverLifetime.z = windForce.z * this.m_multiplier;
			}
			if (this.m_particleForce)
			{
				ParticleSystem.ForceOverLifetimeModule forceOverLifetime = this.m_ps.forceOverLifetime;
				forceOverLifetime.space = ParticleSystemSimulationSpace.World;
				forceOverLifetime.x = windForce.x * this.m_multiplier;
				forceOverLifetime.z = windForce.z * this.m_multiplier;
			}
			if (this.m_particleEmission)
			{
				this.m_ps.emission.rateOverTimeMultiplier = Mathf.Lerp((float)this.m_particleEmissionMin, (float)this.m_particleEmissionMax, EnvMan.instance.GetWindIntensity());
			}
		}
		if (this.m_cloth)
		{
			Vector3 a = EnvMan.instance.GetWindForce();
			if (this.m_checkPlayerShelter && this.m_player != null && this.m_player.InShelter())
			{
				a = Vector3.zero;
			}
			this.m_cloth.externalAcceleration = a * this.m_multiplier;
			this.m_cloth.randomAcceleration = a * this.m_multiplier * this.m_clothRandomAccelerationFactor;
		}
	}

	// Token: 0x04000434 RID: 1076
	public float m_multiplier = 1f;

	// Token: 0x04000435 RID: 1077
	public bool m_smoothUpdate;

	// Token: 0x04000436 RID: 1078
	public bool m_alignToWindDirection;

	// Token: 0x04000437 RID: 1079
	[Header("Particles")]
	public bool m_particleVelocity = true;

	// Token: 0x04000438 RID: 1080
	public bool m_particleForce;

	// Token: 0x04000439 RID: 1081
	public bool m_particleEmission;

	// Token: 0x0400043A RID: 1082
	public int m_particleEmissionMin;

	// Token: 0x0400043B RID: 1083
	public int m_particleEmissionMax = 1;

	// Token: 0x0400043C RID: 1084
	[Header("Cloth")]
	public float m_clothRandomAccelerationFactor = 0.5f;

	// Token: 0x0400043D RID: 1085
	public bool m_checkPlayerShelter;

	// Token: 0x0400043E RID: 1086
	private ParticleSystem m_ps;

	// Token: 0x0400043F RID: 1087
	private Cloth m_cloth;

	// Token: 0x04000440 RID: 1088
	private Player m_player;
}
