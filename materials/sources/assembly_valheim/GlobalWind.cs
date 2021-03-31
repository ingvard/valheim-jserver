using System;
using UnityEngine;

// Token: 0x0200003B RID: 59
public class GlobalWind : MonoBehaviour
{
	// Token: 0x06000430 RID: 1072 RVA: 0x00021E24 File Offset: 0x00020024
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

	// Token: 0x06000431 RID: 1073 RVA: 0x00021EB0 File Offset: 0x000200B0
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

	// Token: 0x04000438 RID: 1080
	public float m_multiplier = 1f;

	// Token: 0x04000439 RID: 1081
	public bool m_smoothUpdate;

	// Token: 0x0400043A RID: 1082
	public bool m_alignToWindDirection;

	// Token: 0x0400043B RID: 1083
	[Header("Particles")]
	public bool m_particleVelocity = true;

	// Token: 0x0400043C RID: 1084
	public bool m_particleForce;

	// Token: 0x0400043D RID: 1085
	public bool m_particleEmission;

	// Token: 0x0400043E RID: 1086
	public int m_particleEmissionMin;

	// Token: 0x0400043F RID: 1087
	public int m_particleEmissionMax = 1;

	// Token: 0x04000440 RID: 1088
	[Header("Cloth")]
	public float m_clothRandomAccelerationFactor = 0.5f;

	// Token: 0x04000441 RID: 1089
	public bool m_checkPlayerShelter;

	// Token: 0x04000442 RID: 1090
	private ParticleSystem m_ps;

	// Token: 0x04000443 RID: 1091
	private Cloth m_cloth;

	// Token: 0x04000444 RID: 1092
	private Player m_player;
}
