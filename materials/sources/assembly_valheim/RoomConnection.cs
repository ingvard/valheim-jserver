using System;
using UnityEngine;

// Token: 0x02000119 RID: 281
public class RoomConnection : MonoBehaviour
{
	// Token: 0x060010B0 RID: 4272 RVA: 0x000769E0 File Offset: 0x00074BE0
	private void OnDrawGizmos()
	{
		if (this.m_entrance)
		{
			Gizmos.color = Color.white;
		}
		else
		{
			Gizmos.color = new Color(1f, 1f, 0f, 1f);
		}
		Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, new Vector3(1f, 1f, 1f));
		Gizmos.DrawCube(Vector3.zero, new Vector3(2f, 0.02f, 0.2f));
		Gizmos.DrawCube(new Vector3(0f, 0f, 0.35f), new Vector3(0.2f, 0.02f, 0.5f));
		Gizmos.matrix = Matrix4x4.identity;
	}

	// Token: 0x060010B1 RID: 4273 RVA: 0x00076AA8 File Offset: 0x00074CA8
	public bool TestContact(RoomConnection other)
	{
		return Vector3.Distance(base.transform.position, other.transform.position) < 0.1f;
	}

	// Token: 0x04000FA1 RID: 4001
	public string m_type = "";

	// Token: 0x04000FA2 RID: 4002
	public bool m_entrance;

	// Token: 0x04000FA3 RID: 4003
	public bool m_allowDoor = true;

	// Token: 0x04000FA4 RID: 4004
	[NonSerialized]
	public int m_placeOrder;
}
