﻿using System;
using UnityEngine;

// Token: 0x0200006E RID: 110
public class PickableItem : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x060006F0 RID: 1776 RVA: 0x00039230 File Offset: 0x00037430
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview.GetZDO() == null)
		{
			return;
		}
		this.SetupRandomPrefab();
		this.m_nview.Register("Pick", new Action<long>(this.RPC_Pick));
		this.SetupItem(true);
	}

	// Token: 0x060006F1 RID: 1777 RVA: 0x00039280 File Offset: 0x00037480
	private void SetupRandomPrefab()
	{
		if (this.m_itemPrefab == null && this.m_randomItemPrefabs.Length != 0)
		{
			int @int = this.m_nview.GetZDO().GetInt("itemPrefab", 0);
			if (@int == 0)
			{
				if (this.m_nview.IsOwner())
				{
					PickableItem.RandomItem randomItem = this.m_randomItemPrefabs[UnityEngine.Random.Range(0, this.m_randomItemPrefabs.Length)];
					this.m_itemPrefab = randomItem.m_itemPrefab;
					this.m_stack = UnityEngine.Random.Range(randomItem.m_stackMin, randomItem.m_stackMax + 1);
					int prefabHash = ObjectDB.instance.GetPrefabHash(this.m_itemPrefab.gameObject);
					this.m_nview.GetZDO().Set("itemPrefab", prefabHash);
					this.m_nview.GetZDO().Set("itemStack", this.m_stack);
					return;
				}
				return;
			}
			else
			{
				GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(@int);
				if (itemPrefab == null)
				{
					ZLog.LogError(string.Concat(new object[]
					{
						"Failed to find saved prefab ",
						@int,
						" in PickableItem ",
						base.gameObject.name
					}));
					return;
				}
				this.m_itemPrefab = itemPrefab.GetComponent<ItemDrop>();
				this.m_stack = this.m_nview.GetZDO().GetInt("itemStack", 0);
			}
		}
	}

	// Token: 0x060006F2 RID: 1778 RVA: 0x000393D4 File Offset: 0x000375D4
	public string GetHoverText()
	{
		if (this.m_picked)
		{
			return "";
		}
		return Localization.instance.Localize(this.GetHoverName() + "\n[<color=yellow><b>$KEY_Use</b></color>] $inventory_pickup");
	}

	// Token: 0x060006F3 RID: 1779 RVA: 0x00039400 File Offset: 0x00037600
	public string GetHoverName()
	{
		if (!this.m_itemPrefab)
		{
			return "None";
		}
		int stackSize = this.GetStackSize();
		if (stackSize > 1)
		{
			return this.m_itemPrefab.m_itemData.m_shared.m_name + " x " + stackSize;
		}
		return this.m_itemPrefab.m_itemData.m_shared.m_name;
	}

	// Token: 0x060006F4 RID: 1780 RVA: 0x00039466 File Offset: 0x00037666
	public bool Interact(Humanoid character, bool repeat)
	{
		if (!this.m_nview.IsValid())
		{
			return false;
		}
		this.m_nview.InvokeRPC("Pick", Array.Empty<object>());
		return true;
	}

	// Token: 0x060006F5 RID: 1781 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x060006F6 RID: 1782 RVA: 0x00039490 File Offset: 0x00037690
	private void RPC_Pick(long sender)
	{
		if (!this.m_nview.IsOwner())
		{
			return;
		}
		if (this.m_picked)
		{
			return;
		}
		this.m_picked = true;
		this.m_pickEffector.Create(base.transform.position, Quaternion.identity, null, 1f);
		this.Drop();
		this.m_nview.Destroy();
	}

	// Token: 0x060006F7 RID: 1783 RVA: 0x000394F0 File Offset: 0x000376F0
	private void Drop()
	{
		Vector3 position = base.transform.position + Vector3.up * 0.2f;
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_itemPrefab.gameObject, position, base.transform.rotation);
		gameObject.GetComponent<ItemDrop>().m_itemData.m_stack = this.GetStackSize();
		gameObject.GetComponent<Rigidbody>().velocity = Vector3.up * 4f;
	}

	// Token: 0x060006F8 RID: 1784 RVA: 0x00039568 File Offset: 0x00037768
	private int GetStackSize()
	{
		return Mathf.Clamp((this.m_stack > 0) ? this.m_stack : this.m_itemPrefab.m_itemData.m_stack, 1, this.m_itemPrefab.m_itemData.m_shared.m_maxStackSize);
	}

	// Token: 0x060006F9 RID: 1785 RVA: 0x000395A8 File Offset: 0x000377A8
	private GameObject GetAttachPrefab()
	{
		Transform transform = this.m_itemPrefab.transform.Find("attach");
		if (transform)
		{
			return transform.gameObject;
		}
		return null;
	}

	// Token: 0x060006FA RID: 1786 RVA: 0x000395DC File Offset: 0x000377DC
	private void SetupItem(bool enabled)
	{
		if (!enabled)
		{
			if (this.m_instance)
			{
				UnityEngine.Object.Destroy(this.m_instance);
				this.m_instance = null;
			}
			return;
		}
		if (this.m_instance)
		{
			return;
		}
		if (this.m_itemPrefab == null)
		{
			return;
		}
		GameObject attachPrefab = this.GetAttachPrefab();
		if (attachPrefab == null)
		{
			ZLog.LogWarning("Failed to get attach prefab for item " + this.m_itemPrefab.name);
			return;
		}
		this.m_instance = UnityEngine.Object.Instantiate<GameObject>(attachPrefab, base.transform.position, base.transform.rotation, base.transform);
		this.m_instance.transform.localPosition = attachPrefab.transform.localPosition;
		this.m_instance.transform.localRotation = attachPrefab.transform.localRotation;
	}

	// Token: 0x060006FB RID: 1787 RVA: 0x000396B4 File Offset: 0x000378B4
	private bool DrawPrefabMesh(ItemDrop prefab)
	{
		if (prefab == null)
		{
			return false;
		}
		bool result = false;
		Gizmos.color = Color.yellow;
		foreach (MeshFilter meshFilter in prefab.gameObject.GetComponentsInChildren<MeshFilter>())
		{
			if (meshFilter && meshFilter.sharedMesh)
			{
				Vector3 position = prefab.transform.position;
				Quaternion lhs = Quaternion.Inverse(prefab.transform.rotation);
				Vector3 point = meshFilter.transform.position - position;
				Vector3 position2 = base.transform.position + base.transform.rotation * point;
				Quaternion rhs = lhs * meshFilter.transform.rotation;
				Quaternion rotation = base.transform.rotation * rhs;
				Gizmos.DrawMesh(meshFilter.sharedMesh, position2, rotation, meshFilter.transform.lossyScale);
				result = true;
			}
		}
		return result;
	}

	// Token: 0x04000779 RID: 1913
	public ItemDrop m_itemPrefab;

	// Token: 0x0400077A RID: 1914
	public int m_stack;

	// Token: 0x0400077B RID: 1915
	public PickableItem.RandomItem[] m_randomItemPrefabs = new PickableItem.RandomItem[0];

	// Token: 0x0400077C RID: 1916
	public EffectList m_pickEffector = new EffectList();

	// Token: 0x0400077D RID: 1917
	private ZNetView m_nview;

	// Token: 0x0400077E RID: 1918
	private GameObject m_instance;

	// Token: 0x0400077F RID: 1919
	private bool m_picked;

	// Token: 0x02000166 RID: 358
	[Serializable]
	public struct RandomItem
	{
		// Token: 0x04001156 RID: 4438
		public ItemDrop m_itemPrefab;

		// Token: 0x04001157 RID: 4439
		public int m_stackMin;

		// Token: 0x04001158 RID: 4440
		public int m_stackMax;
	}
}
