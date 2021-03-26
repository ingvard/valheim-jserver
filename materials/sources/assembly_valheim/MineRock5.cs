using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000E2 RID: 226
public class MineRock5 : MonoBehaviour, IDestructible, Hoverable
{
	// Token: 0x06000E1A RID: 3610 RVA: 0x000646EC File Offset: 0x000628EC
	private void Start()
	{
		Collider[] componentsInChildren = base.gameObject.GetComponentsInChildren<Collider>();
		this.m_hitAreas = new List<MineRock5.HitArea>(componentsInChildren.Length);
		this.m_extraRenderers = new List<Renderer>();
		foreach (Collider collider in componentsInChildren)
		{
			MineRock5.HitArea hitArea = new MineRock5.HitArea();
			hitArea.m_collider = collider;
			hitArea.m_meshFilter = collider.GetComponent<MeshFilter>();
			hitArea.m_meshRenderer = collider.GetComponent<MeshRenderer>();
			hitArea.m_physics = collider.GetComponent<StaticPhysics>();
			hitArea.m_health = this.m_health;
			hitArea.m_baseScale = hitArea.m_collider.transform.localScale.x;
			for (int j = 0; j < collider.transform.childCount; j++)
			{
				Renderer[] componentsInChildren2 = collider.transform.GetChild(j).GetComponentsInChildren<Renderer>();
				this.m_extraRenderers.AddRange(componentsInChildren2);
			}
			this.m_hitAreas.Add(hitArea);
		}
		if (MineRock5.m_rayMask == 0)
		{
			MineRock5.m_rayMask = LayerMask.GetMask(new string[]
			{
				"piece",
				"Default",
				"static_solid",
				"Default_small",
				"terrain"
			});
		}
		if (MineRock5.m_groundLayer == 0)
		{
			MineRock5.m_groundLayer = LayerMask.NameToLayer("terrain");
		}
		Material[] array = null;
		foreach (MineRock5.HitArea hitArea2 in this.m_hitAreas)
		{
			if (array == null || hitArea2.m_meshRenderer.sharedMaterials.Length > array.Length)
			{
				array = hitArea2.m_meshRenderer.sharedMaterials;
			}
		}
		this.m_meshFilter = base.gameObject.AddComponent<MeshFilter>();
		this.m_meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
		this.m_meshRenderer.sharedMaterials = array;
		this.m_meshFilter.mesh = new Mesh();
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview && this.m_nview.GetZDO() != null)
		{
			this.m_nview.Register<HitData, int>("Damage", new Action<long, HitData, int>(this.RPC_Damage));
			this.m_nview.Register<int, float>("SetAreaHealth", new Action<long, int, float>(this.RPC_SetAreaHealth));
		}
		this.CheckForUpdate();
		base.InvokeRepeating("CheckForUpdate", UnityEngine.Random.Range(5f, 10f), 10f);
	}

	// Token: 0x06000E1B RID: 3611 RVA: 0x0006495C File Offset: 0x00062B5C
	private void CheckSupport()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		this.UpdateSupport();
		for (int i = 0; i < this.m_hitAreas.Count; i++)
		{
			MineRock5.HitArea hitArea = this.m_hitAreas[i];
			if (hitArea.m_health > 0f && !hitArea.m_supported)
			{
				HitData hitData = new HitData();
				hitData.m_damage.m_damage = this.m_health;
				hitData.m_point = hitArea.m_collider.bounds.center;
				this.DamageArea(i, hitData);
			}
		}
	}

	// Token: 0x06000E1C RID: 3612 RVA: 0x000649FB File Offset: 0x00062BFB
	private void CheckForUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview.GetZDO().m_dataRevision != this.m_lastDataRevision)
		{
			this.LoadHealth();
			this.UpdateMesh();
		}
	}

	// Token: 0x06000E1D RID: 3613 RVA: 0x00064A30 File Offset: 0x00062C30
	private void LoadHealth()
	{
		byte[] byteArray = this.m_nview.GetZDO().GetByteArray("health");
		if (byteArray != null)
		{
			ZPackage zpackage = new ZPackage(byteArray);
			int num = zpackage.ReadInt();
			for (int i = 0; i < num; i++)
			{
				float health = zpackage.ReadSingle();
				MineRock5.HitArea hitArea = this.GetHitArea(i);
				if (hitArea != null)
				{
					hitArea.m_health = health;
				}
			}
		}
		this.m_lastDataRevision = this.m_nview.GetZDO().m_dataRevision;
	}

	// Token: 0x06000E1E RID: 3614 RVA: 0x00064AA4 File Offset: 0x00062CA4
	private void SaveHealth()
	{
		ZPackage zpackage = new ZPackage();
		zpackage.Write(this.m_hitAreas.Count);
		foreach (MineRock5.HitArea hitArea in this.m_hitAreas)
		{
			zpackage.Write(hitArea.m_health);
		}
		this.m_nview.GetZDO().Set("health", zpackage.GetArray());
		this.m_lastDataRevision = this.m_nview.GetZDO().m_dataRevision;
	}

	// Token: 0x06000E1F RID: 3615 RVA: 0x00064B44 File Offset: 0x00062D44
	private void UpdateMesh()
	{
		MineRock5.m_tempInstancesA.Clear();
		MineRock5.m_tempInstancesB.Clear();
		Material y = this.m_meshRenderer.sharedMaterials[0];
		Matrix4x4 inverse = base.transform.localToWorldMatrix.inverse;
		for (int i = 0; i < this.m_hitAreas.Count; i++)
		{
			MineRock5.HitArea hitArea = this.m_hitAreas[i];
			if (hitArea.m_health > 0f)
			{
				CombineInstance item = default(CombineInstance);
				item.mesh = hitArea.m_meshFilter.sharedMesh;
				item.transform = inverse * hitArea.m_meshFilter.transform.localToWorldMatrix;
				for (int j = 0; j < hitArea.m_meshFilter.sharedMesh.subMeshCount; j++)
				{
					item.subMeshIndex = j;
					if (hitArea.m_meshRenderer.sharedMaterials[j] == y)
					{
						MineRock5.m_tempInstancesA.Add(item);
					}
					else
					{
						MineRock5.m_tempInstancesB.Add(item);
					}
				}
				hitArea.m_meshRenderer.enabled = false;
				hitArea.m_collider.gameObject.SetActive(true);
			}
			else
			{
				hitArea.m_collider.gameObject.SetActive(false);
			}
		}
		if (MineRock5.m_tempMeshA == null)
		{
			MineRock5.m_tempMeshA = new Mesh();
			MineRock5.m_tempMeshB = new Mesh();
		}
		MineRock5.m_tempMeshA.CombineMeshes(MineRock5.m_tempInstancesA.ToArray());
		MineRock5.m_tempMeshB.CombineMeshes(MineRock5.m_tempInstancesB.ToArray());
		CombineInstance combineInstance = default(CombineInstance);
		combineInstance.mesh = MineRock5.m_tempMeshA;
		CombineInstance combineInstance2 = default(CombineInstance);
		combineInstance2.mesh = MineRock5.m_tempMeshB;
		this.m_meshFilter.mesh.CombineMeshes(new CombineInstance[]
		{
			combineInstance,
			combineInstance2
		}, false, false);
		this.m_meshRenderer.enabled = true;
		Renderer[] array = new Renderer[this.m_extraRenderers.Count + 1];
		this.m_extraRenderers.CopyTo(0, array, 0, this.m_extraRenderers.Count);
		array[array.Length - 1] = this.m_meshRenderer;
		LODGroup component = base.gameObject.GetComponent<LODGroup>();
		LOD[] lods = component.GetLODs();
		lods[0].renderers = array;
		component.SetLODs(lods);
	}

	// Token: 0x06000E20 RID: 3616 RVA: 0x00064D99 File Offset: 0x00062F99
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_name);
	}

	// Token: 0x06000E21 RID: 3617 RVA: 0x00064DAB File Offset: 0x00062FAB
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000E22 RID: 3618 RVA: 0x000027E2 File Offset: 0x000009E2
	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Default;
	}

	// Token: 0x06000E23 RID: 3619 RVA: 0x00064DB4 File Offset: 0x00062FB4
	public void Damage(HitData hit)
	{
		if (this.m_nview == null || !this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_hitAreas == null)
		{
			return;
		}
		if (hit.m_hitCollider == null)
		{
			ZLog.Log("Minerock hit has no collider");
			return;
		}
		int areaIndex = this.GetAreaIndex(hit.m_hitCollider);
		if (areaIndex == -1)
		{
			ZLog.Log("Invalid hit area on " + base.gameObject.name);
			return;
		}
		this.m_nview.InvokeRPC("Damage", new object[]
		{
			hit,
			areaIndex
		});
	}

	// Token: 0x06000E24 RID: 3620 RVA: 0x00064E4E File Offset: 0x0006304E
	private void RPC_Damage(long sender, HitData hit, int hitAreaIndex)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.DamageArea(hitAreaIndex, hit) && this.m_supportCheck)
		{
			this.CheckSupport();
		}
	}

	// Token: 0x06000E25 RID: 3621 RVA: 0x00064E84 File Offset: 0x00063084
	private bool DamageArea(int hitAreaIndex, HitData hit)
	{
		ZLog.Log("hit mine rock " + hitAreaIndex);
		MineRock5.HitArea hitArea = this.GetHitArea(hitAreaIndex);
		if (hitArea == null)
		{
			ZLog.Log("Missing hit area " + hitAreaIndex);
			return false;
		}
		this.LoadHealth();
		if (hitArea.m_health <= 0f)
		{
			ZLog.Log("Already destroyed");
			return false;
		}
		HitData.DamageModifier type;
		hit.ApplyResistance(this.m_damageModifiers, out type);
		float totalDamage = hit.GetTotalDamage();
		if (hit.m_toolTier < this.m_minToolTier)
		{
			DamageText.instance.ShowText(DamageText.TextType.TooHard, hit.m_point, 0f, false);
			return false;
		}
		DamageText.instance.ShowText(type, hit.m_point, totalDamage, false);
		if (totalDamage <= 0f)
		{
			return false;
		}
		hitArea.m_health -= totalDamage;
		this.SaveHealth();
		this.m_hitEffect.Create(hit.m_point, Quaternion.identity, null, 1f);
		Player closestPlayer = Player.GetClosestPlayer(hit.m_point, 10f);
		if (closestPlayer)
		{
			closestPlayer.AddNoise(100f);
		}
		if (hitArea.m_health <= 0f)
		{
			this.m_nview.InvokeRPC(ZNetView.Everybody, "SetAreaHealth", new object[]
			{
				hitAreaIndex,
				hitArea.m_health
			});
			this.m_destroyedEffect.Create(hit.m_point, Quaternion.identity, null, 1f);
			foreach (GameObject original in this.m_dropItems.GetDropList())
			{
				Vector3 position = hit.m_point + UnityEngine.Random.insideUnitSphere * 0.3f;
				UnityEngine.Object.Instantiate<GameObject>(original, position, Quaternion.identity);
			}
			if (this.AllDestroyed())
			{
				this.m_nview.Destroy();
			}
			return true;
		}
		return false;
	}

	// Token: 0x06000E26 RID: 3622 RVA: 0x00065078 File Offset: 0x00063278
	private bool AllDestroyed()
	{
		for (int i = 0; i < this.m_hitAreas.Count; i++)
		{
			if (this.m_hitAreas[i].m_health > 0f)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000E27 RID: 3623 RVA: 0x000650B8 File Offset: 0x000632B8
	private bool NonDestroyed()
	{
		for (int i = 0; i < this.m_hitAreas.Count; i++)
		{
			if (this.m_hitAreas[i].m_health <= 0f)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000E28 RID: 3624 RVA: 0x000650F8 File Offset: 0x000632F8
	private void RPC_SetAreaHealth(long sender, int index, float health)
	{
		MineRock5.HitArea hitArea = this.GetHitArea(index);
		if (hitArea != null)
		{
			hitArea.m_health = health;
		}
		this.UpdateMesh();
	}

	// Token: 0x06000E29 RID: 3625 RVA: 0x00065120 File Offset: 0x00063320
	private int GetAreaIndex(Collider area)
	{
		for (int i = 0; i < this.m_hitAreas.Count; i++)
		{
			if (this.m_hitAreas[i].m_collider == area)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x06000E2A RID: 3626 RVA: 0x0006515F File Offset: 0x0006335F
	private MineRock5.HitArea GetHitArea(int index)
	{
		if (index < 0 || index >= this.m_hitAreas.Count)
		{
			return null;
		}
		return this.m_hitAreas[index];
	}

	// Token: 0x06000E2B RID: 3627 RVA: 0x00065184 File Offset: 0x00063384
	private void UpdateSupport()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (!this.m_haveSetupBounds)
		{
			this.SetupColliders();
			this.m_haveSetupBounds = true;
		}
		foreach (MineRock5.HitArea hitArea in this.m_hitAreas)
		{
			hitArea.m_supported = false;
		}
		Vector3 position = base.transform.position;
		for (int i = 0; i < 3; i++)
		{
			foreach (MineRock5.HitArea hitArea2 in this.m_hitAreas)
			{
				if (!hitArea2.m_supported)
				{
					int num = Physics.OverlapBoxNonAlloc(position + hitArea2.m_bound.m_pos, hitArea2.m_bound.m_size, MineRock5.m_tempColliders, hitArea2.m_bound.m_rot, MineRock5.m_rayMask);
					for (int j = 0; j < num; j++)
					{
						Collider collider = MineRock5.m_tempColliders[j];
						if (!(collider == hitArea2.m_collider) && !(collider.attachedRigidbody != null) && !collider.isTrigger)
						{
							hitArea2.m_supported = (hitArea2.m_supported || this.GetSupport(collider));
							if (hitArea2.m_supported)
							{
								break;
							}
						}
					}
				}
			}
		}
		ZLog.Log("Suport time " + (Time.realtimeSinceStartup - realtimeSinceStartup) * 1000f);
	}

	// Token: 0x06000E2C RID: 3628 RVA: 0x00065320 File Offset: 0x00063520
	private bool GetSupport(Collider c)
	{
		if (c.gameObject.layer == MineRock5.m_groundLayer)
		{
			return true;
		}
		IDestructible componentInParent = c.gameObject.GetComponentInParent<IDestructible>();
		if (componentInParent != null)
		{
			if (componentInParent == this)
			{
				foreach (MineRock5.HitArea hitArea in this.m_hitAreas)
				{
					if (hitArea.m_collider == c)
					{
						return hitArea.m_supported;
					}
				}
			}
			return c.transform.position.y < base.transform.position.y;
		}
		return true;
	}

	// Token: 0x06000E2D RID: 3629 RVA: 0x000653D0 File Offset: 0x000635D0
	private void SetupColliders()
	{
		Vector3 position = base.transform.position;
		foreach (MineRock5.HitArea hitArea in this.m_hitAreas)
		{
			hitArea.m_bound.m_rot = Quaternion.identity;
			hitArea.m_bound.m_pos = hitArea.m_collider.bounds.center - position;
			hitArea.m_bound.m_size = hitArea.m_collider.bounds.size * 0.5f;
		}
	}

	// Token: 0x04000CCC RID: 3276
	private static Mesh m_tempMeshA;

	// Token: 0x04000CCD RID: 3277
	private static Mesh m_tempMeshB;

	// Token: 0x04000CCE RID: 3278
	private static List<CombineInstance> m_tempInstancesA = new List<CombineInstance>();

	// Token: 0x04000CCF RID: 3279
	private static List<CombineInstance> m_tempInstancesB = new List<CombineInstance>();

	// Token: 0x04000CD0 RID: 3280
	public string m_name = "";

	// Token: 0x04000CD1 RID: 3281
	public float m_health = 2f;

	// Token: 0x04000CD2 RID: 3282
	public HitData.DamageModifiers m_damageModifiers;

	// Token: 0x04000CD3 RID: 3283
	public int m_minToolTier;

	// Token: 0x04000CD4 RID: 3284
	public bool m_supportCheck = true;

	// Token: 0x04000CD5 RID: 3285
	public EffectList m_destroyedEffect = new EffectList();

	// Token: 0x04000CD6 RID: 3286
	public EffectList m_hitEffect = new EffectList();

	// Token: 0x04000CD7 RID: 3287
	public DropTable m_dropItems;

	// Token: 0x04000CD8 RID: 3288
	private List<MineRock5.HitArea> m_hitAreas;

	// Token: 0x04000CD9 RID: 3289
	private List<Renderer> m_extraRenderers;

	// Token: 0x04000CDA RID: 3290
	private bool m_haveSetupBounds;

	// Token: 0x04000CDB RID: 3291
	private ZNetView m_nview;

	// Token: 0x04000CDC RID: 3292
	private MeshFilter m_meshFilter;

	// Token: 0x04000CDD RID: 3293
	private MeshRenderer m_meshRenderer;

	// Token: 0x04000CDE RID: 3294
	private uint m_lastDataRevision;

	// Token: 0x04000CDF RID: 3295
	private const int m_supportIterations = 3;

	// Token: 0x04000CE0 RID: 3296
	private static int m_rayMask = 0;

	// Token: 0x04000CE1 RID: 3297
	private static int m_groundLayer = 0;

	// Token: 0x04000CE2 RID: 3298
	private static Collider[] m_tempColliders = new Collider[128];

	// Token: 0x020001A1 RID: 417
	private struct BoundData
	{
		// Token: 0x040012D5 RID: 4821
		public Vector3 m_pos;

		// Token: 0x040012D6 RID: 4822
		public Quaternion m_rot;

		// Token: 0x040012D7 RID: 4823
		public Vector3 m_size;
	}

	// Token: 0x020001A2 RID: 418
	private class HitArea
	{
		// Token: 0x040012D8 RID: 4824
		public Collider m_collider;

		// Token: 0x040012D9 RID: 4825
		public MeshRenderer m_meshRenderer;

		// Token: 0x040012DA RID: 4826
		public MeshFilter m_meshFilter;

		// Token: 0x040012DB RID: 4827
		public StaticPhysics m_physics;

		// Token: 0x040012DC RID: 4828
		public float m_health;

		// Token: 0x040012DD RID: 4829
		public MineRock5.BoundData m_bound;

		// Token: 0x040012DE RID: 4830
		public bool m_supported;

		// Token: 0x040012DF RID: 4831
		public float m_baseScale;
	}
}
