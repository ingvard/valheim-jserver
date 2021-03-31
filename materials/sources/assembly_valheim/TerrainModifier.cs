using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000101 RID: 257
[ExecuteInEditMode]
public class TerrainModifier : MonoBehaviour
{
	// Token: 0x06000F84 RID: 3972 RVA: 0x0006DE10 File Offset: 0x0006C010
	private void Awake()
	{
		TerrainModifier.m_instances.Add(this);
		TerrainModifier.m_needsSorting = true;
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_wasEnabled = base.enabled;
		if (base.enabled)
		{
			if (TerrainModifier.m_triggerOnPlaced)
			{
				this.OnPlaced();
			}
			this.PokeHeightmaps();
		}
	}

	// Token: 0x06000F85 RID: 3973 RVA: 0x0006DE61 File Offset: 0x0006C061
	private void OnDestroy()
	{
		TerrainModifier.m_instances.Remove(this);
		TerrainModifier.m_needsSorting = true;
		if (this.m_wasEnabled)
		{
			this.PokeHeightmaps();
		}
	}

	// Token: 0x06000F86 RID: 3974 RVA: 0x0006DE84 File Offset: 0x0006C084
	private void PokeHeightmaps()
	{
		bool delayed = !TerrainModifier.m_triggerOnPlaced;
		foreach (Heightmap heightmap in Heightmap.GetAllHeightmaps())
		{
			if (heightmap.TerrainVSModifier(this))
			{
				heightmap.Poke(delayed);
			}
		}
		if (ClutterSystem.instance)
		{
			ClutterSystem.instance.ResetGrass(base.transform.position, this.GetRadius());
		}
	}

	// Token: 0x06000F87 RID: 3975 RVA: 0x0006DF10 File Offset: 0x0006C110
	public float GetRadius()
	{
		float num = 0f;
		if (this.m_level && this.m_levelRadius > num)
		{
			num = this.m_levelRadius;
		}
		if (this.m_smooth && this.m_smoothRadius > num)
		{
			num = this.m_smoothRadius;
		}
		if (this.m_paintCleared && this.m_paintRadius > num)
		{
			num = this.m_paintRadius;
		}
		return num;
	}

	// Token: 0x06000F88 RID: 3976 RVA: 0x0006DF6C File Offset: 0x0006C16C
	public static void SetTriggerOnPlaced(bool trigger)
	{
		TerrainModifier.m_triggerOnPlaced = trigger;
	}

	// Token: 0x06000F89 RID: 3977 RVA: 0x0006DF74 File Offset: 0x0006C174
	private void OnPlaced()
	{
		this.RemoveOthers(base.transform.position, this.GetRadius() / 4f);
		this.m_onPlacedEffect.Create(base.transform.position, Quaternion.identity, null, 1f);
		if (this.m_spawnOnPlaced)
		{
			if (!this.m_spawnAtMaxLevelDepth && Heightmap.AtMaxLevelDepth(base.transform.position + Vector3.up * this.m_levelOffset))
			{
				return;
			}
			if (UnityEngine.Random.value <= this.m_chanceToSpawn)
			{
				Vector3 b = UnityEngine.Random.insideUnitCircle * 0.2f;
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_spawnOnPlaced, base.transform.position + Vector3.up * 0.5f + b, Quaternion.identity);
				gameObject.GetComponent<ItemDrop>().m_itemData.m_stack = UnityEngine.Random.Range(1, this.m_maxSpawned + 1);
				gameObject.GetComponent<Rigidbody>().velocity = Vector3.up * 4f;
			}
		}
	}

	// Token: 0x06000F8A RID: 3978 RVA: 0x0006E094 File Offset: 0x0006C294
	private static void GetModifiers(Vector3 point, float range, List<TerrainModifier> modifiers, TerrainModifier ignore = null)
	{
		foreach (TerrainModifier terrainModifier in TerrainModifier.m_instances)
		{
			if (!(terrainModifier == ignore) && Utils.DistanceXZ(point, terrainModifier.transform.position) < range)
			{
				modifiers.Add(terrainModifier);
			}
		}
	}

	// Token: 0x06000F8B RID: 3979 RVA: 0x0006E104 File Offset: 0x0006C304
	public static Piece FindClosestModifierPieceInRange(Vector3 point, float range)
	{
		float num = 999999f;
		TerrainModifier terrainModifier = null;
		foreach (TerrainModifier terrainModifier2 in TerrainModifier.m_instances)
		{
			if (!(terrainModifier2.m_nview == null))
			{
				float num2 = Utils.DistanceXZ(point, terrainModifier2.transform.position);
				if (num2 <= range && num2 <= num)
				{
					num = num2;
					terrainModifier = terrainModifier2;
				}
			}
		}
		if (terrainModifier)
		{
			return terrainModifier.GetComponent<Piece>();
		}
		return null;
	}

	// Token: 0x06000F8C RID: 3980 RVA: 0x0006E198 File Offset: 0x0006C398
	private void RemoveOthers(Vector3 point, float range)
	{
		List<TerrainModifier> list = new List<TerrainModifier>();
		TerrainModifier.GetModifiers(point, range, list, this);
		int num = 0;
		foreach (TerrainModifier terrainModifier in list)
		{
			if ((this.m_level || !terrainModifier.m_level) && (!this.m_paintCleared || this.m_paintType != TerrainModifier.PaintType.Reset || (terrainModifier.m_paintCleared && terrainModifier.m_paintType == TerrainModifier.PaintType.Reset)) && terrainModifier.m_nview && terrainModifier.m_nview.IsValid())
			{
				num++;
				terrainModifier.m_nview.ClaimOwnership();
				terrainModifier.m_nview.Destroy();
			}
		}
	}

	// Token: 0x06000F8D RID: 3981 RVA: 0x0006E258 File Offset: 0x0006C458
	private static int SortByModifiers(TerrainModifier a, TerrainModifier b)
	{
		if (a.m_playerModifiction != b.m_playerModifiction)
		{
			return a.m_playerModifiction.CompareTo(b.m_playerModifiction);
		}
		if (a.m_sortOrder == b.m_sortOrder)
		{
			return a.GetCreationTime().CompareTo(b.GetCreationTime());
		}
		return a.m_sortOrder.CompareTo(b.m_sortOrder);
	}

	// Token: 0x06000F8E RID: 3982 RVA: 0x0006E2B9 File Offset: 0x0006C4B9
	public static List<TerrainModifier> GetAllInstances()
	{
		if (TerrainModifier.m_needsSorting)
		{
			TerrainModifier.m_instances.Sort(new Comparison<TerrainModifier>(TerrainModifier.SortByModifiers));
			TerrainModifier.m_needsSorting = false;
		}
		return TerrainModifier.m_instances;
	}

	// Token: 0x06000F8F RID: 3983 RVA: 0x0006E2E4 File Offset: 0x0006C4E4
	private void OnDrawGizmosSelected()
	{
		Gizmos.matrix = Matrix4x4.TRS(base.transform.position + Vector3.up * this.m_levelOffset, Quaternion.identity, new Vector3(1f, 0f, 1f));
		if (this.m_level)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(Vector3.zero, this.m_levelRadius);
		}
		if (this.m_smooth)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(Vector3.zero, this.m_smoothRadius);
		}
		if (this.m_paintCleared)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(Vector3.zero, this.m_paintRadius);
		}
		Gizmos.matrix = Matrix4x4.identity;
	}

	// Token: 0x06000F90 RID: 3984 RVA: 0x0006E3A4 File Offset: 0x0006C5A4
	public long GetCreationTime()
	{
		if (this.m_nview && this.m_nview.GetZDO() != null)
		{
			return this.m_nview.GetZDO().m_timeCreated;
		}
		return 0L;
	}

	// Token: 0x04000E4E RID: 3662
	private static bool m_triggerOnPlaced = false;

	// Token: 0x04000E4F RID: 3663
	public int m_sortOrder;

	// Token: 0x04000E50 RID: 3664
	public bool m_playerModifiction;

	// Token: 0x04000E51 RID: 3665
	public float m_levelOffset;

	// Token: 0x04000E52 RID: 3666
	[Header("Level")]
	public bool m_level;

	// Token: 0x04000E53 RID: 3667
	public float m_levelRadius = 2f;

	// Token: 0x04000E54 RID: 3668
	public bool m_square = true;

	// Token: 0x04000E55 RID: 3669
	[Header("Smooth")]
	public bool m_smooth;

	// Token: 0x04000E56 RID: 3670
	public float m_smoothRadius = 2f;

	// Token: 0x04000E57 RID: 3671
	public float m_smoothPower = 3f;

	// Token: 0x04000E58 RID: 3672
	[Header("Paint")]
	public bool m_paintCleared = true;

	// Token: 0x04000E59 RID: 3673
	public bool m_paintHeightCheck;

	// Token: 0x04000E5A RID: 3674
	public TerrainModifier.PaintType m_paintType;

	// Token: 0x04000E5B RID: 3675
	public float m_paintRadius = 2f;

	// Token: 0x04000E5C RID: 3676
	[Header("Effects")]
	public EffectList m_onPlacedEffect = new EffectList();

	// Token: 0x04000E5D RID: 3677
	[Header("Spawn items")]
	public GameObject m_spawnOnPlaced;

	// Token: 0x04000E5E RID: 3678
	public float m_chanceToSpawn = 1f;

	// Token: 0x04000E5F RID: 3679
	public int m_maxSpawned = 1;

	// Token: 0x04000E60 RID: 3680
	public bool m_spawnAtMaxLevelDepth = true;

	// Token: 0x04000E61 RID: 3681
	private bool m_wasEnabled;

	// Token: 0x04000E62 RID: 3682
	private ZNetView m_nview;

	// Token: 0x04000E63 RID: 3683
	private static List<TerrainModifier> m_instances = new List<TerrainModifier>();

	// Token: 0x04000E64 RID: 3684
	private static bool m_needsSorting = false;

	// Token: 0x020001AD RID: 429
	public enum PaintType
	{
		// Token: 0x04001314 RID: 4884
		Dirt,
		// Token: 0x04001315 RID: 4885
		Cultivate,
		// Token: 0x04001316 RID: 4886
		Paved,
		// Token: 0x04001317 RID: 4887
		Reset
	}
}
