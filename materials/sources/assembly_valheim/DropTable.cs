using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000069 RID: 105
[Serializable]
public class DropTable
{
	// Token: 0x06000687 RID: 1671 RVA: 0x00036826 File Offset: 0x00034A26
	public DropTable Clone()
	{
		return base.MemberwiseClone() as DropTable;
	}

	// Token: 0x06000688 RID: 1672 RVA: 0x00036834 File Offset: 0x00034A34
	public List<ItemDrop.ItemData> GetDropListItems()
	{
		List<ItemDrop.ItemData> list = new List<ItemDrop.ItemData>();
		if (this.m_drops.Count == 0)
		{
			return list;
		}
		if (UnityEngine.Random.value > this.m_dropChance)
		{
			return list;
		}
		List<DropTable.DropData> list2 = new List<DropTable.DropData>(this.m_drops);
		float num = 0f;
		foreach (DropTable.DropData dropData in list2)
		{
			num += dropData.m_weight;
		}
		int num2 = UnityEngine.Random.Range(this.m_dropMin, this.m_dropMax + 1);
		for (int i = 0; i < num2; i++)
		{
			float num3 = UnityEngine.Random.Range(0f, num);
			bool flag = false;
			float num4 = 0f;
			foreach (DropTable.DropData dropData2 in list2)
			{
				num4 += dropData2.m_weight;
				if (num3 <= num4)
				{
					flag = true;
					this.AddItemToList(list, dropData2);
					if (this.m_oneOfEach)
					{
						list2.Remove(dropData2);
						num -= dropData2.m_weight;
						break;
					}
					break;
				}
			}
			if (!flag && list2.Count > 0)
			{
				this.AddItemToList(list, list2[0]);
			}
		}
		return list;
	}

	// Token: 0x06000689 RID: 1673 RVA: 0x00036990 File Offset: 0x00034B90
	private void AddItemToList(List<ItemDrop.ItemData> toDrop, DropTable.DropData data)
	{
		ItemDrop.ItemData itemData = data.m_item.GetComponent<ItemDrop>().m_itemData;
		ItemDrop.ItemData itemData2 = itemData.Clone();
		itemData2.m_dropPrefab = data.m_item;
		int min = Mathf.Max(1, data.m_stackMin);
		int num = Mathf.Min(itemData.m_shared.m_maxStackSize, data.m_stackMax);
		itemData2.m_stack = UnityEngine.Random.Range(min, num + 1);
		toDrop.Add(itemData2);
	}

	// Token: 0x0600068A RID: 1674 RVA: 0x000369FC File Offset: 0x00034BFC
	public List<GameObject> GetDropList()
	{
		int amount = UnityEngine.Random.Range(this.m_dropMin, this.m_dropMax + 1);
		return this.GetDropList(amount);
	}

	// Token: 0x0600068B RID: 1675 RVA: 0x00036A24 File Offset: 0x00034C24
	private List<GameObject> GetDropList(int amount)
	{
		List<GameObject> list = new List<GameObject>();
		if (this.m_drops.Count == 0)
		{
			return list;
		}
		if (UnityEngine.Random.value > this.m_dropChance)
		{
			return list;
		}
		List<DropTable.DropData> list2 = new List<DropTable.DropData>(this.m_drops);
		float num = 0f;
		foreach (DropTable.DropData dropData in list2)
		{
			num += dropData.m_weight;
		}
		for (int i = 0; i < amount; i++)
		{
			float num2 = UnityEngine.Random.Range(0f, num);
			bool flag = false;
			float num3 = 0f;
			foreach (DropTable.DropData dropData2 in list2)
			{
				num3 += dropData2.m_weight;
				if (num2 <= num3)
				{
					flag = true;
					int num4 = UnityEngine.Random.Range(dropData2.m_stackMin, dropData2.m_stackMax);
					for (int j = 0; j < num4; j++)
					{
						list.Add(dropData2.m_item);
					}
					if (this.m_oneOfEach)
					{
						list2.Remove(dropData2);
						num -= dropData2.m_weight;
						break;
					}
					break;
				}
			}
			if (!flag && list2.Count > 0)
			{
				list.Add(list2[0].m_item);
			}
		}
		return list;
	}

	// Token: 0x0600068C RID: 1676 RVA: 0x00036B98 File Offset: 0x00034D98
	public bool IsEmpty()
	{
		return this.m_drops.Count == 0;
	}

	// Token: 0x0400074A RID: 1866
	public List<DropTable.DropData> m_drops = new List<DropTable.DropData>();

	// Token: 0x0400074B RID: 1867
	public int m_dropMin = 1;

	// Token: 0x0400074C RID: 1868
	public int m_dropMax = 1;

	// Token: 0x0400074D RID: 1869
	[Range(0f, 1f)]
	public float m_dropChance = 1f;

	// Token: 0x0400074E RID: 1870
	public bool m_oneOfEach;

	// Token: 0x02000163 RID: 355
	[Serializable]
	public struct DropData
	{
		// Token: 0x0400114A RID: 4426
		public GameObject m_item;

		// Token: 0x0400114B RID: 4427
		public int m_stackMin;

		// Token: 0x0400114C RID: 4428
		public int m_stackMax;

		// Token: 0x0400114D RID: 4429
		public float m_weight;
	}
}
