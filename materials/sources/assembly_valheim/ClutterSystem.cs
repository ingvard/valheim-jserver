using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x020000BE RID: 190
public class ClutterSystem : MonoBehaviour
{
	// Token: 0x17000030 RID: 48
	// (get) Token: 0x06000C90 RID: 3216 RVA: 0x00059946 File Offset: 0x00057B46
	public static ClutterSystem instance
	{
		get
		{
			return ClutterSystem.m_instance;
		}
	}

	// Token: 0x06000C91 RID: 3217 RVA: 0x00059950 File Offset: 0x00057B50
	private void Awake()
	{
		ClutterSystem.m_instance = this;
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
		{
			return;
		}
		this.ApplySettings();
		this.m_placeRayMask = LayerMask.GetMask(new string[]
		{
			"terrain"
		});
		this.m_grassRoot = new GameObject("grassroot");
		this.m_grassRoot.transform.SetParent(base.transform);
	}

	// Token: 0x06000C92 RID: 3218 RVA: 0x000599B4 File Offset: 0x00057BB4
	public void ApplySettings()
	{
		ClutterSystem.Quality @int = (ClutterSystem.Quality)PlayerPrefs.GetInt("ClutterQuality", 2);
		if (this.m_quality == @int)
		{
			return;
		}
		this.m_quality = @int;
		this.ClearAll();
	}

	// Token: 0x06000C93 RID: 3219 RVA: 0x000599E4 File Offset: 0x00057BE4
	private void LateUpdate()
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		Vector3 center = (!GameCamera.InFreeFly() && Player.m_localPlayer) ? Player.m_localPlayer.transform.position : mainCamera.transform.position;
		if (this.m_forceRebuild)
		{
			if (this.IsHeightmapReady())
			{
				this.m_forceRebuild = false;
				this.UpdateGrass(Time.deltaTime, true, center);
			}
		}
		else if (this.IsHeightmapReady())
		{
			this.UpdateGrass(Time.deltaTime, false, center);
		}
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer != null)
		{
			this.m_oldPlayerPos = Vector3.Lerp(this.m_oldPlayerPos, localPlayer.transform.position, this.m_playerPushFade);
			Shader.SetGlobalVector("_PlayerPosition", localPlayer.transform.position);
			Shader.SetGlobalVector("_PlayerOldPosition", this.m_oldPlayerPos);
			return;
		}
		Shader.SetGlobalVector("_PlayerPosition", new Vector3(999999f, 999999f, 999999f));
		Shader.SetGlobalVector("_PlayerOldPosition", new Vector3(999999f, 999999f, 999999f));
	}

	// Token: 0x06000C94 RID: 3220 RVA: 0x00059B14 File Offset: 0x00057D14
	public Vector2Int GetVegPatch(Vector3 point)
	{
		int x = Mathf.FloorToInt((point.x + this.m_grassPatchSize / 2f) / this.m_grassPatchSize);
		int y = Mathf.FloorToInt((point.z + this.m_grassPatchSize / 2f) / this.m_grassPatchSize);
		return new Vector2Int(x, y);
	}

	// Token: 0x06000C95 RID: 3221 RVA: 0x00059B66 File Offset: 0x00057D66
	public Vector3 GetVegPatchCenter(Vector2Int p)
	{
		return new Vector3((float)p.x * this.m_grassPatchSize, 0f, (float)p.y * this.m_grassPatchSize);
	}

	// Token: 0x06000C96 RID: 3222 RVA: 0x00059B90 File Offset: 0x00057D90
	private bool IsHeightmapReady()
	{
		Camera mainCamera = Utils.GetMainCamera();
		return mainCamera && !Heightmap.HaveQueuedRebuild(mainCamera.transform.position, this.m_distance);
	}

	// Token: 0x06000C97 RID: 3223 RVA: 0x00059BC8 File Offset: 0x00057DC8
	private void UpdateGrass(float dt, bool rebuildAll, Vector3 center)
	{
		if (this.m_quality == ClutterSystem.Quality.Off)
		{
			return;
		}
		this.GeneratePatches(rebuildAll, center);
		this.TimeoutPatches(dt);
	}

	// Token: 0x06000C98 RID: 3224 RVA: 0x00059BE4 File Offset: 0x00057DE4
	private void GeneratePatches(bool rebuildAll, Vector3 center)
	{
		bool flag = false;
		Vector2Int vegPatch = this.GetVegPatch(center);
		this.GeneratePatch(center, vegPatch, ref flag, rebuildAll);
		int num = Mathf.CeilToInt((this.m_distance - this.m_grassPatchSize / 2f) / this.m_grassPatchSize);
		for (int i = 1; i <= num; i++)
		{
			for (int j = vegPatch.x - i; j <= vegPatch.x + i; j++)
			{
				this.GeneratePatch(center, new Vector2Int(j, vegPatch.y - i), ref flag, rebuildAll);
				this.GeneratePatch(center, new Vector2Int(j, vegPatch.y + i), ref flag, rebuildAll);
			}
			for (int k = vegPatch.y - i + 1; k <= vegPatch.y + i - 1; k++)
			{
				this.GeneratePatch(center, new Vector2Int(vegPatch.x - i, k), ref flag, rebuildAll);
				this.GeneratePatch(center, new Vector2Int(vegPatch.x + i, k), ref flag, rebuildAll);
			}
		}
	}

	// Token: 0x06000C99 RID: 3225 RVA: 0x00059CE4 File Offset: 0x00057EE4
	private void GeneratePatch(Vector3 camPos, Vector2Int p, ref bool generated, bool rebuildAll)
	{
		if (Utils.DistanceXZ(this.GetVegPatchCenter(p), camPos) > this.m_distance)
		{
			return;
		}
		ClutterSystem.PatchData patchData;
		if (this.m_patches.TryGetValue(p, out patchData) && !patchData.m_reset)
		{
			patchData.m_timer = 0f;
			return;
		}
		if (rebuildAll || !generated || this.m_menuHack)
		{
			ClutterSystem.PatchData patchData2 = this.GenerateVegPatch(p, this.m_grassPatchSize);
			if (patchData2 != null)
			{
				ClutterSystem.PatchData patchData3;
				if (this.m_patches.TryGetValue(p, out patchData3))
				{
					foreach (GameObject obj in patchData3.m_objects)
					{
						UnityEngine.Object.Destroy(obj);
					}
					this.FreePatch(patchData3);
					this.m_patches.Remove(p);
				}
				this.m_patches.Add(p, patchData2);
				generated = true;
			}
		}
	}

	// Token: 0x06000C9A RID: 3226 RVA: 0x00059DC4 File Offset: 0x00057FC4
	private void TimeoutPatches(float dt)
	{
		this.m_tempToRemovePair.Clear();
		foreach (KeyValuePair<Vector2Int, ClutterSystem.PatchData> item in this.m_patches)
		{
			item.Value.m_timer += dt;
			if (item.Value.m_timer >= 2f)
			{
				this.m_tempToRemovePair.Add(item);
			}
		}
		foreach (KeyValuePair<Vector2Int, ClutterSystem.PatchData> keyValuePair in this.m_tempToRemovePair)
		{
			foreach (GameObject obj in keyValuePair.Value.m_objects)
			{
				UnityEngine.Object.Destroy(obj);
			}
			this.m_patches.Remove(keyValuePair.Key);
			this.FreePatch(keyValuePair.Value);
		}
	}

	// Token: 0x06000C9B RID: 3227 RVA: 0x00059EF0 File Offset: 0x000580F0
	private void ClearAll()
	{
		foreach (KeyValuePair<Vector2Int, ClutterSystem.PatchData> keyValuePair in this.m_patches)
		{
			foreach (GameObject obj in keyValuePair.Value.m_objects)
			{
				UnityEngine.Object.Destroy(obj);
			}
			this.FreePatch(keyValuePair.Value);
		}
		this.m_patches.Clear();
		this.m_forceRebuild = true;
	}

	// Token: 0x06000C9C RID: 3228 RVA: 0x00059FA0 File Offset: 0x000581A0
	public void ResetGrass(Vector3 center, float radius)
	{
		float num = this.m_grassPatchSize / 2f;
		foreach (KeyValuePair<Vector2Int, ClutterSystem.PatchData> keyValuePair in this.m_patches)
		{
			Vector3 center2 = keyValuePair.Value.center;
			if (center2.x + num >= center.x - radius && center2.x - num <= center.x + radius && center2.z + num >= center.z - radius && center2.z - num <= center.z + radius)
			{
				keyValuePair.Value.m_reset = true;
				this.m_forceRebuild = true;
			}
		}
	}

	// Token: 0x06000C9D RID: 3229 RVA: 0x0005A064 File Offset: 0x00058264
	public bool GetGroundInfo(Vector3 p, out Vector3 point, out Vector3 normal, out Heightmap hmap, out Heightmap.Biome biome)
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(p + Vector3.up * 500f, Vector3.down, out raycastHit, 1000f, this.m_placeRayMask))
		{
			point = raycastHit.point;
			normal = raycastHit.normal;
			hmap = raycastHit.collider.GetComponent<Heightmap>();
			biome = hmap.GetBiome(point);
			return true;
		}
		point = p;
		normal = Vector3.up;
		hmap = null;
		biome = Heightmap.Biome.Meadows;
		return false;
	}

	// Token: 0x06000C9E RID: 3230 RVA: 0x0005A0F8 File Offset: 0x000582F8
	private Heightmap.Biome GetPatchBiomes(Vector3 center, float halfSize)
	{
		Heightmap.Biome biome = Heightmap.FindBiomeClutter(new Vector3(center.x - halfSize, 0f, center.z - halfSize));
		Heightmap.Biome biome2 = Heightmap.FindBiomeClutter(new Vector3(center.x + halfSize, 0f, center.z - halfSize));
		Heightmap.Biome biome3 = Heightmap.FindBiomeClutter(new Vector3(center.x - halfSize, 0f, center.z + halfSize));
		Heightmap.Biome biome4 = Heightmap.FindBiomeClutter(new Vector3(center.x + halfSize, 0f, center.z + halfSize));
		if (biome == Heightmap.Biome.None || biome2 == Heightmap.Biome.None || biome3 == Heightmap.Biome.None || biome4 == Heightmap.Biome.None)
		{
			return Heightmap.Biome.None;
		}
		return biome | biome2 | biome3 | biome4;
	}

	// Token: 0x06000C9F RID: 3231 RVA: 0x0005A19C File Offset: 0x0005839C
	private ClutterSystem.PatchData GenerateVegPatch(Vector2Int patchID, float size)
	{
		Vector3 vegPatchCenter = this.GetVegPatchCenter(patchID);
		float num = size / 2f;
		Heightmap.Biome patchBiomes = this.GetPatchBiomes(vegPatchCenter, num);
		if (patchBiomes == Heightmap.Biome.None)
		{
			return null;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		ClutterSystem.PatchData patchData = this.AllocatePatch();
		patchData.center = vegPatchCenter;
		for (int i = 0; i < this.m_clutter.Count; i++)
		{
			ClutterSystem.Clutter clutter = this.m_clutter[i];
			if (clutter.m_enabled && (patchBiomes & clutter.m_biome) != Heightmap.Biome.None)
			{
				InstanceRenderer instanceRenderer = null;
				UnityEngine.Random.InitState(patchID.x * (patchID.y * 1374) + i * 9321);
				Vector3 b = new Vector3(clutter.m_fractalOffset, 0f, 0f);
				float num2 = Mathf.Cos(0.017453292f * clutter.m_maxTilt);
				int num3 = (this.m_quality == ClutterSystem.Quality.High) ? clutter.m_amount : (clutter.m_amount / 2);
				num3 = (int)((float)num3 * this.m_amountScale);
				int j = 0;
				while (j < num3)
				{
					Vector3 vector = new Vector3(UnityEngine.Random.Range(vegPatchCenter.x - num, vegPatchCenter.x + num), 0f, UnityEngine.Random.Range(vegPatchCenter.z - num, vegPatchCenter.z + num));
					float num4 = (float)UnityEngine.Random.Range(0, 360);
					if (!clutter.m_inForest)
					{
						goto IL_161;
					}
					float forestFactor = WorldGenerator.GetForestFactor(vector);
					if (forestFactor >= clutter.m_forestTresholdMin && forestFactor <= clutter.m_forestTresholdMax)
					{
						goto IL_161;
					}
					IL_3D5:
					j++;
					continue;
					IL_161:
					if (clutter.m_fractalScale > 0f)
					{
						float num5 = Utils.Fbm(vector * 0.01f * clutter.m_fractalScale + b, 3, 1.6f, 0.7f);
						if (num5 < clutter.m_fractalTresholdMin || num5 > clutter.m_fractalTresholdMax)
						{
							goto IL_3D5;
						}
					}
					Vector3 vector2;
					Vector3 vector3;
					Heightmap heightmap;
					Heightmap.Biome biome;
					if (!this.GetGroundInfo(vector, out vector2, out vector3, out heightmap, out biome) || (clutter.m_biome & biome) == Heightmap.Biome.None)
					{
						goto IL_3D5;
					}
					float num6 = vector2.y - this.m_waterLevel;
					if (num6 < clutter.m_minAlt || num6 > clutter.m_maxAlt || vector3.y < num2)
					{
						goto IL_3D5;
					}
					if (clutter.m_minOceanDepth != clutter.m_maxOceanDepth)
					{
						float oceanDepth = heightmap.GetOceanDepth(vector);
						if (oceanDepth < clutter.m_minOceanDepth || oceanDepth > clutter.m_maxOceanDepth)
						{
							goto IL_3D5;
						}
					}
					if (!clutter.m_onCleared || !clutter.m_onUncleared)
					{
						bool flag = heightmap.IsCleared(vector2);
						if ((clutter.m_onCleared && !flag) || (clutter.m_onUncleared && flag))
						{
							goto IL_3D5;
						}
					}
					vector = vector2;
					if (clutter.m_snapToWater)
					{
						vector.y = this.m_waterLevel;
					}
					if (clutter.m_randomOffset != 0f)
					{
						vector.y += UnityEngine.Random.Range(-clutter.m_randomOffset, clutter.m_randomOffset);
					}
					Quaternion quaternion = Quaternion.identity;
					if (clutter.m_terrainTilt)
					{
						quaternion = Quaternion.AngleAxis(num4, vector3);
					}
					else
					{
						quaternion = Quaternion.Euler(0f, num4, 0f);
					}
					if (clutter.m_instanced)
					{
						if (instanceRenderer == null)
						{
							GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(clutter.m_prefab, vegPatchCenter, Quaternion.identity, this.m_grassRoot.transform);
							instanceRenderer = gameObject.GetComponent<InstanceRenderer>();
							if (instanceRenderer.m_lodMaxDistance > this.m_distance - this.m_grassPatchSize / 2f)
							{
								instanceRenderer.m_lodMaxDistance = this.m_distance - this.m_grassPatchSize / 2f;
							}
							patchData.m_objects.Add(gameObject);
						}
						float scale = UnityEngine.Random.Range(clutter.m_scaleMin, clutter.m_scaleMax);
						instanceRenderer.AddInstance(vector, quaternion, scale);
						goto IL_3D5;
					}
					GameObject item = UnityEngine.Object.Instantiate<GameObject>(clutter.m_prefab, vector, quaternion, this.m_grassRoot.transform);
					patchData.m_objects.Add(item);
					goto IL_3D5;
				}
			}
		}
		UnityEngine.Random.state = state;
		return patchData;
	}

	// Token: 0x06000CA0 RID: 3232 RVA: 0x0005A5AD File Offset: 0x000587AD
	private ClutterSystem.PatchData AllocatePatch()
	{
		if (this.m_freePatches.Count > 0)
		{
			return this.m_freePatches.Pop();
		}
		return new ClutterSystem.PatchData();
	}

	// Token: 0x06000CA1 RID: 3233 RVA: 0x0005A5CE File Offset: 0x000587CE
	private void FreePatch(ClutterSystem.PatchData patch)
	{
		patch.center = Vector3.zero;
		patch.m_objects.Clear();
		patch.m_timer = 0f;
		patch.m_reset = false;
		this.m_freePatches.Push(patch);
	}

	// Token: 0x04000B77 RID: 2935
	private static ClutterSystem m_instance;

	// Token: 0x04000B78 RID: 2936
	private int m_placeRayMask;

	// Token: 0x04000B79 RID: 2937
	public List<ClutterSystem.Clutter> m_clutter = new List<ClutterSystem.Clutter>();

	// Token: 0x04000B7A RID: 2938
	public float m_grassPatchSize = 8f;

	// Token: 0x04000B7B RID: 2939
	public float m_distance = 40f;

	// Token: 0x04000B7C RID: 2940
	public float m_waterLevel = 27f;

	// Token: 0x04000B7D RID: 2941
	public float m_playerPushFade = 0.05f;

	// Token: 0x04000B7E RID: 2942
	public float m_amountScale = 1f;

	// Token: 0x04000B7F RID: 2943
	public bool m_menuHack;

	// Token: 0x04000B80 RID: 2944
	private Dictionary<Vector2Int, ClutterSystem.PatchData> m_patches = new Dictionary<Vector2Int, ClutterSystem.PatchData>();

	// Token: 0x04000B81 RID: 2945
	private Stack<ClutterSystem.PatchData> m_freePatches = new Stack<ClutterSystem.PatchData>();

	// Token: 0x04000B82 RID: 2946
	private GameObject m_grassRoot;

	// Token: 0x04000B83 RID: 2947
	private Vector3 m_oldPlayerPos = Vector3.zero;

	// Token: 0x04000B84 RID: 2948
	private List<Vector2Int> m_tempToRemove = new List<Vector2Int>();

	// Token: 0x04000B85 RID: 2949
	private List<KeyValuePair<Vector2Int, ClutterSystem.PatchData>> m_tempToRemovePair = new List<KeyValuePair<Vector2Int, ClutterSystem.PatchData>>();

	// Token: 0x04000B86 RID: 2950
	private ClutterSystem.Quality m_quality = ClutterSystem.Quality.High;

	// Token: 0x04000B87 RID: 2951
	private bool m_forceRebuild;

	// Token: 0x0200018F RID: 399
	[Serializable]
	public class Clutter
	{
		// Token: 0x04001251 RID: 4689
		public string m_name = "";

		// Token: 0x04001252 RID: 4690
		public bool m_enabled = true;

		// Token: 0x04001253 RID: 4691
		[BitMask(typeof(Heightmap.Biome))]
		public Heightmap.Biome m_biome;

		// Token: 0x04001254 RID: 4692
		public bool m_instanced;

		// Token: 0x04001255 RID: 4693
		public GameObject m_prefab;

		// Token: 0x04001256 RID: 4694
		public int m_amount = 80;

		// Token: 0x04001257 RID: 4695
		public bool m_onUncleared = true;

		// Token: 0x04001258 RID: 4696
		public bool m_onCleared;

		// Token: 0x04001259 RID: 4697
		public float m_scaleMin = 1f;

		// Token: 0x0400125A RID: 4698
		public float m_scaleMax = 1f;

		// Token: 0x0400125B RID: 4699
		public float m_maxTilt = 18f;

		// Token: 0x0400125C RID: 4700
		public float m_maxAlt = 1000f;

		// Token: 0x0400125D RID: 4701
		public float m_minAlt = 27f;

		// Token: 0x0400125E RID: 4702
		public bool m_snapToWater;

		// Token: 0x0400125F RID: 4703
		public bool m_terrainTilt;

		// Token: 0x04001260 RID: 4704
		public float m_randomOffset;

		// Token: 0x04001261 RID: 4705
		[Header("Ocean depth ")]
		public float m_minOceanDepth;

		// Token: 0x04001262 RID: 4706
		public float m_maxOceanDepth;

		// Token: 0x04001263 RID: 4707
		[Header("Forest fractal 0-1 inside forest")]
		public bool m_inForest;

		// Token: 0x04001264 RID: 4708
		public float m_forestTresholdMin;

		// Token: 0x04001265 RID: 4709
		public float m_forestTresholdMax = 1f;

		// Token: 0x04001266 RID: 4710
		[Header("Fractal placement (m_fractalScale > 0 == enabled) ")]
		public float m_fractalScale;

		// Token: 0x04001267 RID: 4711
		public float m_fractalOffset;

		// Token: 0x04001268 RID: 4712
		public float m_fractalTresholdMin = 0.5f;

		// Token: 0x04001269 RID: 4713
		public float m_fractalTresholdMax = 1f;
	}

	// Token: 0x02000190 RID: 400
	private class PatchData
	{
		// Token: 0x0400126A RID: 4714
		public Vector3 center;

		// Token: 0x0400126B RID: 4715
		public List<GameObject> m_objects = new List<GameObject>();

		// Token: 0x0400126C RID: 4716
		public float m_timer;

		// Token: 0x0400126D RID: 4717
		public bool m_reset;
	}

	// Token: 0x02000191 RID: 401
	public enum Quality
	{
		// Token: 0x0400126F RID: 4719
		Off,
		// Token: 0x04001270 RID: 4720
		Med,
		// Token: 0x04001271 RID: 4721
		High
	}
}
