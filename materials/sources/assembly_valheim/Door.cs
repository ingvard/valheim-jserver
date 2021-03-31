using System;
using UnityEngine;

// Token: 0x020000C4 RID: 196
public class Door : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000CE4 RID: 3300 RVA: 0x0005C490 File Offset: 0x0005A690
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.m_animator = base.GetComponent<Animator>();
		if (this.m_nview)
		{
			this.m_nview.Register<bool>("UseDoor", new Action<long, bool>(this.RPC_UseDoor));
		}
		base.InvokeRepeating("UpdateState", 0f, 0.2f);
	}

	// Token: 0x06000CE5 RID: 3301 RVA: 0x0005C504 File Offset: 0x0005A704
	private void UpdateState()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		int @int = this.m_nview.GetZDO().GetInt("state", 0);
		this.SetState(@int);
	}

	// Token: 0x06000CE6 RID: 3302 RVA: 0x0005C540 File Offset: 0x0005A740
	private void SetState(int state)
	{
		if (this.m_animator.GetInteger("state") != state)
		{
			if (state != 0)
			{
				this.m_openEffects.Create(base.transform.position, base.transform.rotation, null, 1f);
			}
			else
			{
				this.m_closeEffects.Create(base.transform.position, base.transform.rotation, null, 1f);
			}
			this.m_animator.SetInteger("state", state);
		}
	}

	// Token: 0x06000CE7 RID: 3303 RVA: 0x0005C5C8 File Offset: 0x0005A7C8
	private bool CanInteract()
	{
		return (!(this.m_keyItem != null) || this.m_nview.GetZDO().GetInt("state", 0) == 0) && (this.m_animator.GetCurrentAnimatorStateInfo(0).IsTag("open") || this.m_animator.GetCurrentAnimatorStateInfo(0).IsTag("closed"));
	}

	// Token: 0x06000CE8 RID: 3304 RVA: 0x0005C634 File Offset: 0x0005A834
	public string GetHoverText()
	{
		if (!this.m_nview.IsValid())
		{
			return "";
		}
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, false, false))
		{
			return Localization.instance.Localize(this.m_name + "\n$piece_noaccess");
		}
		if (!this.CanInteract())
		{
			return Localization.instance.Localize(this.m_name);
		}
		if (this.m_nview.GetZDO().GetInt("state", 0) != 0)
		{
			return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_door_close");
		}
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_door_open");
	}

	// Token: 0x06000CE9 RID: 3305 RVA: 0x0005C6EE File Offset: 0x0005A8EE
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000CEA RID: 3306 RVA: 0x0005C6F8 File Offset: 0x0005A8F8
	public bool Interact(Humanoid character, bool hold)
	{
		if (hold)
		{
			return false;
		}
		if (!this.CanInteract())
		{
			return false;
		}
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			return true;
		}
		if (this.m_keyItem != null)
		{
			if (!this.HaveKey(character))
			{
				this.m_lockedEffects.Create(base.transform.position, base.transform.rotation, null, 1f);
				character.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_door_needkey", new string[]
				{
					this.m_keyItem.m_itemData.m_shared.m_name
				}), 0, null);
				return true;
			}
			character.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_door_usingkey", new string[]
			{
				this.m_keyItem.m_itemData.m_shared.m_name
			}), 0, null);
		}
		Vector3 normalized = (character.transform.position - base.transform.position).normalized;
		bool flag = Vector3.Dot(base.transform.forward, normalized) < 0f;
		this.m_nview.InvokeRPC("UseDoor", new object[]
		{
			flag
		});
		return true;
	}

	// Token: 0x06000CEB RID: 3307 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000CEC RID: 3308 RVA: 0x0005C83C File Offset: 0x0005AA3C
	private bool HaveKey(Humanoid player)
	{
		return this.m_keyItem == null || player.GetInventory().HaveItem(this.m_keyItem.m_itemData.m_shared.m_name);
	}

	// Token: 0x06000CED RID: 3309 RVA: 0x0005C870 File Offset: 0x0005AA70
	private void RPC_UseDoor(long uid, bool forward)
	{
		if (!this.CanInteract())
		{
			return;
		}
		if (this.m_nview.GetZDO().GetInt("state", 0) == 0)
		{
			if (forward)
			{
				this.m_nview.GetZDO().Set("state", 1);
			}
			else
			{
				this.m_nview.GetZDO().Set("state", -1);
			}
		}
		else
		{
			this.m_nview.GetZDO().Set("state", 0);
		}
		this.UpdateState();
	}

	// Token: 0x04000BCB RID: 3019
	public string m_name = "door";

	// Token: 0x04000BCC RID: 3020
	public GameObject m_doorObject;

	// Token: 0x04000BCD RID: 3021
	public ItemDrop m_keyItem;

	// Token: 0x04000BCE RID: 3022
	public EffectList m_openEffects = new EffectList();

	// Token: 0x04000BCF RID: 3023
	public EffectList m_closeEffects = new EffectList();

	// Token: 0x04000BD0 RID: 3024
	public EffectList m_lockedEffects = new EffectList();

	// Token: 0x04000BD1 RID: 3025
	private ZNetView m_nview;

	// Token: 0x04000BD2 RID: 3026
	private Animator m_animator;
}
