using System;
using UnityEngine;

// Token: 0x02000070 RID: 112
public class Recipe : ScriptableObject
{
	// Token: 0x06000711 RID: 1809 RVA: 0x00039D95 File Offset: 0x00037F95
	public int GetRequiredStationLevel(int quality)
	{
		return Mathf.Max(1, this.m_minStationLevel) + (quality - 1);
	}

	// Token: 0x06000712 RID: 1810 RVA: 0x00039DA7 File Offset: 0x00037FA7
	public CraftingStation GetRequiredStation(int quality)
	{
		if (this.m_craftingStation)
		{
			return this.m_craftingStation;
		}
		if (quality > 1)
		{
			return this.m_repairStation;
		}
		return null;
	}

	// Token: 0x0400078C RID: 1932
	public ItemDrop m_item;

	// Token: 0x0400078D RID: 1933
	public int m_amount = 1;

	// Token: 0x0400078E RID: 1934
	public bool m_enabled = true;

	// Token: 0x0400078F RID: 1935
	[Header("Requirements")]
	public CraftingStation m_craftingStation;

	// Token: 0x04000790 RID: 1936
	public CraftingStation m_repairStation;

	// Token: 0x04000791 RID: 1937
	public int m_minStationLevel = 1;

	// Token: 0x04000792 RID: 1938
	public Piece.Requirement[] m_resources = new Piece.Requirement[0];
}
