using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Token: 0x020000DD RID: 221
public class Location : MonoBehaviour
{
	// Token: 0x06000DFB RID: 3579 RVA: 0x00063B7C File Offset: 0x00061D7C
	private void Awake()
	{
		Location.m_allLocations.Add(this);
		if (this.m_hasInterior)
		{
			Vector3 zoneCenter = this.GetZoneCenter();
			Vector3 position = new Vector3(zoneCenter.x, base.transform.position.y + 5000f, zoneCenter.z);
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_interiorPrefab, position, Quaternion.identity, base.transform);
			gameObject.transform.localScale = new Vector3(ZoneSystem.instance.m_zoneSize, 500f, ZoneSystem.instance.m_zoneSize);
			gameObject.GetComponent<EnvZone>().m_environment = this.m_interiorEnvironment;
		}
	}

	// Token: 0x06000DFC RID: 3580 RVA: 0x00063C20 File Offset: 0x00061E20
	private Vector3 GetZoneCenter()
	{
		Vector2i zone = ZoneSystem.instance.GetZone(base.transform.position);
		return ZoneSystem.instance.GetZonePos(zone);
	}

	// Token: 0x06000DFD RID: 3581 RVA: 0x00063C4E File Offset: 0x00061E4E
	private void OnDestroy()
	{
		Location.m_allLocations.Remove(this);
	}

	// Token: 0x06000DFE RID: 3582 RVA: 0x00063C5C File Offset: 0x00061E5C
	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
		Gizmos.matrix = Matrix4x4.TRS(base.transform.position + new Vector3(0f, -0.01f, 0f), Quaternion.identity, new Vector3(1f, 0.001f, 1f));
		Gizmos.DrawSphere(Vector3.zero, this.m_exteriorRadius);
		Gizmos.matrix = Matrix4x4.identity;
		Utils.DrawGizmoCircle(base.transform.position, this.m_exteriorRadius, 32);
		if (this.m_hasInterior)
		{
			Utils.DrawGizmoCircle(base.transform.position + new Vector3(0f, 5000f, 0f), this.m_interiorRadius, 32);
			Utils.DrawGizmoCircle(base.transform.position, this.m_interiorRadius, 32);
			Gizmos.matrix = Matrix4x4.TRS(base.transform.position + new Vector3(0f, 5000f, 0f), Quaternion.identity, new Vector3(1f, 0.001f, 1f));
			Gizmos.DrawSphere(Vector3.zero, this.m_interiorRadius);
			Gizmos.matrix = Matrix4x4.identity;
		}
	}

	// Token: 0x06000DFF RID: 3583 RVA: 0x00063DB5 File Offset: 0x00061FB5
	private float GetMaxRadius()
	{
		if (!this.m_hasInterior)
		{
			return this.m_exteriorRadius;
		}
		return Mathf.Max(this.m_exteriorRadius, this.m_interiorRadius);
	}

	// Token: 0x06000E00 RID: 3584 RVA: 0x00063DD8 File Offset: 0x00061FD8
	public bool IsInside(Vector3 point, float radius)
	{
		float maxRadius = this.GetMaxRadius();
		return Utils.DistanceXZ(base.transform.position, point) < maxRadius;
	}

	// Token: 0x06000E01 RID: 3585 RVA: 0x00063E00 File Offset: 0x00062000
	public static bool IsInsideLocation(Vector3 point, float distance)
	{
		using (List<Location>.Enumerator enumerator = Location.m_allLocations.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.IsInside(point, distance))
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000E02 RID: 3586 RVA: 0x00063E5C File Offset: 0x0006205C
	public static Location GetLocation(Vector3 point)
	{
		foreach (Location location in Location.m_allLocations)
		{
			if (location.IsInside(point, 0f))
			{
				return location;
			}
		}
		return null;
	}

	// Token: 0x06000E03 RID: 3587 RVA: 0x00063EBC File Offset: 0x000620BC
	public static bool IsInsideNoBuildLocation(Vector3 point)
	{
		foreach (Location location in Location.m_allLocations)
		{
			if (location.m_noBuild && location.IsInside(point, 0f))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x04000CAD RID: 3245
	[FormerlySerializedAs("m_radius")]
	public float m_exteriorRadius = 20f;

	// Token: 0x04000CAE RID: 3246
	public bool m_noBuild = true;

	// Token: 0x04000CAF RID: 3247
	public bool m_clearArea = true;

	// Token: 0x04000CB0 RID: 3248
	[Header("Other")]
	public bool m_applyRandomDamage;

	// Token: 0x04000CB1 RID: 3249
	[Header("Interior")]
	public bool m_hasInterior;

	// Token: 0x04000CB2 RID: 3250
	public float m_interiorRadius = 20f;

	// Token: 0x04000CB3 RID: 3251
	public string m_interiorEnvironment = "";

	// Token: 0x04000CB4 RID: 3252
	public GameObject m_interiorPrefab;

	// Token: 0x04000CB5 RID: 3253
	private static List<Location> m_allLocations = new List<Location>();
}
