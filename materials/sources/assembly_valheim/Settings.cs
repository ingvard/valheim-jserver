using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x0200005D RID: 93
public class Settings : MonoBehaviour
{
	// Token: 0x1700000F RID: 15
	// (get) Token: 0x060005FF RID: 1535 RVA: 0x00033239 File Offset: 0x00031439
	public static Settings instance
	{
		get
		{
			return Settings.m_instance;
		}
	}

	// Token: 0x06000600 RID: 1536 RVA: 0x00033240 File Offset: 0x00031440
	private void Awake()
	{
		Settings.m_instance = this;
		this.m_bindDialog.SetActive(false);
		this.m_resDialog.SetActive(false);
		this.m_resSwitchDialog.SetActive(false);
		this.m_resListBaseSize = this.m_resListRoot.rect.height;
		this.LoadSettings();
		this.SetupKeys();
	}

	// Token: 0x06000601 RID: 1537 RVA: 0x0003329C File Offset: 0x0003149C
	private void OnDestroy()
	{
		Settings.m_instance = null;
	}

	// Token: 0x06000602 RID: 1538 RVA: 0x000332A4 File Offset: 0x000314A4
	private void Update()
	{
		if (this.m_bindDialog.activeSelf)
		{
			this.UpdateBinding();
			return;
		}
		this.UpdateResSwitch(Time.deltaTime);
		AudioListener.volume = this.m_volumeSlider.value;
		MusicMan.m_masterMusicVolume = this.m_musicVolumeSlider.value;
		AudioMan.SetSFXVolume(this.m_sfxVolumeSlider.value);
		this.SetQualityText(this.m_shadowQualityText, (int)this.m_shadowQuality.value);
		this.SetQualityText(this.m_lodText, (int)this.m_lod.value);
		this.SetQualityText(this.m_lightsText, (int)this.m_lights.value);
		this.SetQualityText(this.m_vegetationText, (int)this.m_vegetation.value);
		this.m_resButtonText.text = string.Concat(new object[]
		{
			this.m_selectedRes.width,
			"x",
			this.m_selectedRes.height,
			"  ",
			this.m_selectedRes.refreshRate,
			"hz"
		});
		this.m_guiScaleText.text = this.m_guiScaleSlider.value.ToString() + "%";
		GuiScaler.SetScale(this.m_guiScaleSlider.value / 100f);
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			this.OnBack();
		}
	}

	// Token: 0x06000603 RID: 1539 RVA: 0x00033414 File Offset: 0x00031614
	private void SetQualityText(Text text, int level)
	{
		switch (level)
		{
		case 0:
			text.text = Localization.instance.Localize("[$settings_low]");
			return;
		case 1:
			text.text = Localization.instance.Localize("[$settings_medium]");
			return;
		case 2:
			text.text = Localization.instance.Localize("[$settings_high]");
			return;
		case 3:
			text.text = Localization.instance.Localize("[$settings_veryhigh]");
			return;
		default:
			return;
		}
	}

	// Token: 0x06000604 RID: 1540 RVA: 0x0003348F File Offset: 0x0003168F
	public void OnBack()
	{
		this.RevertMode();
		this.LoadSettings();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	// Token: 0x06000605 RID: 1541 RVA: 0x000334A8 File Offset: 0x000316A8
	public void OnOk()
	{
		this.SaveSettings();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	// Token: 0x06000606 RID: 1542 RVA: 0x000334BC File Offset: 0x000316BC
	private void SaveSettings()
	{
		PlayerPrefs.SetFloat("MasterVolume", this.m_volumeSlider.value);
		PlayerPrefs.SetFloat("MouseSensitivity", this.m_sensitivitySlider.value);
		PlayerPrefs.SetFloat("MusicVolume", this.m_musicVolumeSlider.value);
		PlayerPrefs.SetFloat("SfxVolume", this.m_sfxVolumeSlider.value);
		PlayerPrefs.SetInt("ContinousMusic", this.m_continousMusic.isOn ? 1 : 0);
		PlayerPrefs.SetInt("InvertMouse", this.m_invertMouse.isOn ? 1 : 0);
		PlayerPrefs.SetFloat("GuiScale", this.m_guiScaleSlider.value / 100f);
		PlayerPrefs.SetInt("CameraShake", this.m_cameraShake.isOn ? 1 : 0);
		PlayerPrefs.SetInt("ShipCameraTilt", this.m_shipCameraTilt.isOn ? 1 : 0);
		PlayerPrefs.SetInt("QuickPieceSelect", this.m_quickPieceSelect.isOn ? 1 : 0);
		PlayerPrefs.SetInt("KeyHints", this.m_showKeyHints.isOn ? 1 : 0);
		PlayerPrefs.SetInt("DOF", this.m_dofToggle.isOn ? 1 : 0);
		PlayerPrefs.SetInt("VSync", this.m_vsyncToggle.isOn ? 1 : 0);
		PlayerPrefs.SetInt("Bloom", this.m_bloomToggle.isOn ? 1 : 0);
		PlayerPrefs.SetInt("SSAO", this.m_ssaoToggle.isOn ? 1 : 0);
		PlayerPrefs.SetInt("SunShafts", this.m_sunshaftsToggle.isOn ? 1 : 0);
		PlayerPrefs.SetInt("AntiAliasing", this.m_aaToggle.isOn ? 1 : 0);
		PlayerPrefs.SetInt("ChromaticAberration", this.m_caToggle.isOn ? 1 : 0);
		PlayerPrefs.SetInt("MotionBlur", this.m_motionblurToggle.isOn ? 1 : 0);
		PlayerPrefs.SetInt("SoftPart", this.m_softPartToggle.isOn ? 1 : 0);
		PlayerPrefs.SetInt("Tesselation", this.m_tesselationToggle.isOn ? 1 : 0);
		PlayerPrefs.SetInt("ShadowQuality", (int)this.m_shadowQuality.value);
		PlayerPrefs.SetInt("LodBias", (int)this.m_lod.value);
		PlayerPrefs.SetInt("Lights", (int)this.m_lights.value);
		PlayerPrefs.SetInt("ClutterQuality", (int)this.m_vegetation.value);
		ZInput.SetGamepadEnabled(this.m_gamepadEnabled.isOn);
		ZInput.instance.Save();
		if (GameCamera.instance)
		{
			GameCamera.instance.ApplySettings();
		}
		if (CameraEffects.instance)
		{
			CameraEffects.instance.ApplySettings();
		}
		if (ClutterSystem.instance)
		{
			ClutterSystem.instance.ApplySettings();
		}
		if (MusicMan.instance)
		{
			MusicMan.instance.ApplySettings();
		}
		if (GameCamera.instance)
		{
			GameCamera.instance.ApplySettings();
		}
		if (KeyHints.instance)
		{
			KeyHints.instance.ApplySettings();
		}
		Settings.ApplyQualitySettings();
		this.ApplyMode();
		PlayerController.m_mouseSens = this.m_sensitivitySlider.value;
		PlayerController.m_invertMouse = this.m_invertMouse.isOn;
		Localization.instance.SetLanguage(this.m_languageKey);
		GuiScaler.LoadGuiScale();
		PlayerPrefs.Save();
	}

	// Token: 0x06000607 RID: 1543 RVA: 0x00033823 File Offset: 0x00031A23
	public static void ApplyStartupSettings()
	{
		QualitySettings.vSyncCount = ((PlayerPrefs.GetInt("VSync", 0) == 1) ? 1 : 0);
		Settings.ApplyQualitySettings();
	}

	// Token: 0x06000608 RID: 1544 RVA: 0x00033848 File Offset: 0x00031A48
	private static void ApplyQualitySettings()
	{
		QualitySettings.softParticles = (PlayerPrefs.GetInt("SoftPart", 1) == 1);
		if (PlayerPrefs.GetInt("Tesselation", 1) == 1)
		{
			Shader.EnableKeyword("TESSELATION_ON");
		}
		else
		{
			Shader.DisableKeyword("TESSELATION_ON");
		}
		switch (PlayerPrefs.GetInt("LodBias", 2))
		{
		case 0:
			QualitySettings.lodBias = 1f;
			break;
		case 1:
			QualitySettings.lodBias = 1.5f;
			break;
		case 2:
			QualitySettings.lodBias = 2f;
			break;
		case 3:
			QualitySettings.lodBias = 5f;
			break;
		}
		switch (PlayerPrefs.GetInt("Lights", 2))
		{
		case 0:
			QualitySettings.pixelLightCount = 2;
			break;
		case 1:
			QualitySettings.pixelLightCount = 4;
			break;
		case 2:
			QualitySettings.pixelLightCount = 8;
			break;
		}
		Settings.ApplyShadowQuality();
	}

	// Token: 0x06000609 RID: 1545 RVA: 0x00033920 File Offset: 0x00031B20
	private static void ApplyShadowQuality()
	{
		switch (PlayerPrefs.GetInt("ShadowQuality", 2))
		{
		case 0:
			QualitySettings.shadowCascades = 2;
			QualitySettings.shadowDistance = 80f;
			QualitySettings.shadowResolution = ShadowResolution.Low;
			return;
		case 1:
			QualitySettings.shadowCascades = 3;
			QualitySettings.shadowDistance = 120f;
			QualitySettings.shadowResolution = ShadowResolution.Medium;
			return;
		case 2:
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowDistance = 150f;
			QualitySettings.shadowResolution = ShadowResolution.High;
			return;
		default:
			return;
		}
	}

	// Token: 0x0600060A RID: 1546 RVA: 0x00033990 File Offset: 0x00031B90
	private void LoadSettings()
	{
		ZInput.instance.Load();
		AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", AudioListener.volume);
		MusicMan.m_masterMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
		AudioMan.SetSFXVolume(PlayerPrefs.GetFloat("SfxVolume", 1f));
		this.m_continousMusic.isOn = (PlayerPrefs.GetInt("ContinousMusic", 1) == 1);
		PlayerController.m_mouseSens = PlayerPrefs.GetFloat("MouseSensitivity", PlayerController.m_mouseSens);
		PlayerController.m_invertMouse = (PlayerPrefs.GetInt("InvertMouse", 0) == 1);
		float @float = PlayerPrefs.GetFloat("GuiScale", 1f);
		this.m_volumeSlider.value = AudioListener.volume;
		this.m_sensitivitySlider.value = PlayerController.m_mouseSens;
		this.m_sfxVolumeSlider.value = AudioMan.GetSFXVolume();
		this.m_musicVolumeSlider.value = MusicMan.m_masterMusicVolume;
		this.m_guiScaleSlider.value = @float * 100f;
		this.m_invertMouse.isOn = PlayerController.m_invertMouse;
		this.m_gamepadEnabled.isOn = ZInput.IsGamepadEnabled();
		this.m_languageKey = Localization.instance.GetSelectedLanguage();
		this.m_language.text = Localization.instance.Localize("$language_" + this.m_languageKey.ToLower());
		this.m_cameraShake.isOn = (PlayerPrefs.GetInt("CameraShake", 1) == 1);
		this.m_shipCameraTilt.isOn = (PlayerPrefs.GetInt("ShipCameraTilt", 1) == 1);
		this.m_quickPieceSelect.isOn = (PlayerPrefs.GetInt("QuickPieceSelect", 0) == 1);
		this.m_showKeyHints.isOn = (PlayerPrefs.GetInt("KeyHints", 1) == 1);
		this.m_dofToggle.isOn = (PlayerPrefs.GetInt("DOF", 1) == 1);
		this.m_vsyncToggle.isOn = (PlayerPrefs.GetInt("VSync", 0) == 1);
		this.m_bloomToggle.isOn = (PlayerPrefs.GetInt("Bloom", 1) == 1);
		this.m_ssaoToggle.isOn = (PlayerPrefs.GetInt("SSAO", 1) == 1);
		this.m_sunshaftsToggle.isOn = (PlayerPrefs.GetInt("SunShafts", 1) == 1);
		this.m_aaToggle.isOn = (PlayerPrefs.GetInt("AntiAliasing", 1) == 1);
		this.m_caToggle.isOn = (PlayerPrefs.GetInt("ChromaticAberration", 1) == 1);
		this.m_motionblurToggle.isOn = (PlayerPrefs.GetInt("MotionBlur", 1) == 1);
		this.m_softPartToggle.isOn = (PlayerPrefs.GetInt("SoftPart", 1) == 1);
		this.m_tesselationToggle.isOn = (PlayerPrefs.GetInt("Tesselation", 1) == 1);
		this.m_shadowQuality.value = (float)PlayerPrefs.GetInt("ShadowQuality", 2);
		this.m_lod.value = (float)PlayerPrefs.GetInt("LodBias", 2);
		this.m_lights.value = (float)PlayerPrefs.GetInt("Lights", 2);
		this.m_vegetation.value = (float)PlayerPrefs.GetInt("ClutterQuality", 2);
		this.m_fullscreenToggle.isOn = Screen.fullScreen;
		this.m_oldFullscreen = this.m_fullscreenToggle.isOn;
		this.m_oldRes = Screen.currentResolution;
		this.m_oldRes.width = Screen.width;
		this.m_oldRes.height = Screen.height;
		this.m_selectedRes = this.m_oldRes;
		ZLog.Log(string.Concat(new object[]
		{
			"Current res ",
			Screen.currentResolution.width,
			"x",
			Screen.currentResolution.height,
			"     ",
			Screen.width,
			"x",
			Screen.height
		}));
	}

	// Token: 0x0600060B RID: 1547 RVA: 0x00033DA8 File Offset: 0x00031FA8
	private void SetupKeys()
	{
		foreach (Settings.KeySetting keySetting in this.m_keys)
		{
			keySetting.m_keyTransform.GetComponentInChildren<Button>().onClick.AddListener(new UnityAction(this.OnKeySet));
		}
		this.UpdateBindings();
	}

	// Token: 0x0600060C RID: 1548 RVA: 0x00033E1C File Offset: 0x0003201C
	private void UpdateBindings()
	{
		foreach (Settings.KeySetting keySetting in this.m_keys)
		{
			keySetting.m_keyTransform.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text = Localization.instance.GetBoundKeyString(keySetting.m_keyName);
		}
	}

	// Token: 0x0600060D RID: 1549 RVA: 0x00033E90 File Offset: 0x00032090
	private void OnKeySet()
	{
		foreach (Settings.KeySetting keySetting in this.m_keys)
		{
			if (keySetting.m_keyTransform.GetComponentInChildren<Button>().gameObject == EventSystem.current.currentSelectedGameObject)
			{
				this.OpenBindDialog(keySetting.m_keyName);
				return;
			}
		}
		ZLog.Log("NOT FOUND");
	}

	// Token: 0x0600060E RID: 1550 RVA: 0x00033F18 File Offset: 0x00032118
	private void OpenBindDialog(string keyName)
	{
		ZLog.Log("BInding key " + keyName);
		ZInput.instance.StartBindKey(keyName);
		this.m_bindDialog.SetActive(true);
	}

	// Token: 0x0600060F RID: 1551 RVA: 0x00033F41 File Offset: 0x00032141
	private void UpdateBinding()
	{
		if (this.m_bindDialog.activeSelf && ZInput.instance.EndBindKey())
		{
			this.m_bindDialog.SetActive(false);
			this.UpdateBindings();
		}
	}

	// Token: 0x06000610 RID: 1552 RVA: 0x00033F6E File Offset: 0x0003216E
	public void ResetBindings()
	{
		ZInput.instance.Reset();
		this.UpdateBindings();
	}

	// Token: 0x06000611 RID: 1553 RVA: 0x00033F80 File Offset: 0x00032180
	public void OnLanguageLeft()
	{
		this.m_languageKey = Localization.instance.GetPrevLanguage(this.m_languageKey);
		this.m_language.text = Localization.instance.Localize("$language_" + this.m_languageKey.ToLower());
	}

	// Token: 0x06000612 RID: 1554 RVA: 0x00033FD0 File Offset: 0x000321D0
	public void OnLanguageRight()
	{
		this.m_languageKey = Localization.instance.GetNextLanguage(this.m_languageKey);
		this.m_language.text = Localization.instance.Localize("$language_" + this.m_languageKey.ToLower());
	}

	// Token: 0x06000613 RID: 1555 RVA: 0x0003401D File Offset: 0x0003221D
	public void OnShowResList()
	{
		this.m_resDialog.SetActive(true);
		this.FillResList();
	}

	// Token: 0x06000614 RID: 1556 RVA: 0x00034034 File Offset: 0x00032234
	private void UpdateValidResolutions()
	{
		Resolution[] array = Screen.resolutions;
		if (array.Length == 0)
		{
			array = new Resolution[]
			{
				this.m_oldRes
			};
		}
		this.m_resolutions.Clear();
		foreach (Resolution item in array)
		{
			if ((item.width >= this.m_minResWidth && item.height >= this.m_minResHeight) || item.width == this.m_oldRes.width || item.height == this.m_oldRes.height)
			{
				this.m_resolutions.Add(item);
			}
		}
		if (this.m_resolutions.Count == 0)
		{
			Resolution item2 = default(Resolution);
			item2.width = 1280;
			item2.height = 720;
			item2.refreshRate = 60;
			this.m_resolutions.Add(item2);
		}
	}

	// Token: 0x06000615 RID: 1557 RVA: 0x00034118 File Offset: 0x00032318
	private void FillResList()
	{
		foreach (GameObject obj in this.m_resObjects)
		{
			UnityEngine.Object.Destroy(obj);
		}
		this.m_resObjects.Clear();
		this.UpdateValidResolutions();
		float num = 0f;
		foreach (Resolution resolution in this.m_resolutions)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_resListElement, this.m_resListRoot.transform);
			gameObject.SetActive(true);
			gameObject.GetComponentInChildren<Button>().onClick.AddListener(new UnityAction(this.OnResClick));
			(gameObject.transform as RectTransform).anchoredPosition = new Vector2(0f, num * -this.m_resListSpace);
			gameObject.GetComponentInChildren<Text>().text = string.Concat(new object[]
			{
				resolution.width,
				"x",
				resolution.height,
				"  ",
				resolution.refreshRate,
				"hz"
			});
			this.m_resObjects.Add(gameObject);
			num += 1f;
		}
		float size = Mathf.Max(this.m_resListBaseSize, num * this.m_resListSpace);
		this.m_resListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
		this.m_resListScroll.value = 1f;
	}

	// Token: 0x06000616 RID: 1558 RVA: 0x000342C4 File Offset: 0x000324C4
	public void OnResCancel()
	{
		this.m_resDialog.SetActive(false);
	}

	// Token: 0x06000617 RID: 1559 RVA: 0x000342D4 File Offset: 0x000324D4
	private void OnResClick()
	{
		this.m_resDialog.SetActive(false);
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		for (int i = 0; i < this.m_resObjects.Count; i++)
		{
			if (currentSelectedGameObject == this.m_resObjects[i])
			{
				this.m_selectedRes = this.m_resolutions[i];
				return;
			}
		}
	}

	// Token: 0x06000618 RID: 1560 RVA: 0x00034335 File Offset: 0x00032535
	public void OnApplyMode()
	{
		this.ApplyMode();
		this.ShowResSwitchCountdown();
	}

	// Token: 0x06000619 RID: 1561 RVA: 0x00034344 File Offset: 0x00032544
	private void ApplyMode()
	{
		if (Screen.width == this.m_selectedRes.width && Screen.height == this.m_selectedRes.height && this.m_fullscreenToggle.isOn == Screen.fullScreen)
		{
			return;
		}
		Screen.SetResolution(this.m_selectedRes.width, this.m_selectedRes.height, this.m_fullscreenToggle.isOn, this.m_selectedRes.refreshRate);
		this.m_modeApplied = true;
	}

	// Token: 0x0600061A RID: 1562 RVA: 0x000343C0 File Offset: 0x000325C0
	private void RevertMode()
	{
		if (!this.m_modeApplied)
		{
			return;
		}
		this.m_modeApplied = false;
		this.m_selectedRes = this.m_oldRes;
		this.m_fullscreenToggle.isOn = this.m_oldFullscreen;
		Screen.SetResolution(this.m_oldRes.width, this.m_oldRes.height, this.m_oldFullscreen, this.m_oldRes.refreshRate);
	}

	// Token: 0x0600061B RID: 1563 RVA: 0x00034426 File Offset: 0x00032626
	private void ShowResSwitchCountdown()
	{
		this.m_resSwitchDialog.SetActive(true);
		this.m_resCountdownTimer = 5f;
	}

	// Token: 0x0600061C RID: 1564 RVA: 0x0003443F File Offset: 0x0003263F
	public void OnResSwitchOK()
	{
		this.m_resSwitchDialog.SetActive(false);
	}

	// Token: 0x0600061D RID: 1565 RVA: 0x00034450 File Offset: 0x00032650
	private void UpdateResSwitch(float dt)
	{
		if (this.m_resSwitchDialog.activeSelf)
		{
			this.m_resCountdownTimer -= dt;
			this.m_resSwitchCountdown.text = Mathf.CeilToInt(this.m_resCountdownTimer).ToString();
			if (this.m_resCountdownTimer <= 0f)
			{
				this.RevertMode();
				this.m_resSwitchDialog.SetActive(false);
			}
		}
	}

	// Token: 0x0600061E RID: 1566 RVA: 0x000344B5 File Offset: 0x000326B5
	public void OnResetTutorial()
	{
		Player.ResetSeenTutorials();
	}

	// Token: 0x040006A9 RID: 1705
	private static Settings m_instance;

	// Token: 0x040006AA RID: 1706
	[Header("Inout")]
	public Slider m_sensitivitySlider;

	// Token: 0x040006AB RID: 1707
	public Toggle m_invertMouse;

	// Token: 0x040006AC RID: 1708
	public Toggle m_gamepadEnabled;

	// Token: 0x040006AD RID: 1709
	public GameObject m_bindDialog;

	// Token: 0x040006AE RID: 1710
	public List<Settings.KeySetting> m_keys = new List<Settings.KeySetting>();

	// Token: 0x040006AF RID: 1711
	[Header("Misc")]
	public Toggle m_cameraShake;

	// Token: 0x040006B0 RID: 1712
	public Toggle m_shipCameraTilt;

	// Token: 0x040006B1 RID: 1713
	public Toggle m_quickPieceSelect;

	// Token: 0x040006B2 RID: 1714
	public Toggle m_showKeyHints;

	// Token: 0x040006B3 RID: 1715
	public Slider m_guiScaleSlider;

	// Token: 0x040006B4 RID: 1716
	public Text m_guiScaleText;

	// Token: 0x040006B5 RID: 1717
	public Text m_language;

	// Token: 0x040006B6 RID: 1718
	public Button m_resetTutorial;

	// Token: 0x040006B7 RID: 1719
	[Header("Audio")]
	public Slider m_volumeSlider;

	// Token: 0x040006B8 RID: 1720
	public Slider m_sfxVolumeSlider;

	// Token: 0x040006B9 RID: 1721
	public Slider m_musicVolumeSlider;

	// Token: 0x040006BA RID: 1722
	public Toggle m_continousMusic;

	// Token: 0x040006BB RID: 1723
	public AudioMixer m_masterMixer;

	// Token: 0x040006BC RID: 1724
	[Header("Graphics")]
	public Toggle m_dofToggle;

	// Token: 0x040006BD RID: 1725
	public Toggle m_vsyncToggle;

	// Token: 0x040006BE RID: 1726
	public Toggle m_bloomToggle;

	// Token: 0x040006BF RID: 1727
	public Toggle m_ssaoToggle;

	// Token: 0x040006C0 RID: 1728
	public Toggle m_sunshaftsToggle;

	// Token: 0x040006C1 RID: 1729
	public Toggle m_aaToggle;

	// Token: 0x040006C2 RID: 1730
	public Toggle m_caToggle;

	// Token: 0x040006C3 RID: 1731
	public Toggle m_motionblurToggle;

	// Token: 0x040006C4 RID: 1732
	public Toggle m_tesselationToggle;

	// Token: 0x040006C5 RID: 1733
	public Toggle m_softPartToggle;

	// Token: 0x040006C6 RID: 1734
	public Toggle m_fullscreenToggle;

	// Token: 0x040006C7 RID: 1735
	public Slider m_shadowQuality;

	// Token: 0x040006C8 RID: 1736
	public Text m_shadowQualityText;

	// Token: 0x040006C9 RID: 1737
	public Slider m_lod;

	// Token: 0x040006CA RID: 1738
	public Text m_lodText;

	// Token: 0x040006CB RID: 1739
	public Slider m_lights;

	// Token: 0x040006CC RID: 1740
	public Text m_lightsText;

	// Token: 0x040006CD RID: 1741
	public Slider m_vegetation;

	// Token: 0x040006CE RID: 1742
	public Text m_vegetationText;

	// Token: 0x040006CF RID: 1743
	public Text m_resButtonText;

	// Token: 0x040006D0 RID: 1744
	public GameObject m_resDialog;

	// Token: 0x040006D1 RID: 1745
	public GameObject m_resListElement;

	// Token: 0x040006D2 RID: 1746
	public RectTransform m_resListRoot;

	// Token: 0x040006D3 RID: 1747
	public Scrollbar m_resListScroll;

	// Token: 0x040006D4 RID: 1748
	public float m_resListSpace = 20f;

	// Token: 0x040006D5 RID: 1749
	public GameObject m_resSwitchDialog;

	// Token: 0x040006D6 RID: 1750
	public Text m_resSwitchCountdown;

	// Token: 0x040006D7 RID: 1751
	public int m_minResWidth = 1280;

	// Token: 0x040006D8 RID: 1752
	public int m_minResHeight = 720;

	// Token: 0x040006D9 RID: 1753
	private string m_languageKey = "";

	// Token: 0x040006DA RID: 1754
	private bool m_oldFullscreen;

	// Token: 0x040006DB RID: 1755
	private Resolution m_oldRes;

	// Token: 0x040006DC RID: 1756
	private Resolution m_selectedRes;

	// Token: 0x040006DD RID: 1757
	private List<GameObject> m_resObjects = new List<GameObject>();

	// Token: 0x040006DE RID: 1758
	private List<Resolution> m_resolutions = new List<Resolution>();

	// Token: 0x040006DF RID: 1759
	private float m_resListBaseSize;

	// Token: 0x040006E0 RID: 1760
	private bool m_modeApplied;

	// Token: 0x040006E1 RID: 1761
	private float m_resCountdownTimer = 1f;

	// Token: 0x0200015A RID: 346
	[Serializable]
	public class KeySetting
	{
		// Token: 0x04001133 RID: 4403
		public string m_keyName = "";

		// Token: 0x04001134 RID: 4404
		public RectTransform m_keyTransform;
	}
}
