using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000B8 RID: 184
public class Beacon : MonoBehaviour
{
	// Token: 0x06000C57 RID: 3159 RVA: 0x00058ABE File Offset: 0x00056CBE
	private void Awake()
	{
		Beacon.m_instances.Add(this);
	}

	// Token: 0x06000C58 RID: 3160 RVA: 0x00058ACB File Offset: 0x00056CCB
	private void OnDestroy()
	{
		Beacon.m_instances.Remove(this);
	}

	// Token: 0x06000C59 RID: 3161 RVA: 0x00058ADC File Offset: 0x00056CDC
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

	// Token: 0x06000C5A RID: 3162 RVA: 0x00058B60 File Offset: 0x00056D60
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

	// Token: 0x04000B52 RID: 2898
	public float m_range = 20f;

	// Token: 0x04000B53 RID: 2899
	private static List<Beacon> m_instances = new List<Beacon>();
}
