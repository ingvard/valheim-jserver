using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000CD RID: 205
public class FishingFloat : MonoBehaviour, IProjectile
{
	// Token: 0x06000D44 RID: 3396 RVA: 0x0000AC4C File Offset: 0x00008E4C
	public string GetTooltipString(int itemQuality)
	{
		return "";
	}

	// Token: 0x06000D45 RID: 3397 RVA: 0x0005EACC File Offset: 0x0005CCCC
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_floating = base.GetComponent<Floating>();
		this.m_nview.Register<ZDOID>("Nibble", new Action<long, ZDOID>(this.RPC_Nibble));
	}

	// Token: 0x06000D46 RID: 3398 RVA: 0x0005EB19 File Offset: 0x0005CD19
	private void OnDestroy()
	{
		FishingFloat.m_allInstances.Remove(this);
	}

	// Token: 0x06000D47 RID: 3399 RVA: 0x0005EB28 File Offset: 0x0005CD28
	public void Setup(Character owner, Vector3 velocity, float hitNoise, HitData hitData, ItemDrop.ItemData item)
	{
		FishingFloat fishingFloat = FishingFloat.FindFloat(owner);
		if (fishingFloat)
		{
			ZNetScene.instance.Destroy(fishingFloat.gameObject);
		}
		ZDOID zdoid = owner.GetZDOID();
		this.m_nview.GetZDO().Set("RodOwner", zdoid);
		FishingFloat.m_allInstances.Add(this);
		Transform rodTop = this.GetRodTop(owner);
		if (rodTop == null)
		{
			ZLog.LogWarning("Failed to find fishing rod top");
			return;
		}
		this.m_rodLine.SetPeer(owner.GetZDOID());
		this.m_lineLength = Vector3.Distance(rodTop.position, base.transform.position);
		owner.Message(MessageHud.MessageType.Center, this.m_lineLength.ToString("0m"), 0, null);
	}

	// Token: 0x06000D48 RID: 3400 RVA: 0x0005EBE0 File Offset: 0x0005CDE0
	public Character GetOwner()
	{
		if (!this.m_nview.IsValid())
		{
			return null;
		}
		ZDOID zdoid = this.m_nview.GetZDO().GetZDOID("RodOwner");
		GameObject gameObject = ZNetScene.instance.FindInstance(zdoid);
		if (gameObject == null)
		{
			return null;
		}
		return gameObject.GetComponent<Character>();
	}

	// Token: 0x06000D49 RID: 3401 RVA: 0x0005EC30 File Offset: 0x0005CE30
	private Transform GetRodTop(Character owner)
	{
		Transform transform = Utils.FindChild(owner.transform, "_RodTop");
		if (transform == null)
		{
			ZLog.LogWarning("Failed to find fishing rod top");
			return null;
		}
		return transform;
	}

	// Token: 0x06000D4A RID: 3402 RVA: 0x0005EC64 File Offset: 0x0005CE64
	private void FixedUpdate()
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		Character owner = this.GetOwner();
		if (!owner)
		{
			ZLog.LogWarning("Fishing rod not found, destroying fishing float");
			this.m_nview.Destroy();
			return;
		}
		Transform rodTop = this.GetRodTop(owner);
		if (!rodTop)
		{
			ZLog.LogWarning("Fishing rod not found, destroying fishing float");
			this.m_nview.Destroy();
			return;
		}
		if (owner.InAttack() || owner.IsHoldingAttack())
		{
			this.m_nview.Destroy();
			return;
		}
		float magnitude = (rodTop.transform.position - base.transform.position).magnitude;
		Fish fish = this.GetCatch();
		if (!owner.HaveStamina(0f) && fish != null)
		{
			this.SetCatch(null);
			fish = null;
			this.Message("$msg_fishing_lost", true);
		}
		if (fish)
		{
			owner.UseStamina(this.m_hookedStaminaPerSec * fixedDeltaTime);
		}
		if (!fish && Utils.LengthXZ(this.m_body.velocity) > 2f)
		{
			this.TryToHook();
		}
		if (owner.IsBlocking() && owner.HaveStamina(0f))
		{
			float num = this.m_pullStaminaUse;
			if (fish != null)
			{
				num += fish.m_staminaUse;
			}
			owner.UseStamina(num * fixedDeltaTime);
			if (this.m_lineLength > magnitude - 0.2f)
			{
				float lineLength = this.m_lineLength;
				this.m_lineLength -= fixedDeltaTime * this.m_pullLineSpeed;
				this.TryToHook();
				if ((int)this.m_lineLength != (int)lineLength)
				{
					this.Message(this.m_lineLength.ToString("0m"), false);
				}
			}
			if (this.m_lineLength <= 0.5f)
			{
				if (fish)
				{
					if (fish.Pickup(owner as Humanoid))
					{
						this.Message("$msg_fishing_catched " + fish.GetHoverName(), true);
						this.SetCatch(null);
					}
					return;
				}
				this.m_nview.Destroy();
				return;
			}
		}
		this.m_rodLine.m_slack = (1f - Utils.LerpStep(this.m_lineLength / 2f, this.m_lineLength, magnitude)) * this.m_maxLineSlack;
		if (magnitude - this.m_lineLength > this.m_breakDistance || magnitude > this.m_maxDistance)
		{
			this.Message("$msg_fishing_linebroke", true);
			this.m_nview.Destroy();
			this.m_lineBreakEffect.Create(base.transform.position, Quaternion.identity, null, 1f);
			return;
		}
		if (fish)
		{
			Utils.Pull(this.m_body, fish.transform.position, 0.5f, this.m_moveForce, 0.5f, 0.3f);
		}
		Utils.Pull(this.m_body, rodTop.transform.position, this.m_lineLength, this.m_moveForce, 1f, 0.3f);
	}

	// Token: 0x06000D4B RID: 3403 RVA: 0x0005EF54 File Offset: 0x0005D154
	private void TryToHook()
	{
		if (this.m_nibbler != null && Time.time - this.m_nibbleTime < 0.5f && this.GetCatch() == null)
		{
			this.Message("$msg_fishing_hooked", true);
			this.SetCatch(this.m_nibbler);
			this.m_nibbler = null;
		}
	}

	// Token: 0x06000D4C RID: 3404 RVA: 0x0005EFB0 File Offset: 0x0005D1B0
	private void SetCatch(Fish fish)
	{
		if (fish)
		{
			this.m_nview.GetZDO().Set("CatchID", fish.GetZDOID());
			this.m_hookLine.SetPeer(fish.GetZDOID());
			return;
		}
		this.m_nview.GetZDO().Set("CatchID", ZDOID.None);
		this.m_hookLine.SetPeer(ZDOID.None);
	}

	// Token: 0x06000D4D RID: 3405 RVA: 0x0005F01C File Offset: 0x0005D21C
	public Fish GetCatch()
	{
		if (!this.m_nview.IsValid())
		{
			return null;
		}
		ZDOID zdoid = this.m_nview.GetZDO().GetZDOID("CatchID");
		if (!zdoid.IsNone())
		{
			GameObject gameObject = ZNetScene.instance.FindInstance(zdoid);
			if (gameObject)
			{
				return gameObject.GetComponent<Fish>();
			}
		}
		return null;
	}

	// Token: 0x06000D4E RID: 3406 RVA: 0x0005F073 File Offset: 0x0005D273
	public bool IsInWater()
	{
		return this.m_floating.IsInWater();
	}

	// Token: 0x06000D4F RID: 3407 RVA: 0x0005F080 File Offset: 0x0005D280
	public void Nibble(Fish fish)
	{
		this.m_nview.InvokeRPC("Nibble", new object[]
		{
			fish.GetZDOID()
		});
	}

	// Token: 0x06000D50 RID: 3408 RVA: 0x0005F0A8 File Offset: 0x0005D2A8
	public void RPC_Nibble(long sender, ZDOID fishID)
	{
		if (Time.time - this.m_nibbleTime < 1f)
		{
			return;
		}
		if (this.GetCatch() != null)
		{
			return;
		}
		this.m_nibbleEffect.Create(base.transform.position, Quaternion.identity, base.transform, 1f);
		this.m_body.AddForce(Vector3.down * this.m_nibbleForce, ForceMode.VelocityChange);
		GameObject gameObject = ZNetScene.instance.FindInstance(fishID);
		if (gameObject)
		{
			this.m_nibbler = gameObject.GetComponent<Fish>();
			this.m_nibbleTime = Time.time;
		}
	}

	// Token: 0x06000D51 RID: 3409 RVA: 0x0005F146 File Offset: 0x0005D346
	public static List<FishingFloat> GetAllInstances()
	{
		return FishingFloat.m_allInstances;
	}

	// Token: 0x06000D52 RID: 3410 RVA: 0x0005F150 File Offset: 0x0005D350
	private static FishingFloat FindFloat(Character owner)
	{
		foreach (FishingFloat fishingFloat in FishingFloat.m_allInstances)
		{
			if (owner == fishingFloat.GetOwner())
			{
				return fishingFloat;
			}
		}
		return null;
	}

	// Token: 0x06000D53 RID: 3411 RVA: 0x0005F1B0 File Offset: 0x0005D3B0
	public static FishingFloat FindFloat(Fish fish)
	{
		foreach (FishingFloat fishingFloat in FishingFloat.m_allInstances)
		{
			if (fishingFloat.GetCatch() == fish)
			{
				return fishingFloat;
			}
		}
		return null;
	}

	// Token: 0x06000D54 RID: 3412 RVA: 0x0005F210 File Offset: 0x0005D410
	private void Message(string msg, bool prioritized = false)
	{
		if (!prioritized && Time.time - this.m_msgTime < 1f)
		{
			return;
		}
		this.m_msgTime = Time.time;
		Character owner = this.GetOwner();
		if (owner)
		{
			owner.Message(MessageHud.MessageType.Center, Localization.instance.Localize(msg), 0, null);
		}
	}

	// Token: 0x04000C21 RID: 3105
	public float m_maxDistance = 30f;

	// Token: 0x04000C22 RID: 3106
	public float m_moveForce = 10f;

	// Token: 0x04000C23 RID: 3107
	public float m_pullLineSpeed = 1f;

	// Token: 0x04000C24 RID: 3108
	public float m_pullStaminaUse = 10f;

	// Token: 0x04000C25 RID: 3109
	public float m_hookedStaminaPerSec = 1f;

	// Token: 0x04000C26 RID: 3110
	public float m_breakDistance = 4f;

	// Token: 0x04000C27 RID: 3111
	public float m_range = 10f;

	// Token: 0x04000C28 RID: 3112
	public float m_nibbleForce = 10f;

	// Token: 0x04000C29 RID: 3113
	public EffectList m_nibbleEffect = new EffectList();

	// Token: 0x04000C2A RID: 3114
	public EffectList m_lineBreakEffect = new EffectList();

	// Token: 0x04000C2B RID: 3115
	public float m_maxLineSlack = 0.3f;

	// Token: 0x04000C2C RID: 3116
	public LineConnect m_rodLine;

	// Token: 0x04000C2D RID: 3117
	public LineConnect m_hookLine;

	// Token: 0x04000C2E RID: 3118
	private ZNetView m_nview;

	// Token: 0x04000C2F RID: 3119
	private Rigidbody m_body;

	// Token: 0x04000C30 RID: 3120
	private Floating m_floating;

	// Token: 0x04000C31 RID: 3121
	private float m_lineLength;

	// Token: 0x04000C32 RID: 3122
	private float m_msgTime;

	// Token: 0x04000C33 RID: 3123
	private Fish m_nibbler;

	// Token: 0x04000C34 RID: 3124
	private float m_nibbleTime;

	// Token: 0x04000C35 RID: 3125
	private static List<FishingFloat> m_allInstances = new List<FishingFloat>();
}
