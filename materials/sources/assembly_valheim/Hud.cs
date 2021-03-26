using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000053 RID: 83
public class Hud : MonoBehaviour
{
	// Token: 0x060004F4 RID: 1268 RVA: 0x0002944D File Offset: 0x0002764D
	private void OnDestroy()
	{
		Hud.m_instance = null;
	}

	// Token: 0x17000009 RID: 9
	// (get) Token: 0x060004F5 RID: 1269 RVA: 0x00029455 File Offset: 0x00027655
	public static Hud instance
	{
		get
		{
			return Hud.m_instance;
		}
	}

	// Token: 0x060004F6 RID: 1270 RVA: 0x0002945C File Offset: 0x0002765C
	private void Awake()
	{
		Hud.m_instance = this;
		this.m_pieceSelectionWindow.SetActive(false);
		this.m_loadingScreen.gameObject.SetActive(false);
		this.m_statusEffectTemplate.gameObject.SetActive(false);
		this.m_eventBar.SetActive(false);
		this.m_gpRoot.gameObject.SetActive(false);
		this.m_betaText.SetActive(false);
		UIInputHandler closePieceSelectionButton = this.m_closePieceSelectionButton;
		closePieceSelectionButton.m_onLeftClick = (Action<UIInputHandler>)Delegate.Combine(closePieceSelectionButton.m_onLeftClick, new Action<UIInputHandler>(this.OnClosePieceSelection));
		UIInputHandler closePieceSelectionButton2 = this.m_closePieceSelectionButton;
		closePieceSelectionButton2.m_onRightClick = (Action<UIInputHandler>)Delegate.Combine(closePieceSelectionButton2.m_onRightClick, new Action<UIInputHandler>(this.OnClosePieceSelection));
		if (SteamManager.APP_ID == 1223920U)
		{
			this.m_betaText.SetActive(true);
		}
		foreach (GameObject gameObject in this.m_pieceCategoryTabs)
		{
			this.m_buildCategoryNames.Add(gameObject.transform.Find("Text").GetComponent<Text>().text);
			UIInputHandler component = gameObject.GetComponent<UIInputHandler>();
			component.m_onLeftDown = (Action<UIInputHandler>)Delegate.Combine(component.m_onLeftDown, new Action<UIInputHandler>(this.OnLeftClickCategory));
		}
	}

	// Token: 0x060004F7 RID: 1271 RVA: 0x00029594 File Offset: 0x00027794
	private void SetVisible(bool visible)
	{
		if (visible == this.IsVisible())
		{
			return;
		}
		if (visible)
		{
			this.m_rootObject.transform.localPosition = new Vector3(0f, 0f, 0f);
			return;
		}
		this.m_rootObject.transform.localPosition = new Vector3(10000f, 0f, 0f);
	}

	// Token: 0x060004F8 RID: 1272 RVA: 0x000295F7 File Offset: 0x000277F7
	private bool IsVisible()
	{
		return this.m_rootObject.transform.localPosition.x < 1000f;
	}

	// Token: 0x060004F9 RID: 1273 RVA: 0x00029618 File Offset: 0x00027818
	private void Update()
	{
		this.m_saveIcon.SetActive(ZNet.instance != null && ZNet.instance.IsSaving());
		this.m_badConnectionIcon.SetActive(ZNet.instance != null && ZNet.instance.HasBadConnection() && Mathf.Sin(Time.time * 10f) > 0f);
		Player localPlayer = Player.m_localPlayer;
		this.UpdateDamageFlash(Time.deltaTime);
		if (localPlayer)
		{
			if (Input.GetKeyDown(KeyCode.F3) && Input.GetKey(KeyCode.LeftControl))
			{
				this.m_userHidden = !this.m_userHidden;
			}
			this.SetVisible(!this.m_userHidden && !localPlayer.InCutscene());
			this.UpdateBuild(localPlayer, false);
			this.m_tempStatusEffects.Clear();
			localPlayer.GetSEMan().GetHUDStatusEffects(this.m_tempStatusEffects);
			this.UpdateStatusEffects(this.m_tempStatusEffects);
			this.UpdateGuardianPower(localPlayer);
			float attackDrawPercentage = localPlayer.GetAttackDrawPercentage();
			this.UpdateFood(localPlayer);
			this.UpdateHealth(localPlayer);
			this.UpdateStamina(localPlayer);
			this.UpdateStealth(localPlayer, attackDrawPercentage);
			this.UpdateCrosshair(localPlayer, attackDrawPercentage);
			this.UpdateEvent(localPlayer);
			this.UpdateActionProgress(localPlayer);
		}
	}

	// Token: 0x060004FA RID: 1274 RVA: 0x00029758 File Offset: 0x00027958
	private void LateUpdate()
	{
		this.UpdateBlackScreen(Player.m_localPlayer, Time.deltaTime);
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer)
		{
			this.UpdateShipHud(localPlayer, Time.deltaTime);
		}
	}

	// Token: 0x060004FB RID: 1275 RVA: 0x0002978F File Offset: 0x0002798F
	private float GetFadeDuration(Player player)
	{
		if (player != null)
		{
			if (player.IsDead())
			{
				return 9.5f;
			}
			if (player.IsSleeping())
			{
				return 3f;
			}
		}
		return 1f;
	}

	// Token: 0x060004FC RID: 1276 RVA: 0x000297BC File Offset: 0x000279BC
	private void UpdateBlackScreen(Player player, float dt)
	{
		if (!(player == null) && !player.IsDead() && !player.IsTeleporting() && !Game.instance.IsShuttingDown() && !player.IsSleeping())
		{
			this.m_haveSetupLoadScreen = false;
			float fadeDuration = this.GetFadeDuration(player);
			float num = this.m_loadingScreen.alpha;
			num = Mathf.MoveTowards(num, 0f, dt / fadeDuration);
			this.m_loadingScreen.alpha = num;
			if (this.m_loadingScreen.alpha <= 0f)
			{
				this.m_loadingScreen.gameObject.SetActive(false);
			}
			return;
		}
		this.m_loadingScreen.gameObject.SetActive(true);
		float num2 = this.m_loadingScreen.alpha;
		float fadeDuration2 = this.GetFadeDuration(player);
		num2 = Mathf.MoveTowards(num2, 1f, dt / fadeDuration2);
		if (Game.instance.IsShuttingDown())
		{
			num2 = 1f;
		}
		this.m_loadingScreen.alpha = num2;
		if (player != null && player.IsSleeping())
		{
			this.m_sleepingProgress.SetActive(true);
			this.m_loadingProgress.SetActive(false);
			this.m_teleportingProgress.SetActive(false);
			return;
		}
		if (player != null && player.ShowTeleportAnimation())
		{
			this.m_loadingProgress.SetActive(false);
			this.m_sleepingProgress.SetActive(false);
			this.m_teleportingProgress.SetActive(true);
			return;
		}
		if (Game.instance && Game.instance.WaitingForRespawn())
		{
			if (!this.m_haveSetupLoadScreen)
			{
				this.m_haveSetupLoadScreen = true;
				if (this.m_useRandomImages)
				{
					int num3 = UnityEngine.Random.Range(0, this.m_loadingImages);
					string text = this.m_loadingImagePath + "loading" + num3.ToString();
					ZLog.Log("Loading image:" + text);
					this.m_loadingImage.sprite = Resources.Load<Sprite>(text);
				}
				string text2 = this.m_loadingTips[UnityEngine.Random.Range(0, this.m_loadingTips.Count)];
				ZLog.Log("tip:" + text2);
				this.m_loadingTip.text = Localization.instance.Localize(text2);
			}
			this.m_loadingProgress.SetActive(true);
			this.m_sleepingProgress.SetActive(false);
			this.m_teleportingProgress.SetActive(false);
			return;
		}
		this.m_loadingProgress.SetActive(false);
		this.m_sleepingProgress.SetActive(false);
		this.m_teleportingProgress.SetActive(false);
	}

	// Token: 0x060004FD RID: 1277 RVA: 0x00029A24 File Offset: 0x00027C24
	private void UpdateShipHud(Player player, float dt)
	{
		Ship controlledShip = player.GetControlledShip();
		if (controlledShip == null)
		{
			this.m_shipHudRoot.gameObject.SetActive(false);
			return;
		}
		Ship.Speed speedSetting = controlledShip.GetSpeedSetting();
		float rudder = controlledShip.GetRudder();
		float rudderValue = controlledShip.GetRudderValue();
		this.m_shipHudRoot.SetActive(true);
		this.m_rudderSlow.SetActive(speedSetting == Ship.Speed.Slow);
		this.m_rudderForward.SetActive(speedSetting == Ship.Speed.Half);
		this.m_rudderFastForward.SetActive(speedSetting == Ship.Speed.Full);
		this.m_rudderBackward.SetActive(speedSetting == Ship.Speed.Back);
		this.m_rudderLeft.SetActive(false);
		this.m_rudderRight.SetActive(false);
		this.m_fullSail.SetActive(speedSetting == Ship.Speed.Full);
		this.m_halfSail.SetActive(speedSetting == Ship.Speed.Half);
		this.m_rudder.SetActive(speedSetting == Ship.Speed.Slow || speedSetting == Ship.Speed.Back || (speedSetting == Ship.Speed.Stop && Mathf.Abs(rudderValue) > 0.2f));
		if ((rudder > 0f && rudderValue < 1f) || (rudder < 0f && rudderValue > -1f))
		{
			this.m_shipRudderIcon.transform.Rotate(new Vector3(0f, 0f, 200f * -rudder * dt));
		}
		if (Mathf.Abs(rudderValue) < 0.02f)
		{
			this.m_shipRudderIndicator.gameObject.SetActive(false);
		}
		else
		{
			this.m_shipRudderIndicator.gameObject.SetActive(true);
			if (rudderValue > 0f)
			{
				this.m_shipRudderIndicator.fillClockwise = true;
				this.m_shipRudderIndicator.fillAmount = rudderValue * 0.25f;
			}
			else
			{
				this.m_shipRudderIndicator.fillClockwise = false;
				this.m_shipRudderIndicator.fillAmount = -rudderValue * 0.25f;
			}
		}
		float shipYawAngle = controlledShip.GetShipYawAngle();
		this.m_shipWindIndicatorRoot.localRotation = Quaternion.Euler(0f, 0f, shipYawAngle);
		float windAngle = controlledShip.GetWindAngle();
		this.m_shipWindIconRoot.localRotation = Quaternion.Euler(0f, 0f, windAngle);
		float windAngleFactor = controlledShip.GetWindAngleFactor();
		this.m_shipWindIcon.color = Color.Lerp(new Color(0.2f, 0.2f, 0.2f, 1f), Color.white, windAngleFactor);
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		this.m_shipControlsRoot.transform.position = mainCamera.WorldToScreenPoint(controlledShip.m_controlGuiPos.position);
	}

	// Token: 0x060004FE RID: 1278 RVA: 0x00029C84 File Offset: 0x00027E84
	private void UpdateActionProgress(Player player)
	{
		string text;
		float value;
		player.GetActionProgress(out text, out value);
		if (!string.IsNullOrEmpty(text))
		{
			this.m_actionBarRoot.SetActive(true);
			this.m_actionProgress.SetValue(value);
			this.m_actionName.text = Localization.instance.Localize(text);
			return;
		}
		this.m_actionBarRoot.SetActive(false);
	}

	// Token: 0x060004FF RID: 1279 RVA: 0x00029CE0 File Offset: 0x00027EE0
	private void UpdateCrosshair(Player player, float bowDrawPercentage)
	{
		GameObject hoverObject = player.GetHoverObject();
		Hoverable hoverable = hoverObject ? hoverObject.GetComponentInParent<Hoverable>() : null;
		if (hoverable != null && !TextViewer.instance.IsVisible())
		{
			this.m_hoverName.text = hoverable.GetHoverText();
			this.m_crosshair.color = ((this.m_hoverName.text.Length > 0) ? Color.yellow : new Color(1f, 1f, 1f, 0.5f));
		}
		else
		{
			this.m_crosshair.color = new Color(1f, 1f, 1f, 0.5f);
			this.m_hoverName.text = "";
		}
		Piece hoveringPiece = player.GetHoveringPiece();
		if (hoveringPiece)
		{
			WearNTear component = hoveringPiece.GetComponent<WearNTear>();
			if (component)
			{
				this.m_pieceHealthRoot.gameObject.SetActive(true);
				this.m_pieceHealthBar.SetValue(component.GetHealthPercentage());
			}
			else
			{
				this.m_pieceHealthRoot.gameObject.SetActive(false);
			}
		}
		else
		{
			this.m_pieceHealthRoot.gameObject.SetActive(false);
		}
		if (bowDrawPercentage > 0f)
		{
			float num = Mathf.Lerp(1f, 0.15f, bowDrawPercentage);
			this.m_crosshairBow.gameObject.SetActive(true);
			this.m_crosshairBow.transform.localScale = new Vector3(num, num, num);
			this.m_crosshairBow.color = Color.Lerp(new Color(1f, 1f, 1f, 0f), Color.yellow, bowDrawPercentage);
			return;
		}
		this.m_crosshairBow.gameObject.SetActive(false);
	}

	// Token: 0x06000500 RID: 1280 RVA: 0x00029E87 File Offset: 0x00028087
	private void FixedUpdate()
	{
		this.UpdatePieceBar(Time.fixedDeltaTime);
	}

	// Token: 0x06000501 RID: 1281 RVA: 0x00029E94 File Offset: 0x00028094
	private void UpdateStealth(Player player, float bowDrawPercentage)
	{
		float stealthFactor = player.GetStealthFactor();
		if ((player.IsCrouching() || stealthFactor < 1f) && bowDrawPercentage == 0f)
		{
			if (player.IsSensed())
			{
				this.m_targetedAlert.SetActive(true);
				this.m_targeted.SetActive(false);
				this.m_hidden.SetActive(false);
			}
			else if (player.IsTargeted())
			{
				this.m_targetedAlert.SetActive(false);
				this.m_targeted.SetActive(true);
				this.m_hidden.SetActive(false);
			}
			else
			{
				this.m_targetedAlert.SetActive(false);
				this.m_targeted.SetActive(false);
				this.m_hidden.SetActive(true);
			}
			this.m_stealthBar.gameObject.SetActive(true);
			this.m_stealthBar.SetValue(stealthFactor);
			return;
		}
		this.m_targetedAlert.SetActive(false);
		this.m_hidden.SetActive(false);
		this.m_targeted.SetActive(false);
		this.m_stealthBar.gameObject.SetActive(false);
	}

	// Token: 0x06000502 RID: 1282 RVA: 0x00029F9C File Offset: 0x0002819C
	private void SetHealthBarSize(float size)
	{
		size = Mathf.Ceil(size);
		float size2 = Mathf.Max(size + 56f, 138f);
		this.m_healthPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size2);
		this.m_healthBarRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
		this.m_healthBarSlow.SetWidth(size);
		this.m_healthBarFast.SetWidth(size);
	}

	// Token: 0x06000503 RID: 1283 RVA: 0x00029FF5 File Offset: 0x000281F5
	private void SetStaminaBarSize(float size)
	{
		this.m_staminaBar2Root.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size + this.m_staminaBarBorderBuffer);
		this.m_staminaBar2Slow.SetWidth(size);
		this.m_staminaBar2Fast.SetWidth(size);
	}

	// Token: 0x06000504 RID: 1284 RVA: 0x0002A024 File Offset: 0x00028224
	private void UpdateFood(Player player)
	{
		List<Player.Food> foods = player.GetFoods();
		float num = player.GetBaseFoodHP() / 25f * 32f;
		this.m_foodBaseBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num);
		float num2 = num;
		for (int i = 0; i < this.m_foodBars.Length; i++)
		{
			Image image = this.m_foodBars[i];
			Image image2 = this.m_foodIcons[i];
			if (i < foods.Count)
			{
				image.gameObject.SetActive(true);
				Player.Food food = foods[i];
				float num3 = food.m_health / 25f * 32f;
				image.color = food.m_item.m_shared.m_foodColor;
				image.rectTransform.anchoredPosition = new Vector2(num2, 0f);
				image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Ceil(num3));
				num2 += num3;
				image2.gameObject.SetActive(true);
				image2.sprite = food.m_item.GetIcon();
				if (food.CanEatAgain())
				{
					image2.color = new Color(1f, 1f, 1f, 0.6f + Mathf.Sin(Time.time * 10f) * 0.4f);
				}
				else
				{
					image2.color = Color.white;
				}
			}
			else
			{
				image.gameObject.SetActive(false);
				image2.gameObject.SetActive(false);
			}
		}
		float size = Mathf.Ceil(player.GetMaxHealth() / 25f * 32f);
		this.m_foodBarRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
	}

	// Token: 0x06000505 RID: 1285 RVA: 0x0002A1C0 File Offset: 0x000283C0
	private void UpdateHealth(Player player)
	{
		float maxHealth = player.GetMaxHealth();
		this.SetHealthBarSize(maxHealth / 25f * 32f);
		float health = player.GetHealth();
		this.m_healthBarFast.SetMaxValue(maxHealth);
		this.m_healthBarFast.SetValue(health);
		this.m_healthBarSlow.SetMaxValue(maxHealth);
		this.m_healthBarSlow.SetValue(health);
		string text = Mathf.CeilToInt(player.GetHealth()).ToString();
		string text2 = Mathf.CeilToInt(player.GetMaxHealth()).ToString();
		this.m_healthText.text = text.ToString();
		this.m_healthMaxText.text = text2.ToString();
	}

	// Token: 0x06000506 RID: 1286 RVA: 0x0002A26C File Offset: 0x0002846C
	private void UpdateStamina(Player player)
	{
		float stamina = player.GetStamina();
		float maxStamina = player.GetMaxStamina();
		this.m_staminaBar.SetActive(false);
		this.m_staminaAnimator.SetBool("Visible", stamina < maxStamina);
		this.SetStaminaBarSize(player.GetMaxStamina() / 25f * 32f);
		RectTransform rectTransform = this.m_staminaBar2Root.transform as RectTransform;
		if (this.m_buildHud.activeSelf || this.m_shipHudRoot.activeSelf)
		{
			rectTransform.anchoredPosition = new Vector2(0f, 190f);
		}
		else
		{
			rectTransform.anchoredPosition = new Vector2(0f, 130f);
		}
		this.m_staminaBar2Slow.SetValue(stamina / maxStamina);
		this.m_staminaBar2Fast.SetValue(stamina / maxStamina);
	}

	// Token: 0x06000507 RID: 1287 RVA: 0x0002A334 File Offset: 0x00028534
	public void DamageFlash()
	{
		Color color = this.m_damageScreen.color;
		color.a = 1f;
		this.m_damageScreen.color = color;
		this.m_damageScreen.gameObject.SetActive(true);
	}

	// Token: 0x06000508 RID: 1288 RVA: 0x0002A378 File Offset: 0x00028578
	private void UpdateDamageFlash(float dt)
	{
		Color color = this.m_damageScreen.color;
		color.a = Mathf.MoveTowards(color.a, 0f, dt * 4f);
		this.m_damageScreen.color = color;
		if (color.a <= 0f)
		{
			this.m_damageScreen.gameObject.SetActive(false);
		}
	}

	// Token: 0x06000509 RID: 1289 RVA: 0x0002A3DC File Offset: 0x000285DC
	private void UpdatePieceList(Player player, Vector2Int selectedNr, Piece.PieceCategory category, bool updateAllBuildStatuses)
	{
		List<Piece> buildPieces = player.GetBuildPieces();
		int num = 10;
		int num2 = 5;
		if (buildPieces.Count <= 1)
		{
			num = 1;
			num2 = 1;
		}
		if (this.m_pieceIcons.Count != num * num2)
		{
			foreach (Hud.PieceIconData pieceIconData in this.m_pieceIcons)
			{
				UnityEngine.Object.Destroy(pieceIconData.m_go);
			}
			this.m_pieceIcons.Clear();
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num; j++)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_pieceIconPrefab, this.m_pieceListRoot);
					(gameObject.transform as RectTransform).anchoredPosition = new Vector2((float)j * this.m_pieceIconSpacing, (float)(-(float)i) * this.m_pieceIconSpacing);
					Hud.PieceIconData pieceIconData2 = new Hud.PieceIconData();
					pieceIconData2.m_go = gameObject;
					pieceIconData2.m_tooltip = gameObject.GetComponent<UITooltip>();
					pieceIconData2.m_icon = gameObject.transform.Find("icon").GetComponent<Image>();
					pieceIconData2.m_marker = gameObject.transform.Find("selected").gameObject;
					pieceIconData2.m_upgrade = gameObject.transform.Find("upgrade").gameObject;
					pieceIconData2.m_icon.color = new Color(1f, 0f, 1f, 0f);
					UIInputHandler component = gameObject.GetComponent<UIInputHandler>();
					component.m_onLeftDown = (Action<UIInputHandler>)Delegate.Combine(component.m_onLeftDown, new Action<UIInputHandler>(this.OnLeftClickPiece));
					component.m_onRightDown = (Action<UIInputHandler>)Delegate.Combine(component.m_onRightDown, new Action<UIInputHandler>(this.OnRightClickPiece));
					component.m_onPointerEnter = (Action<UIInputHandler>)Delegate.Combine(component.m_onPointerEnter, new Action<UIInputHandler>(this.OnHoverPiece));
					component.m_onPointerExit = (Action<UIInputHandler>)Delegate.Combine(component.m_onPointerExit, new Action<UIInputHandler>(this.OnHoverPieceExit));
					this.m_pieceIcons.Add(pieceIconData2);
				}
			}
		}
		for (int k = 0; k < num2; k++)
		{
			for (int l = 0; l < num; l++)
			{
				int num3 = k * num + l;
				Hud.PieceIconData pieceIconData3 = this.m_pieceIcons[num3];
				pieceIconData3.m_marker.SetActive(new Vector2Int(l, k) == selectedNr);
				if (num3 < buildPieces.Count)
				{
					Piece piece = buildPieces[num3];
					pieceIconData3.m_icon.sprite = piece.m_icon;
					pieceIconData3.m_icon.enabled = true;
					pieceIconData3.m_tooltip.m_text = piece.m_name;
					pieceIconData3.m_upgrade.SetActive(piece.m_isUpgrade);
				}
				else
				{
					pieceIconData3.m_icon.enabled = false;
					pieceIconData3.m_tooltip.m_text = "";
					pieceIconData3.m_upgrade.SetActive(false);
				}
			}
		}
		this.UpdatePieceBuildStatus(buildPieces, player);
		if (updateAllBuildStatuses)
		{
			this.UpdatePieceBuildStatusAll(buildPieces, player);
		}
		if (this.m_lastPieceCategory != category)
		{
			this.m_lastPieceCategory = category;
			this.m_pieceBarPosX = this.m_pieceBarTargetPosX;
			this.UpdatePieceBuildStatusAll(buildPieces, player);
		}
	}

	// Token: 0x0600050A RID: 1290 RVA: 0x0002A728 File Offset: 0x00028928
	private void OnLeftClickCategory(UIInputHandler ih)
	{
		for (int i = 0; i < this.m_pieceCategoryTabs.Length; i++)
		{
			if (this.m_pieceCategoryTabs[i] == ih.gameObject)
			{
				Player.m_localPlayer.SetBuildCategory(i);
				return;
			}
		}
	}

	// Token: 0x0600050B RID: 1291 RVA: 0x0002A769 File Offset: 0x00028969
	private void OnLeftClickPiece(UIInputHandler ih)
	{
		this.SelectPiece(ih);
		Hud.HidePieceSelection();
	}

	// Token: 0x0600050C RID: 1292 RVA: 0x0002A777 File Offset: 0x00028977
	private void OnRightClickPiece(UIInputHandler ih)
	{
		if (this.IsQuickPieceSelectEnabled())
		{
			this.SelectPiece(ih);
			Hud.HidePieceSelection();
		}
	}

	// Token: 0x0600050D RID: 1293 RVA: 0x0002A790 File Offset: 0x00028990
	private void OnHoverPiece(UIInputHandler ih)
	{
		Vector2Int selectedGrid = this.GetSelectedGrid(ih);
		if (selectedGrid.x != -1)
		{
			this.m_hoveredPiece = Player.m_localPlayer.GetPiece(selectedGrid);
		}
	}

	// Token: 0x0600050E RID: 1294 RVA: 0x0002A7C0 File Offset: 0x000289C0
	private void OnHoverPieceExit(UIInputHandler ih)
	{
		this.m_hoveredPiece = null;
	}

	// Token: 0x0600050F RID: 1295 RVA: 0x0002A7C9 File Offset: 0x000289C9
	public bool IsQuickPieceSelectEnabled()
	{
		return PlayerPrefs.GetInt("QuickPieceSelect", 0) == 1;
	}

	// Token: 0x06000510 RID: 1296 RVA: 0x0002A7DC File Offset: 0x000289DC
	private Vector2Int GetSelectedGrid(UIInputHandler ih)
	{
		int num = 10;
		int num2 = 5;
		for (int i = 0; i < num2; i++)
		{
			for (int j = 0; j < num; j++)
			{
				int index = i * num + j;
				if (this.m_pieceIcons[index].m_go == ih.gameObject)
				{
					return new Vector2Int(j, i);
				}
			}
		}
		return new Vector2Int(-1, -1);
	}

	// Token: 0x06000511 RID: 1297 RVA: 0x0002A83C File Offset: 0x00028A3C
	private void SelectPiece(UIInputHandler ih)
	{
		Vector2Int selectedGrid = this.GetSelectedGrid(ih);
		if (selectedGrid.x != -1)
		{
			Player.m_localPlayer.SetSelectedPiece(selectedGrid);
			this.m_selectItemEffect.Create(base.transform.position, Quaternion.identity, null, 1f);
		}
	}

	// Token: 0x06000512 RID: 1298 RVA: 0x0002A888 File Offset: 0x00028A88
	private void UpdatePieceBuildStatus(List<Piece> pieces, Player player)
	{
		if (this.m_pieceIcons.Count == 0)
		{
			return;
		}
		if (this.m_pieceIconUpdateIndex >= this.m_pieceIcons.Count)
		{
			this.m_pieceIconUpdateIndex = 0;
		}
		Hud.PieceIconData pieceIconData = this.m_pieceIcons[this.m_pieceIconUpdateIndex];
		if (this.m_pieceIconUpdateIndex < pieces.Count)
		{
			Piece piece = pieces[this.m_pieceIconUpdateIndex];
			bool flag = player.HaveRequirements(piece, Player.RequirementMode.CanBuild);
			pieceIconData.m_icon.color = (flag ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 0f, 1f, 0f));
		}
		this.m_pieceIconUpdateIndex++;
	}

	// Token: 0x06000513 RID: 1299 RVA: 0x0002A944 File Offset: 0x00028B44
	private void UpdatePieceBuildStatusAll(List<Piece> pieces, Player player)
	{
		for (int i = 0; i < this.m_pieceIcons.Count; i++)
		{
			Hud.PieceIconData pieceIconData = this.m_pieceIcons[i];
			if (i < pieces.Count)
			{
				Piece piece = pieces[i];
				bool flag = player.HaveRequirements(piece, Player.RequirementMode.CanBuild);
				pieceIconData.m_icon.color = (flag ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 0f, 1f, 0f));
			}
			else
			{
				pieceIconData.m_icon.color = Color.white;
			}
		}
		this.m_pieceIconUpdateIndex = 0;
	}

	// Token: 0x06000514 RID: 1300 RVA: 0x0002A9EC File Offset: 0x00028BEC
	private void UpdatePieceBar(float dt)
	{
		this.m_pieceBarPosX = Mathf.Lerp(this.m_pieceBarPosX, this.m_pieceBarTargetPosX, 0.1f);
		this.m_pieceListRoot.anchoredPosition.x = Mathf.Round(this.m_pieceBarPosX);
	}

	// Token: 0x06000515 RID: 1301 RVA: 0x0002AA38 File Offset: 0x00028C38
	public void TogglePieceSelection()
	{
		this.m_hoveredPiece = null;
		if (this.m_pieceSelectionWindow.activeSelf)
		{
			this.m_pieceSelectionWindow.SetActive(false);
			return;
		}
		this.m_pieceSelectionWindow.SetActive(true);
		this.UpdateBuild(Player.m_localPlayer, true);
	}

	// Token: 0x06000516 RID: 1302 RVA: 0x0002AA73 File Offset: 0x00028C73
	private void OnClosePieceSelection(UIInputHandler ih)
	{
		Hud.HidePieceSelection();
	}

	// Token: 0x06000517 RID: 1303 RVA: 0x0002AA7A File Offset: 0x00028C7A
	public static void HidePieceSelection()
	{
		if (Hud.m_instance == null)
		{
			return;
		}
		Hud.m_instance.m_closePieceSelection = 2;
	}

	// Token: 0x06000518 RID: 1304 RVA: 0x0002AA95 File Offset: 0x00028C95
	public static bool IsPieceSelectionVisible()
	{
		return !(Hud.m_instance == null) && Hud.m_instance.m_buildHud.activeSelf && Hud.m_instance.m_pieceSelectionWindow.activeSelf;
	}

	// Token: 0x06000519 RID: 1305 RVA: 0x0002AAC8 File Offset: 0x00028CC8
	private void UpdateBuild(Player player, bool forceUpdateAllBuildStatuses)
	{
		if (!player.InPlaceMode())
		{
			this.m_buildHud.SetActive(false);
			this.m_pieceSelectionWindow.SetActive(false);
			return;
		}
		if (this.m_closePieceSelection > 0)
		{
			this.m_closePieceSelection--;
			if (this.m_closePieceSelection <= 0 && this.m_pieceSelectionWindow.activeSelf)
			{
				this.m_pieceSelectionWindow.SetActive(false);
			}
		}
		Piece piece;
		Vector2Int selectedNr;
		int num;
		Piece.PieceCategory pieceCategory;
		bool flag;
		player.GetBuildSelection(out piece, out selectedNr, out num, out pieceCategory, out flag);
		this.m_buildHud.SetActive(true);
		if (this.m_pieceSelectionWindow.activeSelf)
		{
			this.UpdatePieceList(player, selectedNr, pieceCategory, forceUpdateAllBuildStatuses);
			this.m_pieceCategoryRoot.SetActive(flag);
			if (flag)
			{
				for (int i = 0; i < this.m_pieceCategoryTabs.Length; i++)
				{
					GameObject gameObject = this.m_pieceCategoryTabs[i];
					Transform transform = gameObject.transform.Find("Selected");
					string text = string.Concat(new object[]
					{
						this.m_buildCategoryNames[i],
						" [<color=yellow>",
						player.GetAvailableBuildPiecesInCategory((Piece.PieceCategory)i),
						"</color>]"
					});
					if (i == (int)pieceCategory)
					{
						transform.gameObject.SetActive(true);
						transform.GetComponentInChildren<Text>().text = text;
					}
					else
					{
						transform.gameObject.SetActive(false);
						gameObject.GetComponentInChildren<Text>().text = text;
					}
				}
			}
		}
		if (this.m_hoveredPiece && (ZInput.IsGamepadActive() || !player.IsPieceAvailable(this.m_hoveredPiece)))
		{
			this.m_hoveredPiece = null;
		}
		if (this.m_hoveredPiece)
		{
			this.SetupPieceInfo(this.m_hoveredPiece);
			return;
		}
		this.SetupPieceInfo(piece);
	}

	// Token: 0x0600051A RID: 1306 RVA: 0x0002AC7C File Offset: 0x00028E7C
	private void SetupPieceInfo(Piece piece)
	{
		if (piece == null)
		{
			this.m_buildSelection.text = Localization.instance.Localize("$hud_nothingtobuild");
			this.m_pieceDescription.text = "";
			this.m_buildIcon.enabled = false;
			for (int i = 0; i < this.m_requirementItems.Length; i++)
			{
				this.m_requirementItems[i].SetActive(false);
			}
			return;
		}
		Player localPlayer = Player.m_localPlayer;
		this.m_buildSelection.text = Localization.instance.Localize(piece.m_name);
		this.m_pieceDescription.text = Localization.instance.Localize(piece.m_description);
		this.m_buildIcon.enabled = true;
		this.m_buildIcon.sprite = piece.m_icon;
		for (int j = 0; j < this.m_requirementItems.Length; j++)
		{
			if (j < piece.m_resources.Length)
			{
				Piece.Requirement req = piece.m_resources[j];
				this.m_requirementItems[j].SetActive(true);
				InventoryGui.SetupRequirement(this.m_requirementItems[j].transform, req, localPlayer, false, 0);
			}
			else
			{
				this.m_requirementItems[j].SetActive(false);
			}
		}
		if (piece.m_craftingStation)
		{
			CraftingStation craftingStation = CraftingStation.HaveBuildStationInRange(piece.m_craftingStation.m_name, localPlayer.transform.position);
			GameObject gameObject = this.m_requirementItems[piece.m_resources.Length];
			gameObject.SetActive(true);
			Image component = gameObject.transform.Find("res_icon").GetComponent<Image>();
			Text component2 = gameObject.transform.Find("res_name").GetComponent<Text>();
			Text component3 = gameObject.transform.Find("res_amount").GetComponent<Text>();
			UITooltip component4 = gameObject.GetComponent<UITooltip>();
			component.sprite = piece.m_craftingStation.m_icon;
			component2.text = Localization.instance.Localize(piece.m_craftingStation.m_name);
			component4.m_text = piece.m_craftingStation.m_name;
			if (craftingStation != null)
			{
				craftingStation.ShowAreaMarker();
				component.color = Color.white;
				component3.text = "";
				component3.color = Color.white;
				return;
			}
			component.color = Color.gray;
			component3.text = "None";
			component3.color = ((Mathf.Sin(Time.time * 10f) > 0f) ? Color.red : Color.white);
		}
	}

	// Token: 0x0600051B RID: 1307 RVA: 0x0002AEE4 File Offset: 0x000290E4
	private void UpdateGuardianPower(Player player)
	{
		StatusEffect statusEffect;
		float num;
		player.GetGuardianPowerHUD(out statusEffect, out num);
		if (!statusEffect)
		{
			this.m_gpRoot.gameObject.SetActive(false);
			return;
		}
		this.m_gpRoot.gameObject.SetActive(true);
		this.m_gpIcon.sprite = statusEffect.m_icon;
		this.m_gpIcon.color = ((num <= 0f) ? Color.white : new Color(1f, 0f, 1f, 0f));
		this.m_gpName.text = Localization.instance.Localize(statusEffect.m_name);
		if (num > 0f)
		{
			this.m_gpCooldown.text = StatusEffect.GetTimeString(num, false, false);
			return;
		}
		this.m_gpCooldown.text = Localization.instance.Localize("$hud_ready");
	}

	// Token: 0x0600051C RID: 1308 RVA: 0x0002AFC0 File Offset: 0x000291C0
	private void UpdateStatusEffects(List<StatusEffect> statusEffects)
	{
		if (this.m_statusEffects.Count != statusEffects.Count)
		{
			foreach (RectTransform rectTransform in this.m_statusEffects)
			{
				UnityEngine.Object.Destroy(rectTransform.gameObject);
			}
			this.m_statusEffects.Clear();
			for (int i = 0; i < statusEffects.Count; i++)
			{
				RectTransform rectTransform2 = UnityEngine.Object.Instantiate<RectTransform>(this.m_statusEffectTemplate, this.m_statusEffectListRoot);
				rectTransform2.gameObject.SetActive(true);
				rectTransform2.anchoredPosition = new Vector3(-4f - (float)i * this.m_statusEffectSpacing, 0f, 0f);
				this.m_statusEffects.Add(rectTransform2);
			}
		}
		for (int j = 0; j < statusEffects.Count; j++)
		{
			StatusEffect statusEffect = statusEffects[j];
			RectTransform rectTransform3 = this.m_statusEffects[j];
			Image component = rectTransform3.Find("Icon").GetComponent<Image>();
			component.sprite = statusEffect.m_icon;
			if (statusEffect.m_flashIcon)
			{
				component.color = ((Mathf.Sin(Time.time * 10f) > 0f) ? new Color(1f, 0.5f, 0.5f, 1f) : Color.white);
			}
			else
			{
				component.color = Color.white;
			}
			rectTransform3.Find("Cooldown").gameObject.SetActive(statusEffect.m_cooldownIcon);
			rectTransform3.GetComponentInChildren<Text>().text = Localization.instance.Localize(statusEffect.m_name);
			Text component2 = rectTransform3.Find("TimeText").GetComponent<Text>();
			string iconText = statusEffect.GetIconText();
			if (!string.IsNullOrEmpty(iconText))
			{
				component2.gameObject.SetActive(true);
				component2.text = iconText;
			}
			else
			{
				component2.gameObject.SetActive(false);
			}
			if (statusEffect.m_isNew)
			{
				statusEffect.m_isNew = false;
				rectTransform3.GetComponentInChildren<Animator>().SetTrigger("flash");
			}
		}
	}

	// Token: 0x0600051D RID: 1309 RVA: 0x0002B1E4 File Offset: 0x000293E4
	private void UpdateEvent(Player player)
	{
		RandomEvent activeEvent = RandEventSystem.instance.GetActiveEvent();
		if (activeEvent != null && !EnemyHud.instance.ShowingBossHud() && activeEvent.GetTime() > 3f)
		{
			this.m_eventBar.SetActive(true);
			this.m_eventName.text = Localization.instance.Localize(activeEvent.GetHudText());
			return;
		}
		this.m_eventBar.SetActive(false);
	}

	// Token: 0x0600051E RID: 1310 RVA: 0x0002B24C File Offset: 0x0002944C
	public void ToggleBetaTextVisible()
	{
		this.m_betaText.SetActive(!this.m_betaText.activeSelf);
	}

	// Token: 0x0600051F RID: 1311 RVA: 0x0002B267 File Offset: 0x00029467
	public void FlashHealthBar()
	{
		this.m_healthAnimator.SetTrigger("Flash");
	}

	// Token: 0x06000520 RID: 1312 RVA: 0x0002B279 File Offset: 0x00029479
	public void StaminaBarUppgradeFlash()
	{
		this.m_staminaAnimator.SetBool("Visible", true);
		this.m_staminaAnimator.SetTrigger("Flash");
	}

	// Token: 0x06000521 RID: 1313 RVA: 0x0002B29C File Offset: 0x0002949C
	public void StaminaBarNoStaminaFlash()
	{
		if (this.m_staminaAnimator.GetCurrentAnimatorStateInfo(0).IsTag("nostamina"))
		{
			return;
		}
		this.m_staminaAnimator.SetBool("Visible", true);
		this.m_staminaAnimator.SetTrigger("NoStamina");
	}

	// Token: 0x06000522 RID: 1314 RVA: 0x0002B2E6 File Offset: 0x000294E6
	public static bool IsUserHidden()
	{
		return Hud.m_instance && Hud.m_instance.m_userHidden;
	}

	// Token: 0x0400053B RID: 1339
	private static Hud m_instance;

	// Token: 0x0400053C RID: 1340
	public GameObject m_rootObject;

	// Token: 0x0400053D RID: 1341
	public Text m_buildSelection;

	// Token: 0x0400053E RID: 1342
	public Text m_pieceDescription;

	// Token: 0x0400053F RID: 1343
	public Image m_buildIcon;

	// Token: 0x04000540 RID: 1344
	public GameObject m_buildHud;

	// Token: 0x04000541 RID: 1345
	public GameObject m_saveIcon;

	// Token: 0x04000542 RID: 1346
	public GameObject m_badConnectionIcon;

	// Token: 0x04000543 RID: 1347
	public GameObject m_betaText;

	// Token: 0x04000544 RID: 1348
	[Header("Piece")]
	public GameObject[] m_requirementItems = new GameObject[0];

	// Token: 0x04000545 RID: 1349
	public GameObject[] m_pieceCategoryTabs = new GameObject[0];

	// Token: 0x04000546 RID: 1350
	public GameObject m_pieceSelectionWindow;

	// Token: 0x04000547 RID: 1351
	public GameObject m_pieceCategoryRoot;

	// Token: 0x04000548 RID: 1352
	public RectTransform m_pieceListRoot;

	// Token: 0x04000549 RID: 1353
	public RectTransform m_pieceListMask;

	// Token: 0x0400054A RID: 1354
	public GameObject m_pieceIconPrefab;

	// Token: 0x0400054B RID: 1355
	public UIInputHandler m_closePieceSelectionButton;

	// Token: 0x0400054C RID: 1356
	public EffectList m_selectItemEffect = new EffectList();

	// Token: 0x0400054D RID: 1357
	public float m_pieceIconSpacing = 64f;

	// Token: 0x0400054E RID: 1358
	private float m_pieceBarPosX;

	// Token: 0x0400054F RID: 1359
	private float m_pieceBarTargetPosX;

	// Token: 0x04000550 RID: 1360
	private Piece.PieceCategory m_lastPieceCategory = Piece.PieceCategory.Max;

	// Token: 0x04000551 RID: 1361
	[Header("Health")]
	public RectTransform m_healthBarRoot;

	// Token: 0x04000552 RID: 1362
	public RectTransform m_healthPanel;

	// Token: 0x04000553 RID: 1363
	private const float m_healthPanelBuffer = 56f;

	// Token: 0x04000554 RID: 1364
	private const float m_healthPanelMinSize = 138f;

	// Token: 0x04000555 RID: 1365
	public Animator m_healthAnimator;

	// Token: 0x04000556 RID: 1366
	public GuiBar m_healthBarFast;

	// Token: 0x04000557 RID: 1367
	public GuiBar m_healthBarSlow;

	// Token: 0x04000558 RID: 1368
	public Text m_healthText;

	// Token: 0x04000559 RID: 1369
	public Text m_healthMaxText;

	// Token: 0x0400055A RID: 1370
	[Header("Food")]
	public Image[] m_foodBars;

	// Token: 0x0400055B RID: 1371
	public Image[] m_foodIcons;

	// Token: 0x0400055C RID: 1372
	public RectTransform m_foodBarRoot;

	// Token: 0x0400055D RID: 1373
	public RectTransform m_foodBaseBar;

	// Token: 0x0400055E RID: 1374
	public Image m_foodIcon;

	// Token: 0x0400055F RID: 1375
	public Color m_foodColorHungry = Color.white;

	// Token: 0x04000560 RID: 1376
	public Color m_foodColorFull = Color.white;

	// Token: 0x04000561 RID: 1377
	public Text m_foodText;

	// Token: 0x04000562 RID: 1378
	[Header("Action bar")]
	public GameObject m_actionBarRoot;

	// Token: 0x04000563 RID: 1379
	public GuiBar m_actionProgress;

	// Token: 0x04000564 RID: 1380
	public Text m_actionName;

	// Token: 0x04000565 RID: 1381
	[Header("Guardian power")]
	public RectTransform m_gpRoot;

	// Token: 0x04000566 RID: 1382
	public Text m_gpName;

	// Token: 0x04000567 RID: 1383
	public Text m_gpCooldown;

	// Token: 0x04000568 RID: 1384
	public Image m_gpIcon;

	// Token: 0x04000569 RID: 1385
	[Header("Stamina")]
	public GameObject m_staminaBar;

	// Token: 0x0400056A RID: 1386
	public GuiBar m_staminaBarFast;

	// Token: 0x0400056B RID: 1387
	public GuiBar m_staminaBarSlow;

	// Token: 0x0400056C RID: 1388
	public Animator m_staminaAnimator;

	// Token: 0x0400056D RID: 1389
	private float m_staminaBarBorderBuffer = 16f;

	// Token: 0x0400056E RID: 1390
	public RectTransform m_staminaBar2Root;

	// Token: 0x0400056F RID: 1391
	public GuiBar m_staminaBar2Fast;

	// Token: 0x04000570 RID: 1392
	public GuiBar m_staminaBar2Slow;

	// Token: 0x04000571 RID: 1393
	[Header("Loading")]
	public CanvasGroup m_loadingScreen;

	// Token: 0x04000572 RID: 1394
	public GameObject m_loadingProgress;

	// Token: 0x04000573 RID: 1395
	public GameObject m_sleepingProgress;

	// Token: 0x04000574 RID: 1396
	public GameObject m_teleportingProgress;

	// Token: 0x04000575 RID: 1397
	public Image m_loadingImage;

	// Token: 0x04000576 RID: 1398
	public Text m_loadingTip;

	// Token: 0x04000577 RID: 1399
	public bool m_useRandomImages = true;

	// Token: 0x04000578 RID: 1400
	public string m_loadingImagePath = "/loadingscreens/";

	// Token: 0x04000579 RID: 1401
	public int m_loadingImages = 2;

	// Token: 0x0400057A RID: 1402
	public List<string> m_loadingTips = new List<string>();

	// Token: 0x0400057B RID: 1403
	[Header("Crosshair")]
	public Image m_crosshair;

	// Token: 0x0400057C RID: 1404
	public Image m_crosshairBow;

	// Token: 0x0400057D RID: 1405
	public Text m_hoverName;

	// Token: 0x0400057E RID: 1406
	public RectTransform m_pieceHealthRoot;

	// Token: 0x0400057F RID: 1407
	public GuiBar m_pieceHealthBar;

	// Token: 0x04000580 RID: 1408
	public Image m_damageScreen;

	// Token: 0x04000581 RID: 1409
	[Header("Target")]
	public GameObject m_targetedAlert;

	// Token: 0x04000582 RID: 1410
	public GameObject m_targeted;

	// Token: 0x04000583 RID: 1411
	public GameObject m_hidden;

	// Token: 0x04000584 RID: 1412
	public GuiBar m_stealthBar;

	// Token: 0x04000585 RID: 1413
	[Header("Status effect")]
	public RectTransform m_statusEffectListRoot;

	// Token: 0x04000586 RID: 1414
	public RectTransform m_statusEffectTemplate;

	// Token: 0x04000587 RID: 1415
	public float m_statusEffectSpacing = 55f;

	// Token: 0x04000588 RID: 1416
	private List<RectTransform> m_statusEffects = new List<RectTransform>();

	// Token: 0x04000589 RID: 1417
	[Header("Ship hud")]
	public GameObject m_shipHudRoot;

	// Token: 0x0400058A RID: 1418
	public GameObject m_shipControlsRoot;

	// Token: 0x0400058B RID: 1419
	public GameObject m_rudderLeft;

	// Token: 0x0400058C RID: 1420
	public GameObject m_rudderRight;

	// Token: 0x0400058D RID: 1421
	public GameObject m_rudderSlow;

	// Token: 0x0400058E RID: 1422
	public GameObject m_rudderForward;

	// Token: 0x0400058F RID: 1423
	public GameObject m_rudderFastForward;

	// Token: 0x04000590 RID: 1424
	public GameObject m_rudderBackward;

	// Token: 0x04000591 RID: 1425
	public GameObject m_halfSail;

	// Token: 0x04000592 RID: 1426
	public GameObject m_fullSail;

	// Token: 0x04000593 RID: 1427
	public GameObject m_rudder;

	// Token: 0x04000594 RID: 1428
	public RectTransform m_shipWindIndicatorRoot;

	// Token: 0x04000595 RID: 1429
	public Image m_shipWindIcon;

	// Token: 0x04000596 RID: 1430
	public RectTransform m_shipWindIconRoot;

	// Token: 0x04000597 RID: 1431
	public Image m_shipRudderIndicator;

	// Token: 0x04000598 RID: 1432
	public Image m_shipRudderIcon;

	// Token: 0x04000599 RID: 1433
	[Header("Event")]
	public GameObject m_eventBar;

	// Token: 0x0400059A RID: 1434
	public Text m_eventName;

	// Token: 0x0400059B RID: 1435
	private bool m_userHidden;

	// Token: 0x0400059C RID: 1436
	private CraftingStation m_currentCraftingStation;

	// Token: 0x0400059D RID: 1437
	private List<string> m_buildCategoryNames = new List<string>();

	// Token: 0x0400059E RID: 1438
	private List<StatusEffect> m_tempStatusEffects = new List<StatusEffect>();

	// Token: 0x0400059F RID: 1439
	private List<Hud.PieceIconData> m_pieceIcons = new List<Hud.PieceIconData>();

	// Token: 0x040005A0 RID: 1440
	private int m_pieceIconUpdateIndex;

	// Token: 0x040005A1 RID: 1441
	private bool m_haveSetupLoadScreen;

	// Token: 0x040005A2 RID: 1442
	private int m_closePieceSelection;

	// Token: 0x040005A3 RID: 1443
	private Piece m_hoveredPiece;

	// Token: 0x0200014B RID: 331
	private class PieceIconData
	{
		// Token: 0x040010E3 RID: 4323
		public GameObject m_go;

		// Token: 0x040010E4 RID: 4324
		public Image m_icon;

		// Token: 0x040010E5 RID: 4325
		public GameObject m_marker;

		// Token: 0x040010E6 RID: 4326
		public GameObject m_upgrade;

		// Token: 0x040010E7 RID: 4327
		public UITooltip m_tooltip;
	}
}
