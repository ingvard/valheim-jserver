using System;
using UnityEngine;

// Token: 0x020000E7 RID: 231
public class Plant : SlowUpdate, Hoverable
{
	// Token: 0x06000E50 RID: 3664 RVA: 0x00066448 File Offset: 0x00064648
	public override void Awake()
	{
		base.Awake();
		this.m_nview = base.gameObject.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		if (this.m_nview.IsOwner() && this.m_nview.GetZDO().GetLong("plantTime", 0L) == 0L)
		{
			this.m_nview.GetZDO().Set("plantTime", ZNet.instance.GetTime().Ticks);
		}
		this.m_spawnTime = Time.time;
	}

	// Token: 0x06000E51 RID: 3665 RVA: 0x000664D4 File Offset: 0x000646D4
	public string GetHoverText()
	{
		switch (this.m_status)
		{
		case Plant.Status.Healthy:
			return Localization.instance.Localize(this.m_name + " ( $piece_plant_healthy )");
		case Plant.Status.NoSun:
			return Localization.instance.Localize(this.m_name + " ( $piece_plant_nosun )");
		case Plant.Status.NoSpace:
			return Localization.instance.Localize(this.m_name + " ( $piece_plant_nospace )");
		case Plant.Status.WrongBiome:
			return Localization.instance.Localize(this.m_name + " ( $piece_plant_wrongbiome )");
		case Plant.Status.NotCultivated:
			return Localization.instance.Localize(this.m_name + " ( $piece_plant_notcultivated )");
		default:
			return "";
		}
	}

	// Token: 0x06000E52 RID: 3666 RVA: 0x00066593 File Offset: 0x00064793
	public string GetHoverName()
	{
		return Localization.instance.Localize(this.m_name);
	}

	// Token: 0x06000E53 RID: 3667 RVA: 0x000665A8 File Offset: 0x000647A8
	private double TimeSincePlanted()
	{
		DateTime d = new DateTime(this.m_nview.GetZDO().GetLong("plantTime", ZNet.instance.GetTime().Ticks));
		return (ZNet.instance.GetTime() - d).TotalSeconds;
	}

	// Token: 0x06000E54 RID: 3668 RVA: 0x000665FC File Offset: 0x000647FC
	public override void SUpdate()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (Time.time - this.m_updateTime < 10f)
		{
			return;
		}
		this.m_updateTime = Time.time;
		double num = this.TimeSincePlanted();
		this.UpdateHealth(num);
		float growTime = this.GetGrowTime();
		if (this.m_healthyGrown)
		{
			bool flag = num > (double)(growTime * 0.5f);
			this.m_healthy.SetActive(!flag && this.m_status == Plant.Status.Healthy);
			this.m_unhealthy.SetActive(!flag && this.m_status > Plant.Status.Healthy);
			this.m_healthyGrown.SetActive(flag && this.m_status == Plant.Status.Healthy);
			this.m_unhealthyGrown.SetActive(flag && this.m_status > Plant.Status.Healthy);
		}
		else
		{
			this.m_healthy.SetActive(this.m_status == Plant.Status.Healthy);
			this.m_unhealthy.SetActive(this.m_status > Plant.Status.Healthy);
		}
		if (this.m_nview.IsOwner() && Time.time - this.m_spawnTime > 10f && num > (double)growTime)
		{
			this.Grow();
		}
	}

	// Token: 0x06000E55 RID: 3669 RVA: 0x00066724 File Offset: 0x00064924
	private float GetGrowTime()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState((int)((ulong)this.m_nview.GetZDO().m_uid.id + (ulong)this.m_nview.GetZDO().m_uid.userID));
		float value = UnityEngine.Random.value;
		UnityEngine.Random.state = state;
		return Mathf.Lerp(this.m_growTime, this.m_growTimeMax, value);
	}

	// Token: 0x06000E56 RID: 3670 RVA: 0x00066788 File Offset: 0x00064988
	private void Grow()
	{
		if (this.m_status != Plant.Status.Healthy)
		{
			if (this.m_destroyIfCantGrow)
			{
				this.Destroy();
			}
			return;
		}
		GameObject original = this.m_grownPrefabs[UnityEngine.Random.Range(0, this.m_grownPrefabs.Length)];
		Quaternion quaternion = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original, base.transform.position, quaternion);
		ZNetView component = gameObject.GetComponent<ZNetView>();
		float num = UnityEngine.Random.Range(this.m_minScale, this.m_maxScale);
		component.SetLocalScale(new Vector3(num, num, num));
		TreeBase component2 = gameObject.GetComponent<TreeBase>();
		if (component2)
		{
			component2.Grow();
		}
		this.m_nview.Destroy();
		this.m_growEffect.Create(base.transform.position, quaternion, null, num);
	}

	// Token: 0x06000E57 RID: 3671 RVA: 0x00066850 File Offset: 0x00064A50
	private void UpdateHealth(double timeSincePlanted)
	{
		if (timeSincePlanted < 10.0)
		{
			this.m_status = Plant.Status.Healthy;
			return;
		}
		Heightmap heightmap = Heightmap.FindHeightmap(base.transform.position);
		if (heightmap)
		{
			if ((heightmap.GetBiome(base.transform.position) & this.m_biome) == Heightmap.Biome.None)
			{
				this.m_status = Plant.Status.WrongBiome;
				return;
			}
			if (this.m_needCultivatedGround && !heightmap.IsCultivated(base.transform.position))
			{
				this.m_status = Plant.Status.NotCultivated;
				return;
			}
		}
		if (this.HaveRoof())
		{
			this.m_status = Plant.Status.NoSun;
			return;
		}
		if (!this.HaveGrowSpace())
		{
			this.m_status = Plant.Status.NoSpace;
			return;
		}
		this.m_status = Plant.Status.Healthy;
	}

	// Token: 0x06000E58 RID: 3672 RVA: 0x000668F8 File Offset: 0x00064AF8
	private void Destroy()
	{
		IDestructible component = base.GetComponent<IDestructible>();
		if (component != null)
		{
			HitData hitData = new HitData();
			hitData.m_damage.m_damage = 9999f;
			component.Damage(hitData);
		}
	}

	// Token: 0x06000E59 RID: 3673 RVA: 0x0006692C File Offset: 0x00064B2C
	private bool HaveRoof()
	{
		if (Plant.m_roofMask == 0)
		{
			Plant.m_roofMask = LayerMask.GetMask(new string[]
			{
				"Default",
				"static_solid",
				"piece"
			});
		}
		return Physics.Raycast(base.transform.position, Vector3.up, 100f, Plant.m_roofMask);
	}

	// Token: 0x06000E5A RID: 3674 RVA: 0x0006698C File Offset: 0x00064B8C
	private bool HaveGrowSpace()
	{
		if (Plant.m_spaceMask == 0)
		{
			Plant.m_spaceMask = LayerMask.GetMask(new string[]
			{
				"Default",
				"static_solid",
				"Default_small",
				"piece",
				"piece_nonsolid"
			});
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, this.m_growRadius, Plant.m_spaceMask);
		for (int i = 0; i < array.Length; i++)
		{
			Plant component = array[i].GetComponent<Plant>();
			if (!component || (!(component == this) && component.GetStatus() == Plant.Status.Healthy))
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000E5B RID: 3675 RVA: 0x00066A27 File Offset: 0x00064C27
	private Plant.Status GetStatus()
	{
		return this.m_status;
	}

	// Token: 0x04000D2A RID: 3370
	public string m_name = "Plant";

	// Token: 0x04000D2B RID: 3371
	public float m_growTime = 10f;

	// Token: 0x04000D2C RID: 3372
	public float m_growTimeMax = 2000f;

	// Token: 0x04000D2D RID: 3373
	public GameObject[] m_grownPrefabs = new GameObject[0];

	// Token: 0x04000D2E RID: 3374
	public float m_minScale = 1f;

	// Token: 0x04000D2F RID: 3375
	public float m_maxScale = 1f;

	// Token: 0x04000D30 RID: 3376
	public float m_growRadius = 1f;

	// Token: 0x04000D31 RID: 3377
	public bool m_needCultivatedGround;

	// Token: 0x04000D32 RID: 3378
	public bool m_destroyIfCantGrow;

	// Token: 0x04000D33 RID: 3379
	[SerializeField]
	private GameObject m_healthy;

	// Token: 0x04000D34 RID: 3380
	[SerializeField]
	private GameObject m_unhealthy;

	// Token: 0x04000D35 RID: 3381
	[SerializeField]
	private GameObject m_healthyGrown;

	// Token: 0x04000D36 RID: 3382
	[SerializeField]
	private GameObject m_unhealthyGrown;

	// Token: 0x04000D37 RID: 3383
	[BitMask(typeof(Heightmap.Biome))]
	public Heightmap.Biome m_biome;

	// Token: 0x04000D38 RID: 3384
	public EffectList m_growEffect = new EffectList();

	// Token: 0x04000D39 RID: 3385
	private Plant.Status m_status;

	// Token: 0x04000D3A RID: 3386
	private ZNetView m_nview;

	// Token: 0x04000D3B RID: 3387
	private float m_updateTime;

	// Token: 0x04000D3C RID: 3388
	private float m_spawnTime;

	// Token: 0x04000D3D RID: 3389
	private static int m_spaceMask;

	// Token: 0x04000D3E RID: 3390
	private static int m_roofMask;

	// Token: 0x020001A6 RID: 422
	private enum Status
	{
		// Token: 0x040012F2 RID: 4850
		Healthy,
		// Token: 0x040012F3 RID: 4851
		NoSun,
		// Token: 0x040012F4 RID: 4852
		NoSpace,
		// Token: 0x040012F5 RID: 4853
		WrongBiome,
		// Token: 0x040012F6 RID: 4854
		NotCultivated
	}
}
