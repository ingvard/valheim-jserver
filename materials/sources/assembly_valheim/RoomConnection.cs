using System;
using UnityEngine;

// Token: 0x02000119 RID: 281
public class RoomConnection : MonoBehaviour
{
	// Token: 0x060010AF RID: 4271 RVA: 0x00076858 File Offset: 0x00074A58
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

	// Token: 0x060010B0 RID: 4272 RVA: 0x00076920 File Offset: 0x00074B20
	public bool TestContact(RoomConnection other)
	{
		return Vector3.Distance(base.transform.position, other.transform.position) < 0.1f;
	}

	// Token: 0x04000F9B RID: 3995
	public string m_type = "";

	// Token: 0x04000F9C RID: 3996
	public bool m_entrance;

	// Token: 0x04000F9D RID: 3997
	public bool m_allowDoor = true;

	// Token: 0x04000F9E RID: 3998
	[NonSerialized]
	public int m_placeOrder;
}
