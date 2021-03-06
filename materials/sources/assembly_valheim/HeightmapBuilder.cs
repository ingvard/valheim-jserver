﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

// Token: 0x020000D2 RID: 210
public class HeightmapBuilder
{
	// Token: 0x17000031 RID: 49
	// (get) Token: 0x06000DAE RID: 3502 RVA: 0x00061AF0 File Offset: 0x0005FCF0
	public static HeightmapBuilder instance
	{
		get
		{
			if (HeightmapBuilder.m_instance == null)
			{
				HeightmapBuilder.m_instance = new HeightmapBuilder();
			}
			return HeightmapBuilder.m_instance;
		}
	}

	// Token: 0x06000DAF RID: 3503 RVA: 0x00061B08 File Offset: 0x0005FD08
	public HeightmapBuilder()
	{
		HeightmapBuilder.m_instance = this;
		this.m_builder = new Thread(new ThreadStart(this.BuildThread));
		this.m_builder.Start();
	}

	// Token: 0x06000DB0 RID: 3504 RVA: 0x00061B64 File Offset: 0x0005FD64
	public void Dispose()
	{
		if (this.m_builder != null)
		{
			ZLog.Log("Stoping build thread");
			this.m_lock.WaitOne();
			this.m_stop = true;
			this.m_builder.Abort();
			this.m_lock.ReleaseMutex();
			this.m_builder = null;
		}
		if (this.m_lock != null)
		{
			this.m_lock.Close();
			this.m_lock = null;
		}
	}

	// Token: 0x06000DB1 RID: 3505 RVA: 0x00061BD0 File Offset: 0x0005FDD0
	private void BuildThread()
	{
		ZLog.Log("Builder started");
		while (!this.m_stop)
		{
			this.m_lock.WaitOne();
			bool flag = this.m_toBuild.Count > 0;
			this.m_lock.ReleaseMutex();
			if (flag)
			{
				this.m_lock.WaitOne();
				HeightmapBuilder.HMBuildData hmbuildData = this.m_toBuild[0];
				this.m_lock.ReleaseMutex();
				new Stopwatch().Start();
				this.Build(hmbuildData);
				this.m_lock.WaitOne();
				this.m_toBuild.Remove(hmbuildData);
				this.m_ready.Add(hmbuildData);
				while (this.m_ready.Count > 16)
				{
					this.m_ready.RemoveAt(0);
				}
				this.m_lock.ReleaseMutex();
			}
			Thread.Sleep(10);
		}
	}

	// Token: 0x06000DB2 RID: 3506 RVA: 0x00061CAC File Offset: 0x0005FEAC
	private void Build(HeightmapBuilder.HMBuildData data)
	{
		int num = data.m_width + 1;
		int num2 = num * num;
		Vector3 vector = data.m_center + new Vector3((float)data.m_width * data.m_scale * -0.5f, 0f, (float)data.m_width * data.m_scale * -0.5f);
		WorldGenerator worldGen = data.m_worldGen;
		data.m_cornerBiomes = new Heightmap.Biome[4];
		data.m_cornerBiomes[0] = worldGen.GetBiome(vector.x, vector.z);
		data.m_cornerBiomes[1] = worldGen.GetBiome(vector.x + (float)data.m_width * data.m_scale, vector.z);
		data.m_cornerBiomes[2] = worldGen.GetBiome(vector.x, vector.z + (float)data.m_width * data.m_scale);
		data.m_cornerBiomes[3] = worldGen.GetBiome(vector.x + (float)data.m_width * data.m_scale, vector.z + (float)data.m_width * data.m_scale);
		Heightmap.Biome biome = data.m_cornerBiomes[0];
		Heightmap.Biome biome2 = data.m_cornerBiomes[1];
		Heightmap.Biome biome3 = data.m_cornerBiomes[2];
		Heightmap.Biome biome4 = data.m_cornerBiomes[3];
		data.m_baseHeights = new List<float>(num * num);
		for (int i = 0; i < num2; i++)
		{
			data.m_baseHeights.Add(0f);
		}
		for (int j = 0; j < num; j++)
		{
			float wy = vector.z + (float)j * data.m_scale;
			float t = Mathf.SmoothStep(0f, 1f, (float)j / (float)data.m_width);
			for (int k = 0; k < num; k++)
			{
				float wx = vector.x + (float)k * data.m_scale;
				float t2 = Mathf.SmoothStep(0f, 1f, (float)k / (float)data.m_width);
				float value;
				if (data.m_distantLod)
				{
					Heightmap.Biome biome5 = worldGen.GetBiome(wx, wy);
					value = worldGen.GetBiomeHeight(biome5, wx, wy);
				}
				else if (biome3 == biome && biome2 == biome && biome4 == biome)
				{
					value = worldGen.GetBiomeHeight(biome, wx, wy);
				}
				else
				{
					float biomeHeight = worldGen.GetBiomeHeight(biome, wx, wy);
					float biomeHeight2 = worldGen.GetBiomeHeight(biome2, wx, wy);
					float biomeHeight3 = worldGen.GetBiomeHeight(biome3, wx, wy);
					float biomeHeight4 = worldGen.GetBiomeHeight(biome4, wx, wy);
					float a = Mathf.Lerp(biomeHeight, biomeHeight2, t2);
					float b = Mathf.Lerp(biomeHeight3, biomeHeight4, t2);
					value = Mathf.Lerp(a, b, t);
				}
				data.m_baseHeights[j * num + k] = value;
			}
		}
		if (data.m_distantLod)
		{
			for (int l = 0; l < 4; l++)
			{
				List<float> list = new List<float>(data.m_baseHeights);
				for (int m = 1; m < num - 1; m++)
				{
					for (int n = 1; n < num - 1; n++)
					{
						float num3 = list[m * num + n];
						float num4 = list[(m - 1) * num + n];
						float num5 = list[(m + 1) * num + n];
						float num6 = list[m * num + n - 1];
						float num7 = list[m * num + n + 1];
						if (Mathf.Abs(num3 - num4) > 10f)
						{
							num3 = (num3 + num4) * 0.5f;
						}
						if (Mathf.Abs(num3 - num5) > 10f)
						{
							num3 = (num3 + num5) * 0.5f;
						}
						if (Mathf.Abs(num3 - num6) > 10f)
						{
							num3 = (num3 + num6) * 0.5f;
						}
						if (Mathf.Abs(num3 - num7) > 10f)
						{
							num3 = (num3 + num7) * 0.5f;
						}
						data.m_baseHeights[m * num + n] = num3;
					}
				}
			}
		}
	}

	// Token: 0x06000DB3 RID: 3507 RVA: 0x00062098 File Offset: 0x00060298
	public HeightmapBuilder.HMBuildData RequestTerrainSync(Vector3 center, int width, float scale, bool distantLod, WorldGenerator worldGen)
	{
		HeightmapBuilder.HMBuildData hmbuildData;
		do
		{
			hmbuildData = this.RequestTerrain(center, width, scale, distantLod, worldGen);
		}
		while (hmbuildData == null);
		return hmbuildData;
	}

	// Token: 0x06000DB4 RID: 3508 RVA: 0x000620B8 File Offset: 0x000602B8
	public HeightmapBuilder.HMBuildData RequestTerrain(Vector3 center, int width, float scale, bool distantLod, WorldGenerator worldGen)
	{
		this.m_lock.WaitOne();
		for (int i = 0; i < this.m_ready.Count; i++)
		{
			HeightmapBuilder.HMBuildData hmbuildData = this.m_ready[i];
			if (hmbuildData.IsEqual(center, width, scale, distantLod, worldGen))
			{
				this.m_ready.RemoveAt(i);
				this.m_lock.ReleaseMutex();
				return hmbuildData;
			}
		}
		for (int j = 0; j < this.m_toBuild.Count; j++)
		{
			if (this.m_toBuild[j].IsEqual(center, width, scale, distantLod, worldGen))
			{
				this.m_lock.ReleaseMutex();
				return null;
			}
		}
		this.m_toBuild.Add(new HeightmapBuilder.HMBuildData(center, width, scale, distantLod, worldGen));
		this.m_lock.ReleaseMutex();
		return null;
	}

	// Token: 0x06000DB5 RID: 3509 RVA: 0x0006217C File Offset: 0x0006037C
	public bool IsTerrainReady(Vector3 center, int width, float scale, bool distantLod, WorldGenerator worldGen)
	{
		this.m_lock.WaitOne();
		for (int i = 0; i < this.m_ready.Count; i++)
		{
			if (this.m_ready[i].IsEqual(center, width, scale, distantLod, worldGen))
			{
				this.m_lock.ReleaseMutex();
				return true;
			}
		}
		for (int j = 0; j < this.m_toBuild.Count; j++)
		{
			if (this.m_toBuild[j].IsEqual(center, width, scale, distantLod, worldGen))
			{
				this.m_lock.ReleaseMutex();
				return false;
			}
		}
		this.m_toBuild.Add(new HeightmapBuilder.HMBuildData(center, width, scale, distantLod, worldGen));
		this.m_lock.ReleaseMutex();
		return false;
	}

	// Token: 0x04000C66 RID: 3174
	private static HeightmapBuilder m_instance;

	// Token: 0x04000C67 RID: 3175
	private const int m_maxReadyQueue = 16;

	// Token: 0x04000C68 RID: 3176
	private List<HeightmapBuilder.HMBuildData> m_toBuild = new List<HeightmapBuilder.HMBuildData>();

	// Token: 0x04000C69 RID: 3177
	private List<HeightmapBuilder.HMBuildData> m_ready = new List<HeightmapBuilder.HMBuildData>();

	// Token: 0x04000C6A RID: 3178
	private Thread m_builder;

	// Token: 0x04000C6B RID: 3179
	private Mutex m_lock = new Mutex();

	// Token: 0x04000C6C RID: 3180
	private bool m_stop;

	// Token: 0x0200019A RID: 410
	public class HMBuildData
	{
		// Token: 0x060011A1 RID: 4513 RVA: 0x00079305 File Offset: 0x00077505
		public HMBuildData(Vector3 center, int width, float scale, bool distantLod, WorldGenerator worldGen)
		{
			this.m_center = center;
			this.m_width = width;
			this.m_scale = scale;
			this.m_distantLod = distantLod;
			this.m_worldGen = worldGen;
		}

		// Token: 0x060011A2 RID: 4514 RVA: 0x00079332 File Offset: 0x00077532
		public bool IsEqual(Vector3 center, int width, float scale, bool distantLod, WorldGenerator worldGen)
		{
			return this.m_center == center && this.m_width == width && this.m_scale == scale && this.m_distantLod == distantLod && this.m_worldGen == worldGen;
		}

		// Token: 0x040012A4 RID: 4772
		public Vector3 m_center;

		// Token: 0x040012A5 RID: 4773
		public int m_width;

		// Token: 0x040012A6 RID: 4774
		public float m_scale;

		// Token: 0x040012A7 RID: 4775
		public bool m_distantLod;

		// Token: 0x040012A8 RID: 4776
		public bool m_menu;

		// Token: 0x040012A9 RID: 4777
		public WorldGenerator m_worldGen;

		// Token: 0x040012AA RID: 4778
		public Heightmap.Biome[] m_cornerBiomes;

		// Token: 0x040012AB RID: 4779
		public List<float> m_baseHeights;
	}
}
