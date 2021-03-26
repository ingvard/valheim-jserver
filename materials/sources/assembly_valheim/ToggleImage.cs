using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000064 RID: 100
public class ToggleImage : MonoBehaviour
{
	// Token: 0x0600065F RID: 1631 RVA: 0x00035DDA File Offset: 0x00033FDA
	private void Awake()
	{
		this.m_toggle = base.GetComponent<Toggle>();
	}

	// Token: 0x06000660 RID: 1632 RVA: 0x00035DE8 File Offset: 0x00033FE8
	private void Update()
	{
		if (this.m_toggle.isOn)
		{
			this.m_targetImage.sprite = this.m_onImage;
			return;
		}
		this.m_targetImage.sprite = this.m_offImage;
	}

	// Token: 0x0400071B RID: 1819
	private Toggle m_toggle;

	// Token: 0x0400071C RID: 1820
	public Image m_targetImage;

	// Token: 0x0400071D RID: 1821
	public Sprite m_onImage;

	// Token: 0x0400071E RID: 1822
	public Sprite m_offImage;
}
