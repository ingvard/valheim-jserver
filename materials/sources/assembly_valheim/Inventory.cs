using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200006A RID: 106
public class Inventory
{
	// Token: 0x0600068E RID: 1678 RVA: 0x00036BD4 File Offset: 0x00034DD4
	public Inventory(string name, Sprite bkg, int w, int h)
	{
		this.m_bkg = bkg;
		this.m_name = name;
		this.m_width = w;
		this.m_height = h;
	}

	// Token: 0x0600068F RID: 1679 RVA: 0x00036C30 File Offset: 0x00034E30
	private bool AddItem(ItemDrop.ItemData item, int amount, int x, int y)
	{
		amount = Mathf.Min(amount, item.m_stack);
		if (x < 0 || y < 0 || x >= this.m_width || y >= this.m_height)
		{
			return false;
		}
		ItemDrop.ItemData itemAt = this.GetItemAt(x, y);
		bool result;
		if (itemAt != null)
		{
			if (itemAt.m_shared.m_name != item.m_shared.m_name || (itemAt.m_shared.m_maxQuality > 1 && itemAt.m_quality != item.m_quality))
			{
				return false;
			}
			int num = itemAt.m_shared.m_maxStackSize - itemAt.m_stack;
			if (num <= 0)
			{
				return false;
			}
			int num2 = Mathf.Min(num, amount);
			itemAt.m_stack += num2;
			item.m_stack -= num2;
			result = (num2 == amount);
			ZLog.Log(string.Concat(new object[]
			{
				"Added to stack",
				itemAt.m_stack,
				" ",
				item.m_stack
			}));
		}
		else
		{
			ItemDrop.ItemData itemData = item.Clone();
			itemData.m_stack = amount;
			itemData.m_gridPos = new Vector2i(x, y);
			this.m_inventory.Add(itemData);
			item.m_stack -= amount;
			result = true;
		}
		this.Changed();
		return result;
	}

	// Token: 0x06000690 RID: 1680 RVA: 0x00036D7C File Offset: 0x00034F7C
	public bool CanAddItem(GameObject prefab, int stack = -1)
	{
		ItemDrop component = prefab.GetComponent<ItemDrop>();
		return !(component == null) && this.CanAddItem(component.m_itemData, stack);
	}

	// Token: 0x06000691 RID: 1681 RVA: 0x00036DA8 File Offset: 0x00034FA8
	public bool CanAddItem(ItemDrop.ItemData item, int stack = -1)
	{
		if (this.HaveEmptySlot())
		{
			return true;
		}
		if (stack <= 0)
		{
			stack = item.m_stack;
		}
		return this.FindFreeStackSpace(item.m_shared.m_name) >= stack;
	}

	// Token: 0x06000692 RID: 1682 RVA: 0x00036DD8 File Offset: 0x00034FD8
	public bool AddItem(ItemDrop.ItemData item)
	{
		bool result = true;
		if (item.m_shared.m_maxStackSize > 1)
		{
			int i = 0;
			while (i < item.m_stack)
			{
				ItemDrop.ItemData itemData = this.FindFreeStackItem(item.m_shared.m_name, item.m_quality);
				if (itemData != null)
				{
					itemData.m_stack++;
					i++;
				}
				else
				{
					int stack = item.m_stack - i;
					item.m_stack = stack;
					Vector2i vector2i = this.FindEmptySlot(this.TopFirst(item));
					if (vector2i.x >= 0)
					{
						item.m_gridPos = vector2i;
						this.m_inventory.Add(item);
						break;
					}
					result = false;
					break;
				}
			}
		}
		else
		{
			Vector2i vector2i2 = this.FindEmptySlot(this.TopFirst(item));
			if (vector2i2.x >= 0)
			{
				item.m_gridPos = vector2i2;
				this.m_inventory.Add(item);
			}
			else
			{
				result = false;
			}
		}
		this.Changed();
		return result;
	}

	// Token: 0x06000693 RID: 1683 RVA: 0x00036EB1 File Offset: 0x000350B1
	private bool TopFirst(ItemDrop.ItemData item)
	{
		return item.IsWeapon() || (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Tool || item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield || item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Utility);
	}

	// Token: 0x06000694 RID: 1684 RVA: 0x00036EEC File Offset: 0x000350EC
	public void MoveAll(Inventory fromInventory)
	{
		List<ItemDrop.ItemData> list = new List<ItemDrop.ItemData>(fromInventory.GetAllItems());
		List<ItemDrop.ItemData> list2 = new List<ItemDrop.ItemData>();
		foreach (ItemDrop.ItemData itemData in list)
		{
			if (this.AddItem(itemData, itemData.m_stack, itemData.m_gridPos.x, itemData.m_gridPos.y))
			{
				fromInventory.RemoveItem(itemData);
			}
			else
			{
				list2.Add(itemData);
			}
		}
		foreach (ItemDrop.ItemData item in list2)
		{
			if (!this.AddItem(item))
			{
				break;
			}
			fromInventory.RemoveItem(item);
		}
		this.Changed();
		fromInventory.Changed();
	}

	// Token: 0x06000695 RID: 1685 RVA: 0x00036FCC File Offset: 0x000351CC
	public void MoveItemToThis(Inventory fromInventory, ItemDrop.ItemData item)
	{
		if (this.AddItem(item))
		{
			fromInventory.RemoveItem(item);
		}
		this.Changed();
		fromInventory.Changed();
	}

	// Token: 0x06000696 RID: 1686 RVA: 0x00036FEB File Offset: 0x000351EB
	public bool MoveItemToThis(Inventory fromInventory, ItemDrop.ItemData item, int amount, int x, int y)
	{
		bool result = this.AddItem(item, amount, x, y);
		if (item.m_stack == 0)
		{
			fromInventory.RemoveItem(item);
			return result;
		}
		fromInventory.Changed();
		return result;
	}

	// Token: 0x06000697 RID: 1687 RVA: 0x00037010 File Offset: 0x00035210
	public bool RemoveItem(int index)
	{
		if (index < 0 || index >= this.m_inventory.Count)
		{
			return false;
		}
		this.m_inventory.RemoveAt(index);
		this.Changed();
		return true;
	}

	// Token: 0x06000698 RID: 1688 RVA: 0x00037039 File Offset: 0x00035239
	public bool ContainsItem(ItemDrop.ItemData item)
	{
		return this.m_inventory.Contains(item);
	}

	// Token: 0x06000699 RID: 1689 RVA: 0x00037048 File Offset: 0x00035248
	public bool RemoveOneItem(ItemDrop.ItemData item)
	{
		if (!this.m_inventory.Contains(item))
		{
			return false;
		}
		if (item.m_stack > 1)
		{
			item.m_stack--;
			this.Changed();
		}
		else
		{
			this.m_inventory.Remove(item);
			this.Changed();
		}
		return true;
	}

	// Token: 0x0600069A RID: 1690 RVA: 0x00037098 File Offset: 0x00035298
	public bool RemoveItem(ItemDrop.ItemData item)
	{
		if (!this.m_inventory.Contains(item))
		{
			ZLog.Log("Item is not in this container");
			return false;
		}
		this.m_inventory.Remove(item);
		this.Changed();
		return true;
	}

	// Token: 0x0600069B RID: 1691 RVA: 0x000370C8 File Offset: 0x000352C8
	public bool RemoveItem(ItemDrop.ItemData item, int amount)
	{
		amount = Mathf.Min(item.m_stack, amount);
		if (amount == item.m_stack)
		{
			return this.RemoveItem(item);
		}
		if (!this.m_inventory.Contains(item))
		{
			return false;
		}
		item.m_stack -= amount;
		this.Changed();
		return true;
	}

	// Token: 0x0600069C RID: 1692 RVA: 0x0003711C File Offset: 0x0003531C
	public void RemoveItem(string name, int amount)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_shared.m_name == name)
			{
				int num = Mathf.Min(itemData.m_stack, amount);
				itemData.m_stack -= num;
				amount -= num;
				if (amount <= 0)
				{
					break;
				}
			}
		}
		this.m_inventory.RemoveAll((ItemDrop.ItemData x) => x.m_stack <= 0);
		this.Changed();
	}

	// Token: 0x0600069D RID: 1693 RVA: 0x000371D4 File Offset: 0x000353D4
	public bool HaveItem(string name)
	{
		using (List<ItemDrop.ItemData>.Enumerator enumerator = this.m_inventory.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_shared.m_name == name)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x0600069E RID: 1694 RVA: 0x00037238 File Offset: 0x00035438
	public void GetAllPieceTables(List<PieceTable> tables)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_shared.m_buildPieces != null && !tables.Contains(itemData.m_shared.m_buildPieces))
			{
				tables.Add(itemData.m_shared.m_buildPieces);
			}
		}
	}

	// Token: 0x0600069F RID: 1695 RVA: 0x000372BC File Offset: 0x000354BC
	public int CountItems(string name)
	{
		int num = 0;
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_shared.m_name == name)
			{
				num += itemData.m_stack;
			}
		}
		return num;
	}

	// Token: 0x060006A0 RID: 1696 RVA: 0x00037328 File Offset: 0x00035528
	public ItemDrop.ItemData GetItem(int index)
	{
		return this.m_inventory[index];
	}

	// Token: 0x060006A1 RID: 1697 RVA: 0x00037338 File Offset: 0x00035538
	public ItemDrop.ItemData GetItem(string name)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_shared.m_name == name)
			{
				return itemData;
			}
		}
		return null;
	}

	// Token: 0x060006A2 RID: 1698 RVA: 0x000373A0 File Offset: 0x000355A0
	public ItemDrop.ItemData GetAmmoItem(string ammoName)
	{
		int num = 0;
		ItemDrop.ItemData itemData = null;
		foreach (ItemDrop.ItemData itemData2 in this.m_inventory)
		{
			if ((itemData2.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Ammo || itemData2.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable) && itemData2.m_shared.m_ammoType == ammoName)
			{
				int num2 = itemData2.m_gridPos.y * this.m_width + itemData2.m_gridPos.x;
				if (num2 < num || itemData == null)
				{
					num = num2;
					itemData = itemData2;
				}
			}
		}
		return itemData;
	}

	// Token: 0x060006A3 RID: 1699 RVA: 0x00037450 File Offset: 0x00035650
	private int FindFreeStackSpace(string name)
	{
		int num = 0;
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_shared.m_name == name && itemData.m_stack < itemData.m_shared.m_maxStackSize)
			{
				num += itemData.m_shared.m_maxStackSize - itemData.m_stack;
			}
		}
		return num;
	}

	// Token: 0x060006A4 RID: 1700 RVA: 0x000374DC File Offset: 0x000356DC
	private ItemDrop.ItemData FindFreeStackItem(string name, int quality)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_shared.m_name == name && itemData.m_quality == quality && itemData.m_stack < itemData.m_shared.m_maxStackSize)
			{
				return itemData;
			}
		}
		return null;
	}

	// Token: 0x060006A5 RID: 1701 RVA: 0x00037560 File Offset: 0x00035760
	public int NrOfItems()
	{
		return this.m_inventory.Count;
	}

	// Token: 0x060006A6 RID: 1702 RVA: 0x0003756D File Offset: 0x0003576D
	public float SlotsUsedPercentage()
	{
		return (float)this.m_inventory.Count / (float)(this.m_width * this.m_height) * 100f;
	}

	// Token: 0x060006A7 RID: 1703 RVA: 0x00037590 File Offset: 0x00035790
	public void Print()
	{
		for (int i = 0; i < this.m_inventory.Count; i++)
		{
			ItemDrop.ItemData itemData = this.m_inventory[i];
			ZLog.Log(string.Concat(new object[]
			{
				i.ToString(),
				": ",
				itemData.m_shared.m_name,
				"  ",
				itemData.m_stack,
				" / ",
				itemData.m_shared.m_maxStackSize
			}));
		}
	}

	// Token: 0x060006A8 RID: 1704 RVA: 0x00037621 File Offset: 0x00035821
	public int GetEmptySlots()
	{
		return this.m_height * this.m_width - this.m_inventory.Count;
	}

	// Token: 0x060006A9 RID: 1705 RVA: 0x0003763C File Offset: 0x0003583C
	public bool HaveEmptySlot()
	{
		return this.m_inventory.Count < this.m_width * this.m_height;
	}

	// Token: 0x060006AA RID: 1706 RVA: 0x00037658 File Offset: 0x00035858
	private Vector2i FindEmptySlot(bool topFirst)
	{
		if (topFirst)
		{
			for (int i = 0; i < this.m_height; i++)
			{
				for (int j = 0; j < this.m_width; j++)
				{
					if (this.GetItemAt(j, i) == null)
					{
						return new Vector2i(j, i);
					}
				}
			}
		}
		else
		{
			for (int k = this.m_height - 1; k >= 0; k--)
			{
				for (int l = 0; l < this.m_width; l++)
				{
					if (this.GetItemAt(l, k) == null)
					{
						return new Vector2i(l, k);
					}
				}
			}
		}
		return new Vector2i(-1, -1);
	}

	// Token: 0x060006AB RID: 1707 RVA: 0x000376DC File Offset: 0x000358DC
	public ItemDrop.ItemData GetOtherItemAt(int x, int y, ItemDrop.ItemData oldItem)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData != oldItem && itemData.m_gridPos.x == x && itemData.m_gridPos.y == y)
			{
				return itemData;
			}
		}
		return null;
	}

	// Token: 0x060006AC RID: 1708 RVA: 0x00037750 File Offset: 0x00035950
	public ItemDrop.ItemData GetItemAt(int x, int y)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_gridPos.x == x && itemData.m_gridPos.y == y)
			{
				return itemData;
			}
		}
		return null;
	}

	// Token: 0x060006AD RID: 1709 RVA: 0x000377C0 File Offset: 0x000359C0
	public List<ItemDrop.ItemData> GetEquipedtems()
	{
		List<ItemDrop.ItemData> list = new List<ItemDrop.ItemData>();
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_equiped)
			{
				list.Add(itemData);
			}
		}
		return list;
	}

	// Token: 0x060006AE RID: 1710 RVA: 0x00037824 File Offset: 0x00035A24
	public void GetWornItems(List<ItemDrop.ItemData> worn)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_shared.m_useDurability && itemData.m_durability < itemData.GetMaxDurability())
			{
				worn.Add(itemData);
			}
		}
	}

	// Token: 0x060006AF RID: 1711 RVA: 0x00037894 File Offset: 0x00035A94
	public void GetValuableItems(List<ItemDrop.ItemData> items)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_shared.m_value > 0)
			{
				items.Add(itemData);
			}
		}
	}

	// Token: 0x060006B0 RID: 1712 RVA: 0x000378F8 File Offset: 0x00035AF8
	public List<ItemDrop.ItemData> GetAllItems()
	{
		return this.m_inventory;
	}

	// Token: 0x060006B1 RID: 1713 RVA: 0x00037900 File Offset: 0x00035B00
	public void GetAllItems(string name, List<ItemDrop.ItemData> items)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_shared.m_name == name)
			{
				items.Add(itemData);
			}
		}
	}

	// Token: 0x060006B2 RID: 1714 RVA: 0x00037968 File Offset: 0x00035B68
	public void GetAllItems(ItemDrop.ItemData.ItemType type, List<ItemDrop.ItemData> items)
	{
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_shared.m_itemType == type)
			{
				items.Add(itemData);
			}
		}
	}

	// Token: 0x060006B3 RID: 1715 RVA: 0x000379CC File Offset: 0x00035BCC
	public int GetWidth()
	{
		return this.m_width;
	}

	// Token: 0x060006B4 RID: 1716 RVA: 0x000379D4 File Offset: 0x00035BD4
	public int GetHeight()
	{
		return this.m_height;
	}

	// Token: 0x060006B5 RID: 1717 RVA: 0x000379DC File Offset: 0x00035BDC
	public string GetName()
	{
		return this.m_name;
	}

	// Token: 0x060006B6 RID: 1718 RVA: 0x000379E4 File Offset: 0x00035BE4
	public Sprite GetBkg()
	{
		return this.m_bkg;
	}

	// Token: 0x060006B7 RID: 1719 RVA: 0x000379EC File Offset: 0x00035BEC
	public void Save(ZPackage pkg)
	{
		pkg.Write(this.currentVersion);
		pkg.Write(this.m_inventory.Count);
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_dropPrefab == null)
			{
				ZLog.Log("Item missing prefab " + itemData.m_shared.m_name);
				pkg.Write("");
			}
			else
			{
				pkg.Write(itemData.m_dropPrefab.name);
			}
			pkg.Write(itemData.m_stack);
			pkg.Write(itemData.m_durability);
			pkg.Write(itemData.m_gridPos);
			pkg.Write(itemData.m_equiped);
			pkg.Write(itemData.m_quality);
			pkg.Write(itemData.m_variant);
			pkg.Write(itemData.m_crafterID);
			pkg.Write(itemData.m_crafterName);
		}
	}

	// Token: 0x060006B8 RID: 1720 RVA: 0x00037B04 File Offset: 0x00035D04
	public void Load(ZPackage pkg)
	{
		int num = pkg.ReadInt();
		int num2 = pkg.ReadInt();
		this.m_inventory.Clear();
		for (int i = 0; i < num2; i++)
		{
			string text = pkg.ReadString();
			int stack = pkg.ReadInt();
			float durability = pkg.ReadSingle();
			Vector2i pos = pkg.ReadVector2i();
			bool equiped = pkg.ReadBool();
			int quality = 1;
			if (num >= 101)
			{
				quality = pkg.ReadInt();
			}
			int variant = 0;
			if (num >= 102)
			{
				variant = pkg.ReadInt();
			}
			long crafterID = 0L;
			string crafterName = "";
			if (num >= 103)
			{
				crafterID = pkg.ReadLong();
				crafterName = pkg.ReadString();
			}
			if (text != "")
			{
				this.AddItem(text, stack, durability, pos, equiped, quality, variant, crafterID, crafterName);
			}
		}
		this.Changed();
	}

	// Token: 0x060006B9 RID: 1721 RVA: 0x00037BD0 File Offset: 0x00035DD0
	public ItemDrop.ItemData AddItem(string name, int stack, int quality, int variant, long crafterID, string crafterName)
	{
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(name);
		if (itemPrefab == null)
		{
			ZLog.Log("Failed to find item prefab " + name);
			return null;
		}
		ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
		if (component == null)
		{
			ZLog.Log("Invalid item " + name);
			return null;
		}
		if (this.FindEmptySlot(this.TopFirst(component.m_itemData)).x == -1)
		{
			return null;
		}
		ItemDrop.ItemData result = null;
		int i = stack;
		while (i > 0)
		{
			ZNetView.m_forceDisableInit = true;
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(itemPrefab);
			ZNetView.m_forceDisableInit = false;
			ItemDrop component2 = gameObject.GetComponent<ItemDrop>();
			if (component2 == null)
			{
				ZLog.Log("Missing itemdrop in " + name);
				UnityEngine.Object.Destroy(gameObject);
				return null;
			}
			int num = Mathf.Min(i, component2.m_itemData.m_shared.m_maxStackSize);
			i -= num;
			component2.m_itemData.m_stack = num;
			component2.m_itemData.m_quality = quality;
			component2.m_itemData.m_variant = variant;
			component2.m_itemData.m_durability = component2.m_itemData.GetMaxDurability();
			component2.m_itemData.m_crafterID = crafterID;
			component2.m_itemData.m_crafterName = crafterName;
			this.AddItem(component2.m_itemData);
			result = component2.m_itemData;
			UnityEngine.Object.Destroy(gameObject);
		}
		return result;
	}

	// Token: 0x060006BA RID: 1722 RVA: 0x00037D2C File Offset: 0x00035F2C
	private bool AddItem(string name, int stack, float durability, Vector2i pos, bool equiped, int quality, int variant, long crafterID, string crafterName)
	{
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(name);
		if (itemPrefab == null)
		{
			ZLog.Log("Failed to find item prefab " + name);
			return false;
		}
		ZNetView.m_forceDisableInit = true;
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(itemPrefab);
		ZNetView.m_forceDisableInit = false;
		ItemDrop component = gameObject.GetComponent<ItemDrop>();
		if (component == null)
		{
			ZLog.Log("Missing itemdrop in " + name);
			UnityEngine.Object.Destroy(gameObject);
			return false;
		}
		component.m_itemData.m_stack = Mathf.Min(stack, component.m_itemData.m_shared.m_maxStackSize);
		component.m_itemData.m_durability = durability;
		component.m_itemData.m_equiped = equiped;
		component.m_itemData.m_quality = quality;
		component.m_itemData.m_variant = variant;
		component.m_itemData.m_crafterID = crafterID;
		component.m_itemData.m_crafterName = crafterName;
		this.AddItem(component.m_itemData, component.m_itemData.m_stack, pos.x, pos.y);
		UnityEngine.Object.Destroy(gameObject);
		return true;
	}

	// Token: 0x060006BB RID: 1723 RVA: 0x00037E38 File Offset: 0x00036038
	public void MoveInventoryToGrave(Inventory original)
	{
		this.m_inventory.Clear();
		this.m_width = original.m_width;
		this.m_height = original.m_height;
		foreach (ItemDrop.ItemData itemData in original.m_inventory)
		{
			if (!itemData.m_shared.m_questItem && !itemData.m_equiped)
			{
				this.m_inventory.Add(itemData);
			}
		}
		original.m_inventory.RemoveAll((ItemDrop.ItemData x) => !x.m_shared.m_questItem && !x.m_equiped);
		original.Changed();
		this.Changed();
	}

	// Token: 0x060006BC RID: 1724 RVA: 0x00037F00 File Offset: 0x00036100
	private void Changed()
	{
		this.UpdateTotalWeight();
		if (this.m_onChanged != null)
		{
			this.m_onChanged();
		}
	}

	// Token: 0x060006BD RID: 1725 RVA: 0x00037F1B File Offset: 0x0003611B
	public void RemoveAll()
	{
		this.m_inventory.Clear();
		this.Changed();
	}

	// Token: 0x060006BE RID: 1726 RVA: 0x00037F30 File Offset: 0x00036130
	private void UpdateTotalWeight()
	{
		this.m_totalWeight = 0f;
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			this.m_totalWeight += itemData.GetWeight();
		}
	}

	// Token: 0x060006BF RID: 1727 RVA: 0x00037F9C File Offset: 0x0003619C
	public float GetTotalWeight()
	{
		return this.m_totalWeight;
	}

	// Token: 0x060006C0 RID: 1728 RVA: 0x00037FA4 File Offset: 0x000361A4
	public void GetBoundItems(List<ItemDrop.ItemData> bound)
	{
		bound.Clear();
		foreach (ItemDrop.ItemData itemData in this.m_inventory)
		{
			if (itemData.m_gridPos.y == 0)
			{
				bound.Add(itemData);
			}
		}
	}

	// Token: 0x060006C1 RID: 1729 RVA: 0x0003800C File Offset: 0x0003620C
	public bool IsTeleportable()
	{
		using (List<ItemDrop.ItemData>.Enumerator enumerator = this.m_inventory.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (!enumerator.Current.m_shared.m_teleportable)
				{
					return false;
				}
			}
		}
		return true;
	}

	// Token: 0x0400074F RID: 1871
	private int currentVersion = 103;

	// Token: 0x04000750 RID: 1872
	public Action m_onChanged;

	// Token: 0x04000751 RID: 1873
	private string m_name = "";

	// Token: 0x04000752 RID: 1874
	private Sprite m_bkg;

	// Token: 0x04000753 RID: 1875
	private List<ItemDrop.ItemData> m_inventory = new List<ItemDrop.ItemData>();

	// Token: 0x04000754 RID: 1876
	private int m_width = 4;

	// Token: 0x04000755 RID: 1877
	private int m_height = 4;

	// Token: 0x04000756 RID: 1878
	private float m_totalWeight;
}
