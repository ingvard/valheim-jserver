using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000FB RID: 251
public class StationExtension : MonoBehaviour, Hoverable
{
	// Token: 0x06000F52 RID: 3922 RVA: 0x0006D22F File Offset: 0x0006B42F
	private void Awake()
	{
		if (base.GetComponent<ZNetView>().GetZDO() == null)
		{
			return;
		}
		this.m_piece = base.GetComponent<Piece>();
		StationExtension.m_allExtensions.Add(this);
	}

	// Token: 0x06000F53 RID: 3923 RVA: 0x0006D256 File Offset: 0x0006B456
	private void OnDestroy()
	{
		if (this.m_connection)
		{
			UnityEngine.Object.Destroy(this.m_connection);
			this.m_connection = null;
		}
		StationExtension.m_allExtensions.Remove(this);
	}

	// Token: 0x06000F54 RID: 3924 RVA: 0x0006D283 File Offset: 0x0006B483
	public string GetHoverText()
	{
		this.PokeEffect();
		return Localization.instance.Localize(this.m_piece.m_name);
	}

	// Token: 0x06000F55 RID: 3925 RVA: 0x0006D2A0 File Offset: 0x0006B4A0
	public string GetHoverName()
	{
		return Localization.instance.Localize(this.m_piece.m_name);
	}

	// Token: 0x06000F56 RID: 3926 RVA: 0x0006D2B7 File Offset: 0x0006B4B7
	public string GetExtensionName()
	{
		return this.m_piece.m_name;
	}

	// Token: 0x06000F57 RID: 3927 RVA: 0x0006D2C4 File Offset: 0x0006B4C4
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

	// Token: 0x06000F58 RID: 3928 RVA: 0x0006D350 File Offset: 0x0006B550
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

	// Token: 0x06000F59 RID: 3929 RVA: 0x0006D3B0 File Offset: 0x0006B5B0
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

	// Token: 0x06000F5A RID: 3930 RVA: 0x0006D42C File Offset: 0x0006B62C
	public List<CraftingStation> FindStationsInRange(Vector3 center)
	{
		List<CraftingStation> list = new List<CraftingStation>();
		CraftingStation.FindStationsInRange(this.m_craftingStation.m_name, center, this.m_maxStationDistance, list);
		return list;
	}

	// Token: 0x06000F5B RID: 3931 RVA: 0x0006D458 File Offset: 0x0006B658
	public CraftingStation FindClosestStationInRange(Vector3 center)
	{
		return CraftingStation.FindClosestStationInRange(this.m_craftingStation.m_name, center, this.m_maxStationDistance);
	}

	// Token: 0x06000F5C RID: 3932 RVA: 0x0006D474 File Offset: 0x0006B674
	private void PokeEffect()
	{
		CraftingStation craftingStation = this.FindClosestStationInRange(base.transform.position);
		if (craftingStation)
		{
			this.StartConnectionEffect(craftingStation);
		}
	}

	// Token: 0x06000F5D RID: 3933 RVA: 0x0006D4A2 File Offset: 0x0006B6A2
	public void StartConnectionEffect(CraftingStation station)
	{
		this.StartConnectionEffect(station.GetConnectionEffectPoint());
	}

	// Token: 0x06000F5E RID: 3934 RVA: 0x0006D4B0 File Offset: 0x0006B6B0
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

	// Token: 0x06000F5F RID: 3935 RVA: 0x0006D561 File Offset: 0x0006B761
	public void StopConnectionEffect()
	{
		if (this.m_connection)
		{
			UnityEngine.Object.Destroy(this.m_connection);
			this.m_connection = null;
		}
	}

	// Token: 0x06000F60 RID: 3936 RVA: 0x0006D584 File Offset: 0x0006B784
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

	// Token: 0x04000E2D RID: 3629
	public CraftingStation m_craftingStation;

	// Token: 0x04000E2E RID: 3630
	public float m_maxStationDistance = 5f;

	// Token: 0x04000E2F RID: 3631
	public GameObject m_connectionPrefab;

	// Token: 0x04000E30 RID: 3632
	private GameObject m_connection;

	// Token: 0x04000E31 RID: 3633
	private Piece m_piece;

	// Token: 0x04000E32 RID: 3634
	private Collider[] m_colliders;

	// Token: 0x04000E33 RID: 3635
	private static List<StationExtension> m_allExtensions = new List<StationExtension>();
}
