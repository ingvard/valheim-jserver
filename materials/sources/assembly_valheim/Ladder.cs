using System;
using UnityEngine;

// Token: 0x020000D9 RID: 217
public class Ladder : MonoBehaviour, Interactable, Hoverable
{
	// Token: 0x06000DE9 RID: 3561 RVA: 0x00063628 File Offset: 0x00061828
	public bool Interact(Humanoid character, bool hold)
	{
		if (hold)
		{
			return false;
		}
		if (!this.InUseDistance(character))
		{
			return false;
		}
		character.transform.position = this.m_targetPos.position;
		character.transform.rotation = this.m_targetPos.rotation;
		character.SetLookDir(this.m_targetPos.forward);
		return false;
	}

	// Token: 0x06000DEA RID: 3562 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000DEB RID: 3563 RVA: 0x00063683 File Offset: 0x00061883
	public string GetHoverText()
	{
		if (!this.InUseDistance(Player.m_localPlayer))
		{
			return Localization.instance.Localize("<color=grey>$piece_toofar</color>");
		}
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use");
	}

	// Token: 0x06000DEC RID: 3564 RVA: 0x000636BC File Offset: 0x000618BC
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000DED RID: 3565 RVA: 0x000636C4 File Offset: 0x000618C4
	private bool InUseDistance(Humanoid human)
	{
		return Vector3.Distance(human.transform.position, base.transform.position) < this.m_useDistance;
	}

	// Token: 0x04000C93 RID: 3219
	public Transform m_targetPos;

	// Token: 0x04000C94 RID: 3220
	public string m_name = "Ladder";

	// Token: 0x04000C95 RID: 3221
	public float m_useDistance = 2f;
}
