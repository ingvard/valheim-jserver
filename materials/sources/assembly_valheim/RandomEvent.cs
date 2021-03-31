using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000A9 RID: 169
[Serializable]
public class RandomEvent
{
	// Token: 0x06000BA1 RID: 2977 RVA: 0x00053558 File Offset: 0x00051758
	public RandomEvent Clone()
	{
		RandomEvent randomEvent = base.MemberwiseClone() as RandomEvent;
		randomEvent.m_spawn = new List<SpawnSystem.SpawnData>();
		foreach (SpawnSystem.SpawnData spawnData in this.m_spawn)
		{
			randomEvent.m_spawn.Add(spawnData.Clone());
		}
		return randomEvent;
	}

	// Token: 0x06000BA2 RID: 2978 RVA: 0x000535D0 File Offset: 0x000517D0
	public bool Update(bool server, bool active, bool playerInArea, float dt)
	{
		if (this.m_pauseIfNoPlayerInArea && !playerInArea)
		{
			return false;
		}
		this.m_time += dt;
		return this.m_duration > 0f && this.m_time > this.m_duration;
	}

	// Token: 0x06000BA3 RID: 2979 RVA: 0x0005360C File Offset: 0x0005180C
	public void OnActivate()
	{
		this.m_active = true;
		if (this.m_firstActivation)
		{
			this.m_firstActivation = false;
			if (this.m_startMessage != "")
			{
				MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, this.m_startMessage, 0, null);
			}
		}
	}

	// Token: 0x06000BA4 RID: 2980 RVA: 0x00053649 File Offset: 0x00051849
	public void OnDeactivate(bool end)
	{
		this.m_active = false;
		if (end && this.m_endMessage != "")
		{
			MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, this.m_endMessage, 0, null);
		}
	}

	// Token: 0x06000BA5 RID: 2981 RVA: 0x0005367A File Offset: 0x0005187A
	public string GetHudText()
	{
		return this.m_startMessage;
	}

	// Token: 0x06000BA6 RID: 2982 RVA: 0x000027E0 File Offset: 0x000009E0
	public void OnStart()
	{
	}

	// Token: 0x06000BA7 RID: 2983 RVA: 0x000027E0 File Offset: 0x000009E0
	public void OnStop()
	{
	}

	// Token: 0x06000BA8 RID: 2984 RVA: 0x00053682 File Offset: 0x00051882
	public bool InEventBiome()
	{
		return (EnvMan.instance.GetCurrentBiome() & this.m_biome) > Heightmap.Biome.None;
	}

	// Token: 0x06000BA9 RID: 2985 RVA: 0x00053698 File Offset: 0x00051898
	public float GetTime()
	{
		return this.m_time;
	}

	// Token: 0x04000ACE RID: 2766
	public string m_name = "";

	// Token: 0x04000ACF RID: 2767
	public bool m_enabled = true;

	// Token: 0x04000AD0 RID: 2768
	public bool m_random = true;

	// Token: 0x04000AD1 RID: 2769
	public float m_duration = 60f;

	// Token: 0x04000AD2 RID: 2770
	public bool m_nearBaseOnly = true;

	// Token: 0x04000AD3 RID: 2771
	public bool m_pauseIfNoPlayerInArea = true;

	// Token: 0x04000AD4 RID: 2772
	[BitMask(typeof(Heightmap.Biome))]
	public Heightmap.Biome m_biome;

	// Token: 0x04000AD5 RID: 2773
	[Header("( Keys required to be TRUE )")]
	public List<string> m_requiredGlobalKeys = new List<string>();

	// Token: 0x04000AD6 RID: 2774
	[Header("( Keys required to be FALSE )")]
	public List<string> m_notRequiredGlobalKeys = new List<string>();

	// Token: 0x04000AD7 RID: 2775
	[Space(20f)]
	public string m_startMessage = "";

	// Token: 0x04000AD8 RID: 2776
	public string m_endMessage = "";

	// Token: 0x04000AD9 RID: 2777
	public string m_forceMusic = "";

	// Token: 0x04000ADA RID: 2778
	public string m_forceEnvironment = "";

	// Token: 0x04000ADB RID: 2779
	public List<SpawnSystem.SpawnData> m_spawn = new List<SpawnSystem.SpawnData>();

	// Token: 0x04000ADC RID: 2780
	private bool m_firstActivation = true;

	// Token: 0x04000ADD RID: 2781
	private bool m_active;

	// Token: 0x04000ADE RID: 2782
	[NonSerialized]
	public float m_time;

	// Token: 0x04000ADF RID: 2783
	[NonSerialized]
	public Vector3 m_pos = Vector3.zero;
}
