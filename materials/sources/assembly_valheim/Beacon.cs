using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000B8 RID: 184
public class Beacon : MonoBehaviour
{
	// Token: 0x06000C58 RID: 3160 RVA: 0x00058C46 File Offset: 0x00056E46
	private void Awake()
	{
		Beacon.m_instances.Add(this);
	}

	// Token: 0x06000C59 RID: 3161 RVA: 0x00058C53 File Offset: 0x00056E53
	private void OnDestroy()
	{
		Beacon.m_instances.Remove(this);
	}

	// Token: 0x06000C5A RID: 3162 RVA: 0x00058C64 File Offset: 0x00056E64
	public static Beacon FindClosestBeaconInRange(Vector3 point)
	{
		Beacon beacon = null;
		float num = 999999f;
		foreach (Beacon beacon2 in Beacon.m_instances)
		{
			float num2 = Vector3.Distance(point, beacon2.transform.position);
			if (num2 < beacon2.m_range && (beacon == null || num2 < num))
			{
				beacon = beacon2;
				num = num2;
			}
		}
		return beacon;
	}

	// Token: 0x06000C5B RID: 3163 RVA: 0x00058CE8 File Offset: 0x00056EE8
	public static void FindBeaconsInRange(Vector3 point, List<Beacon> becons)
	{
		foreach (Beacon beacon in Beacon.m_instances)
		{
			if (Vector3.Distance(point, beacon.transform.position) < beacon.m_range)
			{
				becons.Add(beacon);
			}
		}
	}

	// Token: 0x04000B58 RID: 2904
	public float m_range = 20f;

	// Token: 0x04000B59 RID: 2905
	private static List<Beacon> m_instances = new List<Beacon>();
}
