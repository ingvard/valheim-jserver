using System;
using UnityEngine;

// Token: 0x0200010E RID: 270
public class Vegvisir : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x06000FF1 RID: 4081 RVA: 0x0007041E File Offset: 0x0006E61E
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_name + " " + this.m_pinName + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_register_location ");
	}

	// Token: 0x06000FF2 RID: 4082 RVA: 0x00070445 File Offset: 0x0006E645
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000FF3 RID: 4083 RVA: 0x00070450 File Offset: 0x0006E650
	public bool Interact(Humanoid character, bool hold)
	{
		if (hold)
		{
			return false;
		}
		Game.instance.DiscoverClosestLocation(this.m_locationName, base.transform.position, this.m_pinName, (int)this.m_pinType);
		Gogan.LogEvent("Game", "Vegvisir", this.m_locationName, 0L);
		return true;
	}

	// Token: 0x06000FF4 RID: 4084 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x04000ED6 RID: 3798
	public string m_name = "$piece_vegvisir";

	// Token: 0x04000ED7 RID: 3799
	public string m_locationName = "";

	// Token: 0x04000ED8 RID: 3800
	public string m_pinName = "Pin";

	// Token: 0x04000ED9 RID: 3801
	public Minimap.PinType m_pinType;
}
