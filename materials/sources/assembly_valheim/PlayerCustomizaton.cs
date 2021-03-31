using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200005B RID: 91
public class PlayerCustomizaton : MonoBehaviour
{
	// Token: 0x060005E6 RID: 1510 RVA: 0x00032B5C File Offset: 0x00030D5C
	private void OnEnable()
	{
		this.m_maleToggle.isOn = true;
		this.m_femaleToggle.isOn = false;
		this.m_beardPanel.gameObject.SetActive(true);
		this.m_beards = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Beard");
		this.m_hairs = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Customization, "Hair");
		this.m_beards.Sort((ItemDrop x, ItemDrop y) => Localization.instance.Localize(x.m_itemData.m_shared.m_name).CompareTo(Localization.instance.Localize(y.m_itemData.m_shared.m_name)));
		this.m_hairs.Sort((ItemDrop x, ItemDrop y) => Localization.instance.Localize(x.m_itemData.m_shared.m_name).CompareTo(Localization.instance.Localize(y.m_itemData.m_shared.m_name)));
		this.m_beards.Remove(this.m_noBeard);
		this.m_beards.Insert(0, this.m_noBeard);
		this.m_hairs.Remove(this.m_noHair);
		this.m_hairs.Insert(0, this.m_noHair);
	}

	// Token: 0x060005E7 RID: 1511 RVA: 0x00032C5C File Offset: 0x00030E5C
	private void Update()
	{
		if (this.GetPlayer() == null)
		{
			return;
		}
		this.m_selectedHair.text = Localization.instance.Localize(this.GetHair());
		this.m_selectedBeard.text = Localization.instance.Localize(this.GetBeard());
		Color c = Color.Lerp(this.m_skinColor0, this.m_skinColor1, this.m_skinHue.value);
		this.GetPlayer().SetSkinColor(Utils.ColorToVec3(c));
		Color c2 = Color.Lerp(this.m_hairColor0, this.m_hairColor1, this.m_hairTone.value) * Mathf.Lerp(this.m_hairMinLevel, this.m_hairMaxLevel, this.m_hairLevel.value);
		this.GetPlayer().SetHairColor(Utils.ColorToVec3(c2));
	}

	// Token: 0x060005E8 RID: 1512 RVA: 0x00032D2B File Offset: 0x00030F2B
	private Player GetPlayer()
	{
		return base.GetComponentInParent<FejdStartup>().GetPreviewPlayer();
	}

	// Token: 0x060005E9 RID: 1513 RVA: 0x000027E0 File Offset: 0x000009E0
	public void OnHairHueChange(float v)
	{
	}

	// Token: 0x060005EA RID: 1514 RVA: 0x000027E0 File Offset: 0x000009E0
	public void OnSkinHueChange(float v)
	{
	}

	// Token: 0x060005EB RID: 1515 RVA: 0x00032D38 File Offset: 0x00030F38
	public void SetPlayerModel(int index)
	{
		this.GetPlayer().SetPlayerModel(index);
		if (index == 1)
		{
			this.ResetBeard();
		}
	}

	// Token: 0x060005EC RID: 1516 RVA: 0x00032D50 File Offset: 0x00030F50
	public void OnHairLeft()
	{
		this.SetHair(this.GetHairIndex() - 1);
	}

	// Token: 0x060005ED RID: 1517 RVA: 0x00032D60 File Offset: 0x00030F60
	public void OnHairRight()
	{
		this.SetHair(this.GetHairIndex() + 1);
	}

	// Token: 0x060005EE RID: 1518 RVA: 0x00032D70 File Offset: 0x00030F70
	public void OnBeardLeft()
	{
		if (this.GetPlayer().GetPlayerModel() == 1)
		{
			return;
		}
		this.SetBeard(this.GetBeardIndex() - 1);
	}

	// Token: 0x060005EF RID: 1519 RVA: 0x00032D8F File Offset: 0x00030F8F
	public void OnBeardRight()
	{
		if (this.GetPlayer().GetPlayerModel() == 1)
		{
			return;
		}
		this.SetBeard(this.GetBeardIndex() + 1);
	}

	// Token: 0x060005F0 RID: 1520 RVA: 0x00032DAE File Offset: 0x00030FAE
	private void ResetBeard()
	{
		this.GetPlayer().SetBeard(this.m_noBeard.gameObject.name);
	}

	// Token: 0x060005F1 RID: 1521 RVA: 0x00032DCB File Offset: 0x00030FCB
	private void SetBeard(int index)
	{
		if (index < 0 || index >= this.m_beards.Count)
		{
			return;
		}
		this.GetPlayer().SetBeard(this.m_beards[index].gameObject.name);
	}

	// Token: 0x060005F2 RID: 1522 RVA: 0x00032E04 File Offset: 0x00031004
	private void SetHair(int index)
	{
		ZLog.Log("Set hair " + index);
		if (index < 0 || index >= this.m_hairs.Count)
		{
			return;
		}
		this.GetPlayer().SetHair(this.m_hairs[index].gameObject.name);
	}

	// Token: 0x060005F3 RID: 1523 RVA: 0x00032E5C File Offset: 0x0003105C
	private int GetBeardIndex()
	{
		string beard = this.GetPlayer().GetBeard();
		for (int i = 0; i < this.m_beards.Count; i++)
		{
			if (this.m_beards[i].gameObject.name == beard)
			{
				return i;
			}
		}
		return 0;
	}

	// Token: 0x060005F4 RID: 1524 RVA: 0x00032EAC File Offset: 0x000310AC
	private int GetHairIndex()
	{
		string hair = this.GetPlayer().GetHair();
		for (int i = 0; i < this.m_hairs.Count; i++)
		{
			if (this.m_hairs[i].gameObject.name == hair)
			{
				return i;
			}
		}
		return 0;
	}

	// Token: 0x060005F5 RID: 1525 RVA: 0x00032EFC File Offset: 0x000310FC
	private string GetHair()
	{
		return this.m_hairs[this.GetHairIndex()].m_itemData.m_shared.m_name;
	}

	// Token: 0x060005F6 RID: 1526 RVA: 0x00032F1E File Offset: 0x0003111E
	private string GetBeard()
	{
		return this.m_beards[this.GetBeardIndex()].m_itemData.m_shared.m_name;
	}

	// Token: 0x04000692 RID: 1682
	public Color m_skinColor0 = Color.white;

	// Token: 0x04000693 RID: 1683
	public Color m_skinColor1 = Color.white;

	// Token: 0x04000694 RID: 1684
	public Color m_hairColor0 = Color.white;

	// Token: 0x04000695 RID: 1685
	public Color m_hairColor1 = Color.white;

	// Token: 0x04000696 RID: 1686
	public float m_hairMaxLevel = 1f;

	// Token: 0x04000697 RID: 1687
	public float m_hairMinLevel = 0.1f;

	// Token: 0x04000698 RID: 1688
	public Text m_selectedBeard;

	// Token: 0x04000699 RID: 1689
	public Text m_selectedHair;

	// Token: 0x0400069A RID: 1690
	public Slider m_skinHue;

	// Token: 0x0400069B RID: 1691
	public Slider m_hairLevel;

	// Token: 0x0400069C RID: 1692
	public Slider m_hairTone;

	// Token: 0x0400069D RID: 1693
	public RectTransform m_beardPanel;

	// Token: 0x0400069E RID: 1694
	public Toggle m_maleToggle;

	// Token: 0x0400069F RID: 1695
	public Toggle m_femaleToggle;

	// Token: 0x040006A0 RID: 1696
	public ItemDrop m_noHair;

	// Token: 0x040006A1 RID: 1697
	public ItemDrop m_noBeard;

	// Token: 0x040006A2 RID: 1698
	private List<ItemDrop> m_beards;

	// Token: 0x040006A3 RID: 1699
	private List<ItemDrop> m_hairs;
}
