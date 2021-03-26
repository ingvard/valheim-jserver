using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x0200009D RID: 157
public class Game : MonoBehaviour
{
	// Token: 0x17000026 RID: 38
	// (get) Token: 0x06000AAE RID: 2734 RVA: 0x0004D0AC File Offset: 0x0004B2AC
	public static Game instance
	{
		get
		{
			return Game.m_instance;
		}
	}

	// Token: 0x06000AAF RID: 2735 RVA: 0x0004D0B4 File Offset: 0x0004B2B4
	private void Awake()
	{
		Game.m_instance = this;
		ZInput.Initialize();
		if (!global::Console.instance)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.m_consolePrefab);
		}
		if (string.IsNullOrEmpty(Game.m_profileFilename))
		{
			this.m_playerProfile = new PlayerProfile("Developer");
			this.m_playerProfile.SetName("Odev");
			this.m_playerProfile.Load();
		}
		else
		{
			ZLog.Log("Loading player profile " + Game.m_profileFilename);
			this.m_playerProfile = new PlayerProfile(Game.m_profileFilename);
			this.m_playerProfile.Load();
		}
		base.InvokeRepeating("CollectResources", 600f, 600f);
		Gogan.LogEvent("Screen", "Enter", "InGame", 0L);
		Gogan.LogEvent("Game", "InputMode", ZInput.IsGamepadActive() ? "Gamepad" : "MK", 0L);
	}

	// Token: 0x06000AB0 RID: 2736 RVA: 0x0004D19D File Offset: 0x0004B39D
	private void OnDestroy()
	{
		Game.m_instance = null;
	}

	// Token: 0x06000AB1 RID: 2737 RVA: 0x0004D1A8 File Offset: 0x0004B3A8
	private void Start()
	{
		Application.targetFrameRate = -1;
		ZRoutedRpc.instance.Register("SleepStart", new Action<long>(this.SleepStart));
		ZRoutedRpc.instance.Register("SleepStop", new Action<long>(this.SleepStop));
		ZRoutedRpc.instance.Register<float>("Ping", new Action<long, float>(this.RPC_Ping));
		ZRoutedRpc.instance.Register<float>("Pong", new Action<long, float>(this.RPC_Pong));
		ZRoutedRpc.instance.Register<string, int, Vector3>("DiscoverLocationRespons", new Action<long, string, int, Vector3>(this.RPC_DiscoverLocationRespons));
		if (ZNet.instance.IsServer())
		{
			ZRoutedRpc.instance.Register<string, Vector3, string, int>("DiscoverClosestLocation", new RoutedMethod<string, Vector3, string, int>.Method(this.RPC_DiscoverClosestLocation));
			base.StartCoroutine("ConnectPortals");
			base.InvokeRepeating("UpdateSleeping", 2f, 2f);
		}
	}

	// Token: 0x06000AB2 RID: 2738 RVA: 0x0004D28C File Offset: 0x0004B48C
	private void ServerLog()
	{
		int peerConnections = ZNet.instance.GetPeerConnections();
		int num = ZDOMan.instance.NrOfObjects();
		int sentZDOs = ZDOMan.instance.GetSentZDOs();
		int recvZDOs = ZDOMan.instance.GetRecvZDOs();
		ZLog.Log(string.Concat(new object[]
		{
			" Connections ",
			peerConnections,
			" ZDOS:",
			num,
			"  sent:",
			sentZDOs,
			" recv:",
			recvZDOs
		}));
	}

	// Token: 0x06000AB3 RID: 2739 RVA: 0x0004D319 File Offset: 0x0004B519
	private void CollectResources()
	{
		Resources.UnloadUnusedAssets();
	}

	// Token: 0x06000AB4 RID: 2740 RVA: 0x0004D321 File Offset: 0x0004B521
	public void Logout()
	{
		if (this.m_shuttingDown)
		{
			return;
		}
		this.Shutdown();
		SceneManager.LoadScene("start");
	}

	// Token: 0x06000AB5 RID: 2741 RVA: 0x0004D33C File Offset: 0x0004B53C
	public bool IsShuttingDown()
	{
		return this.m_shuttingDown;
	}

	// Token: 0x06000AB6 RID: 2742 RVA: 0x0004D344 File Offset: 0x0004B544
	private void OnApplicationQuit()
	{
		if (this.m_shuttingDown)
		{
			return;
		}
		ZLog.Log("Game - OnApplicationQuit");
		this.Shutdown();
		Thread.Sleep(2000);
	}

	// Token: 0x06000AB7 RID: 2743 RVA: 0x0004D369 File Offset: 0x0004B569
	private void Shutdown()
	{
		if (this.m_shuttingDown)
		{
			return;
		}
		this.m_shuttingDown = true;
		this.SavePlayerProfile(true);
		ZNetScene.instance.Shutdown();
		ZNet.instance.Shutdown();
	}

	// Token: 0x06000AB8 RID: 2744 RVA: 0x0004D398 File Offset: 0x0004B598
	private void SavePlayerProfile(bool setLogoutPoint)
	{
		if (Player.m_localPlayer)
		{
			this.m_playerProfile.SavePlayerData(Player.m_localPlayer);
			Minimap.instance.SaveMapData();
			if (setLogoutPoint)
			{
				this.m_playerProfile.SaveLogoutPoint();
			}
		}
		this.m_playerProfile.Save();
	}

	// Token: 0x06000AB9 RID: 2745 RVA: 0x0004D3E8 File Offset: 0x0004B5E8
	private Player SpawnPlayer(Vector3 spawnPoint)
	{
		ZLog.DevLog("Spawning player:" + Time.frameCount);
		Player component = UnityEngine.Object.Instantiate<GameObject>(this.m_playerPrefab, spawnPoint, Quaternion.identity).GetComponent<Player>();
		component.SetLocalPlayer();
		this.m_playerProfile.LoadPlayerData(component);
		ZNet.instance.SetCharacterID(component.GetZDOID());
		component.OnSpawned();
		return component;
	}

	// Token: 0x06000ABA RID: 2746 RVA: 0x0004D450 File Offset: 0x0004B650
	private Bed FindBedNearby(Vector3 point, float maxDistance)
	{
		foreach (Bed bed in UnityEngine.Object.FindObjectsOfType<Bed>())
		{
			if (bed.IsCurrent())
			{
				return bed;
			}
		}
		return null;
	}

	// Token: 0x06000ABB RID: 2747 RVA: 0x0004D480 File Offset: 0x0004B680
	private bool FindSpawnPoint(out Vector3 point, out bool usedLogoutPoint, float dt)
	{
		this.m_respawnWait += dt;
		usedLogoutPoint = false;
		if (this.m_playerProfile.HaveLogoutPoint())
		{
			Vector3 logoutPoint = this.m_playerProfile.GetLogoutPoint();
			ZNet.instance.SetReferencePosition(logoutPoint);
			if (this.m_respawnWait <= 8f || !ZNetScene.instance.IsAreaReady(logoutPoint))
			{
				point = Vector3.zero;
				return false;
			}
			float num;
			if (!ZoneSystem.instance.GetGroundHeight(logoutPoint, out num))
			{
				ZLog.Log("Invalid spawn point, no ground " + logoutPoint);
				this.m_respawnWait = 0f;
				this.m_playerProfile.ClearLoguoutPoint();
				point = Vector3.zero;
				return false;
			}
			this.m_playerProfile.ClearLoguoutPoint();
			point = logoutPoint;
			if (point.y < num)
			{
				point.y = num;
			}
			point.y += 0.25f;
			usedLogoutPoint = true;
			ZLog.Log("Spawned after " + this.m_respawnWait);
			return true;
		}
		else if (this.m_playerProfile.HaveCustomSpawnPoint())
		{
			Vector3 customSpawnPoint = this.m_playerProfile.GetCustomSpawnPoint();
			ZNet.instance.SetReferencePosition(customSpawnPoint);
			if (this.m_respawnWait <= 8f || !ZNetScene.instance.IsAreaReady(customSpawnPoint))
			{
				point = Vector3.zero;
				return false;
			}
			Bed bed = this.FindBedNearby(customSpawnPoint, 5f);
			if (bed != null)
			{
				ZLog.Log("Found bed at custom spawn point");
				point = bed.GetSpawnPoint();
				return true;
			}
			ZLog.Log("Failed to find bed at custom spawn point, using original");
			this.m_playerProfile.ClearCustomSpawnPoint();
			this.m_respawnWait = 0f;
			point = Vector3.zero;
			return false;
		}
		else
		{
			Vector3 a;
			if (ZoneSystem.instance.GetLocationIcon(this.m_StartLocation, out a))
			{
				point = a + Vector3.up * 2f;
				ZNet.instance.SetReferencePosition(point);
				return ZNetScene.instance.IsAreaReady(point);
			}
			ZNet.instance.SetReferencePosition(Vector3.zero);
			point = Vector3.zero;
			return false;
		}
	}

	// Token: 0x06000ABC RID: 2748 RVA: 0x0004D69E File Offset: 0x0004B89E
	private static Vector3 GetPointOnCircle(float distance, float angle)
	{
		return new Vector3(Mathf.Sin(angle) * distance, 0f, Mathf.Cos(angle) * distance);
	}

	// Token: 0x06000ABD RID: 2749 RVA: 0x0004D6BA File Offset: 0x0004B8BA
	public void RequestRespawn(float delay)
	{
		base.CancelInvoke("_RequestRespawn");
		base.Invoke("_RequestRespawn", delay);
	}

	// Token: 0x06000ABE RID: 2750 RVA: 0x0004D6D4 File Offset: 0x0004B8D4
	private void _RequestRespawn()
	{
		ZLog.Log("Starting respawn");
		if (Player.m_localPlayer)
		{
			this.m_playerProfile.SavePlayerData(Player.m_localPlayer);
		}
		if (Player.m_localPlayer)
		{
			ZNetScene.instance.Destroy(Player.m_localPlayer.gameObject);
			ZNet.instance.SetCharacterID(ZDOID.None);
		}
		this.m_respawnWait = 0f;
		this.m_requestRespawn = true;
		MusicMan.instance.TriggerMusic("respawn");
	}

	// Token: 0x06000ABF RID: 2751 RVA: 0x0004D757 File Offset: 0x0004B957
	private void Update()
	{
		ZInput.Update(Time.deltaTime);
		this.UpdateSaving(Time.deltaTime);
	}

	// Token: 0x06000AC0 RID: 2752 RVA: 0x0004D770 File Offset: 0x0004B970
	private void FixedUpdate()
	{
		if (!this.m_haveSpawned && ZNet.GetConnectionStatus() == ZNet.ConnectionStatus.Connected)
		{
			this.m_haveSpawned = true;
			this.RequestRespawn(0f);
		}
		ZInput.FixedUpdate(Time.fixedDeltaTime);
		if (ZNet.GetConnectionStatus() != ZNet.ConnectionStatus.Connecting && ZNet.GetConnectionStatus() != ZNet.ConnectionStatus.Connected)
		{
			ZLog.Log("Lost connection to server:" + ZNet.GetConnectionStatus());
			this.Logout();
			return;
		}
		this.UpdateRespawn(Time.fixedDeltaTime);
	}

	// Token: 0x06000AC1 RID: 2753 RVA: 0x0004D7E4 File Offset: 0x0004B9E4
	private void UpdateSaving(float dt)
	{
		this.m_saveTimer += dt;
		if (this.m_saveTimer > 1200f)
		{
			this.m_saveTimer = 0f;
			this.SavePlayerProfile(false);
			if (ZNet.instance)
			{
				ZNet.instance.Save(false);
			}
		}
	}

	// Token: 0x06000AC2 RID: 2754 RVA: 0x0004D838 File Offset: 0x0004BA38
	private void UpdateRespawn(float dt)
	{
		Vector3 vector;
		bool flag;
		if (this.m_requestRespawn && this.FindSpawnPoint(out vector, out flag, dt))
		{
			if (!flag)
			{
				this.m_playerProfile.SetHomePoint(vector);
			}
			this.SpawnPlayer(vector);
			this.m_requestRespawn = false;
			if (this.m_firstSpawn)
			{
				this.m_firstSpawn = false;
				Chat.instance.SendText(Talker.Type.Shout, "I have arrived!");
			}
			GC.Collect();
		}
	}

	// Token: 0x06000AC3 RID: 2755 RVA: 0x0004D89C File Offset: 0x0004BA9C
	public bool WaitingForRespawn()
	{
		return this.m_requestRespawn;
	}

	// Token: 0x06000AC4 RID: 2756 RVA: 0x0004D8A4 File Offset: 0x0004BAA4
	public PlayerProfile GetPlayerProfile()
	{
		return this.m_playerProfile;
	}

	// Token: 0x06000AC5 RID: 2757 RVA: 0x0004D8AC File Offset: 0x0004BAAC
	public static void SetProfile(string filename)
	{
		Game.m_profileFilename = filename;
	}

	// Token: 0x06000AC6 RID: 2758 RVA: 0x0004D8B4 File Offset: 0x0004BAB4
	private IEnumerator ConnectPortals()
	{
		for (;;)
		{
			this.m_tempPortalList.Clear();
			int index = 0;
			bool done = false;
			do
			{
				done = ZDOMan.instance.GetAllZDOsWithPrefabIterative(this.m_portalPrefab.name, this.m_tempPortalList, ref index);
				yield return null;
			}
			while (!done);
			foreach (ZDO zdo in this.m_tempPortalList)
			{
				ZDOID zdoid = zdo.GetZDOID("target");
				string @string = zdo.GetString("tag", "");
				if (!zdoid.IsNone())
				{
					ZDO zdo2 = ZDOMan.instance.GetZDO(zdoid);
					if (zdo2 == null || zdo2.GetString("tag", "") != @string)
					{
						zdo.SetOwner(ZDOMan.instance.GetMyID());
						zdo.Set("target", ZDOID.None);
						ZDOMan.instance.ForceSendZDO(zdo.m_uid);
					}
				}
			}
			foreach (ZDO zdo3 in this.m_tempPortalList)
			{
				string string2 = zdo3.GetString("tag", "");
				if (zdo3.GetZDOID("target").IsNone())
				{
					ZDO zdo4 = this.FindRandomUnconnectedPortal(this.m_tempPortalList, zdo3, string2);
					if (zdo4 != null)
					{
						zdo3.SetOwner(ZDOMan.instance.GetMyID());
						zdo4.SetOwner(ZDOMan.instance.GetMyID());
						zdo3.Set("target", zdo4.m_uid);
						zdo4.Set("target", zdo3.m_uid);
						ZDOMan.instance.ForceSendZDO(zdo3.m_uid);
						ZDOMan.instance.ForceSendZDO(zdo4.m_uid);
					}
				}
			}
			yield return new WaitForSeconds(5f);
		}
		yield break;
	}

	// Token: 0x06000AC7 RID: 2759 RVA: 0x0004D8C4 File Offset: 0x0004BAC4
	private ZDO FindRandomUnconnectedPortal(List<ZDO> portals, ZDO skip, string tag)
	{
		List<ZDO> list = new List<ZDO>();
		foreach (ZDO zdo in portals)
		{
			if (zdo != skip && zdo.GetZDOID("target").IsNone() && !(zdo.GetString("tag", "") != tag))
			{
				list.Add(zdo);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	// Token: 0x06000AC8 RID: 2760 RVA: 0x0004D968 File Offset: 0x0004BB68
	private ZDO FindClosestUnconnectedPortal(List<ZDO> portals, ZDO skip, Vector3 refPos)
	{
		ZDO zdo = null;
		float num = 99999f;
		foreach (ZDO zdo2 in portals)
		{
			if (zdo2 != skip && zdo2.GetZDOID("target").IsNone())
			{
				float num2 = Vector3.Distance(refPos, zdo2.GetPosition());
				if (zdo == null || num2 < num)
				{
					zdo = zdo2;
					num = num2;
				}
			}
		}
		return zdo;
	}

	// Token: 0x06000AC9 RID: 2761 RVA: 0x0004D9F0 File Offset: 0x0004BBF0
	private void UpdateSleeping()
	{
		if (!ZNet.instance.IsServer())
		{
			return;
		}
		if (this.m_sleeping)
		{
			if (!EnvMan.instance.IsTimeSkipping())
			{
				this.m_sleeping = false;
				ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SleepStop", Array.Empty<object>());
				return;
			}
		}
		else if (!EnvMan.instance.IsTimeSkipping())
		{
			if (!EnvMan.instance.IsAfternoon() && !EnvMan.instance.IsNight())
			{
				return;
			}
			if (!this.EverybodyIsTryingToSleep())
			{
				return;
			}
			EnvMan.instance.SkipToMorning();
			this.m_sleeping = true;
			ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SleepStart", Array.Empty<object>());
		}
	}

	// Token: 0x06000ACA RID: 2762 RVA: 0x0004DA98 File Offset: 0x0004BC98
	private bool EverybodyIsTryingToSleep()
	{
		List<ZDO> allCharacterZDOS = ZNet.instance.GetAllCharacterZDOS();
		if (allCharacterZDOS.Count == 0)
		{
			return false;
		}
		using (List<ZDO>.Enumerator enumerator = allCharacterZDOS.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (!enumerator.Current.GetBool("inBed", false))
				{
					return false;
				}
			}
		}
		return true;
	}

	// Token: 0x06000ACB RID: 2763 RVA: 0x0004DB08 File Offset: 0x0004BD08
	private void SleepStart(long sender)
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer)
		{
			localPlayer.SetSleeping(true);
		}
	}

	// Token: 0x06000ACC RID: 2764 RVA: 0x0004DB2C File Offset: 0x0004BD2C
	private void SleepStop(long sender)
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer)
		{
			localPlayer.SetSleeping(false);
			localPlayer.AttachStop();
		}
	}

	// Token: 0x06000ACD RID: 2765 RVA: 0x0004DB54 File Offset: 0x0004BD54
	public void DiscoverClosestLocation(string name, Vector3 point, string pinName, int pinType)
	{
		ZLog.Log("DiscoverClosestLocation");
		ZRoutedRpc.instance.InvokeRoutedRPC("DiscoverClosestLocation", new object[]
		{
			name,
			point,
			pinName,
			pinType
		});
	}

	// Token: 0x06000ACE RID: 2766 RVA: 0x0004DB90 File Offset: 0x0004BD90
	private void RPC_DiscoverClosestLocation(long sender, string name, Vector3 point, string pinName, int pinType)
	{
		ZoneSystem.LocationInstance locationInstance;
		if (ZoneSystem.instance.FindClosestLocation(name, point, out locationInstance))
		{
			ZLog.Log("Found location of type " + name);
			ZRoutedRpc.instance.InvokeRoutedRPC(sender, "DiscoverLocationRespons", new object[]
			{
				pinName,
				pinType,
				locationInstance.m_position
			});
			return;
		}
		ZLog.LogWarning("Failed to find location of type " + name);
	}

	// Token: 0x06000ACF RID: 2767 RVA: 0x0004DC01 File Offset: 0x0004BE01
	private void RPC_DiscoverLocationRespons(long sender, string pinName, int pinType, Vector3 pos)
	{
		Minimap.instance.DiscoverLocation(pos, (Minimap.PinType)pinType, pinName);
	}

	// Token: 0x06000AD0 RID: 2768 RVA: 0x0004DC12 File Offset: 0x0004BE12
	public void Ping()
	{
		if (global::Console.instance)
		{
			global::Console.instance.Print("Ping sent to server");
		}
		ZRoutedRpc.instance.InvokeRoutedRPC("Ping", new object[]
		{
			Time.time
		});
	}

	// Token: 0x06000AD1 RID: 2769 RVA: 0x0004DC51 File Offset: 0x0004BE51
	private void RPC_Ping(long sender, float time)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC(sender, "Pong", new object[]
		{
			time
		});
	}

	// Token: 0x06000AD2 RID: 2770 RVA: 0x0004DC74 File Offset: 0x0004BE74
	private void RPC_Pong(long sender, float time)
	{
		float num = Time.time - time;
		string text = "Got ping reply from server: " + (int)(num * 1000f) + " ms";
		ZLog.Log(text);
		if (global::Console.instance)
		{
			global::Console.instance.Print(text);
		}
	}

	// Token: 0x06000AD3 RID: 2771 RVA: 0x0004DCC3 File Offset: 0x0004BEC3
	public void SetForcePlayerDifficulty(int players)
	{
		this.m_forcePlayers = players;
	}

	// Token: 0x06000AD4 RID: 2772 RVA: 0x0004DCCC File Offset: 0x0004BECC
	private int GetPlayerDifficulty(Vector3 pos)
	{
		if (this.m_forcePlayers > 0)
		{
			return this.m_forcePlayers;
		}
		int num = Player.GetPlayersInRangeXZ(pos, 200f);
		if (num < 1)
		{
			num = 1;
		}
		return num;
	}

	// Token: 0x06000AD5 RID: 2773 RVA: 0x0004DCFC File Offset: 0x0004BEFC
	public float GetDifficultyDamageScale(Vector3 pos)
	{
		int playerDifficulty = this.GetPlayerDifficulty(pos);
		return 1f + (float)(playerDifficulty - 1) * 0.04f;
	}

	// Token: 0x06000AD6 RID: 2774 RVA: 0x0004DD24 File Offset: 0x0004BF24
	public float GetDifficultyHealthScale(Vector3 pos)
	{
		int playerDifficulty = this.GetPlayerDifficulty(pos);
		return 1f + (float)(playerDifficulty - 1) * 0.4f;
	}

	// Token: 0x04000A10 RID: 2576
	private List<ZDO> m_tempPortalList = new List<ZDO>();

	// Token: 0x04000A11 RID: 2577
	private static Game m_instance;

	// Token: 0x04000A12 RID: 2578
	public GameObject m_playerPrefab;

	// Token: 0x04000A13 RID: 2579
	public GameObject m_portalPrefab;

	// Token: 0x04000A14 RID: 2580
	public GameObject m_consolePrefab;

	// Token: 0x04000A15 RID: 2581
	public string m_StartLocation = "StartTemple";

	// Token: 0x04000A16 RID: 2582
	private static string m_profileFilename;

	// Token: 0x04000A17 RID: 2583
	private PlayerProfile m_playerProfile;

	// Token: 0x04000A18 RID: 2584
	private bool m_requestRespawn;

	// Token: 0x04000A19 RID: 2585
	private float m_respawnWait;

	// Token: 0x04000A1A RID: 2586
	private const float m_respawnLoadDuration = 8f;

	// Token: 0x04000A1B RID: 2587
	private bool m_haveSpawned;

	// Token: 0x04000A1C RID: 2588
	private bool m_firstSpawn = true;

	// Token: 0x04000A1D RID: 2589
	private bool m_shuttingDown;

	// Token: 0x04000A1E RID: 2590
	private Vector3 m_randomStartPoint = Vector3.zero;

	// Token: 0x04000A1F RID: 2591
	private UnityEngine.Random.State m_spawnRandomState;

	// Token: 0x04000A20 RID: 2592
	private bool m_sleeping;

	// Token: 0x04000A21 RID: 2593
	private const float m_collectResourcesInterval = 600f;

	// Token: 0x04000A22 RID: 2594
	private float m_saveTimer;

	// Token: 0x04000A23 RID: 2595
	private const float m_saveInterval = 1200f;

	// Token: 0x04000A24 RID: 2596
	private const float m_difficultyScaleRange = 200f;

	// Token: 0x04000A25 RID: 2597
	private const float m_damageScalePerPlayer = 0.04f;

	// Token: 0x04000A26 RID: 2598
	private const float m_healthScalePerPlayer = 0.4f;

	// Token: 0x04000A27 RID: 2599
	private int m_forcePlayers;
}
