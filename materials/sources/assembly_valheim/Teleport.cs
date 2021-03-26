using System;
using UnityEngine;

// Token: 0x020000FD RID: 253
public class Teleport : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000F68 RID: 3944 RVA: 0x0006D6E3 File Offset: 0x0006B8E3
	public string GetHoverText()
	{
		return Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>] " + this.m_hoverText);
	}

	// Token: 0x06000F69 RID: 3945 RVA: 0x0000AC4C File Offset: 0x00008E4C
	public string GetHoverName()
	{
		return "";
	}

	// Token: 0x06000F6A RID: 3946 RVA: 0x0006D700 File Offset: 0x0006B900
	private void OnTriggerEnter(Collider collider)
	{
		Player component = collider.GetComponent<Player>();
		if (component == null)
		{
			return;
		}
		if (Player.m_localPlayer != component)
		{
			return;
		}
		this.Interact(component, false);
	}

	// Token: 0x06000F6B RID: 3947 RVA: 0x0006D738 File Offset: 0x0006B938
	public bool Interact(Humanoid character, bool hold)
	{
		if (hold)
		{
			return false;
		}
		if (this.m_targetPoint == null)
		{
			return false;
		}
		if (character.TeleportTo(this.m_targetPoint.GetTeleportPoint(), this.m_targetPoint.transform.rotation, false))
		{
			if (this.m_enterText.Length > 0)
			{
				MessageHud.instance.ShowBiomeFoundMsg(this.m_enterText, false);
			}
			return true;
		}
		return false;
	}

	// Token: 0x06000F6C RID: 3948 RVA: 0x0006D7A0 File Offset: 0x0006B9A0
	private Vector3 GetTeleportPoint()
	{
		return base.transform.position + base.transform.forward - base.transform.up;
	}

	// Token: 0x06000F6D RID: 3949 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000F6E RID: 3950 RVA: 0x000027E0 File Offset: 0x000009E0
	private void OnDrawGizmos()
	{
	}

	// Token: 0x04000E39 RID: 3641
	public string m_hoverText = "$location_enter";

	// Token: 0x04000E3A RID: 3642
	public string m_enterText = "";

	// Token: 0x04000E3B RID: 3643
	public Teleport m_targetPoint;
}
