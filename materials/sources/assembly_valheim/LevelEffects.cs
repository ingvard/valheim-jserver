using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200000D RID: 13
public class LevelEffects : MonoBehaviour
{
	// Token: 0x06000153 RID: 339 RVA: 0x0000A630 File Offset: 0x00008830
	private void Start()
	{
		this.m_character = base.GetComponentInParent<Character>();
		Character character = this.m_character;
		character.m_onLevelSet = (Action<int>)Delegate.Combine(character.m_onLevelSet, new Action<int>(this.OnLevelSet));
		this.SetupLevelVisualization(this.m_character.GetLevel());
	}

	// Token: 0x06000154 RID: 340 RVA: 0x0000A681 File Offset: 0x00008881
	private void OnLevelSet(int level)
	{
		this.SetupLevelVisualization(level);
	}

	// Token: 0x06000155 RID: 341 RVA: 0x0000A68C File Offset: 0x0000888C
	private void SetupLevelVisualization(int level)
	{
		if (level <= 1)
		{
			return;
		}
		if (this.m_levelSetups.Count >= level - 1)
		{
			LevelEffects.LevelSetup levelSetup = this.m_levelSetups[level - 2];
			base.transform.localScale = new Vector3(levelSetup.m_scale, levelSetup.m_scale, levelSetup.m_scale);
			if (this.m_mainRender)
			{
				string key = this.m_character.m_name + level.ToString();
				Material material;
				if (LevelEffects.m_materials.TryGetValue(key, out material))
				{
					Material[] sharedMaterials = this.m_mainRender.sharedMaterials;
					sharedMaterials[0] = material;
					this.m_mainRender.sharedMaterials = sharedMaterials;
				}
				else
				{
					Material[] sharedMaterials2 = this.m_mainRender.sharedMaterials;
					sharedMaterials2[0] = new Material(sharedMaterials2[0]);
					sharedMaterials2[0].SetFloat("_Hue", levelSetup.m_hue);
					sharedMaterials2[0].SetFloat("_Saturation", levelSetup.m_saturation);
					sharedMaterials2[0].SetFloat("_Value", levelSetup.m_value);
					this.m_mainRender.sharedMaterials = sharedMaterials2;
					LevelEffects.m_materials[key] = sharedMaterials2[0];
				}
			}
			if (this.m_baseEnableObject)
			{
				this.m_baseEnableObject.SetActive(false);
			}
			if (levelSetup.m_enableObject)
			{
				levelSetup.m_enableObject.SetActive(true);
			}
		}
	}

	// Token: 0x06000156 RID: 342 RVA: 0x0000A7DC File Offset: 0x000089DC
	public void GetColorChanges(out float hue, out float saturation, out float value)
	{
		int level = this.m_character.GetLevel();
		if (level > 1 && this.m_levelSetups.Count >= level - 1)
		{
			LevelEffects.LevelSetup levelSetup = this.m_levelSetups[level - 2];
			hue = levelSetup.m_hue;
			saturation = levelSetup.m_saturation;
			value = levelSetup.m_value;
			return;
		}
		hue = 0f;
		saturation = 0f;
		value = 0f;
	}

	// Token: 0x04000111 RID: 273
	public Renderer m_mainRender;

	// Token: 0x04000112 RID: 274
	public GameObject m_baseEnableObject;

	// Token: 0x04000113 RID: 275
	public List<LevelEffects.LevelSetup> m_levelSetups = new List<LevelEffects.LevelSetup>();

	// Token: 0x04000114 RID: 276
	private static Dictionary<string, Material> m_materials = new Dictionary<string, Material>();

	// Token: 0x04000115 RID: 277
	private Character m_character;

	// Token: 0x02000123 RID: 291
	[Serializable]
	public class LevelSetup
	{
		// Token: 0x04000FDA RID: 4058
		public float m_scale = 1f;

		// Token: 0x04000FDB RID: 4059
		public float m_hue;

		// Token: 0x04000FDC RID: 4060
		public float m_saturation;

		// Token: 0x04000FDD RID: 4061
		public float m_value;

		// Token: 0x04000FDE RID: 4062
		public GameObject m_enableObject;
	}
}
