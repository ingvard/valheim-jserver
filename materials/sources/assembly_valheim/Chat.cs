using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Token: 0x0200004C RID: 76
public class Chat : MonoBehaviour
{
	// Token: 0x17000004 RID: 4
	// (get) Token: 0x060004A6 RID: 1190 RVA: 0x000253C5 File Offset: 0x000235C5
	public static Chat instance
	{
		get
		{
			return Chat.m_instance;
		}
	}

	// Token: 0x060004A7 RID: 1191 RVA: 0x000253CC File Offset: 0x000235CC
	private void Awake()
	{
		Chat.m_instance = this;
		ZRoutedRpc.instance.Register<Vector3, int, string, string>("ChatMessage", new RoutedMethod<Vector3, int, string, string>.Method(this.RPC_ChatMessage));
		this.AddString(Localization.instance.Localize("/w [text] - $chat_whisper"));
		this.AddString(Localization.instance.Localize("/s [text] - $chat_shout"));
		this.AddString(Localization.instance.Localize("/killme - $chat_kill"));
		this.AddString(Localization.instance.Localize("/resetspawn - $chat_resetspawn"));
		this.AddString(Localization.instance.Localize("/[emote]"));
		this.AddString(Localization.instance.Localize("Emotes: sit,wave,challenge,cheer,nonono,thumbsup,point"));
		this.AddString("");
		this.m_input.gameObject.SetActive(false);
		this.m_worldTextBase.SetActive(false);
	}

	// Token: 0x060004A8 RID: 1192 RVA: 0x000254A0 File Offset: 0x000236A0
	public bool HasFocus()
	{
		return this.m_chatWindow.gameObject.activeInHierarchy && this.m_input.isFocused;
	}

	// Token: 0x060004A9 RID: 1193 RVA: 0x000254C1 File Offset: 0x000236C1
	public bool IsChatDialogWindowVisible()
	{
		return this.m_chatWindow.gameObject.activeSelf;
	}

	// Token: 0x060004AA RID: 1194 RVA: 0x000254D4 File Offset: 0x000236D4
	private void Update()
	{
		this.m_hideTimer += Time.deltaTime;
		this.m_chatWindow.gameObject.SetActive(this.m_hideTimer < this.m_hideDelay);
		if (!this.m_wasFocused)
		{
			if (Input.GetKeyDown(KeyCode.Return) && Player.m_localPlayer != null && !global::Console.IsVisible() && !TextInput.IsVisible() && !Minimap.InTextInput() && !Menu.IsVisible())
			{
				this.m_hideTimer = 0f;
				this.m_chatWindow.gameObject.SetActive(true);
				this.m_input.gameObject.SetActive(true);
				this.m_input.ActivateInputField();
			}
		}
		else if (this.m_wasFocused)
		{
			this.m_hideTimer = 0f;
			if (Input.GetKeyDown(KeyCode.Return))
			{
				if (!string.IsNullOrEmpty(this.m_input.text))
				{
					this.InputText();
					this.m_input.text = "";
				}
				EventSystem.current.SetSelectedGameObject(null);
				this.m_input.gameObject.SetActive(false);
			}
		}
		this.m_wasFocused = this.m_input.isFocused;
	}

	// Token: 0x060004AB RID: 1195 RVA: 0x00025608 File Offset: 0x00023808
	private void LateUpdate()
	{
		this.UpdateWorldTexts(Time.deltaTime);
		this.UpdateNpcTexts(Time.deltaTime);
	}

	// Token: 0x060004AC RID: 1196 RVA: 0x00025620 File Offset: 0x00023820
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

	// Token: 0x060004AD RID: 1197 RVA: 0x00025698 File Offset: 0x00023898
	public void OnNewChatMessage(GameObject go, long senderID, Vector3 pos, Talker.Type type, string user, string text)
	{
		text = text.Replace('<', ' ');
		text = text.Replace('>', ' ');
		this.AddString(user, text, type);
		this.AddInworldText(go, senderID, pos, type, user, text);
	}

	// Token: 0x060004AE RID: 1198 RVA: 0x000256D0 File Offset: 0x000238D0
	private void UpdateWorldTexts(float dt)
	{
		Chat.WorldTextInstance worldTextInstance = null;
		Camera mainCamera = Utils.GetMainCamera();
		if (mainCamera == null)
		{
			return;
		}
		foreach (Chat.WorldTextInstance worldTextInstance2 in this.m_worldTexts)
		{
			worldTextInstance2.m_timer += dt;
			if (worldTextInstance2.m_timer > this.m_worldTextTTL && worldTextInstance == null)
			{
				worldTextInstance = worldTextInstance2;
			}
			Chat.WorldTextInstance worldTextInstance3 = worldTextInstance2;
			worldTextInstance3.m_position.y = worldTextInstance3.m_position.y + dt * 0.15f;
			Vector3 vector = Vector3.zero;
			if (worldTextInstance2.m_go)
			{
				Character component = worldTextInstance2.m_go.GetComponent<Character>();
				if (component)
				{
					vector = component.GetHeadPoint() + Vector3.up * 0.3f;
				}
				else
				{
					vector = worldTextInstance2.m_go.transform.position + Vector3.up * 0.3f;
				}
			}
			else
			{
				vector = worldTextInstance2.m_position + Vector3.up * 0.3f;
			}
			Vector3 vector2 = mainCamera.WorldToScreenPoint(vector);
			if (vector2.x < 0f || vector2.x > (float)Screen.width || vector2.y < 0f || vector2.y > (float)Screen.height || vector2.z < 0f)
			{
				Vector3 vector3 = vector - mainCamera.transform.position;
				bool flag = Vector3.Dot(mainCamera.transform.right, vector3) < 0f;
				Vector3 vector4 = vector3;
				vector4.y = 0f;
				float magnitude = vector4.magnitude;
				float y = vector3.y;
				Vector3 a = mainCamera.transform.forward;
				a.y = 0f;
				a.Normalize();
				a *= magnitude;
				Vector3 b = a + Vector3.up * y;
				vector2 = mainCamera.WorldToScreenPoint(mainCamera.transform.position + b);
				vector2.x = (float)(flag ? 0 : Screen.width);
			}
			RectTransform rectTransform = worldTextInstance2.m_gui.transform as RectTransform;
			vector2.x = Mathf.Clamp(vector2.x, rectTransform.rect.width / 2f, (float)Screen.width - rectTransform.rect.width / 2f);
			vector2.y = Mathf.Clamp(vector2.y, rectTransform.rect.height / 2f, (float)Screen.height - rectTransform.rect.height);
			vector2.z = Mathf.Min(vector2.z, 100f);
			worldTextInstance2.m_gui.transform.position = vector2;
		}
		if (worldTextInstance != null)
		{
			UnityEngine.Object.Destroy(worldTextInstance.m_gui);
			this.m_worldTexts.Remove(worldTextInstance);
		}
	}

	// Token: 0x060004AF RID: 1199 RVA: 0x000259F8 File Offset: 0x00023BF8
	private void AddInworldText(GameObject go, long senderID, Vector3 position, Talker.Type type, string user, string text)
	{
		Chat.WorldTextInstance worldTextInstance = this.FindExistingWorldText(senderID);
		if (worldTextInstance == null)
		{
			worldTextInstance = new Chat.WorldTextInstance();
			worldTextInstance.m_talkerID = senderID;
			worldTextInstance.m_gui = UnityEngine.Object.Instantiate<GameObject>(this.m_worldTextBase, base.transform);
			worldTextInstance.m_gui.gameObject.SetActive(true);
			worldTextInstance.m_textField = worldTextInstance.m_gui.transform.Find("Text").GetComponent<Text>();
			this.m_worldTexts.Add(worldTextInstance);
		}
		worldTextInstance.m_name = user;
		worldTextInstance.m_type = type;
		worldTextInstance.m_go = go;
		worldTextInstance.m_position = position;
		Color color;
		switch (type)
		{
		case Talker.Type.Whisper:
			color = new Color(1f, 1f, 1f, 0.75f);
			text = text.ToLowerInvariant();
			goto IL_104;
		case Talker.Type.Shout:
			color = Color.yellow;
			text = text.ToUpper();
			goto IL_104;
		case Talker.Type.Ping:
			color = new Color(0.6f, 0.7f, 1f, 1f);
			text = "PING";
			goto IL_104;
		}
		color = Color.white;
		IL_104:
		worldTextInstance.m_textField.color = color;
		worldTextInstance.m_textField.GetComponent<Outline>().enabled = (type > Talker.Type.Whisper);
		worldTextInstance.m_timer = 0f;
		worldTextInstance.m_text = text;
		this.UpdateWorldTextField(worldTextInstance);
	}

	// Token: 0x060004B0 RID: 1200 RVA: 0x00025B44 File Offset: 0x00023D44
	private void UpdateWorldTextField(Chat.WorldTextInstance wt)
	{
		string text = "";
		if (wt.m_type == Talker.Type.Shout || wt.m_type == Talker.Type.Ping)
		{
			text = wt.m_name + ": ";
		}
		text += wt.m_text;
		wt.m_textField.text = text;
	}

	// Token: 0x060004B1 RID: 1201 RVA: 0x00025B94 File Offset: 0x00023D94
	private Chat.WorldTextInstance FindExistingWorldText(long senderID)
	{
		foreach (Chat.WorldTextInstance worldTextInstance in this.m_worldTexts)
		{
			if (worldTextInstance.m_talkerID == senderID)
			{
				return worldTextInstance;
			}
		}
		return null;
	}

	// Token: 0x060004B2 RID: 1202 RVA: 0x00025BF0 File Offset: 0x00023DF0
	private void AddString(string user, string text, Talker.Type type)
	{
		Color color = Color.white;
		if (type != Talker.Type.Whisper)
		{
			if (type == Talker.Type.Shout)
			{
				color = Color.yellow;
				text = text.ToUpper();
			}
			else
			{
				color = Color.white;
			}
		}
		else
		{
			color = new Color(1f, 1f, 1f, 0.75f);
			text = text.ToLowerInvariant();
		}
		string text2 = string.Concat(new string[]
		{
			"<color=orange>",
			user,
			"</color>: <color=#",
			ColorUtility.ToHtmlStringRGBA(color),
			">",
			text,
			"</color>"
		});
		this.AddString(text2);
	}

	// Token: 0x060004B3 RID: 1203 RVA: 0x00025C89 File Offset: 0x00023E89
	private void AddString(string text)
	{
		this.m_chatBuffer.Add(text);
		while (this.m_chatBuffer.Count > 15)
		{
			this.m_chatBuffer.RemoveAt(0);
		}
		this.UpdateChat();
	}

	// Token: 0x060004B4 RID: 1204 RVA: 0x00025CBC File Offset: 0x00023EBC
	private void InputText()
	{
		string text = this.m_input.text;
		if (text == "/resetspawn")
		{
			PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
			if (playerProfile != null)
			{
				playerProfile.ClearCustomSpawnPoint();
			}
			this.AddString("Reseting spawn point");
			return;
		}
		if (text == "/killme")
		{
			HitData hitData = new HitData();
			hitData.m_damage.m_damage = 99999f;
			Player.m_localPlayer.Damage(hitData);
			return;
		}
		Talker.Type type = Talker.Type.Normal;
		if (text.StartsWith("/s ") || text.StartsWith("/S "))
		{
			type = Talker.Type.Shout;
			text = text.Substring(3);
		}
		if (text.StartsWith("/w ") || text.StartsWith("/W "))
		{
			type = Talker.Type.Whisper;
			text = text.Substring(3);
		}
		if (text.StartsWith("/wave"))
		{
			Player.m_localPlayer.StartEmote("wave", true);
			return;
		}
		if (text.StartsWith("/sit"))
		{
			Player.m_localPlayer.StartEmote("sit", false);
			return;
		}
		if (text.StartsWith("/challenge"))
		{
			Player.m_localPlayer.StartEmote("challenge", true);
			return;
		}
		if (text.StartsWith("/cheer"))
		{
			Player.m_localPlayer.StartEmote("cheer", true);
			return;
		}
		if (text.StartsWith("/nonono"))
		{
			Player.m_localPlayer.StartEmote("nonono", true);
			return;
		}
		if (text.StartsWith("/thumbsup"))
		{
			Player.m_localPlayer.StartEmote("thumbsup", true);
			return;
		}
		if (text.StartsWith("/point"))
		{
			Player.m_localPlayer.FaceLookDirection();
			Player.m_localPlayer.StartEmote("point", true);
			return;
		}
		this.SendText(type, text);
	}

	// Token: 0x060004B5 RID: 1205 RVA: 0x00025E5F File Offset: 0x0002405F
	private void RPC_ChatMessage(long sender, Vector3 position, int type, string name, string text)
	{
		this.OnNewChatMessage(null, sender, position, (Talker.Type)type, name, text);
	}

	// Token: 0x060004B6 RID: 1206 RVA: 0x00025E70 File Offset: 0x00024070
	public void SendText(Talker.Type type, string text)
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer)
		{
			if (type == Talker.Type.Shout)
			{
				ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", new object[]
				{
					localPlayer.GetHeadPoint(),
					2,
					localPlayer.GetPlayerName(),
					text
				});
				return;
			}
			localPlayer.GetComponent<Talker>().Say(type, text);
		}
	}

	// Token: 0x060004B7 RID: 1207 RVA: 0x00025EDC File Offset: 0x000240DC
	public void SendPing(Vector3 position)
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer)
		{
			Vector3 vector = position;
			vector.y = localPlayer.transform.position.y;
			ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", new object[]
			{
				vector,
				3,
				localPlayer.GetPlayerName(),
				""
			});
		}
	}

	// Token: 0x060004B8 RID: 1208 RVA: 0x00025F50 File Offset: 0x00024150
	public void GetShoutWorldTexts(List<Chat.WorldTextInstance> texts)
	{
		foreach (Chat.WorldTextInstance worldTextInstance in this.m_worldTexts)
		{
			if (worldTextInstance.m_type == Talker.Type.Shout)
			{
				texts.Add(worldTextInstance);
			}
		}
	}

	// Token: 0x060004B9 RID: 1209 RVA: 0x00025FAC File Offset: 0x000241AC
	public void GetPingWorldTexts(List<Chat.WorldTextInstance> texts)
	{
		foreach (Chat.WorldTextInstance worldTextInstance in this.m_worldTexts)
		{
			if (worldTextInstance.m_type == Talker.Type.Ping)
			{
				texts.Add(worldTextInstance);
			}
		}
	}

	// Token: 0x060004BA RID: 1210 RVA: 0x00026008 File Offset: 0x00024208
	private void UpdateNpcTexts(float dt)
	{
		Chat.NpcText npcText = null;
		Camera mainCamera = Utils.GetMainCamera();
		foreach (Chat.NpcText npcText2 in this.m_npcTexts)
		{
			if (!npcText2.m_go)
			{
				npcText2.m_gui.SetActive(false);
				if (npcText == null)
				{
					npcText = npcText2;
				}
			}
			else
			{
				if (npcText2.m_timeout)
				{
					npcText2.m_ttl -= dt;
					if (npcText2.m_ttl <= 0f)
					{
						npcText2.SetVisible(false);
						if (!npcText2.IsVisible())
						{
							npcText = npcText2;
							continue;
						}
						continue;
					}
				}
				Vector3 vector = npcText2.m_go.transform.position + npcText2.m_offset;
				Vector3 vector2 = mainCamera.WorldToScreenPoint(vector);
				if (vector2.x < 0f || vector2.x > (float)Screen.width || vector2.y < 0f || vector2.y > (float)Screen.height || vector2.z < 0f)
				{
					npcText2.SetVisible(false);
				}
				else
				{
					npcText2.SetVisible(true);
					RectTransform rectTransform = npcText2.m_gui.transform as RectTransform;
					vector2.x = Mathf.Clamp(vector2.x, rectTransform.rect.width / 2f, (float)Screen.width - rectTransform.rect.width / 2f);
					vector2.y = Mathf.Clamp(vector2.y, rectTransform.rect.height / 2f, (float)Screen.height - rectTransform.rect.height);
					npcText2.m_gui.transform.position = vector2;
				}
				if (Vector3.Distance(mainCamera.transform.position, vector) > npcText2.m_cullDistance)
				{
					npcText2.SetVisible(false);
					if (npcText == null && !npcText2.IsVisible())
					{
						npcText = npcText2;
					}
				}
			}
		}
		if (npcText != null)
		{
			this.ClearNpcText(npcText);
		}
	}

	// Token: 0x060004BB RID: 1211 RVA: 0x00026234 File Offset: 0x00024434
	public void SetNpcText(GameObject talker, Vector3 offset, float cullDistance, float ttl, string topic, string text, bool large)
	{
		Chat.NpcText npcText = this.FindNpcText(talker);
		if (npcText != null)
		{
			this.ClearNpcText(npcText);
		}
		npcText = new Chat.NpcText();
		npcText.m_go = talker;
		npcText.m_gui = UnityEngine.Object.Instantiate<GameObject>(large ? this.m_npcTextBaseLarge : this.m_npcTextBase, base.transform);
		npcText.m_gui.SetActive(true);
		npcText.m_animator = npcText.m_gui.GetComponent<Animator>();
		npcText.m_topicField = npcText.m_gui.transform.Find("Topic").GetComponent<Text>();
		npcText.m_textField = npcText.m_gui.transform.Find("Text").GetComponent<Text>();
		npcText.m_ttl = ttl;
		npcText.m_timeout = (ttl > 0f);
		npcText.m_offset = offset;
		npcText.m_cullDistance = cullDistance;
		if (topic.Length > 0)
		{
			npcText.m_textField.text = "<color=orange>" + Localization.instance.Localize(topic) + "</color>\n" + Localization.instance.Localize(text);
		}
		else
		{
			npcText.m_textField.text = Localization.instance.Localize(text);
		}
		this.m_npcTexts.Add(npcText);
	}

	// Token: 0x060004BC RID: 1212 RVA: 0x00026368 File Offset: 0x00024568
	public bool IsDialogVisible(GameObject talker)
	{
		Chat.NpcText npcText = this.FindNpcText(talker);
		return npcText != null && npcText.IsVisible();
	}

	// Token: 0x060004BD RID: 1213 RVA: 0x00026388 File Offset: 0x00024588
	public void ClearNpcText(GameObject talker)
	{
		Chat.NpcText npcText = this.FindNpcText(talker);
		if (npcText != null)
		{
			this.ClearNpcText(npcText);
		}
	}

	// Token: 0x060004BE RID: 1214 RVA: 0x000263A7 File Offset: 0x000245A7
	private void ClearNpcText(Chat.NpcText npcText)
	{
		UnityEngine.Object.Destroy(npcText.m_gui);
		this.m_npcTexts.Remove(npcText);
	}

	// Token: 0x060004BF RID: 1215 RVA: 0x000263C4 File Offset: 0x000245C4
	private Chat.NpcText FindNpcText(GameObject go)
	{
		foreach (Chat.NpcText npcText in this.m_npcTexts)
		{
			if (npcText.m_go == go)
			{
				return npcText;
			}
		}
		return null;
	}

	// Token: 0x040004E0 RID: 1248
	private static Chat m_instance;

	// Token: 0x040004E1 RID: 1249
	public RectTransform m_chatWindow;

	// Token: 0x040004E2 RID: 1250
	public Text m_output;

	// Token: 0x040004E3 RID: 1251
	public InputField m_input;

	// Token: 0x040004E4 RID: 1252
	public float m_hideDelay = 10f;

	// Token: 0x040004E5 RID: 1253
	public float m_worldTextTTL = 5f;

	// Token: 0x040004E6 RID: 1254
	public GameObject m_worldTextBase;

	// Token: 0x040004E7 RID: 1255
	public GameObject m_npcTextBase;

	// Token: 0x040004E8 RID: 1256
	public GameObject m_npcTextBaseLarge;

	// Token: 0x040004E9 RID: 1257
	private List<Chat.WorldTextInstance> m_worldTexts = new List<Chat.WorldTextInstance>();

	// Token: 0x040004EA RID: 1258
	private List<Chat.NpcText> m_npcTexts = new List<Chat.NpcText>();

	// Token: 0x040004EB RID: 1259
	private float m_hideTimer = 9999f;

	// Token: 0x040004EC RID: 1260
	private bool m_wasFocused;

	// Token: 0x040004ED RID: 1261
	private const int m_maxBufferLength = 15;

	// Token: 0x040004EE RID: 1262
	private List<string> m_chatBuffer = new List<string>();

	// Token: 0x02000144 RID: 324
	public class WorldTextInstance
	{
		// Token: 0x040010AF RID: 4271
		public long m_talkerID;

		// Token: 0x040010B0 RID: 4272
		public GameObject m_go;

		// Token: 0x040010B1 RID: 4273
		public Vector3 m_position;

		// Token: 0x040010B2 RID: 4274
		public float m_timer;

		// Token: 0x040010B3 RID: 4275
		public GameObject m_gui;

		// Token: 0x040010B4 RID: 4276
		public Text m_textField;

		// Token: 0x040010B5 RID: 4277
		public string m_name = "";

		// Token: 0x040010B6 RID: 4278
		public Talker.Type m_type;

		// Token: 0x040010B7 RID: 4279
		public string m_text = "";
	}

	// Token: 0x02000145 RID: 325
	public class NpcText
	{
		// Token: 0x06001109 RID: 4361 RVA: 0x000777D1 File Offset: 0x000759D1
		public void SetVisible(bool visible)
		{
			this.m_animator.SetBool("visible", visible);
		}

		// Token: 0x0600110A RID: 4362 RVA: 0x000777E4 File Offset: 0x000759E4
		public bool IsVisible()
		{
			return this.m_animator.GetCurrentAnimatorStateInfo(0).IsTag("visible") || this.m_animator.GetBool("visible");
		}

		// Token: 0x040010B8 RID: 4280
		public GameObject m_go;

		// Token: 0x040010B9 RID: 4281
		public Vector3 m_offset = Vector3.zero;

		// Token: 0x040010BA RID: 4282
		public float m_cullDistance = 20f;

		// Token: 0x040010BB RID: 4283
		public GameObject m_gui;

		// Token: 0x040010BC RID: 4284
		public Animator m_animator;

		// Token: 0x040010BD RID: 4285
		public Text m_textField;

		// Token: 0x040010BE RID: 4286
		public Text m_topicField;

		// Token: 0x040010BF RID: 4287
		public float m_ttl;

		// Token: 0x040010C0 RID: 4288
		public bool m_timeout;
	}
}
