﻿using System;
using UnityEngine;

// Token: 0x02000010 RID: 16
public class Procreation : MonoBehaviour
{
	// Token: 0x06000251 RID: 593 RVA: 0x00012CF8 File Offset: 0x00010EF8
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_baseAI = base.GetComponent<BaseAI>();
		this.m_character = base.GetComponent<Character>();
		this.m_tameable = base.GetComponent<Tameable>();
		base.InvokeRepeating("Procreate", UnityEngine.Random.Range(this.m_updateInterval, this.m_updateInterval + this.m_updateInterval * 0.5f), this.m_updateInterval);
	}

	// Token: 0x06000252 RID: 594 RVA: 0x00012D64 File Offset: 0x00010F64
	private void Procreate()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		if (!this.m_character.IsTamed())
		{
			return;
		}
		if (this.m_offspringPrefab == null)
		{
			string prefabName = ZNetView.GetPrefabName(this.m_offspring);
			this.m_offspringPrefab = ZNetScene.instance.GetPrefab(prefabName);
			int prefab = this.m_nview.GetZDO().GetPrefab();
			this.m_myPrefab = ZNetScene.instance.GetPrefab(prefab);
		}
		if (this.IsPregnant())
		{
			if (this.IsDue())
			{
				this.ResetPregnancy();
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_offspringPrefab, base.transform.position - base.transform.forward * this.m_spawnOffset, Quaternion.LookRotation(-base.transform.forward, Vector3.up));
				Character component = gameObject.GetComponent<Character>();
				if (component)
				{
					component.SetTamed(this.m_character.IsTamed());
					component.SetLevel(Mathf.Max(this.m_minOffspringLevel, this.m_character.GetLevel()));
				}
				this.m_birthEffects.Create(gameObject.transform.position, Quaternion.identity, null, 1f);
				return;
			}
		}
		else
		{
			if (UnityEngine.Random.value <= this.m_pregnancyChance)
			{
				return;
			}
			if (this.m_baseAI.IsAlerted())
			{
				return;
			}
			if (this.m_tameable.IsHungry())
			{
				return;
			}
			int nrOfInstances = SpawnSystem.GetNrOfInstances(this.m_myPrefab, base.transform.position, this.m_totalCheckRange, false, false);
			int nrOfInstances2 = SpawnSystem.GetNrOfInstances(this.m_offspringPrefab, base.transform.position, this.m_totalCheckRange, false, false);
			if (nrOfInstances + nrOfInstances2 >= this.m_maxCreatures)
			{
				return;
			}
			if (SpawnSystem.GetNrOfInstances(this.m_myPrefab, base.transform.position, this.m_partnerCheckRange, false, true) < 2)
			{
				return;
			}
			this.m_loveEffects.Create(base.transform.position, Quaternion.identity, null, 1f);
			int num = this.m_nview.GetZDO().GetInt("lovePoints", 0);
			num++;
			this.m_nview.GetZDO().Set("lovePoints", num);
			if (num >= this.m_requiredLovePoints)
			{
				this.m_nview.GetZDO().Set("lovePoints", 0);
				this.MakePregnant();
			}
		}
	}

	// Token: 0x06000253 RID: 595 RVA: 0x00012FC1 File Offset: 0x000111C1
	public bool ReadyForProcreation()
	{
		return this.m_character.IsTamed() && !this.IsPregnant() && !this.m_tameable.IsHungry();
	}

	// Token: 0x06000254 RID: 596 RVA: 0x00012FE8 File Offset: 0x000111E8
	private void MakePregnant()
	{
		this.m_nview.GetZDO().Set("pregnant", ZNet.instance.GetTime().Ticks);
	}

	// Token: 0x06000255 RID: 597 RVA: 0x0001301C File Offset: 0x0001121C
	private void ResetPregnancy()
	{
		this.m_nview.GetZDO().Set("pregnant", 0L);
	}

	// Token: 0x06000256 RID: 598 RVA: 0x00013038 File Offset: 0x00011238
	private bool IsDue()
	{
		long @long = this.m_nview.GetZDO().GetLong("pregnant", 0L);
		if (@long == 0L)
		{
			return false;
		}
		DateTime d = new DateTime(@long);
		return (ZNet.instance.GetTime() - d).TotalSeconds > (double)this.m_pregnancyDuration;
	}

	// Token: 0x06000257 RID: 599 RVA: 0x0001308B File Offset: 0x0001128B
	public bool IsPregnant()
	{
		return this.m_nview.IsValid() && this.m_nview.GetZDO().GetLong("pregnant", 0L) != 0L;
	}

	// Token: 0x040001B8 RID: 440
	public float m_updateInterval = 10f;

	// Token: 0x040001B9 RID: 441
	public float m_totalCheckRange = 10f;

	// Token: 0x040001BA RID: 442
	public int m_maxCreatures = 4;

	// Token: 0x040001BB RID: 443
	public float m_partnerCheckRange = 3f;

	// Token: 0x040001BC RID: 444
	public float m_pregnancyChance = 0.5f;

	// Token: 0x040001BD RID: 445
	public float m_pregnancyDuration = 10f;

	// Token: 0x040001BE RID: 446
	public int m_requiredLovePoints = 4;

	// Token: 0x040001BF RID: 447
	public GameObject m_offspring;

	// Token: 0x040001C0 RID: 448
	public int m_minOffspringLevel;

	// Token: 0x040001C1 RID: 449
	public float m_spawnOffset = 2f;

	// Token: 0x040001C2 RID: 450
	public EffectList m_birthEffects = new EffectList();

	// Token: 0x040001C3 RID: 451
	public EffectList m_loveEffects = new EffectList();

	// Token: 0x040001C4 RID: 452
	private GameObject m_myPrefab;

	// Token: 0x040001C5 RID: 453
	private GameObject m_offspringPrefab;

	// Token: 0x040001C6 RID: 454
	private ZNetView m_nview;

	// Token: 0x040001C7 RID: 455
	private BaseAI m_baseAI;

	// Token: 0x040001C8 RID: 456
	private Character m_character;

	// Token: 0x040001C9 RID: 457
	private Tameable m_tameable;
}
