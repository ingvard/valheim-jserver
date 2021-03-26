using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000F5 RID: 245
public class Sign : MonoBehaviour, Hoverable, Interactable, TextReceiver
{
	// Token: 0x06000F12 RID: 3858 RVA: 0x0006BBA4 File Offset: 0x00069DA4
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

	// Token: 0x06000F13 RID: 3859 RVA: 0x0006BBDC File Offset: 0x00069DDC
	public string GetHoverText()
	{
		if (!PrivateArea.CheckAccess(base.transform.position, 0f, false, false))
		{
			return "\"" + this.GetText() + "\"";
		}
		return "\"" + this.GetText() + "\"\n" + Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use");
	}

	// Token: 0x06000F14 RID: 3860 RVA: 0x0006BC47 File Offset: 0x00069E47
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000F15 RID: 3861 RVA: 0x0006BC4F File Offset: 0x00069E4F
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

	// Token: 0x06000F16 RID: 3862 RVA: 0x0006BC88 File Offset: 0x00069E88
	private void UpdateText()
	{
		string text = this.GetText();
		if (this.m_textWidget.text == text)
		{
			return;
		}
		this.m_textWidget.text = text;
	}

	// Token: 0x06000F17 RID: 3863 RVA: 0x0006BCBC File Offset: 0x00069EBC
	public string GetText()
	{
		return this.m_nview.GetZDO().GetString("text", this.m_defaultText);
	}

	// Token: 0x06000F18 RID: 3864 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000F19 RID: 3865 RVA: 0x0006BCDC File Offset: 0x00069EDC
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

	// Token: 0x04000E04 RID: 3588
	public Text m_textWidget;

	// Token: 0x04000E05 RID: 3589
	public string m_name = "Sign";

	// Token: 0x04000E06 RID: 3590
	public string m_defaultText = "Sign";

	// Token: 0x04000E07 RID: 3591
	public int m_characterLimit = 50;

	// Token: 0x04000E08 RID: 3592
	private ZNetView m_nview;
}
