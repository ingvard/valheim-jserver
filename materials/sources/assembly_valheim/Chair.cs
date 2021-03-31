using System;
using UnityEngine;

// Token: 0x020000BD RID: 189
public class Chair : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000C8A RID: 3210 RVA: 0x00059988 File Offset: 0x00057B88
	public string GetHoverText()
	{
		if (Time.time - Chair.m_lastSitTime < 2f)
		{
			return "";
		}
		if (!this.InUseDistance(Player.m_localPlayer))
		{
			return Localization.instance.Localize("<color=grey>$piece_toofar</color>");
		}
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use");
	}

	// Token: 0x06000C8B RID: 3211 RVA: 0x000599E4 File Offset: 0x00057BE4
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000C8C RID: 3212 RVA: 0x000599EC File Offset: 0x00057BEC
	public bool Interact(Humanoid human, bool hold)
	{
		if (hold)
		{
			return false;
		}
		Player player = human as Player;
		if (!this.InUseDistance(player))
		{
			return false;
		}
		if (Time.time - Chair.m_lastSitTime < 2f)
		{
			return false;
		}
		if (player)
		{
			if (player.IsEncumbered())
			{
				return false;
			}
			player.AttachStart(this.m_attachPoint, false, false, this.m_attachAnimation, this.m_detachOffset);
			Chair.m_lastSitTime = Time.time;
		}
		return false;
	}

	// Token: 0x06000C8D RID: 3213 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000C8E RID: 3214 RVA: 0x00059A5B File Offset: 0x00057C5B
	private bool InUseDistance(Humanoid human)
	{
		return Vector3.Distance(human.transform.position, this.m_attachPoint.position) < this.m_useDistance;
	}

	// Token: 0x04000B76 RID: 2934
	public string m_name = "Chair";

	// Token: 0x04000B77 RID: 2935
	public float m_useDistance = 2f;

	// Token: 0x04000B78 RID: 2936
	public Transform m_attachPoint;

	// Token: 0x04000B79 RID: 2937
	public Vector3 m_detachOffset = new Vector3(0f, 0.5f, 0f);

	// Token: 0x04000B7A RID: 2938
	public string m_attachAnimation = "attach_chair";

	// Token: 0x04000B7B RID: 2939
	private const float m_minSitDelay = 2f;

	// Token: 0x04000B7C RID: 2940
	private static float m_lastSitTime;
}
