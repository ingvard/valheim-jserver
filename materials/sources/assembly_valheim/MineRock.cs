using System;
using UnityEngine;

// Token: 0x020000E1 RID: 225
public class MineRock : MonoBehaviour, IDestructible, Hoverable
{
	// Token: 0x06000E0F RID: 3599 RVA: 0x00064258 File Offset: 0x00062458
	private void Start()
	{
		this.m_hitAreas = ((this.m_areaRoot != null) ? this.m_areaRoot.GetComponentsInChildren<Collider>() : base.gameObject.GetComponentsInChildren<Collider>());
		if (this.m_baseModel)
		{
			this.m_areaMeshes = new MeshRenderer[this.m_hitAreas.Length][];
			for (int i = 0; i < this.m_hitAreas.Length; i++)
			{
				this.m_areaMeshes[i] = this.m_hitAreas[i].GetComponents<MeshRenderer>();
			}
		}
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview && this.m_nview.GetZDO() != null)
		{
			this.m_nview.Register<HitData, int>("Hit", new Action<long, HitData, int>(this.RPC_Hit));
			this.m_nview.Register<int>("Hide", new Action<long, int>(this.RPC_Hide));
		}
		base.InvokeRepeating("UpdateVisability", UnityEngine.Random.Range(1f, 2f), 10f);
	}

	// Token: 0x06000E10 RID: 3600 RVA: 0x00064356 File Offset: 0x00062556
	public string GetHoverText()
	{
		return Localization.instance.Localize(this.m_name);
	}

	// Token: 0x06000E11 RID: 3601 RVA: 0x00064368 File Offset: 0x00062568
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000E12 RID: 3602 RVA: 0x00064370 File Offset: 0x00062570
	private void UpdateVisability()
	{
		bool flag = false;
		for (int i = 0; i < this.m_hitAreas.Length; i++)
		{
			Collider collider = this.m_hitAreas[i];
			if (collider)
			{
				string name = "Health" + i.ToString();
				bool flag2 = this.m_nview.GetZDO().GetFloat(name, this.m_health) > 0f;
				collider.gameObject.SetActive(flag2);
				if (!flag2)
				{
					flag = true;
				}
			}
		}
		if (this.m_baseModel)
		{
			this.m_baseModel.SetActive(!flag);
			foreach (MeshRenderer[] array in this.m_areaMeshes)
			{
				for (int k = 0; k < array.Length; k++)
				{
					array[k].enabled = flag;
				}
			}
		}
	}

	// Token: 0x06000E13 RID: 3603 RVA: 0x000027E2 File Offset: 0x000009E2
	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Default;
	}

	// Token: 0x06000E14 RID: 3604 RVA: 0x00064444 File Offset: 0x00062644
	public void Damage(HitData hit)
	{
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
		ZLog.Log("Hit mine rock area " + areaIndex);
		this.m_nview.InvokeRPC("Hit", new object[]
		{
			hit,
			areaIndex
		});
	}

	// Token: 0x06000E15 RID: 3605 RVA: 0x000644D0 File Offset: 0x000626D0
	private void RPC_Hit(long sender, HitData hit, int hitAreaIndex)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		Collider hitArea = this.GetHitArea(hitAreaIndex);
		if (hitArea == null)
		{
			ZLog.Log("Missing hit area " + hitAreaIndex);
			return;
		}
		string name = "Health" + hitAreaIndex.ToString();
		float num = this.m_nview.GetZDO().GetFloat(name, this.m_health);
		if (num <= 0f)
		{
			ZLog.Log("Already destroyed");
			return;
		}
		HitData.DamageModifier type;
		hit.ApplyResistance(this.m_damageModifiers, out type);
		float totalDamage = hit.GetTotalDamage();
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
		this.m_nview.GetZDO().Set(name, num);
		this.m_hitEffect.Create(hit.m_point, Quaternion.identity, null, 1f);
		Player closestPlayer = Player.GetClosestPlayer(base.transform.position, 10f);
		if (closestPlayer)
		{
			closestPlayer.AddNoise(100f);
		}
		if (this.m_onHit != null)
		{
			this.m_onHit();
		}
		if (num <= 0f)
		{
			this.m_destroyedEffect.Create(hitArea.bounds.center, Quaternion.identity, null, 1f);
			this.m_nview.InvokeRPC(ZNetView.Everybody, "Hide", new object[]
			{
				hitAreaIndex
			});
			foreach (GameObject original in this.m_dropItems.GetDropList())
			{
				Vector3 position = hit.m_point - hit.m_dir * 0.2f + UnityEngine.Random.insideUnitSphere * 0.3f;
				UnityEngine.Object.Instantiate<GameObject>(original, position, Quaternion.identity);
			}
			if (this.m_removeWhenDestroyed && this.AllDestroyed())
			{
				this.m_nview.Destroy();
			}
		}
	}

	// Token: 0x06000E16 RID: 3606 RVA: 0x00064710 File Offset: 0x00062910
	private bool AllDestroyed()
	{
		for (int i = 0; i < this.m_hitAreas.Length; i++)
		{
			string name = "Health" + i.ToString();
			if (this.m_nview.GetZDO().GetFloat(name, this.m_health) > 0f)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000E17 RID: 3607 RVA: 0x00064764 File Offset: 0x00062964
	private void RPC_Hide(long sender, int index)
	{
		Collider hitArea = this.GetHitArea(index);
		if (hitArea)
		{
			hitArea.gameObject.SetActive(false);
		}
		if (this.m_baseModel && this.m_baseModel.activeSelf)
		{
			this.m_baseModel.SetActive(false);
			foreach (MeshRenderer[] array in this.m_areaMeshes)
			{
				for (int j = 0; j < array.Length; j++)
				{
					array[j].enabled = true;
				}
			}
		}
	}

	// Token: 0x06000E18 RID: 3608 RVA: 0x000647E8 File Offset: 0x000629E8
	private int GetAreaIndex(Collider area)
	{
		for (int i = 0; i < this.m_hitAreas.Length; i++)
		{
			if (this.m_hitAreas[i] == area)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x06000E19 RID: 3609 RVA: 0x0006481B File Offset: 0x00062A1B
	private Collider GetHitArea(int index)
	{
		if (index < 0 || index >= this.m_hitAreas.Length)
		{
			return null;
		}
		return this.m_hitAreas[index];
	}

	// Token: 0x04000CC4 RID: 3268
	public string m_name = "";

	// Token: 0x04000CC5 RID: 3269
	public float m_health = 2f;

	// Token: 0x04000CC6 RID: 3270
	public bool m_removeWhenDestroyed = true;

	// Token: 0x04000CC7 RID: 3271
	public HitData.DamageModifiers m_damageModifiers;

	// Token: 0x04000CC8 RID: 3272
	public int m_minToolTier;

	// Token: 0x04000CC9 RID: 3273
	public GameObject m_areaRoot;

	// Token: 0x04000CCA RID: 3274
	public GameObject m_baseModel;

	// Token: 0x04000CCB RID: 3275
	public EffectList m_destroyedEffect = new EffectList();

	// Token: 0x04000CCC RID: 3276
	public EffectList m_hitEffect = new EffectList();

	// Token: 0x04000CCD RID: 3277
	public DropTable m_dropItems;

	// Token: 0x04000CCE RID: 3278
	public Action m_onHit;

	// Token: 0x04000CCF RID: 3279
	private Collider[] m_hitAreas;

	// Token: 0x04000CD0 RID: 3280
	private MeshRenderer[][] m_areaMeshes;

	// Token: 0x04000CD1 RID: 3281
	private ZNetView m_nview;
}
