using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000F8 RID: 248
[ExecuteInEditMode]
public class SnapToGround : MonoBehaviour
{
	// Token: 0x06000F43 RID: 3907 RVA: 0x0006CE6E File Offset: 0x0006B06E
	private void Awake()
	{
		SnapToGround.m_allSnappers.Add(this);
		this.m_inList = true;
	}

	// Token: 0x06000F44 RID: 3908 RVA: 0x0006CE82 File Offset: 0x0006B082
	private void OnDestroy()
	{
		if (this.m_inList)
		{
			SnapToGround.m_allSnappers.Remove(this);
			this.m_inList = false;
		}
	}

	// Token: 0x06000F45 RID: 3909 RVA: 0x0006CEA0 File Offset: 0x0006B0A0
	private void Snap()
	{
		if (ZoneSystem.instance == null)
		{
			return;
		}
		float groundHeight = ZoneSystem.instance.GetGroundHeight(base.transform.position);
		Vector3 position = base.transform.position;
		position.y = groundHeight + this.m_offset;
		base.transform.position = position;
		ZNetView component = base.GetComponent<ZNetView>();
		if (component != null && component.IsOwner())
		{
			component.GetZDO().SetPosition(position);
		}
	}

	// Token: 0x06000F46 RID: 3910 RVA: 0x0006CF1C File Offset: 0x0006B11C
	public bool HaveUnsnapped()
	{
		return SnapToGround.m_allSnappers.Count > 0;
	}

	// Token: 0x06000F47 RID: 3911 RVA: 0x0006CF2C File Offset: 0x0006B12C
	public static void SnappAll()
	{
		if (SnapToGround.m_allSnappers.Count == 0)
		{
			return;
		}
		Heightmap.ForceGenerateAll();
		foreach (SnapToGround snapToGround in SnapToGround.m_allSnappers)
		{
			snapToGround.Snap();
			snapToGround.m_inList = false;
		}
		SnapToGround.m_allSnappers.Clear();
	}

	// Token: 0x04000E24 RID: 3620
	public float m_offset;

	// Token: 0x04000E25 RID: 3621
	private static List<SnapToGround> m_allSnappers = new List<SnapToGround>();

	// Token: 0x04000E26 RID: 3622
	private bool m_inList;
}
