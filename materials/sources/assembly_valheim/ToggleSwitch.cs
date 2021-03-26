using System;
using UnityEngine;

// Token: 0x02000107 RID: 263
public class ToggleSwitch : MonoBehaviour, Interactable, Hoverable
{
	// Token: 0x06000FA5 RID: 4005 RVA: 0x0006E73C File Offset: 0x0006C93C
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

	// Token: 0x06000FA6 RID: 4006 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000FA7 RID: 4007 RVA: 0x0006E759 File Offset: 0x0006C959
	public string GetHoverText()
	{
		return this.m_hoverText;
	}

	// Token: 0x06000FA8 RID: 4008 RVA: 0x0006E761 File Offset: 0x0006C961
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000FA9 RID: 4009 RVA: 0x0006E769 File Offset: 0x0006C969
	public void SetState(bool enabled)
	{
		this.m_state = enabled;
		this.m_renderer.material = (this.m_state ? this.m_enableMaterial : this.m_disableMaterial);
	}

	// Token: 0x04000E6A RID: 3690
	public MeshRenderer m_renderer;

	// Token: 0x04000E6B RID: 3691
	public Material m_enableMaterial;

	// Token: 0x04000E6C RID: 3692
	public Material m_disableMaterial;

	// Token: 0x04000E6D RID: 3693
	public Action<ToggleSwitch, Humanoid> m_onUse;

	// Token: 0x04000E6E RID: 3694
	public string m_hoverText = "";

	// Token: 0x04000E6F RID: 3695
	public string m_name = "";

	// Token: 0x04000E70 RID: 3696
	private bool m_state;
}
