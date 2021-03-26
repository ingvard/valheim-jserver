using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000091 RID: 145
public class ZSyncAnimation : MonoBehaviour
{
	// Token: 0x060009C1 RID: 2497 RVA: 0x00046C64 File Offset: 0x00044E64
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_animator = base.GetComponentInChildren<Animator>();
		this.m_animator.logWarnings = false;
		this.m_nview.Register<string>("SetTrigger", new Action<long, string>(this.RPC_SetTrigger));
		this.m_boolHashes = new int[this.m_syncBools.Count];
		this.m_boolDefaults = new bool[this.m_syncBools.Count];
		for (int i = 0; i < this.m_syncBools.Count; i++)
		{
			this.m_boolHashes[i] = ZSyncAnimation.GetHash(this.m_syncBools[i]);
			this.m_boolDefaults[i] = this.m_animator.GetBool(this.m_boolHashes[i]);
		}
		this.m_floatHashes = new int[this.m_syncFloats.Count];
		this.m_floatDefaults = new float[this.m_syncFloats.Count];
		for (int j = 0; j < this.m_syncFloats.Count; j++)
		{
			this.m_floatHashes[j] = ZSyncAnimation.GetHash(this.m_syncFloats[j]);
			this.m_floatDefaults[j] = this.m_animator.GetFloat(this.m_floatHashes[j]);
		}
		this.m_intHashes = new int[this.m_syncInts.Count];
		this.m_intDefaults = new int[this.m_syncInts.Count];
		for (int k = 0; k < this.m_syncInts.Count; k++)
		{
			this.m_intHashes[k] = ZSyncAnimation.GetHash(this.m_syncInts[k]);
			this.m_intDefaults[k] = this.m_animator.GetInteger(this.m_intHashes[k]);
		}
		if (ZSyncAnimation.m_forwardSpeedID == 0)
		{
			ZSyncAnimation.m_forwardSpeedID = ZSyncAnimation.GetHash("forward_speed");
			ZSyncAnimation.m_sidewaySpeedID = ZSyncAnimation.GetHash("sideway_speed");
			ZSyncAnimation.m_animSpeedID = ZSyncAnimation.GetHash("anim_speed");
		}
		if (this.m_nview.GetZDO() == null)
		{
			base.enabled = false;
			return;
		}
		this.SyncParameters();
	}

	// Token: 0x060009C2 RID: 2498 RVA: 0x00046E62 File Offset: 0x00045062
	public static int GetHash(string name)
	{
		return Animator.StringToHash(name);
	}

	// Token: 0x060009C3 RID: 2499 RVA: 0x00046E6A File Offset: 0x0004506A
	private void FixedUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.SyncParameters();
	}

	// Token: 0x060009C4 RID: 2500 RVA: 0x00046E80 File Offset: 0x00045080
	private void SyncParameters()
	{
		ZDO zdo = this.m_nview.GetZDO();
		if (!this.m_nview.IsOwner())
		{
			for (int i = 0; i < this.m_boolHashes.Length; i++)
			{
				int num = this.m_boolHashes[i];
				bool @bool = zdo.GetBool(438569 + num, this.m_boolDefaults[i]);
				this.m_animator.SetBool(num, @bool);
			}
			for (int j = 0; j < this.m_floatHashes.Length; j++)
			{
				int num2 = this.m_floatHashes[j];
				float @float = zdo.GetFloat(438569 + num2, this.m_floatDefaults[j]);
				if (this.m_smoothCharacterSpeeds && (num2 == ZSyncAnimation.m_forwardSpeedID || num2 == ZSyncAnimation.m_sidewaySpeedID))
				{
					this.m_animator.SetFloat(num2, @float, 0.2f, Time.fixedDeltaTime);
				}
				else
				{
					this.m_animator.SetFloat(num2, @float);
				}
			}
			for (int k = 0; k < this.m_intHashes.Length; k++)
			{
				int num3 = this.m_intHashes[k];
				int @int = zdo.GetInt(438569 + num3, this.m_intDefaults[k]);
				this.m_animator.SetInteger(num3, @int);
			}
			float float2 = zdo.GetFloat(ZSyncAnimation.m_animSpeedID, 1f);
			this.m_animator.speed = float2;
			return;
		}
		zdo.Set(ZSyncAnimation.m_animSpeedID, this.m_animator.speed);
	}

	// Token: 0x060009C5 RID: 2501 RVA: 0x00046FE7 File Offset: 0x000451E7
	public void SetTrigger(string name)
	{
		this.m_nview.InvokeRPC(ZNetView.Everybody, "SetTrigger", new object[]
		{
			name
		});
	}

	// Token: 0x060009C6 RID: 2502 RVA: 0x00047008 File Offset: 0x00045208
	public void SetBool(string name, bool value)
	{
		int hash = ZSyncAnimation.GetHash(name);
		this.SetBool(hash, value);
	}

	// Token: 0x060009C7 RID: 2503 RVA: 0x00047024 File Offset: 0x00045224
	public void SetBool(int hash, bool value)
	{
		if (this.m_animator.GetBool(hash) == value)
		{
			return;
		}
		this.m_animator.SetBool(hash, value);
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(438569 + hash, value);
		}
	}

	// Token: 0x060009C8 RID: 2504 RVA: 0x00047080 File Offset: 0x00045280
	public void SetFloat(string name, float value)
	{
		int hash = ZSyncAnimation.GetHash(name);
		this.SetFloat(hash, value);
	}

	// Token: 0x060009C9 RID: 2505 RVA: 0x0004709C File Offset: 0x0004529C
	public void SetFloat(int hash, float value)
	{
		if (Mathf.Abs(this.m_animator.GetFloat(hash) - value) < 0.01f)
		{
			return;
		}
		if (this.m_smoothCharacterSpeeds && (hash == ZSyncAnimation.m_forwardSpeedID || hash == ZSyncAnimation.m_sidewaySpeedID))
		{
			this.m_animator.SetFloat(hash, value, 0.2f, Time.fixedDeltaTime);
		}
		else
		{
			this.m_animator.SetFloat(hash, value);
		}
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(438569 + hash, value);
		}
	}

	// Token: 0x060009CA RID: 2506 RVA: 0x00047134 File Offset: 0x00045334
	public void SetInt(string name, int value)
	{
		int hash = ZSyncAnimation.GetHash(name);
		this.SetInt(hash, value);
	}

	// Token: 0x060009CB RID: 2507 RVA: 0x00047150 File Offset: 0x00045350
	public void SetInt(int hash, int value)
	{
		if (this.m_animator.GetInteger(hash) == value)
		{
			return;
		}
		this.m_animator.SetInteger(hash, value);
		if (this.m_nview.GetZDO() != null && this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set(438569 + hash, value);
		}
	}

	// Token: 0x060009CC RID: 2508 RVA: 0x000471AC File Offset: 0x000453AC
	private void RPC_SetTrigger(long sender, string name)
	{
		this.m_animator.SetTrigger(name);
	}

	// Token: 0x060009CD RID: 2509 RVA: 0x000471BA File Offset: 0x000453BA
	public void SetSpeed(float speed)
	{
		this.m_animator.speed = speed;
	}

	// Token: 0x060009CE RID: 2510 RVA: 0x000471C8 File Offset: 0x000453C8
	public bool IsOwner()
	{
		return this.m_nview.IsValid() && this.m_nview.IsOwner();
	}

	// Token: 0x040008CF RID: 2255
	private ZNetView m_nview;

	// Token: 0x040008D0 RID: 2256
	private Animator m_animator;

	// Token: 0x040008D1 RID: 2257
	public List<string> m_syncBools = new List<string>();

	// Token: 0x040008D2 RID: 2258
	public List<string> m_syncFloats = new List<string>();

	// Token: 0x040008D3 RID: 2259
	public List<string> m_syncInts = new List<string>();

	// Token: 0x040008D4 RID: 2260
	public bool m_smoothCharacterSpeeds = true;

	// Token: 0x040008D5 RID: 2261
	private static int m_forwardSpeedID;

	// Token: 0x040008D6 RID: 2262
	private static int m_sidewaySpeedID;

	// Token: 0x040008D7 RID: 2263
	private static int m_animSpeedID;

	// Token: 0x040008D8 RID: 2264
	private int[] m_boolHashes;

	// Token: 0x040008D9 RID: 2265
	private bool[] m_boolDefaults;

	// Token: 0x040008DA RID: 2266
	private int[] m_floatHashes;

	// Token: 0x040008DB RID: 2267
	private float[] m_floatDefaults;

	// Token: 0x040008DC RID: 2268
	private int[] m_intHashes;

	// Token: 0x040008DD RID: 2269
	private int[] m_intDefaults;

	// Token: 0x040008DE RID: 2270
	private const int m_zdoSalt = 438569;
}
