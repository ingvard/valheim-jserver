using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000FB RID: 251
public class StationExtension : MonoBehaviour, Hoverable
{
	// Token: 0x06000F53 RID: 3923 RVA: 0x0006D3B7 File Offset: 0x0006B5B7
	private void Awake()
	{
		if (base.GetComponent<ZNetView>().GetZDO() == null)
		{
			return;
		}
		this.m_piece = base.GetComponent<Piece>();
		StationExtension.m_allExtensions.Add(this);
	}

	// Token: 0x06000F54 RID: 3924 RVA: 0x0006D3DE File Offset: 0x0006B5DE
	private void OnDestroy()
	{
		if (this.m_connection)
		{
			UnityEngine.Object.Destroy(this.m_connection);
			this.m_connection = null;
		}
		StationExtension.m_allExtensions.Remove(this);
	}

	// Token: 0x06000F55 RID: 3925 RVA: 0x0006D40B File Offset: 0x0006B60B
	public string GetHoverText()
	{
		this.PokeEffect();
		return Localization.instance.Localize(this.m_piece.m_name);
	}

	// Token: 0x06000F56 RID: 3926 RVA: 0x0006D428 File Offset: 0x0006B628
	public string GetHoverName()
	{
		return Localization.instance.Localize(this.m_piece.m_name);
	}

	// Token: 0x06000F57 RID: 3927 RVA: 0x0006D43F File Offset: 0x0006B63F
	public string GetExtensionName()
	{
		return this.m_piece.m_name;
	}

	// Token: 0x06000F58 RID: 3928 RVA: 0x0006D44C File Offset: 0x0006B64C
	public static void FindExtensions(CraftingStation station, Vector3 pos, List<StationExtension> extensions)
	{
		foreach (StationExtension stationExtension in StationExtension.m_allExtensions)
		{
			if (Vector3.Distance(stationExtension.transform.position, pos) < stationExtension.m_maxStationDistance && stationExtension.m_craftingStation.m_name == station.m_name && !StationExtension.ExtensionInList(extensions, stationExtension))
			{
				extensions.Add(stationExtension);
			}
		}
	}

	// Token: 0x06000F59 RID: 3929 RVA: 0x0006D4D8 File Offset: 0x0006B6D8
	private static bool ExtensionInList(List<StationExtension> extensions, StationExtension extension)
	{
		using (List<StationExtension>.Enumerator enumerator = extensions.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.GetExtensionName() == extension.GetExtensionName())
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000F5A RID: 3930 RVA: 0x0006D538 File Offset: 0x0006B738
	public bool OtherExtensionInRange(float radius)
	{
		foreach (StationExtension stationExtension in StationExtension.m_allExtensions)
		{
			if (!(stationExtension == this) && Vector3.Distance(stationExtension.transform.position, base.transform.position) < radius)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000F5B RID: 3931 RVA: 0x0006D5B4 File Offset: 0x0006B7B4
	public List<CraftingStation> FindStationsInRange(Vector3 center)
	{
		List<CraftingStation> list = new List<CraftingStation>();
		CraftingStation.FindStationsInRange(this.m_craftingStation.m_name, center, this.m_maxStationDistance, list);
		return list;
	}

	// Token: 0x06000F5C RID: 3932 RVA: 0x0006D5E0 File Offset: 0x0006B7E0
	public CraftingStation FindClosestStationInRange(Vector3 center)
	{
		return CraftingStation.FindClosestStationInRange(this.m_craftingStation.m_name, center, this.m_maxStationDistance);
	}

	// Token: 0x06000F5D RID: 3933 RVA: 0x0006D5FC File Offset: 0x0006B7FC
	private void PokeEffect()
	{
		CraftingStation craftingStation = this.FindClosestStationInRange(base.transform.position);
		if (craftingStation)
		{
			this.StartConnectionEffect(craftingStation);
		}
	}

	// Token: 0x06000F5E RID: 3934 RVA: 0x0006D62A File Offset: 0x0006B82A
	public void StartConnectionEffect(CraftingStation station)
	{
		this.StartConnectionEffect(station.GetConnectionEffectPoint());
	}

	// Token: 0x06000F5F RID: 3935 RVA: 0x0006D638 File Offset: 0x0006B838
	public void StartConnectionEffect(Vector3 targetPos)
	{
		Vector3 center = this.GetCenter();
		if (this.m_connection == null)
		{
			this.m_connection = UnityEngine.Object.Instantiate<GameObject>(this.m_connectionPrefab, center, Quaternion.identity);
		}
		Vector3 vector = targetPos - center;
		Quaternion rotation = Quaternion.LookRotation(vector.normalized);
		this.m_connection.transform.position = center;
		this.m_connection.transform.rotation = rotation;
		this.m_connection.transform.localScale = new Vector3(1f, 1f, vector.magnitude);
		base.CancelInvoke("StopConnectionEffect");
		base.Invoke("StopConnectionEffect", 1f);
	}

	// Token: 0x06000F60 RID: 3936 RVA: 0x0006D6E9 File Offset: 0x0006B8E9
	public void StopConnectionEffect()
	{
		if (this.m_connection)
		{
			UnityEngine.Object.Destroy(this.m_connection);
			this.m_connection = null;
		}
	}

	// Token: 0x06000F61 RID: 3937 RVA: 0x0006D70C File Offset: 0x0006B90C
	private Vector3 GetCenter()
	{
		if (this.m_colliders == null)
		{
			this.m_colliders = base.GetComponentsInChildren<Collider>();
		}
		Vector3 position = base.transform.position;
		foreach (Collider collider in this.m_colliders)
		{
			if (collider.bounds.max.y > position.y)
			{
				position.y = collider.bounds.max.y;
			}
		}
		return position;
	}

	// Token: 0x04000E33 RID: 3635
	public CraftingStation m_craftingStation;

	// Token: 0x04000E34 RID: 3636
	public float m_maxStationDistance = 5f;

	// Token: 0x04000E35 RID: 3637
	public GameObject m_connectionPrefab;

	// Token: 0x04000E36 RID: 3638
	private GameObject m_connection;

	// Token: 0x04000E37 RID: 3639
	private Piece m_piece;

	// Token: 0x04000E38 RID: 3640
	private Collider[] m_colliders;

	// Token: 0x04000E39 RID: 3641
	private static List<StationExtension> m_allExtensions = new List<StationExtension>();
}
