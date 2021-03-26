using System;
using UnityEngine;

// Token: 0x020000D9 RID: 217
public class Ladder : MonoBehaviour, Interactable, Hoverable
{
	// Token: 0x06000DE8 RID: 3560 RVA: 0x000634A0 File Offset: 0x000616A0
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

	// Token: 0x06000DE9 RID: 3561 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000DEA RID: 3562 RVA: 0x000634FB File Offset: 0x000616FB
	public string GetHoverText()
	{
		if (!this.InUseDistance(Player.m_localPlayer))
		{
			return Localization.instance.Localize("<color=grey>$piece_toofar</color>");
		}
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use");
	}

	// Token: 0x06000DEB RID: 3563 RVA: 0x00063534 File Offset: 0x00061734
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000DEC RID: 3564 RVA: 0x0006353C File Offset: 0x0006173C
	private bool InUseDistance(Humanoid human)
	{
		return Vector3.Distance(human.transform.position, base.transform.position) < this.m_useDistance;
	}

	// Token: 0x04000C8D RID: 3213
	public Transform m_targetPos;

	// Token: 0x04000C8E RID: 3214
	public string m_name = "Ladder";

	// Token: 0x04000C8F RID: 3215
	public float m_useDistance = 2f;
}
