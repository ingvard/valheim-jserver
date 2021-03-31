using System;

// Token: 0x02000099 RID: 153
[Serializable]
public class EnvEntry
{
	// Token: 0x0400093F RID: 2367
	public string m_environment = "";

	// Token: 0x04000940 RID: 2368
	public float m_weight = 1f;

	// Token: 0x04000941 RID: 2369
	[NonSerialized]
	public EnvSetup m_env;
}
