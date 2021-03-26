using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Token: 0x020000A8 RID: 168
public class RandEventSystem : MonoBehaviour
{
	// Token: 0x1700002B RID: 43
	// (get) Token: 0x06000B7C RID: 2940 RVA: 0x00052842 File Offset: 0x00050A42
	public static RandEventSystem instance
	{
		get
		{
			return RandEventSystem.m_instance;
		}
	}

	// Token: 0x06000B7D RID: 2941 RVA: 0x00052849 File Offset: 0x00050A49
	private void Awake()
	{
		RandEventSystem.m_instance = this;
	}

	// Token: 0x06000B7E RID: 2942 RVA: 0x00052851 File Offset: 0x00050A51
	private void OnDestroy()
	{
		RandEventSystem.m_instance = null;
	}

	// Token: 0x06000B7F RID: 2943 RVA: 0x00052859 File Offset: 0x00050A59
	private void Start()
	{
		ZRoutedRpc.instance.Register<string, float, Vector3>("SetEvent", new Action<long, string, float, Vector3>(this.RPC_SetEvent));
	}

	// Token: 0x06000B80 RID: 2944 RVA: 0x00052878 File Offset: 0x00050A78
	private void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		this.UpdateForcedEvents(fixedDeltaTime);
		this.UpdateRandomEvent(fixedDeltaTime);
		if (this.m_forcedEvent != null)
		{
			this.m_forcedEvent.Update(ZNet.instance.IsServer(), this.m_forcedEvent == this.m_activeEvent, true, fixedDeltaTime);
		}
		if (this.m_randomEvent != null && ZNet.instance.IsServer())
		{
			bool playerInArea = this.IsAnyPlayerInEventArea(this.m_randomEvent);
			if (this.m_randomEvent.Update(true, this.m_randomEvent == this.m_activeEvent, playerInArea, fixedDeltaTime))
			{
				this.SetRandomEvent(null, Vector3.zero);
			}
		}
		if (this.m_forcedEvent != null)
		{
			this.SetActiveEvent(this.m_forcedEvent, false);
			return;
		}
		if (this.m_randomEvent == null || !Player.m_localPlayer)
		{
			this.SetActiveEvent(null, false);
			return;
		}
		if (this.IsInsideRandomEventArea(this.m_randomEvent, Player.m_localPlayer.transform.position))
		{
			this.SetActiveEvent(this.m_randomEvent, false);
			return;
		}
		this.SetActiveEvent(null, false);
	}

	// Token: 0x06000B81 RID: 2945 RVA: 0x00052978 File Offset: 0x00050B78
	private bool IsInsideRandomEventArea(RandomEvent re, Vector3 position)
	{
		return position.y <= 3000f && Utils.DistanceXZ(position, re.m_pos) < this.m_randomEventRange;
	}

	// Token: 0x06000B82 RID: 2946 RVA: 0x000529A0 File Offset: 0x00050BA0
	private void UpdateRandomEvent(float dt)
	{
		if (ZNet.instance.IsServer())
		{
			this.m_eventTimer += dt;
			if (this.m_eventTimer > this.m_eventIntervalMin * 60f)
			{
				this.m_eventTimer = 0f;
				if (UnityEngine.Random.Range(0f, 100f) <= this.m_eventChance)
				{
					this.StartRandomEvent();
				}
			}
			this.m_sendTimer += dt;
			if (this.m_sendTimer > 2f)
			{
				this.m_sendTimer = 0f;
				this.SendCurrentRandomEvent();
			}
		}
	}

	// Token: 0x06000B83 RID: 2947 RVA: 0x00052A30 File Offset: 0x00050C30
	private void UpdateForcedEvents(float dt)
	{
		this.m_forcedEventUpdateTimer += dt;
		if (this.m_forcedEventUpdateTimer > 2f)
		{
			this.m_forcedEventUpdateTimer = 0f;
			string forcedEvent = this.GetForcedEvent();
			this.SetForcedEvent(forcedEvent);
		}
	}

	// Token: 0x06000B84 RID: 2948 RVA: 0x00052A74 File Offset: 0x00050C74
	private void SetForcedEvent(string name)
	{
		if (this.m_forcedEvent != null && name != null && this.m_forcedEvent.m_name == name)
		{
			return;
		}
		if (this.m_forcedEvent != null)
		{
			if (this.m_forcedEvent == this.m_activeEvent)
			{
				this.SetActiveEvent(null, true);
			}
			this.m_forcedEvent.OnStop();
			this.m_forcedEvent = null;
		}
		RandomEvent @event = this.GetEvent(name);
		if (@event != null)
		{
			this.m_forcedEvent = @event.Clone();
			this.m_forcedEvent.OnStart();
		}
	}

	// Token: 0x06000B85 RID: 2949 RVA: 0x00052AF4 File Offset: 0x00050CF4
	private string GetForcedEvent()
	{
		if (EnemyHud.instance != null)
		{
			Character activeBoss = EnemyHud.instance.GetActiveBoss();
			if (activeBoss != null && activeBoss.m_bossEvent.Length > 0)
			{
				return activeBoss.m_bossEvent;
			}
			string @event = EventZone.GetEvent();
			if (@event != null)
			{
				return @event;
			}
		}
		return null;
	}

	// Token: 0x06000B86 RID: 2950 RVA: 0x00052B44 File Offset: 0x00050D44
	private void SendCurrentRandomEvent()
	{
		if (this.m_randomEvent != null)
		{
			ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SetEvent", new object[]
			{
				this.m_randomEvent.m_name,
				this.m_randomEvent.m_time,
				this.m_randomEvent.m_pos
			});
			return;
		}
		ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "SetEvent", new object[]
		{
			"",
			0f,
			Vector3.zero
		});
	}

	// Token: 0x06000B87 RID: 2951 RVA: 0x00052BE4 File Offset: 0x00050DE4
	private void RPC_SetEvent(long sender, string eventName, float time, Vector3 pos)
	{
		if (ZNet.instance.IsServer())
		{
			return;
		}
		if (this.m_randomEvent == null || this.m_randomEvent.m_name != eventName)
		{
			this.SetRandomEventByName(eventName, pos);
		}
		if (this.m_randomEvent != null)
		{
			this.m_randomEvent.m_time = time;
			this.m_randomEvent.m_pos = pos;
		}
	}

	// Token: 0x06000B88 RID: 2952 RVA: 0x00052C44 File Offset: 0x00050E44
	public void StartRandomEvent()
	{
		if (!ZNet.instance.IsServer())
		{
			return;
		}
		List<KeyValuePair<RandomEvent, Vector3>> possibleRandomEvents = this.GetPossibleRandomEvents();
		ZLog.Log("Possible events:" + possibleRandomEvents.Count);
		if (possibleRandomEvents.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<RandomEvent, Vector3> keyValuePair in possibleRandomEvents)
		{
			ZLog.DevLog("Event " + keyValuePair.Key.m_name);
		}
		KeyValuePair<RandomEvent, Vector3> keyValuePair2 = possibleRandomEvents[UnityEngine.Random.Range(0, possibleRandomEvents.Count)];
		this.SetRandomEvent(keyValuePair2.Key, keyValuePair2.Value);
	}

	// Token: 0x06000B89 RID: 2953 RVA: 0x00052D04 File Offset: 0x00050F04
	private RandomEvent GetEvent(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		foreach (RandomEvent randomEvent in this.m_events)
		{
			if (randomEvent.m_name == name && randomEvent.m_enabled)
			{
				return randomEvent;
			}
		}
		return null;
	}

	// Token: 0x06000B8A RID: 2954 RVA: 0x00052D78 File Offset: 0x00050F78
	public void SetRandomEventByName(string name, Vector3 pos)
	{
		RandomEvent @event = this.GetEvent(name);
		this.SetRandomEvent(@event, pos);
	}

	// Token: 0x06000B8B RID: 2955 RVA: 0x00052D95 File Offset: 0x00050F95
	public void ResetRandomEvent()
	{
		this.SetRandomEvent(null, Vector3.zero);
	}

	// Token: 0x06000B8C RID: 2956 RVA: 0x00052DA3 File Offset: 0x00050FA3
	public bool HaveEvent(string name)
	{
		return this.GetEvent(name) != null;
	}

	// Token: 0x06000B8D RID: 2957 RVA: 0x00052DB0 File Offset: 0x00050FB0
	private void SetRandomEvent(RandomEvent ev, Vector3 pos)
	{
		if (this.m_randomEvent != null)
		{
			if (this.m_randomEvent == this.m_activeEvent)
			{
				this.SetActiveEvent(null, true);
			}
			this.m_randomEvent.OnStop();
			this.m_randomEvent = null;
		}
		if (ev != null)
		{
			this.m_randomEvent = ev.Clone();
			this.m_randomEvent.m_pos = pos;
			this.m_randomEvent.OnStart();
			ZLog.Log("Random event set:" + ev.m_name);
			if (Player.m_localPlayer)
			{
				Player.m_localPlayer.ShowTutorial("randomevent", false);
			}
		}
		if (ZNet.instance.IsServer())
		{
			this.SendCurrentRandomEvent();
		}
	}

	// Token: 0x06000B8E RID: 2958 RVA: 0x00052E58 File Offset: 0x00051058
	private bool IsAnyPlayerInEventArea(RandomEvent re)
	{
		foreach (ZDO zdo in ZNet.instance.GetAllCharacterZDOS())
		{
			if (this.IsInsideRandomEventArea(re, zdo.GetPosition()))
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000B8F RID: 2959 RVA: 0x00052EC0 File Offset: 0x000510C0
	private List<KeyValuePair<RandomEvent, Vector3>> GetPossibleRandomEvents()
	{
		List<KeyValuePair<RandomEvent, Vector3>> list = new List<KeyValuePair<RandomEvent, Vector3>>();
		List<ZDO> allCharacterZDOS = ZNet.instance.GetAllCharacterZDOS();
		foreach (RandomEvent randomEvent in this.m_events)
		{
			if (randomEvent.m_enabled && randomEvent.m_random && this.HaveGlobalKeys(randomEvent))
			{
				List<Vector3> validEventPoints = this.GetValidEventPoints(randomEvent, allCharacterZDOS);
				if (validEventPoints.Count != 0)
				{
					Vector3 value = validEventPoints[UnityEngine.Random.Range(0, validEventPoints.Count)];
					list.Add(new KeyValuePair<RandomEvent, Vector3>(randomEvent, value));
				}
			}
		}
		return list;
	}

	// Token: 0x06000B90 RID: 2960 RVA: 0x00052F70 File Offset: 0x00051170
	private List<Vector3> GetValidEventPoints(RandomEvent ev, List<ZDO> characters)
	{
		List<Vector3> list = new List<Vector3>();
		foreach (ZDO zdo in characters)
		{
			if (this.InValidBiome(ev, zdo) && this.CheckBase(ev, zdo) && zdo.GetPosition().y <= 3000f)
			{
				list.Add(zdo.GetPosition());
			}
		}
		return list;
	}

	// Token: 0x06000B91 RID: 2961 RVA: 0x00052FF0 File Offset: 0x000511F0
	private bool InValidBiome(RandomEvent ev, ZDO zdo)
	{
		if (ev.m_biome == Heightmap.Biome.None)
		{
			return true;
		}
		Vector3 position = zdo.GetPosition();
		return (WorldGenerator.instance.GetBiome(position) & ev.m_biome) != Heightmap.Biome.None;
	}

	// Token: 0x06000B92 RID: 2962 RVA: 0x00053025 File Offset: 0x00051225
	private bool CheckBase(RandomEvent ev, ZDO zdo)
	{
		return ev.m_nearBaseOnly && zdo.GetInt("baseValue", 0) >= 3;
	}

	// Token: 0x06000B93 RID: 2963 RVA: 0x00053044 File Offset: 0x00051244
	private bool HaveGlobalKeys(RandomEvent ev)
	{
		foreach (string name in ev.m_requiredGlobalKeys)
		{
			if (!ZoneSystem.instance.GetGlobalKey(name))
			{
				return false;
			}
		}
		foreach (string name2 in ev.m_notRequiredGlobalKeys)
		{
			if (ZoneSystem.instance.GetGlobalKey(name2))
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000B94 RID: 2964 RVA: 0x000530F0 File Offset: 0x000512F0
	public List<SpawnSystem.SpawnData> GetCurrentSpawners()
	{
		if (this.m_activeEvent != null)
		{
			return this.m_activeEvent.m_spawn;
		}
		return null;
	}

	// Token: 0x06000B95 RID: 2965 RVA: 0x00053107 File Offset: 0x00051307
	public string GetEnvOverride()
	{
		if (this.m_activeEvent != null && !string.IsNullOrEmpty(this.m_activeEvent.m_forceEnvironment) && this.m_activeEvent.InEventBiome())
		{
			return this.m_activeEvent.m_forceEnvironment;
		}
		return null;
	}

	// Token: 0x06000B96 RID: 2966 RVA: 0x0005313D File Offset: 0x0005133D
	public string GetMusicOverride()
	{
		if (this.m_activeEvent != null && !string.IsNullOrEmpty(this.m_activeEvent.m_forceMusic))
		{
			return this.m_activeEvent.m_forceMusic;
		}
		return null;
	}

	// Token: 0x06000B97 RID: 2967 RVA: 0x00053168 File Offset: 0x00051368
	private void SetActiveEvent(RandomEvent ev, bool end = false)
	{
		if (ev != null && this.m_activeEvent != null && ev.m_name == this.m_activeEvent.m_name)
		{
			return;
		}
		if (this.m_activeEvent != null)
		{
			this.m_activeEvent.OnDeactivate(end);
			this.m_activeEvent = null;
		}
		if (ev != null)
		{
			this.m_activeEvent = ev;
			if (this.m_activeEvent != null)
			{
				this.m_activeEvent.OnActivate();
			}
		}
	}

	// Token: 0x06000B98 RID: 2968 RVA: 0x000531D1 File Offset: 0x000513D1
	public static bool InEvent()
	{
		return !(RandEventSystem.m_instance == null) && RandEventSystem.m_instance.m_activeEvent != null;
	}

	// Token: 0x06000B99 RID: 2969 RVA: 0x000531EF File Offset: 0x000513EF
	public static bool HaveActiveEvent()
	{
		return !(RandEventSystem.m_instance == null) && (RandEventSystem.m_instance.m_activeEvent != null || RandEventSystem.m_instance.m_randomEvent != null || RandEventSystem.m_instance.m_activeEvent != null);
	}

	// Token: 0x06000B9A RID: 2970 RVA: 0x00053229 File Offset: 0x00051429
	public RandomEvent GetCurrentRandomEvent()
	{
		return this.m_randomEvent;
	}

	// Token: 0x06000B9B RID: 2971 RVA: 0x00053231 File Offset: 0x00051431
	public RandomEvent GetActiveEvent()
	{
		return this.m_activeEvent;
	}

	// Token: 0x06000B9C RID: 2972 RVA: 0x0005323C File Offset: 0x0005143C
	public void PrepareSave()
	{
		this.m_tempSaveEventTimer = this.m_eventTimer;
		if (this.m_randomEvent != null)
		{
			this.m_tempSaveRandomEvent = this.m_randomEvent.m_name;
			this.m_tempSaveRandomEventTime = this.m_randomEvent.m_time;
			this.m_tempSaveRandomEventPos = this.m_randomEvent.m_pos;
			return;
		}
		this.m_tempSaveRandomEvent = "";
		this.m_tempSaveRandomEventTime = 0f;
		this.m_tempSaveRandomEventPos = Vector3.zero;
	}

	// Token: 0x06000B9D RID: 2973 RVA: 0x000532B4 File Offset: 0x000514B4
	public void SaveAsync(BinaryWriter writer)
	{
		writer.Write(this.m_tempSaveEventTimer);
		writer.Write(this.m_tempSaveRandomEvent);
		writer.Write(this.m_tempSaveRandomEventTime);
		writer.Write(this.m_tempSaveRandomEventPos.x);
		writer.Write(this.m_tempSaveRandomEventPos.y);
		writer.Write(this.m_tempSaveRandomEventPos.z);
	}

	// Token: 0x06000B9E RID: 2974 RVA: 0x00053318 File Offset: 0x00051518
	public void Load(BinaryReader reader, int version)
	{
		this.m_eventTimer = reader.ReadSingle();
		if (version >= 25)
		{
			string text = reader.ReadString();
			float time = reader.ReadSingle();
			Vector3 pos;
			pos.x = reader.ReadSingle();
			pos.y = reader.ReadSingle();
			pos.z = reader.ReadSingle();
			if (!string.IsNullOrEmpty(text))
			{
				this.SetRandomEventByName(text, pos);
				if (this.m_randomEvent != null)
				{
					this.m_randomEvent.m_time = time;
					this.m_randomEvent.m_pos = pos;
				}
			}
		}
	}

	// Token: 0x04000AB9 RID: 2745
	private static RandEventSystem m_instance;

	// Token: 0x04000ABA RID: 2746
	public float m_eventIntervalMin = 1f;

	// Token: 0x04000ABB RID: 2747
	public float m_eventChance = 25f;

	// Token: 0x04000ABC RID: 2748
	public float m_randomEventRange = 200f;

	// Token: 0x04000ABD RID: 2749
	private float m_eventTimer;

	// Token: 0x04000ABE RID: 2750
	private float m_sendTimer;

	// Token: 0x04000ABF RID: 2751
	public List<RandomEvent> m_events = new List<RandomEvent>();

	// Token: 0x04000AC0 RID: 2752
	private RandomEvent m_randomEvent;

	// Token: 0x04000AC1 RID: 2753
	private float m_forcedEventUpdateTimer;

	// Token: 0x04000AC2 RID: 2754
	private RandomEvent m_forcedEvent;

	// Token: 0x04000AC3 RID: 2755
	private RandomEvent m_activeEvent;

	// Token: 0x04000AC4 RID: 2756
	private float m_tempSaveEventTimer;

	// Token: 0x04000AC5 RID: 2757
	private string m_tempSaveRandomEvent;

	// Token: 0x04000AC6 RID: 2758
	private float m_tempSaveRandomEventTime;

	// Token: 0x04000AC7 RID: 2759
	private Vector3 m_tempSaveRandomEventPos;
}
