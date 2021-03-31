using System;
using UnityEngine;

// Token: 0x020000B9 RID: 185
public class Bed : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000C5E RID: 3166 RVA: 0x00058D73 File Offset: 0x00056F73
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.m_nview.Register<long, string>("SetOwner", new Action<long, long, string>(this.RPC_SetOwner));
	}

	// Token: 0x06000C5F RID: 3167 RVA: 0x00058DAC File Offset: 0x00056FAC
	public string GetHoverText()
	{
		string ownerName = this.GetOwnerName();
		if (ownerName == "")
		{
			return Localization.instance.Localize("$piece_bed_unclaimed\n[<color=yellow><b>$KEY_Use</b></color>] $piece_bed_claim");
		}
		string text = ownerName + "'s $piece_bed";
		if (!this.IsMine())
		{
			return Localization.instance.Localize(text);
		}
		if (this.IsCurrent())
		{
			return Localization.instance.Localize(text + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_bed_sleep");
		}
		return Localization.instance.Localize(text + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_bed_setspawn");
	}

	// Token: 0x06000C60 RID: 3168 RVA: 0x00058E30 File Offset: 0x00057030
	public string GetHoverName()
	{
		return Localization.instance.Localize("$piece_bed");
	}

	// Token: 0x06000C61 RID: 3169 RVA: 0x00058E44 File Offset: 0x00057044
	public bool Interact(Humanoid human, bool repeat)
	{
		if (repeat)
		{
			return false;
		}
		long playerID = Game.instance.GetPlayerProfile().GetPlayerID();
		bool owner = this.GetOwner() != 0L;
		Player human2 = human as Player;
		if (!owner)
		{
			ZLog.Log("Has no creator");
			if (!this.CheckExposure(human2))
			{
				return false;
			}
			this.SetOwner(playerID, Game.instance.GetPlayerProfile().GetName());
			Game.instance.GetPlayerProfile().SetCustomSpawnPoint(this.GetSpawnPoint());
			human.Message(MessageHud.MessageType.Center, "$msg_spawnpointset", 0, null);
		}
		else if (this.IsMine())
		{
			ZLog.Log("Is mine");
			if (this.IsCurrent())
			{
				ZLog.Log("is current spawnpoint");
				if (!EnvMan.instance.IsAfternoon() && !EnvMan.instance.IsNight())
				{
					human.Message(MessageHud.MessageType.Center, "$msg_cantsleep", 0, null);
					return false;
				}
				if (!this.CheckEnemies(human2))
				{
					return false;
				}
				if (!this.CheckExposure(human2))
				{
					return false;
				}
				if (!this.CheckFire(human2))
				{
					return false;
				}
				if (!this.CheckWet(human2))
				{
					return false;
				}
				human.AttachStart(this.m_spawnPoint, true, true, "attach_bed", new Vector3(0f, 0.5f, 0f));
				return false;
			}
			else
			{
				ZLog.Log("Not current spawn point");
				if (!this.CheckExposure(human2))
				{
					return false;
				}
				Game.instance.GetPlayerProfile().SetCustomSpawnPoint(this.GetSpawnPoint());
				human.Message(MessageHud.MessageType.Center, "$msg_spawnpointset", 0, null);
			}
		}
		return false;
	}

	// Token: 0x06000C62 RID: 3170 RVA: 0x00058FA8 File Offset: 0x000571A8
	private bool CheckWet(Player human)
	{
		if (human.GetSEMan().HaveStatusEffect("Wet"))
		{
			human.Message(MessageHud.MessageType.Center, "$msg_bedwet", 0, null);
			return false;
		}
		return true;
	}

	// Token: 0x06000C63 RID: 3171 RVA: 0x00058FCD File Offset: 0x000571CD
	private bool CheckEnemies(Player human)
	{
		if (human.IsSensed())
		{
			human.Message(MessageHud.MessageType.Center, "$msg_bedenemiesnearby", 0, null);
			return false;
		}
		return true;
	}

	// Token: 0x06000C64 RID: 3172 RVA: 0x00058FE8 File Offset: 0x000571E8
	private bool CheckExposure(Player human)
	{
		float num;
		bool flag;
		Cover.GetCoverForPoint(this.GetSpawnPoint(), out num, out flag);
		if (!flag)
		{
			human.Message(MessageHud.MessageType.Center, "$msg_bedneedroof", 0, null);
			return false;
		}
		if (num < 0.8f)
		{
			human.Message(MessageHud.MessageType.Center, "$msg_bedtooexposed", 0, null);
			return false;
		}
		ZLog.Log(string.Concat(new object[]
		{
			"exporeusre check ",
			num,
			"  ",
			flag.ToString()
		}));
		return true;
	}

	// Token: 0x06000C65 RID: 3173 RVA: 0x00059063 File Offset: 0x00057263
	private bool CheckFire(Player human)
	{
		if (!EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.Heat, 0f))
		{
			human.Message(MessageHud.MessageType.Center, "$msg_bednofire", 0, null);
			return false;
		}
		return true;
	}

	// Token: 0x06000C66 RID: 3174 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000C67 RID: 3175 RVA: 0x00059093 File Offset: 0x00057293
	public bool IsCurrent()
	{
		return this.IsMine() && Vector3.Distance(this.GetSpawnPoint(), Game.instance.GetPlayerProfile().GetCustomSpawnPoint()) < 1f;
	}

	// Token: 0x06000C68 RID: 3176 RVA: 0x000590C0 File Offset: 0x000572C0
	public Vector3 GetSpawnPoint()
	{
		return this.m_spawnPoint.position;
	}

	// Token: 0x06000C69 RID: 3177 RVA: 0x000590D0 File Offset: 0x000572D0
	private bool IsMine()
	{
		long playerID = Game.instance.GetPlayerProfile().GetPlayerID();
		long owner = this.GetOwner();
		return playerID == owner;
	}

	// Token: 0x06000C6A RID: 3178 RVA: 0x000590F6 File Offset: 0x000572F6
	private void SetOwner(long uid, string name)
	{
		this.m_nview.InvokeRPC("SetOwner", new object[]
		{
			uid,
			name
		});
	}

	// Token: 0x06000C6B RID: 3179 RVA: 0x0005911B File Offset: 0x0005731B
	private void RPC_SetOwner(long sender, long uid, string name)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		this.m_nview.GetZDO().Set("owner", uid);
		this.m_nview.GetZDO().Set("ownerName", name);
	}

	// Token: 0x06000C6C RID: 3180 RVA: 0x00059157 File Offset: 0x00057357
	private long GetOwner()
	{
		return this.m_nview.GetZDO().GetLong("owner", 0L);
	}

	// Token: 0x06000C6D RID: 3181 RVA: 0x00059170 File Offset: 0x00057370
	private string GetOwnerName()
	{
		return this.m_nview.GetZDO().GetString("ownerName", "");
	}

	// Token: 0x04000B5A RID: 2906
	public Transform m_spawnPoint;

	// Token: 0x04000B5B RID: 2907
	public float m_monsterCheckRadius = 20f;

	// Token: 0x04000B5C RID: 2908
	private ZNetView m_nview;
}
