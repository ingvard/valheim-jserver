using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x02000112 RID: 274
public class WearNTear : MonoBehaviour, IDestructible
{
	// Token: 0x06001013 RID: 4115 RVA: 0x0007113C File Offset: 0x0006F33C
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		this.m_piece = base.GetComponent<Piece>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.m_nview.Register<HitData>("WNTDamage", new Action<long, HitData>(this.RPC_Damage));
		this.m_nview.Register("WNTRemove", new Action<long>(this.RPC_Remove));
		this.m_nview.Register("WNTRepair", new Action<long>(this.RPC_Repair));
		this.m_nview.Register<float>("WNTHealthChanged", new Action<long, float>(this.RPC_HealthChanged));
		if (this.m_autoCreateFragments)
		{
			this.m_nview.Register("WNTCreateFragments", new Action<long>(this.RPC_CreateFragments));
		}
		if (WearNTear.m_rayMask == 0)
		{
			WearNTear.m_rayMask = LayerMask.GetMask(new string[]
			{
				"piece",
				"Default",
				"static_solid",
				"Default_small",
				"terrain"
			});
		}
		WearNTear.m_allInstances.Add(this);
		this.m_myIndex = WearNTear.m_allInstances.Count - 1;
		this.m_createTime = Time.time;
		this.m_support = this.GetMaxSupport();
		if (WearNTear.m_randomInitialDamage)
		{
			float value = UnityEngine.Random.Range(0.1f * this.m_health, this.m_health * 0.6f);
			this.m_nview.GetZDO().Set("health", value);
		}
		this.UpdateVisual(false);
	}

	// Token: 0x06001014 RID: 4116 RVA: 0x000712B8 File Offset: 0x0006F4B8
	private void OnDestroy()
	{
		if (this.m_myIndex != -1)
		{
			WearNTear.m_allInstances[this.m_myIndex] = WearNTear.m_allInstances[WearNTear.m_allInstances.Count - 1];
			WearNTear.m_allInstances[this.m_myIndex].m_myIndex = this.m_myIndex;
			WearNTear.m_allInstances.RemoveAt(WearNTear.m_allInstances.Count - 1);
		}
	}

	// Token: 0x06001015 RID: 4117 RVA: 0x00071328 File Offset: 0x0006F528
	public bool Repair()
	{
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		if (this.m_nview.GetZDO().GetFloat("health", this.m_health) >= this.m_health)
		{
			return false;
		}
		if (Time.time - this.m_lastRepair < 1f)
		{
			return false;
		}
		this.m_lastRepair = Time.time;
		this.m_nview.InvokeRPC("WNTRepair", Array.Empty<object>());
		return true;
	}

	// Token: 0x06001016 RID: 4118 RVA: 0x000713A0 File Offset: 0x0006F5A0
	private void RPC_Repair(long sender)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		this.m_nview.GetZDO().Set("health", this.m_health);
		this.m_nview.InvokeRPC(ZNetView.Everybody, "WNTHealthChanged", new object[]
		{
			this.m_health
		});
	}

	// Token: 0x06001017 RID: 4119 RVA: 0x0007140C File Offset: 0x0006F60C
	private float GetSupport()
	{
		if (!this.m_nview.IsValid())
		{
			return this.GetMaxSupport();
		}
		if (!this.m_nview.HasOwner())
		{
			return this.GetMaxSupport();
		}
		if (this.m_nview.IsOwner())
		{
			return this.m_support;
		}
		return this.m_nview.GetZDO().GetFloat("support", this.GetMaxSupport());
	}

	// Token: 0x06001018 RID: 4120 RVA: 0x00071470 File Offset: 0x0006F670
	private float GetSupportColorValue()
	{
		float num = this.GetSupport();
		float num2;
		float num3;
		float num4;
		float num5;
		this.GetMaterialProperties(out num2, out num3, out num4, out num5);
		if (num >= num2)
		{
			return -1f;
		}
		num -= num3;
		return Mathf.Clamp01(num / (num2 * 0.5f - num3));
	}

	// Token: 0x06001019 RID: 4121 RVA: 0x000714B0 File Offset: 0x0006F6B0
	public void OnPlaced()
	{
		this.m_createTime = -1f;
	}

	// Token: 0x0600101A RID: 4122 RVA: 0x000714C0 File Offset: 0x0006F6C0
	private List<Renderer> GetHighlightRenderers()
	{
		MeshRenderer[] componentsInChildren = base.GetComponentsInChildren<MeshRenderer>(true);
		SkinnedMeshRenderer[] componentsInChildren2 = base.GetComponentsInChildren<SkinnedMeshRenderer>(true);
		List<Renderer> list = new List<Renderer>();
		list.AddRange(componentsInChildren);
		list.AddRange(componentsInChildren2);
		return list;
	}

	// Token: 0x0600101B RID: 4123 RVA: 0x000714F0 File Offset: 0x0006F6F0
	public void Highlight()
	{
		if (this.m_oldMaterials == null)
		{
			this.m_oldMaterials = new List<WearNTear.OldMeshData>();
			foreach (Renderer renderer in this.GetHighlightRenderers())
			{
				WearNTear.OldMeshData oldMeshData = default(WearNTear.OldMeshData);
				oldMeshData.m_materials = renderer.sharedMaterials;
				oldMeshData.m_color = new Color[oldMeshData.m_materials.Length];
				oldMeshData.m_emissiveColor = new Color[oldMeshData.m_materials.Length];
				for (int i = 0; i < oldMeshData.m_materials.Length; i++)
				{
					if (oldMeshData.m_materials[i].HasProperty("_Color"))
					{
						oldMeshData.m_color[i] = oldMeshData.m_materials[i].GetColor("_Color");
					}
					if (oldMeshData.m_materials[i].HasProperty("_EmissionColor"))
					{
						oldMeshData.m_emissiveColor[i] = oldMeshData.m_materials[i].GetColor("_EmissionColor");
					}
				}
				oldMeshData.m_renderer = renderer;
				this.m_oldMaterials.Add(oldMeshData);
			}
		}
		float supportColorValue = this.GetSupportColorValue();
		Color color = new Color(0.6f, 0.8f, 1f);
		if (supportColorValue >= 0f)
		{
			color = Color.Lerp(new Color(1f, 0f, 0f), new Color(0f, 1f, 0f), supportColorValue);
			float h;
			float s;
			float v;
			Color.RGBToHSV(color, out h, out s, out v);
			s = Mathf.Lerp(1f, 0.5f, supportColorValue);
			v = Mathf.Lerp(1.2f, 0.9f, supportColorValue);
			color = Color.HSVToRGB(h, s, v);
		}
		foreach (WearNTear.OldMeshData oldMeshData2 in this.m_oldMaterials)
		{
			if (oldMeshData2.m_renderer)
			{
				foreach (Material material in oldMeshData2.m_renderer.materials)
				{
					material.SetColor("_EmissionColor", color * 0.4f);
					material.color = color;
				}
			}
		}
		base.CancelInvoke("ResetHighlight");
		base.Invoke("ResetHighlight", 0.2f);
	}

	// Token: 0x0600101C RID: 4124 RVA: 0x00071770 File Offset: 0x0006F970
	private void ResetHighlight()
	{
		if (this.m_oldMaterials != null)
		{
			foreach (WearNTear.OldMeshData oldMeshData in this.m_oldMaterials)
			{
				if (oldMeshData.m_renderer)
				{
					Material[] materials = oldMeshData.m_renderer.materials;
					if (materials.Length != 0)
					{
						if (materials[0] == oldMeshData.m_materials[0])
						{
							if (materials.Length == oldMeshData.m_color.Length)
							{
								for (int i = 0; i < materials.Length; i++)
								{
									if (materials[i].HasProperty("_Color"))
									{
										materials[i].SetColor("_Color", oldMeshData.m_color[i]);
									}
									if (materials[i].HasProperty("_EmissionColor"))
									{
										materials[i].SetColor("_EmissionColor", oldMeshData.m_emissiveColor[i]);
									}
								}
							}
						}
						else if (materials.Length == oldMeshData.m_materials.Length)
						{
							oldMeshData.m_renderer.materials = oldMeshData.m_materials;
						}
					}
				}
			}
			this.m_oldMaterials = null;
		}
	}

	// Token: 0x0600101D RID: 4125 RVA: 0x00071894 File Offset: 0x0006FA94
	private void SetupColliders()
	{
		this.m_colliders = base.GetComponentsInChildren<Collider>(true);
		this.m_bounds = new List<WearNTear.BoundData>();
		foreach (Collider collider in this.m_colliders)
		{
			if (!collider.isTrigger && !(collider.attachedRigidbody != null))
			{
				WearNTear.BoundData item = default(WearNTear.BoundData);
				if (collider is BoxCollider)
				{
					BoxCollider boxCollider = collider as BoxCollider;
					item.m_rot = boxCollider.transform.rotation;
					item.m_pos = boxCollider.transform.position + boxCollider.transform.TransformVector(boxCollider.center);
					item.m_size = new Vector3(boxCollider.transform.lossyScale.x * boxCollider.size.x, boxCollider.transform.lossyScale.y * boxCollider.size.y, boxCollider.transform.lossyScale.z * boxCollider.size.z);
				}
				else
				{
					item.m_rot = Quaternion.identity;
					item.m_pos = collider.bounds.center;
					item.m_size = collider.bounds.size;
				}
				item.m_size.x = item.m_size.x + 0.3f;
				item.m_size.y = item.m_size.y + 0.3f;
				item.m_size.z = item.m_size.z + 0.3f;
				item.m_size *= 0.5f;
				this.m_bounds.Add(item);
			}
		}
	}

	// Token: 0x0600101E RID: 4126 RVA: 0x00071A54 File Offset: 0x0006FC54
	private bool ShouldUpdate()
	{
		return this.m_createTime < 0f || Time.time - this.m_createTime > 30f;
	}

	// Token: 0x0600101F RID: 4127 RVA: 0x00071A78 File Offset: 0x0006FC78
	public void UpdateWear()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		if (this.m_nview.IsOwner() && this.ShouldUpdate())
		{
			if (ZNetScene.instance.OutsideActiveArea(base.transform.position))
			{
				this.m_support = this.GetMaxSupport();
				this.m_nview.GetZDO().Set("support", this.m_support);
				return;
			}
			float num = 0f;
			bool flag = this.HaveRoof();
			bool flag2 = EnvMan.instance.IsWet() && !flag;
			if (this.m_wet)
			{
				this.m_wet.SetActive(flag2);
			}
			if (this.m_noRoofWear && this.GetHealthPercentage() > 0.5f)
			{
				if (flag2 || this.IsUnderWater())
				{
					if (this.m_rainTimer == 0f)
					{
						this.m_rainTimer = Time.time;
					}
					else if (Time.time - this.m_rainTimer > 60f)
					{
						this.m_rainTimer = Time.time;
						num += 5f;
					}
				}
				else
				{
					this.m_rainTimer = 0f;
				}
			}
			if (this.m_noSupportWear)
			{
				this.UpdateSupport();
				if (!this.HaveSupport())
				{
					num = 100f;
				}
			}
			if (num > 0f && !this.CanBeRemoved())
			{
				num = 0f;
			}
			if (num > 0f)
			{
				float damage = num / 100f * this.m_health;
				this.ApplyDamage(damage);
			}
		}
		this.UpdateVisual(true);
	}

	// Token: 0x06001020 RID: 4128 RVA: 0x00071BEC File Offset: 0x0006FDEC
	private Vector3 GetCOM()
	{
		return base.transform.position + base.transform.rotation * this.m_comOffset;
	}

	// Token: 0x06001021 RID: 4129 RVA: 0x00071C14 File Offset: 0x0006FE14
	private void UpdateSupport()
	{
		if (this.m_colliders == null)
		{
			this.SetupColliders();
		}
		float num;
		float num2;
		float num3;
		float num4;
		this.GetMaterialProperties(out num, out num2, out num3, out num4);
		WearNTear.m_tempSupportPoints.Clear();
		WearNTear.m_tempSupportPointValues.Clear();
		Vector3 com = this.GetCOM();
		float a = 0f;
		foreach (WearNTear.BoundData boundData in this.m_bounds)
		{
			int num5 = Physics.OverlapBoxNonAlloc(boundData.m_pos, boundData.m_size, WearNTear.m_tempColliders, boundData.m_rot, WearNTear.m_rayMask);
			for (int i = 0; i < num5; i++)
			{
				Collider collider = WearNTear.m_tempColliders[i];
				if (!this.m_colliders.Contains(collider) && !(collider.attachedRigidbody != null) && !collider.isTrigger)
				{
					WearNTear componentInParent = collider.GetComponentInParent<WearNTear>();
					if (componentInParent == null)
					{
						this.m_support = num;
						this.m_nview.GetZDO().Set("support", this.m_support);
						return;
					}
					if (componentInParent.m_supports)
					{
						float num6 = Vector3.Distance(com, componentInParent.transform.position) + 0.1f;
						float support = componentInParent.GetSupport();
						a = Mathf.Max(a, support - num3 * num6 * support);
						Vector3 vector = this.FindSupportPoint(com, componentInParent, collider);
						if (vector.y < com.y + 0.05f)
						{
							Vector3 normalized = (vector - com).normalized;
							if (normalized.y < 0f)
							{
								float t = Mathf.Acos(1f - Mathf.Abs(normalized.y)) / 1.5707964f;
								float num7 = Mathf.Lerp(num3, num4, t);
								float b = support - num7 * num6 * support;
								a = Mathf.Max(a, b);
							}
							float item = support - num4 * num6 * support;
							WearNTear.m_tempSupportPoints.Add(vector);
							WearNTear.m_tempSupportPointValues.Add(item);
						}
					}
				}
			}
		}
		if (WearNTear.m_tempSupportPoints.Count > 0 && WearNTear.m_tempSupportPoints.Count >= 2)
		{
			for (int j = 0; j < WearNTear.m_tempSupportPoints.Count; j++)
			{
				Vector3 from = WearNTear.m_tempSupportPoints[j] - com;
				from.y = 0f;
				for (int k = 0; k < WearNTear.m_tempSupportPoints.Count; k++)
				{
					if (j != k)
					{
						Vector3 to = WearNTear.m_tempSupportPoints[k] - com;
						to.y = 0f;
						if (Vector3.Angle(from, to) >= 100f)
						{
							float b2 = (WearNTear.m_tempSupportPointValues[j] + WearNTear.m_tempSupportPointValues[k]) * 0.5f;
							a = Mathf.Max(a, b2);
						}
					}
				}
			}
		}
		this.m_support = Mathf.Min(a, num);
		this.m_nview.GetZDO().Set("support", this.m_support);
	}

	// Token: 0x06001022 RID: 4130 RVA: 0x00071F5C File Offset: 0x0007015C
	private Vector3 FindSupportPoint(Vector3 com, WearNTear wnt, Collider otherCollider)
	{
		MeshCollider meshCollider = otherCollider as MeshCollider;
		if (!(meshCollider != null) || meshCollider.convex)
		{
			return otherCollider.ClosestPoint(com);
		}
		RaycastHit raycastHit;
		if (meshCollider.Raycast(new Ray(com, Vector3.down), out raycastHit, 10f))
		{
			return raycastHit.point;
		}
		return (com + wnt.GetCOM()) * 0.5f;
	}

	// Token: 0x06001023 RID: 4131 RVA: 0x00071FC1 File Offset: 0x000701C1
	private bool HaveSupport()
	{
		return this.m_support >= this.GetMinSupport();
	}

	// Token: 0x06001024 RID: 4132 RVA: 0x00071FD4 File Offset: 0x000701D4
	private bool IsUnderWater()
	{
		float waterLevel = WaterVolume.GetWaterLevel(base.transform.position, 1f);
		return base.transform.position.y < waterLevel;
	}

	// Token: 0x06001025 RID: 4133 RVA: 0x0007200C File Offset: 0x0007020C
	private bool HaveRoof()
	{
		int num = Physics.SphereCastNonAlloc(base.transform.position, 0.1f, Vector3.up, WearNTear.m_raycastHits, 100f, WearNTear.m_rayMask);
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = WearNTear.m_raycastHits[i];
			if (!raycastHit.collider.gameObject.CompareTag("leaky"))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06001026 RID: 4134 RVA: 0x00072078 File Offset: 0x00070278
	private void RPC_HealthChanged(long peer, float health)
	{
		float health2 = health / this.m_health;
		this.SetHealthVisual(health2, true);
	}

	// Token: 0x06001027 RID: 4135 RVA: 0x00072096 File Offset: 0x00070296
	private void UpdateVisual(bool triggerEffects)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.SetHealthVisual(this.GetHealthPercentage(), triggerEffects);
	}

	// Token: 0x06001028 RID: 4136 RVA: 0x000720B4 File Offset: 0x000702B4
	private void SetHealthVisual(float health, bool triggerEffects)
	{
		if (this.m_worn == null && this.m_broken == null && this.m_new == null)
		{
			return;
		}
		if (health > 0.75f)
		{
			if (this.m_worn != this.m_new)
			{
				this.m_worn.SetActive(false);
			}
			if (this.m_broken != this.m_new)
			{
				this.m_broken.SetActive(false);
			}
			this.m_new.SetActive(true);
			return;
		}
		if (health > 0.25f)
		{
			if (triggerEffects && !this.m_worn.activeSelf)
			{
				this.m_switchEffect.Create(base.transform.position, base.transform.rotation, base.transform, 1f);
			}
			if (this.m_new != this.m_worn)
			{
				this.m_new.SetActive(false);
			}
			if (this.m_broken != this.m_worn)
			{
				this.m_broken.SetActive(false);
			}
			this.m_worn.SetActive(true);
			return;
		}
		if (triggerEffects && !this.m_broken.activeSelf)
		{
			this.m_switchEffect.Create(base.transform.position, base.transform.rotation, base.transform, 1f);
		}
		if (this.m_new != this.m_broken)
		{
			this.m_new.SetActive(false);
		}
		if (this.m_worn != this.m_broken)
		{
			this.m_worn.SetActive(false);
		}
		this.m_broken.SetActive(true);
	}

	// Token: 0x06001029 RID: 4137 RVA: 0x00072259 File Offset: 0x00070459
	public float GetHealthPercentage()
	{
		if (!this.m_nview.IsValid())
		{
			return 1f;
		}
		return Mathf.Clamp01(this.m_nview.GetZDO().GetFloat("health", this.m_health) / this.m_health);
	}

	// Token: 0x0600102A RID: 4138 RVA: 0x000027E2 File Offset: 0x000009E2
	public DestructibleType GetDestructibleType()
	{
		return DestructibleType.Default;
	}

	// Token: 0x0600102B RID: 4139 RVA: 0x00072295 File Offset: 0x00070495
	public void Damage(HitData hit)
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.m_nview.InvokeRPC("WNTDamage", new object[]
		{
			hit
		});
	}

	// Token: 0x0600102C RID: 4140 RVA: 0x000722BF File Offset: 0x000704BF
	private bool CanBeRemoved()
	{
		return !this.m_piece || this.m_piece.CanBeRemoved();
	}

	// Token: 0x0600102D RID: 4141 RVA: 0x000722DC File Offset: 0x000704DC
	private void RPC_Damage(long sender, HitData hit)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_nview.GetZDO().GetFloat("health", this.m_health) <= 0f)
		{
			return;
		}
		HitData.DamageModifier type;
		hit.ApplyResistance(this.m_damages, out type);
		float totalDamage = hit.GetTotalDamage();
		if (this.m_piece && this.m_piece.IsPlacedByPlayer())
		{
			PrivateArea.CheckInPrivateArea(base.transform.position, true);
		}
		DamageText.instance.ShowText(type, hit.m_point, totalDamage, false);
		if (totalDamage <= 0f)
		{
			return;
		}
		this.ApplyDamage(totalDamage);
		this.m_hitEffect.Create(hit.m_point, Quaternion.identity, base.transform, 1f);
		if (this.m_hitNoise > 0f)
		{
			Player closestPlayer = Player.GetClosestPlayer(hit.m_point, 10f);
			if (closestPlayer)
			{
				closestPlayer.AddNoise(this.m_hitNoise);
			}
		}
		if (this.m_onDamaged != null)
		{
			this.m_onDamaged();
		}
	}

	// Token: 0x0600102E RID: 4142 RVA: 0x000723E8 File Offset: 0x000705E8
	public bool ApplyDamage(float damage)
	{
		float num = this.m_nview.GetZDO().GetFloat("health", this.m_health);
		if (num <= 0f)
		{
			return false;
		}
		num -= damage;
		this.m_nview.GetZDO().Set("health", num);
		if (num <= 0f)
		{
			this.Destroy();
		}
		else
		{
			this.m_nview.InvokeRPC(ZNetView.Everybody, "WNTHealthChanged", new object[]
			{
				num
			});
		}
		return true;
	}

	// Token: 0x0600102F RID: 4143 RVA: 0x0007246A File Offset: 0x0007066A
	public void Remove()
	{
		if (!this.m_nview.IsValid())
		{
			return;
		}
		this.m_nview.InvokeRPC("WNTRemove", Array.Empty<object>());
	}

	// Token: 0x06001030 RID: 4144 RVA: 0x0007248F File Offset: 0x0007068F
	private void RPC_Remove(long sender)
	{
		if (!this.m_nview.IsValid() || !this.m_nview.IsOwner())
		{
			return;
		}
		this.Destroy();
	}

	// Token: 0x06001031 RID: 4145 RVA: 0x000724B4 File Offset: 0x000706B4
	private void Destroy()
	{
		this.m_nview.GetZDO().Set("health", 0f);
		if (this.m_piece)
		{
			this.m_piece.DropResources();
		}
		if (this.m_onDestroyed != null)
		{
			this.m_onDestroyed();
		}
		if (this.m_destroyNoise > 0f)
		{
			Player closestPlayer = Player.GetClosestPlayer(base.transform.position, 10f);
			if (closestPlayer)
			{
				closestPlayer.AddNoise(this.m_destroyNoise);
			}
		}
		this.m_destroyedEffect.Create(base.transform.position, base.transform.rotation, base.transform, 1f);
		if (this.m_autoCreateFragments)
		{
			this.m_nview.InvokeRPC(ZNetView.Everybody, "WNTCreateFragments", Array.Empty<object>());
		}
		ZNetScene.instance.Destroy(base.gameObject);
	}

	// Token: 0x06001032 RID: 4146 RVA: 0x0007259C File Offset: 0x0007079C
	private void RPC_CreateFragments(long peer)
	{
		this.ResetHighlight();
		if (this.m_fragmentRoots != null && this.m_fragmentRoots.Length != 0)
		{
			foreach (GameObject gameObject in this.m_fragmentRoots)
			{
				gameObject.SetActive(true);
				Destructible.CreateFragments(gameObject, false);
			}
			return;
		}
		Destructible.CreateFragments(base.gameObject, true);
	}

	// Token: 0x06001033 RID: 4147 RVA: 0x000725F4 File Offset: 0x000707F4
	private float GetMaxSupport()
	{
		float result;
		float num;
		float num2;
		float num3;
		this.GetMaterialProperties(out result, out num, out num2, out num3);
		return result;
	}

	// Token: 0x06001034 RID: 4148 RVA: 0x00072610 File Offset: 0x00070810
	private float GetMinSupport()
	{
		float num;
		float result;
		float num2;
		float num3;
		this.GetMaterialProperties(out num, out result, out num2, out num3);
		return result;
	}

	// Token: 0x06001035 RID: 4149 RVA: 0x0007262C File Offset: 0x0007082C
	private void GetMaterialProperties(out float maxSupport, out float minSupport, out float horizontalLoss, out float verticalLoss)
	{
		switch (this.m_materialType)
		{
		case WearNTear.MaterialType.Wood:
			maxSupport = 100f;
			minSupport = 10f;
			verticalLoss = 0.125f;
			horizontalLoss = 0.2f;
			return;
		case WearNTear.MaterialType.Stone:
			maxSupport = 1000f;
			minSupport = 100f;
			verticalLoss = 0.125f;
			horizontalLoss = 1f;
			return;
		case WearNTear.MaterialType.Iron:
			maxSupport = 1500f;
			minSupport = 20f;
			verticalLoss = 0.07692308f;
			horizontalLoss = 0.07692308f;
			return;
		case WearNTear.MaterialType.HardWood:
			maxSupport = 140f;
			minSupport = 10f;
			verticalLoss = 0.1f;
			horizontalLoss = 0.16666667f;
			return;
		default:
			maxSupport = 0f;
			minSupport = 0f;
			verticalLoss = 0f;
			horizontalLoss = 0f;
			return;
		}
	}

	// Token: 0x06001036 RID: 4150 RVA: 0x000726ED File Offset: 0x000708ED
	public static List<WearNTear> GetAllInstaces()
	{
		return WearNTear.m_allInstances;
	}

	// Token: 0x04000EF5 RID: 3829
	public static bool m_randomInitialDamage = false;

	// Token: 0x04000EF6 RID: 3830
	public Action m_onDestroyed;

	// Token: 0x04000EF7 RID: 3831
	public Action m_onDamaged;

	// Token: 0x04000EF8 RID: 3832
	[Header("Wear")]
	public GameObject m_new;

	// Token: 0x04000EF9 RID: 3833
	public GameObject m_worn;

	// Token: 0x04000EFA RID: 3834
	public GameObject m_broken;

	// Token: 0x04000EFB RID: 3835
	public GameObject m_wet;

	// Token: 0x04000EFC RID: 3836
	public bool m_noRoofWear = true;

	// Token: 0x04000EFD RID: 3837
	public bool m_noSupportWear = true;

	// Token: 0x04000EFE RID: 3838
	public WearNTear.MaterialType m_materialType;

	// Token: 0x04000EFF RID: 3839
	public bool m_supports = true;

	// Token: 0x04000F00 RID: 3840
	public Vector3 m_comOffset = Vector3.zero;

	// Token: 0x04000F01 RID: 3841
	[Header("Destruction")]
	public float m_health = 100f;

	// Token: 0x04000F02 RID: 3842
	public HitData.DamageModifiers m_damages;

	// Token: 0x04000F03 RID: 3843
	public float m_hitNoise;

	// Token: 0x04000F04 RID: 3844
	public float m_destroyNoise;

	// Token: 0x04000F05 RID: 3845
	[Header("Effects")]
	public EffectList m_destroyedEffect = new EffectList();

	// Token: 0x04000F06 RID: 3846
	public EffectList m_hitEffect = new EffectList();

	// Token: 0x04000F07 RID: 3847
	public EffectList m_switchEffect = new EffectList();

	// Token: 0x04000F08 RID: 3848
	public bool m_autoCreateFragments = true;

	// Token: 0x04000F09 RID: 3849
	public GameObject[] m_fragmentRoots;

	// Token: 0x04000F0A RID: 3850
	private const float m_noFireDrain = 0.0049603176f;

	// Token: 0x04000F0B RID: 3851
	private const float m_noSupportDrain = 25f;

	// Token: 0x04000F0C RID: 3852
	private const float m_rainDamageTime = 60f;

	// Token: 0x04000F0D RID: 3853
	private const float m_rainDamage = 5f;

	// Token: 0x04000F0E RID: 3854
	private const float m_comTestWidth = 0.2f;

	// Token: 0x04000F0F RID: 3855
	private const float m_comMinAngle = 100f;

	// Token: 0x04000F10 RID: 3856
	private const float m_minFireDistance = 20f;

	// Token: 0x04000F11 RID: 3857
	private const int m_wearUpdateIntervalMinutes = 60;

	// Token: 0x04000F12 RID: 3858
	private const float m_privateAreaModifier = 0.5f;

	// Token: 0x04000F13 RID: 3859
	private static RaycastHit[] m_raycastHits = new RaycastHit[128];

	// Token: 0x04000F14 RID: 3860
	private static Collider[] m_tempColliders = new Collider[128];

	// Token: 0x04000F15 RID: 3861
	private static int m_rayMask = 0;

	// Token: 0x04000F16 RID: 3862
	private static List<WearNTear> m_allInstances = new List<WearNTear>();

	// Token: 0x04000F17 RID: 3863
	private static List<Vector3> m_tempSupportPoints = new List<Vector3>();

	// Token: 0x04000F18 RID: 3864
	private static List<float> m_tempSupportPointValues = new List<float>();

	// Token: 0x04000F19 RID: 3865
	private ZNetView m_nview;

	// Token: 0x04000F1A RID: 3866
	private Collider[] m_colliders;

	// Token: 0x04000F1B RID: 3867
	private float m_support = 1f;

	// Token: 0x04000F1C RID: 3868
	private float m_createTime;

	// Token: 0x04000F1D RID: 3869
	private int m_myIndex = -1;

	// Token: 0x04000F1E RID: 3870
	private float m_rainTimer;

	// Token: 0x04000F1F RID: 3871
	private float m_lastRepair;

	// Token: 0x04000F20 RID: 3872
	private Piece m_piece;

	// Token: 0x04000F21 RID: 3873
	private List<WearNTear.BoundData> m_bounds;

	// Token: 0x04000F22 RID: 3874
	private List<WearNTear.OldMeshData> m_oldMaterials;

	// Token: 0x020001B2 RID: 434
	public enum MaterialType
	{
		// Token: 0x04001327 RID: 4903
		Wood,
		// Token: 0x04001328 RID: 4904
		Stone,
		// Token: 0x04001329 RID: 4905
		Iron,
		// Token: 0x0400132A RID: 4906
		HardWood
	}

	// Token: 0x020001B3 RID: 435
	private struct BoundData
	{
		// Token: 0x0400132B RID: 4907
		public Vector3 m_pos;

		// Token: 0x0400132C RID: 4908
		public Quaternion m_rot;

		// Token: 0x0400132D RID: 4909
		public Vector3 m_size;
	}

	// Token: 0x020001B4 RID: 436
	private struct OldMeshData
	{
		// Token: 0x0400132E RID: 4910
		public Renderer m_renderer;

		// Token: 0x0400132F RID: 4911
		public Material[] m_materials;

		// Token: 0x04001330 RID: 4912
		public Color[] m_color;

		// Token: 0x04001331 RID: 4913
		public Color[] m_emissiveColor;
	}
}
