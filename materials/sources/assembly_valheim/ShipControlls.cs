using System;
using UnityEngine;

// Token: 0x020000F3 RID: 243
public class ShipControlls : MonoBehaviour, Interactable, Hoverable
{
	// Token: 0x06000EFE RID: 3838 RVA: 0x0006B560 File Offset: 0x00069760
	private void Awake()
	{
		this.m_nview = this.m_ship.GetComponent<ZNetView>();
		this.m_nview.Register<ZDOID>("RequestControl", new Action<long, ZDOID>(this.RPC_RequestControl));
		this.m_nview.Register<ZDOID>("ReleaseControl", new Action<long, ZDOID>(this.RPC_ReleaseControl));
		this.m_nview.Register<bool>("RequestRespons", new Action<long, bool>(this.RPC_RequestRespons));
	}

	// Token: 0x06000EFF RID: 3839 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000F00 RID: 3840 RVA: 0x0006B5D4 File Offset: 0x000697D4
	public bool Interact(Humanoid character, bool repeat)
	{
		if (repeat)
		{
			return false;
		}
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		if (!this.InUseDistance(character))
		{
			return false;
		}
		Player player = character as Player;
		if (player == null)
		{
			return false;
		}
		if (player.IsEncumbered())
		{
			return false;
		}
		if (player.GetStandingOnShip() != this.m_ship)
		{
			return false;
		}
		this.m_nview.InvokeRPC("RequestControl", new object[]
		{
			player.GetZDOID()
		});
		return false;
	}

	// Token: 0x06000F01 RID: 3841 RVA: 0x0006B656 File Offset: 0x00069856
	public Ship GetShip()
	{
		return this.m_ship;
	}

	// Token: 0x06000F02 RID: 3842 RVA: 0x0006B65E File Offset: 0x0006985E
	public string GetHoverText()
	{
		if (!this.InUseDistance(Player.m_localPlayer))
		{
			return Localization.instance.Localize("<color=grey>$piece_toofar</color>");
		}
		return Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>] " + this.m_hoverText);
	}

	// Token: 0x06000F03 RID: 3843 RVA: 0x0006B697 File Offset: 0x00069897
	public string GetHoverName()
	{
		return Localization.instance.Localize(this.m_hoverText);
	}

	// Token: 0x06000F04 RID: 3844 RVA: 0x0006B6AC File Offset: 0x000698AC
	private void RPC_RequestControl(long sender, ZDOID playerID)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.m_ship.IsPlayerInBoat(playerID))
		{
			return;
		}
		if (this.GetUser() == playerID || !this.HaveValidUser())
		{
			this.m_nview.GetZDO().Set("user", playerID);
			this.m_nview.InvokeRPC(sender, "RequestRespons", new object[]
			{
				true
			});
			return;
		}
		this.m_nview.InvokeRPC(sender, "RequestRespons", new object[]
		{
			false
		});
	}

	// Token: 0x06000F05 RID: 3845 RVA: 0x0006B743 File Offset: 0x00069943
	private void RPC_ReleaseControl(long sender, ZDOID playerID)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.GetUser() == playerID)
		{
			this.m_nview.GetZDO().Set("user", ZDOID.None);
		}
	}

	// Token: 0x06000F06 RID: 3846 RVA: 0x0006B77C File Offset: 0x0006997C
	private void RPC_RequestRespons(long sender, bool granted)
	{
		if (!Player.m_localPlayer)
		{
			return;
		}
		if (granted)
		{
			Player.m_localPlayer.StartShipControl(this);
			if (this.m_attachPoint != null)
			{
				Player.m_localPlayer.AttachStart(this.m_attachPoint, false, false, this.m_attachAnimation, this.m_detachOffset);
				return;
			}
		}
		else
		{
			Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_inuse", 0, null);
		}
	}

	// Token: 0x06000F07 RID: 3847 RVA: 0x0006B7E4 File Offset: 0x000699E4
	public void OnUseStop(Player player)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.m_nview.InvokeRPC("ReleaseControl", new object[]
		{
			player.GetZDOID()
		});
		if (this.m_attachPoint != null)
		{
			player.AttachStop();
		}
	}

	// Token: 0x06000F08 RID: 3848 RVA: 0x0006B838 File Offset: 0x00069A38
	public bool HaveValidUser()
	{
		ZDOID user = this.GetUser();
		return !user.IsNone() && this.m_ship.IsPlayerInBoat(user);
	}

	// Token: 0x06000F09 RID: 3849 RVA: 0x0006B864 File Offset: 0x00069A64
	public bool IsLocalUser()
	{
		if (!Player.m_localPlayer)
		{
			return false;
		}
		ZDOID user = this.GetUser();
		return !user.IsNone() && user == Player.m_localPlayer.GetZDOID();
	}

	// Token: 0x06000F0A RID: 3850 RVA: 0x0006B8A1 File Offset: 0x00069AA1
	private ZDOID GetUser()
	{
		if (!this.m_nview.IsValid())
		{
			return ZDOID.None;
		}
		return this.m_nview.GetZDO().GetZDOID("user");
	}

	// Token: 0x06000F0B RID: 3851 RVA: 0x0006B8CB File Offset: 0x00069ACB
	private bool InUseDistance(Humanoid human)
	{
		return Vector3.Distance(human.transform.position, this.m_attachPoint.position) < this.m_maxUseRange;
	}

	// Token: 0x04000DF3 RID: 3571
	public string m_hoverText = "";

	// Token: 0x04000DF4 RID: 3572
	public Ship m_ship;

	// Token: 0x04000DF5 RID: 3573
	public float m_maxUseRange = 10f;

	// Token: 0x04000DF6 RID: 3574
	public Transform m_attachPoint;

	// Token: 0x04000DF7 RID: 3575
	public Vector3 m_detachOffset = new Vector3(0f, 0.5f, 0f);

	// Token: 0x04000DF8 RID: 3576
	public string m_attachAnimation = "attach_chair";

	// Token: 0x04000DF9 RID: 3577
	private ZNetView m_nview;
}
