using System;
using UnityEngine;

// Token: 0x020000FF RID: 255
public class TeleportWorld : MonoBehaviour, Hoverable, Interactable, TextReceiver
{
	// Token: 0x06000F73 RID: 3955 RVA: 0x0006D9B0 File Offset: 0x0006BBB0
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			base.enabled = false;
			return;
		}
		this.m_hadTarget = this.HaveTarget();
		this.m_nview.Register<string>("SetTag", new Action<long, string>(this.RPC_SetTag));
		base.InvokeRepeating("UpdatePortal", 0.5f, 0.5f);
	}

	// Token: 0x06000F74 RID: 3956 RVA: 0x0006DA1C File Offset: 0x0006BC1C
	public string GetHoverText()
	{
		string text = this.GetText();
		string text2 = this.HaveTarget() ? "$piece_portal_connected" : "$piece_portal_unconnected";
		return Localization.instance.Localize(string.Concat(new string[]
		{
			"$piece_portal $piece_portal_tag:\"",
			text,
			"\"  [",
			text2,
			"]\n[<color=yellow><b>$KEY_Use</b></color>] $piece_portal_settag"
		}));
	}

	// Token: 0x06000F75 RID: 3957 RVA: 0x0006DA7A File Offset: 0x0006BC7A
	public string GetHoverName()
	{
		return "Teleport";
	}

	// Token: 0x06000F76 RID: 3958 RVA: 0x0006DA84 File Offset: 0x0006BC84
	public bool Interact(Humanoid human, bool hold)
	{
		if (hold)
		{
			return false;
		}
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			human.Message(MessageHud.MessageType.Center, "$piece_noaccess", 0, null);
			return true;
		}
		TextInput.instance.RequestText(this, "$piece_portal_tag", 10);
		return true;
	}

	// Token: 0x06000F77 RID: 3959 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000F78 RID: 3960 RVA: 0x0006DAD4 File Offset: 0x0006BCD4
	private void UpdatePortal()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		Player closestPlayer = Player.GetClosestPlayer(this.m_proximityRoot.position, this.m_activationRange);
		bool flag = this.HaveTarget();
		if (flag && !this.m_hadTarget)
		{
			this.m_connected.Create(base.transform.position, base.transform.rotation, null, 1f);
		}
		this.m_hadTarget = flag;
		this.m_target_found.SetActive(closestPlayer && closestPlayer.IsTeleportable() && this.TargetFound());
	}

	// Token: 0x06000F79 RID: 3961 RVA: 0x0006DB6C File Offset: 0x0006BD6C
	private void Update()
	{
		this.m_colorAlpha = Mathf.MoveTowards(this.m_colorAlpha, this.m_hadTarget ? 1f : 0f, Time.deltaTime);
		this.m_model.material.SetColor("_EmissionColor", Color.Lerp(this.m_colorUnconnected, this.m_colorTargetfound, this.m_colorAlpha));
	}

	// Token: 0x06000F7A RID: 3962 RVA: 0x0006DBD0 File Offset: 0x0006BDD0
	public void Teleport(Player player)
	{
		if (!this.TargetFound())
		{
			return;
		}
		if (!player.IsTeleportable())
		{
			player.Message(MessageHud.MessageType.Center, "$msg_noteleport", 0, null);
			return;
		}
		ZLog.Log("Teleporting " + player.GetPlayerName());
		ZDOID zdoid = this.m_nview.GetZDO().GetZDOID("target");
		if (zdoid == ZDOID.None)
		{
			return;
		}
		ZDO zdo = ZDOMan.instance.GetZDO(zdoid);
		Vector3 position = zdo.GetPosition();
		Quaternion rotation = zdo.GetRotation();
		Vector3 a = rotation * Vector3.forward;
		Vector3 pos = position + a * this.m_exitDistance + Vector3.up;
		player.TeleportTo(pos, rotation, true);
	}

	// Token: 0x06000F7B RID: 3963 RVA: 0x0006DC83 File Offset: 0x0006BE83
	public string GetText()
	{
		return this.m_nview.GetZDO().GetString("tag", "");
	}

	// Token: 0x06000F7C RID: 3964 RVA: 0x0006DC9F File Offset: 0x0006BE9F
	public void SetText(string text)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.m_nview.InvokeRPC("SetTag", new object[]
		{
			text
		});
	}

	// Token: 0x06000F7D RID: 3965 RVA: 0x0006DCCC File Offset: 0x0006BECC
	private void RPC_SetTag(long sender, string tag)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (this.GetText() == tag)
		{
			return;
		}
		this.m_nview.GetZDO().Set("tag", tag);
	}

	// Token: 0x06000F7E RID: 3966 RVA: 0x0006DD19 File Offset: 0x0006BF19
	private bool HaveTarget()
	{
		return this.m_nview.GetZDO().GetZDOID("target") != ZDOID.None;
	}

	// Token: 0x06000F7F RID: 3967 RVA: 0x0006DD3C File Offset: 0x0006BF3C
	private bool TargetFound()
	{
		ZDOID zdoid = this.m_nview.GetZDO().GetZDOID("target");
		if (zdoid == ZDOID.None)
		{
			return false;
		}
		if (ZDOMan.instance.GetZDO(zdoid) == null)
		{
			ZDOMan.instance.RequestZDO(zdoid);
			return false;
		}
		return true;
	}

	// Token: 0x04000E42 RID: 3650
	public float m_activationRange = 5f;

	// Token: 0x04000E43 RID: 3651
	public float m_exitDistance = 1f;

	// Token: 0x04000E44 RID: 3652
	public Transform m_proximityRoot;

	// Token: 0x04000E45 RID: 3653
	[ColorUsage(true, true)]
	public Color m_colorUnconnected = Color.white;

	// Token: 0x04000E46 RID: 3654
	[ColorUsage(true, true)]
	public Color m_colorTargetfound = Color.white;

	// Token: 0x04000E47 RID: 3655
	public EffectFade m_target_found;

	// Token: 0x04000E48 RID: 3656
	public MeshRenderer m_model;

	// Token: 0x04000E49 RID: 3657
	public EffectList m_connected;

	// Token: 0x04000E4A RID: 3658
	private ZNetView m_nview;

	// Token: 0x04000E4B RID: 3659
	private bool m_hadTarget;

	// Token: 0x04000E4C RID: 3660
	private float m_colorAlpha;
}
