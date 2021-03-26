using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

// Token: 0x02000098 RID: 152
[Serializable]
public class BiomeEnvSetup
{
	// Token: 0x04000934 RID: 2356
	public string m_name = "";

	// Token: 0x04000935 RID: 2357
	public Heightmap.Biome m_biome = Heightmap.Biome.Meadows;

	// Token: 0x04000936 RID: 2358
	public List<EnvEntry> m_environments = new List<EnvEntry>();

	// Token: 0x04000937 RID: 2359
	public string m_musicMorning = "morning";

	// Token: 0x04000938 RID: 2360
	public string m_musicEvening = "evening";

	// Token: 0x04000939 RID: 2361
	[FormerlySerializedAs("m_musicRandomDay")]
	public string m_musicDay = "";

	// Token: 0x0400093A RID: 2362
	[FormerlySerializedAs("m_musicRandomNight")]
	public string m_musicNight = "";
}
