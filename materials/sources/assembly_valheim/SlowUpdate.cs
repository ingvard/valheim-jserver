using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000AD RID: 173
public class SlowUpdate : MonoBehaviour
{
	// Token: 0x06000BBA RID: 3002 RVA: 0x00053A58 File Offset: 0x00051C58
	public virtual void Awake()
	{
		SlowUpdate.m_allInstances.Add(this);
		this.m_myIndex = SlowUpdate.m_allInstances.Count - 1;
	}

	// Token: 0x06000BBB RID: 3003 RVA: 0x00053A78 File Offset: 0x00051C78
	public virtual void OnDestroy()
	{
		if (this.m_myIndex != -1)
		{
			SlowUpdate.m_allInstances[this.m_myIndex] = SlowUpdate.m_allInstances[SlowUpdate.m_allInstances.Count - 1];
			SlowUpdate.m_allInstances[this.m_myIndex].m_myIndex = this.m_myIndex;
			SlowUpdate.m_allInstances.RemoveAt(SlowUpdate.m_allInstances.Count - 1);
		}
	}

	// Token: 0x06000BBC RID: 3004 RVA: 0x000027E0 File Offset: 0x000009E0
	public virtual void SUpdate()
	{
	}

	// Token: 0x06000BBD RID: 3005 RVA: 0x00053AE5 File Offset: 0x00051CE5
	public static List<SlowUpdate> GetAllInstaces()
	{
		return SlowUpdate.m_allInstances;
	}

	// Token: 0x04000AED RID: 2797
	private static List<SlowUpdate> m_allInstances = new List<SlowUpdate>();

	// Token: 0x04000AEE RID: 2798
	private int m_myIndex = -1;
}
