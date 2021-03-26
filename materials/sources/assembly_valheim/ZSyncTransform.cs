using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000092 RID: 146
public class ZSyncTransform : MonoBehaviour
{
	// Token: 0x060009D0 RID: 2512 RVA: 0x00047214 File Offset: 0x00045414
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_projectile = base.GetComponent<Projectile>();
		this.m_character = base.GetComponent<Character>();
		if (this.m_nview.GetZDO() == null)
		{
			base.enabled = false;
			return;
		}
		if (this.m_body)
		{
			this.m_isKinematicBody = this.m_body.isKinematic;
			this.m_useGravity = this.m_body.useGravity;
		}
	}

	// Token: 0x060009D1 RID: 2513 RVA: 0x00047295 File Offset: 0x00045495
	private Vector3 GetVelocity()
	{
		if (this.m_body != null)
		{
			return this.m_body.velocity;
		}
		if (this.m_projectile != null)
		{
			return this.m_projectile.GetVelocity();
		}
		return Vector3.zero;
	}

	// Token: 0x060009D2 RID: 2514 RVA: 0x000472D0 File Offset: 0x000454D0
	private Vector3 GetPosition()
	{
		if (!this.m_body)
		{
			return base.transform.position;
		}
		return this.m_body.position;
	}

	// Token: 0x060009D3 RID: 2515 RVA: 0x000472F8 File Offset: 0x000454F8
	private void OwnerSync()
	{
		ZDO zdo = this.m_nview.GetZDO();
		if (!zdo.IsOwner())
		{
			return;
		}
		if (base.transform.position.y < -5000f)
		{
			if (this.m_body)
			{
				this.m_body.velocity = Vector3.zero;
			}
			ZLog.Log("Object fell out of world:" + base.gameObject.name);
			float groundHeight = ZoneSystem.instance.GetGroundHeight(base.transform.position);
			Vector3 position = base.transform.position;
			position.y = groundHeight + 1f;
			base.transform.position = position;
			return;
		}
		if (this.m_syncPosition)
		{
			zdo.SetPosition(this.GetPosition());
			zdo.Set(ZSyncTransform.m_velHash, this.GetVelocity());
			if (this.m_characterParentSync)
			{
				ZDOID id;
				Vector3 value;
				Vector3 value2;
				if (this.m_character.GetRelativePosition(out id, out value, out value2))
				{
					zdo.Set(ZSyncTransform.m_parentIDHash, id);
					zdo.Set(ZSyncTransform.m_relPos, value);
					zdo.Set(ZSyncTransform.m_velHash, value2);
				}
				else
				{
					zdo.Set(ZSyncTransform.m_parentIDHash, ZDOID.None);
				}
			}
		}
		if (this.m_syncRotation && base.transform.hasChanged)
		{
			Quaternion rotation = this.m_body ? this.m_body.rotation : base.transform.rotation;
			zdo.SetRotation(rotation);
		}
		if (this.m_syncScale && base.transform.hasChanged)
		{
			zdo.Set(ZSyncTransform.m_scaleHash, base.transform.localScale);
		}
		if (this.m_body)
		{
			if (this.m_syncBodyVelocity)
			{
				this.m_nview.GetZDO().Set(ZSyncTransform.m_bodyVel, this.m_body.velocity);
				this.m_nview.GetZDO().Set(ZSyncTransform.m_bodyAVel, this.m_body.angularVelocity);
			}
			this.m_body.useGravity = this.m_useGravity;
		}
		base.transform.hasChanged = false;
	}

	// Token: 0x060009D4 RID: 2516 RVA: 0x00047500 File Offset: 0x00045700
	private void SyncPosition(ZDO zdo, float dt)
	{
		if (this.m_characterParentSync && zdo.HasOwner())
		{
			ZDOID zdoid = zdo.GetZDOID(ZSyncTransform.m_parentIDHash);
			if (!zdoid.IsNone())
			{
				GameObject gameObject = ZNetScene.instance.FindInstance(zdoid);
				if (gameObject)
				{
					ZSyncTransform component = gameObject.GetComponent<ZSyncTransform>();
					if (component)
					{
						component.ClientSync(dt);
					}
					Vector3 vector = zdo.GetVec3(ZSyncTransform.m_relPos, Vector3.zero);
					Vector3 vec = zdo.GetVec3(ZSyncTransform.m_velHash, Vector3.zero);
					if (zdo.m_dataRevision != this.m_posRevision)
					{
						this.m_posRevision = zdo.m_dataRevision;
						this.m_targetPosTimer = 0f;
					}
					this.m_targetPosTimer += dt;
					this.m_targetPosTimer = Mathf.Min(this.m_targetPosTimer, 2f);
					vector += vec * this.m_targetPosTimer;
					if (!this.m_haveTempRelPos)
					{
						this.m_haveTempRelPos = true;
						this.m_tempRelPos = vector;
					}
					if (Vector3.Distance(this.m_tempRelPos, vector) > 0.001f)
					{
						this.m_tempRelPos = Vector3.Lerp(this.m_tempRelPos, vector, 0.2f);
						vector = this.m_tempRelPos;
					}
					Vector3 vector2 = gameObject.transform.TransformPoint(vector);
					if (Vector3.Distance(base.transform.position, vector2) > 0.001f)
					{
						base.transform.position = vector2;
					}
					return;
				}
			}
		}
		this.m_haveTempRelPos = false;
		Vector3 vector3 = zdo.GetPosition();
		if (zdo.m_dataRevision != this.m_posRevision)
		{
			this.m_posRevision = zdo.m_dataRevision;
			this.m_targetPosTimer = 0f;
		}
		if (zdo.HasOwner())
		{
			this.m_targetPosTimer += dt;
			this.m_targetPosTimer = Mathf.Min(this.m_targetPosTimer, 2f);
			Vector3 vec2 = zdo.GetVec3(ZSyncTransform.m_velHash, Vector3.zero);
			vector3 += vec2 * this.m_targetPosTimer;
		}
		float num = Vector3.Distance(base.transform.position, vector3);
		if (num > 0.001f)
		{
			base.transform.position = ((num < 5f) ? Vector3.Lerp(base.transform.position, vector3, 0.2f) : vector3);
		}
	}

	// Token: 0x060009D5 RID: 2517 RVA: 0x00047738 File Offset: 0x00045938
	private void ClientSync(float dt)
	{
		ZDO zdo = this.m_nview.GetZDO();
		if (zdo.IsOwner())
		{
			return;
		}
		int frameCount = Time.frameCount;
		if (this.m_lastUpdateFrame == frameCount)
		{
			return;
		}
		this.m_lastUpdateFrame = frameCount;
		if (this.m_isKinematicBody)
		{
			if (this.m_syncPosition)
			{
				Vector3 vector = zdo.GetPosition();
				if (Vector3.Distance(this.m_body.position, vector) > 5f)
				{
					this.m_body.position = vector;
				}
				else
				{
					if (Vector3.Distance(this.m_body.position, vector) > 0.01f)
					{
						vector = Vector3.Lerp(this.m_body.position, vector, 0.2f);
					}
					this.m_body.MovePosition(vector);
				}
			}
			if (this.m_syncRotation)
			{
				Quaternion rotation = zdo.GetRotation();
				if (Quaternion.Angle(this.m_body.rotation, rotation) > 45f)
				{
					this.m_body.rotation = rotation;
				}
				else
				{
					this.m_body.MoveRotation(rotation);
				}
			}
		}
		else
		{
			if (this.m_syncPosition)
			{
				this.SyncPosition(zdo, dt);
			}
			if (this.m_syncRotation)
			{
				Quaternion rotation2 = zdo.GetRotation();
				if (Quaternion.Angle(base.transform.rotation, rotation2) > 0.001f)
				{
					base.transform.rotation = Quaternion.Slerp(base.transform.rotation, rotation2, 0.2f);
				}
			}
			if (this.m_body)
			{
				this.m_body.useGravity = false;
				if (this.m_syncBodyVelocity && this.m_nview.HasOwner())
				{
					Vector3 vec = zdo.GetVec3(ZSyncTransform.m_bodyVel, Vector3.zero);
					Vector3 vec2 = zdo.GetVec3(ZSyncTransform.m_bodyAVel, Vector3.zero);
					if (vec.magnitude > 0.01f || vec2.magnitude > 0.01f)
					{
						this.m_body.velocity = vec;
						this.m_body.angularVelocity = vec2;
					}
					else
					{
						this.m_body.Sleep();
					}
				}
				else if (!this.m_body.IsSleeping())
				{
					this.m_body.velocity = Vector3.zero;
					this.m_body.angularVelocity = Vector3.zero;
					this.m_body.Sleep();
				}
			}
		}
		if (this.m_syncScale)
		{
			Vector3 vec3 = zdo.GetVec3(ZSyncTransform.m_scaleHash, base.transform.localScale);
			base.transform.localScale = vec3;
		}
	}

	// Token: 0x060009D6 RID: 2518 RVA: 0x00047992 File Offset: 0x00045B92
	private void FixedUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.ClientSync(Time.fixedDeltaTime);
	}

	// Token: 0x060009D7 RID: 2519 RVA: 0x000479AD File Offset: 0x00045BAD
	private void LateUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.OwnerSync();
	}

	// Token: 0x060009D8 RID: 2520 RVA: 0x000479AD File Offset: 0x00045BAD
	public void SyncNow()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.OwnerSync();
	}

	// Token: 0x040008DF RID: 2271
	public bool m_syncPosition = true;

	// Token: 0x040008E0 RID: 2272
	public bool m_syncRotation = true;

	// Token: 0x040008E1 RID: 2273
	public bool m_syncScale;

	// Token: 0x040008E2 RID: 2274
	public bool m_syncBodyVelocity;

	// Token: 0x040008E3 RID: 2275
	public bool m_characterParentSync;

	// Token: 0x040008E4 RID: 2276
	private const float m_smoothness = 0.2f;

	// Token: 0x040008E5 RID: 2277
	private bool m_isKinematicBody;

	// Token: 0x040008E6 RID: 2278
	private bool m_useGravity = true;

	// Token: 0x040008E7 RID: 2279
	private Vector3 m_tempRelPos;

	// Token: 0x040008E8 RID: 2280
	private bool m_haveTempRelPos;

	// Token: 0x040008E9 RID: 2281
	private float m_targetPosTimer;

	// Token: 0x040008EA RID: 2282
	private uint m_posRevision;

	// Token: 0x040008EB RID: 2283
	private int m_lastUpdateFrame = -1;

	// Token: 0x040008EC RID: 2284
	private static int m_velHash = "vel".GetStableHashCode();

	// Token: 0x040008ED RID: 2285
	private static int m_scaleHash = "scale".GetStableHashCode();

	// Token: 0x040008EE RID: 2286
	private static int m_bodyVel = "body_vel".GetStableHashCode();

	// Token: 0x040008EF RID: 2287
	private static int m_bodyAVel = "body_avel".GetStableHashCode();

	// Token: 0x040008F0 RID: 2288
	private static int m_relPos = "relPos".GetStableHashCode();

	// Token: 0x040008F1 RID: 2289
	private static KeyValuePair<int, int> m_parentIDHash = ZDO.GetHashZDOID("parentID");

	// Token: 0x040008F2 RID: 2290
	private ZNetView m_nview;

	// Token: 0x040008F3 RID: 2291
	private Rigidbody m_body;

	// Token: 0x040008F4 RID: 2292
	private Projectile m_projectile;

	// Token: 0x040008F5 RID: 2293
	private Character m_character;
}
