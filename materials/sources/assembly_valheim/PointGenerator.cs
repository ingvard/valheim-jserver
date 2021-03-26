using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000E8 RID: 232
public class PointGenerator
{
	// Token: 0x06000E5E RID: 3678 RVA: 0x00066A9C File Offset: 0x00064C9C
	public PointGenerator(int amount, float gridSize)
	{
		this.m_amount = amount;
		this.m_gridSize = gridSize;
	}

	// Token: 0x06000E5F RID: 3679 RVA: 0x00066AE8 File Offset: 0x00064CE8
	public void Update(Vector3 center, float radius, List<Vector3> newPoints, List<Vector3> removedPoints)
	{
		Vector2Int grid = this.GetGrid(center);
		if (this.m_currentCenterGrid == grid)
		{
			newPoints.Clear();
			removedPoints.Clear();
			return;
		}
		int num = Mathf.CeilToInt(radius / this.m_gridSize);
		if (this.m_currentCenterGrid != grid || this.m_currentGridWith != num)
		{
			this.RegeneratePoints(grid, num);
		}
	}

	// Token: 0x06000E60 RID: 3680 RVA: 0x00066B48 File Offset: 0x00064D48
	private void RegeneratePoints(Vector2Int centerGrid, int gridWith)
	{
		this.m_currentCenterGrid = centerGrid;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		this.m_points.Clear();
		for (int i = centerGrid.y - gridWith; i <= centerGrid.y + gridWith; i++)
		{
			for (int j = centerGrid.x - gridWith; j <= centerGrid.x + gridWith; j++)
			{
				UnityEngine.Random.InitState(j + i * 100);
				Vector3 gridPos = this.GetGridPos(new Vector2Int(j, i));
				for (int k = 0; k < this.m_amount; k++)
				{
					Vector3 item = new Vector3(UnityEngine.Random.Range(gridPos.x - this.m_gridSize, gridPos.x + this.m_gridSize), UnityEngine.Random.Range(gridPos.z - this.m_gridSize, gridPos.z + this.m_gridSize));
					this.m_points.Add(item);
				}
			}
		}
		UnityEngine.Random.state = state;
	}

	// Token: 0x06000E61 RID: 3681 RVA: 0x00066C38 File Offset: 0x00064E38
	public Vector2Int GetGrid(Vector3 point)
	{
		int x = Mathf.FloorToInt((point.x + this.m_gridSize / 2f) / this.m_gridSize);
		int y = Mathf.FloorToInt((point.z + this.m_gridSize / 2f) / this.m_gridSize);
		return new Vector2Int(x, y);
	}

	// Token: 0x06000E62 RID: 3682 RVA: 0x00066C8A File Offset: 0x00064E8A
	public Vector3 GetGridPos(Vector2Int grid)
	{
		return new Vector3((float)grid.x * this.m_gridSize, 0f, (float)grid.y * this.m_gridSize);
	}

	// Token: 0x04000D3F RID: 3391
	private int m_amount;

	// Token: 0x04000D40 RID: 3392
	private float m_gridSize = 8f;

	// Token: 0x04000D41 RID: 3393
	private Vector2Int m_currentCenterGrid = new Vector2Int(99999, 99999);

	// Token: 0x04000D42 RID: 3394
	private int m_currentGridWith;

	// Token: 0x04000D43 RID: 3395
	private List<Vector3> m_points = new List<Vector3>();
}
