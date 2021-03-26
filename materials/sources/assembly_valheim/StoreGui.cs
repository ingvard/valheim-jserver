using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200005F RID: 95
public class StoreGui : MonoBehaviour
{
	// Token: 0x17000010 RID: 16
	// (get) Token: 0x06000623 RID: 1571 RVA: 0x00034740 File Offset: 0x00032940
	public static StoreGui instance
	{
		get
		{
			return StoreGui.m_instance;
		}
	}

	// Token: 0x06000624 RID: 1572 RVA: 0x00034748 File Offset: 0x00032948
	private void Awake()
	{
		StoreGui.m_instance = this;
		this.m_rootPanel.SetActive(false);
		this.m_itemlistBaseSize = this.m_listRoot.rect.height;
	}

	// Token: 0x06000625 RID: 1573 RVA: 0x00034780 File Offset: 0x00032980
	private void OnDestroy()
	{
		if (StoreGui.m_instance == this)
		{
			StoreGui.m_instance = null;
		}
	}

	// Token: 0x06000626 RID: 1574 RVA: 0x00034798 File Offset: 0x00032998
	private void Update()
	{
		if (!this.m_rootPanel.activeSelf)
		{
			this.m_hiddenFrames++;
			return;
		}
		this.m_hiddenFrames = 0;
		if (!this.m_trader)
		{
			this.Hide();
			return;
		}
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null || localPlayer.IsDead() || localPlayer.InCutscene())
		{
			this.Hide();
			return;
		}
		if (Vector3.Distance(this.m_trader.transform.position, Player.m_localPlayer.transform.position) > this.m_hideDistance)
		{
			this.Hide();
			return;
		}
		if (InventoryGui.IsVisible() || Minimap.IsOpen())
		{
			this.Hide();
			return;
		}
		if ((Chat.instance == null || !Chat.instance.HasFocus()) && !global::Console.IsVisible() && !Menu.IsVisible() && TextViewer.instance && !TextViewer.instance.IsVisible() && !localPlayer.InCutscene() && (ZInput.GetButtonDown("JoyButtonB") || Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("Use")))
		{
			ZInput.ResetButtonStatus("JoyButtonB");
			this.Hide();
		}
		this.UpdateBuyButton();
		this.UpdateSellButton();
		this.UpdateRecipeGamepadInput();
		this.m_coinText.text = this.GetPlayerCoins().ToString();
	}

	// Token: 0x06000627 RID: 1575 RVA: 0x000348F3 File Offset: 0x00032AF3
	public void Show(Trader trader)
	{
		if (this.m_trader == trader && StoreGui.IsVisible())
		{
			return;
		}
		this.m_trader = trader;
		this.m_rootPanel.SetActive(true);
		this.FillList();
	}

	// Token: 0x06000628 RID: 1576 RVA: 0x00034924 File Offset: 0x00032B24
	public void Hide()
	{
		this.m_trader = null;
		this.m_rootPanel.SetActive(false);
	}

	// Token: 0x06000629 RID: 1577 RVA: 0x00034939 File Offset: 0x00032B39
	public static bool IsVisible()
	{
		return StoreGui.m_instance && StoreGui.m_instance.m_hiddenFrames <= 1;
	}

	// Token: 0x0600062A RID: 1578 RVA: 0x00034959 File Offset: 0x00032B59
	public void OnBuyItem()
	{
		this.BuySelectedItem();
	}

	// Token: 0x0600062B RID: 1579 RVA: 0x00034964 File Offset: 0x00032B64
	private void BuySelectedItem()
	{
		if (this.m_selectedItem == null || !this.CanAfford(this.m_selectedItem))
		{
			return;
		}
		int stack = Mathf.Min(this.m_selectedItem.m_stack, this.m_selectedItem.m_prefab.m_itemData.m_shared.m_maxStackSize);
		int quality = this.m_selectedItem.m_prefab.m_itemData.m_quality;
		int variant = this.m_selectedItem.m_prefab.m_itemData.m_variant;
		if (Player.m_localPlayer.GetInventory().AddItem(this.m_selectedItem.m_prefab.name, stack, quality, variant, 0L, "") != null)
		{
			Player.m_localPlayer.GetInventory().RemoveItem(this.m_coinPrefab.m_itemData.m_shared.m_name, this.m_selectedItem.m_price);
			this.m_trader.OnBought(this.m_selectedItem);
			this.m_buyEffects.Create(base.transform.position, Quaternion.identity, null, 1f);
			Player.m_localPlayer.ShowPickupMessage(this.m_selectedItem.m_prefab.m_itemData, this.m_selectedItem.m_prefab.m_itemData.m_stack);
			this.FillList();
			Gogan.LogEvent("Game", "BoughtItem", this.m_selectedItem.m_prefab.name, 0L);
		}
	}

	// Token: 0x0600062C RID: 1580 RVA: 0x00034AC5 File Offset: 0x00032CC5
	public void OnSellItem()
	{
		this.SellItem();
	}

	// Token: 0x0600062D RID: 1581 RVA: 0x00034AD0 File Offset: 0x00032CD0
	private void SellItem()
	{
		ItemDrop.ItemData sellableItem = this.GetSellableItem();
		if (sellableItem == null)
		{
			return;
		}
		int stack = sellableItem.m_shared.m_value * sellableItem.m_stack;
		Player.m_localPlayer.GetInventory().RemoveItem(sellableItem);
		Player.m_localPlayer.GetInventory().AddItem(this.m_coinPrefab.gameObject.name, stack, this.m_coinPrefab.m_itemData.m_quality, this.m_coinPrefab.m_itemData.m_variant, 0L, "");
		string text;
		if (sellableItem.m_stack > 1)
		{
			text = sellableItem.m_stack + "x" + sellableItem.m_shared.m_name;
		}
		else
		{
			text = sellableItem.m_shared.m_name;
		}
		this.m_sellEffects.Create(base.transform.position, Quaternion.identity, null, 1f);
		Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize("$msg_sold", new string[]
		{
			text,
			stack.ToString()
		}), 0, sellableItem.m_shared.m_icons[0]);
		this.m_trader.OnSold();
		this.FillList();
		Gogan.LogEvent("Game", "SoldItem", text, 0L);
	}

	// Token: 0x0600062E RID: 1582 RVA: 0x00034C13 File Offset: 0x00032E13
	private int GetPlayerCoins()
	{
		return Player.m_localPlayer.GetInventory().CountItems(this.m_coinPrefab.m_itemData.m_shared.m_name);
	}

	// Token: 0x0600062F RID: 1583 RVA: 0x00034C3C File Offset: 0x00032E3C
	private bool CanAfford(Trader.TradeItem item)
	{
		int playerCoins = this.GetPlayerCoins();
		return item.m_price <= playerCoins;
	}

	// Token: 0x06000630 RID: 1584 RVA: 0x00034C5C File Offset: 0x00032E5C
	private void FillList()
	{
		int playerCoins = this.GetPlayerCoins();
		int num = this.GetSelectedItemIndex();
		List<Trader.TradeItem> items = this.m_trader.m_items;
		foreach (GameObject obj in this.m_itemList)
		{
			UnityEngine.Object.Destroy(obj);
		}
		this.m_itemList.Clear();
		float num2 = (float)items.Count * this.m_itemSpacing;
		num2 = Mathf.Max(this.m_itemlistBaseSize, num2);
		this.m_listRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num2);
		for (int i = 0; i < items.Count; i++)
		{
			Trader.TradeItem tradeItem = items[i];
			GameObject element = UnityEngine.Object.Instantiate<GameObject>(this.m_listElement, this.m_listRoot);
			element.SetActive(true);
			(element.transform as RectTransform).anchoredPosition = new Vector2(0f, (float)i * -this.m_itemSpacing);
			bool flag = tradeItem.m_price <= playerCoins;
			Image component = element.transform.Find("icon").GetComponent<Image>();
			component.sprite = tradeItem.m_prefab.m_itemData.m_shared.m_icons[0];
			component.color = (flag ? Color.white : new Color(1f, 0f, 1f, 0f));
			string text = Localization.instance.Localize(tradeItem.m_prefab.m_itemData.m_shared.m_name);
			if (tradeItem.m_stack > 1)
			{
				text = text + " x" + tradeItem.m_stack;
			}
			Text component2 = element.transform.Find("name").GetComponent<Text>();
			component2.text = text;
			component2.color = (flag ? Color.white : Color.grey);
			UITooltip component3 = element.GetComponent<UITooltip>();
			component3.m_topic = tradeItem.m_prefab.m_itemData.m_shared.m_name;
			component3.m_text = tradeItem.m_prefab.m_itemData.GetTooltip();
			Text component4 = Utils.FindChild(element.transform, "price").GetComponent<Text>();
			component4.text = tradeItem.m_price.ToString();
			if (!flag)
			{
				component4.color = Color.grey;
			}
			element.GetComponent<Button>().onClick.AddListener(delegate
			{
				this.OnSelectedItem(element);
			});
			this.m_itemList.Add(element);
		}
		if (num < 0)
		{
			num = 0;
		}
		this.SelectItem(num, false);
	}

	// Token: 0x06000631 RID: 1585 RVA: 0x00034F30 File Offset: 0x00033130
	private void OnSelectedItem(GameObject button)
	{
		int index = this.FindSelectedRecipe(button);
		this.SelectItem(index, false);
	}

	// Token: 0x06000632 RID: 1586 RVA: 0x00034F50 File Offset: 0x00033150
	private int FindSelectedRecipe(GameObject button)
	{
		for (int i = 0; i < this.m_itemList.Count; i++)
		{
			if (this.m_itemList[i] == button)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x06000633 RID: 1587 RVA: 0x00034F8C File Offset: 0x0003318C
	private void SelectItem(int index, bool center)
	{
		ZLog.Log("Setting selected recipe " + index);
		for (int i = 0; i < this.m_itemList.Count; i++)
		{
			bool active = i == index;
			this.m_itemList[i].transform.Find("selected").gameObject.SetActive(active);
		}
		if (center && index >= 0)
		{
			this.m_itemEnsureVisible.CenterOnItem(this.m_itemList[index].transform as RectTransform);
		}
		if (index < 0)
		{
			this.m_selectedItem = null;
			return;
		}
		this.m_selectedItem = this.m_trader.m_items[index];
	}

	// Token: 0x06000634 RID: 1588 RVA: 0x0003503A File Offset: 0x0003323A
	private void UpdateSellButton()
	{
		this.m_sellButton.interactable = (this.GetSellableItem() != null);
	}

	// Token: 0x06000635 RID: 1589 RVA: 0x00035050 File Offset: 0x00033250
	private ItemDrop.ItemData GetSellableItem()
	{
		this.m_tempItems.Clear();
		Player.m_localPlayer.GetInventory().GetValuableItems(this.m_tempItems);
		foreach (ItemDrop.ItemData itemData in this.m_tempItems)
		{
			if (itemData.m_shared.m_name != this.m_coinPrefab.m_itemData.m_shared.m_name)
			{
				return itemData;
			}
		}
		return null;
	}

	// Token: 0x06000636 RID: 1590 RVA: 0x000350EC File Offset: 0x000332EC
	private int GetSelectedItemIndex()
	{
		int result = 0;
		for (int i = 0; i < this.m_trader.m_items.Count; i++)
		{
			if (this.m_trader.m_items[i] == this.m_selectedItem)
			{
				result = i;
			}
		}
		return result;
	}

	// Token: 0x06000637 RID: 1591 RVA: 0x00035134 File Offset: 0x00033334
	private void UpdateBuyButton()
	{
		UITooltip component = this.m_buyButton.GetComponent<UITooltip>();
		if (this.m_selectedItem == null)
		{
			this.m_buyButton.interactable = false;
			component.m_text = "";
			return;
		}
		bool flag = this.CanAfford(this.m_selectedItem);
		bool flag2 = Player.m_localPlayer.GetInventory().HaveEmptySlot();
		this.m_buyButton.interactable = (flag && flag2);
		if (!flag)
		{
			component.m_text = Localization.instance.Localize("$msg_missingrequirement");
			return;
		}
		if (!flag2)
		{
			component.m_text = Localization.instance.Localize("$inventory_full");
			return;
		}
		component.m_text = "";
	}

	// Token: 0x06000638 RID: 1592 RVA: 0x000351D8 File Offset: 0x000333D8
	private void UpdateRecipeGamepadInput()
	{
		if (this.m_itemList.Count > 0)
		{
			if (ZInput.GetButtonDown("JoyLStickDown"))
			{
				this.SelectItem(Mathf.Min(this.m_itemList.Count - 1, this.GetSelectedItemIndex() + 1), true);
			}
			if (ZInput.GetButtonDown("JoyLStickUp"))
			{
				this.SelectItem(Mathf.Max(0, this.GetSelectedItemIndex() - 1), true);
			}
		}
	}

	// Token: 0x040006E4 RID: 1764
	private static StoreGui m_instance;

	// Token: 0x040006E5 RID: 1765
	public GameObject m_rootPanel;

	// Token: 0x040006E6 RID: 1766
	public Button m_buyButton;

	// Token: 0x040006E7 RID: 1767
	public Button m_sellButton;

	// Token: 0x040006E8 RID: 1768
	public RectTransform m_listRoot;

	// Token: 0x040006E9 RID: 1769
	public GameObject m_listElement;

	// Token: 0x040006EA RID: 1770
	public Scrollbar m_listScroll;

	// Token: 0x040006EB RID: 1771
	public ScrollRectEnsureVisible m_itemEnsureVisible;

	// Token: 0x040006EC RID: 1772
	public Text m_coinText;

	// Token: 0x040006ED RID: 1773
	public EffectList m_buyEffects = new EffectList();

	// Token: 0x040006EE RID: 1774
	public EffectList m_sellEffects = new EffectList();

	// Token: 0x040006EF RID: 1775
	public float m_hideDistance = 5f;

	// Token: 0x040006F0 RID: 1776
	public float m_itemSpacing = 64f;

	// Token: 0x040006F1 RID: 1777
	public ItemDrop m_coinPrefab;

	// Token: 0x040006F2 RID: 1778
	private List<GameObject> m_itemList = new List<GameObject>();

	// Token: 0x040006F3 RID: 1779
	private Trader.TradeItem m_selectedItem;

	// Token: 0x040006F4 RID: 1780
	private Trader m_trader;

	// Token: 0x040006F5 RID: 1781
	private float m_itemlistBaseSize;

	// Token: 0x040006F6 RID: 1782
	private int m_hiddenFrames;

	// Token: 0x040006F7 RID: 1783
	private List<ItemDrop.ItemData> m_tempItems = new List<ItemDrop.ItemData>();
}
