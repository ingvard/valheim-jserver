using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

// Token: 0x0200005A RID: 90
public class Minimap : MonoBehaviour
{
	// Token: 0x1700000E RID: 14
	// (get) Token: 0x0600059F RID: 1439 RVA: 0x0002FF8F File Offset: 0x0002E18F
	public static Minimap instance
	{
		get
		{
			return Minimap.m_instance;
		}
	}

	// Token: 0x060005A0 RID: 1440 RVA: 0x0002FF96 File Offset: 0x0002E196
	private void Awake()
	{
		Minimap.m_instance = this;
		this.m_largeRoot.SetActive(false);
		this.m_smallRoot.SetActive(true);
	}

	// Token: 0x060005A1 RID: 1441 RVA: 0x0002FFB6 File Offset: 0x0002E1B6
	private void OnDestroy()
	{
		Minimap.m_instance = null;
	}

	// Token: 0x060005A2 RID: 1442 RVA: 0x0002FFBE File Offset: 0x0002E1BE
	public static bool IsOpen()
	{
		return Minimap.m_instance && Minimap.m_instance.m_largeRoot.activeSelf;
	}

	// Token: 0x060005A3 RID: 1443 RVA: 0x0002FFDD File Offset: 0x0002E1DD
	public static bool InTextInput()
	{
		return Minimap.m_instance && Minimap.m_instance.m_mode == Minimap.MapMode.Large && Minimap.m_instance.m_wasFocused;
	}

	// Token: 0x060005A4 RID: 1444 RVA: 0x00030004 File Offset: 0x0002E204
	private void Start()
	{
		this.m_mapTexture = new Texture2D(this.m_textureSize, this.m_textureSize, TextureFormat.RGBA32, false);
		this.m_mapTexture.wrapMode = TextureWrapMode.Clamp;
		this.m_forestMaskTexture = new Texture2D(this.m_textureSize, this.m_textureSize, TextureFormat.RGBA32, false);
		this.m_forestMaskTexture.wrapMode = TextureWrapMode.Clamp;
		this.m_heightTexture = new Texture2D(this.m_textureSize, this.m_textureSize, TextureFormat.RFloat, false);
		this.m_heightTexture.wrapMode = TextureWrapMode.Clamp;
		this.m_fogTexture = new Texture2D(this.m_textureSize, this.m_textureSize, TextureFormat.RGBA32, false);
		this.m_fogTexture.wrapMode = TextureWrapMode.Clamp;
		this.m_explored = new bool[this.m_textureSize * this.m_textureSize];
		this.m_mapImageLarge.material = UnityEngine.Object.Instantiate<Material>(this.m_mapImageLarge.material);
		this.m_mapImageSmall.material = UnityEngine.Object.Instantiate<Material>(this.m_mapImageSmall.material);
		this.m_mapImageLarge.material.SetTexture("_MainTex", this.m_mapTexture);
		this.m_mapImageLarge.material.SetTexture("_MaskTex", this.m_forestMaskTexture);
		this.m_mapImageLarge.material.SetTexture("_HeightTex", this.m_heightTexture);
		this.m_mapImageLarge.material.SetTexture("_FogTex", this.m_fogTexture);
		this.m_mapImageSmall.material.SetTexture("_MainTex", this.m_mapTexture);
		this.m_mapImageSmall.material.SetTexture("_MaskTex", this.m_forestMaskTexture);
		this.m_mapImageSmall.material.SetTexture("_HeightTex", this.m_heightTexture);
		this.m_mapImageSmall.material.SetTexture("_FogTex", this.m_fogTexture);
		this.m_nameInput.gameObject.SetActive(false);
		UIInputHandler component = this.m_mapImageLarge.GetComponent<UIInputHandler>();
		component.m_onRightClick = (Action<UIInputHandler>)Delegate.Combine(component.m_onRightClick, new Action<UIInputHandler>(this.OnMapRightClick));
		component.m_onMiddleClick = (Action<UIInputHandler>)Delegate.Combine(component.m_onMiddleClick, new Action<UIInputHandler>(this.OnMapMiddleClick));
		component.m_onLeftDown = (Action<UIInputHandler>)Delegate.Combine(component.m_onLeftDown, new Action<UIInputHandler>(this.OnMapLeftDown));
		component.m_onLeftUp = (Action<UIInputHandler>)Delegate.Combine(component.m_onLeftUp, new Action<UIInputHandler>(this.OnMapLeftUp));
		this.SelectIcon(Minimap.PinType.Icon0);
		this.Reset();
	}

	// Token: 0x060005A5 RID: 1445 RVA: 0x0003027C File Offset: 0x0002E47C
	public void Reset()
	{
		Color32[] array = new Color32[this.m_textureSize * this.m_textureSize];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		}
		this.m_fogTexture.SetPixels32(array);
		this.m_fogTexture.Apply();
		for (int j = 0; j < this.m_explored.Length; j++)
		{
			this.m_explored[j] = false;
		}
	}

	// Token: 0x060005A6 RID: 1446 RVA: 0x000302FD File Offset: 0x0002E4FD
	public void ForceRegen()
	{
		if (WorldGenerator.instance != null)
		{
			this.GenerateWorldMap();
		}
	}

	// Token: 0x060005A7 RID: 1447 RVA: 0x0003030C File Offset: 0x0002E50C
	private void Update()
	{
		if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
		{
			return;
		}
		if (Utils.GetMainCamera() == null)
		{
			return;
		}
		if (!this.m_hasGenerated)
		{
			if (WorldGenerator.instance == null)
			{
				return;
			}
			this.GenerateWorldMap();
			this.LoadMapData();
			this.m_hasGenerated = true;
		}
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		this.UpdateExplore(deltaTime, localPlayer);
		if (localPlayer.IsDead())
		{
			this.SetMapMode(Minimap.MapMode.None);
			return;
		}
		if (this.m_mode == Minimap.MapMode.None)
		{
			this.SetMapMode(Minimap.MapMode.Small);
		}
		bool flag = (Chat.instance == null || !Chat.instance.HasFocus()) && !global::Console.IsVisible() && !TextInput.IsVisible() && !Menu.IsVisible() && !InventoryGui.IsVisible();
		if (flag)
		{
			if (Minimap.InTextInput())
			{
				if (Input.GetKeyDown(KeyCode.Escape))
				{
					this.m_namePin = null;
				}
			}
			else if (ZInput.GetButtonDown("Map") || ZInput.GetButtonDown("JoyMap") || (this.m_mode == Minimap.MapMode.Large && (Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("JoyButtonB"))))
			{
				switch (this.m_mode)
				{
				case Minimap.MapMode.None:
					this.SetMapMode(Minimap.MapMode.Small);
					break;
				case Minimap.MapMode.Small:
					this.SetMapMode(Minimap.MapMode.Large);
					break;
				case Minimap.MapMode.Large:
					this.SetMapMode(Minimap.MapMode.Small);
					break;
				}
			}
		}
		if (this.m_mode == Minimap.MapMode.Large)
		{
			this.m_publicPosition.isOn = ZNet.instance.IsReferencePositionPublic();
			this.m_gamepadCrosshair.gameObject.SetActive(ZInput.IsGamepadActive());
		}
		this.UpdateMap(localPlayer, deltaTime, flag);
		this.UpdateDynamicPins(deltaTime);
		this.UpdatePins();
		this.UpdateBiome(localPlayer);
		this.UpdateNameInput();
	}

	// Token: 0x060005A8 RID: 1448 RVA: 0x000304AB File Offset: 0x0002E6AB
	private void ShowPinNameInput(Minimap.PinData pin)
	{
		this.m_namePin = pin;
		this.m_nameInput.text = "";
	}

	// Token: 0x060005A9 RID: 1449 RVA: 0x000304C4 File Offset: 0x0002E6C4
	private void UpdateNameInput()
	{
		if (this.m_namePin == null)
		{
			this.m_wasFocused = false;
		}
		if (this.m_namePin != null && this.m_mode == Minimap.MapMode.Large)
		{
			this.m_nameInput.gameObject.SetActive(true);
			if (!this.m_nameInput.isFocused)
			{
				EventSystem.current.SetSelectedGameObject(this.m_nameInput.gameObject);
			}
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
			{
				string text = this.m_nameInput.text;
				text = text.Replace('$', ' ');
				text = text.Replace('<', ' ');
				text = text.Replace('>', ' ');
				this.m_namePin.m_name = text;
				this.m_namePin = null;
			}
			this.m_wasFocused = true;
			return;
		}
		this.m_nameInput.gameObject.SetActive(false);
	}

	// Token: 0x060005AA RID: 1450 RVA: 0x00030598 File Offset: 0x0002E798
	private void UpdateMap(Player player, float dt, bool takeInput)
	{
		if (takeInput)
		{
			if (this.m_mode == Minimap.MapMode.Large)
			{
				float num = 0f;
				num += Input.GetAxis("Mouse ScrollWheel") * this.m_largeZoom * 2f;
				if (ZInput.GetButton("JoyButtonX"))
				{
					Vector3 viewCenterWorldPoint = this.GetViewCenterWorldPoint();
					Chat.instance.SendPing(viewCenterWorldPoint);
				}
				if (ZInput.GetButton("JoyLTrigger"))
				{
					num -= this.m_largeZoom * dt * 2f;
				}
				if (ZInput.GetButton("JoyRTrigger"))
				{
					num += this.m_largeZoom * dt * 2f;
				}
				if (ZInput.GetButtonDown("MapZoomOut") && !Minimap.InTextInput())
				{
					num -= this.m_largeZoom * 0.5f;
				}
				if (ZInput.GetButtonDown("MapZoomIn") && !Minimap.InTextInput())
				{
					num += this.m_largeZoom * 0.5f;
				}
				this.m_largeZoom = Mathf.Clamp(this.m_largeZoom - num, this.m_minZoom, this.m_maxZoom);
			}
			else
			{
				float num2 = 0f;
				if (ZInput.GetButtonDown("MapZoomOut"))
				{
					num2 -= this.m_smallZoom * 0.5f;
				}
				if (ZInput.GetButtonDown("MapZoomIn"))
				{
					num2 += this.m_smallZoom * 0.5f;
				}
				this.m_smallZoom = Mathf.Clamp(this.m_smallZoom - num2, this.m_minZoom, this.m_maxZoom);
			}
		}
		if (this.m_mode == Minimap.MapMode.Large)
		{
			if (this.m_leftDownTime != 0f && this.m_leftDownTime > this.m_clickDuration && !this.m_dragView)
			{
				this.m_dragWorldPos = this.ScreenToWorldPoint(Input.mousePosition);
				this.m_dragView = true;
				this.m_namePin = null;
			}
			this.m_mapOffset.x = this.m_mapOffset.x + ZInput.GetJoyLeftStickX() * dt * 50000f * this.m_largeZoom;
			this.m_mapOffset.z = this.m_mapOffset.z - ZInput.GetJoyLeftStickY() * dt * 50000f * this.m_largeZoom;
			if (this.m_dragView)
			{
				Vector3 b = this.ScreenToWorldPoint(Input.mousePosition) - this.m_dragWorldPos;
				this.m_mapOffset -= b;
				this.CenterMap(player.transform.position + this.m_mapOffset);
				this.m_dragWorldPos = this.ScreenToWorldPoint(Input.mousePosition);
			}
			else
			{
				this.CenterMap(player.transform.position + this.m_mapOffset);
			}
		}
		else
		{
			this.CenterMap(player.transform.position);
		}
		this.UpdateWindMarker();
		this.UpdatePlayerMarker(player, Utils.GetMainCamera().transform.rotation);
	}

	// Token: 0x060005AB RID: 1451 RVA: 0x00030830 File Offset: 0x0002EA30
	private void SetMapMode(Minimap.MapMode mode)
	{
		if (mode == this.m_mode)
		{
			return;
		}
		this.m_mode = mode;
		switch (mode)
		{
		case Minimap.MapMode.None:
			this.m_largeRoot.SetActive(false);
			this.m_smallRoot.SetActive(false);
			return;
		case Minimap.MapMode.Small:
			this.m_largeRoot.SetActive(false);
			this.m_smallRoot.SetActive(true);
			return;
		case Minimap.MapMode.Large:
			this.m_largeRoot.SetActive(true);
			this.m_smallRoot.SetActive(false);
			this.m_dragView = false;
			this.m_mapOffset = Vector3.zero;
			this.m_namePin = null;
			return;
		default:
			return;
		}
	}

	// Token: 0x060005AC RID: 1452 RVA: 0x000308C4 File Offset: 0x0002EAC4
	private void CenterMap(Vector3 centerPoint)
	{
		float x;
		float y;
		this.WorldToMapPoint(centerPoint, out x, out y);
		Rect uvRect = this.m_mapImageSmall.uvRect;
		uvRect.width = this.m_smallZoom;
		uvRect.height = this.m_smallZoom;
		uvRect.center = new Vector2(x, y);
		this.m_mapImageSmall.uvRect = uvRect;
		RectTransform rectTransform = this.m_mapImageLarge.transform as RectTransform;
		float num = rectTransform.rect.width / rectTransform.rect.height;
		Rect uvRect2 = this.m_mapImageSmall.uvRect;
		uvRect2.width = this.m_largeZoom * num;
		uvRect2.height = this.m_largeZoom;
		uvRect2.center = new Vector2(x, y);
		this.m_mapImageLarge.uvRect = uvRect2;
		if (this.m_mode == Minimap.MapMode.Large)
		{
			this.m_mapImageLarge.material.SetFloat("_zoom", this.m_largeZoom);
			this.m_mapImageLarge.material.SetFloat("_pixelSize", 200f / this.m_largeZoom);
			this.m_mapImageLarge.material.SetVector("_mapCenter", centerPoint);
			return;
		}
		this.m_mapImageSmall.material.SetFloat("_zoom", this.m_smallZoom);
		this.m_mapImageSmall.material.SetFloat("_pixelSize", 200f / this.m_smallZoom);
		this.m_mapImageSmall.material.SetVector("_mapCenter", centerPoint);
	}

	// Token: 0x060005AD RID: 1453 RVA: 0x00030A4B File Offset: 0x0002EC4B
	private void UpdateDynamicPins(float dt)
	{
		this.UpdateProfilePins();
		this.UpdateShoutPins();
		this.UpdatePingPins();
		this.UpdatePlayerPins(dt);
		this.UpdateLocationPins(dt);
		this.UpdateEventPin(dt);
	}

	// Token: 0x060005AE RID: 1454 RVA: 0x00030A74 File Offset: 0x0002EC74
	private void UpdateProfilePins()
	{
		PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
		if (playerProfile.HaveDeathPoint())
		{
			if (this.m_deathPin == null)
			{
				this.m_deathPin = this.AddPin(playerProfile.GetDeathPoint(), Minimap.PinType.Death, "", false, false);
			}
			this.m_deathPin.m_pos = playerProfile.GetDeathPoint();
		}
		else if (this.m_deathPin != null)
		{
			this.RemovePin(this.m_deathPin);
			this.m_deathPin = null;
		}
		if (playerProfile.HaveCustomSpawnPoint())
		{
			if (this.m_spawnPointPin == null)
			{
				this.m_spawnPointPin = this.AddPin(playerProfile.GetCustomSpawnPoint(), Minimap.PinType.Bed, "", false, false);
			}
			this.m_spawnPointPin.m_pos = playerProfile.GetCustomSpawnPoint();
			return;
		}
		if (this.m_spawnPointPin != null)
		{
			this.RemovePin(this.m_spawnPointPin);
			this.m_spawnPointPin = null;
		}
	}

	// Token: 0x060005AF RID: 1455 RVA: 0x00030B3C File Offset: 0x0002ED3C
	private void UpdateEventPin(float dt)
	{
		if (Time.time - this.m_updateEventTime < 1f)
		{
			return;
		}
		this.m_updateEventTime = Time.time;
		RandomEvent currentRandomEvent = RandEventSystem.instance.GetCurrentRandomEvent();
		if (currentRandomEvent != null)
		{
			if (this.m_randEventAreaPin == null)
			{
				this.m_randEventAreaPin = this.AddPin(currentRandomEvent.m_pos, Minimap.PinType.EventArea, "", false, false);
				this.m_randEventAreaPin.m_worldSize = RandEventSystem.instance.m_randomEventRange * 2f;
				this.m_randEventAreaPin.m_worldSize *= 0.9f;
			}
			if (this.m_randEventPin == null)
			{
				this.m_randEventPin = this.AddPin(currentRandomEvent.m_pos, Minimap.PinType.RandomEvent, "", false, false);
				this.m_randEventPin.m_animate = true;
				this.m_randEventPin.m_doubleSize = true;
			}
			this.m_randEventAreaPin.m_pos = currentRandomEvent.m_pos;
			this.m_randEventPin.m_pos = currentRandomEvent.m_pos;
			this.m_randEventPin.m_name = Localization.instance.Localize(currentRandomEvent.GetHudText());
			return;
		}
		if (this.m_randEventPin != null)
		{
			this.RemovePin(this.m_randEventPin);
			this.m_randEventPin = null;
		}
		if (this.m_randEventAreaPin != null)
		{
			this.RemovePin(this.m_randEventAreaPin);
			this.m_randEventAreaPin = null;
		}
	}

	// Token: 0x060005B0 RID: 1456 RVA: 0x00030C80 File Offset: 0x0002EE80
	private void UpdateLocationPins(float dt)
	{
		this.m_updateLocationsTimer -= dt;
		if (this.m_updateLocationsTimer <= 0f)
		{
			this.m_updateLocationsTimer = 5f;
			Dictionary<Vector3, string> dictionary = new Dictionary<Vector3, string>();
			ZoneSystem.instance.GetLocationIcons(dictionary);
			bool flag = false;
			while (!flag)
			{
				flag = true;
				foreach (KeyValuePair<Vector3, Minimap.PinData> keyValuePair in this.m_locationPins)
				{
					if (!dictionary.ContainsKey(keyValuePair.Key))
					{
						ZLog.DevLog("Minimap: Removing location " + keyValuePair.Value.m_name);
						this.RemovePin(keyValuePair.Value);
						this.m_locationPins.Remove(keyValuePair.Key);
						flag = false;
						break;
					}
				}
			}
			foreach (KeyValuePair<Vector3, string> keyValuePair2 in dictionary)
			{
				if (!this.m_locationPins.ContainsKey(keyValuePair2.Key))
				{
					Sprite locationIcon = this.GetLocationIcon(keyValuePair2.Value);
					if (locationIcon)
					{
						Minimap.PinData pinData = this.AddPin(keyValuePair2.Key, Minimap.PinType.None, "", false, false);
						pinData.m_icon = locationIcon;
						pinData.m_doubleSize = true;
						this.m_locationPins.Add(keyValuePair2.Key, pinData);
						ZLog.Log("Minimap: Adding unique location " + keyValuePair2.Key);
					}
				}
			}
		}
	}

	// Token: 0x060005B1 RID: 1457 RVA: 0x00030E24 File Offset: 0x0002F024
	private Sprite GetLocationIcon(string name)
	{
		foreach (Minimap.LocationSpriteData locationSpriteData in this.m_locationIcons)
		{
			if (locationSpriteData.m_name == name)
			{
				return locationSpriteData.m_icon;
			}
		}
		return null;
	}

	// Token: 0x060005B2 RID: 1458 RVA: 0x00030E8C File Offset: 0x0002F08C
	private void UpdatePlayerPins(float dt)
	{
		this.m_tempPlayerInfo.Clear();
		ZNet.instance.GetOtherPublicPlayers(this.m_tempPlayerInfo);
		if (this.m_playerPins.Count != this.m_tempPlayerInfo.Count)
		{
			foreach (Minimap.PinData pin in this.m_playerPins)
			{
				this.RemovePin(pin);
			}
			this.m_playerPins.Clear();
			foreach (ZNet.PlayerInfo playerInfo in this.m_tempPlayerInfo)
			{
				Minimap.PinData item = this.AddPin(Vector3.zero, Minimap.PinType.Player, "", false, false);
				this.m_playerPins.Add(item);
			}
		}
		for (int i = 0; i < this.m_tempPlayerInfo.Count; i++)
		{
			Minimap.PinData pinData = this.m_playerPins[i];
			ZNet.PlayerInfo playerInfo2 = this.m_tempPlayerInfo[i];
			if (pinData.m_name == playerInfo2.m_name)
			{
				pinData.m_pos = Vector3.MoveTowards(pinData.m_pos, playerInfo2.m_position, 200f * dt);
			}
			else
			{
				pinData.m_name = playerInfo2.m_name;
				pinData.m_pos = playerInfo2.m_position;
			}
		}
	}

	// Token: 0x060005B3 RID: 1459 RVA: 0x0003100C File Offset: 0x0002F20C
	private void UpdatePingPins()
	{
		this.m_tempShouts.Clear();
		Chat.instance.GetPingWorldTexts(this.m_tempShouts);
		if (this.m_pingPins.Count != this.m_tempShouts.Count)
		{
			foreach (Minimap.PinData pin in this.m_pingPins)
			{
				this.RemovePin(pin);
			}
			this.m_pingPins.Clear();
			foreach (Chat.WorldTextInstance worldTextInstance in this.m_tempShouts)
			{
				Minimap.PinData pinData = this.AddPin(Vector3.zero, Minimap.PinType.Ping, "", false, false);
				pinData.m_doubleSize = true;
				pinData.m_animate = true;
				this.m_pingPins.Add(pinData);
			}
		}
		for (int i = 0; i < this.m_tempShouts.Count; i++)
		{
			Minimap.PinData pinData2 = this.m_pingPins[i];
			Chat.WorldTextInstance worldTextInstance2 = this.m_tempShouts[i];
			pinData2.m_pos = worldTextInstance2.m_position;
			pinData2.m_name = worldTextInstance2.m_name + ": " + worldTextInstance2.m_text;
		}
	}

	// Token: 0x060005B4 RID: 1460 RVA: 0x0003116C File Offset: 0x0002F36C
	private void UpdateShoutPins()
	{
		this.m_tempShouts.Clear();
		Chat.instance.GetShoutWorldTexts(this.m_tempShouts);
		if (this.m_shoutPins.Count != this.m_tempShouts.Count)
		{
			foreach (Minimap.PinData pin in this.m_shoutPins)
			{
				this.RemovePin(pin);
			}
			this.m_shoutPins.Clear();
			foreach (Chat.WorldTextInstance worldTextInstance in this.m_tempShouts)
			{
				Minimap.PinData pinData = this.AddPin(Vector3.zero, Minimap.PinType.Shout, "", false, false);
				pinData.m_doubleSize = true;
				pinData.m_animate = true;
				this.m_shoutPins.Add(pinData);
			}
		}
		for (int i = 0; i < this.m_tempShouts.Count; i++)
		{
			Minimap.PinData pinData2 = this.m_shoutPins[i];
			Chat.WorldTextInstance worldTextInstance2 = this.m_tempShouts[i];
			pinData2.m_pos = worldTextInstance2.m_position;
			pinData2.m_name = worldTextInstance2.m_name + ": " + worldTextInstance2.m_text;
		}
	}

	// Token: 0x060005B5 RID: 1461 RVA: 0x000312CC File Offset: 0x0002F4CC
	private void UpdatePins()
	{
		RawImage rawImage = (this.m_mode == Minimap.MapMode.Large) ? this.m_mapImageLarge : this.m_mapImageSmall;
		float num = (this.m_mode == Minimap.MapMode.Large) ? this.m_pinSizeLarge : this.m_pinSizeSmall;
		RectTransform rectTransform = (this.m_mode == Minimap.MapMode.Large) ? this.m_pinRootLarge : this.m_pinRootSmall;
		if (this.m_mode != Minimap.MapMode.Large)
		{
			float smallZoom = this.m_smallZoom;
		}
		else
		{
			float largeZoom = this.m_largeZoom;
		}
		foreach (Minimap.PinData pinData in this.m_pins)
		{
			if (this.IsPointVisible(pinData.m_pos, rawImage))
			{
				if (pinData.m_uiElement == null || pinData.m_uiElement.parent != rectTransform)
				{
					if (pinData.m_uiElement != null)
					{
						UnityEngine.Object.Destroy(pinData.m_uiElement.gameObject);
					}
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_pinPrefab);
					gameObject.GetComponent<Image>().sprite = pinData.m_icon;
					pinData.m_uiElement = (gameObject.transform as RectTransform);
					pinData.m_uiElement.SetParent(rectTransform);
					float size = pinData.m_doubleSize ? (num * 2f) : num;
					pinData.m_uiElement.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
					pinData.m_uiElement.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
					pinData.m_checkedElement = gameObject.transform.Find("Checked").gameObject;
					pinData.m_nameElement = gameObject.transform.Find("Name").GetComponent<Text>();
				}
				float mx;
				float my;
				this.WorldToMapPoint(pinData.m_pos, out mx, out my);
				Vector2 anchoredPosition = this.MapPointToLocalGuiPos(mx, my, rawImage);
				pinData.m_uiElement.anchoredPosition = anchoredPosition;
				if (pinData.m_animate)
				{
					float num2 = pinData.m_doubleSize ? (num * 2f) : num;
					num2 *= 0.8f + Mathf.Sin(Time.time * 5f) * 0.2f;
					pinData.m_uiElement.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num2);
					pinData.m_uiElement.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num2);
				}
				if (pinData.m_worldSize > 0f)
				{
					Vector2 size2 = new Vector2(pinData.m_worldSize / this.m_pixelSize / (float)this.m_textureSize, pinData.m_worldSize / this.m_pixelSize / (float)this.m_textureSize);
					Vector2 vector = this.MapSizeToLocalGuiSize(size2, rawImage);
					pinData.m_uiElement.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, vector.x);
					pinData.m_uiElement.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, vector.y);
				}
				pinData.m_checkedElement.SetActive(pinData.m_checked);
				if (pinData.m_name.Length > 0 && this.m_mode == Minimap.MapMode.Large && this.m_largeZoom < this.m_showNamesZoom)
				{
					pinData.m_nameElement.gameObject.SetActive(true);
					pinData.m_nameElement.text = Localization.instance.Localize(pinData.m_name);
				}
				else
				{
					pinData.m_nameElement.gameObject.SetActive(false);
				}
			}
			else if (pinData.m_uiElement != null)
			{
				UnityEngine.Object.Destroy(pinData.m_uiElement.gameObject);
				pinData.m_uiElement = null;
			}
		}
	}

	// Token: 0x060005B6 RID: 1462 RVA: 0x00031638 File Offset: 0x0002F838
	private void UpdateWindMarker()
	{
		Quaternion quaternion = Quaternion.LookRotation(EnvMan.instance.GetWindDir());
		this.m_windMarker.rotation = Quaternion.Euler(0f, 0f, -quaternion.eulerAngles.y);
	}

	// Token: 0x060005B7 RID: 1463 RVA: 0x0003167C File Offset: 0x0002F87C
	private void UpdatePlayerMarker(Player player, Quaternion playerRot)
	{
		Vector3 position = player.transform.position;
		Vector3 eulerAngles = playerRot.eulerAngles;
		this.m_smallMarker.rotation = Quaternion.Euler(0f, 0f, -eulerAngles.y);
		if (this.m_mode == Minimap.MapMode.Large && this.IsPointVisible(position, this.m_mapImageLarge))
		{
			this.m_largeMarker.gameObject.SetActive(true);
			this.m_largeMarker.rotation = this.m_smallMarker.rotation;
			float mx;
			float my;
			this.WorldToMapPoint(position, out mx, out my);
			Vector2 anchoredPosition = this.MapPointToLocalGuiPos(mx, my, this.m_mapImageLarge);
			this.m_largeMarker.anchoredPosition = anchoredPosition;
		}
		else
		{
			this.m_largeMarker.gameObject.SetActive(false);
		}
		Ship controlledShip = player.GetControlledShip();
		if (controlledShip)
		{
			this.m_smallShipMarker.gameObject.SetActive(true);
			Vector3 eulerAngles2 = controlledShip.transform.rotation.eulerAngles;
			this.m_smallShipMarker.rotation = Quaternion.Euler(0f, 0f, -eulerAngles2.y);
			if (this.m_mode == Minimap.MapMode.Large)
			{
				this.m_largeShipMarker.gameObject.SetActive(true);
				Vector3 position2 = controlledShip.transform.position;
				float mx2;
				float my2;
				this.WorldToMapPoint(position2, out mx2, out my2);
				Vector2 anchoredPosition2 = this.MapPointToLocalGuiPos(mx2, my2, this.m_mapImageLarge);
				this.m_largeShipMarker.anchoredPosition = anchoredPosition2;
				this.m_largeShipMarker.rotation = this.m_smallShipMarker.rotation;
				return;
			}
		}
		else
		{
			this.m_smallShipMarker.gameObject.SetActive(false);
			this.m_largeShipMarker.gameObject.SetActive(false);
		}
	}

	// Token: 0x060005B8 RID: 1464 RVA: 0x00031824 File Offset: 0x0002FA24
	private Vector2 MapPointToLocalGuiPos(float mx, float my, RawImage img)
	{
		Vector2 result = default(Vector2);
		result.x = (mx - img.uvRect.xMin) / img.uvRect.width;
		result.y = (my - img.uvRect.yMin) / img.uvRect.height;
		result.x *= img.rectTransform.rect.width;
		result.y *= img.rectTransform.rect.height;
		return result;
	}

	// Token: 0x060005B9 RID: 1465 RVA: 0x000318C4 File Offset: 0x0002FAC4
	private Vector2 MapSizeToLocalGuiSize(Vector2 size, RawImage img)
	{
		size.x /= img.uvRect.width;
		size.y /= img.uvRect.height;
		return new Vector2(size.x * img.rectTransform.rect.width, size.y * img.rectTransform.rect.height);
	}

	// Token: 0x060005BA RID: 1466 RVA: 0x0003193C File Offset: 0x0002FB3C
	private bool IsPointVisible(Vector3 p, RawImage map)
	{
		float num;
		float num2;
		this.WorldToMapPoint(p, out num, out num2);
		return num > map.uvRect.xMin && num < map.uvRect.xMax && num2 > map.uvRect.yMin && num2 < map.uvRect.yMax;
	}

	// Token: 0x060005BB RID: 1467 RVA: 0x0003199C File Offset: 0x0002FB9C
	public void ExploreAll()
	{
		for (int i = 0; i < this.m_textureSize; i++)
		{
			for (int j = 0; j < this.m_textureSize; j++)
			{
				this.Explore(j, i);
			}
		}
		this.m_fogTexture.Apply();
	}

	// Token: 0x060005BC RID: 1468 RVA: 0x000319E0 File Offset: 0x0002FBE0
	private void WorldToMapPoint(Vector3 p, out float mx, out float my)
	{
		int num = this.m_textureSize / 2;
		mx = p.x / this.m_pixelSize + (float)num;
		my = p.z / this.m_pixelSize + (float)num;
		mx /= (float)this.m_textureSize;
		my /= (float)this.m_textureSize;
	}

	// Token: 0x060005BD RID: 1469 RVA: 0x00031A34 File Offset: 0x0002FC34
	private Vector3 MapPointToWorld(float mx, float my)
	{
		int num = this.m_textureSize / 2;
		mx *= (float)this.m_textureSize;
		my *= (float)this.m_textureSize;
		mx -= (float)num;
		my -= (float)num;
		mx *= this.m_pixelSize;
		my *= this.m_pixelSize;
		return new Vector3(mx, 0f, my);
	}

	// Token: 0x060005BE RID: 1470 RVA: 0x00031A8C File Offset: 0x0002FC8C
	private void WorldToPixel(Vector3 p, out int px, out int py)
	{
		int num = this.m_textureSize / 2;
		px = Mathf.RoundToInt(p.x / this.m_pixelSize + (float)num);
		py = Mathf.RoundToInt(p.z / this.m_pixelSize + (float)num);
	}

	// Token: 0x060005BF RID: 1471 RVA: 0x00031AD0 File Offset: 0x0002FCD0
	private void UpdateExplore(float dt, Player player)
	{
		this.m_exploreTimer += Time.deltaTime;
		if (this.m_exploreTimer > this.m_exploreInterval)
		{
			this.m_exploreTimer = 0f;
			this.Explore(player.transform.position, this.m_exploreRadius);
		}
	}

	// Token: 0x060005C0 RID: 1472 RVA: 0x00031B20 File Offset: 0x0002FD20
	private void Explore(Vector3 p, float radius)
	{
		int num = (int)Mathf.Ceil(radius / this.m_pixelSize);
		bool flag = false;
		int num2;
		int num3;
		this.WorldToPixel(p, out num2, out num3);
		for (int i = num3 - num; i <= num3 + num; i++)
		{
			for (int j = num2 - num; j <= num2 + num; j++)
			{
				if (j >= 0 && i >= 0 && j < this.m_textureSize && i < this.m_textureSize && new Vector2((float)(j - num2), (float)(i - num3)).magnitude <= (float)num && this.Explore(j, i))
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			this.m_fogTexture.Apply();
		}
	}

	// Token: 0x060005C1 RID: 1473 RVA: 0x00031BC8 File Offset: 0x0002FDC8
	private bool Explore(int x, int y)
	{
		if (this.m_explored[y * this.m_textureSize + x])
		{
			return false;
		}
		this.m_fogTexture.SetPixel(x, y, new Color32(0, 0, 0, 0));
		this.m_explored[y * this.m_textureSize + x] = true;
		return true;
	}

	// Token: 0x060005C2 RID: 1474 RVA: 0x00031C18 File Offset: 0x0002FE18
	private bool IsExplored(Vector3 worldPos)
	{
		int num;
		int num2;
		this.WorldToPixel(worldPos, out num, out num2);
		return num >= 0 && num < this.m_textureSize && num2 >= 0 && num2 < this.m_textureSize && this.m_explored[num2 * this.m_textureSize + num];
	}

	// Token: 0x060005C3 RID: 1475 RVA: 0x00031C5D File Offset: 0x0002FE5D
	private float GetHeight(int x, int y)
	{
		return this.m_heightTexture.GetPixel(x, y).r;
	}

	// Token: 0x060005C4 RID: 1476 RVA: 0x00031C74 File Offset: 0x0002FE74
	private void GenerateWorldMap()
	{
		int num = this.m_textureSize / 2;
		float num2 = this.m_pixelSize / 2f;
		Color32[] array = new Color32[this.m_textureSize * this.m_textureSize];
		Color32[] array2 = new Color32[this.m_textureSize * this.m_textureSize];
		Color[] array3 = new Color[this.m_textureSize * this.m_textureSize];
		for (int i = 0; i < this.m_textureSize; i++)
		{
			for (int j = 0; j < this.m_textureSize; j++)
			{
				float wx = (float)(j - num) * this.m_pixelSize + num2;
				float wy = (float)(i - num) * this.m_pixelSize + num2;
				Heightmap.Biome biome = WorldGenerator.instance.GetBiome(wx, wy);
				float biomeHeight = WorldGenerator.instance.GetBiomeHeight(biome, wx, wy);
				array[i * this.m_textureSize + j] = this.GetPixelColor(biome);
				array2[i * this.m_textureSize + j] = this.GetMaskColor(wx, wy, biomeHeight, biome);
				array3[i * this.m_textureSize + j] = new Color(biomeHeight, 0f, 0f);
			}
		}
		this.m_forestMaskTexture.SetPixels32(array2);
		this.m_forestMaskTexture.Apply();
		this.m_mapTexture.SetPixels32(array);
		this.m_mapTexture.Apply();
		this.m_heightTexture.SetPixels(array3);
		this.m_heightTexture.Apply();
	}

	// Token: 0x060005C5 RID: 1477 RVA: 0x00031DF8 File Offset: 0x0002FFF8
	private Color GetMaskColor(float wx, float wy, float height, Heightmap.Biome biome)
	{
		if (height < ZoneSystem.instance.m_waterLevel)
		{
			return this.noForest;
		}
		if (biome == Heightmap.Biome.Meadows)
		{
			if (!WorldGenerator.InForest(new Vector3(wx, 0f, wy)))
			{
				return this.noForest;
			}
			return this.forest;
		}
		else if (biome == Heightmap.Biome.Plains)
		{
			if (WorldGenerator.GetForestFactor(new Vector3(wx, 0f, wy)) >= 0.8f)
			{
				return this.noForest;
			}
			return this.forest;
		}
		else
		{
			if (biome == Heightmap.Biome.BlackForest || biome == Heightmap.Biome.Mistlands)
			{
				return this.forest;
			}
			return this.noForest;
		}
	}

	// Token: 0x060005C6 RID: 1478 RVA: 0x00031E88 File Offset: 0x00030088
	private Color GetPixelColor(Heightmap.Biome biome)
	{
		if (biome <= Heightmap.Biome.Plains)
		{
			switch (biome)
			{
			case Heightmap.Biome.Meadows:
				return this.m_meadowsColor;
			case Heightmap.Biome.Swamp:
				return this.m_swampColor;
			case (Heightmap.Biome)3:
				break;
			case Heightmap.Biome.Mountain:
				return this.m_mountainColor;
			default:
				if (biome == Heightmap.Biome.BlackForest)
				{
					return this.m_blackforestColor;
				}
				if (biome == Heightmap.Biome.Plains)
				{
					return this.m_heathColor;
				}
				break;
			}
		}
		else if (biome <= Heightmap.Biome.DeepNorth)
		{
			if (biome == Heightmap.Biome.AshLands)
			{
				return this.m_ashlandsColor;
			}
			if (biome == Heightmap.Biome.DeepNorth)
			{
				return this.m_deepnorthColor;
			}
		}
		else
		{
			if (biome == Heightmap.Biome.Ocean)
			{
				return Color.white;
			}
			if (biome == Heightmap.Biome.Mistlands)
			{
				return this.m_mistlandsColor;
			}
		}
		return Color.white;
	}

	// Token: 0x060005C7 RID: 1479 RVA: 0x00031F38 File Offset: 0x00030138
	private void LoadMapData()
	{
		PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
		if (playerProfile.GetMapData() != null)
		{
			this.SetMapData(playerProfile.GetMapData());
		}
	}

	// Token: 0x060005C8 RID: 1480 RVA: 0x00031F64 File Offset: 0x00030164
	public void SaveMapData()
	{
		Game.instance.GetPlayerProfile().SetMapData(this.GetMapData());
	}

	// Token: 0x060005C9 RID: 1481 RVA: 0x00031F7C File Offset: 0x0003017C
	private byte[] GetMapData()
	{
		ZPackage zpackage = new ZPackage();
		zpackage.Write(Minimap.MAPVERSION);
		zpackage.Write(this.m_textureSize);
		for (int i = 0; i < this.m_explored.Length; i++)
		{
			zpackage.Write(this.m_explored[i]);
		}
		int num = 0;
		using (List<Minimap.PinData>.Enumerator enumerator = this.m_pins.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.m_save)
				{
					num++;
				}
			}
		}
		zpackage.Write(num);
		foreach (Minimap.PinData pinData in this.m_pins)
		{
			if (pinData.m_save)
			{
				zpackage.Write(pinData.m_name);
				zpackage.Write(pinData.m_pos);
				zpackage.Write((int)pinData.m_type);
				zpackage.Write(pinData.m_checked);
			}
		}
		zpackage.Write(ZNet.instance.IsReferencePositionPublic());
		return zpackage.GetArray();
	}

	// Token: 0x060005CA RID: 1482 RVA: 0x000320A8 File Offset: 0x000302A8
	private void SetMapData(byte[] data)
	{
		ZPackage zpackage = new ZPackage(data);
		int num = zpackage.ReadInt();
		int num2 = zpackage.ReadInt();
		if (this.m_textureSize != num2)
		{
			ZLog.LogWarning(string.Concat(new object[]
			{
				"Missmatching mapsize ",
				this.m_mapTexture,
				" vs ",
				num2
			}));
			return;
		}
		this.Reset();
		for (int i = 0; i < this.m_explored.Length; i++)
		{
			if (zpackage.ReadBool())
			{
				int x = i % num2;
				int y = i / num2;
				this.Explore(x, y);
			}
		}
		if (num >= 2)
		{
			int num3 = zpackage.ReadInt();
			this.ClearPins();
			for (int j = 0; j < num3; j++)
			{
				string name = zpackage.ReadString();
				Vector3 pos = zpackage.ReadVector3();
				Minimap.PinType type = (Minimap.PinType)zpackage.ReadInt();
				bool isChecked = num >= 3 && zpackage.ReadBool();
				this.AddPin(pos, type, name, true, isChecked);
			}
		}
		if (num >= 4)
		{
			bool publicReferencePosition = zpackage.ReadBool();
			ZNet.instance.SetPublicReferencePosition(publicReferencePosition);
		}
		this.m_fogTexture.Apply();
	}

	// Token: 0x060005CB RID: 1483 RVA: 0x000321BC File Offset: 0x000303BC
	public bool RemovePin(Vector3 pos, float radius)
	{
		Minimap.PinData closestPin = this.GetClosestPin(pos, radius);
		if (closestPin != null)
		{
			this.RemovePin(closestPin);
			return true;
		}
		return false;
	}

	// Token: 0x060005CC RID: 1484 RVA: 0x000321E0 File Offset: 0x000303E0
	private Minimap.PinData GetClosestPin(Vector3 pos, float radius)
	{
		Minimap.PinData pinData = null;
		float num = 999999f;
		foreach (Minimap.PinData pinData2 in this.m_pins)
		{
			if (pinData2.m_save)
			{
				float num2 = Utils.DistanceXZ(pos, pinData2.m_pos);
				if (num2 < radius && (num2 < num || pinData == null))
				{
					pinData = pinData2;
					num = num2;
				}
			}
		}
		return pinData;
	}

	// Token: 0x060005CD RID: 1485 RVA: 0x00032260 File Offset: 0x00030460
	public void RemovePin(Minimap.PinData pin)
	{
		if (pin.m_uiElement)
		{
			UnityEngine.Object.Destroy(pin.m_uiElement.gameObject);
		}
		this.m_pins.Remove(pin);
	}

	// Token: 0x060005CE RID: 1486 RVA: 0x0003228C File Offset: 0x0003048C
	public void ShowPointOnMap(Vector3 point)
	{
		if (Player.m_localPlayer == null)
		{
			return;
		}
		this.SetMapMode(Minimap.MapMode.Large);
		this.m_mapOffset = point - Player.m_localPlayer.transform.position;
	}

	// Token: 0x060005CF RID: 1487 RVA: 0x000322C0 File Offset: 0x000304C0
	public bool DiscoverLocation(Vector3 pos, Minimap.PinType type, string name)
	{
		if (Player.m_localPlayer == null)
		{
			return false;
		}
		if (this.HaveSimilarPin(pos, type, name, true))
		{
			Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_pin_exist", 0, null);
			this.ShowPointOnMap(pos);
			return false;
		}
		Sprite sprite = this.GetSprite(type);
		Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "$msg_pin_added: " + name, 0, sprite);
		this.AddPin(pos, type, name, true, false);
		this.ShowPointOnMap(pos);
		return true;
	}

	// Token: 0x060005D0 RID: 1488 RVA: 0x00032338 File Offset: 0x00030538
	private bool HaveSimilarPin(Vector3 pos, Minimap.PinType type, string name, bool save)
	{
		foreach (Minimap.PinData pinData in this.m_pins)
		{
			if (pinData.m_name == name && pinData.m_type == type && pinData.m_save == save && Utils.DistanceXZ(pos, pinData.m_pos) < 1f)
			{
				return true;
			}
		}
		return false;
	}

	// Token: 0x060005D1 RID: 1489 RVA: 0x000323C0 File Offset: 0x000305C0
	public Minimap.PinData AddPin(Vector3 pos, Minimap.PinType type, string name, bool save, bool isChecked)
	{
		Minimap.PinData pinData = new Minimap.PinData();
		pinData.m_type = type;
		pinData.m_name = name;
		pinData.m_pos = pos;
		pinData.m_icon = this.GetSprite(type);
		pinData.m_save = save;
		pinData.m_checked = isChecked;
		this.m_pins.Add(pinData);
		return pinData;
	}

	// Token: 0x060005D2 RID: 1490 RVA: 0x00032414 File Offset: 0x00030614
	private Sprite GetSprite(Minimap.PinType type)
	{
		if (type == Minimap.PinType.None)
		{
			return null;
		}
		return this.m_icons.Find((Minimap.SpriteData x) => x.m_name == type).m_icon;
	}

	// Token: 0x060005D3 RID: 1491 RVA: 0x00032458 File Offset: 0x00030658
	private Vector3 GetViewCenterWorldPoint()
	{
		Rect uvRect = this.m_mapImageLarge.uvRect;
		float mx = uvRect.xMin + 0.5f * uvRect.width;
		float my = uvRect.yMin + 0.5f * uvRect.height;
		return this.MapPointToWorld(mx, my);
	}

	// Token: 0x060005D4 RID: 1492 RVA: 0x000324A8 File Offset: 0x000306A8
	private Vector3 ScreenToWorldPoint(Vector3 mousePos)
	{
		Vector2 screenPoint = mousePos;
		RectTransform rectTransform = this.m_mapImageLarge.transform as RectTransform;
		Vector2 point;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, null, out point))
		{
			Vector2 vector = Rect.PointToNormalized(rectTransform.rect, point);
			Rect uvRect = this.m_mapImageLarge.uvRect;
			float mx = uvRect.xMin + vector.x * uvRect.width;
			float my = uvRect.yMin + vector.y * uvRect.height;
			return this.MapPointToWorld(mx, my);
		}
		return Vector3.zero;
	}

	// Token: 0x060005D5 RID: 1493 RVA: 0x00032534 File Offset: 0x00030734
	private void OnMapLeftDown(UIInputHandler handler)
	{
		if (Time.time - this.m_leftClickTime < 0.3f)
		{
			this.OnMapDblClick();
			this.m_leftClickTime = 0f;
			this.m_leftDownTime = 0f;
			return;
		}
		this.m_leftClickTime = Time.time;
		this.m_leftDownTime = Time.time;
	}

	// Token: 0x060005D6 RID: 1494 RVA: 0x00032587 File Offset: 0x00030787
	private void OnMapLeftUp(UIInputHandler handler)
	{
		if (this.m_leftDownTime != 0f)
		{
			if (Time.time - this.m_leftDownTime < this.m_clickDuration)
			{
				this.OnMapLeftClick();
			}
			this.m_leftDownTime = 0f;
		}
		this.m_dragView = false;
	}

	// Token: 0x060005D7 RID: 1495 RVA: 0x000325C4 File Offset: 0x000307C4
	public void OnMapDblClick()
	{
		Vector3 pos = this.ScreenToWorldPoint(Input.mousePosition);
		Minimap.PinData pin = this.AddPin(pos, this.m_selectedType, "", true, false);
		this.ShowPinNameInput(pin);
	}

	// Token: 0x060005D8 RID: 1496 RVA: 0x000325FC File Offset: 0x000307FC
	public void OnMapLeftClick()
	{
		ZLog.Log("Left click");
		Vector3 pos = this.ScreenToWorldPoint(Input.mousePosition);
		Minimap.PinData closestPin = this.GetClosestPin(pos, this.m_removeRadius * (this.m_largeZoom * 2f));
		if (closestPin != null)
		{
			closestPin.m_checked = !closestPin.m_checked;
		}
	}

	// Token: 0x060005D9 RID: 1497 RVA: 0x0003264C File Offset: 0x0003084C
	public void OnMapMiddleClick(UIInputHandler handler)
	{
		Vector3 position = this.ScreenToWorldPoint(Input.mousePosition);
		Chat.instance.SendPing(position);
	}

	// Token: 0x060005DA RID: 1498 RVA: 0x00032670 File Offset: 0x00030870
	public void OnMapRightClick(UIInputHandler handler)
	{
		ZLog.Log("Right click");
		Vector3 pos = this.ScreenToWorldPoint(Input.mousePosition);
		this.RemovePin(pos, this.m_removeRadius * (this.m_largeZoom * 2f));
		this.m_namePin = null;
	}

	// Token: 0x060005DB RID: 1499 RVA: 0x000326B5 File Offset: 0x000308B5
	public void OnPressedIcon0()
	{
		this.SelectIcon(Minimap.PinType.Icon0);
	}

	// Token: 0x060005DC RID: 1500 RVA: 0x000326BE File Offset: 0x000308BE
	public void OnPressedIcon1()
	{
		this.SelectIcon(Minimap.PinType.Icon1);
	}

	// Token: 0x060005DD RID: 1501 RVA: 0x000326C7 File Offset: 0x000308C7
	public void OnPressedIcon2()
	{
		this.SelectIcon(Minimap.PinType.Icon2);
	}

	// Token: 0x060005DE RID: 1502 RVA: 0x000326D0 File Offset: 0x000308D0
	public void OnPressedIcon3()
	{
		this.SelectIcon(Minimap.PinType.Icon3);
	}

	// Token: 0x060005DF RID: 1503 RVA: 0x000326D9 File Offset: 0x000308D9
	public void OnPressedIcon4()
	{
		this.SelectIcon(Minimap.PinType.Icon4);
	}

	// Token: 0x060005E0 RID: 1504 RVA: 0x000326E2 File Offset: 0x000308E2
	public void OnTogglePublicPosition()
	{
		ZNet.instance.SetPublicReferencePosition(this.m_publicPosition.isOn);
	}

	// Token: 0x060005E1 RID: 1505 RVA: 0x000326FC File Offset: 0x000308FC
	private void SelectIcon(Minimap.PinType type)
	{
		this.m_selectedType = type;
		this.m_selectedIcon0.enabled = false;
		this.m_selectedIcon1.enabled = false;
		this.m_selectedIcon2.enabled = false;
		this.m_selectedIcon3.enabled = false;
		this.m_selectedIcon4.enabled = false;
		switch (type)
		{
		case Minimap.PinType.Icon0:
			this.m_selectedIcon0.enabled = true;
			return;
		case Minimap.PinType.Icon1:
			this.m_selectedIcon1.enabled = true;
			return;
		case Minimap.PinType.Icon2:
			this.m_selectedIcon2.enabled = true;
			return;
		case Minimap.PinType.Icon3:
			this.m_selectedIcon3.enabled = true;
			return;
		case Minimap.PinType.Death:
		case Minimap.PinType.Bed:
			break;
		case Minimap.PinType.Icon4:
			this.m_selectedIcon4.enabled = true;
			break;
		default:
			return;
		}
	}

	// Token: 0x060005E2 RID: 1506 RVA: 0x000327B0 File Offset: 0x000309B0
	private void ClearPins()
	{
		foreach (Minimap.PinData pinData in this.m_pins)
		{
			if (pinData.m_uiElement != null)
			{
				UnityEngine.Object.Destroy(pinData.m_uiElement);
			}
		}
		this.m_pins.Clear();
		this.m_deathPin = null;
	}

	// Token: 0x060005E3 RID: 1507 RVA: 0x00032828 File Offset: 0x00030A28
	private void UpdateBiome(Player player)
	{
		if (this.m_mode != Minimap.MapMode.Large || !ZInput.IsMouseActive())
		{
			Heightmap.Biome currentBiome = player.GetCurrentBiome();
			if (currentBiome != this.m_biome)
			{
				this.m_biome = currentBiome;
				string text = Localization.instance.Localize("$biome_" + currentBiome.ToString().ToLower());
				this.m_biomeNameSmall.text = text;
				this.m_biomeNameLarge.text = text;
				this.m_biomeNameSmall.GetComponent<Animator>().SetTrigger("pulse");
			}
			return;
		}
		Vector3 vector = this.ScreenToWorldPoint(Input.mousePosition);
		if (this.IsExplored(vector))
		{
			Heightmap.Biome biome = WorldGenerator.instance.GetBiome(vector);
			string text2 = Localization.instance.Localize("$biome_" + biome.ToString().ToLower());
			this.m_biomeNameLarge.text = text2;
			return;
		}
		this.m_biomeNameLarge.text = "";
	}

	// Token: 0x04000643 RID: 1603
	private Color forest = new Color(1f, 0f, 0f, 0f);

	// Token: 0x04000644 RID: 1604
	private Color noForest = new Color(0f, 0f, 0f, 0f);

	// Token: 0x04000645 RID: 1605
	private static int MAPVERSION = 4;

	// Token: 0x04000646 RID: 1606
	private static Minimap m_instance;

	// Token: 0x04000647 RID: 1607
	public GameObject m_smallRoot;

	// Token: 0x04000648 RID: 1608
	public GameObject m_largeRoot;

	// Token: 0x04000649 RID: 1609
	public RawImage m_mapImageSmall;

	// Token: 0x0400064A RID: 1610
	public RawImage m_mapImageLarge;

	// Token: 0x0400064B RID: 1611
	public RectTransform m_pinRootSmall;

	// Token: 0x0400064C RID: 1612
	public RectTransform m_pinRootLarge;

	// Token: 0x0400064D RID: 1613
	public Text m_biomeNameSmall;

	// Token: 0x0400064E RID: 1614
	public Text m_biomeNameLarge;

	// Token: 0x0400064F RID: 1615
	public RectTransform m_smallShipMarker;

	// Token: 0x04000650 RID: 1616
	public RectTransform m_largeShipMarker;

	// Token: 0x04000651 RID: 1617
	public RectTransform m_smallMarker;

	// Token: 0x04000652 RID: 1618
	public RectTransform m_largeMarker;

	// Token: 0x04000653 RID: 1619
	public RectTransform m_windMarker;

	// Token: 0x04000654 RID: 1620
	public RectTransform m_gamepadCrosshair;

	// Token: 0x04000655 RID: 1621
	public Toggle m_publicPosition;

	// Token: 0x04000656 RID: 1622
	public Image m_selectedIcon0;

	// Token: 0x04000657 RID: 1623
	public Image m_selectedIcon1;

	// Token: 0x04000658 RID: 1624
	public Image m_selectedIcon2;

	// Token: 0x04000659 RID: 1625
	public Image m_selectedIcon3;

	// Token: 0x0400065A RID: 1626
	public Image m_selectedIcon4;

	// Token: 0x0400065B RID: 1627
	public GameObject m_pinPrefab;

	// Token: 0x0400065C RID: 1628
	public InputField m_nameInput;

	// Token: 0x0400065D RID: 1629
	public int m_textureSize = 256;

	// Token: 0x0400065E RID: 1630
	public float m_pixelSize = 64f;

	// Token: 0x0400065F RID: 1631
	public float m_minZoom = 0.01f;

	// Token: 0x04000660 RID: 1632
	public float m_maxZoom = 1f;

	// Token: 0x04000661 RID: 1633
	public float m_showNamesZoom = 0.5f;

	// Token: 0x04000662 RID: 1634
	public float m_exploreInterval = 2f;

	// Token: 0x04000663 RID: 1635
	public float m_exploreRadius = 100f;

	// Token: 0x04000664 RID: 1636
	public float m_removeRadius = 128f;

	// Token: 0x04000665 RID: 1637
	public float m_pinSizeSmall = 32f;

	// Token: 0x04000666 RID: 1638
	public float m_pinSizeLarge = 48f;

	// Token: 0x04000667 RID: 1639
	public float m_clickDuration = 0.25f;

	// Token: 0x04000668 RID: 1640
	public List<Minimap.SpriteData> m_icons = new List<Minimap.SpriteData>();

	// Token: 0x04000669 RID: 1641
	public List<Minimap.LocationSpriteData> m_locationIcons = new List<Minimap.LocationSpriteData>();

	// Token: 0x0400066A RID: 1642
	public Color m_meadowsColor = new Color(0.45f, 1f, 0.43f);

	// Token: 0x0400066B RID: 1643
	public Color m_ashlandsColor = new Color(1f, 0.2f, 0.2f);

	// Token: 0x0400066C RID: 1644
	public Color m_blackforestColor = new Color(0f, 0.7f, 0f);

	// Token: 0x0400066D RID: 1645
	public Color m_deepnorthColor = new Color(1f, 1f, 1f);

	// Token: 0x0400066E RID: 1646
	public Color m_heathColor = new Color(1f, 1f, 0.2f);

	// Token: 0x0400066F RID: 1647
	public Color m_swampColor = new Color(0.6f, 0.5f, 0.5f);

	// Token: 0x04000670 RID: 1648
	public Color m_mountainColor = new Color(1f, 1f, 1f);

	// Token: 0x04000671 RID: 1649
	public Color m_mistlandsColor = new Color(0.5f, 0.5f, 0.5f);

	// Token: 0x04000672 RID: 1650
	private Minimap.PinData m_namePin;

	// Token: 0x04000673 RID: 1651
	private Minimap.PinType m_selectedType;

	// Token: 0x04000674 RID: 1652
	private Minimap.PinData m_deathPin;

	// Token: 0x04000675 RID: 1653
	private Minimap.PinData m_spawnPointPin;

	// Token: 0x04000676 RID: 1654
	private Dictionary<Vector3, Minimap.PinData> m_locationPins = new Dictionary<Vector3, Minimap.PinData>();

	// Token: 0x04000677 RID: 1655
	private float m_updateLocationsTimer;

	// Token: 0x04000678 RID: 1656
	private List<Minimap.PinData> m_pingPins = new List<Minimap.PinData>();

	// Token: 0x04000679 RID: 1657
	private List<Minimap.PinData> m_shoutPins = new List<Minimap.PinData>();

	// Token: 0x0400067A RID: 1658
	private List<Chat.WorldTextInstance> m_tempShouts = new List<Chat.WorldTextInstance>();

	// Token: 0x0400067B RID: 1659
	private List<Minimap.PinData> m_playerPins = new List<Minimap.PinData>();

	// Token: 0x0400067C RID: 1660
	private List<ZNet.PlayerInfo> m_tempPlayerInfo = new List<ZNet.PlayerInfo>();

	// Token: 0x0400067D RID: 1661
	private Minimap.PinData m_randEventPin;

	// Token: 0x0400067E RID: 1662
	private Minimap.PinData m_randEventAreaPin;

	// Token: 0x0400067F RID: 1663
	private float m_updateEventTime;

	// Token: 0x04000680 RID: 1664
	private bool[] m_explored;

	// Token: 0x04000681 RID: 1665
	private List<Minimap.PinData> m_pins = new List<Minimap.PinData>();

	// Token: 0x04000682 RID: 1666
	private Texture2D m_forestMaskTexture;

	// Token: 0x04000683 RID: 1667
	private Texture2D m_mapTexture;

	// Token: 0x04000684 RID: 1668
	private Texture2D m_heightTexture;

	// Token: 0x04000685 RID: 1669
	private Texture2D m_fogTexture;

	// Token: 0x04000686 RID: 1670
	private float m_largeZoom = 0.1f;

	// Token: 0x04000687 RID: 1671
	private float m_smallZoom = 0.01f;

	// Token: 0x04000688 RID: 1672
	private Heightmap.Biome m_biome;

	// Token: 0x04000689 RID: 1673
	private Minimap.MapMode m_mode = Minimap.MapMode.Small;

	// Token: 0x0400068A RID: 1674
	private float m_exploreTimer;

	// Token: 0x0400068B RID: 1675
	private bool m_hasGenerated;

	// Token: 0x0400068C RID: 1676
	private bool m_dragView = true;

	// Token: 0x0400068D RID: 1677
	private Vector3 m_mapOffset = Vector3.zero;

	// Token: 0x0400068E RID: 1678
	private float m_leftDownTime;

	// Token: 0x0400068F RID: 1679
	private float m_leftClickTime;

	// Token: 0x04000690 RID: 1680
	private Vector3 m_dragWorldPos = Vector3.zero;

	// Token: 0x04000691 RID: 1681
	private bool m_wasFocused;

	// Token: 0x02000153 RID: 339
	private enum MapMode
	{
		// Token: 0x0400110D RID: 4365
		None,
		// Token: 0x0400110E RID: 4366
		Small,
		// Token: 0x0400110F RID: 4367
		Large
	}

	// Token: 0x02000154 RID: 340
	public enum PinType
	{
		// Token: 0x04001111 RID: 4369
		Icon0,
		// Token: 0x04001112 RID: 4370
		Icon1,
		// Token: 0x04001113 RID: 4371
		Icon2,
		// Token: 0x04001114 RID: 4372
		Icon3,
		// Token: 0x04001115 RID: 4373
		Death,
		// Token: 0x04001116 RID: 4374
		Bed,
		// Token: 0x04001117 RID: 4375
		Icon4,
		// Token: 0x04001118 RID: 4376
		Shout,
		// Token: 0x04001119 RID: 4377
		None,
		// Token: 0x0400111A RID: 4378
		Boss,
		// Token: 0x0400111B RID: 4379
		Player,
		// Token: 0x0400111C RID: 4380
		RandomEvent,
		// Token: 0x0400111D RID: 4381
		Ping,
		// Token: 0x0400111E RID: 4382
		EventArea
	}

	// Token: 0x02000155 RID: 341
	public class PinData
	{
		// Token: 0x0400111F RID: 4383
		public string m_name;

		// Token: 0x04001120 RID: 4384
		public Minimap.PinType m_type;

		// Token: 0x04001121 RID: 4385
		public Sprite m_icon;

		// Token: 0x04001122 RID: 4386
		public Vector3 m_pos;

		// Token: 0x04001123 RID: 4387
		public bool m_save;

		// Token: 0x04001124 RID: 4388
		public bool m_checked;

		// Token: 0x04001125 RID: 4389
		public bool m_doubleSize;

		// Token: 0x04001126 RID: 4390
		public bool m_animate;

		// Token: 0x04001127 RID: 4391
		public float m_worldSize;

		// Token: 0x04001128 RID: 4392
		public RectTransform m_uiElement;

		// Token: 0x04001129 RID: 4393
		public GameObject m_checkedElement;

		// Token: 0x0400112A RID: 4394
		public Text m_nameElement;
	}

	// Token: 0x02000156 RID: 342
	[Serializable]
	public struct SpriteData
	{
		// Token: 0x0400112B RID: 4395
		public Minimap.PinType m_name;

		// Token: 0x0400112C RID: 4396
		public Sprite m_icon;
	}

	// Token: 0x02000157 RID: 343
	[Serializable]
	public struct LocationSpriteData
	{
		// Token: 0x0400112D RID: 4397
		public string m_name;

		// Token: 0x0400112E RID: 4398
		public Sprite m_icon;
	}
}
