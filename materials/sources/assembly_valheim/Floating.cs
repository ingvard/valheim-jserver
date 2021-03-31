using System;
using UnityEngine;

// Token: 0x020000CF RID: 207
public class Floating : MonoBehaviour, IWaterInteractable
{
	// Token: 0x06000D5B RID: 3419 RVA: 0x0005F484 File Offset: 0x0005D684
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_collider = base.GetComponentInChildren<Collider>();
		this.SetSurfaceEffect(false);
		base.InvokeRepeating("TerrainCheck", UnityEngine.Random.Range(10f, 30f), 30f);
	}

	// Token: 0x06000D5C RID: 3420 RVA: 0x00005933 File Offset: 0x00003B33
	public Transform GetTransform()
	{
		if (this == null)
		{
			return null;
		}
		return base.transform;
	}

	// Token: 0x06000D5D RID: 3421 RVA: 0x0005F4DB File Offset: 0x0005D6DB
	public bool IsOwner()
	{
		return this.m_nview.IsValid() && this.m_nview.IsOwner();
	}

	// Token: 0x06000D5E RID: 3422 RVA: 0x0005F4F8 File Offset: 0x0005D6F8
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

	// Token: 0x06000D5F RID: 3423 RVA: 0x0005F5AC File Offset: 0x0005D7AC
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

	// Token: 0x06000D60 RID: 3424 RVA: 0x0005F71C File Offset: 0x0005D91C
	public bool IsInWater()
	{
		return this.m_inWater > -10000f;
	}

	// Token: 0x06000D61 RID: 3425 RVA: 0x0005F72B File Offset: 0x0005D92B
	private void SetSurfaceEffect(bool enabled)
	{
		if (this.m_surfaceEffects != null)
		{
			this.m_surfaceEffects.SetActive(enabled);
		}
	}

	// Token: 0x06000D62 RID: 3426 RVA: 0x0005F748 File Offset: 0x0005D948
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

	// Token: 0x06000D63 RID: 3427 RVA: 0x0005F807 File Offset: 0x0005DA07
	private float GetFloatDepth()
	{
		return this.m_body.worldCenterOfMass.y - this.m_inWater - this.m_waterLevelOffset;
	}

	// Token: 0x06000D64 RID: 3428 RVA: 0x0005F827 File Offset: 0x0005DA27
	public void SetInWater(float waterLevel)
	{
		this.m_inWater = waterLevel;
		if (!this.m_beenInWater && waterLevel > -10000f && this.GetFloatDepth() < 0f)
		{
			this.m_beenInWater = true;
		}
	}

	// Token: 0x06000D65 RID: 3429 RVA: 0x0005F854 File Offset: 0x0005DA54
	public bool BeenInWater()
	{
		return this.m_beenInWater;
	}

	// Token: 0x06000D66 RID: 3430 RVA: 0x0005F85C File Offset: 0x0005DA5C
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(base.transform.position + Vector3.down * this.m_waterLevelOffset, new Vector3(1f, 0.05f, 1f));
	}

	// Token: 0x04000C3C RID: 3132
	public float m_waterLevelOffset;

	// Token: 0x04000C3D RID: 3133
	public float m_forceDistance = 1f;

	// Token: 0x04000C3E RID: 3134
	public float m_force = 0.5f;

	// Token: 0x04000C3F RID: 3135
	public float m_balanceForceFraction = 0.02f;

	// Token: 0x04000C40 RID: 3136
	public float m_damping = 0.05f;

	// Token: 0x04000C41 RID: 3137
	private static float m_minImpactEffectVelocity = 0.5f;

	// Token: 0x04000C42 RID: 3138
	public EffectList m_impactEffects = new EffectList();

	// Token: 0x04000C43 RID: 3139
	public GameObject m_surfaceEffects;

	// Token: 0x04000C44 RID: 3140
	private float m_inWater = -10000f;

	// Token: 0x04000C45 RID: 3141
	private bool m_beenInWater;

	// Token: 0x04000C46 RID: 3142
	private bool m_wasInWater = true;

	// Token: 0x04000C47 RID: 3143
	private Rigidbody m_body;

	// Token: 0x04000C48 RID: 3144
	private Collider m_collider;

	// Token: 0x04000C49 RID: 3145
	private ZNetView m_nview;
}
