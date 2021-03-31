using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000D1 RID: 209
[ExecuteInEditMode]
public class Heightmap : MonoBehaviour
{
	// Token: 0x06000D6D RID: 3437 RVA: 0x0005F990 File Offset: 0x0005DB90
	private void Awake()
	{
		if (!this.m_isDistantLod)
		{
			Heightmap.m_heightmaps.Add(this);
		}
		this.m_collider = base.GetComponent<MeshCollider>();
	}

	// Token: 0x06000D6E RID: 3438 RVA: 0x0005F9B1 File Offset: 0x0005DBB1
	private void OnDestroy()
	{
		if (!this.m_isDistantLod)
		{
			Heightmap.m_heightmaps.Remove(this);
		}
		if (this.m_materialInstance)
		{
			UnityEngine.Object.DestroyImmediate(this.m_materialInstance);
		}
	}

	// Token: 0x06000D6F RID: 3439 RVA: 0x0005F9DF File Offset: 0x0005DBDF
	private void OnEnable()
	{
		if (this.m_isDistantLod && Application.isPlaying && !this.m_distantLodEditorHax)
		{
			return;
		}
		this.Regenerate();
	}

	// Token: 0x06000D70 RID: 3440 RVA: 0x0005F9FF File Offset: 0x0005DBFF
	private void Update()
	{
		this.Render();
	}

	// Token: 0x06000D71 RID: 3441 RVA: 0x0005FA08 File Offset: 0x0005DC08
	private void Render()
	{
		if (!this.IsVisible())
		{
			return;
		}
		if (this.m_dirty)
		{
			this.m_dirty = false;
			this.m_materialInstance.SetTexture("_ClearedMaskTex", this.m_clearedMask);
			this.RebuildRenderMesh();
		}
		if (this.m_renderMesh)
		{
			Matrix4x4 matrix = Matrix4x4.TRS(base.transform.position, Quaternion.identity, Vector3.one);
			Graphics.DrawMesh(this.m_renderMesh, matrix, this.m_materialInstance, base.gameObject.layer);
		}
	}

	// Token: 0x06000D72 RID: 3442 RVA: 0x0005FA8E File Offset: 0x0005DC8E
	private bool IsVisible()
	{
		return Utils.InsideMainCamera(this.m_boundingSphere) && Utils.InsideMainCamera(this.m_bounds);
	}

	// Token: 0x06000D73 RID: 3443 RVA: 0x0005FAB0 File Offset: 0x0005DCB0
	public static void ForceGenerateAll()
	{
		foreach (Heightmap heightmap in Heightmap.m_heightmaps)
		{
			if (heightmap.HaveQueuedRebuild())
			{
				ZLog.Log("Force generaeting hmap " + heightmap.transform.position);
				heightmap.Regenerate();
			}
		}
	}

	// Token: 0x06000D74 RID: 3444 RVA: 0x0005FB28 File Offset: 0x0005DD28
	public void Poke(bool delayed)
	{
		if (delayed)
		{
			if (this.HaveQueuedRebuild())
			{
				base.CancelInvoke("Regenerate");
			}
			base.InvokeRepeating("Regenerate", 0.1f, 0f);
			return;
		}
		this.Regenerate();
	}

	// Token: 0x06000D75 RID: 3445 RVA: 0x0005FB5C File Offset: 0x0005DD5C
	public bool HaveQueuedRebuild()
	{
		return base.IsInvoking("Regenerate");
	}

	// Token: 0x06000D76 RID: 3446 RVA: 0x0005FB69 File Offset: 0x0005DD69
	public void Regenerate()
	{
		if (this.HaveQueuedRebuild())
		{
			base.CancelInvoke("Regenerate");
		}
		this.Generate();
		this.RebuildCollisionMesh();
		this.UpdateCornerDepths();
		this.m_dirty = true;
	}

	// Token: 0x06000D77 RID: 3447 RVA: 0x0005FB98 File Offset: 0x0005DD98
	private void UpdateCornerDepths()
	{
		float num = ZoneSystem.instance ? ZoneSystem.instance.m_waterLevel : 30f;
		this.m_oceanDepth[0] = this.GetHeight(0, this.m_width);
		this.m_oceanDepth[1] = this.GetHeight(this.m_width, this.m_width);
		this.m_oceanDepth[2] = this.GetHeight(this.m_width, 0);
		this.m_oceanDepth[3] = this.GetHeight(0, 0);
		this.m_oceanDepth[0] = Mathf.Max(0f, num - this.m_oceanDepth[0]);
		this.m_oceanDepth[1] = Mathf.Max(0f, num - this.m_oceanDepth[1]);
		this.m_oceanDepth[2] = Mathf.Max(0f, num - this.m_oceanDepth[2]);
		this.m_oceanDepth[3] = Mathf.Max(0f, num - this.m_oceanDepth[3]);
		this.m_materialInstance.SetFloatArray("_depth", this.m_oceanDepth);
	}

	// Token: 0x06000D78 RID: 3448 RVA: 0x0005FC9D File Offset: 0x0005DE9D
	public float[] GetOceanDepth()
	{
		return this.m_oceanDepth;
	}

	// Token: 0x06000D79 RID: 3449 RVA: 0x0005FCA8 File Offset: 0x0005DEA8
	public static float GetOceanDepthAll(Vector3 worldPos)
	{
		Heightmap heightmap = Heightmap.FindHeightmap(worldPos);
		if (heightmap)
		{
			return heightmap.GetOceanDepth(worldPos);
		}
		return 0f;
	}

	// Token: 0x06000D7A RID: 3450 RVA: 0x0005FCD4 File Offset: 0x0005DED4
	public float GetOceanDepth(Vector3 worldPos)
	{
		int num;
		int num2;
		this.WorldToVertex(worldPos, out num, out num2);
		float t = (float)num / (float)this.m_width;
		float t2 = (float)num2 / (float)this.m_width;
		float a = Mathf.Lerp(this.m_oceanDepth[3], this.m_oceanDepth[2], t);
		float b = Mathf.Lerp(this.m_oceanDepth[0], this.m_oceanDepth[1], t);
		return Mathf.Lerp(a, b, t2);
	}

	// Token: 0x06000D7B RID: 3451 RVA: 0x0005FD38 File Offset: 0x0005DF38
	private void Initialize()
	{
		int num = this.m_width + 1;
		int num2 = num * num;
		if (this.m_heights.Count != num2)
		{
			this.m_heights.Clear();
			for (int i = 0; i < num2; i++)
			{
				this.m_heights.Add(0f);
			}
			this.m_clearedMask = new Texture2D(this.m_width, this.m_width);
			this.m_clearedMask.wrapMode = TextureWrapMode.Clamp;
			this.m_materialInstance = new Material(this.m_material);
			this.m_materialInstance.SetTexture("_ClearedMaskTex", this.m_clearedMask);
		}
	}

	// Token: 0x06000D7C RID: 3452 RVA: 0x0005FDD0 File Offset: 0x0005DFD0
	private void Generate()
	{
		this.Initialize();
		int num = this.m_width + 1;
		int num2 = num * num;
		Vector3 position = base.transform.position;
		if (this.m_buildData == null || this.m_buildData.m_baseHeights.Count != num2 || this.m_buildData.m_center != position || this.m_buildData.m_scale != this.m_scale || this.m_buildData.m_worldGen != WorldGenerator.instance)
		{
			this.m_buildData = HeightmapBuilder.instance.RequestTerrainSync(position, this.m_width, this.m_scale, this.m_isDistantLod, WorldGenerator.instance);
			this.m_cornerBiomes = this.m_buildData.m_cornerBiomes;
		}
		for (int i = 0; i < num2; i++)
		{
			this.m_heights[i] = this.m_buildData.m_baseHeights[i];
		}
		Color[] pixels = new Color[this.m_clearedMask.width * this.m_clearedMask.height];
		this.m_clearedMask.SetPixels(pixels);
		this.ApplyModifiers();
	}

	// Token: 0x06000D7D RID: 3453 RVA: 0x0005FEE0 File Offset: 0x0005E0E0
	private float Distance(float x, float y, float rx, float ry)
	{
		float num = x - rx;
		float num2 = y - ry;
		float num3 = Mathf.Sqrt(num * num + num2 * num2);
		float num4 = 1.414f - num3;
		return num4 * num4 * num4;
	}

	// Token: 0x06000D7E RID: 3454 RVA: 0x0005FF10 File Offset: 0x0005E110
	public List<Heightmap.Biome> GetBiomes()
	{
		List<Heightmap.Biome> list = new List<Heightmap.Biome>();
		foreach (Heightmap.Biome item in this.m_cornerBiomes)
		{
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		return list;
	}

	// Token: 0x06000D7F RID: 3455 RVA: 0x0005FF4D File Offset: 0x0005E14D
	public bool HaveBiome(Heightmap.Biome biome)
	{
		return (this.m_cornerBiomes[0] & biome) != Heightmap.Biome.None || (this.m_cornerBiomes[1] & biome) != Heightmap.Biome.None || (this.m_cornerBiomes[2] & biome) != Heightmap.Biome.None || (this.m_cornerBiomes[3] & biome) > Heightmap.Biome.None;
	}

	// Token: 0x06000D80 RID: 3456 RVA: 0x0005FF84 File Offset: 0x0005E184
	public Heightmap.Biome GetBiome(Vector3 point)
	{
		if (this.m_isDistantLod)
		{
			return WorldGenerator.instance.GetBiome(point.x, point.z);
		}
		if (this.m_cornerBiomes[0] == this.m_cornerBiomes[1] && this.m_cornerBiomes[0] == this.m_cornerBiomes[2] && this.m_cornerBiomes[0] == this.m_cornerBiomes[3])
		{
			return this.m_cornerBiomes[0];
		}
		float x = point.x;
		float z = point.z;
		this.WorldToNormalizedHM(point, out x, out z);
		for (int i = 1; i < Heightmap.tempBiomeWeights.Length; i++)
		{
			Heightmap.tempBiomeWeights[i] = 0f;
		}
		Heightmap.tempBiomeWeights[(int)this.m_cornerBiomes[0]] += this.Distance(x, z, 0f, 0f);
		Heightmap.tempBiomeWeights[(int)this.m_cornerBiomes[1]] += this.Distance(x, z, 1f, 0f);
		Heightmap.tempBiomeWeights[(int)this.m_cornerBiomes[2]] += this.Distance(x, z, 0f, 1f);
		Heightmap.tempBiomeWeights[(int)this.m_cornerBiomes[3]] += this.Distance(x, z, 1f, 1f);
		int result = 0;
		float num = -99999f;
		for (int j = 1; j < Heightmap.tempBiomeWeights.Length; j++)
		{
			if (Heightmap.tempBiomeWeights[j] > num)
			{
				result = j;
				num = Heightmap.tempBiomeWeights[j];
			}
		}
		return (Heightmap.Biome)result;
	}

	// Token: 0x06000D81 RID: 3457 RVA: 0x00060101 File Offset: 0x0005E301
	public Heightmap.BiomeArea GetBiomeArea()
	{
		if (this.IsBiomeEdge())
		{
			return Heightmap.BiomeArea.Edge;
		}
		return Heightmap.BiomeArea.Median;
	}

	// Token: 0x06000D82 RID: 3458 RVA: 0x0006010E File Offset: 0x0005E30E
	public bool IsBiomeEdge()
	{
		return this.m_cornerBiomes[0] != this.m_cornerBiomes[1] || this.m_cornerBiomes[0] != this.m_cornerBiomes[2] || this.m_cornerBiomes[0] != this.m_cornerBiomes[3];
	}

	// Token: 0x06000D83 RID: 3459 RVA: 0x0006014C File Offset: 0x0005E34C
	private void ApplyModifiers()
	{
		List<TerrainModifier> allInstances = TerrainModifier.GetAllInstances();
		float[] array = null;
		float[] levelOnly = null;
		foreach (TerrainModifier terrainModifier in allInstances)
		{
			if (terrainModifier.enabled && this.TerrainVSModifier(terrainModifier))
			{
				if (terrainModifier.m_playerModifiction && array == null)
				{
					array = this.m_heights.ToArray();
					levelOnly = this.m_heights.ToArray();
				}
				this.ApplyModifier(terrainModifier, array, levelOnly);
			}
		}
		this.m_clearedMask.Apply();
	}

	// Token: 0x06000D84 RID: 3460 RVA: 0x000601E4 File Offset: 0x0005E3E4
	private void ApplyModifier(TerrainModifier modifier, float[] baseHeights, float[] levelOnly)
	{
		if (modifier.m_level)
		{
			this.LevelTerrain(modifier.transform.position + Vector3.up * modifier.m_levelOffset, modifier.m_levelRadius, modifier.m_square, baseHeights, levelOnly, modifier.m_playerModifiction);
		}
		if (modifier.m_smooth)
		{
			this.SmoothTerrain2(modifier.transform.position + Vector3.up * modifier.m_levelOffset, modifier.m_smoothRadius, modifier.m_square, levelOnly, modifier.m_smoothPower, modifier.m_playerModifiction);
		}
		if (modifier.m_paintCleared)
		{
			this.PaintCleared(modifier.transform.position, modifier.m_paintRadius, modifier.m_paintType, modifier.m_paintHeightCheck, false);
		}
	}

	// Token: 0x06000D85 RID: 3461 RVA: 0x000602A8 File Offset: 0x0005E4A8
	public bool TerrainVSModifier(TerrainModifier modifier)
	{
		Vector3 position = modifier.transform.position;
		float num = modifier.GetRadius() + 4f;
		Vector3 position2 = base.transform.position;
		float num2 = (float)this.m_width * this.m_scale * 0.5f;
		return position.x + num >= position2.x - num2 && position.x - num <= position2.x + num2 && position.z + num >= position2.z - num2 && position.z - num <= position2.z + num2;
	}

	// Token: 0x06000D86 RID: 3462 RVA: 0x00060340 File Offset: 0x0005E540
	private Vector3 CalcNormal2(List<Vector3> vertises, int x, int y)
	{
		int num = this.m_width + 1;
		Vector3 vector = vertises[y * num + x];
		Vector3 rhs;
		if (x == this.m_width)
		{
			Vector3 b = vertises[y * num + x - 1];
			rhs = vector - b;
		}
		else if (x == 0)
		{
			rhs = vertises[y * num + x + 1] - vector;
		}
		else
		{
			rhs = vertises[y * num + x + 1] - vertises[y * num + x - 1];
		}
		Vector3 lhs;
		if (y == this.m_width)
		{
			Vector3 b2 = this.CalcVertex(x, y - 1);
			lhs = vector - b2;
		}
		else if (y == 0)
		{
			lhs = this.CalcVertex(x, y + 1) - vector;
		}
		else
		{
			lhs = vertises[(y + 1) * num + x] - vertises[(y - 1) * num + x];
		}
		Vector3 result = Vector3.Cross(lhs, rhs);
		result.Normalize();
		return result;
	}

	// Token: 0x06000D87 RID: 3463 RVA: 0x00060428 File Offset: 0x0005E628
	private Vector3 CalcNormal(int x, int y)
	{
		Vector3 vector = this.CalcVertex(x, y);
		Vector3 rhs;
		if (x == this.m_width)
		{
			Vector3 b = this.CalcVertex(x - 1, y);
			rhs = vector - b;
		}
		else
		{
			rhs = this.CalcVertex(x + 1, y) - vector;
		}
		Vector3 lhs;
		if (y == this.m_width)
		{
			Vector3 b2 = this.CalcVertex(x, y - 1);
			lhs = vector - b2;
		}
		else
		{
			lhs = this.CalcVertex(x, y + 1) - vector;
		}
		return Vector3.Cross(lhs, rhs).normalized;
	}

	// Token: 0x06000D88 RID: 3464 RVA: 0x000604B0 File Offset: 0x0005E6B0
	private Vector3 CalcVertex(int x, int y)
	{
		int num = this.m_width + 1;
		Vector3 a = new Vector3((float)this.m_width * this.m_scale * -0.5f, 0f, (float)this.m_width * this.m_scale * -0.5f);
		float y2 = this.m_heights[y * num + x];
		return a + new Vector3((float)x * this.m_scale, y2, (float)y * this.m_scale);
	}

	// Token: 0x06000D89 RID: 3465 RVA: 0x00060528 File Offset: 0x0005E728
	private Color GetBiomeColor(float ix, float iy)
	{
		if (this.m_cornerBiomes[0] == this.m_cornerBiomes[1] && this.m_cornerBiomes[0] == this.m_cornerBiomes[2] && this.m_cornerBiomes[0] == this.m_cornerBiomes[3])
		{
			return Heightmap.GetBiomeColor(this.m_cornerBiomes[0]);
		}
		Color32 biomeColor = Heightmap.GetBiomeColor(this.m_cornerBiomes[0]);
		Color32 biomeColor2 = Heightmap.GetBiomeColor(this.m_cornerBiomes[1]);
		Color32 biomeColor3 = Heightmap.GetBiomeColor(this.m_cornerBiomes[2]);
		Color32 biomeColor4 = Heightmap.GetBiomeColor(this.m_cornerBiomes[3]);
		Color32 a = Color32.Lerp(biomeColor, biomeColor2, ix);
		Color32 b = Color32.Lerp(biomeColor3, biomeColor4, ix);
		return Color32.Lerp(a, b, iy);
	}

	// Token: 0x06000D8A RID: 3466 RVA: 0x000605D4 File Offset: 0x0005E7D4
	public static Color32 GetBiomeColor(Heightmap.Biome biome)
	{
		if (biome <= Heightmap.Biome.Plains)
		{
			switch (biome)
			{
			case Heightmap.Biome.Meadows:
			case (Heightmap.Biome)3:
				break;
			case Heightmap.Biome.Swamp:
				return new Color32(byte.MaxValue, 0, 0, 0);
			case Heightmap.Biome.Mountain:
				return new Color32(0, byte.MaxValue, 0, 0);
			default:
				if (biome == Heightmap.Biome.BlackForest)
				{
					return new Color32(0, 0, byte.MaxValue, 0);
				}
				if (biome == Heightmap.Biome.Plains)
				{
					return new Color32(0, 0, 0, byte.MaxValue);
				}
				break;
			}
		}
		else
		{
			if (biome == Heightmap.Biome.AshLands)
			{
				return new Color32(byte.MaxValue, 0, 0, byte.MaxValue);
			}
			if (biome == Heightmap.Biome.DeepNorth)
			{
				return new Color32(0, byte.MaxValue, 0, 0);
			}
			if (biome == Heightmap.Biome.Mistlands)
			{
				return new Color32(0, 0, byte.MaxValue, byte.MaxValue);
			}
		}
		return new Color32(0, 0, 0, 0);
	}

	// Token: 0x06000D8B RID: 3467 RVA: 0x00060690 File Offset: 0x0005E890
	private void RebuildCollisionMesh()
	{
		if (this.m_collisionMesh == null)
		{
			this.m_collisionMesh = new Mesh();
		}
		int num = this.m_width + 1;
		float num2 = -999999f;
		float num3 = 999999f;
		Heightmap.m_tempVertises.Clear();
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				Vector3 vector = this.CalcVertex(j, i);
				Heightmap.m_tempVertises.Add(vector);
				if (vector.y > num2)
				{
					num2 = vector.y;
				}
				if (vector.y < num3)
				{
					num3 = vector.y;
				}
			}
		}
		this.m_collisionMesh.SetVertices(Heightmap.m_tempVertises);
		int num4 = (num - 1) * (num - 1) * 6;
		if ((ulong)this.m_collisionMesh.GetIndexCount(0) != (ulong)((long)num4))
		{
			Heightmap.m_tempIndices.Clear();
			for (int k = 0; k < num - 1; k++)
			{
				for (int l = 0; l < num - 1; l++)
				{
					int item = k * num + l;
					int item2 = k * num + l + 1;
					int item3 = (k + 1) * num + l + 1;
					int item4 = (k + 1) * num + l;
					Heightmap.m_tempIndices.Add(item);
					Heightmap.m_tempIndices.Add(item4);
					Heightmap.m_tempIndices.Add(item2);
					Heightmap.m_tempIndices.Add(item2);
					Heightmap.m_tempIndices.Add(item4);
					Heightmap.m_tempIndices.Add(item3);
				}
			}
			this.m_collisionMesh.SetIndices(Heightmap.m_tempIndices.ToArray(), MeshTopology.Triangles, 0);
		}
		if (this.m_collider)
		{
			this.m_collider.sharedMesh = this.m_collisionMesh;
		}
		float num5 = (float)this.m_width * this.m_scale * 0.5f;
		this.m_bounds.SetMinMax(base.transform.position + new Vector3(-num5, num3, -num5), base.transform.position + new Vector3(num5, num2, num5));
		this.m_boundingSphere.position = this.m_bounds.center;
		this.m_boundingSphere.radius = Vector3.Distance(this.m_boundingSphere.position, this.m_bounds.max);
	}

	// Token: 0x06000D8C RID: 3468 RVA: 0x000608D4 File Offset: 0x0005EAD4
	private void RebuildRenderMesh()
	{
		if (this.m_renderMesh == null)
		{
			this.m_renderMesh = new Mesh();
		}
		WorldGenerator instance = WorldGenerator.instance;
		int num = this.m_width + 1;
		Vector3 vector = base.transform.position + new Vector3((float)this.m_width * this.m_scale * -0.5f, 0f, (float)this.m_width * this.m_scale * -0.5f);
		Heightmap.m_tempVertises.Clear();
		Heightmap.m_tempUVs.Clear();
		Heightmap.m_tempIndices.Clear();
		Heightmap.m_tempColors.Clear();
		for (int i = 0; i < num; i++)
		{
			float iy = Mathf.SmoothStep(0f, 1f, (float)i / (float)this.m_width);
			for (int j = 0; j < num; j++)
			{
				float ix = Mathf.SmoothStep(0f, 1f, (float)j / (float)this.m_width);
				Heightmap.m_tempUVs.Add(new Vector2((float)j / (float)this.m_width, (float)i / (float)this.m_width));
				if (this.m_isDistantLod)
				{
					float wx = vector.x + (float)j * this.m_scale;
					float wy = vector.z + (float)i * this.m_scale;
					Heightmap.Biome biome = instance.GetBiome(wx, wy);
					Heightmap.m_tempColors.Add(Heightmap.GetBiomeColor(biome));
				}
				else
				{
					Heightmap.m_tempColors.Add(this.GetBiomeColor(ix, iy));
				}
			}
		}
		this.m_collisionMesh.GetVertices(Heightmap.m_tempVertises);
		this.m_collisionMesh.GetIndices(Heightmap.m_tempIndices, 0);
		this.m_renderMesh.Clear();
		this.m_renderMesh.SetVertices(Heightmap.m_tempVertises);
		this.m_renderMesh.SetColors(Heightmap.m_tempColors);
		this.m_renderMesh.SetUVs(0, Heightmap.m_tempUVs);
		this.m_renderMesh.SetIndices(Heightmap.m_tempIndices.ToArray(), MeshTopology.Triangles, 0, true);
		this.m_renderMesh.RecalculateNormals();
		this.m_renderMesh.RecalculateTangents();
	}

	// Token: 0x06000D8D RID: 3469 RVA: 0x00060AE8 File Offset: 0x0005ECE8
	private void SmoothTerrain2(Vector3 worldPos, float radius, bool square, float[] levelOnlyHeights, float power, bool playerModifiction)
	{
		int num;
		int num2;
		this.WorldToVertex(worldPos, out num, out num2);
		float b = worldPos.y - base.transform.position.y;
		float num3 = radius / this.m_scale;
		int num4 = Mathf.CeilToInt(num3);
		Vector2 a = new Vector2((float)num, (float)num2);
		int num5 = this.m_width + 1;
		for (int i = num2 - num4; i <= num2 + num4; i++)
		{
			for (int j = num - num4; j <= num + num4; j++)
			{
				float num6 = Vector2.Distance(a, new Vector2((float)j, (float)i));
				if (num6 <= num3)
				{
					float num7 = num6 / num3;
					if (j >= 0 && i >= 0 && j < num5 && i < num5)
					{
						if (power == 3f)
						{
							num7 = num7 * num7 * num7;
						}
						else
						{
							num7 = Mathf.Pow(num7, power);
						}
						float height = this.GetHeight(j, i);
						float t = 1f - num7;
						float num8 = Mathf.Lerp(height, b, t);
						if (playerModifiction)
						{
							float num9 = levelOnlyHeights[i * num5 + j];
							num8 = Mathf.Clamp(num8, num9 - 1f, num9 + 1f);
						}
						this.SetHeight(j, i, num8);
					}
				}
			}
		}
	}

	// Token: 0x06000D8E RID: 3470 RVA: 0x00060C28 File Offset: 0x0005EE28
	private bool AtMaxWorldLevelDepth(Vector3 worldPos)
	{
		float num;
		this.GetWorldHeight(worldPos, out num);
		float num2;
		this.GetWorldBaseHeight(worldPos, out num2);
		return Mathf.Max(-(num - num2), 0f) >= 7.95f;
	}

	// Token: 0x06000D8F RID: 3471 RVA: 0x00060C64 File Offset: 0x0005EE64
	private bool GetWorldBaseHeight(Vector3 worldPos, out float height)
	{
		int num;
		int num2;
		this.WorldToVertex(worldPos, out num, out num2);
		int num3 = this.m_width + 1;
		if (num < 0 || num2 < 0 || num >= num3 || num2 >= num3)
		{
			height = 0f;
			return false;
		}
		height = this.m_buildData.m_baseHeights[num2 * num3 + num] + base.transform.position.y;
		return true;
	}

	// Token: 0x06000D90 RID: 3472 RVA: 0x00060CC8 File Offset: 0x0005EEC8
	private bool GetWorldHeight(Vector3 worldPos, out float height)
	{
		int num;
		int num2;
		this.WorldToVertex(worldPos, out num, out num2);
		int num3 = this.m_width + 1;
		if (num < 0 || num2 < 0 || num >= num3 || num2 >= num3)
		{
			height = 0f;
			return false;
		}
		height = this.m_heights[num2 * num3 + num] + base.transform.position.y;
		return true;
	}

	// Token: 0x06000D91 RID: 3473 RVA: 0x00060D28 File Offset: 0x0005EF28
	private bool GetAverageWorldHeight(Vector3 worldPos, float radius, out float height)
	{
		int num;
		int num2;
		this.WorldToVertex(worldPos, out num, out num2);
		float num3 = radius / this.m_scale;
		int num4 = Mathf.CeilToInt(num3);
		Vector2 a = new Vector2((float)num, (float)num2);
		int num5 = this.m_width + 1;
		float num6 = 0f;
		int num7 = 0;
		for (int i = num2 - num4; i <= num2 + num4; i++)
		{
			for (int j = num - num4; j <= num + num4; j++)
			{
				if (Vector2.Distance(a, new Vector2((float)j, (float)i)) <= num3 && j >= 0 && i >= 0 && j < num5 && i < num5)
				{
					num6 += this.GetHeight(j, i);
					num7++;
				}
			}
		}
		if (num7 == 0)
		{
			height = 0f;
			return false;
		}
		height = num6 / (float)num7 + base.transform.position.y;
		return true;
	}

	// Token: 0x06000D92 RID: 3474 RVA: 0x00060E00 File Offset: 0x0005F000
	private bool GetMinWorldHeight(Vector3 worldPos, float radius, out float height)
	{
		int num;
		int num2;
		this.WorldToVertex(worldPos, out num, out num2);
		float num3 = radius / this.m_scale;
		int num4 = Mathf.CeilToInt(num3);
		Vector2 a = new Vector2((float)num, (float)num2);
		int num5 = this.m_width + 1;
		height = 99999f;
		for (int i = num2 - num4; i <= num2 + num4; i++)
		{
			for (int j = num - num4; j <= num + num4; j++)
			{
				if (Vector2.Distance(a, new Vector2((float)j, (float)i)) <= num3 && j >= 0 && i >= 0 && j < num5 && i < num5)
				{
					float height2 = this.GetHeight(j, i);
					if (height2 < height)
					{
						height = height2;
					}
				}
			}
		}
		return height != 99999f;
	}

	// Token: 0x06000D93 RID: 3475 RVA: 0x00060EBC File Offset: 0x0005F0BC
	private bool GetMaxWorldHeight(Vector3 worldPos, float radius, out float height)
	{
		int num;
		int num2;
		this.WorldToVertex(worldPos, out num, out num2);
		float num3 = radius / this.m_scale;
		int num4 = Mathf.CeilToInt(num3);
		Vector2 a = new Vector2((float)num, (float)num2);
		int num5 = this.m_width + 1;
		height = -99999f;
		for (int i = num2 - num4; i <= num2 + num4; i++)
		{
			for (int j = num - num4; j <= num + num4; j++)
			{
				if (Vector2.Distance(a, new Vector2((float)j, (float)i)) <= num3 && j >= 0 && i >= 0 && j < num5 && i < num5)
				{
					float height2 = this.GetHeight(j, i);
					if (height2 > height)
					{
						height = height2;
					}
				}
			}
		}
		return height != -99999f;
	}

	// Token: 0x06000D94 RID: 3476 RVA: 0x00060F78 File Offset: 0x0005F178
	public static bool AtMaxLevelDepth(Vector3 worldPos)
	{
		Heightmap heightmap = Heightmap.FindHeightmap(worldPos);
		return heightmap && heightmap.AtMaxWorldLevelDepth(worldPos);
	}

	// Token: 0x06000D95 RID: 3477 RVA: 0x00060FA0 File Offset: 0x0005F1A0
	public static bool GetHeight(Vector3 worldPos, out float height)
	{
		Heightmap heightmap = Heightmap.FindHeightmap(worldPos);
		if (heightmap && heightmap.GetWorldHeight(worldPos, out height))
		{
			return true;
		}
		height = 0f;
		return false;
	}

	// Token: 0x06000D96 RID: 3478 RVA: 0x00060FD0 File Offset: 0x0005F1D0
	public static bool GetAverageHeight(Vector3 worldPos, float radius, out float height)
	{
		List<Heightmap> list = new List<Heightmap>();
		Heightmap.FindHeightmap(worldPos, radius, list);
		float num = 0f;
		int num2 = 0;
		using (List<Heightmap>.Enumerator enumerator = list.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				float num3;
				if (enumerator.Current.GetAverageWorldHeight(worldPos, radius, out num3))
				{
					num += num3;
					num2++;
				}
			}
		}
		if (num2 > 0)
		{
			height = num / (float)num2;
			return true;
		}
		height = 0f;
		return false;
	}

	// Token: 0x06000D97 RID: 3479 RVA: 0x00061054 File Offset: 0x0005F254
	private void SmoothTerrain(Vector3 worldPos, float radius, bool square, float intensity)
	{
		int num;
		int num2;
		this.WorldToVertex(worldPos, out num, out num2);
		float num3 = radius / this.m_scale;
		int num4 = Mathf.CeilToInt(num3);
		Vector2 a = new Vector2((float)num, (float)num2);
		List<KeyValuePair<Vector2i, float>> list = new List<KeyValuePair<Vector2i, float>>();
		for (int i = num2 - num4; i <= num2 + num4; i++)
		{
			for (int j = num - num4; j <= num + num4; j++)
			{
				if ((square || Vector2.Distance(a, new Vector2((float)j, (float)i)) <= num3) && j != 0 && i != 0 && j != this.m_width && i != this.m_width)
				{
					list.Add(new KeyValuePair<Vector2i, float>(new Vector2i(j, i), this.GetAvgHeight(j, i, 1)));
				}
			}
		}
		foreach (KeyValuePair<Vector2i, float> keyValuePair in list)
		{
			float h = Mathf.Lerp(this.GetHeight(keyValuePair.Key.x, keyValuePair.Key.y), keyValuePair.Value, intensity);
			this.SetHeight(keyValuePair.Key.x, keyValuePair.Key.y, h);
		}
	}

	// Token: 0x06000D98 RID: 3480 RVA: 0x00061198 File Offset: 0x0005F398
	private float GetAvgHeight(int cx, int cy, int w)
	{
		int num = this.m_width + 1;
		float num2 = 0f;
		int num3 = 0;
		for (int i = cy - w; i <= cy + w; i++)
		{
			for (int j = cx - w; j <= cx + w; j++)
			{
				if (j >= 0 && i >= 0 && j < num && i < num)
				{
					num2 += this.GetHeight(j, i);
					num3++;
				}
			}
		}
		if (num3 == 0)
		{
			return 0f;
		}
		return num2 / (float)num3;
	}

	// Token: 0x06000D99 RID: 3481 RVA: 0x0006120C File Offset: 0x0005F40C
	private float GroundHeight(Vector3 point)
	{
		Ray ray = new Ray(point + Vector3.up * 100f, Vector3.down);
		RaycastHit raycastHit;
		if (this.m_collider.Raycast(ray, out raycastHit, 300f))
		{
			return raycastHit.point.y;
		}
		return -10000f;
	}

	// Token: 0x06000D9A RID: 3482 RVA: 0x00061264 File Offset: 0x0005F464
	private void FindObjectsToMove(Vector3 worldPos, float area, List<Rigidbody> objects)
	{
		if (this.m_collider == null)
		{
			return;
		}
		foreach (Collider collider in Physics.OverlapBox(worldPos, new Vector3(area / 2f, 500f, area / 2f)))
		{
			if (!(collider == this.m_collider) && collider.attachedRigidbody)
			{
				Rigidbody attachedRigidbody = collider.attachedRigidbody;
				ZNetView component = attachedRigidbody.GetComponent<ZNetView>();
				if (!component || component.IsOwner())
				{
					objects.Add(attachedRigidbody);
				}
			}
		}
	}

	// Token: 0x06000D9B RID: 3483 RVA: 0x000612F4 File Offset: 0x0005F4F4
	private void PaintCleared(Vector3 worldPos, float radius, TerrainModifier.PaintType paintType, bool heightCheck, bool apply)
	{
		worldPos.x -= 0.5f;
		worldPos.z -= 0.5f;
		float num = worldPos.y - base.transform.position.y;
		int num2;
		int num3;
		this.WorldToVertex(worldPos, out num2, out num3);
		float num4 = radius / this.m_scale;
		int num5 = Mathf.CeilToInt(num4);
		Vector2 a = new Vector2((float)num2, (float)num3);
		for (int i = num3 - num5; i <= num3 + num5; i++)
		{
			for (int j = num2 - num5; j <= num2 + num5; j++)
			{
				float num6 = Vector2.Distance(a, new Vector2((float)j, (float)i));
				if (j >= 0 && i >= 0 && j < this.m_clearedMask.width && i < this.m_clearedMask.height && (!heightCheck || this.GetHeight(j, i) <= num))
				{
					float num7 = 1f - Mathf.Clamp01(num6 / num4);
					num7 = Mathf.Pow(num7, 0.1f);
					Color color = this.m_clearedMask.GetPixel(j, i);
					switch (paintType)
					{
					case TerrainModifier.PaintType.Dirt:
						color = Color.Lerp(color, Color.red, num7);
						break;
					case TerrainModifier.PaintType.Cultivate:
						color = Color.Lerp(color, Color.green, num7);
						break;
					case TerrainModifier.PaintType.Paved:
						color = Color.Lerp(color, Color.blue, num7);
						break;
					case TerrainModifier.PaintType.Reset:
						color = Color.Lerp(color, Color.black, num7);
						break;
					}
					this.m_clearedMask.SetPixel(j, i, color);
				}
			}
		}
		if (apply)
		{
			this.m_clearedMask.Apply();
		}
	}

	// Token: 0x06000D9C RID: 3484 RVA: 0x000614A4 File Offset: 0x0005F6A4
	public bool IsCleared(Vector3 worldPos)
	{
		worldPos.x -= 0.5f;
		worldPos.z -= 0.5f;
		int x;
		int y;
		this.WorldToVertex(worldPos, out x, out y);
		Color pixel = this.m_clearedMask.GetPixel(x, y);
		return pixel.r > 0.5f || pixel.g > 0.5f || pixel.b > 0.5f;
	}

	// Token: 0x06000D9D RID: 3485 RVA: 0x00061514 File Offset: 0x0005F714
	public bool IsCultivated(Vector3 worldPos)
	{
		int x;
		int y;
		this.WorldToVertex(worldPos, out x, out y);
		return this.m_clearedMask.GetPixel(x, y).g > 0.5f;
	}

	// Token: 0x06000D9E RID: 3486 RVA: 0x00061548 File Offset: 0x0005F748
	private void WorldToVertex(Vector3 worldPos, out int x, out int y)
	{
		Vector3 vector = worldPos - base.transform.position;
		x = Mathf.FloorToInt(vector.x / this.m_scale + 0.5f) + this.m_width / 2;
		y = Mathf.FloorToInt(vector.z / this.m_scale + 0.5f) + this.m_width / 2;
	}

	// Token: 0x06000D9F RID: 3487 RVA: 0x000615B0 File Offset: 0x0005F7B0
	private void WorldToNormalizedHM(Vector3 worldPos, out float x, out float y)
	{
		float num = (float)this.m_width * this.m_scale;
		Vector3 vector = worldPos - base.transform.position;
		x = vector.x / num + 0.5f;
		y = vector.z / num + 0.5f;
	}

	// Token: 0x06000DA0 RID: 3488 RVA: 0x00061600 File Offset: 0x0005F800
	private void LevelTerrain(Vector3 worldPos, float radius, bool square, float[] baseHeights, float[] levelOnly, bool playerModifiction)
	{
		int num;
		int num2;
		this.WorldToVertex(worldPos, out num, out num2);
		Vector3 vector = worldPos - base.transform.position;
		float num3 = radius / this.m_scale;
		int num4 = Mathf.CeilToInt(num3);
		int num5 = this.m_width + 1;
		Vector2 a = new Vector2((float)num, (float)num2);
		for (int i = num2 - num4; i <= num2 + num4; i++)
		{
			for (int j = num - num4; j <= num + num4; j++)
			{
				if ((square || Vector2.Distance(a, new Vector2((float)j, (float)i)) <= num3) && j >= 0 && i >= 0 && j < num5 && i < num5)
				{
					float num6 = vector.y;
					if (playerModifiction)
					{
						float num7 = baseHeights[i * num5 + j];
						num6 = Mathf.Clamp(num6, num7 - 8f, num7 + 8f);
						levelOnly[i * num5 + j] = num6;
					}
					this.SetHeight(j, i, num6);
				}
			}
		}
	}

	// Token: 0x06000DA1 RID: 3489 RVA: 0x00061700 File Offset: 0x0005F900
	private float GetHeight(int x, int y)
	{
		int num = this.m_width + 1;
		if (x < 0 || y < 0 || x >= num || y >= num)
		{
			return 0f;
		}
		return this.m_heights[y * num + x];
	}

	// Token: 0x06000DA2 RID: 3490 RVA: 0x0006173C File Offset: 0x0005F93C
	private float GetBaseHeight(int x, int y)
	{
		int num = this.m_width + 1;
		if (x < 0 || y < 0 || x >= num || y >= num)
		{
			return 0f;
		}
		return this.m_buildData.m_baseHeights[y * num + x];
	}

	// Token: 0x06000DA3 RID: 3491 RVA: 0x00061780 File Offset: 0x0005F980
	private void SetHeight(int x, int y, float h)
	{
		int num = this.m_width + 1;
		if (x < 0 || y < 0 || x >= num || y >= num)
		{
			return;
		}
		this.m_heights[y * num + x] = h;
	}

	// Token: 0x06000DA4 RID: 3492 RVA: 0x000617B8 File Offset: 0x0005F9B8
	public bool IsPointInside(Vector3 point, float radius = 0f)
	{
		float num = (float)this.m_width * this.m_scale * 0.5f;
		Vector3 position = base.transform.position;
		return point.x + radius >= position.x - num && point.x - radius <= position.x + num && point.z + radius >= position.z - num && point.z - radius <= position.z + num;
	}

	// Token: 0x06000DA5 RID: 3493 RVA: 0x00061831 File Offset: 0x0005FA31
	public static List<Heightmap> GetAllHeightmaps()
	{
		return Heightmap.m_heightmaps;
	}

	// Token: 0x06000DA6 RID: 3494 RVA: 0x00061838 File Offset: 0x0005FA38
	public static Heightmap FindHeightmap(Vector3 point)
	{
		foreach (Heightmap heightmap in Heightmap.m_heightmaps)
		{
			if (heightmap.IsPointInside(point, 0f))
			{
				return heightmap;
			}
		}
		return null;
	}

	// Token: 0x06000DA7 RID: 3495 RVA: 0x00061898 File Offset: 0x0005FA98
	public static void FindHeightmap(Vector3 point, float radius, List<Heightmap> heightmaps)
	{
		foreach (Heightmap heightmap in Heightmap.m_heightmaps)
		{
			if (heightmap.IsPointInside(point, radius))
			{
				heightmaps.Add(heightmap);
			}
		}
	}

	// Token: 0x06000DA8 RID: 3496 RVA: 0x000618F4 File Offset: 0x0005FAF4
	public static Heightmap.Biome FindBiome(Vector3 point)
	{
		Heightmap heightmap = Heightmap.FindHeightmap(point);
		if (heightmap)
		{
			return heightmap.GetBiome(point);
		}
		return Heightmap.Biome.None;
	}

	// Token: 0x06000DA9 RID: 3497 RVA: 0x0006191C File Offset: 0x0005FB1C
	public static bool HaveQueuedRebuild(Vector3 point, float radius)
	{
		Heightmap.tempHmaps.Clear();
		Heightmap.FindHeightmap(point, radius, Heightmap.tempHmaps);
		using (List<Heightmap>.Enumerator enumerator = Heightmap.tempHmaps.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.HaveQueuedRebuild())
				{
					return true;
				}
			}
		}
		return false;
	}

	// Token: 0x06000DAA RID: 3498 RVA: 0x0006198C File Offset: 0x0005FB8C
	public static Heightmap.Biome FindBiomeClutter(Vector3 point)
	{
		if (ZoneSystem.instance && !ZoneSystem.instance.IsZoneLoaded(point))
		{
			return Heightmap.Biome.None;
		}
		Heightmap heightmap = Heightmap.FindHeightmap(point);
		if (heightmap)
		{
			return heightmap.GetBiome(point);
		}
		return Heightmap.Biome.None;
	}

	// Token: 0x06000DAB RID: 3499 RVA: 0x000619CC File Offset: 0x0005FBCC
	public void Clear()
	{
		this.m_heights.Clear();
		this.m_clearedMask = null;
		this.m_materialInstance = null;
		this.m_buildData = null;
		if (this.m_collisionMesh)
		{
			this.m_collisionMesh.Clear();
		}
		if (this.m_renderMesh)
		{
			this.m_renderMesh.Clear();
		}
		if (this.m_collider)
		{
			this.m_collider.sharedMesh = null;
		}
	}

	// Token: 0x04000C4C RID: 3148
	private static float[] tempBiomeWeights = new float[513];

	// Token: 0x04000C4D RID: 3149
	private static List<Heightmap> tempHmaps = new List<Heightmap>();

	// Token: 0x04000C4E RID: 3150
	public int m_width = 32;

	// Token: 0x04000C4F RID: 3151
	public float m_scale = 1f;

	// Token: 0x04000C50 RID: 3152
	public Material m_material;

	// Token: 0x04000C51 RID: 3153
	private const float m_levelMaxDelta = 8f;

	// Token: 0x04000C52 RID: 3154
	private const float m_smoothMaxDelta = 1f;

	// Token: 0x04000C53 RID: 3155
	public bool m_isDistantLod;

	// Token: 0x04000C54 RID: 3156
	public bool m_distantLodEditorHax;

	// Token: 0x04000C55 RID: 3157
	private List<float> m_heights = new List<float>();

	// Token: 0x04000C56 RID: 3158
	private HeightmapBuilder.HMBuildData m_buildData;

	// Token: 0x04000C57 RID: 3159
	private Texture2D m_clearedMask;

	// Token: 0x04000C58 RID: 3160
	private Material m_materialInstance;

	// Token: 0x04000C59 RID: 3161
	private MeshCollider m_collider;

	// Token: 0x04000C5A RID: 3162
	private float[] m_oceanDepth = new float[4];

	// Token: 0x04000C5B RID: 3163
	private Heightmap.Biome[] m_cornerBiomes = new Heightmap.Biome[]
	{
		Heightmap.Biome.Meadows,
		Heightmap.Biome.Meadows,
		Heightmap.Biome.Meadows,
		Heightmap.Biome.Meadows
	};

	// Token: 0x04000C5C RID: 3164
	private Bounds m_bounds;

	// Token: 0x04000C5D RID: 3165
	private BoundingSphere m_boundingSphere;

	// Token: 0x04000C5E RID: 3166
	private Mesh m_collisionMesh;

	// Token: 0x04000C5F RID: 3167
	private Mesh m_renderMesh;

	// Token: 0x04000C60 RID: 3168
	private bool m_dirty;

	// Token: 0x04000C61 RID: 3169
	private static List<Heightmap> m_heightmaps = new List<Heightmap>();

	// Token: 0x04000C62 RID: 3170
	private static List<Vector3> m_tempVertises = new List<Vector3>();

	// Token: 0x04000C63 RID: 3171
	private static List<Vector2> m_tempUVs = new List<Vector2>();

	// Token: 0x04000C64 RID: 3172
	private static List<int> m_tempIndices = new List<int>();

	// Token: 0x04000C65 RID: 3173
	private static List<Color32> m_tempColors = new List<Color32>();

	// Token: 0x02000198 RID: 408
	public enum Biome
	{
		// Token: 0x04001295 RID: 4757
		None,
		// Token: 0x04001296 RID: 4758
		Meadows,
		// Token: 0x04001297 RID: 4759
		Swamp,
		// Token: 0x04001298 RID: 4760
		Mountain = 4,
		// Token: 0x04001299 RID: 4761
		BlackForest = 8,
		// Token: 0x0400129A RID: 4762
		Plains = 16,
		// Token: 0x0400129B RID: 4763
		AshLands = 32,
		// Token: 0x0400129C RID: 4764
		DeepNorth = 64,
		// Token: 0x0400129D RID: 4765
		Ocean = 256,
		// Token: 0x0400129E RID: 4766
		Mistlands = 512,
		// Token: 0x0400129F RID: 4767
		BiomesMax
	}

	// Token: 0x02000199 RID: 409
	public enum BiomeArea
	{
		// Token: 0x040012A1 RID: 4769
		Edge = 1,
		// Token: 0x040012A2 RID: 4770
		Median,
		// Token: 0x040012A3 RID: 4771
		Everything
	}
}
