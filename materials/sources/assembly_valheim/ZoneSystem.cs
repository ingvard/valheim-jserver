using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x020000B6 RID: 182
public class ZoneSystem : MonoBehaviour
{
	// Token: 0x1700002F RID: 47
	// (get) Token: 0x06000C0B RID: 3083 RVA: 0x00055AEC File Offset: 0x00053CEC
	public static ZoneSystem instance
	{
		get
		{
			return ZoneSystem.m_instance;
		}
	}

	// Token: 0x06000C0C RID: 3084 RVA: 0x00055AF4 File Offset: 0x00053CF4
	private void Awake()
	{
		ZoneSystem.m_instance = this;
		this.m_terrainRayMask = LayerMask.GetMask(new string[]
		{
			"terrain"
		});
		this.m_blockRayMask = LayerMask.GetMask(new string[]
		{
			"Default",
			"static_solid",
			"Default_small",
			"piece"
		});
		this.m_solidRayMask = LayerMask.GetMask(new string[]
		{
			"Default",
			"static_solid",
			"Default_small",
			"piece",
			"terrain"
		});
		SceneManager.LoadScene("locations", LoadSceneMode.Additive);
		ZLog.Log("Zonesystem Awake " + Time.frameCount);
	}

	// Token: 0x06000C0D RID: 3085 RVA: 0x00055BB0 File Offset: 0x00053DB0
	private void Start()
	{
		ZLog.Log("Zonesystem Start " + Time.frameCount);
		this.SetupLocations();
		this.ValidateVegetation();
		if (!this.m_locationsGenerated && ZNet.instance.IsServer())
		{
			this.GenerateLocations();
		}
		ZRoutedRpc instance = ZRoutedRpc.instance;
		instance.m_onNewPeer = (Action<long>)Delegate.Combine(instance.m_onNewPeer, new Action<long>(this.OnNewPeer));
		if (ZNet.instance.IsServer())
		{
			ZRoutedRpc.instance.Register<string>("SetGlobalKey", new Action<long, string>(this.RPC_SetGlobalKey));
			return;
		}
		ZRoutedRpc.instance.Register<List<string>>("GlobalKeys", new Action<long, List<string>>(this.RPC_GlobalKeys));
		ZRoutedRpc.instance.Register<ZPackage>("LocationIcons", new Action<long, ZPackage>(this.RPC_LocationIcons));
	}

	// Token: 0x06000C0E RID: 3086 RVA: 0x00055C80 File Offset: 0x00053E80
	private void SendGlobalKeys(long peer)
	{
		List<string> list = new List<string>(this.m_globalKeys);
		ZRoutedRpc.instance.InvokeRoutedRPC(peer, "GlobalKeys", new object[]
		{
			list
		});
	}

	// Token: 0x06000C0F RID: 3087 RVA: 0x00055CB4 File Offset: 0x00053EB4
	private void RPC_GlobalKeys(long sender, List<string> keys)
	{
		ZLog.Log("client got keys " + keys.Count);
		this.m_globalKeys.Clear();
		foreach (string item in keys)
		{
			this.m_globalKeys.Add(item);
		}
	}

	// Token: 0x06000C10 RID: 3088 RVA: 0x00055D30 File Offset: 0x00053F30
	private void SendLocationIcons(long peer)
	{
		ZPackage zpackage = new ZPackage();
		this.tempIconList.Clear();
		this.GetLocationIcons(this.tempIconList);
		zpackage.Write(this.tempIconList.Count);
		foreach (KeyValuePair<Vector3, string> keyValuePair in this.tempIconList)
		{
			zpackage.Write(keyValuePair.Key);
			zpackage.Write(keyValuePair.Value);
		}
		ZRoutedRpc.instance.InvokeRoutedRPC(peer, "LocationIcons", new object[]
		{
			zpackage
		});
	}

	// Token: 0x06000C11 RID: 3089 RVA: 0x00055DE0 File Offset: 0x00053FE0
	private void RPC_LocationIcons(long sender, ZPackage pkg)
	{
		ZLog.Log("client got location icons");
		this.m_locationIcons.Clear();
		int num = pkg.ReadInt();
		for (int i = 0; i < num; i++)
		{
			Vector3 key = pkg.ReadVector3();
			string value = pkg.ReadString();
			this.m_locationIcons[key] = value;
		}
		ZLog.Log("Icons:" + num);
	}

	// Token: 0x06000C12 RID: 3090 RVA: 0x00055E45 File Offset: 0x00054045
	private void OnNewPeer(long peerID)
	{
		if (ZNet.instance.IsServer())
		{
			ZLog.Log("Server: New peer connected,sending global keys");
			this.SendGlobalKeys(peerID);
			this.SendLocationIcons(peerID);
		}
	}

	// Token: 0x06000C13 RID: 3091 RVA: 0x00055E6C File Offset: 0x0005406C
	private void SetupLocations()
	{
		GameObject[] array = Resources.FindObjectsOfTypeAll<GameObject>();
		GameObject gameObject = null;
		foreach (GameObject gameObject2 in array)
		{
			if (gameObject2.name == "_Locations")
			{
				gameObject = gameObject2;
				break;
			}
		}
		Location[] componentsInChildren = gameObject.GetComponentsInChildren<Location>(true);
		Location[] array3 = componentsInChildren;
		for (int i = 0; i < array3.Length; i++)
		{
			if (array3[i].transform.gameObject.activeInHierarchy)
			{
				this.m_error = true;
			}
		}
		foreach (ZoneSystem.ZoneLocation zoneLocation in this.m_locations)
		{
			Transform transform = null;
			foreach (Location location in componentsInChildren)
			{
				if (location.gameObject.name == zoneLocation.m_prefabName)
				{
					transform = location.transform;
					break;
				}
			}
			if (!(transform == null) || zoneLocation.m_enable)
			{
				zoneLocation.m_prefab = transform.gameObject;
				zoneLocation.m_hash = zoneLocation.m_prefab.name.GetStableHashCode();
				Location componentInChildren = zoneLocation.m_prefab.GetComponentInChildren<Location>();
				zoneLocation.m_location = componentInChildren;
				zoneLocation.m_interiorRadius = (componentInChildren.m_hasInterior ? componentInChildren.m_interiorRadius : 0f);
				zoneLocation.m_exteriorRadius = componentInChildren.m_exteriorRadius;
				if (Application.isPlaying)
				{
					ZoneSystem.PrepareNetViews(zoneLocation.m_prefab, zoneLocation.m_netViews);
					ZoneSystem.PrepareRandomSpawns(zoneLocation.m_prefab, zoneLocation.m_randomSpawns);
					if (!this.m_locationsByHash.ContainsKey(zoneLocation.m_hash))
					{
						this.m_locationsByHash.Add(zoneLocation.m_hash, zoneLocation);
					}
				}
			}
		}
	}

	// Token: 0x06000C14 RID: 3092 RVA: 0x0005604C File Offset: 0x0005424C
	public static void PrepareNetViews(GameObject root, List<ZNetView> views)
	{
		views.Clear();
		foreach (ZNetView znetView in root.GetComponentsInChildren<ZNetView>(true))
		{
			if (Utils.IsEnabledInheirarcy(znetView.gameObject, root))
			{
				views.Add(znetView);
			}
		}
	}

	// Token: 0x06000C15 RID: 3093 RVA: 0x00056090 File Offset: 0x00054290
	public static void PrepareRandomSpawns(GameObject root, List<RandomSpawn> randomSpawns)
	{
		randomSpawns.Clear();
		foreach (RandomSpawn randomSpawn in root.GetComponentsInChildren<RandomSpawn>(true))
		{
			if (Utils.IsEnabledInheirarcy(randomSpawn.gameObject, root))
			{
				randomSpawns.Add(randomSpawn);
				randomSpawn.Prepare();
			}
		}
	}

	// Token: 0x06000C16 RID: 3094 RVA: 0x000560D8 File Offset: 0x000542D8
	private void OnDestroy()
	{
		ZoneSystem.m_instance = null;
	}

	// Token: 0x06000C17 RID: 3095 RVA: 0x000560E0 File Offset: 0x000542E0
	private void ValidateVegetation()
	{
		foreach (ZoneSystem.ZoneVegetation zoneVegetation in this.m_vegetation)
		{
			if (zoneVegetation.m_enable && zoneVegetation.m_prefab && zoneVegetation.m_prefab.GetComponent<ZNetView>() == null)
			{
				ZLog.LogError(string.Concat(new string[]
				{
					"Vegetation ",
					zoneVegetation.m_prefab.name,
					" [ ",
					zoneVegetation.m_name,
					"] is missing ZNetView"
				}));
			}
		}
	}

	// Token: 0x06000C18 RID: 3096 RVA: 0x00056194 File Offset: 0x00054394
	public void PrepareSave()
	{
		this.m_tempGeneratedZonesSaveClone = new HashSet<Vector2i>(this.m_generatedZones);
		this.m_tempGlobalKeysSaveClone = new HashSet<string>(this.m_globalKeys);
		this.m_tempLocationsSaveClone = new List<ZoneSystem.LocationInstance>(this.m_locationInstances.Values);
		this.m_tempLocationsGeneratedSaveClone = this.m_locationsGenerated;
	}

	// Token: 0x06000C19 RID: 3097 RVA: 0x000561E8 File Offset: 0x000543E8
	public void SaveASync(BinaryWriter writer)
	{
		writer.Write(this.m_tempGeneratedZonesSaveClone.Count);
		foreach (Vector2i vector2i in this.m_tempGeneratedZonesSaveClone)
		{
			writer.Write(vector2i.x);
			writer.Write(vector2i.y);
		}
		writer.Write(this.m_pgwVersion);
		writer.Write(this.m_locationVersion);
		writer.Write(this.m_tempGlobalKeysSaveClone.Count);
		foreach (string value in this.m_tempGlobalKeysSaveClone)
		{
			writer.Write(value);
		}
		writer.Write(this.m_tempLocationsGeneratedSaveClone);
		writer.Write(this.m_tempLocationsSaveClone.Count);
		foreach (ZoneSystem.LocationInstance locationInstance in this.m_tempLocationsSaveClone)
		{
			writer.Write(locationInstance.m_location.m_prefabName);
			writer.Write(locationInstance.m_position.x);
			writer.Write(locationInstance.m_position.y);
			writer.Write(locationInstance.m_position.z);
			writer.Write(locationInstance.m_placed);
		}
		this.m_tempGeneratedZonesSaveClone.Clear();
		this.m_tempGeneratedZonesSaveClone = null;
		this.m_tempGlobalKeysSaveClone.Clear();
		this.m_tempGlobalKeysSaveClone = null;
		this.m_tempLocationsSaveClone.Clear();
		this.m_tempLocationsSaveClone = null;
	}

	// Token: 0x06000C1A RID: 3098 RVA: 0x000563B0 File Offset: 0x000545B0
	public void Load(BinaryReader reader, int version)
	{
		this.m_generatedZones.Clear();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			Vector2i item = default(Vector2i);
			item.x = reader.ReadInt32();
			item.y = reader.ReadInt32();
			this.m_generatedZones.Add(item);
		}
		if (version >= 13)
		{
			int num2 = reader.ReadInt32();
			int num3 = (version >= 21) ? reader.ReadInt32() : 0;
			if (num2 != this.m_pgwVersion)
			{
				this.m_generatedZones.Clear();
			}
			if (version >= 14)
			{
				this.m_globalKeys.Clear();
				int num4 = reader.ReadInt32();
				for (int j = 0; j < num4; j++)
				{
					string item2 = reader.ReadString();
					this.m_globalKeys.Add(item2);
				}
			}
			if (version >= 18)
			{
				if (version >= 20)
				{
					this.m_locationsGenerated = reader.ReadBoolean();
				}
				this.m_locationInstances.Clear();
				int num5 = reader.ReadInt32();
				for (int k = 0; k < num5; k++)
				{
					string text = reader.ReadString();
					Vector3 zero = Vector3.zero;
					zero.x = reader.ReadSingle();
					zero.y = reader.ReadSingle();
					zero.z = reader.ReadSingle();
					bool generated = false;
					if (version >= 19)
					{
						generated = reader.ReadBoolean();
					}
					ZoneSystem.ZoneLocation location = this.GetLocation(text);
					if (location != null)
					{
						this.RegisterLocation(location, zero, generated);
					}
					else
					{
						ZLog.DevLog("Failed to find location " + text);
					}
				}
				ZLog.Log("Loaded " + num5 + " locations");
				if (num2 != this.m_pgwVersion)
				{
					this.m_locationInstances.Clear();
					this.m_locationsGenerated = false;
				}
				if (num3 != this.m_locationVersion)
				{
					this.m_locationsGenerated = false;
				}
			}
		}
	}

	// Token: 0x06000C1B RID: 3099 RVA: 0x00056574 File Offset: 0x00054774
	private void Update()
	{
		if (ZNet.GetConnectionStatus() != ZNet.ConnectionStatus.Connected)
		{
			return;
		}
		this.m_updateTimer += Time.deltaTime;
		if (this.m_updateTimer > 0.1f)
		{
			this.m_updateTimer = 0f;
			bool flag = this.CreateLocalZones(ZNet.instance.GetReferencePosition());
			this.UpdateTTL(0.1f);
			if (ZNet.instance.IsServer() && !flag)
			{
				this.CreateGhostZones(ZNet.instance.GetReferencePosition());
				foreach (ZNetPeer znetPeer in ZNet.instance.GetPeers())
				{
					this.CreateGhostZones(znetPeer.GetRefPos());
				}
			}
		}
	}

	// Token: 0x06000C1C RID: 3100 RVA: 0x00056644 File Offset: 0x00054844
	private bool CreateGhostZones(Vector3 refPoint)
	{
		Vector2i zone = this.GetZone(refPoint);
		GameObject gameObject;
		if (!this.IsZoneGenerated(zone) && this.SpawnZone(zone, ZoneSystem.SpawnMode.Ghost, out gameObject))
		{
			return true;
		}
		int num = this.m_activeArea + this.m_activeDistantArea;
		for (int i = zone.y - num; i <= zone.y + num; i++)
		{
			for (int j = zone.x - num; j <= zone.x + num; j++)
			{
				Vector2i zoneID = new Vector2i(j, i);
				GameObject gameObject2;
				if (!this.IsZoneGenerated(zoneID) && this.SpawnZone(zoneID, ZoneSystem.SpawnMode.Ghost, out gameObject2))
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000C1D RID: 3101 RVA: 0x000566DC File Offset: 0x000548DC
	private bool CreateLocalZones(Vector3 refPoint)
	{
		Vector2i zone = this.GetZone(refPoint);
		if (this.PokeLocalZone(zone))
		{
			return true;
		}
		for (int i = zone.y - this.m_activeArea; i <= zone.y + this.m_activeArea; i++)
		{
			for (int j = zone.x - this.m_activeArea; j <= zone.x + this.m_activeArea; j++)
			{
				Vector2i vector2i = new Vector2i(j, i);
				if (!(vector2i == zone) && this.PokeLocalZone(vector2i))
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000C1E RID: 3102 RVA: 0x00056764 File Offset: 0x00054964
	private bool PokeLocalZone(Vector2i zoneID)
	{
		ZoneSystem.ZoneData zoneData;
		if (this.m_zones.TryGetValue(zoneID, out zoneData))
		{
			zoneData.m_ttl = 0f;
			return false;
		}
		ZoneSystem.SpawnMode mode = (ZNet.instance.IsServer() && !this.IsZoneGenerated(zoneID)) ? ZoneSystem.SpawnMode.Full : ZoneSystem.SpawnMode.Client;
		GameObject root;
		if (this.SpawnZone(zoneID, mode, out root))
		{
			ZoneSystem.ZoneData zoneData2 = new ZoneSystem.ZoneData();
			zoneData2.m_root = root;
			this.m_zones.Add(zoneID, zoneData2);
			return true;
		}
		return false;
	}

	// Token: 0x06000C1F RID: 3103 RVA: 0x000567D4 File Offset: 0x000549D4
	public bool IsZoneLoaded(Vector3 point)
	{
		Vector2i zone = this.GetZone(point);
		return this.IsZoneLoaded(zone);
	}

	// Token: 0x06000C20 RID: 3104 RVA: 0x000567F0 File Offset: 0x000549F0
	public bool IsZoneLoaded(Vector2i zoneID)
	{
		return this.m_zones.ContainsKey(zoneID);
	}

	// Token: 0x06000C21 RID: 3105 RVA: 0x00056800 File Offset: 0x00054A00
	private bool SpawnZone(Vector2i zoneID, ZoneSystem.SpawnMode mode, out GameObject root)
	{
		Vector3 zonePos = this.GetZonePos(zoneID);
		Heightmap componentInChildren = this.m_zonePrefab.GetComponentInChildren<Heightmap>();
		if (!HeightmapBuilder.instance.IsTerrainReady(zonePos, componentInChildren.m_width, componentInChildren.m_scale, componentInChildren.m_isDistantLod, WorldGenerator.instance))
		{
			root = null;
			return false;
		}
		root = UnityEngine.Object.Instantiate<GameObject>(this.m_zonePrefab, zonePos, Quaternion.identity);
		if ((mode == ZoneSystem.SpawnMode.Ghost || mode == ZoneSystem.SpawnMode.Full) && !this.IsZoneGenerated(zoneID))
		{
			Heightmap componentInChildren2 = root.GetComponentInChildren<Heightmap>();
			this.m_tempClearAreas.Clear();
			this.m_tempSpawnedObjects.Clear();
			this.PlaceLocations(zoneID, zonePos, root.transform, componentInChildren2, this.m_tempClearAreas, mode, this.m_tempSpawnedObjects);
			this.PlaceVegetation(zoneID, zonePos, root.transform, componentInChildren2, this.m_tempClearAreas, mode, this.m_tempSpawnedObjects);
			this.PlaceZoneCtrl(zoneID, zonePos, mode, this.m_tempSpawnedObjects);
			if (mode == ZoneSystem.SpawnMode.Ghost)
			{
				foreach (GameObject obj in this.m_tempSpawnedObjects)
				{
					UnityEngine.Object.Destroy(obj);
				}
				this.m_tempSpawnedObjects.Clear();
				UnityEngine.Object.Destroy(root);
				root = null;
			}
			this.SetZoneGenerated(zoneID);
		}
		return true;
	}

	// Token: 0x06000C22 RID: 3106 RVA: 0x00056940 File Offset: 0x00054B40
	private void PlaceZoneCtrl(Vector2i zoneID, Vector3 zoneCenterPos, ZoneSystem.SpawnMode mode, List<GameObject> spawnedObjects)
	{
		if (mode == ZoneSystem.SpawnMode.Full || mode == ZoneSystem.SpawnMode.Ghost)
		{
			if (mode == ZoneSystem.SpawnMode.Ghost)
			{
				ZNetView.StartGhostInit();
			}
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_zoneCtrlPrefab, zoneCenterPos, Quaternion.identity);
			gameObject.GetComponent<ZNetView>().GetZDO().SetPGWVersion(this.m_pgwVersion);
			if (mode == ZoneSystem.SpawnMode.Ghost)
			{
				spawnedObjects.Add(gameObject);
				ZNetView.FinishGhostInit();
			}
		}
	}

	// Token: 0x06000C23 RID: 3107 RVA: 0x00056998 File Offset: 0x00054B98
	private Vector3 GetRandomPointInRadius(Vector3 center, float radius)
	{
		float f = UnityEngine.Random.value * 3.1415927f * 2f;
		float num = UnityEngine.Random.Range(0f, radius);
		return center + new Vector3(Mathf.Sin(f) * num, 0f, Mathf.Cos(f) * num);
	}

	// Token: 0x06000C24 RID: 3108 RVA: 0x000569E4 File Offset: 0x00054BE4
	private void PlaceVegetation(Vector2i zoneID, Vector3 zoneCenterPos, Transform parent, Heightmap hmap, List<ZoneSystem.ClearArea> clearAreas, ZoneSystem.SpawnMode mode, List<GameObject> spawnedObjects)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		int seed = WorldGenerator.instance.GetSeed();
		float num = this.m_zoneSize / 2f;
		int num2 = 1;
		foreach (ZoneSystem.ZoneVegetation zoneVegetation in this.m_vegetation)
		{
			num2++;
			if (zoneVegetation.m_enable && hmap.HaveBiome(zoneVegetation.m_biome))
			{
				UnityEngine.Random.InitState(seed + zoneID.x * 4271 + zoneID.y * 9187 + zoneVegetation.m_prefab.name.GetStableHashCode());
				int num3 = 1;
				if (zoneVegetation.m_max < 1f)
				{
					if (UnityEngine.Random.value > zoneVegetation.m_max)
					{
						continue;
					}
				}
				else
				{
					num3 = UnityEngine.Random.Range((int)zoneVegetation.m_min, (int)zoneVegetation.m_max + 1);
				}
				bool flag = zoneVegetation.m_prefab.GetComponent<ZNetView>() != null;
				float num4 = Mathf.Cos(0.017453292f * zoneVegetation.m_maxTilt);
				float num5 = Mathf.Cos(0.017453292f * zoneVegetation.m_minTilt);
				float num6 = num - zoneVegetation.m_groupRadius;
				int num7 = zoneVegetation.m_forcePlacement ? (num3 * 50) : num3;
				int num8 = 0;
				for (int i = 0; i < num7; i++)
				{
					Vector3 vector = new Vector3(UnityEngine.Random.Range(zoneCenterPos.x - num6, zoneCenterPos.x + num6), 0f, UnityEngine.Random.Range(zoneCenterPos.z - num6, zoneCenterPos.z + num6));
					int num9 = UnityEngine.Random.Range(zoneVegetation.m_groupSizeMin, zoneVegetation.m_groupSizeMax + 1);
					bool flag2 = false;
					for (int j = 0; j < num9; j++)
					{
						Vector3 vector2 = (j == 0) ? vector : this.GetRandomPointInRadius(vector, zoneVegetation.m_groupRadius);
						float num10 = (float)UnityEngine.Random.Range(0, 360);
						float num11 = UnityEngine.Random.Range(zoneVegetation.m_scaleMin, zoneVegetation.m_scaleMax);
						float x = UnityEngine.Random.Range(-zoneVegetation.m_randTilt, zoneVegetation.m_randTilt);
						float z = UnityEngine.Random.Range(-zoneVegetation.m_randTilt, zoneVegetation.m_randTilt);
						if (!zoneVegetation.m_blockCheck || !this.IsBlocked(vector2))
						{
							Vector3 vector3;
							Heightmap.Biome biome;
							Heightmap.BiomeArea biomeArea;
							Heightmap heightmap;
							this.GetGroundData(ref vector2, out vector3, out biome, out biomeArea, out heightmap);
							if ((zoneVegetation.m_biome & biome) != Heightmap.Biome.None && (zoneVegetation.m_biomeArea & biomeArea) != (Heightmap.BiomeArea)0)
							{
								float num12 = vector2.y - this.m_waterLevel;
								if (num12 >= zoneVegetation.m_minAltitude && num12 <= zoneVegetation.m_maxAltitude)
								{
									if (zoneVegetation.m_minOceanDepth != zoneVegetation.m_maxOceanDepth)
									{
										float oceanDepth = heightmap.GetOceanDepth(vector2);
										if (oceanDepth < zoneVegetation.m_minOceanDepth || oceanDepth > zoneVegetation.m_maxOceanDepth)
										{
											goto IL_437;
										}
									}
									if (vector3.y >= num4 && vector3.y <= num5)
									{
										if (zoneVegetation.m_terrainDeltaRadius > 0f)
										{
											float num13;
											Vector3 vector4;
											this.GetTerrainDelta(vector2, zoneVegetation.m_terrainDeltaRadius, out num13, out vector4);
											if (num13 > zoneVegetation.m_maxTerrainDelta || num13 < zoneVegetation.m_minTerrainDelta)
											{
												goto IL_437;
											}
										}
										if (zoneVegetation.m_inForest)
										{
											float forestFactor = WorldGenerator.GetForestFactor(vector2);
											if (forestFactor < zoneVegetation.m_forestTresholdMin || forestFactor > zoneVegetation.m_forestTresholdMax)
											{
												goto IL_437;
											}
										}
										if (!this.InsideClearArea(clearAreas, vector2))
										{
											if (zoneVegetation.m_snapToWater)
											{
												vector2.y = this.m_waterLevel;
											}
											vector2.y += zoneVegetation.m_groundOffset;
											Quaternion rotation = Quaternion.identity;
											if (zoneVegetation.m_chanceToUseGroundTilt > 0f && UnityEngine.Random.value <= zoneVegetation.m_chanceToUseGroundTilt)
											{
												rotation = Quaternion.AngleAxis(num10, vector3);
											}
											else
											{
												rotation = Quaternion.Euler(x, num10, z);
											}
											if (flag)
											{
												if (mode == ZoneSystem.SpawnMode.Full || mode == ZoneSystem.SpawnMode.Ghost)
												{
													if (mode == ZoneSystem.SpawnMode.Ghost)
													{
														ZNetView.StartGhostInit();
													}
													GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(zoneVegetation.m_prefab, vector2, rotation);
													ZNetView component = gameObject.GetComponent<ZNetView>();
													component.SetLocalScale(new Vector3(num11, num11, num11));
													component.GetZDO().SetPGWVersion(this.m_pgwVersion);
													if (mode == ZoneSystem.SpawnMode.Ghost)
													{
														spawnedObjects.Add(gameObject);
														ZNetView.FinishGhostInit();
													}
												}
											}
											else
											{
												GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(zoneVegetation.m_prefab, vector2, rotation);
												gameObject2.transform.localScale = new Vector3(num11, num11, num11);
												gameObject2.transform.SetParent(parent, true);
											}
											flag2 = true;
										}
									}
								}
							}
						}
						IL_437:;
					}
					if (flag2)
					{
						num8++;
					}
					if (num8 >= num3)
					{
						break;
					}
				}
			}
		}
		UnityEngine.Random.state = state;
	}

	// Token: 0x06000C25 RID: 3109 RVA: 0x00056E94 File Offset: 0x00055094
	private bool InsideClearArea(List<ZoneSystem.ClearArea> areas, Vector3 point)
	{
		foreach (ZoneSystem.ClearArea clearArea in areas)
		{
			if (point.x > clearArea.m_center.x - clearArea.m_radius && point.x < clearArea.m_center.x + clearArea.m_radius && point.z > clearArea.m_center.z - clearArea.m_radius && point.z < clearArea.m_center.z + clearArea.m_radius)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000C26 RID: 3110 RVA: 0x00056F4C File Offset: 0x0005514C
	private ZoneSystem.ZoneLocation GetLocation(int hash)
	{
		ZoneSystem.ZoneLocation result;
		if (this.m_locationsByHash.TryGetValue(hash, out result))
		{
			return result;
		}
		return null;
	}

	// Token: 0x06000C27 RID: 3111 RVA: 0x00056F6C File Offset: 0x0005516C
	private ZoneSystem.ZoneLocation GetLocation(string name)
	{
		foreach (ZoneSystem.ZoneLocation zoneLocation in this.m_locations)
		{
			if (zoneLocation.m_prefabName == name)
			{
				return zoneLocation;
			}
		}
		return null;
	}

	// Token: 0x06000C28 RID: 3112 RVA: 0x00056FD0 File Offset: 0x000551D0
	private void ClearNonPlacedLocations()
	{
		Dictionary<Vector2i, ZoneSystem.LocationInstance> dictionary = new Dictionary<Vector2i, ZoneSystem.LocationInstance>();
		foreach (KeyValuePair<Vector2i, ZoneSystem.LocationInstance> keyValuePair in this.m_locationInstances)
		{
			if (keyValuePair.Value.m_placed)
			{
				dictionary.Add(keyValuePair.Key, keyValuePair.Value);
			}
		}
		this.m_locationInstances = dictionary;
	}

	// Token: 0x06000C29 RID: 3113 RVA: 0x0005704C File Offset: 0x0005524C
	private void CheckLocationDuplicates()
	{
		ZLog.Log("Checking for location duplicates");
		for (int i = 0; i < this.m_locations.Count; i++)
		{
			ZoneSystem.ZoneLocation zoneLocation = this.m_locations[i];
			if (zoneLocation.m_enable)
			{
				for (int j = i + 1; j < this.m_locations.Count; j++)
				{
					ZoneSystem.ZoneLocation zoneLocation2 = this.m_locations[j];
					if (zoneLocation2.m_enable && zoneLocation.m_prefabName == zoneLocation2.m_prefabName)
					{
						ZLog.LogWarning("Two locations points to the same location prefab " + zoneLocation.m_prefabName);
					}
				}
			}
		}
	}

	// Token: 0x06000C2A RID: 3114 RVA: 0x000570E4 File Offset: 0x000552E4
	public void GenerateLocations()
	{
		if (!Application.isPlaying)
		{
			ZLog.Log("Setting up locations");
			this.SetupLocations();
		}
		ZLog.Log("Generating locations");
		DateTime now = DateTime.Now;
		this.m_locationsGenerated = true;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		this.CheckLocationDuplicates();
		this.ClearNonPlacedLocations();
		foreach (ZoneSystem.ZoneLocation zoneLocation in from a in this.m_locations
		orderby a.m_prioritized descending
		select a)
		{
			if (zoneLocation.m_enable && zoneLocation.m_quantity != 0)
			{
				this.GenerateLocations(zoneLocation);
			}
		}
		UnityEngine.Random.state = state;
		ZLog.Log(" Done generating locations, duration:" + (DateTime.Now - now).TotalMilliseconds + " ms");
	}

	// Token: 0x06000C2B RID: 3115 RVA: 0x000571D8 File Offset: 0x000553D8
	private int CountNrOfLocation(ZoneSystem.ZoneLocation location)
	{
		int num = 0;
		using (Dictionary<Vector2i, ZoneSystem.LocationInstance>.ValueCollection.Enumerator enumerator = this.m_locationInstances.Values.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_location.m_prefabName == location.m_prefabName)
				{
					num++;
				}
			}
		}
		if (num > 0)
		{
			ZLog.Log(string.Concat(new object[]
			{
				"Old location found ",
				location.m_prefabName,
				" x ",
				num
			}));
		}
		return num;
	}

	// Token: 0x06000C2C RID: 3116 RVA: 0x0005727C File Offset: 0x0005547C
	private void GenerateLocations(ZoneSystem.ZoneLocation location)
	{
		DateTime now = DateTime.Now;
		UnityEngine.Random.InitState(WorldGenerator.instance.GetSeed() + location.m_prefabName.GetStableHashCode());
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		float locationRadius = Mathf.Max(location.m_exteriorRadius, location.m_interiorRadius);
		int num9 = location.m_prioritized ? 200000 : 100000;
		int num10 = 0;
		int num11 = this.CountNrOfLocation(location);
		float num12 = 10000f;
		if (location.m_centerFirst)
		{
			num12 = location.m_minDistance;
		}
		if (location.m_unique && num11 > 0)
		{
			return;
		}
		int num13 = 0;
		while (num13 < num9 && num11 < location.m_quantity)
		{
			Vector2i randomZone = this.GetRandomZone(num12);
			if (location.m_centerFirst)
			{
				num12 += 1f;
			}
			if (this.m_locationInstances.ContainsKey(randomZone))
			{
				num++;
			}
			else if (!this.IsZoneGenerated(randomZone))
			{
				Vector3 zonePos = this.GetZonePos(randomZone);
				Heightmap.BiomeArea biomeArea = WorldGenerator.instance.GetBiomeArea(zonePos);
				if ((location.m_biomeArea & biomeArea) == (Heightmap.BiomeArea)0)
				{
					num4++;
				}
				else
				{
					for (int i = 0; i < 20; i++)
					{
						num10++;
						Vector3 randomPointInZone = this.GetRandomPointInZone(randomZone, locationRadius);
						float magnitude = randomPointInZone.magnitude;
						if (location.m_minDistance != 0f && magnitude < location.m_minDistance)
						{
							num2++;
						}
						else if (location.m_maxDistance != 0f && magnitude > location.m_maxDistance)
						{
							num2++;
						}
						else
						{
							Heightmap.Biome biome = WorldGenerator.instance.GetBiome(randomPointInZone);
							if ((location.m_biome & biome) == Heightmap.Biome.None)
							{
								num3++;
							}
							else
							{
								randomPointInZone.y = WorldGenerator.instance.GetHeight(randomPointInZone.x, randomPointInZone.z);
								float num14 = randomPointInZone.y - this.m_waterLevel;
								if (num14 < location.m_minAltitude || num14 > location.m_maxAltitude)
								{
									num5++;
								}
								else
								{
									if (location.m_inForest)
									{
										float forestFactor = WorldGenerator.GetForestFactor(randomPointInZone);
										if (forestFactor < location.m_forestTresholdMin || forestFactor > location.m_forestTresholdMax)
										{
											num6++;
											goto IL_27C;
										}
									}
									float num15;
									Vector3 vector;
									WorldGenerator.instance.GetTerrainDelta(randomPointInZone, location.m_exteriorRadius, out num15, out vector);
									if (num15 > location.m_maxTerrainDelta || num15 < location.m_minTerrainDelta)
									{
										num8++;
									}
									else
									{
										if (location.m_minDistanceFromSimilar <= 0f || !this.HaveLocationInRange(location.m_prefabName, location.m_group, randomPointInZone, location.m_minDistanceFromSimilar))
										{
											this.RegisterLocation(location, randomPointInZone, false);
											num11++;
											break;
										}
										num7++;
									}
								}
							}
						}
						IL_27C:;
					}
				}
			}
			num13++;
		}
		if (num11 < location.m_quantity)
		{
			ZLog.LogWarning(string.Concat(new object[]
			{
				"Failed to place all ",
				location.m_prefabName,
				", placed ",
				num11,
				" out of ",
				location.m_quantity
			}));
			ZLog.DevLog("errorLocationInZone " + num);
			ZLog.DevLog("errorCenterDistance " + num2);
			ZLog.DevLog("errorBiome " + num3);
			ZLog.DevLog("errorBiomeArea " + num4);
			ZLog.DevLog("errorAlt " + num5);
			ZLog.DevLog("errorForest " + num6);
			ZLog.DevLog("errorSimilar " + num7);
			ZLog.DevLog("errorTerrainDelta " + num8);
		}
		DateTime.Now - now;
	}

	// Token: 0x06000C2D RID: 3117 RVA: 0x0005763C File Offset: 0x0005583C
	private Vector2i GetRandomZone(float range)
	{
		int num = (int)range / (int)this.m_zoneSize;
		Vector2i vector2i;
		do
		{
			vector2i = new Vector2i(UnityEngine.Random.Range(-num, num), UnityEngine.Random.Range(-num, num));
		}
		while (this.GetZonePos(vector2i).magnitude >= 10000f);
		return vector2i;
	}

	// Token: 0x06000C2E RID: 3118 RVA: 0x00057684 File Offset: 0x00055884
	private Vector3 GetRandomPointInZone(Vector2i zone, float locationRadius)
	{
		Vector3 zonePos = this.GetZonePos(zone);
		float num = this.m_zoneSize / 2f;
		float x = UnityEngine.Random.Range(-num + locationRadius, num - locationRadius);
		float z = UnityEngine.Random.Range(-num + locationRadius, num - locationRadius);
		return zonePos + new Vector3(x, 0f, z);
	}

	// Token: 0x06000C2F RID: 3119 RVA: 0x000576D0 File Offset: 0x000558D0
	private Vector3 GetRandomPointInZone(float locationRadius)
	{
		Vector3 point = new Vector3(UnityEngine.Random.Range(-10000f, 10000f), 0f, UnityEngine.Random.Range(-10000f, 10000f));
		Vector2i zone = this.GetZone(point);
		Vector3 zonePos = this.GetZonePos(zone);
		float num = this.m_zoneSize / 2f;
		return new Vector3(UnityEngine.Random.Range(zonePos.x - num + locationRadius, zonePos.x + num - locationRadius), 0f, UnityEngine.Random.Range(zonePos.z - num + locationRadius, zonePos.z + num - locationRadius));
	}

	// Token: 0x06000C30 RID: 3120 RVA: 0x00057760 File Offset: 0x00055960
	private void PlaceLocations(Vector2i zoneID, Vector3 zoneCenterPos, Transform parent, Heightmap hmap, List<ZoneSystem.ClearArea> clearAreas, ZoneSystem.SpawnMode mode, List<GameObject> spawnedObjects)
	{
		if (!this.m_locationsGenerated)
		{
			this.GenerateLocations();
		}
		DateTime now = DateTime.Now;
		ZoneSystem.LocationInstance locationInstance;
		if (this.m_locationInstances.TryGetValue(zoneID, out locationInstance))
		{
			if (locationInstance.m_placed)
			{
				return;
			}
			Vector3 position = locationInstance.m_position;
			Vector3 vector;
			Heightmap.Biome biome;
			Heightmap.BiomeArea biomeArea;
			Heightmap heightmap;
			this.GetGroundData(ref position, out vector, out biome, out biomeArea, out heightmap);
			if (locationInstance.m_location.m_snapToWater)
			{
				position.y = this.m_waterLevel;
			}
			if (locationInstance.m_location.m_location.m_clearArea)
			{
				ZoneSystem.ClearArea item = new ZoneSystem.ClearArea(position, locationInstance.m_location.m_exteriorRadius);
				clearAreas.Add(item);
			}
			Quaternion rot = Quaternion.identity;
			if (locationInstance.m_location.m_slopeRotation)
			{
				float num;
				Vector3 vector2;
				this.GetTerrainDelta(position, locationInstance.m_location.m_exteriorRadius, out num, out vector2);
				Vector3 forward = new Vector3(vector2.x, 0f, vector2.z);
				forward.Normalize();
				rot = Quaternion.LookRotation(forward);
				Vector3 eulerAngles = rot.eulerAngles;
				eulerAngles.y = Mathf.Round(eulerAngles.y / 22.5f) * 22.5f;
				rot.eulerAngles = eulerAngles;
			}
			else if (locationInstance.m_location.m_randomRotation)
			{
				rot = Quaternion.Euler(0f, (float)UnityEngine.Random.Range(0, 16) * 22.5f, 0f);
			}
			int seed = WorldGenerator.instance.GetSeed() + zoneID.x * 4271 + zoneID.y * 9187;
			this.SpawnLocation(locationInstance.m_location, seed, position, rot, mode, spawnedObjects);
			locationInstance.m_placed = true;
			this.m_locationInstances[zoneID] = locationInstance;
			TimeSpan timeSpan = DateTime.Now - now;
			ZLog.Log(string.Concat(new object[]
			{
				"Placed locations in zone ",
				zoneID,
				"  duration ",
				timeSpan.TotalMilliseconds,
				" ms"
			}));
			if (locationInstance.m_location.m_unique)
			{
				this.RemoveUnplacedLocations(locationInstance.m_location);
			}
			if (locationInstance.m_location.m_iconPlaced)
			{
				this.SendLocationIcons(ZRoutedRpc.Everybody);
			}
		}
	}

	// Token: 0x06000C31 RID: 3121 RVA: 0x00057980 File Offset: 0x00055B80
	private void RemoveUnplacedLocations(ZoneSystem.ZoneLocation location)
	{
		List<Vector2i> list = new List<Vector2i>();
		foreach (KeyValuePair<Vector2i, ZoneSystem.LocationInstance> keyValuePair in this.m_locationInstances)
		{
			if (keyValuePair.Value.m_location == location && !keyValuePair.Value.m_placed)
			{
				list.Add(keyValuePair.Key);
			}
		}
		foreach (Vector2i key in list)
		{
			this.m_locationInstances.Remove(key);
		}
		ZLog.DevLog("Removed " + list.Count.ToString() + " unplaced locations of type " + location.m_prefabName);
	}

	// Token: 0x06000C32 RID: 3122 RVA: 0x00057A6C File Offset: 0x00055C6C
	public bool TestSpawnLocation(string name, Vector3 pos)
	{
		if (!ZNet.instance.IsServer())
		{
			return false;
		}
		ZoneSystem.ZoneLocation location = this.GetLocation(name);
		if (location == null)
		{
			ZLog.Log("Missing location:" + name);
			global::Console.instance.Print("Missing location:" + name);
			return false;
		}
		if (location.m_prefab == null)
		{
			ZLog.Log("Missing prefab in location:" + name);
			global::Console.instance.Print("Missing location:" + name);
			return false;
		}
		float num = Mathf.Max(location.m_exteriorRadius, location.m_interiorRadius);
		Vector2i zone = this.GetZone(pos);
		Vector3 zonePos = this.GetZonePos(zone);
		pos.x = Mathf.Clamp(pos.x, zonePos.x - this.m_zoneSize / 2f + num, zonePos.x + this.m_zoneSize / 2f - num);
		pos.z = Mathf.Clamp(pos.z, zonePos.z - this.m_zoneSize / 2f + num, zonePos.z + this.m_zoneSize / 2f - num);
		ZLog.Log(string.Concat(new object[]
		{
			"radius ",
			num,
			"  ",
			zonePos,
			" ",
			pos
		}));
		MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Location spawned, world saving DISABLED until restart", 0, null);
		this.m_didZoneTest = true;
		float y = (float)UnityEngine.Random.Range(0, 16) * 22.5f;
		List<GameObject> spawnedGhostObjects = new List<GameObject>();
		this.SpawnLocation(location, UnityEngine.Random.Range(0, 99999), pos, Quaternion.Euler(0f, y, 0f), ZoneSystem.SpawnMode.Full, spawnedGhostObjects);
		return true;
	}

	// Token: 0x06000C33 RID: 3123 RVA: 0x00057C28 File Offset: 0x00055E28
	public GameObject SpawnProxyLocation(int hash, int seed, Vector3 pos, Quaternion rot)
	{
		ZoneSystem.ZoneLocation location = this.GetLocation(hash);
		if (location == null)
		{
			ZLog.LogWarning("Missing location:" + hash);
			return null;
		}
		List<GameObject> spawnedGhostObjects = new List<GameObject>();
		return this.SpawnLocation(location, seed, pos, rot, ZoneSystem.SpawnMode.Client, spawnedGhostObjects);
	}

	// Token: 0x06000C34 RID: 3124 RVA: 0x00057C6C File Offset: 0x00055E6C
	private GameObject SpawnLocation(ZoneSystem.ZoneLocation location, int seed, Vector3 pos, Quaternion rot, ZoneSystem.SpawnMode mode, List<GameObject> spawnedGhostObjects)
	{
		Vector3 position = location.m_prefab.transform.position;
		Quaternion lhs = Quaternion.Inverse(location.m_prefab.transform.rotation);
		UnityEngine.Random.InitState(seed);
		if (mode == ZoneSystem.SpawnMode.Full || mode == ZoneSystem.SpawnMode.Ghost)
		{
			foreach (ZNetView znetView in location.m_netViews)
			{
				znetView.gameObject.SetActive(true);
			}
			foreach (RandomSpawn randomSpawn in location.m_randomSpawns)
			{
				randomSpawn.Randomize();
			}
			WearNTear.m_randomInitialDamage = location.m_location.m_applyRandomDamage;
			foreach (ZNetView znetView2 in location.m_netViews)
			{
				if (znetView2.gameObject.activeSelf)
				{
					Vector3 point = znetView2.gameObject.transform.position - position;
					Vector3 position2 = pos + rot * point;
					Quaternion rhs = lhs * znetView2.gameObject.transform.rotation;
					Quaternion rotation = rot * rhs;
					if (mode == ZoneSystem.SpawnMode.Ghost)
					{
						ZNetView.StartGhostInit();
					}
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(znetView2.gameObject, position2, rotation);
					gameObject.GetComponent<ZNetView>().GetZDO().SetPGWVersion(this.m_pgwVersion);
					DungeonGenerator component = gameObject.GetComponent<DungeonGenerator>();
					if (component)
					{
						component.Generate(mode);
					}
					if (mode == ZoneSystem.SpawnMode.Ghost)
					{
						spawnedGhostObjects.Add(gameObject);
						ZNetView.FinishGhostInit();
					}
				}
			}
			WearNTear.m_randomInitialDamage = false;
			this.CreateLocationProxy(location, seed, pos, rot, mode, spawnedGhostObjects);
			SnapToGround.SnappAll();
			return null;
		}
		foreach (RandomSpawn randomSpawn2 in location.m_randomSpawns)
		{
			randomSpawn2.Randomize();
		}
		foreach (ZNetView znetView3 in location.m_netViews)
		{
			znetView3.gameObject.SetActive(false);
		}
		GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(location.m_prefab, pos, rot);
		gameObject2.SetActive(true);
		SnapToGround.SnappAll();
		return gameObject2;
	}

	// Token: 0x06000C35 RID: 3125 RVA: 0x00057F08 File Offset: 0x00056108
	private void CreateLocationProxy(ZoneSystem.ZoneLocation location, int seed, Vector3 pos, Quaternion rotation, ZoneSystem.SpawnMode mode, List<GameObject> spawnedGhostObjects)
	{
		if (mode == ZoneSystem.SpawnMode.Ghost)
		{
			ZNetView.StartGhostInit();
		}
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_locationProxyPrefab, pos, rotation);
		LocationProxy component = gameObject.GetComponent<LocationProxy>();
		bool spawnNow = mode == ZoneSystem.SpawnMode.Full;
		component.SetLocation(location.m_prefab.name, seed, spawnNow, this.m_pgwVersion);
		if (mode == ZoneSystem.SpawnMode.Ghost)
		{
			spawnedGhostObjects.Add(gameObject);
			ZNetView.FinishGhostInit();
		}
	}

	// Token: 0x06000C36 RID: 3126 RVA: 0x00057F64 File Offset: 0x00056164
	private void RegisterLocation(ZoneSystem.ZoneLocation location, Vector3 pos, bool generated)
	{
		ZoneSystem.LocationInstance value = default(ZoneSystem.LocationInstance);
		value.m_location = location;
		value.m_position = pos;
		value.m_placed = generated;
		Vector2i zone = this.GetZone(pos);
		if (this.m_locationInstances.ContainsKey(zone))
		{
			ZLog.LogWarning("Location already exist in zone " + zone);
			return;
		}
		this.m_locationInstances.Add(zone, value);
	}

	// Token: 0x06000C37 RID: 3127 RVA: 0x00057FCC File Offset: 0x000561CC
	private bool HaveLocationInRange(string prefabName, string group, Vector3 p, float radius)
	{
		foreach (ZoneSystem.LocationInstance locationInstance in this.m_locationInstances.Values)
		{
			if ((locationInstance.m_location.m_prefabName == prefabName || (group.Length > 0 && group == locationInstance.m_location.m_group)) && Vector3.Distance(locationInstance.m_position, p) < radius)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x06000C38 RID: 3128 RVA: 0x00058064 File Offset: 0x00056264
	public bool GetLocationIcon(string name, out Vector3 pos)
	{
		if (ZNet.instance.IsServer())
		{
			using (Dictionary<Vector2i, ZoneSystem.LocationInstance>.Enumerator enumerator = this.m_locationInstances.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<Vector2i, ZoneSystem.LocationInstance> keyValuePair = enumerator.Current;
					if ((keyValuePair.Value.m_location.m_iconAlways || (keyValuePair.Value.m_location.m_iconPlaced && keyValuePair.Value.m_placed)) && keyValuePair.Value.m_location.m_prefabName == name)
					{
						pos = keyValuePair.Value.m_position;
						return true;
					}
				}
				goto IL_F1;
			}
		}
		foreach (KeyValuePair<Vector3, string> keyValuePair2 in this.m_locationIcons)
		{
			if (keyValuePair2.Value == name)
			{
				pos = keyValuePair2.Key;
				return true;
			}
		}
		IL_F1:
		pos = Vector3.zero;
		return false;
	}

	// Token: 0x06000C39 RID: 3129 RVA: 0x0005818C File Offset: 0x0005638C
	public void GetLocationIcons(Dictionary<Vector3, string> icons)
	{
		if (ZNet.instance.IsServer())
		{
			using (Dictionary<Vector2i, ZoneSystem.LocationInstance>.ValueCollection.Enumerator enumerator = this.m_locationInstances.Values.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ZoneSystem.LocationInstance locationInstance = enumerator.Current;
					if (locationInstance.m_location.m_iconAlways || (locationInstance.m_location.m_iconPlaced && locationInstance.m_placed))
					{
						icons[locationInstance.m_position] = locationInstance.m_location.m_prefabName;
					}
				}
				return;
			}
		}
		foreach (KeyValuePair<Vector3, string> keyValuePair in this.m_locationIcons)
		{
			icons.Add(keyValuePair.Key, keyValuePair.Value);
		}
	}

	// Token: 0x06000C3A RID: 3130 RVA: 0x00058274 File Offset: 0x00056474
	private void GetTerrainDelta(Vector3 center, float radius, out float delta, out Vector3 slopeDirection)
	{
		int num = 10;
		float num2 = -999999f;
		float num3 = 999999f;
		Vector3 b = center;
		Vector3 a = center;
		for (int i = 0; i < num; i++)
		{
			Vector2 vector = UnityEngine.Random.insideUnitCircle * radius;
			Vector3 vector2 = center + new Vector3(vector.x, 0f, vector.y);
			float groundHeight = this.GetGroundHeight(vector2);
			if (groundHeight < num3)
			{
				num3 = groundHeight;
				a = vector2;
			}
			if (groundHeight > num2)
			{
				num2 = groundHeight;
				b = vector2;
			}
		}
		delta = num2 - num3;
		slopeDirection = Vector3.Normalize(a - b);
	}

	// Token: 0x06000C3B RID: 3131 RVA: 0x0005830C File Offset: 0x0005650C
	public bool IsBlocked(Vector3 p)
	{
		p.y += 2000f;
		return Physics.Raycast(p, Vector3.down, 10000f, this.m_blockRayMask);
	}

	// Token: 0x06000C3C RID: 3132 RVA: 0x0005833C File Offset: 0x0005653C
	public float GetAverageGroundHeight(Vector3 p, float radius)
	{
		Vector3 origin = p;
		origin.y = 6000f;
		RaycastHit raycastHit;
		if (Physics.Raycast(origin, Vector3.down, out raycastHit, 10000f, this.m_terrainRayMask))
		{
			return raycastHit.point.y;
		}
		return p.y;
	}

	// Token: 0x06000C3D RID: 3133 RVA: 0x00058384 File Offset: 0x00056584
	public float GetGroundHeight(Vector3 p)
	{
		Vector3 origin = p;
		origin.y = 6000f;
		RaycastHit raycastHit;
		if (Physics.Raycast(origin, Vector3.down, out raycastHit, 10000f, this.m_terrainRayMask))
		{
			return raycastHit.point.y;
		}
		return p.y;
	}

	// Token: 0x06000C3E RID: 3134 RVA: 0x000583CC File Offset: 0x000565CC
	public bool GetGroundHeight(Vector3 p, out float height)
	{
		p.y = 6000f;
		RaycastHit raycastHit;
		if (Physics.Raycast(p, Vector3.down, out raycastHit, 10000f, this.m_terrainRayMask))
		{
			height = raycastHit.point.y;
			return true;
		}
		height = 0f;
		return false;
	}

	// Token: 0x06000C3F RID: 3135 RVA: 0x00058418 File Offset: 0x00056618
	public float GetSolidHeight(Vector3 p)
	{
		Vector3 origin = p;
		origin.y += 1000f;
		RaycastHit raycastHit;
		if (Physics.Raycast(origin, Vector3.down, out raycastHit, 2000f, this.m_solidRayMask))
		{
			return raycastHit.point.y;
		}
		return p.y;
	}

	// Token: 0x06000C40 RID: 3136 RVA: 0x00058464 File Offset: 0x00056664
	public bool GetSolidHeight(Vector3 p, out float height)
	{
		p.y += 1000f;
		RaycastHit raycastHit;
		if (Physics.Raycast(p, Vector3.down, out raycastHit, 2000f, this.m_solidRayMask) && !raycastHit.collider.attachedRigidbody)
		{
			height = raycastHit.point.y;
			return true;
		}
		height = 0f;
		return false;
	}

	// Token: 0x06000C41 RID: 3137 RVA: 0x000584C8 File Offset: 0x000566C8
	public bool GetSolidHeight(Vector3 p, float radius, out float height, Transform ignore)
	{
		height = p.y - 1000f;
		p.y += 1000f;
		int num;
		if (radius <= 0f)
		{
			num = Physics.RaycastNonAlloc(p, Vector3.down, this.rayHits, 2000f, this.m_solidRayMask);
		}
		else
		{
			num = Physics.SphereCastNonAlloc(p, radius, Vector3.down, this.rayHits, 2000f, this.m_solidRayMask);
		}
		bool result = false;
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = this.rayHits[i];
			Collider collider = raycastHit.collider;
			if (!(collider.attachedRigidbody != null) && (!(ignore != null) || !Utils.IsParent(collider.transform, ignore)))
			{
				if (raycastHit.point.y > height)
				{
					height = raycastHit.point.y;
				}
				result = true;
			}
		}
		return result;
	}

	// Token: 0x06000C42 RID: 3138 RVA: 0x000585A8 File Offset: 0x000567A8
	public bool GetSolidHeight(Vector3 p, out float height, out Vector3 normal)
	{
		GameObject gameObject;
		return this.GetSolidHeight(p, out height, out normal, out gameObject);
	}

	// Token: 0x06000C43 RID: 3139 RVA: 0x000585C0 File Offset: 0x000567C0
	public bool GetSolidHeight(Vector3 p, out float height, out Vector3 normal, out GameObject go)
	{
		p.y += 1000f;
		RaycastHit raycastHit;
		if (Physics.Raycast(p, Vector3.down, out raycastHit, 2000f, this.m_solidRayMask) && !raycastHit.collider.attachedRigidbody)
		{
			height = raycastHit.point.y;
			normal = raycastHit.normal;
			go = raycastHit.collider.gameObject;
			return true;
		}
		height = 0f;
		normal = Vector3.zero;
		go = null;
		return false;
	}

	// Token: 0x06000C44 RID: 3140 RVA: 0x00058650 File Offset: 0x00056850
	public bool FindFloor(Vector3 p, out float height)
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(p + Vector3.up * 1f, Vector3.down, out raycastHit, 1000f, this.m_solidRayMask))
		{
			height = raycastHit.point.y;
			return true;
		}
		height = 0f;
		return false;
	}

	// Token: 0x06000C45 RID: 3141 RVA: 0x000586A4 File Offset: 0x000568A4
	public void GetGroundData(ref Vector3 p, out Vector3 normal, out Heightmap.Biome biome, out Heightmap.BiomeArea biomeArea, out Heightmap hmap)
	{
		biome = Heightmap.Biome.None;
		biomeArea = Heightmap.BiomeArea.Everything;
		hmap = null;
		RaycastHit raycastHit;
		if (Physics.Raycast(p + Vector3.up * 5000f, Vector3.down, out raycastHit, 10000f, this.m_terrainRayMask))
		{
			p.y = raycastHit.point.y;
			normal = raycastHit.normal;
			Heightmap component = raycastHit.collider.GetComponent<Heightmap>();
			if (component)
			{
				biome = component.GetBiome(raycastHit.point);
				biomeArea = component.GetBiomeArea();
				hmap = component;
			}
			return;
		}
		normal = Vector3.up;
	}

	// Token: 0x06000C46 RID: 3142 RVA: 0x0005874C File Offset: 0x0005694C
	private void UpdateTTL(float dt)
	{
		foreach (KeyValuePair<Vector2i, ZoneSystem.ZoneData> keyValuePair in this.m_zones)
		{
			keyValuePair.Value.m_ttl += dt;
		}
		foreach (KeyValuePair<Vector2i, ZoneSystem.ZoneData> keyValuePair2 in this.m_zones)
		{
			if (keyValuePair2.Value.m_ttl > this.m_zoneTTL && !ZNetScene.instance.HaveInstanceInSector(keyValuePair2.Key))
			{
				UnityEngine.Object.Destroy(keyValuePair2.Value.m_root);
				this.m_zones.Remove(keyValuePair2.Key);
				break;
			}
		}
	}

	// Token: 0x06000C47 RID: 3143 RVA: 0x00058834 File Offset: 0x00056A34
	public void GetVegetation(Heightmap.Biome biome, List<ZoneSystem.ZoneVegetation> vegetation)
	{
		foreach (ZoneSystem.ZoneVegetation zoneVegetation in this.m_vegetation)
		{
			if ((zoneVegetation.m_biome & biome) != Heightmap.Biome.None || zoneVegetation.m_biome == biome)
			{
				vegetation.Add(zoneVegetation);
			}
		}
	}

	// Token: 0x06000C48 RID: 3144 RVA: 0x0005889C File Offset: 0x00056A9C
	public void GetLocations(Heightmap.Biome biome, List<ZoneSystem.ZoneLocation> locations, bool skipDisabled)
	{
		foreach (ZoneSystem.ZoneLocation zoneLocation in this.m_locations)
		{
			if (((zoneLocation.m_biome & biome) != Heightmap.Biome.None || zoneLocation.m_biome == biome) && (!skipDisabled || zoneLocation.m_enable))
			{
				locations.Add(zoneLocation);
			}
		}
	}

	// Token: 0x06000C49 RID: 3145 RVA: 0x00058910 File Offset: 0x00056B10
	public bool FindClosestLocation(string name, Vector3 point, out ZoneSystem.LocationInstance closest)
	{
		float num = 999999f;
		closest = default(ZoneSystem.LocationInstance);
		bool result = false;
		foreach (ZoneSystem.LocationInstance locationInstance in this.m_locationInstances.Values)
		{
			float num2 = Vector3.Distance(locationInstance.m_position, point);
			if (locationInstance.m_location.m_prefabName == name && num2 < num)
			{
				num = num2;
				closest = locationInstance;
				result = true;
			}
		}
		return result;
	}

	// Token: 0x06000C4A RID: 3146 RVA: 0x000589A4 File Offset: 0x00056BA4
	public Vector2i GetZone(Vector3 point)
	{
		int x = Mathf.FloorToInt((point.x + this.m_zoneSize / 2f) / this.m_zoneSize);
		int y = Mathf.FloorToInt((point.z + this.m_zoneSize / 2f) / this.m_zoneSize);
		return new Vector2i(x, y);
	}

	// Token: 0x06000C4B RID: 3147 RVA: 0x000589F6 File Offset: 0x00056BF6
	public Vector3 GetZonePos(Vector2i id)
	{
		return new Vector3((float)id.x * this.m_zoneSize, 0f, (float)id.y * this.m_zoneSize);
	}

	// Token: 0x06000C4C RID: 3148 RVA: 0x00058A1E File Offset: 0x00056C1E
	private void SetZoneGenerated(Vector2i zoneID)
	{
		this.m_generatedZones.Add(zoneID);
	}

	// Token: 0x06000C4D RID: 3149 RVA: 0x00058A2D File Offset: 0x00056C2D
	private bool IsZoneGenerated(Vector2i zoneID)
	{
		return this.m_generatedZones.Contains(zoneID);
	}

	// Token: 0x06000C4E RID: 3150 RVA: 0x00058A3B File Offset: 0x00056C3B
	public bool SkipSaving()
	{
		return this.m_error || this.m_didZoneTest;
	}

	// Token: 0x06000C4F RID: 3151 RVA: 0x00058A4D File Offset: 0x00056C4D
	public void ResetGlobalKeys()
	{
		this.m_globalKeys.Clear();
		this.SendGlobalKeys(ZRoutedRpc.Everybody);
	}

	// Token: 0x06000C50 RID: 3152 RVA: 0x00058A65 File Offset: 0x00056C65
	public void SetGlobalKey(string name)
	{
		ZRoutedRpc.instance.InvokeRoutedRPC("SetGlobalKey", new object[]
		{
			name
		});
	}

	// Token: 0x06000C51 RID: 3153 RVA: 0x00058A80 File Offset: 0x00056C80
	public bool GetGlobalKey(string name)
	{
		return this.m_globalKeys.Contains(name);
	}

	// Token: 0x06000C52 RID: 3154 RVA: 0x00058A8E File Offset: 0x00056C8E
	private void RPC_SetGlobalKey(long sender, string name)
	{
		if (this.m_globalKeys.Contains(name))
		{
			return;
		}
		this.m_globalKeys.Add(name);
		this.SendGlobalKeys(ZRoutedRpc.Everybody);
	}

	// Token: 0x06000C53 RID: 3155 RVA: 0x00058AB7 File Offset: 0x00056CB7
	public List<string> GetGlobalKeys()
	{
		return new List<string>(this.m_globalKeys);
	}

	// Token: 0x06000C54 RID: 3156 RVA: 0x00058AC4 File Offset: 0x00056CC4
	public Dictionary<Vector2i, ZoneSystem.LocationInstance>.ValueCollection GetLocationList()
	{
		return this.m_locationInstances.Values;
	}

	// Token: 0x04000B2D RID: 2861
	private Dictionary<Vector3, string> tempIconList = new Dictionary<Vector3, string>();

	// Token: 0x04000B2E RID: 2862
	private RaycastHit[] rayHits = new RaycastHit[200];

	// Token: 0x04000B2F RID: 2863
	private static ZoneSystem m_instance;

	// Token: 0x04000B30 RID: 2864
	[HideInInspector]
	public List<Heightmap.Biome> m_biomeFolded = new List<Heightmap.Biome>();

	// Token: 0x04000B31 RID: 2865
	[HideInInspector]
	public List<Heightmap.Biome> m_vegetationFolded = new List<Heightmap.Biome>();

	// Token: 0x04000B32 RID: 2866
	[HideInInspector]
	public List<Heightmap.Biome> m_locationFolded = new List<Heightmap.Biome>();

	// Token: 0x04000B33 RID: 2867
	[NonSerialized]
	public bool m_drawLocations;

	// Token: 0x04000B34 RID: 2868
	[NonSerialized]
	public string m_drawLocationsFilter = "";

	// Token: 0x04000B35 RID: 2869
	[global::Tooltip("Zones to load around center sector")]
	public int m_activeArea = 1;

	// Token: 0x04000B36 RID: 2870
	public int m_activeDistantArea = 1;

	// Token: 0x04000B37 RID: 2871
	[global::Tooltip("Zone size, should match netscene sector size")]
	public float m_zoneSize = 64f;

	// Token: 0x04000B38 RID: 2872
	[global::Tooltip("Time before destroying inactive zone")]
	public float m_zoneTTL = 4f;

	// Token: 0x04000B39 RID: 2873
	[global::Tooltip("Time before spawning active zone")]
	public float m_zoneTTS = 4f;

	// Token: 0x04000B3A RID: 2874
	public GameObject m_zonePrefab;

	// Token: 0x04000B3B RID: 2875
	public GameObject m_zoneCtrlPrefab;

	// Token: 0x04000B3C RID: 2876
	public GameObject m_locationProxyPrefab;

	// Token: 0x04000B3D RID: 2877
	public float m_waterLevel = 30f;

	// Token: 0x04000B3E RID: 2878
	[Header("Versions")]
	public int m_pgwVersion = 53;

	// Token: 0x04000B3F RID: 2879
	public int m_locationVersion = 1;

	// Token: 0x04000B40 RID: 2880
	[Header("Generation data")]
	public List<ZoneSystem.ZoneVegetation> m_vegetation = new List<ZoneSystem.ZoneVegetation>();

	// Token: 0x04000B41 RID: 2881
	public List<ZoneSystem.ZoneLocation> m_locations = new List<ZoneSystem.ZoneLocation>();

	// Token: 0x04000B42 RID: 2882
	private Dictionary<int, ZoneSystem.ZoneLocation> m_locationsByHash = new Dictionary<int, ZoneSystem.ZoneLocation>();

	// Token: 0x04000B43 RID: 2883
	private bool m_error;

	// Token: 0x04000B44 RID: 2884
	private bool m_didZoneTest;

	// Token: 0x04000B45 RID: 2885
	private int m_terrainRayMask;

	// Token: 0x04000B46 RID: 2886
	private int m_blockRayMask;

	// Token: 0x04000B47 RID: 2887
	private int m_solidRayMask;

	// Token: 0x04000B48 RID: 2888
	private float m_updateTimer;

	// Token: 0x04000B49 RID: 2889
	private Dictionary<Vector2i, ZoneSystem.ZoneData> m_zones = new Dictionary<Vector2i, ZoneSystem.ZoneData>();

	// Token: 0x04000B4A RID: 2890
	private HashSet<Vector2i> m_generatedZones = new HashSet<Vector2i>();

	// Token: 0x04000B4B RID: 2891
	private bool m_locationsGenerated;

	// Token: 0x04000B4C RID: 2892
	private Dictionary<Vector2i, ZoneSystem.LocationInstance> m_locationInstances = new Dictionary<Vector2i, ZoneSystem.LocationInstance>();

	// Token: 0x04000B4D RID: 2893
	private Dictionary<Vector3, string> m_locationIcons = new Dictionary<Vector3, string>();

	// Token: 0x04000B4E RID: 2894
	private HashSet<string> m_globalKeys = new HashSet<string>();

	// Token: 0x04000B4F RID: 2895
	private HashSet<Vector2i> m_tempGeneratedZonesSaveClone;

	// Token: 0x04000B50 RID: 2896
	private HashSet<string> m_tempGlobalKeysSaveClone;

	// Token: 0x04000B51 RID: 2897
	private List<ZoneSystem.LocationInstance> m_tempLocationsSaveClone;

	// Token: 0x04000B52 RID: 2898
	private bool m_tempLocationsGeneratedSaveClone;

	// Token: 0x04000B53 RID: 2899
	private List<ZoneSystem.ClearArea> m_tempClearAreas = new List<ZoneSystem.ClearArea>();

	// Token: 0x04000B54 RID: 2900
	private List<GameObject> m_tempSpawnedObjects = new List<GameObject>();

	// Token: 0x02000187 RID: 391
	private class ZoneData
	{
		// Token: 0x04001205 RID: 4613
		public GameObject m_root;

		// Token: 0x04001206 RID: 4614
		public float m_ttl;
	}

	// Token: 0x02000188 RID: 392
	private class ClearArea
	{
		// Token: 0x0600118E RID: 4494 RVA: 0x00078F41 File Offset: 0x00077141
		public ClearArea(Vector3 p, float r)
		{
			this.m_center = p;
			this.m_radius = r;
		}

		// Token: 0x04001207 RID: 4615
		public Vector3 m_center;

		// Token: 0x04001208 RID: 4616
		public float m_radius;
	}

	// Token: 0x02000189 RID: 393
	[Serializable]
	public class ZoneVegetation
	{
		// Token: 0x0600118F RID: 4495 RVA: 0x00078F57 File Offset: 0x00077157
		public ZoneSystem.ZoneVegetation Clone()
		{
			return base.MemberwiseClone() as ZoneSystem.ZoneVegetation;
		}

		// Token: 0x04001209 RID: 4617
		public string m_name = "veg";

		// Token: 0x0400120A RID: 4618
		public GameObject m_prefab;

		// Token: 0x0400120B RID: 4619
		public bool m_enable = true;

		// Token: 0x0400120C RID: 4620
		public float m_min;

		// Token: 0x0400120D RID: 4621
		public float m_max = 10f;

		// Token: 0x0400120E RID: 4622
		public bool m_forcePlacement;

		// Token: 0x0400120F RID: 4623
		public float m_scaleMin = 1f;

		// Token: 0x04001210 RID: 4624
		public float m_scaleMax = 1f;

		// Token: 0x04001211 RID: 4625
		public float m_randTilt;

		// Token: 0x04001212 RID: 4626
		public float m_chanceToUseGroundTilt;

		// Token: 0x04001213 RID: 4627
		[BitMask(typeof(Heightmap.Biome))]
		public Heightmap.Biome m_biome;

		// Token: 0x04001214 RID: 4628
		[BitMask(typeof(Heightmap.BiomeArea))]
		public Heightmap.BiomeArea m_biomeArea = Heightmap.BiomeArea.Everything;

		// Token: 0x04001215 RID: 4629
		public bool m_blockCheck = true;

		// Token: 0x04001216 RID: 4630
		public float m_minAltitude = -1000f;

		// Token: 0x04001217 RID: 4631
		public float m_maxAltitude = 1000f;

		// Token: 0x04001218 RID: 4632
		public float m_minOceanDepth;

		// Token: 0x04001219 RID: 4633
		public float m_maxOceanDepth;

		// Token: 0x0400121A RID: 4634
		public float m_minTilt;

		// Token: 0x0400121B RID: 4635
		public float m_maxTilt = 90f;

		// Token: 0x0400121C RID: 4636
		public float m_terrainDeltaRadius;

		// Token: 0x0400121D RID: 4637
		public float m_maxTerrainDelta = 2f;

		// Token: 0x0400121E RID: 4638
		public float m_minTerrainDelta;

		// Token: 0x0400121F RID: 4639
		public bool m_snapToWater;

		// Token: 0x04001220 RID: 4640
		public float m_groundOffset;

		// Token: 0x04001221 RID: 4641
		public int m_groupSizeMin = 1;

		// Token: 0x04001222 RID: 4642
		public int m_groupSizeMax = 1;

		// Token: 0x04001223 RID: 4643
		public float m_groupRadius;

		// Token: 0x04001224 RID: 4644
		[Header("Forest fractal 0-1 inside forest")]
		public bool m_inForest;

		// Token: 0x04001225 RID: 4645
		public float m_forestTresholdMin;

		// Token: 0x04001226 RID: 4646
		public float m_forestTresholdMax = 1f;

		// Token: 0x04001227 RID: 4647
		[HideInInspector]
		public bool m_foldout;
	}

	// Token: 0x0200018A RID: 394
	[Serializable]
	public class ZoneLocation
	{
		// Token: 0x06001191 RID: 4497 RVA: 0x00078FFD File Offset: 0x000771FD
		public ZoneSystem.ZoneLocation Clone()
		{
			return base.MemberwiseClone() as ZoneSystem.ZoneLocation;
		}

		// Token: 0x04001228 RID: 4648
		public bool m_enable = true;

		// Token: 0x04001229 RID: 4649
		public string m_prefabName;

		// Token: 0x0400122A RID: 4650
		[BitMask(typeof(Heightmap.Biome))]
		public Heightmap.Biome m_biome;

		// Token: 0x0400122B RID: 4651
		[BitMask(typeof(Heightmap.BiomeArea))]
		public Heightmap.BiomeArea m_biomeArea = Heightmap.BiomeArea.Everything;

		// Token: 0x0400122C RID: 4652
		public int m_quantity;

		// Token: 0x0400122D RID: 4653
		public float m_chanceToSpawn = 10f;

		// Token: 0x0400122E RID: 4654
		public bool m_prioritized;

		// Token: 0x0400122F RID: 4655
		public bool m_centerFirst;

		// Token: 0x04001230 RID: 4656
		public bool m_unique;

		// Token: 0x04001231 RID: 4657
		public string m_group = "";

		// Token: 0x04001232 RID: 4658
		public float m_minDistanceFromSimilar;

		// Token: 0x04001233 RID: 4659
		public bool m_iconAlways;

		// Token: 0x04001234 RID: 4660
		public bool m_iconPlaced;

		// Token: 0x04001235 RID: 4661
		public bool m_randomRotation = true;

		// Token: 0x04001236 RID: 4662
		public bool m_slopeRotation;

		// Token: 0x04001237 RID: 4663
		public bool m_snapToWater;

		// Token: 0x04001238 RID: 4664
		public float m_maxTerrainDelta = 2f;

		// Token: 0x04001239 RID: 4665
		public float m_minTerrainDelta;

		// Token: 0x0400123A RID: 4666
		[Header("Forest fractal 0-1 inside forest")]
		public bool m_inForest;

		// Token: 0x0400123B RID: 4667
		public float m_forestTresholdMin;

		// Token: 0x0400123C RID: 4668
		public float m_forestTresholdMax = 1f;

		// Token: 0x0400123D RID: 4669
		[Space(10f)]
		public float m_minDistance;

		// Token: 0x0400123E RID: 4670
		public float m_maxDistance;

		// Token: 0x0400123F RID: 4671
		public float m_minAltitude = -1000f;

		// Token: 0x04001240 RID: 4672
		public float m_maxAltitude = 1000f;

		// Token: 0x04001241 RID: 4673
		[NonSerialized]
		public GameObject m_prefab;

		// Token: 0x04001242 RID: 4674
		[NonSerialized]
		public int m_hash;

		// Token: 0x04001243 RID: 4675
		[NonSerialized]
		public Location m_location;

		// Token: 0x04001244 RID: 4676
		[NonSerialized]
		public float m_interiorRadius = 10f;

		// Token: 0x04001245 RID: 4677
		[NonSerialized]
		public float m_exteriorRadius = 10f;

		// Token: 0x04001246 RID: 4678
		[NonSerialized]
		public List<ZNetView> m_netViews = new List<ZNetView>();

		// Token: 0x04001247 RID: 4679
		[NonSerialized]
		public List<RandomSpawn> m_randomSpawns = new List<RandomSpawn>();

		// Token: 0x04001248 RID: 4680
		[HideInInspector]
		public bool m_foldout;
	}

	// Token: 0x0200018B RID: 395
	public struct LocationInstance
	{
		// Token: 0x04001249 RID: 4681
		public ZoneSystem.ZoneLocation m_location;

		// Token: 0x0400124A RID: 4682
		public Vector3 m_position;

		// Token: 0x0400124B RID: 4683
		public bool m_placed;
	}

	// Token: 0x0200018C RID: 396
	public enum SpawnMode
	{
		// Token: 0x0400124D RID: 4685
		Full,
		// Token: 0x0400124E RID: 4686
		Client,
		// Token: 0x0400124F RID: 4687
		Ghost
	}
}
