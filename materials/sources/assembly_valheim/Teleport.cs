using System;
using UnityEngine;

// Token: 0x020000FD RID: 253
public class Teleport : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000F69 RID: 3945 RVA: 0x0006D86B File Offset: 0x0006BA6B
	public string GetHoverText()
	{
		return Localization.instance.Localize("[<color=yellow><b>$KEY_Use</b></color>] " + this.m_hoverText);
	}

	// Token: 0x06000F6A RID: 3946 RVA: 0x0000AC8C File Offset: 0x00008E8C
	public string GetHoverName()
	{
		return "";
	}

	// Token: 0x06000F6B RID: 3947 RVA: 0x0006D888 File Offset: 0x0006BA88
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

	// Token: 0x06000F6C RID: 3948 RVA: 0x0006D8C0 File Offset: 0x0006BAC0
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

	// Token: 0x06000F6D RID: 3949 RVA: 0x0006D928 File Offset: 0x0006BB28
	private Vector3 GetTeleportPoint()
	{
		return base.transform.position + base.transform.forward - base.transform.up;
	}

	// Token: 0x06000F6E RID: 3950 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000F6F RID: 3951 RVA: 0x000027E0 File Offset: 0x000009E0
	private void OnDrawGizmos()
	{
	}

	// Token: 0x04000E3F RID: 3647
	public string m_hoverText = "$location_enter";

	// Token: 0x04000E40 RID: 3648
	public string m_enterText = "";

	// Token: 0x04000E41 RID: 3649
	public Teleport m_targetPoint;
}
