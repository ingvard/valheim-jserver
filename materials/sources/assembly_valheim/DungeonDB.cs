using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x02000116 RID: 278
public class DungeonDB : MonoBehaviour
{
	// Token: 0x17000033 RID: 51
	// (get) Token: 0x06001079 RID: 4217 RVA: 0x00074BF9 File Offset: 0x00072DF9
	public static DungeonDB instance
	{
		get
		{
			return DungeonDB.m_instance;
		}
	}

	// Token: 0x0600107A RID: 4218 RVA: 0x00074C00 File Offset: 0x00072E00
	private void Awake()
	{
		DungeonDB.m_instance = this;
		SceneManager.LoadScene("rooms", LoadSceneMode.Additive);
		ZLog.Log("DungeonDB Awake " + Time.frameCount);
	}

	// Token: 0x0600107B RID: 4219 RVA: 0x00074C2C File Offset: 0x00072E2C
	public bool SkipSaving()
	{
		return this.m_error;
	}

	// Token: 0x0600107C RID: 4220 RVA: 0x00074C34 File Offset: 0x00072E34
	private void Start()
	{
		ZLog.Log("DungeonDB Start " + Time.frameCount);
		this.m_rooms = DungeonDB.SetupRooms();
		this.GenerateHashList();
	}

	// Token: 0x0600107D RID: 4221 RVA: 0x00074C60 File Offset: 0x00072E60
	public static List<DungeonDB.RoomData> GetRooms()
	{
		return DungeonDB.m_instance.m_rooms;
	}

	// Token: 0x0600107E RID: 4222 RVA: 0x00074C6C File Offset: 0x00072E6C
	private static List<DungeonDB.RoomData> SetupRooms()
	{
		GameObject[] array = Resources.FindObjectsOfTypeAll<GameObject>();
		GameObject gameObject = null;
		foreach (GameObject gameObject2 in array)
		{
			if (gameObject2.name == "_Rooms")
			{
				gameObject = gameObject2;
				break;
			}
		}
		if (gameObject == null || (DungeonDB.m_instance && gameObject.activeSelf))
		{
			if (DungeonDB.m_instance)
			{
				DungeonDB.m_instance.m_error = true;
			}
			ZLog.LogError("Rooms are fucked, missing _Rooms or its enabled");
		}
		List<DungeonDB.RoomData> list = new List<DungeonDB.RoomData>();
		for (int j = 0; j < gameObject.transform.childCount; j++)
		{
			Room component = gameObject.transform.GetChild(j).GetComponent<Room>();
			DungeonDB.RoomData roomData = new DungeonDB.RoomData();
			roomData.m_room = component;
			ZoneSystem.PrepareNetViews(component.gameObject, roomData.m_netViews);
			ZoneSystem.PrepareRandomSpawns(component.gameObject, roomData.m_randomSpawns);
			list.Add(roomData);
		}
		return list;
	}

	// Token: 0x0600107F RID: 4223 RVA: 0x00074D60 File Offset: 0x00072F60
	public DungeonDB.RoomData GetRoom(int hash)
	{
		DungeonDB.RoomData result;
		if (this.m_roomByHash.TryGetValue(hash, out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x06001080 RID: 4224 RVA: 0x00074D80 File Offset: 0x00072F80
	private void GenerateHashList()
	{
		this.m_roomByHash.Clear();
		foreach (DungeonDB.RoomData roomData in this.m_rooms)
		{
			int stableHashCode = roomData.m_room.gameObject.name.GetStableHashCode();
			this.m_roomByHash.Add(stableHashCode, roomData);
		}
	}

	// Token: 0x04000F75 RID: 3957
	private static DungeonDB m_instance;

	// Token: 0x04000F76 RID: 3958
	private List<DungeonDB.RoomData> m_rooms = new List<DungeonDB.RoomData>();

	// Token: 0x04000F77 RID: 3959
	private Dictionary<int, DungeonDB.RoomData> m_roomByHash = new Dictionary<int, DungeonDB.RoomData>();

	// Token: 0x04000F78 RID: 3960
	private bool m_error;

	// Token: 0x020001B8 RID: 440
	public class RoomData
	{
		// Token: 0x04001340 RID: 4928
		public Room m_room;

		// Token: 0x04001341 RID: 4929
		[NonSerialized]
		public List<ZNetView> m_netViews = new List<ZNetView>();

		// Token: 0x04001342 RID: 4930
		[NonSerialized]
		public List<RandomSpawn> m_randomSpawns = new List<RandomSpawn>();
	}
}
