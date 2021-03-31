using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000F5 RID: 245
public class Sign : MonoBehaviour, Hoverable, Interactable, TextReceiver
{
	// Token: 0x06000F13 RID: 3859 RVA: 0x0006BD2C File Offset: 0x00069F2C
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.UpdateText();
		base.InvokeRepeating("UpdateText", 2f, 2f);
	}

	// Token: 0x06000F14 RID: 3860 RVA: 0x0006BD64 File Offset: 0x00069F64
	public string GetHoverText()
	{
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, false, false))
		{
			return "\"" + this.GetText() + "\"";
		}
		return "\"" + this.GetText() + "\"\n" + Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use");
	}

	// Token: 0x06000F15 RID: 3861 RVA: 0x0006BDCF File Offset: 0x00069FCF
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000F16 RID: 3862 RVA: 0x0006BDD7 File Offset: 0x00069FD7
	public bool Interact(Humanoid character, bool hold)
	{
		if (hold)
		{
			return false;
		}
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			return false;
		}
		TextInput.instance.RequestText(this, "$piece_sign_input", this.m_characterLimit);
		return true;
	}

	// Token: 0x06000F17 RID: 3863 RVA: 0x0006BE10 File Offset: 0x0006A010
	private void UpdateText()
	{
		string text = this.GetText();
		if (this.m_textWidget.text == text)
		{
			return;
		}
		this.m_textWidget.text = text;
	}

	// Token: 0x06000F18 RID: 3864 RVA: 0x0006BE44 File Offset: 0x0006A044
	public string GetText()
	{
		return this.m_nview.GetZDO().GetString("text", this.m_defaultText);
	}

	// Token: 0x06000F19 RID: 3865 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000F1A RID: 3866 RVA: 0x0006BE64 File Offset: 0x0006A064
	public void SetText(string text)
	{
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, true, false))
		{
			return;
		}
		this.m_nview.ClaimOwnership();
		this.m_textWidget.text = text;
		this.m_nview.GetZDO().Set("text", text);
	}

	// Token: 0x04000E0A RID: 3594
	public Text m_textWidget;

	// Token: 0x04000E0B RID: 3595
	public string m_name = "Sign";

	// Token: 0x04000E0C RID: 3596
	public string m_defaultText = "Sign";

	// Token: 0x04000E0D RID: 3597
	public int m_characterLimit = 50;

	// Token: 0x04000E0E RID: 3598
	private ZNetView m_nview;
}
