using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Token: 0x020000DD RID: 221
public class Location : MonoBehaviour
{
	// Token: 0x06000DFA RID: 3578 RVA: 0x000639F4 File Offset: 0x00061BF4
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

	// Token: 0x06000DFB RID: 3579 RVA: 0x00063A98 File Offset: 0x00061C98
	private Vector3 GetZoneCenter()
	{
		Vector2i zone = ZoneSystem.instance.GetZone(base.transform.position);
		return ZoneSystem.instance.GetZonePos(zone);
	}

	// Token: 0x06000DFC RID: 3580 RVA: 0x00063AC6 File Offset: 0x00061CC6
	private void OnDestroy()
	{
		Location.m_allLocations.Remove(this);
	}

	// Token: 0x06000DFD RID: 3581 RVA: 0x00063AD4 File Offset: 0x00061CD4
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

	// Token: 0x06000DFE RID: 3582 RVA: 0x00063C2D File Offset: 0x00061E2D
	private float GetMaxRadius()
	{
		if (!this.m_hasInterior)
		{
			return this.m_exteriorRadius;
		}
		return Mathf.Max(this.m_exteriorRadius, this.m_interiorRadius);
	}

	// Token: 0x06000DFF RID: 3583 RVA: 0x00063C50 File Offset: 0x00061E50
	public bool IsInside(Vector3 point, float radius)
	{
		float maxRadius = this.GetMaxRadius();
		return Utils.DistanceXZ(base.transform.position, point) < maxRadius;
	}

	// Token: 0x06000E00 RID: 3584 RVA: 0x00063C78 File Offset: 0x00061E78
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

	// Token: 0x06000E01 RID: 3585 RVA: 0x00063CD4 File Offset: 0x00061ED4
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

	// Token: 0x06000E02 RID: 3586 RVA: 0x00063D34 File Offset: 0x00061F34
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

	// Token: 0x04000CA7 RID: 3239
	[FormerlySerializedAs("m_radius")]
	public float m_exteriorRadius = 20f;

	// Token: 0x04000CA8 RID: 3240
	public bool m_noBuild = true;

	// Token: 0x04000CA9 RID: 3241
	public bool m_clearArea = true;

	// Token: 0x04000CAA RID: 3242
	[Header("Other")]
	public bool m_applyRandomDamage;

	// Token: 0x04000CAB RID: 3243
	[Header("Interior")]
	public bool m_hasInterior;

	// Token: 0x04000CAC RID: 3244
	public float m_interiorRadius = 20f;

	// Token: 0x04000CAD RID: 3245
	public string m_interiorEnvironment = "";

	// Token: 0x04000CAE RID: 3246
	public GameObject m_interiorPrefab;

	// Token: 0x04000CAF RID: 3247
	private static List<Location> m_allLocations = new List<Location>();
}
