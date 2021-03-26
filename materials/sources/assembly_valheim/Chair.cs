using System;
using UnityEngine;

// Token: 0x020000BD RID: 189
public class Chair : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000C89 RID: 3209 RVA: 0x00059800 File Offset: 0x00057A00
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

	// Token: 0x06000C8A RID: 3210 RVA: 0x0005985C File Offset: 0x00057A5C
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000C8B RID: 3211 RVA: 0x00059864 File Offset: 0x00057A64
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

	// Token: 0x06000C8C RID: 3212 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000C8D RID: 3213 RVA: 0x000598D3 File Offset: 0x00057AD3
	private bool InUseDistance(Humanoid human)
	{
		return Vector3.Distance(human.transform.position, this.m_attachPoint.position) < this.m_useDistance;
	}

	// Token: 0x04000B70 RID: 2928
	public string m_name = "Chair";

	// Token: 0x04000B71 RID: 2929
	public float m_useDistance = 2f;

	// Token: 0x04000B72 RID: 2930
	public Transform m_attachPoint;

	// Token: 0x04000B73 RID: 2931
	public Vector3 m_detachOffset = new Vector3(0f, 0.5f, 0f);

	// Token: 0x04000B74 RID: 2932
	public string m_attachAnimation = "attach_chair";

	// Token: 0x04000B75 RID: 2933
	private const float m_minSitDelay = 2f;

	// Token: 0x04000B76 RID: 2934
	private static float m_lastSitTime;
}
