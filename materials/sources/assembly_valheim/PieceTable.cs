using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200006F RID: 111
public class PieceTable : MonoBehaviour
{
	// Token: 0x060006FE RID: 1790 RVA: 0x00039884 File Offset: 0x00037A84
	public void UpdateAvailable(HashSet<string> knownRecipies, Player player, bool hideUnavailable, bool noPlacementCost)
	{
		if (this.m_availablePieces.Count == 0)
		{
			for (int i = 0; i < 4; i++)
			{
				this.m_availablePieces.Add(new List<Piece>());
			}
		}
		foreach (List<Piece> list in this.m_availablePieces)
		{
			list.Clear();
		}
		foreach (GameObject gameObject in this.m_pieces)
		{
			Piece component = gameObject.GetComponent<Piece>();
			if (noPlacementCost || (knownRecipies.Contains(component.m_name) && component.m_enabled && (!hideUnavailable || player.HaveRequirements(component, Player.RequirementMode.CanAlmostBuild))))
			{
				if (component.m_category == Piece.PieceCategory.All)
				{
					for (int j = 0; j < 4; j++)
					{
						this.m_availablePieces[j].Add(component);
					}
				}
				else
				{
					this.m_availablePieces[(int)component.m_category].Add(component);
				}
			}
		}
	}

	// Token: 0x060006FF RID: 1791 RVA: 0x000399B0 File Offset: 0x00037BB0
	public GameObject GetSelectedPrefab()
	{
		Piece selectedPiece = this.GetSelectedPiece();
		if (selectedPiece)
		{
			return selectedPiece.gameObject;
		}
		return null;
	}

	// Token: 0x06000700 RID: 1792 RVA: 0x000399D4 File Offset: 0x00037BD4
	public Piece GetPiece(int category, Vector2Int p)
	{
		if (this.m_availablePieces[category].Count == 0)
		{
			return null;
		}
		int num = p.y * 10 + p.x;
		if (num < 0 || num >= this.m_availablePieces[category].Count)
		{
			return null;
		}
		return this.m_availablePieces[category][num];
	}

	// Token: 0x06000701 RID: 1793 RVA: 0x00039A35 File Offset: 0x00037C35
	public Piece GetPiece(Vector2Int p)
	{
		return this.GetPiece((int)this.m_selectedCategory, p);
	}

	// Token: 0x06000702 RID: 1794 RVA: 0x00039A44 File Offset: 0x00037C44
	public bool IsPieceAvailable(Piece piece)
	{
		using (List<Piece>.Enumerator enumerator = this.m_availablePieces[(int)this.m_selectedCategory].GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current == piece)
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000703 RID: 1795 RVA: 0x00039AAC File Offset: 0x00037CAC
	public Piece GetSelectedPiece()
	{
		Vector2Int selectedIndex = this.GetSelectedIndex();
		return this.GetPiece((int)this.m_selectedCategory, selectedIndex);
	}

	// Token: 0x06000704 RID: 1796 RVA: 0x00039ACD File Offset: 0x00037CCD
	public int GetAvailablePiecesInCategory(Piece.PieceCategory cat)
	{
		return this.m_availablePieces[(int)cat].Count;
	}

	// Token: 0x06000705 RID: 1797 RVA: 0x00039AE0 File Offset: 0x00037CE0
	public List<Piece> GetPiecesInSelectedCategory()
	{
		return this.m_availablePieces[(int)this.m_selectedCategory];
	}

	// Token: 0x06000706 RID: 1798 RVA: 0x00039AF3 File Offset: 0x00037CF3
	public int GetAvailablePiecesInSelectedCategory()
	{
		return this.GetAvailablePiecesInCategory(this.m_selectedCategory);
	}

	// Token: 0x06000707 RID: 1799 RVA: 0x00039B01 File Offset: 0x00037D01
	public Vector2Int GetSelectedIndex()
	{
		return this.m_selectedPiece[(int)this.m_selectedCategory];
	}

	// Token: 0x06000708 RID: 1800 RVA: 0x00039B14 File Offset: 0x00037D14
	public void SetSelected(Vector2Int p)
	{
		this.m_selectedPiece[(int)this.m_selectedCategory] = p;
	}

	// Token: 0x06000709 RID: 1801 RVA: 0x00039B28 File Offset: 0x00037D28
	public void LeftPiece()
	{
		if (this.m_availablePieces[(int)this.m_selectedCategory].Count <= 1)
		{
			return;
		}
		Vector2Int vector2Int = this.m_selectedPiece[(int)this.m_selectedCategory];
		int x = vector2Int.x - 1;
		vector2Int.x = x;
		if (vector2Int.x < 0)
		{
			vector2Int.x = 9;
		}
		this.m_selectedPiece[(int)this.m_selectedCategory] = vector2Int;
	}

	// Token: 0x0600070A RID: 1802 RVA: 0x00039B98 File Offset: 0x00037D98
	public void RightPiece()
	{
		if (this.m_availablePieces[(int)this.m_selectedCategory].Count <= 1)
		{
			return;
		}
		Vector2Int vector2Int = this.m_selectedPiece[(int)this.m_selectedCategory];
		int x = vector2Int.x + 1;
		vector2Int.x = x;
		if (vector2Int.x >= 10)
		{
			vector2Int.x = 0;
		}
		this.m_selectedPiece[(int)this.m_selectedCategory] = vector2Int;
	}

	// Token: 0x0600070B RID: 1803 RVA: 0x00039C08 File Offset: 0x00037E08
	public void DownPiece()
	{
		if (this.m_availablePieces[(int)this.m_selectedCategory].Count <= 1)
		{
			return;
		}
		Vector2Int vector2Int = this.m_selectedPiece[(int)this.m_selectedCategory];
		int y = vector2Int.y + 1;
		vector2Int.y = y;
		if (vector2Int.y >= 5)
		{
			vector2Int.y = 0;
		}
		this.m_selectedPiece[(int)this.m_selectedCategory] = vector2Int;
	}

	// Token: 0x0600070C RID: 1804 RVA: 0x00039C78 File Offset: 0x00037E78
	public void UpPiece()
	{
		if (this.m_availablePieces[(int)this.m_selectedCategory].Count <= 1)
		{
			return;
		}
		Vector2Int vector2Int = this.m_selectedPiece[(int)this.m_selectedCategory];
		int y = vector2Int.y - 1;
		vector2Int.y = y;
		if (vector2Int.y < 0)
		{
			vector2Int.y = 4;
		}
		this.m_selectedPiece[(int)this.m_selectedCategory] = vector2Int;
	}

	// Token: 0x0600070D RID: 1805 RVA: 0x00039CE6 File Offset: 0x00037EE6
	public void NextCategory()
	{
		if (!this.m_useCategories)
		{
			return;
		}
		this.m_selectedCategory++;
		if (this.m_selectedCategory == Piece.PieceCategory.Max)
		{
			this.m_selectedCategory = Piece.PieceCategory.Misc;
		}
	}

	// Token: 0x0600070E RID: 1806 RVA: 0x00039D0F File Offset: 0x00037F0F
	public void PrevCategory()
	{
		if (!this.m_useCategories)
		{
			return;
		}
		this.m_selectedCategory--;
		if (this.m_selectedCategory < Piece.PieceCategory.Misc)
		{
			this.m_selectedCategory = Piece.PieceCategory.Furniture;
		}
	}

	// Token: 0x0600070F RID: 1807 RVA: 0x00039D38 File Offset: 0x00037F38
	public void SetCategory(int index)
	{
		if (!this.m_useCategories)
		{
			return;
		}
		this.m_selectedCategory = (Piece.PieceCategory)index;
		this.m_selectedCategory = (Piece.PieceCategory)Mathf.Clamp((int)this.m_selectedCategory, 0, 3);
	}

	// Token: 0x04000784 RID: 1924
	public const int m_gridWidth = 10;

	// Token: 0x04000785 RID: 1925
	public const int m_gridHeight = 5;

	// Token: 0x04000786 RID: 1926
	public List<GameObject> m_pieces = new List<GameObject>();

	// Token: 0x04000787 RID: 1927
	public bool m_useCategories = true;

	// Token: 0x04000788 RID: 1928
	public bool m_canRemovePieces = true;

	// Token: 0x04000789 RID: 1929
	[NonSerialized]
	private List<List<Piece>> m_availablePieces = new List<List<Piece>>();

	// Token: 0x0400078A RID: 1930
	[NonSerialized]
	public Piece.PieceCategory m_selectedCategory;

	// Token: 0x0400078B RID: 1931
	[NonSerialized]
	public Vector2Int[] m_selectedPiece = new Vector2Int[5];
}
