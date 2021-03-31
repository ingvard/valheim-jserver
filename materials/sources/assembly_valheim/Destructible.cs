using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000C3 RID: 195
public class Destructible : MonoBehaviour, IDestructible
{
	// Token: 0x06000CD8 RID: 3288 RVA: 0x0005BE28 File Offset: 0x0005A028
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_body = base.GetComponent<Rigidbody>();
		if (this.m_nview && this.m_nview.GetZDO() != null)
		{
			this.m_nview.Register<HitData>("Damage", new Action<long, HitData>(this.RPC_Damage));
			if (this.m_autoCreateFragments)
			{
				this.m_nview.Register("CreateFragments", new Action<long>(this.RPC_CreateFragments));
			}
			if (this.m_ttl > 0f)
			{
				base.InvokeRepeating("DestroyNow", this.m_ttl, 1f);
			}
		}
	}

	// Token: 0x06000CD9 RID: 3289 RVA: 0x0005BECA File Offset: 0x0005A0CA
	private void Start()
	{
		this.m_firstFrame = false;
	}

	// Token: 0x06000CDA RID: 3290 RVA: 0x000058CD File Offset: 0x00003ACD
	public GameObject GetParentObject()
	{
		return null;
	}

	// Token: 0x06000CDB RID: 3291 RVA: 0x0005BED3 File Offset: 0x0005A0D3
	public DestructibleType GetDestructibleType()
	{
		return this.m_destructibleType;
	}

	// Token: 0x06000CDC RID: 3292 RVA: 0x0005BEDB File Offset: 0x0005A0DB
	public void Damage(HitData hit)
	{
		if (this.m_firstFrame)
		{
			return;
		}
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.m_nview.InvokeRPC("Damage", new object[]
		{
			hit
		});
	}

	// Token: 0x06000CDD RID: 3293 RVA: 0x0005BF10 File Offset: 0x0005A110
	private void RPC_Damage(long sender, HitData hit)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_destroyed)
		{
			return;
		}
		float num = this.m_nview.GetZDO().GetFloat("health", this.m_health);
		HitData.DamageModifier type;
		hit.ApplyResistance(this.m_damages, out type);
		float totalDamage = hit.GetTotalDamage();
		if (this.m_body)
		{
			this.m_body.AddForceAtPosition(hit.m_dir * hit.m_pushForce, hit.m_point, ForceMode.Impulse);
		}
		if (hit.m_toolTier < this.m_minToolTier)
		{
			DamageText.instance.ShowText(DamageText.TextType.TooHard, hit.m_point, 0f, false);
			return;
		}
		DamageText.instance.ShowText(type, hit.m_point, totalDamage, false);
		if (totalDamage <= 0f)
		{
			return;
		}
		num -= totalDamage;
		this.m_nview.GetZDO().Set("health", num);
		this.m_hitEffect.Create(hit.m_point, Quaternion.identity, base.transform, 1f);
		if (this.m_onDamaged != null)
		{
			this.m_onDamaged();
		}
		if (this.m_hitNoise > 0f)
		{
			Player closestPlayer = Player.GetClosestPlayer(hit.m_point, 10f);
			if (closestPlayer)
			{
				closestPlayer.AddNoise(this.m_hitNoise);
			}
		}
		if (num <= 0f)
		{
			this.Destroy();
		}
	}

	// Token: 0x06000CDE RID: 3294 RVA: 0x0005C073 File Offset: 0x0005A273
	private void DestroyNow()
	{
		if (this.m_nview.IsValid() && this.m_nview.IsOwner())
		{
			this.Destroy();
		}
	}

	// Token: 0x06000CDF RID: 3295 RVA: 0x0005C098 File Offset: 0x0005A298
	public void Destroy()
	{
		this.CreateDestructionEffects();
		if (this.m_destroyNoise > 0f)
		{
			Player closestPlayer = Player.GetClosestPlayer(base.transform.position, 10f);
			if (closestPlayer)
			{
				closestPlayer.AddNoise(this.m_destroyNoise);
			}
		}
		if (this.m_spawnWhenDestroyed)
		{
			ZNetView component = UnityEngine.Object.Instantiate<GameObject>(this.m_spawnWhenDestroyed, base.transform.position, base.transform.rotation).GetComponent<ZNetView>();
			component.SetLocalScale(base.transform.localScale);
			component.GetZDO().SetPGWVersion(this.m_nview.GetZDO().GetPGWVersion());
		}
		if (this.m_onDestroyed != null)
		{
			this.m_onDestroyed();
		}
		ZNetScene.instance.Destroy(base.gameObject);
		this.m_destroyed = true;
	}

	// Token: 0x06000CE0 RID: 3296 RVA: 0x0005C16C File Offset: 0x0005A36C
	private void CreateDestructionEffects()
	{
		this.m_destroyedEffect.Create(base.transform.position, base.transform.rotation, base.transform, 1f);
		if (this.m_autoCreateFragments)
		{
			this.m_nview.InvokeRPC(ZNetView.Everybody, "CreateFragments", Array.Empty<object>());
		}
	}

	// Token: 0x06000CE1 RID: 3297 RVA: 0x0005C1C8 File Offset: 0x0005A3C8
	private void RPC_CreateFragments(long peer)
	{
		Destructible.CreateFragments(base.gameObject, true);
	}

	// Token: 0x06000CE2 RID: 3298 RVA: 0x0005C1D8 File Offset: 0x0005A3D8
	public static void CreateFragments(GameObject rootObject, bool visibleOnly = true)
	{
		MeshRenderer[] componentsInChildren = rootObject.GetComponentsInChildren<MeshRenderer>(true);
		int layer = LayerMask.NameToLayer("effect");
		List<Rigidbody> list = new List<Rigidbody>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			if (meshRenderer.gameObject.activeInHierarchy && (!visibleOnly || meshRenderer.isVisible))
			{
				MeshFilter component = meshRenderer.gameObject.GetComponent<MeshFilter>();
				if (!(component == null))
				{
					if (component.sharedMesh == null)
					{
						ZLog.Log("Meshfilter missing mesh " + component.gameObject.name);
					}
					else
					{
						GameObject gameObject = new GameObject();
						gameObject.layer = layer;
						gameObject.transform.position = component.gameObject.transform.position;
						gameObject.transform.rotation = component.gameObject.transform.rotation;
						gameObject.transform.localScale = component.gameObject.transform.lossyScale * 0.9f;
						gameObject.AddComponent<MeshFilter>().sharedMesh = component.sharedMesh;
						MeshRenderer meshRenderer2 = gameObject.AddComponent<MeshRenderer>();
						meshRenderer2.sharedMaterials = meshRenderer.sharedMaterials;
						meshRenderer2.material.SetFloat("_RippleDistance", 0f);
						meshRenderer2.material.SetFloat("_ValueNoise", 0f);
						Rigidbody item = gameObject.AddComponent<Rigidbody>();
						gameObject.AddComponent<BoxCollider>();
						list.Add(item);
						gameObject.AddComponent<TimedDestruction>().Trigger((float)UnityEngine.Random.Range(2, 4));
					}
				}
			}
		}
		if (list.Count > 0)
		{
			Vector3 vector = Vector3.zero;
			int num = 0;
			foreach (Rigidbody rigidbody in list)
			{
				vector += rigidbody.transform.position;
				num++;
			}
			vector /= (float)num;
			foreach (Rigidbody rigidbody2 in list)
			{
				Vector3 vector2 = (rigidbody2.transform.position - vector).normalized * 4f;
				vector2 += UnityEngine.Random.onUnitSphere * 1f;
				rigidbody2.AddForce(vector2, ForceMode.VelocityChange);
			}
		}
	}

	// Token: 0x04000BB9 RID: 3001
	public Action m_onDestroyed;

	// Token: 0x04000BBA RID: 3002
	public Action m_onDamaged;

	// Token: 0x04000BBB RID: 3003
	[Header("Destruction")]
	public DestructibleType m_destructibleType = DestructibleType.Default;

	// Token: 0x04000BBC RID: 3004
	public float m_health = 1f;

	// Token: 0x04000BBD RID: 3005
	public HitData.DamageModifiers m_damages;

	// Token: 0x04000BBE RID: 3006
	public float m_minDamageTreshold;

	// Token: 0x04000BBF RID: 3007
	public int m_minToolTier;

	// Token: 0x04000BC0 RID: 3008
	public float m_hitNoise;

	// Token: 0x04000BC1 RID: 3009
	public float m_destroyNoise;

	// Token: 0x04000BC2 RID: 3010
	public float m_ttl;

	// Token: 0x04000BC3 RID: 3011
	public GameObject m_spawnWhenDestroyed;

	// Token: 0x04000BC4 RID: 3012
	[Header("Effects")]
	public EffectList m_destroyedEffect = new EffectList();

	// Token: 0x04000BC5 RID: 3013
	public EffectList m_hitEffect = new EffectList();

	// Token: 0x04000BC6 RID: 3014
	public bool m_autoCreateFragments;

	// Token: 0x04000BC7 RID: 3015
	private ZNetView m_nview;

	// Token: 0x04000BC8 RID: 3016
	private Rigidbody m_body;

	// Token: 0x04000BC9 RID: 3017
	private bool m_firstFrame = true;

	// Token: 0x04000BCA RID: 3018
	private bool m_destroyed;
}
