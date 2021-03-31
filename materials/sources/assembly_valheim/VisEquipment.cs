using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000019 RID: 25
public class VisEquipment : MonoBehaviour
{
	// Token: 0x060002AB RID: 683 RVA: 0x00015414 File Offset: 0x00013614
	private void Awake()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		Transform transform = base.transform.Find("Visual");
		if (transform == null)
		{
			transform = base.transform;
		}
		this.m_visual = transform.gameObject;
		this.m_lodGroup = this.m_visual.GetComponentInChildren<LODGroup>();
		if (this.m_bodyModel != null && this.m_bodyModel.material.HasProperty("_ChestTex"))
		{
			this.m_emptyBodyTexture = this.m_bodyModel.material.GetTexture("_ChestTex");
		}
	}

	// Token: 0x060002AC RID: 684 RVA: 0x000154AB File Offset: 0x000136AB
	private void Start()
	{
		this.UpdateVisuals();
	}

	// Token: 0x060002AD RID: 685 RVA: 0x000154B4 File Offset: 0x000136B4
	public void SetWeaponTrails(bool enabled)
	{
		if (this.m_useAllTrails)
		{
			MeleeWeaponTrail[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeleeWeaponTrail>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Emit = enabled;
			}
			return;
		}
		if (this.m_rightItemInstance)
		{
			MeleeWeaponTrail[] componentsInChildren = this.m_rightItemInstance.GetComponentsInChildren<MeleeWeaponTrail>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Emit = enabled;
			}
		}
	}

	// Token: 0x060002AE RID: 686 RVA: 0x00015520 File Offset: 0x00013720
	public void SetModel(int index)
	{
		if (this.m_modelIndex == index)
		{
			return;
		}
		if (index < 0 || index >= this.m_models.Length)
		{
			return;
		}
		ZLog.Log("Vis equip model set to " + index);
		this.m_modelIndex = index;
		if (this.m_nview.GetZDO() != null)
		{
			this.m_nview.GetZDO().Set("ModelIndex", this.m_modelIndex);
		}
	}

	// Token: 0x060002AF RID: 687 RVA: 0x0001558B File Offset: 0x0001378B
	public void SetSkinColor(Vector3 color)
	{
		if (color == this.m_skinColor)
		{
			return;
		}
		this.m_skinColor = color;
		if (this.m_nview.GetZDO() != null)
		{
			this.m_nview.GetZDO().Set("SkinColor", this.m_skinColor);
		}
	}

	// Token: 0x060002B0 RID: 688 RVA: 0x000155CB File Offset: 0x000137CB
	public void SetHairColor(Vector3 color)
	{
		if (this.m_hairColor == color)
		{
			return;
		}
		this.m_hairColor = color;
		if (this.m_nview.GetZDO() != null)
		{
			this.m_nview.GetZDO().Set("HairColor", this.m_hairColor);
		}
	}

	// Token: 0x060002B1 RID: 689 RVA: 0x0001560C File Offset: 0x0001380C
	public void SetLeftItem(string name, int variant)
	{
		if (this.m_leftItem == name && this.m_leftItemVariant == variant)
		{
			return;
		}
		this.m_leftItem = name;
		this.m_leftItemVariant = variant;
		if (this.m_nview.GetZDO() != null)
		{
			this.m_nview.GetZDO().Set("LeftItem", string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode());
			this.m_nview.GetZDO().Set("LeftItemVariant", variant);
		}
	}

	// Token: 0x060002B2 RID: 690 RVA: 0x00015688 File Offset: 0x00013888
	public void SetRightItem(string name)
	{
		if (this.m_rightItem == name)
		{
			return;
		}
		this.m_rightItem = name;
		if (this.m_nview.GetZDO() != null)
		{
			this.m_nview.GetZDO().Set("RightItem", string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode());
		}
	}

	// Token: 0x060002B3 RID: 691 RVA: 0x000156E0 File Offset: 0x000138E0
	public void SetLeftBackItem(string name, int variant)
	{
		if (this.m_leftBackItem == name && this.m_leftBackItemVariant == variant)
		{
			return;
		}
		this.m_leftBackItem = name;
		this.m_leftBackItemVariant = variant;
		if (this.m_nview.GetZDO() != null)
		{
			this.m_nview.GetZDO().Set("LeftBackItem", string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode());
			this.m_nview.GetZDO().Set("LeftBackItemVariant", variant);
		}
	}

	// Token: 0x060002B4 RID: 692 RVA: 0x0001575C File Offset: 0x0001395C
	public void SetRightBackItem(string name)
	{
		if (this.m_rightBackItem == name)
		{
			return;
		}
		this.m_rightBackItem = name;
		ZLog.Log("Right back item " + name);
		if (this.m_nview.GetZDO() != null)
		{
			this.m_nview.GetZDO().Set("RightBackItem", string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode());
		}
	}

	// Token: 0x060002B5 RID: 693 RVA: 0x000157C4 File Offset: 0x000139C4
	public void SetChestItem(string name)
	{
		if (this.m_chestItem == name)
		{
			return;
		}
		this.m_chestItem = name;
		if (this.m_nview.GetZDO() != null)
		{
			this.m_nview.GetZDO().Set("ChestItem", string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode());
		}
	}

	// Token: 0x060002B6 RID: 694 RVA: 0x0001581C File Offset: 0x00013A1C
	public void SetLegItem(string name)
	{
		if (this.m_legItem == name)
		{
			return;
		}
		this.m_legItem = name;
		if (this.m_nview.GetZDO() != null)
		{
			this.m_nview.GetZDO().Set("LegItem", string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode());
		}
	}

	// Token: 0x060002B7 RID: 695 RVA: 0x00015874 File Offset: 0x00013A74
	public void SetHelmetItem(string name)
	{
		if (this.m_helmetItem == name)
		{
			return;
		}
		this.m_helmetItem = name;
		if (this.m_nview.GetZDO() != null)
		{
			this.m_nview.GetZDO().Set("HelmetItem", string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode());
		}
	}

	// Token: 0x060002B8 RID: 696 RVA: 0x000158CC File Offset: 0x00013ACC
	public void SetShoulderItem(string name, int variant)
	{
		if (this.m_shoulderItem == name && this.m_shoulderItemVariant == variant)
		{
			return;
		}
		this.m_shoulderItem = name;
		this.m_shoulderItemVariant = variant;
		if (this.m_nview.GetZDO() != null)
		{
			this.m_nview.GetZDO().Set("ShoulderItem", string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode());
			this.m_nview.GetZDO().Set("ShoulderItemVariant", variant);
		}
	}

	// Token: 0x060002B9 RID: 697 RVA: 0x00015948 File Offset: 0x00013B48
	public void SetBeardItem(string name)
	{
		if (this.m_beardItem == name)
		{
			return;
		}
		this.m_beardItem = name;
		if (this.m_nview.GetZDO() != null)
		{
			this.m_nview.GetZDO().Set("BeardItem", string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode());
		}
	}

	// Token: 0x060002BA RID: 698 RVA: 0x000159A0 File Offset: 0x00013BA0
	public void SetHairItem(string name)
	{
		if (this.m_hairItem == name)
		{
			return;
		}
		this.m_hairItem = name;
		if (this.m_nview.GetZDO() != null)
		{
			this.m_nview.GetZDO().Set("HairItem", string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode());
		}
	}

	// Token: 0x060002BB RID: 699 RVA: 0x000159F8 File Offset: 0x00013BF8
	public void SetUtilityItem(string name)
	{
		if (this.m_utilityItem == name)
		{
			return;
		}
		this.m_utilityItem = name;
		if (this.m_nview.GetZDO() != null)
		{
			this.m_nview.GetZDO().Set("UtilityItem", string.IsNullOrEmpty(name) ? 0 : name.GetStableHashCode());
		}
	}

	// Token: 0x060002BC RID: 700 RVA: 0x000154AB File Offset: 0x000136AB
	private void Update()
	{
		this.UpdateVisuals();
	}

	// Token: 0x060002BD RID: 701 RVA: 0x00015A4E File Offset: 0x00013C4E
	private void UpdateVisuals()
	{
		this.UpdateEquipmentVisuals();
		if (this.m_isPlayer)
		{
			this.UpdateBaseModel();
			this.UpdateColors();
		}
	}

	// Token: 0x060002BE RID: 702 RVA: 0x00015A6C File Offset: 0x00013C6C
	private void UpdateColors()
	{
		Color value = Utils.Vec3ToColor(this.m_skinColor);
		Color value2 = Utils.Vec3ToColor(this.m_hairColor);
		if (this.m_nview.GetZDO() != null)
		{
			value = Utils.Vec3ToColor(this.m_nview.GetZDO().GetVec3("SkinColor", Vector3.one));
			value2 = Utils.Vec3ToColor(this.m_nview.GetZDO().GetVec3("HairColor", Vector3.one));
		}
		this.m_bodyModel.materials[0].SetColor("_SkinColor", value);
		this.m_bodyModel.materials[1].SetColor("_SkinColor", value2);
		if (this.m_beardItemInstance)
		{
			Renderer[] componentsInChildren = this.m_beardItemInstance.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material.SetColor("_SkinColor", value2);
			}
		}
		if (this.m_hairItemInstance)
		{
			Renderer[] componentsInChildren = this.m_hairItemInstance.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material.SetColor("_SkinColor", value2);
			}
		}
	}

	// Token: 0x060002BF RID: 703 RVA: 0x00015B84 File Offset: 0x00013D84
	private void UpdateBaseModel()
	{
		if (this.m_models.Length == 0)
		{
			return;
		}
		int num = this.m_modelIndex;
		if (this.m_nview.GetZDO() != null)
		{
			num = this.m_nview.GetZDO().GetInt("ModelIndex", 0);
		}
		if (this.m_currentModelIndex != num || this.m_bodyModel.sharedMesh != this.m_models[num].m_mesh)
		{
			this.m_currentModelIndex = num;
			this.m_bodyModel.sharedMesh = this.m_models[num].m_mesh;
			this.m_bodyModel.materials[0].SetTexture("_MainTex", this.m_models[num].m_baseMaterial.GetTexture("_MainTex"));
			this.m_bodyModel.materials[0].SetTexture("_SkinBumpMap", this.m_models[num].m_baseMaterial.GetTexture("_SkinBumpMap"));
		}
	}

	// Token: 0x060002C0 RID: 704 RVA: 0x00015C6C File Offset: 0x00013E6C
	private void UpdateEquipmentVisuals()
	{
		int hash = 0;
		int rightHandEquiped = 0;
		int chestEquiped = 0;
		int legEquiped = 0;
		int hash2 = 0;
		int beardEquiped = 0;
		int num = 0;
		int hash3 = 0;
		int utilityEquiped = 0;
		int leftItem = 0;
		int rightItem = 0;
		int variant = this.m_shoulderItemVariant;
		int variant2 = this.m_leftItemVariant;
		int leftVariant = this.m_leftBackItemVariant;
		ZDO zdo = this.m_nview.GetZDO();
		if (zdo != null)
		{
			hash = zdo.GetInt("LeftItem", 0);
			rightHandEquiped = zdo.GetInt("RightItem", 0);
			chestEquiped = zdo.GetInt("ChestItem", 0);
			legEquiped = zdo.GetInt("LegItem", 0);
			hash2 = zdo.GetInt("HelmetItem", 0);
			hash3 = zdo.GetInt("ShoulderItem", 0);
			utilityEquiped = zdo.GetInt("UtilityItem", 0);
			if (this.m_isPlayer)
			{
				beardEquiped = zdo.GetInt("BeardItem", 0);
				num = zdo.GetInt("HairItem", 0);
				leftItem = zdo.GetInt("LeftBackItem", 0);
				rightItem = zdo.GetInt("RightBackItem", 0);
				variant = zdo.GetInt("ShoulderItemVariant", 0);
				variant2 = zdo.GetInt("LeftItemVariant", 0);
				leftVariant = zdo.GetInt("LeftBackItemVariant", 0);
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(this.m_leftItem))
			{
				hash = this.m_leftItem.GetStableHashCode();
			}
			if (!string.IsNullOrEmpty(this.m_rightItem))
			{
				rightHandEquiped = this.m_rightItem.GetStableHashCode();
			}
			if (!string.IsNullOrEmpty(this.m_chestItem))
			{
				chestEquiped = this.m_chestItem.GetStableHashCode();
			}
			if (!string.IsNullOrEmpty(this.m_legItem))
			{
				legEquiped = this.m_legItem.GetStableHashCode();
			}
			if (!string.IsNullOrEmpty(this.m_helmetItem))
			{
				hash2 = this.m_helmetItem.GetStableHashCode();
			}
			if (!string.IsNullOrEmpty(this.m_shoulderItem))
			{
				hash3 = this.m_shoulderItem.GetStableHashCode();
			}
			if (!string.IsNullOrEmpty(this.m_utilityItem))
			{
				utilityEquiped = this.m_utilityItem.GetStableHashCode();
			}
			if (this.m_isPlayer)
			{
				if (!string.IsNullOrEmpty(this.m_beardItem))
				{
					beardEquiped = this.m_beardItem.GetStableHashCode();
				}
				if (!string.IsNullOrEmpty(this.m_hairItem))
				{
					num = this.m_hairItem.GetStableHashCode();
				}
				if (!string.IsNullOrEmpty(this.m_leftBackItem))
				{
					leftItem = this.m_leftBackItem.GetStableHashCode();
				}
				if (!string.IsNullOrEmpty(this.m_rightBackItem))
				{
					rightItem = this.m_rightBackItem.GetStableHashCode();
				}
			}
		}
		bool flag = false;
		flag = (this.SetRightHandEquiped(rightHandEquiped) || flag);
		flag = (this.SetLeftHandEquiped(hash, variant2) || flag);
		flag = (this.SetChestEquiped(chestEquiped) || flag);
		flag = (this.SetLegEquiped(legEquiped) || flag);
		flag = (this.SetHelmetEquiped(hash2, num) || flag);
		flag = (this.SetShoulderEquiped(hash3, variant) || flag);
		flag = (this.SetUtilityEquiped(utilityEquiped) || flag);
		if (this.m_isPlayer)
		{
			flag = (this.SetBeardEquiped(beardEquiped) || flag);
			flag = (this.SetBackEquiped(leftItem, rightItem, leftVariant) || flag);
			if (this.m_helmetHideHair)
			{
				num = 0;
			}
			flag = (this.SetHairEquiped(num) || flag);
		}
		if (flag)
		{
			this.UpdateLodgroup();
		}
	}

	// Token: 0x060002C1 RID: 705 RVA: 0x00015F6C File Offset: 0x0001416C
	protected void UpdateLodgroup()
	{
		if (this.m_lodGroup == null)
		{
			return;
		}
		Renderer[] componentsInChildren = this.m_visual.GetComponentsInChildren<Renderer>();
		LOD[] lods = this.m_lodGroup.GetLODs();
		lods[0].renderers = componentsInChildren;
		this.m_lodGroup.SetLODs(lods);
	}

	// Token: 0x060002C2 RID: 706 RVA: 0x00015FBC File Offset: 0x000141BC
	private bool SetRightHandEquiped(int hash)
	{
		if (this.m_currentRightItemHash == hash)
		{
			return false;
		}
		if (this.m_rightItemInstance)
		{
			UnityEngine.Object.Destroy(this.m_rightItemInstance);
			this.m_rightItemInstance = null;
		}
		this.m_currentRightItemHash = hash;
		if (hash != 0)
		{
			this.m_rightItemInstance = this.AttachItem(hash, 0, this.m_rightHand, true);
		}
		return true;
	}

	// Token: 0x060002C3 RID: 707 RVA: 0x00016014 File Offset: 0x00014214
	private bool SetLeftHandEquiped(int hash, int variant)
	{
		if (this.m_currentLeftItemHash == hash && this.m_currentLeftItemVariant == variant)
		{
			return false;
		}
		if (this.m_leftItemInstance)
		{
			UnityEngine.Object.Destroy(this.m_leftItemInstance);
			this.m_leftItemInstance = null;
		}
		this.m_currentLeftItemHash = hash;
		this.m_currentLeftItemVariant = variant;
		if (hash != 0)
		{
			this.m_leftItemInstance = this.AttachItem(hash, variant, this.m_leftHand, true);
		}
		return true;
	}

	// Token: 0x060002C4 RID: 708 RVA: 0x0001607C File Offset: 0x0001427C
	private bool SetBackEquiped(int leftItem, int rightItem, int leftVariant)
	{
		if (this.m_currentLeftBackItemHash == leftItem && this.m_currentRightBackItemHash == rightItem && this.m_currentLeftBackItemVariant == leftVariant)
		{
			return false;
		}
		if (this.m_leftBackItemInstance)
		{
			UnityEngine.Object.Destroy(this.m_leftBackItemInstance);
			this.m_leftBackItemInstance = null;
		}
		if (this.m_rightBackItemInstance)
		{
			UnityEngine.Object.Destroy(this.m_rightBackItemInstance);
			this.m_rightBackItemInstance = null;
		}
		this.m_currentLeftBackItemHash = leftItem;
		this.m_currentRightBackItemHash = rightItem;
		this.m_currentLeftBackItemVariant = leftVariant;
		if (this.m_currentLeftBackItemHash != 0)
		{
			this.m_leftBackItemInstance = this.AttachBackItem(leftItem, leftVariant, false);
		}
		if (this.m_currentRightBackItemHash != 0)
		{
			this.m_rightBackItemInstance = this.AttachBackItem(rightItem, 0, true);
		}
		return true;
	}

	// Token: 0x060002C5 RID: 709 RVA: 0x00016128 File Offset: 0x00014328
	private GameObject AttachBackItem(int hash, int variant, bool rightHand)
	{
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(hash);
		if (itemPrefab == null)
		{
			ZLog.Log("Missing back attach item prefab: " + hash);
			return null;
		}
		ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
		ItemDrop.ItemData.ItemType itemType = (component.m_itemData.m_shared.m_attachOverride != ItemDrop.ItemData.ItemType.None) ? component.m_itemData.m_shared.m_attachOverride : component.m_itemData.m_shared.m_itemType;
		if (itemType != ItemDrop.ItemData.ItemType.Torch)
		{
			if (itemType <= ItemDrop.ItemData.ItemType.TwoHandedWeapon)
			{
				switch (itemType)
				{
				case ItemDrop.ItemData.ItemType.OneHandedWeapon:
					return this.AttachItem(hash, variant, this.m_backMelee, true);
				case ItemDrop.ItemData.ItemType.Bow:
					return this.AttachItem(hash, variant, this.m_backBow, true);
				case ItemDrop.ItemData.ItemType.Shield:
					return this.AttachItem(hash, variant, this.m_backShield, true);
				default:
					if (itemType == ItemDrop.ItemData.ItemType.TwoHandedWeapon)
					{
						return this.AttachItem(hash, variant, this.m_backTwohandedMelee, true);
					}
					break;
				}
			}
			else
			{
				if (itemType == ItemDrop.ItemData.ItemType.Tool)
				{
					return this.AttachItem(hash, variant, this.m_backTool, true);
				}
				if (itemType == ItemDrop.ItemData.ItemType.Attach_Atgeir)
				{
					return this.AttachItem(hash, variant, this.m_backAtgeir, true);
				}
			}
			return null;
		}
		if (rightHand)
		{
			return this.AttachItem(hash, variant, this.m_backMelee, false);
		}
		return this.AttachItem(hash, variant, this.m_backTool, false);
	}

	// Token: 0x060002C6 RID: 710 RVA: 0x00016254 File Offset: 0x00014454
	private bool SetChestEquiped(int hash)
	{
		if (this.m_currentChestItemHash == hash)
		{
			return false;
		}
		this.m_currentChestItemHash = hash;
		if (this.m_bodyModel == null)
		{
			return true;
		}
		if (this.m_chestItemInstances != null)
		{
			foreach (GameObject gameObject in this.m_chestItemInstances)
			{
				if (this.m_lodGroup)
				{
					Utils.RemoveFromLodgroup(this.m_lodGroup, gameObject);
				}
				UnityEngine.Object.Destroy(gameObject);
			}
			this.m_chestItemInstances = null;
			this.m_bodyModel.material.SetTexture("_ChestTex", this.m_emptyBodyTexture);
			this.m_bodyModel.material.SetTexture("_ChestBumpMap", null);
			this.m_bodyModel.material.SetTexture("_ChestMetal", null);
		}
		if (this.m_currentChestItemHash == 0)
		{
			return true;
		}
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(hash);
		if (itemPrefab == null)
		{
			ZLog.Log("Missing chest item " + hash);
			return true;
		}
		ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
		if (component.m_itemData.m_shared.m_armorMaterial)
		{
			this.m_bodyModel.material.SetTexture("_ChestTex", component.m_itemData.m_shared.m_armorMaterial.GetTexture("_ChestTex"));
			this.m_bodyModel.material.SetTexture("_ChestBumpMap", component.m_itemData.m_shared.m_armorMaterial.GetTexture("_ChestBumpMap"));
			this.m_bodyModel.material.SetTexture("_ChestMetal", component.m_itemData.m_shared.m_armorMaterial.GetTexture("_ChestMetal"));
		}
		this.m_chestItemInstances = this.AttachArmor(hash, -1);
		return true;
	}

	// Token: 0x060002C7 RID: 711 RVA: 0x00016430 File Offset: 0x00014630
	private bool SetShoulderEquiped(int hash, int variant)
	{
		if (this.m_currentShoulderItemHash == hash && this.m_currenShoulderItemVariant == variant)
		{
			return false;
		}
		this.m_currentShoulderItemHash = hash;
		this.m_currenShoulderItemVariant = variant;
		if (this.m_bodyModel == null)
		{
			return true;
		}
		if (this.m_shoulderItemInstances != null)
		{
			foreach (GameObject gameObject in this.m_shoulderItemInstances)
			{
				if (this.m_lodGroup)
				{
					Utils.RemoveFromLodgroup(this.m_lodGroup, gameObject);
				}
				UnityEngine.Object.Destroy(gameObject);
			}
			this.m_shoulderItemInstances = null;
		}
		if (this.m_currentShoulderItemHash == 0)
		{
			return true;
		}
		if (ObjectDB.instance.GetItemPrefab(hash) == null)
		{
			ZLog.Log("Missing shoulder item " + hash);
			return true;
		}
		this.m_shoulderItemInstances = this.AttachArmor(hash, variant);
		return true;
	}

	// Token: 0x060002C8 RID: 712 RVA: 0x00016520 File Offset: 0x00014720
	private bool SetLegEquiped(int hash)
	{
		if (this.m_currentLegItemHash == hash)
		{
			return false;
		}
		this.m_currentLegItemHash = hash;
		if (this.m_bodyModel == null)
		{
			return true;
		}
		if (this.m_legItemInstances != null)
		{
			foreach (GameObject obj in this.m_legItemInstances)
			{
				UnityEngine.Object.Destroy(obj);
			}
			this.m_legItemInstances = null;
			this.m_bodyModel.material.SetTexture("_LegsTex", this.m_emptyBodyTexture);
			this.m_bodyModel.material.SetTexture("_LegsBumpMap", null);
			this.m_bodyModel.material.SetTexture("_LegsMetal", null);
		}
		if (this.m_currentLegItemHash == 0)
		{
			return true;
		}
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(hash);
		if (itemPrefab == null)
		{
			ZLog.Log("Missing legs item " + hash);
			return true;
		}
		ItemDrop component = itemPrefab.GetComponent<ItemDrop>();
		if (component.m_itemData.m_shared.m_armorMaterial)
		{
			this.m_bodyModel.material.SetTexture("_LegsTex", component.m_itemData.m_shared.m_armorMaterial.GetTexture("_LegsTex"));
			this.m_bodyModel.material.SetTexture("_LegsBumpMap", component.m_itemData.m_shared.m_armorMaterial.GetTexture("_LegsBumpMap"));
			this.m_bodyModel.material.SetTexture("_LegsMetal", component.m_itemData.m_shared.m_armorMaterial.GetTexture("_LegsMetal"));
		}
		this.m_legItemInstances = this.AttachArmor(hash, -1);
		return true;
	}

	// Token: 0x060002C9 RID: 713 RVA: 0x000166E0 File Offset: 0x000148E0
	private bool SetBeardEquiped(int hash)
	{
		if (this.m_currentBeardItemHash == hash)
		{
			return false;
		}
		if (this.m_beardItemInstance)
		{
			UnityEngine.Object.Destroy(this.m_beardItemInstance);
			this.m_beardItemInstance = null;
		}
		this.m_currentBeardItemHash = hash;
		if (hash != 0)
		{
			this.m_beardItemInstance = this.AttachItem(hash, 0, this.m_helmet, true);
		}
		return true;
	}

	// Token: 0x060002CA RID: 714 RVA: 0x00016738 File Offset: 0x00014938
	private bool SetHairEquiped(int hash)
	{
		if (this.m_currentHairItemHash == hash)
		{
			return false;
		}
		if (this.m_hairItemInstance)
		{
			UnityEngine.Object.Destroy(this.m_hairItemInstance);
			this.m_hairItemInstance = null;
		}
		this.m_currentHairItemHash = hash;
		if (hash != 0)
		{
			this.m_hairItemInstance = this.AttachItem(hash, 0, this.m_helmet, true);
		}
		return true;
	}

	// Token: 0x060002CB RID: 715 RVA: 0x00016790 File Offset: 0x00014990
	private bool SetHelmetEquiped(int hash, int hairHash)
	{
		if (this.m_currentHelmetItemHash == hash)
		{
			return false;
		}
		if (this.m_helmetItemInstance)
		{
			UnityEngine.Object.Destroy(this.m_helmetItemInstance);
			this.m_helmetItemInstance = null;
		}
		this.m_currentHelmetItemHash = hash;
		this.m_helmetHideHair = this.HelmetHidesHair(hash);
		if (hash != 0)
		{
			this.m_helmetItemInstance = this.AttachItem(hash, 0, this.m_helmet, true);
		}
		return true;
	}

	// Token: 0x060002CC RID: 716 RVA: 0x000167F4 File Offset: 0x000149F4
	private bool SetUtilityEquiped(int hash)
	{
		if (this.m_currentUtilityItemHash == hash)
		{
			return false;
		}
		if (this.m_utilityItemInstances != null)
		{
			foreach (GameObject gameObject in this.m_utilityItemInstances)
			{
				if (this.m_lodGroup)
				{
					Utils.RemoveFromLodgroup(this.m_lodGroup, gameObject);
				}
				UnityEngine.Object.Destroy(gameObject);
			}
			this.m_utilityItemInstances = null;
		}
		this.m_currentUtilityItemHash = hash;
		if (hash != 0)
		{
			this.m_utilityItemInstances = this.AttachArmor(hash, -1);
		}
		return true;
	}

	// Token: 0x060002CD RID: 717 RVA: 0x00016894 File Offset: 0x00014A94
	private bool HelmetHidesHair(int itemHash)
	{
		if (itemHash == 0)
		{
			return false;
		}
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemHash);
		return !(itemPrefab == null) && itemPrefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_helmetHideHair;
	}

	// Token: 0x060002CE RID: 718 RVA: 0x000168D4 File Offset: 0x00014AD4
	private List<GameObject> AttachArmor(int itemHash, int variant = -1)
	{
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemHash);
		if (itemPrefab == null)
		{
			ZLog.Log(string.Concat(new object[]
			{
				"Missing attach item: ",
				itemHash,
				"  ob:",
				base.gameObject.name
			}));
			return null;
		}
		List<GameObject> list = new List<GameObject>();
		int childCount = itemPrefab.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = itemPrefab.transform.GetChild(i);
			if (child.gameObject.name.StartsWith("attach_"))
			{
				string text = child.gameObject.name.Substring(7);
				GameObject gameObject;
				if (text == "skin")
				{
					gameObject = UnityEngine.Object.Instantiate<GameObject>(child.gameObject, this.m_bodyModel.transform.position, this.m_bodyModel.transform.parent.rotation, this.m_bodyModel.transform.parent);
					gameObject.SetActive(true);
					foreach (SkinnedMeshRenderer skinnedMeshRenderer in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
					{
						skinnedMeshRenderer.rootBone = this.m_bodyModel.rootBone;
						skinnedMeshRenderer.bones = this.m_bodyModel.bones;
					}
					foreach (Cloth cloth in gameObject.GetComponentsInChildren<Cloth>())
					{
						if (this.m_clothColliders.Length != 0)
						{
							if (cloth.capsuleColliders.Length != 0)
							{
								List<CapsuleCollider> list2 = new List<CapsuleCollider>(this.m_clothColliders);
								list2.AddRange(cloth.capsuleColliders);
								cloth.capsuleColliders = list2.ToArray();
							}
							else
							{
								cloth.capsuleColliders = this.m_clothColliders;
							}
						}
					}
				}
				else
				{
					Transform transform = Utils.FindChild(this.m_visual.transform, text);
					if (transform == null)
					{
						ZLog.LogWarning("Missing joint " + text + " in item " + itemPrefab.name);
						goto IL_268;
					}
					gameObject = UnityEngine.Object.Instantiate<GameObject>(child.gameObject);
					gameObject.SetActive(true);
					gameObject.transform.SetParent(transform);
					gameObject.transform.localPosition = Vector3.zero;
					gameObject.transform.localRotation = Quaternion.identity;
				}
				if (variant >= 0)
				{
					IEquipmentVisual componentInChildren = gameObject.GetComponentInChildren<IEquipmentVisual>();
					if (componentInChildren != null)
					{
						componentInChildren.Setup(variant);
					}
				}
				this.CleanupInstance(gameObject);
				this.EnableEquipedEffects(gameObject);
				list.Add(gameObject);
			}
			IL_268:;
		}
		return list;
	}

	// Token: 0x060002CF RID: 719 RVA: 0x00016B58 File Offset: 0x00014D58
	protected GameObject AttachItem(int itemHash, int variant, Transform joint, bool enableEquipEffects = true)
	{
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemHash);
		if (itemPrefab == null)
		{
			ZLog.Log(string.Concat(new object[]
			{
				"Missing attach item: ",
				itemHash,
				"  ob:",
				base.gameObject.name,
				"  joint:",
				joint ? joint.name : "none"
			}));
			return null;
		}
		GameObject gameObject = null;
		int childCount = itemPrefab.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = itemPrefab.transform.GetChild(i);
			if (child.gameObject.name == "attach" || child.gameObject.name == "attach_skin")
			{
				gameObject = child.gameObject;
				break;
			}
		}
		if (gameObject == null)
		{
			return null;
		}
		GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject);
		gameObject2.SetActive(true);
		this.CleanupInstance(gameObject2);
		if (enableEquipEffects)
		{
			this.EnableEquipedEffects(gameObject2);
		}
		if (gameObject.name == "attach_skin")
		{
			gameObject2.transform.SetParent(this.m_bodyModel.transform.parent);
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in gameObject2.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				skinnedMeshRenderer.rootBone = this.m_bodyModel.rootBone;
				skinnedMeshRenderer.bones = this.m_bodyModel.bones;
			}
		}
		else
		{
			gameObject2.transform.SetParent(joint);
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localRotation = Quaternion.identity;
		}
		IEquipmentVisual componentInChildren = gameObject2.GetComponentInChildren<IEquipmentVisual>();
		if (componentInChildren != null)
		{
			componentInChildren.Setup(variant);
		}
		return gameObject2;
	}

	// Token: 0x060002D0 RID: 720 RVA: 0x00016D38 File Offset: 0x00014F38
	private void CleanupInstance(GameObject instance)
	{
		Collider[] componentsInChildren = instance.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
	}

	// Token: 0x060002D1 RID: 721 RVA: 0x00016D64 File Offset: 0x00014F64
	private void EnableEquipedEffects(GameObject instance)
	{
		Transform transform = instance.transform.Find("equiped");
		if (transform)
		{
			transform.gameObject.SetActive(true);
		}
	}

	// Token: 0x060002D2 RID: 722 RVA: 0x00016D98 File Offset: 0x00014F98
	public int GetModelIndex()
	{
		int result = this.m_modelIndex;
		if (this.m_nview.IsValid())
		{
			result = this.m_nview.GetZDO().GetInt("ModelIndex", 0);
		}
		return result;
	}

	// Token: 0x04000216 RID: 534
	public SkinnedMeshRenderer m_bodyModel;

	// Token: 0x04000217 RID: 535
	[Header("Attachment points")]
	public Transform m_leftHand;

	// Token: 0x04000218 RID: 536
	public Transform m_rightHand;

	// Token: 0x04000219 RID: 537
	public Transform m_helmet;

	// Token: 0x0400021A RID: 538
	public Transform m_backShield;

	// Token: 0x0400021B RID: 539
	public Transform m_backMelee;

	// Token: 0x0400021C RID: 540
	public Transform m_backTwohandedMelee;

	// Token: 0x0400021D RID: 541
	public Transform m_backBow;

	// Token: 0x0400021E RID: 542
	public Transform m_backTool;

	// Token: 0x0400021F RID: 543
	public Transform m_backAtgeir;

	// Token: 0x04000220 RID: 544
	public CapsuleCollider[] m_clothColliders = new CapsuleCollider[0];

	// Token: 0x04000221 RID: 545
	public VisEquipment.PlayerModel[] m_models = new VisEquipment.PlayerModel[0];

	// Token: 0x04000222 RID: 546
	public bool m_isPlayer;

	// Token: 0x04000223 RID: 547
	public bool m_useAllTrails;

	// Token: 0x04000224 RID: 548
	private string m_leftItem = "";

	// Token: 0x04000225 RID: 549
	private string m_rightItem = "";

	// Token: 0x04000226 RID: 550
	private string m_chestItem = "";

	// Token: 0x04000227 RID: 551
	private string m_legItem = "";

	// Token: 0x04000228 RID: 552
	private string m_helmetItem = "";

	// Token: 0x04000229 RID: 553
	private string m_shoulderItem = "";

	// Token: 0x0400022A RID: 554
	private string m_beardItem = "";

	// Token: 0x0400022B RID: 555
	private string m_hairItem = "";

	// Token: 0x0400022C RID: 556
	private string m_utilityItem = "";

	// Token: 0x0400022D RID: 557
	private string m_leftBackItem = "";

	// Token: 0x0400022E RID: 558
	private string m_rightBackItem = "";

	// Token: 0x0400022F RID: 559
	private int m_shoulderItemVariant;

	// Token: 0x04000230 RID: 560
	private int m_leftItemVariant;

	// Token: 0x04000231 RID: 561
	private int m_leftBackItemVariant;

	// Token: 0x04000232 RID: 562
	private GameObject m_leftItemInstance;

	// Token: 0x04000233 RID: 563
	private GameObject m_rightItemInstance;

	// Token: 0x04000234 RID: 564
	private GameObject m_helmetItemInstance;

	// Token: 0x04000235 RID: 565
	private List<GameObject> m_chestItemInstances;

	// Token: 0x04000236 RID: 566
	private List<GameObject> m_legItemInstances;

	// Token: 0x04000237 RID: 567
	private List<GameObject> m_shoulderItemInstances;

	// Token: 0x04000238 RID: 568
	private List<GameObject> m_utilityItemInstances;

	// Token: 0x04000239 RID: 569
	private GameObject m_beardItemInstance;

	// Token: 0x0400023A RID: 570
	private GameObject m_hairItemInstance;

	// Token: 0x0400023B RID: 571
	private GameObject m_leftBackItemInstance;

	// Token: 0x0400023C RID: 572
	private GameObject m_rightBackItemInstance;

	// Token: 0x0400023D RID: 573
	private int m_currentLeftItemHash;

	// Token: 0x0400023E RID: 574
	private int m_currentRightItemHash;

	// Token: 0x0400023F RID: 575
	private int m_currentChestItemHash;

	// Token: 0x04000240 RID: 576
	private int m_currentLegItemHash;

	// Token: 0x04000241 RID: 577
	private int m_currentHelmetItemHash;

	// Token: 0x04000242 RID: 578
	private int m_currentShoulderItemHash;

	// Token: 0x04000243 RID: 579
	private int m_currentBeardItemHash;

	// Token: 0x04000244 RID: 580
	private int m_currentHairItemHash;

	// Token: 0x04000245 RID: 581
	private int m_currentUtilityItemHash;

	// Token: 0x04000246 RID: 582
	private int m_currentLeftBackItemHash;

	// Token: 0x04000247 RID: 583
	private int m_currentRightBackItemHash;

	// Token: 0x04000248 RID: 584
	private int m_currenShoulderItemVariant;

	// Token: 0x04000249 RID: 585
	private int m_currentLeftItemVariant;

	// Token: 0x0400024A RID: 586
	private int m_currentLeftBackItemVariant;

	// Token: 0x0400024B RID: 587
	private bool m_helmetHideHair;

	// Token: 0x0400024C RID: 588
	private Texture m_emptyBodyTexture;

	// Token: 0x0400024D RID: 589
	private int m_modelIndex;

	// Token: 0x0400024E RID: 590
	private Vector3 m_skinColor = Vector3.one;

	// Token: 0x0400024F RID: 591
	private Vector3 m_hairColor = Vector3.one;

	// Token: 0x04000250 RID: 592
	private int m_currentModelIndex;

	// Token: 0x04000251 RID: 593
	private ZNetView m_nview;

	// Token: 0x04000252 RID: 594
	private GameObject m_visual;

	// Token: 0x04000253 RID: 595
	private LODGroup m_lodGroup;

	// Token: 0x0200012F RID: 303
	[Serializable]
	public class PlayerModel
	{
		// Token: 0x0400102C RID: 4140
		public Mesh m_mesh;

		// Token: 0x0400102D RID: 4141
		public Material m_baseMaterial;
	}
}
