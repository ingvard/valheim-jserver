using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000068 RID: 104
public class CraftingStation : MonoBehaviour, Hoverable, Interactable
{
	// Token: 0x0600066E RID: 1646 RVA: 0x000360A8 File Offset: 0x000342A8
	private void Start()
	{
		this.m_nview = base.GetComponent<ZNetView>();
		if (this.m_nview && this.m_nview.GetZDO() == null)
		{
			return;
		}
		CraftingStation.m_allStations.Add(this);
		if (this.m_areaMarker)
		{
			this.m_areaMarker.SetActive(false);
		}
		if (this.m_craftRequireFire)
		{
			base.InvokeRepeating("CheckFire", 1f, 1f);
		}
	}

	// Token: 0x0600066F RID: 1647 RVA: 0x0003611D File Offset: 0x0003431D
	private void OnDestroy()
	{
		CraftingStation.m_allStations.Remove(this);
	}

	// Token: 0x06000670 RID: 1648 RVA: 0x0003612C File Offset: 0x0003432C
	public bool Interact(Humanoid user, bool repeat)
	{
		if (repeat)
		{
			return false;
		}
		if (user == Player.m_localPlayer)
		{
			if (!this.InUseDistance(user))
			{
				return false;
			}
			Player player = user as Player;
			if (this.CheckUsable(player, true))
			{
				player.SetCraftingStation(this);
				InventoryGui.instance.Show(null);
				return false;
			}
		}
		return false;
	}

	// Token: 0x06000671 RID: 1649 RVA: 0x000023E2 File Offset: 0x000005E2
	public bool UseItem(Humanoid user, ItemDrop.ItemData item)
	{
		return false;
	}

	// Token: 0x06000672 RID: 1650 RVA: 0x0003617C File Offset: 0x0003437C
	public bool CheckUsable(Player player, bool showMessage)
	{
		if (this.m_craftRequireRoof)
		{
			float num;
			bool flag;
			Cover.GetCoverForPoint(this.m_roofCheckPoint.position, out num, out flag);
			if (!flag)
			{
				if (showMessage)
				{
					player.Message(MessageHud.MessageType.Center, "$msg_stationneedroof", 0, null);
				}
				return false;
			}
			if (num < 0.7f)
			{
				if (showMessage)
				{
					player.Message(MessageHud.MessageType.Center, "$msg_stationtooexposed", 0, null);
				}
				return false;
			}
		}
		if (this.m_craftRequireFire && !this.m_haveFire)
		{
			if (showMessage)
			{
				player.Message(MessageHud.MessageType.Center, "$msg_needfire", 0, null);
			}
			return false;
		}
		return true;
	}

	// Token: 0x06000673 RID: 1651 RVA: 0x000361FA File Offset: 0x000343FA
	public string GetHoverText()
	{
		if (!this.InUseDistance(Player.m_localPlayer))
		{
			return Localization.instance.Localize("<color=grey>$piece_toofar</color>");
		}
		return Localization.instance.Localize(this.m_name + "\n[<color=yellow><b>$KEY_Use</b></color>] $piece_use ");
	}

	// Token: 0x06000674 RID: 1652 RVA: 0x00036233 File Offset: 0x00034433
	public string GetHoverName()
	{
		return this.m_name;
	}

	// Token: 0x06000675 RID: 1653 RVA: 0x0003623B File Offset: 0x0003443B
	public void ShowAreaMarker()
	{
		if (this.m_areaMarker)
		{
			this.m_areaMarker.SetActive(true);
			base.CancelInvoke("HideMarker");
			base.Invoke("HideMarker", 0.5f);
			this.PokeInUse();
		}
	}

	// Token: 0x06000676 RID: 1654 RVA: 0x00036277 File Offset: 0x00034477
	private void HideMarker()
	{
		this.m_areaMarker.SetActive(false);
	}

	// Token: 0x06000677 RID: 1655 RVA: 0x00036288 File Offset: 0x00034488
	public static void UpdateKnownStationsInRange(Player player)
	{
		Vector3 position = player.transform.position;
		foreach (CraftingStation craftingStation in CraftingStation.m_allStations)
		{
			if (Vector3.Distance(craftingStation.transform.position, position) < craftingStation.m_discoverRange)
			{
				player.AddKnownStation(craftingStation);
			}
		}
	}

	// Token: 0x06000678 RID: 1656 RVA: 0x00036300 File Offset: 0x00034500
	private void FixedUpdate()
	{
		if (this.m_nview == null || !this.m_nview.IsValid())
		{
			return;
		}
		this.m_useTimer += Time.fixedDeltaTime;
		this.m_updateExtensionTimer += Time.fixedDeltaTime;
		if (this.m_inUseObject)
		{
			this.m_inUseObject.SetActive(this.m_useTimer < 1f);
		}
	}

	// Token: 0x06000679 RID: 1657 RVA: 0x00036374 File Offset: 0x00034574
	private void CheckFire()
	{
		this.m_haveFire = EffectArea.IsPointInsideArea(base.transform.position, EffectArea.Type.Burning, 0.25f);
		if (this.m_haveFireObject)
		{
			this.m_haveFireObject.SetActive(this.m_haveFire);
		}
	}

	// Token: 0x0600067A RID: 1658 RVA: 0x000363C0 File Offset: 0x000345C0
	public void PokeInUse()
	{
		this.m_useTimer = 0f;
		this.TriggerExtensionEffects();
	}

	// Token: 0x0600067B RID: 1659 RVA: 0x000363D4 File Offset: 0x000345D4
	public static CraftingStation GetCraftingStation(Vector3 point)
	{
		if (CraftingStation.m_triggerMask == 0)
		{
			CraftingStation.m_triggerMask = LayerMask.GetMask(new string[]
			{
				"character_trigger"
			});
		}
		foreach (Collider collider in Physics.OverlapSphere(point, 0.1f, CraftingStation.m_triggerMask, QueryTriggerInteraction.Collide))
		{
			if (collider.gameObject.CompareTag("StationUseArea"))
			{
				CraftingStation componentInParent = collider.GetComponentInParent<CraftingStation>();
				if (componentInParent != null)
				{
					return componentInParent;
				}
			}
		}
		return null;
	}

	// Token: 0x0600067C RID: 1660 RVA: 0x0003644C File Offset: 0x0003464C
	public static CraftingStation HaveBuildStationInRange(string name, Vector3 point)
	{
		foreach (CraftingStation craftingStation in CraftingStation.m_allStations)
		{
			if (!(craftingStation.m_name != name))
			{
				float rangeBuild = craftingStation.m_rangeBuild;
				if (Vector3.Distance(craftingStation.transform.position, point) < rangeBuild)
				{
					return craftingStation;
				}
			}
		}
		return null;
	}

	// Token: 0x0600067D RID: 1661 RVA: 0x000364C8 File Offset: 0x000346C8
	public static void FindStationsInRange(string name, Vector3 point, float range, List<CraftingStation> stations)
	{
		foreach (CraftingStation craftingStation in CraftingStation.m_allStations)
		{
			if (!(craftingStation.m_name != name) && Vector3.Distance(craftingStation.transform.position, point) < range)
			{
				stations.Add(craftingStation);
			}
		}
	}

	// Token: 0x0600067E RID: 1662 RVA: 0x0003653C File Offset: 0x0003473C
	public static CraftingStation FindClosestStationInRange(string name, Vector3 point, float range)
	{
		CraftingStation craftingStation = null;
		float num = 99999f;
		foreach (CraftingStation craftingStation2 in CraftingStation.m_allStations)
		{
			if (!(craftingStation2.m_name != name))
			{
				float num2 = Vector3.Distance(craftingStation2.transform.position, point);
				if (num2 < range && (num2 < num || craftingStation == null))
				{
					craftingStation = craftingStation2;
					num = num2;
				}
			}
		}
		return craftingStation;
	}

	// Token: 0x0600067F RID: 1663 RVA: 0x000365CC File Offset: 0x000347CC
	private List<StationExtension> GetExtensions()
	{
		if (this.m_updateExtensionTimer > 2f)
		{
			this.m_updateExtensionTimer = 0f;
			this.m_attachedExtensions.Clear();
			StationExtension.FindExtensions(this, base.transform.position, this.m_attachedExtensions);
		}
		return this.m_attachedExtensions;
	}

	// Token: 0x06000680 RID: 1664 RVA: 0x0003661C File Offset: 0x0003481C
	private void TriggerExtensionEffects()
	{
		Vector3 connectionEffectPoint = this.GetConnectionEffectPoint();
		foreach (StationExtension stationExtension in this.GetExtensions())
		{
			if (stationExtension)
			{
				stationExtension.StartConnectionEffect(connectionEffectPoint);
			}
		}
	}

	// Token: 0x06000681 RID: 1665 RVA: 0x00036680 File Offset: 0x00034880
	public Vector3 GetConnectionEffectPoint()
	{
		if (this.m_connectionPoint)
		{
			return this.m_connectionPoint.position;
		}
		return base.transform.position;
	}

	// Token: 0x06000682 RID: 1666 RVA: 0x000366A6 File Offset: 0x000348A6
	public int GetLevel()
	{
		return 1 + this.GetExtensions().Count;
	}

	// Token: 0x06000683 RID: 1667 RVA: 0x000366B5 File Offset: 0x000348B5
	public bool InUseDistance(Humanoid human)
	{
		return Vector3.Distance(human.transform.position, base.transform.position) < this.m_useDistance;
	}

	// Token: 0x0400072D RID: 1837
	public string m_name = "";

	// Token: 0x0400072E RID: 1838
	public Sprite m_icon;

	// Token: 0x0400072F RID: 1839
	public float m_discoverRange = 4f;

	// Token: 0x04000730 RID: 1840
	public float m_rangeBuild = 10f;

	// Token: 0x04000731 RID: 1841
	public bool m_craftRequireRoof = true;

	// Token: 0x04000732 RID: 1842
	public bool m_craftRequireFire = true;

	// Token: 0x04000733 RID: 1843
	public Transform m_roofCheckPoint;

	// Token: 0x04000734 RID: 1844
	public Transform m_connectionPoint;

	// Token: 0x04000735 RID: 1845
	public bool m_showBasicRecipies;

	// Token: 0x04000736 RID: 1846
	public float m_useDistance = 2f;

	// Token: 0x04000737 RID: 1847
	public int m_useAnimation;

	// Token: 0x04000738 RID: 1848
	public GameObject m_areaMarker;

	// Token: 0x04000739 RID: 1849
	public GameObject m_inUseObject;

	// Token: 0x0400073A RID: 1850
	public GameObject m_haveFireObject;

	// Token: 0x0400073B RID: 1851
	public EffectList m_craftItemEffects = new EffectList();

	// Token: 0x0400073C RID: 1852
	public EffectList m_craftItemDoneEffects = new EffectList();

	// Token: 0x0400073D RID: 1853
	public EffectList m_repairItemDoneEffects = new EffectList();

	// Token: 0x0400073E RID: 1854
	private const float m_updateExtensionInterval = 2f;

	// Token: 0x0400073F RID: 1855
	private float m_updateExtensionTimer;

	// Token: 0x04000740 RID: 1856
	private float m_useTimer = 10f;

	// Token: 0x04000741 RID: 1857
	private bool m_haveFire;

	// Token: 0x04000742 RID: 1858
	private ZNetView m_nview;

	// Token: 0x04000743 RID: 1859
	private List<StationExtension> m_attachedExtensions = new List<StationExtension>();

	// Token: 0x04000744 RID: 1860
	private static List<CraftingStation> m_allStations = new List<CraftingStation>();

	// Token: 0x04000745 RID: 1861
	private static int m_triggerMask = 0;
}
