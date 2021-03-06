﻿using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x0200009C RID: 156
public class FejdStartup : MonoBehaviour
{
	// Token: 0x17000025 RID: 37
	// (get) Token: 0x06000A51 RID: 2641 RVA: 0x0004AF06 File Offset: 0x00049106
	public static FejdStartup instance
	{
		get
		{
			return FejdStartup.m_instance;
		}
	}

	// Token: 0x06000A52 RID: 2642 RVA: 0x0004AF10 File Offset: 0x00049110
	private void Awake()
	{
		FejdStartup.m_instance = this;
		QualitySettings.maxQueuedFrames = 1;
		ZLog.Log("Valheim version:" + global::Version.GetVersionString());
		Settings.ApplyStartupSettings();
		WorldGenerator.Initialize(World.GetMenuWorld());
		if (!global::Console.instance)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.m_consolePrefab);
		}
		this.m_mainCamera.transform.position = this.m_cameraMarkerMain.transform.position;
		this.m_mainCamera.transform.rotation = this.m_cameraMarkerMain.transform.rotation;
		ZLog.Log("Render threading mode:" + SystemInfo.renderingThreadingMode);
		Gogan.StartSession();
		Gogan.LogEvent("Game", "Version", global::Version.GetVersionString(), 0L);
		Gogan.LogEvent("Game", "SteamID", SteamManager.APP_ID.ToString(), 0L);
		Gogan.LogEvent("Screen", "Enter", "StartMenu", 0L);
		this.ParseArguments();
		this.InitializeSteam();
	}

	// Token: 0x06000A53 RID: 2643 RVA: 0x0004B016 File Offset: 0x00049216
	private void OnDestroy()
	{
		FejdStartup.m_instance = null;
	}

	// Token: 0x06000A54 RID: 2644 RVA: 0x0004B020 File Offset: 0x00049220
	private void Start()
	{
		Application.targetFrameRate = 60;
		this.SetupGui();
		this.SetupObjectDB();
		ZInput.Initialize();
		MusicMan.instance.Reset();
		MusicMan.instance.TriggerMusic("menu");
		this.ShowConnectError();
		ZSteamMatchmaking.Initialize();
		base.InvokeRepeating("UpdateServerList", 0.5f, 0.5f);
		if (FejdStartup.m_firstStartup)
		{
			this.HandleStartupJoin();
		}
		this.m_menuAnimator.SetBool("FirstStartup", FejdStartup.m_firstStartup);
		FejdStartup.m_firstStartup = false;
		string @string = PlayerPrefs.GetString("profile");
		if (@string.Length > 0)
		{
			this.SetSelectedProfile(@string);
			return;
		}
		this.m_profiles = PlayerProfile.GetAllPlayerProfiles();
		if (this.m_profiles.Count > 0)
		{
			this.SetSelectedProfile(this.m_profiles[0].GetFilename());
			return;
		}
		this.UpdateCharacterList();
	}

	// Token: 0x06000A55 RID: 2645 RVA: 0x0004B0FC File Offset: 0x000492FC
	private void SetupGui()
	{
		this.HideAll();
		this.m_mainMenu.SetActive(true);
		if (SteamManager.APP_ID == 1223920U)
		{
			this.m_betaText.SetActive(true);
			if (!Debug.isDebugBuild && !this.AcceptedNDA())
			{
				this.m_ndaPanel.SetActive(true);
				this.m_mainMenu.SetActive(false);
			}
		}
		this.m_manualIPButton.gameObject.SetActive(true);
		this.m_serverListBaseSize = this.m_serverListRoot.rect.height;
		this.m_worldListBaseSize = this.m_worldListRoot.rect.height;
		this.m_versionLabel.text = "version " + global::Version.GetVersionString();
		Localization.instance.Localize(base.transform);
	}

	// Token: 0x06000A56 RID: 2646 RVA: 0x0004B1C8 File Offset: 0x000493C8
	private void HideAll()
	{
		this.m_worldVersionPanel.SetActive(false);
		this.m_playerVersionPanel.SetActive(false);
		this.m_newGameVersionPanel.SetActive(false);
		this.m_loading.SetActive(false);
		this.m_characterSelectScreen.SetActive(false);
		this.m_creditsPanel.SetActive(false);
		this.m_startGamePanel.SetActive(false);
		this.m_joinIPPanel.SetActive(false);
		this.m_createWorldPanel.SetActive(false);
		this.m_mainMenu.SetActive(false);
		this.m_ndaPanel.SetActive(false);
		this.m_betaText.SetActive(false);
	}

	// Token: 0x06000A57 RID: 2647 RVA: 0x0004B268 File Offset: 0x00049468
	private bool InitializeSteam()
	{
		if (SteamManager.Initialize())
		{
			string personaName = SteamFriends.GetPersonaName();
			ZLog.Log("Steam initialized, persona:" + personaName);
			return true;
		}
		ZLog.LogError("Steam is not initialized");
		Application.Quit();
		return false;
	}

	// Token: 0x06000A58 RID: 2648 RVA: 0x0004B2A4 File Offset: 0x000494A4
	private void HandleStartupJoin()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			string a = commandLineArgs[i];
			if (a == "+connect" && i < commandLineArgs.Length - 1)
			{
				string text = commandLineArgs[i + 1];
				ZLog.Log("JOIN " + text);
				ZSteamMatchmaking.instance.QueueServerJoin(text);
			}
			else if (a == "+connect_lobby" && i < commandLineArgs.Length - 1)
			{
				string s = commandLineArgs[i + 1];
				CSteamID lobbyID = new CSteamID(ulong.Parse(s));
				ZSteamMatchmaking.instance.QueueLobbyJoin(lobbyID);
			}
		}
	}

	// Token: 0x06000A59 RID: 2649 RVA: 0x0004B338 File Offset: 0x00049538
	private void ParseArguments()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i] == "-console")
			{
				global::Console.SetConsoleEnabled(true);
			}
		}
	}

	// Token: 0x06000A5A RID: 2650 RVA: 0x0004B370 File Offset: 0x00049570
	private bool ParseServerArguments()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		string text = "Dedicated";
		string password = "";
		string text2 = "";
		int num = 2456;
		bool flag = true;
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			string a = commandLineArgs[i];
			if (a == "-world")
			{
				string text3 = commandLineArgs[i + 1];
				if (text3 != "")
				{
					text = text3;
				}
				i++;
			}
			else if (a == "-name")
			{
				string text4 = commandLineArgs[i + 1];
				if (text4 != "")
				{
					text2 = text4;
				}
				i++;
			}
			else if (a == "-port")
			{
				string text5 = commandLineArgs[i + 1];
				if (text5 != "")
				{
					num = int.Parse(text5);
				}
				i++;
			}
			else if (a == "-password")
			{
				password = commandLineArgs[i + 1];
				i++;
			}
			else if (a == "-savedir")
			{
				Utils.SetSaveDataPath(commandLineArgs[i + 1]);
				i++;
			}
			else if (a == "-public")
			{
				string a2 = commandLineArgs[i + 1];
				if (a2 != "")
				{
					flag = (a2 == "1");
				}
				i++;
			}
		}
		if (text2 == "")
		{
			text2 = text;
		}
		World createWorld = World.GetCreateWorld(text);
		if (flag && !this.IsPublicPasswordValid(password, createWorld))
		{
			string publicPasswordError = this.GetPublicPasswordError(password, createWorld);
			ZLog.LogError("Error bad password:" + publicPasswordError);
			Application.Quit();
			return false;
		}
		ZNet.SetServer(true, true, flag, text2, password, createWorld);
		ZNet.ResetServerHost();
		SteamManager.SetServerPort(num);
		ZSteamSocket.SetDataPort(num);
		return true;
	}

	// Token: 0x06000A5B RID: 2651 RVA: 0x0004B538 File Offset: 0x00049738
	private void SetupObjectDB()
	{
		ObjectDB objectDB = base.gameObject.AddComponent<ObjectDB>();
		ObjectDB component = this.m_gameMainPrefab.GetComponent<ObjectDB>();
		objectDB.CopyOtherDB(component);
	}

	// Token: 0x06000A5C RID: 2652 RVA: 0x0004B564 File Offset: 0x00049764
	private void ShowConnectError()
	{
		ZNet.ConnectionStatus connectionStatus = ZNet.GetConnectionStatus();
		if (connectionStatus != ZNet.ConnectionStatus.Connected && connectionStatus != ZNet.ConnectionStatus.Connecting && connectionStatus != ZNet.ConnectionStatus.None)
		{
			this.m_connectionFailedPanel.SetActive(true);
			switch (connectionStatus)
			{
			case ZNet.ConnectionStatus.ErrorVersion:
				this.m_connectionFailedError.text = Localization.instance.Localize("$error_incompatibleversion");
				return;
			case ZNet.ConnectionStatus.ErrorDisconnected:
				this.m_connectionFailedError.text = Localization.instance.Localize("$error_disconnected");
				return;
			case ZNet.ConnectionStatus.ErrorConnectFailed:
				this.m_connectionFailedError.text = Localization.instance.Localize("$error_failedconnect");
				return;
			case ZNet.ConnectionStatus.ErrorPassword:
				this.m_connectionFailedError.text = Localization.instance.Localize("$error_password");
				return;
			case ZNet.ConnectionStatus.ErrorAlreadyConnected:
				this.m_connectionFailedError.text = Localization.instance.Localize("$error_alreadyconnected");
				return;
			case ZNet.ConnectionStatus.ErrorBanned:
				this.m_connectionFailedError.text = Localization.instance.Localize("$error_banned");
				return;
			case ZNet.ConnectionStatus.ErrorFull:
				this.m_connectionFailedError.text = Localization.instance.Localize("$error_serverfull");
				break;
			default:
				return;
			}
		}
	}

	// Token: 0x06000A5D RID: 2653 RVA: 0x0004B678 File Offset: 0x00049878
	public void OnNewVersionButtonDownload()
	{
		Application.OpenURL(this.m_downloadUrl);
		Application.Quit();
	}

	// Token: 0x06000A5E RID: 2654 RVA: 0x0004B68A File Offset: 0x0004988A
	public void OnNewVersionButtonContinue()
	{
		this.m_newGameVersionPanel.SetActive(false);
	}

	// Token: 0x06000A5F RID: 2655 RVA: 0x0004B698 File Offset: 0x00049898
	public void OnStartGame()
	{
		Gogan.LogEvent("Screen", "Enter", "StartGame", 0L);
		this.m_mainMenu.SetActive(false);
		this.ShowCharacterSelection();
	}

	// Token: 0x06000A60 RID: 2656 RVA: 0x0004B6C2 File Offset: 0x000498C2
	private void ShowStartGame()
	{
		this.m_mainMenu.SetActive(false);
		this.m_startGamePanel.SetActive(true);
		this.m_createWorldPanel.SetActive(false);
	}

	// Token: 0x06000A61 RID: 2657 RVA: 0x0004B6E8 File Offset: 0x000498E8
	public void OnSelectWorldTab()
	{
		this.UpdateWorldList(true);
		if (this.m_world == null)
		{
			string @string = PlayerPrefs.GetString("world");
			if (@string.Length > 0)
			{
				this.m_world = this.FindWorld(@string);
			}
			if (this.m_world == null)
			{
				this.m_world = ((this.m_worlds.Count > 0) ? this.m_worlds[0] : null);
			}
			if (this.m_world != null)
			{
				this.UpdateWorldList(true);
			}
		}
	}

	// Token: 0x06000A62 RID: 2658 RVA: 0x0004B760 File Offset: 0x00049960
	private World FindWorld(string name)
	{
		foreach (World world in this.m_worlds)
		{
			if (world.m_name == name)
			{
				return world;
			}
		}
		return null;
	}

	// Token: 0x06000A63 RID: 2659 RVA: 0x0004B7C4 File Offset: 0x000499C4
	private void UpdateWorldList(bool centerSelection)
	{
		this.m_worlds = World.GetWorldList();
		foreach (GameObject obj in this.m_worldListElements)
		{
			UnityEngine.Object.Destroy(obj);
		}
		this.m_worldListElements.Clear();
		float num = (float)this.m_worlds.Count * this.m_worldListElementStep;
		num = Mathf.Max(this.m_worldListBaseSize, num);
		this.m_worldListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num);
		for (int i = 0; i < this.m_worlds.Count; i++)
		{
			World world = this.m_worlds[i];
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_worldListElement, this.m_worldListRoot);
			gameObject.SetActive(true);
			(gameObject.transform as RectTransform).anchoredPosition = new Vector2(0f, (float)i * -this.m_worldListElementStep);
			gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(this.OnSelectWorld));
			Text component = gameObject.transform.Find("seed").GetComponent<Text>();
			component.text = "Seed:" + world.m_seedName;
			gameObject.transform.Find("name").GetComponent<Text>().text = world.m_name;
			if (world.m_loadError)
			{
				component.text = " [LOAD ERROR]";
			}
			else if (world.m_versionError)
			{
				component.text = " [BAD VERSION]";
			}
			RectTransform rectTransform = gameObject.transform.Find("selected") as RectTransform;
			bool flag = this.m_world != null && world.m_name == this.m_world.m_name;
			rectTransform.gameObject.SetActive(flag);
			if (flag && centerSelection)
			{
				this.m_worldListEnsureVisible.CenterOnItem(rectTransform);
			}
			this.m_worldListElements.Add(gameObject);
		}
	}

	// Token: 0x06000A64 RID: 2660 RVA: 0x0004B9C4 File Offset: 0x00049BC4
	public void OnWorldRemove()
	{
		if (this.m_world == null)
		{
			return;
		}
		this.m_removeWorldName.text = this.m_world.m_name;
		this.m_removeWorldDialog.SetActive(true);
	}

	// Token: 0x06000A65 RID: 2661 RVA: 0x0004B9F1 File Offset: 0x00049BF1
	public void OnButtonRemoveWorldYes()
	{
		World.RemoveWorld(this.m_world.m_name);
		this.m_world = null;
		this.SetSelectedWorld(0, true);
		this.m_removeWorldDialog.SetActive(false);
	}

	// Token: 0x06000A66 RID: 2662 RVA: 0x0004BA1E File Offset: 0x00049C1E
	public void OnButtonRemoveWorldNo()
	{
		this.m_removeWorldDialog.SetActive(false);
	}

	// Token: 0x06000A67 RID: 2663 RVA: 0x0004BA2C File Offset: 0x00049C2C
	private void OnSelectWorld()
	{
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		int index = this.FindSelectedWorld(currentSelectedGameObject);
		this.SetSelectedWorld(index, false);
	}

	// Token: 0x06000A68 RID: 2664 RVA: 0x0004BA54 File Offset: 0x00049C54
	private void SetSelectedWorld(int index, bool centerSelection)
	{
		if (this.m_worlds.Count == 0)
		{
			return;
		}
		index = Mathf.Clamp(index, 0, this.m_worlds.Count - 1);
		this.m_world = this.m_worlds[index];
		this.UpdateWorldList(centerSelection);
	}

	// Token: 0x06000A69 RID: 2665 RVA: 0x0004BA94 File Offset: 0x00049C94
	private int GetSelectedWorld()
	{
		if (this.m_world == null)
		{
			return -1;
		}
		for (int i = 0; i < this.m_worlds.Count; i++)
		{
			if (this.m_worlds[i].m_name == this.m_world.m_name)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x06000A6A RID: 2666 RVA: 0x0004BAE8 File Offset: 0x00049CE8
	private int FindSelectedWorld(GameObject button)
	{
		for (int i = 0; i < this.m_worldListElements.Count; i++)
		{
			if (this.m_worldListElements[i] == button)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x06000A6B RID: 2667 RVA: 0x0004BB22 File Offset: 0x00049D22
	public void OnWorldNew()
	{
		this.m_createWorldPanel.SetActive(true);
		this.m_newWorldName.text = "";
		this.m_newWorldSeed.text = World.GenerateSeed();
	}

	// Token: 0x06000A6C RID: 2668 RVA: 0x0004BB50 File Offset: 0x00049D50
	public void OnNewWorldDone()
	{
		string text = this.m_newWorldName.text;
		string text2 = this.m_newWorldSeed.text;
		if (World.HaveWorld(text))
		{
			return;
		}
		this.m_world = new World(text, text2);
		this.m_world.SaveWorldMetaData();
		this.UpdateWorldList(true);
		this.ShowStartGame();
		Gogan.LogEvent("Menu", "NewWorld", text, 0L);
	}

	// Token: 0x06000A6D RID: 2669 RVA: 0x0004BBB5 File Offset: 0x00049DB5
	public void OnNewWorldBack()
	{
		this.ShowStartGame();
	}

	// Token: 0x06000A6E RID: 2670 RVA: 0x0004BBC0 File Offset: 0x00049DC0
	public void OnWorldStart()
	{
		if (this.m_world == null || this.m_world.m_versionError || this.m_world.m_loadError)
		{
			return;
		}
		PlayerPrefs.SetString("world", this.m_world.m_name);
		bool isOn = this.m_publicServerToggle.isOn;
		bool isOn2 = this.m_openServerToggle.isOn;
		string text = this.m_serverPassword.text;
		ZNet.SetServer(true, isOn2, isOn, this.m_world.m_name, text, this.m_world);
		ZNet.ResetServerHost();
		string eventLabel = "open:" + isOn2.ToString() + ",public:" + isOn.ToString();
		Gogan.LogEvent("Menu", "WorldStart", eventLabel, 0L);
		this.TransitionToMainScene();
	}

	// Token: 0x06000A6F RID: 2671 RVA: 0x0004BC80 File Offset: 0x00049E80
	private void ShowCharacterSelection()
	{
		Gogan.LogEvent("Screen", "Enter", "CharacterSelection", 0L);
		ZLog.Log("show character selection");
		this.m_characterSelectScreen.SetActive(true);
		this.m_selectCharacterPanel.SetActive(true);
		this.m_newCharacterPanel.SetActive(false);
	}

	// Token: 0x06000A70 RID: 2672 RVA: 0x0004BCD4 File Offset: 0x00049ED4
	public void OnServerFilterChanged()
	{
		ZSteamMatchmaking.instance.SetNameFilter(this.m_filterInputField.text);
		ZSteamMatchmaking.instance.SetFriendFilter(this.m_friendFilterSwitch.isOn);
		PlayerPrefs.SetInt("publicfilter", this.m_publicFilterSwitch.isOn ? 1 : 0);
	}

	// Token: 0x06000A71 RID: 2673 RVA: 0x0004BD28 File Offset: 0x00049F28
	public void RequestServerList()
	{
		ZLog.DevLog("Request serverlist");
		if (!this.m_serverRefreshButton.interactable)
		{
			ZLog.DevLog("Server queue already running");
			return;
		}
		this.m_serverRefreshButton.interactable = false;
		this.m_lastServerListRequesTime = Time.time;
		ZSteamMatchmaking.instance.RequestServerlist();
	}

	// Token: 0x06000A72 RID: 2674 RVA: 0x0004BD78 File Offset: 0x00049F78
	private void UpdateServerList()
	{
		this.m_serverRefreshButton.interactable = (Time.time - this.m_lastServerListRequesTime > 1f);
		this.m_serverCount.text = this.m_serverListElements.Count.ToString() + " / " + ZSteamMatchmaking.instance.GetTotalNrOfServers();
		if (this.m_serverListRevision == ZSteamMatchmaking.instance.GetServerListRevision())
		{
			return;
		}
		this.m_serverListRevision = ZSteamMatchmaking.instance.GetServerListRevision();
		this.m_serverList.Clear();
		ZSteamMatchmaking.instance.GetServers(this.m_serverList);
		this.m_serverList.Sort((ServerData a, ServerData b) => a.m_name.CompareTo(b.m_name));
		if (this.m_joinServer != null && !this.m_serverList.Contains(this.m_joinServer))
		{
			ZLog.Log("Serverlist does not contain selected server, clearing");
			if (this.m_serverList.Count > 0)
			{
				this.m_joinServer = this.m_serverList[0];
			}
			else
			{
				this.m_joinServer = null;
			}
		}
		this.UpdateServerListGui(false);
	}

	// Token: 0x06000A73 RID: 2675 RVA: 0x0004BE98 File Offset: 0x0004A098
	private void UpdateServerListGui(bool centerSelection)
	{
		if (this.m_serverList.Count != this.m_serverListElements.Count)
		{
			foreach (GameObject obj in this.m_serverListElements)
			{
				UnityEngine.Object.Destroy(obj);
			}
			this.m_serverListElements.Clear();
			float num = (float)this.m_serverList.Count * this.m_serverListElementStep;
			num = Mathf.Max(this.m_serverListBaseSize, num);
			this.m_serverListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num);
			for (int i = 0; i < this.m_serverList.Count; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_serverListElement, this.m_serverListRoot);
				gameObject.SetActive(true);
				(gameObject.transform as RectTransform).anchoredPosition = new Vector2(0f, (float)i * -this.m_serverListElementStep);
				gameObject.GetComponent<Button>().onClick.AddListener(new UnityAction(this.OnSelectedServer));
				this.m_serverListElements.Add(gameObject);
			}
		}
		for (int j = 0; j < this.m_serverList.Count; j++)
		{
			ServerData serverData = this.m_serverList[j];
			GameObject gameObject2 = this.m_serverListElements[j];
			gameObject2.GetComponentInChildren<Text>().text = j + ". " + serverData.m_name;
			gameObject2.GetComponentInChildren<UITooltip>().m_text = serverData.ToString();
			gameObject2.transform.Find("version").GetComponent<Text>().text = serverData.m_version;
			gameObject2.transform.Find("players").GetComponent<Text>().text = string.Concat(new object[]
			{
				"Players:",
				serverData.m_players,
				" / ",
				this.m_serverPlayerLimit
			});
			gameObject2.transform.Find("Private").gameObject.SetActive(serverData.m_password);
			Transform transform = gameObject2.transform.Find("selected");
			bool flag = this.m_joinServer != null && this.m_joinServer.Equals(serverData);
			transform.gameObject.SetActive(flag);
			if (centerSelection && flag)
			{
				this.m_serverListEnsureVisible.CenterOnItem(transform as RectTransform);
			}
		}
	}

	// Token: 0x06000A74 RID: 2676 RVA: 0x0004C118 File Offset: 0x0004A318
	private void OnSelectedServer()
	{
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		int index = this.FindSelectedServer(currentSelectedGameObject);
		this.m_joinServer = this.m_serverList[index];
		this.UpdateServerListGui(false);
	}

	// Token: 0x06000A75 RID: 2677 RVA: 0x0004C151 File Offset: 0x0004A351
	private void SetSelectedServer(int index, bool centerSelection)
	{
		if (this.m_serverList.Count == 0)
		{
			return;
		}
		index = Mathf.Clamp(index, 0, this.m_serverList.Count - 1);
		this.m_joinServer = this.m_serverList[index];
		this.UpdateServerListGui(centerSelection);
	}

	// Token: 0x06000A76 RID: 2678 RVA: 0x0004C190 File Offset: 0x0004A390
	private int GetSelectedServer()
	{
		if (this.m_joinServer == null)
		{
			return -1;
		}
		for (int i = 0; i < this.m_serverList.Count; i++)
		{
			if (this.m_joinServer.Equals(this.m_serverList[i]))
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x06000A77 RID: 2679 RVA: 0x0004C1DC File Offset: 0x0004A3DC
	private int FindSelectedServer(GameObject button)
	{
		for (int i = 0; i < this.m_serverListElements.Count; i++)
		{
			if (this.m_serverListElements[i] == button)
			{
				return i;
			}
		}
		return -1;
	}

	// Token: 0x06000A78 RID: 2680 RVA: 0x0004C216 File Offset: 0x0004A416
	public void OnJoinStart()
	{
		this.JoinServer();
	}

	// Token: 0x06000A79 RID: 2681 RVA: 0x0004C220 File Offset: 0x0004A420
	private void JoinServer()
	{
		ZNet.SetServer(false, false, false, "", "", null);
		if (this.m_joinServer.m_steamHostID != 0UL)
		{
			ZNet.SetServerHost(this.m_joinServer.m_steamHostID);
		}
		else
		{
			ZNet.SetServerHost(this.m_joinServer.m_steamHostAddr);
		}
		Gogan.LogEvent("Menu", "JoinServer", "", 0L);
		this.TransitionToMainScene();
	}

	// Token: 0x06000A7A RID: 2682 RVA: 0x0004C28B File Offset: 0x0004A48B
	public void OnJoinIPOpen()
	{
		this.m_joinIPPanel.SetActive(true);
		this.m_joinIPAddress.ActivateInputField();
	}

	// Token: 0x06000A7B RID: 2683 RVA: 0x0004C2A4 File Offset: 0x0004A4A4
	public void OnJoinIPConnect()
	{
		this.m_joinIPPanel.SetActive(true);
		string[] array = this.m_joinIPAddress.text.Split(new char[]
		{
			':'
		});
		if (array.Length == 0)
		{
			return;
		}
		string text = array[0];
		int num = this.m_joinHostPort;
		int num2;
		if (array.Length > 1 && int.TryParse(array[1], out num2))
		{
			num = num2;
		}
		if (text.Length == 0)
		{
			return;
		}
		ZSteamMatchmaking.instance.QueueServerJoin(text + ":" + num);
	}

	// Token: 0x06000A7C RID: 2684 RVA: 0x0004C321 File Offset: 0x0004A521
	public void OnJoinIPBack()
	{
		this.m_joinIPPanel.SetActive(false);
	}

	// Token: 0x06000A7D RID: 2685 RVA: 0x0004C330 File Offset: 0x0004A530
	public void OnServerListTab()
	{
		bool publicFilter = PlayerPrefs.GetInt("publicfilter", 0) == 1;
		this.SetPublicFilter(publicFilter);
		if (!this.m_doneInitialServerListRequest)
		{
			this.m_doneInitialServerListRequest = true;
			this.RequestServerList();
		}
		this.UpdateServerListGui(true);
		this.m_filterInputField.ActivateInputField();
	}

	// Token: 0x06000A7E RID: 2686 RVA: 0x0004C37A File Offset: 0x0004A57A
	private void SetPublicFilter(bool enabled)
	{
		this.m_friendFilterSwitch.isOn = !enabled;
		this.m_publicFilterSwitch.isOn = enabled;
	}

	// Token: 0x06000A7F RID: 2687 RVA: 0x0004C397 File Offset: 0x0004A597
	public void OnStartGameBack()
	{
		this.m_startGamePanel.SetActive(false);
		this.ShowCharacterSelection();
	}

	// Token: 0x06000A80 RID: 2688 RVA: 0x0004C3AC File Offset: 0x0004A5AC
	public void OnCredits()
	{
		this.m_creditsPanel.SetActive(true);
		this.m_mainMenu.SetActive(false);
		Gogan.LogEvent("Screen", "Enter", "Credits", 0L);
		this.m_creditsList.anchoredPosition = new Vector2(0f, 0f);
	}

	// Token: 0x06000A81 RID: 2689 RVA: 0x0004C401 File Offset: 0x0004A601
	public void OnCreditsBack()
	{
		this.m_mainMenu.SetActive(true);
		this.m_creditsPanel.SetActive(false);
		Gogan.LogEvent("Screen", "Enter", "StartMenu", 0L);
	}

	// Token: 0x06000A82 RID: 2690 RVA: 0x0004C431 File Offset: 0x0004A631
	public void OnSelelectCharacterBack()
	{
		this.m_characterSelectScreen.SetActive(false);
		this.m_mainMenu.SetActive(true);
		this.m_queuedJoinServer = null;
		Gogan.LogEvent("Screen", "Enter", "StartMenu", 0L);
	}

	// Token: 0x06000A83 RID: 2691 RVA: 0x0004C468 File Offset: 0x0004A668
	public void OnAbort()
	{
		Application.Quit();
	}

	// Token: 0x06000A84 RID: 2692 RVA: 0x0004C46F File Offset: 0x0004A66F
	public void OnWorldVersionYes()
	{
		this.m_worldVersionPanel.SetActive(false);
	}

	// Token: 0x06000A85 RID: 2693 RVA: 0x0004C47D File Offset: 0x0004A67D
	public void OnPlayerVersionOk()
	{
		this.m_playerVersionPanel.SetActive(false);
	}

	// Token: 0x06000A86 RID: 2694 RVA: 0x0004C48B File Offset: 0x0004A68B
	private void FixedUpdate()
	{
		ZInput.FixedUpdate(Time.fixedDeltaTime);
	}

	// Token: 0x06000A87 RID: 2695 RVA: 0x0004C497 File Offset: 0x0004A697
	private void UpdateCursor()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = ZInput.IsMouseActive();
	}

	// Token: 0x06000A88 RID: 2696 RVA: 0x0004C4AC File Offset: 0x0004A6AC
	private void Update()
	{
		ZInput.Update(Time.deltaTime);
		this.UpdateCursor();
		this.UpdateGamepad();
		this.CheckPendingSteamJoinRequest();
		if (MasterClient.instance != null)
		{
			MasterClient.instance.Update(Time.deltaTime);
		}
		if (ZBroastcast.instance != null)
		{
			ZBroastcast.instance.Update(Time.deltaTime);
		}
		this.UpdateCharacterRotation(Time.deltaTime);
		this.UpdateCamera(Time.deltaTime);
		if (this.m_newCharacterPanel.activeInHierarchy)
		{
			this.m_csNewCharacterDone.interactable = (this.m_csNewCharacterName.text.Length >= 3);
		}
		if (this.m_serverListPanel.activeInHierarchy)
		{
			this.m_joinGameButton.interactable = (this.m_joinServer != null);
		}
		if (this.m_createWorldPanel.activeInHierarchy)
		{
			this.m_newWorldDone.interactable = (this.m_newWorldName.text.Length >= 5);
		}
		if (this.m_startGamePanel.activeInHierarchy)
		{
			this.m_worldStart.interactable = this.CanStartServer();
			this.m_worldRemove.interactable = (this.m_world != null);
			this.UpdatePasswordError();
		}
		if (this.m_joinIPPanel.activeInHierarchy)
		{
			this.m_joinIPJoinButton.interactable = (this.m_joinIPAddress.text.Length > 0);
		}
		if (this.m_startGamePanel.activeInHierarchy)
		{
			this.m_publicServerToggle.interactable = this.m_openServerToggle.isOn;
			this.m_serverPassword.interactable = this.m_openServerToggle.isOn;
		}
		if (this.m_creditsPanel.activeInHierarchy)
		{
			RectTransform rectTransform = this.m_creditsList.parent as RectTransform;
			Vector3[] array = new Vector3[4];
			this.m_creditsList.GetWorldCorners(array);
			Vector3[] array2 = new Vector3[4];
			rectTransform.GetWorldCorners(array2);
			float num = array2[1].y - array2[0].y;
			if ((double)array[3].y < (double)num * 0.5)
			{
				Vector3 position = this.m_creditsList.position;
				position.y += Time.deltaTime * this.m_creditsSpeed * num;
				this.m_creditsList.position = position;
			}
		}
	}

	// Token: 0x06000A89 RID: 2697 RVA: 0x0004C6D6 File Offset: 0x0004A8D6
	private void LateUpdate()
	{
		if (Input.GetKeyDown(KeyCode.F11))
		{
			GameCamera.ScreenShot();
		}
	}

	// Token: 0x06000A8A RID: 2698 RVA: 0x0004C6EC File Offset: 0x0004A8EC
	private void UpdateGamepad()
	{
		if (!ZInput.IsGamepadActive())
		{
			return;
		}
		if (this.m_worldListPanel.activeInHierarchy)
		{
			if (ZInput.GetButtonDown("JoyLStickDown"))
			{
				this.SetSelectedWorld(this.GetSelectedWorld() + 1, true);
			}
			if (ZInput.GetButtonDown("JoyLStickUp"))
			{
				this.SetSelectedWorld(this.GetSelectedWorld() - 1, true);
				return;
			}
		}
		else if (this.m_serverListPanel.activeInHierarchy)
		{
			if (ZInput.GetButtonDown("JoyLStickDown"))
			{
				this.SetSelectedServer(this.GetSelectedServer() + 1, true);
			}
			if (ZInput.GetButtonDown("JoyLStickUp"))
			{
				this.SetSelectedServer(this.GetSelectedServer() - 1, true);
			}
		}
	}

	// Token: 0x06000A8B RID: 2699 RVA: 0x0004C788 File Offset: 0x0004A988
	private void CheckPendingSteamJoinRequest()
	{
		CSteamID that;
		SteamNetworkingIPAddr steamHostAddr;
		if (ZSteamMatchmaking.instance != null && ZSteamMatchmaking.instance.GetJoinHost(out that, out steamHostAddr))
		{
			this.m_queuedJoinServer = new ServerData();
			if (that.IsValid())
			{
				this.m_queuedJoinServer.m_steamHostID = (ulong)that;
			}
			else
			{
				this.m_queuedJoinServer.m_steamHostAddr = steamHostAddr;
			}
			if (this.m_serverListPanel.activeInHierarchy)
			{
				this.m_joinServer = this.m_queuedJoinServer;
				this.m_queuedJoinServer = null;
				this.JoinServer();
				return;
			}
			this.HideAll();
			this.ShowCharacterSelection();
		}
	}

	// Token: 0x06000A8C RID: 2700 RVA: 0x0004C814 File Offset: 0x0004AA14
	private void UpdateCharacterRotation(float dt)
	{
		if (this.m_playerInstance == null)
		{
			return;
		}
		if (!this.m_characterSelectScreen.activeInHierarchy)
		{
			return;
		}
		if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
		{
			float axis = Input.GetAxis("Mouse X");
			this.m_playerInstance.transform.Rotate(0f, -axis * this.m_characterRotateSpeed, 0f);
		}
		float joyRightStickX = ZInput.GetJoyRightStickX();
		if (joyRightStickX != 0f)
		{
			this.m_playerInstance.transform.Rotate(0f, -joyRightStickX * this.m_characterRotateSpeedGamepad * dt, 0f);
		}
	}

	// Token: 0x06000A8D RID: 2701 RVA: 0x0004C8B4 File Offset: 0x0004AAB4
	private void UpdatePasswordError()
	{
		string text = "";
		if (this.m_publicServerToggle.isOn)
		{
			text = this.GetPublicPasswordError(this.m_serverPassword.text, this.m_world);
		}
		this.m_passwordError.text = text;
	}

	// Token: 0x06000A8E RID: 2702 RVA: 0x0004C8F8 File Offset: 0x0004AAF8
	private string GetPublicPasswordError(string password, World world)
	{
		if (password.Length < this.m_minimumPasswordLength)
		{
			return Localization.instance.Localize("$menu_passwordshort");
		}
		if (world != null && (world.m_name.Contains(password) || world.m_seedName.Contains(password)))
		{
			return Localization.instance.Localize("$menu_passwordinvalid");
		}
		return "";
	}

	// Token: 0x06000A8F RID: 2703 RVA: 0x0004C957 File Offset: 0x0004AB57
	private bool IsPublicPasswordValid(string password, World world)
	{
		return password.Length >= this.m_minimumPasswordLength && !world.m_name.Contains(password) && !world.m_seedName.Contains(password);
	}

	// Token: 0x06000A90 RID: 2704 RVA: 0x0004C98C File Offset: 0x0004AB8C
	private bool CanStartServer()
	{
		return this.m_world != null && !this.m_world.m_loadError && !this.m_world.m_versionError && (!this.m_publicServerToggle.isOn || this.IsPublicPasswordValid(this.m_serverPassword.text, this.m_world));
	}

	// Token: 0x06000A91 RID: 2705 RVA: 0x0004C9E8 File Offset: 0x0004ABE8
	private void UpdateCamera(float dt)
	{
		Transform transform = this.m_cameraMarkerMain;
		if (this.m_characterSelectScreen.activeSelf)
		{
			transform = this.m_cameraMarkerCharacter;
		}
		else if (this.m_creditsPanel.activeSelf)
		{
			transform = this.m_cameraMarkerCredits;
		}
		else if (this.m_startGamePanel.activeSelf || this.m_joinIPPanel.activeSelf)
		{
			transform = this.m_cameraMarkerGame;
		}
		this.m_mainCamera.transform.position = Vector3.SmoothDamp(this.m_mainCamera.transform.position, transform.position, ref this.camSpeed, 1.5f, 1000f, dt);
		Vector3 forward = Vector3.SmoothDamp(this.m_mainCamera.transform.forward, transform.forward, ref this.camRotSpeed, 1.5f, 1000f, dt);
		forward.Normalize();
		this.m_mainCamera.transform.rotation = Quaternion.LookRotation(forward);
	}

	// Token: 0x06000A92 RID: 2706 RVA: 0x0004CAD0 File Offset: 0x0004ACD0
	private void UpdateCharacterList()
	{
		if (this.m_profiles == null)
		{
			this.m_profiles = PlayerProfile.GetAllPlayerProfiles();
		}
		if (this.m_profileIndex >= this.m_profiles.Count)
		{
			this.m_profileIndex = this.m_profiles.Count - 1;
		}
		this.m_csRemoveButton.gameObject.SetActive(this.m_profiles.Count > 0);
		this.m_csStartButton.gameObject.SetActive(this.m_profiles.Count > 0);
		this.m_csNewButton.gameObject.SetActive(this.m_profiles.Count > 0);
		this.m_csNewBigButton.gameObject.SetActive(this.m_profiles.Count == 0);
		this.m_csLeftButton.interactable = (this.m_profileIndex > 0);
		this.m_csRightButton.interactable = (this.m_profileIndex < this.m_profiles.Count - 1);
		if (this.m_profileIndex >= 0 && this.m_profileIndex < this.m_profiles.Count)
		{
			PlayerProfile playerProfile = this.m_profiles[this.m_profileIndex];
			this.m_csName.text = playerProfile.GetName();
			this.m_csName.gameObject.SetActive(true);
			this.SetupCharacterPreview(playerProfile);
			return;
		}
		this.m_csName.gameObject.SetActive(false);
		this.ClearCharacterPreview();
	}

	// Token: 0x06000A93 RID: 2707 RVA: 0x0004CC34 File Offset: 0x0004AE34
	private void SetSelectedProfile(string filename)
	{
		if (this.m_profiles == null)
		{
			this.m_profiles = PlayerProfile.GetAllPlayerProfiles();
		}
		this.m_profileIndex = 0;
		for (int i = 0; i < this.m_profiles.Count; i++)
		{
			if (this.m_profiles[i].GetFilename() == filename)
			{
				this.m_profileIndex = i;
				break;
			}
		}
		this.UpdateCharacterList();
	}

	// Token: 0x06000A94 RID: 2708 RVA: 0x0004CC9C File Offset: 0x0004AE9C
	public void OnNewCharacterDone()
	{
		string text = this.m_csNewCharacterName.text;
		string text2 = text.ToLower();
		if (PlayerProfile.HaveProfile(text2))
		{
			this.m_newCharacterError.SetActive(true);
			return;
		}
		Player component = this.m_playerInstance.GetComponent<Player>();
		component.GiveDefaultItems();
		PlayerProfile playerProfile = new PlayerProfile(text2);
		playerProfile.SetName(text);
		playerProfile.SavePlayerData(component);
		playerProfile.Save();
		this.m_selectCharacterPanel.SetActive(true);
		this.m_newCharacterPanel.SetActive(false);
		this.m_profiles = null;
		this.SetSelectedProfile(text2);
		Gogan.LogEvent("Menu", "NewCharacter", text, 0L);
	}

	// Token: 0x06000A95 RID: 2709 RVA: 0x0004CD35 File Offset: 0x0004AF35
	public void OnNewCharacterCancel()
	{
		this.m_selectCharacterPanel.SetActive(true);
		this.m_newCharacterPanel.SetActive(false);
		this.UpdateCharacterList();
	}

	// Token: 0x06000A96 RID: 2710 RVA: 0x0004CD58 File Offset: 0x0004AF58
	public void OnCharacterNew()
	{
		this.m_newCharacterPanel.SetActive(true);
		this.m_selectCharacterPanel.SetActive(false);
		this.m_csNewCharacterName.text = "";
		this.m_newCharacterError.SetActive(false);
		this.SetupCharacterPreview(null);
		Gogan.LogEvent("Screen", "Enter", "CreateCharacter", 0L);
	}

	// Token: 0x06000A97 RID: 2711 RVA: 0x0004CDB8 File Offset: 0x0004AFB8
	public void OnCharacterRemove()
	{
		if (this.m_profileIndex < 0 || this.m_profileIndex >= this.m_profiles.Count)
		{
			return;
		}
		PlayerProfile playerProfile = this.m_profiles[this.m_profileIndex];
		this.m_removeCharacterName.text = playerProfile.GetName();
		this.m_tempRemoveCharacterName = playerProfile.GetFilename();
		this.m_tempRemoveCharacterIndex = this.m_profileIndex;
		this.m_removeCharacterDialog.SetActive(true);
	}

	// Token: 0x06000A98 RID: 2712 RVA: 0x0004CE29 File Offset: 0x0004B029
	public void OnButtonRemoveCharacterYes()
	{
		ZLog.Log("Remove character");
		PlayerProfile.RemoveProfile(this.m_tempRemoveCharacterName);
		this.m_profiles.RemoveAt(this.m_tempRemoveCharacterIndex);
		this.UpdateCharacterList();
		this.m_removeCharacterDialog.SetActive(false);
	}

	// Token: 0x06000A99 RID: 2713 RVA: 0x0004CE63 File Offset: 0x0004B063
	public void OnButtonRemoveCharacterNo()
	{
		this.m_removeCharacterDialog.SetActive(false);
	}

	// Token: 0x06000A9A RID: 2714 RVA: 0x0004CE71 File Offset: 0x0004B071
	public void OnCharacterLeft()
	{
		if (this.m_profileIndex > 0)
		{
			this.m_profileIndex--;
		}
		this.UpdateCharacterList();
	}

	// Token: 0x06000A9B RID: 2715 RVA: 0x0004CE90 File Offset: 0x0004B090
	public void OnCharacterRight()
	{
		if (this.m_profileIndex < this.m_profiles.Count - 1)
		{
			this.m_profileIndex++;
		}
		this.UpdateCharacterList();
	}

	// Token: 0x06000A9C RID: 2716 RVA: 0x0004CEBC File Offset: 0x0004B0BC
	public void OnCharacterStart()
	{
		ZLog.Log("OnCharacterStart");
		if (this.m_profileIndex < 0 || this.m_profileIndex >= this.m_profiles.Count)
		{
			return;
		}
		PlayerProfile playerProfile = this.m_profiles[this.m_profileIndex];
		PlayerPrefs.SetString("profile", playerProfile.GetFilename());
		Game.SetProfile(playerProfile.GetFilename());
		this.m_characterSelectScreen.SetActive(false);
		if (this.m_queuedJoinServer != null)
		{
			this.m_joinServer = this.m_queuedJoinServer;
			this.m_queuedJoinServer = null;
			this.JoinServer();
			return;
		}
		this.ShowStartGame();
		if (this.m_worlds.Count == 0)
		{
			this.OnWorldNew();
		}
	}

	// Token: 0x06000A9D RID: 2717 RVA: 0x0004CF64 File Offset: 0x0004B164
	private void TransitionToMainScene()
	{
		this.m_menuAnimator.SetTrigger("FadeOut");
		base.Invoke("LoadMainScene", 1.5f);
	}

	// Token: 0x06000A9E RID: 2718 RVA: 0x0004CF86 File Offset: 0x0004B186
	private void LoadMainScene()
	{
		this.m_loading.SetActive(true);
		SceneManager.LoadScene("main");
	}

	// Token: 0x06000A9F RID: 2719 RVA: 0x0004CF9E File Offset: 0x0004B19E
	public void OnButtonSettings()
	{
		UnityEngine.Object.Instantiate<GameObject>(this.m_settingsPrefab, base.transform);
	}

	// Token: 0x06000AA0 RID: 2720 RVA: 0x0004CFB2 File Offset: 0x0004B1B2
	public void OnButtonFeedback()
	{
		UnityEngine.Object.Instantiate<GameObject>(this.m_feedbackPrefab, base.transform);
	}

	// Token: 0x06000AA1 RID: 2721 RVA: 0x0004CFC6 File Offset: 0x0004B1C6
	public void OnButtonTwitter()
	{
		Application.OpenURL("https://twitter.com/valheimgame");
	}

	// Token: 0x06000AA2 RID: 2722 RVA: 0x0004CFD2 File Offset: 0x0004B1D2
	public void OnButtonWebPage()
	{
		Application.OpenURL("http://valheimgame.com/");
	}

	// Token: 0x06000AA3 RID: 2723 RVA: 0x0004CFDE File Offset: 0x0004B1DE
	public void OnButtonDiscord()
	{
		Application.OpenURL("https://discord.gg/44qXMJH");
	}

	// Token: 0x06000AA4 RID: 2724 RVA: 0x0004CFEA File Offset: 0x0004B1EA
	public void OnButtonFacebook()
	{
		Application.OpenURL("https://www.facebook.com/valheimgame/");
	}

	// Token: 0x06000AA5 RID: 2725 RVA: 0x0004CFF6 File Offset: 0x0004B1F6
	public void OnButtonShowLog()
	{
		Application.OpenURL(Application.persistentDataPath + "/");
	}

	// Token: 0x06000AA6 RID: 2726 RVA: 0x0004D00C File Offset: 0x0004B20C
	private bool AcceptedNDA()
	{
		return PlayerPrefs.GetInt("accepted_nda", 0) == 1;
	}

	// Token: 0x06000AA7 RID: 2727 RVA: 0x0004D01C File Offset: 0x0004B21C
	public void OnButtonNDAAccept()
	{
		PlayerPrefs.SetInt("accepted_nda", 1);
		this.m_ndaPanel.SetActive(false);
		this.m_mainMenu.SetActive(true);
	}

	// Token: 0x06000AA8 RID: 2728 RVA: 0x0004C468 File Offset: 0x0004A668
	public void OnButtonNDADecline()
	{
		Application.Quit();
	}

	// Token: 0x06000AA9 RID: 2729 RVA: 0x0004D041 File Offset: 0x0004B241
	public void OnConnectionFailedOk()
	{
		this.m_connectionFailedPanel.SetActive(false);
	}

	// Token: 0x06000AAA RID: 2730 RVA: 0x0004D04F File Offset: 0x0004B24F
	public Player GetPreviewPlayer()
	{
		if (this.m_playerInstance != null)
		{
			return this.m_playerInstance.GetComponent<Player>();
		}
		return null;
	}

	// Token: 0x06000AAB RID: 2731 RVA: 0x0004D06C File Offset: 0x0004B26C
	private void ClearCharacterPreview()
	{
		if (this.m_playerInstance)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.m_changeEffectPrefab, this.m_characterPreviewPoint.position, this.m_characterPreviewPoint.rotation);
			UnityEngine.Object.Destroy(this.m_playerInstance);
			this.m_playerInstance = null;
		}
	}

	// Token: 0x06000AAC RID: 2732 RVA: 0x0004D0BC File Offset: 0x0004B2BC
	private void SetupCharacterPreview(PlayerProfile profile)
	{
		this.ClearCharacterPreview();
		ZNetView.m_forceDisableInit = true;
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.m_playerPrefab, this.m_characterPreviewPoint.position, this.m_characterPreviewPoint.rotation);
		ZNetView.m_forceDisableInit = false;
		UnityEngine.Object.Destroy(gameObject.GetComponent<Rigidbody>());
		Animator[] componentsInChildren = gameObject.GetComponentsInChildren<Animator>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].updateMode = AnimatorUpdateMode.Normal;
		}
		Player component = gameObject.GetComponent<Player>();
		if (profile != null)
		{
			profile.LoadPlayerData(component);
		}
		this.m_playerInstance = gameObject;
	}

	// Token: 0x040009AC RID: 2476
	private float m_lastServerListRequesTime = -999f;

	// Token: 0x040009AD RID: 2477
	private Vector3 camSpeed = Vector3.zero;

	// Token: 0x040009AE RID: 2478
	private Vector3 camRotSpeed = Vector3.zero;

	// Token: 0x040009AF RID: 2479
	private static FejdStartup m_instance;

	// Token: 0x040009B0 RID: 2480
	[Header("Start")]
	public Animator m_menuAnimator;

	// Token: 0x040009B1 RID: 2481
	public GameObject m_worldVersionPanel;

	// Token: 0x040009B2 RID: 2482
	public GameObject m_playerVersionPanel;

	// Token: 0x040009B3 RID: 2483
	public GameObject m_newGameVersionPanel;

	// Token: 0x040009B4 RID: 2484
	public GameObject m_connectionFailedPanel;

	// Token: 0x040009B5 RID: 2485
	public Text m_connectionFailedError;

	// Token: 0x040009B6 RID: 2486
	public Text m_newVersionName;

	// Token: 0x040009B7 RID: 2487
	public GameObject m_loading;

	// Token: 0x040009B8 RID: 2488
	public Text m_versionLabel;

	// Token: 0x040009B9 RID: 2489
	public GameObject m_mainMenu;

	// Token: 0x040009BA RID: 2490
	public GameObject m_ndaPanel;

	// Token: 0x040009BB RID: 2491
	public GameObject m_betaText;

	// Token: 0x040009BC RID: 2492
	public GameObject m_characterSelectScreen;

	// Token: 0x040009BD RID: 2493
	public GameObject m_selectCharacterPanel;

	// Token: 0x040009BE RID: 2494
	public GameObject m_newCharacterPanel;

	// Token: 0x040009BF RID: 2495
	public GameObject m_creditsPanel;

	// Token: 0x040009C0 RID: 2496
	public GameObject m_startGamePanel;

	// Token: 0x040009C1 RID: 2497
	public GameObject m_createWorldPanel;

	// Token: 0x040009C2 RID: 2498
	public RectTransform m_creditsList;

	// Token: 0x040009C3 RID: 2499
	public float m_creditsSpeed = 100f;

	// Token: 0x040009C4 RID: 2500
	[Header("Camera")]
	public GameObject m_mainCamera;

	// Token: 0x040009C5 RID: 2501
	public Transform m_cameraMarkerStart;

	// Token: 0x040009C6 RID: 2502
	public Transform m_cameraMarkerMain;

	// Token: 0x040009C7 RID: 2503
	public Transform m_cameraMarkerCharacter;

	// Token: 0x040009C8 RID: 2504
	public Transform m_cameraMarkerCredits;

	// Token: 0x040009C9 RID: 2505
	public Transform m_cameraMarkerGame;

	// Token: 0x040009CA RID: 2506
	public float m_cameraMoveSpeed = 1.5f;

	// Token: 0x040009CB RID: 2507
	public float m_cameraMoveSpeedStart = 1.5f;

	// Token: 0x040009CC RID: 2508
	[Header("Join")]
	public GameObject m_serverListPanel;

	// Token: 0x040009CD RID: 2509
	public Toggle m_publicServerToggle;

	// Token: 0x040009CE RID: 2510
	public Toggle m_openServerToggle;

	// Token: 0x040009CF RID: 2511
	public InputField m_serverPassword;

	// Token: 0x040009D0 RID: 2512
	public RectTransform m_serverListRoot;

	// Token: 0x040009D1 RID: 2513
	public GameObject m_serverListElement;

	// Token: 0x040009D2 RID: 2514
	public ScrollRectEnsureVisible m_serverListEnsureVisible;

	// Token: 0x040009D3 RID: 2515
	public float m_serverListElementStep = 28f;

	// Token: 0x040009D4 RID: 2516
	public Text m_serverCount;

	// Token: 0x040009D5 RID: 2517
	public Button m_serverRefreshButton;

	// Token: 0x040009D6 RID: 2518
	public InputField m_filterInputField;

	// Token: 0x040009D7 RID: 2519
	public Text m_passwordError;

	// Token: 0x040009D8 RID: 2520
	public Button m_manualIPButton;

	// Token: 0x040009D9 RID: 2521
	public GameObject m_joinIPPanel;

	// Token: 0x040009DA RID: 2522
	public Button m_joinIPJoinButton;

	// Token: 0x040009DB RID: 2523
	public InputField m_joinIPAddress;

	// Token: 0x040009DC RID: 2524
	public Button m_joinGameButton;

	// Token: 0x040009DD RID: 2525
	public Toggle m_friendFilterSwitch;

	// Token: 0x040009DE RID: 2526
	public Toggle m_publicFilterSwitch;

	// Token: 0x040009DF RID: 2527
	public int m_minimumPasswordLength = 5;

	// Token: 0x040009E0 RID: 2528
	public float m_characterRotateSpeed = 4f;

	// Token: 0x040009E1 RID: 2529
	public float m_characterRotateSpeedGamepad = 200f;

	// Token: 0x040009E2 RID: 2530
	public int m_joinHostPort = 2456;

	// Token: 0x040009E3 RID: 2531
	public int m_serverPlayerLimit = 10;

	// Token: 0x040009E4 RID: 2532
	[Header("World")]
	public GameObject m_worldListPanel;

	// Token: 0x040009E5 RID: 2533
	public RectTransform m_worldListRoot;

	// Token: 0x040009E6 RID: 2534
	public GameObject m_worldListElement;

	// Token: 0x040009E7 RID: 2535
	public ScrollRectEnsureVisible m_worldListEnsureVisible;

	// Token: 0x040009E8 RID: 2536
	public float m_worldListElementStep = 28f;

	// Token: 0x040009E9 RID: 2537
	public InputField m_newWorldName;

	// Token: 0x040009EA RID: 2538
	public InputField m_newWorldSeed;

	// Token: 0x040009EB RID: 2539
	public Button m_newWorldDone;

	// Token: 0x040009EC RID: 2540
	public Button m_worldStart;

	// Token: 0x040009ED RID: 2541
	public Button m_worldRemove;

	// Token: 0x040009EE RID: 2542
	public GameObject m_removeWorldDialog;

	// Token: 0x040009EF RID: 2543
	public Text m_removeWorldName;

	// Token: 0x040009F0 RID: 2544
	public GameObject m_removeCharacterDialog;

	// Token: 0x040009F1 RID: 2545
	public Text m_removeCharacterName;

	// Token: 0x040009F2 RID: 2546
	[Header("Character selectoin")]
	public Button m_csStartButton;

	// Token: 0x040009F3 RID: 2547
	public Button m_csNewBigButton;

	// Token: 0x040009F4 RID: 2548
	public Button m_csNewButton;

	// Token: 0x040009F5 RID: 2549
	public Button m_csRemoveButton;

	// Token: 0x040009F6 RID: 2550
	public Button m_csLeftButton;

	// Token: 0x040009F7 RID: 2551
	public Button m_csRightButton;

	// Token: 0x040009F8 RID: 2552
	public Button m_csNewCharacterDone;

	// Token: 0x040009F9 RID: 2553
	public GameObject m_newCharacterError;

	// Token: 0x040009FA RID: 2554
	public Text m_csName;

	// Token: 0x040009FB RID: 2555
	public InputField m_csNewCharacterName;

	// Token: 0x040009FC RID: 2556
	[Header("Misc")]
	public Transform m_characterPreviewPoint;

	// Token: 0x040009FD RID: 2557
	public GameObject m_playerPrefab;

	// Token: 0x040009FE RID: 2558
	public GameObject m_gameMainPrefab;

	// Token: 0x040009FF RID: 2559
	public GameObject m_settingsPrefab;

	// Token: 0x04000A00 RID: 2560
	public GameObject m_consolePrefab;

	// Token: 0x04000A01 RID: 2561
	public GameObject m_feedbackPrefab;

	// Token: 0x04000A02 RID: 2562
	public GameObject m_changeEffectPrefab;

	// Token: 0x04000A03 RID: 2563
	private string m_downloadUrl = "";

	// Token: 0x04000A04 RID: 2564
	[TextArea]
	public string m_versionXmlUrl = "https://dl.dropboxusercontent.com/s/5ibm05oelbqt8zq/fejdversion.xml?dl=0";

	// Token: 0x04000A05 RID: 2565
	private World m_world;

	// Token: 0x04000A06 RID: 2566
	private ServerData m_joinServer;

	// Token: 0x04000A07 RID: 2567
	private ServerData m_queuedJoinServer;

	// Token: 0x04000A08 RID: 2568
	private float m_serverListBaseSize;

	// Token: 0x04000A09 RID: 2569
	private float m_worldListBaseSize;

	// Token: 0x04000A0A RID: 2570
	private List<PlayerProfile> m_profiles;

	// Token: 0x04000A0B RID: 2571
	private int m_profileIndex;

	// Token: 0x04000A0C RID: 2572
	private string m_tempRemoveCharacterName = "";

	// Token: 0x04000A0D RID: 2573
	private int m_tempRemoveCharacterIndex = -1;

	// Token: 0x04000A0E RID: 2574
	private List<GameObject> m_serverListElements = new List<GameObject>();

	// Token: 0x04000A0F RID: 2575
	private List<ServerData> m_serverList = new List<ServerData>();

	// Token: 0x04000A10 RID: 2576
	private int m_serverListRevision = -1;

	// Token: 0x04000A11 RID: 2577
	private List<GameObject> m_worldListElements = new List<GameObject>();

	// Token: 0x04000A12 RID: 2578
	private List<World> m_worlds;

	// Token: 0x04000A13 RID: 2579
	private GameObject m_playerInstance;

	// Token: 0x04000A14 RID: 2580
	private bool m_doneInitialServerListRequest;

	// Token: 0x04000A15 RID: 2581
	private static bool m_firstStartup = true;
}
