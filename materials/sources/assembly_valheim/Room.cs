using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000118 RID: 280
public class Room : MonoBehaviour
{
	// Token: 0x060010A7 RID: 4263 RVA: 0x000767D0 File Offset: 0x000749D0
	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		Gizmos.matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, new Vector3(1f, 1f, 1f));
		Gizmos.DrawWireCube(Vector3.zero, new Vector3((float)this.m_size.x, (float)this.m_size.y, (float)this.m_size.z));
		Gizmos.matrix = Matrix4x4.identity;
	}

	// Token: 0x060010A8 RID: 4264 RVA: 0x0007686C File Offset: 0x00074A6C
	public int GetHash()
	{
		return ZNetView.GetPrefabName(base.gameObject).GetStableHashCode();
	}

	// Token: 0x060010A9 RID: 4265 RVA: 0x0007687E File Offset: 0x00074A7E
	private void OnEnable()
	{
		this.m_roomConnections = null;
	}

	// Token: 0x060010AA RID: 4266 RVA: 0x00076887 File Offset: 0x00074A87
	public RoomConnection[] GetConnections()
	{
		if (this.m_roomConnections == null)
		{
			this.m_roomConnections = base.GetComponentsInChildren<RoomConnection>(false);
		}
		return this.m_roomConnections;
	}

	// Token: 0x060010AB RID: 4267 RVA: 0x000768A4 File Offset: 0x00074AA4
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

	// Token: 0x060010AC RID: 4268 RVA: 0x0007691C File Offset: 0x00074B1C
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

	// Token: 0x060010AD RID: 4269 RVA: 0x00076968 File Offset: 0x00074B68
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

	// Token: 0x04000F94 RID: 3988
	private static List<RoomConnection> tempConnections = new List<RoomConnection>();

	// Token: 0x04000F95 RID: 3989
	public Vector3Int m_size = new Vector3Int(8, 4, 8);

	// Token: 0x04000F96 RID: 3990
	[BitMask(typeof(Room.Theme))]
	public Room.Theme m_theme = Room.Theme.Crypt;

	// Token: 0x04000F97 RID: 3991
	public bool m_enabled = true;

	// Token: 0x04000F98 RID: 3992
	public bool m_entrance;

	// Token: 0x04000F99 RID: 3993
	public bool m_endCap;

	// Token: 0x04000F9A RID: 3994
	public int m_endCapPrio;

	// Token: 0x04000F9B RID: 3995
	public int m_minPlaceOrder;

	// Token: 0x04000F9C RID: 3996
	public float m_weight = 1f;

	// Token: 0x04000F9D RID: 3997
	public bool m_faceCenter;

	// Token: 0x04000F9E RID: 3998
	public bool m_perimeter;

	// Token: 0x04000F9F RID: 3999
	[NonSerialized]
	public int m_placeOrder;

	// Token: 0x04000FA0 RID: 4000
	private RoomConnection[] m_roomConnections;

	// Token: 0x020001BC RID: 444
	public enum Theme
	{
		// Token: 0x0400134C RID: 4940
		Crypt = 1,
		// Token: 0x0400134D RID: 4941
		SunkenCrypt,
		// Token: 0x0400134E RID: 4942
		Cave = 4,
		// Token: 0x0400134F RID: 4943
		ForestCrypt = 8,
		// Token: 0x04001350 RID: 4944
		GoblinCamp = 16,
		// Token: 0x04001351 RID: 4945
		MeadowsVillage = 32,
		// Token: 0x04001352 RID: 4946
		MeadowsFarm = 64
	}
}
