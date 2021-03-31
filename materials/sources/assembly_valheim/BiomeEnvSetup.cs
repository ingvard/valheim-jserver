using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

// Token: 0x02000098 RID: 152
[Serializable]
public class BiomeEnvSetup
{
	// Token: 0x04000938 RID: 2360
	public string m_name = "";

	// Token: 0x04000939 RID: 2361
	public Heightmap.Biome m_biome = Heightmap.Biome.Meadows;

	// Token: 0x0400093A RID: 2362
	public List<EnvEntry> m_environments = new List<EnvEntry>();

	// Token: 0x0400093B RID: 2363
	public string m_musicMorning = "morning";

	// Token: 0x0400093C RID: 2364
	public string m_musicEvening = "evening";

	// Token: 0x0400093D RID: 2365
	[FormerlySerializedAs("m_musicRandomDay")]
	public string m_musicDay = "";

	// Token: 0x0400093E RID: 2366
	[FormerlySerializedAs("m_musicRandomNight")]
	public string m_musicNight = "";
}
