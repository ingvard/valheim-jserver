using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000A5 RID: 165
public class ObjectDB : MonoBehaviour
{
	// Token: 0x17000029 RID: 41
	// (get) Token: 0x06000B2A RID: 2858 RVA: 0x00050741 File Offset: 0x0004E941
	public static ObjectDB instance
	{
		get
		{
			return ObjectDB.m_instance;
		}
	}

	// Token: 0x06000B2B RID: 2859 RVA: 0x00050748 File Offset: 0x0004E948
	private void Awake()
	{
		ObjectDB.m_instance = this;
		this.UpdateItemHashes();
	}

	// Token: 0x06000B2C RID: 2860 RVA: 0x00050756 File Offset: 0x0004E956
	public void CopyOtherDB(ObjectDB other)
	{
		this.m_items = other.m_items;
		this.m_recipes = other.m_recipes;
		this.m_StatusEffects = other.m_StatusEffects;
		this.UpdateItemHashes();
	}

	// Token: 0x06000B2D RID: 2861 RVA: 0x00050784 File Offset: 0x0004E984
	private void UpdateItemHashes()
	{
		this.m_itemByHash.Clear();
		foreach (GameObject gameObject in this.m_items)
		{
			this.m_itemByHash.Add(gameObject.name.GetStableHashCode(), gameObject);
		}
	}

	// Token: 0x06000B2E RID: 2862 RVA: 0x000507F4 File Offset: 0x0004E9F4
	public StatusEffect GetStatusEffect(string name)
	{
		foreach (StatusEffect statusEffect in this.m_StatusEffects)
		{
			if (statusEffect.name == name)
			{
				return statusEffect;
			}
		}
		return null;
	}

	// Token: 0x06000B2F RID: 2863 RVA: 0x00050858 File Offset: 0x0004EA58
	public GameObject GetItemPrefab(string name)
	{
		foreach (GameObject gameObject in this.m_items)
		{
			if (gameObject.name == name)
			{
				return gameObject;
			}
		}
		return null;
	}

	// Token: 0x06000B30 RID: 2864 RVA: 0x000508BC File Offset: 0x0004EABC
	public GameObject GetItemPrefab(int hash)
	{
		GameObject result;
		if (this.m_itemByHash.TryGetValue(hash, out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x06000B31 RID: 2865 RVA: 0x00041553 File Offset: 0x0003F753
	public int GetPrefabHash(GameObject prefab)
	{
		return prefab.name.GetStableHashCode();
	}

	// Token: 0x06000B32 RID: 2866 RVA: 0x000508DC File Offset: 0x0004EADC
	public List<ItemDrop> GetAllItems(ItemDrop.ItemData.ItemType type, string startWith)
	{
		List<ItemDrop> list = new List<ItemDrop>();
		foreach (GameObject gameObject in this.m_items)
		{
			ItemDrop component = gameObject.GetComponent<ItemDrop>();
			if (component.m_itemData.m_shared.m_itemType == type && component.gameObject.name.StartsWith(startWith))
			{
				list.Add(component);
			}
		}
		return list;
	}

	// Token: 0x06000B33 RID: 2867 RVA: 0x00050964 File Offset: 0x0004EB64
	public Recipe GetRecipe(ItemDrop.ItemData item)
	{
		foreach (Recipe recipe in this.m_recipes)
		{
			if (!(recipe.m_item == null) && recipe.m_item.m_itemData.m_shared.m_name == item.m_shared.m_name)
			{
				return recipe;
			}
		}
		return null;
	}

	// Token: 0x04000A96 RID: 2710
	private static ObjectDB m_instance;

	// Token: 0x04000A97 RID: 2711
	public List<StatusEffect> m_StatusEffects = new List<StatusEffect>();

	// Token: 0x04000A98 RID: 2712
	public List<GameObject> m_items = new List<GameObject>();

	// Token: 0x04000A99 RID: 2713
	public List<Recipe> m_recipes = new List<Recipe>();

	// Token: 0x04000A9A RID: 2714
	private Dictionary<int, GameObject> m_itemByHash = new Dictionary<int, GameObject>();
}
