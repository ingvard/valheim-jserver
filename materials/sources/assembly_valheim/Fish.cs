using System;
using UnityEngine;

// Token: 0x020000CC RID: 204
public class Fish : MonoBehaviour, IWaterInteractable, Hoverable, Interactable
{
	// Token: 0x06000D2E RID: 3374 RVA: 0x0005E0FC File Offset: 0x0005C2FC
	private void Start()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_spawnPoint = this.m_nview.GetZDO().GetVec3("spawnpoint", base.transform.position);
		if (this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set("spawnpoint", this.m_spawnPoint);
		}
		if (this.m_nview.IsOwner())
		{
			this.RandomizeWaypoint(true);
		}
		if (this.m_nview && this.m_nview.IsValid())
		{
			this.m_nview.Register("RequestPickup", new Action<long>(this.RPC_RequestPickup));
			this.m_nview.Register("Pickup", new Action<long>(this.RPC_Pickup));
		}
	}

	// Token: 0x06000D2F RID: 3375 RVA: 0x0005E1D5 File Offset: 0x0005C3D5
	public bool IsOwner()
	{
		return this.m_nview && this.m_nview.IsValid() && this.m_nview.IsOwner();
	}

	// Token: 0x06000D30 RID: 3376 RVA: 0x0005E200 File Offset: 0x0005C400
	public string GetHoverText()
	{
		string text = this.m_name;
		if (this.IsOutOfWater())
		{
			text += "\n[<color=yellow><b>$KEY_Use</b></color>] $inventory_pickup";
		}
		return Localization.instance.Localize(text);
	}

	// Token: 0x06000D31 RID: 3377 RVA: 0x0005E233 File Offset: 0x0005C433
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000D32 RID: 3378 RVA: 0x0005E23B File Offset: 0x0005C43B
	public bool Interact(Humanoid character, bool repeat)
	{
		return !repeat && this.IsOutOfWater() && this.Pickup(character);
	}

	// Token: 0x06000D33 RID: 3379 RVA: 0x0005E258 File Offset: 0x0005C458
	public bool Pickup(Humanoid character)
	{
		if (!character.GetInventory().CanAddItem(this.m_pickupItem, this.m_pickupItemStackSize))
		{
			character.Message(MessageHud.MessageType.Center, "$msg_noroom", 0, null);
			return false;
		}
		this.m_nview.InvokeRPC("RequestPickup", Array.Empty<object>());
		return true;
	}

	// Token: 0x06000D34 RID: 3380 RVA: 0x0005E2A4 File Offset: 0x0005C4A4
	private void RPC_RequestPickup(long uid)
	{
		if (Time.time - this.m_pickupTime > 2f)
		{
			this.m_pickupTime = Time.time;
			this.m_nview.InvokeRPC(uid, "Pickup", Array.Empty<object>());
		}
	}

	// Token: 0x06000D35 RID: 3381 RVA: 0x0005E2DA File Offset: 0x0005C4DA
	private void RPC_Pickup(long uid)
	{
		if (Player.m_localPlayer && Player.m_localPlayer.PickupPrefab(this.m_pickupItem, this.m_pickupItemStackSize) != null)
		{
			this.m_nview.ClaimOwnership();
			this.m_nview.Destroy();
		}
	}

	// Token: 0x06000D36 RID: 3382 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000D37 RID: 3383 RVA: 0x0005E316 File Offset: 0x0005C516
	public void SetInWater(float waterLevel)
	{
		this.m_inWater = waterLevel;
	}

	// Token: 0x06000D38 RID: 3384 RVA: 0x0000590F File Offset: 0x00003B0F
	public Transform GetTransform()
	{
		if (this == null)
		{
			return null;
		}
		return base.transform;
	}

	// Token: 0x06000D39 RID: 3385 RVA: 0x0005E31F File Offset: 0x0005C51F
	private bool IsOutOfWater()
	{
		return this.m_inWater < base.transform.position.y - this.m_height;
	}

	// Token: 0x06000D3A RID: 3386 RVA: 0x0005E340 File Offset: 0x0005C540
	private void FixedUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		FishingFloat fishingFloat = FishingFloat.FindFloat(this);
		if (fishingFloat)
		{
			Utils.Pull(this.m_body, fishingFloat.transform.position, 1f, this.m_hookForce, 1f, 0.5f);
		}
		if (this.m_inWater <= -10000f || this.m_inWater < base.transform.position.y + this.m_height)
		{
			this.m_body.useGravity = true;
			if (this.IsOutOfWater())
			{
				return;
			}
		}
		this.m_body.useGravity = false;
		bool flag = false;
		Player playerNoiseRange = Player.GetPlayerNoiseRange(base.transform.position, 1f);
		if (playerNoiseRange)
		{
			if (Vector3.Distance(base.transform.position, playerNoiseRange.transform.position) > this.m_avoidRange / 2f)
			{
				Vector3 normalized = (base.transform.position - playerNoiseRange.transform.position).normalized;
				this.SwimDirection(normalized, true, true, fixedDeltaTime);
				return;
			}
			flag = true;
			if (this.m_swimTimer > 0.5f)
			{
				this.m_swimTimer = 0.5f;
			}
		}
		this.m_swimTimer -= fixedDeltaTime;
		if (this.m_swimTimer <= 0f)
		{
			this.RandomizeWaypoint(!flag);
		}
		if (this.m_haveWaypoint)
		{
			if (this.m_waypointFF)
			{
				this.m_waypoint = this.m_waypointFF.transform.position + Vector3.down;
			}
			if (Vector3.Distance(this.m_waypoint, base.transform.position) < 0.2f)
			{
				if (!this.m_waypointFF)
				{
					this.m_haveWaypoint = false;
					return;
				}
				if (Time.time - this.m_lastNibbleTime > 1f)
				{
					this.m_lastNibbleTime = Time.time;
					this.m_waypointFF.Nibble(this);
				}
			}
			Vector3 dir = Vector3.Normalize(this.m_waypoint - base.transform.position);
			this.SwimDirection(dir, flag, false, fixedDeltaTime);
			return;
		}
		this.Stop(fixedDeltaTime);
	}

	// Token: 0x06000D3B RID: 3387 RVA: 0x0005E578 File Offset: 0x0005C778
	private void Stop(float dt)
	{
		if (this.m_inWater < base.transform.position.y + this.m_height)
		{
			return;
		}
		Vector3 forward = base.transform.forward;
		forward.y = 0f;
		forward.Normalize();
		Quaternion to = Quaternion.LookRotation(forward, Vector3.up);
		Quaternion rot = Quaternion.RotateTowards(this.m_body.rotation, to, this.m_turnRate * dt);
		this.m_body.MoveRotation(rot);
		Vector3 force = -this.m_body.velocity * this.m_acceleration;
		this.m_body.AddForce(force, ForceMode.VelocityChange);
	}

	// Token: 0x06000D3C RID: 3388 RVA: 0x0005E620 File Offset: 0x0005C820
	private void SwimDirection(Vector3 dir, bool fast, bool avoidLand, float dt)
	{
		Vector3 forward = dir;
		forward.y = 0f;
		forward.Normalize();
		float num = this.m_turnRate;
		if (fast)
		{
			num *= this.m_avoidSpeedScale;
		}
		Quaternion to = Quaternion.LookRotation(forward, Vector3.up);
		Quaternion rotation = Quaternion.RotateTowards(base.transform.rotation, to, num * dt);
		this.m_body.rotation = rotation;
		float num2 = this.m_speed;
		if (fast)
		{
			num2 *= this.m_avoidSpeedScale;
		}
		if (avoidLand && this.GetPointDepth(base.transform.position + base.transform.forward) < this.m_minDepth)
		{
			num2 = 0f;
		}
		if (fast && Vector3.Dot(dir, base.transform.forward) < 0f)
		{
			num2 = 0f;
		}
		Vector3 forward2 = base.transform.forward;
		forward2.y = dir.y;
		Vector3 vector = forward2 * num2 - this.m_body.velocity;
		if (this.m_inWater < base.transform.position.y + this.m_height && vector.y > 0f)
		{
			vector.y = 0f;
		}
		this.m_body.AddForce(vector * this.m_acceleration, ForceMode.VelocityChange);
	}

	// Token: 0x06000D3D RID: 3389 RVA: 0x0005E778 File Offset: 0x0005C978
	private FishingFloat FindFloat()
	{
		foreach (FishingFloat fishingFloat in FishingFloat.GetAllInstances())
		{
			if (Vector3.Distance(base.transform.position, fishingFloat.transform.position) <= fishingFloat.m_range && fishingFloat.IsInWater() && !(fishingFloat.GetCatch() != null))
			{
				float baseHookChance = this.m_baseHookChance;
				if (UnityEngine.Random.value < baseHookChance)
				{
					return fishingFloat;
				}
			}
		}
		return null;
	}

	// Token: 0x06000D3E RID: 3390 RVA: 0x0005E814 File Offset: 0x0005CA14
	private void RandomizeWaypoint(bool canHook)
	{
		Vector2 vector = UnityEngine.Random.insideUnitCircle * this.m_swimRange;
		this.m_waypoint = this.m_spawnPoint + new Vector3(vector.x, 0f, vector.y);
		this.m_waypointFF = null;
		if (canHook)
		{
			FishingFloat fishingFloat = this.FindFloat();
			if (fishingFloat)
			{
				this.m_waypointFF = fishingFloat;
				this.m_waypoint = fishingFloat.transform.position + Vector3.down;
			}
		}
		float pointDepth = this.GetPointDepth(this.m_waypoint);
		if (pointDepth < this.m_minDepth)
		{
			return;
		}
		Vector3 p = (this.m_waypoint + base.transform.position) * 0.5f;
		if (this.GetPointDepth(p) < this.m_minDepth)
		{
			return;
		}
		float max = Mathf.Min(this.m_maxDepth, pointDepth - this.m_height);
		float waterLevel = WaterVolume.GetWaterLevel(this.m_waypoint, 1f);
		this.m_waypoint.y = waterLevel - UnityEngine.Random.Range(this.m_minDepth, max);
		this.m_haveWaypoint = true;
		this.m_swimTimer = UnityEngine.Random.Range(this.m_wpDurationMin, this.m_wpDurationMax);
	}

	// Token: 0x06000D3F RID: 3391 RVA: 0x0005E940 File Offset: 0x0005CB40
	private float GetPointDepth(Vector3 p)
	{
		float num;
		if (ZoneSystem.instance.GetSolidHeight(p, out num))
		{
			return ZoneSystem.instance.m_waterLevel - num;
		}
		return 0f;
	}

	// Token: 0x06000D40 RID: 3392 RVA: 0x0005E96E File Offset: 0x0005CB6E
	private bool DangerNearby()
	{
		return Player.GetPlayerNoiseRange(base.transform.position, 1f) != null;
	}

	// Token: 0x06000D41 RID: 3393 RVA: 0x0005E990 File Offset: 0x0005CB90
	public ZDOID GetZDOID()
	{
		return this.m_nview.GetZDO().m_uid;
	}

	// Token: 0x06000D42 RID: 3394 RVA: 0x0005E9A4 File Offset: 0x0005CBA4
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(base.transform.position + Vector3.up * this.m_height, new Vector3(1f, 0.02f, 1f));
	}

	// Token: 0x04000C05 RID: 3077
	public string m_name = "Fish";

	// Token: 0x04000C06 RID: 3078
	public float m_swimRange = 20f;

	// Token: 0x04000C07 RID: 3079
	public float m_minDepth = 1f;

	// Token: 0x04000C08 RID: 3080
	public float m_maxDepth = 4f;

	// Token: 0x04000C09 RID: 3081
	public float m_speed = 10f;

	// Token: 0x04000C0A RID: 3082
	public float m_acceleration = 5f;

	// Token: 0x04000C0B RID: 3083
	public float m_turnRate = 10f;

	// Token: 0x04000C0C RID: 3084
	public float m_wpDurationMin = 4f;

	// Token: 0x04000C0D RID: 3085
	public float m_wpDurationMax = 4f;

	// Token: 0x04000C0E RID: 3086
	public float m_avoidSpeedScale = 2f;

	// Token: 0x04000C0F RID: 3087
	public float m_avoidRange = 5f;

	// Token: 0x04000C10 RID: 3088
	public float m_height = 0.2f;

	// Token: 0x04000C11 RID: 3089
	public float m_eatDuration = 4f;

	// Token: 0x04000C12 RID: 3090
	public float m_hookForce = 4f;

	// Token: 0x04000C13 RID: 3091
	public float m_staminaUse = 1f;

	// Token: 0x04000C14 RID: 3092
	public float m_baseHookChance = 0.5f;

	// Token: 0x04000C15 RID: 3093
	public GameObject m_pickupItem;

	// Token: 0x04000C16 RID: 3094
	public int m_pickupItemStackSize = 1;

	// Token: 0x04000C17 RID: 3095
	private Vector3 m_spawnPoint;

	// Token: 0x04000C18 RID: 3096
	private Vector3 m_waypoint;

	// Token: 0x04000C19 RID: 3097
	private FishingFloat m_waypointFF;

	// Token: 0x04000C1A RID: 3098
	private bool m_haveWaypoint;

	// Token: 0x04000C1B RID: 3099
	private float m_swimTimer;

	// Token: 0x04000C1C RID: 3100
	private float m_lastNibbleTime;

	// Token: 0x04000C1D RID: 3101
	private float m_inWater = -10000f;

	// Token: 0x04000C1E RID: 3102
	private float m_pickupTime;

	// Token: 0x04000C1F RID: 3103
	private ZNetView m_nview;

	// Token: 0x04000C20 RID: 3104
	private Rigidbody m_body;
}
