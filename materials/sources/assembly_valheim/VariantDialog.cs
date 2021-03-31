using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000066 RID: 102
public class VariantDialog : MonoBehaviour
{
	// Token: 0x06000669 RID: 1641 RVA: 0x00035F9C File Offset: 0x0003419C
	public void Setup(ItemDrop.ItemData item)
	{
		base.gameObject.SetActive(true);
		foreach (GameObject obj in this.m_elements)
		{
			UnityEngine.Object.Destroy(obj);
		}
		this.m_elements.Clear();
		for (int i = 0; i < item.m_shared.m_variants; i++)
		{
			Sprite sprite = item.m_shared.m_icons[i];
			int num = i / this.m_gridWidth;
			int num2 = i % this.m_gridWidth;
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_elementPrefab, Vector3.zero, Quaternion.identity, this.m_listRoot);
			gameObject.SetActive(true);
			(gameObject.transform as RectTransform).anchoredPosition = new Vector2((float)num2 * this.m_spacing, (float)(-(float)num) * this.m_spacing);
			Button component = gameObject.transform.Find("Button").GetComponent<Button>();
			int buttonIndex = i;
			component.onClick.AddListener(delegate
			{
				this.OnClicked(buttonIndex);
			});
			component.GetComponent<Image>().sprite = sprite;
			this.m_elements.Add(gameObject);
		}
	}

	// Token: 0x0600066A RID: 1642 RVA: 0x000347C8 File Offset: 0x000329C8
	public void OnClose()
	{
		base.gameObject.SetActive(false);
	}

	// Token: 0x0600066B RID: 1643 RVA: 0x000360EC File Offset: 0x000342EC
	private void OnClicked(int index)
	{
		ZLog.Log("Clicked button " + index);
		base.gameObject.SetActive(false);
		this.m_selected(index);
	}

	// Token: 0x0400072A RID: 1834
	public Transform m_listRoot;

	// Token: 0x0400072B RID: 1835
	public GameObject m_elementPrefab;

	// Token: 0x0400072C RID: 1836
	public float m_spacing = 70f;

	// Token: 0x0400072D RID: 1837
	public int m_gridWidth = 5;

	// Token: 0x0400072E RID: 1838
	private List<GameObject> m_elements = new List<GameObject>();

	// Token: 0x0400072F RID: 1839
	public Action<int> m_selected;
}
