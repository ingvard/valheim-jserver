﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000052 RID: 82
public class HotkeyBar : MonoBehaviour
{
	// Token: 0x060004F2 RID: 1266 RVA: 0x00028E98 File Offset: 0x00027098
	private void Update()
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer && !InventoryGui.IsVisible() && !Menu.IsVisible() && !GameCamera.InFreeFly())
		{
			if (ZInput.GetButtonDown("JoyDPadLeft"))
			{
				this.m_selected = Mathf.Max(0, this.m_selected - 1);
			}
			if (ZInput.GetButtonDown("JoyDPadRight"))
			{
				this.m_selected = Mathf.Min(this.m_elements.Count - 1, this.m_selected + 1);
			}
			if (ZInput.GetButtonDown("JoyDPadUp"))
			{
				localPlayer.UseHotbarItem(this.m_selected + 1);
			}
		}
		if (this.m_selected > this.m_elements.Count - 1)
		{
			this.m_selected = Mathf.Max(0, this.m_elements.Count - 1);
		}
		this.UpdateIcons(localPlayer);
	}

	// Token: 0x060004F3 RID: 1267 RVA: 0x00028F64 File Offset: 0x00027164
	private void UpdateIcons(Player player)
	{
		if (!player || player.IsDead())
		{
			foreach (HotkeyBar.ElementData elementData in this.m_elements)
			{
				UnityEngine.Object.Destroy(elementData.m_go);
			}
			this.m_elements.Clear();
			return;
		}
		player.GetInventory().GetBoundItems(this.m_items);
		this.m_items.Sort((ItemDrop.ItemData x, ItemDrop.ItemData y) => x.m_gridPos.x.CompareTo(y.m_gridPos.x));
		int num = 0;
		foreach (ItemDrop.ItemData itemData in this.m_items)
		{
			if (itemData.m_gridPos.x + 1 > num)
			{
				num = itemData.m_gridPos.x + 1;
			}
		}
		if (this.m_elements.Count != num)
		{
			foreach (HotkeyBar.ElementData elementData2 in this.m_elements)
			{
				UnityEngine.Object.Destroy(elementData2.m_go);
			}
			this.m_elements.Clear();
			for (int i = 0; i < num; i++)
			{
				HotkeyBar.ElementData elementData3 = new HotkeyBar.ElementData();
				elementData3.m_go = UnityEngine.Object.Instantiate<GameObject>(this.m_elementPrefab, base.transform);
				elementData3.m_go.transform.localPosition = new Vector3((float)i * this.m_elementSpace, 0f, 0f);
				elementData3.m_go.transform.Find("binding").GetComponent<Text>().text = (i + 1).ToString();
				elementData3.m_icon = elementData3.m_go.transform.transform.Find("icon").GetComponent<Image>();
				elementData3.m_durability = elementData3.m_go.transform.Find("durability").GetComponent<GuiBar>();
				elementData3.m_amount = elementData3.m_go.transform.Find("amount").GetComponent<Text>();
				elementData3.m_equiped = elementData3.m_go.transform.Find("equiped").gameObject;
				elementData3.m_queued = elementData3.m_go.transform.Find("queued").gameObject;
				elementData3.m_selection = elementData3.m_go.transform.Find("selected").gameObject;
				this.m_elements.Add(elementData3);
			}
		}
		foreach (HotkeyBar.ElementData elementData4 in this.m_elements)
		{
			elementData4.m_used = false;
		}
		bool flag = ZInput.IsGamepadActive();
		for (int j = 0; j < this.m_items.Count; j++)
		{
			ItemDrop.ItemData itemData2 = this.m_items[j];
			HotkeyBar.ElementData elementData5 = this.m_elements[itemData2.m_gridPos.x];
			elementData5.m_used = true;
			elementData5.m_icon.gameObject.SetActive(true);
			elementData5.m_icon.sprite = itemData2.GetIcon();
			elementData5.m_durability.gameObject.SetActive(itemData2.m_shared.m_useDurability);
			if (itemData2.m_shared.m_useDurability)
			{
				if (itemData2.m_durability <= 0f)
				{
					elementData5.m_durability.SetValue(1f);
					elementData5.m_durability.SetColor((Mathf.Sin(Time.time * 10f) > 0f) ? Color.red : new Color(0f, 0f, 0f, 0f));
				}
				else
				{
					elementData5.m_durability.SetValue(itemData2.GetDurabilityPercentage());
					elementData5.m_durability.ResetColor();
				}
			}
			elementData5.m_equiped.SetActive(itemData2.m_equiped);
			elementData5.m_queued.SetActive(player.IsItemQueued(itemData2));
			if (itemData2.m_shared.m_maxStackSize > 1)
			{
				elementData5.m_amount.gameObject.SetActive(true);
				elementData5.m_amount.text = itemData2.m_stack.ToString() + "/" + itemData2.m_shared.m_maxStackSize.ToString();
			}
			else
			{
				elementData5.m_amount.gameObject.SetActive(false);
			}
		}
		for (int k = 0; k < this.m_elements.Count; k++)
		{
			HotkeyBar.ElementData elementData6 = this.m_elements[k];
			elementData6.m_selection.SetActive(flag && k == this.m_selected);
			if (!elementData6.m_used)
			{
				elementData6.m_icon.gameObject.SetActive(false);
				elementData6.m_durability.gameObject.SetActive(false);
				elementData6.m_equiped.SetActive(false);
				elementData6.m_queued.SetActive(false);
				elementData6.m_amount.gameObject.SetActive(false);
			}
		}
	}

	// Token: 0x0400053A RID: 1338
	public GameObject m_elementPrefab;

	// Token: 0x0400053B RID: 1339
	public float m_elementSpace = 70f;

	// Token: 0x0400053C RID: 1340
	private int m_selected;

	// Token: 0x0400053D RID: 1341
	private List<HotkeyBar.ElementData> m_elements = new List<HotkeyBar.ElementData>();

	// Token: 0x0400053E RID: 1342
	private List<ItemDrop.ItemData> m_items = new List<ItemDrop.ItemData>();

	// Token: 0x02000149 RID: 329
	private class ElementData
	{
		// Token: 0x040010E0 RID: 4320
		public bool m_used;

		// Token: 0x040010E1 RID: 4321
		public GameObject m_go;

		// Token: 0x040010E2 RID: 4322
		public Image m_icon;

		// Token: 0x040010E3 RID: 4323
		public GuiBar m_durability;

		// Token: 0x040010E4 RID: 4324
		public Text m_amount;

		// Token: 0x040010E5 RID: 4325
		public GameObject m_equiped;

		// Token: 0x040010E6 RID: 4326
		public GameObject m_queued;

		// Token: 0x040010E7 RID: 4327
		public GameObject m_selection;
	}
}
