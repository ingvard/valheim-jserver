using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Token: 0x020000A6 RID: 166
public class Pathfinding : MonoBehaviour
{
	// Token: 0x1700002A RID: 42
	// (get) Token: 0x06000B34 RID: 2868 RVA: 0x00050898 File Offset: 0x0004EA98
	public static Pathfinding instance
	{
		get
		{
			return Pathfinding.m_instance;
		}
	}

	// Token: 0x06000B35 RID: 2869 RVA: 0x0005089F File Offset: 0x0004EA9F
	private void Awake()
	{
		Pathfinding.m_instance = this;
		this.SetupAgents();
		this.m_path = new NavMeshPath();
	}

	// Token: 0x06000B36 RID: 2870 RVA: 0x000508B8 File Offset: 0x0004EAB8
	private void ClearAgentSettings()
	{
		List<NavMeshBuildSettings> list = new List<NavMeshBuildSettings>();
		for (int i = 0; i < NavMesh.GetSettingsCount(); i++)
		{
			list.Add(NavMesh.GetSettingsByIndex(i));
		}
		foreach (NavMeshBuildSettings navMeshBuildSettings in list)
		{
			if (navMeshBuildSettings.agentTypeID != 0)
			{
				NavMesh.RemoveSettings(navMeshBuildSettings.agentTypeID);
			}
		}
	}

	// Token: 0x06000B37 RID: 2871 RVA: 0x00050938 File Offset: 0x0004EB38
	private void OnDestroy()
	{
		foreach (Pathfinding.NavMeshTile navMeshTile in this.m_tiles.Values)
		{
			this.ClearLinks(navMeshTile);
			if (navMeshTile.m_data)
			{
				NavMesh.RemoveNavMeshData(navMeshTile.m_instance);
			}
		}
		this.m_tiles.Clear();
		this.DestroyAllLinks();
	}

	// Token: 0x06000B38 RID: 2872 RVA: 0x000509BC File Offset: 0x0004EBBC
	private Pathfinding.AgentSettings AddAgent(Pathfinding.AgentType type, Pathfinding.AgentSettings copy = null)
	{
		while (type + 1 > (Pathfinding.AgentType)this.m_agentSettings.Count)
		{
			this.m_agentSettings.Add(null);
		}
		Pathfinding.AgentSettings agentSettings = new Pathfinding.AgentSettings(type);
		if (copy != null)
		{
			agentSettings.m_build.agentHeight = copy.m_build.agentHeight;
			agentSettings.m_build.agentClimb = copy.m_build.agentClimb;
			agentSettings.m_build.agentRadius = copy.m_build.agentRadius;
			agentSettings.m_build.agentSlope = copy.m_build.agentSlope;
		}
		this.m_agentSettings[(int)type] = agentSettings;
		return agentSettings;
	}

	// Token: 0x06000B39 RID: 2873 RVA: 0x00050A5C File Offset: 0x0004EC5C
	private void SetupAgents()
	{
		this.ClearAgentSettings();
		Pathfinding.AgentSettings agentSettings = this.AddAgent(Pathfinding.AgentType.Humanoid, null);
		agentSettings.m_build.agentHeight = 1.8f;
		agentSettings.m_build.agentClimb = 0.3f;
		agentSettings.m_build.agentRadius = 0.4f;
		agentSettings.m_build.agentSlope = 85f;
		this.AddAgent(Pathfinding.AgentType.Wolf, agentSettings).m_build.agentSlope = 85f;
		this.AddAgent(Pathfinding.AgentType.HumanoidNoSwim, agentSettings).m_canSwim = false;
		Pathfinding.AgentSettings agentSettings2 = this.AddAgent(Pathfinding.AgentType.HumanoidBigNoSwim, null);
		agentSettings2.m_build.agentHeight = 2.5f;
		agentSettings2.m_build.agentClimb = 0.3f;
		agentSettings2.m_build.agentRadius = 0.5f;
		agentSettings2.m_build.agentSlope = 85f;
		agentSettings2.m_canSwim = false;
		this.AddAgent(Pathfinding.AgentType.HumanoidAvoidWater, agentSettings).m_avoidWater = true;
		Pathfinding.AgentSettings agentSettings3 = this.AddAgent(Pathfinding.AgentType.TrollSize, null);
		agentSettings3.m_build.agentHeight = 7f;
		agentSettings3.m_build.agentClimb = 0.6f;
		agentSettings3.m_build.agentRadius = 1f;
		agentSettings3.m_build.agentSlope = 85f;
		Pathfinding.AgentSettings agentSettings4 = this.AddAgent(Pathfinding.AgentType.GoblinBruteSize, null);
		agentSettings4.m_build.agentHeight = 3.5f;
		agentSettings4.m_build.agentClimb = 0.3f;
		agentSettings4.m_build.agentRadius = 0.8f;
		agentSettings4.m_build.agentSlope = 85f;
		Pathfinding.AgentSettings agentSettings5 = this.AddAgent(Pathfinding.AgentType.HugeSize, null);
		agentSettings5.m_build.agentHeight = 10f;
		agentSettings5.m_build.agentClimb = 0.6f;
		agentSettings5.m_build.agentRadius = 2f;
		agentSettings5.m_build.agentSlope = 85f;
		Pathfinding.AgentSettings agentSettings6 = this.AddAgent(Pathfinding.AgentType.HorseSize, null);
		agentSettings6.m_build.agentHeight = 2.5f;
		agentSettings6.m_build.agentClimb = 0.3f;
		agentSettings6.m_build.agentRadius = 0.8f;
		agentSettings6.m_build.agentSlope = 85f;
		Pathfinding.AgentSettings agentSettings7 = this.AddAgent(Pathfinding.AgentType.Fish, null);
		agentSettings7.m_build.agentHeight = 0.5f;
		agentSettings7.m_build.agentClimb = 1f;
		agentSettings7.m_build.agentRadius = 0.5f;
		agentSettings7.m_build.agentSlope = 90f;
		agentSettings7.m_canSwim = true;
		agentSettings7.m_canWalk = false;
		agentSettings7.m_swimDepth = 0.4f;
		agentSettings7.m_areaMask = 12;
		Pathfinding.AgentSettings agentSettings8 = this.AddAgent(Pathfinding.AgentType.BigFish, null);
		agentSettings8.m_build.agentHeight = 1.5f;
		agentSettings8.m_build.agentClimb = 1f;
		agentSettings8.m_build.agentRadius = 1f;
		agentSettings8.m_build.agentSlope = 90f;
		agentSettings8.m_canSwim = true;
		agentSettings8.m_canWalk = false;
		agentSettings8.m_swimDepth = 1.5f;
		agentSettings8.m_areaMask = 12;
		NavMesh.SetAreaCost(0, this.m_defaultCost);
		NavMesh.SetAreaCost(3, this.m_waterCost);
	}

	// Token: 0x06000B3A RID: 2874 RVA: 0x00050D40 File Offset: 0x0004EF40
	private Pathfinding.AgentSettings GetSettings(Pathfinding.AgentType agentType)
	{
		return this.m_agentSettings[(int)agentType];
	}

	// Token: 0x06000B3B RID: 2875 RVA: 0x00050D4E File Offset: 0x0004EF4E
	private int GetAgentID(Pathfinding.AgentType agentType)
	{
		return this.GetSettings(agentType).m_build.agentTypeID;
	}

	// Token: 0x06000B3C RID: 2876 RVA: 0x00050D64 File Offset: 0x0004EF64
	private void Update()
	{
		if (this.IsBuilding())
		{
			return;
		}
		this.m_updatePathfindingTimer += Time.deltaTime;
		if (this.m_updatePathfindingTimer > 0.1f)
		{
			this.m_updatePathfindingTimer = 0f;
			this.UpdatePathfinding();
		}
		if (!this.IsBuilding())
		{
			this.DestroyQueuedNavmeshData();
		}
	}

	// Token: 0x06000B3D RID: 2877 RVA: 0x00050DB8 File Offset: 0x0004EFB8
	private void DestroyAllLinks()
	{
		while (this.m_linkRemoveQueue.Count > 0 || this.m_tileRemoveQueue.Count > 0)
		{
			this.DestroyQueuedNavmeshData();
		}
	}

	// Token: 0x06000B3E RID: 2878 RVA: 0x00050DE0 File Offset: 0x0004EFE0
	private void DestroyQueuedNavmeshData()
	{
		if (this.m_linkRemoveQueue.Count > 0)
		{
			int num = Mathf.Min(this.m_linkRemoveQueue.Count, Mathf.Max(25, this.m_linkRemoveQueue.Count / 40));
			for (int i = 0; i < num; i++)
			{
				NavMesh.RemoveLink(this.m_linkRemoveQueue.Dequeue());
			}
			return;
		}
		if (this.m_tileRemoveQueue.Count > 0)
		{
			NavMesh.RemoveNavMeshData(this.m_tileRemoveQueue.Dequeue());
		}
	}

	// Token: 0x06000B3F RID: 2879 RVA: 0x00050E5C File Offset: 0x0004F05C
	private void UpdatePathfinding()
	{
		this.Buildtiles();
		this.TimeoutTiles();
	}

	// Token: 0x06000B40 RID: 2880 RVA: 0x00050E6A File Offset: 0x0004F06A
	public bool HavePath(Vector3 from, Vector3 to, Pathfinding.AgentType agentType)
	{
		return this.GetPath(from, to, null, agentType, true, false);
	}

	// Token: 0x06000B41 RID: 2881 RVA: 0x00050E78 File Offset: 0x0004F078
	public bool FindValidPoint(out Vector3 point, Vector3 center, float range, Pathfinding.AgentType agentType)
	{
		this.PokePoint(center, agentType);
		Pathfinding.AgentSettings settings = this.GetSettings(agentType);
		NavMeshHit navMeshHit;
		if (NavMesh.SamplePosition(center, out navMeshHit, range, new NavMeshQueryFilter
		{
			agentTypeID = (int)settings.m_agentType,
			areaMask = settings.m_areaMask
		}))
		{
			point = navMeshHit.position;
			return true;
		}
		point = center;
		return false;
	}

	// Token: 0x06000B42 RID: 2882 RVA: 0x00050EDC File Offset: 0x0004F0DC
	public bool GetPath(Vector3 from, Vector3 to, List<Vector3> path, Pathfinding.AgentType agentType, bool requireFullPath = false, bool cleanup = true)
	{
		if (path != null)
		{
			path.Clear();
		}
		this.PokeArea(from, agentType);
		this.PokeArea(to, agentType);
		Pathfinding.AgentSettings settings = this.GetSettings(agentType);
		if (!this.SnapToNavMesh(ref from, settings))
		{
			return false;
		}
		if (!this.SnapToNavMesh(ref to, settings))
		{
			return false;
		}
		NavMeshQueryFilter filter = new NavMeshQueryFilter
		{
			agentTypeID = settings.m_build.agentTypeID,
			areaMask = settings.m_areaMask
		};
		if (!NavMesh.CalculatePath(from, to, filter, this.m_path))
		{
			return false;
		}
		if (this.m_path.status == NavMeshPathStatus.PathPartial && requireFullPath)
		{
			return false;
		}
		if (path != null)
		{
			path.AddRange(this.m_path.corners);
			if (cleanup)
			{
				this.CleanPath(path, settings);
			}
		}
		return true;
	}

	// Token: 0x06000B43 RID: 2883 RVA: 0x00050F98 File Offset: 0x0004F198
	private void CleanPath(List<Vector3> basePath, Pathfinding.AgentSettings settings)
	{
		if (basePath.Count <= 2)
		{
			return;
		}
		NavMeshQueryFilter filter = default(NavMeshQueryFilter);
		filter.agentTypeID = settings.m_build.agentTypeID;
		filter.areaMask = settings.m_areaMask;
		int num = 0;
		this.optPath.Clear();
		this.optPath.Add(basePath[num]);
		do
		{
			num = this.FindNextNode(basePath, filter, num);
			this.optPath.Add(basePath[num]);
		}
		while (num < basePath.Count - 1);
		this.tempPath.Clear();
		this.tempPath.Add(this.optPath[0]);
		for (int i = 1; i < this.optPath.Count - 1; i++)
		{
			Vector3 vector = this.optPath[i - 1];
			Vector3 vector2 = this.optPath[i];
			Vector3 vector3 = this.optPath[i + 1];
			Vector3 normalized = (vector3 - vector2).normalized;
			Vector3 normalized2 = (vector2 - vector).normalized;
			Vector3 vector4 = vector2 - (normalized + normalized2).normalized * Vector3.Distance(vector2, vector) * 0.33f;
			vector4.y = (vector2.y + vector.y) * 0.5f;
			Vector3 normalized3 = (vector4 - vector2).normalized;
			NavMeshHit navMeshHit;
			if (!NavMesh.Raycast(vector2 + normalized3 * 0.1f, vector4, out navMeshHit, filter) && !NavMesh.Raycast(vector4, vector, out navMeshHit, filter))
			{
				this.tempPath.Add(vector4);
			}
			this.tempPath.Add(vector2);
			Vector3 vector5 = vector2 + (normalized + normalized2).normalized * Vector3.Distance(vector2, vector3) * 0.33f;
			vector5.y = (vector2.y + vector3.y) * 0.5f;
			Vector3 normalized4 = (vector5 - vector2).normalized;
			if (!NavMesh.Raycast(vector2 + normalized4 * 0.1f, vector5, out navMeshHit, filter) && !NavMesh.Raycast(vector5, vector3, out navMeshHit, filter))
			{
				this.tempPath.Add(vector5);
			}
		}
		this.tempPath.Add(this.optPath[this.optPath.Count - 1]);
		basePath.Clear();
		basePath.AddRange(this.tempPath);
	}

	// Token: 0x06000B44 RID: 2884 RVA: 0x00051230 File Offset: 0x0004F430
	private int FindNextNode(List<Vector3> path, NavMeshQueryFilter filter, int start)
	{
		for (int i = start + 2; i < path.Count; i++)
		{
			NavMeshHit navMeshHit;
			if (NavMesh.Raycast(path[start], path[i], out navMeshHit, filter))
			{
				return i - 1;
			}
		}
		return path.Count - 1;
	}

	// Token: 0x06000B45 RID: 2885 RVA: 0x00051274 File Offset: 0x0004F474
	private bool SnapToNavMesh(ref Vector3 point, Pathfinding.AgentSettings settings)
	{
		if (ZoneSystem.instance)
		{
			float num;
			if (ZoneSystem.instance.GetGroundHeight(point, out num) && point.y < num)
			{
				point.y = num;
			}
			if (settings.m_canSwim)
			{
				point.y = Mathf.Max(ZoneSystem.instance.m_waterLevel - settings.m_swimDepth, point.y);
			}
		}
		NavMeshQueryFilter filter = default(NavMeshQueryFilter);
		filter.agentTypeID = settings.m_build.agentTypeID;
		filter.areaMask = settings.m_areaMask;
		NavMeshHit navMeshHit;
		if (NavMesh.SamplePosition(point, out navMeshHit, 1.5f, filter))
		{
			point = navMeshHit.position;
			return true;
		}
		if (NavMesh.SamplePosition(point, out navMeshHit, 10f, filter))
		{
			point = navMeshHit.position;
			return true;
		}
		if (NavMesh.SamplePosition(point, out navMeshHit, 20f, filter))
		{
			point = navMeshHit.position;
			return true;
		}
		return false;
	}

	// Token: 0x06000B46 RID: 2886 RVA: 0x00051370 File Offset: 0x0004F570
	private void TimeoutTiles()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		foreach (KeyValuePair<Vector3Int, Pathfinding.NavMeshTile> keyValuePair in this.m_tiles)
		{
			if (realtimeSinceStartup - keyValuePair.Value.m_pokeTime > this.m_tileTimeout)
			{
				this.ClearLinks(keyValuePair.Value);
				if (keyValuePair.Value.m_instance.valid)
				{
					this.m_tileRemoveQueue.Enqueue(keyValuePair.Value.m_instance);
				}
				this.m_tiles.Remove(keyValuePair.Key);
				break;
			}
		}
	}

	// Token: 0x06000B47 RID: 2887 RVA: 0x00051424 File Offset: 0x0004F624
	private void PokeArea(Vector3 point, Pathfinding.AgentType agentType)
	{
		Vector3Int tile = this.GetTile(point, agentType);
		this.PokeTile(tile);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				if (j != 0 || i != 0)
				{
					Vector3Int tileID = new Vector3Int(tile.x + j, tile.y + i, tile.z);
					this.PokeTile(tileID);
				}
			}
		}
	}

	// Token: 0x06000B48 RID: 2888 RVA: 0x00051488 File Offset: 0x0004F688
	private void PokePoint(Vector3 point, Pathfinding.AgentType agentType)
	{
		Vector3Int tile = this.GetTile(point, agentType);
		this.PokeTile(tile);
	}

	// Token: 0x06000B49 RID: 2889 RVA: 0x000514A5 File Offset: 0x0004F6A5
	private void PokeTile(Vector3Int tileID)
	{
		this.GetNavTile(tileID).m_pokeTime = Time.realtimeSinceStartup;
	}

	// Token: 0x06000B4A RID: 2890 RVA: 0x000514B8 File Offset: 0x0004F6B8
	private void Buildtiles()
	{
		if (this.UpdateAsyncBuild())
		{
			return;
		}
		Pathfinding.NavMeshTile navMeshTile = null;
		float num = 0f;
		foreach (Pathfinding.NavMeshTile navMeshTile2 in this.m_tiles.Values)
		{
			float num2 = navMeshTile2.m_pokeTime - navMeshTile2.m_buildTime;
			if (num2 > this.m_updateInterval && (navMeshTile == null || num2 > num))
			{
				navMeshTile = navMeshTile2;
				num = num2;
			}
		}
		if (navMeshTile != null)
		{
			this.BuildTile(navMeshTile);
			navMeshTile.m_buildTime = Time.realtimeSinceStartup;
		}
	}

	// Token: 0x06000B4B RID: 2891 RVA: 0x00051558 File Offset: 0x0004F758
	private void BuildTile(Pathfinding.NavMeshTile tile)
	{
		DateTime now = DateTime.Now;
		List<NavMeshBuildSource> list = new List<NavMeshBuildSource>();
		List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();
		Pathfinding.AgentType z = (Pathfinding.AgentType)tile.m_tile.z;
		Pathfinding.AgentSettings settings = this.GetSettings(z);
		Bounds includedWorldBounds = new Bounds(tile.m_center, new Vector3(this.m_tileSize, 6000f, this.m_tileSize));
		Bounds localBounds = new Bounds(Vector3.zero, new Vector3(this.m_tileSize, 6000f, this.m_tileSize));
		int defaultArea = settings.m_canWalk ? 0 : 1;
		NavMeshBuilder.CollectSources(includedWorldBounds, this.m_layers.value, NavMeshCollectGeometry.PhysicsColliders, defaultArea, markups, list);
		if (settings.m_avoidWater)
		{
			List<NavMeshBuildSource> list2 = new List<NavMeshBuildSource>();
			NavMeshBuilder.CollectSources(includedWorldBounds, this.m_waterLayers.value, NavMeshCollectGeometry.PhysicsColliders, 1, markups, list2);
			using (List<NavMeshBuildSource>.Enumerator enumerator = list2.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					NavMeshBuildSource item = enumerator.Current;
					item.transform *= Matrix4x4.Translate(Vector3.down * 0.2f);
					list.Add(item);
				}
				goto IL_1AE;
			}
		}
		if (settings.m_canSwim)
		{
			List<NavMeshBuildSource> list3 = new List<NavMeshBuildSource>();
			NavMeshBuilder.CollectSources(includedWorldBounds, this.m_waterLayers.value, NavMeshCollectGeometry.PhysicsColliders, 3, markups, list3);
			if (settings.m_swimDepth != 0f)
			{
				using (List<NavMeshBuildSource>.Enumerator enumerator = list3.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						NavMeshBuildSource item2 = enumerator.Current;
						item2.transform *= Matrix4x4.Translate(Vector3.down * settings.m_swimDepth);
						list.Add(item2);
					}
					goto IL_1AE;
				}
			}
			list.AddRange(list3);
		}
		IL_1AE:
		if (tile.m_data == null)
		{
			tile.m_data = new NavMeshData();
			tile.m_data.position = tile.m_center;
		}
		this.m_buildOperation = NavMeshBuilder.UpdateNavMeshDataAsync(tile.m_data, settings.m_build, list, localBounds);
		this.m_buildTile = tile;
	}

	// Token: 0x06000B4C RID: 2892 RVA: 0x0005177C File Offset: 0x0004F97C
	private bool IsBuilding()
	{
		return this.m_buildOperation != null && !this.m_buildOperation.isDone;
	}

	// Token: 0x06000B4D RID: 2893 RVA: 0x00051798 File Offset: 0x0004F998
	private bool UpdateAsyncBuild()
	{
		if (this.m_buildOperation == null)
		{
			return false;
		}
		if (!this.m_buildOperation.isDone)
		{
			return true;
		}
		if (!this.m_buildTile.m_instance.valid)
		{
			this.m_buildTile.m_instance = NavMesh.AddNavMeshData(this.m_buildTile.m_data);
		}
		this.RebuildLinks(this.m_buildTile);
		this.m_buildOperation = null;
		this.m_buildTile = null;
		return true;
	}

	// Token: 0x06000B4E RID: 2894 RVA: 0x00051806 File Offset: 0x0004FA06
	private void ClearLinks(Pathfinding.NavMeshTile tile)
	{
		this.ClearLinks(tile.m_links1);
		this.ClearLinks(tile.m_links2);
	}

	// Token: 0x06000B4F RID: 2895 RVA: 0x00051820 File Offset: 0x0004FA20
	private void ClearLinks(List<KeyValuePair<Vector3, NavMeshLinkInstance>> links)
	{
		foreach (KeyValuePair<Vector3, NavMeshLinkInstance> keyValuePair in links)
		{
			this.m_linkRemoveQueue.Enqueue(keyValuePair.Value);
		}
		links.Clear();
	}

	// Token: 0x06000B50 RID: 2896 RVA: 0x00051880 File Offset: 0x0004FA80
	private void RebuildLinks(Pathfinding.NavMeshTile tile)
	{
		Pathfinding.AgentType z = (Pathfinding.AgentType)tile.m_tile.z;
		Pathfinding.AgentSettings settings = this.GetSettings(z);
		float num = this.m_tileSize / 2f;
		this.ConnectAlongEdge(tile.m_links1, tile.m_center + new Vector3(num, 0f, num), tile.m_center + new Vector3(num, 0f, -num), this.m_linkWidth, settings);
		this.ConnectAlongEdge(tile.m_links2, tile.m_center + new Vector3(-num, 0f, num), tile.m_center + new Vector3(num, 0f, num), this.m_linkWidth, settings);
	}

	// Token: 0x06000B51 RID: 2897 RVA: 0x00051934 File Offset: 0x0004FB34
	private void ConnectAlongEdge(List<KeyValuePair<Vector3, NavMeshLinkInstance>> links, Vector3 p0, Vector3 p1, float step, Pathfinding.AgentSettings settings)
	{
		Vector3 normalized = (p1 - p0).normalized;
		Vector3 a = Vector3.Cross(Vector3.up, normalized);
		float num = Vector3.Distance(p0, p1);
		bool canSwim = settings.m_canSwim;
		this.tempStitchPoints.Clear();
		for (float num2 = step / 2f; num2 <= num; num2 += step)
		{
			Vector3 p2 = p0 + normalized * num2;
			this.FindGround(p2, canSwim, this.tempStitchPoints, settings);
		}
		if (this.CompareLinks(this.tempStitchPoints, links))
		{
			return;
		}
		this.ClearLinks(links);
		foreach (Vector3 vector in this.tempStitchPoints)
		{
			NavMeshLinkInstance value = NavMesh.AddLink(new NavMeshLinkData
			{
				startPosition = vector - a * 0.1f,
				endPosition = vector + a * 0.1f,
				width = step,
				costModifier = this.m_linkCost,
				bidirectional = true,
				agentTypeID = settings.m_build.agentTypeID,
				area = 2
			});
			if (value.valid)
			{
				links.Add(new KeyValuePair<Vector3, NavMeshLinkInstance>(vector, value));
			}
		}
	}

	// Token: 0x06000B52 RID: 2898 RVA: 0x00051AA4 File Offset: 0x0004FCA4
	private bool CompareLinks(List<Vector3> tempStitchPoints, List<KeyValuePair<Vector3, NavMeshLinkInstance>> links)
	{
		if (tempStitchPoints.Count != links.Count)
		{
			return false;
		}
		for (int i = 0; i < tempStitchPoints.Count; i++)
		{
			if (tempStitchPoints[i] != links[i].Key)
			{
				return false;
			}
		}
		return true;
	}

	// Token: 0x06000B53 RID: 2899 RVA: 0x00051AF4 File Offset: 0x0004FCF4
	private bool SnapToNearestGround(Vector3 p, out Vector3 pos, float range)
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(p + Vector3.up, Vector3.down, out raycastHit, range + 1f, this.m_layers.value | this.m_waterLayers.value))
		{
			pos = raycastHit.point;
			return true;
		}
		if (Physics.Raycast(p + Vector3.up * range, Vector3.down, out raycastHit, range, this.m_layers.value | this.m_waterLayers.value))
		{
			pos = raycastHit.point;
			return true;
		}
		pos = p;
		return false;
	}

	// Token: 0x06000B54 RID: 2900 RVA: 0x00051B98 File Offset: 0x0004FD98
	private void FindGround(Vector3 p, bool testWater, List<Vector3> hits, Pathfinding.AgentSettings settings)
	{
		p.y = 6000f;
		int layerMask = testWater ? (this.m_layers.value | this.m_waterLayers.value) : this.m_layers.value;
		float agentHeight = settings.m_build.agentHeight;
		float y = p.y;
		int num = Physics.RaycastNonAlloc(p, Vector3.down, this.tempHitArray, 10000f, layerMask);
		for (int i = 0; i < num; i++)
		{
			Vector3 point = this.tempHitArray[i].point;
			if (Mathf.Abs(point.y - y) >= agentHeight)
			{
				y = point.y;
				if ((1 << this.tempHitArray[i].collider.gameObject.layer & this.m_waterLayers) != 0)
				{
					point.y -= settings.m_swimDepth;
				}
				hits.Add(point);
			}
		}
	}

	// Token: 0x06000B55 RID: 2901 RVA: 0x00051C90 File Offset: 0x0004FE90
	private Pathfinding.NavMeshTile GetNavTile(Vector3 point, Pathfinding.AgentType agent)
	{
		Vector3Int tile = this.GetTile(point, agent);
		return this.GetNavTile(tile);
	}

	// Token: 0x06000B56 RID: 2902 RVA: 0x00051CB0 File Offset: 0x0004FEB0
	private Pathfinding.NavMeshTile GetNavTile(Vector3Int tile)
	{
		if (tile == this.m_cachedTileID)
		{
			return this.m_cachedTile;
		}
		Pathfinding.NavMeshTile navMeshTile;
		if (this.m_tiles.TryGetValue(tile, out navMeshTile))
		{
			this.m_cachedTileID = tile;
			this.m_cachedTile = navMeshTile;
			return navMeshTile;
		}
		navMeshTile = new Pathfinding.NavMeshTile();
		navMeshTile.m_tile = tile;
		navMeshTile.m_center = this.GetTilePos(tile);
		this.m_tiles.Add(tile, navMeshTile);
		this.m_cachedTileID = tile;
		this.m_cachedTile = navMeshTile;
		return navMeshTile;
	}

	// Token: 0x06000B57 RID: 2903 RVA: 0x00051D28 File Offset: 0x0004FF28
	private Vector3Int GetTile(Vector3 point, Pathfinding.AgentType agent)
	{
		int x = Mathf.FloorToInt((point.x + this.m_tileSize / 2f) / this.m_tileSize);
		int y = Mathf.FloorToInt((point.z + this.m_tileSize / 2f) / this.m_tileSize);
		return new Vector3Int(x, y, (int)agent);
	}

	// Token: 0x06000B58 RID: 2904 RVA: 0x00051D7B File Offset: 0x0004FF7B
	public Vector3 GetTilePos(Vector3Int id)
	{
		return new Vector3((float)id.x * this.m_tileSize, 2500f, (float)id.y * this.m_tileSize);
	}

	// Token: 0x04000A95 RID: 2709
	private List<Vector3> tempPath = new List<Vector3>();

	// Token: 0x04000A96 RID: 2710
	private List<Vector3> optPath = new List<Vector3>();

	// Token: 0x04000A97 RID: 2711
	private List<Vector3> tempStitchPoints = new List<Vector3>();

	// Token: 0x04000A98 RID: 2712
	private RaycastHit[] tempHitArray = new RaycastHit[255];

	// Token: 0x04000A99 RID: 2713
	private static Pathfinding m_instance;

	// Token: 0x04000A9A RID: 2714
	public LayerMask m_layers;

	// Token: 0x04000A9B RID: 2715
	public LayerMask m_waterLayers;

	// Token: 0x04000A9C RID: 2716
	private Dictionary<Vector3Int, Pathfinding.NavMeshTile> m_tiles = new Dictionary<Vector3Int, Pathfinding.NavMeshTile>();

	// Token: 0x04000A9D RID: 2717
	public float m_tileSize = 32f;

	// Token: 0x04000A9E RID: 2718
	public float m_defaultCost = 1f;

	// Token: 0x04000A9F RID: 2719
	public float m_waterCost = 4f;

	// Token: 0x04000AA0 RID: 2720
	public float m_linkCost = 10f;

	// Token: 0x04000AA1 RID: 2721
	public float m_linkWidth = 1f;

	// Token: 0x04000AA2 RID: 2722
	public float m_updateInterval = 5f;

	// Token: 0x04000AA3 RID: 2723
	public float m_tileTimeout = 30f;

	// Token: 0x04000AA4 RID: 2724
	private const float m_tileHeight = 6000f;

	// Token: 0x04000AA5 RID: 2725
	private const float m_tileY = 2500f;

	// Token: 0x04000AA6 RID: 2726
	private float m_updatePathfindingTimer;

	// Token: 0x04000AA7 RID: 2727
	private Queue<Vector3Int> m_queuedAreas = new Queue<Vector3Int>();

	// Token: 0x04000AA8 RID: 2728
	private Queue<NavMeshLinkInstance> m_linkRemoveQueue = new Queue<NavMeshLinkInstance>();

	// Token: 0x04000AA9 RID: 2729
	private Queue<NavMeshDataInstance> m_tileRemoveQueue = new Queue<NavMeshDataInstance>();

	// Token: 0x04000AAA RID: 2730
	private Vector3Int m_cachedTileID = new Vector3Int(-9999999, -9999999, -9999999);

	// Token: 0x04000AAB RID: 2731
	private Pathfinding.NavMeshTile m_cachedTile;

	// Token: 0x04000AAC RID: 2732
	private List<Pathfinding.AgentSettings> m_agentSettings = new List<Pathfinding.AgentSettings>();

	// Token: 0x04000AAD RID: 2733
	private AsyncOperation m_buildOperation;

	// Token: 0x04000AAE RID: 2734
	private Pathfinding.NavMeshTile m_buildTile;

	// Token: 0x04000AAF RID: 2735
	private List<KeyValuePair<Pathfinding.NavMeshTile, Pathfinding.NavMeshTile>> m_edgeBuildQueue = new List<KeyValuePair<Pathfinding.NavMeshTile, Pathfinding.NavMeshTile>>();

	// Token: 0x04000AB0 RID: 2736
	private NavMeshPath m_path;

	// Token: 0x0200017D RID: 381
	private class NavMeshTile
	{
		// Token: 0x040011A6 RID: 4518
		public Vector3Int m_tile;

		// Token: 0x040011A7 RID: 4519
		public Vector3 m_center;

		// Token: 0x040011A8 RID: 4520
		public float m_pokeTime = -1000f;

		// Token: 0x040011A9 RID: 4521
		public float m_buildTime = -1000f;

		// Token: 0x040011AA RID: 4522
		public NavMeshData m_data;

		// Token: 0x040011AB RID: 4523
		public NavMeshDataInstance m_instance;

		// Token: 0x040011AC RID: 4524
		public List<KeyValuePair<Vector3, NavMeshLinkInstance>> m_links1 = new List<KeyValuePair<Vector3, NavMeshLinkInstance>>();

		// Token: 0x040011AD RID: 4525
		public List<KeyValuePair<Vector3, NavMeshLinkInstance>> m_links2 = new List<KeyValuePair<Vector3, NavMeshLinkInstance>>();
	}

	// Token: 0x0200017E RID: 382
	public enum AgentType
	{
		// Token: 0x040011AF RID: 4527
		Humanoid = 1,
		// Token: 0x040011B0 RID: 4528
		TrollSize,
		// Token: 0x040011B1 RID: 4529
		HugeSize,
		// Token: 0x040011B2 RID: 4530
		HorseSize,
		// Token: 0x040011B3 RID: 4531
		HumanoidNoSwim,
		// Token: 0x040011B4 RID: 4532
		HumanoidAvoidWater,
		// Token: 0x040011B5 RID: 4533
		Fish,
		// Token: 0x040011B6 RID: 4534
		Wolf,
		// Token: 0x040011B7 RID: 4535
		BigFish,
		// Token: 0x040011B8 RID: 4536
		GoblinBruteSize,
		// Token: 0x040011B9 RID: 4537
		HumanoidBigNoSwim
	}

	// Token: 0x0200017F RID: 383
	public enum AreaType
	{
		// Token: 0x040011BB RID: 4539
		Default,
		// Token: 0x040011BC RID: 4540
		NotWalkable,
		// Token: 0x040011BD RID: 4541
		Jump,
		// Token: 0x040011BE RID: 4542
		Water
	}

	// Token: 0x02000180 RID: 384
	private class AgentSettings
	{
		// Token: 0x0600117A RID: 4474 RVA: 0x00078AB4 File Offset: 0x00076CB4
		public AgentSettings(Pathfinding.AgentType type)
		{
			this.m_agentType = type;
			this.m_build = NavMesh.CreateSettings();
		}

		// Token: 0x040011BF RID: 4543
		public Pathfinding.AgentType m_agentType;

		// Token: 0x040011C0 RID: 4544
		public NavMeshBuildSettings m_build;

		// Token: 0x040011C1 RID: 4545
		public bool m_canWalk = true;

		// Token: 0x040011C2 RID: 4546
		public bool m_avoidWater;

		// Token: 0x040011C3 RID: 4547
		public bool m_canSwim = true;

		// Token: 0x040011C4 RID: 4548
		public float m_swimDepth;

		// Token: 0x040011C5 RID: 4549
		public int m_areaMask = -1;
	}
}
