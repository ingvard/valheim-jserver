using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200010B RID: 267
public class TreeLog : MonoBehaviour, IDestructible
{
	// Token: 0x06000FC9 RID: 4041 RVA: 0x0006F148 File Offset: 0x0006D348
	private void Awake()
	{
		this.m_body = base.GetComponent<Rigidbody>();
		this.m_body.maxDepenetrationVelocity = 1f;
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_nview.Register<HitData>("Damage", new Action<long, HitData>(this.RPC_Damage));
		if (this.m_nview.IsOwner())
		{
			float @float = this.m_nview.GetZDO().GetFloat("health", -1f);
			if (@float == -1f)
			{
				this.m_nview.GetZDO().Set("health", this.m_health);
			}
			else if (@float <= 0f)
			{
				this.m_nview.Destroy();
			}
		}
		base.Invoke("EnableDamage", 0.2f);
	}

	// Token: 0x06000FCA RID: 4042 RVA: 0x0006F209 File Offset: 0x0006D409
	private void EnableDamage()
	{
		this.m_firstFrame = false;
	}

	// Token: 0x06000FCB RID: 4043 RVA: 0x0006ED3B File Offset: 0x0006CF3B
	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Tree;
	}

	// Token: 0x06000FCC RID: 4044 RVA: 0x0006F212 File Offset: 0x0006D412
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

	// Token: 0x06000FCD RID: 4045 RVA: 0x0006F248 File Offset: 0x0006D448
	private void RPC_Damage(long sender, HitData hit)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		float num = this.m_nview.GetZDO().GetFloat("health", 0f);
		if (num <= 0f)
		{
			return;
		}
		HitData.DamageModifier type;
		hit.ApplyResistance(this.m_damages, out type);
		float totalDamage = hit.GetTotalDamage();
		if (hit.m_toolTier < this.m_minToolTier)
		{
			DamageText.instance.ShowText(DamageText.TextType.TooHard, hit.m_point, 0f, false);
			return;
		}
		if (this.m_body)
		{
			this.m_body.AddForceAtPosition(hit.m_dir * hit.m_pushForce * 2f, hit.m_point, ForceMode.Impulse);
		}
		DamageText.instance.ShowText(type, hit.m_point, totalDamage, false);
		if (totalDamage <= 0f)
		{
			return;
		}
		num -= totalDamage;
		if (num < 0f)
		{
			num = 0f;
		}
		this.m_nview.GetZDO().Set("health", num);
		this.m_hitEffect.Create(hit.m_point, Quaternion.identity, base.transform, 1f);
		if (this.m_hitNoise > 0f)
		{
			Player closestPlayer = Player.GetClosestPlayer(base.transform.position, 10f);
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

	// Token: 0x06000FCE RID: 4046 RVA: 0x0006F3A8 File Offset: 0x0006D5A8
	private void Destroy()
	{
		ZNetScene.instance.Destroy(base.gameObject);
		this.m_destroyedEffect.Create(base.transform.position, base.transform.rotation, base.transform, 1f);
		List<GameObject> dropList = this.m_dropWhenDestroyed.GetDropList();
		for (int i = 0; i < dropList.Count; i++)
		{
			Vector3 position = base.transform.position + base.transform.up * UnityEngine.Random.Range(-this.m_spawnDistance, this.m_spawnDistance) + Vector3.up * 0.3f * (float)i;
			Quaternion rotation = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 360), 0f);
			UnityEngine.Object.Instantiate<GameObject>(dropList[i], position, rotation);
		}
		if (this.m_subLogPrefab != null)
		{
			foreach (Transform transform in this.m_subLogPoints)
			{
				UnityEngine.Object.Instantiate<GameObject>(this.m_subLogPrefab, transform.position, base.transform.rotation).GetComponent<ZNetView>().SetLocalScale(base.transform.localScale);
			}
		}
	}

	// Token: 0x04000E97 RID: 3735
	public float m_health = 60f;

	// Token: 0x04000E98 RID: 3736
	public HitData.DamageModifiers m_damages;

	// Token: 0x04000E99 RID: 3737
	public int m_minToolTier;

	// Token: 0x04000E9A RID: 3738
	public EffectList m_destroyedEffect = new EffectList();

	// Token: 0x04000E9B RID: 3739
	public EffectList m_hitEffect = new EffectList();

	// Token: 0x04000E9C RID: 3740
	public DropTable m_dropWhenDestroyed = new DropTable();

	// Token: 0x04000E9D RID: 3741
	public GameObject m_subLogPrefab;

	// Token: 0x04000E9E RID: 3742
	public Transform[] m_subLogPoints = new Transform[0];

	// Token: 0x04000E9F RID: 3743
	public float m_spawnDistance = 2f;

	// Token: 0x04000EA0 RID: 3744
	public float m_hitNoise = 100f;

	// Token: 0x04000EA1 RID: 3745
	private Rigidbody m_body;

	// Token: 0x04000EA2 RID: 3746
	private ZNetView m_nview;

	// Token: 0x04000EA3 RID: 3747
	private bool m_firstFrame = true;
}
