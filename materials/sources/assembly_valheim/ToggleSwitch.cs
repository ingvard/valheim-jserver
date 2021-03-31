using System;
using UnityEngine;

// Token: 0x02000107 RID: 263
public class ToggleSwitch : MonoBehaviour, Interactable, Hoverable
{
	// Token: 0x06000FA6 RID: 4006 RVA: 0x0006E8C4 File Offset: 0x0006CAC4
	public bool Interact(Humanoid character, bool hold)
	{
		if (hold)
		{
			return false;
		}
		if (this.m_onUse != null)
		{
			this.m_onUse(this, character);
		}
		return true;
	}

	// Token: 0x06000FA7 RID: 4007 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000FA8 RID: 4008 RVA: 0x0006E8E1 File Offset: 0x0006CAE1
	public string GetHoverText()
	{
		return this.m_hoverText;
	}

	// Token: 0x06000FA9 RID: 4009 RVA: 0x0006E8E9 File Offset: 0x0006CAE9
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000FAA RID: 4010 RVA: 0x0006E8F1 File Offset: 0x0006CAF1
	public void SetState(bool enabled)
	{
		this.m_state = enabled;
		this.m_renderer.material = (this.m_state ? this.m_enableMaterial : this.m_disableMaterial);
	}

	// Token: 0x04000E70 RID: 3696
	public MeshRenderer m_renderer;

	// Token: 0x04000E71 RID: 3697
	public Material m_enableMaterial;

	// Token: 0x04000E72 RID: 3698
	public Material m_disableMaterial;

	// Token: 0x04000E73 RID: 3699
	public Action<ToggleSwitch, Humanoid> m_onUse;

	// Token: 0x04000E74 RID: 3700
	public string m_hoverText = "";

	// Token: 0x04000E75 RID: 3701
	public string m_name = "";

	// Token: 0x04000E76 RID: 3702
	private bool m_state;
}
