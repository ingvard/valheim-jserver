﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000050 RID: 80
public class EnemyHud : MonoBehaviour
{
	// Token: 0x17000008 RID: 8
	// (get) Token: 0x060004DF RID: 1247 RVA: 0x000285DA File Offset: 0x000267DA
	public static EnemyHud instance
	{
		get
		{
			return EnemyHud.m_instance;
		}
	}

	// Token: 0x060004E0 RID: 1248 RVA: 0x000285E1 File Offset: 0x000267E1
	private void Awake()
	{
		EnemyHud.m_instance = this;
		this.m_baseHud.SetActive(false);
		this.m_baseHudBoss.SetActive(false);
		this.m_baseHudPlayer.SetActive(false);
	}

	// Token: 0x060004E1 RID: 1249 RVA: 0x0002860D File Offset: 0x0002680D
	private void OnDestroy()
	{
		EnemyHud.m_instance = null;
	}

	// Token: 0x060004E2 RID: 1250 RVA: 0x00028618 File Offset: 0x00026818
	private void LateUpdate()
	{
		this.m_hudRoot.SetActive(!Hud.IsUserHidden());
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer != null)
		{
			this.m_refPoint = localPlayer.transform.position;
		}
		foreach (Character character in Character.GetAllCharacters())
		{
			if (!(character == localPlayer) && this.TestShow(character))
			{
				this.ShowHud(character);
			}
		}
		this.UpdateHuds(localPlayer, Time.deltaTime);
	}

	// Token: 0x060004E3 RID: 1251 RVA: 0x000286BC File Offset: 0x000268BC
	private bool TestShow(Character c)
	{
		float num = Vector3.SqrMagnitude(c.transform.position - this.m_refPoint);
		if (c.IsBoss() && num < this.m_maxShowDistanceBoss * this.m_maxShowDistanceBoss)
		{
			if (num < this.m_maxShowDistanceBoss * this.m_maxShowDistanceBoss && c.GetComponent<BaseAI>().IsAlerted())
			{
				return true;
			}
		}
		else if (num < this.m_maxShowDistance * this.m_maxShowDistance)
		{
			return !c.IsPlayer() || !c.IsCrouching();
		}
		return false;
	}

	// Token: 0x060004E4 RID: 1252 RVA: 0x00028744 File Offset: 0x00026944
	private void ShowHud(Character c)
	{
		EnemyHud.HudData hudData;
		if (this.m_huds.TryGetValue(c, out hudData))
		{
			return;
		}
		GameObject original;
		if (c.IsPlayer())
		{
			original = this.m_baseHudPlayer;
		}
		else if (c.IsBoss())
		{
			original = this.m_baseHudBoss;
		}
		else
		{
			original = this.m_baseHud;
		}
		hudData = new EnemyHud.HudData();
		hudData.m_character = c;
		hudData.m_ai = c.GetComponent<BaseAI>();
		hudData.m_gui = UnityEngine.Object.Instantiate<GameObject>(original, this.m_hudRoot.transform);
		hudData.m_gui.SetActive(true);
		hudData.m_healthRoot = hudData.m_gui.transform.Find("Health").gameObject;
		hudData.m_healthFast = hudData.m_healthRoot.transform.Find("health_fast").GetComponent<GuiBar>();
		hudData.m_healthSlow = hudData.m_healthRoot.transform.Find("health_slow").GetComponent<GuiBar>();
		hudData.m_level2 = (hudData.m_gui.transform.Find("level_2") as RectTransform);
		hudData.m_level3 = (hudData.m_gui.transform.Find("level_3") as RectTransform);
		hudData.m_alerted = (hudData.m_gui.transform.Find("Alerted") as RectTransform);
		hudData.m_aware = (hudData.m_gui.transform.Find("Aware") as RectTransform);
		hudData.m_name = hudData.m_gui.transform.Find("Name").GetComponent<Text>();
		hudData.m_name.text = Localization.instance.Localize(c.GetHoverName());
		this.m_huds.Add(c, hudData);
	}

	// Token: 0x060004E5 RID: 1253 RVA: 0x000288F0 File Offset: 0x00026AF0
	private void UpdateHuds(Player player, float dt)
	{
		Camera mainCamera = Utils.GetMainCamera();
		if (!mainCamera)
		{
			return;
		}
		Character y = player ? player.GetHoverCreature() : null;
		if (player)
		{
			player.IsCrouching();
		}
		Character character = null;
		foreach (KeyValuePair<Character, EnemyHud.HudData> keyValuePair in this.m_huds)
		{
			EnemyHud.HudData value = keyValuePair.Value;
			if (!value.m_character || !this.TestShow(value.m_character))
			{
				if (character == null)
				{
					character = value.m_character;
					UnityEngine.Object.Destroy(value.m_gui);
				}
			}
			else
			{
				if (value.m_character == y)
				{
					value.m_hoverTimer = 0f;
				}
				value.m_hoverTimer += dt;
				float healthPercentage = value.m_character.GetHealthPercentage();
				if (value.m_character.IsPlayer() || value.m_character.IsBoss() || value.m_hoverTimer < this.m_hoverShowDuration)
				{
					value.m_gui.SetActive(true);
					int level = value.m_character.GetLevel();
					if (value.m_level2)
					{
						value.m_level2.gameObject.SetActive(level == 2);
					}
					if (value.m_level3)
					{
						value.m_level3.gameObject.SetActive(level == 3);
					}
					if (!value.m_character.IsBoss() && !value.m_character.IsPlayer())
					{
						bool flag = value.m_character.GetBaseAI().HaveTarget();
						bool flag2 = value.m_character.GetBaseAI().IsAlerted();
						value.m_alerted.gameObject.SetActive(flag2);
						value.m_aware.gameObject.SetActive(!flag2 && flag);
					}
				}
				else
				{
					value.m_gui.SetActive(false);
				}
				value.m_healthSlow.SetValue(healthPercentage);
				value.m_healthFast.SetValue(healthPercentage);
				if (!value.m_character.IsBoss() && value.m_gui.activeSelf)
				{
					Vector3 position = Vector3.zero;
					if (value.m_character.IsPlayer())
					{
						position = value.m_character.GetHeadPoint() + Vector3.up * 0.3f;
					}
					else
					{
						position = value.m_character.GetTopPoint();
					}
					Vector3 vector = mainCamera.WorldToScreenPoint(position);
					if (vector.x < 0f || vector.x > (float)Screen.width || vector.y < 0f || vector.y > (float)Screen.height || vector.z > 0f)
					{
						value.m_gui.transform.position = vector;
						value.m_gui.SetActive(true);
					}
					else
					{
						value.m_gui.SetActive(false);
					}
				}
			}
		}
		if (character != null)
		{
			this.m_huds.Remove(character);
		}
	}

	// Token: 0x060004E6 RID: 1254 RVA: 0x00028C30 File Offset: 0x00026E30
	public bool ShowingBossHud()
	{
		foreach (KeyValuePair<Character, EnemyHud.HudData> keyValuePair in this.m_huds)
		{
			if (keyValuePair.Value.m_character && keyValuePair.Value.m_character.IsBoss())
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060004E7 RID: 1255 RVA: 0x00028CAC File Offset: 0x00026EAC
	public Character GetActiveBoss()
	{
		foreach (KeyValuePair<Character, EnemyHud.HudData> keyValuePair in this.m_huds)
		{
			if (keyValuePair.Value.m_character && keyValuePair.Value.m_character.IsBoss())
			{
				return keyValuePair.Value.m_character;
			}
		}
		return null;
	}

	// Token: 0x04000529 RID: 1321
	private static EnemyHud m_instance;

	// Token: 0x0400052A RID: 1322
	public GameObject m_hudRoot;

	// Token: 0x0400052B RID: 1323
	public GameObject m_baseHud;

	// Token: 0x0400052C RID: 1324
	public GameObject m_baseHudBoss;

	// Token: 0x0400052D RID: 1325
	public GameObject m_baseHudPlayer;

	// Token: 0x0400052E RID: 1326
	public float m_maxShowDistance = 10f;

	// Token: 0x0400052F RID: 1327
	public float m_maxShowDistanceBoss = 100f;

	// Token: 0x04000530 RID: 1328
	public float m_hoverShowDuration = 60f;

	// Token: 0x04000531 RID: 1329
	private Vector3 m_refPoint = Vector3.zero;

	// Token: 0x04000532 RID: 1330
	private Dictionary<Character, EnemyHud.HudData> m_huds = new Dictionary<Character, EnemyHud.HudData>();

	// Token: 0x02000148 RID: 328
	private class HudData
	{
		// Token: 0x040010D4 RID: 4308
		public Character m_character;

		// Token: 0x040010D5 RID: 4309
		public BaseAI m_ai;

		// Token: 0x040010D6 RID: 4310
		public GameObject m_gui;

		// Token: 0x040010D7 RID: 4311
		public GameObject m_healthRoot;

		// Token: 0x040010D8 RID: 4312
		public RectTransform m_level2;

		// Token: 0x040010D9 RID: 4313
		public RectTransform m_level3;

		// Token: 0x040010DA RID: 4314
		public RectTransform m_alerted;

		// Token: 0x040010DB RID: 4315
		public RectTransform m_aware;

		// Token: 0x040010DC RID: 4316
		public GuiBar m_healthFast;

		// Token: 0x040010DD RID: 4317
		public GuiBar m_healthSlow;

		// Token: 0x040010DE RID: 4318
		public Text m_name;

		// Token: 0x040010DF RID: 4319
		public float m_hoverTimer = 99999f;
	}
}
