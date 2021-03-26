using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000A4 RID: 164
public class NavmeshTest : MonoBehaviour
{
	// Token: 0x06000B25 RID: 2853 RVA: 0x000027E0 File Offset: 0x000009E0
	private void Awake()
	{
	}

	// Token: 0x06000B26 RID: 2854 RVA: 0x000503A8 File Offset: 0x0004E5A8
	private void Update()
	{
		if (Pathfinding.instance.GetPath(base.transform.position, this.m_target.position, this.m_path, this.m_agentType, false, this.m_cleanPath))
		{
			this.m_havePath = true;
			return;
		}
		this.m_havePath = false;
	}

	// Token: 0x06000B27 RID: 2855 RVA: 0x000503FC File Offset: 0x0004E5FC
	private void OnDrawGizmos()
	{
		if (this.m_target == null)
		{
			return;
		}
		if (this.m_havePath)
		{
			Gizmos.color = Color.yellow;
			for (int i = 0; i < this.m_path.Count - 1; i++)
			{
				Vector3 a = this.m_path[i];
				Vector3 a2 = this.m_path[i + 1];
				Gizmos.DrawLine(a + Vector3.up * 0.2f, a2 + Vector3.up * 0.2f);
			}
			foreach (Vector3 a3 in this.m_path)
			{
				Gizmos.DrawSphere(a3 + Vector3.up * 0.2f, 0.1f);
			}
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(base.transform.position, 0.3f);
			Gizmos.DrawSphere(this.m_target.position, 0.3f);
			return;
		}
		Gizmos.color = Color.red;
		Gizmos.DrawLine(base.transform.position + Vector3.up * 0.2f, this.m_target.position + Vector3.up * 0.2f);
		Gizmos.DrawSphere(base.transform.position, 0.3f);
		Gizmos.DrawSphere(this.m_target.position, 0.3f);
	}

	// Token: 0x04000A8B RID: 2699
	public Transform m_target;

	// Token: 0x04000A8C RID: 2700
	public Pathfinding.AgentType m_agentType = Pathfinding.AgentType.Humanoid;

	// Token: 0x04000A8D RID: 2701
	public bool m_cleanPath = true;

	// Token: 0x04000A8E RID: 2702
	private List<Vector3> m_path = new List<Vector3>();

	// Token: 0x04000A8F RID: 2703
	private bool m_havePath;
}
