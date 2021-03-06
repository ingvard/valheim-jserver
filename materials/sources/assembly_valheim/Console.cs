﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x0200004E RID: 78
public class Console : MonoBehaviour
{
	// Token: 0x17000006 RID: 6
	// (get) Token: 0x060004C9 RID: 1225 RVA: 0x00026C37 File Offset: 0x00024E37
	public static global::Console instance
	{
		get
		{
			return global::Console.m_instance;
		}
	}

	// Token: 0x060004CA RID: 1226 RVA: 0x00026C40 File Offset: 0x00024E40
	private void Awake()
	{
		global::Console.m_instance = this;
		this.AddString("Valheim " + global::Version.GetVersionString());
		this.AddString("");
		this.AddString("type \"help\" - for commands");
		this.AddString("");
		this.m_chatWindow.gameObject.SetActive(false);
	}

	// Token: 0x060004CB RID: 1227 RVA: 0x00026C9C File Offset: 0x00024E9C
	private void Update()
	{
		if (ZNet.instance && ZNet.instance.InPasswordDialog())
		{
			this.m_chatWindow.gameObject.SetActive(false);
			return;
		}
		if (!this.IsConsoleEnabled())
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.F5) || (global::Console.IsVisible() && Input.GetKeyDown(KeyCode.Escape)))
		{
			this.m_chatWindow.gameObject.SetActive(!this.m_chatWindow.gameObject.activeSelf);
		}
		if (this.m_chatWindow.gameObject.activeInHierarchy)
		{
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				this.m_input.text = this.m_lastEntry;
				this.m_input.caretPosition = this.m_input.text.Length;
			}
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				this.m_input.text = "";
			}
			this.m_input.gameObject.SetActive(true);
			this.m_input.ActivateInputField();
			if (Input.GetKeyDown(KeyCode.Return))
			{
				if (!string.IsNullOrEmpty(this.m_input.text))
				{
					this.InputText();
					this.m_lastEntry = this.m_input.text;
					this.m_input.text = "";
				}
				EventSystem.current.SetSelectedGameObject(null);
				this.m_input.gameObject.SetActive(false);
			}
		}
	}

	// Token: 0x060004CC RID: 1228 RVA: 0x00026DFE File Offset: 0x00024FFE
	public static bool IsVisible()
	{
		return global::Console.m_instance && global::Console.m_instance.m_chatWindow.gameObject.activeInHierarchy;
	}

	// Token: 0x060004CD RID: 1229 RVA: 0x00026E22 File Offset: 0x00025022
	public void Print(string text)
	{
		this.AddString(text);
	}

	// Token: 0x060004CE RID: 1230 RVA: 0x00026E2B File Offset: 0x0002502B
	private void AddString(string text)
	{
		this.m_chatBuffer.Add(text);
		while (this.m_chatBuffer.Count > 30)
		{
			this.m_chatBuffer.RemoveAt(0);
		}
		this.UpdateChat();
	}

	// Token: 0x060004CF RID: 1231 RVA: 0x00026E5C File Offset: 0x0002505C
	private void UpdateChat()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string value in this.m_chatBuffer)
		{
			stringBuilder.Append(value);
			stringBuilder.Append("\n");
		}
		this.m_output.text = stringBuilder.ToString();
	}

	// Token: 0x060004D0 RID: 1232 RVA: 0x00026ED4 File Offset: 0x000250D4
	private void InputText()
	{
		string text = this.m_input.text;
		this.AddString(text);
		string[] array = text.Split(new char[]
		{
			' '
		});
		if (text.StartsWith("help"))
		{
			this.AddString("kick [name/ip/userID] - kick user");
			this.AddString("ban [name/ip/userID] - ban user");
			this.AddString("unban [ip/userID] - unban user");
			this.AddString("banned - list banned users");
			this.AddString("ping - ping server");
			this.AddString("lodbias - set distance lod bias");
			this.AddString("info - print system info");
			this.AddString("save - force saving of world");
			if (this.IsCheatsEnabled())
			{
				this.AddString("genloc - regenerate all locations.");
				this.AddString("debugmode - fly mode");
				this.AddString("spawn [amount] [level] - spawn something");
				this.AddString("pos - print current player position");
				this.AddString("goto [x,z]- teleport");
				this.AddString("exploremap - explore entire map");
				this.AddString("resetmap - reset map exploration");
				this.AddString("killall - kill nearby enemies");
				this.AddString("tame - tame all nearby tameable creatures");
				this.AddString("hair");
				this.AddString("beard");
				this.AddString("location - spawn location");
				this.AddString("raiseskill [skill] [amount]");
				this.AddString("resetskill [skill]");
				this.AddString("freefly - freefly photo mode");
				this.AddString("ffsmooth - freefly smoothness");
				this.AddString("tod -1 OR [0-1]");
				this.AddString("env [env]");
				this.AddString("resetenv");
				this.AddString("wind [angle] [intensity]");
				this.AddString("resetwind");
				this.AddString("god");
				this.AddString("event [name] - start event");
				this.AddString("stopevent - stop current event");
				this.AddString("randomevent");
				this.AddString("resetcharacter - reset character data");
				this.AddString("removedrops - remove all item-drops in area");
				this.AddString("setkey [name]");
				this.AddString("resetkeys [name]");
				this.AddString("listkeys");
				this.AddString("players [nr] - force diffuculty scale ( 0 = reset)");
				this.AddString("dpsdebug - toggle dps debug print");
			}
		}
		if (array[0] == "devcommands")
		{
			this.m_cheat = !this.m_cheat;
			this.AddString("Dev commands: " + this.m_cheat.ToString());
			this.AddString("WARNING: using any dev commands is not recommended and is done on your own risk.");
			Gogan.LogEvent("Cheat", "CheatsEnabled", this.m_cheat.ToString(), 0L);
			return;
		}
		if (array[0] == "hidebetatext" && Hud.instance)
		{
			Hud.instance.ToggleBetaTextVisible();
		}
		if (array[0] == "ping")
		{
			if (Game.instance)
			{
				Game.instance.Ping();
			}
			return;
		}
		if (array[0] == "dpsdebug")
		{
			Character.SetDPSDebug(!Character.IsDPSDebugEnabled());
			this.AddString("DPS debug " + Character.IsDPSDebugEnabled().ToString());
		}
		if (!(array[0] == "lodbias"))
		{
			if (array[0] == "info")
			{
				this.Print("Render threading mode:" + SystemInfo.renderingThreadingMode);
				long totalMemory = GC.GetTotalMemory(false);
				this.Print("Total allocated mem: " + (totalMemory / 1048576L).ToString("0") + "mb");
			}
			if (array[0] == "gc")
			{
				long totalMemory2 = GC.GetTotalMemory(false);
				GC.Collect();
				long totalMemory3 = GC.GetTotalMemory(true);
				long num = totalMemory3 - totalMemory2;
				this.Print(string.Concat(new string[]
				{
					"GC collect, Delta: ",
					(num / 1048576L).ToString("0"),
					"mb   Total left:",
					(totalMemory3 / 1048576L).ToString("0"),
					"mb"
				}));
			}
			if (array[0] == "fov")
			{
				Camera mainCamera = Utils.GetMainCamera();
				if (mainCamera)
				{
					float num2;
					if (array.Length == 1)
					{
						this.Print("Fov:" + mainCamera.fieldOfView);
					}
					else if (float.TryParse(array[1], NumberStyles.Float, CultureInfo.InvariantCulture, out num2) && num2 > 5f)
					{
						this.Print("Setting fov to " + num2);
						Camera[] componentsInChildren = mainCamera.GetComponentsInChildren<Camera>();
						for (int i = 0; i < componentsInChildren.Length; i++)
						{
							componentsInChildren[i].fieldOfView = num2;
						}
					}
				}
			}
			if (ZNet.instance)
			{
				if (text.StartsWith("kick "))
				{
					string user = text.Substring(5);
					ZNet.instance.Kick(user);
					return;
				}
				if (text.StartsWith("ban "))
				{
					string user2 = text.Substring(4);
					ZNet.instance.Ban(user2);
					return;
				}
				if (text.StartsWith("unban "))
				{
					string user3 = text.Substring(6);
					ZNet.instance.Unban(user3);
					return;
				}
				if (text.StartsWith("banned"))
				{
					ZNet.instance.PrintBanned();
					return;
				}
				if (array.Length != 0 && array[0] == "save")
				{
					ZNet.instance.ConsoleSave();
				}
			}
			if (ZNet.instance && ZNet.instance.IsServer() && Player.m_localPlayer && this.IsCheatsEnabled())
			{
				if (array[0] == "genloc")
				{
					ZoneSystem.instance.GenerateLocations();
					return;
				}
				if (array[0] == "players" && array.Length >= 2)
				{
					int num3;
					if (int.TryParse(array[1], out num3))
					{
						Game.instance.SetForcePlayerDifficulty(num3);
						this.Print("Setting players to " + num3);
					}
					return;
				}
				if (array[0] == "setkey")
				{
					if (array.Length >= 2)
					{
						ZoneSystem.instance.SetGlobalKey(array[1]);
						this.Print("Setting global key " + array[1]);
					}
					else
					{
						this.Print("Syntax: setkey [key]");
					}
				}
				if (array[0] == "resetkeys")
				{
					ZoneSystem.instance.ResetGlobalKeys();
					this.Print("Global keys cleared");
				}
				if (array[0] == "listkeys")
				{
					List<string> globalKeys = ZoneSystem.instance.GetGlobalKeys();
					this.Print("Keys " + globalKeys.Count);
					foreach (string text2 in globalKeys)
					{
						this.Print(text2);
					}
				}
				if (array[0] == "debugmode")
				{
					Player.m_debugMode = !Player.m_debugMode;
					this.Print("Debugmode " + Player.m_debugMode.ToString());
				}
				if (array[0] == "raiseskill")
				{
					if (array.Length > 2)
					{
						string name = array[1];
						int num4 = int.Parse(array[2]);
						Player.m_localPlayer.GetSkills().CheatRaiseSkill(name, (float)num4);
						return;
					}
					this.Print("Syntax: raiseskill [skill] [amount]");
					return;
				}
				else if (array[0] == "resetskill")
				{
					if (array.Length > 1)
					{
						string name2 = array[1];
						Player.m_localPlayer.GetSkills().CheatResetSkill(name2);
						return;
					}
					this.Print("Syntax: resetskill [skill]");
					return;
				}
				else
				{
					if (text == "sleep")
					{
						EnvMan.instance.SkipToMorning();
						return;
					}
					if (array[0] == "skiptime")
					{
						double num5 = ZNet.instance.GetTimeSeconds();
						float num6 = 240f;
						if (array.Length > 1)
						{
							num6 = float.Parse(array[1]);
						}
						num5 += (double)num6;
						ZNet.instance.SetNetTime(num5);
						this.Print(string.Concat(new object[]
						{
							"Skipping ",
							num6.ToString("0"),
							"s , Day:",
							EnvMan.instance.GetDay(num5)
						}));
						return;
					}
					if (text == "resetcharacter")
					{
						this.AddString("Reseting character");
						Player.m_localPlayer.ResetCharacter();
						return;
					}
					if (array[0] == "randomevent")
					{
						RandEventSystem.instance.StartRandomEvent();
					}
					if (text.StartsWith("event "))
					{
						if (array.Length <= 1)
						{
							return;
						}
						string text3 = text.Substring(6);
						if (!RandEventSystem.instance.HaveEvent(text3))
						{
							this.Print("Random event not found:" + text3);
							return;
						}
						RandEventSystem.instance.SetRandomEventByName(text3, Player.m_localPlayer.transform.position);
						return;
					}
					else
					{
						if (array[0] == "stopevent")
						{
							RandEventSystem.instance.ResetRandomEvent();
							return;
						}
						if (text.StartsWith("removedrops"))
						{
							this.AddString("Removing item drops");
							ItemDrop[] array2 = UnityEngine.Object.FindObjectsOfType<ItemDrop>();
							for (int i = 0; i < array2.Length; i++)
							{
								ZNetView component = array2[i].GetComponent<ZNetView>();
								if (component && component.IsValid() && component.IsOwner())
								{
									component.Destroy();
								}
							}
						}
						if (text.StartsWith("freefly"))
						{
							this.Print("Toggling free fly camera");
							GameCamera.instance.ToggleFreeFly();
							return;
						}
						if (array[0] == "ffsmooth")
						{
							if (array.Length <= 1)
							{
								this.Print(GameCamera.instance.GetFreeFlySmoothness().ToString());
								return;
							}
							float num7;
							if (!float.TryParse(array[1], NumberStyles.Float, CultureInfo.InvariantCulture, out num7))
							{
								this.Print("syntax error");
								return;
							}
							this.Print("Setting free fly camera smoothing:" + num7);
							GameCamera.instance.SetFreeFlySmoothness(num7);
							return;
						}
						else
						{
							if (text.StartsWith("location "))
							{
								if (array.Length <= 1)
								{
									return;
								}
								string name3 = text.Substring(9);
								Vector3 pos = Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * 10f;
								ZoneSystem.instance.TestSpawnLocation(name3, pos);
							}
							if (array[0] == "spawn")
							{
								if (array.Length <= 1)
								{
									return;
								}
								string text4 = array[1];
								int num8 = (array.Length >= 3) ? int.Parse(array[2]) : 1;
								int num9 = (array.Length >= 4) ? int.Parse(array[3]) : 1;
								GameObject prefab = ZNetScene.instance.GetPrefab(text4);
								if (!prefab)
								{
									Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Missing object " + text4, 0, null);
									return;
								}
								DateTime now = DateTime.Now;
								if (num8 == 1)
								{
									Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Spawning object " + text4, 0, null);
									Character component2 = UnityEngine.Object.Instantiate<GameObject>(prefab, Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * 2f + Vector3.up, Quaternion.identity).GetComponent<Character>();
									if (component2 & num9 > 1)
									{
										component2.SetLevel(num9);
									}
								}
								else
								{
									for (int j = 0; j < num8; j++)
									{
										Vector3 b = UnityEngine.Random.insideUnitSphere * 0.5f;
										Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Spawning object " + text4, 0, null);
										Character component3 = UnityEngine.Object.Instantiate<GameObject>(prefab, Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * 2f + Vector3.up + b, Quaternion.identity).GetComponent<Character>();
										if (component3 & num9 > 1)
										{
											component3.SetLevel(num9);
										}
									}
								}
								ZLog.Log("Spawn time :" + (DateTime.Now - now).TotalMilliseconds + " ms");
								Gogan.LogEvent("Cheat", "Spawn", text4, (long)num8);
								return;
							}
							else
							{
								if (array[0] == "pos")
								{
									Player localPlayer = Player.m_localPlayer;
									if (localPlayer)
									{
										this.AddString("Player position (X,Y,Z):" + localPlayer.transform.position.ToString("F0"));
									}
								}
								if (text.StartsWith("goto "))
								{
									string text5 = text.Substring(5);
									char[] separator = new char[]
									{
										',',
										' '
									};
									string[] array3 = text5.Split(separator);
									if (array3.Length < 2)
									{
										this.AddString("Syntax /goto x,y");
										return;
									}
									try
									{
										float x = float.Parse(array3[0]);
										float z = float.Parse(array3[1]);
										Player localPlayer2 = Player.m_localPlayer;
										if (localPlayer2)
										{
											Vector3 pos2 = new Vector3(x, localPlayer2.transform.position.y, z);
											localPlayer2.TeleportTo(pos2, localPlayer2.transform.rotation, true);
										}
									}
									catch (Exception ex)
									{
										ZLog.Log("parse error:" + ex.ToString() + "  " + text5);
									}
									Gogan.LogEvent("Cheat", "Goto", "", 0L);
									return;
								}
								else
								{
									if (text.StartsWith("exploremap"))
									{
										Minimap.instance.ExploreAll();
										return;
									}
									if (text.StartsWith("resetmap"))
									{
										Minimap.instance.Reset();
										return;
									}
									if (text.StartsWith("puke") && Player.m_localPlayer)
									{
										Player.m_localPlayer.ClearFood();
									}
									if (text.StartsWith("tame"))
									{
										Tameable.TameAllInArea(Player.m_localPlayer.transform.position, 20f);
									}
									if (text.StartsWith("killall"))
									{
										foreach (Character character in Character.GetAllCharacters())
										{
											if (!character.IsPlayer())
											{
												HitData hitData = new HitData();
												hitData.m_damage.m_damage = 1E+10f;
												character.Damage(hitData);
											}
										}
										return;
									}
									if (text.StartsWith("heal"))
									{
										Player.m_localPlayer.Heal(Player.m_localPlayer.GetMaxHealth(), true);
										return;
									}
									if (text.StartsWith("god"))
									{
										Player.m_localPlayer.SetGodMode(!Player.m_localPlayer.InGodMode());
										this.Print("God mode:" + Player.m_localPlayer.InGodMode().ToString());
										Gogan.LogEvent("Cheat", "God", Player.m_localPlayer.InGodMode().ToString(), 0L);
									}
									if (text.StartsWith("ghost"))
									{
										Player.m_localPlayer.SetGhostMode(!Player.m_localPlayer.InGhostMode());
										this.Print("Ghost mode:" + Player.m_localPlayer.InGhostMode().ToString());
										Gogan.LogEvent("Cheat", "Ghost", Player.m_localPlayer.InGhostMode().ToString(), 0L);
									}
									if (text.StartsWith("beard"))
									{
										string beard = (text.Length >= 6) ? text.Substring(6) : "";
										if (Player.m_localPlayer)
										{
											Player.m_localPlayer.SetBeard(beard);
										}
										return;
									}
									if (text.StartsWith("hair"))
									{
										string hair = (text.Length >= 5) ? text.Substring(5) : "";
										if (Player.m_localPlayer)
										{
											Player.m_localPlayer.SetHair(hair);
										}
										return;
									}
									if (text.StartsWith("model "))
									{
										string s = text.Substring(6);
										int playerModel;
										if (Player.m_localPlayer && int.TryParse(s, out playerModel))
										{
											Player.m_localPlayer.SetPlayerModel(playerModel);
										}
										return;
									}
									if (text.StartsWith("tod "))
									{
										float num10;
										if (!float.TryParse(text.Substring(4), NumberStyles.Float, CultureInfo.InvariantCulture, out num10))
										{
											return;
										}
										this.Print("Setting time of day:" + num10);
										if (num10 < 0f)
										{
											EnvMan.instance.m_debugTimeOfDay = false;
										}
										else
										{
											EnvMan.instance.m_debugTimeOfDay = true;
											EnvMan.instance.m_debugTime = Mathf.Clamp01(num10);
										}
									}
									if (array[0] == "env" && array.Length > 1)
									{
										string text6 = text.Substring(4);
										this.Print("Setting debug enviornment:" + text6);
										EnvMan.instance.m_debugEnv = text6;
										return;
									}
									if (text.StartsWith("resetenv"))
									{
										this.Print("Reseting debug enviornment");
										EnvMan.instance.m_debugEnv = "";
										return;
									}
									if (array[0] == "wind" && array.Length == 3)
									{
										float angle = float.Parse(array[1]);
										float intensity = float.Parse(array[2]);
										EnvMan.instance.SetDebugWind(angle, intensity);
									}
									if (array[0] == "resetwind")
									{
										EnvMan.instance.ResetDebugWind();
									}
								}
							}
						}
					}
				}
			}
			return;
		}
		if (array.Length == 1)
		{
			this.Print("Lod bias:" + QualitySettings.lodBias);
			return;
		}
		float num11;
		if (float.TryParse(array[1], NumberStyles.Float, CultureInfo.InvariantCulture, out num11))
		{
			this.Print("Setting lod bias:" + num11);
			QualitySettings.lodBias = num11;
		}
	}

	// Token: 0x060004D1 RID: 1233 RVA: 0x00028014 File Offset: 0x00026214
	public bool IsCheatsEnabled()
	{
		return this.m_cheat && ZNet.instance && ZNet.instance.IsServer();
	}

	// Token: 0x060004D2 RID: 1234 RVA: 0x00028038 File Offset: 0x00026238
	public bool IsConsoleEnabled()
	{
		return global::Console.m_consoleEnabled;
	}

	// Token: 0x060004D3 RID: 1235 RVA: 0x0002803F File Offset: 0x0002623F
	public static void SetConsoleEnabled(bool enabled)
	{
		global::Console.m_consoleEnabled = enabled;
	}

	// Token: 0x04000518 RID: 1304
	private static global::Console m_instance;

	// Token: 0x04000519 RID: 1305
	public RectTransform m_chatWindow;

	// Token: 0x0400051A RID: 1306
	public Text m_output;

	// Token: 0x0400051B RID: 1307
	public InputField m_input;

	// Token: 0x0400051C RID: 1308
	private const int m_maxBufferLength = 30;

	// Token: 0x0400051D RID: 1309
	private List<string> m_chatBuffer = new List<string>();

	// Token: 0x0400051E RID: 1310
	private bool m_cheat;

	// Token: 0x0400051F RID: 1311
	private string m_lastEntry = "";

	// Token: 0x04000520 RID: 1312
	private static bool m_consoleEnabled;
}
