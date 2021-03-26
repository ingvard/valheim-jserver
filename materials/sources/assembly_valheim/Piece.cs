using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000E6 RID: 230
public class Piece : StaticTarget
{
	// Token: 0x06000E41 RID: 3649 RVA: 0x00065DFC File Offset: 0x00063FFC
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		Piece.m_allPieces.Add(this);
		this.m_myListIndex = Piece.m_allPieces.Count - 1;
		if (this.m_nview && this.m_nview.IsValid())
		{
			if (Piece.m_creatorHash == 0)
			{
				Piece.m_creatorHash = "creator".GetStableHashCode();
			}
			this.m_creator = this.m_nview.GetZDO().GetLong(Piece.m_creatorHash, 0L);
		}
	}

	// Token: 0x06000E42 RID: 3650 RVA: 0x00065E80 File Offset: 0x00064080
	private void OnDestroy()
	{
		if (this.m_myListIndex >= 0)
		{
			Piece.m_allPieces[this.m_myListIndex] = Piece.m_allPieces[Piece.m_allPieces.Count - 1];
			Piece.m_allPieces[this.m_myListIndex].m_myListIndex = this.m_myListIndex;
			Piece.m_allPieces.RemoveAt(Piece.m_allPieces.Count - 1);
			this.m_myListIndex = -1;
		}
	}

	// Token: 0x06000E43 RID: 3651 RVA: 0x00065EF4 File Offset: 0x000640F4
	public bool CanBeRemoved()
	{
		Container componentInChildren = base.GetComponentInChildren<Container>();
		if (componentInChildren != null)
		{
			return componentInChildren.CanBeRemoved();
		}
		Ship componentInChildren2 = base.GetComponentInChildren<Ship>();
		return !(componentInChildren2 != null) || componentInChildren2.CanBeRemoved();
	}

	// Token: 0x06000E44 RID: 3652 RVA: 0x00065F30 File Offset: 0x00064130
	public void DropResources()
	{
		Container container = null;
		foreach (Piece.Requirement requirement in this.m_resources)
		{
			if (!(requirement.m_resItem == null) && requirement.m_recover)
			{
				GameObject gameObject = requirement.m_resItem.gameObject;
				int j = requirement.m_amount;
				if (!this.IsPlacedByPlayer())
				{
					j = Mathf.Max(1, j / 3);
				}
				if (this.m_destroyedLootPrefab)
				{
					while (j > 0)
					{
						ItemDrop.ItemData itemData = gameObject.GetComponent<ItemDrop>().m_itemData.Clone();
						itemData.m_dropPrefab = gameObject;
						itemData.m_stack = Mathf.Min(j, itemData.m_shared.m_maxStackSize);
						j -= itemData.m_stack;
						if (container == null || !container.GetInventory().HaveEmptySlot())
						{
							container = UnityEngine.Object.Instantiate<GameObject>(this.m_destroyedLootPrefab, base.transform.position + Vector3.up, Quaternion.identity).GetComponent<Container>();
						}
						container.GetInventory().AddItem(itemData);
					}
				}
				else
				{
					while (j > 0)
					{
						ItemDrop component = UnityEngine.Object.Instantiate<GameObject>(gameObject, base.transform.position + Vector3.up, Quaternion.identity).GetComponent<ItemDrop>();
						component.SetStack(Mathf.Min(j, component.m_itemData.m_shared.m_maxStackSize));
						j -= component.m_itemData.m_stack;
					}
				}
			}
		}
	}

	// Token: 0x06000E45 RID: 3653 RVA: 0x000660B0 File Offset: 0x000642B0
	public override bool IsValidMonsterTarget()
	{
		return this.IsPlacedByPlayer();
	}

	// Token: 0x06000E46 RID: 3654 RVA: 0x000660B8 File Offset: 0x000642B8
	public void SetCreator(long uid)
	{
		if (this.m_nview.IsOwner())
		{
			if (this.GetCreator() != 0L)
			{
				return;
			}
			this.m_creator = uid;
			this.m_nview.GetZDO().Set(Piece.m_creatorHash, uid);
		}
	}

	// Token: 0x06000E47 RID: 3655 RVA: 0x000660ED File Offset: 0x000642ED
	public long GetCreator()
	{
		return this.m_creator;
	}

	// Token: 0x06000E48 RID: 3656 RVA: 0x000660F8 File Offset: 0x000642F8
	public bool IsCreator()
	{
		long creator = this.GetCreator();
		long playerID = Game.instance.GetPlayerProfile().GetPlayerID();
		return creator == playerID;
	}

	// Token: 0x06000E49 RID: 3657 RVA: 0x0006611E File Offset: 0x0006431E
	public bool IsPlacedByPlayer()
	{
		return this.GetCreator() != 0L;
	}

	// Token: 0x06000E4A RID: 3658 RVA: 0x0006612C File Offset: 0x0006432C
	public void SetInvalidPlacementHeightlight(bool enabled)
	{
		if ((enabled && this.m_invalidPlacementMaterials != null) || (!enabled && this.m_invalidPlacementMaterials == null))
		{
			return;
		}
		Renderer[] componentsInChildren = base.GetComponentsInChildren<Renderer>();
		if (enabled)
		{
			this.m_invalidPlacementMaterials = new List<KeyValuePair<Renderer, Material[]>>();
			foreach (Renderer renderer in componentsInChildren)
			{
				Material[] sharedMaterials = renderer.sharedMaterials;
				this.m_invalidPlacementMaterials.Add(new KeyValuePair<Renderer, Material[]>(renderer, sharedMaterials));
			}
			Renderer[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				foreach (Material material in array[i].materials)
				{
					if (material.HasProperty("_EmissionColor"))
					{
						material.SetColor("_EmissionColor", Color.red * 0.7f);
					}
					material.color = Color.red;
				}
			}
			return;
		}
		foreach (KeyValuePair<Renderer, Material[]> keyValuePair in this.m_invalidPlacementMaterials)
		{
			if (keyValuePair.Key)
			{
				keyValuePair.Key.materials = keyValuePair.Value;
			}
		}
		this.m_invalidPlacementMaterials = null;
	}

	// Token: 0x06000E4B RID: 3659 RVA: 0x0006626C File Offset: 0x0006446C
	public static void GetSnapPoints(Vector3 point, float radius, List<Transform> points, List<Piece> pieces)
	{
		if (Piece.pieceRayMask == 0)
		{
			Piece.pieceRayMask = LayerMask.GetMask(new string[]
			{
				"piece",
				"piece_nonsolid"
			});
		}
		int num = Physics.OverlapSphereNonAlloc(point, radius, Piece.pieceColliders, Piece.pieceRayMask);
		for (int i = 0; i < num; i++)
		{
			Piece componentInParent = Piece.pieceColliders[i].GetComponentInParent<Piece>();
			if (componentInParent != null)
			{
				componentInParent.GetSnapPoints(points);
				pieces.Add(componentInParent);
			}
		}
	}

	// Token: 0x06000E4C RID: 3660 RVA: 0x000662E4 File Offset: 0x000644E4
	public static void GetAllPiecesInRadius(Vector3 p, float radius, List<Piece> pieces)
	{
		if (Piece.ghostLayer == 0)
		{
			Piece.ghostLayer = LayerMask.NameToLayer("ghost");
		}
		foreach (Piece piece in Piece.m_allPieces)
		{
			if (piece.gameObject.layer != Piece.ghostLayer && Vector3.Distance(p, piece.transform.position) < radius)
			{
				pieces.Add(piece);
			}
		}
	}

	// Token: 0x06000E4D RID: 3661 RVA: 0x00066374 File Offset: 0x00064574
	public void GetSnapPoints(List<Transform> points)
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (child.CompareTag("snappoint"))
			{
				points.Add(child);
			}
		}
	}

	// Token: 0x04000D01 RID: 3329
	private static int pieceRayMask = 0;

	// Token: 0x04000D02 RID: 3330
	private static Collider[] pieceColliders = new Collider[2000];

	// Token: 0x04000D03 RID: 3331
	private static int ghostLayer = 0;

	// Token: 0x04000D04 RID: 3332
	[Header("Basic stuffs")]
	public Sprite m_icon;

	// Token: 0x04000D05 RID: 3333
	public string m_name = "";

	// Token: 0x04000D06 RID: 3334
	public string m_description = "";

	// Token: 0x04000D07 RID: 3335
	public bool m_enabled = true;

	// Token: 0x04000D08 RID: 3336
	public Piece.PieceCategory m_category;

	// Token: 0x04000D09 RID: 3337
	public bool m_isUpgrade;

	// Token: 0x04000D0A RID: 3338
	[Header("Comfort")]
	public int m_comfort;

	// Token: 0x04000D0B RID: 3339
	public Piece.ComfortGroup m_comfortGroup;

	// Token: 0x04000D0C RID: 3340
	[Header("Placement rules")]
	public bool m_groundPiece;

	// Token: 0x04000D0D RID: 3341
	public bool m_allowAltGroundPlacement;

	// Token: 0x04000D0E RID: 3342
	public bool m_groundOnly;

	// Token: 0x04000D0F RID: 3343
	public bool m_cultivatedGroundOnly;

	// Token: 0x04000D10 RID: 3344
	public bool m_waterPiece;

	// Token: 0x04000D11 RID: 3345
	public bool m_clipGround;

	// Token: 0x04000D12 RID: 3346
	public bool m_clipEverything;

	// Token: 0x04000D13 RID: 3347
	public bool m_noInWater;

	// Token: 0x04000D14 RID: 3348
	public bool m_notOnWood;

	// Token: 0x04000D15 RID: 3349
	public bool m_notOnTiltingSurface;

	// Token: 0x04000D16 RID: 3350
	public bool m_inCeilingOnly;

	// Token: 0x04000D17 RID: 3351
	public bool m_notOnFloor;

	// Token: 0x04000D18 RID: 3352
	public bool m_noClipping;

	// Token: 0x04000D19 RID: 3353
	public bool m_onlyInTeleportArea;

	// Token: 0x04000D1A RID: 3354
	public bool m_allowedInDungeons;

	// Token: 0x04000D1B RID: 3355
	public float m_spaceRequirement;

	// Token: 0x04000D1C RID: 3356
	public bool m_repairPiece;

	// Token: 0x04000D1D RID: 3357
	public bool m_canBeRemoved = true;

	// Token: 0x04000D1E RID: 3358
	[BitMask(typeof(Heightmap.Biome))]
	public Heightmap.Biome m_onlyInBiome;

	// Token: 0x04000D1F RID: 3359
	[Header("Effects")]
	public EffectList m_placeEffect = new EffectList();

	// Token: 0x04000D20 RID: 3360
	[Header("Requirements")]
	public string m_dlc = "";

	// Token: 0x04000D21 RID: 3361
	public CraftingStation m_craftingStation;

	// Token: 0x04000D22 RID: 3362
	public Piece.Requirement[] m_resources = new Piece.Requirement[0];

	// Token: 0x04000D23 RID: 3363
	public GameObject m_destroyedLootPrefab;

	// Token: 0x04000D24 RID: 3364
	private ZNetView m_nview;

	// Token: 0x04000D25 RID: 3365
	private List<KeyValuePair<Renderer, Material[]>> m_invalidPlacementMaterials;

	// Token: 0x04000D26 RID: 3366
	private long m_creator;

	// Token: 0x04000D27 RID: 3367
	private int m_myListIndex = -1;

	// Token: 0x04000D28 RID: 3368
	private static List<Piece> m_allPieces = new List<Piece>();

	// Token: 0x04000D29 RID: 3369
	private static int m_creatorHash = 0;

	// Token: 0x020001A3 RID: 419
	public enum PieceCategory
	{
		// Token: 0x040012E1 RID: 4833
		Misc,
		// Token: 0x040012E2 RID: 4834
		Crafting,
		// Token: 0x040012E3 RID: 4835
		Building,
		// Token: 0x040012E4 RID: 4836
		Furniture,
		// Token: 0x040012E5 RID: 4837
		Max,
		// Token: 0x040012E6 RID: 4838
		All = 100
	}

	// Token: 0x020001A4 RID: 420
	public enum ComfortGroup
	{
		// Token: 0x040012E8 RID: 4840
		None,
		// Token: 0x040012E9 RID: 4841
		Fire,
		// Token: 0x040012EA RID: 4842
		Bed,
		// Token: 0x040012EB RID: 4843
		Banner,
		// Token: 0x040012EC RID: 4844
		Chair
	}

	// Token: 0x020001A5 RID: 421
	[Serializable]
	public class Requirement
	{
		// Token: 0x060011BB RID: 4539 RVA: 0x0007A062 File Offset: 0x00078262
		public int GetAmount(int qualityLevel)
		{
			if (qualityLevel <= 1)
			{
				return this.m_amount;
			}
			return (qualityLevel - 1) * this.m_amountPerLevel;
		}

		// Token: 0x040012ED RID: 4845
		[Header("Resource")]
		public ItemDrop m_resItem;

		// Token: 0x040012EE RID: 4846
		public int m_amount = 1;

		// Token: 0x040012EF RID: 4847
		[Header("Item")]
		public int m_amountPerLevel = 1;

		// Token: 0x040012F0 RID: 4848
		[Header("Piece")]
		public bool m_recover = true;
	}
}
