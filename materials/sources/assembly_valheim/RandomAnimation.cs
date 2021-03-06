﻿using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000012 RID: 18
public class RandomAnimation : MonoBehaviour
{
	// Token: 0x06000264 RID: 612 RVA: 0x000136D4 File Offset: 0x000118D4
	private void Start()
	{
		this.m_anim = base.GetComponentInChildren<Animator>();
		this.m_nview = base.GetComponent<ZNetView>();
	}

	// Token: 0x06000265 RID: 613 RVA: 0x000136F0 File Offset: 0x000118F0
	private void FixedUpdate()
	{
		if (this.m_nview != null && !this.m_nview.IsValid())
		{
			return;
		}
		float fixedDeltaTime = Time.fixedDeltaTime;
		for (int i = 0; i < this.m_values.Count; i++)
		{
			RandomAnimation.RandomValue randomValue = this.m_values[i];
			if (this.m_nview == null || this.m_nview.IsOwner())
			{
				randomValue.m_timer += fixedDeltaTime;
				if (randomValue.m_timer > randomValue.m_interval)
				{
					randomValue.m_timer = 0f;
					randomValue.m_value = UnityEngine.Random.Range(0, randomValue.m_values);
					if (this.m_nview)
					{
						this.m_nview.GetZDO().Set("RA_" + randomValue.m_name, randomValue.m_value);
					}
					if (!randomValue.m_floatValue)
					{
						this.m_anim.SetInteger(randomValue.m_name, randomValue.m_value);
					}
				}
			}
			if (this.m_nview && !this.m_nview.IsOwner())
			{
				int @int = this.m_nview.GetZDO().GetInt("RA_" + randomValue.m_name, 0);
				if (@int != randomValue.m_value)
				{
					randomValue.m_value = @int;
					if (!randomValue.m_floatValue)
					{
						this.m_anim.SetInteger(randomValue.m_name, randomValue.m_value);
					}
				}
			}
			if (randomValue.m_floatValue)
			{
				if (randomValue.m_hashValues == null || randomValue.m_hashValues.Length != randomValue.m_values)
				{
					randomValue.m_hashValues = new int[randomValue.m_values];
					for (int j = 0; j < randomValue.m_values; j++)
					{
						randomValue.m_hashValues[j] = Animator.StringToHash(randomValue.m_name + j.ToString());
					}
				}
				for (int k = 0; k < randomValue.m_values; k++)
				{
					float num = this.m_anim.GetFloat(randomValue.m_hashValues[k]);
					if (k == randomValue.m_value)
					{
						num = Mathf.MoveTowards(num, 1f, fixedDeltaTime / randomValue.m_floatTransition);
					}
					else
					{
						num = Mathf.MoveTowards(num, 0f, fixedDeltaTime / randomValue.m_floatTransition);
					}
					this.m_anim.SetFloat(randomValue.m_hashValues[k], num);
				}
			}
		}
	}

	// Token: 0x040001D7 RID: 471
	public List<RandomAnimation.RandomValue> m_values = new List<RandomAnimation.RandomValue>();

	// Token: 0x040001D8 RID: 472
	private Animator m_anim;

	// Token: 0x040001D9 RID: 473
	private ZNetView m_nview;

	// Token: 0x02000129 RID: 297
	[Serializable]
	public class RandomValue
	{
		// Token: 0x04001000 RID: 4096
		public string m_name;

		// Token: 0x04001001 RID: 4097
		public int m_values;

		// Token: 0x04001002 RID: 4098
		public float m_interval;

		// Token: 0x04001003 RID: 4099
		public bool m_floatValue;

		// Token: 0x04001004 RID: 4100
		public float m_floatTransition = 1f;

		// Token: 0x04001005 RID: 4101
		[NonSerialized]
		public float m_timer;

		// Token: 0x04001006 RID: 4102
		[NonSerialized]
		public int m_value;

		// Token: 0x04001007 RID: 4103
		[NonSerialized]
		public int[] m_hashValues;
	}
}
