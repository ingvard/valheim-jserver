using System;

// Token: 0x02000099 RID: 153
[Serializable]
public class EnvEntry
{
	// Token: 0x0400093B RID: 2363
	public string m_environment = "";

	// Token: 0x0400093C RID: 2364
	public float m_weight = 1f;

	// Token: 0x0400093D RID: 2365
	[NonSerialized]
	public EnvSetup m_env;
}
