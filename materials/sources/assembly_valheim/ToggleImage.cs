using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000064 RID: 100
public class ToggleImage : MonoBehaviour
{
	// Token: 0x06000660 RID: 1632 RVA: 0x00035E8E File Offset: 0x0003408E
	private void Awake()
	{
		this.m_toggle = base.GetComponent<Toggle>();
	}

	// Token: 0x06000661 RID: 1633 RVA: 0x00035E9C File Offset: 0x0003409C
	private void Update()
	{
		if (this.m_toggle.isOn)
		{
			this.m_targetImage.sprite = this.m_onImage;
			return;
		}
		this.m_targetImage.sprite = this.m_offImage;
	}

	// Token: 0x0400071F RID: 1823
	private Toggle m_toggle;

	// Token: 0x04000720 RID: 1824
	public Image m_targetImage;

	// Token: 0x04000721 RID: 1825
	public Sprite m_onImage;

	// Token: 0x04000722 RID: 1826
	public Sprite m_offImage;
}
