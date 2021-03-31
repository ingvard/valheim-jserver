using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000011 RID: 17
public class Ragdoll : MonoBehaviour
{
	// Token: 0x06000259 RID: 601 RVA: 0x00013134 File Offset: 0x00011334
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_bodies = base.GetComponentsInChildren<Rigidbody>();
		base.Invoke("RemoveInitVel", 2f);
		if (this.m_mainModel)
		{
			float @float = this.m_nview.GetZDO().GetFloat("Hue", 0f);
			float float2 = this.m_nview.GetZDO().GetFloat("Saturation", 0f);
			float float3 = this.m_nview.GetZDO().GetFloat("Value", 0f);
			this.m_mainModel.material.SetFloat("_Hue", @float);
			this.m_mainModel.material.SetFloat("_Saturation", float2);
			this.m_mainModel.material.SetFloat("_Value", float3);
		}
		base.InvokeRepeating("DestroyNow", this.m_ttl, 1f);
	}

	// Token: 0x0600025A RID: 602 RVA: 0x00013224 File Offset: 0x00011424
	public Vector3 GetAverageBodyPosition()
	{
		if (this.m_bodies.Length == 0)
		{
			return base.transform.position;
		}
		Vector3 a = Vector3.zero;
		foreach (Rigidbody rigidbody in this.m_bodies)
		{
			a += rigidbody.position;
		}
		return a / (float)this.m_bodies.Length;
	}

	// Token: 0x0600025B RID: 603 RVA: 0x00013284 File Offset: 0x00011484
	private void DestroyNow()
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		Vector3 averageBodyPosition = this.GetAverageBodyPosition();
		this.m_removeEffect.Create(averageBodyPosition, Quaternion.identity, null, 1f);
		this.SpawnLoot(averageBodyPosition);
		ZNetScene.instance.Destroy(base.gameObject);
	}

	// Token: 0x0600025C RID: 604 RVA: 0x000132E2 File Offset: 0x000114E2
	private void RemoveInitVel()
	{
		if (this.m_nview.IsOwner())
		{
			this.m_nview.GetZDO().Set("InitVel", Vector3.zero);
		}
	}

	// Token: 0x0600025D RID: 605 RVA: 0x0001330C File Offset: 0x0001150C
	private void Start()
	{
		Vector3 vec = this.m_nview.GetZDO().GetVec3("InitVel", Vector3.zero);
		if (vec != Vector3.zero)
		{
			vec.y = Mathf.Min(vec.y, 4f);
			Rigidbody[] bodies = this.m_bodies;
			for (int i = 0; i < bodies.Length; i++)
			{
				bodies[i].velocity = vec * UnityEngine.Random.value;
			}
		}
	}

	// Token: 0x0600025E RID: 606 RVA: 0x00013380 File Offset: 0x00011580
	public void Setup(Vector3 velocity, float hue, float saturation, float value, CharacterDrop characterDrop)
	{
		velocity.x *= this.m_velMultiplier;
		velocity.z *= this.m_velMultiplier;
		this.m_nview.GetZDO().Set("InitVel", velocity);
		this.m_nview.GetZDO().Set("Hue", hue);
		this.m_nview.GetZDO().Set("Saturation", saturation);
		this.m_nview.GetZDO().Set("Value", value);
		if (this.m_mainModel)
		{
			this.m_mainModel.material.SetFloat("_Hue", hue);
			this.m_mainModel.material.SetFloat("_Saturation", saturation);
			this.m_mainModel.material.SetFloat("_Value", value);
		}
		if (characterDrop)
		{
			this.SaveLootList(characterDrop);
		}
	}

	// Token: 0x0600025F RID: 607 RVA: 0x0001346C File Offset: 0x0001166C
	private void SaveLootList(CharacterDrop characterDrop)
	{
		List<KeyValuePair<GameObject, int>> list = characterDrop.GenerateDropList();
		if (list.Count > 0)
		{
			ZDO zdo = this.m_nview.GetZDO();
			zdo.Set("drops", list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				KeyValuePair<GameObject, int> keyValuePair = list[i];
				int prefabHash = ZNetScene.instance.GetPrefabHash(keyValuePair.Key);
				zdo.Set("drop_hash" + i, prefabHash);
				zdo.Set("drop_amount" + i, keyValuePair.Value);
			}
		}
	}

	// Token: 0x06000260 RID: 608 RVA: 0x00013508 File Offset: 0x00011708
	private void SpawnLoot(Vector3 center)
	{
		ZDO zdo = this.m_nview.GetZDO();
		int @int = zdo.GetInt("drops", 0);
		if (@int <= 0)
		{
			return;
		}
		List<KeyValuePair<GameObject, int>> list = new List<KeyValuePair<GameObject, int>>();
		for (int i = 0; i < @int; i++)
		{
			int int2 = zdo.GetInt("drop_hash" + i, 0);
			int int3 = zdo.GetInt("drop_amount" + i, 0);
			GameObject prefab = ZNetScene.instance.GetPrefab(int2);
			if (prefab == null)
			{
				ZLog.LogWarning("Ragdoll: Missing prefab:" + int2 + " when dropping loot");
			}
			else
			{
				list.Add(new KeyValuePair<GameObject, int>(prefab, int3));
			}
		}
		CharacterDrop.DropItems(list, center + Vector3.up * 0.75f, 0.5f);
	}

	// Token: 0x06000261 RID: 609 RVA: 0x000135DB File Offset: 0x000117DB
	private void FixedUpdate()
	{
		if (this.m_float)
		{
			this.UpdateFloating(Time.fixedDeltaTime);
		}
	}

	// Token: 0x06000262 RID: 610 RVA: 0x000135F0 File Offset: 0x000117F0
	private void UpdateFloating(float dt)
	{
		foreach (Rigidbody rigidbody in this.m_bodies)
		{
			Vector3 worldCenterOfMass = rigidbody.worldCenterOfMass;
			worldCenterOfMass.y += this.m_floatOffset;
			float waterLevel = WaterVolume.GetWaterLevel(worldCenterOfMass, 1f);
			if (worldCenterOfMass.y < waterLevel)
			{
				float d = (waterLevel - worldCenterOfMass.y) / 0.5f;
				Vector3 a = Vector3.up * 20f * d;
				rigidbody.AddForce(a * dt, ForceMode.VelocityChange);
				rigidbody.velocity -= rigidbody.velocity * 0.05f * d;
			}
		}
	}

	// Token: 0x040001CA RID: 458
	public float m_velMultiplier = 1f;

	// Token: 0x040001CB RID: 459
	public float m_ttl;

	// Token: 0x040001CC RID: 460
	public Renderer m_mainModel;

	// Token: 0x040001CD RID: 461
	public EffectList m_removeEffect = new EffectList();

	// Token: 0x040001CE RID: 462
	public Action<Vector3> m_onDestroyed;

	// Token: 0x040001CF RID: 463
	public bool m_float;

	// Token: 0x040001D0 RID: 464
	public float m_floatOffset = -0.1f;

	// Token: 0x040001D1 RID: 465
	private const float m_floatForce = 20f;

	// Token: 0x040001D2 RID: 466
	private const float m_damping = 0.05f;

	// Token: 0x040001D3 RID: 467
	private ZNetView m_nview;

	// Token: 0x040001D4 RID: 468
	private Rigidbody[] m_bodies;

	// Token: 0x040001D5 RID: 469
	private const float m_dropOffset = 0.75f;

	// Token: 0x040001D6 RID: 470
	private const float m_dropArea = 0.5f;
}
