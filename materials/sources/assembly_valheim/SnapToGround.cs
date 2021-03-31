using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000F8 RID: 248
[ExecuteInEditMode]
public class SnapToGround : MonoBehaviour
{
	// Token: 0x06000F44 RID: 3908 RVA: 0x0006CFF6 File Offset: 0x0006B1F6
	private void Awake()
	{
		SnapToGround.m_allSnappers.Add(this);
		this.m_inList = true;
	}

	// Token: 0x06000F45 RID: 3909 RVA: 0x0006D00A File Offset: 0x0006B20A
	private void OnDestroy()
	{
		if (this.m_inList)
		{
			SnapToGround.m_allSnappers.Remove(this);
			this.m_inList = false;
		}
	}

	// Token: 0x06000F46 RID: 3910 RVA: 0x0006D028 File Offset: 0x0006B228
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

	// Token: 0x06000F47 RID: 3911 RVA: 0x0006D0A4 File Offset: 0x0006B2A4
	public bool HaveUnsnapped()
	{
		return SnapToGround.m_allSnappers.Count > 0;
	}

	// Token: 0x06000F48 RID: 3912 RVA: 0x0006D0B4 File Offset: 0x0006B2B4
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

	// Token: 0x04000E2A RID: 3626
	public float m_offset;

	// Token: 0x04000E2B RID: 3627
	private static List<SnapToGround> m_allSnappers = new List<SnapToGround>();

	// Token: 0x04000E2C RID: 3628
	private bool m_inList;
}
