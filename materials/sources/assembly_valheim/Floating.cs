using System;
using UnityEngine;

// Token: 0x020000CF RID: 207
public class Floating : MonoBehaviour, IWaterInteractable
{
	// Token: 0x06000D5A RID: 3418 RVA: 0x0005F2FC File Offset: 0x0005D4FC
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_collider = base.GetComponentInChildren<Collider>();
		this.SetSurfaceEffect(false);
		base.InvokeRepeating("TerrainCheck", UnityEngine.Random.Range(10f, 30f), 30f);
	}

	// Token: 0x06000D5B RID: 3419 RVA: 0x0000590F File Offset: 0x00003B0F
	public Transform GetTransform()
	{
		if (this == null)
		{
			return null;
		}
		return base.transform;
	}

	// Token: 0x06000D5C RID: 3420 RVA: 0x0005F353 File Offset: 0x0005D553
	public bool IsOwner()
	{
		return this.m_nview.IsValid() && this.m_nview.IsOwner();
	}

	// Token: 0x06000D5D RID: 3421 RVA: 0x0005F370 File Offset: 0x0005D570
	private void TerrainCheck()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		float groundHeight = ZoneSystem.instance.GetGroundHeight(base.transform.position);
		if (base.transform.position.y - groundHeight < -1f)
		{
			Vector3 position = base.transform.position;
			position.y = groundHeight + 1f;
			base.transform.position = position;
			Rigidbody component = base.GetComponent<Rigidbody>();
			if (component)
			{
				component.velocity = Vector3.zero;
			}
			ZLog.Log("Moved up item " + base.gameObject.name);
		}
	}

	// Token: 0x06000D5E RID: 3422 RVA: 0x0005F424 File Offset: 0x0005D624
	private void FixedUpdate()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.IsInWater())
		{
			this.SetSurfaceEffect(false);
			return;
		}
		this.UpdateImpactEffect();
		float floatDepth = this.GetFloatDepth();
		if (floatDepth > 0f)
		{
			this.SetSurfaceEffect(false);
			return;
		}
		this.SetSurfaceEffect(true);
		Vector3 position = this.m_collider.ClosestPoint(base.transform.position + Vector3.down * 1000f);
		Vector3 worldCenterOfMass = this.m_body.worldCenterOfMass;
		float d = Mathf.Clamp01(Mathf.Abs(floatDepth) / this.m_forceDistance);
		Vector3 vector = Vector3.up * this.m_force * d * (Time.fixedDeltaTime * 50f);
		this.m_body.WakeUp();
		this.m_body.AddForceAtPosition(vector * this.m_balanceForceFraction, position, ForceMode.VelocityChange);
		this.m_body.AddForceAtPosition(vector, worldCenterOfMass, ForceMode.VelocityChange);
		this.m_body.velocity = this.m_body.velocity - this.m_body.velocity * this.m_damping * d;
		this.m_body.angularVelocity = this.m_body.angularVelocity - this.m_body.angularVelocity * this.m_damping * d;
	}

	// Token: 0x06000D5F RID: 3423 RVA: 0x0005F594 File Offset: 0x0005D794
	public bool IsInWater()
	{
		return this.m_inWater > -10000f;
	}

	// Token: 0x06000D60 RID: 3424 RVA: 0x0005F5A3 File Offset: 0x0005D7A3
	private void SetSurfaceEffect(bool enabled)
	{
		if (this.m_surfaceEffects != null)
		{
			this.m_surfaceEffects.SetActive(enabled);
		}
	}

	// Token: 0x06000D61 RID: 3425 RVA: 0x0005F5C0 File Offset: 0x0005D7C0
	private void UpdateImpactEffect()
	{
		if (!this.m_body.IsSleeping() && this.m_impactEffects.HasEffects())
		{
			Vector3 vector = this.m_collider.ClosestPoint(base.transform.position + Vector3.down * 1000f);
			if (vector.y < this.m_inWater)
			{
				if (!this.m_wasInWater)
				{
					this.m_wasInWater = true;
					Vector3 pos = vector;
					pos.y = this.m_inWater;
					if (this.m_body.GetPointVelocity(vector).magnitude > Floating.m_minImpactEffectVelocity)
					{
						this.m_impactEffects.Create(pos, Quaternion.identity, null, 1f);
						return;
					}
				}
			}
			else
			{
				this.m_wasInWater = false;
			}
		}
	}

	// Token: 0x06000D62 RID: 3426 RVA: 0x0005F67F File Offset: 0x0005D87F
	private float GetFloatDepth()
	{
		return this.m_body.worldCenterOfMass.y - this.m_inWater - this.m_waterLevelOffset;
	}

	// Token: 0x06000D63 RID: 3427 RVA: 0x0005F69F File Offset: 0x0005D89F
	public void SetInWater(float waterLevel)
	{
		this.m_inWater = waterLevel;
		if (!this.m_beenInWater && waterLevel > -10000f && this.GetFloatDepth() < 0f)
		{
			this.m_beenInWater = true;
		}
	}

	// Token: 0x06000D64 RID: 3428 RVA: 0x0005F6CC File Offset: 0x0005D8CC
	public bool BeenInWater()
	{
		return this.m_beenInWater;
	}

	// Token: 0x06000D65 RID: 3429 RVA: 0x0005F6D4 File Offset: 0x0005D8D4
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(base.transform.position + Vector3.down * this.m_waterLevelOffset, new Vector3(1f, 0.05f, 1f));
	}

	// Token: 0x04000C36 RID: 3126
	public float m_waterLevelOffset;

	// Token: 0x04000C37 RID: 3127
	public float m_forceDistance = 1f;

	// Token: 0x04000C38 RID: 3128
	public float m_force = 0.5f;

	// Token: 0x04000C39 RID: 3129
	public float m_balanceForceFraction = 0.02f;

	// Token: 0x04000C3A RID: 3130
	public float m_damping = 0.05f;

	// Token: 0x04000C3B RID: 3131
	private static float m_minImpactEffectVelocity = 0.5f;

	// Token: 0x04000C3C RID: 3132
	public EffectList m_impactEffects = new EffectList();

	// Token: 0x04000C3D RID: 3133
	public GameObject m_surfaceEffects;

	// Token: 0x04000C3E RID: 3134
	private float m_inWater = -10000f;

	// Token: 0x04000C3F RID: 3135
	private bool m_beenInWater;

	// Token: 0x04000C40 RID: 3136
	private bool m_wasInWater = true;

	// Token: 0x04000C41 RID: 3137
	private Rigidbody m_body;

	// Token: 0x04000C42 RID: 3138
	private Collider m_collider;

	// Token: 0x04000C43 RID: 3139
	private ZNetView m_nview;
}
