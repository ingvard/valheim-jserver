using System;
using UnityEngine;

// Token: 0x02000070 RID: 112
public class Recipe : ScriptableObject
{
	// Token: 0x06000710 RID: 1808 RVA: 0x00039CE1 File Offset: 0x00037EE1
	public int GetRequiredStationLevel(int quality)
	{
		return Mathf.Max(1, this.m_minStationLevel) + (quality - 1);
	}

	// Token: 0x06000711 RID: 1809 RVA: 0x00039CF3 File Offset: 0x00037EF3
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

	// Token: 0x04000788 RID: 1928
	public ItemDrop m_item;

	// Token: 0x04000789 RID: 1929
	public int m_amount = 1;

	// Token: 0x0400078A RID: 1930
	public bool m_enabled = true;

	// Token: 0x0400078B RID: 1931
	[Header("Requirements")]
	public CraftingStation m_craftingStation;

	// Token: 0x0400078C RID: 1932
	public CraftingStation m_repairStation;

	// Token: 0x0400078D RID: 1933
	public int m_minStationLevel = 1;

	// Token: 0x0400078E RID: 1934
	public Piece.Requirement[] m_resources = new Piece.Requirement[0];
}
