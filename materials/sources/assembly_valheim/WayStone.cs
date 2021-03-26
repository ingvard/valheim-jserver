using System;
using UnityEngine;

// Token: 0x02000111 RID: 273
public class WayStone : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x0600100A RID: 4106 RVA: 0x00070E63 File Offset: 0x0006F063
	private void Awake()
	{
		this.m_activeObject.SetActive(false);
	}

	// Token: 0x0600100B RID: 4107 RVA: 0x00070E71 File Offset: 0x0006F071
	public string GetHoverText()
	{
		if (this.m_activeObject.activeSelf)
		{
			return "Activated waystone";
		}
		return Localization.instance.Localize("Waystone\n[<color=yellow><b>$KEY_Use</b></color>] Activate");
	}

	// Token: 0x0600100C RID: 4108 RVA: 0x00070E95 File Offset: 0x0006F095
	public string GetHoverName()
	{
		return "Waystone";
	}

	// Token: 0x0600100D RID: 4109 RVA: 0x00070E9C File Offset: 0x0006F09C
	public bool Interact(Humanoid character, bool hold)
	{
		if (hold)
		{
			return false;
		}
		if (!this.m_activeObject.activeSelf)
		{
			character.Message(MessageHud.MessageType.Center, this.m_activateMessage, 0, null);
			this.m_activeObject.SetActive(true);
			this.m_activeEffect.Create(base.gameObject.transform.position, base.gameObject.transform.rotation, null, 1f);
		}
		return true;
	}

	// Token: 0x0600100E RID: 4110 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x0600100F RID: 4111 RVA: 0x00070F0C File Offset: 0x0006F10C
	private void FixedUpdate()
	{
		if (this.m_activeObject.activeSelf && Game.instance != null)
		{
			Vector3 forward = this.GetSpawnPoint() - base.transform.position;
			forward.y = 0f;
			forward.Normalize();
			this.m_activeObject.transform.rotation = Quaternion.LookRotation(forward);
		}
	}

	// Token: 0x06001010 RID: 4112 RVA: 0x00070F74 File Offset: 0x0006F174
	private Vector3 GetSpawnPoint()
	{
		PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
		if (playerProfile.HaveCustomSpawnPoint())
		{
			return playerProfile.GetCustomSpawnPoint();
		}
		return playerProfile.GetHomePoint();
	}

	// Token: 0x04000EEC RID: 3820
	[TextArea]
	public string m_activateMessage = "You touch the cold stone surface and you think of home.";

	// Token: 0x04000EED RID: 3821
	public GameObject m_activeObject;

	// Token: 0x04000EEE RID: 3822
	public EffectList m_activeEffect;
}
