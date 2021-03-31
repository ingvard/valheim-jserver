using System;
using UnityEngine;

// Token: 0x020000EA RID: 234
public class RandomFlyingBird : MonoBehaviour
{
	// Token: 0x06000E8B RID: 3723 RVA: 0x00067E34 File Offset: 0x00066034
	private void Start()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_anim = base.GetComponentInChildren<ZSyncAnimation>();
		this.m_lodGroup = base.GetComponent<LODGroup>();
		this.m_landedModel.SetActive(true);
		this.m_flyingModel.SetActive(true);
		if (RandomFlyingBird.flapping == 0)
		{
			RandomFlyingBird.flapping = ZSyncAnimation.GetHash("flapping");
		}
		this.m_spawnPoint = this.m_nview.GetZDO().GetVec3("spawnpoint", base.transform.position);
		if (this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set("spawnpoint", this.m_spawnPoint);
		}
		this.m_randomNoiseTimer = UnityEngine.Random.Range(this.m_randomNoiseIntervalMin, this.m_randomNoiseIntervalMax);
		if (this.m_nview.IsOwner())
		{
			this.RandomizeWaypoint(false);
		}
		if (this.m_lodGroup)
		{
			this.m_originalLocalRef = this.m_lodGroup.localReferencePoint;
		}
	}

	// Token: 0x06000E8C RID: 3724 RVA: 0x00067F2C File Offset: 0x0006612C
	private void FixedUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		bool flag = EnvMan.instance.IsDaylight();
		this.m_randomNoiseTimer -= fixedDeltaTime;
		if (this.m_randomNoiseTimer <= 0f)
		{
			if (flag || !this.m_noNoiseAtNight)
			{
				this.m_randomNoise.Create(base.transform.position, Quaternion.identity, base.transform, 1f);
			}
			this.m_randomNoiseTimer = UnityEngine.Random.Range(this.m_randomNoiseIntervalMin, this.m_randomNoiseIntervalMax);
		}
		bool @bool = this.m_nview.GetZDO().GetBool("landed", false);
		this.m_landedModel.SetActive(@bool);
		this.m_flyingModel.SetActive(!@bool);
		this.SetVisible(this.m_nview.HasOwner());
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		this.m_flyTimer += fixedDeltaTime;
		this.m_modeTimer += fixedDeltaTime;
		if (@bool)
		{
			Vector3 forward = base.transform.forward;
			forward.y = 0f;
			forward.Normalize();
			base.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
			this.m_landedTimer += fixedDeltaTime;
			if (((flag || !this.m_noRandomFlightAtNight) && this.m_landedTimer > this.m_landDuration) || this.DangerNearby(base.transform.position))
			{
				this.m_nview.GetZDO().Set("landed", false);
				this.RandomizeWaypoint(false);
				return;
			}
		}
		else
		{
			if (this.m_flapping)
			{
				if (this.m_modeTimer > this.m_flapDuration)
				{
					this.m_modeTimer = 0f;
					this.m_flapping = false;
				}
			}
			else if (this.m_modeTimer > this.m_sailDuration)
			{
				this.m_flapping = true;
				this.m_modeTimer = 0f;
			}
			this.m_anim.SetBool(RandomFlyingBird.flapping, this.m_flapping);
			Vector3 vector = Vector3.Normalize(this.m_waypoint - base.transform.position);
			float num = this.m_groundwp ? (this.m_turnRate * 4f) : this.m_turnRate;
			Vector3 vector2 = Vector3.RotateTowards(base.transform.forward, vector, num * 0.017453292f * fixedDeltaTime, 1f);
			float num2 = Vector3.SignedAngle(base.transform.forward, vector, Vector3.up);
			Vector3 a = Vector3.Cross(vector2, Vector3.up);
			Vector3 a2 = Vector3.up;
			if (num2 > 0f)
			{
				a2 += -a * 1.5f * Utils.LerpStep(0f, 45f, num2);
			}
			else
			{
				a2 += a * 1.5f * Utils.LerpStep(0f, 45f, -num2);
			}
			float num3 = this.m_speed;
			bool flag2 = false;
			if (this.m_groundwp)
			{
				float num4 = Vector3.Distance(base.transform.position, this.m_waypoint);
				if (num4 < 5f)
				{
					num3 *= Mathf.Clamp(num4 / 5f, 0.2f, 1f);
					vector2.y = 0f;
					vector2.Normalize();
					a2 = Vector3.up;
					flag2 = true;
				}
				if (num4 < 0.2f)
				{
					base.transform.position = this.m_waypoint;
					this.m_nview.GetZDO().Set("landed", true);
					this.m_landedTimer = 0f;
					this.m_flapping = true;
					this.m_modeTimer = 0f;
				}
			}
			else if (this.m_flyTimer >= this.m_wpDuration)
			{
				bool ground = UnityEngine.Random.value < this.m_landChance;
				this.RandomizeWaypoint(ground);
			}
			Quaternion to = Quaternion.LookRotation(vector2, a2.normalized);
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, 200f * fixedDeltaTime);
			if (flag2)
			{
				base.transform.position += vector * num3 * fixedDeltaTime;
				return;
			}
			base.transform.position += base.transform.forward * num3 * fixedDeltaTime;
		}
	}

	// Token: 0x06000E8D RID: 3725 RVA: 0x00068388 File Offset: 0x00066588
	private void RandomizeWaypoint(bool ground)
	{
		this.m_flyTimer = 0f;
		Vector3 waypoint;
		if (ground && this.FindLandingPoint(out waypoint))
		{
			this.m_waypoint = waypoint;
			this.m_groundwp = true;
			return;
		}
		Vector2 vector = UnityEngine.Random.insideUnitCircle * this.m_flyRange;
		this.m_waypoint = this.m_spawnPoint + new Vector3(vector.x, 0f, vector.y);
		float num;
		if (ZoneSystem.instance.GetSolidHeight(this.m_waypoint, out num))
		{
			float num2 = ZoneSystem.instance.m_waterLevel + 2f;
			if (num < num2)
			{
				num = num2;
			}
			this.m_waypoint.y = num + UnityEngine.Random.Range(this.m_minAlt, this.m_maxAlt);
		}
		this.m_groundwp = false;
	}

	// Token: 0x06000E8E RID: 3726 RVA: 0x00068444 File Offset: 0x00066644
	private bool FindLandingPoint(out Vector3 waypoint)
	{
		waypoint = new Vector3(0f, -999f, 0f);
		bool result = false;
		for (int i = 0; i < 10; i++)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle * this.m_flyRange;
			Vector3 vector2 = this.m_spawnPoint + new Vector3(vector.x, 0f, vector.y);
			float num;
			if (ZoneSystem.instance.GetSolidHeight(vector2, out num) && num > ZoneSystem.instance.m_waterLevel && num > waypoint.y)
			{
				vector2.y = num;
				if (!this.DangerNearby(vector2))
				{
					waypoint = vector2;
					result = true;
				}
			}
		}
		return result;
	}

	// Token: 0x06000E8F RID: 3727 RVA: 0x000684F0 File Offset: 0x000666F0
	private bool DangerNearby(Vector3 p)
	{
		return Player.IsPlayerInRange(p, this.m_avoidDangerDistance);
	}

	// Token: 0x06000E90 RID: 3728 RVA: 0x00068504 File Offset: 0x00066704
	private void SetVisible(bool visible)
	{
		if (this.m_lodGroup == null)
		{
			return;
		}
		if (this.m_lodVisible == visible)
		{
			return;
		}
		this.m_lodVisible = visible;
		if (this.m_lodVisible)
		{
			this.m_lodGroup.localReferencePoint = this.m_originalLocalRef;
			return;
		}
		this.m_lodGroup.localReferencePoint = new Vector3(999999f, 999999f, 999999f);
	}

	// Token: 0x04000D5F RID: 3423
	public float m_flyRange = 20f;

	// Token: 0x04000D60 RID: 3424
	public float m_minAlt = 5f;

	// Token: 0x04000D61 RID: 3425
	public float m_maxAlt = 20f;

	// Token: 0x04000D62 RID: 3426
	public float m_speed = 10f;

	// Token: 0x04000D63 RID: 3427
	public float m_turnRate = 10f;

	// Token: 0x04000D64 RID: 3428
	public float m_wpDuration = 4f;

	// Token: 0x04000D65 RID: 3429
	public float m_flapDuration = 2f;

	// Token: 0x04000D66 RID: 3430
	public float m_sailDuration = 4f;

	// Token: 0x04000D67 RID: 3431
	public float m_landChance = 0.5f;

	// Token: 0x04000D68 RID: 3432
	public float m_landDuration = 2f;

	// Token: 0x04000D69 RID: 3433
	public float m_avoidDangerDistance = 4f;

	// Token: 0x04000D6A RID: 3434
	public bool m_noRandomFlightAtNight = true;

	// Token: 0x04000D6B RID: 3435
	public float m_randomNoiseIntervalMin = 3f;

	// Token: 0x04000D6C RID: 3436
	public float m_randomNoiseIntervalMax = 6f;

	// Token: 0x04000D6D RID: 3437
	public bool m_noNoiseAtNight = true;

	// Token: 0x04000D6E RID: 3438
	public EffectList m_randomNoise = new EffectList();

	// Token: 0x04000D6F RID: 3439
	public GameObject m_flyingModel;

	// Token: 0x04000D70 RID: 3440
	public GameObject m_landedModel;

	// Token: 0x04000D71 RID: 3441
	private Vector3 m_spawnPoint;

	// Token: 0x04000D72 RID: 3442
	private Vector3 m_waypoint;

	// Token: 0x04000D73 RID: 3443
	private bool m_groundwp;

	// Token: 0x04000D74 RID: 3444
	private float m_flyTimer;

	// Token: 0x04000D75 RID: 3445
	private float m_modeTimer;

	// Token: 0x04000D76 RID: 3446
	private float m_randomNoiseTimer;

	// Token: 0x04000D77 RID: 3447
	private ZSyncAnimation m_anim;

	// Token: 0x04000D78 RID: 3448
	private bool m_flapping = true;

	// Token: 0x04000D79 RID: 3449
	private float m_landedTimer;

	// Token: 0x04000D7A RID: 3450
	private static int flapping;

	// Token: 0x04000D7B RID: 3451
	private ZNetView m_nview;

	// Token: 0x04000D7C RID: 3452
	protected LODGroup m_lodGroup;

	// Token: 0x04000D7D RID: 3453
	private Vector3 m_originalLocalRef;

	// Token: 0x04000D7E RID: 3454
	private bool m_lodVisible = true;
}
