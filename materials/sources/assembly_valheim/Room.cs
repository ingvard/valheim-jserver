using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000118 RID: 280
public class Room : MonoBehaviour
{
	// Token: 0x060010A6 RID: 4262 RVA: 0x00076648 File Offset: 0x00074848
	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, new Vector3(1f, 1f, 1f));
		Gizmos.DrawWireCube(Vector3.zero, new Vector3((float)this.m_size.x, (float)this.m_size.y, (float)this.m_size.z));
		Gizmos.matrix = Matrix4x4.identity;
	}

	// Token: 0x060010A7 RID: 4263 RVA: 0x000766E4 File Offset: 0x000748E4
	public int GetHash()
	{
		return ZNetView.GetPrefabName(base.gameObject).GetStableHashCode();
	}

	// Token: 0x060010A8 RID: 4264 RVA: 0x000766F6 File Offset: 0x000748F6
	private void OnEnable()
	{
		this.m_roomConnections = null;
	}

	// Token: 0x060010A9 RID: 4265 RVA: 0x000766FF File Offset: 0x000748FF
	public RoomConnection[] GetConnections()
	{
		if (this.m_roomConnections == null)
		{
			this.m_roomConnections = base.GetComponentsInChildren<RoomConnection>(false);
		}
		return this.m_roomConnections;
	}

	// Token: 0x060010AA RID: 4266 RVA: 0x0007671C File Offset: 0x0007491C
	public RoomConnection GetConnection(RoomConnection other)
	{
		RoomConnection[] connections = this.GetConnections();
		Room.tempConnections.Clear();
		foreach (RoomConnection roomConnection in connections)
		{
			if (roomConnection.m_type == other.m_type)
			{
				Room.tempConnections.Add(roomConnection);
			}
		}
		if (Room.tempConnections.Count == 0)
		{
			return null;
		}
		return Room.tempConnections[UnityEngine.Random.Range(0, Room.tempConnections.Count)];
	}

	// Token: 0x060010AB RID: 4267 RVA: 0x00076794 File Offset: 0x00074994
	public RoomConnection GetEntrance()
	{
		RoomConnection[] connections = this.GetConnections();
		ZLog.Log("Connections " + connections.Length);
		foreach (RoomConnection roomConnection in connections)
		{
			if (roomConnection.m_entrance)
			{
				return roomConnection;
			}
		}
		return null;
	}

	// Token: 0x060010AC RID: 4268 RVA: 0x000767E0 File Offset: 0x000749E0
	public bool HaveConnection(RoomConnection other)
	{
		RoomConnection[] connections = this.GetConnections();
		for (int i = 0; i < connections.Length; i++)
		{
			if (connections[i].m_type == other.m_type)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x04000F8E RID: 3982
	private static List<RoomConnection> tempConnections = new List<RoomConnection>();

	// Token: 0x04000F8F RID: 3983
	public Vector3Int m_size = new Vector3Int(8, 4, 8);

	// Token: 0x04000F90 RID: 3984
	[BitMask(typeof(Room.Theme))]
	public Room.Theme m_theme = Room.Theme.Crypt;

	// Token: 0x04000F91 RID: 3985
	public bool m_enabled = true;

	// Token: 0x04000F92 RID: 3986
	public bool m_entrance;

	// Token: 0x04000F93 RID: 3987
	public bool m_endCap;

	// Token: 0x04000F94 RID: 3988
	public int m_endCapPrio;

	// Token: 0x04000F95 RID: 3989
	public int m_minPlaceOrder;

	// Token: 0x04000F96 RID: 3990
	public float m_weight = 1f;

	// Token: 0x04000F97 RID: 3991
	public bool m_faceCenter;

	// Token: 0x04000F98 RID: 3992
	public bool m_perimeter;

	// Token: 0x04000F99 RID: 3993
	[NonSerialized]
	public int m_placeOrder;

	// Token: 0x04000F9A RID: 3994
	private RoomConnection[] m_roomConnections;

	// Token: 0x020001BC RID: 444
	public enum Theme
	{
		// Token: 0x04001345 RID: 4933
		Crypt = 1,
		// Token: 0x04001346 RID: 4934
		SunkenCrypt,
		// Token: 0x04001347 RID: 4935
		Cave = 4,
		// Token: 0x04001348 RID: 4936
		ForestCrypt = 8,
		// Token: 0x04001349 RID: 4937
		GoblinCamp = 16,
		// Token: 0x0400134A RID: 4938
		MeadowsVillage = 32,
		// Token: 0x0400134B RID: 4939
		MeadowsFarm = 64
	}
}
