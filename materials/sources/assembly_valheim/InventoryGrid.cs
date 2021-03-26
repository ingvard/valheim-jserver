using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000055 RID: 85
public class InventoryGrid : MonoBehaviour
{
	// Token: 0x06000527 RID: 1319 RVA: 0x000027E0 File Offset: 0x000009E0
	protected void Awake()
	{
	}

	// Token: 0x06000528 RID: 1320 RVA: 0x0002B428 File Offset: 0x00029628
	public void ResetView()
	{
		RectTransform rectTransform = base.transform as RectTransform;
		if (this.m_gridRoot.rect.height > rectTransform.rect.height)
		{
			this.m_gridRoot.pivot = new Vector2(this.m_gridRoot.pivot.x, 1f);
		}
		else
		{
			this.m_gridRoot.pivot = new Vector2(this.m_gridRoot.pivot.x, 0.5f);
		}
		this.m_gridRoot.anchoredPosition = new Vector2(0f, 0f);
	}

	// Token: 0x06000529 RID: 1321 RVA: 0x0002B4CA File Offset: 0x000296CA
	public void UpdateInventory(Inventory inventory, Player player, ItemDrop.ItemData dragItem)
	{
		this.m_inventory = inventory;
		this.UpdateGamepad();
		this.UpdateGui(player, dragItem);
	}

	// Token: 0x0600052A RID: 1322 RVA: 0x0002B4E4 File Offset: 0x000296E4
	private void UpdateGamepad()
	{
		if (!this.m_uiGroup.IsActive())
		{
			return;
		}
		if (ZInput.GetButtonDown("JoyDPadLeft") || ZInput.GetButtonDown("JoyLStickLeft"))
		{
			this.m_selected.x = Mathf.Max(0, this.m_selected.x - 1);
		}
		if (ZInput.GetButtonDown("JoyDPadRight") || ZInput.GetButtonDown("JoyLStickRight"))
		{
			this.m_selected.x = Mathf.Min(this.m_width - 1, this.m_selected.x + 1);
		}
		if (ZInput.GetButtonDown("JoyDPadUp") || ZInput.GetButtonDown("JoyLStickUp"))
		{
			this.m_selected.y = Mathf.Max(0, this.m_selected.y - 1);
		}
		if (ZInput.GetButtonDown("JoyDPadDown") || ZInput.GetButtonDown("JoyLStickDown"))
		{
			this.m_selected.y = Mathf.Min(this.m_width - 1, this.m_selected.y + 1);
		}
		if (ZInput.GetButtonDown("JoyButtonA"))
		{
			InventoryGrid.Modifier arg = InventoryGrid.Modifier.Select;
			if (ZInput.GetButton("JoyLTrigger"))
			{
				arg = InventoryGrid.Modifier.Split;
			}
			if (ZInput.GetButton("JoyRTrigger"))
			{
				arg = InventoryGrid.Modifier.Move;
			}
			ItemDrop.ItemData gamepadSelectedItem = this.GetGamepadSelectedItem();
			this.m_onSelected(this, gamepadSelectedItem, this.m_selected, arg);
		}
		if (ZInput.GetButtonDown("JoyButtonX"))
		{
			ItemDrop.ItemData gamepadSelectedItem2 = this.GetGamepadSelectedItem();
			this.m_onRightClick(this, gamepadSelectedItem2, this.m_selected);
		}
	}

	// Token: 0x0600052B RID: 1323 RVA: 0x0002B650 File Offset: 0x00029850
	private void UpdateGui(Player player, ItemDrop.ItemData dragItem)
	{
		RectTransform rectTransform = base.transform as RectTransform;
		int width = this.m_inventory.GetWidth();
		int height = this.m_inventory.GetHeight();
		if (this.m_selected.x >= width - 1)
		{
			this.m_selected.x = width - 1;
		}
		if (this.m_selected.y >= height - 1)
		{
			this.m_selected.y = height - 1;
		}
		if (this.m_width != width || this.m_height != height)
		{
			this.m_width = width;
			this.m_height = height;
			foreach (InventoryGrid.Element element in this.m_elements)
			{
				UnityEngine.Object.Destroy(element.m_go);
			}
			this.m_elements.Clear();
			Vector2 widgetSize = this.GetWidgetSize();
			Vector2 a = new Vector2(rectTransform.rect.width / 2f, 0f) - new Vector2(widgetSize.x, 0f) * 0.5f;
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					Vector2 b = new Vector3((float)j * this.m_elementSpace, (float)i * -this.m_elementSpace);
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_elementPrefab, this.m_gridRoot);
					(gameObject.transform as RectTransform).anchoredPosition = a + b;
					UIInputHandler componentInChildren = gameObject.GetComponentInChildren<UIInputHandler>();
					componentInChildren.m_onRightDown = (Action<UIInputHandler>)Delegate.Combine(componentInChildren.m_onRightDown, new Action<UIInputHandler>(this.OnRightClick));
					componentInChildren.m_onLeftDown = (Action<UIInputHandler>)Delegate.Combine(componentInChildren.m_onLeftDown, new Action<UIInputHandler>(this.OnLeftClick));
					Text component = gameObject.transform.Find("binding").GetComponent<Text>();
					if (player && i == 0)
					{
						component.text = (j + 1).ToString();
					}
					else
					{
						component.enabled = false;
					}
					InventoryGrid.Element element2 = new InventoryGrid.Element();
					element2.m_pos = new Vector2i(j, i);
					element2.m_go = gameObject;
					element2.m_icon = gameObject.transform.Find("icon").GetComponent<Image>();
					element2.m_amount = gameObject.transform.Find("amount").GetComponent<Text>();
					element2.m_quality = gameObject.transform.Find("quality").GetComponent<Text>();
					element2.m_equiped = gameObject.transform.Find("equiped").GetComponent<Image>();
					element2.m_queued = gameObject.transform.Find("queued").GetComponent<Image>();
					element2.m_noteleport = gameObject.transform.Find("noteleport").GetComponent<Image>();
					element2.m_selected = gameObject.transform.Find("selected").gameObject;
					element2.m_tooltip = gameObject.GetComponent<UITooltip>();
					element2.m_durability = gameObject.transform.Find("durability").GetComponent<GuiBar>();
					this.m_elements.Add(element2);
				}
			}
		}
		foreach (InventoryGrid.Element element3 in this.m_elements)
		{
			element3.m_used = false;
		}
		bool flag = this.m_uiGroup.IsActive() && ZInput.IsGamepadActive();
		foreach (ItemDrop.ItemData itemData in this.m_inventory.GetAllItems())
		{
			InventoryGrid.Element element4 = this.GetElement(itemData.m_gridPos.x, itemData.m_gridPos.y, width);
			element4.m_used = true;
			element4.m_icon.enabled = true;
			element4.m_icon.sprite = itemData.GetIcon();
			element4.m_icon.color = ((itemData == dragItem) ? Color.grey : Color.white);
			element4.m_durability.gameObject.SetActive(itemData.m_shared.m_useDurability);
			if (itemData.m_shared.m_useDurability)
			{
				if (itemData.m_durability <= 0f)
				{
					element4.m_durability.SetValue(1f);
					element4.m_durability.SetColor((Mathf.Sin(Time.time * 10f) > 0f) ? Color.red : new Color(0f, 0f, 0f, 0f));
				}
				else
				{
					element4.m_durability.SetValue(itemData.GetDurabilityPercentage());
					element4.m_durability.ResetColor();
				}
			}
			element4.m_equiped.enabled = (player && itemData.m_equiped);
			element4.m_queued.enabled = (player && player.IsItemQueued(itemData));
			element4.m_noteleport.enabled = !itemData.m_shared.m_teleportable;
			if (dragItem == null)
			{
				this.CreateItemTooltip(itemData, element4.m_tooltip);
			}
			element4.m_quality.enabled = (itemData.m_shared.m_maxQuality > 1);
			if (itemData.m_shared.m_maxQuality > 1)
			{
				element4.m_quality.text = itemData.m_quality.ToString();
			}
			element4.m_amount.enabled = (itemData.m_shared.m_maxStackSize > 1);
			if (itemData.m_shared.m_maxStackSize > 1)
			{
				element4.m_amount.text = itemData.m_stack.ToString() + "/" + itemData.m_shared.m_maxStackSize.ToString();
			}
		}
		foreach (InventoryGrid.Element element5 in this.m_elements)
		{
			element5.m_selected.SetActive(flag && element5.m_pos == this.m_selected);
			if (!element5.m_used)
			{
				element5.m_durability.gameObject.SetActive(false);
				element5.m_icon.enabled = false;
				element5.m_amount.enabled = false;
				element5.m_quality.enabled = false;
				element5.m_equiped.enabled = false;
				element5.m_queued.enabled = false;
				element5.m_noteleport.enabled = false;
				element5.m_tooltip.m_text = "";
				element5.m_tooltip.m_topic = "";
			}
		}
		float size = (float)height * this.m_elementSpace;
		this.m_gridRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
	}

	// Token: 0x0600052C RID: 1324 RVA: 0x0002BDA4 File Offset: 0x00029FA4
	private void CreateItemTooltip(ItemDrop.ItemData item, UITooltip tooltip)
	{
		tooltip.Set(item.m_shared.m_name, item.GetTooltip());
	}

	// Token: 0x0600052D RID: 1325 RVA: 0x0002BDBD File Offset: 0x00029FBD
	public Vector2 GetWidgetSize()
	{
		return new Vector2((float)this.m_width * this.m_elementSpace, (float)this.m_height * this.m_elementSpace);
	}

	// Token: 0x0600052E RID: 1326 RVA: 0x0002BDE0 File Offset: 0x00029FE0
	private void OnRightClick(UIInputHandler element)
	{
		GameObject gameObject = element.gameObject;
		Vector2i buttonPos = this.GetButtonPos(gameObject);
		ItemDrop.ItemData itemAt = this.m_inventory.GetItemAt(buttonPos.x, buttonPos.y);
		if (this.m_onRightClick != null)
		{
			this.m_onRightClick(this, itemAt, buttonPos);
		}
	}

	// Token: 0x0600052F RID: 1327 RVA: 0x0002BE2C File Offset: 0x0002A02C
	private void OnLeftClick(UIInputHandler clickHandler)
	{
		GameObject gameObject = clickHandler.gameObject;
		Vector2i buttonPos = this.GetButtonPos(gameObject);
		ItemDrop.ItemData itemAt = this.m_inventory.GetItemAt(buttonPos.x, buttonPos.y);
		InventoryGrid.Modifier arg = InventoryGrid.Modifier.Select;
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			arg = InventoryGrid.Modifier.Split;
		}
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
		{
			arg = InventoryGrid.Modifier.Move;
		}
		if (this.m_onSelected != null)
		{
			this.m_onSelected(this, itemAt, buttonPos, arg);
		}
	}

	// Token: 0x06000530 RID: 1328 RVA: 0x0002BEB0 File Offset: 0x0002A0B0
	private InventoryGrid.Element GetElement(int x, int y, int width)
	{
		int index = y * width + x;
		return this.m_elements[index];
	}

	// Token: 0x06000531 RID: 1329 RVA: 0x0002BED0 File Offset: 0x0002A0D0
	private Vector2i GetButtonPos(GameObject go)
	{
		for (int i = 0; i < this.m_elements.Count; i++)
		{
			if (this.m_elements[i].m_go == go)
			{
				int num = i / this.m_width;
				return new Vector2i(i - num * this.m_width, num);
			}
		}
		return new Vector2i(-1, -1);
	}

	// Token: 0x06000532 RID: 1330 RVA: 0x0002BF30 File Offset: 0x0002A130
	public bool DropItem(Inventory fromInventory, ItemDrop.ItemData item, int amount, Vector2i pos)
	{
		ItemDrop.ItemData itemAt = this.m_inventory.GetItemAt(pos.x, pos.y);
		if (itemAt == item)
		{
			return true;
		}
		if (itemAt != null && (itemAt.m_shared.m_name != item.m_shared.m_name || (item.m_shared.m_maxQuality > 1 && itemAt.m_quality != item.m_quality) || itemAt.m_shared.m_maxStackSize == 1) && item.m_stack == amount)
		{
			fromInventory.RemoveItem(item);
			fromInventory.MoveItemToThis(this.m_inventory, itemAt, itemAt.m_stack, item.m_gridPos.x, item.m_gridPos.y);
			this.m_inventory.MoveItemToThis(fromInventory, item, amount, pos.x, pos.y);
			return true;
		}
		return this.m_inventory.MoveItemToThis(fromInventory, item, amount, pos.x, pos.y);
	}

	// Token: 0x06000533 RID: 1331 RVA: 0x0002C020 File Offset: 0x0002A220
	public ItemDrop.ItemData GetItem(Vector2i cursorPosition)
	{
		foreach (InventoryGrid.Element element in this.m_elements)
		{
			if (RectTransformUtility.RectangleContainsScreenPoint(element.m_go.transform as RectTransform, cursorPosition.ToVector2()))
			{
				Vector2i buttonPos = this.GetButtonPos(element.m_go);
				return this.m_inventory.GetItemAt(buttonPos.x, buttonPos.y);
			}
		}
		return null;
	}

	// Token: 0x06000534 RID: 1332 RVA: 0x0002C0B4 File Offset: 0x0002A2B4
	public Inventory GetInventory()
	{
		return this.m_inventory;
	}

	// Token: 0x06000535 RID: 1333 RVA: 0x0002C0BC File Offset: 0x0002A2BC
	public void SetSelection(Vector2i pos)
	{
		this.m_selected = pos;
	}

	// Token: 0x06000536 RID: 1334 RVA: 0x0002C0C5 File Offset: 0x0002A2C5
	public ItemDrop.ItemData GetGamepadSelectedItem()
	{
		if (!this.m_uiGroup.IsActive())
		{
			return null;
		}
		return this.m_inventory.GetItemAt(this.m_selected.x, this.m_selected.y);
	}

	// Token: 0x06000537 RID: 1335 RVA: 0x0002C0F8 File Offset: 0x0002A2F8
	public RectTransform GetGamepadSelectedElement()
	{
		if (!this.m_uiGroup.IsActive())
		{
			return null;
		}
		if (this.m_selected.x < 0 || this.m_selected.x >= this.m_width || this.m_selected.y < 0 || this.m_selected.y >= this.m_height)
		{
			return null;
		}
		return this.GetElement(this.m_selected.x, this.m_selected.y, this.m_width).m_go.transform as RectTransform;
	}

	// Token: 0x040005A6 RID: 1446
	public Action<InventoryGrid, ItemDrop.ItemData, Vector2i, InventoryGrid.Modifier> m_onSelected;

	// Token: 0x040005A7 RID: 1447
	public Action<InventoryGrid, ItemDrop.ItemData, Vector2i> m_onRightClick;

	// Token: 0x040005A8 RID: 1448
	public GameObject m_elementPrefab;

	// Token: 0x040005A9 RID: 1449
	public RectTransform m_gridRoot;

	// Token: 0x040005AA RID: 1450
	public Scrollbar m_scrollbar;

	// Token: 0x040005AB RID: 1451
	public UIGroupHandler m_uiGroup;

	// Token: 0x040005AC RID: 1452
	public float m_elementSpace = 10f;

	// Token: 0x040005AD RID: 1453
	private int m_width = 4;

	// Token: 0x040005AE RID: 1454
	private int m_height = 4;

	// Token: 0x040005AF RID: 1455
	private Vector2i m_selected = new Vector2i(0, 0);

	// Token: 0x040005B0 RID: 1456
	private Inventory m_inventory;

	// Token: 0x040005B1 RID: 1457
	private List<InventoryGrid.Element> m_elements = new List<InventoryGrid.Element>();

	// Token: 0x0200014C RID: 332
	private class Element
	{
		// Token: 0x040010E8 RID: 4328
		public Vector2i m_pos;

		// Token: 0x040010E9 RID: 4329
		public GameObject m_go;

		// Token: 0x040010EA RID: 4330
		public Image m_icon;

		// Token: 0x040010EB RID: 4331
		public Text m_amount;

		// Token: 0x040010EC RID: 4332
		public Text m_quality;

		// Token: 0x040010ED RID: 4333
		public Image m_equiped;

		// Token: 0x040010EE RID: 4334
		public Image m_queued;

		// Token: 0x040010EF RID: 4335
		public GameObject m_selected;

		// Token: 0x040010F0 RID: 4336
		public Image m_noteleport;

		// Token: 0x040010F1 RID: 4337
		public UITooltip m_tooltip;

		// Token: 0x040010F2 RID: 4338
		public GuiBar m_durability;

		// Token: 0x040010F3 RID: 4339
		public bool m_used;
	}

	// Token: 0x0200014D RID: 333
	public enum Modifier
	{
		// Token: 0x040010F5 RID: 4341
		Select,
		// Token: 0x040010F6 RID: 4342
		Split,
		// Token: 0x040010F7 RID: 4343
		Move
	}
}
